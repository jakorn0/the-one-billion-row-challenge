using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Threading.Channels;

namespace TheOneBillionRowChallenge.Solutions;

public static class V4Parallelize
{
    private class ResultAccumulator(float initialMeasurement)
    {
        public float Min { get; private set; } = initialMeasurement;
        public float Max { get; private set; } = initialMeasurement;
        public double Sum { get; private set; } = initialMeasurement;
        public int Count { get; private set; } = 1;

        public void AddMeasurement(float measurement)
        {
            Min = Math.Min(Min, measurement);
            Max = Math.Max(Max, measurement);
            Sum += measurement;
            Count++;
        }

        public void Join(ResultAccumulator accumulator)
        {
            Min = Math.Min(Min, accumulator.Min);
            Max = Math.Max(Max, accumulator.Max);
            Sum += accumulator.Sum;
            Count += accumulator.Count;
        }
    }

    private static readonly ConcurrentDictionary<string, ResultAccumulator> ResultsDictionary = new(-1, 10_000);

    private static readonly Channel<byte[]> ChunksChannel =
        Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleWriter = true });

    public static void Solve(string inputPath)
    {
        var consumersCount = Environment.ProcessorCount;
        var consumerTasks = new Task[consumersCount];
        for (var i = 0; i < consumersCount; i++)
        {
            consumerTasks[i] = Task.Run(ConsumeChunks);
        }

        const int chunkSize = 4096;
        var buffer = new byte[chunkSize + 100];
        var bufferReadOffset = 0;
        using var fileReader = File.OpenRead(inputPath);

        while (true)
        {
            var bytesCount = fileReader.Read(buffer, bufferReadOffset, chunkSize);
            if (bytesCount == 0 && bufferReadOffset > 0)
            {
                throw new Exception("Some unfinished record was read but there was no continuation in the next chunk");
            }
            if (bytesCount == 0)
            {
                break;
            }

            var bufferSpan = buffer.AsSpan(0, bufferReadOffset + bytesCount);
            var indexOfLastNewLine = bufferSpan.LastIndexOf((byte)'\n');
            var bytesToCopyCount = indexOfLastNewLine + 1;
            var chunk = bufferSpan[..(indexOfLastNewLine + 1)];
            var chunkCopy = new byte[bytesToCopyCount];
            chunk.CopyTo(chunkCopy);
            var success = ChunksChannel.Writer.TryWrite(chunkCopy);
            if (!success)
            {
                throw new Exception("Channel.TryWrite() unexpectedly failed");
            }

            if (buffer.Last() != '\n')
            {
                var remainingPiece = bufferSpan[(indexOfLastNewLine + 1)..];
                remainingPiece.CopyTo(bufferSpan);
                bufferReadOffset = remainingPiece.Length;
            }
            else
            {
                bufferReadOffset = 0;
            }
        }

        ChunksChannel.Writer.Complete();
        Task.WaitAll(consumerTasks);

        Console.Write('{');
        foreach (var (name, accumulator) in ResultsDictionary.OrderBy(entry => entry.Key))
        {
            Console.Write($"{name}={accumulator.Min:0.0}/{accumulator.Sum / accumulator.Count:0.0}/{accumulator.Max:0.0}, ");
        }

        Console.Write('}');
    }

    private static async Task ConsumeChunks()
    {
        var localDictionary = new Dictionary<string, ResultAccumulator>(capacity: 1000);
        var measurementCharBuffer = new char[10];

        await foreach (var chunkArray in ChunksChannel.Reader.ReadAllAsync())
        {
            ProcessSingleChunk(chunkArray, measurementCharBuffer, localDictionary);
        }

        foreach (var (key, value) in localDictionary)
        {
            ResultsDictionary.AddOrUpdate(key, value, (_, accumulator) =>
            {
                accumulator.Join(accumulator);
                return accumulator;
            });
        }
    }

    private static void ProcessSingleChunk(byte[] chunkArray, char[] measurementCharBuffer,
        IDictionary<string, ResultAccumulator> localDictionary)
    {
        var chunk = chunkArray.AsMemory();

        while (true)
        {
            var indexOfNewLine = chunk.Span.IndexOf((byte)'\n');
            if (indexOfNewLine == -1)
            {
                break;
            }

            var recordMemory = chunk[..indexOfNewLine];
            var indexOfSemicolon = recordMemory.Span.IndexOf((byte)';');
            var stationName = Encoding.UTF8.GetString(recordMemory.Span[..indexOfSemicolon]);
            var bytesCount = Encoding.UTF8.GetChars(recordMemory.Span[(indexOfSemicolon + 1)..], measurementCharBuffer);
            var measurement = float.Parse(measurementCharBuffer.AsSpan(0, bytesCount), CultureInfo.InvariantCulture);
            if (localDictionary.TryGetValue(stationName, out var accumulator))
            {
                accumulator.AddMeasurement(measurement);
            }
            else
            {
                localDictionary[stationName] = new ResultAccumulator(measurement);
            }

            chunk = chunk[(indexOfNewLine + 1)..];
        }
    }
}