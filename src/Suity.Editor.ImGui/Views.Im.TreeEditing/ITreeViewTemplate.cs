namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Defines the template for rendering a tree view with ImGui.
/// </summary>
/// <typeparam name="T">The type of data represented by each tree node.</typeparam>
public interface ITreeViewTemplate<T>
    where T : class
{
    /// <summary>
    /// Renders the tree view GUI.
    /// </summary>
    /// <param name="treeViewNode">The root ImGui node for the tree view.</param>
    void TreeViewGui(ImGuiNode treeViewNode);

    /// <summary>
    /// Renders the header section of the tree view.
    /// </summary>
    /// <param name="headerNode">The ImGui node for the header.</param>
    /// <param name="heightHeight">Optional height override for the header.</param>
    void HeaderGui(ImGuiNode headerNode, float? heightHeight = null);

    /// <summary>
    /// Renders a single row in the tree view.
    /// </summary>
    /// <param name="rowNode">The ImGui node for the row.</param>
    /// <param name="item">The visual tree node data to display.</param>
    void RowGui(ImGuiNode rowNode, VisualTreeNode<T> item);

    /// <summary>
    /// Called when a row begins editing mode.
    /// </summary>
    /// <param name="rowNode">The ImGui node for the row being edited.</param>
    void BeginRowEdit(ImGuiNode rowNode);
}