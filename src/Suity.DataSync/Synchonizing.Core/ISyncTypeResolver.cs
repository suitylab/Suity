using System;

namespace Suity.Synchonizing.Core;

public interface ISyncTypeResolver
{
    string ResolveTypeName(Type type, object obj);

    Type ResolveType(string typeName, string parameter);

    object ResolveObject(string typeName, string parameter);

    string ResolveObjectValue(object obj);

    /// <summary>
    /// Create Sync proxy for object
    /// </summary>
    /// <param name="obj">The requested object</param>
    /// <returns>The returned object can be ISyncObject, ISyncList</returns>
    object CreateProxy(object obj);
}