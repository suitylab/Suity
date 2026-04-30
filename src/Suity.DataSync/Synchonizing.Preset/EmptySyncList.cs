namespace Suity.Synchonizing.Preset;

/// <summary>
/// Empty implementation of ISyncList
/// </summary>
public sealed class EmptySyncList : ISyncList
{
    public static readonly EmptySyncList Empty = new();

    private EmptySyncList()
    {
    }

    public int Count => 0;

    public void Sync(IIndexSync sync, ISyncContext context)
    {
    }
}