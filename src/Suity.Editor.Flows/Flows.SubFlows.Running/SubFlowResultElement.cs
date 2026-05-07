using Suity.Editor.AIGC.TaskPages;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a result page element that displays the completion outputs of an AIGC flow page.
/// </summary>
public class SubFlowResultElement : SubFlowGroupElement
{
    readonly SubFlowResultDiagramItem _resultPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowResultElement"/> class.
    /// </summary>
    /// <param name="groupItem">The flow diagram item representing this result group.</param>
    /// <param name="depth">The nesting depth of this element.</param>
    /// <param name="order">The display order of this element. Defaults to -1.</param>
    public SubFlowResultElement(FlowDiagramItem groupItem, int depth, int order = -1)
        : base(groupItem, depth, order)
    {
        DrawLabel = false;
        _resultPage = groupItem as SubFlowResultDiagramItem;
    }

    /// <summary>
    /// Gets the parameter condition that determines when this result page is considered complete.
    /// </summary>
    public ParameterConditions ParameterCondition { get; private set; }

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        ParameterCondition = _resultPage?.Node?.CompletionCondition ?? ParameterConditions.All;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        base.Sync(sync, context);
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        string resultTitle = (Node as IGroupFlowNode)?.GroupName;
        var defNode = (this.Node as ISubFlowDef)?.GetPageDefinition();
        string label;

        if (defNode != null)
        {
            label = $"{defNode.ToDisplayText()} - {resultTitle}";
        }
        else
        {
            label = resultTitle;
        }

        setup.LabelWithIcon(Name, label, Icon, prop => prop.WithStatus(this.GetStatus()));

        
        base.SetupView(setup);


        //if (Option.Mode == PageElementMode.Function)
        //{
        //    return;
        //}

        //if (DiagramItem.Node is IGroupFlowNode group)
        //{
        //    setup.LabelWithIcon(group.GroupName, CoreIconCache.Uncheck);
        //}

        //foreach (var item in ChildElements)
        //{
        //    item.SetupView(setup);
        //}
    }

    /// <inheritdoc/>
    public override bool? GetIsDone() => GetIsDoneOutputs(ParameterCondition);

}
