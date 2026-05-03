using Suity.Drawing;
using Suity.Editor.Flows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Base class for all AIGC flow nodes, providing async computation support.
/// </summary>
[DisplayText("AIGC", "*CoreIcon|AI")]
[ToolTipsText("AIGC related nodes")]
public abstract class AigcFlowNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.AI;

    /// <inheritdoc/>
    public virtual Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Compute(compute);

        var result = compute.GetResult(this);

        return Task.FromResult<object>(result);
    }

    /// <summary>
    /// Gets the workflow setup from the computation context, or throws if not found.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <returns>The workflow setup instance.</returns>
    /// <exception cref="NullReferenceException">Thrown when IWorkflowSetup is not found in the context.</exception>
    protected IWorkflowSetup GetWorkflowOrThrow(IFlowComputation compute)
    {
        var workflow = compute.Context.GetArgument<IWorkflowSetup>();
        if (workflow is null)
        {
            throw new NullReferenceException($"{nameof(IWorkflowSetup)} not found.");
        }

        return workflow;
    }
}