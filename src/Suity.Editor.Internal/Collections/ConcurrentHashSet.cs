using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Collections;

/// <summary>
/// A thread-safe implementation of <see cref="ISet{T}"/> backed by a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
/// <typeparam name="TElement">The type of elements in the set.</typeparam>
public class ConcurrentHashSet<TElement> : ISet<TElement>
{
    private readonly ConcurrentDictionary<TElement, object> _internal;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentHashSet{TElement}"/> class.
    /// </summary>
    /// <param name="elements">An optional collection of elements to initialize the set with.</param>
    public ConcurrentHashSet(IEnumerable<TElement> elements = null)
    {
        _internal = new ConcurrentDictionary<TElement, object>();
        if (elements != null)
            UnionWith(elements);
    }

    /// <summary>
    /// Modifies the current set to contain all elements that are present in either this set or the specified collection.
    /// </summary>
    /// <param name="other">The collection to union with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    public void UnionWith(IEnumerable<TElement> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        foreach (var otherElement in other)
            Add(otherElement);
    }

    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<TElement> other)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Determines whether the current set and a specified collection share common elements.
    /// </summary>
    /// <param name="other">The collection to compare.</param>
    /// <returns><c>true</c> if at least one element is shared; otherwise, <c>false</c>.</returns>
    public bool Overlaps(IEnumerable<TElement> other)
    {
        return other.Any(otherElement => _internal.ContainsKey(otherElement));
    }

    /// <summary>
    /// Determines whether the current set contains exactly the same elements as the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare.</param>
    /// <returns><c>true</c> if the sets are equal; otherwise, <c>false</c>.</returns>
    public bool SetEquals(IEnumerable<TElement> other)
    {
        int otherCount = 0;
        int thisCount = Count;

        foreach (var otherElement in other)
        {
            otherCount++;
            if (!_internal.ContainsKey(otherElement))
                return false;
        }

        return otherCount == thisCount;
    }

    /// <summary>
    /// Adds the specified element to the set.
    /// </summary>
    /// <param name="item">The element to add.</param>
    /// <returns><c>true</c> if the element was added; <c>false</c> if it already existed.</returns>
    public bool Add(TElement item)
    {
        return _internal.TryAdd(item, null);
    }

    /// <summary>
    /// Removes all elements from the set.
    /// </summary>
    public void Clear()
    {
        _internal.Clear();
    }

    /// <summary>
    /// Adds the specified element to the set. This explicit interface implementation delegates to <see cref="Add(TElement)"/>.
    /// </summary>
    /// <param name="item">The element to add.</param>
    void ICollection<TElement>.Add(TElement item)
    {
        Add(item);
    }

    /// <summary>
    /// Determines whether the set contains the specified element.
    /// </summary>
    /// <param name="item">The element to locate.</param>
    /// <returns><c>true</c> if the element is found; otherwise, <c>false</c>.</returns>
    public bool Contains(TElement item)
    {
        return _internal.ContainsKey(item);
    }

    /// <summary>
    /// Copies all elements in the set to a compatible one-dimensional array, starting at the specified index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
    public void CopyTo(TElement[] array, int arrayIndex)
    {
        _internal.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the specified element from the set.
    /// </summary>
    /// <param name="item">The element to remove.</param>
    /// <returns><c>true</c> if the element was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(TElement item)
    {
        return _internal.TryRemove(item, out object ignore);
    }

    /// <summary>
    /// Gets the number of elements in the set.
    /// </summary>
    public int Count => _internal.Count;

    /// <summary>
    /// Gets a value indicating whether the set is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator() => _internal.Keys.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
