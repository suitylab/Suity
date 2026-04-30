using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for file bunch operations (batching multiple files).
/// </summary>
public interface IFileBunchService
{
    /// <summary>
    /// Creates or updates a bunch file.
    /// </summary>
    /// <param name="bunchFiles">The files to include in the bunch.</param>
    /// <param name="bunchFileFullName">The full path to the bunch file.</param>
    /// <returns>True if successful.</returns>
    bool CreateOrUpdate(IEnumerable<FileBunchUpdate> bunchFiles, string bunchFileFullName);

    /// <summary>
    /// Downloads and extracts a bunch file.
    /// </summary>
    /// <param name="bunchFileFullName">The full path to the bunch file.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="replaces">The replacement rules.</param>
    void Download(string bunchFileFullName, string targetDirectory, IEnumerable<FileBunchReplace> replaces);

    /// <summary>
    /// Shrinks a bunch file (removes unnecessary content).
    /// </summary>
    /// <param name="bunchFileFullName">The full path to the bunch file.</param>
    void Shrink(string bunchFileFullName);
}

/// <summary>
/// Represents a file in a file bunch for update.
/// </summary>
public class FileBunchUpdate
{
    /// <summary>
    /// Gets or sets the file ID.
    /// </summary>
    public string FileId { get; set; }

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string FullName { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return FileId;
    }
}

/// <summary>
/// Represents a replacement rule for file bunch operations.
/// </summary>
public class FileBunchReplace
{
    /// <summary>
    /// Gets or sets the ID to replace.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the target code.
    /// </summary>
    public string TargetCode { get; set; }
}