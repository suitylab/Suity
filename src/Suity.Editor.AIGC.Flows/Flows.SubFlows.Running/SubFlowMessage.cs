using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a message element in a sub-graph that handles text-based message display and parameter input/output.
/// </summary>
public class SubFlowMessage : SubFlowElement, IPageMessage, IPageParameterInput, IPageParameterOutput
{
    private readonly PageMessageParameterItem _msg;
    private TextBlock _text = new();
    private string _tooltipText;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowMessage"/> class.
    /// </summary>
    /// <param name="msg">The page message parameter item.</param>
    public SubFlowMessage(PageMessageParameterItem msg)
        : base(msg)
    {
        _msg = msg ?? throw new ArgumentNullException(nameof(msg));
    }

    #region IPageMessage

    /// <summary>
    /// Gets a value indicating whether this message signals task completion.
    /// </summary>
    public bool Required { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this message signals task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this message contributes to chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    public bool TooltipMode { get; private set; }

    /// <summary>
    /// Gets the parameter type, which is always string for message elements.
    /// </summary>
    public TypeDefinition ParameterType => NativeTypes.StringType;

    /// <summary>
    /// Gets the resolved message value.
    /// </summary>
    public object Value => ResolveMessage();

    /// <summary>
    /// Gets or sets a value indicating whether a value has been set. Always returns false for this element.
    /// </summary>
    public bool IsValueSet 
    {
        get => false;
        set { }
    }

    /// <summary>
    /// Ensures and returns the resolved message value.
    /// </summary>
    /// <returns>The resolved message text.</returns>
    public object EnsureValue() => ResolveMessage();

    /// <summary>
    /// Resolves the chat history text representation of the message.
    /// </summary>
    /// <returns>The resolved message text.</returns>
    public HistoryTag ResolveChatHistory(ResolveChatIntents intent) => ResolveMessage();

    /// <summary>
    /// Sets the value for this element. This operation is not supported for message elements.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(object value)
    {
        _text.Text = value?.ToString();
    }

    #endregion

    #region IPageParameterInput

    /// <summary>
    /// Gets a value indicating whether this element is a preset input. Always returns false.
    /// </summary>
    public bool IsPresetInput => false;

    /// <summary>
    /// Gets the outer value from the flow computation context.
    /// </summary>
    /// <param name="outerCompute">The flow computation context.</param>
    /// <returns>The resolved message value.</returns>
    public object GetOuterValue(IFlowComputation outerCompute)
    {
        return ResolveMessage();
    }
    #endregion

    #region IPageParameterOutput

    /// <summary>
    /// Sets the outer value from the flow computation context. This operation is not supported for message elements.
    /// </summary>
    /// <param name="outerCompute">The flow computation context.</param>
    /// <param name="value">The value to set.</param>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
    }
    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        Required = _msg.Node?.Required == true;
        TaskCommit = _msg.Node?.TaskCommit == true;
        ChatHistory = _msg.Node?.ChatHistory == true;
        TooltipMode = _msg.Node?.TooltipMode == true;

        _tooltipText = _msg.Node?.TooltipText;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        _text = sync.Sync(Name, _text, SyncFlag.NotNull) ?? new();
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        if (TooltipMode)
        {
            setup.Tooltips("#Tooltip_" + Name, TextStatus.Normal, ResolveMessage());
        }
        else
        {
            setup.InspectorFieldOf<TextBlock>(new ViewProperty(Name, DisplayText));
        }
    }

    /// <inheritdoc/>
    public override bool? GetIsDone() => null;


    private string ResolveMessage()
    {
        if (TooltipMode)
        {
            string text = _tooltipText ?? string.Empty;

            if (Option.Owner is IAigcWorkflowPage task)
            {
                text = text.Replace("{TaskId}", task.TaskId);
                text = text.Replace("{TaskTitle}", task.DisplayText);
                text = text.Replace("{TaskStatus}", task.DisplayStatus.ToString());
                text = text.Replace("{SubTaskCount}", task.SubTaskCount.ToString());
            }

            return text;
        }
        else
        {
            return _text.Text;
        }
    }

    /// <inheritdoc/>
    public override bool GetCanOutputHistory(FlowDirections direction)
    {
        if (!base.GetCanOutputHistory(direction))
        {
            return false;
        }

        // The upper layer cannot determine the direction of this message, so we need to look upward for the container's nature
        if (direction == FlowDirections.Input)
        {
            return FindParent(o => o is SubFlowResultElement) is null;
        }
        else
        {
            return FindParent(o => o is SubFlowResultElement) != null;
        }

    }
}
