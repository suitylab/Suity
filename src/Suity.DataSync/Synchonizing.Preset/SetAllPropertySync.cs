using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IPropertySync that sets all property values at once
/// </summary>
public class SetAllPropertySync : MarshalByRefObject, IPropertySync
{
    private readonly SyncIntent _intent;
    private readonly Dictionary<string, object> _values;

    public SetAllPropertySync(Dictionary<string, object> values)
    {
        _intent = SyncIntent.Serialize;
        _values = values ?? [];
    }

    public SetAllPropertySync(SyncIntent intent, Dictionary<string, object> values)
    {
        _intent = intent;
        _values = values ?? [];
    }

    public SyncMode Mode => SyncMode.SetAll;
    public SyncIntent Intent => _intent;
    public string Name => null;
    public IEnumerable<string> Names => _values.Keys;
    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if ((flag & SyncFlag.AttributeMode) == SyncFlag.AttributeMode)
        {
            name = "@" + name;
        }

        if (_values.TryGetValue(name, out var value))
        {
            if (value is T tValue)
            {
                return tValue;
            }
        }

        return obj;
    }
}