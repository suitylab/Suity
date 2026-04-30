namespace Suity.Parser;

public static class StringTrim
{
    // http://msdn.microsoft.com/en-us/library/system.char.iswhitespace(v=vs.110).aspx
    // http://en.wikipedia.org/wiki/Byte_order_mark
    const char BOM_CHAR = '\uFEFF';
    const char MONGOLIAN_VOWEL_SEPARATOR = '\u180E';

    public static string TrimEx(string s)
    {
        return TrimEndEx(TrimStartEx(s));
    }

    public static string TrimStartEx(string s)
    {
        if (s.Length == 0)
            return string.Empty;

        var i = 0;
        while (i < s.Length)
        {
            if (IsWhiteSpaceEx(s[i]))
                i++;
            else
                break;
        }
        if (i >= s.Length)
            return string.Empty;
        else
            return s.Substring(i);
    }
    public static string TrimEndEx(string s)
    {
        if (s.Length == 0)
            return string.Empty;

        var i = s.Length - 1;
        while (i >= 0)
        {
            if (IsWhiteSpaceEx(s[i]))
                i--;
            else
                break;
        }
        if (i >= 0)
            return s.Substring(0, i + 1);
        else
            return string.Empty;
    }
    private static bool IsWhiteSpaceEx(char c)
    {
        return
            char.IsWhiteSpace(c) ||
            c == BOM_CHAR ||
            // In .NET 4.6 this was removed from WS based on Unicode 6.3 changes
            c == MONGOLIAN_VOWEL_SEPARATOR;
    }
}
