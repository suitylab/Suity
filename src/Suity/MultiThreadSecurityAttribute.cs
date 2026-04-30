using System;

namespace Suity;

/// <summary>
/// Multithread-safe mode
/// </summary>
public enum MultiThreadSecurityMethods
{
    /// <summary>
    /// Thread safety through locking
    /// </summary>
    LockedSecure,

    /// <summary>
    /// Thread safety through concurrency
    /// </summary>
    ConcurrentSecure,

    /// <summary>
    /// Thread safety via per-thread cache
    /// </summary>
    PerThreadSecure,

    /// <summary>
    /// Thread safety is achieved by initializing the write and then fully reading
    /// </summary>
    ReadonlySecure,

    /// <summary>
    /// Accessed by a dedicated thread, other threads will fail to access
    /// </summary>
    LimitedInOneThread,

    /// <summary>
    /// Thread unsafe
    /// </summary>
    Insecure,
}

/// <summary>
/// Marking the multithreading safety model
/// </summary>
[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class MultiThreadSecurityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the MultiThreadSecurityAttribute class.
    /// </summary>
    /// <param name="medhod">The security method.</param>
    public MultiThreadSecurityAttribute(MultiThreadSecurityMethods medhod)
    {
        SecurityMethod = medhod;
    }

    /// <summary>
    /// Security method
    /// </summary>
    public MultiThreadSecurityMethods SecurityMethod { get; }
}