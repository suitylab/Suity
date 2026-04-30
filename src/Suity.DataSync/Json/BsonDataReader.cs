using Kernys.Bson;
using System;
using System.Collections.Generic;

namespace Suity.Json;

/// <summary>
/// IDataReader implementation that reads data from BSON objects
/// </summary>
public class BsonDataReader : IDataReader
{
    private readonly object _obj;

    public BsonDataReader(object obj)
    {
        _obj = obj;
    }

    private BsonDataReader(BSONValue value)
    {
        _obj = value;
    }

    private BsonDataReader(BSONObject value)
    {
        _obj = value;
    }

    public BsonDataReader(byte[] buf)
    {
        _obj = SimpleBSON.Load(buf);
    }

    public BsonDataReader(byte[] buf, int offset, int length)
    {
        _obj = SimpleBSON.Load(buf, offset, length);
    }

    #region IDataReader

    public bool RandomAccess => true;

    public bool HasNode(string name) => (_obj as BSONObject)?.ContainsKey(name) == true;

    public IDataReader Node(string name)
    {
        BSONObject jobj = _obj as BSONObject;
        if (jobj != null)
        {
            return new BsonDataReader(jobj[name]);
        }
        else
        {
            return EmptyDataReader.Empty;
        }
    }

    public IEnumerable<IDataReader> Nodes(string name)
    {
        BSONObject jobj = _obj as BSONObject;
        if (jobj != null)
        {
            BSONArray ary = jobj[name] as BSONArray;
            if (ary != null)
            {
                foreach (var item in ary)
                {
                    yield return new BsonDataReader(item);
                }
            }
        }
    }

    public bool ReadIsEmpty()
    {
        BSONValue v = _obj as BSONValue;
        return v?.valueType == BSONValue.ValueType.None;
    }

    public string ReadTypeName()
    {
        BSONObject jobj = _obj as BSONObject;
        if (jobj != null)
        {
            return jobj["@type"]?.stringValueSafe;
        }
        else
        {
            return null;
        }
    }

    public bool ReadBoolean()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.boolValueSafe;

            case bool value:
                return value;

            default:
                return default;
        }
    }

    public byte ReadByte()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (byte)b.int32ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToByte(_obj);

            default:
                return default;
        }
    }

    public sbyte ReadSByte()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (sbyte)b.int32ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToSByte(_obj);

            default:
                return default;
        }
    }

    public short ReadInt16()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (short)b.int32ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToInt16(_obj);

            default:
                return default;
        }
    }

    public ushort ReadUInt16()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (ushort)b.int32ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToUInt16(_obj);

            default:
                return default;
        }
    }

    public int ReadInt32()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.int32ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToInt32(_obj);

            default:
                return default;
        }
    }

    public uint ReadUInt32()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (uint)b.int64ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToUInt32(_obj);

            default:
                return default;
        }
    }

    public long ReadInt64()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.int64ValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToInt64(_obj);

            default:
                return default;
        }
    }

    public ulong ReadUInt64()
    {
        switch (_obj)
        {
            case BSONValue b:
                ulong result;
                UInt64.TryParse(b.stringValueSafe, out result);
                return result;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToUInt64(_obj);

            default:
                return default;
        }
    }

    public float ReadSingle()
    {
        switch (_obj)
        {
            case BSONValue b:
                return (float)b.doubleValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToSingle(_obj);

            default:
                return default;
        }
    }

    public double ReadDouble()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.doubleValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return Convert.ToDouble(_obj);

            default:
                return default;
        }
    }

    public string ReadString()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.stringValueSafe;

            case Object p when p.GetType().IsPrimitive:
                return p.ToString();

            default:
                return null;
        }
    }

    public DateTime ReadDateTime()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.dateTimeValueSafe;

            case DateTime p:
                return p;

            default:
                return default;
        }
    }

    public byte[] ReadBytes()
    {
        switch (_obj)
        {
            case BSONValue b:
                return b.binaryValueSafe;

            case Byte[] p:
                return p;

            default:
                return [];
        }
    }

    public object ReadObject() => _obj;

    public IEnumerable<IDataReader> Array()
    {
        BSONArray ary = _obj as BSONArray;
        if (ary != null)
        {
            foreach (var item in ary)
            {
                yield return new BsonDataReader(item);
            }
        }
    }

    #endregion

    public static BsonDataReader LoadBsonDocument(byte[] buf, int offset, int length)
    {
        BSONObject obj = SimpleBSON.Load(buf, offset, length);
        if (obj == null)
        {
            throw new InvalidOperationException();
        }
        return new BsonDataReader(obj);
    }
}