using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Helpers;

/// <summary>
/// Represents a paragraph of text extracted from a stream, optionally associated with an XML tag.
/// </summary>
public class Paragraph
{
    /// <summary>Extracted tag name (e.g., highlight)</summary>
    public string? Name { get; }
    /// <summary>Full original tag text (e.g., &lt;highlight color='red'&gt;)</summary>
    public string? TagText { get; }

    /// <summary>Internal buffer for accumulating paragraph content.</summary>
    internal StringBuilder Content { get; } = new StringBuilder();
    /// <summary>Gets a value indicating whether this paragraph has been completed.</summary>
    public bool IsCompleted { get; internal set; }
    /// <summary>Gets the complete text content of this paragraph.</summary>
    public string Text => Content.ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="Paragraph"/> class.
    /// </summary>
    /// <param name="name">The extracted tag name, or null for untagged paragraphs.</param>
    /// <param name="tagText">The full original tag text, or null.</param>
    public Paragraph(string? name = null, string? tagText = null)
    {
        Name = name;
        TagText = tagText;
    }

    /// <inheritdoc/>
    public override string ToString() => Text;
}

/// <summary>
/// Segments a continuous text stream into paragraphs based on XML-style tags.
/// Supports detecting opening and closing tags to create structured paragraph boundaries.
/// </summary>
public class TextSegmenter
{
    /// <summary>Parsing state machine states.</summary>
    private enum ParseState { Normal, InPotentialTag }
    private ParseState _state = ParseState.Normal;

    // Dual-buffer design: _tagBuffer stores the full tag, _nameBuffer stores only the tag name
    private readonly StringBuilder _tagBuffer = new StringBuilder();
    private readonly StringBuilder _nameBuffer = new StringBuilder();
    private bool _isExtractingName = false;

    /// <summary>Gets the paragraph currently being accumulated.</summary>
    public Paragraph CurrentParagraph { get; private set; } = null!;
    /// <summary>Gets a read-only list of all paragraphs produced by this segmenter.</summary>
    public IReadOnlyList<Paragraph> Paragraphs => _paragraphs.AsReadOnly();
    private readonly List<Paragraph> _paragraphs = new List<Paragraph>();

    /// <summary>Event raised when a paragraph is completed.</summary>
    public event Action<Paragraph>? OnParagraphCompleted;
    /// <summary>Event raised when a new paragraph is started.</summary>
    public event Action<Paragraph>? OnParagraphStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextSegmenter"/> class.
    /// </summary>
    public TextSegmenter() => CreateParagraph();

    /// <summary>
    /// Feeds input text into the segmenter for parsing.
    /// </summary>
    /// <param name="text">The text chunk to process.</param>
    public void InputText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (_state == ParseState.Normal)
            {
                if (c == '<')
                {
                    _state = ParseState.InPotentialTag;
                    _tagBuffer.Clear(); _tagBuffer.Append(c);
                    _nameBuffer.Clear();
                    _isExtractingName = true;
                }
                else
                {
                    CurrentParagraph.Content.Append(c);
                }
            }
            else // InPotentialTag
            {
                _tagBuffer.Append(c);

                // Extract tag name cooperatively
                if (_isExtractingName)
                {
                    if (c == '<' || c == '/')
                    {
                        // Ignore start and closing characters
                    }
                    else if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '>')
                    {
                        _isExtractingName = false;
                    }
                    else
                    {
                        _nameBuffer.Append(c);
                    }
                }

                if (c == '>')
                {
                    string fullTag = _tagBuffer.ToString();
                    string tagName = _nameBuffer.ToString().Trim();

                    // Reset buffers and state
                    _tagBuffer.Clear();
                    _nameBuffer.Clear();
                    _isExtractingName = false;

                    if (TryProcessTag(fullTag, tagName))
                        _state = ParseState.Normal;
                    else
                    {
                        // Tag is invalid or does not meet segmentation criteria, fall back to plain text
                        CurrentParagraph.Content.Append(fullTag);
                        _state = ParseState.Normal;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to process a detected tag and determine if it should trigger paragraph segmentation.
    /// </summary>
    /// <param name="fullTag">The complete tag text including angle brackets.</param>
    /// <param name="tagName">The extracted tag name.</param>
    /// <returns>True if the tag was processed and state reset to Normal; otherwise, false.</returns>
    private bool TryProcessTag(string fullTag, string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return false;
        // Basic security validation
        if (tagName.IndexOfAny(new[] { '<', '>' }) >= 0) return false;

        bool isClosing = fullTag.Length >= 3 && fullTag[1] == '/';

        if (!isClosing)
        {
            // Loose segmentation core: opening tags only trigger segmentation when the current paragraph is unnamed
            if (string.IsNullOrEmpty(CurrentParagraph.Name))
            {
                CompleteCurrentParagraph();
                CreateParagraph(tagName, fullTag);
                return true;
            }
            return false; // Already inside a named paragraph, ignore nested tags
        }
        else
        {
            // Closing tags only complete the paragraph when the name matches exactly
            if (!string.IsNullOrEmpty(CurrentParagraph.Name) && CurrentParagraph.Name == tagName)
            {
                CompleteCurrentParagraph();
                CreateParagraph();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Creates a new paragraph and adds it to the collection.
    /// </summary>
    /// <param name="name">Optional tag name for the paragraph.</param>
    /// <param name="tagText">Optional full tag text.</param>
    private void CreateParagraph(string? name = null, string? tagText = null)
    {
        CurrentParagraph = new Paragraph(name, tagText);
        _paragraphs.Add(CurrentParagraph);
        OnParagraphStarted?.Invoke(CurrentParagraph);
    }

    /// <summary>
    /// Marks the current paragraph as completed and raises the completion event.
    /// </summary>
    private void CompleteCurrentParagraph()
    {
        CurrentParagraph.IsCompleted = true;
        OnParagraphCompleted?.Invoke(CurrentParagraph);
    }

    /// <summary>
    /// Flushes any pending buffered content and completes the current paragraph.
    /// Call this when the input stream ends to ensure all content is finalized.
    /// </summary>
    public void Flush()
    {
        if (_state == ParseState.InPotentialTag)
        {
            CurrentParagraph.Content.Append(_tagBuffer.ToString());
            _state = ParseState.Normal;
            _tagBuffer.Clear();
            _nameBuffer.Clear();
            _isExtractingName = false;
        }
        if (!CurrentParagraph.IsCompleted)
            CompleteCurrentParagraph();
    }
}
