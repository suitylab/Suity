using Suity.Editor.Flows;
using Suity.Editor.Types;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Asset provider for creating workflow-based chat instances.
/// </summary>
[AssetAutoCreate]
[DisplayText("AIGC Workflow", "*CoreIcon|Workflow")]
[ToolTipsText("Run AIGC workflow.")]
public class WorkflowChatProvider : StandaloneAsset<ILLmChatProvider>, ILLmChatProvider
{
    /// <summary>
    /// Gets the singleton instance of the workflow chat provider.
    /// </summary>
    public static WorkflowChatProvider Instance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowChatProvider"/> class.
    /// </summary>
    public WorkflowChatProvider()
    {
        Instance ??= this;
    }

    /// <inheritdoc/>
    public ILLmChat CreateChat(FunctionContext context)
    {
        var option = context.GetArgument<AigcWorkflowOption>();
        if (option is null)
        {
            throw new AigcException($"{typeof(AigcWorkflowOption)} not found.");
        }

        if (option.Runnable is not { } runnable)
        {
            throw new AigcException($"{typeof(IAigcRunWorkflow)} not found.");
        }

        return new WorkflowChat(runnable, context);
    }
}

/// <summary>
/// Chat implementation for AIGC workflows.
/// </summary>
[DisplayText("AIGC Workflow", "*CoreIcon|Workflow")]
[NativeAlias("Suity.Editor.AIGC.Flows.WorkflowChat")]
internal class WorkflowChat : BaseFlowChat
{
    /// <summary>
    /// Gets the runnable workflow interface.
    /// </summary>
    public IAigcRunWorkflow Runnable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowChat"/> class.
    /// </summary>
    /// <param name="runnable">The runnable workflow.</param>
    /// <param name="context">Optional function context.</param>
    public WorkflowChat(IAigcRunWorkflow runnable, FunctionContext context = null)
        : base(runnable.Name, runnable.ToDisplayTextL(), context)
    {
        Runnable = runnable ?? throw new System.ArgumentNullException(nameof(runnable));
    }

    /// <inheritdoc/>
    protected override FlowNode GetStarterNode()
    {
        var flowNode = Runnable.GetStarterNode(Runner.Context);

        return flowNode;
    }

}