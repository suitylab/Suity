using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Transferring;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows.Nodes;

#region CreateSObjectNode

/// <summary>
/// A flow node that creates a new SObject instance of a specified struct type.
/// Input connectors are dynamically generated for each public field of the struct,
/// allowing values to be assigned during object creation.
/// </summary>
[DisplayText("Create SObject")]
[NativeAlias("Suity.Editor.Flows.Nodes.CreateSObjectNode")]
public class CreateSObject : SValueFlowNode, ITextDisplay, INavigable
{
    /// <summary>
    /// The output connector that provides the created SObject.
    /// </summary>
    protected FlowNodeConnector _out;

    private readonly AssetHolder<DStruct> _structType = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSObject"/> class.
    /// Sets up event handlers for struct type changes and initializes connectors.
    /// </summary>
    public CreateSObject()
    {
        _structType.SelectionChanged += (s, e) => UpdateConnectorQueued();
        _structType.TargetUpdated += _structType_TargetUpdated;
        _structType.ListenEnabled = true;

        UpdateConnector();
    }

    private void _structType_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _structType.Selection = sync.Sync("Struct", _structType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_structType.Selection, new ViewProperty("Struct", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _structType.Target;

        _out = AddDataOutputConnector("SObject", type?.Definition, _structType.Target?.DisplayText);

        if (type != null)
        {
            foreach (var field in type.PublicStructFields)
            {
                AddDataInputConnector(field.Id, field?.FieldType, field?.Name);
            }
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var diagram = DiagramItem.Diagram;

        var type = _structType.Target;
        if (type is null)
        {
            compute.SetValue(_out, null);
            return;
        }

        SObject sobj = type.CreateObject();

        foreach (var field in type.PublicStructFields)
        {
            var conn = GetConnector(field.Id);
            if (conn is null)
            {
                continue;
            }

            if (!diagram.GetIsLinked(conn))
            {
                continue;
            }

            var prop = compute.GetValue(conn);
            prop = Cloner.Clone(prop);

            sobj.SetProperty(field.Id, prop);
        }

        compute.SetValue(_out, sobj);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var @struct = _structType.Target;

            if (@struct != null)
            {
                return $"Create {@struct.DisplayText}";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    /// <inheritdoc/>
    public override object DisplayIcon => _structType.Target?.Icon ?? base.Icon;

    #endregion

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _structType.Id;
}

#endregion

#region GetObjectProperty

/// <summary>
/// A flow node that retrieves a property value from an SObject by field name.
/// Supports type conversion of the output value to a specified type.
/// </summary>
[DisplayText("Property in SObject")]
public class GetObjectProperty : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SObject to read from.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The input connector for the property name to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _property;

    /// <summary>
    /// The output connector that provides the retrieved property value.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    private ITypeDesignSelection _outputValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetObjectProperty"/> class.
    /// Sets up input connectors for the SObject and property name, and an output connector.
    /// </summary>
    public GetObjectProperty()
    {
        _in = AddConnector("SObject", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _property = AddConnector("Property", "*System|String", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);

        _outputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outputValueType.Id = NativeTypes.SingleType.TargetId;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _outputValueType.Id;
        _outputValueType = sync.Sync("OutputValueType", _outputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? _outputValueType;
        if (sync.IsSetterOf("OutputValueType") && _outputValueType != null && _outputValueType.Id != idBefore)
        {
            var dataType = _outputValueType.GetTypeDefinition();
            _out.UpdateDataType(dataType);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_outputValueType, new ViewProperty("OutputValueType", "OutputType"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var sitem = data.GetValueConvert<SItem>(_in);
        SObject sobj;
        if (sitem is SObject s)
        {
            sobj = s;
        }
        else
        {
            sobj = sitem?.Parent as SObject;
        }

        string fieldName = data.GetValueConvert<string>(_property);

        if (sobj is null || string.IsNullOrEmpty(fieldName))
        {
            if (_outputValueType?.GetTypeDefinition() == NativeTypes.SItemType)
            {
                data.SetValue(_out, null);
            }
            else
            {
                data.SetValue(_out, _outputValueType?.GetTypeDefinition().CreateDefaultValue());
            }

            return;
        }

        if (_outputValueType?.GetTypeDefinition() == NativeTypes.SItemType)
        {
            data.SetValue(_out, sobj.GetItemFormatted(fieldName));
        }
        else
        {
            object value = sobj[fieldName];
            object convert = _outputValueType?.GetTypeDefinition().CreateOrRepairValue(value, false);
            data.SetValue(_out, convert);
        }
    }
}

#endregion

#region GetProperty

/// <summary>
/// A flow node that retrieves a specific field value from an SObject.
/// The struct type and field are selected via the inspector, and the output
/// connector type reflects the selected field's data type.
/// Supports optional type conversion for the output value.
/// </summary>
[DisplayText("Get Property")]
[SimpleFlowNodeStyle(Width = 140, Height = 20, HasHeader = false)]
[NativeAlias("Suity.Editor.Flows.Nodes.GetStructProperty")]
public class GetProperty : SValueFlowNode, ITextDisplay, INavigable
{
    /// <summary>
    /// The input connector for the SObject to read from.
    /// </summary>
    protected FlowNodeConnector _in;
    //protected NodeConnector _inSItem;

    /// <summary>
    /// The output connector that provides the retrieved field value.
    /// </summary>
    protected FlowNodeConnector _out;

    private readonly AssetHolder<DStruct> _structType = new();

    private bool _convert;

    private DStructFieldSelection _field = new();
    private ITypeDesignSelection _outputValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProperty"/> class.
    /// Sets up the output value type selection and initializes connectors.
    /// </summary>
    public GetProperty()
    {
        _outputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outputValueType.Id = NativeTypes.StringType.TargetId;

        _structType.SelectionChanged += (s, e) => UpdateConnectorQueued();
        _structType.ListenEnabled = true;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _structType.Selection = sync.Sync("Struct", _structType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        if (sync.IsSetterOf("Struct"))
        {
            _field.SelectedKey = null;
        }
        _field = sync.Sync("Field", _field, SyncFlag.NotNull);
        _field.ObjectType = _structType.Target;

        _convert = sync.Sync("Convert", _convert, SyncFlag.AffectsOthers);

        //Guid idBefore = _outputValueType.Id;
        _outputValueType = sync.Sync("OutputValueType", _outputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? _outputValueType;

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_structType.Selection, new ViewProperty("Struct", "Type"));
        if (_structType.Target != null)
        {
            setup.InspectorField(_field, new ViewProperty("Field", "Field"));
        }

        setup.InspectorField(_convert, new ViewProperty("Convert", "Convert Type"));
        if (_convert)
        {
            setup.InspectorField(_outputValueType, new ViewProperty("OutputValueType", "OutputType"));
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var structType = _structType.Target?.Definition;

        _in = AddDataInputConnector("SObject", structType, _structType.Target?.DisplayText);

        TypeDefinition dataType;
        if (_convert)
        {
            dataType = _outputValueType?.GetTypeDefinition();
        }
        else
        {
            dataType = _field?.Target?.FieldType;
        }

        _out = AddDataOutputConnector("Out", dataType, _field?.Target?.DisplayText);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SObject sobj = data.GetValueConvert<SObject>(_in);
        //if (sobj is null)
        //{
        //    SItem sitem = data.GetValueConvert<SItem>(_inSItem);

        //    if (sitem is SObject)
        //    {
        //        sobj = (SObject)sitem;
        //    }
        //    else
        //    {
        //        sobj = sitem?.Parent as SObject;
        //    }
        //}

        string fieldName = _field?.Target?.Name;
        var type = GetOutputValueType();

        if (sobj is null || string.IsNullOrEmpty(fieldName))
        {
            if (type == NativeTypes.SItemType)
            {
                data.SetValue(_out, null);
            }
            else
            {
                //data.SetValue(_out, type.CreateDefaultValue());
                data.SetValue(_out, null);
            }

            return;
        }

        if (type == NativeTypes.SItemType)
        {
            data.SetValue(_out, sobj.GetItemFormatted(fieldName));
        }
        else
        {
            object value = sobj[fieldName];
            object convert = type.CreateOrRepairValue(value, false);
            data.SetValue(_out, convert);
        }
    }

    /// <summary>
    /// Determines the output value type based on the conversion setting.
    /// Returns the user-specified type if conversion is enabled, otherwise returns the field's native type.
    /// </summary>
    /// <returns>The <see cref="TypeDefinition"/> for the output value.</returns>
    private TypeDefinition GetOutputValueType()
    {
        if (_convert)
        {
            return _outputValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;
        }
        else
        {
            return _field?.Target?.FieldType ?? TypeDefinition.Empty;
        }
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return _field?.Id ?? _structType.Id;
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var vStruct = _structType.Target;
            var vField = _field.Target;

            if (vStruct != null && vField != null)
            {
                return $"{vStruct.DisplayText}.{vField.DisplayText}";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region GetProperties

/// <summary>
/// A flow node that extracts all public field values from an SObject.
/// Output connectors are dynamically generated for each public field of the struct type,
/// providing individual access to each property value.
/// </summary>
[DisplayText("Get Multiple Properties")]
public class GetProperties : SValueFlowNode, ITextDisplay, INavigable
{
    /// <summary>
    /// The input connector for the SObject to read from.
    /// </summary>
    protected FlowNodeConnector _in;

    private readonly AssetHolder<DStruct> _structType = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProperties"/> class.
    /// Sets up event handlers for struct type changes and initializes connectors.
    /// </summary>
    public GetProperties()
    {
        _structType.SelectionChanged += (s, e) => UpdateConnectorQueued();
        _structType.TargetUpdated += _structType_TargetUpdated;
        _structType.ListenEnabled = true;

        UpdateConnector();
    }

    private void _structType_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _structType.Selection = sync.Sync("Struct", _structType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_structType.Selection, new ViewProperty("Struct", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _structType.Target;

        _in = AddDataInputConnector("SObject", type?.Definition, _structType.Target?.DisplayText);

        if (type != null)
        {
            foreach (var field in type.PublicStructFields)
            {
                AddDataOutputConnector(field.Id, field?.FieldType, field?.Name);
            }
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var diagram = DiagramItem.Diagram;

        var type = _structType.Target;
        if (type is null)
        {
            return;
        }

        SObject sobj = compute.GetValue<SObject>(_in);
        foreach (var field in type.PublicStructFields)
        {
            var conn = GetConnector(field.Id);
            if (conn is null)
            {
                continue;
            }

            if (!diagram.GetIsLinked(conn))
            {
                continue;
            }

            var prop = sobj?.GetProperty(field.Id);

            compute.SetValue(conn, prop);
        }
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return _structType.Id;
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var @struct = _structType.Target;

            if (@struct != null)
            {
                return $"Get {@struct.DisplayText} Property";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    /// <inheritdoc/>
    public override object DisplayIcon => _structType.Target?.Icon ?? base.Icon;


    #endregion
}

#endregion

#region SetProperty

/// <summary>
/// A flow node that sets a specific field value on an SObject.
/// The struct type and field are selected via the inspector, and the modified
/// SObject is passed through the output connector.
/// </summary>
[DisplayText("Set Property")]
[SimpleFlowNodeStyle(Width = 140, HasHeader = false)]
[NativeAlias("Suity.Editor.Flows.Nodes.SetStructProperty")]
public class SetProperty : SValueFlowNode, ITextDisplay, INavigable
{
    /// <summary>
    /// The input connector for the SObject to modify.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// The input connector for the value to set on the property.
    /// </summary>
    protected FlowNodeConnector _inProp;

    /// <summary>
    /// The output connector that provides the modified SObject.
    /// </summary>
    protected FlowNodeConnector _out;

    private readonly AssetHolder<DStruct> _structType = new();

    private DStructFieldSelection _field = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetProperty"/> class.
    /// Sets up event handlers for struct type changes and initializes connectors.
    /// </summary>
    public SetProperty()
    {
        _structType.SelectionChanged += (s, e) => UpdateConnectorQueued();
        _structType.ListenEnabled = true;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _structType.Selection = sync.Sync("Struct", _structType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        if (sync.IsSetterOf("Struct"))
        {
            _field.SelectedKey = null;
        }
        _field = sync.Sync("Field", _field, SyncFlag.NotNull);
        _field.ObjectType = _structType.Target;

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_structType.Selection, new ViewProperty("Struct", "Type"));
        if (_structType.Target != null)
        {
            setup.InspectorField(_field, new ViewProperty("Field", "Field"));
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var structType = _structType.Target?.Definition;

        var field = _field.Target;
        var dataType = field?.FieldType;

        _in = AddDataInputConnector("SObject", structType, _structType.Target?.DisplayText);
        _inProp = AddDataInputConnector("Value", dataType, field?.Name);

        _out = AddDataOutputConnector("Out", structType, "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        SObject sobj = compute.GetValueConvert<SObject>(_in);
        object prop = compute.GetValue(_inProp);

        string fieldName = _field?.Target?.Name;

        if (sobj is null || string.IsNullOrEmpty(fieldName))
        {
            compute.SetValue(_out, sobj);

            return;
        }

        sobj.SetProperty(fieldName, prop);
        compute.SetValue(_out, sobj);
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return _field?.Id ?? _structType.Id;
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var vStruct = _structType.Target;
            var vField = _field.Target;

            if (vStruct != null && vField != null)
            {
                return $"Set {vStruct.DisplayText}.{vField.DisplayText}";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region GetPropertyName

/// <summary>
/// A flow node that retrieves the name of an SItem.
/// </summary>
[DisplayText("Get Property Name")]
public class GetPropertyName : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SItem whose name to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the SItem's name as a string.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPropertyName"/> class.
    /// Sets up input and output connectors.
    /// </summary>
    public GetPropertyName()
    {
        _in = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|String", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SItem sitem = data.GetValueConvert<SItem>(_in);
        data.SetValue(_out, sitem?.Name);
    }
}

#endregion

#region GetSArrayItem

/// <summary>
/// A flow node that retrieves an element from an SArray by index.
/// Supports both direct array input and SArray SItem input, with optional
/// type conversion for the output value.
/// </summary>
[DisplayText("Array Element")]
public class GetSArrayItem : SValueFlowNode
{
    /// <summary>
    /// The input connector for a direct array value.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// The input connector for an SArray SItem.
    /// </summary>
    protected FlowNodeConnector _inSitem;

    /// <summary>
    /// The input connector for the zero-based index of the element to retrieve.
    /// </summary>
    protected FlowNodeConnector _index;

    /// <summary>
    /// The output connector that provides the retrieved array element.
    /// </summary>
    protected FlowNodeConnector _out;

    private ITypeDesignSelection _outputValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSArrayItem"/> class.
    /// Sets up the output value type selection and initializes connectors.
    /// </summary>
    public GetSArrayItem()
    {
        _outputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outputValueType.Id = NativeTypes.Int32Type.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _outputValueType.Id;
        _outputValueType = sync.Sync("OutputValueType", _outputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetterOf("OutputValueType") && _outputValueType.Id != idBefore)
        {
            UpdateConnector();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_outputValueType, new ViewProperty("OutputValueType", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _outputValueType?.GetTypeDefinition() ?? TypeDefinition.Unknown;
        var aryType = type.MakeArrayType();

        _in = AddDataInputConnector("Array", aryType, "Array");
        _inSitem = AddDataInputConnector("SArray", NativeTypes.SItemType, "ArraySItem");
        _index = AddDataInputConnector("Index", "*System|Int32", "Index");
        _out = AddDataOutputConnector("Out", type, "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        SItem sitem = compute.GetValueConvert<SItem>(_in) 
            ?? compute.GetValueConvert<SItem>(_inSitem);

        SArray sary;
        if (sitem is SArray s)
        {
            sary = s;
        }
        else
        {
            sary = sitem?.Parent as SArray;
        }

        if (sary is null)
        {
            if (_outputValueType?.GetTypeDefinition() == NativeTypes.SItemType)
            {
                compute.SetValue(_out, null);
            }
            else
            {
                compute.SetValue(_out, _outputValueType?.GetTypeDefinition().CreateDefaultValue());
            }

            return;
        }

        int index = compute.GetValueConvert<int>(_index);

        if (_outputValueType?.GetTypeDefinition() == NativeTypes.SItemType)
        {
            compute.SetValue(_out, sary.GetItemFormatted(index));
        }
        else
        {
            object value = sary[index];
            object convert = _outputValueType?.GetTypeDefinition().CreateOrRepairValue(value, false);
            compute.SetValue(_out, convert);
        }
    }
}

#endregion

#region GetIndex

/// <summary>
/// A flow node that retrieves the zero-based index of an SItem within its parent collection.
/// Returns -1 if the SItem is null or has no index.
/// </summary>
[DisplayText("SItem Index")]
public class GetIndex : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SItem whose index to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the SItem's index as an integer.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetIndex"/> class.
    /// Sets up input and output connectors.
    /// </summary>
    public GetIndex()
    {
        _in = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Int32", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SItem sitem = data.GetValueConvert<SItem>(_in);
        data.SetValue(_out, sitem?.Index ?? -1);
    }
}

#endregion

#region GetSItemValue

/// <summary>
/// A flow node that retrieves the underlying value from an SValue SItem.
/// The output type can be configured to convert the value to a specific type.
/// </summary>
[DisplayText("SItem Value")]
public class GetSItemValue : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SValue SItem to read from.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the extracted value.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    /// <summary>
    /// Gets the type design selection that determines the output value type.
    /// </summary>
    public ITypeDesignSelection ValueType { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSItemValue"/> class.
    /// Sets up input and output connectors with a default output type of Single.
    /// </summary>
    public GetSItemValue()
    {
        _in = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);

        ValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        ValueType.Id = NativeTypes.SingleType.TargetId;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = ValueType?.Id ?? Guid.Empty;
        ValueType = sync.Sync("ValueType", ValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? ValueType;

        if (sync.IsSetterOf("ValueType") && ValueType != null && ValueType.Id != idBefore)
        {
            _out.UpdateDataType(ValueType.GetTypeDefinition());
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(ValueType, new ViewProperty("ValueType", "Type"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var valueType = ValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;

        var svalue = data.GetValueConvert<SItem>(_in) as SValue;
        if (svalue is null)
        {
            if (valueType == NativeTypes.SItemType)
            {
                data.SetValue(_out, null);
            }
            else
            {
                data.SetValue(_out, valueType.CreateDefaultValue());
            }

            return;
        }

        if (valueType == NativeTypes.SItemType)
        {
            data.SetValue(_out, svalue);
        }
        else
        {
            object value = svalue.Value;
            object convert = valueType.CreateOrRepairValue(value, false);
            data.SetValue(_out, convert);
        }
    }
}

#endregion

#region GetSItemParent

/// <summary>
/// A flow node that retrieves the parent SItem of a given SItem.
/// </summary>
[DisplayText("SItem Parent")]
public class GetSItemParent : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SItem whose parent to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the parent SItem.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSItemParent"/> class.
    /// Sets up input and output connectors.
    /// </summary>
    public GetSItemParent()
    {
        _in = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Parent", NativeTypes.SItemType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SItem sitem = data.GetValueConvert<SItem>(_in);

        data.SetValue(_out, sitem?.Parent);
    }
}

#endregion

#region GetCallerSItem

/// <summary>
/// A flow node that retrieves the SItem from the current flow execution context.
/// This is useful for accessing the SItem that triggered the flow.
/// </summary>
[DisplayText("Caller SItem")]
public class GetCallerSItem : SValueFlowNode
{
    /// <summary>
    /// The output connector that provides the caller SItem from the execution context.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCallerSItem"/> class.
    /// Sets up the output connector.
    /// </summary>
    public GetCallerSItem()
    {
        _out = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        data.SetValue(_out, data.Context.Value as SItem);
    }
}

#endregion

#region GetCallerDataRow

/// <summary>
/// A flow node that retrieves the data row (IDataItem) associated with the current flow execution context.
/// This traverses up from the caller SItem to find the root data context.
/// </summary>
[DisplayText("Incoming Data Item")]
public class GetCallerDataRow : SValueFlowNode
{
    /// <summary>
    /// The output connector that provides the data row from the execution context.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCallerDataRow"/> class.
    /// Sets up the output connector.
    /// </summary>
    public GetCallerDataRow()
    {
        _out = AddConnector("DataRow", NativeTypes.DataRowType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var sitem = data.Context.Value as SItem;
        IDataItem dataRow = sitem?.RootContext as IDataItem;

        data.SetValue(_out, dataRow);
    }
}

#endregion

#region GetSItemDataRow

/// <summary>
/// A flow node that retrieves the data row (IDataItem) associated with a given SItem.
/// This traverses up from the SItem to find the root data context.
/// </summary>
[DisplayText("SItem Data Row")]
public class GetSItemDataRow : SValueFlowNode
{
    /// <summary>
    /// The input connector for the SItem whose data row to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the associated data row.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSItemDataRow"/> class.
    /// Sets up input and output connectors.
    /// </summary>
    public GetSItemDataRow()
    {
        _in = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("DataRow", NativeTypes.DataRowType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SItem sitem = data.GetValueConvert<SItem>(_in);
        IDataItem dataRow = sitem?.RootContext as IDataItem;

        data.SetValue(_out, dataRow);
    }
}

#endregion

#region GetDataTable

/// <summary>
/// A flow node that outputs a configured data table asset.
/// The data table is selected via an asset property in the inspector.
/// </summary>
[DisplayText("Input Data Table")]
public class InputDataTable : SValueFlowNode
{
    private readonly AssetProperty<DataTableAsset> _table
        = new("Table", "Data Table");

    private readonly FlowNodeConnector _tableOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputDataTable"/> class.
    /// Sets up the output connector for the data table.
    /// </summary>
    public InputDataTable()
    {
        _tableOut = AddDataOutputConnector("TableOut", NativeTypes.DataTableType, "Data Table");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _table.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _table.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var tableAsset = _table.Target;
        var table = tableAsset.GetDataContainer(true);

        compute.SetValue(_tableOut, table);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var tableAsset = _table.Target;

        if (tableAsset != null)
        {
            return $"Input {tableAsset.ToDisplayText()}"; 
        }
        else
        {
            return base.ToString();
        }
    }
}

#endregion

#region GetDataRow

/// <summary>
/// A flow node that retrieves a specific data row from a data table by name.
/// Supports automatic creation of the data row if it does not exist.
/// </summary>
[DisplayText("Get Data Row")]
public class GetDataRow : SValueFlowNode
{
    private readonly ConnectorAssetProperty<DataTableAsset, IDataContainer> _tableIn
        = new("TableIn", "Data Table", "The data table the data belongs to.");

    private readonly ConnectorStringProperty _rowName
        = new("RowName", "Data Name");

    private readonly ConnectorValueProperty<bool> _autoCreate
        = new("AutoCreate", "Auto Create", false, "Automatically create one when not found in the data table.");

    private readonly FlowNodeConnector _rowOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDataRow"/> class.
    /// Sets up connector properties and the output connector.
    /// </summary>
    public GetDataRow()
    {
        _tableIn.AddConnector(this);
        _rowName.AddConnector(this);
        _autoCreate.AddConnector(this);

        _rowOut = AddDataOutputConnector("RowOut", NativeTypes.DataRowType, "Data Row");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tableIn.Sync(sync);
        _rowName.Sync(sync);
        _autoCreate.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tableIn.InspectorField(setup, this);
        _rowName.InspectorField(setup, this);
        _autoCreate.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var table = _tableIn.GetTarget(compute, this, asset => asset.GetDataContainer(true));
        string name = _rowName.GetValue(compute, this);
        if (table is null || string.IsNullOrWhiteSpace(name))
        {
            compute.SetValue(_rowOut, null);
            return;
        }

        bool autoCreate = _autoCreate.GetValue(compute, this);

        var row = table?.GetData(name);
        if (row is null && autoCreate)
        {
            var target = new SObjectTransferTarget();

            target.Objects[row.Name] = null;

            ContentTransfer<SObjectTransferTarget>.GetAndInput(table, target);
            (table as ICommit)?.Commit(this);
        }

        compute.SetValue(_rowOut, row);
    }
}

#endregion

#region GetDataRowComponent

/// <summary>
/// A flow node that retrieves a specific component type from a data row.
/// Outputs both the component as its native type and as an SItem.
/// </summary>
[DisplayText("Data Row Component")]
public class GetDataRowComponent : SValueFlowNode, ITextDisplay
{
    /// <summary>
    /// The input connector for the data row to read from.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the component as its native type.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// The output connector that provides the component as an SItem.
    /// </summary>
    protected FlowNodeConnector _outSItem;

    private readonly AssetHolder<DStruct> _componentType = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDataRowComponent"/> class.
    /// Sets up event handlers for component type changes and initializes connectors.
    /// </summary>
    public GetDataRowComponent()
    {
        UpdateConnector();

        _componentType.SelectionChanged += (s, e) => _componentType_SelectionChanged();
        _componentType.ListenEnabled = true;
    }

    private void _componentType_SelectionChanged()
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _componentType.Selection = sync.Sync("ComponentType", _componentType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_componentType.Selection, new ViewProperty("ComponentType", "Component Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddConnector("DataRow", NativeTypes.DataRowType, FlowDirections.Input, FlowConnectorTypes.Data);

        _out = AddConnector("Component", _componentType.Target?.Definition, FlowDirections.Output, FlowConnectorTypes.Data);
        _outSItem = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        IDataItem row = data.GetValueConvert<IDataItem>(_in);
        var compType = _componentType.Target?.Definition;

        if (row is null || compType is null)
        {
            data.SetValue(_out, null);
            data.SetValue(_outSItem, null);

            return;
        }

        SObject obj = row.Components.Where(o => o.ObjectType == compType).FirstOrDefault();

        data.SetValue(_out, obj);
        data.SetValue(_outSItem, obj);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var comp = _componentType.Target;

            if (comp != null)
            {
                return $"{comp.DisplayText} Component";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region GetDataRowName

/// <summary>
/// A flow node that retrieves the name of a data row (IDataItem).
/// </summary>
[DisplayText("Data Row Name")]
public class GetDataRowName : SValueFlowNode, ITextDisplay
{
    /// <summary>
    /// The input connector for the data row whose name to retrieve.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// The output connector that provides the data row's name as a string.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDataRowName"/> class.
    /// Sets up input and output connectors.
    /// </summary>
    public GetDataRowName()
    {
        _in = AddConnector("DataRow", NativeTypes.DataRowType, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Name", "*System|String", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        IDataItem row = data.GetValueConvert<IDataItem>(_in);

        if (row is null)
        {
            data.SetValue(_out, null);
            return;
        }

        data.SetValue(_out, row.Name);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText => "Data Row Name";

    /// <inheritdoc/>
    public new object DisplayIcon => null;

    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Normal;

    #endregion
}

#endregion

#region DataTableInput

/// <summary>
/// A flow node that outputs a data table asset by its asset key.
/// The asset key is configured via a connector property in the inspector.
/// </summary>
[DisplayText("Input Data Table")]
public class DataTableInput : SValueFlowNode
{
    private readonly ConnectorAssetProperty<DataTableAsset, string> _assetKey
        = new("AssetKey", "Asset Key");


    private readonly FlowNodeConnector _tableOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableInput"/> class.
    /// Sets up the asset key connector property and the output connector.
    /// </summary>
    public DataTableInput()
    {
        _assetKey.AddConnector(this);

        _tableOut = AddDataOutputConnector("TableOut", NativeTypes.DataTableType, "Data Table");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _assetKey.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _assetKey.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string assetKey = _assetKey.GetTarget(compute, this, s => (s as Asset)?.AssetKey);

        var asset = AssetManager.Instance.GetAsset<DataTableAsset>(assetKey);
        IDataContainer table = asset?.GetDataContainer(true);

        compute.SetValue(_tableOut, table);
    }
}

#endregion

#region DataRowInput

/// <summary>
/// A flow node that outputs a data row asset by its asset key.
/// The asset key is configured via a connector property in the inspector.
/// </summary>
[DisplayText("Input Data Item")]
public class DataRowInput : SValueFlowNode
{
    private readonly ConnectorAssetProperty<DataRowAsset, string> _assetKey
        = new("AssetKey", "Asset Key");


    private readonly FlowNodeConnector _rowOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRowInput"/> class.
    /// Sets up the asset key connector property and the output connector.
    /// </summary>
    public DataRowInput()
    {
        _assetKey.AddConnector(this);

        _rowOut = AddDataOutputConnector("RowOut", NativeTypes.DataRowType, "Data Item");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _assetKey.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _assetKey.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string assetKey = _assetKey.GetTarget(compute, this, s => (s as Asset)?.AssetKey);

        var asset = AssetManager.Instance.GetAsset<DataRowAsset>(assetKey);
        IDataItem row = asset?.GetData(true);

        compute.SetValue(_rowOut, row);
    }
}

#endregion

#region GetDataRowLink

/// <summary>
/// A flow node that creates a data link (SKey) referencing a specific data item asset.
/// The data type and target data item are configurable via the inspector.
/// </summary>
[DisplayText("Data Item Link")]
public class GetDataRowLink : SValueFlowNode, ITextDisplay
{
    /// <summary>
    /// The output connector that provides the data link as an SKey.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private IdAssetSelection _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDataRowLink"/> class.
    /// Sets up the output connector and data link type selection.
    /// </summary>
    public GetDataRowLink()
    {
        _out = AddConnector("Link", UNKNOWN_TYPE, FlowDirections.Output, FlowConnectorTypes.Data);

        _valueType = DTypeManager.Instance.CreateDataLinkTypeDesignSelection();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _valueType?.Id ?? Guid.Empty;
        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? _valueType;
        _data = sync.Sync("Data", _data, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetterOf("ValueType") && _valueType != null && _valueType.Id != idBefore)
        {
            var dataType = _valueType.GetTypeDefinition();
            _out.UpdateDataType(dataType);
            _data.UpdateContentTypeId(_valueType.GetTypeDefinition().OriginType?.TargetId ?? Guid.Empty);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Data Type"));
        setup.InspectorField(_data, new ViewProperty("Data", "Data Item"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var dataAsset = _data.TargetAsset;
        var type = _valueType.GetTypeDefinition();

        if (dataAsset is null || type is null)
        {
            data.SetValue(_out, null);
            return;
        }

        var key = new SKey(type, dataAsset.Id);
        data.SetValue(_out, key);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var d = _data.TargetAsset;

            if (d != null)
            {
                return $"{d.DisplayText} Data Link";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region GetDataRowLink

/// <summary>
/// A flow node that resolves a data link (SKey) and retrieves the referenced data object.
/// The data type and link value are configurable via the inspector.
/// </summary>
[DisplayText("Get Data")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class GetData : SValueFlowNode
{
    /// <summary>
    /// The output connector that provides the resolved data object.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    private ITypeDesignSelection _valueType;
    private SKey _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetData"/> class.
    /// Sets up the output connector and type selection.
    /// </summary>
    public GetData()
    {
        _out = AddConnector("Data", UNKNOWN_TYPE, FlowDirections.Output, FlowConnectorTypes.Data);

        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _valueType?.Id ?? Guid.Empty;
        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? _valueType;
        _data = sync.Sync("Data", _data, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetterOf("ValueType") && _valueType != null &&  _valueType.Id != idBefore)
        {
            _out.UpdateDataType(_valueType.GetTypeDefinition());
            _data.InputType = _valueType.GetTypeDefinition().MakeDataLinkType();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Data Type"));
        setup.InspectorField(_data, new ViewProperty("Data", "Data Item"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var obj = _data.GetData();

        data.SetValue(_out, obj);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var d = _data.TargetAsset;

        if (d != null)
        {
            return $"{d.DisplayText} Data";
        }
        else
        {
            return DisplayText;
        }
    }

}

#endregion

#region LinkToSItem

/// <summary>
/// A flow node that resolves a data link (SKey) to an SObject SItem.
/// Loads the referenced data row asset and extracts the component matching the specified type.
/// </summary>
[DisplayText("Link To SItem")]
public class LinkToSItem : SValueFlowNode, ITextDisplay
{
    /// <summary>
    /// The input connector for the data link (SKey) to resolve.
    /// </summary>
    protected readonly FixedNodeConnector _in;

    /// <summary>
    /// The output connector that provides the resolved SObject as an SItem.
    /// </summary>
    protected readonly FlowNodeConnector _out;

    private ITypeDesignSelection _valueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkToSItem"/> class.
    /// Sets up input and output connectors with a data link type selection.
    /// </summary>
    public LinkToSItem()
    {
        _in = AddConnector("Link", UNKNOWN_TYPE, FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("SItem", NativeTypes.SItemType, FlowDirections.Output, FlowConnectorTypes.Data);

        _valueType = DTypeManager.Instance.CreateDataLinkTypeDesignSelection();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _valueType?.Id ?? Guid.Empty;
        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers) ?? _valueType;
        if (sync.IsSetterOf("ValueType") && _valueType != null && _valueType.Id != idBefore)
        {
            _in.UpdateDataType(_valueType.GetTypeDefinition());
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Data Type"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        var type = _valueType?.GetTypeDefinition()?.ElementType;
        SKey key = data.GetValueConvert<SKey>(_in);

        if (type is null || key is null)
        {
            data.SetValue(_out, null);
            return;
        }

        DataRowAsset dataAsset = AssetManager.Instance.GetAsset<DataRowAsset>(key.Id);
        dataAsset?.GetDocumentEntry(true);
        IDataItem row = dataAsset?.GetStorageObject(true) as IDataItem;
        SObject obj = row?.Components.Where(o => o.ObjectType == type).FirstOrDefault();

        data.SetValue(_out, obj);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var type = _valueType?.GetTypeDefinition()?.ElementType;

            if (type != null)
            {
                return $"{type.ToDisplayString()} To SItem";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region EnumSwitch

/// <summary>
/// A flow node that acts as a switch based on an enum value.
/// Input connectors are dynamically generated for each enum field, and the node
/// outputs the value from the connector matching the input enum value, or the default if no match.
/// </summary>
[DisplayText("Enum Branch", "*CoreIcon|Enum")]
public class EnumSwitch : SValueFlowNode, ITextDisplay
{
    private readonly AssetHolder<DEnum> _enumType = new();

    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private FlowNodeConnector _default;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumSwitch"/> class.
    /// Sets up event handlers for enum type changes and initializes connectors.
    /// </summary>
    public EnumSwitch()
    {
        _enumType.TargetUpdated += _enumType_TargetUpdated;
        _enumType.ListenEnabled = true;

        ValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        ValueType.Id = NativeTypes.SingleType.TargetId;

        UpdateConnector();
    }

    private void _enumType_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <summary>
    /// Gets or sets the enum type asset selection that determines the switch cases.
    /// </summary>
    public AssetSelection<DEnum> EnumType
    {
        get => _enumType.Selection;
        set => _enumType.Selection = value;
    }

    /// <summary>
    /// Gets the type design selection that determines the data type for input and output values.
    /// </summary>
    public ITypeDesignSelection ValueType { get; private set; }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        //Guid idBefore = ValueType.Id;

        EnumType = sync.Sync("EnumType", EnumType, SyncFlag.NotNull) ?? EnumType;
        ValueType = sync.Sync("ValueType", ValueType, SyncFlag.NotNull) ?? ValueType;

        //if (sync.IsSetterOf("ValueType") && ValueType.Id != idBefore)
        //{
        //    string dataType = ValueType.GetTypeDefinition().ToAssetString();
        //    _out.UpdateDataType(dataType);
        //}

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(EnumType, new ViewProperty("EnumType", "Enum"));
        setup.InspectorField(ValueType, new ViewProperty("ValueType", "Value Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var dataType = ValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;

        var en = EnumType.GetTarget();
        if (en != null)
        {
            var enumType = en.Definition;
            _in = AddConnector("In", enumType, FlowDirections.Input, FlowConnectorTypes.Data);

            foreach (var item in en.EnumFields)
            {
                AddConnector(item.Id, dataType, FlowDirections.Input, FlowConnectorTypes.Data, false, item.DisplayText);
            }
        }
        else
        {
            _in = AddConnector("In", FlowNode.UNKNOWN_TYPE, FlowDirections.Input, FlowConnectorTypes.Data);
        }

        _default = AddDataInputConnector("Default", dataType, "Default");

        _out = AddConnector("Out", dataType, FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        FlowNodeConnector conn = null;

        do
        {
            SEnum en = data.GetValueConvert<SEnum>(_in);
            if (en is null)
            {
                break;
            }

            string enumId = en.Field?.Id.ToString();
            if (string.IsNullOrWhiteSpace(enumId))
            {
                break;
            }

            conn = GetConnector(enumId);
        } while (false);

        object value;
        if (conn != null)
        {
            value = data.GetValue(conn);
        }
        else
        {
            value = data.GetValue(_default);
        }

        data.SetValue(_out, value);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var d = _enumType.TargetAsset;

            if (d != null)
            {
                return $"{d.DisplayText} Branch";
            }
            else
            {
                return base.DisplayText;
            }
        }
    }

    #endregion
}

#endregion

#region StructNode

/// <summary>
/// A flow node that outputs a reference to a struct type definition (DType).
/// The struct type is selected via an asset selection in the inspector.
/// </summary>
[DisplayText("Get Struct", "*CoreIcon|Structure")]
public class StructNode : SValueFlowNode
{
    /// <summary>
    /// The asset selection for the struct type to output.
    /// </summary>
    public readonly AssetSelection<DType> _selection;

    private readonly FlowNodeConnector _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructNode"/> class.
    /// Sets up the asset selection and output connector.
    /// </summary>
    public StructNode()
    {
        _selection = new AssetSelection<DType>();
        _output = AddDataOutputConnector("Output", "&*AssetLink|DStruct", "Struct");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StructNode"/> class with a specified type.
    /// </summary>
    /// <param name="type">The DType to set as the selected struct type.</param>
    public StructNode(DType type)
        : this()
    {
        _selection.Target = type;
    }

    /// <summary>
    /// Gets the selected DType asset.
    /// </summary>
    public DType Type => _selection.Target;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        // Although set to read-only, the synchronizer still attempts to write properties one by one
        sync.Sync(nameof(Type), _selection, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_selection, new ViewProperty(nameof(Type), "Struct Type"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_output, Type);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_selection.Target != null)
        {
            return _selection.Target.DisplayText;
        }
        else
        {
            return "Get Struct";
        }
    }
}

#endregion

// Settings ====================================================

#region SetDataRow

/// <summary>
/// A flow node that sets an SObject component into a specified data row.
/// Throws an exception if the data row or component is not specified.
/// </summary>
[DisplayText("Set Data Row")]
public class SetDataRow : SValueFlowNode
{
    /// <summary>
    /// The action input connector that triggers the operation.
    /// </summary>
    private readonly FlowNodeConnector _in;

    private readonly ConnectorAssetProperty<DataRowAsset, IDataItem> _rowIn
        = new("RowIn", "Data Row");

    /// <summary>
    /// The input connector for the SObject component to set.
    /// </summary>
    private readonly FlowNodeConnector _component;


    /// <summary>
    /// The action output connector that signals completion.
    /// </summary>
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetDataRow"/> class.
    /// Sets up input, component, and output connectors.
    /// </summary>
    public SetDataRow()
    {
        _in = AddActionInputConnector("In", "Input");
        _rowIn.AddConnector(this);
        _component = AddDataInputConnector("Component", NativeTypes.ObjectType, "Component");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _rowIn.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _rowIn.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var row = _rowIn.GetTarget(compute, this, asset => asset.GetData(true)) 
            ?? throw new NullReferenceException("Data row not specified");

        var comp = compute.GetValue<SObject>(_component) 
            ?? throw new NullReferenceException("Data not specified");

        var target = new SObjectTransferTarget
        {
            ObjectType = comp.ObjectType,
        };

        target.Objects[row.Name] = comp;

        // Operate on a data row
        ContentTransfer<SObjectTransferTarget>.GetAndInput(row, target);
        (row as ICommit)?.Commit(this);

        compute.SetResult(this, _out);
    }
}

#endregion

#region AddDataRow
/// <summary>
/// A flow node that adds a new data row to a data table with an SObject component.
/// If no name is specified, one will be automatically created. When auto-create requires
/// filling in name and description, it will reference the auto-fill field in the data object.
/// </summary>
[DisplayText("Add Data Row", "*CoreIcon|Row")]
[ToolTipsText("Sets an object into a data row. If no name is specified, one will be automatically created. When auto-create requires filling in name and description, it will reference the auto-fill field in the data object.")]
public class AddDataRow : SValueFlowNode
{
    /// <summary>
    /// The action input connector that triggers the operation.
    /// </summary>
    private readonly FlowNodeConnector _in;

    /// <summary>
    /// The connector property for selecting the target data table.
    /// </summary>
    private ConnectorAssetProperty<DataTableAsset, IDataContainer> _table = new("Table", "Data Table");

    /// <summary>
    /// The input connector for the SObject component to add.
    /// </summary>
    private readonly FlowNodeConnector _component;

    /// <summary>
    /// The action output connector that signals completion.
    /// </summary>
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDataRow"/> class.
    /// Sets up input, table, component, and output connectors.
    /// </summary>
    public AddDataRow()
    {
        _in = AddActionInputConnector("In", "Input");
        _table.AddConnector(this);
        _component = AddDataInputConnector("Component", NativeTypes.ObjectType, "Component");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _table.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _table.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var table = _table.GetTarget(compute, this, o => o.GetStorageObject(true) as IDataContainer)
            ?? throw new NullReferenceException("Table not specified");

        var comp = compute.GetValue<SObject>(_component)
            ?? throw new NullReferenceException("Data not specified");

        var target = new SObjectTransferTarget
        {
            ObjectType = comp.ObjectType,
        };

        target.Objects[string.Empty] = comp;

        // Operate on a data row
        ContentTransfer<SObjectTransferTarget>.GetAndInput(table, target);
        (table as ICommit)?.Commit(this);

        compute.SetResult(this, _out);
    }
} 
#endregion
