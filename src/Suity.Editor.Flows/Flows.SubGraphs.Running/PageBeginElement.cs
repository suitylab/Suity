using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Represents the beginning element of an AIGC page, serving as the entry point for page interactions.
/// </summary>
public class PageBeginElement : SubGraphElement, IPageValueElement
{
    private FlowNodeConnector _connector;
    private object _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBeginElement"/> class.
    /// </summary>
    /// <param name="actionItem">The flow diagram item associated with this begin element.</param>
    public PageBeginElement(FlowDiagramItem actionItem)
        : base(actionItem)
    {
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _connector;

    #region IPageValueElement

    /// <summary>
    /// Gets the type definition of the parameter associated with this page element.
    /// </summary>
    public TypeDefinition ParameterType { get; private set; }

    /// <summary>
    /// Gets the current value held by this element.
    /// </summary>
    public object Value => _value;

    /// <summary>
    /// Gets or sets a value indicating whether a value has been set. Always returns false for this element.
    /// </summary>
    public bool IsValueSet { get => false; set { } }

    /// <summary>
    /// Sets the value for this element.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(object value) => _value = value;

    /// <summary>
    /// Ensures and returns the current value.
    /// </summary>
    /// <returns>The current value.</returns>
    public object EnsureValue() => _value;

    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        ParameterType = (Node as IAigcTypeNode)?.TypeDef;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (Option.Mode != PageElementMode.Page)
        {
            return;
        }

        if (sync.Sync<ButtonValue>(DiagramItem.Name, ButtonValue.Empty) == ButtonValue.Clicked)
        {
            var view = context.GetService<IFlowView>();
            if (!IsInDiagram)
            {
                Logs.LogError(L("This page has expired, please reload."));
                return;
            }

            this.Root?.HandleBeginChat(this, view);
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (Option.Mode != PageElementMode.Page)
        {
            return;
        }

        setup.Button(new ViewProperty(Name, DisplayText, Icon));
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        if (ParameterType is { } typeDef && !TypeDefinition.IsNullOrEmpty(typeDef))
        {
            _connector = node.AddConnector(DiagramItem.Name, typeDef, FlowDirections.Input, FlowConnectorTypes.Action, true, Node?.DisplayText);
        }
        else
        {
            _connector = node.AddActionInputConnector(DiagramItem.Name, Node?.DisplayText);
        }
    }
}
