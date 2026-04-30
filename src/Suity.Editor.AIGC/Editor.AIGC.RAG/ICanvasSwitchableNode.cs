namespace Suity.Editor.AIGC.RAG;

/// <summary>
/// Interface for nodes that can switch canvases and provide knowledge inputs
/// </summary>
public interface ICanvasSwitchableNode
{
    /// <summary>
    /// Gets the knowledge base inputs for this node
    /// </summary>
    /// <returns>Array of knowledge base instances</returns>
    IKnowledgeBase[] GetKnowledgeInputs();
}