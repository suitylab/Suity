using Suity.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.WorkSpaces;

/// <summary>
/// Base class for flow nodes that interact with workspaces.
/// </summary>
[DisplayText("WorkSpace", "*CoreIcon|WorkSpace")]
[ToolTipsText("WorkSpace related nodes")]
public abstract class AigcWorkSpaceNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override ImageDef Icon => GetType().ToDisplayIcon() ?? CoreIconCache.WorkSpace;

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
