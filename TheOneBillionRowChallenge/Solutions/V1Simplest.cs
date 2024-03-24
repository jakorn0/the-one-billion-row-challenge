using System.Globalization;

namespace TheOneBillionRowChallenge.Solutions;

public static class V1Simplest
{
    public static void Solve(string inputPath)
    {
        var dict = new Dictionary<string, List<double>>();
        var reader = File.OpenText(inputPath);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
            {
                break;
            }
            var values = line.Split(';');
            var name = values[0];
            var measurement = double.Parse(values[1], CultureInfo.InvariantCulture);
            if (dict.TryGetValue(name, out var list))
            {
                list.Add(measurement);
            }
            else
            {
                dict[name] = [measurement];
            }
        }

        Console.Write('{');
        foreach (var (name, list) in dict.OrderBy(entry => entry.Key))
        {
            Console.Write($"{name}={list.Min():0.0}/{list.Average():0.0}/{list.Max():0.0}, ");
        }
        Console.Write('}');
    }
}