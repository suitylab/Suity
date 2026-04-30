using System;

namespace Suity;

/// <summary>
/// Represents an action that can be executed and then automatically disposed.
/// Provides a convenient way to execute cleanup code when the object is disposed.
/// </summary>
public class DisposableAction(Action action) : IDisposable
{
    private Action _action = action;

    public void Dispose()
    {
        var action = _action;
        _action = null;

        action?.Invoke();
    }
}