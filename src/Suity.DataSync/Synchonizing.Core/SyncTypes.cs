using Suity.Collections;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Synchronization types and utilities for sync operations
/// </summary>
public static class SyncTypes
{
    internal class ValueResolver
    {
        public Func<string, object> ValueResolve { get; }
        public Func<object, string> StringResolve { get; }

        public ValueResolver(Func<string, object> valueResolve, Func<object, string> stringResolve = null)
        {
            ValueResolve = valueResolve ?? throw new ArgumentNullException(nameof(valueResolve));
            StringResolve = stringResolve ?? (o => o?.ToString());
        }
    }

    internal static ISyncTypeResolver _globalResolver;

    public static void InitializeGlobalResolver(ISyncTypeResolver resolver)
    {
        if (_globalResolver != null)
        {
            throw new InvalidOperationException();
        }

        _globalResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    private static readonly ConcurrentDictionary<Type, SyncObjectProxy> _objectProxyCache = new();
    private static readonly ConcurrentDictionary<Type, SyncListProxy> _listProxyCache = new();
    private static readonly Dictionary<Type, ValueResolver> _valueResolvers = [];

    static SyncTypes()
    {
        // Avoid null values, as this can cause dynamic type handling in subsequent processes to fail to recognize string type.
        _valueResolvers[typeof(string)] = new ValueResolver(s => s ?? string.Empty);
        _valueResolvers[typeof(bool)] = new ValueResolver(s => Convert.ToBoolean(s));
        _valueResolvers[typeof(byte)] = new ValueResolver(s => Convert.ToByte(s));
        _valueResolvers[typeof(char)] = new ValueResolver(s => Convert.ToChar(s));
        _valueResolvers[typeof(decimal)] = new ValueResolver(s => Convert.ToDecimal(s));
        _valueResolvers[typeof(double)] = new ValueResolver(s => Convert.ToDouble(s));
        _valueResolvers[typeof(Int16)] = new ValueResolver(s => Convert.ToInt16(s));
        _valueResolvers[typeof(Int32)] = new ValueResolver(s => Convert.ToInt32(s));
        _valueResolvers[typeof(Int64)] = new ValueResolver(s => Convert.ToInt64(s));
        _valueResolvers[typeof(sbyte)] = new ValueResolver(s => Convert.ToSByte(s));
        _valueResolvers[typeof(Single)] = new ValueResolver(s => Convert.ToSingle(s));
        _valueResolvers[typeof(UInt16)] = new ValueResolver(s => Convert.ToUInt16(s));
        _valueResolvers[typeof(UInt32)] = new ValueResolver(s => Convert.ToUInt32(s));
        _valueResolvers[typeof(UInt64)] = new ValueResolver(s => Convert.ToUInt64(s));
        _valueResolvers[typeof(DateTime)] = new ValueResolver(s => Convert.ToDateTime(s));
        _valueResolvers[typeof(TimeSpan)] = new ValueResolver(s =>
        {
            if (TimeSpan.TryParse(s, out TimeSpan timeSpan))
            {
                return timeSpan;
            }
            else
            {
                return TimeSpan.Zero;
            }
        });
        _valueResolvers[typeof(Guid)] = new ValueResolver(s =>
        {
            if (Guid.TryParseExact(s, "D", out Guid id))
            {
                return id;
            }
            else
            {
                return Guid.Empty;
            }
        });
        _valueResolvers[typeof(ButtonValue)] = new ValueResolver(s =>
        {
            if (ButtonValue.TryParse(s, out ButtonValue value))
            {
                return value;
            }
            else
            {
                return ButtonValue.Empty;
            }
        });
        _valueResolvers[typeof(Color)] = new ValueResolver(s =>
        {
            try
            {
                return ColorTranslator.FromHtml(s);
            }
            catch (Exception)
            {
                return Color.Empty;
            }
        }, o => o is Color c ? ColorTranslator.ToHtml(c) : string.Empty);
    }

    public static ISyncTypeResolver GlobalResolver => _globalResolver;

    internal static void RegisterProxy(Type type, SyncObjectProxy proxy)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (proxy is null)
        {
            throw new ArgumentNullException(nameof(proxy));
        }

        _objectProxyCache[type] = proxy;
    }

    internal static void RegisterProxy(Type type, SyncListProxy proxy)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (proxy is null)
        {
            throw new ArgumentNullException(nameof(proxy));
        }

        _listProxyCache[type] = proxy;
    }

    internal static SyncObjectProxy GetObjectProxyType(object obj)
    {
        return GetObjectProxyType(obj.GetType());
    }

    internal static SyncObjectProxy GetObjectProxyType(Type editedType)
    {
        if (editedType is null)
        {
            return null;
        }

        if (_objectProxyCache.TryGetValue(editedType, out SyncObjectProxy proxy))
        {
            return proxy;
        }

        foreach (Type editorProxyType in typeof(SyncObjectProxy).GetDerivedTypes())
        {
            var assignment = editorProxyType.GetAttributesCached<SyncProxyUsageAttribute>().FirstOrDefault();
            if (assignment != null && assignment.ObjectType == editedType)
            {
                proxy = (SyncObjectProxy)Activator.CreateInstance(editorProxyType);
                _objectProxyCache[editedType] = proxy;

                return proxy;
            }
        }

        if (editedType.IsGenericType)
        {
            var defType = editedType.GetGenericTypeDefinition();
            if (defType != null && defType != editedType)
            {
                proxy = GetObjectProxyType(defType);
                if (proxy != null)
                {
                    _objectProxyCache[editedType] = proxy;

                    return proxy;
                }
            }
        }

        _objectProxyCache[editedType] = null;

        return null;
    }

    internal static SyncObjectProxy CreateObjectProxy(object obj)
    {
        SyncObjectProxy proxy = GetObjectProxyType(obj);
        //if (proxy is null)
        //{
        //    proxy = WrapSyncObjectProxy(obj);
        //    if (proxy != null)
        //    {
        //        RegisterProxy(obj.GetType(), proxy);
        //    }
        //}

        if (proxy != null)
        {
            proxy = proxy.Clone();
            proxy.Target = obj;

            return proxy;
        }

        return null;
    }

    internal static SyncListProxy GetListProxyType(object list)
    {
        Type editedType = list.GetType();

        if (_listProxyCache.TryGetValue(editedType, out SyncListProxy proxy))
        {
            return proxy;
        }

        foreach (Type editorProxyType in typeof(SyncListProxy).GetDerivedTypes())
        {
            var assignment = editorProxyType.GetAttributesCached<SyncProxyUsageAttribute>().FirstOrDefault();
            if (assignment != null && assignment.ObjectType == editedType)
            {
                proxy = (SyncListProxy)Activator.CreateInstance(editorProxyType);
                _listProxyCache[editedType] = proxy;
                return proxy;
            }
        }

        if (editedType.IsGenericType)
        {
            Type[] args = editedType.GetGenericArguments();
            if (args.Length == 1)
            {
                Type iListType = typeof(IList<>).MakeGenericType(args);
                if (iListType.IsAssignableFrom(editedType))
                {
                    Type proxyType = typeof(GenericListProxy<>).MakeGenericType(args);
                    proxy = (SyncListProxy)Activator.CreateInstance(proxyType);
                    _listProxyCache[editedType] = proxy;

                    return proxy;
                }
            }
        }

        return null;
    }

    internal static SyncListProxy CreateListProxy(object list)
    {
        SyncListProxy proxy = GetListProxyType(list);
        if (proxy != null)
        {
            proxy = proxy.Clone();
            proxy.Target = list;

            return proxy;
        }
        else
        {
            return null;
        }
    }

    public static ISyncObject GetSyncObject(object obj)
    {
        if (obj is null)
        {
            return null;
        }
        else if (obj is ISyncObject syncObject)
        {
            return syncObject;
        }
        else
        {
            return CreateObjectProxy(obj);
        }
    }

    public static ISyncList GetSyncList(object list)
    {
        if (list is null)
        {
            return null;
        }
        else if (list is ISyncList syncList)
        {
            return syncList;
        }
        else if (list is ISyncNode syncNode)
        {
            return syncNode.GetList();
        }
        else
        {
            return CreateListProxy(list);
        }
    }

    internal static ValueResolver GetValueResolver(Type type)
    {
        return _valueResolvers.GetValueSafe(type);
    }

    internal static object CreateObject(Type type, ISyncTypeResolver callerResolver, ISyncTypeResolver localResolver)
    {
        return CreateObject(type, null, null, callerResolver, localResolver);
    }

    internal static object CreateObject(Type type, string overrideTypeName, string content, ISyncTypeResolver callerResolver, ISyncTypeResolver localResolver, bool throwException = true)
    {
        Type defaultOverrideType = type;
        object obj = null;

        // Check override type
        if (!string.IsNullOrEmpty(overrideTypeName))
        {
            if (localResolver != null)
            {
                obj = ResolveObject(overrideTypeName, content, localResolver);
                if (obj != null)
                {
                    return obj;
                }
            }

            if (callerResolver != null)
            {
                obj = ResolveObject(overrideTypeName, content, callerResolver);
                if (obj != null)
                {
                    return obj;
                }
            }

            var globalResolver = _globalResolver;
            if (globalResolver != null)
            {
                obj = ResolveObject(overrideTypeName, content, globalResolver);
                if (obj != null)
                {
                    return obj;
                }
            }

            // Resolve type
            do
            {
                defaultOverrideType = localResolver?.ResolveType(overrideTypeName, content);
                if (defaultOverrideType != null)
                {
                    break;
                }
                
                defaultOverrideType = callerResolver?.ResolveType(overrideTypeName, content);
                if (defaultOverrideType != null)
                {
                    break;
                }

                defaultOverrideType = globalResolver?.ResolveType(overrideTypeName, content);
                if (defaultOverrideType != null)
                {
                    break;
                }

                defaultOverrideType = DefaultSyncTypeResolver.Instance.ResolveType(overrideTypeName, content);

            } while (false);


            if (defaultOverrideType is null)
            {
                throw new TypeResolveException($"Can not resolve type : {overrideTypeName} L:{localResolver?.GetType().Name} C:{callerResolver?.GetType().Name} G:{globalResolver?.GetType().Name}");
            }
        }

        // Various types of support built through content
        if (_valueResolvers.TryGetValue(defaultOverrideType, out var vr))
        {
            try
            {
                return vr.ValueResolve(content);
            }
            catch (Exception)
            {
                return null;
            }
        }

        if (defaultOverrideType.IsEnum)
        {
            try
            {
                obj = Enum.Parse(defaultOverrideType, content);
            }
            catch (Exception)
            {
                Array enumValues = Enum.GetValues(defaultOverrideType);
                obj = enumValues.Length > 0 ? enumValues.GetValue(0) : null;
            }

            return obj;
        }

        if (typeof(ISerializeAsString).IsAssignableFrom(defaultOverrideType))
        {
            var sas = (ISerializeAsString)Activator.CreateInstance(defaultOverrideType);
            if (sas != null)
            {
                sas.Key = content;

                return sas;
            }
            else
            {
                return null;
            }
        }

        // No type definition
        if (defaultOverrideType == typeof(object))
        {
            return null;
        }

        // DBNull
        if (defaultOverrideType == typeof(DBNull))
        {
            return DBNull.Value;
        }

        if (localResolver != null)
        {
            string typeName = localResolver.ResolveTypeName(type, null);
            if (!string.IsNullOrEmpty(typeName))
            {
                obj = ResolveObject(typeName, content, localResolver);
                if (obj != null)
                {
                    return obj;
                }
            }

            // Attempt to have localParser create default objects
            obj = localResolver.ResolveObject(null, content);
            if (obj != null)
            {
                return obj;
            }

            Type defaultObjectType = localResolver.ResolveType(null, content);
            if (defaultObjectType != null)
            {
                obj = Activator.CreateInstance(defaultObjectType);
                if (obj != null)
                {
                    return obj;
                }
            }
        }

        // If there is content, attempt to create with 1 parameter.
        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                obj = Activator.CreateInstance(defaultOverrideType, content);
            }
            catch (Exception)
            {
            }
        }

        if (obj is null)
        {
            try
            {
                // Attempt to create without parameters
                obj = Activator.CreateInstance(defaultOverrideType);
            }
            catch (Exception)
            {
            }
        }

        if (obj is null)
        {
            if (throwException)
            {
                throw new TypeResolveException("Can not create object : " + defaultOverrideType.FullName);
            }
        }

        return obj;
    }

    private static object ResolveObject(string typeName, string content, ISyncTypeResolver resolver)
    {
        object obj = resolver.ResolveObject(typeName, content);
        if (obj != null)
        {
            return obj;
        }

        Type objType = resolver.ResolveType(typeName, content);
        if (objType != null)
        {
            if (objType.IsEnum)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    obj = Enum.Parse(objType, content);
                }
                else
                {
                    obj = Enum.GetValues(objType).GetValue(0);
                }
            }
            else if (typeof(ISerializeAsString).IsAssignableFrom(objType))
            {
                obj = Activator.CreateInstance(objType);
                ((ISerializeAsString)obj).Key = content;
            }
            else if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    // Attempt to create with 1 parameter.
                    obj = Activator.CreateInstance(objType, content);
                }
                catch (Exception err)
                {
                    Logs.LogError(err);
                }
            }

            if (obj is null)
            {
                try
                {
                    // Attempt to create without parameters
                    obj = Activator.CreateInstance(objType);
                }
                catch (Exception)
                {
                }
            }
        }

        return obj;
    }
}
/*
internal class ExchangeSync(IPropertySync sync) : IExchange
{
    private readonly IPropertySync _sync = sync ?? throw new ArgumentNullException(nameof(sync));

    public object Exchange(string name, object value)
    {
        return _sync.Sync(name, value);
    }
}*/