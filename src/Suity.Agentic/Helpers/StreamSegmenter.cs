using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Helpers;

public class Paragraph
{
    /// <summary>提取的标签名（如 highlight）</summary>
    public string? Name { get; }
    /// <summary>完整原始标签文本（如 <highlight color='red'>）</summary>
    public string? TagText { get; }

    internal StringBuilder Content { get; } = new StringBuilder();
    public bool IsCompleted { get; internal set; }
    public string Text => Content.ToString();

    public Paragraph(string? name = null, string? tagText = null)
    {
        Name = name;
        TagText = tagText;
    }

    public override string ToString() => Text;
}

public class TextSegmenter
{
    private enum ParseState { Normal, InPotentialTag }
    private ParseState _state = ParseState.Normal;

    // 双缓冲设计：_tagBuffer 存完整标签，_nameBuffer 仅存标签名
    private readonly StringBuilder _tagBuffer = new StringBuilder();
    private readonly StringBuilder _nameBuffer = new StringBuilder();
    private bool _isExtractingName = false;

    public Paragraph CurrentParagraph { get; private set; } = null!;
    public IReadOnlyList<Paragraph> Paragraphs => _paragraphs.AsReadOnly();
    private readonly List<Paragraph> _paragraphs = new List<Paragraph>();

    public event Action<Paragraph>? OnParagraphCompleted;
    public event Action<Paragraph>? OnParagraphStarted;

    public TextSegmenter() => CreateParagraph();

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

                // 协同提取标签名
                if (_isExtractingName)
                {
                    if (c == '<' || c == '/')
                    {
                        // 忽略起始符和闭合符
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

                    // 重置缓冲与状态
                    _tagBuffer.Clear();
                    _nameBuffer.Clear();
                    _isExtractingName = false;

                    if (TryProcessTag(fullTag, tagName))
                        _state = ParseState.Normal;
                    else
                    {
                        // 标签无效或不满足分段条件，降级为普通文本
                        CurrentParagraph.Content.Append(fullTag);
                        _state = ParseState.Normal;
                    }
                }
            }
        }
    }

    private bool TryProcessTag(string fullTag, string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return false;
        // 基础安全校验
        if (tagName.IndexOfAny(new[] { '<', '>' }) >= 0) return false;

        bool isClosing = fullTag.Length >= 3 && fullTag[1] == '/';

        if (!isClosing)
        {
            // 🌟 松散分段核心：仅当当前为无名段落时，起始标签才触发分段
            if (string.IsNullOrEmpty(CurrentParagraph.Name))
            {
                CompleteCurrentParagraph();
                CreateParagraph(tagName, fullTag);
                return true;
            }
            return false; // 已在命名段落内，忽略嵌套标签
        }
        else
        {
            // 🌟 结束标签仅在与当前段落名称完全匹配时才完结
            if (!string.IsNullOrEmpty(CurrentParagraph.Name) && CurrentParagraph.Name == tagName)
            {
                CompleteCurrentParagraph();
                CreateParagraph();
                return true;
            }
            return false;
        }
    }

    private void CreateParagraph(string? name = null, string? tagText = null)
    {
        CurrentParagraph = new Paragraph(name, tagText);
        _paragraphs.Add(CurrentParagraph);
        OnParagraphStarted?.Invoke(CurrentParagraph);
    }

    private void CompleteCurrentParagraph()
    {
        CurrentParagraph.IsCompleted = true;
        OnParagraphCompleted?.Invoke(CurrentParagraph);
    }

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