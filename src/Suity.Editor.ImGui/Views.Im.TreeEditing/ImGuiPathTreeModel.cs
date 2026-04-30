using Suity.Views.PathTree;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// A tree model that bridges <see cref="PathTreeModel"/> with the ImGui tree view system.
/// Provides path node hierarchy visualization and editing capabilities through the <see cref="IImGuiTreeModel{T}"/> interface.
/// </summary>
public class ImGuiPathTreeModel : PathTreeModel, IImGuiTreeModel<PathNode>
{
    private VisualTreeData<PathNode>? _treeData;
    private int _defaultHeight = ImGuiTreeView.DefaultRowHeight;

    /// <summary>
    /// Occurs when the tree structure or content has changed and requires a refresh.
    /// </summary>
    public event EventHandler? TreeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiPathTreeModel"/> class.
    /// </summary>
    public ImGuiPathTreeModel()
    {
    }

    /// <summary>
    /// Gets or sets the default row height for tree nodes in the ImGui view.
    /// </summary>
    public int DefaultHeight
    {
        get => _defaultHeight;
        set => _defaultHeight = value;
    }

    /// <inheritdoc/>
    protected override void OnSuspendLayout()
    {
        base.OnSuspendLayout();
    }

    /// <inheritdoc/>
    protected override void OnResumeLayout()
    {
        base.OnResumeLayout();

        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnNodeChanged(PathNode node)
    {
        if (Suspended)
        {
            return;
        }

        var parent = node.Parent;
        if (parent is null)
        {
            return;
        }

        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnStructureChanged()
    {
        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnStructureChanged(PathNode node)
    {
        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnNodeInserted(PathNode parent, int index, PathNode node)
    {
        RegisterPath(node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    /// <inheritdoc/>
    public override void OnNodeRemoved(PathNode parent, int index, PathNode node)
    {
        UnregisterPath(node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    private void RaiseRefresh()
    {
        _treeData?.QueueRefresh();

        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    #region IImGuiTreeModel

    /// <summary>
    /// Gets or sets the visual tree data associated with this model.
    /// Setting this property triggers an immediate refresh of the tree view.
    /// </summary>
    public VisualTreeData<PathNode>? TreeData
    {
        get => _treeData;
        set
        {
            _treeData = value;
            _treeData?.Refresh();
        }
    }

    /// <summary>
    /// Gets the child nodes of the root node.
    /// </summary>
    /// <returns>An enumerable of root-level <see cref="PathNode"/> objects, or null if there are no root nodes.</returns>
    public IEnumerable<PathNode>? GetChildNodes()
    {
        return RootNode.NodeList;
    }

    /// <summary>
    /// Gets the child nodes of the specified parent node.
    /// </summary>
    /// <param name="node">The parent node whose children to retrieve.</param>
    /// <returns>An enumerable of child <see cref="PathNode"/> objects, or null if the node has no children.</returns>
    public IEnumerable<PathNode>? GetChildNodes(PathNode node)
    {
        return node.NodeList;
    }

    /// <summary>
    /// Determines whether the specified node can be expanded to show child nodes.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node can be expanded; otherwise, false.</returns>
    public bool GetCanExpand(PathNode node) => node.CanExpand;

    /// <summary>
    /// Gets the display height for the specified tree node.
    /// </summary>
    /// <param name="node">The node to get the height for.</param>
    /// <returns>The default row height for all nodes.</returns>
    public int? GetHeight(PathNode node)
    {
        return _defaultHeight;
    }

    /// <summary>
    /// Gets a unique identifier for the specified node.
    /// </summary>
    /// <param name="node">The node to get the ID for.</param>
    /// <returns>The unique node ID string.</returns>
    public string GetId(PathNode node)
    {
        return node.NodeId;
    }

    /// <summary>
    /// Gets whether the specified node is currently in an expanded state.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node is expanded; false if collapsed or if the node is not a <see cref="PopulatePathNode"/>.</returns>
    public bool? GetIsExpanded(PathNode node)
    {
        if (node is PopulatePathNode pNode)
        {
            return pNode.Expanded;
        }

        return false;
    }

    /// <summary>
    /// Gets the parent node of the specified node.
    /// </summary>
    /// <param name="node">The node to get the parent for.</param>
    /// <returns>The parent <see cref="PathNode"/>, or null if the node is a root node.</returns>
    public PathNode? GetParent(PathNode node)
    {
        return node.Parent;
    }



    #endregion
}