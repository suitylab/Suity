using System;

namespace Suity.Rex;

/// <summary>
/// A simple listener implementation that directly invokes callbacks.
/// </summary>
/// <typeparam name="T">The type of value this listener observes.</typeparam>
internal class ActionListener<T> : IRexListener<T>
{
    internal Action<T> _callBack;

    /// <summary>
    /// Initializes a new empty instance.
    /// </summary>
    public ActionListener()
    {
    }

    /// <inheritdoc/>
    public IRexHandle Subscribe(Action<T> callBack)
    {
        _callBack += callBack;

        return this;
    }

    /// <summary>
    /// Invokes all registered callbacks with the specified result.
    /// </summary>
    /// <param name="result">The result value to emit.</param>
    internal void HandleCallBack(T result)
    {
        _callBack?.Invoke(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _callBack = null;
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        return this;
    }
}