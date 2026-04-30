using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Extension methods for ISyncList index operations
/// </summary>
public static class SingleIndexSyncExtensions
{
    public static Type GetElementType(this ISyncList list)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateElementTypeGetter();
        list.Sync(sync, SyncContext.Empty);

        return sync.Value as Type;
    }

    public static object GetItem(this ISyncList list, int index)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateGetter(index);
        list.Sync(sync, SyncContext.Empty);

        return sync.Value;
    }

    public static void SetItem(this ISyncList list, int index, object value)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateSetter(index, value);
        list.Sync(sync, SyncContext.Empty);
    }

    public static object CreateNewItem(this ISyncList list, string parameter = null)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateActivator(parameter);
        list.Sync(sync, SyncContext.Empty);

        return sync.Value;
    }

    public static void Add(this ISyncList list, object value)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateInserter(list.Count, value);
        list.Sync(sync, SyncContext.Empty);
    }

    public static void Insert(this ISyncList list, int index, object value)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateInserter(index, value);
        list.Sync(sync, SyncContext.Empty);
    }

    public static void RemoveAt(this ISyncList list, int index)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var sync = SingleIndexSync.CreateRemover(index);
        list.Sync(sync, SyncContext.Empty);
    }
}