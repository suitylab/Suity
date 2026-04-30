namespace Suity;

/// <summary>
/// Defines an interface for logging operations.
/// </summary>
public interface IOperationLog
{
    void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful);
}

/// <summary>
/// Provides an empty implementation of IOperationLog that does nothing.
/// </summary>
public sealed class EmptyOperationLog : IOperationLog
{
    public static readonly EmptyOperationLog Empty = new EmptyOperationLog();

    private EmptyOperationLog()
    {
    }

    public void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful)
    {
    }
}