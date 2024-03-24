using System.Diagnostics;
using TheOneBillionRowChallenge.Solutions;

const string inputPath = "";

var stopwatch = Stopwatch.StartNew();
V4Parallelize.Solve(inputPath);
Console.WriteLine($"\n\n{stopwatch.Elapsed.Minutes} min {stopwatch.Elapsed.Seconds} sec");
