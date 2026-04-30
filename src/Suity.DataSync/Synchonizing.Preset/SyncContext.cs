using Suity.Synchonizing.Core;
using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Default implementation of ISyncContext
/// </summary>
public class SyncContext : MarshalByRefObject, ISyncContext
{
    public static readonly SyncContext Empty = new();

    private readonly object _parent;
    private readonly ISyncTypeResolver _resolver;
    private readonly IServiceProvider _provider;

    public object Parent => _parent;

    public ISyncTypeResolver Resolver => _resolver;

    public IServiceProvider Provider => _provider;

    private SyncContext()
    {
    }

    public SyncContext(object parent)
    {
        _parent = parent;
    }

    public SyncContext(object parent, ISyncTypeResolver resolver, IServiceProvider provider)
    {
        _parent = parent;
        _resolver = resolver;
        _provider = provider;
    }

    public object GetService(Type serviceType)
    {
        return _provider?.GetService(serviceType);
    }

    // TODO: Can the Parent mechanism be cancelled?
    internal SyncContext CreateNew(object newParent)
    {
        return new SyncContext(newParent, _resolver, _provider);
    }
}