using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ComputerBeacon.Json;

/// <summary>
/// A class for parsing JSON syntax strings
/// </summary>
public static class Parser
{
    /// <summary>
    /// Parse a JSON string into a JsonObject or JsonArray instance
    /// </summary>
    /// <param name="s">JSON string</param>
    /// <returns>a JsonObject or JsonArray instance, depending on the input string</returns>
    /// <exception cref="FormatException">The string contains invalid JSON syntax.</exception>
    public static object Parse(string s)
    {
        Stack<IJsonContainer> stack = new Stack<IJsonContainer>();
        object root = null;

        StringBuilder sb = new StringBuilder();
        string key = null;
        bool aftercomma = false;

        short state = 0;
        int length = s.Length;
        char c;
        uint hexvalue;
        int i = 0;

        int strStart = -1;
        int strLength = 0;
        do
        {
            c = s[i];

            switch (state)
            {
                #region ReadChar

                case 4:
                    switch (c)
                    {
                        case '"':
                            if (strLength > 0)
                            {
                                sb.Append(s, strStart, strLength);
                                strLength = 0;
                            }
                            strStart = -1;
                            if (!stack.Peek().IsArray && key == null)
                            {
                                if (sb.Length == 0) throw MakeException(s, i, "Key in JSON object cannot be empty string");
                                state = 7;
                            }
                            else
                            {
                                stack.Peek().InternalAdd(key, sb.ToString());
                                key = null;
                                sb.Length = 0;
                                state = 8;
                            }
                            continue;
                        case '\\':
                            if (strLength > 0)
                            {
                                sb.Append(s, strStart, strLength);
                                strLength = 0;
                            }
                            strStart = -1;
                            state = 5;
                            continue;
                        default:
                            ++strLength;
                            continue;
                    }

                #endregion

                #region WaitingValue

                case 1:
                    if (c == ' ' || c == '\n' || c == '\r' || c == '\t') continue;
                    if (c == '"')
                    {
                        strStart = i + 1;
                        state = 4; continue;
                    }
                    if (c == '{')
                    {
                        aftercomma = false;
                        var jo = new JsonObject();
                        stack.Peek().InternalAdd(key, jo);
                        stack.Push(jo);
                        key = null;

                        state = 3; continue;
                    }
                    if (c == '[')
                    {
                        aftercomma = false;
                        var ja = new JsonArray();
                        stack.Peek().InternalAdd(key, ja);
                        stack.Push(ja);
                        key = null;

                        continue;
                    }
                    if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '-')
                    {
                        sb.Append(c);
                        state = 2; continue;
                    }
                    if (!aftercomma && c == ']')
                    {
                        if (!stack.Peek().IsArray) throw MakeException(s, i, "Invalid ']' character");
                        stack.Pop();
                        if (stack.Count > 0)
                        {
                            state = 8; continue;
                        }
                        state = 9; continue;
                    }
                    throw MakeException(s, i, "Unknown value expression.");

                #endregion

                #region ReadValue

                case 2:
                    if (c == ' ' || c == '\n' || c == '\r' || c == '\t') continue;
                    if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.' || c == '+')
                    {
                        sb.Append(c);
                        continue;
                    }
                    if (c == ',')
                    {
                        aftercomma = true;
                        stack.Peek().InternalAdd(key, ParseJsonValue(sb.ToString()));
                        key = null;
                        sb.Length = 0;
                        if (stack.Peek().IsArray)
                        {
                            state = 1; continue;
                        }
                        else
                        {
                            state = 3; continue;
                        }
                    }
                    if (c == ']')
                    {
                        if (!stack.Peek().IsArray) throw MakeException(s, i, "Invalid ']' character");
                        stack.Peek().InternalAdd(null, ParseJsonValue(sb.ToString()));
                        stack.Pop();
                        sb.Length = 0;
                        if (stack.Count > 0)
                        {
                            state = 8; continue;
                        }
                        state = 9; continue;
                    }
                    if (c == '}')
                    {
                        if (stack.Peek().IsArray) throw MakeException(s, i, "Invalid '}' character");
                        stack.Peek().InternalAdd(key, ParseJsonValue(sb.ToString()));
                        stack.Pop();
                        key = null;
                        sb.Length = 0;
                        if (stack.Count > 0)
                        {
                            state = 8; continue;
                        }
                        state = 9; continue;
                    }
                    throw MakeException(s, i, "Invalid character in non-string value");

                #endregion

                #region WaitBeginString

                case 3:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            continue;
                        case '"':
                            strStart = i + 1;
                            state = 4;
                            continue;
                        case '}':
                            if (aftercomma) goto default;
                            stack.Pop(); //waitbeginstring can only be entered by '{', therefore pop must be valid
                            if (stack.Count == 0) state = 9;
                            else state = 8;
                            continue;
                        default:
                            throw MakeException(s, i, "Expected double quotation character to mark beginning of string");
                    }

                #endregion

                #region ReadEscapedChar

                case 5:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t': continue;
                        case '\\':
                            sb.Append('\\');
                            strStart = i + 1;
                            state = 4; continue;
                        case '/':
                            sb.Append('/');
                            strStart = i + 1;
                            state = 4; continue;
                        case '"':
                            sb.Append('"');
                            strStart = i + 1;
                            state = 4; continue;
                        case 'n':
                            sb.Append('\n');
                            strStart = i + 1;
                            state = 4; continue;
                        case 'r':
                            sb.Append('\r');
                            strStart = i + 1;
                            state = 4; continue;
                        case 't':
                            sb.Append('\t');
                            strStart = i + 1;
                            state = 4; continue;
                        case 'u':
                            if (i + 4 >= length) throw new FormatException("Incomplete JSON string");
                            hexvalue = (CharToHex(s[i + 1]) << 12) | (CharToHex(s[i + 2]) << 8) | (CharToHex(s[i + 3]) << 4) | CharToHex(s[i + 4]);
                            sb.Append((char)hexvalue);
                            i += 4;
                            strStart = i + 1;
                            state = 4;
                            continue;
                        default:
                            throw MakeException(s, i, "Unknown escaped character");
                    }

                #endregion

                #region WaitColon

                case 7:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t': continue;
                        case ':':
                            key = sb.ToString();
                            sb.Length = 0;
                            state = 1;
                            continue;
                        default:
                            throw MakeException(s, i, "Expected colon(:) to seperate key and values in JSON object");
                    }

                #endregion

                #region WaitClose

                case 8:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            continue;
                        case ',':
                            aftercomma = true;
                            state = 1;
                            continue;
                        case ']':
                            if (!stack.Peek().IsArray) throw MakeException(s, i, "Invalid ']' character");
                            stack.Pop();
                            if (stack.Count == 0) state = 9;
                            continue;
                        case '}':
                            if (stack.Peek().IsArray) throw MakeException(s, i, "Invalid '}' character");
                            stack.Pop();
                            if (stack.Count == 0) state = 9;
                            continue;
                        default:
                            throw MakeException(s, i, "Expect comma or close bracket after value");
                    }

                #endregion

                #region Start

                case 0:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            continue;
                        case '[':
                            var ja = new JsonArray();
                            root = ja;
                            stack.Push(ja);
                            state = 1; continue;
                        case '{':
                            var jo = new JsonObject();
                            root = jo;
                            stack.Push(jo);
                            state = 3; continue;
                        default:
                            throw MakeException(s, i, "Expect '{' or '[' to begin JSON string");
                    }

                #endregion

                #region End

                case 9:
                    switch (c)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            continue;
                        default:
                            throw MakeException(s, i, "Unexpected character(s) after termination of JSON string");
                    }

                    #endregion
            }
        } while (++i < length);
        if (state != 9) throw new FormatException("Incomplete JSON string");

        return root;
    }

    private static uint CharToHex(char c)
    {
        if (c >= '0' && c <= '9') return (uint)(c - '0');
        if (c >= 'a' && c <= 'f') return (uint)(c - 'a' + 10);
        if (c >= 'A' && c <= 'F') return (uint)(c - 'A' + 10);
        throw new FormatException(c + " is not a valid hex value");
    }

    private static object ParseJsonValue(string jsonString)
    {
#if BRIDGE
        int result;
        if (int.TryParse(jsonString, out result)) return result;

        long result_long;
        if (long.TryParse(jsonString, out result_long)) return result_long;

        double result_double;
        if (double.TryParse(jsonString,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out result_double)) return result_double;

        if (jsonString == "true" || jsonString == "True") return true;
        if (jsonString == "false" || jsonString == "False") return false;
        if (jsonString == "null") return null;

        throw new FormatException(string.Format("Unknown JSON value: \"{0}\"", jsonString));
#else
        int result;
        if (int.TryParse(jsonString, NumberStyles.AllowLeadingSign, System.Globalization.NumberFormatInfo.InvariantInfo, out result)) return result;

        long result_long;
        if (long.TryParse(jsonString, NumberStyles.AllowLeadingSign, System.Globalization.NumberFormatInfo.InvariantInfo, out result_long)) return result_long;

        double result_double;
        if (double.TryParse(jsonString, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out result_double)) return result_double;

        if (jsonString == "true" || jsonString == "True") return true;
        if (jsonString == "false" || jsonString == "False") return false;
        if (jsonString == "null") return null;

        throw new FormatException(string.Format("Unknown JSON value: \"{0}\"", jsonString));
#endif
    }

    private static FormatException MakeException(string errorString, int position, string message)
    {
        int start = position - 5;
        if (start < 0) start = 0;
        int length = errorString.Length - position;
        if (length > 5) length = 5;
        length += 5;
        StringBuilder sb = new StringBuilder(message);
        sb.Append(" at character position " + position + ", near ");
        Stringifier.writeEscapedString(sb, errorString.Substring(start, length));
        return new FormatException(sb.ToString());
    }
}