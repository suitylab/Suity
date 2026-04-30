using System;
using System.Collections.Generic;
using System.Linq;
using Suity.Collections;

namespace Suity.Helpers;

/// <summary>
/// Provides parsing functionality for MSON (a custom markup/object notation) format.
/// MSON supports structures, arrays, maps, and key-value pairs with a lexer-based approach.
/// </summary>
public static class MsonParser
{
    /// <summary>
    /// Token ID representing the 'null' keyword.
    /// </summary>
    public const int Id_Null = 1;

    /// <summary>
    /// Token ID representing the 'true' keyword.
    /// </summary>
    public const int Id_True = 2;

    /// <summary>
    /// Token ID representing the 'false' keyword.
    /// </summary>
    public const int Id_False = 3;

    /// <summary>
    /// Token ID representing the map operator '>>'.
    /// </summary>
    public const int Id_Map = 10;

    /// <summary>
    /// Token ID representing the assignment operator ':'.
    /// </summary>
    public const int Id_Assign = 11;

    /// <summary>
    /// Gets the lexer settings used for parsing MSON strings, including symbol and keyword definitions.
    /// </summary>
    private static LexerSettings _settings;
    public static LexerSettings Settings => _settings;

    private class ExpectedItem
    {
        public readonly MsonNode ParentNode;

        public readonly List<Token> Left = [];
        public int OpId = 0;
        public readonly List<Token> Right = [];
        public MsonNode RightNode;

        public bool ExpectNext = false;

        public bool HasValue => OpId != 0 || RightNode != null || Left.Count > 0 || Right.Count > 0 || ExpectNext;

        public ExpectedItem(MsonNode parentNode)
        {
            ParentNode = parentNode ?? throw new ArgumentNullException(nameof(parentNode));
        }

        public void AddToken(Token token)
        {
            if (OpId == 0)
            {
                Left.Add(token);
            }
            else
            {
                Right.Add(token);
            }
        }

        public void Add(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj is Token token)
            {
                AddToken(token);
            }
            else if (obj is MsonNode node)
            {
                if (OpId != 0)
                {
                    RightNode = node;
                }
            }
        }

        public MsonNode GetValue(bool clear = true)
        {
            MsonNode result = null;

            MsonNodeType type = MsonNodeType.Value;

            type = OpId switch
            {
                Id_Assign => MsonNodeType.Value,
                Id_Map => MsonNodeType.Map,
                _ => MsonNodeType.Value,
            };

            if (RightNode != null)
            {
                result = RightNode;
                result.Key = string.Join(string.Empty, Left.Select(o => o.Value));
            }
            else if (OpId == 0)
            {
                if (Left.Count == 0)
                {
                    result = new MsonNode(null);
                }
                else if (Left.Count == 1)
                {
                    result = new MsonNode(Left[0].Value);
                }
                else
                {
                    result = new MsonNode(string.Join(string.Empty, Left.Select(o => o.Value)));
                }
                result.Key = null;
            }
            else
            {
                if (Right.Count == 0)
                {
                    result = new MsonNode(null);
                }
                else if (Right.Count == 1)
                {
                    result = new MsonNode(Right[0].Value);
                }
                else
                {
                    result = new MsonNode(string.Join(string.Empty, Right.Select(o => o.Value)));
                }
                result.Key = string.Join(string.Empty, Left.Select(o => o.Value));
            }

            if (type == MsonNodeType.Map)
            {
                result.Type = type;
            }

            if (clear)
            {
                Clear();
            }

            return result;
        }

        public void Clear()
        {
            Left.Clear();
            RightNode = null;
            Right.Clear();
            OpId = 0;
            ExpectNext = false;
        }
    }

    static MsonParser()
    {
        _settings = LexerSettings.Default.Clone();
        _settings.Symbols = new Dictionary<string, int>
        {
            { ">>", Id_Map },
            { ":", Id_Assign }
        };

        _settings.Keywords = new Dictionary<string, int>
        {
            { "null", Id_Null },
            { "false", Id_False },
            { "true", Id_True }
        };
    }

    /// <summary>
    /// Parses a MSON string into an abstract syntax tree (AST) represented by a root MsonNode.
    /// </summary>
    /// <param name="str">The MSON string to parse. Returns an empty root node if null or empty.</param>
    /// <returns>The root MsonNode containing the parsed AST.</returns>
    public static MsonNode Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new MsonNode(MsonNodeType.Root);
        }

        var lexer = new Lexer(str, _settings);
        return Parse(lexer);
    }

    /// <summary>
    /// Parses a sequence of tokens into an abstract syntax tree (AST) represented by a root MsonNode.
    /// </summary>
    /// <param name="tokens">The tokens to parse, typically produced by a Lexer.</param>
    /// <returns>The root MsonNode containing the parsed AST.</returns>
    /// <exception cref="MsonException">Thrown when the token sequence contains structural errors, such as unmatched brackets or unclosed structures.</exception>
    public static MsonNode Parse(IEnumerable<Token> tokens)
    {
        var stack = new Stack<ExpectedItem>();
        var root = new MsonNode(MsonNodeType.Root);
        var item = new ExpectedItem(root);

        stack.Push(item);

        Token last = null;

        foreach (var token in tokens)
        {
            last = token;

            switch (token.Type)
            {
                case TokenType.Char:
                    {
                        char c = (char)token.Value;
                        switch (c)
                        {
                            case ',':
                                {
                                    MsonNode tokenValue = item.GetValue();
                                    item.ParentNode.AddNode(tokenValue);
                                    item.ExpectNext = true;

                                    break;
                                }
                            case '[':
                                item.RightNode = new MsonNode(MsonNodeType.Array);
                                stack.Push(item);
                                item = new ExpectedItem(item.RightNode);
                                break;

                            case ']':
                                {
                                    if (item.HasValue)
                                    {
                                        MsonNode tokenValue = item.GetValue();
                                        item.ParentNode.AddNode(tokenValue);
                                    }

                                    item.Clear();
                                    item = stack.Pop();
                                    if (stack.Count == 0)
                                    {
                                        throw new MsonException(token, "Extra ]");
                                    }
                                    if (item.RightNode.Type != MsonNodeType.Array)
                                    {
                                        throw new MsonException(token, "Required ]");
                                    }

                                    item.ExpectNext = false;
                                    break;
                                }
                            case '{':
                                item.RightNode = new MsonNode(MsonNodeType.Struct);
                                stack.Push(item);
                                item = new ExpectedItem(item.RightNode);
                                break;

                            case '}':
                                {
                                    if (item.HasValue)
                                    {
                                        MsonNode tokenValue = item.GetValue();
                                        item.ParentNode.AddNode(tokenValue);
                                    }

                                    item.Clear();
                                    item = stack.Pop();
                                    if (stack.Count == 0)
                                    {
                                        throw new MsonException(token, "Extra }");
                                    }
                                    if (item.RightNode.Type != MsonNodeType.Struct)
                                    {
                                        throw new MsonException(token, "Required }");
                                    }

                                    item.ExpectNext = false;
                                    break;
                                }
                            default:
                                item.AddToken(token);
                                item.ExpectNext = false;
                                break;
                        }
                    }
                    break;

                case TokenType.Symbol:
                    item.OpId = token.Id;
                    item.ExpectNext = false;
                    break;

                case TokenType.Number:
                    item.AddToken(token);
                    item.ExpectNext = false;
                    break;

                case TokenType.Decimal:
                    item.AddToken(token);
                    item.ExpectNext = false;
                    break;

                case TokenType.Integer:
                    item.AddToken(token);
                    item.ExpectNext = false;
                    break;

                case TokenType.Identifier:
                    item.AddToken(token);
                    item.ExpectNext = false;
                    break;

                case TokenType.Keyword:
                    {
                        switch (token.Id)
                        {
                            case Id_Null:
                                item.AddToken(new Token(token, null));
                                item.ExpectNext = false;
                                break;

                            case Id_True:
                                item.AddToken(new Token(token, true));
                                item.ExpectNext = false;
                                break;

                            case Id_False:
                                item.AddToken(new Token(token, false));
                                item.ExpectNext = false;
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                case TokenType.QuotedString:
                    item.AddToken(token);
                    item.ExpectNext = false;
                    break;

                case TokenType.WhiteSpace:
                    break;

                case TokenType.EndOfLine:
                    break;

                case TokenType.Comment:
                    break;

                case TokenType.Start:
                    break;

                case TokenType.End:
                    break;

                default:
                    break;
            }
        }

        if (item.HasValue)
        {
            MsonNode tokenValue = item.GetValue();
            item.ParentNode.AddNode(tokenValue);
        }

        if (stack.Count != 1)
        {
            throw new MsonException(last, "Structure not closed");
        }

        return root;
    }
}

/// <summary>
/// Exception thrown when an error occurs during MSON parsing, such as syntax errors or unmatched delimiters.
/// </summary>
[Serializable]
public class MsonException : Exception
{
    /// <summary>
    /// Gets the token associated with the parsing error.
    /// </summary>
    public Token Token { get; }

    /// <summary>
    /// Initializes a new instance of the MsonException class with the associated token.
    /// </summary>
    /// <param name="token">The token where the error occurred.</param>
    public MsonException(Token token)
    {
        Token = token;
    }

    /// <summary>
    /// Initializes a new instance of the MsonException class with the associated token and an error message.
    /// </summary>
    /// <param name="token">The token where the error occurred.</param>
    /// <param name="message">The error message describing the parsing issue.</param>
    public MsonException(Token token, string message) : base($"{message} {token?.ToPositionString()}")
    {
        Token = token;
    }

    /// <summary>
    /// Initializes a new instance of the MsonException class with the associated token, an error message, and an inner exception.
    /// </summary>
    /// <param name="token">The token where the error occurred.</param>
    /// <param name="message">The error message describing the parsing issue.</param>
    /// <param name="inner">The inner exception that caused this exception.</param>
    public MsonException(Token token, string message, Exception inner) : base($"{message} {token?.ToPositionString()}", inner)
    {
        Token = token;
    }

    /// <summary>
    /// Initializes a new instance of the MsonException class with serialized data for deserialization.
    /// </summary>
    /// <param name="info">The SerializationInfo holding the serialized object data.</param>
    /// <param name="context">The StreamingContext containing the source and destination of the serialized stream.</param>
    protected MsonException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Represents the type of a node in the MSON abstract syntax tree.
/// </summary>
public enum MsonNodeType
{
    /// <summary>
    /// A leaf node that holds a primitive value (string, number, boolean, or null).
    /// </summary>
    Value,

    /// <summary>
    /// The root node of the MSON AST, containing top-level child nodes.
    /// </summary>
    Root,

    /// <summary>
    /// A struct node delimited by curly braces '{}', containing key-value pairs or named members.
    /// </summary>
    Struct,

    /// <summary>
    /// An array node delimited by square brackets '[]', containing an ordered list of values.
    /// </summary>
    Array,

    /// <summary>
    /// A map node using the '>>' operator, representing a key-to-value mapping.
    /// </summary>
    Map,
}

/// <summary>
/// Represents a node in the MSON abstract syntax tree (AST).
/// A node can be a value, a container (struct, array, root), or a map entry.
/// </summary>
public class MsonNode
{
    /// <summary>
    /// Gets or sets the type of this node (Value, Root, Struct, Array, or Map).
    /// </summary>
    public MsonNodeType Type;

    /// <summary>
    /// Gets or sets the key associated with this node, used in key-value pairs and map entries.
    /// </summary>
    public string Key;

    /// <summary>
    /// Gets or sets the value stored in this node for Value-type nodes.
    /// Can be a string, number, boolean, or null.
    /// </summary>
    public object Value;

    /// <summary>
    /// Gets or sets the list of child nodes contained within this node.
    /// Used by container types (Root, Struct, Array) and Map nodes.
    /// </summary>
    public List<MsonNode> ChildNodes;

    /// <summary>
    /// Initializes a new instance of MsonNode as a container type (Root, Struct, or Array).
    /// Automatically initializes the ChildNodes collection for container types.
    /// </summary>
    /// <param name="type">The type of node to create. Must be Root, Struct, or Array for container behavior.</param>
    public MsonNode(MsonNodeType type)
    {
        Type = type;
        switch (type)
        {
            case MsonNodeType.Root:
            case MsonNodeType.Struct:
            case MsonNodeType.Array:
                ChildNodes = [];
                break;
        }
    }

    /// <summary>
    /// Initializes a new instance of MsonNode as a Value node with the specified value.
    /// </summary>
    /// <param name="value">The value to store in this node.</param>
    public MsonNode(object value)
    {
        Type = MsonNodeType.Value;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of MsonNode as a Value node with a key and value.
    /// </summary>
    /// <param name="key">The key associated with this value node.</param>
    /// <param name="value">The value to store in this node.</param>
    public MsonNode(string key, object value)
    {
        Type = MsonNodeType.Value;
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Adds a child node to this node's ChildNodes collection.
    /// Initializes the collection if it is null.
    /// </summary>
    /// <param name="node">The child node to add.</param>
    public void AddNode(MsonNode node)
    {
        (ChildNodes ??= []).Add(node);
    }

    /// <summary>
    /// Gets a child node by its zero-based index. Returns null if the index is out of range.
    /// </summary>
    /// <param name="index">The zero-based index of the child node to retrieve.</param>
    /// <returns>The child node at the specified index, or null if not found.</returns>
    public MsonNode this[int index] => ChildNodes?.GetListItemSafe(index);

    /// <summary>
    /// Gets the first child node with the specified key.
    /// </summary>
    /// <param name="key">The key to search for among child nodes.</param>
    /// <returns>The first child node with the matching key, or null if not found.</returns>
    public MsonNode this[string key] => ChildNodes?.Where(o => o.Key == key).FirstOrDefault();

    /// <summary>
    /// Gets the number of child nodes contained in this node.
    /// Returns 0 if ChildNodes is null.
    /// </summary>
    public int ChildCount => ChildNodes?.Count ?? 0;

    /// <summary>
    /// Returns a string representation of this MSON node, formatted according to its type.
    /// Value nodes are serialized as their raw value, containers use appropriate delimiters,
    /// and key-value pairs include the key prefix.
    /// </summary>
    /// <returns>A string representation of this node in MSON format.</returns>
    public override string ToString()
    {
        string text;

        if (Value == null)
        {
            text = "null";
        }
        else
        {
            text = Value switch
            {
                bool b => b ? "true" : "false",
                string str => $"\"{str}\"",
                _ => Value.ToString() ?? string.Empty,
            };
        }

        switch (Type)
        {
            case MsonNodeType.Root:
                return string.Join(",", ChildNodes ?? Array.Empty<MsonNode>() as IEnumerable<MsonNode>);

            case MsonNodeType.Struct:
                return $"{{{string.Join(",", ChildNodes ?? Array.Empty<MsonNode>() as IEnumerable<MsonNode>)}}}";

            case MsonNodeType.Array:
                return $"[{string.Join(",", ChildNodes ?? Array.Empty<MsonNode>() as IEnumerable<MsonNode>)}]";

            case MsonNodeType.Map:
                if (ChildNodes != null)
                {
                    text = $"{{{string.Join(",", ChildNodes ?? Array.Empty<MsonNode>() as IEnumerable<MsonNode>)}}}";
                }
                if (!string.IsNullOrEmpty(Key))
                {
                    return $"\"{Key}\">>{text}";
                }
                else
                {
                    return $">>{text}";
                }
            case MsonNodeType.Value:
            default:
                if (!string.IsNullOrEmpty(Key))
                {
                    return $"\"{Key}\":{text}";
                }
                else
                {
                    return text;
                }
        }
    }
}