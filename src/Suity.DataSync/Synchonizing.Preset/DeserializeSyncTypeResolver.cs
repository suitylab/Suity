using Suity.Synchonizing.Core;
using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of ISyncTypeResolver for deserialization
/// </summary>
public class DeserializeSyncTypeResolver : ISyncTypeResolver
{
    public static readonly DeserializeSyncTypeResolver Instance = new();

    public DeserializeSyncTypeResolver()
    {
    }

    public Type ResolveType(string typeName, string parameter) => typeName switch
    {
        "DataRef" or "Enum" => typeof(string),
        _ => null,
    };

    public string ResolveTypeName(Type type, object obj)
    {
        return null;
    }

    public object ResolveObject(string typeName, string parameter)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return null;
        }

        return typeName switch
        {
            // Forcefully convert String. Empty, otherwise it will be considered as parsing failure
            "DataRef" or "Enum" => parameter ?? string.Empty, 
            _ => null,
        };

        /*
        if (typeName.StartsWith("Object:"))
        {
            string subTypeName = typeName.RemoveFromFirst(7);
            try
            {
                object obj = ObjectType.CreateObject(subTypeName);
                if (obj != null)
                {
                    //RemoteHelpers.FillInitialStruct(obj);
                    return obj;
                }
            }
            catch (Exception)
            {
            }
        }
        */
    }

    public string ResolveObjectValue(object obj)
    {
        return null;
    }

    public object CreateProxy(object obj)
    {
        /*
        if (obj != null && ObjectType.GetClassTypeInfo(obj.GetType()) != null)
        {
            return new Proxy(obj);
        }
        */
        return null;
    }

    /*
    class Proxy : ISyncObject
    {
        readonly object _obj;
        readonly string _typeName;

        public Proxy(object obj)
        {
            _obj = obj ?? throw new ArgumentNullException(nameof(obj));
            _typeName = ObjectType.GetClassTypeInfo(_obj.GetType())?.Key;
        }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            if (_obj == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(_typeName))
            {
                return;
            }
            if (sync.Mode != SyncMode.SetAll)
            {
                return;
            }

            foreach (var name in sync.Names)
            {
                object value = ObjectType.GetProperty(_obj, name);
                if (value is System.Collections.IList)
                {
                    sync.Sync(name, value, SyncFlag.ReadOnly);
                }
                else
                {
                    value = sync.Sync(name, value);
                    ObjectType.SetProperty(_obj, name, value);
                }
            }
        }
    }
    */
}