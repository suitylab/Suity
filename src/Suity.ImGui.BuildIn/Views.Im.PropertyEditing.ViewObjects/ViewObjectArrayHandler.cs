using Suity.Collections;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using System;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Array handler for <see cref="ISyncList"/> types, providing list operations for property editing.
/// </summary>
internal class SyncListArrayHandler : ArrayHandler<ISyncList>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncListArrayHandler"/> class.
    /// </summary>
    public SyncListArrayHandler()
    {
    }

    /// <inheritdoc/>
    public override Type? GetElementType(IValueTarget target)
    {
        return target.GetValues()
            .OfType<ISyncList>()
            .Select(o => o.GetElementType())
            .SkipNull()
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    protected override bool CanDisplay(ISyncList list)
    {
        //return list.ListViewId == 0 || list.ListViewId == ViewIds.Inspector;
        return true;
    }

    /// <inheritdoc/>
    protected override int GetLength(ISyncList list)
    {
        return list.Count;
    }

    /// <inheritdoc/>
    protected override void SetLength(ISyncList list, int count, Func<object?> creation)
    {
        if (count < 0)
        {
            return;
        }

        if (list.Count == count)
        {
            return;
        }

        while (list.Count < count)
        {
            var newValue = list.CreateNewItem() ?? creation();
            list.Add(newValue);
        }
        while (list.Count > count)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    /// <inheritdoc/>
    protected override object GetItemAt(ISyncList list, int index)
    {
        return list.GetItem(index);
    }

    /// <inheritdoc/>
    protected override void SetItemAt(ISyncList list, int index, object? item)
    {
        list.SetItem(index, item);
    }

    /// <inheritdoc/>
    protected override void InsertItemAt(ISyncList list, int index, object? item)
    {
        list.Insert(index, item);
    }

    /// <inheritdoc/>
    protected override void RemoveItemAt(ISyncList list, int index)
    {
        list.RemoveAt(index);
    }
}

/// <summary>
/// Array handler for <see cref="IViewList"/> types, providing list operations with view ID filtering.
/// </summary>
internal class ViewListArrayHandler : ArrayHandler<IViewList>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewListArrayHandler"/> class.
    /// </summary>
    public ViewListArrayHandler()
    {
    }

    /// <inheritdoc/>
    public override Type? GetElementType(IValueTarget target)
    {
        return target.GetValues()
            .OfType<IViewList>()
            .Select(o => o.GetElementType())
            .SkipNull()
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    protected override bool CanDisplay(IViewList list)
    {
        return list.ListViewId == 0 || list.ListViewId == ViewIds.Inspector;
    }

    /// <inheritdoc/>
    protected override int GetLength(IViewList list)
    {
        return list.Count;
    }

    /// <inheritdoc/>
    protected override void SetLength(IViewList list, int count, Func<object?> creation)
    {
        if (count < 0)
        {
            return;
        }

        if (list.Count == count)
        {
            return;
        }

        while (list.Count < count)
        {
            var newValue = list.CreateNewItem() ?? creation();
            list.Add(newValue);
        }
        while (list.Count > count)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    /// <inheritdoc/>
    protected override object GetItemAt(IViewList list, int index)
    {
        return list.GetItem(index);
    }

    /// <inheritdoc/>
    protected override void SetItemAt(IViewList list, int index, object? item)
    {
        list.SetItem(index, item);
    }

    /// <inheritdoc/>
    protected override void InsertItemAt(IViewList list, int index, object? item)
    {
        list.Insert(index, item);
    }

    /// <inheritdoc/>
    protected override void RemoveItemAt(IViewList list, int index)
    {
        list.RemoveAt(index);
    }
}

/// <summary>
/// Array handler for <see cref="SArray"/> types, providing array operations for SObject-based collections.
/// </summary>
internal class SArrayHandler : ArrayHandler<SArray>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SArrayHandler"/> class.
    /// </summary>
    public SArrayHandler()
    {
    }

    /// <inheritdoc/>
    public override Type? GetElementType(IValueTarget target)
    {
        return target.GetValues()
            .OfType<SArray>()
            .Select(o => o.InputType.ElementType.GetEditedType())
            .SkipNull()
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    protected override bool CanDisplay(SArray list)
    {
        return true;
    }

    /// <inheritdoc/>
    protected override int GetLength(SArray list)
    {
        return list.Count;
    }

    /// <inheritdoc/>
    protected override void SetLength(SArray list, int count, Func<object?> creation)
    {
        if (count < 0)
        {
            return;
        }

        if (list.Count == count)
        {
            return;
        }

        while (list.Count < count)
        {
            var newValue = list.InputType?.ElementType?.CreateValue() ?? creation();
            list.Add(newValue);
        }
        while (list.Count > count)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    /// <inheritdoc/>
    protected override object GetItemAt(SArray list, int index)
    {
        return list.GetItem(index);
    }

    /// <inheritdoc/>
    protected override void SetItemAt(SArray list, int index, object? item)
    {
        list.SetValue(index, item);
    }

    /// <inheritdoc/>
    protected override void InsertItemAt(SArray list, int index, object? item)
    {
        list.Insert(index, item);
    }

    /// <inheritdoc/>
    protected override void RemoveItemAt(SArray list, int index)
    {
        list.RemoveAt(index);
    }
}