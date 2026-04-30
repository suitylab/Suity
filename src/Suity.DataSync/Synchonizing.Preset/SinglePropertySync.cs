using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IPropertySync for single property read/write operations
/// </summary>
public sealed class SinglePropertySync : MarshalByRefObject, IPropertySync
{
    private readonly SyncMode _mode;
    private readonly string _name;
    private object _value;
    private Type _baseType;
    private SyncFlag _flag;

    private SinglePropertySync(SyncMode mode, string name, object value)
    {
        _mode = mode;
        _name = name;
        _value = value;
    }

    public SyncMode Mode => _mode;
    public SyncIntent Intent => SyncIntent.View;

    public string Name => _name;

    public IEnumerable<string> Names { get { yield return _name; } }

    public object Value => _value;
    public Type ValueBaseType => _baseType;
    public SyncFlag Flag => _flag;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if (flag.HasFlag(SyncFlag.AttributeMode))
        {
            name = "@" + name;
        }

        if (_mode == SyncMode.Get)
        {
            if (name == _name)
            {
                _value = obj;
                _baseType = typeof(T);
                _flag = flag;
                //string null protection
                if (_baseType == typeof(string) && _value == null)
                {
                    _value = string.Empty;
                }
            }

            return obj;
        }
        else if (_mode == SyncMode.Set)
        {
            if (name == _name)
            {
                _baseType = typeof(T);
                _flag = flag;

                if (flag.HasFlag(SyncFlag.GetOnly))
                {
                    if (_value != null && _value is T tValue)
                    {
                        Cloner.CloneProperty(tValue, obj);

                        return obj;
                    }
                    else
                    {
                        return default;
                    }
                }
                else
                {
                    T result = _value is T tValue ? tValue : default;
                    if (result == null && flag.HasFlag(SyncFlag.NotNull))
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
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Create read synchronization
    /// </summary>
    /// <param name="name">Property name</param>
    public static SinglePropertySync CreateGetter(string name) => new(SyncMode.Get, name, null);

    /// <summary>
    /// Create write synchronization
    /// </summary>
    /// <param name="name">Property name</param>
    /// <param name="value">Value to be set</param>
    public static SinglePropertySync CreateSetter(string name, object value) => new(SyncMode.Set, name, value);
}