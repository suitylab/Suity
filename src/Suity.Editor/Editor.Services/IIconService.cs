using Suity.Drawing;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for retrieving icons for files and resources.
/// </summary>
public interface IIconService
{
    /// <summary>
    /// Gets the exact icon for a file path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The icon image, or null if not found.</returns>
    ImageDef GetIconForFileExact(string path);

    /// <summary>
    /// Gets the icon for a file path (may use cache).
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The icon image, or null if not found.</returns>
    ImageDef GetIconForFile(string path);

    /// <summary>
    /// Gets an icon by its ID.
    /// </summary>
    /// <param name="id">The icon ID.</param>
    /// <returns>The icon image, or null if not found.</returns>
    ImageDef GetIconById(Guid id);
}

/// <summary>
/// Empty implementation of the icon service.
/// </summary>
internal sealed class EmptyIconService : IIconService
{
    /// <summary>
    /// Gets the singleton instance of EmptyIconService.
    /// </summary>
    public static readonly EmptyIconService Empty = new EmptyIconService();

    private EmptyIconService()
    {
    }

    /// <inheritdoc/>
    public ImageDef GetIconForFileExact(string path)
    {
        return null;
    }

    /// <inheritdoc/>
    public ImageDef GetIconForFile(string path)
    {
        return null;
    }

    /// <inheritdoc/>
    public ImageDef GetIconById(Guid id)
    {
        return null;
    }
}