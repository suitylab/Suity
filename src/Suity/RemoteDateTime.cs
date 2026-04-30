using System;

namespace Suity;

/// <summary>
/// Provides utilities for synchronizing time between local and remote systems.
/// </summary>
public class RemoteDateTime
{
    private DateTime _remoteBegin;
    private DateTime _localBegin;

    /// <summary>
    /// Initializes a new instance of the RemoteDateTime class.
    /// Sets both remote and local begin times to the current UTC time.
    /// </summary>
    public RemoteDateTime()
    {
        _remoteBegin = _localBegin = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the RemoteDateTime class with a specified remote time.
    /// </summary>
    /// <param name="remoteNow">The current remote time.</param>
    public RemoteDateTime(DateTime remoteNow)
    {
        _remoteBegin = remoteNow;
        _localBegin = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the RemoteDateTime class with specified remote and local times.
    /// </summary>
    /// <param name="remoteBegin">The remote initialization time.</param>
    /// <param name="localBegin">The local initialization time.</param>
    public RemoteDateTime(DateTime remoteBegin, DateTime localBegin)
    {
        _remoteBegin = remoteBegin;
        _localBegin = localBegin;
    }

    /// <summary>
    /// Server initialization time
    /// </summary>
    public DateTime RemoteBegin => _remoteBegin;

    /// <summary>
    /// Client initialization time
    /// </summary>
    public DateTime LocalBegin => _localBegin;

    /// <summary>
    /// Current time on the server
    /// </summary>
    public DateTime RemoteNow => _remoteBegin + (DateTime.UtcNow - _localBegin);

    /// <summary>
    /// Update the current time on the server
    /// </summary>
    /// <param name="remoteNow"></param>
    public void UpdateRemoteNow(DateTime remoteNow)
    {
        _remoteBegin = remoteNow;
        _localBegin = DateTime.UtcNow;
    }

    /// <summary>
    /// Get the server time through local time
    /// </summary>
    /// <param name="localTime">Local time</param>
    /// <returns>Returns the server time</returns>
    public DateTime GetRemoteTime(DateTime localTime)
    {
        return RemoteBegin + (localTime - _localBegin);
    }

    /// <summary>
    /// Get local time through server time
    /// </summary>
    /// <param name="remoteTime">Server time</param>
    /// <returns>Returns local time</returns>
    public DateTime GetLocalTime(DateTime remoteTime)
    {
        return _localBegin + (remoteTime - _remoteBegin);
    }

    /// <summary>
    /// Get the server's future timestamp
    /// </summary>
    /// <param name="restSeconds">Remaining seconds</param>
    /// <returns></returns>
    public DateTime GetRemoteFuture(int restSeconds)
    {
        return RemoteNow + TimeSpan.FromSeconds(restSeconds);
    }

    /// <summary>
    /// Get the remaining time
    /// </summary>
    /// <param name="remoteStartTime">Server startup timestamp</param>
    /// <param name="seconds">Total time period</param>
    /// <returns></returns>
    public TimeSpan GetRestTimeSpan(DateTime remoteStartTime, int seconds)
    {
        DateTime finishTime = remoteStartTime + TimeSpan.FromSeconds(seconds);

        return finishTime - RemoteNow;
    }

    /// <summary>
    /// Get the remaining time
    /// </summary>
    /// <param name="remoteStartTime">Server startup timestamp</param>
    /// <param name="span">Total time period</param>
    /// <returns></returns>
    public TimeSpan GetRestTimeSpan(DateTime remoteStartTime, TimeSpan span)
    {
        DateTime finishTime = remoteStartTime + span;

        return finishTime - RemoteNow;
    }

    /// <summary>
    /// Get the remaining time
    /// </summary>
    /// <param name="remoteFinishTime">Server completion timestamp</param>
    /// <returns></returns>
    public TimeSpan GetRestTimeSpan(DateTime remoteFinishTime)
    {
        return remoteFinishTime - RemoteNow;
    }
}