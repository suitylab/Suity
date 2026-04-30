using System;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Writes structured data with hierarchical indentation formatting.
/// Implements <see cref="IDataWriter"/> to support nested nodes, primitive values,
/// and byte arrays, producing a human-readable indented text representation.
/// </summary>
public class IndentDataWriter : IDataWriter
{
    private readonly StringBuilder _builder;
    private readonly int _depth;
    private readonly string _indentation;

    /// <summary>
    /// Initializes a new instance of <see cref="IndentDataWriter"/> with a fresh builder
    /// at the root indentation level.
    /// </summary>
    public IndentDataWriter()
    {
        _builder = new StringBuilder();
        _depth = 1;
        _indentation = new string(' ', _depth * 2); // 2 spaces per level of indentation
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IndentDataWriter"/> that appends to an
    /// existing <see cref="StringBuilder"/> at the specified nesting depth.
    /// </summary>
    /// <param name="builder">The string builder to write output into.</param>
    /// <param name="depth">The current indentation depth level.</param>
    internal IndentDataWriter(StringBuilder builder, int depth)
    {
        _builder = builder;
        _depth = depth;
        _indentation = new string(' ', depth * 2); // 2 spaces per level of indentation
    }

    /// <summary>
    /// Clears all previously written content so the writer can be reused.
    /// </summary>
    public void Reset()
    {
        _builder.Clear();
    }

    /// <summary>
    /// Writes a named node header and returns a new <see cref="IDataWriter"/> for writing
    /// child content at a deeper indentation level.
    /// </summary>
    /// <param name="name">The name of the node to write.</param>
    /// <returns>A child writer scoped to the next indentation level.</returns>
    public IDataWriter Node(string name)
    {
        _builder.AppendLine($"{_indentation}{name}:");
        return new IndentDataWriter(_builder, _depth + 1);
    }

    /// <summary>
    /// Writes a named array header and returns an <see cref="IDataArrayWriter"/> for
    /// writing array items at a deeper indentation level.
    /// </summary>
    /// <param name="name">The name of the array to write.</param>
    /// <param name="count">The expected number of items in the array.</param>
    /// <returns>An array writer scoped to the next indentation level.</returns>
    public IDataArrayWriter Nodes(string name, int count)
    {
        _builder.AppendLine($"{_indentation}{name}:");
        return new IndentDataArrayWriter(_builder, _depth + 1);
    }

    /// <summary>
    /// Writes an underscore placeholder when <paramref name="empty"/> is true,
    /// indicating that the current node contains no data.
    /// </summary>
    /// <param name="empty">Whether the node is empty.</param>
    public void WriteEmpty(bool empty)
    {
        if (empty)
        {
            _builder.AppendLine($"{_indentation}_");
        }
    }

    /// <summary>
    /// Writes the type name of the current data node as a formatted markdown label.
    /// </summary>
    /// <param name="typeName">The type name to display.</param>
    public void WriteTypeName(string typeName)
    {
        _builder.AppendLine($"{_indentation}**Type:** `{typeName}`");
    }

    /// <summary>
    /// Writes a boolean value as a checkmark or cross symbol followed by True or False.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    public void WriteBoolean(bool value)
    {
        _builder.AppendLine($"{_indentation}{(value ? "✔ True" : "✖ False")}");
    }

    /// <summary>
    /// Writes a string value at the current indentation level.
    /// </summary>
    /// <param name="value">The string to write.</param>
    public void WriteString(string value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value formatted as <c>yyyy-MM-dd HH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The date and time to write.</param>
    public void WriteDateTime(DateTime value)
    {
        _builder.AppendLine($"{_indentation}{value.ToString("yyyy-MM-dd HH:mm:ss")}");
    }

    /// <summary>
    /// Writes a 32-bit signed integer at the current indentation level.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    public void WriteInt32(int value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer at the current indentation level.
    /// </summary>
    /// <param name="value">The unsigned integer value to write.</param>
    public void WriteUInt32(uint value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a 64-bit signed integer at the current indentation level.
    /// </summary>
    /// <param name="value">The long integer value to write.</param>
    public void WriteInt64(long value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a 64-bit unsigned integer at the current indentation level.
    /// </summary>
    /// <param name="value">The unsigned long integer value to write.</param>
    public void WriteUInt64(ulong value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a single-precision floating-point number using the general ("G") format.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    public void WriteSingle(float value)
    {
        _builder.AppendLine($"{_indentation}{value.ToString("G")}");
    }

    /// <summary>
    /// Writes a double-precision floating-point number using the general ("G") format.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteDouble(double value)
    {
        _builder.AppendLine($"{_indentation}{value.ToString("G")}");
    }

    /// <summary>
    /// Writes an 8-bit unsigned integer at the current indentation level.
    /// </summary>
    /// <param name="value">The byte value to write.</param>
    public void WriteByte(byte value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes an 8-bit signed integer at the current indentation level.
    /// </summary>
    /// <param name="value">The sbyte value to write.</param>
    public void WriteSByte(sbyte value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a 16-bit signed integer at the current indentation level.
    /// </summary>
    /// <param name="value">The short integer value to write.</param>
    public void WriteInt16(short value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer at the current indentation level.
    /// </summary>
    /// <param name="value">The unsigned short integer value to write.</param>
    public void WriteUInt16(ushort value)
    {
        _builder.AppendLine($"{_indentation}{value}");
    }

    /// <summary>
    /// Writes a byte array segment as a Base64-encoded string wrapped in a code block.
    /// </summary>
    /// <param name="b">The byte array containing the data.</param>
    /// <param name="offset">The zero-based offset in the array where the segment begins.</param>
    /// <param name="length">The number of bytes to write.</param>
    public void WriteBytes(byte[] b, int offset, int length)
    {
        string value = Convert.ToBase64String(b, offset, length);
        _builder.AppendLine($"{_indentation}```");
        _builder.AppendLine(value);
        _builder.AppendLine($"{_indentation}```");
    }

    /// <summary>
    /// Writes an arbitrary object by calling its <see cref="object.ToString"/> method,
    /// or writes "null" if the object is null.
    /// </summary>
    /// <param name="obj">The object to write.</param>
    public void WriteObject(object obj)
    {
        _builder.AppendLine($"{_indentation}{obj?.ToString() ?? "null"}");
    }

    /// <summary>
    /// Returns the complete formatted string that has been written so far.
    /// </summary>
    /// <returns>The accumulated indented text output.</returns>
    public override string ToString()
    {
        return _builder.ToString();
    }
}

/// <summary>
/// Writes arrays of items with hierarchical indentation formatting.
/// Implements <see cref="IDataArrayWriter"/> to produce individual <see cref="IDataWriter"/>
/// instances for each array element at the appropriate nesting depth.
/// </summary>
public class IndentDataArrayWriter : IDataArrayWriter
{
    private readonly StringBuilder _builder;
    private readonly int _depth;
    private readonly string _indentation;

    /// <summary>
    /// Initializes a new instance of <see cref="IndentDataArrayWriter"/> that appends to
    /// an existing <see cref="StringBuilder"/> at the specified nesting depth.
    /// </summary>
    /// <param name="builder">The string builder to write output into.</param>
    /// <param name="depth">The current indentation depth level for array items.</param>
    public IndentDataArrayWriter(StringBuilder builder, int depth)
    {
        _builder = builder;
        _depth = depth;
        _indentation = new string(' ', depth * 2); // 2 spaces per level of indentation
    }

    /// <summary>
    /// Returns a new <see cref="IDataWriter"/> for writing the next array item
    /// at the current indentation level.
    /// </summary>
    /// <returns>A writer scoped to the current array item depth.</returns>
    public IDataWriter Item()
    {
        //_builder.AppendLine($"{_indentation}- ");
        return new IndentDataWriter(_builder, _depth); // + 1);
    }

    /// <summary>
    /// Signals that all array items have been written. Currently a no-op placeholder.
    /// </summary>
    public void Finish()
    {
    }
}
