using Suity.Selecting;

namespace Suity.Editor.Selecting;

/// <summary>
/// Provides selection lists for assets.
/// </summary>
public interface ISelectionListProvider
{
    /// <summary>
    /// Gets a selection list for the specified asset type.
    /// </summary>
    /// <typeparam name="T">The type of asset.</typeparam>
    /// <param name="filter">The asset filter.</param>
    /// <returns>The selection list.</returns>
    ISelectionList GetSelectionList<T>(IAssetFilter filter) where T : class;
}