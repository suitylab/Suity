using Suity.Editor.VirtualTree;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents drag data for virtual tree nodes, extending base drag data with support for single node value retrieval.
/// </summary>
public class VirtualTreeDragData(IEnumerable<VisualTreeNode<VirtualNode>> nodes) : VirtualTreeDragData<VirtualNode>(nodes)
{
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