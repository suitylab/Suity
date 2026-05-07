using Suity.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;

/// <summary>
/// Base class for flow nodes that interact with external services or systems.
/// </summary>
[DisplayText("External", "*CoreIcon|System")]
[ToolTipsText("External service related nodes")]
public abstract class ExternalNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override ImageDef Icon => GetType().ToDisplayIcon() ?? CoreIconCache.System;

    /// <summary>
    /// Asynchronously computes the node's output.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Compute(compute);

        return Task.FromResult<object>(null);
    }
}
