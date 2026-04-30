using System.Collections.Generic;

namespace Suity.Helpers;

/// <summary>
/// Represents a fragment of code delimited by start and end markers.
/// Provides utilities for parsing and replacing code fragments within a larger code string.
/// </summary>
internal class CodeFragment
{
    /// <summary>
    /// The starting index of the fragment within the original code string.
    /// </summary>
    public int Index;

    /// <summary>
    /// The total length of the fragment, including the start and end markers.
    /// </summary>
    public int Length;

    /// <summary>
    /// The start marker string that delimits the beginning of the fragment.
    /// </summary>
    public string StartId;

    /// <summary>
    /// The end marker string that delimits the end of the fragment.
    /// </summary>
    public string EndId;

    /// <summary>
    /// The code content between the start and end markers (excluding the markers themselves).
    /// </summary>
    public string Code;

    /// <summary>
    /// Parses a code string and yields all fragments found between the specified start and end markers.
    /// </summary>
    /// <param name="code">The code string to parse.</param>
    /// <param name="startId">The start marker string.</param>
    /// <param name="endId">The end marker string.</param>
    /// <returns>An enumerable of <see cref="CodeFragment"/> objects found in the code.</returns>
    public static IEnumerable<CodeFragment> Parse(string code, string startId, string endId)
    {
        if (string.IsNullOrEmpty(code))
        {
            yield break;
        }

        int index = 0;
        while (index < code.Length)
        {
            int iBegin = code.IndexOf(startId, index);
            if (iBegin < 0)
            {
                break;
            }
            index = iBegin + startId.Length;

            int iEnd = code.IndexOf(endId, index);
            if (iEnd < 0)
            {
                break;
            }
            index = iEnd + endId.Length;

            string str = code.Substring(iBegin + startId.Length, iEnd - iBegin - startId.Length);
            yield return new CodeFragment { Index = iBegin, Length = iEnd - iBegin + endId.Length, StartId = startId, EndId = endId, Code = str };
        }

        yield break;
    }

    /// <summary>
    /// Replaces a code fragment in the original code string with a new string.
    /// </summary>
    /// <param name="code">The original code string.</param>
    /// <param name="f">The fragment to replace, as returned by <see cref="Parse"/>.</param>
    /// <param name="newStr">The replacement string.</param>
    /// <returns>A new code string with the fragment replaced.</returns>
    public static string Replace(string code, CodeFragment f, string newStr)
    {
        string before = code[..f.Index];
        string after = code.Substring(f.Index + f.Length, code.Length - f.Index - f.Length);

        return $"{before}{newStr}{after}";
    }
}
