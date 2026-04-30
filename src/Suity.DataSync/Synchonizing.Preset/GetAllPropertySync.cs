using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IPropertySync that gets all property values at once
/// </summary>
public class GetAllPropertySync : MarshalByRefObject, IPropertySync
{
    private readonly SyncIntent _intent;
    private readonly bool _ignoreDefaultValue;

    public Dictionary<string, SyncValueInfo> Values { get; } = [];

    public GetAllPropertySync(bool ignoreDefaultValue)
    {
        _intent = SyncIntent.Serialize;
        _ignoreDefaultValue = ignoreDefaultValue;
    }

    public GetAllPropertySync(SyncIntent intent, bool ignoreDefaultValue)
    {
        _intent = intent;
        _ignoreDefaultValue = ignoreDefaultValue;
    }

    public SyncMode Mode => SyncMode.GetAll;
    public SyncIntent Intent => _intent;
    public string Name => null;
    public IEnumerable<string> Names => Values.Keys;
    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if ((flag & SyncFlag.AttributeMode) == SyncFlag.AttributeMode)
        {
            name = "@" + name;
        }

        if (_ignoreDefaultValue && object.Equals(obj, defaultValue))
        {
            //Do nothing
        }
        else
        {
            Values[name] = new SyncValueInfo(typeof(T), obj, flag);
        }

        return obj;
    }
}