using Suity.Drawing;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC.Flows;

#region WorkflowNode

/// <summary>
/// The starting node of an AIGC workflow, serving as the entry point for workflow execution.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Workflow)]
[DisplayText("AIGC Workflow", "*CoreIcon|Workflow")]
[ToolTipsText("AIGC workflow starting node")]
public class WorkflowNode : DesignFlowNode, IAigcRunWorkflow, IWorkflowSetup
{
    private readonly FlowNodeConnector _begin;
    private readonly FlowNodeConnector _prompt;

    /// <summary>
    /// Gets the begin connector for the workflow.
    /// </summary>
    public FlowNodeConnector BeginConnector => _begin;

    /// <summary>
    /// Gets the prompt connector for the workflow input.
    /// </summary>
    public FlowNodeConnector PromptConnector => _prompt;

    
    private readonly AssetProperty<LLmModelAsset> _defaultModel
        = new("DefaultModel", "Default Language Model");


    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowNode"/> class.
    /// </summary>
    public WorkflowNode()
    {
        _begin = AddActionOutputConnector("Begin", "Start");
        _prompt = AddDataOutputConnector("Prompt", "*System|String", "Prompt");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _defaultModel.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.Label("Process");
        _defaultModel.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override bool IsDataNode => false;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute) => compute.SetResult(this, _begin);

    #region IAigcRunWorkflow
    /// <summary>
    /// Gets the starter node for workflow execution.
    /// </summary>
    /// <param name="ctx">The function context.</param>
    /// <returns>The workflow node itself as the starting point.</returns>
    public FlowNode GetStarterNode(FunctionContext ctx)
    {
        ctx.SetArgument<IWorkflowSetup>(this);

        return this;
    }

    #endregion

    #region IWorkflow

    /// <summary>
    /// Gets the default language model for this workflow.
    /// </summary>
    public ILLmModel DefaultModel => _defaultModel.Target;

    /// <summary>
    /// Gets the diagrams included in this workflow.
    /// </summary>
    public AigcDiagramAsset[] IncludeDiagrams => [];

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI calls.
    /// </summary>
    public bool PauseOnAICall => AIgcFlowPlugin.Instance?.PauseOnAICall ?? false;

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI log output.
    /// </summary>
    public bool PauseOnAILog => AIgcFlowPlugin.Instance?.PauseOnAILog ?? false;

    #endregion
}

#endregion

#region WorkflowDiagramItem

/// <summary>
/// Diagram item representing a <see cref="WorkflowNode"/> in the flow diagram.
/// </summary>
public class WorkflowDiagramItem : FlowDiagramItem<WorkflowNode, WorkflowAssetBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowDiagramItem"/> class.
    /// </summary>
    public WorkflowDiagramItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The workflow node to represent.</param>
    public WorkflowDiagramItem(WorkflowNode node)
        : base(node)
    {
    }

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Workflow";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => NamingVerifier.VerifyIdentifier(name);
}

#endregion

#region WorkflowAsset

/// <summary>
/// Workflow starting node resource that provides chat capabilities.
/// </summary>
public class WorkflowAsset : Asset, ILLmChatProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowAsset"/> class.
    /// </summary>
    public WorkflowAsset()
    {
        UpdateAssetTypes(this.GetType(), typeof(ILLmChatProvider));
    }

    /// <summary>
    /// Gets the default icon for this asset.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.Workflow;

    /// <summary>
    /// Creates a chat instance for this workflow.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <returns>A new <see cref="WorkflowChat"/> instance, or null if the node cannot be resolved.</returns>
    public ILLmChat CreateChat(FunctionContext context)
    {
        var node = (this.GetStorageObject(true) as WorkflowDiagramItem)?.Node;

        if (node != null)
        {
            return new WorkflowChat(node, context);
        }

        return null;
    }
}

/// <summary>
/// Builder class for creating <see cref="WorkflowAsset"/> instances.
/// </summary>
public class WorkflowAssetBuilder : AssetBuilder<WorkflowAsset>
{
}

#endregion
