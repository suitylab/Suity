using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Views.PathTree;

/// <summary>
/// A collection of path nodes that manages child nodes with support for insertion, removal, and layout suspension.
/// </summary>
public sealed class PathNodeCollectionBK : PathNodeCollection
{
    private readonly PathNode _node;
    private readonly List<PathNode> _list = [];
    private int _suspend;

    /// <summary>
    /// Initializes a new instance for the specified parent node.
    /// </summary>
    /// <param name="node">The parent path node.</param>
    internal PathNodeCollectionBK(PathNode node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        while (_list.Count > 0)
        {
            RemoveAt(_list.Count - 1);
        }
    }

    /// <inheritdoc/>
    public override void Add(PathNode item)
    {
        Insert(_list.Count, item);
    }

    /// <inheritdoc/>
    public override void Insert(int index, PathNode item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item.Parent != _node)
        {
            if (item.Parent != null)
            {
                var parent = item.Parent;
                item.Parent.NodeList.Remove(item);

                item._nodeList?.Clear();
                item.OnRemoved(parent);
            }

            item._parent = _node;
            item._index = index;

            for (int i = index; i < _list.Count; i++)
            {
                _list[i]._index++;
            }

            _list.Insert(index, item);

            IPathTreeModel model = _node.FindModel();
            model?.OnNodeInserted(_node, index, item);
        }

        item.OnAdded();
    }

    /// <inheritdoc/>
    public override bool Remove(PathNode item)
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

    /// <inheritdoc/>
    public override void RemoveAll(Predicate<PathNode> predicate)
    {
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            if (predicate(_list[i]))
            {
                RemoveAt(i);
            }
        }
    }

    /// <inheritdoc/>
    public override void RemoveAt(int index)
    {
        PathNode item = _list[index];
        if (item is null)
        {
            return;
        }

        item._parent = null;
        item._index = -1;

        for (int i = index + 1; i < _list.Count; i++)
        {
            _list[i]._index--;
        }

        _list.RemoveAt(index);

        IPathTreeModel model = _node.FindModel();
        model?.OnNodeRemoved(_node, index, item);

        item._nodeList?.Clear();
        item.OnRemoved(_node);
    }

    /// <inheritdoc/>
    public override bool RemoveRange(int index, int count)
    {
        if (index < 0 || count < 0)
        {
            return false;
        }

        if (index + count > _list.Count)
        {
            return false;
        }

        IPathTreeModel model = _node.FindModel();

        for (int i = 0; i < count; i++)
        {
            PathNode item = _list[index + i];
            if (item is null)
            {
                continue;
            }

            item._parent = null;
            item._index = -1;

            item._nodeList?.Clear();
            item.OnRemoved(_node);

            model?.OnNodeRemoved(_node, index + i, item);
        }

        _list.RemoveRange(index, count);

        for (int i = 0; i < _list.Count; i++)
        {
            _list[i]._index = i;
        }

        return true;
    }

    /// <inheritdoc/>
    public override void Sort()
    {
        _list.Sort((a, b) => a.NodePath.CompareTo(b.NodePath));

        for (int i = 0; i < _list.Count; i++)
        {
            _list[i]._index = i;
        }

        _node.FindModel()?.OnStructureChanged(_node);
    }

    /// <inheritdoc/>
    public override PathNode this[int index]
    {
        get
        {
            return _list[index];
        }
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException("item");
            }

            if (object.Equals(_list[index], value))
            {
                return;
            }

            RemoveAt(index);
            Insert(index, value);
        }
    }

    /// <inheritdoc/>
    public override int Count => _list.Count;

    /// <inheritdoc/>
    public override int IndexOf(PathNode node)
    {
        return _list.IndexOf(node);
    }

    /// <inheritdoc/>
    public override IEnumerable<PathNode> Nodes => _list.Pass();

    /// <inheritdoc/>
    public override IEnumerable<T> GetChildNodes<T>()
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
    internal override void SuspendLayout() => _suspend++;

    /// <inheritdoc/>
    internal override void ResumeLayout()
    {
        if (_suspend == 0)
        {
            return;
        }

        _suspend--;

        if (_suspend == 0)
        {
            _node.FindModel()?.OnStructureChanged(_node);
        }
    }

    /// <inheritdoc/>
    public override bool Suspended => _suspend > 0;
}
