using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Read-only implementation of ISyncList
/// </summary>
public class ReadOnlySyncList<T> : ISyncList
{
    private readonly List<T> _list = [];
    public List<T> List => _list;

    int ISyncList.Count => _list.Count;

    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        if (sync.IsGetter())
        {
            sync.SyncGenericIList(_list, typeof(T));
        }
    }
}