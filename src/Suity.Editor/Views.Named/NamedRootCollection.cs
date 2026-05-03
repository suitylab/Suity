using Suity.Drawing;
using Suity.Editor;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Delegate for creating a new named item instance.
/// </summary>
/// <returns>The newly created <see cref="NamedItem"/>.</returns>
internal delegate NamedItem NamedItemCreate();

/// <summary>
/// Delegate for creating named items through a GUI interaction.
/// </summary>
/// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
internal delegate Task<NamedItem[]> NamedItemGuiCreate();

/// <summary>
/// Delegate for configuring a named item through a GUI interaction.
/// </summary>
/// <param name="item">The item to configure.</param>
/// <returns>True if configuration was successful; otherwise, false.</returns>
internal delegate Task<bool> NamedItemGuiConfig(NamedItem item);

/// <summary>
/// Represents the root collection of named items, serving as the top-level container in the named item hierarchy.
/// </summary>
public class NamedRootCollection :
    INamedNode,
    IViewList,
    ITextDisplay,
    IDescriptionDisplay
{
    internal RootCollectionExternal _ex;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedRootCollection"/> class.
    /// </summary>
    public NamedRootCollection()
    {
        _ex = NamedExternal._external.CreateRootCollectionEx(this);
    }

    /// <summary>
    /// Gets the name of this root collection (always null for root).
    /// </summary>
    public virtual string Name => null;

    /// <summary>
    /// Gets the parent node of this collection (always null for root).
    /// </summary>
    public INamedNode ParentNode => null;

    /// <summary>
    /// Gets the top-level items in this collection.
    /// </summary>
    public IEnumerable<NamedItem> Items => _ex.Items;

    /// <summary>
    /// Gets all items in this collection, including nested items.
    /// </summary>
    public IEnumerable<NamedItem> AllItems => _ex.AllItems;

    /// <summary>
    /// Gets all items in this collection sorted by name.
    /// </summary>
    public IEnumerable<NamedItem> AllItemsSorted => _ex.AllItemsSorted;

    /// <summary>
    /// Gets the description of this root collection.
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// Adds an item to this collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(NamedItem item) => _ex.AddItem(item);

    /// <summary>
    /// Removes all items from this collection.
    /// </summary>
    public void Clear() => _ex.Clear();

    /// <summary>
    /// Checks if an item with the specified name exists in the top-level items.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public bool ContainsItem(string name) => _ex.ContainsItem(name, false);

    /// <summary>
    /// Checks if an item with the specified name exists in the collection.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public bool ContainsItem(string name, bool inAllItems) => _ex.ContainsItem(name, inAllItems);

    /// <summary>
    /// Checks if the specified item exists in the collection.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public bool ContainsItem(NamedItem item, bool inAllItems) => _ex.ContainsItem(item, inAllItems);

    /// <summary>
    /// Gets an item by name from the top-level items.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    public NamedItem GetItem(string name) => _ex.GetItem(name, false);

    /// <summary>
    /// Gets an item by name from all items including nested.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    public NamedItem GetItemAll(string name) => _ex.GetItem(name, true);

    /// <summary>
    /// Gets an item by name from the collection.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    public NamedItem GetItem(string name, bool inAllItems) => _ex.GetItem(name, inAllItems);

    /// <summary>
    /// Gets an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    public NamedItem GetItemAt(int index) => _ex.GetItemAt(index);

    /// <summary>
    /// Removes an item from this collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool RemoveItem(NamedItem item) => _ex.RemoveItem(item);

    /// <summary>
    /// Renames an item in this collection.
    /// </summary>
    /// <param name="item">The item to rename.</param>
    /// <param name="newName">The new name for the item.</param>
    /// <returns>True if the rename was successful; otherwise, false.</returns>
    public bool Rename(NamedItem item, string newName) => _ex.Rename(item, newName);

    /// <summary>
    /// Generates a suggested name for an item based on its prefix rules.
    /// </summary>
    /// <param name="item">The item to generate a name for.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string.</returns>
    public string GetSuggestedName(NamedItem item, int digiLen = 2) => _ex.GetSuggestedName(item, digiLen);

    /// <summary>
    /// Generates a suggested name with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string.</returns>
    public string GetSuggestedName(string prefix, int digiLen = 2) => _ex.GetSuggestedName(prefix, digiLen);

    /// <summary>
    /// Resolves a name conflict by generating a non-conflicting name.
    /// </summary>
    /// <param name="name">The original name that may conflict.</param>
    /// <returns>A non-conflicting name, or the original name if not duplicated.</returns>
    public string ResolveConflictName(string name) => _ex.ResolveConflictName(name);

    /// <summary>
    /// Internal method to add an item without triggering external events.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if the item was added; otherwise, false.</returns>
    internal bool InternalAddItem(NamedItem item) => _ex.InternalAddItem(item);

    /// <summary>
    /// Internal method to create a default item for the parent node.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <returns>The newly created <see cref="NamedItem"/>.</returns>
    internal NamedItem InternalCreateDefaultItem(INamedNode parentNode) => _ex.InternalCreateDefaultItem(parentNode);

    /// <summary>
    /// Internal method to show a GUI for creating items of the specified type.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="type">The type of items to create.</param>
    /// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
    internal Task<NamedItem[]> InternalGuiCreateItems(INamedNode parentNode, Type type) => _ex.InternalGuiCreateItems(parentNode, type);

    /// <summary>
    /// Internal method to remove an item without triggering external events.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    internal bool InternalRemoveItem(NamedItem item) => _ex.InternalRemoveItem(item);

    #region IViewList

    /// <summary>
    /// Gets or sets the list view identifier.
    /// </summary>
    public int ListViewId { get; set; } = ViewIds.TreeView;

    /// <summary>
    /// Gets the count of top-level items in this collection.
    /// </summary>
    public int Count => _ex.Count;

    /// <summary>
    /// Checks if the specified value can be dropped into this collection.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    public bool DropInCheck(object value)
        => _ex.CanDropIn(value);

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    public object DropInConvert(object value)
        => _ex.DropInConvert(value);

    /// <summary>
    /// Synchronizes the collection state.
    /// </summary>
    /// <param name="sync">The index synchronizer.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IIndexSync sync, ISyncContext context)
        => _ex.Sync(sync, context);

    #endregion

    #region ITextDisplay

    /// <summary>
    /// Gets the display icon for this collection.
    /// </summary>
    object ITextDisplay.DisplayIcon => OnGetIcon();

    /// <summary>
    /// Gets the display text for this collection.
    /// </summary>
    string ITextDisplay.DisplayText => OnGetText();

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    TextStatus ITextDisplay.DisplayStatus => OnGetTextStatus();

    #endregion

    #region Virtual

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    public virtual TextStatus OnGetTextStatus() => TextStatus.Normal;

    /// <summary>
    /// Gets the synchronization path for this collection.
    /// </summary>
    public virtual SyncPath GetPath() => SyncPath.Empty;

    /// <summary>
    /// Creates a default item for the specified parent node.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <returns>The newly created <see cref="NamedItem"/>, or null if not supported.</returns>
    protected internal virtual NamedItem OnCreateDefaultItem(INamedNode parentNode) => null;

    /// <summary>
    /// Shows a GUI for creating items of the specified type.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="type">The type of items to create.</param>
    /// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
    protected internal virtual Task<NamedItem[]> OnGuiCreateItems(INamedNode parentNode, Type type) => Task.FromResult<NamedItem[]>(null);

    /// <summary>
    /// Checks if the specified value can be dropped into this collection.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    protected internal virtual bool OnDropInCheck(object value) => true;

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    protected internal virtual object OnDropInConvert(object value) => value;

    /// <summary>
    /// Called when an item in this collection has been renamed.
    /// </summary>
    /// <param name="item">The item that was renamed.</param>
    /// <param name="oldName">The previous name.</param>
    protected internal virtual void OnItemRenamed(NamedItem item, string oldName)
    { }

    /// <summary>
    /// Gets the icon to display for this collection.
    /// </summary>
    protected internal virtual ImageDef OnGetIcon() => CoreIconCache.FolderDesign;

    /// <summary>
    /// Gets the display text for this collection.
    /// </summary>
    protected internal virtual string OnGetText() => null;

    /// <summary>
    /// Called when an item is added to this collection.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    /// <param name="isNew">Whether the item is newly created.</param>
    protected internal virtual void OnItemAdded(NamedItem item, bool isNew)
    { }

    /// <summary>
    /// Called when an item is removed from this collection.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    protected internal virtual void OnItemRemoved(NamedItem item)
    { }

    /// <summary>
    /// Generates a suggested name with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string, or null to use default behavior.</returns>
    protected internal virtual string OnGetSuggestedName(string prefix, int digiLen = 2) => null;

    /// <summary>
    /// Resolves a name conflict by generating a non-conflicting name.
    /// </summary>
    /// <param name="name">The original name that may conflict.</param>
    /// <returns>A non-conflicting name, or null to use default behavior.</returns>
    protected internal virtual string OnResolveConflictName(string name) => null;

    #endregion
}
