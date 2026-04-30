using System;
using System.IO;
using Suity.Editor;
using Suity.Editor.Services;

namespace Suity.Helpers;

/// <summary>
/// A soft file system watcher for the editor that supports delayed event raising and automatic pause/resume during file operations.
/// Unlike <see cref="EditorFileSystemWatcher"/>, this version does not implement <see cref="IEditorFileSystemWatcher"/> and manages its own delegates.
/// </summary>
public class EditorFileSystemWatcherSoft : IDisposable
{
    /// <summary>
    /// Delegate for handling file path change events (created, deleted, changed).
    /// </summary>
    /// <param name="fullPath">The full path of the affected file or directory.</param>
    public delegate void PathHandler(string fullPath);

    /// <summary>
    /// Delegate for handling file rename events.
    /// </summary>
    /// <param name="fullPath">The new full path of the renamed file or directory.</param>
    /// <param name="oldFullPath">The old full path of the renamed file or directory.</param>
    public delegate void PathRenameHandler(string fullPath, string oldFullPath);

    private object _owner;
    private FileSystemWatcher _watcher;

    /// <summary>
    /// Occurs when a file or directory is created in the watched path.
    /// </summary>
    public event PathHandler Created;

    /// <summary>
    /// Occurs when a file or directory is deleted from the watched path.
    /// </summary>
    public event PathHandler Deleted;

    /// <summary>
    /// Occurs when a file or directory is changed in the watched path.
    /// </summary>
    public event PathHandler Changed;

    /// <summary>
    /// Occurs when a file or directory is renamed in the watched path.
    /// </summary>
    public event PathRenameHandler Renamed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorFileSystemWatcherSoft"/> class.
    /// </summary>
    /// <param name="path">The directory path to watch.</param>
    /// <param name="owner">The owner object associated with this watcher. Optional.</param>
    public EditorFileSystemWatcherSoft(string path, object owner = null)
    {
        EditorServices.SystemLog.AddLog($"DalayedDirectoryWatcher creating : {path}...");

        _owner = owner;
        _watcher = new FileSystemWatcher(path);

        _watcher.Created += _watcher_Created;
        _watcher.Deleted += _watcher_Deleted;
        _watcher.Changed += _watcher_Changed;
        _watcher.Renamed += _watcher_Renamed;

        FileUnwatchedAction.Pause += EnterUnwatched;
        FileUnwatchedAction.Resume += ExitUnwatched;
        FileUnwatchedAction.Rename += OnCheckRename;

        EditorServices.SystemLog.AddLog($"DalayedDirectoryWatcher created.");
    }

    /// <summary>
    /// Gets or sets the path of the directory to watch.
    /// </summary>
    public string Path
    {
        get => _watcher.Path;
        set => _watcher.Path = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the watcher is currently raising events.
    /// </summary>
    public bool EnableRaisingEvents
    {
        get => _watcher.EnableRaisingEvents;
        set
        {
            _watcher.EnableRaisingEvents = value;
            EditorServices.SystemLog.AddLog($"set DalayedDirectoryWatcher enabled : {Path} = {value}.");
        }
    }

    /// <summary>
    /// Gets or sets the filter string used to determine which files to watch.
    /// </summary>
    public string Filter
    {
        get => _watcher.Filter;
        set => _watcher.Filter = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories should be watched.
    /// </summary>
    public bool IncludeSubdirectories
    {
        get => _watcher.IncludeSubdirectories;
        set => _watcher.IncludeSubdirectories = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether events should be delayed and queued instead of raised immediately.
    /// </summary>
    public bool Delayed { get; set; }

    /// <summary>
    /// Gets the owner object associated with this watcher.
    /// </summary>
    public object Owner => _owner;

    private void _watcher_Created(object sender, FileSystemEventArgs e)
    {
        if (Delayed)
        {
            EditorUtility.AddDelayedAction(new CreatedEvent(this, e.FullPath));
        }
        else
        {
            QueuedAction.Do(() => Created?.Invoke(e.FullPath));
        }
    }

    private void _watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (Delayed)
        {
            EditorUtility.AddDelayedAction(new DeletedEvent(this, e.FullPath));
        }
        else
        {
            QueuedAction.Do(() => Deleted?.Invoke(e.FullPath));
        }
    }

    private void _watcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (Delayed)
        {
            EditorUtility.AddDelayedAction(new ChangedEvent(this, e.FullPath));
        }
        else
        {
            QueuedAction.Do(() => Changed?.Invoke(e.FullPath));
        }
    }

    private void _watcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (Delayed)
        {
            EditorUtility.AddDelayedAction(new RenamedEvent(this, e.FullPath, e.OldFullPath));
        }
        else
        {
            QueuedAction.Do(() => Renamed?.Invoke(e.FullPath, e.OldFullPath));
        }
    }

    private bool _pausing;

    private void EnterUnwatched()
    {
        if (_watcher?.EnableRaisingEvents == true)
        {
            _pausing = true;

            try
            {
                _watcher.EnableRaisingEvents = false;
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    private void ExitUnwatched()
    {
        if (_pausing)
        {
            _pausing = false;

            try
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception err)
            {
                //err.LogError();
            }
        }
    }

    private void OnCheckRename(string oldPath, string newPath)
    {
        if (_watcher != null)
        {
            try
            {
                if (_watcher.Path == oldPath)
                {
                    _watcher.Path = newPath;
                }
            }
            catch (Exception err)
            {
                //err.LogError();
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_watcher != null)
        {
            FileUnwatchedAction.Pause -= EnterUnwatched;
            FileUnwatchedAction.Resume -= ExitUnwatched;
            FileUnwatchedAction.Rename -= OnCheckRename;

            _watcher.EnableRaisingEvents = false;

            _watcher.Created -= _watcher_Created;
            _watcher.Deleted -= _watcher_Deleted;
            _watcher.Changed -= _watcher_Changed;
            _watcher.Renamed -= _watcher_Renamed;

            _watcher.Dispose();
            _watcher = null;
        }
    }

    #region Events

    private class CreatedEvent : DelayedNamedAction<EditorFileSystemWatcherSoft>
    {
        public CreatedEvent(EditorFileSystemWatcherSoft watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Created?.Invoke(Name);
        }
    }

    private class DeletedEvent : DelayedNamedAction<EditorFileSystemWatcherSoft>
    {
        public DeletedEvent(EditorFileSystemWatcherSoft watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Deleted?.Invoke(Name);
        }
    }

    private class ChangedEvent : DelayedNamedAction<EditorFileSystemWatcherSoft>
    {
        public ChangedEvent(EditorFileSystemWatcherSoft watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Changed?.Invoke(Name);
        }
    }

    private class RenamedEvent : DelayedNamedAction<EditorFileSystemWatcherSoft>
    {
        public RenamedEvent(EditorFileSystemWatcherSoft watcher, string fullPath, string oldFullPath)
            : base(watcher, fullPath, oldFullPath)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Renamed?.Invoke(Name, OldName);
        }
    }

    #endregion
}
