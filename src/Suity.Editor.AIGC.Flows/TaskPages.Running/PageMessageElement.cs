using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Views;
using System;

namespace Suity.Editor.AIGC.TaskPages.Running;

/// <summary>
/// Represents a message element in an AIGC page that handles text-based message display and parameter input/output.
/// </summary>
public class PageMessageElement : AigcPageElement, IPageMessage, IPageParameterInput, IPageParameterOutput
{
    private readonly PageMessageParameterItem _msg;
    private string _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageMessageElement"/> class.
    /// </summary>
    /// <param name="msg">The page message parameter item.</param>
    public PageMessageElement(PageMessageParameterItem msg)
        : base(msg)
    {
        _msg = msg ?? throw new ArgumentNullException(nameof(msg));
    }

    #region IPageMessage

    /// <summary>
    /// Gets a value indicating whether this message signals task completion.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this message signals task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this message contributes to chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

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
    public ChatHistoryText ResolveChatHistory() => ResolveMessage();

    /// <summary>
    /// Sets the value for this element. This operation is not supported for message elements.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(object value)
    {
    }

    #endregion

    #region IPageParameterInput

    /// <summary>
    /// Gets a value indicating whether this element is a skill input. Always returns false.
    /// </summary>
    public bool IsSkillInput => false;

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

        TaskCompletion = _msg.Node?.TaskCompletion == true;
        TaskCommit = _msg.Node?.TaskCommit == true;
        ChatHistory = _msg.Node?.ChatHistory == true;

        _text = _msg.Node?.Value ?? string.Empty;
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.Tooltips(Name, TextStatus.Normal, ResolveMessage());
    }

    /// <inheritdoc/>
    public override bool? GetIsDone() => null;


    private string ResolveMessage()
    {
        string text = _text ?? string.Empty;

        if (Option.Owner is IAigcTaskPage task)
        {
            text = text.Replace("{TaskName}", task.TaskName);
            text = text.Replace("{TaskStatus}", task.TaskStatus.ToString());
        }

        return text;
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
            return FindParent(o => o is ResultPageElement) is null;
        }
        else
        {
            return FindParent(o => o is ResultPageElement) != null;
        }

    }
}
