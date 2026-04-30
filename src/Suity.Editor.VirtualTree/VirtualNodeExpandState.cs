using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Manages backup and restoration of virtual node expansion states.
/// </summary>
public class VirtualNodeExpandState
{
    private readonly HashSet<string> _expandStates = [];

    /// <summary>
    /// Backs up the current expansion state of all nodes in the model.
    /// </summary>
    /// <param name="model">The tree model to back up.</param>
    public void Backup(VirtualTreeModel model)
    {
        _expandStates.Clear();

        Visit(model.RootNode, n =>
        {
            if (n.IsExpanded())
            {
                _expandStates.Add(n.GetFullId());
            }
        });
    }

    /// <summary>
    /// Restores the expansion state to nodes in the model.
    /// </summary>
    /// <param name="model">The tree model to restore.</param>
    public void Restore(VirtualTreeModel model)
    {
        Visit(model.RootNode, n =>
        {
            if (_expandStates.Contains(n.GetFullId()))
            {
                n.Expand();
            }
        });
    }

    /// <summary>
    /// Gets the collection of expanded node paths.
    /// </summary>
    /// <returns>An enumerable of full node IDs that are expanded.</returns>
    public IEnumerable<string> GetExpandedPaths()
    {
        return _expandStates;
    }

    /// <summary>
    /// Sets the expanded paths from a collection of node IDs.
    /// </summary>
    /// <param name="strs">The collection of full node IDs to mark as expanded.</param>
    public void SetExpandedPaths(IEnumerable<string> strs)
    {
        _expandStates.Clear();
        _expandStates.AddRange(strs);
    }

    /// <summary>
    /// Recursively visits a node and all its descendants.
    /// </summary>
    /// <param name="node">The root node to start visiting from.</param>
    /// <param name="action">The action to perform on each node.</param>
    private void Visit(VirtualNode node, Action<VirtualNode> action)
    {
        action(node);
        foreach (var childNode in node.Nodes)
        {
            Visit(childNode, action);
        }
    }
}