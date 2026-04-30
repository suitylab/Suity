using Suity;
using Suity.Collections;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// Backing implementation of <see cref="ArrayTarget"/> that manages array element targets and operations.
/// </summary>
public class ArrayTargetBK : ArrayTarget
{
    private readonly PropertyTarget _ownerTarget;
    private readonly ArrayHandler _handler;
    private readonly Type? _elementType;

    private List<PropertyTarget?>? _list;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayTargetBK"/> class.
    /// </summary>
    /// <param name="target">The owner property target.</param>
    /// <param name="handler">The array handler for array operations.</param>
    public ArrayTargetBK(PropertyTarget target, ArrayHandler handler)
    {
        _ownerTarget = target ?? throw new ArgumentNullException(nameof(target));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _elementType = handler.GetElementType(target);

        // string has special properties
        if (ElementType == typeof(string))
        {
            ItemActivator = () => string.Empty;
            ItemCloner = v => v;
        }
    }

    /// <summary>
    /// Gets or sets the factory function for creating new array items.
    /// </summary>
    public Func<object>? ItemActivator { get; set; }

    /// <summary>
    /// Gets or sets the function for cloning existing array items.
    /// </summary>
    public Func<object, object>? ItemCloner { get; set; }

    /// <summary>
    /// Gets a value indicating whether any element target in this array contains an error.
    /// </summary>
    public bool ContainsError => _list?.OfType<PropertyTarget>().Any(o => o.ErrorInHierarchy) == true;

    #region ArrayTarget

    /// <inheritdoc/>
    public override PropertyTarget OwnerTarget => _ownerTarget;

    /// <inheritdoc/>
    public override ArrayHandler Handler => _handler;

    /// <inheritdoc/>
    public override Type? ElementType => _elementType;

    /// <inheritdoc/>
    public override IEnumerable<object?> GetArrays() => _ownerTarget.GetValues();

    /// <inheritdoc/>
    public override int StartIndex { get; set; }

    /// <inheritdoc/>
    public override bool CanDisplay() => _ownerTarget.GetValues().All(_handler.CanDisplay);

    /// <inheritdoc/>
    public override PropertyTarget? GetElementTarget(int index) => _list?.GetListItemSafe(index);

    /// <summary>
    /// Gets or creates an element target at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <param name="creation">The factory function to create a new target if one doesn't exist.</param>
    /// <returns>The existing or newly created property target.</returns>
    public PropertyTarget GetOrCreateElementTarget(int index, Func<PropertyTarget> creation)
    {
        return GetOrCreateElementTarget(index, creation, out _);
    }

    /// <summary>
    /// Gets or creates an element target at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <param name="creation">The factory function to create a new target if one doesn't exist.</param>
    /// <param name="created">When this method returns, contains a value indicating whether a new target was created.</param>
    /// <returns>The existing or newly created property target.</returns>
    public PropertyTarget GetOrCreateElementTarget(int index, Func<PropertyTarget> creation, out bool created)
    {
        if ((_list ??= []).GetListItemSafe(index) is PropertyTarget target)
        {
            created = false;
        }
        else
        {
            target = creation();
            if (target is null)
            {
                throw new NullReferenceException();
            }

            target.Parent = _ownerTarget;

            while (_list.Count <= index)
            {
                _list.Add(null);
            }

            _list[index] = target;

            created = true;
        }

        ConfigChildTarget(target);

        target.Index = index;
        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateElementTarget(int index, Action<PropertyTarget>? createConfig = null)
    {
        var elementType = ElementType;

        var target = GetOrCreateElementTarget(index, () =>
        {
            var t = new PropertyTargetBK($"[{index}]", elementType)
            {
                Disabled = _ownerTarget.Disabled,
                ReadOnly = _ownerTarget.ReadOnly,
                CachedTheme = _ownerTarget.CachedTheme,
            };

            t.Getter = CreateArrayItemGetter(t);
            t.Setter = CreateArrayItemSetter(t);

            return t;
        }, out bool created);

        ConfigChildTarget(target);

        if (created)
        {
            createConfig?.Invoke(target);
        }

        return target;
    }

    /// <inheritdoc/>
    public override IEnumerable<PropertyTarget> Elements
    {
        get
        {
            if (!GetArrayLength().Any())
            {
                yield break;
            }

            int len = GetArrayLength().Max();
            if (len <= 0)
            {
                yield break;
            }

            for (int i = 0; i < len; i++)
            {
                yield return GetOrCreateElementTarget(i);
            }
        }
    }

    private void ConfigChildTarget(PropertyTarget target)
    {
        target.Parent = _ownerTarget;
        target.Disabled = _ownerTarget.Disabled;
        target.ReadOnly = _ownerTarget.ReadOnly;
        target.CacheValues = _ownerTarget.CacheValues;
        target.ValueMultiple = false;
        target.IsAbstract = _ownerTarget.IsAbstract;
        target.Status = TextStatus.Normal;
        target.ServiceProvider = _ownerTarget.ServiceProvider;
        target.ClearGetterCache();
    }

    /// <inheritdoc/>
    public override IEnumerable<int> GetArrayLength()
    {
        return _ownerTarget.GetValues().Select(o => _handler.GetLength(o) ?? 0);
    }

    /// <inheritdoc/>
    public override void SetArrayLength(IEnumerable<int> lengths)
    {
        if (!lengths.Any())
        {
            return;
        }

        if (lengths.CountOne())
        {
            var count = lengths.First();

            foreach (var obj in _ownerTarget.GetValues())
            {
                _handler.SetLength(obj, count, CreateArrayItem);
            }
        }
        else
        {
            int[] ary = lengths.ToArray() ?? [];
            int index = 0;

            foreach (var obj in _ownerTarget.GetValues())
            {
                int count = ary.GetArrayItemMinMax(index);
                _handler.SetLength(obj, count, CreateArrayItem);

                index++;
            }
        }

        RaiseEdited();
    }

    /// <inheritdoc/>
    public override int? GetArrayLengthMax()
    {
        var lengths = GetArrayLength();
        if (lengths.Any())
        {
            return lengths.Max();
        }

        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<object?> GetArrayItemAt(int index)
    {
        return _ownerTarget.GetValues().Select(o => _handler.GetItemAt(o, index));
    }

    /// <inheritdoc/>
    public override void SetArrayItemAt(int index, IEnumerable<object> items)
    {
        if (!items.Any())
        {
            return;
        }

        if (items.CountOne())
        {
            var v = items.First();
            bool clone = false;

            foreach (var obj in _ownerTarget.GetValues())
            {
                v = GetValueAutoClone(v, ref clone);

                _handler.SetItemAt(obj, index, v);
            }
        }
        else
        {
            object[] ary = items.ToArray() ?? [];
            int i2 = 0;

            foreach (var obj in _ownerTarget.GetValues())
            {
                var value = GetArrayItemAutoClone(ary, i2);
                _handler.SetItemAt(obj, index, value);

                i2++;
            }
        }

        _list?.GetListItemSafe(index)?.ClearFields();

        RaiseEdited();
    }

    /// <inheritdoc/>
    public override void PushArrayItem(IEnumerable<object?> objects)
    {
        bool edited = false;

        object?[] values = [.. objects];
        int i = 0;

        int minIndex = int.MaxValue;

        foreach (var obj in _ownerTarget.GetValues())
        {
            var value = GetArrayItemAutoClone(values, i);
            if (value is null)
            {
                i++;
                continue;
            }

            if (_handler.GetLength(obj) is int len)
            {
                _handler.InsertItemAt(obj, len, value);
                edited = true;

                if (len < minIndex)
                {
                    minIndex = len;
                }
            }

            i++;
        }

        ClearTargets(minIndex);

        if (edited)
        {
            RaiseEdited();
        }
    }

    /// <inheritdoc/>
    public override void PopArrayItem()
    {
        bool edited = false;

        int minIndex = int.MaxValue;

        foreach (var obj in _ownerTarget.GetValues())
        {
            if (_handler.GetLength(obj) is int len)
            {
                int index = len - 1;

                _handler.RemoveItemAt(obj, index);
                edited = true;

                if (index < minIndex)
                {
                    minIndex = index;
                }
            }
        }

        ClearTargets(minIndex);

        if (edited)
        {
            RaiseEdited();
        }
    }

    /// <inheritdoc/>
    public override void InsertArrayItemAt(int index, IEnumerable<object?> objects)
    {
        if (index < 0)
        {
            return;
        }

        bool edited = false;

        object?[] values = [.. objects];
        int i2 = 0;

        foreach (var obj in _ownerTarget.GetValues())
        {
            var value = GetArrayItemAutoClone(values, i2);
            if (value is null)
            {
                i2++;
                continue;
            }

            if (_handler.GetLength(obj) >= index)
            {
                _handler.InsertItemAt(obj, index, value);
                edited = true;
            }

            i2++;
        }

        ClearTargets(index);

        if (edited)
        {
            RaiseEdited();
        }
    }

    /// <inheritdoc/>
    public override void RemoveArrayItemAt(int index)
    {
        if (index < 0)
        {
            return;
        }

        bool edited = false;

        foreach (var obj in _ownerTarget.GetValues())
        {
            if (_handler.GetLength(obj) > index)
            {
                _handler.RemoveItemAt(obj, index);
                edited = true;
            }
        }

        ClearTargets(index);

        if (edited)
        {
            RaiseEdited();
        }
    }

    /// <inheritdoc/>
    public override void CloneArrayItemAt(int index)
    {
        if (index < 0)
        {
            return;
        }

        bool edited = false;

        foreach (var obj in _ownerTarget.GetValues())
        {
            if (_handler.GetLength(obj) > index)
            {
                var item = _handler.GetItemAt(obj, index);
                var newItem = CloneArrayItem(item);
                _handler.InsertItemAt(obj, index + 1, newItem);
                edited = true;
            }
        }

        ClearTargets(index);

        if (edited)
        {
            RaiseEdited();
        }
    }

    /// <inheritdoc/>
    public override void SwapArrayItemAt(int index)
    {
        if (index < 0)
        {
            return;
        }

        bool edited = false;

        foreach (var obj in _ownerTarget.GetValues())
        {
            if (_handler.GetLength(obj) > index + 1)
            {
                var item1 = _handler.GetItemAt(obj, index);
                var item2 = _handler.GetItemAt(obj, index + 1);

                // Cannot directly use SetItemAt; for parent-child relationships like SArray, setting one will lose the other
                // _handler.SetItemAt(obj, index, item2);
                // _handler.SetItemAt(obj, index + 1, item1);

                _handler.RemoveItemAt(obj, index);
                _handler.RemoveItemAt(obj, index);

                _handler.InsertItemAt(obj, index, item1);
                _handler.InsertItemAt(obj, index, item2);

                edited = true;
            }
        }

        if (_list != null)
        {
            _list.GetListItemSafe(index)?.ClearFields();
            _list.GetListItemSafe(index + 1)?.ClearFields();
        }

        if (edited)
        {
            RaiseEdited();
        }
    }

    private void ClearTargets(int index)
    {
        if (_list is null)
        {
            return;
        }

        int maxLen = _ownerTarget.GetValues().Select(o => _handler.GetLength(o) ?? 0).Max();

        // Clean up unused Targets in the list
        while (_list.Count > maxLen)
        {
            _list.RemoveAt(_list.Count - 1);
        }

        // Clear field records
        for (int i = index; i < _list.Count; i++)
        {
            var target = _list[i];
            target?.ClearFields();
        }
    }


    #endregion

    #region ITarget

    /// <inheritdoc/>
    public override IEnumerable<object?> GetParentObjects()
    {
        // For ArrayTarget, the array is the parent object

        //return _target.GetParentObjects();
        return GetArrays();
    }

    #endregion

    /// <summary>
    /// Creates a new array item instance.
    /// </summary>
    /// <returns>A new instance of the array element type, or null if creation fails.</returns>
    protected virtual object? CreateArrayItem()
    {
        var creation = ItemActivator;

        if (creation != null)
        {
            return creation();
        }
        else if (ElementType != null)
        {
            try
            {
                return Activator.CreateInstance(ElementType);
            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Clones an existing array item.
    /// </summary>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A cloned copy of the object, or null if cloning fails.</returns>
    protected virtual object? CloneArrayItem(object? obj)
    {
        if (obj?.GetType().IsValueType == true)
        {
            return obj;
        }

        var cloner = ItemCloner;
        object? result = null;

        if (cloner != null && obj != null)
        {
            result = cloner(obj);
            if (result != null)
            {
                return result;
            }
        }

        if (obj is SItem sitem)
        {
            return Cloner.Clone(sitem);
        }

        if (obj is ISyncObject syncObj)
        {
            result = Cloner.Clone(syncObj);
            if (result != null)
            {
                return result;
            }
        }

        result = CreateArrayItem();

        return result;
    }

    /// <summary>
    /// Creates a getter function for retrieving array element values.
    /// </summary>
    /// <param name="element">The element target to create the getter for.</param>
    /// <returns>A function that returns the values at the element's index across all arrays.</returns>
    protected virtual Func<IEnumerable<object?>> CreateArrayItemGetter(PropertyTarget element)
    {
        return () => OwnerTarget.GetValues().Select(o =>
        {
            if (_handler.GetLength(o) > 0)
            {
                return _handler.GetItemAt(o, element.Index);
            }
            else
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Creates a setter function for updating array element values.
    /// </summary>
    /// <param name="element">The element target to create the setter for.</param>
    /// <returns>An action that sets values at the element's index across all arrays.</returns>
    protected virtual Action<IEnumerable<object?>, object?> CreateArrayItemSetter(PropertyTarget element)
    {
        return (values, context) =>
        {
            if (values is null || !values.Any())
            {
                return;
            }

            if (values.CountOne())
            {
                var value = values.First();
                bool clone = false;

                foreach (var obj in OwnerTarget.GetValues())
                {
                    if (_handler.GetLength(obj) > element.Index)
                    {
                        value = GetValueAutoClone(value, ref clone);

                        _handler.SetItemAt(obj, element.Index, value);
                    }
                }
            }
            else
            {
                object?[] ary = [.. values];

                int i = 0;
                foreach (var obj in OwnerTarget.GetValues())
                {
                    if (_handler.GetLength(obj) > element.Index)
                    {
                        object? value = GetArrayItemAutoClone(ary, i);

                        _handler.SetItemAt(obj, element.Index, value);
                    }

                    i++;
                }
            }

            RaiseEdited();
        };
    }

    /// <summary>
    /// Raises the edited event to notify that array contents have changed.
    /// </summary>
    protected virtual void RaiseEdited()
    {
        if (_ownerTarget.WriteBack)
        {
            object?[] values = [.. _ownerTarget.GetValues()];
            _ownerTarget.SetValues(values);
        }
    }


    private object? GetValueAutoClone<T>(T value, ref bool clone)
    {
        if (clone)
        {
            return CloneArrayItem(value);
        }
        else
        {
            clone = true;

            return value;
        }
    }

    private object? GetArrayItemAutoClone<T>(T[] ary, int index)
    {
        if (ary is null || ary.Length == 0)
        {
            return default;
        }

        if (index < 0)
        {
            return CloneArrayItem(ary[0]);
        }
        else if (index >= ary.Length)
        {
            return CloneArrayItem(ary[^1]);
        }
        else
        {
            return ary[index];
        }
    }
}