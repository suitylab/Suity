namespace Suity.Helpers;

/// <summary>
/// Extension methods for string operations.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Formats a string with a single argument.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arg0">The argument.</param>
    /// <returns>The formatted string.</returns>
    public static string Fmt(this string format, object arg0)
    {
        return string.Format(format, arg0);
    }

    /// <summary>
    /// Formats a string with multiple arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The formatted string.</returns>
    public static string Fmt(this string format, params object[] args)
    {
        return string.Format(format, args);
    }

    /// <summary>
    /// Removes characters from the beginning of the string.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveFromFirst(this string str, int count)
    {
        return str.Substring(count, str.Length - count);
    }

    /// <summary>
    /// Removes characters from the end of the string.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveFromLast(this string str, int count)
    {
        return str.Substring(0, str.Length - count);
    }

    /// <summary>
    /// Removes a prefix from the string if it starts with the specified value.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="remove">The prefix to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveFromFirst(this string str, string remove)
    {
        if (str.StartsWith(remove))
        {
            return str.Substring(remove.Length, str.Length - remove.Length);
        }
        else
        {
            return str;
        }
    }

    /// <summary>
    /// Removes a suffix from the string if it ends with the specified value.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="remove">The suffix to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveFromLast(this string str, string remove)
    {
        if (str.EndsWith(remove))
        {
            return str.Substring(0, str.Length - remove.Length);
        }
        else
        {
            return str;
        }
    }

    /// <summary>
    /// Limits the string to a maximum length.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="count">The maximum length.</param>
    /// <returns>The truncated string, or null if input is null.</returns>
    public static string Limit(this string str, int count)
    {
        if (str is null)
        {
            return null;
        }

        if (str.Length > count)
        {
            return str.Substring(0, count);
        }

        return str;
    }

    /// <summary>
    /// Extracts the part before the first occurrence of a character.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="splitter">The character to split on.</param>
    /// <returns>The extracted part, or null if not found.</returns>
    public static string ExtractFirstSplit(this string str, char splitter)
    {
        int index = str.IndexOf(splitter);
        if (index >= 0)
        {
            return str.Substring(0, index);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the part after the last occurrence of a character.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="splitter">The character to split on.</param>
    /// <returns>The extracted part, or null if not found.</returns>
    public static string ExtractLastSplit(this string str, char splitter)
    {
        int index = str.LastIndexOf(splitter);
        if (index >= 0)
        {
            return str.Substring(index + 1, str.Length - index - 1);
        }
        else
        {
            return null;
        }
    }
}