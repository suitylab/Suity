using Suity;
using Suity.Collections;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// Backing implementation of <see cref="PropertyTarget"/> that manages property fields, caching, and value operations.
/// </summary>
public class PropertyTargetBK : PropertyTarget
{
    private Dictionary<string, PropertyTarget>? _fields;
    private List<ColumnPropertyTarget>? _columns;
    private PropertyTarget? _parent;
    private ArrayTargetBK? _arrayTarget;
    private bool _errorSelf;
    private bool _valueMultiple;

    // PropertyTarget caches GetValue() results; otherwise, deeply nested values would repeatedly call GetValue() up the parent chain. Re-fetching PropertyTarget clears the cache
    private object?[]? _getterCache;

    private TextStatus _textStatus;

    private string _id;
    private string _propName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyTargetBK"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public PropertyTargetBK(string name)
        : this(name, typeof(object))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyTargetBK"/> class with the specified name and edited type.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="editedType">The type being edited, or null to use object.</param>
    public PropertyTargetBK(string name, Type? editedType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
        }

        _id = _propName = name;
        PresetType = editedType ?? typeof(object);
    }

    /// <inheritdoc/>
    public override PropertyTarget? Parent
    {
        get => _parent;
        set
        {
            _parent = value;
            _id = _parent != null ? $"{_parent.Id}.{_propName}" : _propName;
        }
    }

    /// <inheritdoc/>
    public override ArrayTarget? ArrayTarget => _arrayTarget;

    /// <inheritdoc/>
    public override string Id => _id;

    /// <inheritdoc/>
    public override string PropertyName => _propName;

    /// <inheritdoc/>
    public override TextStatus Status
    {
        get => _textStatus;
        set => _textStatus = value;
    }

    /// <inheritdoc/>
    public override bool ValueMultiple
    {
        get => _valueMultiple;
        set => _valueMultiple = value;
    }

    /// <inheritdoc/>
    public override bool ErrorSelf
    {
        get => _errorSelf;
        set
        {
            if (_errorSelf != value)
            {
                _errorSelf = value;
            }
        }
    }

    /// <inheritdoc/>
    public override bool ErrorInHierarchy
    {
        get
        {
            if (_errorSelf)
            {
                return true;
            }

            if (_fields?.Values.Any(o => o.ErrorInHierarchy) == true)
            {
                return true;
            }

            if (_arrayTarget?.ContainsError == true)
            {
                return true;
            }

            return false;
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<object?> GetValues()
    {
        if (_getterCache is null || !CacheValues)
        {
            _getterCache = [.. OnGetValues()];

            _valueMultiple = false;

            do
            {
                if (_getterCache.Length <= 1)
                {
                    break;
                }

                if (_getterCache.Any(o => o is null))
                {
                    _valueMultiple = true;
                    break;
                }

                object first = _getterCache[0]!;
                Type type = first.GetType();

                if (type.IsValueType || type == typeof(string))
                {
                    if (!_getterCache.AllEqual())
                    {
                        _valueMultiple = true;
                        break;
                    }
                }

                if (_getterCache.Skip(1).Any(o => !Equality.ObjectEquals(first, o)))
                {
                    _valueMultiple = true;
                    break;
                }
            } while (false);
        }

        return _getterCache;
    }

    /// <inheritdoc/>
    public override void SetValues(IEnumerable<object?> objects, ISetterContext? context = null)
    {
        OnSetValues(objects, context);
        _getterCache = null;

        if (IsAbstract)
        {
            _fields?.Clear();
        }

        RaiseEdited(context);
    }

    /// <inheritdoc/>
    public override void DoWriteBack(ISetterContext? context)
    {
        if (_parent is { })
        {
            _parent.SetValues(_parent.GetValues().ToArray(), context);
        }
    }

    /// <inheritdoc/>
    public override void ClearGetterCache()
    {
        _getterCache = null;
    }

    /// <inheritdoc/>
    public override void ClearFields() => _fields?.Clear();

    /// <inheritdoc/>
    public override PropertyTarget? GetField(string name) => _fields?.GetValueSafe(name);

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField(string name, Func<PropertyTarget> creation, out bool created)
    {
        if ((_fields ??= []).TryGetValue(name, out var target))
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

            ConfigChildTarget(target);

            _fields.Add(name, target);
            created = true;
        }

        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Action<TObject, TValue, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        var target = GetOrCreateField(name, () =>
        {
            var t = new PropertyTargetBK(name, typeof(TValue))
            {
                Getter = CreateFieldGetter(getter),
                Setter = setter != null ? CreateFieldSetter(setter) : null,
                Disabled = Disabled,
                ReadOnly = ReadOnly,
                WriteBack = typeof(TObject).IsValueType,
                CacheValues = CacheValues,
                CachedTheme = CachedTheme,
            };

            return t;
        }, out bool created);

        ConfigChildTarget(target);

        if (created)
        {
            creationConfig?.Invoke(target);
        }

        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateField<TObject>(
        string name,
        Type editedType,
        Func<TObject, object?> getter,
        Action<TObject, object, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        var target = GetOrCreateField(name, () =>
        {
            var t = new PropertyTargetBK(name)
            {
                Getter = CreateFieldGetter(getter),
                Setter = setter != null ? CreateFieldSetter(setter) : null,
                Disabled = Disabled,
                ReadOnly = ReadOnly,
                WriteBack = typeof(TObject).IsValueType,
                CacheValues = CacheValues,
                CachedTheme = CachedTheme,
            };

            return t;
        }, out bool created);

        target.PresetType = editedType ?? throw new ArgumentNullException(nameof(editedType));

        ConfigChildTarget(target);

        if (created)
        {
            creationConfig?.Invoke(target);
        }

        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget GetOrCreateStructField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Func<TObject, TValue, ISetterContext?, TObject>? setter = null,
        Action<PropertyTarget>? creationConfig = null)
    {
        var target = GetOrCreateField(name, () =>
        {
            var t = new PropertyTargetBK(name, typeof(TValue))
            {
                Getter = CreateFieldGetter(getter),
                Setter = setter != null ? CreateFieldStructSetter(setter) : null,
                Disabled = Disabled,
                ReadOnly = ReadOnly,
                WriteBack = typeof(TObject).IsValueType,
                CacheValues = CacheValues,
                CachedTheme = CachedTheme,
            };

            return t;
        }, out bool created);

        ConfigChildTarget(target);

        if (created)
        {
            creationConfig?.Invoke(target);
        }

        return target;
    }

    /// <inheritdoc/>
    public override IEnumerable<PropertyTarget> Fields
        => _fields?.Values ?? (IEnumerable<PropertyTarget>)[];

    /// <inheritdoc/>
    public override int FieldCount => _fields?.Count ?? 0;

    /// <inheritdoc/>
    public override PropertyTarget GetColumnTarget(int index)
    {
        _columns ??= [];

        while (_columns.Count <= index)
        {
            _columns.Add(new ColumnPropertyTarget(this, _columns.Count));
        }

        var target = _columns[index];

        ConfigChildTarget(target);
        target.Styles = Styles;

        return target;
    }

    /// <inheritdoc/>
    public override PropertyTarget CreateConvertedTarget(TargetConversion convert, TargetRevertConversion convertRevert)
    {
        return new ConvertedPropertyTarget(this, convert, convertRevert);
    }

    /// <inheritdoc/>
    public override IEnumerable<object?> GetParentObjects()
    {
        return _parent?.GetValues() ?? [];
    }

    private Func<IEnumerable<object?>> CreateFieldGetter<TObject, TValue>(Func<TObject, TValue?> getter)
    {
        return () => GetValues().Select(o =>
        {
            if (o is TObject to)
            {
                return (object)getter(to)!;
            }
            else
            {
                return null;
            }
        });
    }

    private Action<IEnumerable<object?>, ISetterContext?> CreateFieldSetter<TObject, TValue>(Action<TObject, TValue, ISetterContext?> setter)
    {
        return (values, context) =>
        {
            if (values is null || !values.Any())
            {
                return;
            }

            TValue? last = default;

            if (values.CountOne() && values.First() is TValue tOneValue)
            {
                TValue tValue = tOneValue;

                foreach (var obj in GetValues())
                {
                    // Clone when setting multiple values
                    if (last is TValue tLast && ReferenceEquals(last, tOneValue))
                    {
                        tValue = Cloner.Clone(tLast);
                    }
                    else
                    {
                        last = tValue = tOneValue;
                    }

                    if (obj is TObject tobj)
                    {
                        setter(tobj, tValue, context);
                    }
                }
            }
            else
            {
                TValue[] ary = values?.SafeCast<TValue>().ToArray() ?? [];

                if (ary.Length > 0)
                {
                    int i = 0;
                    foreach (var obj in GetValues())
                    {
                        if (obj is TObject tobj)
                        {
                            TValue tValue = ary.GetArrayItemMinMax(i);
                            // Clone when setting multiple values
                            if (last is TValue tLast && ReferenceEquals(last, tValue))
                            {
                                tValue = Cloner.Clone(tLast);
                            }
                            else
                            {
                                last = tValue;
                            }

                            setter(tobj, tValue, context);
                        }

                        i++;
                    }
                }
            }

            RaiseEdited(context);
        };
    }

    private Action<IEnumerable<object?>, ISetterContext?> CreateFieldStructSetter<TObject, TValue>(Func<TObject, TValue, ISetterContext?, TObject> setter)
    {
        return (values, context) =>
        {
            if (values is null || !values.Any())
            {
                return;
            }

            List<object?> writeBack = [];

            if (values.CountOne() && values.First() is TValue tValue)
            {
                foreach (var obj in GetValues())
                {
                    if (obj is TObject tobj)
                    {
                        writeBack.Add(setter(tobj, tValue, context) ?? obj);
                    }
                    else
                    {
                        writeBack.Add(obj);
                    }
                }

                SetValues(writeBack, context);
            }
            else
            {
                TValue[] ary = values?.Cast<TValue>().ToArray() ?? [];

                if (ary.Length > 0)
                {
                    int i = 0;
                    foreach (var obj in GetValues())
                    {
                        if (obj is TObject tobj)
                        {
                            writeBack.Add(setter(tobj, ary.GetArrayItemMinMax(i), context) ?? obj);
                        }
                        else
                        {
                            writeBack.Add(obj);
                        }

                        i++;
                    }

                    SetValues(writeBack, context);
                }
            }

            RaiseEdited(context);
        };
    }

    private void ConfigChildTarget(PropertyTarget target)
    {
        target.Parent = this;
        target.Disabled = Disabled;
        target.ReadOnly = ReadOnly;
        target.ValueMultiple = false;
        target.Status = TextStatus.Normal;
        target.ServiceProvider = ServiceProvider;
        target.ClearGetterCache();
    }

    /// <inheritdoc/>
    internal override void SetupArray(ArrayHandler handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _arrayTarget = new ArrayTargetBK(this, handler);
    }
}