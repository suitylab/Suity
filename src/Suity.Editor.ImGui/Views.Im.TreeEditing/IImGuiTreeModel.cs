using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Defines a tree model for ImGui-based tree view rendering and editing.
/// </summary>
/// <typeparam name="T">The type of tree node items, which must be a reference type.</typeparam>
public interface IImGuiTreeModel<T> : VisualTreeVisitor<T>
    where T : class
{
    /// <summary>
    /// Event raised when the tree structure or its data changes.
    /// </summary>
    event EventHandler? TreeChanged;

    /// <summary>
    /// Gets or sets the visual tree data associated with this model.
    /// </summary>
    VisualTreeData<T>? TreeData { get; set; }
}

/// <summary>
/// Provides a base implementation for ImGui tree models that manages tree node data,
/// expansion state, and rendering properties.
/// </summary>
/// <typeparam name="T">The type of tree node items, which must be a reference type.</typeparam>
public abstract class ImGuiTreeModel<T> : IImGuiTreeModel<T>
    where T : class
{
    private VisualTreeData<T>? _treeData;
    private int _defaultHeight = ImGuiTreeView.DefaultRowHeight;

    private HashSet<T>? _expaneded;

    /// <inheritdoc/>
    public event EventHandler? TreeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiTreeModel{T}"/> class.
    /// </summary>
    public ImGuiTreeModel()
    { }

    /// <summary>
    /// Gets or sets the default row height for tree nodes that do not specify a custom height.
    /// </summary>
    public int DefaultHeight
    {
        get => _defaultHeight;
        set => _defaultHeight = value;
    }

    /// <inheritdoc/>
    public VisualTreeData<T>? TreeData
    {
        get => _treeData;
        set
        {
            _treeData = value;
            _treeData?.Refresh();
        }
    }

    /// <summary>
    /// Gets a unique identifier string for the specified tree node.
    /// </summary>
    /// <param name="value">The tree node to get the identifier for.</param>
    /// <returns>A unique string identifier for the node.</returns>
    public abstract string GetId(T value);

    /// <summary>
    /// Gets the root-level child nodes of the tree.
    /// </summary>
    /// <returns>An enumerable of root-level tree nodes, or null if there are none.</returns>
    public abstract IEnumerable<T>? GetChildNodes();

    /// <summary>
    /// Gets the child nodes of the specified parent node.
    /// </summary>
    /// <param name="value">The parent node to get children for.</param>
    /// <returns>An enumerable of child nodes, or null if the node has no children.</returns>
    public abstract IEnumerable<T>? GetChildNodes(T value);

    /// <summary>
    /// Determines whether the specified node can be expanded (i.e., has child nodes).
    /// </summary>
    /// <param name="value">The tree node to check.</param>
    /// <returns>True if the node has children and can be expanded; otherwise, false.</returns>
    public virtual bool GetCanExpand(T value) => GetChildNodes(value).Any();

    /// <summary>
    /// Gets the parent node of the specified tree node.
    /// </summary>
    /// <param name="value">The tree node to get the parent for.</param>
    /// <returns>The parent node, or null if the node is a root node or has no parent.</returns>
    public abstract T? GetParent(T value);

    /// <summary>
    /// Gets the row height for the specified tree node.
    /// </summary>
    /// <param name="value">The tree node to get the height for.</param>
    /// <returns>The row height for the node, or the default height if not overridden.</returns>
    public virtual int? GetHeight(T value) => _defaultHeight;

    /// <summary>
    /// Gets whether the specified tree node is currently expanded.
    /// </summary>
    /// <param name="value">The tree node to check.</param>
    /// <returns>True if the node is expanded; otherwise, false.</returns>
    public virtual bool? GetIsExpanded(T value)
    {
        return _expaneded?.Contains(value) == true;
    }

    /// <summary>
    /// Sets the expanded state of the specified tree node.
    /// </summary>
    /// <param name="value">The tree node to expand or collapse.</param>
    /// <param name="expand">True to expand the node; false to collapse it.</param>
    public virtual void SetIsExpanded(T value, bool expand)
    {
        if (value is null)
        {
            return;
        }

        if (expand)
        {
            if (_expaneded is null)
            {
                _expaneded = new HashSet<T>();
            }

            _expaneded.Add(value);
        }
        else
        {
            _expaneded?.Remove(value);
        }
    }
}