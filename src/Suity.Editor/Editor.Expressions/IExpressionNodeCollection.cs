using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// Represents a collection of expression nodes that can be written and accessed.
/// </summary>
public interface IExpressionNodeCollection
{
    /// <summary>
    /// Gets an enumerable of all writable expression elements.
    /// </summary>
    IEnumerable<IExpressionWritable> Writables { get; }

    /// <summary>
    /// Gets an enumerable of all expression writable nodes.
    /// </summary>
    IEnumerable<IExpressionWritableNode> Nodes { get; }

    /// <summary>
    /// Gets the node associated with the specified writable expression.
    /// </summary>
    /// <param name="writables">The writable expression to look up.</param>
    /// <returns>The associated node, or null if not found.</returns>
    IExpressionWritableNode GetNode(IExpressionWritable writables);
}