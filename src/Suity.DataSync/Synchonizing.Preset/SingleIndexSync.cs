using Suity.Synchonizing.Core;
using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IIndexSync for single index read/write operations
/// </summary>
public sealed class SingleIndexSync : MarshalByRefObject, IIndexSync
{
    private readonly SyncMode _mode;
    private readonly int _index;
    private object _value;

    private SingleIndexSync(SyncMode mode, int index, object value)
    {
        _mode = mode;
        _index = index;
        _value = value;
    }

    public SyncMode Mode => _mode;
    public SyncIntent Intent => SyncIntent.View;

    public int Index => _index;
    public object Value => _value;
    public int Count => 0;

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        switch (_mode)
        {
            case SyncMode.RequestElementType:
                _value = obj;
                return obj;

            case SyncMode.Get:
                if (index == _index)
                {
                    _value = obj;
                }
                return obj;

            case SyncMode.Set:
            case SyncMode.Insert:
                if (index == _index)
                {
                    if ((flag & SyncFlag.GetOnly) == SyncFlag.GetOnly)
                    {
                        if (_value != null && obj != null)
                        {
                            Cloner.CloneProperty((T)_value, obj);
                        }
                        return obj;
                    }
                    else
                    {
                        T result = _value is T tValue ? tValue : default;
                        if (result == null && (flag & SyncFlag.NotNull) == SyncFlag.NotNull)
                        {
                            result = obj;
                        }
                        return result;
                    }
                }
                else
                {
                    return obj;
                }
            case SyncMode.CreateNew:
                _value = obj;
                return obj;

            default:
                return obj;
        }
    }

    public string SyncAttribute(string name, string value) => value;

    public static SingleIndexSync CreateElementTypeGetter() => new(SyncMode.RequestElementType, 0, null);

    /// <summary>
    /// Create read synchronization
    /// </summary>
    /// <param name="index"></param>
    public static SingleIndexSync CreateGetter(int index) => new(SyncMode.Get, index, null);

    /// <summary>
    /// Create write synchronization
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value">Value to be set</param>
    public static SingleIndexSync CreateSetter(int index, object value) => new(SyncMode.Set, index, value);

    public static SingleIndexSync CreateInserter(int index, object value) => new(SyncMode.Insert, index, value);

    public static SingleIndexSync CreateRemover(int index) => new(SyncMode.RemoveAt, index, null);

    public static SingleIndexSync CreateActivator(string parameter) => new(SyncMode.CreateNew, 0, parameter);
}