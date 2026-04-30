using System;

namespace Suity.Synchonizing;

/// <summary>
/// Context for synchronization operations
/// </summary>
public interface ISyncContext : IServiceProvider
{
    object Parent { get; }
}