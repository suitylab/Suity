using System;
using System.Collections.Concurrent;

namespace Suity.Collections;

[MultiThreadSecurity(MultiThreadSecurityMethods.ConcurrentSecure)]
public class ConcurrentPool<T> where T : class
{
    /// <summary>
    /// The internal stack used to store the pooled objects.
    /// </summary>
    private readonly ConcurrentStack<T> _pool = new();

    /// <summary>
    /// The factory function used to create new objects when the pool is empty.
    /// </summary>
    private readonly Func<T> _factory;

    /// <summary>
    /// Gets or sets the maximum capacity of the pool. If null, the pool has no maximum capacity.
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConcurrentPool class with a factory function.
    /// </summary>
    /// <param name="factory">The factory function used to create new objects.</param>
    /// <exception cref="ArgumentNullException">Thrown when the factory function is null.</exception>
    public ConcurrentPool(Func<T> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Initializes a new instance of the ConcurrentPool class with a factory function and a maximum capacity.
    /// </summary>
    /// <param name="factory">The factory function used to create new objects.</param>
    /// <param name="capacity">The maximum capacity of the pool.</param>
    /// <exception cref="ArgumentNullException">Thrown when the factory function is null.</exception>
    public ConcurrentPool(Func<T> factory, int capacity)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        Capacity = capacity;
    }

    /// <summary>
    /// Acquires an object from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>The acquired object.</returns>
    public T Acquire()
    {
        if (_pool.TryPop(out T value))
        {
            return value;
        }

        return _factory();
    }

    /// <summary>
    /// Releases an object back into the pool.
    /// </summary>
    /// <param name="value">The object to release.</param>
    public void Release(T value)
    {
        if (!Capacity.HasValue || Capacity.Value < _pool.Count)
        {
            _pool.Push(value);
        }
    }

    /// <summary>
    /// Gets the number of objects currently in the pool.
    /// </summary>
    public int Count => _pool.Count;

    /// <summary>
    /// Clears all objects from the pool.
    /// </summary>
    public void Clear() => _pool.Clear();
}
