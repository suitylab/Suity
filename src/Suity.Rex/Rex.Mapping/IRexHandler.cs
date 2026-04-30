using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines a handler that processes values of a specific type.
/// </summary>
/// <typeparam name="T">The type of value to handle.</typeparam>
public interface IRexHandler<T>
{
    /// <summary>
    /// Handles the specified value.
    /// </summary>
    /// <param name="value">The value to handle.</param>
    /// <returns>True if the value was handled successfully, false otherwise.</returns>
    bool Handle(T value);
}

/// <summary>
/// Delegate for handling values of a specific type.
/// </summary>
/// <typeparam name="T">The type of value to handle.</typeparam>
/// <param name="value">The value to handle.</param>
/// <returns>True if handled successfully, false otherwise.</returns>
public delegate bool RexHandleDelegate<T>(T value);

/// <summary>
/// A handler implementation that wraps a handle delegate.
/// </summary>
/// <typeparam name="T">The type of value to handle.</typeparam>
public class RexHandler<T> : IRexHandler<T>
{
    private readonly RexHandleDelegate<T> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexHandler{T}"/> class.
    /// </summary>
    /// <param name="handler">The handle delegate.</param>
    public RexHandler(RexHandleDelegate<T> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <inheritdoc/>
    public bool Handle(T value)
    {
        return _handler(value);
    }
}