using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Selecting;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for SObject (structured object) operations.
/// </summary>
public interface ISObjectService
{
    /// <summary>
    /// Gets a shortcut selection list for a specific type.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="parent">The parent SObject.</param>
    /// <param name="filter">The asset filter.</param>
    /// <returns>A selection list for the specified type.</returns>
    ISelectionList GetShortcutSelectionList(TypeDefinition type, SObject parent, IAssetFilter filter);
}