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

#region PageEventNode

/// <summary>
/// Provides event trigger support for task pages, such as event startup, etc.
/// </summary>
[SimpleFlowNodeStyle(Color = DEvent.EventColorCode, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Task Page Event", "*CoreIcon|Event")]
[DisplayOrder(3800)]
[ToolTipsText("Provides event trigger support for task pages, such as event startup, etc.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageEventNode")]
public class PageEventNode : SubFlowTypeNode, IFlowRunnable
{
    private FlowNodeConnector _begin;

    readonly ValueProperty<TaskEventTypes> _eventType = new("EventType", "Event Type", TaskEventTypes.TaskBegin);
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
    public TaskEventTypes EventType => _eventType.Value;

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
    public bool MathEvent(TaskEventTypes eventType, string commitName = null)
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

    #region IFlowRunnable

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
    public static Color? ToColor(TaskEventTypes eventType)
    {
        switch (eventType)
        {

            case TaskEventTypes.SubTaskFinished:
                return DEvent.EventTypeColor;

            case TaskEventTypes.SubTaskFailed:
                return FlowColors.ErrorColor;

            case TaskEventTypes.TaskBegin:
            case TaskEventTypes.None:
            default:
                return FlowColors.WorkflowColor;
        }
    }
}

/// <summary>
/// Diagram item representing a <see cref="PageEventNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageEventDiagramItem")]
public class PageEventDiagramItem : FlowDiagramItem<PageEventNode>, ISubFlowElementCreator
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
    /// <returns>A new <see cref="SubFlowBeginElement"/>.</returns>
    public SubFlowElement CreatePageElement() => new SubFlowBeginElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "TaskEvent";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => SubFlowNode.VerifyName(name);
}
#endregion

#region PageCommitNode

/// <summary>
/// Ends the flow and submits the result upward.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Sub-flow Action End - Commit", "*CoreIcon|Task")]
[DisplayOrder(3899)]
[ToolTipsText("End the flow and submit the result upward.")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageCommitNode")]
public class PageCommitNode : BaseSubFlowEndNode, IFlowNodeComputeAsync, ISubFlowEndNode
{
    /// <summary>
    /// The type of commit this end node represents.
    /// </summary>
    protected TaskCommitTypes _endType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCommitNode"/> class.
    /// </summary>
    public PageCommitNode() : base(TaskCommitTypes.TaskFinished) { }

    /// <inheritdoc/>
    public override TaskCommitTypes EndType => _endType;

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
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageCommitDiagramItem")]
public class PageCommitDiagramItem : FlowDiagramItem<PageCommitNode>, ISubFlowElementCreator
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
    /// <returns>A new <see cref="SubFlowEndElement"/>.</returns>
    public SubFlowElement CreatePageElement() => new SubFlowEndElement(this);

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Commit";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => SubFlowNode.VerifyName(name);
}
#endregion