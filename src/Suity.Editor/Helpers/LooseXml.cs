using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Suity.Editor.AIGC.Helpers;

/// <summary>
/// Represents a loose XML tag with a tag name, attributes, and inner text.
/// Used for parsing informal or incomplete XML-like structures in text content.
/// </summary>
[NativeType("XmlTag", CodeBase = "AIGC", Description = "Xml Tag", Icon = "*CoreIcon|Tag")]
public class LooseXmlTag
{
    /// <summary>
    /// Gets or sets the name of the XML tag.
    /// </summary>
    public string TagName { get; set; }

    /// <summary>
    /// Gets the dictionary of attribute key-value pairs for this tag.
    /// </summary>
    public Dictionary<string, string> Attributes { get; init; } = [];

    /// <summary>
    /// Gets or sets the inner text content of the tag.
    /// </summary>
    public string InnerText { get; set; }

    /// <summary>
    /// Retrieves the value of a specified attribute.
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve.</param>
    /// <returns>The attribute value, or null if the attribute does not exist.</returns>
    public string GetAttribute(string name) => Attributes.TryGetValue(name, out var value) ? value : null;

    /// <summary>
    /// Sets or removes an attribute on this tag.
    /// </summary>
    /// <param name="name">The name of the attribute to set.</param>
    /// <param name="value">The value to assign. If null or whitespace, the attribute is removed.</param>
    public void SetAttribute(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            Attributes[name] = value;
        }
        else
        {
            Attributes.Remove(name);
        }
    }

    /// <summary>
    /// Retrieves an attribute value parsed as an integer.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="defaultValue">The value to return if parsing fails or the attribute is missing.</param>
    /// <returns>The parsed integer value, or the default value.</returns>
    public int GetIntAttribute(string name, int defaultValue = 0)
        => int.TryParse(GetAttribute(name), out var result) ? result : defaultValue;

    /// <summary>
    /// Retrieves an attribute value parsed as a float.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="defaultValue">The value to return if parsing fails or the attribute is missing.</param>
    /// <returns>The parsed float value, or the default value.</returns>
    public float GetFloatAttribute(string name, float defaultValue = 0)
        => float.TryParse(GetAttribute(name), out var result) ? result : defaultValue;

    /// <summary>
    /// Retrieves an attribute value as a boolean.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <returns>True if the attribute value equals "true" (case-insensitive); otherwise, false.</returns>
    public bool GetBoolAttribute(string name)
        => string.Equals(GetAttribute(name), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a string representation of this tag in XML format.
    /// If the tag name is empty, defaults to "section".
    /// </summary>
    /// <returns>A formatted XML string including the tag name, attributes, and inner text.</returns>
    public override string ToString()
    {
        string tagName = TagName;
        if (string.IsNullOrWhiteSpace(tagName))
        {
            tagName = "section";
        }

        return $"<{tagName} {string.Join(" ", Attributes.Select(kv => $"{kv.Key}=\"{kv.Value}\""))}>\n{InnerText}\n</{tagName}>";
    }
}

/// <summary>
/// Utility class for extracting and removing loose XML-like tags from text content.
/// Supports parsing tags with attributes and retrieving inner text without requiring well-formed XML.
/// </summary>
public class LooseXml
{
    #region Extract

    /// <summary>
    /// Extracts all nodes matching the specified tag name from the content.
    /// </summary>
    /// <param name="content">The text content to search within.</param>
    /// <param name="tagName">The tag name to match.</param>
    /// <returns>An array of <see cref="LooseXmlTag"/> objects representing the matched nodes.</returns>
    public static LooseXmlTag[] ExtractNodes(string content, string tagName)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        // Phase 1: Extract complete tag blocks
        var blocks = ExtractTagBlocks(content, tagName);

        // Phase 2: Analyze each tag block
        return blocks.Select(b => ParseTagBlock(b, tagName)).Where(t => t != null).ToArray();
    }

    /// <summary>
    /// Extracts the inner text of the first node matching the specified tag name.
    /// </summary>
    /// <param name="content">The text content to search within.</param>
    /// <param name="nodeName">The tag name to match.</param>
    /// <returns>The inner text of the first matching node, or an empty string if no match is found.</returns>
    public static string ExtractInnerText(string content, string nodeName)
    {
        var blocks = ExtractNodes(content, nodeName);
        return blocks.Length > 0 ? blocks[0].InnerText ?? string.Empty : string.Empty;
    }

    private static List<string> ExtractTagBlocks(string content, string nodeName)
    {
        var pattern = $@"<\s*{Regex.Escape(nodeName)}[^>]*>.*?</\s*{Regex.Escape(nodeName)}\s*>";
        return Regex.Matches(content, pattern, RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();
    }

    private static LooseXmlTag ParseTagBlock(string block, string tagName = null)
    {
        var pattern = @"^<[^\s>]+(?:\s+(?<attrs>[^>]*))?>(?<content>.*?)</[^\s>]+>$";
        var match = Regex.Match(block, pattern, RegexOptions.Singleline);

        if (!match.Success) return null;

        return new LooseXmlTag
        {
            TagName = tagName,
            Attributes = ParseAttributes(match.Groups["attrs"].Value),
            InnerText = match.Groups["content"].Value.Trim()
        };
    }

    private static Dictionary<string, string> ParseAttributes(string attrStr)
    {
        var attrs = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(attrStr)) return attrs;

        // The updated regular expression only supports attribute values enclosed in quotation marks
        var attrRegex = new Regex(@"(\w+)=(""(?<dval>[^""]*)""|'(?<sval>[^']*)')");

        foreach (Match m in attrRegex.Matches(attrStr))
        {
            var key = m.Groups[1].Value;
            var value = m.Groups["dval"].Success ? m.Groups["dval"].Value
                       : m.Groups["sval"].Value;

            attrs[key] = value;
        }

        return attrs;
    }
    #endregion

    #region Remove

    /// <summary>
    /// Removes the first occurrence of a tag matching the specified tag name from the text.
    /// </summary>
    /// <param name="text">The text content to modify.</param>
    /// <param name="tagName">The tag name to match and remove.</param>
    /// <returns>The text with the first matching tag removed.</returns>
    public static string RemoveFirstTag(string text, string tagName)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(tagName))
            return text;

        // Build matching pattern, e.g. <!--...-->, use non-greedy mode to avoid matching across multiple tags
        string pattern = $@"<\s*{tagName}[^>]*?>.*?</\s*{tagName}\s*>";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return regex.Replace(text, "", 1); // Replace the first match
    }

    #endregion
}