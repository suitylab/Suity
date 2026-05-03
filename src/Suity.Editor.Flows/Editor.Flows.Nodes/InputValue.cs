using Suity.Drawing;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;
using System.Drawing;

namespace Suity.Editor.Flows.Nodes;

#region InputValue

/// <summary>
/// A flow node that holds and outputs a configurable value of a user-selected type.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Input Value", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.ValueNode")]
public class InputValue : ValueFlowNode, IViewDoubleClickAction
{
    private FlowNodeConnector _out;
    private object _value;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type design selection that determines the data type of the stored value.
    /// </summary>
    public ITypeDesignSelection ValueType => _valueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputValue"/> class with a default string value.
    /// </summary>
    public InputValue()
    {
        //_out = AddConnector("Out", "string", ConnectionDirections.Output, ConnectionTypes.Data);

        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.StringType.TargetId;
        Value = NativeTypes.StringType.CreateOrRepairValue(Value, false);

        base.FlowNodeGui = OnGui;

        UpdateConnector();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputValue"/> class bound to the specified asset.
    /// </summary>
    /// <param name="asset">The asset to reference as the node's value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset"/> is null.</exception>
    public InputValue(Asset asset)
        : this()
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        var dAssetLink = AssetManager.Instance.GetAssetLink(asset.GetType())
            ?? throw new NullReferenceException(nameof(asset));

        string key = "&" + dAssetLink.AssetKey;

        _valueType.SelectedKey = key;

        _value = new SAssetKey(dAssetLink.Definition, asset.Id);
    }

    /// <summary>
    /// Gets or sets the raw value stored by this node.
    /// </summary>
    public object Value
    {
        get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets the <see cref="TypeDefinition"/> that represents the current value type, accounting for array configuration.
    /// </summary>
    /// <returns>The resolved type definition, or <see cref="TypeDefinition.Empty"/> if the type cannot be determined.</returns>
    public TypeDefinition GetTypeDefinition()
    {
        var type = _valueType?.GetTypeDefinition();
        if (_isArray)
        {
            type = type?.MakeArrayType();
        }

        return type ?? TypeDefinition.Empty;
    }

    /// <inheritdoc/>
    public override ImageDef Icon => Value?.ToDisplayIcon() ?? base.Icon;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _valueType?.Id ?? Guid.Empty;

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);
        Value = sync.Sync("Value", Value);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            //string dataType = GetTypeDefinition().ToAssetString();
            //_out.UpdateDataType(dataType);
            UpdateConnectorQueued();
        }

        UpdateDefaultValue();
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        UpdateDefaultValue();

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));

        if (_valueType != null)
        {
            setup.InspectorFieldOfType(GetTypeDefinition(), new ViewProperty("Value", "Value").WithOptional());
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var typeDef = GetTypeDefinition();
        if (typeDef.IsAssetLink)
        {
            typeDef = TypeDefinition.FromNative(typeDef.NativeType) ?? typeDef;
        }

        string dataTypeStr = typeDef?.ToTypeName() ?? "???";

        _out = AddConnector("Out", dataTypeStr, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    private void UpdateDefaultValue()
    {
        var type = GetTypeDefinition();

        if (!TypeDefinition.IsNullOrBroken(type))
        {
            Value = type.CreateOrRepairValue(Value, true);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (_value is SKey key)
        {
            compute.SetValue(_out, key.TargetAsset);
        }
        else if (_value is SAssetKey assetKey)
        {
            compute.SetValue(_out, assetKey.TargetAsset);
        }
        else
        {
            compute.SetValue(_out, _value);
        }
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = EditorUtility.GetBriefStringL(_value);
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there's a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_out, context, text)
        .InitInputDoubleClicked(_ => 
        {
            if (_value != null)
            {
                EditorUtility.GotoDefinition(_value);
            }
        });
    }

    #region IViewDoubleClickAction

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        if (_value != null)
        {
            EditorUtility.GotoDefinition(_value);
        }
    }

    #endregion

    /// <inheritdoc/>
    public override string DisplayText => _value?.ToString() ?? base.Name;
}

#endregion

#region PassValue

/// <summary>
/// A flow node that passes a value from its input connector directly to its output connector without modification.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Pass Value", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.ValuePassNode")]
public class PassValue : ValueFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private readonly TypeDesignSelection _valueType = new();
    private readonly ValueProperty<bool> _isArray = new("IsArray", "Array");

    /// <summary>
    /// Initializes a new instance of the <see cref="PassValue"/> class with a default string data type.
    /// </summary>
    public PassValue()
    {
        _valueType.Id = NativeTypes.StringType.TargetId;
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _valueType, SyncFlag.GetOnly);
        _isArray.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("DataType", "Data Type"));
        _isArray.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _valueType.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray.Value)
        {
            type = type.MakeArrayType();
        }

        _in = AddDataInputConnector("In", type, " ");
        _out = AddDataOutputConnector("Out", type, " ");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var value = compute.GetValue(_in);

        compute.SetValue(_out, value);
    }
}

#endregion

#region PassAction

/// <summary>
/// A flow node that passes an execution action from its input connector directly to its output connector.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Pass Action", "*CoreIcon|Action")]
public class PassAction : ValueFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="PassAction"/> class.
    /// </summary>
    public PassAction()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddActionInputConnector("In", " ");
        _out = AddActionOutputConnector("Out", " ");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetResult(this, _out);
    }
}

#endregion

#region CloneObject

/// <summary>
/// A flow node that clones the input value and outputs the cloned copy.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Clone Object", "*CoreIcon|Object")]
public class CloneObject : ValueFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private readonly ValueProperty<bool> _isArray = new("IsArray", "Array");

    /// <summary>
    /// Initializes a new instance of the <see cref="CloneObject"/> class with a default string value type.
    /// </summary>
    public CloneObject()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        _isArray.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _valueType.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray.Value)
        {
            type = type.MakeArrayType();
        }

        _in = AddDataInputConnector("In", type, " ");
        _out = AddDataOutputConnector("Out", type, " ");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (_isArray.Value)
        {
            var values = compute.GetValues(_in, true) ?? [];
            for (int i = 0; i < values.Length; i++)
            {
                var value = Cloner.Clone(values[i]);
                values[i] = value;
            }

            compute.SetValue(_out, values);
        }
        else
        {
            var value = compute.GetValue(_in);
            value = Cloner.Clone(value);
            compute.SetValue(_out, value);
        }
    }
}

#endregion

#region Null

/// <summary>
/// A flow node that outputs a null value.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Null Value", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.NullNode")]
public class Null : ValueFlowNode
{
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Null"/> class.
    /// </summary>
    public Null()
    {
        _out = AddConnector("Null", "object", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, null);
    }
}

#endregion

#region IsNull

/// <summary>
/// A flow node that checks whether an input value is null and outputs a boolean result.
/// </summary>
[SimpleFlowNodeStyle(Width = 100, Height = 20, HasHeader = false)]
[DisplayText("Check if Null", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.IsNullNode")]
public class IsNull : ValueFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNull"/> class.
    /// </summary>
    public IsNull()
    {
        _in = AddDataInputConnector("In", "object", "Value");
        _out = AddDataOutputConnector("Out", "bool", "Is Null");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var obj = compute.GetValue(_in);
        compute.SetValue(_out, obj is null);
    }
}

#endregion

#region ComposeArray

/// <summary>
/// A flow node that collects multiple input values of a specified type into an array.
/// </summary>
[SimpleFlowNodeStyle(Width = 100, Height = 20, HasHeader = false)]
[DisplayText("Compose Array", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.ComposeArray")]
public class ComposeArray : ValueFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    /// <summary>
    /// Gets the type design selection that determines the element type of the output array.
    /// </summary>
    public ITypeDesignSelection ValueType { get; private set; }

    /// <summary>
    /// Gets or sets whether the output connector uses a multi-value port instead of a single array output.
    /// </summary>
    public bool MultipleConnector { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComposeArray"/> class with a default string element type.
    /// </summary>
    public ComposeArray()
    {
        ValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        ValueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        ValueType = sync.Sync(nameof(ValueType), ValueType, SyncFlag.NotNull);
        MultipleConnector = sync.Sync(nameof(MultipleConnector), MultipleConnector, SyncFlag.NotNull);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(ValueType, new ViewProperty(nameof(ValueType), "Type"));
        setup.InspectorField(MultipleConnector, new ViewProperty(nameof(MultipleConnector), "Multi Value Port"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = ValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;
        string typeName = type.ToTypeName();

        _in = AddConnector("In", typeName, FlowDirections.Input, FlowConnectorTypes.Data, true, "Element");
        if (MultipleConnector)
        {
            _out = AddDataOutputConnector("Out", typeName, "Multi Value");
        }
        else
        {
            var aryType = type.MakeArrayType();
            _out = AddDataOutputConnector("Out", aryType.ToTypeName(), "Array");
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var values = compute.GetValues(_in, true);

        var type = ValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;
        var aryType = type.MakeArrayType();

        var ary = new SArray(aryType);
        foreach (var value in values)
        {
            ary.Add(value);
        }

        compute.SetValue(_out, ary);
    }
}


#endregion

#region BooleanSwitchValue

/// <summary>
/// A flow node that selects one of two input values based on a boolean condition and outputs the selected value.
/// </summary>
//[SimpleFlowNodeStyle(Width = 150, Height = 80, HasHeader = false)]
[DisplayText("Boolean Branch Value", "*CoreIcon|Value")]
[NativeAlias("Suity.Editor.Flows.Nodes.BooleanSwitchValue")]
public class BooleanSwitchValue : ValueFlowNode
{
    private FlowNodeConnector _booleanIn;
    private FlowNodeConnector _valueTrue;
    private FlowNodeConnector _valueFalse;
    private FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Gets the type design selection that determines the data type of the switch values.
    /// </summary>
    public ITypeDesignSelection ValueType => _valueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanSwitchValue"/> class with a default string value type.
    /// </summary>
    public BooleanSwitchValue()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var typeDef = GetTypeDefinition();
        string dataTypeStr = typeDef?.ToTypeName() ?? "???";

        _booleanIn = AddDataInputConnector("Boolean", "bool", "Condition");
        _valueTrue = AddDataInputConnector("True", dataTypeStr, "True Value");
        _valueFalse = AddDataInputConnector("False", dataTypeStr, "False Value");
        _out = AddDataOutputConnector("Out", dataTypeStr, "Output");
    }

    private TypeDefinition GetTypeDefinition()
    {
        var type = _valueType?.GetTypeDefinition();
        if (_isArray)
        {
            type = type?.MakeArrayType();
        }
        return type ?? TypeDefinition.Empty;
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool condition = compute.GetValue<bool>(_booleanIn);
        var value = condition ? compute.GetValue(_valueTrue) : compute.GetValue(_valueFalse);
        compute.SetValue(_out, value);
    }
}

#endregion

#region FirstNotNull

/// <summary>
/// A flow node that evaluates multiple input values and outputs the first non-null value found.
/// </summary>
[DisplayText("First Not Null", "*CoreIcon|Object")]
public class FirstNotNullValue : ValueFlowNode
{
    private FlowNodeConnector _in;

    private FlowNodeConnector _result;
    private FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private readonly ValueProperty<bool> _isArray = new("IsArray", "Array");

    private readonly ValueProperty<int> _valueCount = new("ValueCount", "Value Count");

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstNotNullValue"/> class with a default string value type.
    /// </summary>
    public FirstNotNullValue()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray.Sync(sync);
        _valueCount.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type"));
        _isArray.InspectorField(setup);
        _valueCount.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _valueType.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray.Value)
        {
            type = type.MakeArrayType();
        }

        _in = AddActionInputConnector("In", "Input");

        int count = _valueCount.Value;
        for (int i = 0; i < count; i++)
        {
            AddDataInputConnector("Value" + i, type, $"[{i}]");
        }

        _result = AddDataOutputConnector("Out", type, " ");
        _out = AddActionOutputConnector("In", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        int count = _valueCount.Value;
        for (int i = 0; i < count; i++)
        {
            var connector = GetConnector("Value" + i);
            if (connector is null)
            {
                continue;
            }

            var value = compute.GetValue(connector);
            if (value != null)
            {
                compute.SetValue(_result, value);
                compute.SetResult(this, _out);
                return;
            }
        }

        compute.SetValue(_result, null);
        compute.SetResult(this, _out);
    }
}

#endregion
