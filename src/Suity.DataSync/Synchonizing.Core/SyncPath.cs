using Suity.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Represents a synchronization path that can contain string, int, Guid, or Loid values
/// </summary>
public class SyncPath : IEquatable<SyncPath>, IComparable<SyncPath>, IEnumerable<object>
{
    [ThreadStatic]
    private StringBuilder _builder;

    public static readonly SyncPath Empty = new();

    private readonly object[] _path;

    public SyncPath(string pathItem)
    {
        _path = [ParseStr(pathItem)];
    }

    public SyncPath(params string[] pathItems)
    {
        _path = ParsePath(pathItems);
    }

    public SyncPath(int pathItem)
    {
        _path = [pathItem];
    }

    public SyncPath(Guid pathItem)
    {
        _path = [pathItem];
    }

    public SyncPath(Loid pathItem)
    {
        _path = [pathItem];
    }

    private SyncPath()
    {
        _path = [];
    }

    internal SyncPath(Stack<object> stack)
    {
        object[] path = [.. stack];
        _path = new object[path.Length];

        for (int i = _path.Length - 1; i >= 0; i--)
        {
            _path[i] = path[_path.Length - i - 1];
        }
    }

    internal SyncPath(IEnumerable<object> pathItems)
    {
        _path = pathItems.ToArray();
    }

    private SyncPath(object[] path)
    {
        _path = path;
    }

    public int Length => _path.Length;

    public object this[int index]
    {
        get
        {
            return _path[index];
        }
    }

    public string GetStringAt(int index)
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

        BuildString(_builder, obj);

        return _builder.ToString();
    }

    public int GetIntAt(int index, int defaultValue = -1)
    {
        if (index < 0 || index >= _path.Length)
        {
            return defaultValue;
        }

        if (_path[index] is int v)
        {
            return v;
        }
        else
        {
            return defaultValue;
        }
    }

    public SyncPath SubPath(int index, int length)
    {
        if (length == 0)
        {
            if (index < 0 || index >= _path.Length)
            {
                throw new IndexOutOfRangeException();
            }
            return Empty;
        }

        object[] subPath = new object[length];

        for (int i = 0; i < length; i++)
        {
            subPath[i] = _path[i + index];
        }

        return new SyncPath(subPath);
    }

    public SyncPath Append(string pathItem)
    {
        object obj = ParseStr(pathItem);
        if (obj != null)
        {
            object[] path = new object[_path.Length + 1];
            Array.Copy(_path, path, _path.Length);
            path[path.Length - 1] = obj;

            return new SyncPath(path);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public SyncPath Append(int pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, path, _path.Length);
        path[path.Length - 1] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath Append(Guid pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, path, _path.Length);
        path[path.Length - 1] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath Append(Loid pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, path, _path.Length);
        path[path.Length - 1] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath Prepend(string pathItem)
    {
        object obj = ParseStr(pathItem);
        if (obj != null)
        {
            object[] path = new object[_path.Length + 1];
            Array.Copy(_path, 0, path, 1, _path.Length);
            path[0] = obj;

            return new SyncPath(path);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public SyncPath Prepend(int pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, 0, path, 1, _path.Length);
        path[0] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath Prepend(Guid pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, 0, path, 1, _path.Length);
        path[0] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath Prepend(Loid pathItem)
    {
        object[] path = new object[_path.Length + 1];
        Array.Copy(_path, 0, path, 1, _path.Length);
        path[0] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath RemoveFirst()
    {
        if (_path.Length == 0)
        {
            return this;
        }

        object[] path = new object[_path.Length - 1];
        Array.Copy(_path, 1, path, 0, path.Length);

        return new SyncPath(path);
    }

    public SyncPath RemoveLast()
    {
        if (_path.Length == 0)
        {
            return this;
        }

        object[] path = new object[_path.Length - 1];
        Array.Copy(_path, 0, path, 0, path.Length);

        return new SyncPath(path);
    }

    public SyncPath EditPath(int index, string pathItem)
    {
        object[] path = new object[_path.Length];
        Array.Copy(_path, path, _path.Length);
        path[index] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath EditPath(int index, int pathItem)
    {
        object[] path = new object[_path.Length];
        Array.Copy(_path, path, _path.Length);
        path[index] = pathItem;

        return new SyncPath(path);
    }

    public SyncPath EditPath(int index, Guid pathItem)
    {
        object[] path = new object[_path.Length];
        Array.Copy(_path, path, _path.Length);
        path[index] = pathItem;

        return new SyncPath(path);
    }

    public override string ToString()
    {
        if (_path.Length == 0)
        {
            return string.Empty;
        }

        var builder = _builder ??= new();

        builder.Clear();

        for (int i = 0; i < _path.Length; i++)
        {
            if (i > 0)
            {
                builder.Append('|');
            }

            BuildString(builder, _path[i]);
        }

        var str = builder.ToString();
        builder.Clear();

        return str;
    }

    public string ToString(Func<Guid, string> guidResolve)
    {
        if (_path.Length == 0)
        {
            return string.Empty;
        }

        var builder = _builder ??= new();

        builder.Clear();

        for (int i = 0; i < _path.Length; i++)
        {
            if (i > 0)
            {
                builder.Append('|');
            }

            BuildString(builder, _path[i], guidResolve);
        }

        var str = builder.ToString();
        builder.Clear();

        return str;
    }

    public bool Equals(SyncPath other)
    {
        if (object.Equals(other, null))
        {
            return false;
        }

        if (_path.Length != other._path.Length)
        {
            return false;
        }

        for (int i = 0; i < _path.Length; i++)
        {
            if (!Object.Equals(_path[i], other._path[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int CompareTo(SyncPath other)
    {
        if (object.Equals(other, null))
        {
            return -1;
        }

        for (int i = 0; i < _path.Length; i++)
        {
            object v1 = _path[i];
            object v2 = other._path.GetArrayItemSafe(i);
            if (v2 == null)
            {
                return 1;
            }

            if (v1 is int index1)
            {
                if (v2 is int index2)
                {
                    int v = index1.CompareTo(index2);
                    if (v != 0)
                    {
                        return v;
                    }
                }
                else
                {
                    return -1;
                }
            }
            else if (v1 is string str1)
            {
                if (v2 is string str2)
                {
                    int v = str1.CompareTo(str2);
                    if (v != 0)
                    {
                        return v;
                    }
                }
                else
                {
                    return 1;
                }
            }
        }

        if (other._path.Length > _path.Length)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public bool Match(int index, SyncPath other)
    {
        if (IsNullOrEmpty(other))
        {
            return false;
        }

        if (_path.Length < other.Length + index)
        {
            return false;
        }

        for (int i = 0; i < other.Length; i++)
        {
            if (!Object.Equals(_path[index + i], other._path[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Match(int index, string pathItem)
    {
        if (index >= _path.Length)
        {
            return false;
        }

        object obj = ParseStr(pathItem);
        if (obj != null)
        {
            return Object.Equals(obj, _path[index]);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static bool IsNullOrEmpty(SyncPath path)
    {
        return path == null || path._path.Length == 0;
    }

    public static SyncPath Combine(SyncPath a, SyncPath b)
    {
        if (a == null)
        {
            return b ?? SyncPath.Empty;
        }
        if (b == null)
        {
            return a ?? SyncPath.Empty;
        }

        object[] path = new object[a._path.Length + b._path.Length];
        for (int i = 0; i < a._path.Length; i++)
        {
            path[i] = a._path[i];
        }

        for (int i = 0; i < b._path.Length; i++)
        {
            path[a._path.Length + i] = b._path[i];
        }

        return new SyncPath(path);
    }

    public static SyncPath Create(params string[] path)
    {
        return new SyncPath(path);
    }

    public static SyncPath Create(string pathChain)
    {
        object[] path;

        if (string.IsNullOrEmpty(pathChain))
        {
            path = [];
        }
        else
        {
            string[] split = pathChain.Split('|');
            path = ParsePath(split);
        }

        return new SyncPath(path);
    }

    public static bool TryCreate(string pathChain, out SyncPath path)
    {
        try
        {
            path = Create(pathChain);
            return true;
        }
        catch (Exception)
        {
            path = null;
            return false;
        }
    }

    public static bool TryCreate(IEnumerable<object> pathChain, out SyncPath path)
    {
        object[] paths = pathChain.ToArray();

        foreach (var item in paths)
        {
            if (item is string || item is int || item is Guid || item is Loid)
            {
            }
            else
            {
                path = null;
                return false;
            }
        }

        path = new SyncPath(paths);
        return true;
    }

    private static void BuildString(StringBuilder builder, object obj, Func<Guid, string> guidResolve = null)
    {
        switch (obj)
        {
            case string strValue:
                builder.Append(strValue);
                break;

            case int intValue:
                builder.Append('[');
                builder.Append(intValue);
                builder.Append(']');
                break;

            case Guid guid:
                builder.Append('{');
                if (guidResolve != null)
                {
                    try
                    {
                        builder.Append(guidResolve(guid));
                    }
                    catch (Exception)
                    {
                        builder.Append(guid);
                    }
                }
                else
                {
                    builder.Append(guid);
                }
                builder.Append('}');
                break;

            case Loid loid:
                builder.Append('(');
                builder.Append(loid);
                builder.Append(')');
                break;

            default:
                builder.Append(obj);
                break;
        }
    }

    internal static object[] ParsePath(string[] path)
    {
        if (path == null) throw new ArgumentNullException();

        object[] result = new object[path.Length];

        for (int i = 0; i < path.Length; i++)
        {
            result[i] = ParseStr(path[i]);
        }

        return result;
    }

    internal static object ParseStr(string str)
    {
        if (str == null)
        {
            return string.Empty;
        }

        if (str.Contains('|'))
        {
            throw new InvalidOperationException("String contains '|'");
        }

        if (str.StartsWith("[") && str.EndsWith("]"))
        {
            str = str.Substring(1, str.Length - 2);
            if (!int.TryParse(str, out int index))
            {
                throw new InvalidOperationException("Parse index failed : " + str);
            }

            return index;
        }
        else if (str.StartsWith("{") && str.EndsWith("}"))
        {
            str = str.Substring(1, str.Length - 2);
            if (!Guid.TryParseExact(str, "D", out Guid guid))
            {
                throw new InvalidOperationException("Parse guid failed : " + str);
            }

            return guid;
        }
        else if (str.StartsWith("(") && str.EndsWith(")"))
        {
            str = str.Substring(1, str.Length - 2);
            return new Loid(str);
        }
        else
        {
            return str;
        }
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < _path.Length; i++)
        {
            hash ^= _path[i].GetHashCode();
        }

        return hash;
    }

    public override bool Equals(object obj)
    {
        if (Object.ReferenceEquals(this, obj))
        {
            return true;
        }

        SyncPath other = obj as SyncPath;

        return Equals(other);
    }

    IEnumerator<object> IEnumerable<object>.GetEnumerator() => _path.Pass().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _path.Pass().GetEnumerator();

    public static bool operator ==(SyncPath v1, SyncPath v2)
    {
        if (v1 is null)
        {
            return v2 is null;
        }
        else
        {
            return v1.Equals(v2);
        }
    }

    public static bool operator !=(SyncPath v1, SyncPath v2)
    {
        if (v1 is null)
        {
            return v2 is not null;
        }
        else
        {
            return !v1.Equals(v2);
        }
    }

    public static implicit operator SyncPath(string pathChain)
    {
        return Create(pathChain);
    }
}