using Suity.Collections;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Suity.Editor.Services;

internal sealed class EditorSyncTypeResolver : ISyncTypeResolver
{
    public static readonly EditorSyncTypeResolver Instance = new();

    private readonly Dictionary<string, Type> _stringToTypes = [];
    private readonly Dictionary<Type, string> _typeToStrings = [];

    private bool _aliasInit;
    private readonly object _initLock = new();

    private EditorSyncTypeResolver()
    {
        InitRepair();
    }

    private void InitRepair()
    {
    }

    private void InitForAlias()
    {
        if (_aliasInit)
        {
            return;
        }

        Assembly[] asmQuery = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asmQuery)
        {
            if (asm.IsDynamic)
            {
                continue;
            }

            // Try to retrieve all Types from the current Assembly
            Type[] types;

            try
            {
                types = asm.GetExportedTypes();
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var type in types)
            {
                if (string.IsNullOrEmpty(type.FullName))
                {
                    continue;
                }

                // Register Asset type by default
                if (typeof(Asset).IsAssignableFrom(type))
                {
                    _stringToTypes[type.FullName] = type;
                    _typeToStrings[type] = type.FullName;
                }

                foreach (var alias in type.GetAttributesCached<NativeAliasAttribute>())
                {
                    string name = alias.AliasName;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        // Use type's short name by default
                        name = type.Name;
                    }

                    if (!_stringToTypes.ContainsKey(name))
                    {
                        _stringToTypes[name] = type;
                    }
                    else
                    {
                        Logs.LogError($"Failed to register alias for type:{type.Name}, '{name}' is registered by type:{_stringToTypes[name].Name}");
                    }

                    if (alias.UseForSaving)
                    {
                        if (!_typeToStrings.ContainsKey(type))
                        {
                            _typeToStrings[type] = name;
                        }
                        else
                        {
                            Logs.LogError($"Failed to register alias for type:{type.Name}, type:'{type.Name}' is registered by type:{name}");
                        }
                    }
                }
            }
        }

        _aliasInit = true;
    }

    #region ISyncTypeResolver

    string ISyncTypeResolver.ResolveTypeName(Type type, object obj)
    {
        if (!_aliasInit)
        {
            lock (_initLock)
            {
                InitForAlias();
            }
        }

        if (obj != null)
        {
            type = obj.GetType();
        }

        return _typeToStrings.GetValueSafe(type);
    }

    Type? ISyncTypeResolver.ResolveType(string typeName, string parameter)
    {
        if (!_aliasInit)
        {
            lock (_initLock)
            {
                InitForAlias();
            }
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        Type type = _stringToTypes.GetValueSafe(typeName);
        if (type != null)
        {
            return type;
        }

        //Handle lagacy naming
        if (typeName.StartsWith("SuityEditor"))
        {
            typeName = typeName.Replace("SuityEditor", "Suity.Editor");
            type = DefaultSyncTypeResolver.Instance.ResolveType(typeName, parameter);

            return type;
        }

        if (typeName.StartsWith("Suity.Editor.Expressions"))
        {
            typeName = typeName.Replace("Suity.Editor.Expressions", "Suity.Editor.Design");
            type = DefaultSyncTypeResolver.Instance.ResolveType(typeName, parameter);

            return type;
        }

        return null;
    }

    object? ISyncTypeResolver.ResolveObject(string typeName, string parameter)
    {
        return null;
    }

    string? ISyncTypeResolver.ResolveObjectValue(object obj)
    {
        return null;
    }

    object? ISyncTypeResolver.CreateProxy(object obj)
    {
        return null;
    }

    #endregion
}