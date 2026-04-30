using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides extension methods for creating value actions on property targets.
/// </summary>
public static class ActionSetterExtensions
{
    internal static ActionSetterExternal _external;

    /// <summary>
    /// Creates an action to set values on a value target.
    /// </summary>
    /// <param name="target">The value target to set values on.</param>
    /// <param name="values">The new values to set.</param>
    /// <param name="undoValues">Optional values to use for undo operations.</param>
    /// <returns>An action representing the value change.</returns>
    public static IValueAction SetValuesAction(this IValueTarget target, IEnumerable<object?> values, IEnumerable<object?>? undoValues = null)
        => _external.SetValuesAction(target, values, undoValues);

    /// <summary>
    /// Creates an action to set the count of elements in an array target.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="counts">The new element counts.</param>
    /// <returns>An action representing the count change.</returns>
    public static IValueAction SetCountAction(this ArrayTarget target, IEnumerable<int> counts)
        => _external.SetArrayCountAction(target, counts);

    /// <summary>
    /// Creates an action to push new items to the end of an array.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="values">The values to add.</param>
    /// <returns>An action representing the push operation.</returns>
    public static IValueAction PushItemAtAction(this ArrayTarget target, IEnumerable<object?> values)
        => _external.PushArrayItemAtAction(target, values);

    /// <summary>
    /// Creates an action to remove an item at a single index from an array.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <returns>An action representing the removal operation.</returns>
    public static IValueAction RemoveItemAtAction(this ArrayTarget target, int index)
        => _external.RemoveArrayItemAtAction(target, index);

    /// <summary>
    /// Creates an action to remove items at multiple indexes from an array.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="indexes">The zero-based indexes of the items to remove.</param>
    /// <returns>An action representing the removal operation.</returns>
    public static IValueAction RemoveItemAtAction(this ArrayTarget target, IEnumerable<int> indexes)
        => _external.RemoveArrayItemAtAction(target, indexes);

    /// <summary>
    /// Creates an action to move an item from one index to another in an array.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="index">The current index of the item.</param>
    /// <param name="indexTo">The destination index.</param>
    /// <returns>An action representing the move operation.</returns>
    public static IValueAction RemoveInsertItemAction(this ArrayTarget target, int index, int indexTo)
        => _external.RemoveInsertItemAction(target, index, indexTo);

    /// <summary>
    /// Creates an action to clone an item at the specified index.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="index">The zero-based index of the item to clone.</param>
    /// <returns>An action representing the clone operation.</returns>
    public static IValueAction CloneItemAtAction(this ArrayTarget target, int index)
        => _external.CloneArrayItemAtAction(target, index);

    /// <summary>
    /// Creates an action to swap an item at the specified index with the next item.
    /// </summary>
    /// <param name="target">The array target.</param>
    /// <param name="index">The zero-based index of the item to swap.</param>
    /// <returns>An action representing the swap operation.</returns>
    public static IValueAction SwapItemAtAction(this ArrayTarget target, int index)
        => _external.SwapArrayItemAtAction(target, index);
}
