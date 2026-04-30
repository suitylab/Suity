using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Abstract base class for external named view operations, providing factory methods and text/item manipulation.
/// </summary>
internal abstract class NamedExternal
{
    internal static NamedExternal _external;

    /// <summary>
    /// Creates an external handler for a root collection.
    /// </summary>
    /// <param name="collection">The root collection to create the external handler for.</param>
    /// <returns>A new <see cref="RootCollectionExternal"/> instance.</returns>
    public abstract RootCollectionExternal CreateRootCollectionEx(NamedRootCollection collection);

    /// <summary>
    /// Creates a named item list with drop-in support.
    /// </summary>
    /// <param name="canDropIn">Predicate to determine if an object can be dropped into the list.</param>
    /// <param name="dropInConvert">Function to convert a dropped object into a compatible item.</param>
    /// <returns>A new <see cref="INamedItemList"/> instance.</returns>
    public abstract INamedItemList CreateItemList(Predicate<object> canDropIn, Func<object, object> dropInConvert);

    /// <summary>
    /// Creates a synchronized named list for the specified value type.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the list, must be a class implementing <see cref="ISyncObject"/>.</typeparam>
    /// <param name="nameField">The field name used for identifying items.</param>
    /// <returns>A new <see cref="INamedSyncList{TValue}"/> instance.</returns>
    public abstract INamedSyncList<TValue> CreateNamedSyncList<TValue>(string nameField) where TValue : class, ISyncObject;

    /// <summary>
    /// Creates a render target list for analysis support.
    /// </summary>
    /// <param name="analysis">The analysis support object.</param>
    /// <returns>A new <see cref="INamedRenderTargetList"/> instance.</returns>
    public abstract INamedRenderTargetList CreateRenderTargetList(ISupportAnalysis analysis);

    /// <summary>
    /// Creates a using list from storage locations.
    /// </summary>
    /// <param name="fieldDescription">Description of the field.</param>
    /// <param name="fileNames">Collection of storage locations.</param>
    /// <param name="owner">Optional owner object.</param>
    /// <returns>A new <see cref="INamedUsingList"/> instance.</returns>
    public abstract INamedUsingList CreateUsingList(string fieldDescription, IEnumerable<StorageLocation> fileNames, object owner = null);

    /// <summary>
    /// Creates a using list from GUIDs.
    /// </summary>
    /// <param name="fieldDescription">Description of the field.</param>
    /// <param name="ids">Collection of GUIDs.</param>
    /// <param name="owner">Optional owner object.</param>
    /// <returns>A new <see cref="INamedUsingList"/> instance.</returns>
    public abstract INamedUsingList CreateUsingList(string fieldDescription, IEnumerable<Guid> ids, object owner = null);

    /// <summary>
    /// Sets the text value of a named item.
    /// </summary>
    /// <param name="item">The named item to update.</param>
    /// <param name="text">The new text value.</param>
    /// <param name="setup">The sync context for the operation.</param>
    /// <param name="showNotice">Whether to show a notice to the user.</param>
    public abstract void SetText(NamedItem item, string text, ISyncContext setup, bool showNotice);

    /// <summary>
    /// Sets the text value of a named field.
    /// </summary>
    /// <param name="item">The named field to update.</param>
    /// <param name="text">The new text value.</param>
    /// <param name="setup">The sync context for the operation.</param>
    /// <param name="showNotice">Whether to show a notice to the user.</param>
    public abstract void SetText(NamedField item, string text, ISyncContext setup, bool showNotice);

    /// <summary>
    /// Creates a default item for the specified parent node.
    /// </summary>
    /// <param name="parentNode">The parent node to create the item under.</param>
    /// <param name="itemCreate">Factory function for creating the item.</param>
    /// <returns>The newly created <see cref="NamedItem"/>.</returns>
    public abstract NamedItem CreateDefaultItem(NamedNode parentNode, NamedItemCreate itemCreate);

    /// <summary>
    /// Shows a GUI dialog to create an item.
    /// </summary>
    /// <param name="parentNode">The parent node to create the item under.</param>
    /// <param name="itemCreate">Factory function for creating the item.</param>
    /// <param name="itemConfig">Configuration function for the created item.</param>
    /// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
    public abstract Task<NamedItem[]> GuiCreateItem(NamedNode parentNode, NamedItemGuiCreate itemCreate, NamedItemGuiConfig itemConfig);

    /// <summary>
    /// Resolves the type name for display purposes.
    /// </summary>
    /// <param name="baseItemType">The base item type.</param>
    /// <param name="type">The actual type to resolve.</param>
    /// <param name="obj">The object instance.</param>
    /// <returns>The resolved type name string.</returns>
    public abstract string ResolveTypeName(Type baseItemType, Type type, object obj);

    /// <summary>
    /// Resolves a type from its name and parameter.
    /// </summary>
    /// <param name="baseItemType">The base item type.</param>
    /// <param name="typeName">The type name to resolve.</param>
    /// <param name="parameter">Additional parameter for resolution.</param>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    public abstract Type ResolveType(Type baseItemType, string typeName, string parameter);
}

/// <summary>
/// Abstract base class for root collection external operations, providing item management and synchronization.
/// </summary>
internal abstract class RootCollectionExternal
{
    /// <summary>
    /// Gets all items in the collection, including nested items.
    /// </summary>
    public abstract IEnumerable<NamedItem> AllItems { get; }

    /// <summary>
    /// Gets all items in the collection sorted by name.
    /// </summary>
    public abstract IEnumerable<NamedItem> AllItemsSorted { get; }

    /// <summary>
    /// Gets the top-level items in the collection.
    /// </summary>
    public abstract IEnumerable<NamedItem> Items { get; }

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public abstract void AddItem(NamedItem item);

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Checks if an item with the specified name exists in the collection.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public abstract bool ContainsItem(string name, bool inAllItems);

    /// <summary>
    /// Checks if the specified item exists in the collection.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public abstract bool ContainsItem(NamedItem item, bool inAllItems);

    /// <summary>
    /// Gets an item by name from the collection.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="inAllItems">Whether to search in all items or only top-level items.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    public abstract NamedItem GetItem(string name, bool inAllItems);

    /// <summary>
    /// Gets an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    public abstract NamedItem GetItemAt(int index);

    /// <summary>
    /// Removes an item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public abstract bool RemoveItem(NamedItem item);

    /// <summary>
    /// Renames an item in the collection.
    /// </summary>
    /// <param name="item">The item to rename.</param>
    /// <param name="newName">The new name for the item.</param>
    /// <returns>True if the rename was successful; otherwise, false.</returns>
    public abstract bool Rename(NamedItem item, string newName);

    /// <summary>
    /// Generates a suggested name for an item based on its prefix rules.
    /// </summary>
    /// <param name="item">The item to generate a name for.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string.</returns>
    public abstract string GetSuggestedName(NamedItem item, int digiLen = 2);

    /// <summary>
    /// Generates a suggested name with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string.</returns>
    public abstract string GetSuggestedName(string prefix, int digiLen = 2);

    /// <summary>
    /// Resolves a name conflict by generating a non-conflicting name.
    /// </summary>
    /// <param name="name">The original name that may conflict.</param>
    /// <returns>A non-conflicting name.</returns>
    public abstract string ResolveConflictName(string name);

    /// <summary>
    /// Internal method to add an item without triggering external events.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if the item was added; otherwise, false.</returns>
    public abstract bool InternalAddItem(NamedItem item);

    /// <summary>
    /// Internal method to create a default item for the parent node.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <returns>The newly created <see cref="NamedItem"/>.</returns>
    public abstract NamedItem InternalCreateDefaultItem(INamedNode parentNode);

    /// <summary>
    /// Internal method to show a GUI for creating items of the specified type.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="type">The type of items to create.</param>
    /// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
    public abstract Task<NamedItem[]> InternalGuiCreateItems(INamedNode parentNode, Type type);

    /// <summary>
    /// Internal method to remove an item without triggering external events.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public abstract bool InternalRemoveItem(NamedItem item);

    /// <summary>
    /// Gets the count of top-level items in the collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Checks if the specified value can be dropped into the collection.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    public abstract bool CanDropIn(object value);

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    public abstract object DropInConvert(object value);

    /// <summary>
    /// Synchronizes the collection state.
    /// </summary>
    /// <param name="sync">The index synchronizer.</param>
    /// <param name="context">The sync context.</param>
    public abstract void Sync(IIndexSync sync, ISyncContext context);
}
