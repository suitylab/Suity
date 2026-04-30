using System;

namespace Suity.Synchonizing.Core;

public abstract class SyncObjectProxy : ISyncObject
{
    public object Target { get; internal set; }

    public T TargetAs<T>() where T : class
    {
        return (T)Target;
    }

    public virtual object CreateNew()
    {
        return null;
    }

    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    public virtual SyncObjectProxy Clone()
    {
        return (SyncObjectProxy)Activator.CreateInstance(this.GetType());
    }
}