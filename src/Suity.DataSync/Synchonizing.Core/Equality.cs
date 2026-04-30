using Suity.Synchonizing.Preset;

namespace Suity.Synchonizing.Core;

public static class Equality
{
    public static bool ObjectEquals(object objA, object objB)
    {
        return ObjectEquals(objA, objB, SyncContext.Empty, SyncContext.Empty);
    }

    public static bool ObjectEquals(object objA, object objB, SyncContext contextA, SyncContext contextB)
    {
        if (objA is null && objB is null)
        {
            return true;
        }

        if (objA is null || objB is null)
        {
            return false;
        }

        if (objA.GetType() != objB.GetType())
        {
            return false;
        }

        contextA ??= SyncContext.Empty;

        contextB ??= SyncContext.Empty;

        if (objA.GetType().IsValueType || objA is string)
        {
            return object.Equals(objA, objB);
        }
        else if (objA is ISerializeAsString sA && objB is ISerializeAsString sB)
        {
            return sA.Key == sB.Key;
        }
        else if (objA is ISyncObject syncObject)
        {
            return SyncObjectEquals(syncObject, (ISyncObject)objB, contextA, contextB);
        }
        else if (objA is ISyncList syncList)
        {
            return SyncListEquals(syncList, (ISyncList)objB, contextA, contextB);
        }
        else if (SyncTypes.GetObjectProxyType(objA) != null)
        {
            return SyncObjectEquals(SyncTypes.CreateObjectProxy(objA), SyncTypes.CreateObjectProxy(objB), contextA, contextB);
        }
        else if (SyncTypes.GetListProxyType(objA) != null)
        {
            return SyncListEquals(SyncTypes.CreateListProxy(objA), SyncTypes.CreateListProxy(objB), contextA, contextB);
        }
        else if (contextA.Resolver != null && contextB.Resolver != null)
        {
            object proxyA = contextA.Resolver.CreateProxy(objA);
            object proxyB = contextB.Resolver.CreateProxy(objB);

            if (proxyA is ISyncObject sObjA && proxyB is ISyncObject sObjB)
            {
                return SyncObjectEquals(sObjA, sObjB, contextA, contextB);
            }
            else if (proxyA is ISyncList sListA && proxyB is ISyncList sListB)
            {
                return SyncListEquals(sListA, sListB, contextA, contextB);
            }

            string strA = contextA.Resolver.ResolveObjectValue(objA);
            string strB = contextB.Resolver.ResolveObjectValue(objB);
            if (strA != null || strB != null)
            {
                return strA == strB;
            }
        }

        return object.Equals(objA, objB);
    }

    private static bool SyncObjectEquals(ISyncObject objA, ISyncObject objB, SyncContext contextA, SyncContext contextB)
    {
        var syncA = new GetAllPropertySync(false);
        objA.Sync(syncA, contextA);

        var syncB = new GetAllPropertySync(false);
        objB.Sync(syncB, contextB);

        if (syncA.Values.Count != syncB.Values.Count)
        {
            return false;
        }

        var childContextA = contextA.CreateNew(objA);
        var childContextB = contextB.CreateNew(objB);

        foreach (var key in syncA.Values.Keys)
        {
            if (!syncB.Values.ContainsKey(key))
            {
                return false;
            }

            object childObjA = syncA.Values[key].Value;
            object childObjB = syncB.Values[key].Value;

            if (!ObjectEquals(childObjA, childObjB, childContextA, childContextB))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SyncListEquals(ISyncList listA, ISyncList listB, SyncContext contextA, SyncContext contextB)
    {
        var syncA = new GetAllIndexSync();
        listA.Sync(syncA, contextA);

        var syncB = new GetAllIndexSync();
        listB.Sync(syncB, contextB);

        if (syncA.Values.Count != syncB.Values.Count)
        {
            return false;
        }

        var childContextA = contextA.CreateNew(listA);
        var childContextB = contextB.CreateNew(listB);

        for (int i = 0; i < syncA.Values.Count; i++)
        {
            object childObjA = syncA.Values[i].Value;
            object childObjB = syncB.Values[i].Value;

            if (!ObjectEquals(childObjA, childObjB, childContextA, childContextB))
            {
                return false;
            }
        }

        return true;
    }
}