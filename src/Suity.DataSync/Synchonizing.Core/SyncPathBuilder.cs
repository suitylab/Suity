using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Builder class for creating SyncPath objects
/// </summary>
public class SyncPathBuilder
{
    private readonly LinkedList<object> _list = new();

    public SyncPathBuilder()
    {
    }

    public SyncPathBuilder Append(string pathItem)
    {
        _list.AddLast(SyncPath.ParseStr(pathItem));

        return this;
    }

    public SyncPathBuilder Append(int pathItem)
    {
        _list.AddLast(pathItem);

        return this;
    }

    public SyncPathBuilder Append(Guid pathItem)
    {
        _list.AddLast(pathItem);

        return this;
    }

    public SyncPathBuilder Append(Loid pathItem)
    {
        _list.AddLast(pathItem);

        return this;
    }

    public SyncPathBuilder Append(SyncPath path)
    {
        for (int i = 0; i < path.Length; i++)
        {
            _list.AddLast(path[i]);
        }

        return this;
    }

    public SyncPathBuilder Append(object obj)
    {
        switch (obj)
        {
            case string str:
                Append(str);
                break;

            case int index:
                Append(index);
                break;

            case Guid guid:
                Append(guid);
                break;

            case Loid loid:
                Append(loid);
                break;

            case SyncPath path:
                Append(path);
                break;

            default:
                break;
        }

        return this;
    }

    public SyncPathBuilder Prepend(string pathItem)
    {
        _list.AddFirst(SyncPath.ParseStr(pathItem));

        return this;
    }

    public SyncPathBuilder Prepend(int pathItem)
    {
        _list.AddFirst(pathItem);

        return this;
    }

    public SyncPathBuilder Prepend(Guid pathItem)
    {
        _list.AddFirst(pathItem);

        return this;
    }

    public SyncPathBuilder Prepend(Loid pathItem)
    {
        _list.AddFirst(pathItem);

        return this;
    }

    public SyncPathBuilder Prepend(SyncPath path)
    {
        for (int i = path.Length - 1; i >= 0; i--)
        {
            _list.AddFirst(path[i]);
        }

        return this;
    }

    public SyncPathBuilder Prepend(object obj)
    {
        switch (obj)
        {
            case string str:
                Prepend(str);
                break;

            case int index:
                Prepend(index);
                break;

            case Guid guid:
                Prepend(guid);
                break;

            case Loid loid:
                Prepend(loid);
                break;

            case SyncPath path:
                Prepend(path);
                break;

            default:
                break;
        }

        return this;
    }

    public object First => _list.First?.Value;
    public object Last => _list.Last?.Value;

    public SyncPathBuilder RemoveFirst()
    {
        if (_list.Count > 0)
        {
            _list.RemoveFirst();
        }

        return this;
    }

    public SyncPathBuilder RemoveLast()
    {
        if (_list.Count > 0)
        {
            _list.RemoveLast();
        }

        return this;
    }

    public SyncPathBuilder Trim()
    {
        TrimFirst();
        TrimLast();

        return this;
    }

    public SyncPathBuilder TrimFirst()
    {
        while (_list.First?.Value is string s && string.IsNullOrEmpty(s))
        {
            _list.RemoveFirst();
        }

        return this;
    }

    public SyncPathBuilder TrimLast()
    {
        while (_list.Last?.Value is string s && string.IsNullOrEmpty(s))
        {
            _list.RemoveLast();
        }

        return this;
    }

    public SyncPathBuilder Clear()
    {
        _list.Clear();

        return this;
    }

    public SyncPath ToSyncPath()
    {
        if (_list.Count > 0)
        {
            return new SyncPath(_list);
        }
        else
        {
            return SyncPath.Empty;
        }
    }

    public override string ToString()
    {
        return ToSyncPath().ToString();
    }
}