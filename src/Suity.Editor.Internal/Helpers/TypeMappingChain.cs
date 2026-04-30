using System;
using System.Collections.Generic;

namespace Suity.Helpers;

/// <summary>
/// A chain of type mappings that resolves original types to their mapped types.
/// Supports generic type definitions and inheritance chain traversal.
/// </summary>
public class TypeMappingChain
{
    private readonly Dictionary<Type, Type> _typeMap = [];

    /// <summary>
    /// Adds a type mapping from an original type to a mapped type.
    /// If the original type is generic, also attempts to add a mapping for its generic type definition.
    /// </summary>
    /// <param name="originType">The original type to map from.</param>
    /// <param name="mapType">The target type to map to.</param>
    public void AddMap(Type originType, Type mapType)
    {
        _typeMap[originType] = mapType;

        if (originType.IsGenericType)
        {
            TryAddMap(originType.GetGenericTypeDefinition(), mapType);
        }
    }

    /// <summary>
    /// Attempts to add a type mapping if the original type is not already mapped.
    /// If the original type is generic, also attempts to add a mapping for its generic type definition.
    /// </summary>
    /// <param name="originType">The original type to map from.</param>
    /// <param name="mapType">The target type to map to.</param>
    /// <returns>True if the mapping was added; false if the original type was already mapped.</returns>
    public bool TryAddMap(Type originType, Type mapType)
    {
        if (!_typeMap.ContainsKey(originType))
        {
            _typeMap.Add(originType, mapType);

            if (originType.IsGenericType)
            {
                TryAddMap(originType.GetGenericTypeDefinition(), mapType);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves a single mapped type for the given original type by walking up the inheritance chain.
    /// Returns the first matching mapped type found.
    /// </summary>
    /// <param name="originType">The original type to resolve.</param>
    /// <returns>The mapped type if found; otherwise, null.</returns>
    public Type ResolveType(Type originType)
    {
        Type type = originType;

        while (type != null)
        {
            var resultType = ResolveOne(type);
            if (resultType != null)
            {
                return resultType;
            }

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Resolves all mapped types for the given original type by walking up the entire inheritance chain.
    /// </summary>
    /// <param name="originType">The original type to resolve.</param>
    /// <returns>An array of all mapped types found in the inheritance chain.</returns>
    public Type[] ResolveTypeChain(Type originType)
    {
        Type type = originType;
        List<Type> resultTypes = [];

        while (type != null)
        {
            var resultType = ResolveOne(type);
            if (resultType != null)
            {
                resultTypes.Add(resultType);
            }

            type = type.BaseType;
        }

        return resultTypes.ToArray();
    }

    private Type ResolveOne(Type type)
    {
        if (_typeMap.TryGetValue(type, out Type resultType))
        {
            return resultType;
        }
        else if (type.IsGenericType && _typeMap.TryGetValue(type.GetGenericTypeDefinition(), out Type resultDefType))
        {
            try
            {
                resultType = resultDefType.MakeGenericType(type.GetGenericArguments());
                return resultType;
            }
            catch (Exception)
            {
            }
        }

        return null;
    }
}
