using System.Collections.Generic;
using System.Linq;

namespace Suity.Helpers;

/// <summary>
/// Validates that a string contains only characters from a predefined set of allowed characters.
/// Provides predefined validators for common patterns like filenames and word identifiers.
/// </summary>
public class StringCharVarifier
{
    /// <summary>
    /// A character set valid for filenames, including letters, digits, and common punctuation.
    /// </summary>
    public const string FileName = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890`~!#@%$^&()-=_+[]{};',.";

    /// <summary>
    /// A character set valid for word identifiers, including letters, digits, and underscores.
    /// </summary>
    public const string Word = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";

    /// <summary>
    /// A predefined validator for word identifiers using the <see cref="Word"/> character set.
    /// </summary>
    public static readonly StringCharVarifier WordVarifier = new(Word);

    /// <summary>
    /// A predefined validator for filenames using the <see cref="FileName"/> character set.
    /// </summary>
    public static readonly StringCharVarifier FileNameVarifier = new(FileName);

    private readonly HashSet<char> _dicChars = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StringCharVarifier"/> class with the specified pattern of allowed characters.
    /// </summary>
    /// <param name="pattern">A string containing all allowed characters.</param>
    public StringCharVarifier(string pattern)
    {
        foreach (char c in pattern)
        {
            _dicChars.Add(c);
        }
    }

    /// <summary>
    /// Verifies whether all characters in the given string are contained in the allowed character set.
    /// </summary>
    /// <param name="str">The string to verify.</param>
    /// <returns>True if all characters in the string are valid; otherwise, false.</returns>
    public bool Varify(string str)
    {
        return str.All(_dicChars.Contains);
    }
}
