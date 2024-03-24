using System.Globalization;
using System.Text;

namespace TheOneBillionRowChallenge.Solutions;

public static class V3UniqueHashCodeAssumption
{
    private class ResultAccumulator(string stationName, float measurement)
    {
        public string StationName { get; set; } = stationName;
        public float Min { get; set; } = measurement;
        public float Max { get; set; } = measurement;
        public double Sum { get; set; } = measurement;
        public int Count { get; set; } = 1;
    }

    public static void Solve(string inputPath)
    {
        var dict = new Dictionary<int, ResultAccumulator>(capacity: 10_000);
        using var fileReader = File.OpenRead(inputPath);
        var buffer = new byte[5000];
        var bufferOffset = 0;
        var measurementCharBuffer = new char[10].AsSpan();
        while (true)
        {
            var bytesCount = fileReader.Read(buffer, bufferOffset, 4096);
            if (bytesCount == 0)
            {
                break;
            }

            var bufferSpan = buffer.AsSpan(0, bufferOffset + bytesCount);
            while (true)
            {
                var indexOfNewLine = bufferSpan.IndexOf((byte)'\n');
                if (indexOfNewLine == -1)
                {
                    bufferSpan.CopyTo(buffer);
                    bufferOffset = bufferSpan.Length;
                    break;
                }
                
                var chunk = bufferSpan[..indexOfNewLine];
                var indexOfSemicolon = chunk.IndexOf((byte)';');
                var nameChunk = chunk[..indexOfSemicolon];
                var hashCodeBuilder = new HashCode();
                hashCodeBuilder.AddBytes(nameChunk);
                var hash = hashCodeBuilder.ToHashCode();
                bytesCount = Encoding.UTF8.GetChars(chunk[(indexOfSemicolon + 1)..], measurementCharBuffer);
                var measurement = float.Parse(measurementCharBuffer[..bytesCount], CultureInfo.InvariantCulture);
                
                // I do an assumption here (potentially a risky one) that a hash code is unique
                // This is to avoid allocating a string when it's not necessary
                if (dict.TryGetValue(hash, out var accumulator))
                {
                    if (measurement < accumulator.Min) accumulator.Min = measurement;
                    if (measurement > accumulator.Max) accumulator.Max = measurement;
                    accumulator.Sum += measurement;
                    accumulator.Count++;
                }
                else
                {
                    var stationName = Encoding.UTF8.GetString(nameChunk);
                    dict[hash] = new ResultAccumulator(stationName, measurement);
                }

                bufferSpan = bufferSpan[(indexOfNewLine + 1)..];
            }
        }

        Console.Write('{');
        foreach (var (key, accumulator) in dict.OrderBy(entry => entry.Value.StationName))
        {
            Console.Write($"{accumulator.StationName}={accumulator.Min:0.0}/{accumulator.Sum / accumulator.Count:0.0}/{accumulator.Max:0.0}, ");
        }

        Console.Write('}');
    }
}