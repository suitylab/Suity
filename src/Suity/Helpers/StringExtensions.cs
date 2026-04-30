using System;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides extension methods for string manipulation.
/// </summary>
public static class StringExtensions
{
    [Obsolete]
    public static string Fmt(this string format, object arg0)
    {
        return string.Format(format, arg0);
    }

    [Obsolete]
    public static string Fmt(this string format, params object[] args)
    {
        return string.Format(format, args);
    }

    public static string RemoveFromFirst(this string str, int count)
    {
        return str.Substring(count, str.Length - count);
    }

    public static string RemoveFromLast(this string str, int count)
    {
        return str.Substring(0, str.Length - count);
    }

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

    public static string RemoveFromFirst(this string str, string remove, StringComparison comparison)
    {
        if (str.StartsWith(remove, comparison))
        {
            return str.Substring(remove.Length, str.Length - remove.Length);
        }
        else
        {
            return str;
        }
    }

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

    public static string RemoveFromLast(this string str, string remove, StringComparison comparison)
    {
        if (str.EndsWith(remove, comparison))
        {
            return str.Substring(0, str.Length - remove.Length);
        }
        else
        {
            return str;
        }
    }

    public static string Limit(this string str, int count)
    {
        if (str == null)
        {
            return null;
        }

        if (str.Length > count)
        {
            return str.Substring(0, count);
        }

        return str;
    }

    [Obsolete]
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

    [Obsolete]
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

    public static string FindAndGetBefore(this string str, char value, bool returnOrigin = false)
    {
        int index = str.IndexOf(value);
        if (index >= 0)
        {
            return str.Substring(0, index);
        }

        return returnOrigin ? str : null;
    }

    public static string FindAndGetAfter(this string str, char value, bool returnOrigin = false)
    {
        int index = str.IndexOf(value);
        if (index >= 0)
        {
            return str.Substring(++index);
        }

        return returnOrigin ? str : null;
    }

    public static string FindLastAndGetBefore(this string str, char value, bool returnOrigin = false)
    {
        int index = str.LastIndexOf(value);
        if (index >= 0)
        {
            return str.Substring(0, index);
        }

        return returnOrigin ? str : null;
    }

    public static string FindLastAndGetAfter(this string str, char value, bool returnOrigin = false)
    {
        int index = str.LastIndexOf(value);
        if (index >= 0)
        {
            return str.Substring(++index);
        }

        return returnOrigin ? str : null;
    }

    [ThreadStatic]
    static StringBuilder _builder;
    public static string Escape(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        _builder ??= new StringBuilder();

        _builder.Length = 0;

        _builder.Append(s);
        _builder.Replace("\"", "\\\"")
                 .Replace("\'", "\\\'")
                 .Replace("\\", "\\\\")
                 .Replace("\n", "\\n")
                 .Replace("\r", "\\r")
                 .Replace("\t", "\\t")
                 .Replace("\b", "\\b")
                 .Replace("\f", "\\f");

        s = _builder.ToString();
        _builder.Length = 0;

        return s;
    }

    public static string Unescape(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        _builder ??= new StringBuilder();

        _builder.Length = 0;

        _builder.Append(s);
        _builder.Replace("\\\"", "\"")
                 .Replace("\\\'", "\'")
                 .Replace("\\\\", "\\")
                 .Replace("\\n", "\n")
                 .Replace("\\r", "\r")
                 .Replace("\\t", "\t")
                 .Replace("\\b", "\b")
                 .Replace("\\f", "\f");

        s = _builder.ToString();
        _builder.Length = 0;

        return s;
    }
}