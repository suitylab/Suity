using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents drag-and-drop data for a virtual tree view.
/// </summary>
/// <typeparam name="T">The type of data represented by each tree node.</typeparam>
public class VirtualTreeDragData<T> : IDragData
    where T : class
{
    /// <summary>
    /// The collection of visual tree nodes being dragged.
    /// </summary>
    protected readonly VisualTreeNode<T>[] _nodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTreeDragData{T}"/> class.
    /// </summary>
    /// <param name="nodes">The visual tree nodes being dragged.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodes"/> is null.</exception>
    public VirtualTreeDragData(IEnumerable<VisualTreeNode<T>> nodes)
    {
        _nodes = nodes?.ToArray() ?? throw new ArgumentNullException(nameof(nodes));
    }

    /// <summary>
    /// Retrieves the dragged data in the specified format.
    /// </summary>
    /// <param name="format">The requested data type.</param>
    /// <returns>The dragged data in the requested format, or null if the format is not supported.</returns>
    public virtual object? GetData(Type format)
    {
        if (format == typeof(VisualTreeNode[]))
        {
            return _nodes;
        }
        if (format == typeof(T[]))
        {
            return _nodes.Select(o => o.Value).OfType<T>().ToArray();
        }

        return null;
    }

    /// <summary>
    /// Determines whether data is available in the specified format.
    /// </summary>
    /// <param name="format">The data type to check.</param>
    /// <returns>True if data is available in the specified format; otherwise, false.</returns>
    public virtual bool GetDataPresent(Type format)
    {
        if (format == typeof(VisualTreeNode[]))
        {
            return true;
        }
        if (format == typeof(T[]))
        {
            return true;
        }

        return false;
    }
}

/// <summary>
/// Represents drag-and-drop data specifically for path tree views.
/// </summary>
public class PathTreeDragData : VirtualTreeDragData<PathNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathTreeDragData"/> class.
    /// </summary>
    /// <param name="nodes">The visual tree nodes being dragged.</param>
    public PathTreeDragData(IEnumerable<VisualTreeNode<PathNode>> nodes) : base(nodes)
    {
    }

    /// <inheritdoc/>
    public override object? GetData(Type format)
    {
        var obj = base.GetData(format);
        if (obj is { })
        {
            return obj;
        }

        if (_nodes.Length == 1 && _nodes[0]?.Value?.DisplayedValue is { } value && format.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        return null;
    }

    /// <inheritdoc/>
    public override bool GetDataPresent(Type format)
    {
        bool present = base.GetDataPresent(format);
        if (present)
        {
            return true;
        }

        if (_nodes.Length == 1 && _nodes[0]?.Value?.DisplayedValue is { } value && format.IsAssignableFrom(value.GetType()))
        {
            return true;
        }

        return false;
    }
}