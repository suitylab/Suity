using Suity;
using Suity.Collections;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;

#region Return

/// <summary>
/// Completes the execution of an action sequence and returns the result value.
/// </summary>
[SimpleFlowNodeStyle(Color = "#13A839")]
[DisplayText("Return", "*CoreIcon|Return")]
[ToolTipsText("Complete the execution of this action sequence and return the result value. Note: Cannot return value in loops.")]
public class Return : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _result;

    /// <summary>
    /// Gets or sets whether to combine action and data into one port.
    /// </summary>
    private readonly ValueProperty<bool> _dataActionMode
        = new("DataActionMode", "Data-Action Mode", toolTips: "Combine action and data into one port, used for connecting data-action flows.");

    /// <summary>
    /// Initializes a new instance of the <see cref="Return"/> class.
    /// </summary>
    public Return()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _dataActionMode.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _dataActionMode.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        if (_dataActionMode.Value)
        {
            _in = AddConnector("In", "object", FlowDirections.Input, FlowConnectorTypes.Action, false, "Input");
            _result = null;
        }
        else
        {
            _in = AddActionInputConnector("In", "Input");
            _result = AddDataInputConnector("Result", "object", "Result");
        }
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        object result = _dataActionMode.Value ? compute.GetValue(_in) : compute.GetValue(_result);

        return Task.FromResult(result);
    }
}

#endregion

#region ForeachJsonArray

/// <summary>
/// Iterates through all elements in a JSON array and executes an action for each element.
/// </summary>
[DisplayText("Json Array Element Action", "*CoreIcon|Array")]
[ToolTipsText("Iterate through all elements in Json and execute an action for each element.")]
public class ForeachJsonArray : ActionFlowNode
{
    private FlowNodeConnector _input;
    private FlowNodeConnector _array;

    private FlowNodeConnector _itemOutput;
    private FlowNodeConnector _index;
    private FlowNodeConnector _item;
    private FlowNodeConnector _output;

    /// <summary>
    /// Gets or sets the JSON content type for the array elements.
    /// </summary>
    public JsonContentTypes ContentType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeachJsonArray"/> class.
    /// </summary>
    public ForeachJsonArray()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        ContentType = sync.Sync(nameof(ContentType), ContentType);

        if (sync.IsSetterOf(nameof(ContentType)))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(ContentType, new ViewProperty(nameof(ContentType), "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        _input = AddActionInputConnector("Input", "Input Action");
        _array = AddDataInputConnector("Text", JsonFlowNode.JsonData, "Array");

        _itemOutput = AddActionOutputConnector("ItemOutput", "Member Action");
        _index = AddDataOutputConnector("Index", "int", "Index");
        _item = AddDataOutputConnector("Item", JsonFlowNode.GetJsonDataType(ContentType), "Element");
        _output = AddActionOutputConnector("Output", "Complete Action");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        if (compute.GetValue(_array) is not IDataReader reader)
        {
            return null;
        }

        int i = 0;
        foreach (var childReader in reader.Array())
        {
            compute.InvalidateOutputs(this);

            compute.SetValue(_index, i);
            compute.SetValue(_item, childReader);

            await compute.RunAction(_itemOutput, cancel);

            i++;
        }

        return _output;
    }
}

#endregion

#region ForeachArray

/// <summary>
/// Iterates through all elements in an array and executes an action for each element.
/// </summary>
[NativeAlias("Suity.Editor.Flows.Nodes.SArrayElementAction")]
[DisplayText("Array Element Action", "*CoreIcon|Array")]
[ToolTipsText("Iterate through all elements in the array and execute an action for each element.")]
[NativeAlias("Suity.Editor.Flows.Nodes.ForeachSArray", UseForSaving = false)]
public class ForeachArray : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _array;
    private FlowNodeConnector _arraySitem;

    /// <summary>
    /// Gets or sets the starting index for iteration.
    /// </summary>
    private readonly ConnectorValueProperty<int> _startIndex = new("StartIndex", "Start Index");

    private FlowNodeConnector _itemOutput;
    private FlowNodeConnector _item;
    private FlowNodeConnector _count;
    private FlowNodeConnector _index;
    private FlowNodeConnector _output;

    private ITypeDesignSelection _outputValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeachArray"/> class.
    /// </summary>
    public ForeachArray()
    {
        _outputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outputValueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _startIndex.Sync(sync);

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

        _startIndex.InspectorField(setup, this);
        setup.InspectorField(_outputValueType, new ViewProperty("OutputValueType", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _outputValueType?.GetTypeDefinition() ?? TypeDefinition.Unknown;
        var aryType = type.MakeArrayType();

        _in = AddActionInputConnector("In", "Input");

        _array = AddDataInputConnector("Array", aryType.ToTypeName(), "Array");
        _arraySitem = AddDataInputConnector("SArray", NativeTypes.SItemType.ToTypeName(), "SArray Item");
        _startIndex.AddConnector(this);

        _itemOutput = AddActionOutputConnector("ItemOutput", "Member Action");
        _item = AddDataOutputConnector("Item", type.ToTypeName(), "Element");
        _count = AddDataOutputConnector("Count", "int", "Total Count");
        _index = AddDataOutputConnector("Index", "int", "Current Index");
        _output = AddActionOutputConnector("Output", "Output");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var ary = compute.GetValueConvert<Array>(_array);
        var sitem = compute.GetValueConvert<SItem>(_array) ?? compute.GetValueConvert<SItem>(_arraySitem);

        int startIndex = _startIndex.GetValue(compute, this);

        SArray sary;
        if (sitem is SArray s)
        {
            sary = s;
        }
        else
        {
            sary = sitem?.Parent as SArray;
        }

        if (ary is null && sary is null)
        {
            return null;
        }

        if (ary != null)
        {
            compute.SetValue(_count, ary.Length);

            for (int i = startIndex; i < ary.Length; i++)
            {
                var item = ary.GetValue(i);

                compute.InvalidateOutputs(this);

                compute.SetValue(_index, i);
                compute.SetValue(_item, item);

                await compute.RunAction(_itemOutput, cancel);

                cancel.ThrowIfCancellationRequested();
            }
        }
        else if (sary != null)
        {
            compute.SetValue(_count, sary.Count);

            for (int i = startIndex; i < sary.Count; i++)
            {
                var item = sary[i];

                compute.InvalidateOutputs(this);

                compute.SetValue(_index, i);
                compute.SetValue(_item, item);

                await compute.RunAction(_itemOutput, cancel);

                cancel.ThrowIfCancellationRequested();
            }
        }

        return _output;
    }
}

#endregion

#region ForeachArraySequence

/// <summary>
/// Splits array elements by count and executes an action for each sequence.
/// </summary>
[DisplayText("Array Sequence Action", "*CoreIcon|Array")]
[ToolTipsText("Split array elements by count and execute an action for each sequence.")]
public class ForeachArraySequence : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _array;
    private FlowNodeConnector _arraySitem;

    /// <summary>
    /// Gets or sets the length of each sequence.
    /// </summary>
    private readonly ConnectorValueProperty<int> _sequenceLength = new("SequenceLength", "Sequence Length", 10, "Specify the array length contained in each sequence");

    /// <summary>
    /// Gets or sets the starting index for iteration.
    /// </summary>
    private readonly ConnectorValueProperty<int> _startIndex = new("StartIndex", "Start Index");

    private FlowNodeConnector _itemOutput;
    private FlowNodeConnector _arraySequence;
    private FlowNodeConnector _count;
    private FlowNodeConnector _index;
    private FlowNodeConnector _output;

    private ITypeDesignSelection _outputValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeachArraySequence"/> class.
    /// </summary>
    public ForeachArraySequence()
    {
        _outputValueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outputValueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Guid idBefore = _outputValueType.Id;
        _outputValueType = sync.Sync("OutputValueType", _outputValueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        
        _sequenceLength.Sync(sync);
        _startIndex.Sync(sync);

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

        _sequenceLength.InspectorField(setup, this);
        _startIndex.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _outputValueType?.GetTypeDefinition() ?? TypeDefinition.Unknown;
        var aryType = type.MakeArrayType();

        _in = AddActionInputConnector("In", "Input");

        _array = AddDataInputConnector("Array", aryType.ToTypeName(), "Array");
        _arraySitem = AddDataInputConnector("SArray", NativeTypes.SItemType.ToTypeName(), "SArray Item");

        _sequenceLength.AddConnector(this);
        _startIndex.AddConnector(this);

        _itemOutput = AddActionOutputConnector("ItemOutput", "Member Action");
        _arraySequence = AddDataOutputConnector("ArraySequence", aryType.ToTypeName(), "Array Sequence");
        _count = AddDataOutputConnector("Count", "int", "Total Count");
        _index = AddDataOutputConnector("Index", "int", "Current Index");
        _output = AddActionOutputConnector("Output", "Output");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Array ary = compute.GetValueConvert<Array>(_array);
        SItem sitem = compute.GetValueConvert<SItem>(_array) ?? compute.GetValueConvert<SItem>(_arraySitem);

        int startIndex = _startIndex.GetValue(compute, this);

        SArray sary;
        if (sitem is SArray s)
        {
            sary = s;
        }
        else
        {
            sary = sitem?.Parent as SArray;
        }

        if (ary is null && sary is null)
        {
            return null;
        }

        int seqLen = _sequenceLength.GetValue(compute, this);
        if (seqLen <= 0)
        {
            seqLen = 1;
        }

        List<object> list = [];
        int seqIndex = startIndex;

        if (ary != null)
        {
            int count = ary.Length / seqLen;
            if (ary.Length % seqLen > 0)
            {
                count++;
            }
            compute.SetValue(_count, count);

            for (int i = startIndex * seqLen; i < ary.Length; i++)
            {
                var item = ary.GetValue(i);
                list.Add(item);

                if (list.Count == seqLen)
                {
                    compute.InvalidateOutputs(this);

                    compute.SetValue(_index, seqIndex);
                    compute.SetValue(_arraySequence, list.ToArray());

                    await compute.RunAction(_itemOutput, cancel);

                    cancel.ThrowIfCancellationRequested();

                    list.Clear();
                    seqIndex++;
                }
            }

            if (list.Count > 0)
            {
                compute.InvalidateOutputs(this);

                compute.SetValue(_index, seqIndex);
                compute.SetValue(_arraySequence, list.ToArray());

                await compute.RunAction(_itemOutput, cancel);

                cancel.ThrowIfCancellationRequested();
            }
        }
        else if (sary != null)
        {
            int count = sary.Count / seqLen;
            if (sary.Count % seqLen > 0)
            {
                count++;
            }
            compute.SetValue(_count, count);

            for (int i = startIndex * seqLen; i < sary.Count; i++)
            {
                var item = sary[i];
                list.Add(item);

                if (list.Count == seqLen)
                {
                    compute.InvalidateOutputs(this);

                    compute.SetValue(_index, seqIndex);
                    compute.SetValue(_arraySequence, list.ToArray());

                    await compute.RunAction(_itemOutput, cancel);

                    cancel.ThrowIfCancellationRequested();

                    list.Clear();
                    seqIndex++;
                }
            }

            if (list.Count > 0)
            {
                compute.InvalidateOutputs(this);

                compute.SetValue(_index, seqIndex);
                compute.SetValue(_arraySequence, list.ToArray());

                await compute.RunAction(_itemOutput, cancel);

                cancel.ThrowIfCancellationRequested();
            }
        }

        return _output;
    }
}

#endregion

#region ForTo



#endregion

#region DelayAction

/// <summary>
/// Delays execution for a specified duration before proceeding to the next action.
/// </summary>
[NativeAlias("Suity.Editor.Flows.Nodes.DelayActionNode")]
[DisplayText("Delay", "*CoreIcon|Time")]
[ToolTipsText("Delay for a specified duration before executing the next action.")]
public class DelayAction : ActionFlowNode
{
    private readonly FlowNodeConnector _input;
    private readonly FlowNodeConnector _output;

    /// <summary>
    /// Gets or sets the delay duration in seconds.
    /// </summary>
    public float DelaySecond { get; set; } = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayAction"/> class.
    /// </summary>
    public DelayAction()
    {
        _input = AddActionInputConnector("Input", "Input");
        _output = AddActionOutputConnector("Output", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        DelaySecond = sync.Sync(nameof(DelaySecond), DelaySecond);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(DelaySecond, new ViewProperty(nameof(DelaySecond), "Delay").WithUnit("seconds"));
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        if (DelaySecond > 0)
        {
            await Task.Delay((int)(DelaySecond * 1000), cancel);
        }

        cancel.ThrowIfCancellationRequested();

        return _output;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Delay {DelaySecond} seconds";
    }
}

#endregion

#region ThrowError

/// <summary>
/// Throws an error exception to terminate flow execution, optionally after a delay.
/// </summary>
[NativeAlias("Suity.Editor.Flows.Nodes.ErrorActionNode")]
[DisplayText("Throw Error", "*CoreIcon|Error")]
[ToolTipsText("Throw an error exception and terminate execution.")]
public class ThrowError : ActionFlowNode
{
    private readonly FlowNodeConnector _input;

    private SValue _message = new STextBlock();

    /// <summary>
    /// Gets or sets the delay duration in seconds before throwing the error.
    /// </summary>
    public float DelaySecond { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrowError"/> class.
    /// </summary>
    public ThrowError()
    {
        _input = AddActionInputConnector("Input", "Input");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message = sync.Sync("Message", _message, SyncFlag.NotNull) ?? new STextBlock();
        DelaySecond = sync.Sync(nameof(DelaySecond), DelaySecond);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_message, new ViewProperty("Message", "Error Message"));
        setup.InspectorField(DelaySecond, new ViewProperty(nameof(DelaySecond), "Delay").WithUnit("seconds"));
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        if (DelaySecond > 0)
        {
            await Task.Delay((int)(DelaySecond * 1000), cancel);
        }

        cancel.ThrowIfCancellationRequested();

        throw new FlowNodeRunException(_message?.ToString() ?? string.Empty);
    }
}

#endregion

#region MultipleAction

/// <summary>
/// Executes multiple connected action outputs in sequence.
/// </summary>
[DisplayText("Execute Multiple Actions", "*CoreIcon|Action")]
[ToolTipsText("Execute multiple actions in sequence. Note: Action order follows the Y-axis position in the diagram.")]
public class MultipleAction : ActionFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _actions;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleAction"/> class.
    /// </summary>
    public MultipleAction()
    {
        _in = AddActionInputConnector("In", "Input");
        _actions = AddConnector("Actions", ACTION_TYPE, FlowDirections.Output, FlowConnectorTypes.Action, true, "Multiple Actions");
        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        await compute.RunActions(_actions, cancel);

        return _out;
    }
}

#endregion

#region RunAction

/// <summary>
/// Executes a single connected action and provides access to its return value.
/// </summary>
[DisplayText("Execute Action", "*CoreIcon|Action")]
[ToolTipsText("Execute an action and get its return value.")]
public class RunAction : ActionFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _action;
    private readonly FlowNodeConnector _value;
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunAction"/> class.
    /// </summary>
    public RunAction()
    {
        _in = AddActionInputConnector("In", "Input");
        _action = AddConnector("Action", ACTION_TYPE, FlowDirections.Output, FlowConnectorTypes.Action, false, "Action");
        _value = AddDataOutputConnector("Value", "object", "Return Value");
        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var result = await compute.RunAction(_action, cancel);
        compute.SetValue(_value, result);

        return _out;
    }
}

#endregion

#region DataActionCompose

/// <summary>
/// Combines an action input and a data input into a single data-action flow output.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Data Action Compose", "*CoreIcon|Action")]
[ToolTipsText("Combine an action and data into a data-action flow.")]
public class DataActionCompose : ActionFlowNode, INavigable
{
    private FlowNodeConnector _actionIn;
    private FlowNodeConnector _dataIn;
    private FlowNodeConnector _out;

    private TypeDesignSelection _type = new();
    private bool _isArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataActionCompose"/> class.
    /// </summary>
    public DataActionCompose()
    {
        _type.Id = NativeTypes.StringType.TargetId;
        _type.TargetUpdated += _type_TargetUpdated;
        _type.ListenEnabled = true;

        UpdateConnector();
    }

    private void _type_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _type, SyncFlag.GetOnly);
        _isArray = sync.Sync("IsArray", _isArray);
        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("DataType", "Data Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        // string displayName = _type.GetDType()?.DisplayText ?? "Data";

        _actionIn = AddActionInputConnector("ActionIn", "Action");
        _dataIn = AddDataInputConnector("DataIn", type, "Input");

        _out = AddConnector("Out", type, FlowDirections.Output, FlowConnectorTypes.Action, false, "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var data = compute.GetValue(_dataIn);

        compute.SetValue(_out, data);
        
        return Task.FromResult<object>(_out);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_type.GetDType()?.DisplayText} Compose";
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _type.Id;
}

#endregion

#region DataActionDecompose

/// <summary>
/// Decomposes a data-action flow input into separate action and data outputs.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Data Action Decompose", "*CoreIcon|Action")]
[ToolTipsText("Decompose a data-action flow into an action and data.")]
public class DataActionDecompose : ActionFlowNode, INavigable
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _actionOut;
    private FlowNodeConnector _dataOut;

    private readonly TypeDesignSelection _type = new();
    private bool _isArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataActionDecompose"/> class.
    /// </summary>
    public DataActionDecompose()
    {
        _type.Id = NativeTypes.StringType.TargetId;
        _type.TargetUpdated += _type_TargetUpdated;
        _type.ListenEnabled = true;

        UpdateConnector();
    }

    private void _type_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _type, SyncFlag.GetOnly);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("DataType", "Data Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        // string displayName = _type.GetDType()?.DisplayText ?? "Data";

        _in = AddConnector("In", type, FlowDirections.Input, FlowConnectorTypes.Action, true, "Input");
        _actionOut = AddActionOutputConnector("ActionOut", "Action");
        _dataOut = AddDataOutputConnector("DataOut", type, "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var data = compute.GetValue(_in);
        compute.SetValue(_dataOut, data);

        return Task.FromResult<object>(_actionOut);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_type.GetDType()?.DisplayText} Decompose";
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _type.Id;
}

#endregion

#region MultipleDataActionDecompose

/// <summary>
/// Decomposes one of multiple data-action flow inputs into separate action and data outputs.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Multiple Data Action Decompose", "*CoreIcon|Action")]
[ToolTipsText("Decompose one of the data-action flows into an action and data.")]
public class MultipleDataActionDecompose : ActionFlowNode, INavigable
{
    private FlowNodeConnector _actionOut;
    private FlowNodeConnector _dataOut;

    private readonly TypeDesignSelection _type = new();
    private readonly ValueProperty<bool> _isArray = new("IsArray", "Array");
    private readonly ValueProperty<int> _count = new("Count", "Count", 2);

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleDataActionDecompose"/> class.
    /// </summary>
    public MultipleDataActionDecompose()
    {
        _type.Id = NativeTypes.StringType.TargetId;
        _type.TargetUpdated += _type_TargetUpdated;
        _type.ListenEnabled = true;

        UpdateConnector();
    }

    private void _type_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _type, SyncFlag.GetOnly);
        _isArray.Sync(sync);
        _count.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("DataType", "Data Type"));
        _isArray.InspectorField(setup);
        _count.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        // string displayName = _type.GetDType()?.DisplayText ?? "Data";

        for (int i = 0; i < _count.Value; i++)
        {
            AddConnector("In_" + i, type, FlowDirections.Input, FlowConnectorTypes.Action, true, "Input" + i);
        }
        
        _actionOut = AddActionOutputConnector("ActionOut", "Action");
        _dataOut = AddDataOutputConnector("DataOut", type, "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var state = compute.GetNodeRunningState(this);
        if (state?.Begin is not { } begin)
        {
            return null;
        }

        var data = compute.GetValue(begin);
        compute.SetValue(_dataOut, data);

        return Task.FromResult<object>(_actionOut);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_type.GetDType()?.DisplayText} Decompose";
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _type.Id;
}

#endregion

#region PassDataAction

/// <summary>
/// Passes a data-action flow through unchanged, acting as a transparent conduit.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Pass Data Action", "*CoreIcon|Action")]
public class PassDataAction : ActionFlowNode, INavigable
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private readonly TypeDesignSelection _type = new();
    private bool _isArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="PassDataAction"/> class.
    /// </summary>
    public PassDataAction()
    {
        _type.Id = NativeTypes.StringType.TargetId;
        _type.TargetUpdated += _type_TargetUpdated;
        _type.ListenEnabled = true;

        UpdateConnector();
    }

    private void _type_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _type, SyncFlag.GetOnly);
        _isArray = sync.Sync("IsArray", _isArray);
        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("DataType", "Data Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        // string typeName = type.ToTypeName();

        // string displayName = _type.GetDType()?.DisplayText ?? "Data";

        _in = AddConnector("In", type, FlowDirections.Input, FlowConnectorTypes.Action, false, " ");
        _out = AddConnector("Out", type, FlowDirections.Output, FlowConnectorTypes.Action, false, " ");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var data = compute.GetValue(_in);
        compute.SetValue(_out, data);

        return Task.FromResult<object>(_out);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{_type.GetDType()?.DisplayText} Pass";
    }

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => _type.Id;
}

#endregion

#region DataTypeSwitchAction

/// <summary>
/// Routes execution to different action outputs based on the runtime type of the incoming data.
/// </summary>
[DisplayText("Data Type Action Branch", "*CoreIcon|Action")]
[ToolTipsText("Determine the type of the incoming data and execute different action flows based on the type.")]
public class DataTypeSwitchAction : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _outOther;

    private readonly List<AssetSelection<DStruct>> _types = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeSwitchAction"/> class.
    /// </summary>
    public DataTypeSwitchAction()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("AbstractType", _types, SyncFlag.GetOnly);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_types, new ViewProperty("AbstractType", "Abstract Type").WithWriteBack());
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddConnector("In", "object", FlowDirections.Input, FlowConnectorTypes.Action, true, "Data Action");

        foreach (var type in _types.Select(o => o.Target).SkipNull())
        {
            string typeName = type.Definition.ToTypeName();
            AddConnector(type.Id, typeName, FlowDirections.Output, FlowConnectorTypes.Action, false, $"{type.DisplayText} Action");
        }

        _outOther = AddConnector("OutOther", "object", FlowDirections.Output, FlowConnectorTypes.Action, false, "Other Action");
        //_out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        //bool selected = false;
        //object result = null;

        var obj = compute.GetValue<SObject>(_in);
        if (obj != null)
        {
            var type = obj.ObjectType;
            foreach (var s in _types.Select(o => o.Target).SkipNull())
            {
                if (type == s.Definition)
                {
                    var conn = GetConnector(s.Id);
                    if (conn != null)
                    {
                        //selected = true;
                        compute.SetValue(conn, obj);
                        //result = await compute.RunAction(conn, cancel);
                        //break;
                        return Task.FromResult<object>(conn);
                    }
                }
            }
        }

        //if (!selected)
        //{
        compute.SetValue(_outOther, obj);
        //result = await compute.RunAction(_outNoResult, cancel);
        return Task.FromResult<object>(_outOther);
        //}

        //await compute.RunAction(_out, cancel);

        //return result;
    }
}

#endregion

#region EnumSwitchAction

/// <summary>
/// Routes execution to different action outputs based on the value of an incoming enum.
/// </summary>
[DisplayText("Enum Branch Action", "*CoreIcon|Enum")]
[ToolTipsText("Determine the incoming enum value and execute different action flows based on the value.")]
public class EnumSwitchAction : ActionFlowNode, ITextDisplay
{
    private readonly AssetHolder<DEnum> _enumType = new();

    private FlowNodeConnector _in;
    private FlowNodeConnector _value;

    private FlowNodeConnector _default;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumSwitchAction"/> class.
    /// </summary>
    public EnumSwitchAction()
    {
        _enumType.TargetUpdated += _enumType_TargetUpdated;
        _enumType.ListenEnabled = true;

        UpdateConnector();
    }

    private void _enumType_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateConnectorQueued();
    }

    /// <summary>
    /// Gets or sets the enum type asset used to determine the branch outputs.
    /// </summary>
    public AssetSelection<DEnum> EnumType
    {
        get => _enumType.Selection;
        set => _enumType.Selection = value;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        //Guid idBefore = ValueType.Id;

        EnumType = sync.Sync("EnumType", EnumType, SyncFlag.NotNull) ?? EnumType;

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
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddActionInputConnector("In", "Input");

        var en = EnumType.GetTarget();
        if (en != null)
        {
            string enumType = en.Definition.ToTypeName();
            _value = AddDataInputConnector("Value", enumType, "Enum Value");

            foreach (var item in en.EnumFields)
            {
                AddActionOutputConnector(item.Id, item.DisplayText);
            }
        }
        else
        {
            _value = AddDataInputConnector("Value", UNKNOWN_TYPE, "Enum Value");
        }

        _default = AddActionOutputConnector("Default", "Default");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        SEnum en = compute.GetValueConvert<SEnum>(_value);
        if (en is null)
        {
            return Task.FromResult<object>(_default);
        }

        string enumId = en.Field?.Id.ToString();
        if (string.IsNullOrWhiteSpace(enumId))
        {
            return Task.FromResult<object>(_default);
        }

        var conn = GetConnector(enumId);

        return Task.FromResult<object>(conn);
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

#region If

/// <summary>
/// Conditionally routes execution to a true or false action output based on a boolean input.
/// </summary>
[NativeAlias("Suity.Editor.Flows.Nodes.IfAction")]
[DisplayText("If", "*CoreIcon|If")]
[ToolTipsText("If branch action, execute different action flows based on True and False.")]
public class If : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _value;

    private FlowNodeConnector _true;
    private FlowNodeConnector _false;

    /// <summary>
    /// Initializes a new instance of the <see cref="If"/> class.
    /// </summary>
    public If()
    {
        _in = AddActionInputConnector("In", "Input");
        _value = AddDataInputConnector("Value", "bool", "Boolean");
        _true = AddActionOutputConnector("True", "True");
        _false = AddActionOutputConnector("False", "False");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        bool v = compute.GetValueConvert<bool>(_value);

        if (v)
        {
            return Task.FromResult<object>(_true);
        }
        else
        {
            return Task.FromResult<object>(_false);
        }
    }
}

#endregion

#region While

/// <summary>
/// Repeatedly executes a loop action while a boolean condition remains true, checking the condition before each iteration.
/// </summary>
[DisplayText("While Loop", "*CoreIcon|Loop")]
[ToolTipsText("Loop execution until the condition is False. Check is performed before the action.")]
public class While : ActionFlowNode
{
    private FlowNodeConnector _in;
    private ConnectorValueProperty<bool> _value = new("Value", "Boolean");

    private FlowNodeConnector _loop;
    private FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="While"/> class.
    /// </summary>
    public While()
    {
        _in = AddActionInputConnector("In", "Input");
        _value.AddConnector(this);

        _loop = AddActionOutputConnector("Loop", "Loop Action");
        _out = AddActionOutputConnector("Out", "Complete");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _value.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _value.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        bool v = _value.GetValue(compute, this);

        while (v)
        {
            compute.InvalidateInputs(this);
            compute.InvalidateOutputs(this);

            await compute.RunAction(_loop, cancel);

            await compute.RecomputeInputNodes(this, cancel);
            v = _value.GetValue(compute, this);
        }

        return _out;
    }
}

#endregion

#region DoWhile

/// <summary>
/// Repeatedly executes a loop action while a boolean condition remains true, checking the condition after each iteration.
/// </summary>
[DisplayText("Do-While Loop", "*CoreIcon|Loop")]
[ToolTipsText("Loop execution until the condition is False. Check is performed after the action.")]
public class DoWhile : ActionFlowNode
{
    private FlowNodeConnector _in;
    private ConnectorValueProperty<bool> _value = new("Value", "Boolean");

    private FlowNodeConnector _loop;
    private FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="DoWhile"/> class.
    /// </summary>
    public DoWhile()
    {
        _in = AddActionInputConnector("In", "Input");
        _value.AddConnector(this);

        _loop = AddActionOutputConnector("Loop", "Loop Action");
        _out = AddActionOutputConnector("Out", "Complete");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _value.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _value.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        bool v;

        do
        {
            compute.InvalidateInputs(this);
            compute.InvalidateOutputs(this);

            await compute.RunAction(_loop, cancel);

            await compute.RecomputeInputNodes(this, cancel);
            v = _value.GetValue(compute, this);
        }
        while (v);

        return _out;
    }
}

#endregion

#region CacheValue

/// <summary>
/// Caches an input value and returns the cached value on subsequent accesses within the diagram scope.
/// </summary>
[DisplayText("Cache Value", "*CoreIcon|Value")]
[ToolTipsText("Cache the input value in the node and return the cached value on the next access.")]
public class CacheValue : ActionFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private FlowNodeConnector _dataIn;
    private FlowNodeConnector _dataOut;

    private readonly TypeDesignSelection _type = new();
    private readonly ValueProperty<bool> _isArray = new("IsArray", "Array");
    private readonly ValueProperty<bool> _dataActionMode = new("DataActionMode", "Data-Action Mode");

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheValue"/> class.
    /// </summary>
    public CacheValue()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("DataType", _type, SyncFlag.GetOnly);
        _isArray.Sync(sync);
        _dataActionMode.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("DataType", "Data Type"));
        _isArray.InspectorField(setup);
        _dataActionMode.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? EmptyTypeDefinition.Empty;
        if (_isArray.Value)
        {
            type = type.MakeArrayType();
        }

        string typeName = type.ToTypeName();

        if (_dataActionMode.Value)
        {
            _in = AddConnector("In", typeName, FlowDirections.Input, FlowConnectorTypes.Action, false, "Input");
            _out = AddConnector("Out", typeName, FlowDirections.Output, FlowConnectorTypes.Action, false, "Output");
        }
        else
        {
            _in = AddActionInputConnector("In", "Input");
            _out = AddActionOutputConnector("Out", "Output");

            _dataIn = AddDataInputConnector("DataIn", typeName, "Data Input");
            _dataOut = AddDataOutputConnector("DataOut", typeName, "Data Output");
        }
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var value = compute.GetNodeCache(FlowContextScopes.Diagram, this);
        if (value is null)
        {
            if (_dataActionMode.Value)
            {
                value = compute.GetValue(_in);
            }
            else
            {
                value = compute.GetValue(_dataIn);
            }

            compute.SetNodeCache(FlowContextScopes.Diagram, this, value);
        }

        if (_dataActionMode.Value)
        {
            compute.SetValue(_out, value);
        }
        else
        {
            compute.SetValue(_dataOut, value);
        }

        return Task.FromResult<object>(_out);
    }
}

#endregion

//#region SObjectToActionNode

//public class SObjectToActionNode : ActionFlowNode
//{
//    protected NodeConnector _in;
//    protected NodeConnector _out;

//    protected NodeConnector _action;

//    private readonly AssetSelectionHolder<DStruct> _structType = new AssetSelectionHolder<DStruct>();

//    public SObjectToActionNode()
//    {
//        _structType.SelectionChanged += () => UpdateConnector();
//        _structType.ListenEnabled = true;

//        UpdateConnector();
//    }

//    public override string TypeDisplayName => "SObject Data Action";

//    protected override void OnSync(IPropertySync sync, ISyncContext context)
//    {
//        base.OnSync(sync, context);

//        _structType.Selection = sync.Sync("Struct", _structType.Selection, SyncFlag.NotNull | SyncFlag.AffectsOthers);

//        if (sync.IsSetter())
//        {
//            UpdateConnectorQueued();
//        }
//    }

//    protected override void OnSetupView(IViewObjectSetup setup)
//    {
//        base.OnSetupView(setup);

//        setup.InspectorField(_structType.Selection, new ViewProperty("Struct", "Struct"));
//    }

//    protected override void OnUpdateConnector()
//    {
//        string structType = _structType.Target?.Definition?.ToAssetString() ?? FlowNode.UnknownDataType;

//        _in = AddDataInputConnector("In", structType, _structType.Target?.DisplayText);

//        _action = AddActionOutputConnector("Action", "Action");
//        _out = AddDataOutputConnector("Out", structType, _structType.Target?.DisplayText);

//    }

//    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
//    {
//        SObject sobj = compute.GetValueConvert<SObject>(_in);
//        compute.SetValue(_out, sobj);

//        return await compute.RunAction(_action, cancel);
//    }
//}

//#endregion
