using Suity.Synchonizing.Core;
using System.Collections.Generic;

namespace Suity.Views.Named;

/// <summary>
/// Interface for a named item list that supports view operations, text display, and item management.
/// </summary>
public interface INamedItemList : IViewList, ITextDisplay, IEnumerable<NamedItem>
{
    /// <summary>
    /// Gets or sets the root collection this list belongs to.
    /// </summary>
    NamedRootCollection Root { get; set; }

    /// <summary>
    /// Gets or sets the parent node that contains this list.
    /// </summary>
    INamedNode ParentNode { get; set; }

    /// <summary>
    /// Gets the zero-based index of the specified item.
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    int IndexOf(NamedItem item);

    /// <summary>
    /// Gets the synchronization path for the specified item.
    /// </summary>
    /// <param name="item">The item to get the path for.</param>
    /// <returns>The synchronization path.</returns>
    SyncPath GetPath(NamedItem item);

    /// <summary>
    /// Adds an item to this list.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(NamedItem item);

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index to insert at.</param>
    /// <param name="item">The item to insert.</param>
    void Insert(int index, NamedItem item);

    /// <summary>
    /// Removes an item from this list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    bool Remove(NamedItem item);

    /// <summary>
    /// Removes all items from this list.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets an item by name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    NamedItem GetItem(string name);

    /// <summary>
    /// Gets an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    NamedItem GetItemAt(int index);

    /// <summary>
    /// Gets all items in this list sorted by name.
    /// </summary>
    IEnumerable<NamedItem> AllItemsSorted { get; }
}
