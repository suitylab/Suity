namespace Suity.Synchonizing;

/// <summary>
/// Interface for index-based list synchronization operations
/// </summary>
public interface IIndexSync
{
    SyncMode Mode { get; }
    SyncIntent Intent { get; }
    int Count { get; }
    int Index { get; }
    object Value { get; }

    T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None);

    string SyncAttribute(string name, string value);
}