using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Suity.Editor.AIGC.Tools;

internal static class StringUtility
{
    public struct MatchResult
    {
        public int Index;
        public int Length;
        public bool Found => Index >= 0;
        public static MatchResult NotFound => new() { Index = -1, Length = 0 };
    }

    public static MatchResult IndexOfContent(string content, string pattern, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pattern))
            return MatchResult.NotFound;

        int patternLen = pattern.Length;
        int contentLen = content.Length;

        for (int i = startIndex; i <= contentLen - patternLen; i++)
        {
            int pi = 0;
            int ci = i;
            int matchStart = i;

            while (pi < patternLen && ci < contentLen)
            {
                char pc = pattern[pi];
                char cc = content[ci];

                if (cc == '\r' && pi < patternLen - 1 && pattern[pi + 1] == '\n')
                    continue;

                if (pc == '\r' && cc == '\n')
                {
                    if (pi + 1 < patternLen && pattern[pi + 1] == '\n')
                    {
                        pi += 2;
                        ci += 2;
                        continue;
                    }
                    if (ci + 1 < contentLen && content[ci + 1] == '\n')
                    {
                        pi++;
                        ci += 2;
                        continue;
                    }
                }

                if (pc == '\n')
                {
                    if (cc == '\r')
                    {
                        if (ci + 1 < contentLen && content[ci + 1] == '\n')
                        {
                            ci++;
                        }
                        else if (pi + 1 < patternLen && pattern[pi + 1] == '\n')
                        {
                            pi++;
                            ci++;
                            continue;
                        }
                    }
                    if (cc == '\n' || cc == '\r')
                    {
                        pi++;
                        ci++;
                        continue;
                    }
                }

                if (pc == cc)
                {
                    pi++;
                    ci++;
                    continue;
                }

                break;
            }

            if (pi >= patternLen)
                return new MatchResult { Index = matchStart, Length = ci - matchStart };
        }

        return MatchResult.NotFound;
    }

    /// <summary>
    /// Performs a loose search for the target pattern in the source text starting from a specified index, ignoring all whitespace character differences.
    /// </summary>
    /// <param name="source">The source text</param>
    /// <param name="pattern">The code snippet to find (may contain any whitespace)</param>
    /// <param name="startIndex">The starting search index (inclusive), default is 0. Negative values are treated as 0, returns NotFound if exceeds length</param>
    /// <returns>Match result (contains index and length), returns NotFound if not found</returns>
    public static MatchResult FuzzyMatch(this string source, string pattern, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
            return MatchResult.NotFound;

        // Normalize startIndex
        if (startIndex < 0)
            startIndex = 0;
        if (startIndex >= source.Length)
            return MatchResult.NotFound;

        // Split pattern by consecutive whitespace into non-empty tokens and escape regex special characters
        var tokens = Regex.Split(pattern, @"\s+")
                          .Where(t => !string.IsNullOrEmpty(t))
                          .Select(Regex.Escape)
                          .ToArray();

        if (tokens.Length == 0)
            return MatchResult.NotFound;

        // Join tokens with \s+, which can match any whitespace (across lines, across \r/\n/\r\n)
        string regexPattern = string.Join(@"\s+", tokens);

        // Match starting from startIndex
        var match = Regex.Match(source, regexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        // If match succeeded but position is less than startIndex, continue to find next match
        while (match.Success && match.Index < startIndex)
        {
            match = match.NextMatch();
        }

        if (!match.Success || match.Index < startIndex)
            return MatchResult.NotFound;

        return new MatchResult
        {
            Index = match.Index,
            Length = match.Length
        };
    }

    public static string ReplaceContent(string content, int index, int length, string newContent)
    {
        if (index < 0 || index >= content.Length || index + length > content.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return content.Substring(0, index) + newContent + content.Substring(index + length);
    }
}