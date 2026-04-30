using Suity.Networking;

namespace Suity;

/// <summary>
/// Provides static methods for logging messages, warnings, and errors.
/// </summary>
public static class Logs
{
    public static void LogDebug(object message)
        => Device._current.AddLog(LogMessageType.Debug, message);

    public static void LogInfo(object message)
        => Device._current.AddLog(LogMessageType.Info, message);

    public static void LogWarning(object message)
        => Device._current.AddLog(LogMessageType.Warning, message);

    public static void LogError(object message)
        => Device._current.AddLog(LogMessageType.Error, message);

    public static void AddLog(LogMessageType type, object message)
        => Device._current.AddLog(type, message);

    public static void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message)
        => Device._current.AddNetworkLog(type, direction, sessionId, channelId, message);

    public static void AddResourceLog(string key, string path)
        => Device._current.AddResourceLog(key, path);

    public static void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object value)
        => Device._current.AddEntityLog(roomId, entityId, entityName, actionType, messageType, value);

    public static void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful)
        => Device._current.AddOperationLog(level, category, userId, ip, data, successful);
}