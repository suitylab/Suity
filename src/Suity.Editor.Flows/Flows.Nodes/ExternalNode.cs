using Suity.Drawing;
using Suity.Editor.AIGC.StreamUpdaters;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;

/// <summary>
/// Base class for flow nodes that interact with external services or systems.
/// </summary>
[DisplayText("External", "*CoreIcon|System")]
[ToolTipsText("External service related nodes")]
public abstract class ExternalNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override ImageDef Icon => GetType().ToDisplayIcon() ?? CoreIconCache.System;

    /// <summary>
    /// Asynchronously computes the node's output.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Compute(compute);

        return Task.FromResult<object>(null);
    }
}

#region RunShellCommand

/// <summary>
/// A flow node that executes a shell command cross-platform and captures the console output asynchronously.
/// </summary>
[DisplayText("Run Shell Command", "*CoreIcon|System")]
[NativeAlias("Suity.Editor.AIGC.FLows.External.RunShellCommand")]
[NativeAlias("Suity.Editor.Flows.Nodes.RunShellCommand")]
[NativeAlias("Suity.Editor.Flows.Nodes.RunShellCommandNode")]
public class RunShellCommand : ExternalNode
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

        string? workingDirectory = _workingDirectory.GetValue(compute, this);
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

            string output = await EditorServices.EditorSystem.ExecuteCommandAsync(command, workingDirectory, onOutput, cts.Token);
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
}

#endregion