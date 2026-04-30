using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Base class for all Rex node listeners. Provides common functionality for listener management.
/// </summary>
internal abstract class RexNodeListener : IDisposable
{
    /// <summary>
    /// Gets the listener set that this listener belongs to.
    /// </summary>
    public RexNodeListenerSet Node { get; private set; }

    /// <summary>
    /// Gets or sets an optional tag for identifying this listener.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeListener"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with. Must not be null.</param>
    public RexNodeListener(RexNodeListenerSet node)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>
    /// Invokes the listener with the specified data.
    /// </summary>
    /// <param name="data">The data to pass to the listener.</param>
    public abstract void Invoke(object data);

    /// <summary>
    /// Gets the delegate key used to identify this listener.
    /// </summary>
    /// <returns>The delegate key.</returns>
    public abstract Delegate GetKey();

    /// <summary>
    /// Disposes the listener and removes it from the listener set.
    /// </summary>
    public virtual void Dispose()
    {
        Node.RemoveListener(this);
        Node = null;
    }
}

#region Data Listener

/// <summary>
/// A listener that handles data change notifications for a specific type.
/// </summary>
/// <typeparam name="T">The type of data this listener handles.</typeparam>
internal class RexNodeDataListener<T> : RexNodeListener, IRexHandle
{
    private readonly Action<T> _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeDataListener{T}"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when data changes.</param>
    public RexNodeDataListener(RexNodeListenerSet node, Action<T> callBack)
        : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is T t)
        {
            _callBack(t);
        }
        else if (data == null)
        {
            if (typeof(T).IsClass)
            {
                _callBack(default);
            }
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }

    /// <summary>
    /// Pushes the current data value to the callback immediately.
    /// </summary>
    /// <returns>This listener instance.</returns>
    public IRexHandle Push()
    {
        T data = Node._model.GetData<T>(Node._path);
        _callBack(data);
        return this;
    }
}

#endregion

#region Action Listener

/// <summary>
/// A listener that handles action notifications without arguments.
/// </summary>
internal class RexNodeActionListener : RexNodeListener
{
    private readonly Action _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeActionListener"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    public RexNodeActionListener(RexNodeListenerSet node, Action callBack) : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is ActionArgument)
        {
            _callBack();
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }
}

/// <summary>
/// A listener that handles action notifications with a single argument.
/// </summary>
/// <typeparam name="T">The type of the action argument.</typeparam>
internal class RexNodeActionListener<T> : RexNodeListener
{
    private readonly Action<T> _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeActionListener{T}"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    public RexNodeActionListener(RexNodeListenerSet node, Action<T> callBack) : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is ActionArgument<T> arg)
        {
            _callBack(arg.Arg1);
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }
}

/// <summary>
/// A listener that handles action notifications with two arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
internal class RexNodeActionListener<T1, T2> : RexNodeListener
{
    private readonly Action<T1, T2> _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeActionListener{T1, T2}"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    public RexNodeActionListener(RexNodeListenerSet node, Action<T1, T2> callBack) : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is ActionArgument<T1, T2> arg)
        {
            _callBack(arg.Arg1, arg.Arg2);
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }
}

/// <summary>
/// A listener that handles action notifications with three arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
internal class RexNodeActionListener<T1, T2, T3> : RexNodeListener
{
    private readonly Action<T1, T2, T3> _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeActionListener{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    public RexNodeActionListener(RexNodeListenerSet node, Action<T1, T2, T3> callBack) : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is ActionArgument<T1, T2, T3> actionArgument)
        {
            var arg = actionArgument;
            _callBack(arg.Arg1, arg.Arg2, arg.Arg3);
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }
}

/// <summary>
/// A listener that handles action notifications with four arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
internal class RexNodeActionListener<T1, T2, T3, T4> : RexNodeListener
{
    private readonly Action<T1, T2, T3, T4> _callBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeActionListener{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="callBack">The callback to invoke when the action is dispatched.</param>
    public RexNodeActionListener(RexNodeListenerSet node, Action<T1, T2, T3, T4> callBack) : base(node)
    {
        _callBack = callBack;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        if (data is ActionArgument<T1, T2, T3, T4> actionArgument)
        {
            var arg = actionArgument;
            _callBack(arg.Arg1, arg.Arg2, arg.Arg3, arg.Arg4);
        }
    }

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _callBack;
    }
}

#endregion

#region Mapping Listener

/// <summary>
/// A listener that maps data changes from one path to trigger updates on another path.
/// </summary>
internal class RexNodeMappingListener : RexNodeListener
{
    private readonly Action<object> _action;
    private readonly RexPath _pathTo;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexNodeMappingListener"/> class.
    /// </summary>
    /// <param name="node">The listener set to associate with.</param>
    /// <param name="pathTo">The target path to update when this listener is invoked.</param>
    public RexNodeMappingListener(RexNodeListenerSet node, RexPath pathTo) : base(node)
    {
        _pathTo = pathTo ?? throw new ArgumentNullException();
        _action = new Action<object>(Invoke);
    }

    /// <summary>
    /// Gets the target path that will be updated when this listener is invoked.
    /// </summary>
    public RexPath PathTo => _pathTo;

    /// <inheritdoc/>
    public override Delegate GetKey()
    {
        return _action;
    }

    /// <inheritdoc/>
    public override void Invoke(object data)
    {
        Node._model.UpdateData(_pathTo);
    }
}

#endregion
