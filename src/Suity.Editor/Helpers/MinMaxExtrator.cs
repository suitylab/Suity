using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Suity.Editor.AIGC.Helpers;

/// <summary>
/// Utility class for extracting minimum and maximum numeric values from text patterns.
/// Supports formats like [min..max] in bracketed tags or raw content strings.
/// </summary>
public class MinMaxExtractor
{
    /// <summary>
    /// Attempts to extract a min/max value pair from the first occurrence of a bracketed pattern like [min..max] in the input string.
    /// </summary>
    /// <param name="input">The input string that may contain one or more bracketed expressions.</param>
    /// <param name="min">When this method returns, contains the extracted minimum value if successful; otherwise, zero.</param>
    /// <param name="max">When this method returns, contains the extracted maximum value if successful; otherwise, zero.</param>
    /// <returns>True if a valid min/max pair is found and parsed; otherwise, false.</returns>
    public static bool TryExtractMinMaxTag(string input, out decimal min, out decimal max)
    {
        min = max = 0;

        // Step 1: Match all bracket contents (non greedy mode)
        var bracketMatches = Regex.Matches(input, @"\[(.*?)\]");
        foreach (Match bracketMatch in bracketMatches)
        {
            // Step 2: Remove the parentheses and divide the two parts
            string content = bracketMatch.Groups[1].Value;
            string[] parts = content.Split([".."], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) continue;

            // Step 3: Clean up and attempt to parse numerical values
            if (TryParseNumber(parts[0].Trim(), out decimal candidateMin) &&
                TryParseNumber(parts[1].Trim(), out decimal candidateMax))
            {
                min = candidateMin;
                max = candidateMax;
                return true; // Step 4: Return the first valid pair
            }
        }
        return false;
    }

    /// <summary>
    /// Attempts to extract a min/max value pair directly from a content string in the format "min..max" without requiring brackets.
    /// </summary>
    /// <param name="content">The content string expected to contain two numeric values separated by "..".</param>
    /// <param name="min">When this method returns, contains the extracted minimum value if successful; otherwise, zero.</param>
    /// <param name="max">When this method returns, contains the extracted maximum value if successful; otherwise, zero.</param>
    /// <returns>True if the content contains a valid min/max pair separated by ".."; otherwise, false.</returns>
    public static bool TryExtractMinMax(string content, out decimal min, out decimal max)
    {
        min = max = 0;

        string[] parts = content.Split([".."], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2) return false;

        // Step 3: Clean up and attempt to parse numerical values
        if (TryParseNumber(parts[0].Trim(), out decimal candidateMin) &&
            TryParseNumber(parts[1].Trim(), out decimal candidateMax))
        {
            min = candidateMin;
            max = candidateMax;
            return true; // Step 4: Return the first valid pair
        }

        return false;
    }

    private static bool TryParseNumber(string input, out decimal number)
    {
        // Support cleaning logic for symbols, commas, and decimal points
        string cleanNumber = input
            .Replace(",", "")     // Remove the thousandth comma
            .Replace(" ", "");    // Remove possible middle spaces (such as "-123")

        return decimal.TryParse(
            cleanNumber,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out number
        );
    }
}