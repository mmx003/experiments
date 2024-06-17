#r "nuget: System.Diagnostics.Process, 4.3.0"
#load "Commands/AbstractCommandRunner.cs"
#load "Commands/PingCommandRunner.cs"

using System;

var arguments = Args.ToArray(); // преобразование аргументов в массив строк

if (arguments.Length < 1)
{
    Console.WriteLine("Usage: dotnet script main.csx <host>");
    return;
}

string host = arguments[0];

var pingRunner = new PingCommandRunner();
var result = pingRunner.Ping(host);
Console.WriteLine($"Average Ping Time: {result.AveragePingTime} ms");
Console.WriteLine($"Packet Loss: {result.PercentLoss} %");
