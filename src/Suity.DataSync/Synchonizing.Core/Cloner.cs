using Suity.NodeQuery;
using Suity.Synchonizing.Preset;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Utility class for cloning objects via sync operations
/// </summary>
public static class Cloner
{
    public static object Clone(object source)
    {
        return Clone<object>(source, null, null);
    }

    public static object Clone(object source, ISyncTypeResolver resolver, IServiceProvider provider)
    {
        return Clone<object>(source, resolver, provider);
    }

    public static T Clone<T>(T source)
    {
        return Clone<T>(source, null, null);
    }

    public static T Clone<T>(T objSrc, ISyncTypeResolver resolver, IServiceProvider provider)
    {
        if (objSrc == null)
        {
            return default;
        }

        var type = objSrc.GetType();

        if (type.IsValueType || objSrc is string)
        {
            return objSrc;
        }

        object objClone = null;

        if (objSrc is ISerializeAsString serializeAsString)
        {
            return (T)SyncTypes.CreateObject(objSrc.GetType(), null, serializeAsString.Key, resolver, objSrc as ISyncTypeResolver);
        }

        if (objSrc is ISyncObject)
        {
            objClone = SyncTypes.CreateObject(objSrc.GetType(), resolver, objSrc as ISyncTypeResolver);
        }
        else if (objSrc is ISyncList)
        {
            objClone = SyncTypes.CreateObject(objSrc.GetType(), resolver, objSrc as ISyncTypeResolver);
        }
        else if (SyncTypes.GetObjectProxyType(objSrc) != null)
        {
            objClone = SyncTypes.CreateObject(objSrc.GetType(), resolver, SyncTypes.GetObjectProxyType(objSrc) as ISyncTypeResolver);
        }
        else if (SyncTypes.GetListProxyType(objSrc.GetType()) != null)
        {
            objClone = SyncTypes.CreateObject(objSrc.GetType(), resolver, SyncTypes.GetListProxyType(objSrc) as ISyncTypeResolver);
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            objClone = CloneList((IList)objSrc);
        }

        if (objClone != null)
        {
            CloneProperty(objSrc, objClone, new SyncContext(null, resolver, provider), new SyncContext(null, resolver, provider));
            return (T)objClone;
        }
        else
        {
            //return default;
            throw new CloneFailedException($"Clone {type.Name} failed.");
        }
    }

    public static void CloneProperty(object objFrom, object objTo)
    {
        CloneProperty(objFrom, objTo, SyncContext.Empty, SyncContext.Empty);
    }

    public static void CloneProperty(object objFrom, object objTo, ISyncTypeResolver resolver, IServiceProvider provider)
    {
        var context = new SyncContext(null, resolver, provider);
        CloneProperty(objFrom, objTo, context, context);
    }

    private static void CloneProperty(object objFrom, object objTo, SyncContext contextFrom, SyncContext contextTo)
    {
        if (objFrom is null)
        {
            throw new ArgumentNullException(nameof(objFrom));
        }

        if (objTo is null)
        {
            throw new ArgumentNullException(nameof(objTo));
        }

        if (objFrom.GetType() != objTo.GetType())
        {
            throw new ArgumentException();
        }

        if (objFrom.GetType().IsValueType || objFrom is string)
        {
            //Do nothing
        }
        else if (objFrom is ISyncObject syncObject)
        {
            ClonePropertySyncObject(syncObject, (ISyncObject)objTo, contextFrom, contextTo);
        }
        else if (objFrom is ISyncList syncList)
        {
            ClonePropertySyncList(syncList, (ISyncList)objTo, contextFrom, contextTo);
        }
        else if (SyncTypes.GetObjectProxyType(objFrom) != null)
        {
            ClonePropertySyncObject(SyncTypes.CreateObjectProxy(objFrom), SyncTypes.CreateObjectProxy(objTo), contextFrom, contextTo);
        }
        else if (SyncTypes.GetListProxyType(objFrom) != null)
        {
            ClonePropertySyncList(SyncTypes.CreateListProxy(objFrom), SyncTypes.CreateListProxy(objTo), contextFrom, contextTo);
        }
        else if (objFrom is RawNode rawFrom)
        {
            (objTo as RawNode)?.ClonePropertyFrom(rawFrom);
        }
        else if (contextFrom.Resolver != null && contextTo.Resolver != null)
        {
            object proxyFrom = contextFrom.Resolver.CreateProxy(objFrom);
            object proxyTo = contextTo.Resolver.CreateProxy(objTo);

            if (proxyFrom is ISyncObject sObjFrom && proxyTo is ISyncObject sObjTo)
            {
                ClonePropertySyncObject(sObjFrom, sObjTo, contextFrom, contextTo);
            }
            else if (proxyFrom is ISyncList sListFrom && proxyTo is ISyncList sListTo)
            {
                ClonePropertySyncList(sListFrom, sListTo, contextFrom, contextTo);
            }
        }
    }

    private static void ClonePropertySyncObject(ISyncObject objFrom, ISyncObject objTo, SyncContext contextFrom, SyncContext contextTo)
    {
        var getterSync = new GetAllPropertySync(SyncIntent.Clone, false);
        objFrom.Sync(getterSync, contextFrom);

        var elementContextFrom = contextFrom.CreateNew(objFrom);
        var elementContextTo = contextTo.CreateNew(objTo);

        var setterSync = new ClonePropertySync(getterSync.Values)
        {
            Creater = (elementType, elementParam) =>
            {
                return CreateObject(elementType, elementParam, contextTo.Resolver, objTo as ISyncTypeResolver);
            },

            Cloner = (elementObjFrom, elementObjTo) =>
            {
                CloneProperty(elementObjFrom, elementObjTo, elementContextFrom, elementContextTo);
            }
        };

        objTo.Sync(setterSync, contextTo);
    }

    private static void ClonePropertySyncList(ISyncList objFrom, ISyncList objTo, SyncContext contextFrom, SyncContext contextTo)
    {
        var getterSync = new GetAllIndexSync(SyncIntent.Clone);
        objFrom.Sync(getterSync, contextFrom);

        var elementContextFrom = contextFrom.CreateNew(objFrom);
        var elementContextTo = contextTo.CreateNew(objTo);

        var setterSync = new CloneIndexSync(getterSync.Values, getterSync.Attributes)
        {
            Creater = (elementType, elementParam) =>
            {
                return CreateObject(elementType, elementParam, contextTo.Resolver, objTo as ISyncTypeResolver);
            },

            Cloner = (elementObjFrom, elementObjTo) =>
            {
                CloneProperty(elementObjFrom, elementObjTo, elementContextFrom, elementContextTo);
            }
        };

        objTo.Sync(setterSync, contextTo);
    }

    private static object CreateObject(Type type, object parameter, ISyncTypeResolver callerResolver, ISyncTypeResolver localResolver)
    {
        if (type.IsValueType || type == typeof(string))
        {
            return parameter;
        }
        else
        {
            return SyncTypes.CreateObject(type, callerResolver, localResolver);
        }
    }

    private static object CloneList(IList originalObject)
    {
        var listType = originalObject.GetType(); // Get the type of the original object

        if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
        {
            // var itemType = listType.GetGenericArguments()[0]; // Get the element type of the list

            var clonedList = Activator.CreateInstance(listType) as IList;
            if (clonedList is null)
            {
                return null;
            }

            foreach (var item in originalObject)
            {
                var clonedItem = Clone(item);
                clonedList.Add(clonedItem);
            }

            return clonedList;
        }

        return null;
    }
}


[Serializable]
public class CloneFailedException : Exception
{
    public CloneFailedException() { }
    public CloneFailedException(string message) : base(message) { }
    public CloneFailedException(string message, Exception inner) : base(message, inner) { }
    protected CloneFailedException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}