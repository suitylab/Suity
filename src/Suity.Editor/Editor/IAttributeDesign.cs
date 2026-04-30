using Suity;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

#region IAttributeDesign
/// <summary>
/// Provides methods to manage design attributes for a property or element.
/// </summary>
public interface IAttributeDesign : IAttributeGetter
{
    /// <summary>
    /// Gets the number of attributes in this design.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds a new attribute of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of attribute to add.</typeparam>
    /// <returns>The newly created attribute instance.</returns>
    T AddAttribute<T>() where T : DesignAttribute, new();
    /// <summary>
    /// Sets an attribute of the specified type, adding one if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of attribute to set.</typeparam>
    /// <returns>The attribute instance.</returns>
    T SetAttribute<T>() where T : DesignAttribute, new();
    /// <summary>
    /// Removes the specified attribute from this design.
    /// </summary>
    /// <param name="attibute">The attribute to remove.</param>
    void RemoveAttribute(DesignAttribute attibute);
}
#endregion

#region EmptyAttributeDesign
/// <summary>
/// An empty implementation of <see cref="IAttributeDesign"/> that contains no attributes.
/// </summary>
public sealed class EmptyAttributeDesign : IAttributeDesign
{
    /// <summary>
    /// Gets the singleton instance of an empty attribute design.
    /// </summary>
    public static EmptyAttributeDesign Empty { get; } = new();

    private EmptyAttributeDesign()
    {
    }

    /// <inheritdoc />
    public int Count => 0;

    /// <inheritdoc />
    public IEnumerable<object> GetAttributes(string typeName) => [];

    /// <inheritdoc />
    public IEnumerable<object> GetAttributes() => [];

    /// <inheritdoc />
    public IEnumerable<T> GetAttributes<T>() where T : class => [];

    /// <inheritdoc />
    public T AddAttribute<T>() where T : DesignAttribute, new() => null;

    /// <inheritdoc />
    public T SetAttribute<T>() where T : DesignAttribute, new() => null;

    /// <inheritdoc />
    public void RemoveAttribute(DesignAttribute attibute) { }
}

#endregion

#region SArrayAttributeDesign

/// <summary>
/// An attribute design implementation that stores attributes in an <see cref="SArray"/>.
/// </summary>
public sealed class SArrayAttributeDesign : IAttributeDesign
{
    /// <summary>
    /// Raised when an attribute is added to this design.
    /// </summary>
    public event Action<DesignAttribute> AttributeAdded;
    /// <summary>
    /// Raised when an attribute is removed from this design.
    /// </summary>
    public event Action<DesignAttribute> AttributeRemoved;

    /// <summary>
    /// Gets the underlying <see cref="SArray"/> containing the attributes.
    /// </summary>
    public SArray Array { get; }

    /// <inheritdoc />
    public int Count => Array?.Count ?? 0;

    /// <summary>
    /// Creates a new instance of <see cref="SArrayAttributeDesign"/> with an empty array.
    /// </summary>
    public SArrayAttributeDesign()
    {
        Array = new SArray(NativeTypes.AttributeType.MakeArrayType());
    }

    private SArrayAttributeDesign(SArrayAttributeDesign other)
    {
        Array = Cloner.Clone(other.Array);
    }

    /// <inheritdoc />
    public IEnumerable<object> GetAttributes(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return [];
        }

        if (Array is null)
        {
            return [];
        }

        return Array.Items
            .OfType<SObject>()
            .Where(o => !o.IsComment && o.ObjectType.Target?.FullTypeName == typeName);
    }

    /// <inheritdoc />
    public IEnumerable<object> GetAttributes()
    {
        if (Array is null)
        {
            return [];
        }

        return Array.Items
            .OfType<SObject>()
            .Where(o => !o.IsComment);
    }

    /// <inheritdoc />
    public IEnumerable<T> GetAttributes<T>() where T : class
    {
        if (Array is null)
        {
            return [];
        }

        return Array.Items
            .OfType<SObject>()
            .Where(o => !o.IsComment)
            .Select(o => o.Controller)
            .OfType<T>();
    }

    /// <summary>
    /// Creates a clone of this attribute design.
    /// </summary>
    /// <returns>A new <see cref="SArrayAttributeDesign"/> with cloned attributes.</returns>
    public IAttributeDesign Clone()
    {
        return new SArrayAttributeDesign(this);
    }

    /// <inheritdoc />
    public object GetAttribute(string typeName) => GetAttributes(typeName).FirstOrDefault();

    /// <inheritdoc />
    public T GetAttribute<T>() where T : class => GetAttributes<T>().FirstOrDefault();


    /// <inheritdoc />
    public T AddAttribute<T>() where T : DesignAttribute, new()
    {
        //var dtype = TypeDefinition.FromNative(typeof(T))?.Target as DCompond
        //    ?? throw new InvalidOperationException($"Can not get {nameof(DCompond)} from {typeof(T).Name}");

        var inputType = Array.InputType.ElementType;
        var attr = inputType.CreateControllerObject<T>();
        if (attr != null)
        {
            Array.Add(attr);

            try
            {
                AttributeAdded?.Invoke(attr);
            }
            catch (Exception err)
            {
                err.LogError("Add attribute failed.");
            }
        }

        return attr;
    }

    /// <inheritdoc />
    public T SetAttribute<T>() where T : DesignAttribute, new()
    {
        var obj = GetAttribute<T>();
        if (obj != null)
        {
            return obj;
        }
        else
        {
            return AddAttribute<T>();
        }
    }

    /// <inheritdoc />
    public void RemoveAttribute(DesignAttribute attibute)
    {
        if (attibute?.Target is { } obj)
        {
            Array.RemoveItem(obj);

            try
            {
                AttributeRemoved?.Invoke(attibute);
            }
            catch (Exception err)
            {
                err.LogError("Remove attribute failed.");
            }
        }
    }
}

#endregion

#region AttrubuteDesignExtensions

/// <summary>
/// Extension methods for working with <see cref="IAttributeDesign"/> and related types.
/// </summary>
public static class AttrubuteDesignExtensions
{
    /// <summary>
    /// Gets all attribute type names from the specified getter.
    /// </summary>
    /// <param name="getter">The attribute getter to query.</param>
    /// <returns>An enumerable of type names.</returns>
    public static IEnumerable<string> GetAttributeTypeNames(this IAttributeGetter getter)
    {
        return getter.GetAttributes()
            .OfType<SObject>()
            .Where(o => !o.IsComment)
            .Select(o => o.ObjectType.GetFullTypeNameText());
    }

    /// <summary>
    /// Checks whether the specified attribute type is present.
    /// </summary>
    /// <param name="getter">The attribute getter to query.</param>
    /// <param name="typeName">The full type name to check for.</param>
    /// <returns>True if an attribute of the specified type exists.</returns>
    public static bool Contains(this IAttributeGetter getter, string typeName)
    {
        return getter.GetAttributes()
            .OfType<SObject>()
            .Any(o => !o.IsComment && o.ObjectType.GetFullTypeNameText() == typeName);
    }

    /// <summary>
    /// Adds an attribute of the specified type and configures it using the setter action.
    /// </summary>
    /// <typeparam name="T">The type of attribute to add.</typeparam>
    /// <param name="design">The attribute design to modify.</param>
    /// <param name="setter">An action to configure the newly added attribute.</param>
    /// <returns>The newly created attribute instance.</returns>
    public static T AddAttribute<T>(this IAttributeDesign design, Action<T> setter) where T : DesignAttribute, new()
    {
        var attribute = design.AddAttribute<T>();
        if (attribute != null)
        {
            setter(attribute);
            attribute.Commit();
        }

        return attribute;
    }

    /// <summary>
    /// Sets an attribute of the specified type (adding if not exists) and configures it using the setter action.
    /// </summary>
    /// <typeparam name="T">The type of attribute to set.</typeparam>
    /// <param name="design">The attribute design to modify.</param>
    /// <param name="setter">An action to configure the attribute.</param>
    /// <returns>The attribute instance.</returns>
    public static T SetAttribute<T>(this IAttributeDesign design, Action<T> setter) where T : DesignAttribute, new()
    {
        var attribute = design.SetAttribute<T>();
        if (attribute != null)
        {
            setter(attribute);
            attribute.Commit();
        }

        return attribute;
    }

    /// <summary>
    /// Adds an attribute to a collection that has attribute design support.
    /// </summary>
    /// <typeparam name="T">The type of attribute to add.</typeparam>
    /// <param name="collection">The collection with attribute design support.</param>
    /// <param name="setter">An action to configure the newly added attribute.</param>
    /// <returns>The newly created attribute instance.</returns>
    public static T AddAttribute<T>(this IHasAttributeDesign collection, Action<T> setter) where T : DesignAttribute, new() 
        => collection.Attributes?.AddAttribute<T>(setter);

    /// <summary>
    /// Sets an attribute on a collection that has attribute design support.
    /// </summary>
    /// <typeparam name="T">The type of attribute to set.</typeparam>
    /// <param name="collection">The collection with attribute design support.</param>
    /// <param name="setter">An action to configure the attribute.</param>
    /// <returns>The attribute instance.</returns>
    public static T SetAttribute<T>(this IHasAttributeDesign collection, Action<T> setter) where T : DesignAttribute, new()
        => collection.Attributes?.SetAttribute<T>(setter);

    /// <summary>
    /// Sets an attribute on a collection that has attribute design support.
    /// </summary>
    /// <typeparam name="T">The type of attribute to set.</typeparam>
    /// <param name="collection">The collection with attribute design support.</param>
    /// <returns>The attribute instance.</returns>
    public static T SetAttribute<T>(this IHasAttributeDesign collection) where T : DesignAttribute, new()
        => collection.Attributes?.SetAttribute<T>();

    /// <summary>
    /// Adds a design attribute to a view property.
    /// </summary>
    /// <param name="property">The view property to modify.</param>
    /// <param name="attribute">The attribute to add.</param>
    /// <returns>The same view property for method chaining.</returns>
    public static ViewProperty WithAttribute(this ViewProperty property, DesignAttribute attribute)
    {
        if (property.Attributes is not SArrayAttributeDesign attrs)
        {
            property.Attributes = attrs = new SArrayAttributeDesign();
        }

        var obj = new SObject(attribute);
        attrs.Array.Add(obj);

        return property;
    }

    /// <summary>
    /// Adds an SObject attribute to a view property.
    /// </summary>
    /// <param name="property">The view property to modify.</param>
    /// <param name="obj">The SObject to add as an attribute.</param>
    /// <returns>The same view property for method chaining.</returns>
    public static ViewProperty WithAttribute(this ViewProperty property, SObject obj)
    {
        if (property.Attributes is not SArrayAttributeDesign attrs)
        {
            property.Attributes = attrs = new SArrayAttributeDesign();
        }

        attrs.Array.Add(obj);

        return property;
    }

    /// <summary>
    /// Adds a numeric range attribute to a view property.
    /// </summary>
    /// <param name="viewProperty">The view property to modify.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment step.</param>
    /// <returns>The same view property for method chaining.</returns>
    public static ViewProperty WithRange(this ViewProperty viewProperty, decimal min, decimal max, decimal increment = 0.1m)
    {
        return viewProperty.WithAttribute(new NumericRangeAttribute { Min = min, Max = max, Increment = increment });
    }
}

#endregion
