using System.Collections;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// An empty dictionary that contains no items.
/// </summary>
public sealed class EmptyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    public static readonly EmptyDictionary<TKey, TValue> Empty = new();

    private EmptyDictionary()
    { }

    public int Count => 0;
    public bool IsReadOnly => true;
    public ICollection<TKey> Keys => [];
    public ICollection<TValue> Values => [];

    public TValue this[TKey key]
    {
        get => default;
        set { } 
    }

    public void Add(TKey key, TValue value)
    { }

    public void Add(KeyValuePair<TKey, TValue> item)
    { }

    public void Clear()
    { }

    public bool Contains(KeyValuePair<TKey, TValue> item) => false;

    public bool ContainsKey(TKey key) => false;

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    { }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        IEnumerable<KeyValuePair<TKey, TValue>> e = [];
        return e.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable<KeyValuePair<TKey, TValue>> e = [];
        return e.GetEnumerator();
    }

    public bool Remove(TKey key) => false;

    public bool Remove(KeyValuePair<TKey, TValue> item) => false;

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default;

        return false;
    }
}