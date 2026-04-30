using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Selecting;

/// <summary>
/// A selection list that stores selection items in a dictionary and implements <see cref="ISelectionList"/>.
/// </summary>
public class SelectionList : MarshalByRefObject, ISelectionList
{
    private readonly Dictionary<string, ISelectionItem> _items = [];

    /// <summary>
    /// Initializes a new empty instance of <see cref="SelectionList"/>.
    /// </summary>
    public SelectionList()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList"/> with a single selection item.
    /// </summary>
    /// <param name="item">The selection item to add.</param>
    public SelectionList(ISelectionItem item)
    {
        _items.Add(item.SelectionKey, item);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList"/> with a collection of selection items.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    public SelectionList(IEnumerable<ISelectionItem> items)
    {
        foreach (var item in items)
        {
            _items[item.SelectionKey] = item;
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList"/> with an array of selection items.
    /// </summary>
    /// <param name="items">The array of selection items to add.</param>
    public SelectionList(params ISelectionItem[] items)
    {
        foreach (var item in items)
        {
            _items[item.SelectionKey] = item;
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList"/> with a collection of string keys.
    /// Each key is converted to a <see cref="SelectionItem"/>.
    /// </summary>
    /// <param name="items">The collection of string keys to add.</param>
    public SelectionList(IEnumerable<string> items)
    {
        foreach (var str in items)
        {
            _items.Add(str, new SelectionItem(str));
        }
    }

    /// <summary>
    /// Removes all items from this selection list.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Adds a selection item to this list if it doesn't already exist.
    /// </summary>
    /// <param name="item">The selection item to add.</param>
    public void Add(ISelectionItem item)
    {
        if (!_items.ContainsKey(item.SelectionKey))
        {
            _items.Add(item.SelectionKey, item);
        }
    }

    /// <summary>
    /// Adds a range of selection items to this list, skipping null items and duplicates.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    public void AddRange(IEnumerable<ISelectionItem> items)
    {
        foreach (var item in items.SkipNull())
        {
            if (!_items.ContainsKey(item.SelectionKey))
            {
                _items.Add(item.SelectionKey, item);
            }
        }
    }

    /// <summary>
    /// Adds a string key as a new selection item to this list.
    /// </summary>
    /// <param name="str">The string key to add.</param>
    public void Add(string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        _items.Add(str, new SelectionItem(str));
    }

    /// <summary>
    /// Adds a range of string keys as selection items to this list.
    /// </summary>
    /// <param name="items">The collection of string keys to add.</param>
    public void AddRange(IEnumerable<string> items)
    {
        foreach (var str in items.SkipNull())
        {
            _items.Add(str, new SelectionItem(str));
        }
    }

    #region ISelectionList

    /// <summary>
    /// Gets a selection item by its key.
    /// </summary>
    /// <param name="key">The key of the selection item to retrieve.</param>
    /// <returns>The selection item with the specified key, or null if not found.</returns>
    public ISelectionItem GetItem(string key)
    {
        return _items.GetValueSafe(key);
    }

    /// <summary>
    /// Gets all selection items in this list.
    /// </summary>
    public IEnumerable<ISelectionItem> GetItems()
    {
        return _items.Values;
    }

    #endregion
}

/// <summary>
/// A generic selection list that stores typed selection items and implements <see cref="ISelectionList"/>.
/// </summary>
public class SelectionList<T> : MarshalByRefObject, ISelectionList
    where T : class, ISelectionItem
{
    private readonly Dictionary<string, T> _items = [];

    /// <summary>
    /// Initializes a new empty instance of <see cref="SelectionList{T}"/>.
    /// </summary>
    public SelectionList()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList{T}"/> with a single selection item.
    /// </summary>
    /// <param name="item">The selection item to add.</param>
    public SelectionList(T item)
    {
        _items.Add(item.SelectionKey, item);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList{T}"/> with a collection of selection items.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    public SelectionList(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            _items[item.SelectionKey] = item;
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionList{T}"/> with an array of selection items.
    /// </summary>
    /// <param name="items">The array of selection items to add.</param>
    public SelectionList(params T[] items)
    {
        foreach (var item in items)
        {
            _items[item.SelectionKey] = item;
        }
    }

    /// <summary>
    /// Removes all items from this selection list.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Adds a selection item to this list if it doesn't already exist.
    /// </summary>
    /// <param name="item">The selection item to add.</param>
    public void Add(T item)
    {
        if (!_items.ContainsKey(item.SelectionKey))
        {
            _items.Add(item.SelectionKey, item);
        }
    }

    /// <summary>
    /// Adds a range of selection items to this list, skipping null items and duplicates.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items.SkipNull())
        {
            if (!_items.ContainsKey(item.SelectionKey))
            {
                _items.Add(item.SelectionKey, item);
            }
        }
    }


    #region ISelectionList

    public ISelectionItem GetItem(string key)
    {
        return _items.GetValueSafe(key);
    }

    public IEnumerable<ISelectionItem> GetItems()
    {
        return _items.Values.OfType<ISelectionItem>();
    }

    #endregion
}