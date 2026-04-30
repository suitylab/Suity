using Suity.Helpers;
using System;

namespace Suity.Views.PathTree;

/// <summary>
/// Provides event arguments for a user-initiated file renaming operation.
/// </summary>
public class UserRenamingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current file name before renaming.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the new file name requested by the user.
    /// </summary>
    public string NewFileName { get; }

    /// <summary>
    /// Gets the rename action to perform.
    /// </summary>
    public RenameAction DoRenameAction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRenamingEventArgs"/> class.
    /// </summary>
    /// <param name="fileName">The current file name.</param>
    /// <param name="newFileName">The new file name requested.</param>
    /// <param name="action">The rename action to perform.</param>
    public UserRenamingEventArgs(string fileName, string newFileName, RenameAction action)
    {
        FileName = fileName;
        NewFileName = newFileName;
        DoRenameAction = action;
    }
}

/// <summary>
/// Provides event arguments for a completed file renaming operation.
/// </summary>
public class UserRenamedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new file name after renaming.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the original file name before renaming.
    /// </summary>
    public string OldFileName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRenamedEventArgs"/> class.
    /// </summary>
    /// <param name="fileName">The new file name.</param>
    /// <param name="oldFileName">The original file name.</param>
    public UserRenamedEventArgs(string fileName, string oldFileName)
    {
        FileName = fileName;
        OldFileName = oldFileName;
    }
}
