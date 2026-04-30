using System;
using ComputerBeacon.Json;

namespace Suity.Json;

/// <summary>
/// IDataWriter implementation that writes data to Json objects
/// </summary>
public class JsonDataWriter : IDataWriter
{
    public class NullToken
    {
        public static readonly NullToken Null = new();
    }

    internal Action<object> _setter;
    private object _value;

    public object Value => _value;

    public JsonDataWriter()
    {
        _setter = v => { };
    }

    internal JsonDataWriter(Action<object> setter)
    {
        _setter = setter ?? throw new ArgumentNullException();
    }

    public void Reset()
    {
        _value = null;
    }

    #region IDataWriter

    public IDataWriter Node(string name)
    {
        if (_value is null)
        {
            _value = new JsonObject();
            _setter(_value);
        }

        if (_value is not JsonObject doc)
        {
            return EmptyDataWriter.Empty;
        }

        var childWriter = new JsonDataWriter(v => doc.Add(name, v));

        return childWriter;
    }

    public IDataArrayWriter Nodes(string name, int count)
    {
        if (_value is null)
        {
            _value = new JsonObject();
            _setter(_value);
        }

        if (_value is not JsonObject doc)
        {
            return EmptyDataArrayWriter.Empty;
        }

        var ary = new JsonArray();
        doc.Add(name, ary);

        return new JsonDataArrayWriter(ary);
    }

    public void WriteEmpty(bool empty)
    {
        if (_value != null)
        {
            return;
        }

        if (empty)
        {
            _value = NullToken.Null;
            _setter(null);
        }
    }

    public void WriteTypeName(string typeName)
    {
        if (_value is not JsonObject)
        {
            _value = new JsonObject();
            _setter(_value);
        }

        ((JsonObject)_value).Add("@type", typeName);
    }

    public void WriteBoolean(bool value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteByte(byte value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteSByte(sbyte value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteInt16(short value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteUInt16(ushort value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteInt32(int value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteUInt32(uint value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteInt64(long value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteUInt64(ulong value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteSingle(float value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteDouble(double value)
    {
        _value = value;
        _setter(_value);
    }

    public void WriteString(string value)
    {
        _value = value;
        _setter(value);
    }

    public void WriteDateTime(DateTime value)
    {
        WriteString(value.ToString());
    }

    public void WriteBytes(byte[] b, int offset, int length)
    {
        string value = Convert.ToBase64String(b, offset, length);

        _value = value;
        _setter(_value);
    }

    public void WriteObject(object obj)
    {
        _value = obj;
        _setter(_value);
    }

    #endregion

    public override string ToString()
    {
        return _value?.ToString();
    }

    public string ToString(bool niceFormat)
    {
        if (_value is JsonObject obj)
        {
            return obj.ToString(niceFormat);
        }
        else if (_value is JsonArray ary)
        {
            return ary.ToString(niceFormat);
        }
        else if (_value != null)
        {
            return _value.ToString();
        }
        else
        {
            return null;
        }
    }
}

public class JsonDataArrayWriter : IDataArrayWriter
{
    private readonly JsonArray _ary;
    public JsonArray Value => _ary;

    public JsonDataArrayWriter()
    {
        _ary = [];
    }

    public JsonDataArrayWriter(JsonArray ary)
    {
        _ary = ary ?? throw new ArgumentNullException();
    }

    #region IDataArrayWriter

    public IDataWriter Item()
    {
        JsonDataWriter childWriter = new JsonDataWriter(_ary.Add);

        return childWriter;
    }

    public void Finish()
    {
    }

    #endregion
}