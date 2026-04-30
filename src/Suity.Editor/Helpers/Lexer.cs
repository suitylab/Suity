using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// A lexical analyzer (tokenizer) that breaks input text or a stream into a sequence of tokens.
/// Implements <see cref="IEnumerable{Token}"/> and <see cref="IEnumerator{Token}"/> for iteration support.
/// Supports configurable behavior for handling whitespace, comments, quoted strings, identifiers, numbers, and symbols.
/// </summary>
public class Lexer : IEnumerable<Token>, IEnumerator<Token>
{
    private readonly LexerSettings _settings;
    private LexerBehavior _behavior;
    private TextReader _reader;
    private string _text;
    private int _position;
    private int _start;
    private int _textLen;
    private int _textPos;
    private int _textBeg;
    private int _bufBeg;
    private int _maxSymLen;
    private int _lineBegin;
    private int _lineNumber;
    private int _endLineBegin;
    private int _endLineNumber;
    private StringBuilder _buffer;
    private StringBuilder _tokenBuffer;
    private Token _current;
    private Token _next;

    private Lexer(string text, TextReader reader, LexerBehavior behavior, LexerSettings settings)
    {
        if (settings == null)
        {
            settings = LexerSettings.Default;
        }
        else
        {
            settings = settings.Clone();
        }

        _text = text;
        _reader = reader;
        _behavior = behavior;
        _settings = settings;

        if (settings.Symbols != null)
        {
            foreach (KeyValuePair<string, int> entry in settings.Symbols)
            {
                int len = entry.Key.Length;
                if (len > _maxSymLen)
                {
                    _maxSymLen = len;
                }
            }
        }

        Reset();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text, behavior, and settings.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    /// <param name="behavior">Flags that control lexer behavior such as skipping whitespace or comments.</param>
    /// <param name="settings">Configuration settings for the lexer, or null to use defaults.</param>
    public Lexer(string text, LexerBehavior behavior, LexerSettings settings)
        : this(text, null, behavior, settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text and behavior.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    /// <param name="behavior">Flags that control lexer behavior such as skipping whitespace or comments.</param>
    public Lexer(string text, LexerBehavior behavior)
        : this(text, null, behavior, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text and settings.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    /// <param name="settings">Configuration settings for the lexer, or null to use defaults.</param>
    public Lexer(string text, LexerSettings settings)
        : this(text, null, LexerBehavior.Default, settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    public Lexer(string text)
        : this(text, null, LexerBehavior.Default, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text reader, behavior, and settings.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read input from.</param>
    /// <param name="behavior">Flags that control lexer behavior such as skipping whitespace or comments.</param>
    /// <param name="settings">Configuration settings for the lexer, or null to use defaults.</param>
    public Lexer(TextReader reader, LexerBehavior behavior, LexerSettings settings)
        : this(null, reader, behavior, settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text reader and behavior.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read input from.</param>
    /// <param name="behavior">Flags that control lexer behavior such as skipping whitespace or comments.</param>
    public Lexer(TextReader reader, LexerBehavior behavior)
        : this(null, reader, behavior, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text reader and settings.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read input from.</param>
    /// <param name="settings">Configuration settings for the lexer, or null to use defaults.</param>
    public Lexer(TextReader reader, LexerSettings settings)
        : this(null, reader, LexerBehavior.Default, settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class with the specified text reader.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read input from.</param>
    public Lexer(TextReader reader)
        : this(null, reader, LexerBehavior.Default, null)
    {
    }

    private const int BufferCapacity = 8192;

    private const char EndOfTextChar = unchecked((char)-1);

    /// <summary>
    /// Gets the current token after calling <see cref="GetNextToken"/> or during enumeration.
    /// </summary>
    public Token Current
    {
        get
        {
            return _current;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the lexer has no input text to process.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return _text == null;
        }
    }

    /// <summary>
    /// Resets the lexer to the beginning of the input, clearing any internal state and cached tokens.
    /// If the lexer was created with a <see cref="TextReader"/>, attempts to seek the stream back to the beginning.
    /// </summary>
    public void Reset()
    {
        int readerPos = _position - _textPos;
        _current = new Token(TokenType.Start, null, null, CommonLexem.Start, 0, 0, 0, 0, 0, 0);
        _next = null;
        _textPos = 0;
        _position = 0;
        _textBeg = 0;
        _tokenBuffer = null;
        _buffer = null;
        _bufBeg = -1;

        if (_reader != null)
        {
            if (_text != null && readerPos > 0)
            {
                StreamReader streamReader = _reader as StreamReader;
                if (streamReader != null && streamReader.BaseStream.CanSeek)
                {
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    _text = null;
                }
            }

            if (_text == null)
            {
                _textLen = 0;
                ReadCharBuffer();
            }
        }
        else
        {
            _textLen = (_text == null ? 0 : _text.Length);
        }
    }

    /// <summary>
    /// Retrieves the next token from the input using the specified behavior flags temporarily.
    /// The lexer's original behavior is restored after the token is retrieved.
    /// </summary>
    /// <param name="behavior">The behavior flags to apply for this token retrieval only.</param>
    /// <returns>The next <see cref="Token"/> from the input.</returns>
    public Token GetNextToken(LexerBehavior behavior)
    {
        LexerBehavior saveBehavior = _behavior;
        _behavior = behavior;
        try
        {
            return GetNextToken();
        }
        finally
        {
            _behavior = saveBehavior;
        }
    }

    /// <summary>
    /// Retrieves the next token from the input using the lexer's current behavior settings.
    /// </summary>
    /// <returns>The next <see cref="Token"/> from the input.</returns>
    public Token GetNextToken()
    {
        if (_next != null)
        {
            _current = _next;
            _next = null;
        }
        else
        {
            _current = GetToken();
        }

        return _current;
    }

    /// <summary>
    /// Peeks at the next token from the input using the specified behavior flags temporarily,
    /// without advancing the lexer's position. The lexer's original behavior is restored after peeking.
    /// </summary>
    /// <param name="behavior">The behavior flags to apply for this peek operation only.</param>
    /// <returns>The next <see cref="Token"/> without consuming it.</returns>
    public Token PeekNextToken(LexerBehavior behavior)
    {
        LexerBehavior saveBehavior = _behavior;
        _behavior = behavior;
        try
        {
            return PeekNextToken();
        }
        finally
        {
            _behavior = saveBehavior;
        }
    }

    /// <summary>
    /// Peeks at the next token from the input without advancing the lexer's position.
    /// Subsequent calls return the same token until <see cref="GetNextToken"/> is called.
    /// </summary>
    /// <returns>The next <see cref="Token"/> without consuming it.</returns>
    public Token PeekNextToken()
    {
        if (_next == null)
        {
            _next = GetToken();
        }

        return _next;
    }

    #region Private Implementation

    private Token GetToken()
    {
        if (_text == null)
        {
            return new Token(TokenType.End, "", "", CommonLexem.End, 0, 0, 0, 0, 0, 0);
        }

        _lineBegin = _endLineBegin;
        _lineNumber = _endLineNumber;
        _start = _position;
        _textBeg = _textPos;
        _bufBeg = -1;
        _tokenBuffer = null;
        _buffer = null;

        char currentChar = PeekChar();
        bool skip;
        do
        {
            skip = false;
            // end
            if (currentChar == EndOfTextChar && EndOfText())
            {
                return GetEndToken();
            }

            // separator
            if (currentChar <= ' ')
            {
                bool skipWhiteSpaces = (_behavior & LexerBehavior.SkipWhiteSpaces) != 0;
                do
                {
                    ReadNext();
                    if (skipWhiteSpaces)
                    {
                        _textBeg = _textPos;
                    }

                    if (EndOfLine(currentChar))
                    {
                        if (skipWhiteSpaces)
                        {
                            _textBeg = _textPos;
                        }
                        else if ((_settings.Options & LexerOptions.EndOfLineAsToken) != 0)
                        {
                            return new Token(TokenType.EndOfLine, "", GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _endLineBegin, _endLineNumber);
                        }
                    }

                    currentChar = PeekChar();
                    if (currentChar == EndOfTextChar && EndOfText())
                    {
                        break;
                    }
                } while (currentChar <= ' ');

                if (!skipWhiteSpaces)
                {
                    return new Token(TokenType.WhiteSpace, "", GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _endLineBegin, _endLineNumber);
                }

                _textBeg = _textPos;
                skip = true;
                _start = _position;
            }

            // inline comment
            string[] inlineComments = _settings.InlineComments;
            if (inlineComments != null)
            {
                for (int inlineCommentIndex = 0; inlineCommentIndex < inlineComments.Length; inlineCommentIndex++)
                {
                    string inlineComment = inlineComments[inlineCommentIndex];
                    if (NextSymbolIs(inlineComment))
                    {
                        bool skipComments = ((_behavior & LexerBehavior.SkipComments) != 0);
                        skip = true;
                        if (skipComments)
                        {
                            _textBeg = _textPos;
                        }

                        currentChar = PeekChar();
                        while (true)
                        {
                            if (currentChar == '\r' || currentChar == '\n')
                            {
                                break;
                            }

                            currentChar = NextChar();
                            if (currentChar == EndOfTextChar && EndOfText())
                            {
                                break;
                            }

                            if (skipComments)
                            {
                                _textBeg = _textPos;
                            }
                        }

                        if (skipComments)
                        {
                            _start = _position;
                        }
                        else
                        {
                            return new Token(TokenType.Comment, "", GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
                        }

                        break;
                    }
                }
            }

            // comment
            if (!string.IsNullOrEmpty(_settings.CommentBegin) && NextSymbolIs(_settings.CommentBegin))
            {
                bool skipComments = ((_behavior & LexerBehavior.SkipComments) != 0);
                skip = true;
                if (skipComments)
                {
                    _textBeg = _textPos;
                }

                while (true)
                {
                    if (NextSymbolIs(_settings.CommentEnd))
                    {
                        currentChar = PeekChar();
                        if (skipComments)
                        {
                            _textBeg = _textPos;
                        }

                        break;
                    }

                    currentChar = NextChar();
                    if (currentChar == EndOfTextChar && EndOfText())
                    {
                        break;
                    }
                    else
                    {
                        EndOfLine(currentChar);
                    }

                    if (skipComments)
                    {
                        _textBeg = _textPos;
                    }
                }

                if (skipComments)
                {
                    _start = _position;
                }
                else
                {
                    return new Token(TokenType.Comment, "", GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _endLineBegin, _endLineNumber);
                }
            }

            _lineNumber = _endLineNumber;
            _lineBegin = _endLineBegin;
        } while (skip);

        // quoted string
        char[] stringQuotes = _settings.StringQuotes;
        if (stringQuotes != null)
        {
            for (int i = 0; i < stringQuotes.Length; i++)
            {
                char stringQuoteChar = stringQuotes[i];
                if (currentChar == stringQuoteChar || i == 0 && currentChar == _settings.StringPrefix && PeekChar(1) == stringQuoteChar)
                {
                    return GetQuotedStringToken(currentChar != stringQuoteChar, stringQuoteChar);
                }
            }
        }

        // quoted identifier
        bool isIdentQuote = currentChar == _settings.IdentQuote;
        bool quote = isIdentQuote || currentChar == _settings.IdentQuoteBegin;
        char nextChar;
        if (quote || currentChar == _settings.IdentPrefix && (isIdentQuote = (nextChar = PeekChar(1)) == _settings.IdentQuote || nextChar == _settings.IdentQuoteBegin))
        {
            return GetQuotedIdentifierToken(!quote, isIdentQuote);
        }

        // prefix identifier
        if (currentChar == _settings.IdentPrefix)
        {
            return GetPrefixedIdentifierToken();
        }

        // number
        if (currentChar >= '0' && currentChar <= '9')
        {
            return GetNumberToken(currentChar);
        }

        // keyword / identifier
        if (Char.IsLetter(currentChar) || currentChar == '_' || IsIdentChar(currentChar))
        {
            return GetKeywordOrIdentifierToken(currentChar);
        }

        // predefined symbol
        if (_settings.Symbols != null)
        {
            string symbol = PeekSubstring(_maxSymLen);
            for (int i = symbol.Length; i > 0; i--, symbol = symbol.Substring(0, i))
            {
                int symbolId;
                if (_settings.Symbols.TryGetValue(symbol, out symbolId))
                {
                    Skip(i);
                    string symbolText = (_behavior & LexerBehavior.PersistTokenText) != 0 ? symbol : null;
                    return new Token(TokenType.Symbol, symbol, symbolText, (int)symbolId, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
                }
            }
        }

        // just a char
        currentChar = NextChar();
        string charText = (_behavior & LexerBehavior.PersistTokenText) != 0 ? currentChar.ToString() : null;
        return new Token(TokenType.Char, currentChar, charText, 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
    }

    private Token GetEndToken()
    {
        if (_reader != null)
        {
            _reader.Close();
        }

        return new Token(TokenType.End, "", "", CommonLexem.End, _start, _start, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
    }

    private Token GetQuotedIdentifierToken(bool prefix, bool isIdentQuote)
    {
        if (prefix)
        {
            ReadNext();
        }

        char quoteEnd;
        bool doubleQuote;
        if (isIdentQuote)
        {
            quoteEnd = _settings.IdentQuote;
            doubleQuote = (_settings.Options & LexerOptions.IdentDoubleQuote) != 0;
        }
        else
        {
            quoteEnd = _settings.IdentQuoteEnd;
            doubleQuote = false;
        }

        ReadNext();
        _bufBeg = _textPos;

        while (true)
        {
            char currentChar = NextChar();
            BufferAdd(currentChar);

            if (currentChar == quoteEnd)
            {
                if (doubleQuote && PeekChar() == quoteEnd)
                {
                    EnsureBuffer(1);
                    currentChar = NextChar();
                    BufferAdd(currentChar);
                }
                else
                {
                    break;
                }
            }

            if (currentChar == EndOfTextChar && EndOfText())
            {
                break;
            }
            else
            {
                EndOfLine(currentChar);
            }
        }

        string val = GetBufferValue(-1);
        return new Token(TokenType.Identifier, val, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _endLineBegin, _endLineNumber);
    }

    private Token GetQuotedStringToken(bool prefix, char stringQuoteChar)
    {
        char escapeChar;
        bool escaping;
        bool doubleQuote;
        if (prefix)
        {
            escapeChar = '\0';
            escaping = false;
            doubleQuote = true;
            ReadNext();
        }
        else
        {
            escapeChar = _settings.StringEscapeChar;
            escaping = (_settings.Options & LexerOptions.StringEscaping) != 0;
            doubleQuote = (_settings.Options & LexerOptions.StringDoubleQuote) != 0;
        }

        ReadNext();
        _bufBeg = _textPos;

        while (true)
        {
            char currentChar = NextChar();
            BufferAdd(currentChar);

            if (currentChar == escapeChar && escaping)
            {
                EnsureBuffer(1);
                currentChar = NextChar();
                BufferAdd(currentChar);
            }
            else if (currentChar == stringQuoteChar)
            {
                if (doubleQuote && PeekChar() == stringQuoteChar)
                {
                    EnsureBuffer(1);
                    currentChar = NextChar();
                    BufferAdd(currentChar);
                }
                else
                {
                    break;
                }
            }
            else if (currentChar == EndOfTextChar && EndOfText())
            {
                break;
            }
            else
            {
                EndOfLine(currentChar);
            }
        }

        string val = GetBufferValue(-1);
        return new Token(TokenType.QuotedString, val, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _endLineBegin, _endLineNumber);
    }

    private Token GetKeywordOrIdentifierToken(char currentChar)
    {
        _bufBeg = _textPos;
        do
        {
            ReadNext();
            BufferAdd(currentChar);
            currentChar = PeekChar();
        } while (Char.IsLetterOrDigit(currentChar) || currentChar == '_' || IsIdentChar(currentChar));

        string val = GetBufferValue(0);

        int id = 0;
        TokenType tokenType = TokenType.Identifier;
        if ((_settings.Options & LexerOptions.IdentToUpper) != 0)
        {
            val = val.ToUpper(_settings.CultureInfo);
            if (_settings.Keywords != null && _settings.Keywords.TryGetValue(val, out id))
            {
                tokenType = TokenType.Keyword;
            }
        }
        else
        {
            if (_settings.Keywords != null && _settings.Keywords.TryGetValue(val.ToUpper(_settings.CultureInfo), out id))
            {
                tokenType = TokenType.Keyword;
            }

            if ((_settings.Options & LexerOptions.IdentToLower) != 0)
            {
                val = val.ToLower();
            }
        }

        return new Token(tokenType, val, GetTokenText(), (int)id, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
    }

    private Token GetNumberToken(char currentChar)
    {
        _bufBeg = _textPos;
        do
        {
            ReadNext();
            BufferAdd(currentChar);
            currentChar = PeekChar();
        }
        while (currentChar >= '0' && currentChar <= '9');

        string decimalSeparator = _settings.DecimalSeparator;
        if (SymbolIs(decimalSeparator))
        {
            int ln = decimalSeparator.Length;
            char ch = PeekChar(ln);
            if (ch >= '0' && ch <= '9')
            {
                Skip(ln);
                BufferAdd(decimalSeparator);
                currentChar = ch;
                do
                {
                    ReadNext();
                    BufferAdd(currentChar);
                    currentChar = PeekChar();
                } while (currentChar >= '0' && currentChar <= '9');
            }
        }

        if (char.IsLetter(currentChar))
        {
            do
            {
                ReadNext();
                BufferAdd(currentChar);
                currentChar = PeekChar();
            } while ((currentChar >= '0' && currentChar <= '9') || currentChar == '-' || currentChar == '+' || Char.IsLetter(currentChar));

            string val = GetBufferValue(0);
            return new Token(TokenType.Number, val, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
        }
        else
        {
            string val = GetBufferValue(0);
            decimal decimalVal;
            long intVal;
            if (long.TryParse(val, out intVal))
            {
                return new Token(TokenType.Integer, intVal, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
            }
            else if (decimal.TryParse(val, out decimalVal))
            {
                return new Token(TokenType.Decimal, decimalVal, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
            }
            else
            {
                return new Token(TokenType.Number, val, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
            }
        }
    }

    private Token GetPrefixedIdentifierToken()
    {
        ReadNext();
        _bufBeg = _textPos;

        char currentChar = PeekChar();
        if (Char.IsLetterOrDigit(currentChar) || currentChar == '_' || IsIdentChar(currentChar))
        {
            do
            {
                ReadNext();
                BufferAdd(currentChar);
                currentChar = PeekChar();
            }
            while (Char.IsLetterOrDigit(currentChar) || currentChar == '_' || IsIdentChar(currentChar));
        }

        string val = GetBufferValue(0);
        if ((_settings.Options & LexerOptions.IdentToUpper) != 0)
        {
            val = val.ToUpper(_settings.CultureInfo);
        }
        else if ((_settings.Options & LexerOptions.IdentToLower) != 0)
        {
            val = val.ToLower(_settings.CultureInfo);
        }

        return new Token(TokenType.Identifier, val, GetTokenText(), 0, _start, _position, _lineBegin, _lineNumber, _lineBegin, _lineNumber);
    }

    private bool IsIdentChar(char currentChar)
    {
        char[] identChars = _settings.IdentChars;
        if (identChars != null)
        {
            int len = identChars.Length;
            for (int i = 0; i < len; i++)
            {
                char ch = identChars[i];
                if (currentChar == ch)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private char PeekChar()
    {
        if (_textPos < _textLen)
        {
            return _text[_textPos];
        }

        if (_textLen == BufferCapacity)
        {
            ReadCharBuffer();
            if (_textPos < _textLen)
            {
                return _text[_textPos];
            }
        }

        return EndOfTextChar;
    }

    private char PeekChar(int ofs)
    {
        int i = _textPos + ofs;
        if (i < _textLen)
        {
            return _text[i];
        }

        if (_textLen == BufferCapacity)
        {
            ReadCharBuffer();
            ofs += _textPos;
            if (ofs < _textLen)
            {
                return _text[ofs];
            }
        }

        return EndOfTextChar;
    }

    private string PeekSubstring(int count)
    {
        if (_textPos + count <= _textLen)
        {
            return _text.Substring(_textPos, count);
        }

        if (_textLen == BufferCapacity)
        {
            ReadCharBuffer();
        }

        int i = _textLen - _textPos;
        if (count <= i)
        {
            return _text.Substring(_textPos, count);
        }
        else
        {
            return _text.Substring(_textPos, i);
        }
    }

    private char NextChar()
    {
        if (_textPos < _textLen)
        {
            _position++;
            return _text[_textPos++];
        }

        if (_textLen == BufferCapacity)
        {
            ReadCharBuffer();
            if (_textPos < _textLen)
            {
                _position++;
                return _text[_textPos++];
            }
        }

        return EndOfTextChar;
    }

    private void ReadNext()
    {
        if (_textPos < _textLen)
        {
            _position++;
            _textPos++;
        }
        else
        {
            if (_textLen == BufferCapacity)
            {
                ReadCharBuffer();
                _position++;
                _textPos++;
            }
        }
    }

    private bool NextSymbolIs(string s)
    {
        int ln = s.Length;
        if (_textLen - _textPos < ln && _textLen == BufferCapacity)
        {
            ReadCharBuffer();
        }

        if (_textLen - _textPos < ln || _text[_textPos] != s[0])
        {
            return false;
        }

        if (_settings.CompareInfo.Compare(_text, _textPos, ln, s, 0, ln, CompareOptions.None) == 0)
        {
            _position += ln;
            _textPos += ln;
            return true;
        }

        return false;
    }

    private bool SymbolIs(string s)
    {
        int ln = s.Length;
        if (_textLen - _textPos < ln && _textLen == BufferCapacity)
        {
            ReadCharBuffer();
        }

        if (_textLen - _textPos < ln || _text[_textPos] != s[0])
        {
            return false;
        }

        return (_settings.CompareInfo.Compare(_text, _textPos, ln, s, 0, ln, CompareOptions.None) == 0);
    }

    private void Skip(int ofs)
    {
        if (_textLen - _textPos < ofs && _textLen == BufferCapacity)
        {
            ReadCharBuffer();
        }

        int i = Math.Min(_textLen - _textPos, ofs);
        _position += i;
        _textPos += i;
    }

    private bool EndOfLine(char currentChar)
    {
        if (currentChar == '\r')
        {
            _endLineNumber++;
            _endLineBegin = _position;
            currentChar = PeekChar();
            if (currentChar == '\n')
            {
                ReadNext();
                BufferAdd(currentChar);
                _endLineBegin = _position;
            }

            return true;
        }
        else if (currentChar == '\n')
        {
            _endLineNumber++;
            _endLineBegin = _position;

            return true;
        }

        return false;
    }

    private bool EndOfText()
    {
        if (_textPos < _textLen)
        {
            return false;
        }

        if (_textLen == BufferCapacity)
        {
            ReadCharBuffer();
            return _textPos >= _textLen;
        }

        return true;
    }

    private void BufferAdd(char currentChar)
    {
        if (_buffer != null)
        {
            _buffer.Append(currentChar);
        }
        else if (_bufBeg >= 0 && _textPos >= _textLen)
        {
            _buffer = new StringBuilder(_text, _bufBeg, _textPos - _bufBeg, BufferCapacity);
        }
    }

    private void BufferAdd(string str)
    {
        if (_buffer != null)
        {
            _buffer.Append(str);
        }
        else if (_bufBeg >= 0 && _textPos >= _textLen)
        {
            _buffer = new StringBuilder(_text, _bufBeg, _textPos - _bufBeg, BufferCapacity);
        }
    }

    private void EnsureBuffer(int ofs)
    {
        if (_buffer == null)
        {
            _buffer = new StringBuilder(_text, _bufBeg, _textPos - _bufBeg - ofs, BufferCapacity);
        }
        else
        {
            _buffer.Remove(_buffer.Length - ofs, ofs);
        }
    }

    private string GetBufferValue(int ofs)
    {
        if (_buffer != null)
        {
            return _buffer.ToString(0, _buffer.Length + ofs);
        }
        else
        {
            return _text.Substring(_bufBeg, _textPos - _bufBeg + ofs);
        }
    }

    private void ReadCharBuffer()
    {
        if (_reader == null)
        {
            return;
        }

        if (_tokenBuffer != null)
        {
            _tokenBuffer.Append(_text, 0, _textPos);
        }
        else if (_textBeg < _textPos && (_behavior & LexerBehavior.PersistTokenText) != 0)
        {
            _tokenBuffer = new StringBuilder(_text, _textBeg, _textPos - _textBeg, BufferCapacity);
        }
        else
        {
            _textBeg = 0;
        }

        char[] charBuffer = new char[BufferCapacity];
        if (_textPos < _textLen)
        {
            if (_textPos == 0)
            {
                throw new ArgumentException("'BufferCapacity' too small.");
            }
            _textLen -= _textPos;
            _text.CopyTo(_textPos, charBuffer, 0, _textLen);
        }
        else
        {
            _textLen = 0;
        }

        _textLen += _reader.Read(charBuffer, _textLen, BufferCapacity - _textLen);
        _text = new string(charBuffer, 0, _textLen);
        _textPos = 0;
    }

    private string GetTokenText()
    {
        if (_tokenBuffer != null)
        {
            _tokenBuffer.Append(_text, 0, _textPos);
            return _tokenBuffer.ToString(0, _tokenBuffer.Length);
        }

        if ((_behavior & LexerBehavior.PersistTokenText) == 0)
        {
            return null;
        }
        else
        {
            return _text.Substring(_textBeg, _textPos - _textBeg);
        }
    }

    #endregion

    #region IEnumerable<Token> Members

    /// <summary>
    /// Returns an enumerator that iterates through the tokens produced by this lexer.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{Token}"/> for this lexer.</returns>
    IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
    {
        return this;
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Returns an enumerator that iterates through the tokens produced by this lexer.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> for this lexer.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    #endregion

    #region IEnumerator Members

    /// <summary>
    /// Gets the current token in the enumeration.
    /// </summary>
    object IEnumerator.Current
    {
        get
        {
            return _current;
        }
    }

    /// <summary>
    /// Advances the enumerator to the next token.
    /// </summary>
    /// <returns>true if the enumerator was successfully advanced to the next token; false if the end of the input has been reached.</returns>
    bool IEnumerator.MoveNext()
    {
        return GetNextToken().Type != TokenType.End;
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Releases resources used by the lexer, including closing the underlying <see cref="TextReader"/> if one was provided.
    /// </summary>
    public void Dispose()
    {
        if (_reader != null)
        {
            _reader.Dispose();
        }
    }

    #endregion
}

/// <summary>
/// Represents the type of a token produced by the <see cref="Lexer"/>.
/// </summary>
public enum TokenType
{
    /// <summary>Represents a single character token that did not match any other token type.</summary>
    Char,
    /// <summary>Represents a predefined symbol or operator defined in <see cref="LexerSettings.Symbols"/>.</summary>
    Symbol,
    /// <summary>Represents a numeric value that could not be parsed as an integer or decimal.</summary>
    Number,
    /// <summary>Represents a decimal (floating-point) numeric value.</summary>
    Decimal,
    /// <summary>Represents an integer numeric value.</summary>
    Integer,
    /// <summary>Represents an identifier (variable name, function name, etc.).</summary>
    Identifier,
    /// <summary>Represents a reserved keyword defined in <see cref="LexerSettings.Keywords"/>.</summary>
    Keyword,
    /// <summary>Represents a string literal enclosed in quote characters.</summary>
    QuotedString,
    /// <summary>Represents whitespace (spaces, tabs, etc.) between tokens.</summary>
    WhiteSpace,
    /// <summary>Represents an end-of-line character sequence.</summary>
    EndOfLine,
    /// <summary>Represents a comment (inline or block).</summary>
    Comment,
    /// <summary>Represents the start of tokenization, used as an initial sentinel token.</summary>
    Start,
    /// <summary>Represents the end of the input stream.</summary>
    End
}

/// <summary>
/// Flags that control how the <see cref="Lexer"/> processes input text.
/// </summary>
[Flags]
public enum LexerBehavior
{
    /// <summary>When set, whitespace characters are skipped and not returned as tokens.</summary>
    SkipWhiteSpaces = 1,
    /// <summary>When set, comment content is skipped and not returned as tokens.</summary>
    SkipComments = 2,
    /// <summary>When set, the original text of each token is preserved in the <see cref="Token.Text"/> property.</summary>
    PersistTokenText = 4,
    /// <summary>The default behavior: preserves token text but returns whitespace and comments as tokens.</summary>
    Default = PersistTokenText
}

/// <summary>
/// Configuration options that control how the <see cref="Lexer"/> recognizes identifiers, strings, and line endings.
/// </summary>
[Flags]
public enum LexerOptions
{
    /// <summary>Identifiers are compared case-insensitively when looking up keywords.</summary>
    IdentIgnoreCase = 1,
    /// <summary>Identifier values are converted to lowercase.</summary>
    IdentToLower = 2,
    /// <summary>Identifier values are converted to uppercase.</summary>
    IdentToUpper = 4,
    /// <summary>Allows escaped quote characters inside quoted identifiers by doubling the quote character.</summary>
    IdentDoubleQuote = 8,
    /// <summary>Enables escape character processing inside quoted strings.</summary>
    StringEscaping = 16,
    /// <summary>Allows escaped quote characters inside quoted strings by doubling the quote character.</summary>
    StringDoubleQuote = 32,
    /// <summary>Treats end-of-line sequences as separate tokens instead of whitespace.</summary>
    EndOfLineAsToken = 64
}

/// <summary>
/// Represents a single token produced by the <see cref="Lexer"/>, containing the token type,
/// parsed value, original text, and source position information.
/// </summary>
public sealed class Token
{
    /// <summary>The type of this token, indicating what kind of lexeme was recognized.</summary>
    public readonly TokenType Type;
    /// <summary>The parsed value of the token (e.g., an integer, decimal, or string). May be null for some token types.</summary>
    public readonly object Value;
    /// <summary>The original text from the source that produced this token, or null if not preserved.</summary>
    public readonly string Text;
    /// <summary>A numeric identifier for keywords or symbols, as defined in <see cref="LexerSettings"/>. Zero for other token types.</summary>
    public readonly int Id;
    /// <summary>The zero-based character position in the source where this token begins.</summary>
    public readonly int StartPosition;
    /// <summary>The zero-based character position in the source where this token ends (exclusive).</summary>
    public readonly int EndPosition;
    /// <summary>The zero-based character position in the source where the line containing the start of this token begins.</summary>
    public readonly int LineBegin;
    /// <summary>The one-based line number where this token begins.</summary>
    public readonly int LineNumber;
    /// <summary>The zero-based character position in the source where the line containing the end of this token begins.</summary>
    public readonly int EndLineBegin;
    /// <summary>The one-based line number where this token ends.</summary>
    public readonly int EndLineNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class with all properties specified.
    /// </summary>
    /// <param name="type">The type of the token.</param>
    /// <param name="value">The parsed value of the token.</param>
    /// <param name="text">The original source text of the token.</param>
    /// <param name="id">A numeric identifier for keywords or symbols.</param>
    /// <param name="startPosition">The start position in the source.</param>
    /// <param name="endPosition">The end position in the source.</param>
    /// <param name="lineBegin">The position where the starting line begins.</param>
    /// <param name="lineNumber">The line number where the token starts.</param>
    /// <param name="endLineBegin">The position where the ending line begins.</param>
    /// <param name="endLineNumber">The line number where the token ends.</param>
    public Token(TokenType type, object value, string text, int id, int startPosition, int endPosition, int lineBegin, int lineNumber, int endLineBegin, int endLineNumber)
    {
        Type = type;
        Value = value;
        Text = text;
        Id = id;
        StartPosition = startPosition;
        EndPosition = endPosition;
        LineBegin = lineBegin;
        LineNumber = lineNumber;
        EndLineBegin = endLineBegin;
        EndLineNumber = endLineNumber;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class by copying all properties from another token
    /// except for the value, which is replaced with the specified new value.
    /// </summary>
    /// <param name="other">The token to copy properties from.</param>
    /// <param name="value">The new parsed value for this token.</param>
    public Token(Token other, object value)
    {
        Type = other.Type;
        Value = value;
        Text = other.Text;
        Id = other.Id;
        StartPosition = other.StartPosition;
        EndPosition = other.EndPosition;
        LineBegin = other.LineBegin;
        LineNumber = other.LineNumber;
        EndLineBegin = other.EndLineBegin;
        EndLineNumber = other.EndLineNumber;
    }

    /// <summary>
    /// Gets the zero-based column position of the token's start within its line.
    /// </summary>
    public int LinePosition
    {
        get
        {
            return StartPosition - LineBegin;
        }
    }

    /// <summary>
    /// Gets the zero-based column position of the token's end within its line.
    /// </summary>
    public int EndLinePosition
    {
        get
        {
            return EndPosition - EndLineBegin;
        }
    }

    /// <summary>
    /// Returns a string representation of this token showing its type and parsed value.
    /// </summary>
    /// <returns>A string in the format "TokenType Value".</returns>
    public override string ToString()
    {
        return $"{Type.ToString()} {Value}";
    }

    /// <summary>
    /// Returns a string representation of this token's line and position information.
    /// </summary>
    /// <returns>A string in the format "Line:{LineBegin} Position:{LineNumber}".</returns>
    public string ToPositionString()
    {
        return $"Line:{LineBegin} Position:{LineNumber}";
    }
}

/// <summary>
/// Configuration settings for the <see cref="Lexer"/>, defining which characters and patterns
/// are recognized as strings, identifiers, comments, symbols, keywords, and numbers.
/// </summary>
public sealed class LexerSettings : ICloneable
{
    /// <summary>Gets or sets options that control identifier case handling, string escaping, and line-end tokenization.</summary>
    public LexerOptions Options { get; set; }
    /// <summary>Gets or sets a dictionary of predefined symbols (operators, punctuation) mapped to numeric IDs.</summary>
    public IDictionary<string, int> Symbols { get; set; }
    /// <summary>Gets or sets a dictionary of reserved keywords mapped to numeric IDs.</summary>
    public IDictionary<string, int> Keywords { get; set; }
    /// <summary>Gets or sets the <see cref="CultureInfo"/> used for string comparisons and case conversion.</summary>
    public CultureInfo CultureInfo { get; set; }
    /// <summary>Gets or sets the <see cref="CompareInfo"/> used for culture-aware string comparisons.</summary>
    public CompareInfo CompareInfo { get; set; }
    /// <summary>Gets or sets the array of characters that can delimit a quoted string (e.g., double-quote, single-quote).</summary>
    public char[] StringQuotes { get; set; }
    /// <summary>Gets or sets the escape character used inside quoted strings (e.g., backslash).</summary>
    public char StringEscapeChar { get; set; }
    /// <summary>Gets or sets the prefix character for verbatim/raw strings (e.g., '@' in C#).</summary>
    public char StringPrefix { get; set; }
    /// <summary>Gets or sets the single character used to quote identifiers (e.g., backtick).</summary>
    public char IdentQuote { get; set; }
    /// <summary>Gets or sets the opening character for multi-character quoted identifiers.</summary>
    public char IdentQuoteBegin { get; set; }
    /// <summary>Gets or sets the closing character for multi-character quoted identifiers.</summary>
    public char IdentQuoteEnd { get; set; }
    /// <summary>Gets or sets the prefix character for identifiers (e.g., '$' or '@').</summary>
    public char IdentPrefix { get; set; }
    /// <summary>Gets or sets additional characters that are valid within identifiers beyond letters and digits.</summary>
    public char[] IdentChars { get; set; }
    /// <summary>Gets or sets the prefixes that start an inline/single-line comment (e.g., "//").</summary>
    public string[] InlineComments { get; set; }
    /// <summary>Gets or sets the string that begins a block/multi-line comment (e.g., "/*").</summary>
    public string CommentBegin { get; set; }
    /// <summary>Gets or sets the string that ends a block/multi-line comment (e.g., "*/").</summary>
    public string CommentEnd { get; set; }
    /// <summary>Gets or sets the character sequence used as a decimal separator in numbers (e.g., ".").</summary>
    public string DecimalSeparator { get; set; }

    /// <summary>
    /// Gets the default lexer settings configured for C-style languages.
    /// Supports double/single-quoted strings, C-style comments (// and /* */),
    /// case-insensitive identifiers, and period as the decimal separator.
    /// </summary>
    public static LexerSettings Default
    {
        get
        {
            LexerSettings settings = new LexerSettings();
            settings.CultureInfo = CultureInfo.InvariantCulture;
            settings.CompareInfo = CultureInfo.InvariantCulture.CompareInfo;
            settings.DecimalSeparator = ".";
            settings.Options = LexerOptions.IdentIgnoreCase | LexerOptions.StringDoubleQuote;
            settings.StringQuotes = new char[] { '\"', '\'' };
            settings.InlineComments = new string[] { "//" };
            settings.CommentBegin = "/*";
            settings.CommentEnd = "*/";
            settings.StringEscapeChar = '\\';
            settings.StringPrefix = '@';
            settings.IdentQuote = '\0';
            settings.IdentQuoteBegin = '\0';
            settings.IdentQuoteEnd = '\0';

            return settings;
        }
    }

    #region ICloneable Members

    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    /// Creates a deep copy of this <see cref="LexerSettings"/> instance.
    /// Dictionaries and arrays are cloned; culture settings are preserved.
    /// </summary>
    /// <returns>A new <see cref="LexerSettings"/> instance with the same configuration.</returns>
    public LexerSettings Clone()
    {
        LexerSettings settings = (LexerSettings)MemberwiseClone();

        if (settings.CultureInfo == null)
        {
            settings.CultureInfo = CultureInfo.InvariantCulture;
        }

        if (settings.CompareInfo == null)
        {
            settings.CompareInfo = settings.CultureInfo.CompareInfo;
        }

        if (string.IsNullOrEmpty(settings.DecimalSeparator))
        {
            settings.DecimalSeparator = settings.CultureInfo.NumberFormat.NumberDecimalSeparator;
        }

        if (settings.Symbols != null && settings.Symbols.Count > 0)
        {
            settings.Symbols = new Dictionary<string, int>(settings.Symbols);
        }
        else
        {
            settings.Symbols = null;
        }

        if (settings.Keywords != null && settings.Keywords.Count > 0)
        {
            bool ignoreCase = (settings.Options & LexerOptions.IdentIgnoreCase) != 0;
            settings.Keywords = new Dictionary<string, int>(settings.Keywords, StringComparer.Create(settings.CultureInfo, ignoreCase));
        }
        else
        {
            settings.Keywords = null;
        }

        if (settings.StringQuotes != null)
        {
            settings.StringQuotes = (char[])settings.StringQuotes.Clone();
        }

        if (settings.IdentChars != null)
        {
            settings.IdentChars = (char[])settings.IdentChars.Clone();
        }

        string[] inlineComments = settings.InlineComments;
        if (inlineComments != null)
        {
            int length = inlineComments.Length;
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                string inlineComment = inlineComments[i];
                if (inlineComment == null)
                {
                    continue;
                }

                if (i != count)
                {
                    inlineComments[count] = inlineComment;
                }

                count++;
            }

            if (count == 0)
            {
                settings.InlineComments = null;
            }
            else
            {
                string[] arr = new string[count];
                Array.Copy(inlineComments, 0, arr, 0, count);
            }
        }

        if (!string.IsNullOrEmpty(settings.CommentBegin) && string.IsNullOrEmpty(settings.CommentEnd))
        {
            settings.CommentEnd = settings.CommentBegin;
        }

        return settings;
    }

    #endregion
}

/// <summary>
/// Defines constant integer IDs for common lexem types used internally by the <see cref="Lexer"/>.
/// </summary>
internal static class CommonLexem
{
    /// <summary>Lexem ID representing the start of tokenization.</summary>
    public const int Start = 1;
    /// <summary>Lexem ID representing the end of the input stream.</summary>
    public const int End = 2;
}