using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents a listenable action that can invoke callbacks and return listeners for the response.
/// </summary>
/// <typeparam name="TListen">The type of the response value to listen for.</typeparam>
public sealed class RexActionListenable<TListen> : IRexTreeInstance<ActionArgument<Action<TListen>>>
{
    private readonly RexTree _model;
    private readonly RexPath _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionListenable{TListen}"/> class.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="path">The path within the RexTree.</param>
    public RexActionListenable(RexTree model, RexPath path)
    {
        _model = model;
        _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexActionListenable{TListen}"/> class using an action definition.
    /// </summary>
    /// <param name="model">The RexTree to associate with this action.</param>
    /// <param name="define">The action definition to use.</param>
    public RexActionListenable(RexTree model, RexActionDefine define)
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
    /// Invokes the action immediately and returns a listener for the response.
    /// </summary>
    /// <returns>A listener that will receive the response value.</returns>
    public IRexListener<TListen> Invoke()
    {
        var listener = new ActionListener<TListen>();
        _model.DoAction(_path, new Action<TListen>(listener.HandleCallBack));

        return listener;
    }

    /// <summary>
    /// Invokes the action in a queued manner and returns a listener for the response.
    /// </summary>
    /// <returns>A listener that will receive the response value.</returns>
    public IRexListener<TListen> InvokeQueued()
    {
        var listener = new ActionListener<TListen>();
        _model.DoActionQueued(_path, new Action<TListen>(listener.HandleCallBack));

        return listener;
    }

    /// <summary>
    /// Adds an action listener that will be invoked when this action is dispatched.
    /// </summary>
    /// <param name="action">The callback to invoke with an <see cref="Action{TListen}"/> delegate.</param>
    /// <param name="tag">An optional tag for identifying this listener.</param>
    public void AddActionListener(Action<Action<TListen>> action, string tag = null)
    {
        _model.AddActionListener(_path, action, tag);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _path.ToString();
    }
}
