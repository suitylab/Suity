using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// Represents a root property target that holds a collection of objects as the editing entry point.
/// </summary>
public class RootPropertyTarget : PropertyTargetBK
{
    private object[]? _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget"/> class with the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the root property.</param>
    public RootPropertyTarget(string propertyName)
        : base(propertyName, typeof(object))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget"/> class with the specified values.
    /// </summary>
    /// <param name="values">The collection of objects to edit.</param>
    public RootPropertyTarget(IEnumerable<object> values)
        : this("Root", values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget"/> class with the specified values and property name.
    /// </summary>
    /// <param name="values">The collection of objects to edit.</param>
    /// <param name="propertyName">The name of the root property.</param>
    public RootPropertyTarget(IEnumerable<object> values, string propertyName)
        : this(propertyName, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget"/> class with the specified name and values.
    /// </summary>
    /// <param name="name">The name of the root property.</param>
    /// <param name="values">The collection of objects to edit.</param>
    public RootPropertyTarget(string name, IEnumerable<object> values)
        : base(name, values.GetCommonType())
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = [.. values];

        Getter = () => _values.Cast<object>();
        Setter = (values, context) =>
        {
            _values = [.. values.Cast<object>()];
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget"/> class with the specified name and values.
    /// </summary>
    /// <param name="name">The name of the root property.</param>
    /// <param name="values">The array of objects to edit.</param>
    public RootPropertyTarget(string name, params object[] values)
        : base(name, values.GetCommonType())
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = [.. values];

        Getter = () => _values.Cast<object>();
        Setter = (values, context) =>
        {
            _values = [.. values.Cast<object>()];
        };
    }

    /// <inheritdoc/>
    public override bool IsRoot => true;

    /// <summary>
    /// Gets the collection of objects being edited by this root target.
    /// </summary>
    public IEnumerable<object> Values => _values ?? [];

    // public override IEnumerable<object?> GetParentObjects() => _values ?? EmptyArray<object>.Empty;

    /// <inheritdoc/>
    public override IEnumerable<object?> GetValues() => _values.Pass() ?? [];
}

/// <summary>
/// Represents a generic root property target that holds a collection of typed objects as the editing entry point.
/// </summary>
/// <typeparam name="T">The type of objects being edited.</typeparam>
public class RootPropertyTarget<T> : PropertyTargetBK
{
    private T[]? _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget{T}"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the root property.</param>
    public RootPropertyTarget(string name)
        : base(name, typeof(T))
    {
        _values = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget{T}"/> class with the specified name and values.
    /// </summary>
    /// <param name="name">The name of the root property.</param>
    /// <param name="values">The collection of typed objects to edit.</param>
    public RootPropertyTarget(string name, IEnumerable<T> values)
        : base(name, typeof(T))
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = [.. values];

        Getter = () => _values.Cast<object>();
        Setter = (values, context) =>
        {
            _values = [.. values.Cast<T>()];
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPropertyTarget{T}"/> class with the specified name and values.
    /// </summary>
    /// <param name="name">The name of the root property.</param>
    /// <param name="values">The array of typed objects to edit.</param>
    public RootPropertyTarget(string name, params T[] values)
        : base(name, typeof(T))
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        _values = [.. values];

        Getter = () => _values.Cast<object>();
        Setter = (values, context) =>
        {
            _values = [.. values.Cast<T>()];
        };
    }

    /// <inheritdoc/>
    public override bool IsRoot => true;

    /// <summary>
    /// Gets the collection of typed objects being edited by this root target.
    /// </summary>
    public IEnumerable<T> Values => _values ?? [];
    // public override IEnumerable<object?> GetParentObjects() => _values?.OfType<object>() ?? EmptyArray<object>.Empty;
}