using System;
using System.Collections.Generic;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents the core RexTree data structure. Manages a hierarchical tree of nodes with support for data storage,
/// action dispatching, listener management, and path mapping.
/// </summary>
public class RexTree /*: Suity.Object */
{
    /// <summary>
    /// Gets the global RexTree instance shared across the application.
    /// </summary>
    public static readonly RexTree Global = new(true);

    internal readonly bool _isGlobal;
    private readonly RexNode _rootNode;
    private readonly Dictionary<RexPath, RexNodeListenerSet> _listeners = [];

    /// <summary>
    /// Occurs when data is set at a path in the tree.
    /// </summary>
    public event EventHandler<RexPathValueEventArgs> PathSet;

    /// <summary>
    /// Occurs when computed data is set at a path in the tree.
    /// </summary>
    public event EventHandler<RexPathEventArgs> PathSetComputed;

    /// <summary>
    /// Occurs when data is updated at a path in the tree.
    /// </summary>
    public event EventHandler<RexPathEventArgs> PathUpdate;

    /// <summary>
    /// Occurs when data is retrieved from a path in the tree.
    /// </summary>
    public event EventHandler<RexPathEventArgs> PathGet;

    /// <summary>
    /// Occurs when an action is dispatched at a path in the tree.
    /// </summary>
    public event EventHandler<RexPathValueEventArgs> PathDoAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexTree"/> class with an empty root node.
    /// </summary>
    public RexTree()
    {
        _rootNode = new RexNode(PathItem.Empty)
        {
            _model = this,
            _path = RexPath.Empty
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexTree"/> class and sets the root data deeply.
    /// </summary>
    /// <param name="rootData">The root data to set.</param>
    public RexTree(object rootData)
        : this()
    {
        SetDataDeep(RexPath.Empty, rootData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexTree"/> class as a global tree.
    /// </summary>
    /// <param name="isGlobal">True if this is the global tree.</param>
    private RexTree(bool isGlobal)
        : this()
    {
        _isGlobal = isGlobal;
    }

    //protected override string GetName()
    //{
    //    if (_isGlobal)
    //    {
    //        return "Global RexTree";
    //    }
    //    else
    //    {
    //        return base.GetName();
    //    }
    //}

    /// <summary>
    /// Gets the root node of the tree.
    /// </summary>
    public RexNode RootNode => _rootNode;

    /// <summary>
    /// Gets the node at the specified path.
    /// </summary>
    /// <param name="path">The path to the node.</param>
    /// <returns>The node at the specified path, or null if not found.</returns>
    public RexNode GetNode(RexPath path)
    {
        RexNode currentNode = _rootNode;
        foreach (var item in path.Items)
        {
            currentNode = currentNode.GetNode(item);
            if (currentNode is null)
            {
                return null;
            }
        }

        return currentNode;
    }

    #region Data

    /// <summary>
    /// Sets data deeply at the root path, replacing all nested child data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The data to set.</param>
    public void SetDataDeep<T>(T data)
    {
        SetDataDeep(RexPath.Empty, data);
    }

    /// <summary>
    /// Sets data deeply at the specified path, replacing all nested child data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="path">The path to set the data at.</param>
    /// <param name="data">The data to set.</param>
    public void SetDataDeep<T>(RexPath path, T data)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathSet(path, data);

        RexNode currentNode = EnsureNode(path);

        currentNode.SetDataDeep(data);
    }

    /// <summary>
    /// Sets data at the specified path without affecting child nodes.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="path">The path to set the data at.</param>
    /// <param name="data">The data to set.</param>
    public void SetData<T>(RexPath path, T data)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathSet(path, data);

        RexNode currentNode = _rootNode;
        PathItem lastPathItem = PathItem.Empty;

        foreach (var item in path.Items)
        {
            currentNode = currentNode.EnsureNode(item);
            lastPathItem = item;
        }
        currentNode.SetData(data);
        //if (currentWNode != null && currentWNode != currentNode)
        //{
        //    currentWNode.UpdateDataListenerDeep(true, lastPathItem);
        //}
    }

    /// <summary>
    /// Sets the default data at the specified path if no data currently exists.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="path">The path to set the data at.</param>
    /// <param name="defaultData">The default data to set.</param>
    public void SetDefaultData<T>(RexPath path, T defaultData)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathSet(path, defaultData);

        RexNode currentNode = _rootNode;
        RexNode currentWNode = null;
        PathItem lastPathItem = PathItem.Empty;

        foreach (var item in path.Items)
        {
            currentNode = currentNode.EnsureNode(item);
            lastPathItem = item;
        }
        currentNode.SetDefaultData(defaultData);
        //if (currentWNode != null && currentWNode != currentNode)
        //{
        //    currentWNode.UpdateDataListenerDeep(true, lastPathItem);
        //}
    }

    /// <summary>
    /// Triggers an update notification at the specified path, causing listeners to re-evaluate.
    /// </summary>
    /// <param name="path">The path to update.</param>
    public void UpdateData(RexPath path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathUpdate(path);

        RexNode currentNode = _rootNode;
        RexNode currentWNode = null;
        PathItem lastPathItem = PathItem.Empty;

        foreach (var item in path.Items)
        {
            currentNode = currentNode.GetNode(item);
            lastPathItem = item;
            if (currentNode is null && currentWNode is null)
            {
                return;
            }
        }
        currentNode?.UpdateData();
        //if (currentWNode != null && currentWNode != currentNode)
        //{
        //    currentWNode.UpdateDataListenerDeep(false, lastPathItem);
        //}
    }

    /// <summary>
    /// Sets data at the specified path in a queued manner for deferred execution.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="path">The path to set the data at.</param>
    /// <param name="data">The data to set.</param>
    public void SetDataQueued<T>(RexPath path, T data)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => SetData<T>(path, data));
    }

    /// <summary>
    /// Triggers an update notification at the specified path in a queued manner.
    /// </summary>
    /// <param name="path">The path to update.</param>
    public void UpdateDataQueued(RexPath path)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => UpdateData(path));
    }

    /// <summary>
    /// Sets computed data at the specified path with optional getter and setter functions.
    /// </summary>
    /// <typeparam name="T">The type of the computed data.</typeparam>
    /// <param name="path">The path to set the computed data at.</param>
    /// <param name="getter">The function to compute the value. Can be null.</param>
    /// <param name="setter">The action to set the value. Can be null.</param>
    /// <returns>A disposable that can remove the computed data when disposed.</returns>
    public IDisposable SetComputedData<T>(RexPath path, Func<T> getter = null, Action<T> setter = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathSetComputed(path);

        RexNode currentNode = _rootNode;
        foreach (var item in path.Items)
        {
            currentNode = currentNode.EnsureNode(item);
        }
        if (getter != null || setter != null)
        {
            return currentNode.SetComputed(new ComputedData<T>(getter, setter));
        }
        else
        {
            return currentNode.SetComputed(null);
        }
    }

    /// <summary>
    /// Gets the data at the specified path.
    /// </summary>
    /// <param name="path">The path to retrieve data from.</param>
    /// <returns>The data at the specified path, or null if not found.</returns>
    public object GetData(RexPath path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        OnPathGet(path);

        RexNode currentNode = GetNode(path);

        return currentNode?.GetData();
    }

    /// <summary>
    /// Gets the data at the specified path, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the data to.</typeparam>
    /// <param name="path">The path to retrieve data from.</param>
    /// <returns>The data cast to type <typeparamref name="T"/>, or default if not found or incompatible.</returns>
    public T GetData<T>(RexPath path)
    {
        object data = GetData(path);

        if (data is T t)
        {
            return t;
        }
        else
        {
            return default;
        }
    }

    #endregion

    #region DoAction

    /// <summary>
    /// Dispatches an action without arguments at the specified path.
    /// </summary>
    /// <param name="path">The path to dispatch the action at.</param>
    public void DoAction(RexPath path)
    {
        DoAction(path, ActionArgument.Empty as ActionArguments);
    }

    /// <summary>
    /// Dispatches an action with a single argument at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="argument">The argument to pass.</param>
    public void DoAction<T>(RexPath path, T argument)
    {
        DoAction(path, new ActionArgument<T>(argument) as ActionArguments);
    }

    /// <summary>
    /// Dispatches an action with two arguments at the specified path.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void DoAction<T1, T2>(RexPath path, T1 arg1, T2 arg2)
    {
        DoAction(path, new ActionArgument<T1, T2>(arg1, arg2) as ActionArguments);
    }

    /// <summary>
    /// Dispatches an action with three arguments at the specified path.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void DoAction<T1, T2, T3>(RexPath path, T1 arg1, T2 arg2, T3 arg3)
    {
        DoAction(path, new ActionArgument<T1, T2, T3>(arg1, arg2, arg3) as ActionArguments);
    }

    /// <summary>
    /// Dispatches an action with four arguments at the specified path.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void DoAction<T1, T2, T3, T4>(RexPath path, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        DoAction(path, new ActionArgument<T1, T2, T3, T4>(arg1, arg2, arg3, arg4) as ActionArguments);
    }

    /// <summary>
    /// Dispatches an action with the specified arguments at the given path.
    /// </summary>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arguments">The action arguments to pass.</param>
    public void DoAction(RexPath path, ActionArguments arguments)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        OnPathDoAction(path, arguments);

        var currentNode = GetListener(path);
        currentNode?.DispatchAction(arguments);

        //var obj = GetData(path);
        //if (obj != null)
        //{
        //    if (obj is Action<ActionArguments>)
        //    {
        //        ((Action<ActionArguments>)obj)(arguments);
        //    }
        //    else if (obj is Action && arguments is ActionArgument)
        //    {
        //        ((Action)obj)();
        //    }
        //}
    }

    /// <summary>
    /// Dispatches an action without arguments at the specified path in a queued manner.
    /// </summary>
    /// <param name="path">The path to dispatch the action at.</param>
    public void DoActionQueued(RexPath path)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => DoAction(path));
    }

    /// <summary>
    /// Dispatches an action with a single argument at the specified path in a queued manner.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="argument">The argument to pass.</param>
    public void DoActionQueued<T>(RexPath path, T argument)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => DoAction(path, argument));
    }

    /// <summary>
    /// Dispatches an action with two arguments at the specified path in a queued manner.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void DoActionQueued<T1, T2>(RexPath path, T1 arg1, T2 arg2)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => DoAction(path, arg1, arg2));
    }

    /// <summary>
    /// Dispatches an action with three arguments at the specified path in a queued manner.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void DoActionQueued<T1, T2, T3>(RexPath path, T1 arg1, T2 arg2, T3 arg3)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => DoAction(path, arg1, arg2, arg3));
    }

    /// <summary>
    /// Dispatches an action with four arguments at the specified path in a queued manner.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="path">The path to dispatch the action at.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void DoActionQueued<T1, T2, T3, T4>(RexPath path, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        RexGlobalResolve.Current?.DoQueuedAction(() => DoAction(path, arg1, arg2, arg3, arg4));
    }

    #endregion

    #region Listener

    /// <summary>
    /// Adds a data listener that will be invoked when data changes at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke with the new data.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A handle that can be used to manage the listener.</returns>
    public IRexHandle AddDataListener<T>(RexPath path, Action<T> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddDataListener<T>(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds an action listener without arguments.
    /// </summary>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexPath path, Action callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddActionListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with a single argument.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener<T>(RexPath path, Action<T> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddActionListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with two arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener<T1, T2>(RexPath path, Action<T1, T2> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }
        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddActionListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with three arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener<T1, T2, T3>(RexPath path, Action<T1, T2, T3> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddActionListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds an action listener with four arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener<T1, T2, T3, T4>(RexPath path, Action<T1, T2, T3, T4> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddActionListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Adds a before-listener that will be invoked before regular data listeners.
    /// Can be cancelled by throwing a <see cref="RexCancelException"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke with the data.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A handle that can be used to manage the listener.</returns>
    public IRexHandle AddBeforeListener<T>(RexPath path, Action<T> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddBeforeListener<T>(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Removes a before-listener.
    /// </summary>
    /// <typeparam name="T">The type of data the listener was registered for.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveBeforeListener<T>(RexPath path, Action<T> callBack)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        RexNodeListenerSet currentNode = GetListener(path);
        if (currentNode != null)
        {
            return currentNode.RemoveBeforeListener(callBack);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Adds an after-listener that will be invoked after regular data listeners.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="path">The path to listen on.</param>
    /// <param name="callBack">The callback to invoke with the data.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A handle that can be used to manage the listener.</returns>
    public IRexHandle AddAfterListener<T>(RexPath path, Action<T> callBack, string tag = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        RexNodeListenerSet currentNode = EnsureListener(path);
        var listener = currentNode.AddAfterListener(callBack);
        listener.Tag = tag;

        return listener;
    }

    /// <summary>
    /// Removes an after-listener.
    /// </summary>
    /// <typeparam name="T">The type of data the listener was registered for.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool UnsetAfterListener<T>(RexPath path, Action<T> callBack)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        RexNodeListenerSet currentNode = GetListener(path);
        if (currentNode != null)
        {
            return currentNode.RemoveAfterListener(callBack);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes an action listener without arguments.
    /// </summary>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexPath path, Action callBack)
    {
        return RemoveDelegateListener(path, callBack);
    }

    /// <summary>
    /// Removes an action listener with a single argument.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener<T>(RexPath path, Action<T> callBack)
    {
        return RemoveDelegateListener(path, callBack);
    }

    /// <summary>
    /// Removes an action listener with two arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener<T1, T2>(RexPath path, Action<T1, T2> callBack)
    {
        return RemoveDelegateListener(path, callBack);
    }

    /// <summary>
    /// Removes an action listener with three arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener<T1, T2, T3>(RexPath path, Action<T1, T2, T3> callBack)
    {
        return RemoveDelegateListener(path, callBack);
    }

    /// <summary>
    /// Removes an action listener with four arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener<T1, T2, T3, T4>(RexPath path, Action<T1, T2, T3, T4> callBack)
    {
        return RemoveDelegateListener(path, callBack);
    }

    /// <summary>
    /// Removes a listener by its delegate from all listener collections.
    /// </summary>
    /// <param name="path">The path the listener was registered on.</param>
    /// <param name="callBack">The delegate to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    private bool RemoveDelegateListener(RexPath path, Delegate callBack)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        RexNodeListenerSet currentNode = GetListener(path);
        if (currentNode != null)
        {
            return currentNode.RemoveListener(callBack);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all listeners with the specified tag from the entire tree.
    /// </summary>
    /// <param name="tag">The tag to match.</param>
    /// <returns>The number of listeners removed.</returns>
    public int RemoveListenersByTag(string tag)
    {
        int count = 0;
        _rootNode.RemoveListenersByTagDeep(tag, ref count);

        return count;
    }

    #endregion

    #region Mapping

    /// <summary>
    /// Adds a mapping that will trigger updates on the target path when the source path changes.
    /// </summary>
    /// <param name="path">The source path to monitor.</param>
    /// <param name="pathTo">The target path to update.</param>
    /// <returns>A disposable that can remove the mapping when disposed.</returns>
    public IDisposable AddMapping(RexPath path, RexPath pathTo)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (pathTo is null)
        {
            throw new ArgumentNullException(nameof(pathTo));
        }

        var listener = EnsureListener(path).AddMapping(pathTo);

        return listener;
    }

    /// <summary>
    /// Adds multiple mappings from a source path to multiple target paths.
    /// </summary>
    /// <param name="path">The source path to monitor.</param>
    /// <param name="pathTos">The target paths to update.</param>
    public void AddMappings(RexPath path, params RexPath[] pathTos)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        var node = EnsureListener(path);
        foreach (var pathTo in pathTos)
        {
            if (pathTo != null)
            {
                node.AddMapping(pathTo);
            }
        }
    }

    /// <summary>
    /// Adds multiple mappings from multiple source paths to a single target path.
    /// </summary>
    /// <param name="pathTo">The target path to update.</param>
    /// <param name="paths">The source paths to monitor.</param>
    public void AddMappingsFrom(RexPath pathTo, params RexPath[] paths)
    {
        if (pathTo is null)
        {
            throw new ArgumentNullException(nameof(pathTo));
        }

        foreach (var path in paths)
        {
            if (path != null)
            {
                EnsureListener(path).AddMapping(pathTo);
            }
        }
    }

    /// <summary>
    /// Removes a mapping from a source path to a target path.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <param name="pathTo">The target path.</param>
    /// <returns>True if the mapping was found and removed; otherwise, false.</returns>
    public bool RemoveMapping(RexPath path, RexPath pathTo)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (pathTo is null)
        {
            throw new ArgumentNullException(nameof(pathTo));
        }

        var node = GetListener(path);
        if (node != null)
        {
            return node.RemoveMapping(pathTo);
        }

        return false;
    }

    /// <summary>
    /// Removes multiple mappings from a source path to multiple target paths.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <param name="pathTos">The target paths.</param>
    public void RemoveMappings(RexPath path, params RexPath[] pathTos)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        var node = GetListener(path);
        if (node != null)
        {
            foreach (var pathTo in pathTos)
            {
                if (pathTo != null)
                {
                    node.RemoveMapping(pathTo);
                }
            }
        }
    }

    /// <summary>
    /// Removes multiple mappings from multiple source paths to a single target path.
    /// </summary>
    /// <param name="pathTo">The target path.</param>
    /// <param name="paths">The source paths.</param>
    public void RemoveMappingsFrom(RexPath pathTo, params RexPath[] paths)
    {
        if (pathTo is null)
        {
            throw new ArgumentNullException(nameof(pathTo));
        }

        foreach (var path in paths)
        {
            var node = path != null ? GetListener(path) : null;
            node?.RemoveMapping(pathTo);
        }
    }

    #endregion

    /// <summary>
    /// Clears all data and child nodes from the tree.
    /// </summary>
    public void Clear()
    {
        _rootNode.Clear();
    }

    #region Internal

    /// <summary>
    /// Raises the PathSet event.
    /// </summary>
    /// <param name="path">The path where data was set.</param>
    /// <param name="value">The value that was set.</param>
    internal void OnPathSet(RexPath path, object value)
    {
        PathSet?.Invoke(this, new RexPathValueEventArgs(path, value));
    }

    /// <summary>
    /// Raises the PathSetComputed event.
    /// </summary>
    /// <param name="path">The path where computed data was set.</param>
    internal void OnPathSetComputed(RexPath path)
    {
        PathSetComputed?.Invoke(this, new RexPathEventArgs(path));
    }

    /// <summary>
    /// Raises the PathGet event.
    /// </summary>
    /// <param name="path">The path where data was retrieved.</param>
    internal void OnPathGet(RexPath path)
    {
        PathGet?.Invoke(this, new RexPathEventArgs(path));
    }

    /// <summary>
    /// Raises the PathUpdate event.
    /// </summary>
    /// <param name="path">The path that was updated.</param>
    internal void OnPathUpdate(RexPath path)
    {
        PathUpdate?.Invoke(this, new RexPathEventArgs(path));
    }

    /// <summary>
    /// Raises the PathDoAction event.
    /// </summary>
    /// <param name="path">The path where the action was dispatched.</param>
    /// <param name="args">The action arguments.</param>
    internal void OnPathDoAction(RexPath path, ActionArguments args)
    {
        PathDoAction?.Invoke(this, new RexPathValueEventArgs(path, args));
    }

    /// <summary>
    /// Ensures that all nodes along the specified path exist, creating them if necessary.
    /// </summary>
    /// <param name="path">The path to ensure nodes for.</param>
    /// <returns>The node at the end of the path.</returns>
    private RexNode EnsureNode(RexPath path)
    {
        RexNode currentNode = _rootNode;
        foreach (var item in path.Items)
        {
            currentNode = currentNode.EnsureNode(item);
        }

        return currentNode;
    }

    /// <summary>
    /// Ensures that a listener set exists for the specified path, creating it if necessary.
    /// </summary>
    /// <param name="path">The path to ensure a listener for.</param>
    /// <returns>The listener set for the path.</returns>
    private RexNodeListenerSet EnsureListener(RexPath path)
    {
        if (!_listeners.TryGetValue(path, out RexNodeListenerSet listener))
        {
            listener = new RexNodeListenerSet(this, path);
            _listeners.Add(path, listener);
            RexNode node = GetNode(path);
            if (node != null)
            {
                node._listener = listener;
            }
        }

        return listener;
    }

    /// <summary>
    /// Gets the listener set for the specified path, if it exists.
    /// </summary>
    /// <param name="path">The path to get the listener set for.</param>
    /// <returns>The listener set, or null if not found.</returns>
    internal RexNodeListenerSet GetListener(RexPath path)
    {
        return _listeners.GetValueSafe(path);
    }

    #endregion
}
