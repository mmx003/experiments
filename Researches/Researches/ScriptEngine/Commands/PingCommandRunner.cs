using System;
using System.Linq;

public class PingCommandRunner : AbstractCommandRunner
{
    public (double AveragePingTime, double PercentLoss) Ping(string host)
    {
        var result = ExecuteCommand("ping", host);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Ping command failed with exit code {result.ExitCode}. Error: {result.Error}");
        }

        string output = result.Output;

        // Parse the output to find AveragePingTime and PercentLoss
        double averagePingTime = ParseAveragePingTime(output);
        double percentLoss = ParsePercentLoss(output);

        return (averagePingTime, percentLoss);
    }

    private double ParseAveragePingTime(string output)
    {
        // Find the line containing the average ping time
        var lines = output.Split('\n');
        var timeLine = lines.FirstOrDefault(line => line.Contains("Average") || line.Contains("avg"));

        if (timeLine == null)
        {
            throw new InvalidOperationException("Could not find the average ping time in the output.");
        }

        // Extract the average time value
        var timePart = timeLine.Split('=').Last().Trim();
        var timeValue = new string(timePart.Where(c => char.IsDigit(c) || c == '.').ToArray());

        return double.Parse(timeValue);
    }

    private double ParsePercentLoss(string output)
    {
        // Find the line containing the packet loss information
        var lines = output.Split('\n');
        var lossLine = lines.FirstOrDefault(line => line.Contains("loss") || line.Contains("Lost"));

        if (lossLine == null)
        {
            throw new InvalidOperationException("Could not find the packet loss information in the output.");
        }

        // Extract the packet loss value
        var lossPart = lossLine.Split(',').FirstOrDefault(part => part.Contains("loss") || part.Contains("Lost"));
        var lossValue = new string(lossPart.Where(char.IsDigit).ToArray());

        return double.Parse(lossValue);
    }
}
