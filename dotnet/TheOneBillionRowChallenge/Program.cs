using System.Diagnostics;
using TheOneBillionRowChallenge.Solutions;

const string filePrefix = "100m_";
const string inputPath = $"/Users/jakub.ornass/Projects/1brc/data/{filePrefix}measurements.txt";

var stopwatch = Stopwatch.StartNew();
V4Parallelize.Solve(inputPath);
Console.WriteLine($"\n\n{stopwatch.Elapsed.Minutes} min {stopwatch.Elapsed.Seconds} sec");
