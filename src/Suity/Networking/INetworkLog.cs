namespace Suity.Networking;

/// <summary>
/// Defines an interface for logging network events.
/// </summary>
public interface INetworkLog
{
    void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message);
}

/// <summary>
/// Provides an empty implementation of INetworkLog that does nothing.
/// </summary>
public sealed class EmptyNetworkLog : INetworkLog
{
    public static readonly EmptyNetworkLog Empty = new EmptyNetworkLog();

    private EmptyNetworkLog()
    { }

    public void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message)
    {
    }
}