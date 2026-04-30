using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Selecting;

/// <summary>
/// A selection list that supports multiple child lists and allows retrieving items from any of them.
/// </summary>
public class MultipleSelectionList : ISelectionList
{
    private readonly List<ISelectionList> _childLists = [];

    /// <summary>
    /// Adds a child selection list to this multiple selection list.
    /// </summary>
    /// <param name="list">The selection list to add.</param>
    public void AddList(ISelectionList list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }
        if (list == this)
        {
            return;
        }

        _childLists.Add(list);
    }

    /// <summary>
    /// Gets a selection item by its key, searching through all child lists in order.
    /// </summary>
    /// <param name="key">The key of the selection item to retrieve.</param>
    /// <returns>The first matching selection item found, or null if not found.</returns>
    public ISelectionItem GetItem(string key)
    {
        foreach (var list in _childLists)
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
    public IEnumerable<ISelectionItem> GetItems() => _childLists.SelectMany(o => o.GetItems());
}