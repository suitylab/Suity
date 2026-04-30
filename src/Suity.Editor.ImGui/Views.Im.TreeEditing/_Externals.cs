using Suity.Views.PathTree;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides external abstraction for ImGui-based tree view operations.
/// </summary>
/// <typeparam name="T">The type of data represented by tree nodes.</typeparam>
internal abstract class ImGuiTreeViewExternal<T>
    where T : class
{
    /// <summary>
    /// Gets or sets the tree model that provides data for the tree view.
    /// </summary>
    public abstract IImGuiTreeModel<T>? TreeModel { get; set; }

    /// <summary>
    /// Gets the visual tree data containing the hierarchical representation of nodes.
    /// </summary>
    public abstract VisualTreeData<T>? TreeData { get; }

    /// <summary>
    /// Gets a value indicating whether the tree view is currently watching for selection changes.
    /// </summary>
    public abstract bool SelectionWatching { get; }

    /// <summary>
    /// Renders the tree view GUI and returns the resulting tree node.
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering.</param>
    /// <param name="id">The unique identifier for the tree view.</param>
    /// <param name="config">An optional action to configure the tree node.</param>
    /// <returns>The rendered <see cref="ImGuiNode"/> representing the tree view.</returns>
    public abstract ImGuiNode OnGui(ImGui gui, string id, Action<ImGuiNode>? config);

    /// <summary>
    /// Searches for and returns the tree view node in the current GUI hierarchy.
    /// </summary>
    /// <returns>The <see cref="ImGuiNode"/> if found; otherwise, <c>null</c>.</returns>
    public abstract ImGuiNode? FindTreeViewNode();

    /// <summary>
    /// Creates a new visual tree data structure from the specified visitor.
    /// </summary>
    /// <param name="visitor">The visitor used to traverse and collect tree nodes.</param>
    /// <param name="defaultHeight">The default height for the visual tree.</param>
    /// <returns>A new <see cref="VisualTreeData{T}"/> instance.</returns>
    public abstract VisualTreeData<T> CreateVisualTreeData(VisualTreeVisitor<T> visitor, int defaultHeight);

    /// <summary>
    /// Selects a single node in the tree view.
    /// </summary>
    /// <param name="node">The node to select.</param>
    /// <param name="append">If <c>true</c>, adds to the current selection; otherwise, replaces it.</param>
    /// <param name="notify">If <c>true</c>, triggers selection change notifications.</param>
    /// <returns>The selected <see cref="VisualTreeNode"/> if successful; otherwise, <c>null</c>.</returns>
    public abstract VisualTreeNode? SelectNode(T node, bool append, bool notify);

    /// <summary>
    /// Selects multiple nodes in the tree view.
    /// </summary>
    /// <param name="nodes">The collection of nodes to select.</param>
    /// <param name="notify">If <c>true</c>, triggers selection change notifications.</param>
    public abstract void SelectNodes(IEnumerable<T> nodes, bool notify);

    /// <summary>
    /// Executes an action without triggering selection watch callbacks.
    /// </summary>
    /// <param name="action">The action to execute while selection watching is disabled.</param>
    public abstract void UnwatchedSelectionAction(Action action);

    /// <summary>
    /// Scrolls the tree view to make the specified visual tree node visible.
    /// </summary>
    /// <param name="gui">The ImGui instance used for scrolling.</param>
    /// <param name="treeNode">The visual tree node to scroll to.</param>
    /// <returns><c>true</c> if scrolling was performed; otherwise, <c>false</c>.</returns>
    public abstract bool ScrollToPosition(ImGui gui, VisualTreeNode treeNode);

    /// <summary>
    /// Scrolls the tree view to make the specified visual tree node visible.
    /// </summary>
    /// <param name="treeView">The tree view node to scroll within.</param>
    /// <param name="treeNode">The visual tree node to scroll to.</param>
    /// <returns><c>true</c> if scrolling was performed; otherwise, <c>false</c>.</returns>
    public abstract bool ScrollToPosition(ImGuiNode treeView, VisualTreeNode treeNode);

    /// <summary>
    /// Handles keyboard navigation to move the selection up in the tree.
    /// </summary>
    /// <param name="treeView">The tree view node to navigate within.</param>
    /// <returns>The resulting input state after handling the move.</returns>
    public abstract GuiInputState HandleMoveUp(ImGuiNode treeView);

    /// <summary>
    /// Handles keyboard navigation to move the selection down in the tree.
    /// </summary>
    /// <param name="treeView">The tree view node to navigate within.</param>
    /// <returns>The resulting input state after handling the move.</returns>
    public abstract GuiInputState HandleMoveDown(ImGuiNode treeView);

    /// <summary>
    /// Queues a refresh of the tree view and returns the refreshed node.
    /// </summary>
    /// <returns>The refreshed <see cref="ImGuiNode"/> if successful; otherwise, <c>null</c>.</returns>
    public abstract ImGuiNode? QueueRefresh();

    /// <summary>
    /// Handles any necessary updates when the context menu has changed.
    /// </summary>
    public abstract void HandleMenuChanged();
}

/// <summary>
/// Provides external abstraction for path-based tree view operations, including drag-and-drop support.
/// </summary>
internal abstract class ImGuiPathTreeExternal
{
    /// <summary>
    /// The singleton instance of the external implementation.
    /// </summary>
    internal static ImGuiPathTreeExternal _external;

    /// <summary>
    /// Creates an external tree view wrapper for the specified tree view.
    /// </summary>
    /// <typeparam name="T">The type of data represented by tree nodes.</typeparam>
    /// <param name="treeView">The tree view to wrap.</param>
    /// <returns>A new <see cref="ImGuiTreeViewExternal{T}"/> instance.</returns>
    public abstract ImGuiTreeViewExternal<T> CreateTreeViewEx<T>(ImGuiTreeView<T> treeView)
        where T : class;

    /// <summary>
    /// Handles drag-over events for the path tree view.
    /// </summary>
    /// <param name="treeView">The path tree view receiving the drag event.</param>
    /// <param name="dropEvent">The drag event containing drop information.</param>
    /// <returns><c>true</c> if the drag-over was handled; otherwise, <c>false</c>.</returns>
    public abstract bool HandleDragOver(ImGuiPathTreeView treeView, IDragEvent dropEvent);

    /// <summary>
    /// Handles drag-drop events for the path tree view.
    /// </summary>
    /// <param name="treeView">The path tree view receiving the drop event.</param>
    /// <param name="dropEvent">The drag event containing drop information.</param>
    public abstract void HandleDragDrop(ImGuiPathTreeView treeView, IDragEvent dropEvent);

    /// <summary>
    /// Determines the target path node for a drop operation based on the current drag state.
    /// </summary>
    /// <param name="node">The ImGui node involved in the drag operation.</param>
    /// <param name="mode">The drag-and-drop mode being used.</param>
    /// <returns>The target <see cref="PathNode"/> for dropping; otherwise, <c>null</c>.</returns>
    public abstract PathNode? GetPathTreeDroppingNode(ImGuiNode node, ImTreeNodeDragDropMode mode);
}