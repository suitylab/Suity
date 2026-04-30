using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines a reducer that transforms state based on actions.
/// </summary>
/// <typeparam name="T">The type of state to reduce.</typeparam>
public interface IRexReducer<T>
{
    /// <summary>
    /// Reduces the current state to a new state.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="name">The name identifier for the reduction.</param>
    /// <param name="payload">The payload data for the reduction.</param>
    /// <returns>The new reduced state, or null if not handled.</returns>
    T Reduce(T state, string name, object payload);
}

/// <summary>
/// Delegate for reducing state.
/// </summary>
/// <typeparam name="T">The type of state to reduce.</typeparam>
/// <param name="state">The current state.</param>
/// <param name="name">The name identifier for the reduction.</param>
/// <param name="payload">The payload data for the reduction.</param>
/// <returns>The new reduced state.</returns>
public delegate T RexReduceDelegate<T>(T state, string name, object payload);

/// <summary>
/// A reducer implementation that wraps a reduce delegate.
/// </summary>
/// <typeparam name="T">The type of state to reduce.</typeparam>
public class RexReducer<T> : IRexReducer<T>
{
    private readonly RexReduceDelegate<T> _reduce;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexReducer{T}"/> class.
    /// </summary>
    /// <param name="reduce">The reduce delegate.</param>
    public RexReducer(RexReduceDelegate<T> reduce)
    {
        _reduce = reduce ?? throw new ArgumentNullException(nameof(reduce));
    }

    /// <inheritdoc/>
    public T Reduce(T state, string name, object payload)
    {
        return _reduce(state, name, payload);
    }
}