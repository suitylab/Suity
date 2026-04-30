using Suity.Collections;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

#region ValueAction

/// <summary>
/// Abstract base class for value actions that can be executed and undone in the property grid.
/// Implements synchronization context support for cross-AppDomain calls.
/// </summary>
public abstract class ValueAction(ITarget target) : MarshalByRefObject,
    IValueAction, ISetterContext, ISyncContext
{
    /// <summary>
    /// Gets the target object that this action operates on.
    /// </summary>
    public ITarget Target { get; } = target ?? throw new ArgumentNullException(nameof(target));

    /// <summary>
    /// Gets the parent objects in the target hierarchy.
    /// </summary>
    public virtual IEnumerable<object> ParentObjects => Target.GetParentObjects().OfType<object>();

    /// <summary>
    /// Gets the name of this action, typically the property name being modified.
    /// </summary>
    public virtual string? Name => null;

    /// <summary>
    /// Gets or sets a value indicating whether this action is a preview (non-committed) operation.
    /// </summary>
    public bool Preview { get; set; }

    /// <summary>
    /// Executes the action, applying the changes to the target.
    /// </summary>
    public abstract void DoAction();

    /// <summary>
    /// Undoes the action, restoring the previous state.
    /// </summary>
    public abstract void UndoAction();


    #region ISyncContext

    /// <inheritdoc/>
    object? ISyncContext.Parent => Target;

    /// <inheritdoc/>
    object? IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsAssignableFrom(this.GetType()))
        {
            return this;
        }

        if (Target is IServiceProvider provider)
        {
            return provider.GetService(serviceType);
        }

        return null;
    } 
    #endregion
}

#endregion

#region ValueSetterAction

/// <summary>
/// Action that sets new values on a property target, storing undo values for reversal.
/// </summary>
public class ValueSetterAction : ValueAction, ISetterDataRecord
{
    private readonly object?[] _undoValues;
    private readonly object?[] _values;

    private readonly IValueTarget _target;


    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSetterAction"/> class.
    /// </summary>
    /// <param name="target">The value target to modify.</param>
    /// <param name="values">The new values to set.</param>
    /// <param name="undoValues">Optional undo values. If not provided, current values are cloned.</param>
    public ValueSetterAction(IValueTarget target, IEnumerable<object?> values, IEnumerable<object?> undoValues = null) 
        : base(target)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = values.ToArray();

        // Some values are special, such as read-only Selection, which won't replace the original value but only modify the SelectedKey property. Therefore, we need to clone the original value first.
        _undoValues = undoValues?.ToArray() ?? target.GetValues().Select(o => Cloner.Clone(o)).ToArray();

        _target = target;
    }

    /// <inheritdoc/>
    public override string? Name => _target.PropertyName;

    /// <summary>
    /// Gets or sets additional data associated with this setter action.
    /// </summary>
    public object? Data { get; set; }

    /// <inheritdoc/>
    public override void DoAction()
    {
        _target.SetValues(_values, this);
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        _target.SetValues(_undoValues, this);
    }

    private void DoPostAction()
    {

    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Set {Name}";
    }

}

#endregion

#region OptionalSetterAction

/// <summary>
/// Action that sets the optional state (enabled/disabled) of property values.
/// </summary>
public class OptionalSetterAction : ValueAction, ISetterDataRecord
{
    private readonly bool[] _undoValues;
    private readonly bool[] _values;

    private readonly IValueTarget _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalSetterAction"/> class with a single boolean value.
    /// </summary>
    /// <param name="target">The value target to modify.</param>
    /// <param name="value">The optional state to set.</param>
    public OptionalSetterAction(IValueTarget target, bool value) : this(target, [value])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalSetterAction"/> class with multiple boolean values.
    /// </summary>
    /// <param name="target">The value target to modify.</param>
    /// <param name="values">The optional states to set for each value.</param>
    public OptionalSetterAction(IValueTarget target, IEnumerable<bool> values) : base(target)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = [.. values];
        if (_values.Length == 0)
        {
            _values = [false];
        }

        // Some values are special, such as read-only Selection, which won't replace the original value but only modify the SelectedKey property. Therefore, we need to clone the original value first.
        _undoValues = target.GetValues().As<IViewOptional>().Select(o => o?.IsOptional ?? false).ToArray();

        _target = target;
    }

    /// <inheritdoc/>
    public override string? Name => "Set Optional";

    /// <summary>
    /// Gets or sets additional data associated with this setter action.
    /// </summary>
    public object? Data { get; set; }

    /// <inheritdoc/>
    public override void DoAction()
    {
        var objs = _target.GetValues().ToArray();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is IViewOptional opt)
            {
                opt.IsOptional = _values.GetArrayItemMinMax(i);
            }
        }
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        var objs = _target.GetValues().ToArray();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is IViewOptional opt)
            {
                opt.IsOptional = _undoValues.GetArrayItemMinMax(i);
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Set Optional";
    }

}

#endregion

#region ArraySetCountAction

/// <summary>
/// Action that changes the length of an array property, preserving undo information.
/// </summary>
public class ArraySetCountAction : ValueAction
{
    private readonly List<object?>[] _undoAllValues;

    private readonly int[] _counts;
    private readonly ArrayTarget _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArraySetCountAction"/> class.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="counts">The new lengths to set for each array.</param>
    public ArraySetCountAction(ArrayTarget target, IEnumerable<int> counts) : base(target)
    {
        if (counts is null)
        {
            throw new ArgumentNullException(nameof(counts));
        }

        _counts = counts.ToArray();

        var arrays = target.GetArrays().ToArray();
        var handler = target.Handler;
        _undoAllValues = new List<object?>[arrays.Length];

        for (int i = 0; i < arrays.Length; i++)
        {
            var list = new List<object?>();
            int len = handler.GetLength(arrays[i]) ?? 0;
            for (int j = 0; j < len; j++)
            {
                list.Add(handler.GetItemAt(arrays[i], j));
            }
            _undoAllValues[i] = list;
        }

        _target = target;
    }

    /// <inheritdoc/>
    public override void DoAction()
    {
        _target.SetArrayLength(_counts);
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        var arrays = _target.GetArrays().ToArray();
        var handler = _target.Handler;

        for (int i = 0; i < _undoAllValues.Length; i++)
        {
            var list = _undoAllValues[i];
            handler.SetLength(arrays[i], list.Count, () => null);

            for (int j = 0; j < list.Count; j++)
            {
                handler.SetItemAt(arrays[i], j, list[j]);
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Set array length";
    }
}

#endregion

#region ArrayRemoveItemAction

/// <summary>
/// Action that removes one or more items from an array property.
/// </summary>
public class ArrayRemoveItemAction : ValueAction
{
    private readonly ArrayTarget _target;

    private readonly object?[] _undoObjects;
    private readonly int? _index;

    private readonly object?[]? _arrays;
    private readonly int[]? _indexes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayRemoveItemAction"/> class for a single index.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to remove.</param>
    public ArrayRemoveItemAction(ArrayTarget target, int index) : base(target)
    {
        _target = target;
        _index = index;
        _undoObjects = target.GetArrayItemAt(index).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayRemoveItemAction"/> class for multiple indexes.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="indexes">The indexes of items to remove from each array.</param>
    public ArrayRemoveItemAction(ArrayTarget target, IEnumerable<int> indexes) : base(target)
    {
        _target = target;
        _indexes = indexes.ToArray();

        var handler = _target.Handler;

        _arrays = target.GetArrays().ToArray();
        _undoObjects = new object[_arrays.Length];

        for (int i = 0; i < _arrays.Length; i++)
        {
            int index = _indexes.GetArrayItemMinMax(i);
            if (index >= 0)
            {
                _undoObjects[i] = handler.GetItemAt(_arrays[i], index);
            }
        }
    }

    /// <inheritdoc/>
    public override string? Name => _index.ToString();

    /// <inheritdoc/>
    public override void DoAction()
    {
        if (_index.HasValue)
        {
            _target.RemoveArrayItemAt(_index.Value);
        }
        else if (_arrays is { } && _indexes is { })
        {
            var handler = _target.Handler;

            for (int i = 0; i < _arrays.Length; i++)
            {
                int index = _indexes.GetArrayItemMinMax(i);
                if (index >= 0)
                {
                    handler.RemoveItemAt(_arrays[i], index);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        if (_index.HasValue)
        {
            _target.InsertArrayItemAt(_index.Value, _undoObjects);
        }
        else if (_arrays is { } && _indexes is { })
        {
            var handler = _target.Handler;

            for (int i = 0; i < _arrays.Length; i++)
            {
                int index = _indexes.GetArrayItemMinMax(i);
                if (index >= 0)
                {
                    handler.InsertItemAt(_arrays[i], index, _undoObjects[i]);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Remove array element";
    }
}

#endregion

#region ArrayRemoveInsertAction

/// <summary>
/// Action that moves an array item from one position to another by removing and re-inserting.
/// </summary>
public class ArrayRemoveInsertAction : ValueAction
{
    private readonly ArrayTarget _target;

    private readonly object?[] _undoObjects;
    private readonly int _index;
    private readonly int _indexTo;

    private readonly object?[]? _arrays;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayRemoveInsertAction"/> class.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The source index of the item to move.</param>
    /// <param name="indexTo">The destination index where the item should be moved.</param>
    public ArrayRemoveInsertAction(ArrayTarget target, int index, int indexTo) : base(target)
    {
        _target = target;
        _index = index;
        _indexTo = indexTo;
        _undoObjects = target.GetArrayItemAt(index).ToArray();
    }

    /// <inheritdoc/>
    public override string? Name => _index.ToString();

    /// <inheritdoc/>
    public override void DoAction()
    {
        if (_indexTo > _index)
        {
            _target.RemoveArrayItemAt(_index);
            _target.InsertArrayItemAt(_indexTo - 1, _undoObjects);
        }
        else
        {
            _target.RemoveArrayItemAt(_index);
            _target.InsertArrayItemAt(_indexTo, _undoObjects);
        }
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        if (_indexTo > _index)
        {
            _target.RemoveArrayItemAt(_indexTo - 1);
            _target.InsertArrayItemAt(_index, _undoObjects);
        }
        else
        {
            _target.RemoveArrayItemAt(_indexTo);
            _target.InsertArrayItemAt(_index, _undoObjects);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Remove and insert array element";
    }
}

#endregion

#region ArrayPushItemAction

/// <summary>
/// Action that adds (pushes) new items to the end of an array property.
/// </summary>
public class ArrayPushItemAction : ValueAction
{
    private readonly IEnumerable<object?> _objects;

    private readonly ArrayTarget _target;

    private readonly int[] _indexes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayPushItemAction"/> class.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="objects">The objects to add to the arrays.</param>
    public ArrayPushItemAction(ArrayTarget target, IEnumerable<object?> objects) : base(target)
    {
        _target = target;
        _objects = objects;
        _indexes = _target.GetArrayLength().ToArray();
    }

    /// <inheritdoc/>
    public override string? Name => string.Empty;

    /// <inheritdoc/>
    public override void DoAction()
    {
        _target.PushArrayItem(_objects);
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        _target.PopArrayItem();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Add array element";
    }
}

#endregion

#region ArrayCloneItemAction

/// <summary>
/// Action that clones an item at a specific index in an array property.
/// </summary>
public class ArrayCloneItemAction : ValueAction
{
    private readonly int _index;
    private readonly ArrayTarget _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayCloneItemAction"/> class.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to clone.</param>
    public ArrayCloneItemAction(ArrayTarget target, int index) : base(target)
    {
        _target = target;
        _index = index;
    }

    /// <inheritdoc/>
    public override string? Name => _index.ToString();

    /// <inheritdoc/>
    public override void DoAction()
    {
        _target.CloneArrayItemAt(_index);
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        _target.RemoveArrayItemAt(_index);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Clone array element";
    }
}

#endregion

#region ArraySwapItemAction

/// <summary>
/// Action that swaps an item at a specific index with the next item in an array property.
/// </summary>
public class ArraySwapItemAction : ValueAction
{
    private readonly int _index;
    private readonly ArrayTarget _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArraySwapItemAction"/> class.
    /// </summary>
    /// <param name="target">The array target to modify.</param>
    /// <param name="index">The index of the item to swap with the next item.</param>
    public ArraySwapItemAction(ArrayTarget target, int index) : base(target)
    {
        _target = target;
        _index = index;
    }

    /// <inheritdoc/>
    public override string? Name => _index.ToString();

    /// <inheritdoc/>
    public override void DoAction()
    {
        _target.SwapArrayItemAt(_index);
    }

    /// <inheritdoc/>
    public override void UndoAction()
    {
        _target.SwapArrayItemAt(_index);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Swap array element";
    }
}

#endregion
