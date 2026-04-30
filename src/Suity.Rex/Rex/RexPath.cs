using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex;

/// <summary>
/// Represents a single segment in a Rex path, containing either a key or an index.
/// </summary>
public struct PathItem
{
    /// <summary>
    /// Gets an empty <see cref="PathItem"/> instance.
    /// </summary>
    public static readonly PathItem Empty = new(string.Empty, -1);

    /// <summary>
    /// Gets a wildcard <see cref="PathItem"/> instance that matches any segment.
    /// </summary>
    public static readonly PathItem WildCard = new("*", -1);

    /// <summary>
    /// The key name of this path segment. Null if this is an index-based segment.
    /// </summary>
    public readonly string Key;

    /// <summary>
    /// The zero-based index of this path segment. -1 if this is a key-based segment.
    /// </summary>
    public readonly int Index;

    internal PathItem(string key, int index)
    {
        Key = key;
        Index = index;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Key ?? $"[{Index}]";
    }
}

/// <summary>
/// Represents a path used to navigate and identify locations within a reactive data structure.
/// Paths consist of segments that can be either property keys or array indices.
/// </summary>
public class RexPath
{
    /// <summary>
    /// Gets an empty <see cref="RexPath"/> instance.
    /// </summary>
    public static readonly RexPath Empty = new RexPath();

    private readonly string _raw;
    private readonly PathItem[] _items;

    private RexPath()
    {
        _raw = string.Empty;
        _items = [];
    }

    private RexPath(string raw, PathItem[] items)
    {
        _raw = raw;
        _items = items;
    }

    /// <summary>
    /// Initializes a new instance with a single path item.
    /// </summary>
    /// <param name="item">The path item to initialize with.</param>
    public RexPath(PathItem item)
    {
        _raw = item.ToString();
        _items = [item];
    }

    /// <summary>
    /// Initializes a new instance by parsing a path string.
    /// Path segments are separated by dots. Index segments are enclosed in brackets (e.g., "[0]").
    /// </summary>
    /// <param name="path">The path string to parse.</param>
    public RexPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _raw = string.Empty;
            _items = [];

            return;
        }

        _raw = path;

        string[] strs = path.Split('.');
        _items = new PathItem[strs.Length];

        for (int i = 0; i < strs.Length; i++)
        {
            string str = strs[i];

            if (str.StartsWith("[") && str.EndsWith("]"))
            {
                string intStr = str.Substring(1, str.Length - 2);
                if (int.TryParse(intStr, out int value) && value >= 0)
                {
                    _items[i] = new PathItem(null, value);
                }
                else
                {
                    _items[i] = new PathItem(str, -1);
                }
            }
            else
            {
                _items[i] = new PathItem(str, -1);
            }
        }
    }

    /// <summary>
    /// Gets the number of segments in this path.
    /// </summary>
    public int Count => _items.Length;

    /// <summary>
    /// Gets the path item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the path item to get.</param>
    /// <returns>The path item at the specified index.</returns>
    public PathItem this[int index] => _items[index];

    /// <summary>
    /// Gets all path items in this path.
    /// </summary>
    public IEnumerable<PathItem> Items => _items.Select(o => o);

    /// <summary>
    /// Gets the first path item, or <see cref="PathItem.Empty"/> if the path is empty.
    /// </summary>
    public PathItem First => _items.Length > 0 ? _items[0] : PathItem.Empty;

    /// <summary>
    /// Gets the last path item, or <see cref="PathItem.Empty"/> if the path is empty.
    /// </summary>
    public PathItem Last => _items.Length > 0 ? _items[_items.Length - 1] : PathItem.Empty;

    /// <inheritdoc/>
    public override string ToString()
    {
        return _raw;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        var temp = obj as RexPath;
        if (temp is null)
        {
            return false;
        }

        return _raw == temp._raw;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _raw.GetHashCode();
    }

    /// <summary>
    /// Returns a new path with the specified item appended to the end.
    /// </summary>
    /// <param name="item">The path item to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the item appended.</returns>
    public RexPath Append(PathItem item)
    {
        PathItem[] items = new PathItem[_items.Length + 1];
        _items.CopyTo(items, 0);
        items[items.Length - 1] = item;
        string raw = string.IsNullOrEmpty(_raw) ? item.ToString() : _raw + "." + item.ToString();

        return new RexPath(raw, items);
    }

    /// <summary>
    /// Returns a new path with the specified key appended to the end.
    /// </summary>
    /// <param name="key">The key to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the key appended.</returns>
    public RexPath Append(string key)
    {
        return Append(new RexPath(key));
    }

    /// <summary>
    /// Returns a new path with the specified index appended to the end.
    /// </summary>
    /// <param name="index">The index to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the index appended.</returns>
    public RexPath Append(int index)
    {
        return Append(new PathItem(null, index));
    }

    /// <summary>
    /// Returns a new path with the specified path appended to the end.
    /// </summary>
    /// <param name="path">The path to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the path appended.</returns>
    public RexPath Append(RexPath path)
    {
        PathItem[] items = new PathItem[_items.Length + path._items.Length];
        _items.CopyTo(items, 0);
        for (int i = 0; i < path._items.Length; i++)
        {
            items[_items.Length + i] = path._items[i];
        }
        string raw = string.IsNullOrEmpty(_raw) ? path._raw : _raw + "." + path._raw;

        return new RexPath(raw, items);
    }

    /// <summary>
    /// Returns a new path with the specified item prepended to the beginning.
    /// </summary>
    /// <param name="item">The path item to prepend.</param>
    /// <returns>A new <see cref="RexPath"/> with the item prepended.</returns>
    public RexPath Prepend(PathItem item)
    {
        PathItem[] items = new PathItem[_items.Length + 1];
        _items.CopyTo(items, 1);
        items[0] = item;
        string raw = string.IsNullOrEmpty(_raw) ? item.ToString() : item.ToString() + "." + _raw;

        return new RexPath(raw, items);
    }

    /// <summary>
    /// Returns a new path with the specified key prepended to the beginning.
    /// </summary>
    /// <param name="key">The key to prepend.</param>
    /// <returns>A new <see cref="RexPath"/> with the key prepended.</returns>
    public RexPath Prepend(string key)
    {
        return Prepend(new RexPath(key));
    }

    /// <summary>
    /// Returns a new path with the specified index prepended to the beginning.
    /// </summary>
    /// <param name="index">The index to prepend.</param>
    /// <returns>A new <see cref="RexPath"/> with the index prepended.</returns>
    public RexPath Prepend(int index)
    {
        return Prepend(new PathItem(null, index));
    }

    /// <summary>
    /// Returns a new path with the specified path prepended to the beginning.
    /// </summary>
    /// <param name="path">The path to prepend.</param>
    /// <returns>A new <see cref="RexPath"/> with the path prepended.</returns>
    public RexPath Prepend(RexPath path)
    {
        return path.Append(this);
    }

    /// <summary>
    /// Determines whether two <see cref="RexPath"/> instances are equal.
    /// </summary>
    public static bool operator ==(RexPath rec1, RexPath rec2)
    {
        return Equals(rec1, rec2);
    }

    /// <summary>
    /// Determines whether two <see cref="RexPath"/> instances are not equal.
    /// </summary>
    public static bool operator !=(RexPath rec1, RexPath rec2)
    {
        return !Equals(rec1, rec2);
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="RexPath"/>.
    /// </summary>
    /// <param name="path">The path string to convert.</param>
    public static implicit operator RexPath(string path)
    {
        return new RexPath(path);
    }

    /// <summary>
    /// Implicitly converts an integer index to a <see cref="RexPath"/>.
    /// </summary>
    /// <param name="index">The index to convert.</param>
    public static implicit operator RexPath(int index)
    {
        return new RexPath(new PathItem(null, index));
    }

    /// <summary>
    /// Explicitly converts a <see cref="RexPath"/> to its string representation.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    public static explicit operator string(RexPath path)
    {
        return path.ToString();
    }

    /// <summary>
    /// Implicitly converts a <see cref="PathItem"/> to a <see cref="RexPath"/>.
    /// </summary>
    /// <param name="item">The path item to convert.</param>
    public static implicit operator RexPath(PathItem item)
    {
        return new RexPath(item);
    }

    /// <summary>
    /// Appends a path item to the specified path.
    /// </summary>
    /// <param name="s">The path to append to.</param>
    /// <param name="item">The item to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the item appended.</returns>
    public static RexPath operator +(RexPath s, PathItem item)
    {
        s ??= Empty;

        return s.Append(item);
    }

    /// <summary>
    /// Appends a key to the specified path.
    /// </summary>
    /// <param name="s">The path to append to.</param>
    /// <param name="str">The key to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the key appended.</returns>
    public static RexPath operator +(RexPath s, string str)
    {
        s ??= Empty;

        return s.Append(str);
    }

    /// <summary>
    /// Appends an index to the specified path.
    /// </summary>
    /// <param name="s">The path to append to.</param>
    /// <param name="index">The index to append.</param>
    /// <returns>A new <see cref="RexPath"/> with the index appended.</returns>
    public static RexPath operator +(RexPath s, int index)
    {
        s ??= Empty;

        return s.Append(index);
    }
}

/// <summary>
/// Event arguments containing a <see cref="RexPath"/>.
/// </summary>
public class RexPathEventArgs(RexPath path) : EventArgs
{
    /// <summary>
    /// Gets the path associated with this event.
    /// </summary>
    public RexPath Path { get; } = path;
}

/// <summary>
/// Event arguments containing a <see cref="RexPath"/> and a value.
/// </summary>
public class RexPathValueEventArgs(RexPath path, object value) : RexPathEventArgs(path)
{
    /// <summary>
    /// Gets the value associated with this event.
    /// </summary>
    public object Value { get; } = value;
}