using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents a node in the RexTree hierarchy. Each node can contain data, child nodes, computed data, and listeners.
/// </summary>
public class RexNode
{
    /// <summary>
    /// Gets a value indicating whether to set the child nodes dictionary to null when empty.
    /// </summary>
    public static readonly bool SetNullWhenEmtpy = false;

    private readonly PathItem _localPath;
    internal RexPath _path;

    internal RexTree _model;
    internal RexNode _parent;
    internal RexNodeListenerSet _listener;

    private Dictionary<PathItem, RexNode> _childNodes;

    private object _data;
    private ComputedData _computed;
    private int _counter;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNode"/> class.
    /// </summary>
    /// <param name="pathNode">The local path item for this node.</param>
    internal RexNode(PathItem pathNode)
    {
        _localPath = pathNode;
    }

    /// <summary>
    /// Gets the parent node of this node. Returns null for the root node.
    /// </summary>
    public RexNode Parent => _parent;

    /// <summary>
    /// Gets the local path item for this node.
    /// </summary>
    public PathItem LocalPath => _localPath;

    /// <summary>
    /// Gets the full path of this node within the RexTree.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Gets the collection of child nodes.
    /// </summary>
    public IEnumerable<RexNode> ChildNodes => _childNodes != null ? _childNodes.Values.Pass() : [];

    /// <summary>
    /// Gets a value indicating whether this node has no child nodes.
    /// </summary>
    public bool IsTerminal => _childNodes is null || _childNodes.Count == 0;

    /// <summary>
    /// Gets a value indicating whether this node contains action data.
    /// </summary>
    public bool IsAction
    {
        get
        {
            var obj = GetData();

            return obj is Action<ActionArguments> || obj is Action;
        }
    }

    /// <summary>
    /// Gets the data stored in this node. If computed data is set, returns the computed value.
    /// </summary>
    /// <returns>The data stored in this node, or null if no data is set.</returns>
    public object GetData()
    {
        if (_computed != null)
        {
            return _computed.GetData();
        }

        return _data;
    }

    /// <summary>
    /// Gets a value indicating whether this node has computed data.
    /// </summary>
    public bool IsComputedData => _computed != null;

    /// <summary>
    /// Gets the counter that increments each time the node's data or actions change.
    /// </summary>
    public int Counter => _counter;

    /// <summary>
    /// Gets the number of listeners attached to this node.
    /// </summary>
    public int ListenerCount => _listener?.ListenerCount ?? 0;

    /// <summary>
    /// Adds a child node to this node. If the child already has a parent, it is removed from the old parent first.
    /// </summary>
    /// <param name="node">The child node to add.</param>
    internal void AddChild(RexNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (node == this)
        {
            throw new InvalidOperationException();
        }

        node._parent?.RemoveChild(node);

        _childNodes ??= [];
        if (_childNodes.TryGetValue(node.LocalPath, out RexNode current))
        {
            current._parent = null;
        }

        _childNodes[node.LocalPath] = node;
        node._model = _model;
        node._parent = this;
        node._path = _path.Append(node.LocalPath);
        node._listener = _model.GetListener(node._path);
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="node">The child node to remove.</param>
    /// <returns>True if the node was successfully removed; otherwise, false.</returns>
    internal bool RemoveChild(RexNode node)
    {
        if (node is null)
        {
            return false;
        }

        if (node._parent != this)
        {
            return false;
        }

        if (_childNodes is null)
        {
            return false;
        }

        if (_childNodes.TryGetValue(node.LocalPath, out RexNode current) && current == node)
        {
            _childNodes.Remove(node.LocalPath);
            node._model = null;
            node._parent = null;
            node._path = null;
            node._listener = null;

            if (SetNullWhenEmtpy)
            {
                if (_childNodes.Count == 0)
                {
                    _childNodes = null;
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures a child node exists for the specified path item, creating it if necessary.
    /// </summary>
    /// <param name="childPath">The path item for the child node.</param>
    /// <returns>The existing or newly created child node.</returns>
    internal RexNode EnsureNode(PathItem childPath)
    {
        _childNodes ??= [];

        if (_childNodes.TryGetValue(childPath, out var node))
        {
            return node;
        }
        else
        {
            node = new RexNode(childPath);
            AddChild(node);

            return node;
        }
    }

    /// <summary>
    /// Gets a child node for the specified path item, if it exists.
    /// </summary>
    /// <param name="childPath">The path item for the child node.</param>
    /// <returns>The child node, or null if not found.</returns>
    internal RexNode GetNode(PathItem childPath)
    {
        if (_childNodes != null && _childNodes.TryGetValue(childPath, out RexNode node))
        {
            return node;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the data for this node and dispatches it to listeners.
    /// If computed data is set, delegates to the computed data setter.
    /// </summary>
    /// <param name="data">The data to set.</param>
    internal void SetData(object data)
    {
        if (_computed != null)
        {
            _computed.SetData(data);

            return;
        }

        _data = data;

        _listener?.DispatchData(data);

        _counter++;
    }

    /// <summary>
    /// Sets the data deeply for this node, clearing all child nodes and recursively setting data for properties and array items.
    /// </summary>
    /// <param name="data">The data to set deeply.</param>
    internal void SetDataDeep(object data)
    {
        if (_computed != null)
        {
            _computed.SetData(data);

            return;
        }

        Clear();

        _data = data;

        _listener?.DispatchData(data);

        _counter++;

        // Recursively set data for object properties
        var propNames = RexGlobalResolve.Current?.GetPropertyNames(data) ?? [];
        foreach (var name in propNames)
        {
            var value = RexGlobalResolve.Current?.GetProperty(data, name);
            EnsureNode(new PathItem(name, -1)).SetDataDeep(value);
        }

        // Handle arrays
        if (data is Array ary)
        {
            for (int i = 0; i < ary.Length; i++)
            {
                EnsureNode(new PathItem(null, i)).SetDataDeep(ary.GetValue(i));
            }

            return;
        }

        // Handle lists
        if (data is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                EnsureNode(new PathItem(null, i)).SetDataDeep(list[i]);
            }
        }
    }

    /// <summary>
    /// Triggers an update notification by dispatching the current data to listeners.
    /// </summary>
    internal void UpdateData()
    {
        _listener?.DispatchData(GetData());
    }

    /// <summary>
    /// Sets the default data for this node if no data is currently set.
    /// </summary>
    /// <param name="data">The default data to set.</param>
    internal void SetDefaultData(object data)
    {
        if (_computed != null)
        {
            return;
        }

        if (_data != null)
        {
            return;
        }

        _data = data;

        _listener?.DispatchData(data);

        _counter++;
    }

    /// <summary>
    /// Sets or clears the computed data for this node.
    /// </summary>
    /// <param name="computed">The computed data to set, or null to clear.</param>
    /// <returns>A disposable that clears the computed data when disposed.</returns>
    internal IDisposable SetComputed(ComputedData computed)
    {
        _computed = computed;
        if (computed != null)
        {
            _data = null;
        }
        _counter++;

        return new RexDisposableAction(() => _computed = null);
    }

    /// <summary>
    /// Dispatches an action to the listeners attached to this node.
    /// </summary>
    /// <param name="argument">The action arguments to dispatch.</param>
    /// <returns>True if any listener handled the action; otherwise, false.</returns>
    internal bool DoAction(ActionArguments argument)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        _counter++;

        return _listener?.DispatchAction(argument) ?? false;
    }

    /// <summary>
    /// Recursively removes all listeners with the specified tag from this node and its children.
    /// </summary>
    /// <param name="tag">The tag to match.</param>
    /// <param name="count">Reference to the counter to increment for each removed listener.</param>
    internal void RemoveListenersByTagDeep(string tag, ref int count)
    {
        _listener?.RemoveByTag(tag, ref count);

        if (_childNodes != null)
        {
            foreach (var node in _childNodes.Values)
            {
                node.RemoveListenersByTagDeep(tag, ref count);
            }
        }
    }

    /// <summary>
    /// Clears all data, computed data, and child nodes from this node.
    /// </summary>
    internal void Clear()
    {
        _data = null;
        _computed = null;

        _childNodes?.Clear();
    }

    /// <summary>
    /// Gets a brief string representation of the data stored in this node.
    /// </summary>
    /// <returns>A string describing the node's data.</returns>
    public string GetBreifString()
    {
        var data = GetData();
        string str;

        if (data is null)
        {
            if (_childNodes?.Count > 0)
            {
                str = string.Empty;
            }
            else
            {
                str = "null";
            }
        }
        else if (data is string)
        {
            str = string.Format("\"{0}\"", data);
        }
        else if (data is Action<ActionArguments> || data is Action)
        {
            str = "[Action]";
        }
        else if (data is ICollection collection)
        {
            str = string.Format("[{0} items]", collection.Count);
        }
        else
        {
            str = data.ToString();
        }

        if (IsComputedData)
        {
            str = "(Computed) " + str;
        }

        return str;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_parent != null)
        {
            return $"{_parent}.{_localPath}";
        }
        else
        {
            return _localPath.ToString();
        }
    }
}
