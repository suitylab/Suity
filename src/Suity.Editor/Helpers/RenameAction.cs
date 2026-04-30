namespace Suity.Helpers;

/// <summary>
/// Represents a single file rename operation, containing both the new and original file names.
/// </summary>
public struct RenameItem
{
    /// <summary>
    /// Gets the new file name after the rename operation.
    /// </summary>
    public string FileName;

    /// <summary>
    /// Gets the original file name before the rename operation.
    /// </summary>
    public string OldFileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameItem"/> struct with the specified file names.
    /// </summary>
    /// <param name="fileName">The new file name after renaming.</param>
    /// <param name="oldFileName">The original file name before renaming.</param>
    public RenameItem(string fileName, string oldFileName)
    {
        FileName = fileName;
        OldFileName = oldFileName;
    }

    /// <summary>
    /// Returns a string representation of this rename operation in the format "OldFileName -> FileName".
    /// </summary>
    /// <returns>A string showing the transition from the old file name to the new file name.</returns>
    public override string ToString()
    {
        return OldFileName + " -> " + FileName;
    }
}

/// <summary>
/// Represents a delegate that performs a rename operation and returns an array of <see cref="RenameItem"/> objects describing the changes made.
/// </summary>
/// <returns>An array of <see cref="RenameItem"/> instances representing all file rename operations performed.</returns>
public delegate RenameItem[] RenameAction();