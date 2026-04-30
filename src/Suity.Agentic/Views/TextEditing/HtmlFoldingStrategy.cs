using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using HtmlAgilityPack;

namespace Suity.Editor.Views.TextEditing;

/// <summary>
/// Holds information about the start of a fold in an html string.
/// </summary>
internal sealed class HtmlFoldStart : NewFolding
{
    internal int StartLine;
    internal string ElementName;
}

/// <summary>
/// Determines folds for an html string in the editor.
/// </summary>
public class HtmlFoldingStrategy
{
    /// <summary>
    /// Flag indicating whether attributes should be displayed on folded
    /// elements.
    /// </summary>
    public bool ShowAttributesWhenFolded { get; set; }

    /// <summary>
    /// Create <see cref="NewFolding"/>s for the specified document and updates the folding manager with them.
    /// </summary>
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        var foldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(foldings, firstErrorOffset);
    }

    /// <summary>
    /// Create <see cref="NewFolding"/>s for the specified document.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        try
        {
            var html = document.Text;
            var htmlDoc = new HtmlDocument();
            // Enable tolerant parsing for HTML
            htmlDoc.OptionFixNestedTags = true;
            //htmlDoc.OptionReadComment = true;
            htmlDoc.LoadHtml(html);

            return CreateNewFoldings(document, htmlDoc, out firstErrorOffset);
        }
        catch (Exception)
        {
            firstErrorOffset = 0;
            return Enumerable.Empty<NewFolding>();
        }
    }

    /// <summary>
    /// Create <see cref="NewFolding"/>s for the specified document.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, HtmlDocument htmlDoc, out int firstErrorOffset)
    {
        var foldMarkers = new List<NewFolding>();
        var stack = new Stack<HtmlFoldStart>();

        try
        {
            ProcessNode(document, htmlDoc.DocumentNode, foldMarkers, stack);
            firstErrorOffset = -1;
        }
        catch (Exception)
        {
            firstErrorOffset = 0;
        }

        // Sort foldings by start offset to ensure proper nesting order
        foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return foldMarkers;
    }

    private void ProcessNode(TextDocument document, HtmlNode node, List<NewFolding> foldMarkers, Stack<HtmlFoldStart> stack)
    {
        switch (node.NodeType)
        {
            case HtmlNodeType.Element:
                // Handle element start tag (skip void/self-closing elements)
                if (!IsVoidElement(node.Name))
                {
                    var foldStart = CreateElementFoldStart(document, node);
                    if (foldStart != null)
                    {
                        stack.Push(foldStart);
                    }
                }
                break;

            case HtmlNodeType.Comment:
                CreateCommentFold(document, foldMarkers, node);
                break;

                // HtmlNodeType.Text and HtmlNodeType.Document don't create folds
        }

        // Recursively process child nodes
        if (node.HasChildNodes)
        {
            foreach (var child in node.ChildNodes)
            {
                ProcessNode(document, child, foldMarkers, stack);
            }
        }

        // Handle element end tag - match with start tag from stack
        if (node.NodeType == HtmlNodeType.Element && !IsVoidElement(node.Name))
        {
            if (stack.Count > 0)
            {
                var foldStart = stack.Pop();
                // Match element names (case-insensitive for HTML)
                if (foldStart.ElementName.Equals(node.Name, StringComparison.OrdinalIgnoreCase))
                {
                    CreateElementFold(document, foldMarkers, node, foldStart);
                }
            }
        }
    }

    private HtmlFoldStart CreateElementFoldStart(TextDocument document, HtmlNode node)
    {
        var line = node.Line;
        var position = node.LinePosition;

        // Validate line information
        if (line <= 0 || line > document.LineCount)
            return null;

        // Adjust position to point to the '<' character of start tag
        var offset = document.GetOffset(line, Math.Max(1, position - 1));

        var foldStart = new HtmlFoldStart
        {
            StartLine = line,
            StartOffset = offset,
            ElementName = node.Name
        };

        // Build fold display name
        if (ShowAttributesWhenFolded && node.HasAttributes)
        {
            foldStart.Name = string.Concat("<", node.Name, " ", GetAttributeFoldText(node), ">");
        }
        else
        {
            foldStart.Name = string.Concat("<", node.Name, ">");
        }

        return foldStart;
    }

    private void CreateElementFold(TextDocument document, List<NewFolding> foldMarkers, HtmlNode node, HtmlFoldStart foldStart)
    {
        var endNode = node.EndNode;
        if (endNode == null)
            return; // Element has no explicit end tag

        var endLine = endNode.Line;
        var endPosition = endNode.LinePosition;

        // Only create fold if start and end tags are on different lines
        if (endLine > foldStart.StartLine)
        {
            // Calculate end offset: position after "</tagName>"
            var endLineObj = document.GetLineByNumber(endLine);
            var endOffset = document.GetOffset(endLine,
                Math.Min(endPosition + node.Name.Length + 2, endLineObj.Length + 1));

            foldStart.EndOffset = endOffset;
            foldMarkers.Add(foldStart);
        }
    }

    /// <summary>
    /// Creates a comment fold if the comment spans more than one line.
    /// </summary>
    private static void CreateCommentFold(TextDocument document, List<NewFolding> foldMarkers, HtmlNode node)
    {
        var comment = node.InnerHtml;
        if (string.IsNullOrEmpty(comment))
            return;

        // Check if comment spans multiple lines
        var firstNewLine = comment.IndexOf('\n');
        if (firstNewLine < 0)
            return; // Single line comment, no fold needed

        var startLine = node.Line;
        var endLine = node.EndNode?.Line ?? startLine;

        // Validate line information
        if (startLine <= 0 || startLine > document.LineCount || endLine > document.LineCount)
            return;

        // Calculate start offset (account for "<!--" prefix)
        var startOffset = document.GetOffset(startLine, Math.Max(1, node.LinePosition - 4));

        // Calculate end offset (account for "-->" suffix)
        var endLineObj = document.GetLineByNumber(endLine);
        var endOffset = document.GetOffset(endLine, Math.Min(
            (node.EndNode?.LinePosition ?? node.LinePosition) + 3,
            endLineObj.Length + 1));

        // Build fold display text (first line of comment)
        var foldText = string.Concat("<!--", comment.Substring(0, firstNewLine).TrimEnd('\r'), "-->");
        foldMarkers.Add(new NewFolding(startOffset, endOffset) { Name = foldText });
    }

    /// <summary>
    /// Gets the element's attributes as a string for display when folded.
    /// </summary>
    private static string GetAttributeFoldText(HtmlNode node)
    {
        var text = new StringBuilder();

        foreach (var attr in node.Attributes)
        {
            // Skip internal attributes used by HtmlAgilityPack
            if (attr.Name.StartsWith(":"))
                continue;

            text.Append(attr.Name);
            text.Append("=\"");
            text.Append(HtmlEncodeAttributeValue(attr.Value, '"'));
            text.Append('"');
            text.Append(' ');
        }

        // Remove trailing space if any attributes were added
        if (text.Length > 0)
            text.Length--;

        return text.ToString();
    }

    /// <summary>
    /// HTML-encode the attribute value for safe display.
    /// </summary>
    private static string HtmlEncodeAttributeValue(string value, char quoteChar)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace(quoteChar == '"' ? "\"" : "'",
                           quoteChar == '"' ? "&quot;" : "&apos;");
    }

    /// <summary>
    /// Determines if an HTML element is a void element (self-closing, no end tag).
    /// </summary>
    private static bool IsVoidElement(string tagName)
    {
        // HTML5 void elements that never have closing tags
        var voidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };
        return voidElements.Contains(tagName);
    }
}