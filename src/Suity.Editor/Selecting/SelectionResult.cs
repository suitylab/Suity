using System;
using System.Collections.Generic;

namespace Suity.Selecting;

/// <summary>
/// Represents the result of a single selection dialog operation.
/// </summary>
public class SelectionResult
{
    /// <summary>
    /// A predefined result representing a failed selection with no data.
    /// </summary>
    public static readonly SelectionResult EmptyFailed = new(false, null);
    /// <summary>
    /// A predefined result representing a successful selection with no data.
    /// </summary>
    public static readonly SelectionResult EmptySuccess = new(true, null);

    /// <summary>
    /// Gets a value indicating whether the selection was successful.
    /// </summary>
    public bool IsSuccess { get; }
    /// <summary>
    /// Gets the key of the selected item.
    /// </summary>
    public string SelectedKey { get; }
    /// <summary>
    /// Gets the selected item.
    /// </summary>
    public ISelectionItem Item { get; }
    /// <summary>
    /// Gets the text associated with the selection result.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionResult"/>.
    /// </summary>
    /// <param name="isSuccess">Whether the selection was successful.</param>
    /// <param name="key">The key of the selected item.</param>
    /// <param name="item">The selected item.</param>
    /// <param name="text">The text associated with the result.</param>
    public SelectionResult(bool isSuccess, string key, ISelectionItem item = null, string text = null)
    {
        IsSuccess = isSuccess;
        SelectedKey = key;
        Item = item;
        Text = text;
    }
}

/// <summary>
/// Represents the result of a multiple selection dialog operation.
/// </summary>
public class MultipleSelectionResult
{
    /// <summary>
    /// A predefined result representing a successful multiple selection with no data.
    /// </summary>
    public static readonly MultipleSelectionResult EmptySuccess = new(true);
    /// <summary>
    /// A predefined result representing a failed multiple selection with no data.
    /// </summary>
    public static readonly MultipleSelectionResult EmptyFailed = new(false);

    /// <summary>
    /// Gets a value indicating whether the selection was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the collection of selected items.
    /// </summary>
    public IEnumerable<ISelectionItem> Items { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="MultipleSelectionResult"/> with the selected items.
    /// </summary>
    /// <param name="items">The collection of selected items.</param>
    public MultipleSelectionResult(IEnumerable<ISelectionItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        IsSuccess = true;
        Items = [.. items];
    }

    private MultipleSelectionResult(bool success)
    {
        IsSuccess = success;
        Items = [];
    }
}