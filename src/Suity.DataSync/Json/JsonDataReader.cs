using System;
using System.Collections.Generic;
using ComputerBeacon.Json;


namespace Suity.Json;

/// <summary>
/// IDataReader implementation that reads data from Json objects
/// </summary>
public class JsonDataReader : IDataReader
{
    private readonly object _obj;

    public JsonDataReader(object obj)
    {
        _obj = obj;
    }

    public JsonDataReader(JsonObject obj)
    {
        _obj = obj;
    }

    public JsonDataReader(string json)
    {
        _obj = Parser.Parse(json);
    }

    #region IDataReader

    public bool RandomAccess => true;

    public bool HasNode(string name) => (_obj as JsonObject)?.ContainsKey(name) == true;

    public IDataReader Node(string name)
    {
        if (_obj is JsonObject jobj)
        {
            return new JsonDataReader(jobj[name]);
        }
        else
        {
            return EmptyDataReader.Empty;
        }
    }

    public IEnumerable<IDataReader> Nodes(string name)
    {
        if (_obj is JsonObject jobj)
        {
            if (jobj[name] is JsonArray ary)
            {
                foreach (var item in ary)
                {
                    yield return new JsonDataReader(item);
                }
            }
        }
    }

    public bool ReadIsEmpty()
    {
        return _obj == null;
    }

    public string ReadTypeName()
    {
        if (_obj is JsonObject jobj)
        {
            return jobj["@type"] as string;
        }
        else
        {
            return null;
        }
    }

    public bool ReadBoolean() => (_obj != null) ? Convert.ToBoolean(_obj) : default;

    public byte ReadByte() => (_obj != null) ? Convert.ToByte(_obj) : default;

    public double ReadDouble() => (_obj != null) ? Convert.ToDouble(_obj) : default;

    public short ReadInt16() => (_obj != null) ? Convert.ToInt16(_obj) : default;

    public int ReadInt32() => (_obj != null) ? Convert.ToInt32(_obj) : default;

    public long ReadInt64() => (_obj != null) ? Convert.ToInt64(_obj) : default;

    public sbyte ReadSByte() => (_obj != null) ? Convert.ToSByte(_obj) : default;

    public float ReadSingle() => (_obj != null) ? Convert.ToSingle(_obj) : default;

    public string ReadString()
    {
        if (_obj is IJsonContainer)
        {
            return null;
        }

        return (_obj != null) ? Convert.ToString(_obj) : null;
    }

    public ushort ReadUInt16() => (_obj != null) ? Convert.ToUInt16(_obj) : default;

    public uint ReadUInt32() => (_obj != null) ? Convert.ToUInt32(_obj) : default;

    public ulong ReadUInt64() => (_obj != null) ? Convert.ToUInt64(_obj) : default;

    public DateTime ReadDateTime()
    {
        string str = ReadString();
        if (DateTime.TryParse(str, out DateTime result))
        {
            return result;
        }
        else
        {
            return default;
        }
    }

    public byte[] ReadBytes()
    {
        string str = ReadString();
        return Convert.FromBase64String(str);
    }

    public object ReadObject() => _obj;

    public IEnumerable<IDataReader> Array()
    {
        if (_obj is JsonArray ary)
        {
            foreach (var item in ary)
            {
                yield return new JsonDataReader(item);
            }
        }
    }

    #endregion

    public override string ToString()
    {
        return _obj?.ToString() ?? string.Empty;
    }

    public static IDataReader Create(string json)
    {
        if (Parser.Parse(json) is JsonObject obj)
        {
            return new JsonDataReader(obj);
        }
        else
        {
            return EmptyDataReader.Empty;
        }
    }
}