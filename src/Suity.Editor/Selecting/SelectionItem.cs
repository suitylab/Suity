using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Selecting;

/// <summary>
/// A basic implementation of <see cref="ISelectionItem"/> with display and color support.
/// </summary>
public class SelectionItem : MarshalByRefObject,
    ISelectionItem, INamed, ITextDisplay, IViewColor
{
    /// <summary>
    /// Initializes a new instance of <see cref="SelectionItem"/> with the specified key.
    /// </summary>
    /// <param name="key">The unique selection key.</param>
    public SelectionItem(string key)
    {
        SelectionKey = key;
    }

    /// <summary>
    /// Gets the unique key that identifies this selection item.
    /// </summary>
    public string SelectionKey { get; }

    /// <summary>
    /// Gets or sets the name of this selection item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the display text for this selection item.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// Gets the display icon for this selection item.
    /// </summary>
    public object DisplayIcon { get; init; }

    /// <summary>
    /// Gets the display status of this selection item (always <see cref="TextStatus.Normal"/>).
    /// </summary>
    public TextStatus DisplayStatus => TextStatus.Normal;

    /// <summary>
    /// Gets the color associated with this selection item.
    /// </summary>
    public Color? ViewColor { get; init; }

    /// <summary>
    /// Returns the hash code for this selection item based on the selection key.
    /// </summary>
    public override int GetHashCode() => SelectionKey?.GetHashCode() ?? 0;

    /// <summary>
    /// Determines whether the specified object is equal to this selection item.
    /// Two <see cref="SelectionItem"/> instances are considered equal if they have the same selection key.
    /// </summary>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        var other = obj as SelectionItem;
        if (Equals(other, null))
        {
            return false;
        }

        return SelectionKey == other.SelectionKey;
    }

    /// <summary>
    /// Determines whether two <see cref="SelectionItem"/> instances are equal by comparing their selection keys.
    /// </summary>
    public static bool operator ==(SelectionItem v1, SelectionItem v2)
    {
        if (Equals(v1, null)) return Equals(v2, null); else return v1.Equals(v2);
    }

    /// <summary>
    /// Determines whether two <see cref="SelectionItem"/> instances are not equal by comparing their selection keys.
    /// </summary>
    public static bool operator !=(SelectionItem v1, SelectionItem v2)
    {
        if (Equals(v1, null)) return !Equals(v2, null); else return !v1.Equals(v2);
    }

    /// <summary>
    /// Returns the localized display text or the selection key as a string representation.
    /// </summary>
    public override string ToString() => L(DisplayText) ?? SelectionKey;
}