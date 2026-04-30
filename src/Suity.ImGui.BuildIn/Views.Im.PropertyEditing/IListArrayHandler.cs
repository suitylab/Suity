using Suity.Collections;
using System;
using System.Collections;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Array handler implementation for <see cref="IList"/> types, providing operations
/// for getting, setting, inserting, and removing items in list-based collections.
/// </summary>
public class IListArrayHandler : ArrayHandler<IList>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IListArrayHandler"/> class.
    /// </summary>
    public IListArrayHandler()
    {
    }

    /// <inheritdoc/>
    public override Type? GetElementType(IValueTarget target)
    {
        Type? editedType = target.PresetType;
        if (editedType is null)
        {
            return null;
        }

        if (editedType.IsArray)
        {
            return editedType.GetElementType();
        }

        if (typeof(IList).IsAssignableFrom(editedType))
        {
            return editedType.GetGenericArguments()?.GetArrayItemSafe(0);
        }

        return null;
    }

    /// <inheritdoc/>
    protected override bool CanDisplay(IList list)
    {
        return true;
    }

    /// <inheritdoc/>
    protected override int GetLength(IList list)
    {
        return list.Count;
    }

    /// <inheritdoc/>
    protected override void SetLength(IList list, int count, Func<object?> creation)
    {
        if (count < 0)
        {
            return;
        }

        if (list.Count == count)
        {
            return;
        }

        // Add items if the list needs to grow
        while (list.Count < count)
        {
            var newValue = creation.Invoke();
            list.Add(newValue);
        }
        // Remove items if the list needs to shrink
        while (list.Count > count)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    /// <inheritdoc/>
    protected override object GetItemAt(IList list, int index)
    {
        return list.GetIListItemSafe(index);
    }

    /// <inheritdoc/>
    protected override void SetItemAt(IList list, int index, object? item)
    {
        list[index] = item;
    }

    /// <inheritdoc/>
    protected override void InsertItemAt(IList list, int index, object? item)
    {
        list.Insert(index, item);
    }

    /// <inheritdoc/>
    protected override void RemoveItemAt(IList list, int index)
    {
        list.RemoveAt(index);
    }
}
