using System.Diagnostics;
using System.Text;

namespace Zipadee.Build.Tests.Infrastructure;

internal sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
{
    public string CombinedOutput => StdOut + Environment.NewLine + StdErr;
}

/// <summary>
/// Runs an external process to completion, capturing its output. Every test in this project goes
/// through here to invoke `dotnet build` and 7z - never a raw <see cref="Process"/> call - so
/// timeout/output-capture behavior is consistent everywhere.
/// </summary>
internal static class ProcessRunner
{
    public static ProcessResult Run(string fileName, string arguments, string workingDirectory, int timeoutMs = 180_000)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdOut.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stdErr.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!process.WaitForExit(timeoutMs))
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
            throw new TimeoutException($"'{fileName} {arguments}' (in '{workingDirectory}') did not exit within {timeoutMs}ms.");
        }

        // Guarantees the async output/error handlers above have fully drained before we read the
        // StringBuilders - WaitForExit(int) alone doesn't promise that on every runtime.
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, stdOut.ToString(), stdErr.ToString());
    }
}
