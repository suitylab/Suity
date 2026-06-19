namespace Suity.Editor.Flows.Agentic;

/// <summary>
/// Base class for AIGC page operation flow nodes.
/// Provides common functionality for nodes that interact with AIGC pages.
/// </summary>
[DisplayText("Agent Node", "*CoreIcon|Agent")]
[ToolTipsText("AIGC agent operation related nodes")]
public abstract class AgentNode : FlowNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentNode"/> class.
    /// </summary>
    protected AgentNode()
        : base()
    {
    }
}