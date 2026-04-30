using System;

namespace Suity.Rex;

/// <summary>
/// Represents a reactive listener that can subscribe to value changes of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value this listener observes.</typeparam>
public interface IRexListener<T> : IRexHandle
{
    /// <summary>
    /// Subscribes a callback to be invoked when a new value is emitted.
    /// </summary>
    /// <param name="callBack">The action to execute when a value is received.</param>
    /// <returns>A handle that can be used to manage the subscription lifecycle.</returns>
    IRexHandle Subscribe(Action<T> callBack);
}

/// <summary>
/// A no-op implementation of <see cref="IRexListener{T}"/> that ignores all subscriptions.
/// </summary>
/// <typeparam name="T">The type of value this listener observes.</typeparam>
public class EmptyRexListener<T> : IRexListener<T>
{
    /// <summary>
    /// Gets the singleton empty listener instance.
    /// </summary>
    public static readonly EmptyRexListener<T> Empty = new();

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        return this;
    }

    /// <inheritdoc/>
    public IRexHandle Subscribe(Action<T> callBack)
    {
        return this;
    }
}