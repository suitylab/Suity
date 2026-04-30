using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Views.PathTree;

/// <summary>
/// Manages the expand/collapse state of path nodes in a tree, supporting backup and restore operations.
/// </summary>
public class PathNodeExpandState
{
    private readonly HashSet<string> _expandStates = [];

    /// <summary>
    /// Backs up the current expand state of all populate path nodes in the specified tree model.
    /// </summary>
    /// <param name="model">The path tree model to backup from.</param>
    public void Backup(PathTreeModel model)
    {
        _expandStates.Clear();

        Visit(model.RootNode, n =>
        {
            if (n is PopulatePathNode p && p.Expanded)
            {
                _expandStates.Add(p.NodeId);
            }
        });
    }

    /// <summary>
    /// Restores the expand state of all populate path nodes in the specified tree model from the backed-up state.
    /// </summary>
    /// <param name="model">The path tree model to restore to.</param>
    public void Restore(PathTreeModel model)
    {
        Visit(model.RootNode, n =>
        {
            if (n is PopulatePathNode p && _expandStates.Contains(n.NodeId))
            {
                p.Expanded = true;
            }
        });
    }

    /// <summary>
    /// Gets the collection of expanded node identifiers.
    /// </summary>
    /// <returns>An enumerable of expanded node path strings.</returns>
    public IEnumerable<string> GetExpandedPaths() => _expandStates;

    /// <summary>
    /// Sets the expanded node identifiers from a collection of path strings.
    /// </summary>
    /// <param name="strs">The collection of node paths to mark as expanded.</param>
    public void SetExpandedPaths(IEnumerable<string> strs)
    {
        _expandStates.Clear();
        _expandStates.AddRange(strs);
    }

    private void Visit(PathNode node, Action<PathNode> action)
    {
        action(node);
        foreach (var childNode in node.NodeList)
        {
            Visit(childNode, action);
        }
    }
}
