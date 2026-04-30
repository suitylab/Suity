using System;

namespace Suity.Helpers;

/// <summary>
/// Utility class for parsing and generating keys with numeric suffixes (e.g., "item001", "file002").
/// Supports extracting the prefix and numeric portion from existing keys, as well as creating new keys
/// with zero-padded numeric values or finding the next available key using a uniqueness predicate.
/// </summary>
public static class KeyIncrementHelper
{
    /// <summary>
    /// Parses a key string into its prefix, digit length, and numeric value components.
    /// For example, "item003" would yield prefix="item", digiLen=3, digiValue=3.
    /// If the key contains no trailing digits, the entire key is returned as the prefix
    /// with digiLen=1 and digiValue=0.
    /// </summary>
    /// <param name="key">The key string to parse (e.g., "item001", "file042", "name").</param>
    /// <param name="prefix">The non-digit prefix portion of the key.</param>
    /// <param name="digiLen">The number of digits in the numeric suffix, including leading zeros.</param>
    /// <param name="digiValue">The numeric value of the trailing digits as an unsigned 64-bit integer.</param>
    public static void ParseKey(string key, out string prefix, out int digiLen, out ulong digiValue)
    {
        string digiCode = GetLastDigiCode(key);
        if (digiCode.Length == 0)
        {
            prefix = key;
            digiLen = 1;
            digiValue = 0;
        }
        else if (digiCode.Length == key.Length)
        {
            prefix = string.Empty;
            digiLen = digiCode.Length;
            digiValue = ulong.Parse(digiCode);
        }
        else
        {
            prefix = key[..^digiCode.Length];
            digiLen = digiCode.Length;
            digiValue = ulong.Parse(digiCode);
        }
    }

    /// <summary>
    /// Generates a unique key by incrementing the numeric suffix starting from 1 until the predicate returns true.
    /// This is useful for finding the next available key that does not conflict with existing keys.
    /// </summary>
    /// <param name="prefix">The non-digit prefix for the key (e.g., "item", "file").</param>
    /// <param name="digilen">The number of digits to use for the numeric suffix, including leading zeros.</param>
    /// <param name="predicate">A function that returns true when a generated key is acceptable (e.g., not already in use).</param>
    /// <returns>The first key for which the predicate returns true.</returns>
    public static string MakeKey(string prefix, int digilen, Predicate<string> predicate)
    {
        for (ulong i = 1; ; i++)
        {
            string key = MakeKey(prefix, digilen, i);
            if (predicate(key))
            {
                return key;
            }
        }
    }

    /// <summary>
    /// Creates a key by combining a prefix with a zero-padded numeric value.
    /// For example, with prefix="item", digiLen=3, and digiValue=5, the result would be "item005".
    /// </summary>
    /// <param name="prefix">The non-digit prefix for the key (e.g., "item", "file").</param>
    /// <param name="digiLen">The number of digits to use for the numeric suffix, including leading zeros.</param>
    /// <param name="digiValue">The numeric value to append, which will be zero-padded to match digiLen.</param>
    /// <returns>A formatted key string with the prefix and zero-padded numeric suffix.</returns>
    public static string MakeKey(string prefix, int digiLen, ulong digiValue)
    {
        string format = "{0}{1:" + new string('0', digiLen) + "}";
        return string.Format(format, prefix, digiValue);
    }

    private static string GetLastDigiCode(string str)
    {
        int index = str.Length - 1;
        while (index >= 0)
        {
            if (!char.IsDigit(str[index]))
            {
                break;
            }
            index--;
        }

        if (index == str.Length - 1)
        {
            return string.Empty;
        }
        else if (index < 0)
        {
            return str;
        }
        else
        {
            return str.Substring(index + 1, str.Length - index - 1);
        }
    }
}