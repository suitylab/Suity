using System;

namespace Suity.Helpers;

/// <summary>
/// Provides utilities for synchronizing UTC time between local and remote systems.
/// </summary>
public class RemoteUtcDateTime
{
    private DateTime _remoteBegin;
    private DateTime _localBegin;

    public RemoteUtcDateTime()
    {
        _remoteBegin = _localBegin = DateTime.UtcNow;
    }

    /// <summary>
    /// Remote initialization time
    /// </summary>
    public DateTime RemoteBegin => _remoteBegin;

    /// <summary>
    /// Client initialization time
    /// </summary>
    public DateTime LocalBegin => _localBegin;

    /// <summary>
    /// Remote current time
    /// </summary>
    public DateTime RemoteNow => _remoteBegin + (DateTime.UtcNow - _localBegin);

    /// <summary>
    /// Update remote current time
    /// </summary>
    /// <param name="remoteNow"></param>
    public void UpdateRemoteNow(DateTime remoteNow)
    {
        _remoteBegin = remoteNow;
        _localBegin = DateTime.UtcNow;
    }

    /// <summary>
    /// Update remote current time
    /// </summary>
    /// <param name="remoteNow">Remote current time</param>
    /// <param name="timeSpanPingPong">The time difference between the request and the result calculated by the client. The remote time will be increased by half of this value as the reference delay value.</param>
    public void UpdateRemoteNow(DateTime remoteNow, TimeSpan timeSpanPingPong)
    {
        UpdateRemoteNow(remoteNow, timeSpanPingPong.TotalSeconds);
    }

    /// <summary>
    /// Update remote current time
    /// </summary>
    /// <param name="remoteNow"></param>
    /// <param name="timeSpanPingPongSec">The time difference between the request and the result calculated by the client. The remote time will be increased by half of this value as the reference delay value.</param>
    public void UpdateRemoteNow(DateTime remoteNow, double timeSpanPingPongSec)
    {
        _localBegin = DateTime.UtcNow;

        var latency = TimeSpan.FromSeconds(timeSpanPingPongSec * 0.5f);

        _remoteBegin = remoteNow + latency;
    }

    /// <summary>
    /// Get remote time through local time
    /// </summary>
    /// <param name="localTime">Local time</param>
    /// <returns>Returns the remote time</returns>
    public DateTime GetRemoteTime(DateTime localTime)
    {
        return RemoteBegin + (localTime - _localBegin);
    }

    /// <summary>
    /// Get local time from remote time
    /// </summary>
    /// <param name="remoteTime">Remote time</param>
    /// <returns>Returns local time</returns>
    public DateTime GetLocalTime(DateTime remoteTime)
    {
        return _localBegin + (remoteTime - _remoteBegin);
    }

    /// <summary>
    /// Get remote future timestamp
    /// </summary>
    /// <param name="restSeconds">Remaining seconds</param>
    /// <returns></returns>
    public DateTime GetRemoteFuture(TimeSpan timeSpan)
    {
        return RemoteNow + timeSpan;
    }

    /// <summary>
    /// Get the remaining time
    /// </summary>
    /// <param name="remoteStartTime">Remote start timestamp</param>
    /// <param name="timeSpan">Total time period</param>
    /// <returns></returns>
    public TimeSpan GetRestTimeSpan(DateTime remoteStartTime, TimeSpan timeSpan)
    {
        DateTime finishTime = remoteStartTime + timeSpan;
        return finishTime - RemoteNow;
    }

    /// <summary>
    /// Get the remaining time
    /// </summary>
    /// <param name="remoteFinishTime">Remote completion timestamp</param>
    /// <returns></returns>
    public TimeSpan GetRestTimeSpan(DateTime remoteFinishTime)
    {
        return remoteFinishTime - RemoteNow;
    }
}