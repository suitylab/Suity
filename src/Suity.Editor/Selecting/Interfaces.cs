using Suity.Drawing;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Selecting;

/// <summary>
/// Represents a selection that can be synchronized and validated.
/// </summary>
public interface ISelection : ISyncObject
{
    /// <summary>
    /// Gets the selection list associated with this selection.
    /// </summary>
    ISelectionList GetList();

    /// <summary>
    /// Gets or sets the key of the currently selected item.
    /// </summary>
    string SelectedKey { get; set; }

    /// <summary>
    /// Gets a value indicating whether the current selection is valid.
    /// </summary>
    bool IsValid { get; }
}

/// <summary>
/// The data association container interface of the reference selection interface
/// </summary>
public interface ISelectionList
{
    /// <summary>
    /// Get items
    /// </summary>
    /// <returns></returns>
    IEnumerable<ISelectionItem> GetItems();

    /// <summary>
    /// Retrieve whether the specified container contains items
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Return whether the specified container contains items</returns>
    ISelectionItem GetItem(string key);
}

/// <summary>
/// Represents an empty selection list that contains no items.
/// </summary>
public sealed class EmptySelectionList : MarshalByRefObject, ISelectionList
{
    /// <summary>
    /// The singleton instance of the empty selection list.
    /// </summary>
    public static readonly EmptySelectionList Empty = new();

    private EmptySelectionList()
    { }

    /// <summary>
    /// Gets the title of this selection list (always empty string).
    /// </summary>
    public string Title => string.Empty;

    /// <summary>
    /// Gets the icon associated with this selection list (always null).
    /// </summary>
    public ImageDef Icon => null;

    /// <summary>
    /// Gets all selection items in this list (always empty).
    /// </summary>
    public IEnumerable<ISelectionItem> GetItems() => [];

    /// <summary>
    /// Gets a selection item by its key (always returns null).
    /// </summary>
    public ISelectionItem GetItem(string item) => null;
}

/// <summary>
/// Represents a selectable item with a unique key.
/// </summary>
public interface ISelectionItem
{
    /// <summary>
    /// Gets the unique key that identifies this selection item.
    /// </summary>
    string SelectionKey { get; }
}

/// <summary>
/// Represents a selection node that can contain child items and may itself be selectable.
/// </summary>
public interface ISelectionNode : ISelectionItem, ISelectionList
{
    /// <summary>
    /// Gets a value indicating whether this node can be selected.
    /// </summary>
    bool Selectable { get; }
}