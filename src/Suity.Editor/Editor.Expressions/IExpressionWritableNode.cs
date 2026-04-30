using System;

namespace Suity.Editor.Expressions;

/// <summary>
/// Represents a writable node in the expression tree with parent-child relationships.
/// </summary>
public interface IExpressionWritableNode
{
    /// <summary>
    /// Gets the writable expression value associated with this node.
    /// </summary>
    IExpressionWritable Value { get; }

    /// <summary>
    /// Gets the parent node, or null if this is a root node.
    /// </summary>
    IExpressionWritableNode Parent { get; }

    /// <summary>
    /// Gets the child nodes of this node.
    /// </summary>
    IExpressionWritableNode[] ChildNodes { get; }

    /// <summary>
    /// Finds the first ancestor of the specified type in the parent chain.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <returns>The first ancestor of the specified type, or default if not found.</returns>
    T FindParent<T>();

    /// <summary>
    /// Finds the first child of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of child to find.</typeparam>
    /// <returns>The first child of the specified type, or default if not found.</returns>
    T FindChild<T>();

    /// <summary>
    /// Finds all children of the specified type and invokes the callback for each.
    /// </summary>
    /// <typeparam name="T">The type of children to find.</typeparam>
    /// <param name="found">The callback to invoke for each matching child.</param>
    void FindChildMany<T>(Action<T> found);
}