using System.Collections;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// An empty collection that contains no items.
/// </summary>
public sealed class EmptyCollection<T> : ICollection<T>
{
    public static readonly EmptyCollection<T> Empty = new();

    private EmptyCollection()
    { }

    public int Count => 0;

    public bool IsReadOnly => true;

    public void Add(T item)
    { }

    public void Clear()
    { }

    public bool Contains(T item) => false;

    public void CopyTo(T[] array, int arrayIndex)
    { }

    public IEnumerator<T> GetEnumerator()
    {
        IEnumerable<T> e = [];
        return e.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable<T> e = [];
        return e.GetEnumerator();
    }

    public bool Remove(T item) => false;
}