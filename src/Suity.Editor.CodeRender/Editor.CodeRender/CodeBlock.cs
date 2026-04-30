namespace Suity.Editor.CodeRender;

/// <summary>
/// Represents a block of code that can be manipulated and converted to string.
/// </summary>
public class CodeBlock
{
    /// <summary>
    /// An empty code block instance.
    /// </summary>
    public static readonly CodeBlock Empty = new CodeBlock(string.Empty);

    /// <summary>
    /// Gets the code string contained in this block.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Creates a new code block with the specified code.
    /// </summary>
    /// <param name="code">The code string.</param>
    public CodeBlock(string code)
    {
        Code = code;
    }

    /// <summary>
    /// Returns a new code block with the specified indentation applied.
    /// </summary>
    /// <param name="indent">The number of indentation levels (each level is 4 spaces).</param>
    /// <returns>A new indented code block.</returns>
    public CodeBlock Indent(int indent)
    {
        string str = new string(' ', indent * 4) + Code.Replace("\r\n", "\r\n" + new string(' ', indent * 4)).TrimEnd(' ');
        return str;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Code;
    }

    /// <summary>
    /// Implicitly converts a CodeBlock to a string.
    /// </summary>
    /// <param name="block">The code block.</param>
    public static implicit operator string(CodeBlock block)
    {
        return block.Code;
    }

    /// <summary>
    /// Implicitly converts a string to a CodeBlock.
    /// </summary>
    /// <param name="str">The code string.</param>
    public static implicit operator CodeBlock(string str)
    {
        return new CodeBlock(str);
    }
}