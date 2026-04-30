using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// Provides extension methods for path tree node operations, including path setup and drag-and-drop utilities.
/// </summary>
public static class PathTreeExtensions
{
    /// <summary>
    /// Sets the node path for a path node, optionally building from the parent's path.
    /// </summary>
    /// <typeparam name="T">The type of path node.</typeparam>
    /// <param name="node">The node to configure.</param>
    /// <param name="nodePath">The path segment to set.</param>
    /// <returns>The configured node for chaining.</returns>
    public static T WithNodePath<T>(this T node, string nodePath) where T : PathNode
    {
        if (node.Parent != null)
        {
            node.SetupNodePath($"{node.Parent.NodePath}/{nodePath}");
        }
        else
        {
            node.SetupNodePath(nodePath);
        }

        return node;
    }

    /// <summary>
    /// Sets the node path for a path node using an explicit parent node.
    /// </summary>
    /// <typeparam name="T">The type of path node.</typeparam>
    /// <param name="node">The node to configure.</param>
    /// <param name="parent">The parent node to build the path from.</param>
    /// <param name="nodePath">The path segment to append to the parent's path.</param>
    /// <returns>The configured node for chaining.</returns>
    public static T WithNodePath<T>(this T node, PathNode parent, string nodePath) where T : PathNode
    {
        node.SetupNodePath($"{parent.NodePath}/{nodePath}");

        return node;
    }

    /// <summary>
    /// Determines whether all path nodes in the collection share the same parent.
    /// </summary>
    /// <param name="pathNodes">The collection of path nodes to check.</param>
    /// <returns>True if all nodes share the same non-null parent; otherwise, false.</returns>
    public static bool IsParentSame(this IEnumerable<PathNode> pathNodes)
    {
        PathNode parent = null;
        foreach (var pathNode in pathNodes)
        {
            if (pathNode.Parent == null)
            {
                return false;
            }
            if (parent == null)
            {
                parent = pathNode.Parent;
            }
            else
            {
                if (pathNode.Parent != parent)
                {
                    return false;
                }
            }
        }

        return parent != null;
    }

    /// <summary>
    /// Gets the common type of all path nodes being dragged in a drag event.
    /// </summary>
    /// <param name="e">The drag event data.</param>
    /// <returns>The common type of dragged nodes, or null if no path nodes are present.</returns>
    public static Type GetDraggingNodeCommonType(this DragEventData e)
    {
        if (!e.GetDataPresent(typeof(PathNode[])))
        {
            return null;
        }

        var nodes = e.GetData<PathNode[]>();
        return nodes.GetCommonType();
    }

    /// <summary>
    /// Retrieves the dragged nodes cast to the specified type from a drag event.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <param name="e">The drag event.</param>
    /// <returns>An enumerable of dragged nodes of type T.</returns>
    public static IEnumerable<T> GetDraggingNodes<T>(this IDragEvent e) where T : PathNode
    {
        return GetDraggingNodes<T>(e.Data);
    }

    /// <summary>
    /// Retrieves the dragged nodes cast to the specified type from drag event data.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <param name="data">The drag event data.</param>
    /// <returns>An enumerable of dragged nodes of type T.</returns>
    public static IEnumerable<T> GetDraggingNodes<T>(this DragEventData data) where T : PathNode
    {
        if (!data.GetDataPresent(typeof(PathNode[])))
        {
            return [];
        }

        return data.GetData<PathNode[]>().OfType<T>();
    }

    /// <summary>
    /// Retrieves the dragged nodes cast to the specified type from a drag event, with an option to filter by exact type.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <param name="e">The drag event.</param>
    /// <param name="thisTypeOnly">If true, only returns nodes of exactly type T; otherwise includes derived types.</param>
    /// <returns>An enumerable of dragged nodes of type T.</returns>
    public static IEnumerable<T> GetDraggingNodes<T>(this IDragEvent e, bool thisTypeOnly) where T : PathNode
    {
        return GetDraggingNodes<T>(e.Data, thisTypeOnly);
    }

    /// <summary>
    /// Retrieves the dragged nodes cast to the specified type from drag event data, with an option to filter by exact type.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <param name="data">The drag event data.</param>
    /// <param name="thisTypeOnly">If true, only returns nodes of exactly type T; otherwise includes derived types.</param>
    /// <returns>An enumerable of dragged nodes of type T.</returns>
    public static IEnumerable<T> GetDraggingNodes<T>(this DragEventData data, bool thisTypeOnly) where T : PathNode
    {
        if (!data.GetDataPresent(typeof(PathNode[])))
        {
            return [];
        }

        if (thisTypeOnly)
        {
            return data.GetData<PathNode[]>().OfType<T>();
        }
        else
        {
            var selection = data.GetData<PathNode[]>().OfType<T>();
            if (selection.All(o => o is T))
            {
                return selection.OfType<T>();
            }
            else
            {
                return [];
            }
        }
    }
}
