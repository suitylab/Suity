using System.IO;

namespace Suity.Helpers;

/// <summary>
/// Represents a file with its file path and associated format name.
/// </summary>
public class CommonFileInfo
{
    private readonly string _filePath;
    private readonly string _formatName;

    /// <summary>
    /// Gets the normalized file path with backslash separators.
    /// </summary>
    public string FilePath => _filePath;

    /// <summary>
    /// Gets the format name associated with the file.
    /// </summary>
    public string FormatName => _formatName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFileInfo"/> class with default values.
    /// </summary>
    public CommonFileInfo()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFileInfo"/> class with the specified path and format name.
    /// </summary>
    /// <param name="path">The file path, which will be normalized to use backslash separators.</param>
    /// <param name="formatName">The format name associated with the file.</param>
    public CommonFileInfo(string path, string formatName)
    {
        _filePath = path.Replace('/', '\\');
        _formatName = formatName;
    }

    /// <summary>
    /// Returns the file name portion of the file path.
    /// </summary>
    /// <returns>The file name extracted from the file path.</returns>
    public override string ToString() => Path.GetFileName(FilePath);
}