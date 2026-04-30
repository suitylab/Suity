using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Helpers;

public static class TextHelper
{
    /// <summary>
    /// Searches for all occurrences of text in the given content and returns their locations.
    /// </summary>
    /// <param name="fullText">The complete text to search within.</param>
    /// <param name="searchText">The text pattern to search for.</param>
    /// <param name="option">Search options to control matching behavior.</param>
    /// <returns>An array of TextSearchResult containing all match positions with line content; returns empty array if no matches found.</returns>
    public static TextSearchResult[] SearchText(string fullText, string searchText, SearchOption option)
    {
        // Parameter validation
        if (string.IsNullOrEmpty(fullText) || string.IsNullOrEmpty(searchText))
            return Array.Empty<TextSearchResult>();

        // Configure string comparison mode
        StringComparison comparison = (option & SearchOption.MatchCase) == SearchOption.MatchCase
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        // Split text into lines, supporting multiple newline formats
        string[] lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var results = new List<TextSearchResult>();

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string currentLine = lines[lineIndex];
            int searchStart = 0;

            // Search for all matches in the current line
            while (searchStart <= currentLine.Length - searchText.Length)
            {
                int matchIndex = currentLine.IndexOf(searchText, searchStart, comparison);

                if (matchIndex < 0)
                    break; // No more matches in this line

                // If whole word matching is enabled, validate word boundaries
                if ((option & SearchOption.MatchWholeWord) == SearchOption.MatchWholeWord)
                {
                    bool isWordStart = matchIndex == 0 || !char.IsLetterOrDigit(currentLine[matchIndex - 1]);
                    bool isWordEnd = matchIndex + searchText.Length == currentLine.Length ||
                                     !char.IsLetterOrDigit(currentLine[matchIndex + searchText.Length]);

                    if (isWordStart && isWordEnd)
                    {
                        results.Add(new TextSearchResult(lineIndex + 1, matchIndex, searchText.Length, currentLine));
                    }
                }
                else
                {
                    // No whole word constraint, add the match directly with line content
                    results.Add(new TextSearchResult(lineIndex + 1, matchIndex, searchText.Length, currentLine));
                }

                // Continue searching from the next character position
                searchStart = matchIndex + 1;
            }
        }

        return results.ToArray();
    }
}