using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

#region WorkflowLog

/// <summary>
/// Node that outputs workflow logs with optional forced pause support.
/// </summary>
[DisplayText("AI Workflow Log", "*CoreIcon|Conversation")]
[ToolTipsText("Output workflow logs, can support forced pause.")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkflowLog")]
public class WorkflowLog : AigcFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private readonly ConnectorValueProperty<bool> _forcePause
        = new("ForcePause", "Force Pause");

    private readonly ConnectorTextBlockProperty _message
        = new("Message", "Message");

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowLog"/> class.
    /// </summary>
    public WorkflowLog()
    {
        _in = AddActionInputConnector("In", "Input");
        _message.AddConnector(this);
        _forcePause.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
        _forcePause.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
        _forcePause.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>();
        if (conversation is null)
        {
            throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");
        }

        var agent = compute.Context.GetArgument<IWorkflowSetup>();
        bool pause = agent?.PauseOnAILog == true;

        // Skip if not pausing
        string text = _message.GetText(compute, this) ?? string.Empty;
        if (!pause && !_forcePause.GetValue(compute, this))
        {
            conversation.AddSystemMessage(text);

            return _out;
        }

        bool ok = await compute.PauseDialog(text, cancel);
        cancel.ThrowIfCancellationRequested();

        if (ok)
        {
            return _out;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Diagram?.GetIsLinked(_forcePause.Connector) == true)
        {
            return DisplayText;
        }

        if (_forcePause.BaseValue)
        {
            return $"{DisplayText}(Paused)";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion
