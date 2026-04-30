using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Suity.Reflecting;

/// <summary>
/// Provides methods for resolving types by their short names within an assembly.
/// </summary>
public static class AssemblyShortTypeName
{
    private static readonly Dictionary<Assembly, UniqueMultiDictionary<string, Type>> _typeNames = [];

    public static Type ResolveExportedClassType(Assembly assembly, string name)
    {
        var collection = EnsureCollection(assembly);
        return collection[name].FirstOrDefault();
    }

    private static UniqueMultiDictionary<string, Type> EnsureCollection(Assembly asm)
    {
        if (!_typeNames.TryGetValue(asm, out var collection))
        {
            collection = new();
            _typeNames.Add(asm, collection);

            try
            {
                var types = asm.GetExportedTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        collection.Add(type.Name, type);
                    }
                }
            }
            catch (Exception err)
            {
                err.LogError($"Assembly is dynamic or not supported : {asm.FullName}");
            }
        }

        return collection;
    }
}