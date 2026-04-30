using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

#region Enums

/// <summary>
/// Specifies the selection mode for tree view controls.
/// </summary>
public enum ImTreeViewSelectionMode
{
    /// <summary>
    /// Only a single node can be selected at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple nodes can be selected across the entire tree.
    /// </summary>
    Multiple,

    /// <summary>
    /// Multiple nodes can be selected, but only within the same parent.
    /// </summary>
    MultipleSameParent,
}

/// <summary>
/// Specifies the drag and drop mode for tree node operations.
/// </summary>
public enum ImTreeNodeDragDropMode
{
    /// <summary>
    /// No drag and drop operation.
    /// </summary>
    None,

    /// <summary>
    /// Drop inside the target node (as a child).
    /// </summary>
    Inside,

    /// <summary>
    /// Drop before the target node.
    /// </summary>
    Previous,

    /// <summary>
    /// Drop after the target node.
    /// </summary>
    Next,
}

#endregion

#region VisualTreeVisitor<T>

/// <summary>
/// Interface for visiting and traversing tree node data structures.
/// </summary>
/// <typeparam name="T">The type of tree node data.</typeparam>
public interface VisualTreeVisitor<T>
    where T : class
{
    /// <summary>
    /// Gets the root child nodes of the tree.
    /// </summary>
    IEnumerable<T>? GetChildNodes();

    /// <summary>
    /// Gets the child nodes of the specified parent node.
    /// </summary>
    IEnumerable<T>? GetChildNodes(T value);

    /// <summary>
    /// Gets whether the specified node can be expanded.
    /// </summary>
    public bool GetCanExpand(T value);

    /// <summary>
    /// Gets the parent of the specified node.
    /// </summary>
    T? GetParent(T value);

    /// <summary>
    /// Gets the unique identifier for the specified node.
    /// </summary>
    string GetId(T value);

    /// <summary>
    /// Gets the height of the specified node, or null for default height.
    /// </summary>
    int? GetHeight(T value);

    /// <summary>
    /// Gets whether the specified node is expanded, or null for default state.
    /// </summary>
    bool? GetIsExpanded(T value);

    /// <summary>
    /// Sets whether the specified node is expanded.
    /// </summary>
    void SetIsExpanded(T value, bool expand);
}

#endregion

#region VisualTreeData

/// <summary>
/// Abstract base class for tree view data models that manage node selection, expansion, and drag-drop operations.
/// </summary>
public abstract class VisualTreeData
{
    /// <summary>
    /// Gets the underlying list data for the tree view.
    /// </summary>
    public abstract VisualListData ListData { get; }

    /// <summary>
    /// Gets the currently selected node, or null if no selection.
    /// </summary>
    public abstract VisualTreeNode? SelectedNode { get; }

    /// <summary>
    /// Gets all currently selected nodes.
    /// </summary>
    public abstract IEnumerable<VisualTreeNode> SelectedNodes { get; }

    /// <summary>
    /// Gets or sets whether nodes should be initially expanded.
    /// </summary>
    public bool InitExpand { get; set; }

    /// <summary>
    /// Gets or sets the width of indentation per level.
    /// </summary>
    public float IndentWidth { get; set; } = 8;

    /// <summary>
    /// Gets or sets the spacing between tree nodes.
    /// </summary>
    public abstract float Spacing { get; set; }

    /// <summary>
    /// Gets or sets the width of the tree view.
    /// </summary>
    public abstract float? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of tree node headers.
    /// </summary>
    public abstract float? HeaderHeight { get; set; }

    /// <summary>
    /// Refresh request, used to avoid repeated refreshes caused by multiple event triggers
    /// </summary>
    public bool RefreshRequired { get; protected set; } = true;

    /// <summary>
    /// Gets or sets the selection mode for the tree view.
    /// </summary>
    /// <summary>
    /// Gets or sets the selection mode for the tree view.
    /// </summary>
    public ImTreeViewSelectionMode SelectionMode { get; set; }

    /// <summary>
    /// Refreshes the tree data.
    /// </summary>
    public abstract void Refresh();

    /// <summary>
    /// Checks if a refresh is required and performs it if so.
    /// </summary>
    public void CheckRefresh()
    {
        if (RefreshRequired)
        {
            RefreshRequired = false;
            Refresh();
        }
    }

    /// <summary>
    /// Queues a refresh request.
    /// </summary>
    public void QueueRefresh()
    {
        RefreshRequired = true;
    }

    /// <summary>
    /// Cleans up pooled tree nodes.
    /// </summary>
    public abstract void CleanUpPool();

    /// <summary>
    /// Gets a tree node by its ID.
    /// </summary>
    /// <param name="id">The node ID.</param>
    /// <returns>The tree node.</returns>
    public abstract VisualTreeNode GetNode(string id);

    #region Selection

    /// <summary>
    /// Adds a node to the current selection.
    /// </summary>
    public abstract void AppendSelection(VisualTreeNode node);

    /// <summary>
    /// Sets the selection to a single node.
    /// </summary>
    public abstract void SetSelection(VisualTreeNode node);

    /// <summary>
    /// Sets the selection to multiple nodes.
    /// </summary>
    public abstract void SetSelections(IEnumerable<VisualTreeNode> nodes);

    /// <summary>
    /// Sets the selection to a range of nodes by index.
    /// </summary>
    public abstract void SetSelections(int fromIndex, int toIndex);

    /// <summary>
    /// Toggles the selection state of a node.
    /// </summary>
    public abstract void ToggleSelection(VisualTreeNode node);

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public abstract void ClearSelection();

    #endregion

    #region Drag Drop

    /// <summary>
    /// Gets the node currently being dropped onto.
    /// </summary>
    public VisualTreeNode? DroppingNode { get; private set; }

    /// <summary>
    /// Gets the current drag and drop mode.
    /// </summary>
    public ImTreeNodeDragDropMode DroppingMode { get; private set; }

    /// <summary>
    /// Gets or sets whether the drop action has been performed.
    /// </summary>
    public bool DropAction { get; set; }

    /// <summary>
    /// Sets the current dropping node and mode internally.
    /// </summary>
    internal void SetDroppingNode(VisualTreeNode node, ImTreeNodeDragDropMode mode, bool dropAction)
    {
        DroppingNode = node ?? throw new ArgumentNullException(nameof(node));
        DroppingMode = mode;
        DropAction = dropAction;
    }

    /// <summary>
    /// Clears the current dropping node internally.
    /// </summary>
    internal void ClearDroppingNode()
    {
        if (DroppingNode is { })
        {
            DroppingNode = null;
            DroppingMode = ImTreeNodeDragDropMode.None;
        }
    }

    #endregion
}

#endregion

#region VisualTreeData<T>

/// <summary>
/// Generic abstract base class for tree view data models with typed node values.
/// </summary>
/// <typeparam name="T">The type of node data.</typeparam>
public abstract class VisualTreeData<T> : VisualTreeData
    where T : class
{
    private readonly VisualTreeVisitor<T> _visitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualTreeData{T}"/> class.
    /// </summary>
    /// <param name="visitor">The visitor for traversing tree nodes.</param>
    public VisualTreeData(VisualTreeVisitor<T> visitor)
    {
        _visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
    }

    /// <summary>
    /// Gets the tree visitor.
    /// </summary>
    public VisualTreeVisitor<T> Visitor => _visitor;

    /// <summary>
    /// Event raised when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Ensures a tree node exists for the given value, creating one if necessary.
    /// </summary>
    public abstract VisualTreeNode<T> EnsureNode(T value);

    /// <summary>
    /// Gets the currently selected typed node, or null if no selection.
    /// </summary>
    public abstract VisualTreeNode<T>? SelectedNodeT { get; }

    /// <summary>
    /// Gets all currently selected typed nodes.
    /// </summary>
    public abstract IEnumerable<VisualTreeNode<T>> SelectedNodesT { get; }

    /// <summary>
    /// Gets or sets the template for rendering tree node headers.
    /// </summary>
    public abstract ContentTemplate? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering tree node rows.
    /// </summary>
    public abstract ContentTemplate<VisualTreeNode<T>>? RowTemplate { get; set; }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

#endregion

#region EmptyVisualTreeVisitor

/// <summary>
/// A no-op implementation of VisualTreeVisitor that returns empty results.
/// </summary>
/// <typeparam name="T">The type of tree node data.</typeparam>
public class EmptyVisualTreeVisitor<T> : VisualTreeVisitor<T>
    where T : class
{
    /// <summary>
    /// Gets the singleton empty visitor instance.
    /// </summary>
    public static EmptyVisualTreeVisitor<T> Empty = new EmptyVisualTreeVisitor<T>();

    private EmptyVisualTreeVisitor()
    {
    }

    /// <inheritdoc/>
    public IEnumerable<T>? GetChildNodes() => null;

    /// <inheritdoc/>
    public IEnumerable<T>? GetChildNodes(T value) => null;

    /// <inheritdoc/>
    public bool GetCanExpand(T value) => false;

    /// <inheritdoc/>
    public int? GetHeight(T value) => null;

    /// <inheritdoc/>
    public string GetId(T value) => "---";

    /// <inheritdoc/>
    public bool? GetIsExpanded(T value) => null;

    /// <inheritdoc/>
    public T? GetParent(T value) => null;

    /// <inheritdoc/>
    public void SetIsExpanded(T value, bool expand)
    { }
}

#endregion
