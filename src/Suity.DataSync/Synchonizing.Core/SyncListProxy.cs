using System;

namespace Suity.Synchonizing.Core;

public abstract class SyncListProxy : ISyncList
{
    public abstract int Count { get; }

    public object Target { get; internal set; }

    public virtual object CreateNew()
    {
        return null;
    }

    public virtual void Sync(IIndexSync sync, ISyncContext context)
    {
    }

    public virtual SyncListProxy Clone()
    {
        return (SyncListProxy)Activator.CreateInstance(this.GetType());
    }
}