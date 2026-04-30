using Suity.Helpers;
using Suity.Helpers.Conversion;
using System;
using System.Collections.Generic;

namespace Suity;

/// <summary>
/// Binary Data Reader
/// </summary>
[MultiThreadSecurity(MultiThreadSecurityMethods.Insecure)]
public class BinaryDataReader : IDataReader
{
    protected internal byte[] _buffer;
    private int _offset;

    internal BinaryDataReader()
    {
    }

    public BinaryDataReader(byte[] buffer)
    {
        _buffer = buffer;
    }

    public BinaryDataReader(byte[] buffer, int offset)
    {
        _buffer = buffer;
        _offset = offset;
    }

    public int Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public byte[] Buffer => _buffer;

    public int Length => _buffer.Length;
    public int Rest => _buffer.Length - _offset;

    #region IDataReader

    public bool RandomAccess => false;

    public bool HasNode(string name) => false;

    public IDataReader Node(string name)
    {
        return this;
    }

    public IEnumerable<IDataReader> Nodes(string name)
    {
        return Array();
    }

    public bool ReadIsEmpty()
    {
        bool isEmpty = !EndianBitConverter.Little.ToBoolean(_buffer, _offset);
        _offset++;

        return isEmpty;
    }

    public string ReadTypeName()
    {
        int len = _buffer[_offset];
        _offset++;
        string typeName = ByteHelpers.GetUTF8(_buffer, _offset, len);
        _offset += len;

        return typeName;
    }

    public bool ReadBoolean()
    {
        bool value = EndianBitConverter.Little.ToBoolean(_buffer, _offset);
        _offset++;

        return value;
    }

    public byte ReadByte()
    {
        byte value = _buffer[_offset];
        _offset++;

        return value;
    }

    public sbyte ReadSByte()
    {
        throw new NotImplementedException();
    }

    public short ReadInt16()
    {
        short value = EndianBitConverter.Little.ToInt16(_buffer, _offset);
        _offset += 2;

        return value;
    }

    public ushort ReadUInt16()
    {
        ushort value = EndianBitConverter.Little.ToUInt16(_buffer, _offset);
        _offset += 2;

        return value;
    }

    public int ReadInt32()
    {
        int value = EndianBitConverter.Little.ToInt32(_buffer, _offset);
        _offset += 4;

        return value;
    }

    public uint ReadUInt32()
    {
        uint value = EndianBitConverter.Little.ToUInt32(_buffer, _offset);
        _offset += 4;

        return value;
    }

    public long ReadInt64()
    {
        long value = EndianBitConverter.Little.ToInt64(_buffer, _offset);
        _offset += 8;

        return value;
    }

    public ulong ReadUInt64()
    {
        ulong value = EndianBitConverter.Little.ToUInt64(_buffer, _offset);
        _offset += 8;

        return value;
    }

    public float ReadSingle()
    {
        float value = EndianBitConverter.Little.ToSingle(_buffer, _offset);
        _offset += 4;

        return value;
    }

    public double ReadDouble()
    {
        double value = EndianBitConverter.Little.ToDouble(_buffer, _offset);
        _offset += 8;

        return value;
    }

    public string ReadString()
    {
        int len = EndianBitConverter.Little.ToUInt16(_buffer, _offset);
        _offset += 2;

        if (len == ushort.MaxValue)
        {
            len = EndianBitConverter.Little.ToInt32(_buffer, _offset);
            _offset += 4;
        }

        string value = ByteHelpers.GetUTF8(_buffer, _offset, len);
        _offset += len;

        return value;
    }

    public DateTime ReadDateTime()
    {
        long ticks = ReadInt64();

        return new DateTime(ticks);
    }

    public byte[] ReadBytes()
    {
        int len = EndianBitConverter.Little.ToInt32(_buffer, _offset);
        _offset += 4;
        byte[] data = new byte[len];
        System.Buffer.BlockCopy(_buffer, _offset, data, 0, len);
        _offset += len;

        return data;
    }

    public object ReadObject() => null;

    public IEnumerable<IDataReader> Array()
    {
        int len = EndianBitConverter.Little.ToUInt16(_buffer, _offset);
        _offset += 2;
        if (len == ushort.MaxValue)
        {
            len = EndianBitConverter.Little.ToInt32(_buffer, _offset);
            _offset += 4;
        }

        for (int i = 0; i < len; i++)
        {
            yield return this;
        }
    }

    #endregion

    public virtual void Reset()
    {
        _offset = 0;
    }

    public byte[] ReadRawBytes(int len)
    {
        byte[] data = new byte[len];
        System.Buffer.BlockCopy(_buffer, _offset, data, 0, len);
        _offset += len;

        return data;
    }

    public void CopyTo(byte[] b, int srcOffset, int destOffset, int length)
    {
        System.Buffer.BlockCopy(_buffer, srcOffset, b, destOffset, length);
    }
}