using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Suity.Helpers;

/// <summary>
/// Naming Varifier
/// </summary>
public static class NamingVerifier
{
    private const string FirstCharString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
    private const string NumberCharString = "1234567890";
    private const string FileExceptCharString = "/\\:*?\"<>|";

    private static readonly HashSet<char> FirstChar = [];
    private static readonly HashSet<char> NumberChar = [];
    private static readonly HashSet<char> FileExceptChar = [];

    // Valid XML attribute name regex (supports namespace prefix)
    private static readonly Regex XmlNameRegex = new Regex(
        @"^[A-Za-z_:][A-Za-z0-9_:\.\-]*$",
        RegexOptions.Compiled
    );

    // More strict regex (disallows colon, recommended for regular attributes)
    private static readonly Regex XmlNCNameRegex = new Regex(
        @"^[A-Za-z_][A-Za-z0-9_\.\-]*$",
        RegexOptions.Compiled
    );

    static NamingVerifier()
    {
        foreach (char c in FirstCharString)
        {
            FirstChar.Add(c);
        }

        foreach (char c in NumberCharString)
        {
            NumberChar.Add(c);
        }

        foreach (char c in FileExceptCharString)
        {
            FileExceptChar.Add(c);
        }
    }

    /// <summary>
    /// Verify identifier
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool VerifyIdentifier(string str)
    {
        // Determine whether the string is empty or blank
        if (string.IsNullOrWhiteSpace(str)) return false;

        // Traverse every character of the string
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            // If it is the first character
            if (i == 0)
            {
                // Determine if the first character is in 'FirstChar'
                if (!FirstChar.Contains(c))
                {
                    // If not available, return false
                    return false;
                }
            }
            // If it is after the first character
            else
            {
                // Determine whether the character is in 'FirstChar' and 'NumberChar'
                if (!FirstChar.Contains(c) && !NumberChar.Contains(c))
                {
                    // If not available, return false
                    return false;
                }
            }
        }
        // If all characters meet the conditions, return true
        return true;
    }

    /// <summary>
    /// Verify file name
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool VerifyFileName(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return false;
        if (str.StartsWith(".")) return false;
        if (str.EndsWith(".")) return false;

        return str.All(c => !FileExceptChar.Contains(c));
    }

    /// <summary>
    /// Validate namespace
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool VerifyNameSpace(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return false;
        if (str.StartsWith(".")) return false;
        if (str.EndsWith(".")) return false;
        if (str.Contains("..")) return false;

        return str.All(c => FirstChar.Contains(c) || NumberChar.Contains(c) || c == '.');
    }

    /// <summary>
    /// Check if a string is a valid XML tag name
    /// </summary>
    /// <param name="tagName">The tag name to check</param>
    /// <returns>Returns true if valid, otherwise false</returns>
    public static bool VerifyXmlTagName(string tagName)
    {
        if (string.IsNullOrEmpty(tagName))
            return false;

        // Cannot start with "xml" (case-insensitive)
        if (tagName.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
            return false;

        // Basic format validation
        if (!XmlNameRegex.IsMatch(tagName))
            return false;

        // Cannot contain spaces
        if (tagName.Contains(" "))
            return false;

        // Cannot start with hyphen or dot (though regex already restricts, explicit check is clearer)
        if (tagName[0] == '-' || tagName[0] == '.')
            return false;

        // Cannot start with colon (XML spec allows it, but generally not recommended)
        // Uncomment the line below for strict common practice compliance
        // if (tagName[0] == ':') return false;

        return true;
    }

    /// <summary>
    /// Check if a string is a valid XML attribute name
    /// </summary>
    /// <param name="attrName">The attribute name to check</param>
    /// <param name="allowNamespace">Whether to allow namespace prefix (colon)</param>
    /// <returns>Returns true if valid, otherwise false</returns>
    public static bool VerifyXmlAttributeName(string attrName, bool allowNamespace = false)
    {
        if (string.IsNullOrEmpty(attrName))
            return false;

        // Cannot start with "xml" (case-insensitive)
        if (attrName.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot contain spaces
        if (attrName.Contains(" "))
            return false;

        // Cannot contain XML special characters
        if (attrName.IndexOfAny(new[] { '<', '>', '"', '\'', '=', '/', '?', '[', ']', '(', ')', '{', '}' }) >= 0)
            return false;

        // Choose regex based on namespace allowance
        Regex regex = allowNamespace ? XmlNameRegex : XmlNCNameRegex;
        if (!regex.IsMatch(attrName))
            return false;

        // Colon cannot appear at start or end, and cannot appear consecutively
        if (attrName.StartsWith(":") || attrName.EndsWith(":") || attrName.Contains("::"))
            return false;

        return true;
    }
}