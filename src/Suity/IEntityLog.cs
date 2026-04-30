namespace Suity;

/// <summary>
/// Defines an interface for logging entity actions.
/// </summary>
public interface IEntityLog
{
    void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object value);
}

/// <summary>
/// Provides an empty implementation of IEntityLog that does nothing.
/// </summary>
public sealed class EmptyEntityLog : IEntityLog
{
    public static readonly EmptyEntityLog Empty = new EmptyEntityLog();

    private EmptyEntityLog()
    {
    }

    public void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object value)
    {
    }
}