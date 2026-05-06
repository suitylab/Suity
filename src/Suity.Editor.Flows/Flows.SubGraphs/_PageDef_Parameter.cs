using Suity.Drawing;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Flows.SubGraphs.Running;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Drawing;

namespace Suity.Editor.AIGC.Flows.Pages;

#region PageParameterInputNode
/// <summary>
/// Provides input parameter support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Input Parameter", "*CoreIcon|Parameter")]
[DisplayOrder(3000)]
[ToolTipsText("Provides input parameter support for AIGC page actions.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterNode")]
public class PageParameterInputNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _out;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private object _value;


    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterInputNode"/> class.
    /// </summary>
    public PageParameterInputNode()
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
/// Diagram item representing a <see cref="PageParameterInputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParameterDiagramItem")]
public class PageParameterInputItem : FlowDiagramItem<PageParameterInputNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterInputItem"/> class.
    /// </summary>
    public PageParameterInputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterInputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page parameter input node.</param>
    public PageParameterInputItem(PageParameterInputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageParameterInputElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Paramater";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageSkillParameterInputNode
/// <summary>
/// Provides skill parameter support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.AgentBg, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Skill Parameter", "*CoreIcon|Skill")]
[DisplayOrder(2990)]
[ToolTipsText("Provides skill parameter support for AIGC page actions.")]
public class PageSkillParameterNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _out;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private object _value;


    /// <summary>
    /// Initializes a new instance of the <see cref="PageSkillParameterNode"/> class.
    /// </summary>
    public PageSkillParameterNode()
        : base(NativeTypes.StringType.TargetId)
    {
        Value = NativeTypes.StringType.CreateOrRepairValue(Value, false);

        base.FlowNodeGui = OnGui;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Skill;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets or sets the default value for this skill parameter.
    /// </summary>
    public object Value
    {
        get => _value;
        set => _value = value;
    }

    /// <inheritdoc/>
    public override bool IsSkillParameter => true;


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
            compute.SetValue(_out, null);
            return;
        }

        if (!caller.TryGetParameter(compute, this.Name, out object value))
        {
            compute.SetValue(_out, null);
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
/// Diagram item representing a <see cref="PageSkillParameterNode"/> in the flow diagram.
/// </summary>
public class PageSkillParameterItem : FlowDiagramItem<PageSkillParameterNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageSkillParameterItem"/> class.
    /// </summary>
    public PageSkillParameterItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSkillParameterItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page skill parameter node.</param>
    public PageSkillParameterItem(PageSkillParameterNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageSkillParameterElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "SkillParamater";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PagePromptParameterInputNode

/// <summary>
/// Provides prompt input value support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Prompt Parameter", "*CoreIcon|Prompt")]
[DisplayOrder(2980)]
[ToolTipsText("Provides prompt input value support for AIGC page actions.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PagePromptParameterInputNode")]
public class PagePromptParameterInputNode : AigcPageTypeDefNode
{
    readonly private FlowNodeConnector _prompt;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagePromptParameterInputNode"/> class.
    /// </summary>
    public PagePromptParameterInputNode()
    {
        base.EditTypeEnabled = false;

        TypeDef = NativeTypes.StringType;

        _prompt = AddDataOutputConnector("Prompt", TypeDef, "Prompt");

        this.FlowNodeGui = OnGui;
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Prompt;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (compute.Context.GetArgument<IAigcTaskPage>() is not { } task)
        {
            compute.SetValue(_prompt, string.Empty);
            return;
        }

        string prompt = task.GetPrompt(false);
        compute.SetValue(_prompt, prompt ?? string.Empty);
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_prompt, context, text);
    }
}

/// <summary>
/// Diagram item representing a <see cref="PagePromptParameterInputNode"/> in the flow diagram.
/// </summary>
public class PagePromptParameterInputItem : FlowDiagramItem<PagePromptParameterInputNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagePromptParameterInputItem"/> class.
    /// </summary>
    public PagePromptParameterInputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagePromptParameterInputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page prompt parameter input node.</param>
    public PagePromptParameterInputItem(PagePromptParameterInputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PagePromptParameterElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "PromptParamater";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageMessageParameterNode

/// <summary>
/// Provides message input parameter support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Message, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Message Parameter", "*CoreIcon|Comment")]
[DisplayOrder(2970)]
[ToolTipsText("Provides input parameter support for AIGC page actions.")]
public class PageMessageParameterNode : AigcPageTypeDefNode
{
    private readonly TextBlockProperty _message = new("Message", "Message", string.Empty, "Supported placeholders: {TaskName} {TaskStatus}");


    /// <summary>
    /// Initializes a new instance of the <see cref="PageMessageParameterNode"/> class.
    /// </summary>
    public PageMessageParameterNode()
    {
        base.EditTypeEnabled = false;
        this.FlowNodeGui = OnGui;

        TaskCompletion = false;
        TaskCommit = true;
        ChatHistory = true;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Comment;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Value
    {
        get => _message.Text;
        set => _message.Text = value;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _message.InspectorField(setup);
    }


    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSimpleFrame(FlowDirections.Input, context, text);
    }
}

/// <summary>
/// Diagram item representing a <see cref="PageMessageParameterNode"/> in the flow diagram.
/// </summary>
public class PageMessageParameterItem : FlowDiagramItem<PageMessageParameterNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageMessageParameterItem"/> class.
    /// </summary>
    public PageMessageParameterItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageMessageParameterItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page message parameter node.</param>
    public PageMessageParameterItem(PageMessageParameterNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageMessageElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Message";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion


#region PageParameterOutputNode
/// <summary>
/// Provides output value support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Output Parameter", "*CoreIcon|Parameter")]
[DisplayOrder(2900)]
[ToolTipsText("Provides output parameter support for AIGC page actions.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageOutputNode")]
public class PageParameterOutputNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private object _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterOutputNode"/> class.
    /// </summary>
    public PageParameterOutputNode()
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
/// Diagram item representing a <see cref="PageParameterOutputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageOutputDiagramItem")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageParamterOutputItem")]
public class PageParameterOutputItem : FlowDiagramItem<PageParameterOutputNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterOutputItem"/> class.
    /// </summary>
    public PageParameterOutputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageParameterOutputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page parameter output node.</param>
    public PageParameterOutputItem(PageParameterOutputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageParameterOutputElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Output";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageArticleOutputNode

/// <summary>
/// Provides article output support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Output Article", "*CoreIcon|Article")]
[DisplayOrder(2800)]
[ToolTipsText("Provides article output support for AIGC page actions.")]
public class PageArticleOutputNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _refInput;

    private readonly StringProperty _articlePath = new("ArticlePath", "Article Path", "", "Use '/' to separate article hierarchy levels.");
    private readonly ValueProperty<ArticleFields> _writingTarget = new("WritingTarget", "Writing Target", ArticleFields.Content);
    private readonly ValueProperty<bool> _multipleSection = new("MultipleSection", "Multi-Section Article", false, "Whether it is a multi-section article.");
    private readonly ValueProperty<bool> _passToSubTasks = new("PassToSubTasks", "Pass To Sub-Tasks", false, "When enabled, sub-tasks will use this article location.");

    /// <summary>
    /// Initializes a new instance of the <see cref="PageArticleOutputNode"/> class.
    /// </summary>
    public PageArticleOutputNode()
    {
        base.FlowNodeGui = OnGui;
        base.EditTypeEnabled = false;
        base.TypeDef = TypeDefinition.FromNative<IArticle>();
        base.LinkedMode = true;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Article;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets the article path.
    /// </summary>
    public string ArticlePath => _articlePath.Value;

    /// <summary>
    /// Gets the writing target field for the article.
    /// </summary>
    public ArticleFields WritingTarget => _writingTarget.Value;

    /// <summary>
    /// Gets a value indicating whether this is a multi-section article.
    /// </summary>
    public bool MultipleSection => _multipleSection.Value;

    /// <summary>
    /// Gets a value indicating whether the article should be passed to sub-tasks.
    /// </summary>
    public bool PassToSubTasks => _passToSubTasks.Value;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _articlePath.Sync(sync);
        _writingTarget.Sync(sync);
        _multipleSection.Sync(sync);
        _passToSubTasks.Sync(sync);

        if (sync.IsSetterOf("RefConnector"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _articlePath.InspectorField(setup);
        _writingTarget.InspectorField(setup);
        _multipleSection.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _passToSubTasks.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = TypeDefinition.FromNative<IArticle>();

        _refInput = AddConnector("RefIn", type, FlowDirections.Input, FlowConnectorTypes.Control, true, "Parameter Reference");
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSimpleFrame(FlowDirections.Output, context, text, editorGui: DrawExEditorGui);
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
/// Diagram item representing a <see cref="PageArticleOutputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageArticleOutputDiagramItem")]
public class PageArticleOutputItem : FlowDiagramItem<PageArticleOutputNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageArticleOutputItem"/> class.
    /// </summary>
    public PageArticleOutputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageArticleOutputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page article output node.</param>
    public PageArticleOutputItem(PageArticleOutputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageArticleOutputElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Article";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}

#endregion

#region PageFileOutputNode
/// <summary>
/// Provides file output support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.PageParameter, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Output File", "*CoreIcon|File")]
[DisplayOrder(2750)]
[ToolTipsText("Provides file output support for AIGC page actions.")]
public class PageFileOutputNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    private string _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageFileOutputNode"/> class.
    /// </summary>
    public PageFileOutputNode()
        : base(NativeTypes.StringType.TargetId)
    {
        base.EditTypeEnabled = false;

        Value = NativeTypes.StringType.CreateOrRepairValue(Value, false);

        base.FlowNodeGui = OnGui;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.File;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets or sets the file output value.
    /// </summary>
    public object Value
    {
        get => _value;
        set => _value = value as string ?? string.Empty;
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
/// Diagram item representing a <see cref="PageFileOutputNode"/> in the flow diagram.
/// </summary>
public class PageFileOutputItem : FlowDiagramItem<PageFileOutputNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageFileOutputItem"/> class.
    /// </summary>
    public PageFileOutputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageFileOutputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page file output node.</param>
    public PageFileOutputItem(PageFileOutputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageFileOutputElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "File";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageSubTaskOutputNode

/// <summary>
/// Provides sub-task output support for AIGC page actions.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Output SubTask", "*CoreIcon|Task")]
[DisplayOrder(2700)]
[ToolTipsText("Provides sub-task output support for AIGC page actions.")]
public class PageSubTaskOutputNode : AigcPageTypeDefNode
{
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _allSubTasks = new("AllSubTasks", "All SubTasks", false, "Reference all first-level sub-tasks, otherwise only reference the last sub-task.");
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubTaskOutputNode"/> class.
    /// </summary>
    public PageSubTaskOutputNode()
    {
        TypeDef = TypeDefinition.FromNative<IAigcPageInstance>();

        base.FlowNodeGui = OnGui;
        // Disable type editing
        base.EditTypeEnabled = false;

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Task;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <summary>
    /// Gets a value indicating whether to reference all first-level sub-tasks.
    /// </summary>
    public bool AllSubTasks => _allSubTasks.Value;


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _allSubTasks.Sync(sync);

        _refConnector.Sync(sync);
        if (sync.IsSetterOf("AllSubTasks") || sync.IsSetterOf("RefConnector"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _refConnector.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _allSubTasks.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var type = TypeDefinition.FromNative<IAigcPageInstance>();
        if (_allSubTasks.Value)
        {
            type = type.MakeArrayType();
        }

        if (_refConnector.Value)
        {
            _refInput = AddConnector("RefIn", type, FlowDirections.Input, FlowConnectorTypes.Control, true, "Parameter Reference");
        }
        else
        {
            _refInput = null;
        }
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSimpleFrame(FlowDirections.Output, context, text, editorGui: DrawExEditorGui);
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
/// Diagram item representing a <see cref="PageSubTaskOutputNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageSubTaskOutputDiagramItem")]
public class PageSubTaskOutputItem : FlowDiagramItem<PageSubTaskOutputNode>, ISubGraphElementCreator
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubTaskOutputItem"/> class.
    /// </summary>
    public PageSubTaskOutputItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubTaskOutputItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page sub-task output node.</param>
    public PageSubTaskOutputItem(PageSubTaskOutputNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    public SubGraphElement CreatePageElement() => new PageSubTaskOutputElement(this);


    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "SubTask";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}

#endregion
