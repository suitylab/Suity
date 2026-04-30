using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IIndexSync that gets all index values at once
/// </summary>
public class GetAllIndexSync : MarshalByRefObject, IIndexSync
{
    public readonly List<SyncValueInfo> Values = [];
    public readonly Dictionary<string, string> Attributes = new();

    private readonly SyncIntent _intent;

    public GetAllIndexSync()
    {
        _intent = SyncIntent.Serialize;
    }

    public GetAllIndexSync(SyncIntent intent)
    {
        _intent = intent;
    }

    public SyncMode Mode => SyncMode.GetAll;
    public SyncIntent Intent => _intent;
    public int Count => Values.Count;
    public int Index => -1;
    public object Value { get; private set; }

    public int SyncCount(int count)
    {
        Value = count;

        return count;
    }

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        while (Values.Count <= index)
        {
            Values.Add(null);
        }

        Values[index] = new SyncValueInfo(typeof(T), obj, flag);

        return obj;
    }

    public string SyncAttribute(string name, string value)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Attributes[name] = value;
        }

        return value;
    }
}