using Suity.Helpers;
using Suity.Reflecting;
using System;

namespace Suity.Synchonizing.Core;

public class DefaultSyncTypeResolver : ISyncTypeResolver
{
    public static readonly DefaultSyncTypeResolver Instance = new();

    public string ResolveTypeName(Type type, object obj)
    {
        if (obj != null)
        {
            type = obj.GetType();
        }

        return type.GetTypeId();
    }

    public Type ResolveType(string typeName, string parameter)
    {
        return typeName.ResolveType();
    }

    public object ResolveObject(string typeName, string parameter)
    {
        return null;
    }

    public string ResolveObjectValue(object obj)
    {
        return null;
    }

    public object CreateProxy(object obj)
    {
        return null;
    }
}