using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Suity.Editor.Helpers;

public static class ShellCommandHelper
{
    public static async Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
    {
        bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        string shell = isWindows ? "cmd.exe" : "/bin/bash";
        string arguments = isWindows ? $"/C {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

        Encoding outputEncoding;
        if (isWindows)
        {
            int consoleCodePage = GetConsoleCodePage();
            outputEncoding = Encoding.GetEncoding(consoleCodePage);
        }
        else
        {
            outputEncoding = Encoding.UTF8;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = outputEncoding,
            StandardErrorEncoding = outputEncoding,
        };

        startInfo.EnvironmentVariables["FORCE_COLOR"] = "1";
        startInfo.EnvironmentVariables["NPM_CONFIG_COLOR"] = "always";
        startInfo.EnvironmentVariables["CI"] = "true";
        startInfo.EnvironmentVariables["NONINTERACTIVE"] = "1";

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                string cleanLine = StripAnsiCodes(e.Data);
                outputBuilder.AppendLine(cleanLine);
                onOutput?.Invoke(cleanLine + Environment.NewLine);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                string cleanLine = StripAnsiCodes(e.Data);
                errorBuilder.AppendLine(cleanLine);
                onOutput?.Invoke("[STDERR] " + cleanLine + Environment.NewLine);
            }
        };

        if (!process.Start())
        {
            return "Failed to start process.";
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            // Asynchronously wait for process to exit, binding cancellation token (including timeout)
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(true); // Recursively kill the process tree
            }
            throw; // Continue throwing to be caught by upper layer
        }

        StringBuilder result = new StringBuilder(outputBuilder.ToString());
        if (errorBuilder.Length > 0)
        {
            if (result.Length > 0) result.AppendLine();
            result.Append("[STDERR]").AppendLine().Append(errorBuilder);
        }

        return result.ToString();
    }

    private static readonly Regex AnsiRegex = new(@"\x1b\[[0-9;]*[a-zA-Z]|\x1b[()][a-zA-Z0-9]|\x1b\][^\x07]*\x07|\x1b\[[0-9;]*[A-Z]", RegexOptions.Compiled);

    private static string StripAnsiCodes(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return AnsiRegex.Replace(text, "");
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetConsoleOutputCP();
    private static int GetConsoleCodePage() => GetConsoleOutputCP();
}
