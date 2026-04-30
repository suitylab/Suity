using Suity.Views.PathTree;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides extension methods for path tree view drag and drop operations.
/// </summary>
public static class ImGuiPathTreeExtensions
{
    /// <summary>
    /// Handles the drag-over event for the path tree view.
    /// </summary>
    /// <param name="treeView">The path tree view instance.</param>
    /// <param name="dropEvent">The drag event containing drop information.</param>
    /// <returns>True if the drag-over was handled; otherwise, false.</returns>
    public static bool HandleDragOver(this ImGuiPathTreeView treeView, IDragEvent dropEvent)
        => ImGuiPathTreeExternal._external.HandleDragOver(treeView, dropEvent);

    /// <summary>
    /// Handles the drag-drop event for the path tree view.
    /// </summary>
    /// <param name="treeView">The path tree view instance.</param>
    /// <param name="dropEvent">The drag event containing drop information.</param>
    public static void HandleDragDrop(this ImGuiPathTreeView treeView, IDragEvent dropEvent)
        => ImGuiPathTreeExternal._external.HandleDragDrop(treeView, dropEvent);

    /// <summary>
    /// Gets the path tree node that is the current drop target.
    /// </summary>
    /// <param name="node">The ImGui node to check.</param>
    /// <param name="mode">The drag and drop mode.</param>
    /// <returns>The path node being dropped on, or null if none.</returns>
    public static PathNode? GetPathTreeDroppingNode(this ImGuiNode node, ImTreeNodeDragDropMode mode)
        => ImGuiPathTreeExternal._external.GetPathTreeDroppingNode(node, mode);
}