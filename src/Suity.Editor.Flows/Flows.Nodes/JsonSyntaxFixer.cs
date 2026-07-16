using System.Text;

namespace Suity.Editor.Flows.Nodes;

public class JsonSyntaxFixer
{
    /// <summary>
    /// Fixes invalid backslash escapes in a JSON string.
    /// </summary>
    public static string FixInvalidEscapes(string json)
    {
        if (string.IsNullOrEmpty(json)) return json;

        var sb = new StringBuilder(json.Length + 10); // Pre-allocate capacity
        bool inString = false; // State: whether currently inside a JSON string

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (!inString)
            {
                // Outside a string, only look for double quotes
                if (c == '"')
                {
                    inString = true;
                }
                sb.Append(c);
            }
            else
            {
                // Inside a string
                if (c == '\\')
                {
                    // Check the character after the backslash
                    if (i + 1 < json.Length)
                    {
                        char nextChar = json[i + 1];

                        // If it is a valid escape character
                        if (IsValidJsonEscapeChar(nextChar))
                        {
                            // Special handling for \uXXXX format
                            if (nextChar == 'u')
                            {
                                // Simple handling: keep \u, subsequent characters will be handled by the next loop iterations
                                // A rigorous approach would check if it's followed by exactly 4 hex digits
                                sb.Append(c);
                            }
                            else
                            {
                                sb.Append(c);
                                sb.Append(nextChar);
                                i++; // Skip the already processed next character
                            }
                        }
                        else
                        {
                            // [Core Fix]: Invalid escape character (e.g., \a, \c, \x)
                            // Escape \ to \\, making it a valid literal backslash in JSON
                            sb.Append('\\');
                            sb.Append('\\');
                            // Note: do not increment i here, let the next loop iteration handle nextChar
                        }
                    }
                    else
                    {
                        // [Core Fix]: Backslash is the last character of the string (orphaned backslash)
                        sb.Append('\\');
                        sb.Append('\\');
                    }
                }
                else if (c == '"')
                {
                    // Encountered an unescaped double quote, indicating the end of the string
                    inString = false;
                    sb.Append(c);
                }
                else
                {
                    // Normal character
                    sb.Append(c);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether a character is a valid escape character in the JSON standard.
    /// </summary>
    private static bool IsValidJsonEscapeChar(char c)
    {
        // Valid escapes: \" \\ \/ \b \f \n \r \t \uXXXX
        return c == '"' || c == '\\' || c == '/' ||
               c == 'b' || c == 'f' || c == 'n' ||
               c == 'r' || c == 't' || c == 'u';
    }
}