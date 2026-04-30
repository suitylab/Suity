namespace Suity;

/// <summary>
/// Defines an interface for logging runtime messages.
/// </summary>
public interface IRuntimeLog
{
    void AddLog(LogMessageType type, object message);
}

/// <summary>
/// Provides an empty implementation of IRuntimeLog that does nothing.
/// </summary>
public sealed class EmptyRuntimeLog : IRuntimeLog
{
    public static readonly EmptyRuntimeLog Empty = new EmptyRuntimeLog();

    private EmptyRuntimeLog()
    {
    }

    public void AddLog(LogMessageType type, object message)
    {
    }
}