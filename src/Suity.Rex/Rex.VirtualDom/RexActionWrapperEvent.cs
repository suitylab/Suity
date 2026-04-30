using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Wraps a RexTree action path to provide event-like add/remove listener functionality without arguments.
/// </summary>
public class RexActionWrapperEvent : IRexEvent
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionWrapperEvent(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionWrapperEvent(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent"/> class using a RexAction.
    /// </summary>
    /// <param name="action">The RexAction to wrap.</param>
    public RexActionWrapperEvent(RexAction action)
    {
        _model = action.Tree;
        _path = action.Path;
    }

    /// <summary>
    /// Adds a listener that will be invoked when the action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke.</param>
    public void AddListener(Action action)
    {
        _model.AddActionListener(_path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public void RemoveListener(Action action)
    {
        _model.RemoveListener(_path, action);
    }
}

/// <summary>
/// Wraps a RexTree action path to provide event-like add/remove listener functionality with a single argument.
/// </summary>
/// <typeparam name="T">The type of the action argument.</typeparam>
public class RexActionWrapperEvent<T> : IRexEvent<T>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionWrapperEvent(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionWrapperEvent(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T}"/> class using a RexAction.
    /// </summary>
    /// <param name="action">The RexAction to wrap.</param>
    public RexActionWrapperEvent(RexAction<T> action)
    {
        _model = action.Tree;
        _path = action.Path;
    }

    /// <summary>
    /// Adds a listener that will be invoked when the action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action argument.</param>
    public void AddListener(Action<T> action)
    {
        _model.AddActionListener(_path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public void RemoveListener(Action<T> action)
    {
        _model.RemoveListener(_path, action);
    }
}

/// <summary>
/// Wraps a RexTree action path to provide event-like add/remove listener functionality with two arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public class RexActionWrapperEvent<T1, T2> : IRexEvent<T1, T2>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionWrapperEvent(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionWrapperEvent(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2}"/> class using a RexAction.
    /// </summary>
    /// <param name="action">The RexAction to wrap.</param>
    public RexActionWrapperEvent(RexAction<T1, T2> action)
    {
        _model = action.Tree;
        _path = action.Path;
    }

    /// <summary>
    /// Adds a listener that will be invoked when the action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    public void AddListener(Action<T1, T2> action)
    {
        _model.AddActionListener(_path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public void RemoveListener(Action<T1, T2> action)
    {
        _model.RemoveListener(_path, action);
    }
}

/// <summary>
/// Wraps a RexTree action path to provide event-like add/remove listener functionality with three arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public class RexActionWrapperEvent<T1, T2, T3> : IRexEvent<T1, T2, T3>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionWrapperEvent(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionWrapperEvent(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3}"/> class using a RexAction.
    /// </summary>
    /// <param name="action">The RexAction to wrap.</param>
    public RexActionWrapperEvent(RexAction<T1, T2, T3> action)
    {
        _model = action.Tree;
        _path = action.Path;
    }

    /// <summary>
    /// Adds a listener that will be invoked when the action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    public void AddListener(Action<T1, T2, T3> action)
    {
        _model.AddActionListener(_path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public void RemoveListener(Action<T1, T2, T3> action)
    {
        _model.RemoveListener(_path, action);
    }
}

/// <summary>
/// Wraps a RexTree action path to provide event-like add/remove listener functionality with four arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public class RexActionWrapperEvent<T1, T2, T3, T4> : IRexEvent<T1, T2, T3, T4>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionWrapperEvent(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3, T4}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this event.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionWrapperEvent(RexTree model, RexActionDefine define)
    {
        _model = model;
        _path = define.Path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionWrapperEvent{T1, T2, T3, T4}"/> class using a RexAction.
    /// </summary>
    /// <param name="action">The RexAction to wrap.</param>
    public RexActionWrapperEvent(RexAction<T1, T2, T3, T4> action)
    {
        _model = action.Tree;
        _path = action.Path;
    }

    /// <summary>
    /// Adds a listener that will be invoked when the action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with the action arguments.</param>
    public void AddListener(Action<T1, T2, T3, T4> action)
    {
        _model.AddActionListener(_path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public void RemoveListener(Action<T1, T2, T3, T4> action)
    {
        _model.RemoveListener(_path, action);
    }
}
