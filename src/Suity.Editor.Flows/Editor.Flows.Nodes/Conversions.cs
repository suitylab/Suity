using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.Nodes;

#region Convert

/// <summary>
/// Converts a value from one type to another.
/// </summary>
[DisplayText("Type Conversion")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 140, Height = 20)]
public class Convert : ValueFlowNode, ITextDisplay
{
    /// <summary>
    /// Input data connector.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// Output data connector.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Convert"/> class.
    /// </summary>
    public Convert()
    {
        InputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        OutputValueType = DTypeManager.Instance.CreateTypeDesignSelection();

        InputValueType.Id = NativeTypes.SingleType.TargetId;
        OutputValueType.Id = NativeTypes.SingleType.TargetId;

        UpdateConnector();
    }

    /// <summary>
    /// Gets the type selection for the input value.
    /// </summary>
    public ITypeDesignSelection InputValueType { get; private set; }

    /// <summary>
    /// Gets the type selection for the output value.
    /// </summary>
    public ITypeDesignSelection OutputValueType { get; private set; }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        InputValueType = sync.Sync("InputValueType", InputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        OutputValueType = sync.Sync("OutputValueType", OutputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(InputValueType, new ViewProperty("InputValueType", "Input Type"));
        setup.InspectorField(OutputValueType, new ViewProperty("OutputValueType", "Output Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var inputType = InputValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;
        var outputType = OutputValueType?.GetTypeDefinition() ?? TypeDefinition.Empty;

        _in = AddDataInputConnector("In", inputType.ToTypeName(), inputType.ToDisplayString());
        _out = AddDataOutputConnector("Out", outputType.ToTypeName(), outputType.ToDisplayString());
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        object value = data.GetValue(_in);
        object converted = OutputValueType.GetTypeDefinition().CreateOrRepairValue(value, false);

        data.SetValue(_out, converted);
    }

    #region ITextDisplay

    /// <inheritdoc/>
    string ITextDisplay.DisplayText => $"{InputValueType.GetTypeDefinition().ToDisplayString()} -> {OutputValueType.GetTypeDefinition().ToDisplayString()}";

    /// <inheritdoc/>
    object ITextDisplay.DisplayIcon => null;

    /// <inheritdoc/>
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion
}

#endregion

#region ConvertOrDefault

/// <summary>
/// Converts a value from one type to another, or returns a default value if conversion fails.
/// </summary>
[DisplayText("Type Conversion or Default")]
public class ConvertOrDefault : Convert
{
    private object _defaultValue;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _defaultValue = sync.Sync("DefaultValue", _defaultValue);

        UpdateDefaultValue();
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        UpdateDefaultValue();

        base.OnSetupView(setup);

        setup.InspectorFieldOfType(OutputValueType.GetTypeDefinition(), new ViewProperty("DefaultValue", "Default Value"));
    }

    /// <summary>
    /// Updates the default value to match the output type definition.
    /// </summary>
    private void UpdateDefaultValue()
    {
        var type = OutputValueType.GetTypeDefinition();

        if (!TypeDefinition.IsNullOrBroken(type))
        {
            _defaultValue = type.CreateOrRepairValue(_defaultValue, false);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        object value = data.GetValue(_in);
        if (value != null && OutputValueType.GetTypeDefinition().TryConvertValue(value, false, out object converted))
        {
            data.SetValue(_out, converted);
        }
        else
        {
            UpdateDefaultValue();
            data.SetValue(_out, _defaultValue);
        }
    }
}

#endregion

#region KeyToValue

/// <summary>
/// Retrieves a value from an asset link by its key.
/// </summary>
[DisplayText("Get Value from Link")]
public class KeyToValue : ValueFlowNode
{
    /// <summary>
    /// Input connector for the asset key.
    /// </summary>
    protected readonly FlowNodeConnector _in;

    /// <summary>
    /// Output connector for the resolved value.
    /// </summary>
    protected readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyToValue"/> class.
    /// </summary>
    public KeyToValue()
    {
        _in = AddConnector("In", "&*AssetLink|Value", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);

        OutputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        OutputValueType.Id = NativeTypes.SingleType.TargetId;
    }

    /// <summary>
    /// Gets the type selection for the output value.
    /// </summary>
    public ITypeDesignSelection OutputValueType { get; private set; }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid outputIdBefore = OutputValueType.Id;

        OutputValueType = sync.Sync("OutputValueType", OutputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);

        if (sync.IsSetterOf("OutputValueType") && OutputValueType.Id != outputIdBefore)
        {
            string outputDataType = OutputValueType.GetTypeDefinition().ToTypeName();
            _out.UpdateDataType(outputDataType);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(OutputValueType, new ViewProperty("OutputValueType", "Output Type"));
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation data)
    {
        SAssetKey key = data.GetValueConvert<SAssetKey>(_in);
        if (key is null)
        {
            data.SetValue(_out, null);
            return;
        }

        ValueAsset asset = AssetManager.Instance.GetAsset<ValueAsset>(key.Id);
        if (asset is null)
        {
            data.SetValue(_out, null);
            return;
        }

        object value = asset.GetValue(data.Context.Value, data.Context.GetArgument("ResolveContext") as ICondition);
        object converted = OutputValueType.GetTypeDefinition().CreateOrRepairValue(value, false);

        data.SetValue(_out, converted);
    }
}

#endregion
