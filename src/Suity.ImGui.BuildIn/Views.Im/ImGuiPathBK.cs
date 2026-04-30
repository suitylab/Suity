using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Views.Im;

/// <summary>
/// Internal implementation of <see cref="ImGuiPath"/> representing a hierarchical path of string segments.
/// </summary>
internal class ImGuiPathBK : ImGuiPath
{
    [ThreadStatic]
    private StringBuilder? _builder;

    internal readonly string[] _path;
    private string? _cachedString;

    /// <summary>
    /// Initializes a new path with a single segment.
    /// </summary>
    /// <param name="pathItem">The path segment.</param>
    public ImGuiPathBK(string pathItem)
    {
        _path = [ParseStr(pathItem)];
    }

    /// <summary>
    /// Initializes a new path with multiple segments.
    /// </summary>
    /// <param name="pathItems">The path segments.</param>
    public ImGuiPathBK(params string[] pathItems)
    {
        _path = [.. pathItems];
    }

    private ImGuiPathBK()
    {
        _path = [];
    }

    internal ImGuiPathBK(Stack<string> stack)
    {
        string[] path = [.. stack];
        _path = new string[path.Length];

        for (int i = _path.Length - 1; i >= 0; i--)
        {
            _path[i] = path[_path.Length - i - 1];
        }
    }

    internal ImGuiPathBK(IEnumerable<string> pathItems)
    {
        _path = [.. pathItems];
    }

    /// <inheritdoc/>
    public override int Length => _path.Length;

    /// <inheritdoc/>
    public override bool IsEmpty => _path.Length == 0;

    /// <inheritdoc/>
    public override string this[int index] => _path[index];

    /// <inheritdoc/>
    public override string? GetStringAt(int index)
    {
        if (index < 0 || index >= _path.Length)
        {
            return null;
        }

        var obj = _path[index];
        if (obj is string str)
        {
            return str;
        }

        _builder ??= new StringBuilder();
        _builder.Clear();

        _builder.Append(obj);

        return _builder.ToString();
    }

    /// <inheritdoc/>
    public override ImGuiPath SubPath(int index, int length)
    {
        if (length == 0)
        {
            if (index < 0 || index >= _path.Length)
            {
                throw new IndexOutOfRangeException();
            }

            return Empty;
        }

        string[] subPath = new string[length];

        for (int i = 0; i < length; i++)
        {
            subPath[i] = _path[i + index];
        }

        return new ImGuiPathBK(subPath);
    }

    /// <inheritdoc/>
    public override ImGuiPath Append(string pathItem)
    {
        string obj = ParseStr(pathItem);
        if (obj != null)
        {
            string[] path = new string[_path.Length + 1];
            Array.Copy(_path, path, _path.Length);
            path[^1] = obj;

            return new ImGuiPathBK(path);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc/>
    public override ImGuiPath Prepend(string pathItem)
    {
        string obj = ParseStr(pathItem);
        if (obj != null)
        {
            string[] path = new string[_path.Length + 1];
            Array.Copy(_path, 0, path, 1, _path.Length);
            path[0] = obj;

            return new ImGuiPathBK(path);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc/>
    public override ImGuiPath RemoveFirst()
    {
        if (_path.Length == 0)
        {
            return this;
        }

        string[] path = new string[_path.Length - 1];
        Array.Copy(_path, 1, path, 0, path.Length);

        return new ImGuiPathBK(path);
    }

    /// <inheritdoc/>
    public override ImGuiPath RemoveLast()
    {
        if (_path.Length == 0)
        {
            return this;
        }

        string[] path = new string[_path.Length - 1];
        Array.Copy(_path, 0, path, 0, path.Length);

        return new ImGuiPathBK(path);
    }

    /// <inheritdoc/>
    public override ImGuiPath EditPath(int index, string pathItem)
    {
        string[] path = new string[_path.Length];
        Array.Copy(_path, path, _path.Length);
        path[index] = pathItem;

        return new ImGuiPathBK(path);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_cachedString is { } str)
        {
            return str;
        }

        if (_path.Length == 0)
        {
            return string.Empty;
        }

        _builder ??= new StringBuilder();
        _builder.Clear();

        for (int i = 0; i < _path.Length; i++)
        {
            if (i > 0)
            {
                _builder.Append('>');
            }

            _builder.Append(_path[i]);
        }

        _cachedString = _builder.ToString();
        return _cachedString;
    }

    /// <inheritdoc/>
    public override bool Equals(ImGuiPath? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Object.Equals(other, null))
        {
            return false;
        }

        ImGuiPathBK bOther = (ImGuiPathBK)other;

        if (_path.Length != bOther._path.Length)
        {
            return false;
        }

        for (int i = 0; i < _path.Length; i++)
        {
            if (!Object.Equals(_path[i], bOther._path[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int CompareTo(ImGuiPath other)
    {
        if (object.Equals(other, null))
        {
            return -1;
        }

        ImGuiPathBK bOther = (ImGuiPathBK)other;

        for (int i = 0; i < _path.Length; i++)
        {
            string v1 = _path[i];
            string v2 = bOther._path.GetArrayItemSafe(i);
            if (v2 == null)
            {
                return 1;
            }

            int v = v1.CompareTo(v2);
            if (v != 0)
            {
                return v;
            }
        }

        if (bOther._path.Length > _path.Length)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    /// <inheritdoc/>
    public override bool Match(int index, ImGuiPath other)
    {
        if (other.IsEmpty)
        {
            return false;
        }

        ImGuiPathBK bOther = (ImGuiPathBK)other;

        if (_path.Length < bOther.Length + index)
        {
            return false;
        }

        for (int i = 0; i < bOther._path.Length; i++)
        {
            if (!Object.Equals(_path[index + i], bOther._path[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Match(int index, string pathItem)
    {
        if (index >= _path.Length)
        {
            return false;
        }

        string obj = ParseStr(pathItem);
        if (obj != null)
        {
            return Object.Equals(obj, _path[index]);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc/>
    protected override int _GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < _path.Length; i++)
        {
            hash ^= _path[i].GetHashCode();
        }

        return hash;
    }

    /// <summary>
    /// Validates a path segment string, ensuring it does not contain the '>' delimiter.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>The validated string, or empty string if null.</returns>
    internal static string ParseStr(string str)
    {
        if (str == null)
        {
            return string.Empty;
        }

        if (str.Contains('>'))
        {
            throw new InvalidOperationException("String contains '>'");
        }

        return str;
    }
}