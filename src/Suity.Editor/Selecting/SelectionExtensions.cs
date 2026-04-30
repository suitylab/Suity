using Suity.Editor;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Selecting;

/// <summary>
/// Extension methods for working with selection items and lists.
/// </summary>
public static class SelectionExtensions
{
    /// <summary>
    /// Concatenates a selection item to the beginning of a selection list.
    /// </summary>
    /// <param name="item">The selection item to prepend.</param>
    /// <param name="list">The selection list to prepend to.</param>
    /// <returns>A new <see cref="UnionSelectionList"/> containing the item followed by the list items.</returns>
    public static ISelectionList Concat(this ISelectionItem item, ISelectionList list)
    {
        if (list is null)
        {
            return null;
        }

        if (item is null)
        {
            return list;
        }

        if (list is not UnionSelectionList uList)
        {
            uList = new UnionSelectionList(list);
        }

        var newList = new SelectionList(item);
        uList._lists.Insert(0, newList);

        return uList;
    }

    /// <summary>
    /// Concatenates a selection item to the end of a selection list.
    /// </summary>
    /// <param name="list">The selection list to append to.</param>
    /// <param name="item">The selection item to append.</param>
    /// <returns>A new <see cref="UnionSelectionList"/> containing the list items followed by the item.</returns>
    public static ISelectionList Concat(this ISelectionList list, ISelectionItem item)
    {
        if (list is null)
        {
            return null;
        }

        if (item is null)
        {
            return list;
        }

        if (list is not UnionSelectionList uList)
        {
            uList = new UnionSelectionList(list);
        }

        var newList = new SelectionList(item);
        uList._lists.Add(newList);

        return uList;
    }

    /// <summary>
    /// Concatenates a collection of selection items to the end of a selection list.
    /// </summary>
    /// <param name="list">The selection list to append to.</param>
    /// <param name="items">The collection of selection items to append.</param>
    /// <returns>A new <see cref="UnionSelectionList"/> containing the list items followed by the new items.</returns>
    public static ISelectionList Concat(this ISelectionList list, IEnumerable<ISelectionItem> items)
    {
        if (list is null)
        {
            return null;
        }

        if (items is null)
        {
            return list;
        }

        if (list is not UnionSelectionList uList)
        {
            uList = new UnionSelectionList(list);
        }

        var newList = new SelectionList(items);
        uList._lists.Add(newList);

        return uList;
    }

    /// <summary>
    /// Concatenates an array of selection items to the end of a selection list.
    /// </summary>
    /// <param name="list">The selection list to append to.</param>
    /// <param name="items">The array of selection items to append.</param>
    /// <returns>A new <see cref="UnionSelectionList"/> containing the list items followed by the new items.</returns>
    public static ISelectionList Concat(this ISelectionList list, params ISelectionItem[] items)
    {
        if (list is null)
        {
            return null;
        }

        if (items is null)
        {
            return list;
        }

        if (list is not UnionSelectionList uList)
        {
            uList = new UnionSelectionList(list);
        }

        var newList = new SelectionList(items);
        uList._lists.Add(newList);

        return uList;
    }

    /// <summary>
    /// Shows a selection dialog to create an object of a type derived from the specified base type.
    /// </summary>
    /// <param name="baseType">The base type to find derived types from.</param>
    /// <param name="title">The title of the selection dialog.</param>
    /// <returns>A tuple containing a success flag and the created object, or (false, null) if failed.</returns>
    public static async Task<(bool isSuccess, object createdObject)> GuiCreateObjectAsync(this Type baseType, string title)
    {
        if (baseType is null)
        {
            return (false, null);
        }

        var selectionList = new SelectionList();

        foreach (var type in baseType.GetDerivedTypes().Where(o => !o.IsAbstract))
        {
            selectionList.Add(new TypedSelectionItem(type));
        }

        var result = await selectionList.ShowSelectionGUIAsync(title);

        if (!result.IsSuccess)
        {
            return (false, null);
        }

        if (result.Item is null)
        {
            return (true, null);
        }
        
        if (result.Item is not TypedSelectionItem typedItem)
        {
            return (false, null);
        }

        try
        {
            var obj = typedItem.Type.CreateInstanceOf();
            return (true, obj);
        }
        catch (Exception)
        {
        }
        
        return (false, null);
    }
}

/// <summary>
/// A selection list that combines multiple child selection lists into a unified view.
/// </summary>
public class UnionSelectionList : ISelectionList
{
    internal readonly List<ISelectionList> _lists = [];

    /// <summary>
    /// Initializes a new instance of <see cref="UnionSelectionList"/> with a single child list.
    /// </summary>
    /// <param name="list">The child selection list to add.</param>
    public UnionSelectionList(ISelectionList list)
    {
        _lists.Add(list);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UnionSelectionList"/> with a collection of child lists.
    /// </summary>
    /// <param name="lists">The collection of child selection lists to add.</param>
    public UnionSelectionList(IEnumerable<ISelectionList> lists)
    {
        _lists.AddRange(lists);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UnionSelectionList"/> with an array of child lists.
    /// </summary>
    /// <param name="lists">The array of child selection lists to add.</param>
    public UnionSelectionList(params ISelectionList[] lists)
    {
        _lists.AddRange(lists);
    }

    /// <summary>
    /// Adds a child selection list to this union.
    /// </summary>
    /// <param name="list">The selection list to add.</param>
    public void AddList(ISelectionList list)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        _lists.Add(list);
    }

    /// <summary>
    /// Gets a selection item by its key, searching through all child lists in order.
    /// </summary>
    /// <param name="key">The key of the selection item to retrieve.</param>
    /// <returns>The first matching selection item found, or null if not found.</returns>
    public ISelectionItem GetItem(string key)
    {
        foreach (var list in _lists)
        {
            var item = list.GetItem(key);
            if (item != null)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all selection items from all child lists.
    /// </summary>
    public IEnumerable<ISelectionItem> GetItems()
    {
        return _lists.SelectMany(o => o.GetItems());
    }
}