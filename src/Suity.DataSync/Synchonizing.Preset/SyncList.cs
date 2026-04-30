using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Base class for sync lists
/// </summary>
public abstract class SyncList<T> : ISyncList, IDropInCheck
{
    private readonly List<T> _list = [];

    public List<T> List => _list;

    int ISyncList.Count => _list.Count;

    public event EventHandler<IndexEventArgs<T, int>> Added;

    public event EventHandler<EventArgs<T>> Removed;

    public SyncList()
    {
    }

    public virtual void Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(_list, typeof(T), OnCheckValue, () => OnCreateItem(), OnAdded, OnRemoved);
    }

    protected virtual void OnAdded(T obj, int index)
    {
        Added?.Invoke(this, new IndexEventArgs<T, int>(obj, index));
    }

    protected virtual void OnRemoved(T obj)
    {
        Removed?.Invoke(this, new EventArgs<T>(obj));
    }

    protected abstract T OnCreateItem();

    protected virtual bool OnCheckValue(T obj) => true;

    public virtual bool DropInCheck(object value)
    {
        return value is T;
    }

    public virtual object DropInConvert(object value)
    {
        if (value is T)
        {
            return value;
        }

        return null;
    }
}

public class FactorySyncList<T> : SyncList<T> 
{
    private readonly Func<T> _factoryFunc;
    private readonly Predicate<T> _checkFunc;

    public FactorySyncList(Func<T> factoryFunc, Predicate<T> checkFunc = null)
    {
        _factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
        _checkFunc = checkFunc;
    }

    protected override T OnCreateItem()
    {
        // This method cannot be used as the default creation method, as it is only intended for UI creation. During serialization, it would cause the UI panel to open.

        return _factoryFunc();
    }

    protected override bool OnCheckValue(T obj)
    {
        return _checkFunc?.Invoke(obj) ?? true;
    }
}

public class AutoNewSyncList<T> : FactorySyncList<T> where T : new()
{
    public AutoNewSyncList(Predicate<T> checkFunc = null)
        : base(() => new T(), checkFunc)
    {
    }
}