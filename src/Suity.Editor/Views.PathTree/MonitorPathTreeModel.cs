using Suity.Editor.View.ViewModel;
using Suity.Views.Im;
using Suity.Views.PathTree;
using System;
using System.Threading;

namespace Suity.Editor.View;

/// <summary>
/// A path tree model designed for monitoring scenarios, supporting incremental node additions and automatic refresh notifications.
/// </summary>
public class MonitorPathTreeModel : PathTreeModel
{
    /// <summary>
    /// Maximum number of entries allowed in the monitor.
    /// </summary>
    public const int MaxEntry = 10000;

    /// <summary>
    /// The visual tree data backing the path tree.
    /// </summary>
    protected VisualTreeData<PathNode> _treeData;

    /// <summary>
    /// The default row height for nodes in the tree.
    /// </summary>
    protected int _defaultHeight = LogNode.DefaultRowHeight;

    private long _incr;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorPathTreeModel"/> class.
    /// </summary>
    public MonitorPathTreeModel()
    {
    }


    /// <summary>
    /// Gets or sets the default row height for nodes in the tree.
    /// </summary>
    public int DefaultHeight
    {
        get => _defaultHeight;
        set => _defaultHeight = value;
    }

    /// <summary>
    /// Gets or sets a predicate used to filter log nodes.
    /// </summary>
    public Predicate<LogNode> Filter { get; set; }

    /// <summary>
    /// Adds a monitor node to the tree with an auto-generated unique path.
    /// </summary>
    /// <param name="node">The log node to add.</param>
    public void AddMonitorNode(LogNode node)
    {
        long id = Interlocked.Increment(ref _incr);

        node.SetupNodePath($"id-{id}");

        Add(node);
    }

    /// <summary>
    /// Triggers a refresh of the tree data.
    /// </summary>
    public virtual void RaiseRefresh()
    {
        _treeData?.QueueRefresh();
    }

    #region Override

    /// <inheritdoc />
    protected override void OnSuspendLayout()
    {
        base.OnSuspendLayout();
    }

    /// <inheritdoc />
    protected override void OnResumeLayout()
    {
        base.OnResumeLayout();

        RaiseRefresh();
    }

    /// <inheritdoc />
    public override void OnNodeChanged(PathNode node)
    {
        base.OnNodeChanged(node);

        //TODO: Should single node change trigger global refresh? (when height is unchanged)
        RaiseRefresh();
    }

    /// <inheritdoc />
    public override void OnStructureChanged()
    {
        base.OnStructureChanged();

        RaiseRefresh();
    }

    /// <inheritdoc />
    public override void OnStructureChanged(PathNode node)
    {
        base.OnStructureChanged(node);

        RaiseRefresh();
    }

    /// <inheritdoc />
    public override void OnNodeInserted(PathNode parent, int index, PathNode node)
    {
        base.OnNodeInserted(parent, index, node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    /// <inheritdoc />
    public override void OnNodeRemoved(PathNode parent, int index, PathNode node)
    {
        base.OnNodeRemoved(parent, index, node);

        if (!Suspended)
        {
            RaiseRefresh();
        }
    }

    #endregion

    
}
