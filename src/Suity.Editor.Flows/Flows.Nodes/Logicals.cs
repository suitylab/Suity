using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Flows.Nodes;

#region BooleanSwitch

/// <summary>
/// A flow node that selects between two input values based on a boolean condition.
/// Outputs the "True" value when the condition is true, otherwise outputs the "False" value.
/// </summary>
[DisplayText("Boolean Logic")]
public class BooleanSwitch : ValueFlowNode
{
    /// <summary>
    /// Input connector for the boolean condition value.
    /// </summary>
    protected readonly FixedNodeConnector _in;

    /// <summary>
    /// Input connector for the value to output when the condition is true.
    /// </summary>
    protected readonly FixedNodeConnector _true;

    /// <summary>
    /// Input connector for the value to output when the condition is false.
    /// </summary>
    protected readonly FixedNodeConnector _false;

    /// <summary>
    /// Output connector that emits the selected value.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanSwitch"/> class
    /// and configures the input and output connectors.
    /// </summary>
    public BooleanSwitch()
    {
        _in = AddConnector("In", "*System|Boolean", FlowDirections.Input, FlowConnectorTypes.Data);
        _true = AddConnector("True", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _false = AddConnector("False", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);

        ValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        ValueType.Id = NativeTypes.SingleType.TargetId;
    }

    /// <summary>
    /// Gets the type design selection that determines the data type of the output value.
    /// </summary>
    public ITypeDesignSelection ValueType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the output type should be an array of <see cref="ValueType"/>.
    /// </summary>
    public bool IsArray { get; private set; }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        //Guid idBefore = ValueType.Id;

        ValueType = sync.Sync(nameof(ValueType), ValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        IsArray = sync.Sync(nameof(IsArray), IsArray);

        if (sync.IsSetter())
        {
            var typeDef = ValueType.GetTypeDefinition();
            if (IsArray)
            {
                typeDef = typeDef.MakeArrayType();
            }

            string dataType = typeDef.ToTypeName();
            _true.UpdateDataType(dataType);
            _false.UpdateDataType(dataType);
            _out.UpdateDataType(dataType);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(ValueType, new ViewProperty(nameof(ValueType), "Type"));
        setup.InspectorField(ValueType, new ViewProperty(nameof(IsArray), "Array"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        bool b = data.GetValueConvert<bool>(_in);
        object value = b ? data.GetValue(_true) : data.GetValue(_false);

        var type = ValueType.GetTypeDefinition();
        if (IsArray)
        {
            //TODO: Array conversion not yet implemented
            data.SetValue(_out, value);
        }
        else
        {
            object converted = type.CreateOrRepairValue(value, false);
            data.SetValue(_out, converted);
        }
    }
}

#endregion
