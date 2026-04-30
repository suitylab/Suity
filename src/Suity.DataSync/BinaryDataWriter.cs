using Suity.Helpers;
using Suity.Helpers.Conversion;
using System;

namespace Suity;

/// <summary>
/// Binary Data Writer
/// </summary>
[MultiThreadSecurity(MultiThreadSecurityMethods.Insecure)]
public class BinaryDataWriter : IDataWriter, IDataArrayWriter
{
    protected internal byte[] _buffer;

    private int _offset;

    public BinaryDataWriter()
        : this(256)
    {
    }

    public BinaryDataWriter(int initialCapacity)
    {
        _buffer = new byte[initialCapacity];
    }

    public BinaryDataWriter(byte[] buffer)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        _buffer = buffer;
        _offset = buffer.Length;
    }

    public int Offset
    {
        get => _offset;
        set => _offset = value;
    }

    /// <summary>
    /// Internal array
    /// </summary>
    public byte[] Buffer => _buffer;

    /// <summary>
    /// Ensure that the length of the array can cover the current offset value
    /// </summary>
    public void EnsureBufferOffset()
    {
        EnsureTotalLength(_offset);
    }

    public byte[] ToBytes()
    {
        EnsureTotalLength(_offset);

        byte[] b = new byte[_offset];
        Array.Copy(_buffer, b, _offset);

        return b;
    }

    /// <summary>
    /// Reset Offset
    /// </summary>
    public virtual void Reset()
    {
        _offset = 0;
    }

    /// <summary>
    /// Fired when the array is rebuilt
    /// </summary>
    protected virtual void OnResized()
    {
    }

    private void EnsureSize(int size)
    {
        while (_buffer.Length < _offset + size)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            OnResized();
        }
    }

    private void EnsureTotalLength(int length)
    {
        while (_buffer.Length < length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            OnResized();
        }
    }

    #region IDataWriter

    public IDataWriter Node(string name)
    {
        return this;
    }

    public IDataArrayWriter Nodes(string name, int count)
    {
        if (count < ushort.MaxValue)
        {
            EnsureSize(2);

            EndianBitConverter.Little.CopyBytes((ushort)count, _buffer, _offset);
            _offset += 2;
        }
        else
        {
            EnsureSize(6);

            EndianBitConverter.Little.CopyBytes(ushort.MaxValue, _buffer, _offset);
            _offset += 2;

            EndianBitConverter.Little.CopyBytes(count, _buffer, _offset);
            _offset += 4;
        }

        return this;
    }

    public void WriteEmpty(bool empty)
    {
        EnsureSize(1);
        EndianBitConverter.Little.CopyBytes(!empty, _buffer, _offset);
        _offset++;
    }

    public void WriteTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentNullException();
        }

        int len = typeName.GetUTF8Length();
        if (len > byte.MaxValue)
        {
            throw new InvalidOperationException();
        }

        EnsureSize(len + 1);
        typeName.ToUTF8Bytes(_buffer, _offset + 1);
        _buffer[_offset] = (byte)len;

        _offset += len + 1;
    }

    public void WriteBoolean(bool value)
    {
        EnsureSize(1);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset++;
    }

    public void WriteByte(byte value)
    {
        EnsureSize(1);
        _buffer[_offset] = value;

        _offset++;
    }

    public void WriteSByte(sbyte value)
    {
        throw new NotImplementedException();
    }

    public void WriteInt16(short value)
    {
        EnsureSize(2);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 2;
    }

    public void WriteUInt16(ushort value)
    {
        EnsureSize(2);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 2;
    }

    public void WriteInt32(int value)
    {
        EnsureSize(4);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 4;
    }

    public void WriteUInt32(uint value)
    {
        EnsureSize(4);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 4;
    }

    public void WriteInt64(long value)
    {
        EnsureSize(8);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 8;
    }

    public void WriteUInt64(ulong value)
    {
        EnsureSize(8);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 8;
    }

    public void WriteSingle(float value)
    {
        EnsureSize(4);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 4;
    }

    public void WriteDouble(double value)
    {
        EnsureSize(8);
        EndianBitConverter.Little.CopyBytes(value, _buffer, _offset);

        _offset += 8;
    }

    public void WriteString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            EnsureSize(2);
            EndianBitConverter.Little.CopyBytes((ushort)0, _buffer, _offset);

            _offset += 2;
            return;
        }

        int len = value.GetUTF8Length();
        if (len < ushort.MaxValue)
        {
            EnsureSize(len + 2);
            value.ToUTF8Bytes(_buffer, _offset + 2);
            EndianBitConverter.Little.CopyBytes((ushort)len, _buffer, _offset);

            _offset += len + 2;
        }
        else
        {
            EnsureSize(len + 6);
            value.ToUTF8Bytes(_buffer, _offset + 6);
            EndianBitConverter.Little.CopyBytes(ushort.MaxValue, _buffer, _offset);
            EndianBitConverter.Little.CopyBytes(len, _buffer, _offset + 2);

            _offset += len + 6;
        }
    }

    public void WriteDateTime(DateTime value)
    {
        WriteInt64(value.Ticks);
    }

    public void WriteBytes(byte[] b, int offset, int length)
    {
        EnsureSize(length + 4);
        EndianBitConverter.Little.CopyBytes(length, _buffer, _offset);
        _offset += 4;
        System.Buffer.BlockCopy(b, offset, _buffer, _offset, length);
        _offset += length;
    }

    public void WriteObject(object obj)
    {
    }

    #endregion

    /// <summary>
    /// Writing raw byte arrays
    /// </summary>
    /// <param name="b"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    public void WriteRawBytes(byte[] b, int offset, int length)
    {
        EnsureSize(length);
        System.Buffer.BlockCopy(b, offset, _buffer, _offset, length);
        _offset += length;
    }

    #region IDataArrayWriter

    public IDataWriter Item()
    {
        return this;
    }

    public void Finish()
    {
    }

    #endregion
}