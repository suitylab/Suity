using Suity.Collections;
using Suity.Drawing;
using Suity.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Selecting;

/// <summary>
/// A simple implementation of <see cref="ISelectionList"/> that stores selection items in a dictionary.
/// </summary>
public class SimpleSelectionList : ISelectionList
{
    private readonly Dictionary<string, ISelectionItem> _items = [];

    /// <summary>
    /// Initializes a new instance of <see cref="SimpleSelectionList"/> with the specified keys.
    /// </summary>
    /// <param name="keys">The collection of keys to create selection items from.</param>
    public SimpleSelectionList(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            _items.Add(key, new SelectionItem(key));
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SimpleSelectionList"/> with the specified selection items.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    public SimpleSelectionList(IEnumerable<ISelectionItem> items)
    {
        foreach (var item in items)
        {
            _items.Add(item.SelectionKey, item);
        }
    }

    #region ISelectionList

    /// <summary>
    /// Gets the title of this selection list.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the icon associated with this selection list.
    /// </summary>
    public ImageDef Icon { get; }

    /// <summary>
    /// Gets all selection items in this list.
    /// </summary>
    /// <returns>An enumerable of all non-null selection items.</returns>
    public IEnumerable<ISelectionItem> GetItems() => _items.Values.Where(f => f != null);

    /// <summary>
    /// Gets a selection item by its key.
    /// </summary>
    /// <param name="key">The key of the selection item to retrieve.</param>
    /// <returns>The selection item with the specified key, or null if not found.</returns>
    public ISelectionItem GetItem(string key) => _items.GetValueSafe(key);

    #endregion

    /// <summary>
    /// Shows an asynchronous selection GUI with the specified items.
    /// </summary>
    /// <param name="items">The collection of selection items to display.</param>
    /// <param name="title">The title of the selection dialog.</param>
    /// <param name="option">Optional configuration for the selection dialog.</param>
    /// <returns>A task representing the asynchronous operation, returning the selection result.</returns>
    public static Task<SelectionResult> ShowSelectionGUIAsync(IEnumerable<ISelectionItem> items, string title, SelectionOption option = null)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        SimpleSelectionList list = new SimpleSelectionList(items);

        return list.ShowSelectionGUIAsync(title, option);
    }
}