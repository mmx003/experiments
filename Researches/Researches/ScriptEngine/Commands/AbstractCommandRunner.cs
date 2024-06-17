using System.Diagnostics;

public abstract class AbstractCommandRunner
{
    protected (int ExitCode, string Output, string Error) ExecuteCommand(string command, string args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using (var process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return (process.ExitCode, output, error);
        }
    }
}
