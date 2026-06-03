using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

public abstract class SyncList : ISyncList, IDropInCheck
{
    public abstract int Count { get; }

    public abstract bool DropInCheck(object value);
    public abstract object DropInConvert(object value);
    public abstract void Sync(IIndexSync sync, ISyncContext context);

    public static ISyncList CreateReadonly(Array array)
    {
        Type elementType = array.GetType().GetElementType();
        Type syncListType = typeof(ReadonlySyncList<>).MakeGenericType(elementType);

        var list = (ISyncList)Activator.CreateInstance(syncListType, array);
        var sync = new SetAllIndexSync(array);
        list.Sync(sync, SyncContext.Empty);

        return list;
    }
}

/// <summary>
/// Base class for sync lists
/// </summary>
public abstract class SyncList<T> : SyncList
{
    private readonly List<T> _list = [];

    public List<T> List => _list;

    public override int Count => _list.Count;

    public event EventHandler<IndexEventArgs<T, int>> Added;

    public event EventHandler<EventArgs<T>> Removed;

    protected SyncList()
    {
    }

    protected SyncList(IEnumerable<T> values)
    {
        _list.AddRange(values);
    }

    public override void Sync(IIndexSync sync, ISyncContext context)
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

    public override bool DropInCheck(object value)
    {
        return value is T;
    }

    public override object DropInConvert(object value)
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

public class ReadonlySyncList<T> : SyncList<T>
{
    public ReadonlySyncList()
    {
    }

    public ReadonlySyncList(IEnumerable<T> values) : base(values)
    {
    }

    protected override T OnCreateItem()
    {
        return default;
    }
    protected override bool OnCheckValue(T obj) => false;

    public override void Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIListReadOnly(List);
    }
    public override bool DropInCheck(object value) => false;
    public override object DropInConvert(object value) => false;
}