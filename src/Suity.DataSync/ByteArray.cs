namespace Suity;

/// <summary>
/// Byte Array
/// </summary>
[MultiThreadSecurity(MultiThreadSecurityMethods.Insecure)]
public sealed class ByteArray : BinaryDataWriter
{
    private readonly BinaryDataReader _reader;

    public ByteArray()
        : base()
    {
        _reader = new BinaryDataReader(Buffer);
    }

    public ByteArray(int initialCapacity)
        : base(initialCapacity)
    {
        _reader = new BinaryDataReader(Buffer);
    }

    public ByteArray(byte[] buffer)
        : base(buffer)
    {
        _reader = new BinaryDataReader(Buffer);
    }

    /// <summary>
    /// The reader
    /// </summary>
    public BinaryDataReader Reader => _reader;

    /// <summary>
    /// The degree between the writer offset and the reader offset
    /// </summary>
    public int RestFromReader => Offset - _reader.Offset;

    /// <inheritdoc/>
    public override void Reset()
    {
        base.Reset();
        _reader.Reset();
    }

    protected override void OnResized()
    {
        _reader._buffer = Buffer;
    }
}