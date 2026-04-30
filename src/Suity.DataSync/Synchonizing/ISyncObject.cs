namespace Suity.Synchonizing;

/// <summary>
/// Interface for objects that can synchronize their data
/// </summary>
public interface ISyncObject
{
    /// <summary>
    /// Perform synchronization
    /// </summary>
    /// <param name="sync"></param>
    /// <param name="context"></param>
    void Sync(IPropertySync sync, ISyncContext context);
}