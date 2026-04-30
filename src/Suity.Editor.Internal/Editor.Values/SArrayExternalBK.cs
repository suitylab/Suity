using Suity.Collections;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Back-end implementation of <see cref="SArrayExternal"/> that manages array elements,
/// synchronization, type validation, and element indexing.
/// </summary>
internal class SArrayExternalBK : SArrayExternal
{
    private readonly List<SItem> _list = [];

    private readonly SArray _ary;

    /// <summary>
    /// Initializes a new instance with the specified array.
    /// </summary>
    /// <param name="ary">The parent SArray.</param>
    public SArrayExternalBK(SArray ary)
    {
        _ary = ary;
    }

    /// <summary>
    /// Initializes a new instance with the specified array and initial values.
    /// </summary>
    /// <param name="ary">The parent SArray.</param>
    /// <param name="values">The initial values to add.</param>
    public SArrayExternalBK(SArray ary, IEnumerable<object> values)
    {
        _ary = ary;

        foreach (object value in values)
        {
            Add(value);
        }
    }

    /// <inheritdoc/>
    public override void OnInputTypeChanged()
    {
        TypeDefinition innerType = _ary.InputType.ElementType ?? TypeDefinition.Empty;
        foreach (var item in _list)
        {
            item.InputType = innerType;
        }
    }

    #region Members

    /// <inheritdoc/>
    public override object this[int index]
    {
        get => EnsureValue(index);
        set => SetValue(index, value);
    }

    /// <inheritdoc/>
    public override object GetValue(int index, ICondition context = null)
    {
        if (index >= 0 && index < _list.Count)
        {
            return SItem.ResolveValue(_list[index], context);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override object EnsureValue(int index, ICondition context = null)
    {
        if (index >= 0 && index < _list.Count)
        {
            SItem item = _list[index];

            var type = _ary.InputType.ElementType;
            if (type != null)
            {
                bool nullable = item.GetParentField()?.Optional == true;

                if (type.SupportValue(item, nullable))
                {
                    return SItem.ResolveValue(item, context);
                }
                else if (item is SUnknownValue)
                {
                    // Unknown value, ignore repair
                    return SItem.ResolveValue(item, context);
                }
                else
                {
                    object fixedValue = type.CreateOrRepairValue(item, nullable);
                    // Note: Set operation is built into Get, which will break enumeration operations
                    SetValue(index, fixedValue);

                    return SItem.ResolveValue(fixedValue, context);
                }
            }
            else
            {
                return SItem.ResolveValue(item, context);
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override void SetValue(int index, object value)
    {
        if (value == _ary)
        {
            throw new ArgumentException();
        }

        if (index < 0 || index >= _list.Count)
        {
            return;
        }

        SItem evict = _list[index];
        evict.Parent = null;
        evict.Index = -1;

        SItem item = SItem.ResolveSItem(value);
        item.Unparent();
        item.Parent = _ary;
        item.FieldId = Guid.Empty;
        item.Index = index;

        _list[index] = item;
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetValues(ICondition context = null)
    {
        return _list.Select(o => SItem.ResolveValue(o, context));
    }

    /// <inheritdoc/>
    public override SItem GetItem(int index)
    {
        if (index >= 0 && index < _list.Count)
        {
            return _list[index];
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override SItem GetItemFormatted(int index)
    {
        if (index >= 0 && index < _list.Count)
        {
            SItem item = _list[index];

            var type = _ary.InputType.ElementType;

            if (type != null)
            {
                bool nullable = item.GetParentField()?.Optional == true;

                if (type.SupportValue(item, nullable))
                {
                    return item;
                }
                else
                {
                    object fixedValue = type.CreateOrRepairValue(item, nullable);
                    // Note: Set operation is built into Get, which will break enumeration operations
                    SetValue(index, fixedValue);

                    return _list[index];
                }
            }
            else
            {
                return item;
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override void SetItem(int index, SItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }
        if (index < 0 || index >= _list.Count)
        {
            throw new InvalidOperationException();
        }

        SItem evict = _list[index];
        evict.Parent = null;
        evict.Index = -1;

        item.Unparent();
        item.Parent = _ary;
        item.FieldId = Guid.Empty;
        item.Index = index;

        _list[index] = item;
    }

    /// <inheritdoc/>
    public override IEnumerable<SItem> Items => _list.Pass();

    /// <inheritdoc/>
    public override void Add(object value)
    {
        if (value == _ary)
        {
            throw new ArgumentException();
        }

        SItem item = SItem.ResolveSItem(value);
        item.Unparent();
        item.FieldId = Guid.Empty;
        item.Parent = _ary;
        item.Index = _list.Count;

        _list.Add(item);
    }

    /// <inheritdoc/>
    public override void Insert(int index, object value)
    {
        if (value == _ary)
        {
            throw new ArgumentException();
        }

        SItem item = SItem.ResolveSItem(value);
        item.Unparent();
        item.FieldId = Guid.Empty;
        item.Parent = _ary;

        _list.Insert(index, item);

        UpdateIndex(index);
    }

    /// <inheritdoc/>
    public override void RemoveItem(SItem item)
    {
        if (item.Parent != _ary)
        {
            return;
        }

        int index = item.Index;
        if (index < 0)
        {
            return;
        }
        if (_list[index] != item)
        {
            throw new InvalidOperationException();
        }

        RemoveAt(index);
    }

    /// <inheritdoc/>
    public override void RemoveAt(int index)
    {
        if (index < 0 || index >= _list.Count)
        {
            throw new ArgumentException(nameof(index));
        }

        SItem item = _list[index];
        _list.RemoveAt(index);
        item.Parent = null;
        item.Index = -1;

        UpdateIndex(index);
    }

    /// <inheritdoc/>
    public override bool Clear()
    {
        foreach (SItem item in _list)
        {
            item.Parent = null;
            item.Index = -1;
        }
        _list.Clear();

        return true;
    }

    /// <inheritdoc/>
    public override int Count => _list.Count;

    /// <inheritdoc/>
    public override bool MergeTo(SArray target, bool skipDynamic)
    {
        var ary = this._ary;

        if (target is null)
        {
            return false;
        }

        if (ReferenceEquals(target, ary))
        {
            return false;
        }

        if (target.InputType != ary.InputType)
        {
            return false;
        }

        for (int i = 0; i < _list.Count; i++)
        {
            var value = _list[i];

            if (i >= target.Count)
            {
                target.Add(Cloner.Clone(value));
                continue;
            }
            
            var targetValue = target.GetItem(i);
            if (skipDynamic && targetValue is SDynamic)
            {
                continue;
            }

            bool merged = false;

            if (value is SObject childSObj && targetValue is SObject childTargetSObj)
            {
                if (childSObj.MergeTo(childTargetSObj, skipDynamic))
                {
                    merged = true;
                }
            }
            else if (value is SArray childAry && targetValue is SArray childTargetAry)
            {
                if (childAry.MergeTo(childTargetAry, skipDynamic))
                {
                    merged = true;
                }
            }

            if (!merged)
            {
                target.SetItem(i, Cloner.Clone(value));
            }
        }

        while (target.Count > _list.Count)
        {
            target.RemoveAt(target.Count - 1);
        }

        return true;
    }

    /// <inheritdoc/>
    public override Array ToArray()
    {
        Type type = _ary.InputType.Target?.NativeType;
        if (type is null)
        {
            return null;
        }

        int len = _ary.Count;
        var ary = Array.CreateInstance(type, len);

        for (int i = 0; i < _ary.Count; i++)
        {
            var value = this[i];
            var v = ResolveValue(value, type);
            ary.SetValue(v, i);
        }

        return ary;
    }

    /// <inheritdoc/>
    public override Array ToArray(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        Array ary = Array.CreateInstance(type, Count);

        for (int i = 0; i < Count; i++)
        {
            var value = this[i];
            var v = ResolveValue(value, type);
            ary.SetValue(v, i);
        }

        return ary;
    }

    /// <inheritdoc/>
    public override T[] ToArray<T>(T defaultValue = default)
    {
        T[] ary = new T[Count];

        for (int i = 0; i < Count; i++)
        {
            var value = this[i];
            var v = ResolveValue(value, typeof(T));
            if (v is T t)
            {
                ary[i] = t;
            }
            else
            {
                ary[i] = defaultValue;
            }
        }

        return ary;
    }

    /// <summary>
    /// Resolves a value to the specified target type.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <param name="type">The target type.</param>
    /// <returns>The resolved value, or the type's default if conversion fails.</returns>
    private object ResolveValue(object value, Type type)
    {
        if (value != null)
        {
            if (type.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (type.IsEnum && value is SEnum e)
            {
                try
                {
                    if (Enum.Parse(type, e.Value) is { } v)
                    {
                        return v;
                    }
                }
                catch (Exception)
                {
                }

                return Activator.CreateInstance(type);
            }
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Updates the index values for all items starting from the specified position.
    /// </summary>
    /// <param name="begin">The starting index.</param>
    private void UpdateIndex(int begin = 0)
    {
        for (int i = begin; i < _list.Count; i++)
        {
            var item = _list[i];
            item.Index = i;
        }
    }

    #endregion

    #region Comparison section

    /// <inheritdoc/>
    public override bool ValueEquals(object other)
    {
        if (other is not SArray v)
        {
            return false;
        }

        var otherEx = (SArrayExternalBK)(v._ex);

        if (_list.Count != otherEx._list.Count) return false;
        for (int i = 0; i < _list.Count; i++)
        {
            if (!SItem.ValueEquals(_list[i], otherEx._list[i]))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region ISyncList

    /// <inheritdoc/>
    public override void Sync(IIndexSync sync, ISyncContext context)
    {
        if (sync.IsSetter())
        {
            if (sync.SyncSetTypeDefinition(SArray.Attribute_InputType, _ary.InputType, out TypeDefinition newInputType, out string newTypeId))
            {
                _ary.InputType = newInputType;
            }
        }
        else
        {
            sync.SyncGetTypeDefinition(SArray.Attribute_InputType, _ary.InputType);
        }

        switch (sync.Mode)
        {
            case SyncMode.Get:
                sync.Sync(sync.Index, GetItem(sync.Index));
                break;

            case SyncMode.Set:
                this[sync.Index] = SItem.ResolveSItem(sync.Sync<object>(sync.Index, null));
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < _list.Count; i++)
                {
                    sync.Sync(i, _list[i]);
                }
                break;

            case SyncMode.SetAll:
                Clear();
                for (int i = 0; i < sync.Count; i++)
                {
                    Add(sync.Sync<object>(i, null));
                }
                break;

            case SyncMode.Insert:
                Insert(sync.Index, sync.Sync<object>(sync.Index, null));
                break;

            case SyncMode.RemoveAt:
                RemoveAt(sync.Index);
                break;

            case SyncMode.CreateNew:
                sync.Sync(0, _ary.InputType?.ElementType?.CreateValue());
                break;

            default:
                break;
        }
    }

    #endregion

    /// <inheritdoc/>
    public override void AutoConvertValue()
    {
        TypeDefinition elementType = _ary.InputType.ElementType;
        foreach (var item in _list)
        {
            item.InputType = elementType;
        }
    }

    /// <inheritdoc/>
    public override void Validate(ValidationContext context)
    {
        if (!_ary.InputType.IsArray)
        {
            context.Report(L($"{_ary.InputType} is not an array type"));

            return;
        }

        TypeDefinition innerType = _ary.InputType.ElementType;

        foreach (SItem item in _list)
        {
            if (item is not SValue && item.InputType != innerType)
            {
                context.Report(L($"Array element type {item.InputType} does not match array type {_ary.InputType}"));
            }
        }
    }

    /// <inheritdoc/>
    public override FieldObject GetField(SItem item)
    {
        return _ary.Parent?.GetField(_ary);
    }

    /// <summary>
    /// Validates that all items have correct index values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when an index mismatch is detected.</exception>
    public override void AssertIndex()
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i].Index != i)
            {
                throw new InvalidOperationException("Index assertion failed.");
            }
        }
    }
}
