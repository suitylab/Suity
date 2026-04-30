using Suity.Collections;
using Suity.Editor.Helpers;
using System;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender.Replacing;

/// <summary>
/// Represents the type of a token in the segment parsing process.
/// </summary>
public enum SegmentTokenType
{
    /// <summary>
    /// No token type.
    /// </summary>
    None,
    /// <summary>
    /// Regular code content.
    /// </summary>
    Code,
    /// <summary>
    /// Begin tag prefix (e.g., //{#).
    /// </summary>
    PrefixBegin,
    /// <summary>
    /// End tag prefix (e.g., //}#).
    /// </summary>
    PrefixEnd,
    /// <summary>
    /// Tag suffix (e.g., #//).
    /// </summary>
    Suffix,
}

/// <summary>
/// Reads tokens sequentially during the segment parsing process.
/// </summary>
internal class SegmentTokenReader
{
    private readonly List<WordTreeExToken<SegmentTokenType>> _list;

    /// <summary>
    /// Gets or sets the current index in the token list.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTokenReader"/> class.
    /// </summary>
    /// <param name="list">The list of tokens to read from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null.</exception>
    public SegmentTokenReader(List<WordTreeExToken<SegmentTokenType>> list)
    {
        _list = list ?? throw new ArgumentNullException(nameof(list));
    }

    /// <summary>
    /// Gets a value indicating whether the reader has reached the end of the token list.
    /// </summary>
    public bool IsEnd => Index >= _list.Count;

    /// <summary>
    /// Peeks at the current token without advancing the reader.
    /// </summary>
    /// <returns>The current token, or null if at the end.</returns>
    public WordTreeExToken<SegmentTokenType> Peek()
    {
        return _list.GetListItemSafe(Index);
    }

    /// <summary>
    /// Peeks at a token at a relative position and checks if it matches the specified type.
    /// </summary>
    /// <param name="type">The token type to check for.</param>
    /// <param name="forward">Number of positions to look ahead (default is 0).</param>
    /// <returns><c>true</c> if the token at the specified position matches the type; otherwise, <c>false</c>.</returns>
    public bool PeekTokenType(SegmentTokenType type, int forward = 0)
    {
        return _list.GetListItemSafe(Index + forward)?.Type == type;
    }

    /// <summary>
    /// Reads the current token and advances the reader.
    /// </summary>
    /// <returns>The current token, or null if at the end.</returns>
    public WordTreeExToken<SegmentTokenType> Read()
    {
        if (!IsEnd)
        {
            var token = _list[Index];
            Index++;
            return token;
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// Parses code strings into a tree of <see cref="SegmentNode"/> objects based on segment markers.
/// </summary>
public class SegmentParser
{
    /// <summary>
    /// Gets the configuration used for parsing segment markers.
    /// </summary>
    public CodeSegmentConfig Config { get; }

    private readonly WordTreeEx<SegmentTokenType> _tokenizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentParser"/> class.
    /// </summary>
    /// <param name="config">The segment configuration defining prefix and suffix markers.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public SegmentParser(CodeSegmentConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        _tokenizer = new WordTreeEx<SegmentTokenType>();
        _tokenizer.Add(config.PrefixBegin, SegmentTokenType.PrefixBegin);
        _tokenizer.Add(config.PrefixEnd, SegmentTokenType.PrefixEnd);
        _tokenizer.Add(config.Suffix, SegmentTokenType.Suffix);
    }

    /// <summary>
    /// Parses the specified code string into a <see cref="SegmentRootNode"/> tree.
    /// </summary>
    /// <param name="code">The code string to parse.</param>
    /// <param name="defaultKeyString">Default key string used when parsing tags with incomplete key parts.</param>
    /// <returns>The parsed <see cref="SegmentRootNode"/>, or null if parsing fails due to malformed tags.</returns>
    public SegmentRootNode Parse(string code, string defaultKeyString)
    {
        List<WordTreeExToken<SegmentTokenType>> list = _tokenizer.Tokenize(code, SegmentTokenType.Code);

        var reader = new SegmentTokenReader(list);

        var root = new SegmentRootNode();
        SegmentElementNode current = root;

        while (!reader.IsEnd)
        {
            var token = reader.Read();

            switch (token.Type)
            {
                case SegmentTokenType.Code:
                    current.Add(new SegmentTextNode(token.Value));
                    break;

                case SegmentTokenType.PrefixBegin:
                    {
                        if (!reader.PeekTokenType(SegmentTokenType.Code, 0))
                        {
                            return null;
                        }
                        if (!reader.PeekTokenType(SegmentTokenType.Suffix, 1))
                        {
                            return null;
                        }

                        var tagCode = reader.Read();
                        reader.Read();

                        var node = new SegmentTagNode(Config, tagCode.Value, defaultKeyString);
                        current.Add(node);
                        current = node;
                    }
                    break;

                case SegmentTokenType.PrefixEnd:
                    if (!reader.PeekTokenType(SegmentTokenType.Code, 0))
                    {
                        return null;
                    }
                    if (!reader.PeekTokenType(SegmentTokenType.Suffix, 1))
                    {
                        return null;
                    }

                    reader.Read();
                    reader.Read();

                    current = current.Parent as SegmentElementNode;
                    if (current == null)
                    {
                        return null;
                    }

                    break;

                case SegmentTokenType.Suffix:
                case SegmentTokenType.None:
                default:
                    current.Add(new SegmentTextNode(token.Value));
                    break;
            }
        }

        return root;
    }

    private static readonly Dictionary<CodeSegmentConfig, SegmentParser> _parsers = [];

    /// <summary>
    /// Gets or creates a cached parser instance for the specified configuration.
    /// </summary>
    /// <param name="config">The segment configuration.</param>
    /// <returns>A <see cref="SegmentParser"/> instance for the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static SegmentParser GetParser(CodeSegmentConfig config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        lock (_parsers)
        {
            return _parsers.GetOrAdd(config, _ => new SegmentParser(config));
        }
    }
}