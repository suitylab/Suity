using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Helpers;

/// <summary>
/// Helper methods for finding common types in collections.
/// </summary>
internal static class CommonTypeHelper
{
    /// <summary>
    /// Finds the common base type among a collection of types.
    /// </summary>
    /// <param name="source">The collection of types.</param>
    /// <returns>The common base type, or null if the collection is empty.</returns>
    public static Type GetCommonType(this IEnumerable<Type> source)
    {
        if (!source.Any())
        {
            return null;
        }

        Type commonType = source.First();
        foreach (Type o in source.Skip(1))
        {
            Type curType = o;
            while (commonType != curType && !commonType.IsAssignableFrom(curType))
            {
                commonType = commonType.BaseType;
            }
        }

        return commonType;
    }

    /// <summary>
    /// Finds the common base type among a collection of objects.
    /// </summary>
    /// <param name="source">The collection of objects.</param>
    /// <returns>The common base type, or null if the collection is empty.</returns>
    public static Type GetCommonType(this IEnumerable<object> source)
    {
        if (!source.Any())
        {
            return null;
        }

        Type commonType = source.First().GetType();
        foreach (object o in source.Skip(1))
        {
            Type curType = o.GetType();
            while (commonType != curType && !commonType.IsAssignableFrom(curType))
            {
                commonType = commonType.BaseType;
            }
        }

        return commonType;
    }
}