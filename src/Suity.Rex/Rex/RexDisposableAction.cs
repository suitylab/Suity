using System;

namespace Suity.Rex;

/// <summary>
/// An <see cref="IDisposable"/> that executes a specified action when disposed.
/// </summary>
public class RexDisposableAction(Action action) : IDisposable
{
    private Action _action = action;

    /// <inheritdoc/>
    public void Dispose()
    {
        var action = _action;
        _action = null;

        action?.Invoke();
    }
}