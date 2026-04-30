using Suity.Collections;
using System.Collections.Generic;

namespace Suity.Editor.Helpers;

public class WordTreeExNode<T>
{
    private readonly char _char;
    private bool _isTerminal;
    private T _type;

    private Dictionary<char, WordTreeExNode<T>> _childNodes;

    public string Value { get; private set; }
    public T Type { get; private set; }

    public WordTreeExNode(char word)
    {
        _char = word;
    }

    internal WordTreeExNode()
    {
    }

    internal void Add(string match, string value, T type)
    {
        if (string.IsNullOrEmpty(match))
        {
            return;
        }

        char w = match[0];
        string rest = match[1..];

        WordTreeExNode<T> childNode = EnsureNode(w);
        if (rest.Length > 0)
        {
            childNode.Add(rest, value, type);
        }
        else
        {
            childNode._isTerminal = true;
            childNode.Value = value;
            childNode.Type = type;
        }
    }

    internal WordTreeExNode<T> Match(string str, int index)
    {
        if (str[index] != _char)
        {
            return null;
        }

        if (_isTerminal)
        {
            return this;
        }

        int next = index + 1;
        if (_childNodes != null && next < str.Length && _childNodes.TryGetValue(str[next], out var childNode))
        {
            return childNode.Match(str, next);
        }

        return null;
    }

    internal WordTreeExNode<T> MatchChildren(string str, int index)
    {
        int next = index;
        if (_childNodes != null && next < str.Length && _childNodes.TryGetValue(str[next], out var childNode))
        {
            return childNode.Match(str, next);
        }

        return null;
    }

    private WordTreeExNode<T> EnsureNode(char w)
    {
        return (_childNodes ??= []).GetOrAdd(w, _ => new WordTreeExNode<T>(w));
    }

    private WordTreeExNode<T> GetNode(char w)
    {
        if (_childNodes is null)
        {
            return null;
        }

        return _childNodes.GetValueSafe(w);
    }

    public override string ToString()
    {
        return $"{Value}, {Type}";
    }
}

public class WordTreeExToken<T>
{
    public string Value;
    public T Type;

    public override string ToString()
    {
        return $"{Value}, {Type}";
    }
}

public class WordTreeEx<T>
{
    private readonly WordTreeExNode<T> _root = new();

    public void Add(string value, T type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        _root.Add(value, value, type);
    }

    public WordTreeExNode<T> Match(string str, int index)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }

        return _root.MatchChildren(str, index);
    }

    public List<WordTreeExToken<T>> Tokenize(string str, T commonType)
    {
        List<WordTreeExToken<T>> list = [];

        if (string.IsNullOrEmpty(str))
        {
            return list;
        }

        int i = 0;
        int commonBegin = 0;
        int len = str.Length;

        while (i < len)
        {
            var node = Match(str, i);
            if (node != null)
            {
                if (i > commonBegin)
                {
                    list.Add(new WordTreeExToken<T>
                    {
                        Type = commonType,
                        Value = str[commonBegin..i],
                    });
                }

                list.Add(new WordTreeExToken<T>
                {
                    Type = node.Type,
                    Value = node.Value,
                });
                i += node.Value.Length;
                commonBegin = i;
            }
            else
            {
                i++;
            }
        }

        if (i >= len && commonBegin < len)
        {
            list.Add(new WordTreeExToken<T>
            {
                Type = commonType,
                Value = str[commonBegin..len],
            });
        }

        return list;
    }
}