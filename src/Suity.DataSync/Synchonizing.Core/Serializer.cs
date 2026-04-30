using Suity.NodeQuery;
using Suity.Synchonizing.Preset;
using System;

namespace Suity.Synchonizing.Core;

public static class Serializer
{
    public static void Serialize(object obj, INodeWriter writer, SyncIntent intent = SyncIntent.Serialize)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (obj is null)
        {
            return;
        }

        Serialize(obj, intent, writer, SyncContext.Empty);
    }

    public static void Serialize(object obj, INodeWriter writer, ISyncTypeResolver resolver, IServiceProvider provider = null, SyncIntent intent = SyncIntent.Serialize)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (obj is null)
        {
            return;
        }

        if (resolver != null)
        {
            string typeName = resolver.ResolveTypeName(obj.GetType(), obj);
            if (!string.IsNullOrEmpty(typeName))
            {
                writer.SetAttribute("type", typeName);
            }
        }

        Serialize(obj, intent, writer, new SyncContext(null, resolver, provider));
    }

    public static void Serialize(object obj, INodeWriter writer, string typeName, SyncIntent intent = SyncIntent.Serialize)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (obj is null)
        {
            return;
        }

        writer.SetAttribute("type", typeName);
        Serialize(obj, intent, writer, SyncContext.Empty);
    }

    private static void Serialize(object obj, SyncIntent intent, INodeWriter writer, SyncContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (obj is null)
        {
            writer.SetAttribute("null", "true");

            return;
        }

        var type = obj.GetType();

        if (type.IsPrimitive || type.IsEnum || obj is string)
        {
            writer.SetValueObj(obj);
        }
        else if (obj is ISerializeAsString serializeAsString)
        {
            writer.SetValueObj(serializeAsString.Key);
        }
        else if (obj is ISyncObject syncObject)
        {
            SerializeSyncObject(syncObject, intent, writer, context);
        }
        else if (obj is ISyncList syncList)
        {
            SerializeSyncList(syncList, intent, writer, context);
        }
        else if (SyncTypes.GetValueResolver(type) is SyncTypes.ValueResolver vr)
        {
            try
            {
                writer.SetValueObj(vr.StringResolve(obj) ?? string.Empty);
            }
            catch (Exception)
            {
                writer.SetValueObj(string.Empty);
            }
        }
        else if (SyncTypes.GetObjectProxyType(obj) != null)
        {
            SerializeSyncObject(SyncTypes.CreateObjectProxy(obj), intent, writer, context);
        }
        else if (SyncTypes.GetListProxyType(obj) != null)
        {
            SerializeSyncList(SyncTypes.CreateListProxy(obj), intent, writer, context);
        }
        else if (obj is INodeReader nodeReader)
        {
            nodeReader.WriteTo(writer);
        }
        else if (context.Resolver != null)
        {
            object proxy = context.Resolver.CreateProxy(obj);

            if (proxy is ISyncObject syncObject2)
            {
                SerializeSyncObject(syncObject2, intent, writer, context);
            }
            else if (proxy is ISyncList syncList2)
            {
                SerializeSyncList(syncList2, intent, writer, context);
            }
            else
            {
                string str = context.Resolver.ResolveObjectValue(obj);
                if (!string.IsNullOrEmpty(str))
                {
                    writer.SetValueObj(str);
                }
            }
        }
        else
        {
            // Do nothing.
        }
    }

    private static void SerializeSyncObject(ISyncObject obj, SyncIntent intent, INodeWriter writer, SyncContext context)
    {
        var sync = new GetAllPropertySync(intent, true);
        obj.Sync(sync, context);

        var childContext = context.CreateNew(obj);
        foreach (var pair in sync.Values)
        {
            if ((pair.Value.Flag & SyncFlag.ByRef) == SyncFlag.ByRef)
            {
                continue;
            }

            if ((pair.Value.Flag & SyncFlag.NoSerialize) == SyncFlag.NoSerialize)
            {
                continue;
            }

            if ((pair.Value.Flag & SyncFlag.AttributeMode) != SyncFlag.AttributeMode)
            {
                writer.SetElement(pair.Key, w =>
                {
                    CheckForTypeName(pair.Value, w, childContext.Resolver, obj);
                    Serialize(pair.Value.Value, intent, w, childContext);
                });
            }
            else
            {
                if (pair.Value.Value is not string)
                {
                    throw new InvalidOperationException("SyncFlag.AttributeMode supports String type only.");
                }

                writer.SetAttribute(pair.Key.TrimStart('@'), pair.Value.Value);
            }
        }
    }

    private static void SerializeSyncList(ISyncList list, SyncIntent intent, INodeWriter writer, SyncContext context)
    {
        var sync = new GetAllIndexSync(intent);
        list.Sync(sync, context);

        var childContext = context.CreateNew(list);
        foreach (var pair in sync.Attributes)
        {
            writer.SetAttribute(pair.Key, pair.Value);
        }

        for (int i = 0; i < sync.Values.Count; i++)
        {
            var info = sync.Values[i];

            if ((info.Flag & SyncFlag.ByRef) == SyncFlag.ByRef)
            {
                continue;
            }

            // When no type is provided, use the default type of the list
            if (info.BaseType == typeof(object))
            {
                info.BaseType = list.GetElementType() ?? typeof(object);
            }

            writer.AddArrayItem(w =>
            {
                CheckForTypeName(info, w, childContext.Resolver, list);
                Serialize(info.Value, intent, w, childContext);
            });
        }
    }

    private static void CheckForTypeName(SyncValueInfo typedValue, INodeWriter writer, ISyncTypeResolver callerResolver, object localResolverObj)
    {
        if (typedValue.Value != null && typedValue.BaseType != typedValue.Value.GetType() && (typedValue.Flag & SyncFlag.ByRef) == SyncFlag.None)
        {
            var globalResolver = SyncTypes._globalResolver;

            if (localResolverObj is ISyncTypeResolver localResolver)
            {
                string typeName = localResolver.ResolveTypeName(typedValue.Value.GetType(), typedValue.Value);
                if (!string.IsNullOrEmpty(typeName))
                {
                    writer.SetAttribute("type", typeName);
                    return;
                }
                else if (typeName == string.Empty) // Default type
                {
                    return;
                }
            }

            if (callerResolver != null)
            {
                string typeName = callerResolver.ResolveTypeName(typedValue.Value.GetType(), typedValue.Value);
                if (!string.IsNullOrEmpty(typeName))
                {
                    writer.SetAttribute("type", typeName);
                    return;
                }
            }

            if (globalResolver != null)
            {
                string typeName = globalResolver.ResolveTypeName(typedValue.Value.GetType(), typedValue.Value);
                if (!string.IsNullOrEmpty(typeName))
                {
                    writer.SetAttribute("type", typeName);
                    return;
                }
            }

            string originTypeName = DefaultSyncTypeResolver.Instance.ResolveTypeName(typedValue.Value.GetType(), typedValue.Value);
            writer.SetAttribute("type", originTypeName);

            //throw new InvalidOperationException("Can not resolve type : " + typedValue.Value.GetType());
        }
    }

    public static object Deserialize(INodeReader reader, ISyncTypeResolver resolver, IServiceProvider provider = null)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        string typeName = reader.GetAttribute("type");
        if (string.IsNullOrEmpty(typeName))
        {
            throw new NullReferenceException("typeName");
        }

        Type type = resolver.ResolveType(typeName, reader.GetStringValue())
            ?? throw new NullReferenceException("ResolveType");

        object obj = Activator.CreateInstance(type)
            ?? throw new NullReferenceException("CreateInstance");

        Deserialize(obj, reader, new SyncContext(null, resolver, provider));

        return obj;
    }

    public static T Deserialize<T>(INodeReader reader, ISyncTypeResolver resolver = null, IServiceProvider provider = null)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var context = new SyncContext(null, resolver, provider);
        object obj = CreateObject(typeof(T), reader, context, null);

        if (obj is T tObj)
        {
            Deserialize(obj, reader, context);
            return tObj;
        }
        else
        {
            return default;
        }
    }

    public static object Deserialize(INodeReader reader, Type type, ISyncTypeResolver resolver = null, IServiceProvider provider = null)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var context = new SyncContext(null, resolver, provider);
        object obj = CreateObject(type, reader, context, null);

        Deserialize(obj, reader, context);

        return obj;
    }

    public static void Deserialize(object obj, INodeReader reader)
    {
        Deserialize(obj, reader, SyncContext.Empty);
    }

    public static void Deserialize(object obj, INodeReader reader, ISyncTypeResolver resolver, IServiceProvider provider = null)
    {
        Deserialize(obj, reader, new SyncContext(null, resolver, provider));
    }

    private static void Deserialize(object obj, INodeReader reader, SyncContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (obj is null)
        {
            // Do nothing.
        }
        else if (obj is ISerializeAsString)
        {
            // Do nothing.
        }
        else if (obj is ISyncObject syncObject)
        {
            DeserializeSyncObject(syncObject, reader, context);
        }
        else if (obj is ISyncList syncList)
        {
            DeserializeSyncList(syncList, reader, context);
        }
        else if (SyncTypes.GetObjectProxyType(obj) != null)
        {
            DeserializeSyncObject(SyncTypes.CreateObjectProxy(obj), reader, context);
        }
        else if (SyncTypes.GetListProxyType(obj) != null)
        {
            DeserializeSyncList(SyncTypes.CreateListProxy(obj), reader, context);
        }
        else if (obj is RawNode rawNode)
        {
            DeserializeRawNode(rawNode, reader, context);
        }
        else if (context.Resolver != null)
        {
            object proxy = context.Resolver.CreateProxy(obj);

            if (proxy is ISyncObject syncObject2)
            {
                DeserializeSyncObject(syncObject2, reader, context);
            }
            else if (proxy is ISyncList syncList2)
            {
                DeserializeSyncList(syncList2, reader, context);
            }
        }
        else
        {
            // Do nothing.
        }
    }

    private static void DeserializeSyncObject(ISyncObject obj, INodeReader reader, SyncContext context)
    {
        SyncContext elementContext = context.CreateNew(obj);

        var sync = new DeserializePropertySync(reader)
        {
            Creater = (elementType, elementReader) =>
            {
                try
                {
                    return CreateObject(elementType, elementReader, context, obj as ISyncTypeResolver);
                }
                catch (TypeResolveException e)
                {
                    e.LogError($"Create object failed: {elementType?.Name}.");

                    return null;
                }
            },

            Deserializer = (elementObj, elementReader) =>
            {
                Deserialize(elementObj, elementReader, elementContext);
            },

            Value = reader.NodeValueObj,
        };

        obj.Sync(sync, context);
    }

    private static void DeserializeSyncList(ISyncList list, INodeReader reader, SyncContext context)
    {
        SyncContext elementContext = context.CreateNew(list);

        var sync = new DeserializeIndexSync(reader)
        {
            Creater = (elementType, elementReader) =>
            {
                try
                {
                    return CreateObjectForList(elementType, elementReader, context, list);
                }
                catch (TypeResolveException e)
                {
                    Logs.LogError(e.Message);
                    return null;
                }
            },

            Deserializer = (elementObj, elementReader) =>
            {
                Deserialize(elementObj, elementReader, elementContext);
            },

            Value = reader.NodeValueObj,
        };

        list.Sync(sync, context);
    }

    private static void DeserializeRawNode(RawNode node, INodeReader reader, SyncContext context)
    {
        node.Read(reader);
    }

    private static object CreateObject(Type type, INodeReader reader, SyncContext context, ISyncTypeResolver localResolver)
    {
        string t = type.Name;

        bool isNull = reader.GetAttribute("null") == "true";
        if (isNull)
        {
            return null;
        }

        string typeName = reader.GetAttribute("type");
        string content = reader.GetStringValue() ?? string.Empty;

        return SyncTypes.CreateObject(type, typeName, content, context.Resolver, localResolver);
    }

    private static object CreateObjectForList(Type type, INodeReader reader, SyncContext context, ISyncList list)
    {
        // Empty judgment
        bool isNull = reader.GetAttribute("null") == "true";
        if (isNull)
        {
            return null;
        }

        // Type override
        string typeName = reader.GetAttribute("type");
        string content = reader.GetStringValue() ?? string.Empty;

        try
        {
            object obj;

            // Attempt to build coverage types and request types
            obj = SyncTypes.CreateObject(type, typeName, content, context.Resolver, list as ISyncTypeResolver);
            if (obj != null)
            {
                return obj;
            }

            // List default constructor construction
            obj = list.CreateNewItem(content);
            if (obj != null)
            {
                if (obj is ISerializeAsString serializeAsString)
                {
                    serializeAsString.Key = content;
                }

                return obj;
            }

            // List default type construction
            var listElementType = list.GetElementType();
            if (listElementType != null && listElementType != typeof(object))
            {
                return SyncTypes.CreateObject(listElementType, typeName, content, context.Resolver, list as ISyncTypeResolver);
            }

            return null;
        }
        catch (TypeResolveException e)
        {
            e.LogError();
            throw;
        }
    }
}