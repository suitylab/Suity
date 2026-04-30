using System;

namespace Suity;

/// <summary>
/// Data writer interface
/// </summary>
public interface IDataWriter
{
    IDataWriter Node(string name);

    IDataArrayWriter Nodes(string name, int count);

    void WriteEmpty(bool empty);

    void WriteTypeName(string typeName);

    void WriteBoolean(bool value);

    void WriteByte(byte value);

    void WriteSByte(sbyte value);

    void WriteInt16(short value);

    void WriteUInt16(ushort value);

    void WriteInt32(int value);

    void WriteUInt32(uint value);

    void WriteInt64(long value);

    void WriteUInt64(ulong value);

    void WriteSingle(float value);

    void WriteDouble(double value);

    void WriteString(string value);

    void WriteDateTime(DateTime value);

    void WriteBytes(byte[] b, int offset, int length);

    void WriteObject(object obj);
}

/// <summary>
/// Data array writer interface for writing arrays of data
/// </summary>
public interface IDataArrayWriter
{
    IDataWriter Item();

    void Finish();
}

/// <summary>
/// Interface for types that can write their data to an IDataWriter
/// </summary>
public interface IDataWritable
{
    void WriteData(IDataWriter writer);
}

public sealed class EmptyDataWriter : IDataWriter
{
    public static EmptyDataWriter Empty { get; } = new();

    private EmptyDataWriter()
    { }

    #region IDataWriter

    public IDataWriter Node(string name)
    {
        return EmptyDataWriter.Empty;
    }

    public IDataArrayWriter Nodes(string name, int count)
    {
        return EmptyDataArrayWriter.Empty;
    }

    public void WriteEmpty(bool empty)
    {
    }

    public void WriteTypeName(string typeName)
    {
    }

    public void WriteBoolean(bool value)
    {
    }

    public void WriteByte(byte value)
    {
    }

    public void WriteSByte(sbyte value)
    {
    }

    public void WriteInt16(short value)
    {
    }

    public void WriteUInt16(ushort value)
    {
    }

    public void WriteInt32(int value)
    {
    }

    public void WriteUInt32(uint value)
    {
    }

    public void WriteInt64(long value)
    {
    }

    public void WriteUInt64(ulong value)
    {
    }

    public void WriteSingle(float value)
    {
    }

    public void WriteDouble(double value)
    {
    }

    public void WriteString(string value)
    {
    }

    public void WriteDateTime(DateTime value)
    {
    }

    public void WriteBytes(byte[] b, int offset, int length)
    {
    }

    public void WriteObject(object obj)
    {
    }

    #endregion
}

/// <summary>
/// Empty implementation of IDataArrayWriter that does nothing
/// </summary>
public sealed class EmptyDataArrayWriter : IDataArrayWriter
{
    public static readonly EmptyDataArrayWriter Empty = new();

    private EmptyDataArrayWriter()
    { }

    #region IDataArrayWriter

    public IDataWriter Item() => EmptyDataWriter.Empty;

    public void Finish()
    {
    }

    #endregion
}

public static class IDataWriterExtensions
{
    public static void WriteObject(this IDataWriter writer, object obj)
    {
        if (obj is null)
        {
            writer.WriteEmpty(true);
            return;
        }

        var typeCode = Type.GetTypeCode(obj.GetType());

        switch (typeCode)
        {
            case TypeCode.Boolean:
                writer.WriteBoolean((bool)obj);
                break;
            case TypeCode.Byte:
                writer.WriteByte((byte)obj);
                break;
            case TypeCode.Char:
                writer.WriteEmpty(true);
                break;
            case TypeCode.DateTime:
                writer.WriteDateTime((DateTime)obj);
                break;
            case TypeCode.DBNull:
                writer.WriteEmpty(true);
                break;
            case TypeCode.Decimal:
                writer.WriteDouble(Convert.ToDouble(obj));
                break;
            case TypeCode.Double:
                writer.WriteDouble((double)obj);
                break;
            case TypeCode.Empty:
                writer.WriteEmpty(true);
                break;
            case TypeCode.Int16:
                writer.WriteInt16((short)obj);
                break;
            case TypeCode.Int32:
                writer.WriteInt32((int)obj);
                break;
            case TypeCode.Int64:
                writer.WriteInt64((long)obj);
                break;
            case TypeCode.Object:
                writer.WriteEmpty(true);
                break;
            case TypeCode.SByte:
                writer.WriteSByte((sbyte)obj);
                break;
            case TypeCode.Single:
                writer.WriteSingle((float)obj);
                break;
            case TypeCode.String:
                writer.WriteString((string)obj);
                break;
            case TypeCode.UInt16:
                writer.WriteUInt16((ushort)obj);
                break;
            case TypeCode.UInt32:
                writer.WriteUInt32((uint)obj);
                break;
            case TypeCode.UInt64:
                writer.WriteUInt64((ulong)obj);
                break;
            default:
                writer.WriteEmpty(true);
                break;
        }
    }
}