using System;
using System.Drawing;

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
    Image GetIconForFileExact(string path);

    /// <summary>
    /// Gets the icon for a file path (may use cache).
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The icon image, or null if not found.</returns>
    Image GetIconForFile(string path);

    /// <summary>
    /// Gets an icon by its ID.
    /// </summary>
    /// <param name="id">The icon ID.</param>
    /// <returns>The icon image, or null if not found.</returns>
    Image GetIconById(Guid id);
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
    public Image GetIconForFileExact(string path)
    {
        return null;
    }

    /// <inheritdoc/>
    public Image GetIconForFile(string path)
    {
        return null;
    }

    /// <inheritdoc/>
    public Image GetIconById(Guid id)
    {
        return null;
    }
}