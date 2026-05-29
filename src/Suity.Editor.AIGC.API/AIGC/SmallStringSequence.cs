using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.AIGC;

public class SmallStringSequence
{
    private readonly LinkedList<string> _items = new();
    private readonly int _maxLength;
    private readonly StringBuilder _cachedBuilder = new();

    public SmallStringSequence(int maxLength)
    {
        _maxLength = maxLength < 1 ? 1 : maxLength;
    }

    public void Append(string t)
    {
        if (string.IsNullOrEmpty(t))
        {
            return;
        }

        _items.AddLast(t);

        while (_items.Count > _maxLength)
        {
            _items.RemoveFirst();
        }
    }

    public override string ToString()
    {
        _cachedBuilder.Clear();
        foreach (var item in _items)
        {
            _cachedBuilder.Append(item);
        }
        return _cachedBuilder.ToString();
    }
}