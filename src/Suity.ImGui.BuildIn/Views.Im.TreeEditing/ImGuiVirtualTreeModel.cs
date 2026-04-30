using Suity.Editor.VirtualTree;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents a virtual tree model for ImGui-based tree views, implementing tree data management and node operations.
/// </summary>
public class ImGuiVirtualTreeModel : VirtualTreeModel, IImGuiTreeModel<VirtualNode>
{
    private VisualTreeData<VirtualNode>? _treeData;
    private int _defaultHeight = ImGuiTreeView.DefaultRowHeight;

    /// <summary>
    /// Occurs when the tree structure changes.
    /// </summary>
    public event EventHandler? TreeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiVirtualTreeModel"/> class.
    /// </summary>
    public ImGuiVirtualTreeModel()
    {
    }

    /// <summary>
    /// Gets or sets the default height for tree nodes.
    /// </summary>
    public int DefaultHeight
    {
        get => _defaultHeight;
        set => _defaultHeight = value;
    }

    /// <summary>
    /// Gets or sets the visual tree data associated with this model.
    /// </summary>
    public VisualTreeData<VirtualNode>? TreeData
    {
        get => _treeData;
        set
        {
            _treeData = value;
            _treeData?.Refresh();
        }
    }

    #region Notify

    /// <inheritdoc/>
    public override void NotifyNodeChanged(VirtualNode node)
    {
        _treeData?.QueueRefresh();
        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public override void NotifyNodeInserted(VirtualNode parent, int index, VirtualNode node)
    {
        _treeData?.QueueRefresh();
        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public override void NotifyNodeRemoved(VirtualNode parent, int index, VirtualNode node)
    {
        _treeData?.QueueRefresh();
        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Expand handling

    /// <inheritdoc/>
    public override bool IsExpanded(VirtualNode virtualNode)
    {
        return virtualNode.TempExpanded;
    }

    /// <inheritdoc/>
    public override void Expand(VirtualNode virtualNode)
    {
        if (!virtualNode.TempExpanded)
        {
            virtualNode.TempExpanded = true;
            virtualNode.PerformGetValue();
        }
    }

    #endregion

    #region IVirtualTreeVisitor

    /// <inheritdoc/>
    public IEnumerable<VirtualNode> GetChildNodes() => Root.Nodes;

    /// <inheritdoc/>
    public IEnumerable<VirtualNode> GetChildNodes(VirtualNode node) => node.Nodes;

    /// <inheritdoc/>
    public bool GetCanExpand(VirtualNode node) => node.Nodes.Count > 0;

    /// <inheritdoc/>
    public VirtualNode GetParent(VirtualNode node) => node.Parent;

    /// <inheritdoc/>
    public int? GetHeight(VirtualNode node) => _defaultHeight;

    /// <inheritdoc/>
    public string GetId(VirtualNode node) => node.GetFullId();

    /// <inheritdoc/>
    public bool? GetIsExpanded(VirtualNode node) => node.TempExpanded;

    /// <inheritdoc/>
    public void SetIsExpanded(VirtualNode node, bool expand)
    {
        if (node.TempExpanded != expand)
        {
            node.TempExpanded = expand;
            if (expand)
            {
                node.PerformGetValue();
            }
        }
    }

    #endregion
}