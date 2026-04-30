using Suity.Editor.Flows;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Base class for data row related AIGC flow nodes, providing async computation support.
/// </summary>
[DisplayText("Data Row", "*CoreIcon|Row")]
[ToolTipsText("Data row related nodes")]
public abstract class AigcDataRowNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override Image Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Row;

    /// <inheritdoc/>
    public virtual Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Compute(compute);

        return Task.FromResult<object>(null);
    }
}