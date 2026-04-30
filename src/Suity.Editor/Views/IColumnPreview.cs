namespace Suity.Views;

/// <summary>
/// Represents a column view that supports preview path management.
/// </summary>
public interface IColumnPreview
{
    /// <summary>
    /// Adds a preview path to the column.
    /// </summary>
    /// <param name="path">The preview path to add.</param>
    /// <returns>True if the path was added successfully; otherwise, false.</returns>
    bool AddPreviewPath(PreviewPath path);

    /// <summary>
    /// Removes a preview path from the column.
    /// </summary>
    /// <param name="path">The preview path to remove.</param>
    /// <returns>True if the path was removed successfully; otherwise, false.</returns>
    bool RemovePreviewPath(PreviewPath path);

    /// <summary>
    /// Clears all preview paths from the column.
    /// </summary>
    void ClearPreviewPaths();
}