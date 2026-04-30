using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Backend implementation of <see cref="ActionSetterExternal"/> that creates value action instances
/// for property grid operations such as setting values, array manipulation, and item reordering.
/// </summary>
internal class ActionSetterExternalBK : ActionSetterExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ActionSetterExternalBK"/>.
    /// </summary>
    public static ActionSetterExternalBK Instance { get; } = new ActionSetterExternalBK();

    /// <inheritdoc/>
    public override IValueAction SetValuesAction(IValueTarget target, IEnumerable<object?> values, IEnumerable<object?>? undoValues = null)
    {
        return new ValueSetterAction(target, values, undoValues);
    }

    /// <inheritdoc/>
    public override IValueAction SetArrayCountAction(ArrayTarget target, IEnumerable<int> counts)
    {
        return new ArraySetCountAction(target, counts);
    }

    /// <inheritdoc/>
    public override IValueAction PushArrayItemAtAction(ArrayTarget target, IEnumerable<object?> values)
    {
        return new ArrayPushItemAction(target, values);
    }

    /// <inheritdoc/>
    public override IValueAction RemoveArrayItemAtAction(ArrayTarget target, int index)
    {
        return new ArrayRemoveItemAction(target, index);
    }

    /// <inheritdoc/>
    public override IValueAction RemoveArrayItemAtAction(ArrayTarget target, IEnumerable<int> indexes)
    {
        return new ArrayRemoveItemAction(target, indexes);
    }

    /// <inheritdoc/>
    public override IValueAction RemoveInsertItemAction(ArrayTarget target, int index, int indexTo)
    {
        return new ArrayRemoveInsertAction(target, index, indexTo);
    }

    /// <inheritdoc/>
    public override IValueAction CloneArrayItemAtAction(ArrayTarget target, int index)
    {
        return new ArrayCloneItemAction(target, index);
    }

    /// <inheritdoc/>
    public override IValueAction SwapArrayItemAtAction(ArrayTarget target, int index)
    {
        return new ArraySwapItemAction(target, index);
    }
}
