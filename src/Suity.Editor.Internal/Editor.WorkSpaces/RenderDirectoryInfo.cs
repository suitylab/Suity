namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Tracks the rendering status and file change states within a directory.
/// </summary>
internal class RenderDirectoryInfo
{
    /// <summary>
    /// The relative path of the directory.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// Indicates whether this directory contains files that are being rendered.
    /// </summary>
    public bool Rendering;

    /// <summary>
    /// Indicates whether the directory contains files scheduled to be added.
    /// </summary>
    public bool ContainsAddingFiles;

    /// <summary>
    /// Indicates whether the directory contains files scheduled to be removed.
    /// </summary>
    public bool ContainsRemovingFiles;

    /// <summary>
    /// Indicates whether the directory contains files with rendering errors.
    /// </summary>
    public bool ContainsErrorFiles;

    /// <summary>
    /// Indicates whether the directory contains files scheduled to be updated.
    /// </summary>
    public bool ContainsUpdatingFiles;

    /// <summary>
    /// Indicates whether the directory contains externally modified files.
    /// </summary>
    public bool ContainsModifiedFiles;

    /// <summary>
    /// Initializes a new instance for the specified directory path.
    /// </summary>
    /// <param name="path">The relative path of the directory.</param>
    public RenderDirectoryInfo(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Resets all file state flags to false, preserving the path and rendering status.
    /// </summary>
    public void Clear()
    {
        ContainsAddingFiles = false;
        ContainsRemovingFiles = false;
        ContainsErrorFiles = false;
        ContainsUpdatingFiles = false;
        ContainsModifiedFiles = false;
    }
}