using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IIndexSync that sets all index values at once
/// </summary>
public class SetAllIndexSync : MarshalByRefObject, IIndexSync
{
    private readonly SyncIntent _intent;
    private readonly object[] _values;

    public SetAllIndexSync(object[] values) : this(SyncIntent.Serialize, values)
    {
    }

    public SetAllIndexSync(Array values) : this(SyncIntent.Serialize, values)
    {
    }

    public SetAllIndexSync(SyncIntent intent, object[] values)
    {
        _intent = intent;
        _values = values ?? [];
    }

    public SetAllIndexSync(SyncIntent intent, Array values)
    {
        _intent = intent;
        _values = values != null ? new object[values.Length] : [];
        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _values[i] = values.GetValue(i);
            }
        }
    }

    public SyncMode Mode => SyncMode.SetAll;
    public SyncIntent Intent => _intent;
    public int Count => _values.Length;
    public int Index => -1;
    public object Value => null;

    public int SyncCount(int count) => count;

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        if (index >= 0 && index < _values.Length && _values[index] is T tValue)
        {
            return tValue;
        }

        return obj;
    }

    public string SyncAttribute(string name, string value) => value;
}