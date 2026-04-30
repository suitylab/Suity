using System;

namespace Suity.Rex;

/// <summary>
/// Wraps an <see cref="IDisposable"/> to execute an additional action when disposed.
/// </summary>
internal class DisposableWrapper : IDisposable
{
    private IDisposable _disposable;
    private Action _action;

    /// <summary>
    /// Initializes a new instance with the specified disposable and action.
    /// </summary>
    /// <param name="disposable">The disposable to wrap.</param>
    /// <param name="action">The action to execute on disposal.</param>
    public DisposableWrapper(IDisposable disposable, Action action)
    {
        _disposable = disposable;
        _action = action;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var dispose = _disposable;
        var action = _action;
        _disposable = null;
        _action = null;

        action?.Invoke();
        _disposable?.Dispose();
    }
}

/// <summary>
/// Wraps an <see cref="IRexHandle"/> to execute an additional action when disposed.
/// </summary>
internal class RexHandleWrapper : IRexHandle
{
    private IRexHandle _handle;
    private Action _disposeAction;

    /// <summary>
    /// Initializes a new instance with the specified handle and dispose action.
    /// </summary>
    /// <param name="handle">The handle to wrap.</param>
    /// <param name="disposeAction">The action to execute on disposal.</param>
    public RexHandleWrapper(IRexHandle handle, Action disposeAction)
    {
        _handle = handle;
        _disposeAction = disposeAction;
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        _handle?.Push();
        return this;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var dispose = _handle;
        var action = _disposeAction;
        _handle = null;
        _disposeAction = null;

        action?.Invoke();
        _handle?.Dispose();
    }
}