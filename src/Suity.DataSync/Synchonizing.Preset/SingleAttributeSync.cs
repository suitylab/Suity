using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IIndexSync for single attribute read/write operations
/// </summary>
public sealed class SingleAttributeSync : MarshalByRefObject, IIndexSync
{
    private readonly SyncMode _mode;
    private readonly string _name;
    private string _value;

    private SingleAttributeSync(SyncMode mode, string name, string value)
    {
        _mode = mode;
        _name = name;
        _value = value;
    }

    public SyncMode Mode => _mode;
    public SyncIntent Intent => SyncIntent.View;

    public int Count => 0;

    public int Index => 0;

    public object Value => null;

    public int SyncCount(int count)
    {
        return count;
    }

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        return obj;
    }

    public string SyncAttribute(string name, string value)
    {
        if (_mode == SyncMode.Get)
        {
            if (name == _name)
            {
                _value = value;
            }

            return value;
        }
        else if (_mode == SyncMode.Set)
        {
            if (name == _name)
            {
                return _value;
            }
            else
            {
                return value;
            }
        }
        else
        {
            return value;
        }
    }

    /// <summary>
    /// Create read synchronization
    /// </summary>
    /// <param name="index"></param>
    public static SingleAttributeSync CreateGetter(string name) => new(SyncMode.Get, name, null);

    /// <summary>
    /// Create write synchronization
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value">value to be set</param>
    public static SingleAttributeSync CreateSetter(string name, string value) => new(SyncMode.Set, name, value);
}