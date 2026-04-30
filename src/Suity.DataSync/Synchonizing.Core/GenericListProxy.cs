using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

internal class GenericListProxy<T> : SyncListProxy
{
    public override int Count => ((IList<T>)Target).Count;

    public override void Sync(IIndexSync sync, ISyncContext context)
    {
        IList<T> list = (IList<T>)Target;
        sync.SyncGenericIList<T>(list);
    }
}