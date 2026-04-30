using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

#region Op

/// <summary>
/// Defines operations for a multiple item that can hold multiple values of the same type.
/// </summary>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public interface IMultipleItemOp<TValue> : IMultipleItem<TValue>
    where TValue : class
{
    /// <summary>
    /// Adds a value to this item.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the value was added; otherwise, false.</returns>
    bool Add(TValue value);

    /// <summary>
    /// Removes a value from this item.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the value was removed; otherwise, false.</returns>
    bool Remove(TValue value);

    /// <summary>
    /// Notifies that a value has been updated.
    /// </summary>
    /// <param name="value">The updated value.</param>
    void Update(TValue value);
}

/// <summary>
/// Defines keyed operations for a multiple item with a key and multiple values.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public interface IMultipleItemOp<TKey, TValue> : IMultipleItem<TKey, TValue>, IMultipleItemOp<TValue>
    where TValue : class
{
}

/// <summary>
/// Defines named operations for a multiple item with a name and multiple values.
/// </summary>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public interface INamedMultipleItemOp<TValue> : INamedMultipleItem<TValue>, IMultipleItemOp<TValue>
    where TValue : class
{
}

#endregion

#region MultipleItem<TValue>

/// <summary>
/// Represents a container that can hold multiple values of the same type, with thread-safe operations.
/// </summary>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public class MultipleItem<TValue> : IMultipleItemOp<TValue>
    where TValue : class
{
    /// <summary>
    /// A concurrent pool of HashSet instances to reduce allocations.
    /// </summary>
    private static readonly ConcurrentPool<HashSet<TValue>> _hashPool = new(() => []);

    /// <summary>
    /// The primary value.
    /// </summary>
    private TValue _value;

    /// <summary>
    /// The set of additional values when multiple values are present.
    /// </summary>
    private HashSet<TValue> _multiple;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleItem{TValue}"/> class.
    /// </summary>
    public MultipleItem()
    {
    }

    /// <summary>
    /// Gets the primary value.
    /// </summary>
    public TValue Value => _value;

    /// <summary>
    /// Gets the first value that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to test values against.</param>
    /// <returns>The matching value, or null if none found.</returns>
    public TValue GetValue(Func<TValue, bool> predicate)
    {
        lock (this)
        {
            if (_multiple != null)
            {
                return _multiple.FirstOrDefault(predicate);
            }
            else if (_value != null && predicate(_value))
            {
                return _value;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets all values contained in this item as an array.
    /// </summary>
    public TValue[] Values
    {
        get
        {
            lock (this)
            {
                if (_multiple != null)
                {
                    return [.. _multiple];
                }
                else if (_value != null)
                {
                    return [_value];
                }
                else
                {
                    return [];
                }
            }
        }
    }

    /// <summary>
    /// Gets the number of values contained in this item.
    /// </summary>
    public int Count
    {
        get
        {
            lock (this)
            {
                return _multiple?.Count ?? (_value != null ? 1 : 0);
            }
        }
    }

    /// <inheritdoc/>
    public bool Add(TValue value)
    {
        if (value is null)
        {
            return false;
        }

        bool added = false;
        bool multiple = false;

        lock (this)
        {
            if (ReferenceEquals(_value, value))
            {
                return false;
            }

            if (_value is null)
            {
                _value = value;
                added = true;
            }
            else
            {

                if (_multiple is null)
                {
                    _multiple = _hashPool.Acquire();
                    _multiple.Add(_value);
                }

                if (_multiple.Add(value))
                {
                    added = true;
                    if (_multiple.Count > 1)
                    {
                        multiple = true;
                    }
                }
            }

            if (added)
            {
                OnAdded(value);
                if (multiple)
                {
                    OnMultipleValue(value);
                }
            }
        }

        return added;
    }

    /// <inheritdoc/>
    public bool Remove(TValue value)
    {
        if (value is null)
        {
            return false;
        }


        bool removed = false;

        lock (this)
        {
            if (_multiple != null)
            {
                removed = _multiple.Remove(value);
                if (_multiple.Count == 0)
                {
                    var r = _multiple;
                    _multiple = null;
                    _hashPool.Release(r);
                }
            }

            if (ReferenceEquals(_value, value))
            {
                removed = true;

                if (_multiple?.Count > 0)
                {
                    _value = _multiple.FirstOrDefault();

                    // Thread safety issue occurs here, so locking is needed
                    if (_multiple.Count == 1)
                    {
                        var r = _multiple;
                        _multiple = null;
                        r.Clear();
                        _hashPool.Release(r);
                    }
                }
                else
                {
                    _value = null;
                }
            }
        }

        if (removed)
        {
            OnRemoved(value);
        }

        return removed;
    }

    public bool Contains(TValue value)
    {
        if (ReferenceEquals(_value, value))
        {
            return true;
        }

        lock (this)
        {
            return _multiple?.Contains(value) ?? false;
        }
    }

    public void Update(TValue value)
    {
    }

    protected virtual void OnAdded(TValue value)
    { }

    protected virtual void OnRemoved(TValue value)
    { }

    protected virtual void OnMultipleValue(TValue value)
    { }
}

#endregion

#region NamedMultipleItem<TValue>

/// <summary>
/// A named multiple item that associates a name with a collection of values.
/// </summary>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public class NamedMultipleItem<TValue> : MultipleItem<TValue>, INamedMultipleItemOp<TValue>
    where TValue : class
{
    /// <summary>
    /// Gets the name of this item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedMultipleItem{TValue}"/> class.
    /// </summary>
    /// <param name="name">The name for this item. Cannot be null.</param>
    public NamedMultipleItem(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }
}

#endregion

#region MultipleItem<TKey, TValue>

/// <summary>
/// A keyed multiple item that associates a key with a collection of values,
/// notifying its owning collection when items change.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of values held by this item.</typeparam>
public class MultipleItem<TKey, TValue> : IMultipleItemOp<TKey, TValue>
    where TValue : class
{
    private readonly TKey _key;
    /// <summary>
    /// The owning collection that receives notifications when this item changes.
    /// </summary>
    internal MultipleItemCollection<TKey, TValue> _collection;

    private readonly MultipleItem<TValue> _item = new();

    /// <summary>
    /// Gets the key associated with this item.
    /// </summary>
    public TKey Key => _key;
    /// <summary>
    /// Gets the primary value in this item.
    /// </summary>
    public TValue Value => _item.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleItem{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="key">The key for this item.</param>
    /// <param name="collection">The owning collection.</param>
    internal MultipleItem(TKey key, MultipleItemCollection<TKey, TValue> collection)
    {
        _key = key;
        _collection = collection;
    }

    /// <summary>
    /// Gets the number of values in this item.
    /// </summary>
    public int Count => _item.Count;
    /// <summary>
    /// Gets all values in this item as an array.
    /// </summary>
    public TValue[] Values => _item.Values;

    /// <inheritdoc/>
    public bool Add(TValue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        bool added = false;
        var collection = _collection;
        if (collection is null)
        {
            return false;
        }

        lock (collection)
        {
            added = _item.Add(value);
        }

        if (added)
        {
            collection.InternalNotifyItemAdded(this, value);
        }

        return added;
    }

    /// <inheritdoc/>
    public bool Remove(TValue value)
    {
        if (value is null)
        {
            return false;
        }

        bool removed = false;
        var collection = _collection;
        if (collection is null)
        {
            return false;
        }

        lock (collection)
        {
            removed = _item.Remove(value);

            if (removed && _item.Value is null)
            {
                collection._assets.Remove(_key);
                _collection = null;
            }
        }

        if (removed)
        {
            collection.InternalNotifyItemRemoved(this, value);
        }

        return removed;
    }

    /// <summary>
    /// Determines whether this item contains the specified value.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the value is present; otherwise, false.</returns>
    public bool Contains(TValue value)
    {
        if (value is null)
        {
            return false;
        }

        var collection = _collection;
        if (collection is null)
        {
            return false;
        }

        lock (collection)
        {
            return _item.Contains(value);
        }
    }

    /// <inheritdoc/>
    public void Update(TValue value)
    {
        if (value is null)
        {
            return;
        }

        var collection = _collection;
        if (collection is null)
        {
            return;
        }

        lock (collection)
        {
            if (!_item.Contains(value))
            {
                return;
            }
        }

        collection.InternalNotifyItemUpdated(this, value);
    }
}

#endregion

#region MultipleItemCollection<TKey, TValue>

/// <summary>
/// A keyed collection that manages multiple items, each holding multiple values,
/// with events for add, remove, and update notifications.
/// </summary>
/// <typeparam name="TKey">The type of keys.</typeparam>
/// <typeparam name="TValue">The type of values held by items.</typeparam>
public class MultipleItemCollection<TKey, TValue> where TValue : class
{
    /// <summary>
    /// The dictionary of keyed items. Internal for access by <see cref="MultipleItem{TKey, TValue}"/>.
    /// </summary>
    internal readonly Dictionary<TKey, MultipleItem<TKey, TValue>> _assets;

    /// <summary>
    /// Occurs when a value is added to any item in this collection.
    /// </summary>
    public event Action<TValue> ValueAdded;

    /// <summary>
    /// Occurs when a value is removed from any item in this collection.
    /// </summary>
    public event Action<TValue> ValueRemoved;

    /// <summary>
    /// Occurs when a value is updated in any item in this collection.
    /// </summary>
    public event Action<TValue> ValueUpdated;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleItemCollection{TKey, TValue}"/> class.
    /// </summary>
    public MultipleItemCollection()
    {
        _assets = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleItemCollection{TKey, TValue}"/> class with a custom key comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer for keys.</param>
    public MultipleItemCollection(IEqualityComparer<TKey> comparer)
    {
        _assets = new(comparer);
    }

    /// <summary>
    /// Adds a value to the item associated with the specified key, creating the item if it doesn't exist.
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The multiple item entry for the key.</returns>
    public MultipleItem<TKey, TValue> AddValue(TKey key, TValue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var entry = _assets.GetOrAdd(key, _ => new MultipleItem<TKey, TValue>(key, this));

        entry.Add(value);

        return entry;
    }

    /// <summary>
    /// Removes a value from the item associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the value was removed; otherwise, false.</returns>
    public bool RemoveValue(TKey key, TValue value)
    {
        if (value is null)
        {
            return false;
        }

        return _assets.GetValueSafe(key)?.Remove(value) ?? false;
    }

    /// <summary>
    /// Gets the primary value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The primary value, or null if not found.</returns>
    public TValue GetValue(TKey key)
    {
        return _assets.GetValueSafe(key)?.Value;
    }

    /// <summary>
    /// Gets the number of keyed items in this collection.
    /// </summary>
    public int Count => _assets.Count;

    /// <summary>
    /// Notifies listeners that a value was added to an item.
    /// </summary>
    /// <param name="item">The item that received the value.</param>
    /// <param name="value">The added value.</param>
    internal void InternalNotifyItemAdded(MultipleItem<TKey, TValue> item, TValue value)
    {
        ValueAdded?.Invoke(value);
    }

    /// <summary>
    /// Notifies listeners that a value was removed from an item.
    /// </summary>
    /// <param name="item">The item that lost the value.</param>
    /// <param name="value">The removed value.</param>
    internal void InternalNotifyItemRemoved(MultipleItem<TKey, TValue> item, TValue value)
    {
        ValueRemoved?.Invoke(value);
    }

    /// <summary>
    /// Notifies listeners that a value was updated in an item.
    /// </summary>
    /// <param name="item">The item that contains the updated value.</param>
    /// <param name="value">The updated value.</param>
    internal void InternalNotifyItemUpdated(MultipleItem<TKey, TValue> item, TValue value)
    {
        ValueUpdated?.Invoke(value);
    }
}

#endregion

#region MultipleItemRegHandle<TValue>

/// <summary>
/// A registry handle that wraps a single multiple item and value, supporting update and dispose operations.
/// </summary>
/// <typeparam name="TValue">The type of value held by this handle.</typeparam>
public class MultipleItemRegHandle<TValue> : IRegistryHandle<TValue>
    where TValue : class
{
    private IMultipleItemOp<TValue> _item;
    private TValue _value;

    /// <summary>
    /// Gets the multiple item associated with this handle.
    /// </summary>
    public IMultipleItemOp<TValue> Item => _item;

    /// <summary>
    /// Gets the value associated with this handle.
    /// </summary>
    public TValue Value => _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleItemRegHandle{TValue}"/> class.
    /// </summary>
    /// <param name="item">The multiple item to wrap. Cannot be null.</param>
    /// <param name="value">The value to wrap. Cannot be null.</param>
    public MultipleItemRegHandle(IMultipleItemOp<TValue> item, TValue value)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Notifies the item that the value has been updated.
    /// </summary>
    public void Update()
    {
        _item.Update(Value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _item.Remove(Value);
        _item = null;
        _value = null;
    }
}

#endregion

#region EntryChainRegHandle<TValue>

/// <summary>
/// A registry handle that manages a chain of multiple item entries,
/// applying operations across all entries in the chain.
/// </summary>
/// <typeparam name="TValue">The type of value held by this handle.</typeparam>
public class EntryChainRegHandle<TValue> : IRegistryHandle<TValue>
    where TValue : class
{
    private readonly TValue _type;
    private readonly List<IMultipleItemOp<TValue>> _entryTrain = new List<IMultipleItemOp<TValue>>();

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryChainRegHandle{TValue}"/> class.
    /// </summary>
    /// <param name="type">The value to manage across entries. Cannot be null.</param>
    public EntryChainRegHandle(TValue type)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Adds an entry to the chain.
    /// </summary>
    /// <param name="entry">The entry to add. Cannot be null.</param>
    public void AddEntry(IMultipleItemOp<TValue> entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _entryTrain.Add(entry);
    }

    /// <summary>
    /// Notifies all entries in the chain that the value has been updated.
    /// </summary>
    public void Update()
    {
        foreach (var entry in _entryTrain)
        {
            entry.Update(_type);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var entry in _entryTrain)
        {
            entry.Remove(_type);
        }

        _entryTrain.Clear();
    }
}

#endregion
