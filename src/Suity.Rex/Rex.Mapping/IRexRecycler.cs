using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines a recycler that disposes or returns produced instances.
/// </summary>
/// <typeparam name="T">The type of product to recycle.</typeparam>
public interface IRexRecycler<T>
{
    /// <summary>
    /// Recycles a produced instance.
    /// </summary>
    /// <param name="name">The name identifier for the recycling.</param>
    /// <param name="product">The product instance to recycle.</param>
    /// <returns>True if the recycling was successful, false otherwise.</returns>
    bool Recycle(string name, T product);
}

/// <summary>
/// Delegate for recycling produced instances.
/// </summary>
/// <typeparam name="T">The type of product to recycle.</typeparam>
/// <param name="name">The name identifier for the recycling.</param>
/// <param name="product">The product instance to recycle.</param>
/// <returns>True if recycled successfully, false otherwise.</returns>
public delegate bool RexRecycleDelegate<T>(string name, T product);

/// <summary>
/// A recycler implementation that wraps a recycle delegate.
/// </summary>
/// <typeparam name="T">The type of product to recycle.</typeparam>
public class RexRecycler<T> : IRexRecycler<T>
{
    private readonly RexRecycleDelegate<T> _recycle;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexRecycler{T}"/> class.
    /// </summary>
    /// <param name="recycle">The recycle delegate.</param>
    public RexRecycler(RexRecycleDelegate<T> recycle)
    {
        _recycle = recycle ?? throw new ArgumentNullException(nameof(recycle));
    }

    /// <inheritdoc/>
    public bool Recycle(string name, T product)
    {
        return _recycle?.Invoke(name, product) ?? false;
    }
}