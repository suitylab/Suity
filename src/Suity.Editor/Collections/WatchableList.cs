using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// Represents the update mode for watchable list events.
/// </summary>
public enum EventListUpdateMode
{
    Added,
    Removed,
    Changed,
}

/// <summary>
/// A list that notifies listeners when items are added, removed, or changed.
/// </summary>
public class WatchableList<T> : IList<T>, ICollection<T>, IList
{
    private readonly List<T> _list;

    public WatchableList()
    {
        _list = [];
    }

    public WatchableList(IEnumerable<T> items)
    {
        _list = [.. items];
    }

    public event Action<EventListUpdateMode, int, T> Updated;

    #region IList<T>

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        Updated?.Invoke(EventListUpdateMode.Added, index, default);
    }

    public void RemoveAt(int index)
    {
        T item = _list[index];
        _list.RemoveAt(index);
        Updated?.Invoke(EventListUpdateMode.Removed, index, item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            var old = _list[index];
            _list[index] = value;
            Updated?.Invoke(EventListUpdateMode.Changed, index, old);
        }
    }

    public void Add(T item)
    {
        _list.Add(item);
        Updated?.Invoke(EventListUpdateMode.Added, _list.Count - 1, default);
    }

    public void Clear()
    {
        foreach (var item in _list)
        {
            Updated?.Invoke(EventListUpdateMode.Removed, 0, item);
        }
        _list.Clear();
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public bool Remove(T item)
    {
        int index = _list.IndexOf(item);
        if (index > 0)
        {
            _list.RemoveAt(index);
            Updated?.Invoke(EventListUpdateMode.Removed, index, item);
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();

    #endregion

    #region IList

    int IList.Add(object value)
    {
        _list.Add((T)value);
        Updated?.Invoke(EventListUpdateMode.Added, _list.Count - 1, default);

        return _list.Count - 1;
    }

    void IList.Clear()
    {
        this.Clear();
    }

    bool IList.Contains(object value)
    {
        return _list.Contains((T)value);
    }

    int IList.IndexOf(object value)
    {
        return _list.IndexOf((T)value);
    }

    void IList.Insert(int index, object value)
    {
        _list.Insert(index, (T)value);
        Updated?.Invoke(EventListUpdateMode.Added, index, default);
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    void IList.Remove(object value)
    {
        int index = _list.IndexOf((T)value);
        if (index > 0)
        {
            _list.RemoveAt(index);
            Updated?.Invoke(EventListUpdateMode.Removed, index, (T)value);
        }
    }

    void IList.RemoveAt(int index)
    {
        T item = _list[index];
        _list.RemoveAt(index);
        Updated?.Invoke(EventListUpdateMode.Removed, index, item);
    }

    object IList.this[int index]
    {
        get => _list[index];
        set
        {
            var old = _list[index];
            _list[index] = (T)value;
            Updated?.Invoke(EventListUpdateMode.Changed, index, (T)old);
        }
    }

    #endregion

    #region ICollection

    void ICollection.CopyTo(Array array, int index)
    {
        ((IList)_list).CopyTo(array, index);
    }

    int ICollection.Count => _list.Count;

    bool ICollection.IsSynchronized => ((ICollection)_list).IsSynchronized;

    object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

    #endregion

    public int RemoveAll(Predicate<T> match)
    {
        return _list.RemoveAll(match);
    }

    public T Find(Predicate<T> match)
    {
        return _list.Find(match);
    }

    public void Sort(Comparison<T> comparison)
    {
        _list.Sort(comparison);
    }

    public override string ToString()
    {
        return _list.Count.ToString();
    }
}