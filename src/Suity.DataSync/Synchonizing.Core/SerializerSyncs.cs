using Suity.NodeQuery;
using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

internal delegate object DeserializeElementCreate(Type type, INodeReader reader);

internal delegate void DeserializeElementDeserialize(object obj, INodeReader reader);

internal class DeserializePropertySync(INodeReader reader) : MarshalByRefObject, IPropertySync
{
    private readonly INodeReader _reader = reader;

    public DeserializeElementCreate Creater;
    public DeserializeElementDeserialize Deserializer;

    public SyncMode Mode => SyncMode.SetAll;
    public SyncIntent Intent => SyncIntent.Serialize;

    public string Name => default;
    public IEnumerable<string> Names => _reader.NodeNames;
    public object Value { get; internal set; }

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if ((flag & SyncFlag.NoSerialize) == SyncFlag.NoSerialize)
        {
            return obj;
        }

        bool readOnly = (flag & SyncFlag.GetOnly) == SyncFlag.GetOnly;
        bool notNull = (flag & SyncFlag.NotNull) == SyncFlag.NotNull;
        bool attrMode = (flag & SyncFlag.AttributeMode) == SyncFlag.AttributeMode;

        if (!attrMode)
        {
            INodeReader subReader = _reader.Node(name);
            if (!subReader.Exist)
            {
                return (readOnly || notNull) ? obj : defaultValue;
            }

            object resultObj = readOnly ? obj : Creater(typeof(T), subReader);

            if (resultObj is T tObj)
            {
                Deserializer(resultObj, subReader);

                return tObj;
            }
            else
            {
                T noResult = readOnly ? obj : defaultValue;
                if (noResult == null && notNull)
                {
                    noResult = obj;
                }

                return noResult;
            }
        }
        else
        {
            if (typeof(T) != typeof(string))
            {
                throw new InvalidOperationException("SyncFlag.AttributeMode supports String type only.");
            }
            object resultObj = readOnly ? (object)obj : _reader.GetAttribute(name);

            if (resultObj is T t)
            {
                return t;
            }
            else
            {
                T noResult = readOnly ? obj : defaultValue;
                if (noResult == null && notNull)
                {
                    noResult = obj;
                }

                return noResult;
            }
        }
    }
}

internal class DeserializeIndexSync(INodeReader reader) : MarshalByRefObject, IIndexSync
{
    private readonly INodeReader _reader = reader;

    public DeserializeElementCreate Creater;
    public DeserializeElementDeserialize Deserializer;

    public SyncMode Mode => SyncMode.SetAll;
    public SyncIntent Intent => SyncIntent.Serialize;

    public int Count => _reader.ChildCount;
    public int Index => -1;
    public object Value { get; internal set; }

    public int SyncCount(int count)
    {
        return _reader.ChildCount;
    }

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        bool readOnly = (flag & SyncFlag.GetOnly) == SyncFlag.GetOnly;
        bool notNull = (flag & SyncFlag.NotNull) == SyncFlag.NotNull;

        INodeReader subReader = _reader.Node(index);
        if (!subReader.Exist)
        {
            return (readOnly || notNull) ? obj : default;
        }

        object result = readOnly ? obj : Creater(typeof(T), subReader);

        if (result is T tResult)
        {
            Deserializer(result, subReader);

            return tResult;
        }
        else
        {
            T noResult = readOnly ? obj : default;
            if (noResult == null && notNull)
            {
                noResult = obj;
            }

            return noResult;
        }
    }

    public string SyncAttribute(string name, string value)
    {
        return _reader.GetAttribute(name);
    }
}