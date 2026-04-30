using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Abstract base class for a typed value item in a value collection.
/// </summary>
public abstract class ValueItem
{
    /// <summary>
    /// Gets the type of value this item holds.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// Gets the current value.
    /// </summary>
    /// <returns>The value as an object.</returns>
    public abstract object? GetValue();

    /// <summary>
    /// Creates a clone of this item for another parent collection.
    /// </summary>
    /// <param name="otherParent">The target parent collection.</param>
    /// <returns>A cloned value item.</returns>
    public abstract ValueItem Clone(ValueCollection otherParent);

    /// <summary>
    /// Clears the stored value.
    /// </summary>
    public abstract void Clear();
}

/// <summary>
/// A typed value item that supports inheritance from a base collection.
/// </summary>
/// <typeparam name="T">The type of value, must be a class.</typeparam>
public class ValueItem<T> : ValueItem
    where T : class
{
    private readonly ValueCollection _collection;
    private readonly ValueItem<T>? _baseItem;

    private T? _value;

    /// <summary>
    /// Initializes a new value item linked to a parent collection.
    /// </summary>
    /// <param name="parent">The parent value collection.</param>
    public ValueItem(ValueCollection parent)
    {
        _collection = parent ?? throw new ArgumentNullException(nameof(parent));

        if (_collection.BaseValues is { } baseValues)
        {
            _baseItem = baseValues.EnsureValueItem<T>();
        }
    }

    /// <summary>
    /// Initializes a new value item with an initial value.
    /// </summary>
    /// <param name="parent">The parent value collection.</param>
    /// <param name="value">The initial value.</param>
    public ValueItem(ValueCollection parent, T? value)
        : this(parent)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the parent collection.
    /// </summary>
    public ValueCollection Collection => _collection;

    /// <summary>
    /// Gets or sets the value. Falls back to the base item's value if not set.
    /// </summary>
    public T? Value
    {
        get => _value ?? _baseItem?.Value;
        set => _value = value;
    }

    /// <inheritdoc/>
    public override Type ValueType => typeof(T);

    /// <inheritdoc/>
    public override object? GetValue() => _value ?? _baseItem?.Value;

    /// <inheritdoc/>
    public override ValueItem Clone(ValueCollection otherParent)
    {
        return new ValueItem<T>(otherParent, _value);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        _value = null;
    }
}

/// <summary>
/// Gui value cache collection
/// </summary>
public sealed class ValueCollection : IValueCollection
{
    private readonly ValueCollection? _baseValues;

    private readonly Dictionary<Type, ValueItem> _values = [];
    private Dictionary<string, ITransitionFactory>? _transitions;

    // ReSharper disable once EmptyConstructor
    public ValueCollection()
    {
    }

    public ValueCollection(ValueCollection? baseValues)
    {
        _baseValues = baseValues;
    }

    public ValueCollection? BaseValues => _baseValues;

    #region Value

    public T? GetValue<T>() where T : class
    {
        //if (_values.TryGetValue(typeof(T), out var item) && item is ValueItem<T> tItem)
        //{
        //    return tItem.Value;
        //}
        //else
        //{
        //    return _baseValues?.GetValue<T>();
        //    //return null;
        //}

        return EnsureValueItem<T>().Value;
    }

    public object? GetValue(Type type)
    {
        return EnsureValueItem(type).GetValue();
    }

    public void SetValue<T>(T value) where T : class
    {
        if (_values.TryGetValue(typeof(T), out var item) && item is ValueItem<T> tItem)
        {
            tItem.Value = value;
        }
        else
        {
            _values[typeof(T)] = new ValueItem<T>(this, value);
        }
    }

    public void SetValue<T>(T value, out bool valueSet) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (_values.TryGetValue(typeof(T), out var item) && item is ValueItem<T> tItem)
        {
            if (Equals(tItem.Value, value))
            {
                valueSet = false;
                return;
            }

            tItem.Value = value;
        }
        else
        {
            _values[typeof(T)] = new ValueItem<T>(this, value);
        }

        valueSet = true;
    }

    public ValueItem EnsureValueItem(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsValueType)
        {
            throw new ArgumentException("Type must be class.", nameof(type));
        }

        if (_values.TryGetValue(type, out var item) && item is ValueItem tItem && tItem.ValueType == type)
        {
            return tItem;
        }
        else
        {
            var itemT = typeof(ValueItem<>).MakeGenericType(type);

            var newItem = Activator.CreateInstance(itemT, this) as ValueItem;

            if (newItem is null)
            {
                throw new NullReferenceException($"Cannot create ValueItem for type {type}.");
            }

            _values[type] = newItem;
            return newItem;
        }
    }


    public ValueItem<T> EnsureValueItem<T>() where T : class
    {
        if (_values.TryGetValue(typeof(T), out var item) && item is ValueItem<T> tItem)
        {
            return tItem;
        }
        else
        {
            tItem = new ValueItem<T>(this);
            _values[typeof(T)] = tItem;

            return tItem;
        }
    }

    public bool RemoveValue<T>() where T : class
    {
        if (_values.TryGetValue(typeof(T), out var item) && item is ValueItem<T> tItem)
        {
            tItem.Value = null;

            return true;
        }

        return false;
    }

    public T GetOrCreateValue<T>(Func<T> creation) where T : class
    {
        var value = GetValue<T>();

        if (value != null)
        {
            return value;
        }
        else
        {
            value = creation();
            SetValue(value);

            return value;
        }
    }

    public T GetOrCreateValue<T>(Func<T> creation, out bool created) where T : class
    {
        var value = GetValue<T>();

        if (value != null)
        {
            created = false;

            return value;
        }
        else
        {
            value = creation();
            SetValue(value);
            created = true;

            return value;
        }
    }

    public T GetOrCreateValue<T>() where T : class, new()
    {
        var value = GetValue<T>();

        if (value != null)
        {
            return value;
        }
        else
        {
            value = new T();
            SetValue(value);

            return value;
        }
    }

    public T GetOrCreateValue<T>(out bool created) where T : class, new()
    {
        var value = GetValue<T>();

        if (value != null)
        {
            created = false;

            return value;
        }
        else
        {
            value = new T();
            SetValue(value);
            created = true;

            return value;
        }
    }

    public void DoValueAction<T>(Action<T> action) where T : class
    {
        var value = GetValue<T>();
        if (value != null)
        {
            action(value);
        }
    }

    public void Clear()
    {
        foreach (var item in _values.Values)
        {
            item.Clear();
        }
    }

    public int Count => _values.Count;

    public bool IsEmpty => _values.Count == 0;

    public IEnumerable<object> Values => _values.Values.Select(o => o.GetValue()).SkipNull()!;

    #endregion

    #region Transition

    public void SetTransition(string? targetState, ITransitionFactory transition)
    {
        targetState ??= string.Empty;
        if (transition is null)
        {
            throw new ArgumentNullException(nameof(transition));
        }

        (_transitions ??= [])[targetState] = transition;
    }

    public void SetTransition(string? targetState, ITransitionFactory transition, out bool valueSet)
    {
        targetState ??= string.Empty;
        if (transition is null)
        {
            throw new ArgumentNullException(nameof(transition));
        }

        if (_transitions?.TryGetValue(targetState, out ITransitionFactory? current) == true && Equals(current, transition))
        {
            valueSet = false;
            return;
        }

        (_transitions ??= [])[targetState] = transition;
        valueSet = true;
    }

    public bool RemoveTransition(string? targetState)
    {
        targetState ??= string.Empty;

        bool removed = _transitions?.Remove(targetState) == true;
        if (removed)
        {
            if (_transitions!.Count == 0)
            {
                _transitions = null;
            }

            return true;
        }

        return false;
    }

    public ITransitionFactory? GetTransition(string? targetState)
    {
        targetState ??= string.Empty;

        return _transitions?.GetValueSafe(targetState);
    }

    #endregion

    public void CopyTo(ValueCollection other)
    {
        if (Equals(other))
        {
            throw new ArgumentException(nameof(other));
        }

        if (_values.Count == 0)
        {
            return;
        }

        foreach (var pair in _values)
        {
            other._values[pair.Key] = pair.Value.Clone(other);
        }

        if (_transitions is { })
        {
            other._transitions ??= [];

            foreach (var pair in _transitions)
            {
                other._transitions[pair.Key] = pair.Value;
            }
        }
    }

    public void CopyTo(ref ValueCollection? other)
    {
        if (Equals(other))
        {
            throw new ArgumentException(nameof(other));
        }

        if (_values.Count == 0)
        {
            return;
        }

        other ??= new();

        foreach (var pair in _values)
        {
            other._values[pair.Key] = pair.Value.Clone(other);
        }

        if (_transitions is { })
        {
            other._transitions ??= [];

            foreach (var pair in _transitions)
            {
                other._transitions[pair.Key] = pair.Value;
            }
        }
    }
}


/// <summary>
/// Value and style storage unit
/// </summary>
/// <typeparam name="T">The type of value, must be a class with a parameterless constructor.</typeparam>
public sealed class ValueStyleSlot<T>(ValueItem<T> value) where T : class, new()
{
    private readonly ValueItem<T> _value = value ?? throw new ArgumentNullException(nameof(value));
    private ValueItem<T>? _style;

    /// <summary>
    /// Represents the value of itself
    /// </summary>
    public ValueItem<T> Value => _value;

    /// <summary>
    /// Represents the value passed from outside
    /// </summary>
    public ValueItem<T>? Style
    {
        get => _style;
        set => _style = value;
    }

    /// <summary>
    /// Gets or creates the value using the default constructor.
    /// </summary>
    /// <returns>The existing or newly created value.</returns>
    public T GetOrCreateValue()
    {
        return _value.Value ??= new();
    }

    /// <summary>
    /// Gets or creates the value using the provided factory function.
    /// </summary>
    /// <param name="creation">The factory function to create the value.</param>
    /// <returns>The existing or newly created value.</returns>
    public T GetOrCreateValue(Func<T> creation)
    {
        return _value.Value ??= creation();
    }

    /// <summary>
    /// Removes the stored value.
    /// </summary>
    public void RemoveValue()
    {
        _value.Value = null;
    }

    /// <summary>
    /// Gets the value considering flags and animation state.
    /// </summary>
    /// <param name="flag">The node flags that affect value resolution order.</param>
    /// <param name="animation">The current animation, if any.</param>
    /// <returns>The resolved value, or null if not available.</returns>
    public T? GetValue(ImGuiNodeFlags flag, IGuiAnimation? animation)
    {
        // Temporarily support getting values from _values
        if (flag.HasFlag(ImGuiNodeFlags.OverrideDisabled))
        {
            return
                animation?.GetValue<T>()
                ?? _style?.Value
                ///////////////////////////////////
                ?? _value?.Value;
        }
        else
        {
            return
                _value?.Value
                ///////////////////////////////////
                ?? animation?.GetValue<T>()
                ?? _style?.Value;
        }
    }
}