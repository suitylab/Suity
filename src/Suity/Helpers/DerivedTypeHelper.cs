using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Suity.Helpers;

/// <summary>
/// Provides methods for discovering and caching derived types.
/// </summary>
public static class DerivedTypeHelper
{
    private static readonly Dictionary<Type, List<Type>> _derivedTypeDict = [];

    private static HashSet<Assembly> _excludedAsms;

    private static readonly HashSet<Assembly> _unsupportedAsms = [];

    public static void InitializeExcludedAssemblies(IEnumerable<Assembly> assemblies)
    {
        if (_excludedAsms != null)
        {
            return;
        }

        _excludedAsms = [.. assemblies];
    }

    public static void InitializeExcludedAssemblies(params Assembly[] assemblies)
    {
        if (_excludedAsms != null)
        {
            return;
        }

        _excludedAsms = [.. assemblies];
    }

    /// <summary>
    /// Get all derived types based on the specified base type
    /// </summary>
    /// <param name="baseType">Base type</param>
    /// <returns>Returns all derived types based on the specified base type</returns>
    public static IEnumerable<Type> GetDerivedTypes(this Type baseType)
    {
        lock (_derivedTypeDict)
        {
            if (_derivedTypeDict.TryGetValue(baseType, out var availTypes))
            {
                return availTypes.Pass();
            }

            availTypes = [];
            Assembly[] asmQuery = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in asmQuery)
            {
                if (asm.IsDynamic)
                {
                    continue;
                }

                if (_excludedAsms?.Contains(asm) == true)
                {
                    continue;
                }

                // Try to retrieve all Types from the current Assembly
                Type[] types;

                try
                {
                    types = asm.GetExportedTypes();
                }
                catch (Exception err)
                {
                    if (_unsupportedAsms.Add(asm))
                    {
                        Logs.LogWarning($"Assembly is dynamic or not supported : {asm.FullName}");
                        Logs.LogWarning(err);
                    }

                    continue;
                }

                // Add the matching subset of these types to the result
                availTypes.AddRange(
                    from t in types
                    where IsAssignableFromAdv(t, baseType)
                    orderby t.Name
                    select t);
            }

            _derivedTypeDict[baseType] = availTypes;

            return availTypes;
        }
    }

    public static IEnumerable<Type> GetAvailableDerivedTypes(this Type baseType)
    {
        return GetDerivedTypes(baseType).Where(o => !o.HasAttributeCached<NotAvailableAttribute>());
    }

    public static IEnumerable<Type> GetAvailableClassTypes(this Type baseType)
    {
        return GetDerivedTypes(baseType)
            .Where(o => o.IsPublic && o.IsClass && !o.IsAbstract && !o.IsInterface)
            .Where(o => !o.HasAttributeCached<NotAvailableAttribute>());
    }


    public static IEnumerable<Type> GetDerivedTypes(this Type baseType, Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes().Where(t => IsAssignableFromAdv(t, baseType));
        }
        catch (Exception err)
        {
            err.LogError($"Assembly is dynamic or not supported : {assembly.FullName}");

            return [];
        }
    }

    public static IEnumerable<T> CreateDerivedClasses<T>()
        where T : class
    {
        var types = GetAvailableClassTypes(typeof(T));
        return types.Select(type =>
        {
            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception err)
            {
                err.LogError($"Create {nameof(T)} failed: " + type.FullName);
                return null;
            }
        }).SkipNull();
    }

    private static bool IsAssignableFromAdv(Type type, Type baseType)
    {
        if (type == baseType)
        {
            return false;
        }

        if (baseType.IsAssignableFrom(type))
        {
            return true;
        }

        Type b = type.BaseType;
        while (b != null && b != typeof(object))
        {
            if (b.IsGenericType && b.GetGenericTypeDefinition() == baseType)
            {
                return true;
            }

            b = b.BaseType;
        }

        return false;
    }

    public static IEnumerable<Type> GetGenericDerivedType(this Type genericDefinition, Type genericArgument)
    {
        Type baseType = genericDefinition.MakeGenericType([genericArgument]);

        return GetDerivedTypes(baseType);
    }

    public static IEnumerable<Type> GetGenericDerivedType(this Type genericDefinition, params Type[] genericArguments)
    {
        Type baseType = genericDefinition.MakeGenericType(genericArguments);

        return GetDerivedTypes(baseType);
    }

    public static void ResetCache()
    {
        lock (_derivedTypeDict)
        {
            _derivedTypeDict.Clear();
        }
    }
}