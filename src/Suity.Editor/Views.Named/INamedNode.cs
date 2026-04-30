using Suity.Synchonizing.Core;
using System.Collections.Generic;

namespace Suity.Views.Named;

/// <summary>
/// Interface for a named node that can contain child named items in a hierarchical structure.
/// </summary>
public interface INamedNode : INamed
{
    /// <summary>
    /// Gets the parent node of this node.
    /// </summary>
    INamedNode ParentNode { get; }

    /// <summary>
    /// Gets the count of direct child items.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the direct child items of this node.
    /// </summary>
    IEnumerable<NamedItem> Items { get; }

    /// <summary>
    /// Gets all child items of this node, including nested items.
    /// </summary>
    IEnumerable<NamedItem> AllItems { get; }

    /// <summary>
    /// Gets all child items of this node sorted by name.
    /// </summary>
    IEnumerable<NamedItem> AllItemsSorted { get; }

    /// <summary>
    /// Gets a child item by name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    NamedItem GetItem(string name);

    /// <summary>
    /// Gets a child item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    NamedItem GetItemAt(int index);

    /// <summary>
    /// Adds a child item to this node.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void AddItem(NamedItem item);

    /// <summary>
    /// Removes a child item from this node.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    bool RemoveItem(NamedItem item);

    /// <summary>
    /// Removes all child items from this node.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the synchronization path for this node.
    /// </summary>
    /// <returns>The synchronization path.</returns>
    SyncPath GetPath();
}
