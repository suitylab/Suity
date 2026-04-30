using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents an action in the Rex tree without arguments.
/// Provides methods to invoke, add listeners, and remove listeners.
/// </summary>
public sealed class RexAction : IRexTreeInstance<ActionArgument>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexAction(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexAction(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Gets the RexTree associated with this action.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this action.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Invokes the action immediately.
    /// </summary>
    public void Invoke()
    {
        _model.DoAction(_path);
    }

    /// <summary>
    /// Invokes the action in a queued manner for deferred execution.
    /// </summary>
    public void InvokeQueued()
    {
        _model.DoActionQueued(_path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(Action action, string tag = null)
    {
        return _model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="action">The callback to invoke.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(Action action, string tag = null)
    {
        return _model.AddActionListener(_path, () => RexGlobalResolve.Current?.DoQueuedAction(action), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(Action action)
    {
        return _model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Represents an action in the Rex tree with a single argument.
/// Provides methods to invoke, add listeners, and remove listeners.
/// </summary>
/// <typeparam name="T">The type of the action argument.</typeparam>
public sealed class RexAction<T> : IRexTreeInstance<ActionArgument<T>>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexAction(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexAction(RexTree model, RexActionDefine<T> define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Gets the RexTree associated with this action.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this action.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Invokes the action immediately with an argument.
    /// </summary>
    /// <param name="argument">The argument to pass to the action.</param>
    public void Invoke(T argument)
    {
        _model.DoAction<T>(_path, argument);
    }

    /// <summary>
    /// Invokes the action in a queued manner with an argument.
    /// </summary>
    /// <param name="argument">The argument to pass to the action.</param>
    public void InvokeQueued(T argument)
    {
        _model.DoActionQueued<T>(_path, argument);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action argument.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(Action<T> action, string tag = null)
    {
        return _model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="action">The callback to invoke with the action argument.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(Action<T> action, string tag = null)
    {
        return _model.AddActionListener<T>(_path, v => RexGlobalResolve.Current?.DoQueuedAction(() => action(v)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(Action<T> action)
    {
        return _model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Represents an action in the Rex tree with two arguments.
/// Provides methods to invoke, add listeners, and remove listeners.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public sealed class RexAction<T1, T2> : IRexTreeInstance<ActionArgument<T1, T2>>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexAction(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexAction(RexTree model, RexActionDefine<T1, T2> define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Gets the RexTree associated with this action.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this action.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Invokes the action immediately with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void Invoke(T1 arg1, T2 arg2)
    {
        _model.DoAction(_path, arg1, arg2);
    }

    /// <summary>
    /// Invokes the action in a queued manner with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void InvokeQueued(T1 arg1, T2 arg2)
    {
        _model.DoActionQueued(_path, arg1, arg2);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(Action<T1, T2> action, string tag = null)
    {
        return _model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(Action<T1, T2> action, string tag = null)
    {
        return _model.AddActionListener<T1, T2>(_path, (v1, v2) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(Action<T1, T2> action)
    {
        return _model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Represents an action in the Rex tree with three arguments.
/// Provides methods to invoke, add listeners, and remove listeners.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public sealed class RexAction<T1, T2, T3> : IRexTreeInstance<ActionArgument<T1, T2, T3>>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexAction(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2, T3}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexAction(RexTree model, RexActionDefine<T1, T2, T3> define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Gets the RexTree associated with this action.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this action.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Invokes the action immediately with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        _model.DoAction(_path, arg1, arg2, arg3);
    }

    /// <summary>
    /// Invokes the action in a queued manner with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void InvokeQueued(T1 arg1, T2 arg2, T3 arg3)
    {
        _model.DoActionQueued(_path, arg1, arg2, arg3);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(Action<T1, T2, T3> action, string tag = null)
    {
        return _model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(Action<T1, T2, T3> action, string tag = null)
    {
        return _model.AddActionListener<T1, T2, T3>(_path, (v1, v2, v3) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2, v3)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(Action<T1, T2, T3> action)
    {
        return _model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Represents an action in the Rex tree with four arguments.
/// Provides methods to invoke, add listeners, and remove listeners.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public sealed class RexAction<T1, T2, T3, T4> : IRexTreeInstance<ActionArgument<T1, T2, T3, T4>>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexAction(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAction{T1, T2, T3, T4}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexAction(RexTree model, RexActionDefine<T1, T2, T3, T4> define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Gets the RexTree associated with this action.
    /// </summary>
    public RexTree Tree => _model;

    /// <summary>
    /// Gets the path within the RexTree for this action.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Invokes the action immediately with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _model.DoAction(_path, arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// Invokes the action in a queued manner with arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void InvokeQueued(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _model.DoActionQueued(_path, arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(Action<T1, T2, T3, T4> action, string tag = null)
    {
        return _model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(Action<T1, T2, T3, T4> action, string tag = null)
    {
        return _model.AddActionListener<T1, T2, T3, T4>(_path, (v1, v2, v3, v4) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2, v3, v4)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(Action<T1, T2, T3, T4> action)
    {
        return _model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}
