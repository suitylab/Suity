using System;

namespace Suity.Editor;

/// <summary>
/// Represents a collection that can hold multiple items of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of values stored in the collection.</typeparam>
public interface IMultipleItem<TValue>
    where TValue : class
{
    /// <summary>
    /// Gets a single value from the collection.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Checks whether the collection contains the specified value.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the collection contains the value; otherwise, false.</returns>
    bool Contains(TValue value);

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets an array of all values in the collection.
    /// </summary>
    TValue[] Values { get; }
}

/// <summary>
/// Represents a collection that can hold multiple items with a key identifier.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of values stored in the collection.</typeparam>
public interface IMultipleItem<TKey, TValue> : IMultipleItem<TValue>
    where TValue : class
{
    /// <summary>
    /// Gets the key associated with this collection.
    /// </summary>
    TKey Key { get; }
}

/// <summary>
/// Represents a collection that can hold multiple items with a name.
/// </summary>
/// <typeparam name="TValue">The type of values stored in the collection.</typeparam>
public interface INamedMultipleItem<TValue> : IMultipleItem<TValue>
    where TValue : class
{
    /// <summary>
    /// Gets the name associated with this collection.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Provides a handle for updating a registered item.
/// </summary>
/// <typeparam name="T">The type of the registered item.</typeparam>
public interface IRegistryHandle<T> : IDisposable
    where T : class
{
    /// <summary>
    /// Updates the registered item.
    /// </summary>
    void Update();
}