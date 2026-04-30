namespace Suity.Synchonizing;

/// <summary>
/// Interface for lists that support synchronization
/// </summary>
public interface ISyncList
{
    int Count { get; }

    void Sync(IIndexSync sync, ISyncContext context);
}