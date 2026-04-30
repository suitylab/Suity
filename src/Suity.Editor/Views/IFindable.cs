using Suity.Synchonizing.Core;

namespace Suity.Views;

/// <summary>
/// Represents an object that can be found by a unique key.
/// </summary>
public interface IFindable
{
    /// <summary>
    /// Gets the key used to find this object.
    /// </summary>
    /// <returns>The finding key.</returns>
    string GetFindingKey();
}


/// <summary>
/// Constrain reference lookup scope
/// </summary>
public interface IFindReferenceScope
{
    bool IncludeChildAssets { get; }

    bool IsInScope(SyncPathReportItem item);
}
