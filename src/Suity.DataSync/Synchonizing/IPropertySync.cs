using System.Collections.Generic;

namespace Suity.Synchonizing;

/// <summary>
/// Interface for property synchronization operations
/// </summary>
public interface IPropertySync
{
    SyncMode Mode { get; }
    SyncIntent Intent { get; }
    string Name { get; }
    IEnumerable<string> Names { get; }
    object Value { get; }

    T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null);
}