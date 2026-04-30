using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Views.PathTree;

/// <summary>
/// An abstract collection of path nodes that supports enumeration and common collection operations.
/// </summary>
public abstract class PathNodeCollection : IEnumerable<PathNode>
{
    /// <summary>
    /// Factory function used to create a path node collection for a given node.
    /// </summary>
    internal static Func<PathNode, PathNodeCollection> _factory;

    /// <summary>
    /// Removes all nodes from the collection.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Adds a node to the end of the collection.
    /// </summary>
    /// <param name="item">The node to add.</param>
    public abstract void Add(PathNode item);

    /// <summary>
    /// Inserts a node at the specified index in the collection.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The node to insert.</param>
    public abstract void Insert(int index, PathNode item);

    /// <summary>
    /// Removes the first occurrence of a specific node from the collection.
    /// </summary>
    /// <param name="item">The node to remove.</param>
    /// <returns>True if the node was successfully removed; otherwise, false.</returns>
    public abstract bool Remove(PathNode item);

    /// <summary>
    /// Removes all nodes that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to test nodes against.</param>
    public abstract void RemoveAll(Predicate<PathNode> predicate);

    /// <summary>
    /// Removes the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the node to remove.</param>
    public abstract void RemoveAt(int index);

    /// <summary>
    /// Removes a range of nodes starting at the specified index.
    /// </summary>
    /// <param name="index">The zero-based starting index.</param>
    /// <param name="count">The number of nodes to remove.</param>
    /// <returns>True if the range was successfully removed; otherwise, false.</returns>
    public abstract bool RemoveRange(int index, int count);

    /// <summary>
    /// Sorts the nodes in the collection using the default comparer.
    /// </summary>
    public abstract void Sort();

    /// <summary>
    /// Gets or sets the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the node.</param>
    public abstract PathNode this[int index] { get; set; }

    /// <summary>
    /// Gets the number of nodes in the collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Returns the index of the specified node in the collection.
    /// </summary>
    /// <param name="node">The node to find.</param>
    /// <returns>The zero-based index of the node, or -1 if not found.</returns>
    public abstract int IndexOf(PathNode node);

    /// <summary>
    /// Gets an enumerable of all nodes in the collection.
    /// </summary>
    public abstract IEnumerable<PathNode> Nodes { get; }

    /// <summary>
    /// Gets all child nodes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes of type T.</returns>
    public abstract IEnumerable<T> GetChildNodes<T>() where T : PathNode;

    /// <summary>
    /// Suspends layout updates for the collection to batch multiple changes.
    /// </summary>
    internal abstract void SuspendLayout();

    /// <summary>
    /// Resumes layout updates after suspension and applies pending changes.
    /// </summary>
    internal abstract void ResumeLayout();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<PathNode> GetEnumerator() => Nodes.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Nodes.GetEnumerator();

    /// <summary>
    /// Gets a value indicating whether layout updates are currently suspended.
    /// </summary>
    public abstract bool Suspended { get; }
}
