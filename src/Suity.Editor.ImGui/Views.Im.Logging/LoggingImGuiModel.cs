using Suity.Editor.View;
using Suity.Editor.View.ViewModel;
using Suity.Views.Im.TreeEditing;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.Logging;

/// <summary>
/// Provides a tree model for logging views with ImGui support.
/// </summary>
public class LoggingImGuiModel : MonitorPathTreeModel, IImGuiTreeModel<PathNode>
{
    /// <inheritdoc/>
    public override void RaiseRefresh()
    {
        base.RaiseRefresh();

        TreeChanged?.Invoke(this, EventArgs.Empty);
    }

    #region IImGuiTreeModel

    /// <summary>
    /// Occurs when the tree structure changes and needs to be refreshed.
    /// </summary>
    public event EventHandler? TreeChanged;

    /// <summary>
    /// Gets or sets the visual tree data for the path nodes.
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

    /// <summary>
    /// Gets the child nodes of the root node, optionally filtered by the current filter predicate.
    /// </summary>
    /// <returns>A collection of path nodes that are children of the root node.</returns>
    public IEnumerable<PathNode> GetChildNodes()
    {
        var filter = Filter;
        if (filter != null)
        {
            return RootNode.NodeList
                .OfType<LogNode>()
                .Where(o => filter(o));
        }
        else
        {
            return RootNode.NodeList;
        }
    }

    /// <summary>
    /// Gets the child nodes of the specified node.
    /// </summary>
    /// <param name="node">The parent node whose children to retrieve.</param>
    /// <returns>A collection of child nodes.</returns>
    public IEnumerable<PathNode> GetChildNodes(PathNode node) => node.NodeList;

    /// <summary>
    /// Determines whether the specified node can be expanded.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns><c>true</c> if the node can be expanded; otherwise, <c>false</c>.</returns>
    public bool GetCanExpand(PathNode node) => node.CanExpand;

    /// <summary>
    /// Gets the height of the specified node, or the default height if not specified.
    /// </summary>
    /// <param name="node">The node to get the height for.</param>
    /// <returns>The height of the node, or <c>null</c> to use the default height.</returns>
    public int? GetHeight(PathNode node) => (node as LogNode)?.Height ?? _defaultHeight;

    /// <summary>
    /// Gets the unique identifier for the specified node.
    /// </summary>
    /// <param name="node">The node to get the ID for.</param>
    /// <returns>The unique node identifier.</returns>
    public string GetId(PathNode node) => node.NodeId;

    /// <summary>
    /// Gets whether the specified node is currently expanded.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns><c>true</c> if the node is expanded; <c>false</c> if collapsed or not expandable.</returns>
    public bool? GetIsExpanded(PathNode node)
    {
        if (node is PopulatePathNode pNode)
        {
            return pNode.Expanded;
        }

        return false;
    }

    /// <summary>
    /// Gets the parent of the specified node.
    /// </summary>
    /// <param name="node">The node whose parent to retrieve.</param>
    /// <returns>The parent node, or <c>null</c> if the node is a root node.</returns>
    public PathNode GetParent(PathNode node) => node.Parent;

    /// <summary>
    /// Sets whether the specified node is expanded or collapsed.
    /// </summary>
    /// <param name="node">The node to expand or collapse.</param>
    /// <param name="expand"><c>true</c> to expand the node; <c>false</c> to collapse it.</param>
    public new void SetIsExpanded(PathNode node, bool expand)
    {
        base.SetIsExpanded(node, expand);
    }

    #endregion
}
