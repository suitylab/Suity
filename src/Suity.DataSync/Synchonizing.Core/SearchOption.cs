namespace Suity.Synchonizing.Core;

/// <summary>
/// Options for searching operations
/// </summary>
public enum SearchOption
{
    None = 0x0,
    MatchCase = 0x1,
    MatchWholeWord = 0x2,
}


/// <summary>
/// Represents the result of a text search operation, containing the location of the match.
/// </summary>
/// <param name="line">The line number where the match was found.</param>
/// <param name="offset">The character offset within the line where the match starts.</param>
/// <param name="length">The length of the matched text.</param>
/// <param name="lineString">The content of the line where the match was found.</param>
public record TextSearchResult(int line, int offset, int length, string lineString);