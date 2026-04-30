using Suity.Synchonizing;

namespace Suity.Editor.Expressions;

/// <summary>
/// Represents an import item for namespaces or types.
/// </summary>
public class ImportItem : ISyncObject
{
    /// <summary>
    /// The namespace to import.
    /// </summary>
    public string NameSpace;

    /// <summary>
    /// The name to import.
    /// </summary>
    public string Name;

    /// <summary>
    /// Synchronizes the properties with the given sync object.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        NameSpace = sync.Sync("nameSpace", NameSpace, SyncFlag.AttributeMode);
        Name = sync.Sync("name", Name, SyncFlag.AttributeMode);
    }
}