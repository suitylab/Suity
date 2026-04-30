using System;
using System.Collections.Generic;

namespace Suity;

/// <summary>
/// Data reader interface
/// </summary>
public interface IDataReader
{
    bool RandomAccess { get; }

    bool HasNode(string name);

    IDataReader Node(string name);
    IEnumerable<IDataReader> Nodes(string name);

    bool ReadIsEmpty();
    string ReadTypeName();


    bool ReadBoolean();
    byte ReadByte();
    sbyte ReadSByte();
    short ReadInt16();
    ushort ReadUInt16();
    int ReadInt32();
    uint ReadUInt32();
    long ReadInt64();
    ulong ReadUInt64();
    float ReadSingle();
    double ReadDouble();
    string ReadString();
    DateTime ReadDateTime();
    byte[] ReadBytes();
    object ReadObject();
    IEnumerable<IDataReader> Array();
}

public sealed class EmptyDataReader : IDataReader
{
    public static EmptyDataReader Empty { get; } = new();

    private EmptyDataReader()
    { }

    #region IDataReader

    public bool RandomAccess => false;

    public bool HasNode(string name) => false;

    public IDataReader Node(string name) => this;

    public IEnumerable<IDataReader> Nodes(string name) { yield break; }

    public bool ReadIsEmpty() => true;

    public string ReadTypeName() => null;

    public bool ReadBoolean() => default;

    public byte ReadByte() => default;

    public sbyte ReadSByte() => default;

    public short ReadInt16() => default;

    public ushort ReadUInt16() => default;

    public int ReadInt32() => default;

    public uint ReadUInt32() => default;

    public long ReadInt64() => default;

    public ulong ReadUInt64() => default;

    public float ReadSingle() => default;

    public double ReadDouble() => default;

    public string ReadString() => default;

    public DateTime ReadDateTime() => default;

    public byte[] ReadBytes() => [];

    public object ReadObject() => null;

    public IEnumerable<IDataReader> Array() => [];

    #endregion
}