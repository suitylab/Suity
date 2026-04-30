using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC.TaskPages;

/// <summary>
/// Represents the end element of an AIGC page, handling page completion and parameter output.
/// </summary>
public class PageEndElement : AigcPageElement, IPageParameterOutput
{
    private readonly FlowDiagramItem _endItem;

    private object _value;
    private FlowNodeConnector _connector;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageEndElement"/> class.
    /// </summary>
    /// <param name="endItem">The flow diagram item representing the end node.</param>
    public PageEndElement(FlowDiagramItem endItem)
        : base(endItem)
    {
        _endItem = endItem ?? throw new System.ArgumentNullException(nameof(endItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    /// <summary>
    /// Gets the AIGC end node associated with this element.
    /// </summary>
    public IAigcEndNode EndNode { get; private set; }

    #region IPageParameterOutput

    /// <summary>
    /// Gets the type definition of the output parameter.
    /// </summary>
    public TypeDefinition ParameterType { get; private set; }

    /// <summary>
    /// Gets the type of page commit operation for this end element.
    /// </summary>
    public PageCommitTypes EndType { get; private set; }

    /// <summary>
    /// Gets the current value of this output parameter.
    /// </summary>
    public object Value => _value;

    /// <summary>
    /// Gets or sets a value indicating whether a value has been set.
    /// </summary>
    public bool IsValueSet { get; set; }

    /// <summary>
    /// Sets the value for this output parameter.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(object value)
    {
        _value = value;
        IsValueSet = true;
    }

    /// <summary>
    /// Ensures the value is initialized with a default value if necessary and returns it.
    /// </summary>
    /// <returns>The current or default value.</returns>
    public object EnsureValue()
    {
        UpdateDefaultValue(ParameterType);
        return _value;
    }

    /// <summary>
    /// Gets a value indicating whether this element signals task completion.
    /// </summary>
    public bool TaskCompletion => false;

    /// <summary>
    /// Gets a value indicating whether this element signals task commit.
    /// </summary>
    public bool TaskCommit => false;

    /// <summary>
    /// Gets a value indicating whether this element contributes to chat history.
    /// </summary>
    public bool ChatHistory => false;

    /// <summary>
    /// Resolves the chat history text representation of the current value.
    /// </summary>
    /// <returns>The chat history text.</returns>
    public ChatHistoryText ResolveChatHistory() => ConvertChatHistoryText(ParameterType, _value);



    /// <summary>
    /// Sets the outer value from the flow computation context.
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

        EndNode = _endItem.Node as IAigcEndNode;
        ParameterType = EndNode?.TypeDef ?? TypeDefinition.Empty;
        EndType = EndNode?.EndType ?? PageCommitTypes.None;
    }


    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        var valueType = ParameterType ?? TypeDefinition.Empty;
        UpdateDefaultValue(valueType);
        _value = sync.Sync(Name, _value);
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        var valueType = ParameterType ?? TypeDefinition.Empty;
        if (TypeDefinition.IsNullOrEmpty(valueType))
        {
            return;
        }

        UpdateDefaultValue(valueType);

        var property = new ViewProperty(Name, DisplayText)
            .WithExpand()
            .WithOptional()
            .WithStatus(GetStatus());

        setup.InspectorFieldOfType(valueType, property);
    }

    /// <inheritdoc/>
    public override void UpdateFromOther(IAigcPageElement other)
    {
        if (other is PageEndElement otherOutput)
        {
            UpdateFromOther(otherOutput);
        }
    }

    /// <summary>
    /// Updates the current element's value from another <see cref="PageEndElement"/>.
    /// </summary>
    /// <param name="otherParameter">The other page end element to copy values from.</param>
    public void UpdateFromOther(PageEndElement otherParameter)
    {
        _value = otherParameter._value;
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        if (ParameterType is { } typeDef && !TypeDefinition.IsNullOrEmpty(typeDef))
        {
            _connector = node.AddConnector(Name, typeDef, FlowDirections.Output, FlowConnectorTypes.Action, false, Node?.DisplayText);
        }
        else
        {
            _connector = node.AddActionOutputConnector(Name, DisplayText);
        }
    }

    /// <inheritdoc/>
    public override bool? GetIsDone() => null;


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
