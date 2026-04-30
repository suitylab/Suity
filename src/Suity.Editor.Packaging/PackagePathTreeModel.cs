using Suity.Views.Im;
using Suity.Views.Im.TreeEditing;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Editor.Packaging;

/// <summary>
/// A tree model specialized for package path operations, supporting ImGui tree rendering and change notifications.
/// </summary>
public class PackagePathTreeModel : PathTreeModel, IImGuiTreeModel<PathNode>
{
    private VisualTreeData<PathNode> _treeData;
    private int _defaultHeight = ImGuiTreeView.DefaultRowHeight;

    private long _incr;

    /// <summary>
    /// Occurs when the tree structure has changed and requires a refresh.
    /// </summary>
    public event EventHandler TreeChanged;

    /// <summary>
    /// Occurs when a log node is added to the tree.
    /// </summary>
    public event Action<PathNode> LogAdded;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackagePathTreeModel"/> class.
    /// </summary>
    public PackagePathTreeModel()
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

    /// <summary>
    /// Adds a log node to the tree with a unique generated ID and raises the <see cref="LogAdded"/> event.
    /// </summary>
    /// <param name="node">The path node to add as a log entry.</param>
    public void AddLogNode(PathNode node)
    {
        long id = Interlocked.Increment(ref _incr);

        node.SetupNodePath($"id-{id}");

        Add(node);

        LogAdded?.Invoke(node);
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
        base.OnNodeChanged(node);

        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnStructureChanged()
    {
        base.OnStructureChanged();

        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnStructureChanged(PathNode node)
    {
        base.OnStructureChanged(node);

        RaiseRefresh();
    }

    /// <inheritdoc/>
    public override void OnNodeInserted(PathNode parent, int index, PathNode node)
    {
        base.OnNodeInserted(parent, index, node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    /// <inheritdoc/>
    public override void OnNodeRemoved(PathNode parent, int index, PathNode node)
    {
        base.OnNodeRemoved(parent, index, node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    /// <summary>
    /// Raises the <see cref="TreeChanged"/> event and queues a visual refresh on the tree data.
    /// </summary>
    private void RaiseRefresh()
    {
        _treeData?.QueueRefresh();
        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    #region IImGuiTreeModel

    /// <summary>
    /// Gets or sets the visual tree data used by the ImGui tree rendering system.
    /// </summary>
    public VisualTreeData<PathNode> TreeData
    {
        get => _treeData;
        set
        {
            _treeData = value;
            _treeData?.Refresh();
        }
    }

    /// <inheritdoc/>
    public IEnumerable<PathNode> GetChildNodes() => RootNode.NodeList;

    /// <inheritdoc/>
    public IEnumerable<PathNode> GetChildNodes(PathNode node) => node.NodeList;

    /// <inheritdoc/>
    public bool GetCanExpand(PathNode node) => node.CanExpand;

    /// <inheritdoc/>
    public int? GetHeight(PathNode node) => _defaultHeight;

    /// <inheritdoc/>
    public string GetId(PathNode node) => node.NodeId;

    /// <inheritdoc/>
    public bool? GetIsExpanded(PathNode node)
    {
        if (node is PopulatePathNode pNode)
        {
            return pNode.Expanded;
        }

        return false;
    }

    /// <inheritdoc/>
    public PathNode GetParent(PathNode node) => node.Parent;

    /// <inheritdoc/>
    public new void SetIsExpanded(PathNode node, bool expand)
    {
        base.SetIsExpanded(node, expand);
    }

    #endregion
}
