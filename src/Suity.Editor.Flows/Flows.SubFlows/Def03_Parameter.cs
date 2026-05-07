using Suity.Drawing;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Drawing;

namespace Suity.Editor.Flows.SubFlows;

#region SubflowParameterInputNode
/// <summary>
/// Provides input parameter support for Sub-flow actions.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Sub-flow Input Parameter", "*CoreIcon|Parameter")]
[DisplayOrder(3000)]
[ToolTipsText("Provides input parameter support for Sub-flow actions.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterInputNode")]
public class SubFlowParameterInputNode : SubFlowTypeNode
{
    private FlowNodeConnector _out;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private object _value;


    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterInputNode"/> class.
    /// </summary>
    public SubFlowParameterInputNode()
        : base(NativeTypes.StringType.TargetId)
    {
        Value = NativeTypes.StringType.CreateOrRepairValue(Value, false);

        this.FlowNodeGui = OnGui;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Parameter;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets or sets the default value for this parameter.
    /// </summary>
    public object Value
    {
        get => _value;
        set => _value = value;
    }

    /// <inheritdoc/>
    protected override void OnSyncValue(IPropertySync sync, ISyncContext context)
    {
        Value = sync.Sync("Value", Value);

        _refConnector.Sync(sync);
        if (sync.IsSetterOf("RefConnector"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupViewValue(IViewObjectSetup setup)
    {
        _refConnector.InspectorField(setup);

        if (TypeDef is { } typeDef)
        {
            setup.InspectorFieldOfType(typeDef, new ViewProperty("Value", "Value").WithOptional());
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _out = AddConnector("Out", TypeDef, FlowDirections.Output, FlowConnectorTypes.Data);

        if (_refConnector.Value)
        {
            _refInput = AddConnector("RefIn", TypeDef, FlowDirections.Input, FlowConnectorTypes.Control, true, "Parameter Reference");
        }
        else
        {
            _refInput = null;
        }
    }

    /// <inheritdoc/>
    protected override void UpdateDefaultValue()
    {
        var type = TypeDef;

        if (!TypeDefinition.IsNullOrBroken(type))
        {
            Value = type.CreateOrRepairValue(Value, true);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            compute.SetValue(_out, Value); // Set default value
            return;
        }

        if (!caller.TryGetParameter(compute, this.Name, out object value))
        {
            compute.SetValue(_out, Value); // Set default value
            return;
        }

        EditorServices.TypeConvertService.TryConvert(_out, value, out var converted);

        compute.SetValue(_out, converted);
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_out, context, text, editorGui: DrawExEditorGui);
    }

    private bool DrawExEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Input && _refInput != null)
        {
            gui.HorizontalLayout("#control-input")
            .OnInitialize(n =>
            {
                n.InitClass("debug_draw");
                n.InitFit();
                n.InitHorizontalAlignment(GuiAlignment.Center);
                n.InitPadding(1);
            })
            .OnContent(() =>
            {
                gui.FlowConnectorPoint(_refInput, context, _refInput.Name);
            });
        }

        return true;
    }
}


/// <summary>
/// Diagram item representing a <see cref="SubFlowParameterInputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterInputItem")]
public class SubFlowParameterInputItem : FlowDiagramItem<SubFlowParameterInputNode>, ISubFlowElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterInputItem"/> class.
    /// </summary>
    public SubFlowParameterInputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterInputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page parameter input node.</param>
    public SubFlowParameterInputItem(SubFlowParameterInputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubFlowElement CreatePageElement() => new SubFlowParameterInput(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Paramater";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => SubFlowNode.VerifyName(name);
}
#endregion

#region SubFlowParameterOutputNode
/// <summary>
/// Provides output value support for Sub-flow actions.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Sub-flow Output Parameter", "*CoreIcon|Parameter")]
[DisplayOrder(2900)]
[ToolTipsText("Provides output parameter support for Sub-flow actions.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterOutputNode")]
public class SubFlowParameterOutputNode : SubFlowTypeNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private object _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterOutputNode"/> class.
    /// </summary>
    public SubFlowParameterOutputNode()
        : base(NativeTypes.StringType.TargetId)
    {
        Value = NativeTypes.StringType.CreateOrRepairValue(Value, false);

        base.FlowNodeGui = OnGui;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Parameter;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets or sets the default value for this output parameter.
    /// </summary>
    public object Value
    {
        get => _value;
        set => _value = value;
    }

    /// <inheritdoc/>
    protected override void OnSyncValue(IPropertySync sync, ISyncContext context)
    {
        Value = sync.Sync("Value", Value);

        _refConnector.Sync(sync);
        if (sync.IsSetterOf("RefConnector"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupViewValue(IViewObjectSetup setup)
    {
        _refConnector.InspectorField(setup);

        if (TypeDef is { } typeDef)
        {
            setup.InspectorFieldOfType(typeDef, new ViewProperty("Value", "Value").WithOptional());
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddConnector("In", TypeDef, FlowDirections.Input, FlowConnectorTypes.Data);

        if (_refConnector.Value)
        {
            _refInput = AddConnector("RefIn", TypeDef, FlowDirections.Input, FlowConnectorTypes.Control, true, "Parameter Reference");
        }
        else
        {
            _refInput = null;
        }
    }

    /// <inheritdoc/>
    protected override void UpdateDefaultValue()
    {
        var type = TypeDef;

        if (!TypeDefinition.IsNullOrBroken(type))
        {
            Value = type.CreateOrRepairValue(Value, true);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var value = compute.GetValue(_in);
        compute.SetResult(this, value);
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_in, context, text, editorGui: DrawExEditorGui);
    }

    private bool DrawExEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Input && _refInput != null)
        {
            gui.HorizontalLayout("#control-input")
            .OnInitialize(n =>
            {
                n.InitClass("debug_draw");
                n.InitFit();
                n.InitHorizontalAlignment(GuiAlignment.Center);
                n.InitPadding(1);
            })
            .OnContent(() => 
            {
                gui.FlowConnectorPoint(_refInput, context, _refInput.Name);
            });
        }

        return true;
    }
}

/// <summary>
/// Diagram item representing a <see cref="SubFlowParameterOutputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageOutputDiagramItem")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterOutputItem")]
public class SubFlowParameterOutputItem : FlowDiagramItem<SubFlowParameterOutputNode>, ISubFlowElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterOutputItem"/> class.
    /// </summary>
    public SubFlowParameterOutputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowParameterOutputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page parameter output node.</param>
    public SubFlowParameterOutputItem(SubFlowParameterOutputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubFlowElement CreatePageElement() => new SubFlowParameterOutput(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Output";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => SubFlowNode.VerifyName(name);
}
#endregion