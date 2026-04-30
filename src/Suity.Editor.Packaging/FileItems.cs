namespace Suity.Editor.Packaging;

/// <summary>
/// Represents a render file with its associated language information used during package import.
/// </summary>
internal class RenderFile
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName;

    /// <summary>
    /// Gets or sets the programming or markup language of the render file.
    /// </summary>
    public string Language;
}

/// <summary>
/// Represents a workspace file entry with location and master status information.
/// </summary>
internal class WorkSpaceFile
{
    /// <summary>
    /// Gets or sets the full relative file name within the workspace.
    /// </summary>
    public string FileName;

    /// <summary>
    /// Gets or sets the local file name relative to the workspace root.
    /// </summary>
    public string LocalFileName;

    /// <summary>
    /// Gets or sets the name of the workspace this file belongs to.
    /// </summary>
    public string WorkSpace;

    /// <summary>
    /// Gets or sets a value indicating whether this file resides in the master directory.
    /// </summary>
    public bool InMaster;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceFile"/> class.
    /// </summary>
    public WorkSpaceFile()
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return FileName;
    }
}
