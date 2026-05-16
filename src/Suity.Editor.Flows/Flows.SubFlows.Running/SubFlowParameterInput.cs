using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a page element that handles parameter input for Sub-flow.
/// </summary>
public class SubFlowParameterInput : SubFlowElement, IPageParameterInput
{
    private readonly SubFlowParameterInputItem _inputItem;
    private object _value;
    private FlowNodeConnector _connector;


    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterInput"/> class.
    /// </summary>
    /// <param name="parameterItem">The parameter input item to associate with this element.</param>
    public SubFlowParameterInput(SubFlowParameterInputItem parameterItem)
        : base(parameterItem)
    {
        _inputItem = parameterItem ?? throw new ArgumentNullException(nameof(parameterItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    #region IPageParameterInput

    /// <summary>
    /// Gets the type definition of the parameter.
    /// </summary>
    public TypeDefinition ParameterType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this input is related to task completion.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this input is related to task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this input includes chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the parameter is displayed as a link address instead of content.
    /// </summary>
    public bool LinkedMode { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a preset input. Always returns <c>false</c> for this element type.
    /// </summary>
    public bool IsPresetInput => false;

    /// <inheritdoc/>
    public HistoryText ResolveChatHistory() => SubFlowExtensions.ConvertChatHistoryText(ParameterType, _value, LinkedMode);

    /// <summary>
    /// Gets the current value of the parameter.
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
            return _value;
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        var node = _inputItem.Node;

        ParameterType = node?.TypeDef ?? TypeDefinition.Empty;

        TaskCompletion = node?.TaskCompletion == true;
        TaskCommit = node?.TaskCommit == true;
        ChatHistory = node?.ChatHistory == true;
        LinkedMode = node?.LinkedMode == true;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (Option.Mode == PageElementMode.Preset)
        {
            return;
        }

        // var valueType = ParameterType;
        UpdateDefaultValue(ParameterType);

        _value = sync.Sync(Name, _value);
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (Option.Mode == PageElementMode.Preset)
        {
            return;
        }

        var valueType = ParameterType;
        UpdateDefaultValue(valueType);

        var property = new ViewProperty(Name, DisplayText)
            .WithExpand()
            .WithOptional()
            .WithStatus(GetStatus());

        // In function mode, check if the connection point corresponding to the configuration property has been connected
        if (Option.Mode == PageElementMode.Function && _connector != null && Option.Owner is FlowNode flowNode)
        {
            property.ConfigConnected(flowNode.Diagram, _connector);
        }

        setup.InspectorFieldOfType(valueType, property);
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        var valueType = ParameterType;
        _connector = node.AddDataInputConnector(Name, valueType, DisplayText);
    }


    /// <inheritdoc/>
    public override void UpdateFromOther(ISubFlowElement other)
    {
        if (other is SubFlowParameterInput otherParameter)
        {
            UpdateFromOther(otherParameter);
        }
    }

    /// <summary>
    /// Updates the value from another <see cref="SubFlowParameterInput"/>.
    /// </summary>
    /// <param name="otherParameter">The source element to copy the value from.</param>
    public void UpdateFromOther(SubFlowParameterInput otherParameter)
    {
        _value = otherParameter._value;
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (TaskCompletion)
        {
            return !SubFlowHelper.GetIsValueEmpty(Value);
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
            _value = type.CreateOrRepairValue(_value, true);
        }
        else
        {
            _value = null;
        }
    }
}
