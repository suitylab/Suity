using System;

namespace Suity.Rex;

/// <summary>
/// Represents a disposable handle that supports reference counting for subscription management.
/// </summary>
public interface IRexHandle : IDisposable
{
    /// <summary>
    /// Increments the reference count to prevent premature disposal.
    /// </summary>
    /// <returns>The current handle instance.</returns>
    IRexHandle Push();
}