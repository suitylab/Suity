using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Collections;

/// <summary>
/// Represents a node in a word tree.
/// </summary>
internal class WordTreeNode
{
    private readonly char _chr;

    internal string _word;

    /// <summary>
    /// Indicates if the node is a terminal node.
    /// </summary>
    internal bool _isTerminal;

    private Dictionary<char, WordTreeNode> _childNodes;

    public WordTreeNode(char chr)
    {
        _chr = chr;
    }

    public string Word => _word;

    /// <summary>
    /// Adds a word to the tree.
    /// </summary>
    /// <param name="code">The word to add.</param>
    public void Add(string code, string word)
    {
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        char c = code[0];
        string rest = code.Substring(1, code.Length - 1);

        WordTreeNode childNode = EnsureNode(c);
        if (rest.Length > 0)
        {
            childNode.Add(rest, word);
        }
        else
        {
            childNode._isTerminal = true;
            childNode._word = word;
        }
    }

    /// <summary>
    /// Matches a substring of a string starting at a given index.
    /// </summary>
    /// <param name="str">The string to match.</param>
    /// <param name="index">The starting index in the string.</param>
    /// <returns>True if the substring matches, otherwise false.</returns>
    public bool Match(string str, int index, out string word)
    {
        if (str[index] != _chr)
        {
            word = null;
            return false;
        }

        if (_isTerminal)
        {
            word = _word;
            return true;
        }

        if (_childNodes != null && index + 1 < str.Length)
        {
            foreach (var nod in _childNodes.Values)
            {
                if (nod.Match(str, index + 1, out word))
                {
                    return true;
                }
            }
        }

        word = null;
        return false;
    }

    /// <summary>
    /// Ensures that a child node exists for a given character.
    /// </summary>
    /// <param name="c">The character to ensure a node for.</param>
    /// <returns>The child node.</returns>
    private WordTreeNode EnsureNode(char c) => (_childNodes ??= []).GetOrAdd(c, _ => new(c));

    /// <summary>
    /// Gets the child node for a given character.
    /// </summary>
    /// <param name="c">The character to get the node for.</param>
    /// <returns>The child node, or null if it does not exist.</returns>
    private WordTreeNode GetNode(char c) => _childNodes?.GetValueSafe(c);
}

/// <summary>
/// Represents a word tree.
/// </summary>
public class WordTree
{
    private Dictionary<char, WordTreeNode> _childNodes = [];

    public WordTree()
    {
    }

    public WordTree(IEnumerable<string> words)
    {
        AddRange(words);
    }

    /// <summary>
    /// Adds a word to the tree.
    /// </summary>
    /// <param name="word">The word to add.</param>
    public void Add(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        char c = word[0];
        string rest = word.Substring(1, word.Length - 1);

        WordTreeNode childNode = EnsureNode(c);
        if (rest.Length > 0)
        {
            childNode.Add(rest, word);
        }
        else
        {
            childNode._isTerminal = true;
            childNode._word = word;
        }
    }

    public void AddRange(IEnumerable<string> words)
    {
        foreach (var word in words.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            Add(word.Trim());
        }
    }

    /// <summary>
    /// Matches a string against the tree.
    /// </summary>
    /// <param name="str">The string to match.</param>
    /// <returns>True if the string matches, otherwise false.</returns>
    public bool Match(string str, out string word)
    {
        if (string.IsNullOrEmpty(str))
        {
            word = null;
            return false;
        }

        for (int i = 0; i < str.Length; i++)
        {
            foreach (var node in _childNodes.Values)
            {
                if (node.Match(str, i, out word))
                {
                    return true;
                }
            }
        }

        word = null;
        return false;
    }

    public string Mask(string str, string mask)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        HashSet<string> words = null;

        for (int i = 0; i < str.Length; i++)
        {
            foreach (var node in _childNodes.Values)
            {
                if (node.Match(str, i, out string word))
                {
                    (words ??= []).Add(word);
                }
            }
        }

        if (words is null)
        {
            return str;
        }

        var builder = new StringBuilder(str);
        foreach (var word in words)
        {
            builder.Replace(word, mask);
        }

        return builder.ToString();
    }


    /// <summary>
    /// Ensures that a child node exists for a given character.
    /// </summary>
    /// <param name="w">The character to ensure a node for.</param>
    /// <returns>The child node.</returns>
    private WordTreeNode EnsureNode(char w) => (_childNodes ??= []).GetOrAdd(w, _ => new(w));

    /// <summary>
    /// Gets the child node for a given character.
    /// </summary>
    /// <param name="w">The character to get the node for.</param>
    /// <returns>The child node, or null if it does not exist.</returns>
    private WordTreeNode GetNode(char w) => _childNodes?.GetValueSafe(w);
}
