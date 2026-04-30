using System;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Writes structured data in Markdown format, implementing the <see cref="IDataWriter"/> interface.
/// Uses heading levels to represent nesting depth and formats values using Markdown syntax.
/// </summary>
public class MarkdownDataWriter : IDataWriter
{
    private readonly StringBuilder _builder;
    private readonly int _depth;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownDataWriter"/> class with a new empty builder at depth 1.
    /// </summary>
    public MarkdownDataWriter()
    {
        _builder = new StringBuilder();
        _depth = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownDataWriter"/> class using an existing <see cref="StringBuilder"/> and nesting depth.
    /// </summary>
    /// <param name="builder">The string builder to append Markdown content to.</param>
    /// <param name="depth">The current nesting depth used to determine heading levels.</param>
    internal MarkdownDataWriter(StringBuilder builder, int depth)
    {
        _builder = builder;
        _depth = depth;
    }

    /// <summary>
    /// Clears all previously written content, resetting the writer to an empty state.
    /// </summary>
    public void Reset()
    {
        _builder.Clear();
    }

    /// <summary>
    /// Writes a named node as a Markdown heading and returns a new writer for its child content at an increased depth.
    /// </summary>
    /// <param name="name">The name of the node to write as a heading.</param>
    /// <returns>A new <see cref="IDataWriter"/> instance for writing child content at the next nesting level.</returns>
    public IDataWriter Node(string name)
    {
        int headerLevel = Math.Min(_depth + 1, 6);
        _builder.AppendLine($"{new string('#', headerLevel)} {name}");
        return new MarkdownDataWriter(_builder, _depth + 1);
    }

    /// <summary>
    /// Writes a named collection of nodes as a Markdown heading and returns an array writer for its items.
    /// </summary>
    /// <param name="name">The name of the node collection to write as a heading.</param>
    /// <param name="count">The expected number of items in the collection.</param>
    /// <returns>An <see cref="IDataArrayWriter"/> instance for writing individual items in the collection.</returns>
    public IDataArrayWriter Nodes(string name, int count)
    {
        int headerLevel = Math.Min(_depth + 1, 6);
        _builder.AppendLine($"{new string('#', headerLevel)} {name}");
        return new MarkdownDataArrayWriter(_builder, _depth + 1);
    }

    /// <summary>
    /// Writes an underscore indicator when the data is empty.
    /// </summary>
    /// <param name="empty">Whether the current data node is empty.</param>
    public void WriteEmpty(bool empty)
    {
        if (empty)
        {
            _builder.AppendLine("_");
        }
    }

    /// <summary>
    /// Writes the type name of the current data node as bold inline code.
    /// </summary>
    /// <param name="typeName">The type name to write.</param>
    public void WriteTypeName(string typeName)
    {
        _builder.AppendLine($"**Type:** `{typeName}`");
    }

    /// <summary>
    /// Writes a boolean value as a checkmark or cross symbol followed by True or False.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    public void WriteBoolean(bool value)
    {
        _builder.AppendLine(value ? "✔ True" : "✖ False");
    }

    /// <summary>
    /// Writes a string value as plain text.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteString(string value)
    {
        _builder.AppendLine(value);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value formatted as yyyy-MM-dd HH:mm:ss.
    /// </summary>
    /// <param name="value">The date and time value to write.</param>
    public void WriteDateTime(DateTime value)
    {
        _builder.AppendLine(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    /// <summary>
    /// Writes a 32-bit signed integer value as text.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    public void WriteInt32(int value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer value as text.
    /// </summary>
    /// <param name="value">The unsigned integer value to write.</param>
    public void WriteUInt32(uint value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a 64-bit signed integer value as text.
    /// </summary>
    /// <param name="value">The long integer value to write.</param>
    public void WriteInt64(long value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a 64-bit unsigned integer value as text.
    /// </summary>
    /// <param name="value">The unsigned long integer value to write.</param>
    public void WriteUInt64(ulong value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a single-precision floating-point value using the general format specifier.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    public void WriteSingle(float value)
    {
        _builder.AppendLine(value.ToString("G"));
    }

    /// <summary>
    /// Writes a double-precision floating-point value using the general format specifier.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteDouble(double value)
    {
        _builder.AppendLine(value.ToString("G"));
    }

    /// <summary>
    /// Writes an 8-bit unsigned integer (byte) value as text.
    /// </summary>
    /// <param name="value">The byte value to write.</param>
    public void WriteByte(byte value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes an 8-bit signed integer (sbyte) value as text.
    /// </summary>
    /// <param name="value">The sbyte value to write.</param>
    public void WriteSByte(sbyte value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a 16-bit signed integer (short) value as text.
    /// </summary>
    /// <param name="value">The short integer value to write.</param>
    public void WriteInt16(short value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer (ushort) value as text.
    /// </summary>
    /// <param name="value">The unsigned short integer value to write.</param>
    public void WriteUInt16(ushort value)
    {
        _builder.AppendLine(value.ToString());
    }

    /// <summary>
    /// Writes a byte array segment as a Base64-encoded string inside a fenced code block.
    /// </summary>
    /// <param name="b">The byte array containing the data.</param>
    /// <param name="offset">The starting index in the array.</param>
    /// <param name="length">The number of bytes to write.</param>
    public void WriteBytes(byte[] b, int offset, int length)
    {
        string value = Convert.ToBase64String(b, offset, length);
        _builder.AppendLine("```");
        _builder.AppendLine(value);
        _builder.AppendLine("```");
    }

    /// <summary>
    /// Writes an arbitrary object by calling its <see cref="object.ToString"/> method, or "null" if the object is null.
    /// </summary>
    /// <param name="obj">The object to write.</param>
    public void WriteObject(object obj)
    {
        _builder.AppendLine(obj?.ToString() ?? "null");
    }

    /// <summary>
    /// Returns the complete Markdown content written so far.
    /// </summary>
    /// <returns>The Markdown-formatted string built by this writer.</returns>
    public override string ToString()
    {
        return _builder.ToString();
    }

}

/// <summary>
/// Writes arrays of data items in Markdown format using list syntax, implementing the <see cref="IDataArrayWriter"/> interface.
/// Each item is prefixed with a Markdown list bullet and delegated to a <see cref="MarkdownDataWriter"/> for content.
/// </summary>
public class MarkdownDataArrayWriter : IDataArrayWriter
{
    private readonly StringBuilder _builder;
    private readonly int _depth;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownDataArrayWriter"/> class using an existing <see cref="StringBuilder"/> and nesting depth.
    /// </summary>
    /// <param name="builder">The string builder to append Markdown content to.</param>
    /// <param name="depth">The current nesting depth used for child writers.</param>
    public MarkdownDataArrayWriter(StringBuilder builder, int depth)
    {
        _builder = builder;
        _depth = depth;
    }

    /// <summary>
    /// Begins writing a new array item by appending a Markdown list bullet and returns a writer for the item's content.
    /// </summary>
    /// <returns>An <see cref="IDataWriter"/> instance for writing the content of the current array item.</returns>
    public IDataWriter Item()
    {
        _builder.Append("- ");
        return new MarkdownDataWriter(_builder, _depth);
    }

    /// <summary>
    /// Completes the array writing operation. Currently a no-op as Markdown list items are written inline.
    /// </summary>
    public void Finish()
    {
    }
}
