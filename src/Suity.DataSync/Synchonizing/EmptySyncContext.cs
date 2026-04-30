using System;

namespace Suity.Synchonizing;

public class EmptySyncContext : MarshalByRefObject, ISyncContext
{
    public static EmptySyncContext Empty { get; } = new();

    public object Parent => null;

    public object GetService(Type serviceType) => null;
}