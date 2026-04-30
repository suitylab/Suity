using System;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// A collection that maintains unique elements while preserving insertion order.
/// Combines a <see cref="List{T}"/> for ordered access and a <see cref="HashSet{T}"/> for fast uniqueness checks.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class UniqueList<T> : IEnumerable<T>, ICollection<T>
{
    private readonly List<T> _list = [];
    private readonly HashSet<T> _hashSet = [];

    /// <summary>
    /// Initializes a new empty instance of the <see cref="UniqueList{T}"/> class.
    /// </summary>
    public UniqueList()
    {
    }

    /// <summary>
    /// Adds an item to the collection if it is not already present.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns><c>true</c> if the item was added; <c>false</c> if the item already existed.</returns>
    public bool Add(T item)
    {
        if (_hashSet.Add(item))
        {
            _list.Add(item);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was removed; <c>false</c> if the item was not found.</returns>
    public bool Remove(T item)
    {
        if (_hashSet.Remove(item))
        {
            _list.Remove(item);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// When setting, throws <see cref="ArgumentException"/> if the new value already exists in the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public T this[int index]
    {
        get
        {
            return _list[index];
        }
        set
        {
            if (_hashSet.Contains(value))
            {
                throw new ArgumentException("Argument is not unique.");
            }

            T current = _list[index];
            _list[index] = value;
            _hashSet.Remove(current);
            _hashSet.Add(value);
        }
    }

    /// <summary>
    /// Inserts an item at the specified index if it is not already present.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    /// <returns><c>true</c> if the item was inserted; <c>false</c> if the item already existed or the index is out of range.</returns>
    public bool Insert(int index, T item)
    {
        if (index >= 0 && index <= _list.Count && _hashSet.Add(item))
        {
            _list.Insert(index, item);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        T current = _list[index];
        _list.RemoveAt(index);
        _hashSet.Remove(current);
    }

    /// <summary>
    /// Removes all elements from the collection.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
        _hashSet.Clear();
    }

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Determines whether the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
    public bool Contains(T item)
    {
        return _hashSet.Contains(item);
    }

    /// <summary>
    /// Searches for the specified item and returns the zero-based index of the first occurrence.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>The zero-based index of the item if found; otherwise, -1.</returns>
    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
        this.Add(item);
    }

    /// <summary>
    /// Copies all elements in the collection to a compatible one-dimensional array, starting at the specified index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }
}
