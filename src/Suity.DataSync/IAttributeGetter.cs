using System.Collections.Generic;
using System.Linq;

namespace Suity;

/// <summary>
/// Provides methods to get custom attributes from an object
/// </summary>
public interface IAttributeGetter
{
    IEnumerable<object> GetAttributes();

    IEnumerable<object> GetAttributes(string typeName);

    IEnumerable<T> GetAttributes<T>() where T : class;
}

/// <summary>
/// Empty implementation of IAttributeGetter that returns no attributes
/// </summary>
public sealed class EmptyAttributeGetter : IAttributeGetter
{
    public static EmptyAttributeGetter Empty { get; } = new();

    private EmptyAttributeGetter()
    {
    }

    public IEnumerable<object> GetAttributes() => [];

    public IEnumerable<object> GetAttributes(string typeName) => [];

    public IEnumerable<T> GetAttributes<T>() where T : class => [];
}

/// <summary>
/// Extension methods for IAttributeGetter
/// </summary>
public static class IAttributeGetterExtensions
{
    public static bool ContainsAttribute(this IAttributeGetter getter, string typeName) => getter.GetAttributes(typeName)?.Any() == true;

    public static object GetAttribute(this IAttributeGetter getter, string typeName) => getter.GetAttributes(typeName)?.FirstOrDefault();

    public static bool ContainsAttribute<T>(this IAttributeGetter getter) where T : class => getter.GetAttributes<T>()?.Any() == true;

    public static T GetAttribute<T>(this IAttributeGetter getter) where T : class => getter.GetAttributes<T>()?.FirstOrDefault();

    public static IEnumerable<T> GetAttributes<T>(this IAttributeGetter getter) where T : class => getter.GetAttributes<T>();
}
