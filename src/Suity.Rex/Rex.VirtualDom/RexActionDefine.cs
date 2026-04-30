using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Defines an action in the Rex tree without arguments.
/// Provides methods to invoke, listen, and create action instances.
/// </summary>
public sealed class RexActionDefine : IRexTreeDefine<ActionArgument>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this action definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionDefine"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Invokes the action immediately on the specified RexTree.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    public void Invoke(RexTree model)
    {
        model.DoAction(_path);
    }

    /// <summary>
    /// Invokes the action in a queued manner on the specified RexTree.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    public void InvokeQueued(RexTree model)
    {
        model.DoActionQueued(_path);
    }

    /// <summary>
    /// Creates a new <see cref="RexAction"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the action.</param>
    /// <returns>A new <see cref="RexAction"/> instance.</returns>
    public RexAction MakeAction(RexTree model)
    {
        return new RexAction(model, _path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexTree model, Action action, string tag = null)
    {
        return model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(RexTree model, Action action, string tag = null)
    {
        return model.AddActionListener(_path, () => RexGlobalResolve.Current?.DoQueuedAction(action), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="model">The RexTree to remove the listener from.</param>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexTree model, Action action)
    {
        return model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Defines an action in the Rex tree with a single argument.
/// Provides methods to invoke, listen, and create action instances.
/// </summary>
/// <typeparam name="T">The type of the action argument.</typeparam>
public sealed class RexActionDefine<T> : IRexTreeDefine<ActionArgument<T>>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this action definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionDefine{T}"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Invokes the action immediately on the specified RexTree with an argument.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="argument">The argument to pass to the action.</param>
    public void Invoke(RexTree model, T argument)
    {
        model.DoAction<T>(_path, argument);
    }

    /// <summary>
    /// Invokes the action in a queued manner on the specified RexTree with an argument.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="argument">The argument to pass to the action.</param>
    public void InvokeQueued(RexTree model, T argument)
    {
        model.DoActionQueued<T>(_path, argument);
    }

    /// <summary>
    /// Creates a new <see cref="RexAction{T}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the action.</param>
    /// <returns>A new <see cref="RexAction{T}"/> instance.</returns>
    public RexAction<T> MakeAction(RexTree model)
    {
        return new RexAction<T>(model, _path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action argument.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexTree model, Action<T> action, string tag = null)
    {
        return model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action argument.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(RexTree model, Action<T> action, string tag = null)
    {
        return model.AddActionListener<T>(_path, v => RexGlobalResolve.Current?.DoQueuedAction(() => action(v)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="model">The RexTree to remove the listener from.</param>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexTree model, Action<T> action)
    {
        return model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Defines an action in the Rex tree with two arguments.
/// Provides methods to invoke, listen, and create action instances.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public sealed class RexActionDefine<T1, T2> : IRexTreeDefine<ActionArgument<T1, T2>>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this action definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionDefine{T1, T2}"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Invokes the action immediately on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void Invoke(RexTree model, T1 arg1, T2 arg2)
    {
        model.DoAction(_path, arg1, arg2);
    }

    /// <summary>
    /// Invokes the action in a queued manner on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void InvokeQueued(RexTree model, T1 arg1, T2 arg2)
    {
        model.DoActionQueued(_path, arg1, arg2);
    }

    /// <summary>
    /// Creates a new <see cref="RexAction{T1, T2}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the action.</param>
    /// <returns>A new <see cref="RexAction{T1, T2}"/> instance.</returns>
    public RexAction<T1, T2> MakeAction(RexTree model)
    {
        return new RexAction<T1, T2>(model, _path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexTree model, Action<T1, T2> action, string tag = null)
    {
        return model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(RexTree model, Action<T1, T2> action, string tag = null)
    {
        return model.AddActionListener<T1, T2>(_path, (v1, v2) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="model">The RexTree to remove the listener from.</param>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexTree model, Action<T1, T2> action)
    {
        return model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Defines an action in the Rex tree with three arguments.
/// Provides methods to invoke, listen, and create action instances.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public sealed class RexActionDefine<T1, T2, T3> : IRexTreeDefine<ActionArgument<T1, T2, T3>>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this action definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionDefine{T1, T2, T3}"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Invokes the action immediately on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void Invoke(RexTree model, T1 arg1, T2 arg2, T3 arg3)
    {
        model.DoAction(_path, arg1, arg2, arg3);
    }

    /// <summary>
    /// Invokes the action in a queued manner on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void InvokeQueued(RexTree model, T1 arg1, T2 arg2, T3 arg3)
    {
        model.DoActionQueued(_path, arg1, arg2, arg3);
    }

    /// <summary>
    /// Creates a new <see cref="RexAction{T1, T2, T3}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the action.</param>
    /// <returns>A new <see cref="RexAction{T1, T2, T3}"/> instance.</returns>
    public RexAction<T1, T2, T3> MakeAction(RexTree model)
    {
        return new RexAction<T1, T2, T3>(model, _path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexTree model, Action<T1, T2, T3> action, string tag = null)
    {
        return model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(RexTree model, Action<T1, T2, T3> action, string tag = null)
    {
        return model.AddActionListener<T1, T2, T3>(_path, (v1, v2, v3) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2, v3)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="model">The RexTree to remove the listener from.</param>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexTree model, Action<T1, T2, T3> action)
    {
        return model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}

/// <summary>
/// Defines an action in the Rex tree with four arguments.
/// Provides methods to invoke, listen, and create action instances.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public sealed class RexActionDefine<T1, T2, T3, T4> : IRexTreeDefine<ActionArgument<T1, T2, T3, T4>>
{
    private readonly RexPath _path;

    /// <summary>
    /// Gets the path within the RexTree for this action definition.
    /// </summary>
    public RexPath Path => _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionDefine{T1, T2, T3, T4}"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionDefine(RexPath path)
    {
        _path = path;
    }

    /// <summary>
    /// Invokes the action immediately on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void Invoke(RexTree model, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        model.DoAction(_path, arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// Invokes the action in a queued manner on the specified RexTree with arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void InvokeQueued(RexTree model, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        model.DoActionQueued(_path, arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// Creates a new <see cref="RexAction{T1, T2, T3, T4}"/> instance for this definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with the action.</param>
    /// <returns>A new <see cref="RexAction{T1, T2, T3, T4}"/> instance.</returns>
    public RexAction<T1, T2, T3, T4> MakeAction(RexTree model)
    {
        return new RexAction<T1, T2, T3, T4>(model, _path);
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddActionListener(RexTree model, Action<T1, T2, T3, T4> action, string tag = null)
    {
        return model.AddActionListener(_path, action, tag);
    }

    /// <summary>
    /// Adds a queued action listener that will execute the callback in a queued manner.
    /// </summary>
    /// <param name="model">The RexTree to add the listener to.</param>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    /// <returns>A disposable that can remove the listener when disposed.</returns>
    public IDisposable AddQueuedActionListener(RexTree model, Action<T1, T2, T3, T4> action, string tag = null)
    {
        return model.AddActionListener<T1, T2, T3, T4>(_path, (v1, v2, v3, v4) => RexGlobalResolve.Current?.DoQueuedAction(() => action(v1, v2, v3, v4)), tag);
    }

    /// <summary>
    /// Removes a previously added action listener.
    /// </summary>
    /// <param name="model">The RexTree to remove the listener from.</param>
    /// <param name="action">The callback to remove.</param>
    /// <returns>True if the listener was found and removed; otherwise, false.</returns>
    public bool RemoveListener(RexTree model, Action<T1, T2, T3, T4> action)
    {
        return model.RemoveListener(_path, action);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}
