using Suity.Editor.AIGC.StreamUpdaters;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;

#region RunShellCommand

/// <summary>
/// A flow node that executes a shell command cross-platform and captures the console output asynchronously.
/// </summary>
[DisplayText("Run Shell Command", "*CoreIcon|System")]
[NativeAlias("Suity.Editor.AIGC.FLows.External.RunShellCommand")]
public class RunShellCommand : AigcExternalNode
{
    private readonly FlowNodeConnector _in;
    private readonly ConnectorStringProperty _command = new("Command", "Command", "", "The shell command to execute.");
    private readonly ConnectorStringProperty _workingDirectory = new("WorkingDirectory", "Working Directory", "", "The working directory for the command. If empty, uses the current directory.");
    private readonly ConnectorValueProperty<int> _timeout = new("Timeout", "Timeout (s)", 0, "Maximum time to wait for command completion in seconds. 0 means no timeout.");
    private readonly FlowNodeConnector _out;
    private readonly FlowNodeConnector _result;

    public RunShellCommand()
    {
        _in = this.AddActionInputConnector("In", "Input");
        _command.AddConnector(this);
        _workingDirectory.AddConnector(this);
        _timeout.AddConnector(this);
        _out = this.AddActionOutputConnector("Out", "Output");
        _result = this.AddDataOutputConnector("Result", "string", "Result");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _command.Sync(sync);
        _workingDirectory.Sync(sync);
        _timeout.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _command.InspectorField(setup, this);
        _workingDirectory.InspectorField(setup, this);
        _timeout.InspectorField(setup, this);
    }

    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        string command = _command.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command is null or empty.");
        }

        int timeoutSec = _timeout.GetValue(compute, this);
        int timeoutMs = timeoutSec > 0 ? timeoutSec * 1000 : Timeout.Infinite;

        string workingDirectory = _workingDirectory.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            workingDirectory = null;
        }

        var conversation = compute.Context.GetArgument<IConversationHandler>();
        SimpleStreamUpdater? updater = null;
        Action<string>? onOutput = null;

        if (conversation != null)
        {
            updater = new SimpleStreamUpdater { Conversation = conversation };
            onOutput = updater.Append;
        }

        try
        {
            // Merge external cancellaTtion token with timeout token
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel);
            if (timeoutMs != Timeout.Infinite)
            {
                cts.CancelAfter(timeoutMs);
            }

            string output = await ExecuteCommandAsync(command, workingDirectory, onOutput, cts.Token);
            compute.SetValue(_result, output);
        }
        catch (OperationCanceledException)
        {
            string message = cancel.IsCancellationRequested ? "Command cancelled by user." : $"Command timed out after {timeoutSec}s.";
            compute.SetValue(_result, message);
            onOutput?.Invoke($"\n[SYSTEM] {message}\n");
        }
        finally
        {
            updater?.Dispose();
        }

        return _out;
    }

    private static readonly Regex AnsiRegex = new(@"\x1b\[[0-9;]*[a-zA-Z]|\x1b[()][a-zA-Z0-9]|\x1b\][^\x07]*\x07|\x1b\[[0-9;]*[A-Z]", RegexOptions.Compiled);

    private static string StripAnsiCodes(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return AnsiRegex.Replace(text, "");
    }

    private static async Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
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

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetConsoleOutputCP();
    private static int GetConsoleCodePage() => GetConsoleOutputCP();
}

#endregion