using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a sub-graph element that handles skill parameter input for AIGC tasks.
/// </summary>
public class SubFlowSkillParameter : SubFlowElement, IPageParameterInput
{
    private readonly PageSkillParameterItem _inputItem;
    private object _value;
    private FlowNodeConnector _connector;


    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowSkillParameter"/> class.
    /// </summary>
    /// <param name="parameterItem">The skill parameter item to associate with this element.</param>
    public SubFlowSkillParameter(PageSkillParameterItem parameterItem)
        : base(parameterItem)
    {
        _inputItem = parameterItem ?? throw new ArgumentNullException(nameof(parameterItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    /// <summary>
    /// Gets the underlying skill parameter item.
    /// </summary>
    public PageSkillParameterItem ParameterItem => _inputItem;

    #region IPageParameterInput

    /// <summary>
    /// Gets the type definition of the parameter.
    /// </summary>
    public TypeDefinition ParameterType { get; private set; }

    /// <summary>
    /// Gets the resolved skill definition value.
    /// </summary>
    public object Value => ResolveSkillDefValue();

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
    public object EnsureValue() => ResolveSkillDefValue();

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
    /// Gets a value indicating whether this is a skill input. Always returns <c>true</c> for this element type.
    /// </summary>
    public bool IsPresetInput => true;

    /// <inheritdoc/>
    public ChatHistoryText ResolveChatHistory() => ConvertChatHistoryText(ParameterType, _value, LinkedMode);

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

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (Option.Mode == PageElementMode.Preset)
        {
            var valueType = ParameterType;
            UpdateDefaultValue(ParameterType);

            // Skill parameters need to notify parent to save
            _value = sync.Sync(Name, _value, SyncFlag.AffectsParent);
        }
        else
        {
            if (sync.Intent == SyncIntent.View && sync.IsGetter())
            {
                var value = ResolveSkillDefValue();
                sync.Sync(Name, value, SyncFlag.GetOnly);
            }
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        var valueType = ParameterType;
        UpdateDefaultValue(valueType);

        var property = new ViewProperty(Name, DisplayText)
            .WithExpand()
            .WithOptional()
            .WithWriteBack() // Important, skill parameters need to notify parent to save
            .WithStatus(GetStatus());

        if (Option.Mode != PageElementMode.Preset)
        {
            property.WithReadOnly();
        }

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
        if (other is SubFlowSkillParameter otherParameter)
        {
            UpdateFromOther(otherParameter);
        }
    }

    /// <summary>
    /// Updates the value from another <see cref="SubFlowSkillParameter"/>.
    /// </summary>
    /// <param name="otherParameter">The source element to copy the value from.</param>
    public void UpdateFromOther(SubFlowSkillParameter otherParameter)
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

    /// <summary>
    /// Resolves the value from the skill definition, or falls back to the local value.
    /// </summary>
    /// <returns>The resolved skill parameter value.</returns>
    public object ResolveSkillDefValue()
    {
        if (Option.Mode != PageElementMode.Preset)
        {
            if ((Root as IHasPreset)?.GetPreset() is { } skill && skill.TryGetParameter(Name, out var value))
            {
                //TODO: Do we need to Clone once to avoid modification?
                return value;
            }
        }

        UpdateDefaultValue(ParameterType);
        return _value;
    }
}
