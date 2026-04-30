using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// A property target that represents a single column view of a multi-column property, delegating most operations to the original target.
/// </summary>
internal class ColumnPropertyTarget : PropertyTargetBK
{
    private readonly PropertyTarget _origin;
    private readonly int _columnIndex;

    private Dictionary<string, ColumnPropertyTarget>? _fields;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnPropertyTarget"/> class.
    /// </summary>
    /// <param name="origin">The original property target to wrap.</param>
    /// <param name="columnIndex">The zero-based index of the column this target represents.</param>
    public ColumnPropertyTarget(PropertyTarget origin, int columnIndex)
        : base(origin.PropertyName, origin.PresetType)
    {
        _origin = origin ?? throw new ArgumentNullException(nameof(origin));
        _columnIndex = columnIndex;

        Parent = origin.Parent;
        Index = origin.Index;
        Description = origin.Description;
        Status = origin.Status;
        Disabled = origin.Disabled;
        ReadOnly = origin.ReadOnly;
        Optional = origin.Optional;
        Styles = origin.Styles;
        WriteBack = origin.WriteBack;
        CacheValues = origin.CacheValues;
        InitExpanded = origin.InitExpanded;
        Color = origin.Color;
        SupportMultipleColumn = false;
        Attributes = origin.Attributes;
        ServiceProvider = origin.ServiceProvider;
        CachedTheme = origin.CachedTheme;

        if (origin.ArrayTarget is { })
        {
            SetupArray(origin.ArrayTarget.Handler);
        }

        foreach (var field in origin.Fields)
        {
            if (!string.IsNullOrWhiteSpace(field.PropertyName))
            {
                GetOrCreateField(field.PropertyName, () => new ColumnPropertyTarget(field, columnIndex));
            }
        }
    }

    /// <inheritdoc/>
    public override bool IsRoot => _origin.IsRoot;

    /// <inheritdoc/>
    public override bool ValueMultiple
    {
        get => base.ValueMultiple || _origin.ValueMultiple;
        set => base.ValueMultiple = value;
    }

    /// <inheritdoc/>
    public override void ClearFields() => _fields?.Clear();

    /// <inheritdoc/>
    public override PropertyTarget? GetField(string name)
    {
        if ((_fields ??= []).TryGetValue(name, out var target))
        {
            return target;
        }
        else
        {
            var originTarget = _origin.GetField(name);
            target = (ColumnPropertyTarget)originTarget?.GetColumnTarget(_columnIndex)!;
            _fields[name] = target;
        }

        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField(string name, Func<PropertyTarget> creation, out bool created)
    {
        created = false;

        if ((_fields ??= []).TryGetValue(name, out var target))
        {
            return target;
        }
        else
        {
            var originTarget = _origin.GetOrCreateField(name, creation, out created);
            target = (ColumnPropertyTarget)originTarget?.GetColumnTarget(_columnIndex)!;
            _fields[name] = target;
        }

        return target!;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Action<TObject, TValue, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        if ((_fields ??= []).TryGetValue(name, out var target))
        {
        }
        else
        {
            var originTarget = _origin.GetOrCreateField(name, getter, setter, creationConfig);
            target = (ColumnPropertyTarget)originTarget?.GetColumnTarget(_columnIndex)!;
            _fields[name] = target;
        }

        return target!;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField<TObject>(
        string name,
        Type editedType,
        Func<TObject, object?> getter,
        Action<TObject, object, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        if ((_fields ??= []).TryGetValue(name, out var target))
        {
        }
        else
        {
            var originTarget = _origin.GetOrCreateField(name, editedType, getter, setter, creationConfig);
            target = (ColumnPropertyTarget)originTarget?.GetColumnTarget(_columnIndex)!;
            _fields[name] = target;
        }

        return target!;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateStructField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Func<TObject, TValue, ISetterContext?, TObject>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        if ((_fields ??= []).TryGetValue(name, out var target))
        {
        }
        else
        {
            var originTarget = _origin.GetOrCreateStructField(name, getter, setter, creationConfig);
            target = (ColumnPropertyTarget)originTarget?.GetColumnTarget(_columnIndex)!;
            _fields[name] = target;
        }

        return target!;
    }

    /// <inheritdoc/>
    public override IEnumerable<PropertyTarget> Fields
    {
        get
        {
            _fields ??= [];

            foreach (var originTarget in _origin.Fields)
            {
                if (!_fields.TryGetValue(originTarget.PropertyName, out var target))
                {
                    target = (ColumnPropertyTarget)originTarget.GetColumnTarget(_columnIndex)!;
                    _fields[originTarget.PropertyName] = target;
                }

                yield return target;
            }
        }
    }

    /// <inheritdoc/>
    public override int FieldCount => _origin.FieldCount;

    /// <inheritdoc/>
    protected override IEnumerable<object?> OnGetValues() => _origin.GetValues().Skip(_columnIndex).Take(1);

    /// <inheritdoc/>
    public override IEnumerable<object?> GetParentObjects() => _origin.GetParentObjects().Skip(_columnIndex).Take(1);

    /// <inheritdoc/>
    protected override void OnSetValues(IEnumerable<object?> objects, ISetterContext? context)
    {
        if (!objects.Any())
        {
            return;
        }

        var ary = _origin.GetValues().ToArray();
        if (ary.Length <= _columnIndex)
        {
            return;
        }

        ary[_columnIndex] = objects.First();

        _origin.SetValues(ary, context);
    }
}