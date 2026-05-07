using Suity.Editor.Flows;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Base class for AIGC page operation flow nodes.
/// Provides common functionality for nodes that interact with AIGC pages.
/// </summary>
[DisplayText("AIGC Page Operations", "*CoreIcon|Page")]
[ToolTipsText("AIGC page operation related nodes")]
public abstract class AigcPageNode : FlowNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AigcPageNode"/> class.
    /// </summary>
    protected AigcPageNode()
        : base()
    {
    }
}