namespace Suity.Synchonizing.Preset;

/// <summary>
/// Empty implementation of ISyncObject
/// </summary>
public sealed class EmptySyncObject : ISyncObject
{
    public static readonly EmptySyncObject Empty = new();

    private EmptySyncObject()
    {
    }

    public void Sync(IPropertySync sync, ISyncContext context)
    {
    }
}