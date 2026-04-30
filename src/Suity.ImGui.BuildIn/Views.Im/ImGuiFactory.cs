namespace Suity.Views.Im;

/// <summary>
/// Abstract factory for creating and recycling ImGui nodes.
/// </summary>
public abstract class ImGuiFactory
{
    /// <summary>
    /// Creates a new ImGui node with the specified GUI context and ID.
    /// </summary>
    /// <param name="gui">The ImGui context.</param>
    /// <param name="id">The local ID for the node.</param>
    /// <returns>A new <see cref="ImGuiNode"/> instance.</returns>
    public abstract ImGuiNode CreateNode(ImGui gui, string id);

    /// <summary>
    /// Recycles a previously created node back to the pool.
    /// </summary>
    public abstract void RecycleNode();
}