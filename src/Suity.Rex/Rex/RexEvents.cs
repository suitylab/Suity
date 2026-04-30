using System;

namespace Suity.Rex;

#region IRexEvent

/// <summary>
/// Defines an event that can notify listeners without arguments.
/// </summary>
public interface IRexEvent
{
    /// <summary>
    /// Adds a listener to be invoked when the event is triggered.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void AddListener(Action action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action action);
}

/// <summary>
/// Defines an event that can notify listeners with a single argument of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the event argument.</typeparam>
public interface IRexEvent<T>
{
    /// <summary>
    /// Adds a listener to be invoked when the event is triggered.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void AddListener(Action<T> action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action<T> action);
}

/// <summary>
/// Defines an event that can notify listeners with two arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public interface IRexEvent<T1, T2>
{
    /// <summary>
    /// Adds a listener to be invoked when the event is triggered.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void AddListener(Action<T1, T2> action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action<T1, T2> action);
}

/// <summary>
/// Defines an event that can notify listeners with three arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public interface IRexEvent<T1, T2, T3>
{
    /// <summary>
    /// Adds a listener to be invoked when the event is triggered.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void AddListener(Action<T1, T2, T3> action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action<T1, T2, T3> action);
}

/// <summary>
/// Defines an event that can notify listeners with four arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public interface IRexEvent<T1, T2, T3, T4>
{
    /// <summary>
    /// Adds a listener to be invoked when the event is triggered.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void AddListener(Action<T1, T2, T3, T4> action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action<T1, T2, T3, T4> action);
}

#endregion

#region RexEventHandle

/// <summary>
/// A handle for managing parameterless event subscriptions.
/// </summary>
public class RexEventHandle : IRexEvent
{
    private Action _action;

    /// <inheritdoc/>
    public void AddListener(Action action)
    {
        _action += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action action)
    {
        _action -= action;
    }

    /// <summary>
    /// Invokes all registered listeners.
    /// </summary>
    public void Invoke()
    {
        _action?.Invoke();
    }
}

/// <summary>
/// A handle for managing event subscriptions with a single argument.
/// </summary>
/// <typeparam name="T">The type of the event argument.</typeparam>
public class RexEventHandle<T> : IRexEvent<T>
{
    private Action<T> _action;

    /// <inheritdoc/>
    public void AddListener(Action<T> action)
    {
        _action += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T> action)
    {
        _action -= action;
    }

    /// <summary>
    /// Invokes all registered listeners with the specified argument.
    /// </summary>
    /// <param name="arg">The argument to pass to listeners.</param>
    public void Invoke(T arg)
    {
        _action?.Invoke(arg);
    }
}

/// <summary>
/// A handle for managing event subscriptions with two arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public class RexEventHandle<T1, T2> : IRexEvent<T1, T2>
{
    private Action<T1, T2> _action;

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2> action)
    {
        _action += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2> action)
    {
        _action -= action;
    }

    /// <summary>
    /// Invokes all registered listeners with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public void Invoke(T1 arg1, T2 arg2)
    {
        _action?.Invoke(arg1, arg2);
    }
}

/// <summary>
/// A handle for managing event subscriptions with three arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public class RexEventHandle<T1, T2, T3> : IRexEvent<T1, T2, T3>
{
    private Action<T1, T2, T3> _action;

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2, T3> action)
    {
        _action += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2, T3> action)
    {
        _action -= action;
    }

    /// <summary>
    /// Invokes all registered listeners with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        _action?.Invoke(arg1, arg2, arg3);
    }
}

/// <summary>
/// A handle for managing event subscriptions with four arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public class RexEventHandle<T1, T2, T3, T4> : IRexEvent<T1, T2, T3, T4>
{
    private Action<T1, T2, T3, T4> _action;

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2, T3, T4> action)
    {
        _action += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2, T3, T4> action)
    {
        _action -= action;
    }

    /// <summary>
    /// Invokes all registered listeners with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _action?.Invoke(arg1, arg2, arg3, arg4);
    }
}

#endregion

#region RexEvent

/// <summary>
/// A read-only wrapper around a <see cref="RexEventHandle"/> that exposes event subscription capabilities.
/// </summary>
public class RexEvent : IRexEvent
{
    private readonly RexEventHandle _handle;

    /// <summary>
    /// Initializes a new instance with the specified event handle.
    /// </summary>
    /// <param name="handle">The underlying event handle. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is null.</exception>
    public RexEvent(RexEventHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public void AddListener(Action action)
    {
        _handle.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action action)
    {
        _handle.RemoveListener(action);
    }
}

/// <summary>
/// A read-only wrapper around a <see cref="RexEventHandle{T}"/> that exposes event subscription capabilities.
/// </summary>
/// <typeparam name="T">The type of the event argument.</typeparam>
public class RexEvent<T> : IRexEvent<T>
{
    private readonly RexEventHandle<T> _handle;

    /// <summary>
    /// Initializes a new instance with the specified event handle.
    /// </summary>
    /// <param name="handle">The underlying event handle. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is null.</exception>
    public RexEvent(RexEventHandle<T> handle)
    {
        _handle = handle ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public void AddListener(Action<T> action)
    {
        _handle.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T> action)
    {
        _handle.RemoveListener(action);
    }
}

/// <summary>
/// A read-only wrapper around a <see cref="RexEventHandle{T1, T2}"/> that exposes event subscription capabilities.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public class RexEvent<T1, T2> : IRexEvent<T1, T2>
{
    private readonly RexEventHandle<T1, T2> _handle;

    /// <summary>
    /// Initializes a new instance with the specified event handle.
    /// </summary>
    /// <param name="handle">The underlying event handle. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is null.</exception>
    public RexEvent(RexEventHandle<T1, T2> handle)
    {
        _handle = handle ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2> action)
    {
        _handle.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2> action)
    {
        _handle.RemoveListener(action);
    }
}

/// <summary>
/// A read-only wrapper around a <see cref="RexEventHandle{T1, T2, T3}"/> that exposes event subscription capabilities.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public class RexEvent<T1, T2, T3> : IRexEvent<T1, T2, T3>
{
    private readonly RexEventHandle<T1, T2, T3> _handle;

    /// <summary>
    /// Initializes a new instance with the specified event handle.
    /// </summary>
    /// <param name="handle">The underlying event handle. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is null.</exception>
    public RexEvent(RexEventHandle<T1, T2, T3> handle)
    {
        _handle = handle ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2, T3> action)
    {
        _handle.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2, T3> action)
    {
        _handle.RemoveListener(action);
    }
}

/// <summary>
/// A read-only wrapper around a <see cref="RexEventHandle{T1, T2, T3, T4}"/> that exposes event subscription capabilities.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public class RexEvent<T1, T2, T3, T4> : IRexEvent<T1, T2, T3, T4>
{
    private readonly RexEventHandle<T1, T2, T3, T4> _handle;

    /// <summary>
    /// Initializes a new instance with the specified event handle.
    /// </summary>
    /// <param name="handle">The underlying event handle. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is null.</exception>
    public RexEvent(RexEventHandle<T1, T2, T3, T4> handle)
    {
        _handle = handle ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public void AddListener(Action<T1, T2, T3, T4> action)
    {
        _handle.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T1, T2, T3, T4> action)
    {
        _handle.RemoveListener(action);
    }
}

#endregion
