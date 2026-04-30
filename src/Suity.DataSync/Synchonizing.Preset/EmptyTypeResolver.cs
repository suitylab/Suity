using Suity.Synchonizing.Core;
using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Empty implementation of ISyncTypeResolver
/// </summary>
public class EmptyTypeResolver : ISyncTypeResolver
{
    public static readonly EmptyTypeResolver Instance = new();

    public string ResolveTypeName(Type type, object obj)
    {
        return null;
    }

    public Type ResolveType(string typeName, string parameter) => null;

    public object ResolveObject(string typeName, string parameter) => null;

    public string ResolveObjectValue(object obj) => null;

    public object CreateProxy(object obj) => null;
}