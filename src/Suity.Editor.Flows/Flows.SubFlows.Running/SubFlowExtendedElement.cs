using Suity.Views;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a sub-page element that groups related flow nodes and delegates completion status to its result page.
/// </summary>
public class SubFlowExtendedElement : SubFlowGroupElement
{
    readonly SubflowExtendedDiagramItem _subPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowExtendedElement"/> class.
    /// </summary>
    /// <param name="extensionItem">The flow diagram item representing this sub-page group.</param>
    /// <param name="depth">The nesting depth of this element.</param>
    /// <param name="order">The display order of this element. Defaults to -1.</param>
    public SubFlowExtendedElement(FlowDiagramItem extensionItem, int depth, int order = -1)
        : base(extensionItem, depth, order)
    {
        _subPage = extensionItem as SubflowExtendedDiagramItem;
    }

    /// <summary>
    /// Gets the parameter condition that determines when this sub-page is considered complete.
    /// </summary>
    public ParameterConditions ParameterCondition { get; private set; }

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        ParameterCondition = _subPage?.Node?.CompletionCondition ?? ParameterConditions.All;
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (GetIsDoneInputs(ParameterCondition).IsFalse())
        {
            return false;
        }

        return ResultPage?.GetIsDone();
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (Option.Mode.IsTaskOrPage())
        {
            setup.LabelWithIcon(Name, GroupName, Icon, prop => prop.WithStatus(GetStatus()));
        }
        else
        {
            setup.LabelWithIcon(Name, GroupName, Icon);
        }


        foreach (var element in ChildElements)
        {
            element.SetupView(setup);
        }
    }
}
