using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor.VirtualTree;

public partial class VirtualNode
{
/// <summary>
/// Represents a collection of child nodes belonging to a virtual node.
/// </summary>
public sealed class NodeCollection : IEnumerable<VirtualNode>
{
    private readonly VirtualNode _owner;
    private readonly List<VirtualNode> _list = [];

    /// <summary>
    /// Initializes a new instance for the specified owner node.
    /// </summary>
    /// <param name="owner">The node that owns this collection.</param>
    internal NodeCollection(VirtualNode owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// Removes all nodes from the collection.
    /// </summary>
    public void Clear()
    {
        while (_list.Count != 0)
        {
            RemoveAt(_list.Count - 1);
        }
    }

    /// <summary>
    /// Adds a node to the end of the collection.
    /// </summary>
    /// <param name="item">The node to add.</param>
    public void Add(VirtualNode item)
    {
        Insert(_list.Count, item);
    }

    /// <summary>
    /// Adds a node before another node in the collection.
    /// </summary>
    /// <param name="item">The node to add.</param>
    /// <param name="itemBefore">The node to insert before.</param>
    public void Add(VirtualNode item, VirtualNode itemBefore)
    {
        int index = IndexOf(itemBefore);
        if (index >= 0)
        {
            Insert(index, item);
        }
        else
        {
            Insert(_list.Count, item);
        }
    }

    /// <summary>
    /// Inserts a node at the specified index.
    /// </summary>
    /// <param name="index">The index to insert at.</param>
    /// <param name="item">The node to insert.</param>
    public void Insert(int index, VirtualNode item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item.Parent != _owner)
        {
            item.Parent?.Nodes.Remove(item);
            item._parent = _owner;
            item._index = index;

            for (int i = index; i < _list.Count; i++)
            {
                _list[i]._index++;
            }

            _list.Insert(index, item);
            item._model = _owner.FindModel();
            item.OnAdded();

            VirtualTreeModel model = _owner.FindModel();
            model?.NotifyNodeInserted(_owner, index, item);
        }
    }

    /// <summary>
    /// Removes a node from the collection.
    /// </summary>
    /// <param name="item">The node to remove.</param>
    /// <returns>True if the node was found and removed, false otherwise.</returns>
    public bool Remove(VirtualNode item)
    {
        int index = _list.IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the node at the specified index.
    /// </summary>
    /// <param name="index">The index of the node to remove.</param>
    public void RemoveAt(int index)
    {
        VirtualNode item = _list[index];
        item._parent = null;
        item._index = -1;

        for (int i = index + 1; i < _list.Count; i++)
        {
            _list[i]._index--;
        }

        _list.RemoveAt(index);

        VirtualTreeModel model = _owner.FindModel();
        model?.NotifyNodeRemoved(_owner, index, item);

        item.OnRemoved();
    }

    /// <summary>
    /// Gets or sets the node at the specified index.
    /// </summary>
    /// <param name="index">The index of the node.</param>
    /// <returns>The node at the specified index.</returns>
    public VirtualNode this[int index]
    {
        get
        {
            return _list[index];
        }
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            RemoveAt(index);
            Insert(index, value);
        }
    }

    /// <summary>
    /// Gets the node at the specified index, or null if out of range.
    /// </summary>
    /// <param name="index">The index to retrieve.</param>
    /// <returns>The node at the index, or null if out of range.</returns>
    public VirtualNode GetItemAtSafe(int index)
    {
        return _list.GetListItemSafe(index);
    }

    /// <summary>
    /// Gets the number of nodes in the collection.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the index of the specified node.
    /// </summary>
    /// <param name="node">The node to find.</param>
    /// <returns>The zero-based index, or -1 if not found.</returns>
    public int IndexOf(VirtualNode node)
    {
        return _list.IndexOf(node);
    }

    /// <summary>
    /// Gets all child nodes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes matching the type.</returns>
    public IEnumerable<T> GetChildNodes<T>() where T : VirtualNode
    {
        foreach (var node in _list)
        {
            if (node is T t)
            {
                yield return t;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<VirtualNode> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}
}