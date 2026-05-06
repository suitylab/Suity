using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;


#region LinqSelect

/// <summary>
/// A flow node that projects each element of a sequence into a new form, equivalent to LINQ's <c>Select</c> operator.
/// </summary>
[DisplayText("Select Element", "*CoreIcon|Array")]
[ToolTipsText("Select a new element from each element in the sequence and generate a new sequence.")]
public class LinqSelect : LinqFlowNode, IFlowNodeComputeAsync
{
    private ITypeDesignSelection _inType;
    private ITypeDesignSelection _outType;

    /// <summary>
    /// Input connector that receives the source array of elements.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// Output connector that emits the resulting array of projected elements.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// Action connector that invokes the projection function for each element in the input sequence.
    /// </summary>
    protected FlowNodeConnector _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqSelect"/> class and creates the type design selections.
    /// </summary>
    public LinqSelect()
    {
        _inType = DTypeManager.Instance.CreateTypeDesignSelection();
        _outType = DTypeManager.Instance.CreateTypeDesignSelection();

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inType = sync.Sync("InType", _inType, SyncFlag.NotNull);
        _outType = sync.Sync("OutType", _outType, SyncFlag.NotNull);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_inType, new ViewProperty("InType", "Input Type"));
        setup.InspectorField(_outType, new ViewProperty("OutType", "Output Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var inType = _inType.GetTypeDefinition() ?? TypeDefinition.Empty;
        var outType = _outType.GetTypeDefinition() ?? TypeDefinition.Empty;

        var inAryType = inType.MakeArrayType();
        var outAryType = outType.MakeArrayType();

        _in = AddDataInputConnector("In", inAryType, "Input");

        _out = AddDataOutputConnector("Out", outAryType, "Output");
        _func = AddConnector("Func", inType, FlowDirections.Output, FlowConnectorTypes.Action, false, "Function");
    }

    /// <inheritdoc/>
    public async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var ary = compute.GetValues<object>(_in, true);

        List<object> results = [];

        foreach (var item in ary)
        {
            compute.SetValue(_func, item);
            var r = await compute.RunAction(_func, cancel);
            results.Add(r);
        }

        compute.SetValue(_out, results.ToArray());

        return null;
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        var typeName = _inType.GetDType()?.ToDisplayText();

        if (!string.IsNullOrWhiteSpace(typeName))
        {
            return $"Select {typeName} Element";
        }
        else
        {
            return base.ToString();
        }
    }
}

#endregion

#region LinqConcat

/// <summary>
/// A flow node that concatenates multiple sequences front-to-back, equivalent to LINQ's <c>Concat</c> operator.
/// The concatenation order is determined by the chart Y-axis position of the input connections.
/// </summary>
[DisplayText("Concat Elements", "*CoreIcon|Array")]
[ToolTipsText("Simply concatenate multiple sequences front-to-back, with concatenation order arranged by chart Y-axis position.")]
public class LinqConcat : LinqFlowNode, IFlowNodeComputeAsync
{
    private ITypeDesignSelection _type;

    /// <summary>
    /// Input connector that receives multiple arrays to concatenate.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// Output connector that emits the concatenated array.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqConcat"/> class and creates the type design selection.
    /// </summary>
    public LinqConcat()
    {
        _type = DTypeManager.Instance.CreateTypeDesignSelection();

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _type = sync.Sync("Type", _type, SyncFlag.NotNull, description: "Type");

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("Type", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;

        var aryType = type.MakeArrayType();

        _in = AddConnector("In", aryType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Input");
        _out = AddDataOutputConnector("Out", aryType, "Output");
    }

    /// <inheritdoc/>
    public Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var ary = compute.GetValues<object>(_in, true);

        compute.SetValue(_out, ary);

        return Task.FromResult<object>(null);
    }
}

#endregion

#region LinqUnion

/// <summary>
/// A flow node that produces the set union of multiple sequences, removing duplicates, equivalent to LINQ's <c>Union</c> operator.
/// The union order is determined by the chart Y-axis position of the input connections.
/// </summary>
[DisplayText("Union Elements", "*CoreIcon|Array")]
[ToolTipsText("Union multiple sequences and remove duplicates, with union order arranged by chart Y-axis position.")]
public class LinqUnion : LinqFlowNode, IFlowNodeComputeAsync
{
    private ITypeDesignSelection _type;

    /// <summary>
    /// Input connector that receives multiple arrays to union.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// Output connector that emits the unioned array with duplicates removed.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqUnion"/> class and creates the type design selection.
    /// </summary>
    public LinqUnion()
    {
        _type = DTypeManager.Instance.CreateTypeDesignSelection();

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _type = sync.Sync("Type", _type, SyncFlag.NotNull, description: "Type");

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("Type", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;

        var aryType = type.MakeArrayType();

        _in = AddConnector("In", aryType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Input");
        _out = AddDataOutputConnector("Out", aryType, "Output");
    }

    /// <inheritdoc/>
    public Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var ary = compute.GetValues<object>(_in, true);

        compute.SetValue(_out, ary.Distinct().ToArray());

        return Task.FromResult<object>(null);
    }
}

#endregion

#region LinqFirstOrDefault

/// <summary>
/// A flow node that returns the first element of a sequence, or a default value if the sequence is empty,
/// equivalent to LINQ's <c>FirstOrDefault</c> operator.
/// </summary>
[DisplayText("First Element", "*CoreIcon|Array")]
public class LinqFirstOrDefault : LinqFlowNode, IFlowNodeComputeAsync
{
    private ITypeDesignSelection _type;

    /// <summary>
    /// Input connector that receives the source array.
    /// </summary>
    protected FlowNodeConnector _in;

    /// <summary>
    /// Output connector that emits the first element of the input array, or the default value if empty.
    /// </summary>
    protected FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqFirstOrDefault"/> class and creates the type design selection.
    /// </summary>
    public LinqFirstOrDefault()
    {
        _type = DTypeManager.Instance.CreateTypeDesignSelection();

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _type = sync.Sync("Type", _type, SyncFlag.NotNull, description: "Type");

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_type, new ViewProperty("Type", "Type"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = _type.GetTypeDefinition() ?? TypeDefinition.Empty;

        var aryType = type.MakeArrayType();

        _in = AddConnector("In", aryType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Input");
        _out = AddDataOutputConnector("Out", type, "Output");
    }

    /// <inheritdoc/>
    public Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var ary = compute.GetValues<object>(_in, true);

        compute.SetValue(_out, ary.FirstOrDefault());

        return Task.FromResult<object>(null);
    }
}

#endregion
