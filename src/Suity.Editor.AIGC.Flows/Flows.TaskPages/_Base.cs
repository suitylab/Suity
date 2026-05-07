using Suity.Editor.Flows;

namespace Suity.Editor.Flows.TaskPages;

/// <summary>
/// Base class for AIGC page operation flow nodes.
/// Provides common functionality for nodes that interact with AIGC pages.
/// </summary>
[DisplayText("AIGC Page Operations", "*CoreIcon|Page")]
[ToolTipsText("AIGC page operation related nodes")]
public abstract class TaskPageNode : FlowNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPageNode"/> class.
    /// </summary>
    protected TaskPageNode()
        : base()
    {
    }
}