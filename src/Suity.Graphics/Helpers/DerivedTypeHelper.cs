using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Suity.Helpers;

/// <summary>
/// Derived type cache
/// </summary>
internal static class DerivedTypeHelper
{
    private static readonly Dictionary<Type, List<Type>> _derivedTypeDict = [];

    /// <summary>
    /// Retrieve all derived types based on the specified base type
    /// </summary>
    /// <param name="baseType">Base type</param>
    /// <returns>Return all derived types based on the specified base type</returns>
    public static IEnumerable<Type> GetDerivedTypes(this Type baseType)
    {
        lock (_derivedTypeDict)
        {
            if (_derivedTypeDict.TryGetValue(baseType, out List<Type> availTypes))
            {
                return availTypes.Select(o => o);
            }

            availTypes = new List<Type>();
            Assembly[] asmQuery = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in asmQuery)
            {
                if (asm.IsDynamic)
                {
                    continue;
                }

                // Try to retrieve all Types from the current Assembly
                Type[] types;
                try { types = asm.GetExportedTypes(); }
                catch (Exception err)
                {
                    // Logs.LogWarning($"Assembly is dynamic or not supported : {asm.FullName}");
                    // Logs.LogWarning(err);
                    continue;
                }

                // Add the matching subset of these types to the result
                availTypes.AddRange(
                    from t in types
                    where t != baseType && baseType.IsAssignableFrom(t)
                    orderby t.Name
                    select t);
            }

            _derivedTypeDict[baseType] = availTypes;

            return availTypes;
        }
    }

    /// <summary>
    /// Retrieves derived types from a specific assembly.
    /// </summary>
    /// <param name="baseType">The base type.</param>
    /// <param name="assembly">The assembly to search in.</param>
    /// <returns>Collection of derived types.</returns>
    public static IEnumerable<Type> GetDerivedTypes(this Type baseType, Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type));
        }
        catch (Exception)
        {
            throw;
            //return EmptyArray<Type>.Empty;
        }
    }

    /// <summary>
    /// Retrieves derived types of a generic type definition with a specific argument.
    /// </summary>
    /// <param name="genericDefinition">The generic type definition.</param>
    /// <param name="genericArgument">The generic argument.</param>
    /// <returns>Collection of derived types.</returns>
    public static IEnumerable<Type> GetGenericDerivedType(this Type genericDefinition, Type genericArgument)
    {
        Type baseType = genericDefinition.MakeGenericType([genericArgument]);

        return GetDerivedTypes(baseType);
    }

    /// <summary>
    /// Retrieves derived types of a generic type definition with a specific argument from an assembly.
    /// </summary>
    /// <param name="genericDefinition">The generic type definition.</param>
    /// <param name="genericArgument">The generic argument.</param>
    /// <param name="assembly">The assembly to search in.</param>
    /// <returns>Collection of derived types.</returns>
    public static IEnumerable<Type> GetGenericDerivedType(this Type genericDefinition, Type genericArgument, Assembly assembly)
    {
        Type baseType = genericDefinition.MakeGenericType([genericArgument]);

        return GetDerivedTypes(baseType, assembly);
    }

    /// <summary>
    /// Clears the derived type cache.
    /// </summary>
    public static void ResetCache()
    {
        lock (_derivedTypeDict)
        {
            _derivedTypeDict.Clear();
        }
    }
}