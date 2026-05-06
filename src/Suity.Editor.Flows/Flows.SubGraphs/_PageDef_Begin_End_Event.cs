using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Flows.SubGraphs.Running;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows.Pages;

#region PageBeginNode

/// <summary>
/// Provides action start support for AIGC pages, such as button clicks, etc.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Workflow, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Action Begin", "*CoreIcon|Begin")]
[DisplayOrder(4000)]
[ToolTipsText("Provides action start support for AIGC pages, such as button clicks, etc.")]
public class PageBeginNode : AigcPageTypeDefNode, IAigcRunWorkflow
{
    private FlowNodeConnector _begin;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBeginNode"/> class.
    /// </summary>
    public PageBeginNode()
    {
        base.FlowNodeGui = OnGui;
        Optional = true;

        UpdateConnector();
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        return gui.FlowSingleConnectorFrame(_begin, context, text);
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Begin;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        if (TypeDefinition.IsNullOrEmpty(TypeDef))
        {
            _begin = this.AddActionOutputConnector("Out", "Output");
        }
        else
        {
            _begin = this.AddConnector("Out", TypeDef, FlowDirections.Output, FlowConnectorTypes.Action, false, "Output");
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            compute.SetResult(this, "end");
            return;
        }

        caller.OnBeginFlow(compute, this.Name);

        var type = this.TypeDef;
        if (!TypeDefinition.IsNullOrEmpty(type))
        {
            if (caller.TryGetParameter(compute, this.Name, out var param) && param != null)
            {
                var sourceType = TypeDefinition.FromNative(param.GetType());
                EditorServices.TypeConvertService.TryConvert(sourceType, type, false, param, out var converted);
                compute.SetValue(_begin, converted);
            }
        }

        compute.SetResult(this, _begin);
    }

    #region IAigcRunWorkflow

    /// <inheritdoc/>
    public FlowNode GetStarterNode(FunctionContext ctx)
    {
        return this;
    }

    #endregion
}

/// <summary>
/// Diagram item representing a <see cref="PageBeginNode"/> in the flow diagram.
/// </summary>
public class PageBeginDiagramItem : FlowDiagramItem<PageBeginNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageBeginDiagramItem"/> class.
    /// </summary>
    public PageBeginDiagramItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBeginDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page begin node.</param>
    public PageBeginDiagramItem(PageBeginNode node)
        : base(node)
    {
    }

    /// <summary>
    /// Creates a new page begin element from this diagram item.
    /// </summary>
    /// <returns>A new <see cref="PageBeginElement"/>.</returns>
    public SubGraphElement CreatePageElement() => new PageBeginElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Begin";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageEventNode

/// <summary>
/// Provides event trigger support for task pages, such as event startup, etc.
/// </summary>
[SimpleFlowNodeStyle(Color = DEvent.EventColorCode, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Task Page Event", "*CoreIcon|Event")]
[DisplayOrder(3800)]
[ToolTipsText("Provides event trigger support for task pages, such as event startup, etc.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.TaskPageEventNode")]
public class PageEventNode : AigcPageTypeDefNode, IAigcRunWorkflow
{
    private FlowNodeConnector _begin;

    readonly ValueProperty<AigcTaskEventTypes> _eventType = new("EventType", "Event Type", AigcTaskEventTypes.TaskBegin);
    readonly StringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "The submission name of the sub-task.");

    /// <summary>
    /// Initializes a new instance of the <see cref="PageEventNode"/> class.
    /// </summary>
    public PageEventNode()
    {
        base.FlowNodeGui = OnGui;
        Optional = true;

        UpdateConnector();
    }

    /// <summary>
    /// Gets the type of event this node responds to.
    /// </summary>
    public AigcTaskEventTypes EventType => _eventType.Value;

    /// <summary>
    /// Gets the commit name associated with this event node.
    /// </summary>
    public string CommitName => _commitName.Text;

    /// <summary>
    /// Determines whether this node matches the specified event type and commit name.
    /// </summary>
    /// <param name="eventType">The event type to match.</param>
    /// <param name="commitName">The commit name to match, or null for any commit.</param>
    /// <returns>True if the event matches; otherwise, false.</returns>
    public bool MathEvent(AigcTaskEventTypes eventType, string commitName = null)
    {
        // Event type must match
        if (eventType != _eventType.Value)
        {
            return false;
        }

        string myCommitName = _commitName.Text?.Trim();
        if (string.IsNullOrWhiteSpace(myCommitName))
        {
            myCommitName = null;
        }
        if (myCommitName is null)
        {
            // When commit name is not set, any commit is acceptable.
            return true;
        }

        // After that, commit name must match exactly
        commitName = commitName?.Trim();
        if (string.IsNullOrWhiteSpace(commitName))
        {
            commitName = null;
        }

        return myCommitName == commitName;
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        var node = gui.FlowSingleConnectorFrame(_begin, context, text);
        if (ToColor(_eventType.Value) is { } color)
        {
            node.OverrideColor(color);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Event;

    /// <inheritdoc/>
    public override Color? BackgroundColor => TitleColor;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _eventType.Sync(sync);
        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _eventType.InspectorField(setup);
        _commitName.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        if (TypeDefinition.IsNullOrEmpty(TypeDef))
        {
            _begin = this.AddActionOutputConnector("Out", "Output");
        }
        else
        {
            _begin = this.AddConnector("Out", TypeDef, FlowDirections.Output, FlowConnectorTypes.Action, false, "Output");
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            compute.SetResult(this, "end");
            return;
        }

        caller.OnBeginFlow(compute, this.Name);

        var type = this.TypeDef;
        if (!TypeDefinition.IsNullOrEmpty(type))
        {
            if (caller.TryGetParameter(compute, this.Name, out var param) && param != null)
            {
                TypeDefinition sourceType;
                if (param is SItem sItem)
                {
                    sourceType = sItem.InputType;
                }
                else
                {
                    sourceType = TypeDefinition.FromNative(param.GetType());
                }
                
                EditorServices.TypeConvertService.TryConvert(sourceType, type, false, param, out var converted);
                compute.SetValue(_begin, converted);
            }
        }

        compute.SetResult(this, _begin);
    }

    #region IAigcRunWorkflow

    /// <inheritdoc/>
    public FlowNode GetStarterNode(FunctionContext ctx)
    {
        return this;
    }

    #endregion

    /// <summary>
    /// Converts an event type to its corresponding display color.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <returns>The associated color, or null.</returns>
    public static Color? ToColor(AigcTaskEventTypes eventType)
    {
        switch (eventType)
        {

            case AigcTaskEventTypes.SubTaskFinished:
                return DEvent.EventTypeColor;

            case AigcTaskEventTypes.SubTaskFailed:
                return AigcColors.ErrorColor;

            case AigcTaskEventTypes.TaskBegin:
            case AigcTaskEventTypes.None:
            default:
                return AigcColors.WorkflowColor;
        }
    }
}

/// <summary>
/// Diagram item representing a <see cref="PageEventNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.TaskPageEventDiagramItem")]
public class PageEventDiagramItem : FlowDiagramItem<PageEventNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageEventDiagramItem"/> class.
    /// </summary>
    public PageEventDiagramItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageEventDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page event node.</param>
    public PageEventDiagramItem(PageEventNode node)
        : base(node)
    {
    }

    /// <summary>
    /// Creates a new page begin element from this diagram item.
    /// </summary>
    /// <returns>A new <see cref="PageBeginElement"/>.</returns>
    public SubGraphElement CreatePageElement() => new PageBeginElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "TaskEvent";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageBaseEndNode

/// <summary>
/// Abstract base class for page end nodes that handle flow completion and commit operations.
/// </summary>
public abstract class PageBaseEndNode : AigcPageTypeDefNode, IFlowNodeComputeAsync, IAigcEndNode
{
    /// <summary>
    /// The type of commit this end node represents.
    /// </summary>
    protected PageCommitTypes _endType;

    private FlowNodeConnector _end;
    private FlowNodeConnector _refInput;
    private readonly ValueProperty<bool> _refConnector = new("RefConnector", "Reference Port");

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBaseEndNode"/> class with the specified end type.
    /// </summary>
    /// <param name="endType">The type of commit this end node represents.</param>
    protected PageBaseEndNode(PageCommitTypes endType)
    {
        _endType = endType;

        base.FlowNodeGui = OnGui;
        Optional = true;

        UpdateConnector();
    }

    /// <summary>
    /// Gets the type of commit this end node represents.
    /// </summary>
    public PageCommitTypes EndType => _endType;

    /// <inheritdoc/>
    protected override void OnSyncValue(IPropertySync sync, ISyncContext context)
    {
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
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string text = DisplayText;
        if (string.IsNullOrEmpty(text))
        {
            // Ensure there is a space for layout placeholder.
            text = " ";
        }

        var node = gui.FlowSingleConnectorFrame(_end, context, text, editorGui: DrawExEditorGui);
        if (ToColor(_endType) is { } color)
        {
            node.OverrideColor(color);
        }

        return node;
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

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        if (TypeDefinition.IsNullOrEmpty(TypeDef))
        {
            _end = this.AddActionInputConnector("In", "Input");
        }
        else
        {
            _end = this.AddConnector("In", TypeDef, FlowDirections.Input, FlowConnectorTypes.Action, true, "Input");
        }

        if (_refConnector.Value)
        {
            _refInput = AddConnector("RefIn", TypeDef, FlowDirections.Input, FlowConnectorTypes.Control, false, "Parameter Reference");
        }
        else
        {
            _refInput = null;
        }
    }

    /// <inheritdoc/>
    public async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        if (compute.Context.GetArgument<IFlowCallerContext>() is not { } caller)
        {
            return EndType.ToString();
        }

        do
        {
            var dataNames = caller.GetDatasToCompute(compute, this.Name) ?? [];
            if (dataNames.Length == 0)
            {
                break;
            }

            if (this.Diagram is not { } diagram)
            {
                break;
            }

            var dataNodes = dataNames.Select(diagram.GetNode).SkipNull();
            foreach (var dataNode in dataNodes)
            {
                await compute.ComputeData(dataNode, cancel);
            }
        } while (false);

        var value = compute.GetValue(_end);

        int callingStack = compute.CallingStack;
        if (callingStack > 1)
        {
            // If not in the first stack, it means we need to execute the caller's external flow.
            return await caller.CallFunction(compute, this.Name, value, cancel);
        }
        else
        {
            caller.OnEndFlow(compute, this.Name, value);
            return EndType.ToString();
        }
    }

    /// <summary>
    /// Converts a page commit type to its corresponding display color.
    /// </summary>
    /// <param name="endType">The commit type.</param>
    /// <returns>The associated color, or null.</returns>
    public static Color? ToColor(PageCommitTypes endType)
    {
        switch (endType)
        {
            case PageCommitTypes.None:
                return AigcColors.WorkflowColor;

            case PageCommitTypes.TaskFinished:
                return DEvent.EventTypeColor;

            case PageCommitTypes.TaskFailed:
                return AigcColors.ErrorColor;

            default:
                return null;
        }
    }
}
#endregion

#region PageEndNode

/// <summary>
/// Provides action end support for AIGC pages.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Workflow, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Action End", "*CoreIcon|End")]
[DisplayOrder(3900)]
[ToolTipsText("Provides action end support for AIGC pages.")]
public class PageEndNode : PageBaseEndNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageEndNode"/> class.
    /// </summary>
    public PageEndNode() : base(PageCommitTypes.None) { }
}

/// <summary>
/// Diagram item representing a <see cref="PageEndNode"/> in the flow diagram.
/// </summary>
public class PageEndDiagramItem : FlowDiagramItem<PageEndNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageEndDiagramItem"/> class.
    /// </summary>
    public PageEndDiagramItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageEndDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page end node.</param>
    public PageEndDiagramItem(PageEndNode node)
        : base(node)
    {
    }

    /// <summary>
    /// Creates a new page end element from this diagram item.
    /// </summary>
    /// <returns>A new <see cref="PageEndElement"/>.</returns>
    public SubGraphElement CreatePageElement() => new PageEndElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "End";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion

#region PageCommitNode

/// <summary>
/// Ends the flow and submits the result upward.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("AIGC Page Action End - Commit", "*CoreIcon|Task")]
[DisplayOrder(3899)]
[ToolTipsText("End the flow and submit the result upward.")]
public class PageCommitNode : PageBaseEndNode, IFlowNodeComputeAsync, IAigcEndNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageCommitNode"/> class.
    /// </summary>
    public PageCommitNode() : base(PageCommitTypes.TaskFinished) { }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _endType = sync.Sync("CommitType", _endType);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.InspectorField(_endType, new ViewProperty("CommitType", "Commit Method"));
    }
}

/// <summary>
/// Diagram item representing a <see cref="PageCommitNode"/> in the flow diagram.
/// </summary>
public class PageCommitDiagramItem : FlowDiagramItem<PageCommitNode>, ISubGraphElementCreator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageCommitDiagramItem"/> class.
    /// </summary>
    public PageCommitDiagramItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCommitDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page commit node.</param>
    public PageCommitDiagramItem(PageCommitNode node)
        : base(node)
    {
    }

    /// <summary>
    /// Creates a new page end element from this diagram item.
    /// </summary>
    /// <returns>A new <see cref="PageEndElement"/>.</returns>
    public SubGraphElement CreatePageElement() => new PageEndElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Commit";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);
}
#endregion
