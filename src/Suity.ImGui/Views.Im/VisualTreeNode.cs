using System;

namespace Suity.Views.Im;

/// <summary>
/// Abstract base class for tree view nodes in the visual tree.
/// </summary>
public abstract class VisualTreeNode
{
    private bool _isSelected;

    /// <summary>
    /// Internal flag indicating whether this node is currently pooled.
    /// </summary>
    internal bool _pooled;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualTreeNode"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    public VisualTreeNode(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    /// <summary>
    /// Gets the tree data this node belongs to.
    /// </summary>
    public abstract VisualTreeData Tree { get; }

    /// <summary>
    /// Gets the underlying data object as an object.
    /// </summary>
    public abstract object ValueObject { get; }

    /// <summary>
    /// Gets the unique identifier for this node.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the index of this node within its parent.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets or sets the path to this node in the ImGui tree.
    /// </summary>
    public ImGuiPath? NodePath { get; internal set; }

    /// <summary>
    /// Gets the parent node, or null if this is a root node.
    /// </summary>
    public virtual VisualTreeNode? Parent => null;

    /// <summary>
    /// Gets or sets whether this node can be expanded.
    /// </summary>
    public bool CanExpand { get; internal set; }

    /// <summary>
    /// Gets or sets whether this node is expanded.
    /// </summary>
    public virtual bool Expanded { get; set; }

    /// <summary>
    /// Gets or sets whether this node is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the indentation level of this node.
    /// </summary>
    public float Indent { get; internal set; }

    /// <summary>
    /// Gets or sets the height of this node.
    /// </summary>
    public float Height { get; internal set; }

    /// <summary>
    /// Gets or sets the position of this node in the list.
    /// </summary>
    public float Position { get; internal set; }

    /// <summary>
    /// Gets or sets whether the mouse is pressed on this node.
    /// </summary>
    public bool MouseDown { get; set; }

    /// <summary>
    /// Is drag and drop included in the request
    /// </summary>
    public bool DragRequesting { get; set; }

    /// <summary>
    /// Refreshes this node's data.
    /// </summary>
    public virtual void Refresh()
    {
    }
}

/// <summary>
/// Generic tree node with typed value.
/// </summary>
/// <typeparam name="T">The type of the node's underlying data.</typeparam>
public class VisualTreeNode<T> : VisualTreeNode
    where T : class
{
    /// <summary>
    /// Gets the tree data model for this node.
    /// </summary>
    public VisualTreeData<T> TreeModel { get; }

    /// <inheritdoc/>
    public override VisualTreeData Tree => TreeModel;

    /// <inheritdoc/>
    public override object ValueObject => Value;

    /// <summary>
    /// Gets the typed value of this node.
    /// </summary>
    public T Value { get; internal set; }

    /// <summary>
    /// Gets the parent typed node, or null if this is a root node.
    /// </summary>
    public VisualTreeNode<T>? ParentNode { get; internal set; }

    /// <inheritdoc/>
    public override VisualTreeNode? Parent => ParentNode;

    /// <inheritdoc/>
    public override bool Expanded
    {
        get => base.Expanded;
        set
        {
            base.Expanded = value;
            TreeModel.QueueRefresh();
            TreeModel.Visitor.SetIsExpanded(Value, value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualTreeNode{T}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="tree">The tree data model.</param>
    /// <param name="value">The node's underlying data.</param>
    public VisualTreeNode(string id, VisualTreeData<T> tree, T value)
        : base(id)
    {
        TreeModel = tree ?? throw new ArgumentNullException(nameof(tree));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc/>
    public override void Refresh()
    {
        TreeModel.Refresh();
    }
}
