using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Manages a collection of listeners for a specific node path in the RexTree.
/// Handles data listeners, action listeners, before/after listeners, and path mappings.
/// </summary>
internal class RexNodeListenerSet
{
    /// <summary>
    /// Gets a value indicating whether to set the listener dictionary to null when empty.
    /// </summary>
    public static readonly bool SetNullWhenEmtpy = false;

    internal readonly RexTree _model;
    internal readonly RexPath _path;

    internal Dictionary<Delegate, RexNodeListener> _listeners;
    internal Dictionary<Delegate, RexNodeListener> _beforeListeners;
    internal Dictionary<Delegate, RexNodeListener> _afterListeners;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeListenerSet"/> class.
    /// </summary>
    /// <param name="model">The RexTree this listener set belongs to.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexNodeListenerSet(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Gets the total count of all listeners (regular, before, and after).
    /// </summary>
    public int ListenerCount
    {
        get
        {
            int count = _listeners?.Count ?? 0;
            count += _beforeListeners?.Count ?? 0;
            count += _afterListeners?.Count ?? 0;

            return count;
        }
    }

    /// <summary>
    /// Adds a data listener that will be invoked when data changes at this path.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="action">The callback to invoke with the new data.</param>
    /// <returns>The created data listener.</returns>
    internal RexNodeDataListener<T> AddDataListener<T>(Action<T> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeDataListener<T>(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an action listener without arguments.
    /// </summary>
    /// <param name="action">The callback to invoke when the action is dispatched.</param>
    /// <returns>The created action listener.</returns>
    internal RexNodeListener AddActionListener(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeActionListener(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with a single argument.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The callback to invoke when the action is dispatched.</param>
    /// <returns>The created action listener.</returns>
    internal RexNodeListener AddActionListener<T>(Action<T> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeActionListener<T>(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with two arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="action">The callback to invoke when the action is dispatched.</param>
    /// <returns>The created action listener.</returns>
    internal RexNodeListener AddActionListener<T1, T2>(Action<T1, T2> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeActionListener<T1, T2>(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with three arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="action">The callback to invoke when the action is dispatched.</param>
    /// <returns>The created action listener.</returns>
    internal RexNodeListener AddActionListener<T1, T2, T3>(Action<T1, T2, T3> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeActionListener<T1, T2, T3>(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with four arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="action">The callback to invoke when the action is dispatched.</param>
    /// <returns>The created action listener.</returns>
    internal RexNodeListener AddActionListener<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var listener = new RexNodeActionListener<T1, T2, T3, T4>(this, action);
        EnsureListenerDic()[action] = listener;

        return listener;
    }

    /// <summary>
    /// Removes a listener by its delegate key.
    /// </summary>
    /// <param name="action">The delegate key of the listener to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    internal bool RemoveListener(Delegate action)
    {
        if (action is null)
        {
            return false;
        }

        if (_listeners is null)
        {
            return false;
        }

        bool removed = _listeners.Remove(action);
        if (SetNullWhenEmtpy)
        {
            if (_listeners.Count == 0)
            {
                _listeners = null;
            }
        }

        return removed;
    }

    /// <summary>
    /// Removes a listener from all listener collections (regular, before, and after).
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    internal void RemoveListener(RexNodeListener listener)
    {
        RemoveListener(listener.GetKey());
        RemoveBeforeListener(listener.GetKey());
        RemoveAfterListener(listener.GetKey());
    }

    /// <summary>
    /// Adds a before-listener that will be invoked before regular listeners.
    /// Can be cancelled by throwing a <see cref="RexCancelException"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="action">The callback to invoke with the data.</param>
    /// <returns>The created data listener.</returns>
    internal RexNodeDataListener<T> AddBeforeListener<T>(Action<T> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _beforeListeners ??= [];

        var listener = new RexNodeDataListener<T>(this, action);
        _beforeListeners[action] = listener;

        return listener;
    }

    /// <summary>
    /// Adds an after-listener that will be invoked after regular listeners.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="action">The callback to invoke with the data.</param>
    /// <returns>The created data listener.</returns>
    internal RexNodeDataListener<T> AddAfterListener<T>(Action<T> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _afterListeners ??= [];

        var listener = new RexNodeDataListener<T>(this, action);
        _afterListeners[action] = listener;

        return listener;
    }

    /// <summary>
    /// Removes a before-listener by its delegate key.
    /// </summary>
    /// <param name="action">The delegate key of the listener to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    internal bool RemoveBeforeListener(Delegate action)
    {
        if (action is null)
        {
            return false;
        }

        if (_beforeListeners is null)
        {
            return false;
        }

        bool removed = _beforeListeners.Remove(action);
        if (SetNullWhenEmtpy)
        {
            if (_beforeListeners.Count == 0)
            {
                _beforeListeners = null;
            }
        }

        return removed;
    }

    /// <summary>
    /// Removes an after-listener by its delegate key.
    /// </summary>
    /// <param name="action">The delegate key of the listener to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    internal bool RemoveAfterListener(Delegate action)
    {
        if (action is null)
        {
            return false;
        }

        if (_afterListeners is null)
        {
            return false;
        }

        bool removed = _afterListeners.Remove(action);
        if (SetNullWhenEmtpy)
        {
            if (_afterListeners.Count == 0)
            {
                _afterListeners = null;
            }
        }

        return removed;
    }

    /// <summary>
    /// Adds a mapping that will trigger updates on the target path when this path changes.
    /// </summary>
    /// <param name="pathTo">The target path to update.</param>
    /// <returns>The created mapping listener.</returns>
    internal RexNodeListener AddMapping(RexPath pathTo)
    {
        if (pathTo is null)
        {
            throw new ArgumentNullException(nameof(pathTo));
        }

        if (_model is null)
        {
            throw new NullReferenceException("Model");
        }

        if (_listeners != null)
        {
            RexNodeMappingListener current = _listeners.Values.OfType<RexNodeMappingListener>().FirstOrDefault(o => o.PathTo == pathTo);
            if (current != null)
            {
                return current;
            }
        }
        else
        {
            _listeners = [];
        }

        var listener = new RexNodeMappingListener(this, pathTo);
        _listeners.Add(listener.GetKey(), listener);

        return listener;
    }

    /// <summary>
    /// Removes a mapping to the specified target path.
    /// </summary>
    /// <param name="pathTo">The target path of the mapping to remove.</param>
    /// <returns>True if the mapping was found and removed; otherwise, false.</returns>
    internal bool RemoveMapping(RexPath pathTo)
    {
        if (_listeners != null)
        {
            RexNodeMappingListener current = _listeners.Values.OfType<RexNodeMappingListener>().FirstOrDefault(o => o.PathTo == pathTo);
            if (current != null)
            {
                _listeners.Remove(current.GetKey());

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes all listeners with the specified tag and increments the count.
    /// </summary>
    /// <param name="tag">The tag to match.</param>
    /// <param name="count">Reference to the counter to increment for each removed listener.</param>
    internal void RemoveByTag(string tag, ref int count)
    {
        List<Delegate> removes = null;

        if (_listeners != null)
        {
            foreach (var pair in _listeners)
            {
                if (pair.Value.Tag == tag)
                {
                    (removes ??= []).Add(pair.Key);
                }
            }
        }

        if (_beforeListeners != null)
        {
            foreach (var pair in _beforeListeners)
            {
                if (pair.Value.Tag == tag)
                {
                    (removes ??= []).Add(pair.Key);
                }
            }
        }

        if (_afterListeners != null)
        {
            foreach (var pair in _afterListeners)
            {
                if (pair.Value.Tag == tag)
                {
                    (removes ??= []).Add(pair.Key);
                }
            }
        }

        if (removes != null)
        {
            foreach (var remove in removes)
            {
                _listeners.Remove(remove);
                count++;
            }
        }
    }

    /// <summary>
    /// Clears all listeners (regular, before, and after).
    /// </summary>
    internal void Clear()
    {
        _listeners?.Clear();
        _beforeListeners?.Clear();
        _afterListeners?.Clear();
    }

    /// <summary>
    /// Dispatches data to all listeners in order: before, regular, then after.
    /// </summary>
    /// <param name="data">The data to dispatch.</param>
    /// <returns>True if any regular listener handled the data; otherwise, false.</returns>
    internal bool DispatchData(object data)
    {
        List<RexNodeListener> list = _listPool.Acquire();

        if (_beforeListeners != null)
        {
            try
            {
                list.AddRange(_beforeListeners.Values);
                foreach (var listener in list)
                {
                    listener.Invoke(data);
                }

                list.Clear();
            }
            catch (RexCancelException)
            {
                list.Clear();
                _listPool.Release(list);

                return false;
            }
        }

        bool handled = false;

        if (_listeners != null)
        {
            list.AddRange(_listeners.Values);
            foreach (var listener in list)
            {
                listener.Invoke(data);
                handled = true;
            }
            list.Clear();
        }

        if (_afterListeners != null)
        {
            list.AddRange(_afterListeners.Values);
            foreach (var listener in list)
            {
                listener.Invoke(data);
            }
            list.Clear();
        }

        _listPool.Release(list);

        return handled;
    }

    /// <summary>
    /// Dispatches an action to all listeners in order: before, regular, then after.
    /// Exceptions are caught and logged.
    /// </summary>
    /// <param name="arguments">The action arguments to dispatch.</param>
    /// <returns>True if any regular listener handled the action; otherwise, false.</returns>
    internal bool DispatchAction(ActionArguments arguments)
    {
        List<RexNodeListener> list = _listPool.Acquire();

        if (_beforeListeners != null)
        {
            try
            {
                list.AddRange(_beforeListeners.Values);
                foreach (var listener in list)
                {
                    try
                    {
                        listener.Invoke(arguments);
                    }
                    catch (Exception err)
                    {
                        RexGlobalResolve.Current?.LogException(err);
                    }
                }
                list.Clear();
            }
            catch (RexCancelException)
            {
                list.Clear();
                _listPool.Release(list);

                return false;
            }
        }

        bool handled = false;

        if (_listeners != null)
        {
            list.AddRange(_listeners.Values);
            foreach (var listener in list)
            {
                try
                {
                    listener.Invoke(arguments);
                    handled = true;
                }
                catch (Exception err)
                {
                    RexGlobalResolve.Current?.LogException(err);
                }
            }
            list.Clear();
        }
        if (_afterListeners != null)
        {
            list.AddRange(_afterListeners.Values);
            foreach (var listener in list)
            {
                try
                {
                    listener.Invoke(arguments);
                }
                catch (Exception err)
                {
                    RexGlobalResolve.Current?.LogException(err);
                }
            }
            list.Clear();
        }

        _listPool.Release(list);

        return handled;
    }

    /// <summary>
    /// Ensures the listener dictionary is initialized and returns it.
    /// </summary>
    /// <returns>The listener dictionary.</returns>
    private Dictionary<Delegate, RexNodeListener> EnsureListenerDic()
    {
        return _listeners ??= [];
    }

    // Object pool for reducing list allocations during dispatch
    private static readonly ConcurrentPool<List<RexNodeListener>> _listPool = new(() => []);
}
