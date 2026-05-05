using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.AIGC.TaskPages.Running;

/// <summary>
/// Represents a page element that handles parameter output for AIGC tasks.
/// </summary>
public class PageParameterOutputElement : AigcPageElement, IPageParameterOutput
{
    private readonly PageParameterOutputItem _outputItem;
    private object _value;
    private FlowNodeConnector _connector;


    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterOutputElement"/> class.
    /// </summary>
    /// <param name="outputItem">The parameter output item to associate with this element.</param>
    public PageParameterOutputElement(PageParameterOutputItem outputItem)
        : base(outputItem)
    {
        _outputItem = outputItem ?? throw new ArgumentNullException(nameof(outputItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    #region IPageParameterOutput

    /// <summary>
    /// Gets the type definition of the output parameter.
    /// </summary>
    public TypeDefinition ParameterType { get; private set; }

    /// <summary>
    /// Gets the current value of the output parameter.
    /// </summary>
    public object Value => _value;

    /// <summary>
    /// Gets or sets a value indicating whether a value has been explicitly set.
    /// </summary>
    public bool IsValueSet { get; set; }


    /// <inheritdoc/>
    public void SetValue(object value)
    {
        _value = value;
        IsValueSet = true;
    }

    /// <inheritdoc/>
    public object EnsureValue()
    {
        UpdateDefaultValue(ParameterType);
        return _value;
    }

    /// <summary>
    /// Gets a value indicating whether this output is related to task completion.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output is related to task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output includes chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the parameter is displayed as a link address instead of content.
    /// </summary>
    public bool LinkedMode { get; private set; }

    /// <inheritdoc/>
    public ChatHistoryText ResolveChatHistory() => ConvertChatHistoryText(ParameterType, _value, LinkedMode);

    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        var node = _outputItem.Node;

        ParameterType = node?.TypeDef ?? TypeDefinition.Empty;

        TaskCompletion = node?.TaskCompletion == true;
        TaskCommit = node?.TaskCommit == true;
        ChatHistory = node?.ChatHistory == true;
        LinkedMode = node?.LinkedMode == true;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        var valueType = ParameterType;
        UpdateDefaultValue(valueType);

        bool hasText1 = !string.IsNullOrWhiteSpace((_value as STextBlock)?.TextValue);

        _value = sync.Sync(Name, _value);

        bool hasText2 = !string.IsNullOrWhiteSpace((_value as STextBlock)?.TextValue);

        if (hasText1 && !hasText2)
        {
            //Logs.LogWarning("0-00");
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        var valueType = ParameterType;
        UpdateDefaultValue(valueType);

        var property = new ViewProperty(Name, DisplayText)
            .WithExpand()
            .WithOptional()
            .WithStatus(GetStatus());

        setup.InspectorFieldOfType(valueType, property);
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        var valueType = ParameterType;
        _connector = node.AddDataOutputConnector(Name, valueType, DisplayText);
    }

    /// <inheritdoc/>
    public override void UpdateFromOther(IAigcPageElement other)
    {
        if (other is PageParameterOutputElement otherOutput)
        {
            UpdateFromOther(otherOutput);
        }
    }

    /// <summary>
    /// Updates the value from another <see cref="PageParameterOutputElement"/>.
    /// </summary>
    /// <param name="otherParameter">The source element to copy the value from.</param>
    public void UpdateFromOther(PageParameterOutputElement otherParameter)
    {
        _value = otherParameter._value;
    }

    /// <summary>
    /// Sets the output value to the outer flow computation via the connector.
    /// </summary>
    /// <param name="outerCompute">The flow computation to set the value on.</param>
    /// <param name="value">The value to set.</param>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
        if (_connector != null)
        {
            outerCompute.SetValue(_connector, value);
        }
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (TaskCompletion)
        {
            return !PageHelper.GetIsValueEmpty(Value);
        }
        else
        {
            return null;
        }
    }

    private void UpdateDefaultValue(TypeDefinition type)
    {
        if (!TypeDefinition.IsNullOrEmpty(type))
        {
            var repaire = type.CreateOrRepairValue(_value, true);
            _value = repaire;
        }
        else
        {
            _value = null;
        }
    }
}
