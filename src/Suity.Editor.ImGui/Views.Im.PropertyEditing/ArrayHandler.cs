using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides abstract operations for handling array-like collections in property editing.
/// </summary>
public abstract class ArrayHandler
{
    /// <summary>
    /// Gets the element type for the specified value target.
    /// </summary>
    /// <param name="target">The value target to inspect.</param>
    /// <returns>The element type, or <c>null</c> if it cannot be determined.</returns>
    public abstract Type? GetElementType(IValueTarget target);

    /// <summary>
    /// Determines whether the specified array can be displayed.
    /// </summary>
    /// <param name="array">The array object to check.</param>
    /// <returns><c>true</c> if the array can be displayed; otherwise, <c>false</c>.</returns>
    public abstract bool CanDisplay(object? array);

    /// <summary>
    /// Gets the length of the specified array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <returns>The length of the array, or <c>null</c> if it cannot be determined.</returns>
    public abstract int? GetLength(object? array);

    /// <summary>
    /// Sets the length of the specified array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <param name="count">The new length to set.</param>
    /// <param name="creation">A function that creates new elements when the array grows.</param>
    /// <returns><c>true</c> if the length was successfully set; otherwise, <c>false</c>.</returns>
    public abstract bool SetLength(object? array, int count, Func<object?> creation);

    /// <summary>
    /// Gets the item at the specified index in the array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The item at the specified index, or <c>null</c> if not found.</returns>
    public abstract object? GetItemAt(object? array, int index);

    /// <summary>
    /// Sets the item at the specified index in the array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <param name="index">The zero-based index.</param>
    /// <param name="item">The item to set.</param>
    /// <returns><c>true</c> if the item was successfully set; otherwise, <c>false</c>.</returns>
    public abstract bool SetItemAt(object? array, int index, object? item);

    /// <summary>
    /// Inserts an item at the specified index in the array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
    /// <returns><c>true</c> if the item was successfully inserted; otherwise, <c>false</c>.</returns>
    public abstract bool InsertItemAt(object? array, int index, object? item);

    /// <summary>
    /// Removes the item at the specified index from the array.
    /// </summary>
    /// <param name="array">The array object.</param>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <returns><c>true</c> if the item was successfully removed; otherwise, <c>false</c>.</returns>
    public abstract bool RemoveItemAt(object? array, int index);
}

/// <summary>
/// Provides strongly-typed abstract operations for handling array-like collections.
/// </summary>
/// <typeparam name="T">The type of the array collection.</typeparam>
public abstract class ArrayHandler<T> : ArrayHandler
{
    /// <inheritdoc/>
    public override bool CanDisplay(object? list)
    {
        if (list is T tList)
        {
            return CanDisplay(tList);
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override int? GetLength(object? list)
    {
        if (list is T tList)
        {
            return GetLength(tList);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override bool SetLength(object? list, int count, Func<object?> creation)
    {
        if (list is T tList)
        {
            SetLength(tList, count, creation);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override object? GetItemAt(object? list, int index)
    {
        if (list is T tList)
        {
            return GetItemAt(tList, index);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override bool SetItemAt(object? list, int index, object? item)
    {
        if (list is T tList)
        {
            SetItemAt(tList, index, item);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool InsertItemAt(object? list, int index, object? item)
    {
        if (list is T tList)
        {
            InsertItemAt(tList, index, item);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool RemoveItemAt(object? list, int index)
    {
        if (list is T tList)
        {
            RemoveItemAt(tList, index);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified typed array can be displayed.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <returns><c>true</c> if the array can be displayed; otherwise, <c>false</c>.</returns>
    protected abstract bool CanDisplay(T list);

    /// <summary>
    /// Gets the length of the specified typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <returns>The length of the array.</returns>
    protected abstract int GetLength(T list);

    /// <summary>
    /// Sets the length of the specified typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <param name="count">The new length to set.</param>
    /// <param name="creation">A function that creates new elements when the array grows.</param>
    protected abstract void SetLength(T list, int count, Func<object?> creation);

    /// <summary>
    /// Gets the item at the specified index in the typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The item at the specified index.</returns>
    protected abstract object GetItemAt(T list, int index);

    /// <summary>
    /// Sets the item at the specified index in the typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <param name="index">The zero-based index.</param>
    /// <param name="item">The item to set.</param>
    protected abstract void SetItemAt(T list, int index, object? item);

    /// <summary>
    /// Inserts an item at the specified index in the typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
    protected abstract void InsertItemAt(T list, int index, object? item);

    /// <summary>
    /// Removes the item at the specified index from the typed array.
    /// </summary>
    /// <param name="list">The typed array object.</param>
    /// <param name="index">The zero-based index of the item to remove.</param>
    protected abstract void RemoveItemAt(T list, int index);
}
