using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IPropertySync that gets all field names
/// </summary>
public class GetFieldsPropertySync(SyncIntent intent = SyncIntent.Serialize) : MarshalByRefObject, IPropertySync
{
    public readonly HashSet<string> Fields = [];

    public SyncMode Mode => SyncMode.GetAll;

    public SyncIntent Intent { get; } = intent;

    public string Name => null;

    public IEnumerable<string> Names => Fields;

    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if ((flag & SyncFlag.AttributeMode) != SyncFlag.AttributeMode)
        {
            Fields.Add(name);
        }

        return obj;
    }
}