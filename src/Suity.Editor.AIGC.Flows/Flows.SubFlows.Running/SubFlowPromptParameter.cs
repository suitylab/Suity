using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a sub-graph element that handles prompt parameter input for sub-graph.
/// </summary>
public class SubFlowPromptParameter : SubFlowElement, IPageParameterInput
{
    private readonly PagePromptParameterInputItem _inputItem;
    private FlowNodeConnector _connector;

    private readonly TextBlock _cachedPrompt = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowPromptParameter"/> class.
    /// </summary>
    /// <param name="parameterItem">The prompt parameter input item to associate with this element.</param>
    public SubFlowPromptParameter(PagePromptParameterInputItem parameterItem)
        : base(parameterItem)
    {
        _inputItem = parameterItem ?? throw new ArgumentNullException(nameof(parameterItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    #region IPageParameterInput

    /// <summary>
    /// Gets a value indicating whether this input is related to task completion.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a skill input. Always returns <c>false</c> for this element type.
    /// </summary>
    public bool IsSkillInput => false;

    /// <summary>
    /// Gets a value indicating whether this input is related to task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this input includes chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <summary>
    /// Gets the type definition of the parameter. Always returns the native string type for prompt parameters.
    /// </summary>
    public TypeDefinition ParameterType => NativeTypes.StringType;

    /// <summary>
    /// Gets the resolved prompt text as the parameter value.
    /// </summary>
    public object Value => ResolvePrmopt();

    /// <summary>
    /// Gets or sets a value indicating whether a value has been explicitly set.
    /// </summary>
    public bool IsValueSet { get; set; }

    /// <inheritdoc/>
    public object EnsureValue() => ResolvePrmopt();

    /// <inheritdoc/>
    public object GetOuterValue(IFlowComputation outerCompute)
    {
        if (Option.Owner is FlowNode node
            && node.Diagram is { } diagram
            && _connector != null
            && diagram.GetIsLinked(_connector))
        {
            return outerCompute.GetValue(_connector);
        }
        else
        {
            return ResolvePrmopt();
        }
    }

    /// <inheritdoc/>
    public ChatHistoryText ResolveChatHistory() => ResolvePrmopt();

    /// <inheritdoc/>
    public void SetValue(object value)
    {
        if (value is not string prompt)
        {
            return;
        }

        SetPrompt(prompt);
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        TaskCompletion = _inputItem.Node?.TaskCompletion == true;
        TaskCommit = _inputItem.Node?.TaskCommit == true;
        ChatHistory = _inputItem.Node?.ChatHistory == true;
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (TaskCompletion)
        {
            var prompt = ResolvePrmopt();

            return !string.IsNullOrWhiteSpace(prompt);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (Option.Mode == PageElementMode.Skill)
        {
            return;
        }

        if (sync.Intent == SyncIntent.View)
        {
            ResolvePrmopt();
            sync.Sync("Prompt", _cachedPrompt, SyncFlag.GetOnly);
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (Option.Mode == PageElementMode.Skill)
        {
            return;
        }

        var valueType = ParameterType;

        var property = new ViewProperty(Name, DisplayText)
            .WithExpand()
            .WithReadOnly()
            .WithStatus(GetStatus());

        // In function mode, check if the connection point corresponding to the configuration property has been connected
        if (Option.Mode == PageElementMode.Function && _connector != null && Option.Owner is FlowNode flowNode)
        {
            property.ConfigConnected(flowNode.Diagram, _connector);
        }

        setup.InspectorField(_cachedPrompt, property);
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        _connector = node.AddDataInputConnector(Name, NativeTypes.StringType, DisplayText);
    }

    /// <inheritdoc/>
    public override void UpdateFromOther(ISubFlowElement other)
    {
        if (other is SubFlowPromptParameter otherParameter)
        {
            UpdateFromOther(otherParameter);
        }
    }

    /// <summary>
    /// Updates the prompt from another <see cref="SubFlowPromptParameter"/>.
    /// </summary>
    /// <param name="otherParameter">The source element to copy the prompt from.</param>
    public void UpdateFromOther(SubFlowPromptParameter otherParameter)
    {
        string prompt = otherParameter.ResolvePrmopt();
        SetPrompt(prompt);
    }

    private string ResolvePrmopt()
    {
        var task = Option.Owner as IAigcTaskPage;
        
        if (task?.GetPrompt(false) is string prompt)
        {
            _cachedPrompt.Text = prompt;
        }

        return _cachedPrompt.Text;
    }

    private void SetPrompt(string prompt)
    {
        _cachedPrompt.Text = prompt;

        if (Option.Owner is IAigcTaskPage task)
        {
            task.SetPrompt(prompt);
        }
    }
}
