using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines a producer that creates instances of a specific type.
/// </summary>
/// <typeparam name="T">The type of product to produce.</typeparam>
public interface IRexProducer<T>
{
    /// <summary>
    /// Produces an instance of the specified type.
    /// </summary>
    /// <param name="name">The name identifier for the production.</param>
    /// <returns>The produced instance, or null if not available.</returns>
    T Produce(string name);
}

/// <summary>
/// Delegate for producing instances of a specific type.
/// </summary>
/// <typeparam name="T">The type of product to produce.</typeparam>
/// <param name="name">The name identifier for the production.</param>
/// <returns>The produced instance.</returns>
public delegate T RexProduceDelegate<T>(string name);

/// <summary>
/// A producer implementation that wraps a produce delegate.
/// </summary>
/// <typeparam name="T">The type of product to produce.</typeparam>
public class RexProducer<T> : IRexProducer<T>
{
    private readonly RexProduceDelegate<T> _produce;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexProducer{T}"/> class.
    /// </summary>
    /// <param name="produce">The produce delegate.</param>
    public RexProducer(RexProduceDelegate<T> produce)
    {
        _produce = produce ?? throw new ArgumentNullException(nameof(produce));
    }

    /// <inheritdoc/>
    public T Produce(string name)
    {
        return _produce(name);
    }
}