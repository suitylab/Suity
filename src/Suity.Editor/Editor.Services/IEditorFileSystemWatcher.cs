using System;

namespace Suity.Editor.Services;

/// <summary>
/// Interface for watching file system changes.
/// </summary>
public interface IEditorFileSystemWatcher : IDisposable
{
    /// <summary>
    /// Delegate for handling path events.
    /// </summary>
    public delegate void PathHandler(string fullPath);

    /// <summary>
    /// Delegate for handling rename events.
    /// </summary>
    public delegate void PathRenameHandler(string fullPath, string oldFullPath);


    /// <summary>
    /// Event raised when a file is created.
    /// </summary>
    event PathHandler Created;

    /// <summary>
    /// Event raised when a file is deleted.
    /// </summary>
    event PathHandler Deleted;

    /// <summary>
    /// Event raised when a file is changed.
    /// </summary>
    event PathHandler Changed;

    /// <summary>
    /// Event raised when a file is renamed.
    /// </summary>
    event PathRenameHandler Renamed;

    /// <summary>
    /// Gets or sets the path being watched.
    /// </summary>
    string Path { get; set; }

    /// <summary>
    /// Gets or sets whether events are raised.
    /// </summary>
    bool EnableRaisingEvents { get; set; }

    /// <summary>
    /// Gets or sets the filter pattern.
    /// </summary>
    string Filter { get; set; }

    /// <summary>
    /// Gets or sets whether to include subdirectories.
    /// </summary>
    bool IncludeSubdirectories { get; set; }

    /// <summary>
    /// Gets or sets whether to use delayed events.
    /// </summary>
    bool Delayed { get; set; }

    /// <summary>
    /// Gets the owner object.
    /// </summary>
    object Owner { get; }
}
