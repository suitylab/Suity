using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Interface for operations on virtual node lists, such as creating, adding, and removing items.
/// </summary>
public interface IVirtualNodeListOperation
{
    /// <summary>
    /// Displays a UI for creating a new list item.
    /// </summary>
    /// <param name="typeHint">Optional type hint for the item to create.</param>
    /// <returns>The created item, or null if creation was cancelled.</returns>
    Task<object> GuiCreateItemAsync(Type typeHint = null);

    /// <summary>
    /// Displays a UI for creating multiple list items.
    /// </summary>
    /// <param name="typeHint">Optional type hint for the items to create.</param>
    /// <returns>An array of created items.</returns>
    Task<object[]> GuiCreateItemsAsync(Type typeHint = null);

    /// <summary>
    /// Checks whether an item can be added to the list.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the item can be added, false otherwise.</returns>
    bool CanAddItem(object value);

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Inserts a value as a list item at the specified index.
    /// </summary>
    /// <param name="index">The index to insert at.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="config">Whether to configure the new node after insertion.</param>
    /// <returns>The created virtual node.</returns>
    VirtualNode InsertListItem(int index, object value, bool config);

    /// <summary>
    /// Removes a list item represented by a virtual node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    void RemoveListItem(VirtualNode node);

    /// <summary>
    /// Removes multiple list items represented by virtual nodes.
    /// </summary>
    /// <param name="nodes">The nodes to remove.</param>
    void RemoveListItems(IEnumerable<VirtualNode> nodes);
}