using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

#region CombinedAttributes

/// <summary>
/// Combines two attribute getters into one.
/// </summary>
public sealed class CombinedAttributes : IAttributeGetter
{
    private readonly IAttributeGetter _first;
    private readonly IAttributeGetter _second;

    public CombinedAttributes(IAttributeGetter first, IAttributeGetter second)
    {
        _first = first;
        _second = second;
    }

    public IEnumerable<object> GetAttributes()
    {
        return _first.GetAttributes().Concat(_second.GetAttributes());
    }

    public IEnumerable<object> GetAttributes(string typeName)
    {
        return _first.GetAttributes(typeName).Concat(_second.GetAttributes(typeName));
    }

    public IEnumerable<T> GetAttributes<T>() where T : class
    {
        return _first.GetAttributes<T>().Concat(_second.GetAttributes<T>());
    }


}

#endregion

#region SingleAttributes

/// <summary>
/// Provides a single attribute instance.
/// </summary>
public class SingleAttributes<TAttribute> : IAttributeGetter where TAttribute : SObjectController, new()
{
    public static SingleAttributes<TAttribute> Instance { get; } = new();

    private readonly TAttribute _attr = new();

    private SingleAttributes()
    {
    }

    public IEnumerable<object> GetAttributes()
    {
        return [_attr];
    }

    public IEnumerable<object> GetAttributes(string typeName)
    {
        if (_attr.NativeDType?.FullTypeName == typeName)
        {
            return [_attr];
        }

        return [];
    }

    public IEnumerable<T> GetAttributes<T>() where T : class
    {
        if (typeof(T) == typeof(TAttribute))
        {
            return [(T)(object)_attr];
        }

        return [];
    }
}

#endregion

#region AssetTypeBindingAttribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class AssetTypeBindingAttribute : Attribute
{
    public AssetTypeBindingAttribute(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        TypeName = typeName;
    }

    public AssetTypeBindingAttribute(string typeName, string description)
        : this(typeName)
    {
        Description = description;
    }

    public string TypeName { get; }

    public string Description { get; }

    public override string ToString()
    {
        return TypeName;
    }
}
#endregion

#region AssetAutoCreateAttribute
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AssetAutoCreateAttribute : Attribute
{
}

#endregion

#region NotAvailableException
[Serializable]
public class NotAvailableException : Exception
{
    public NotAvailableException() { }
    public NotAvailableException(string message) : base(message) { }
    public NotAvailableException(string message, Exception inner) : base(message, inner) { }
    protected NotAvailableException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
} 
#endregion