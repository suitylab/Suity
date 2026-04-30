using System;
using System.Collections.Generic;

namespace Suity.Editor.Values;

/// <summary>
/// Provides data reading capabilities for <see cref="SObject"/> and related types,
/// implementing the <see cref="IDataReader"/> interface for serialization and deserialization.
/// </summary>
public class SObjectDataReader : IDataReader
{
    private readonly object _obj;

    /// <summary>
    /// Initializes a new instance with the specified object.
    /// </summary>
    /// <param name="obj">The object to read data from.</param>
    public SObjectDataReader(object obj)
    {
        _obj = obj;
    }

    #region IDataReader

    /// <inheritdoc/>
    public bool RandomAccess => true;

    /// <inheritdoc/>
    public bool HasNode(string name) => (_obj as SObject)?.ContainsProperty(name) == true;

    /// <inheritdoc/>
    public IDataReader Node(string name)
    {
        if (_obj is SObject obj)
        {
            return new SObjectDataReader(obj[name]);
        }
        else
        {
            return EmptyDataReader.Empty;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IDataReader> Nodes(string name)
    {
        if (_obj is SObject obj)
        {
            if (obj[name] is SArray ary)
            {
                foreach (var item in ary.GetValues())
                {
                    yield return new SObjectDataReader(item);
                }
            }
        }
    }

    /// <inheritdoc/>
    public bool ReadIsEmpty()
    {
        return _obj == null;
    }

    /// <inheritdoc/>
    public string ReadTypeName()
    {
        if (_obj is SObject jobj)
        {
            return jobj["@type"] as string;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool ReadBoolean()
    {
        if (_obj is bool x)
        {
            return x;
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public byte ReadByte()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToByte(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public sbyte ReadSByte()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToSByte(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public short ReadInt16()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToInt16(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public ushort ReadUInt16()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToUInt16(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public int ReadInt32()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToInt32(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public uint ReadUInt32()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToUInt32(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public long ReadInt64()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToInt64(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public ulong ReadUInt64()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToUInt64(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public float ReadSingle()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToSingle(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public double ReadDouble()
    {
        if (_obj is int || _obj is long || _obj is double)
        {
            return Convert.ToDouble(_obj);
        }
        else
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public string ReadString()
    {
        return _obj?.ToString();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public byte[] ReadBytes()
    {
        string str = ReadString();

        return Convert.FromBase64String(str);
    }

    /// <inheritdoc/>
    public object ReadObject()
    {
        return _obj;
    }

    /// <inheritdoc/>
    public IEnumerable<IDataReader> Array()
    {
        if (_obj is SArray ary)
        {
            foreach (var item in ary.GetValues())
            {
                yield return new SObjectDataReader(item);
            }
        }
    }

    #endregion
}
