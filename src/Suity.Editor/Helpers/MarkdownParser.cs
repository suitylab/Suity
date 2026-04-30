using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarkedNet;
using Suity.Editor.Documents;

namespace Suity.Editor.AIGC.Helpers;

/// <summary>
/// Represents a tree node for a section of markdown content.
/// Contains the heading title, associated content, heading level, and references to child and parent nodes.
/// </summary>
public class MarkdownNode
{
    /// <summary>
    /// Gets or sets the title of this heading node.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the markdown content that belongs to this heading section.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the heading level (1-6 for h1-h6, 0 for root/document level).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the list of child nodes that are subsections of this heading.
    /// </summary>
    public List<MarkdownNode> Children { get; set; }

    /// <summary>
    /// Gets or sets the parent node of this heading in the tree hierarchy.
    /// </summary>
    public MarkdownNode Parent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownNode"/> class with default values.
    /// </summary>
    public MarkdownNode()
    {
        Children = new List<MarkdownNode>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownNode"/> class with the specified title, content, and heading level.
    /// </summary>
    /// <param name="title">The heading title.</param>
    /// <param name="content">The markdown content belonging to this heading.</param>
    /// <param name="level">The heading level (1-6 for h1-h6, 0 for root).</param>
    public MarkdownNode(string title, string content, int level) : this()
    {
        Title = title;
        Content = content;
        Level = level;
    }
}

/// <summary>
/// Holds information about a heading found in markdown text, including its title, level, and position.
/// </summary>
public class HeadingInfo
{
    /// <summary>
    /// Gets or sets the title text of the heading.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the heading level (1 for h1, 2 for h2, etc.).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the character position of the heading within the markdown text.
    /// </summary>
    public int Position { get; set; }
}

/// <summary>
/// Provides static methods to parse markdown text into heading tree structures,
/// extract heading information, and convert the tree to HTML.
/// </summary>
public static class MarkdownParser
{
    /// <summary>
    /// Parses markdown text and builds a hierarchical tree structure based on headings.
    /// Each heading becomes a node with its associated content and child subsections.
    /// </summary>
    /// <param name="markdownText">The raw markdown text to parse.</param>
    /// <returns>The root <see cref="MarkdownNode"/> of the heading tree.</returns>
    public static MarkdownNode ParseMarkdownToTree(string markdownText)
    {
        if (string.IsNullOrEmpty(markdownText))
            return new MarkdownNode();

        // Extract all heading information
        var headings = ExtractHeadings(markdownText);

        // If no headings exist, return a root node containing all content
        if (headings.Count == 0)
        {
            return new MarkdownNode("Document", markdownText, 0);
        }

        // Extract content corresponding to each heading
        var headingContents = ExtractHeadingContents(markdownText, headings);

        // Build tree structure
        return BuildTreeStructure(headings, headingContents);
    }

    /// <summary>
    /// Extract all heading information
    /// </summary>
    private static List<HeadingInfo> ExtractHeadings(string markdownText)
    {
        var headings = new List<HeadingInfo>();
        var lines = markdownText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("#"))
            {
                // Match heading format: # heading text or ## heading text etc.
                var match = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (match.Success)
                {
                    var level = match.Groups[1].Value.Length;
                    var title = match.Groups[2].Value.Trim();

                    headings.Add(new HeadingInfo
                    {
                        Title = title,
                        Level = level,
                        Position = markdownText.IndexOf(line)
                    });
                }
            }
        }

        return headings.OrderBy(h => h.Position).ToList();
    }

    /// <summary>
    /// Extract content corresponding to each heading
    /// </summary>
    private static Dictionary<string, string> ExtractHeadingContents(string markdownText, List<HeadingInfo> headings)
    {
        var contents = new Dictionary<string, string>();

        for (int i = 0; i < headings.Count; i++)
        {
            var currentHeading = headings[i];
            var nextHeading = i + 1 < headings.Count ? headings[i + 1] : null;

            int startPosition = currentHeading.Position;
            int endPosition = nextHeading?.Position ?? markdownText.Length;

            // Find the end position of the heading line
            var headingLineEnd = markdownText.IndexOf('\n', startPosition);
            if (headingLineEnd == -1) headingLineEnd = markdownText.Length;

            // Content starts after the heading line
            var contentStart = headingLineEnd + 1;

            if (contentStart < endPosition)
            {
                var content = markdownText.Substring(contentStart, endPosition - contentStart).Trim();
                contents[currentHeading.Title] = content;
            }
            else
            {
                contents[currentHeading.Title] = "";
            }
        }

        return contents;
    }

    /// <summary>
    /// Build tree structure
    /// </summary>
    private static MarkdownNode BuildTreeStructure(List<HeadingInfo> headings, Dictionary<string, string> contents)
    {
        var root = new MarkdownNode("Root", "", 0);
        var stack = new Stack<MarkdownNode>();
        stack.Push(root);

        foreach (var heading in headings)
        {
            var node = new MarkdownNode(heading.Title, contents.ContainsKey(heading.Title) ? contents[heading.Title] : "", heading.Level);

            // Adjust stack to ensure correct parent nodes
            while (stack.Peek().Level >= heading.Level)
            {
                stack.Pop();
            }

            // Set parent-child relationship
            var parent = stack.Peek();
            node.Parent = parent;
            parent.Children.Add(node);

            stack.Push(node);
        }

        return root;
    }

    /// <summary>
    /// Converts a markdown tree structure to HTML by rendering headings and parsing content with Marked.
    /// </summary>
    /// <param name="root">The root <see cref="MarkdownNode"/> of the tree to convert.</param>
    /// <returns>An HTML string representation of the markdown tree.</returns>
    public static string TreeToHtml(MarkdownNode root)
    {
        var marked = new Marked();
        return TreeToHtmlRecursive(root, marked);
    }

    private static string TreeToHtmlRecursive(MarkdownNode node, Marked marked)
    {
        var html = "";

        if (!string.IsNullOrEmpty(node.Title) && node.Level > 0)
        {
            html += $"<h{node.Level}>{node.Title}</h{node.Level}>";
        }

        if (!string.IsNullOrEmpty(node.Content))
        {
            html += marked.Parse(node.Content);
        }

        foreach (var child in node.Children)
        {
            html += TreeToHtmlRecursive(child, marked);
        }

        return html;
    }

    /// <summary>
    /// Prints the tree structure to the console for debugging purposes.
    /// Displays heading levels, titles, and a preview of content for each node.
    /// </summary>
    /// <param name="node">The <see cref="MarkdownNode"/> to start printing from.</param>
    /// <param name="indent">The current indentation string (used for recursive calls).</param>
    public static void PrintTree(MarkdownNode node, string indent = "")
    {
        if (!string.IsNullOrEmpty(node.Title))
        {
            Console.WriteLine($"{indent}{new string('#', node.Level)} {node.Title}");
            if (!string.IsNullOrEmpty(node.Content))
            {
                Console.WriteLine($"{indent}  Content: {node.Content.Substring(0, Math.Min(50, node.Content.Length))}...");
            }
        }

        foreach (var child in node.Children)
        {
            PrintTree(child, indent + "  ");
        }
    }


    /// <summary>
    /// Applies the markdown tree node structure to an <see cref="IArticle"/> instance.
    /// Sets the article title and content, then recursively creates child articles for each subsection.
    /// </summary>
    /// <param name="node">The <see cref="MarkdownNode"/> containing the title and content to apply.</param>
    /// <param name="article">The <see cref="IArticle"/> to populate with the node data.</param>
    public static void ApplyToArticle(this MarkdownNode node, IArticle article)
    {
        article.Title = node.Title?.Trim() ?? string.Empty;
        article.Content = node.Content?.Trim() ?? string.Empty;

        if (node.Children?.Count > 0)
        {
            foreach (var child in node.Children)
            {
                string childTitle = child.Title?.Trim() ?? string.Empty;
                var childArticle = article.GetOrAddArticle(childTitle);
                ApplyToArticle(child, childArticle);
            }
        }

        article.Commit();
    }
}