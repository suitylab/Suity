using System;                
using System.IO;
using Suity.Editor;
using Suity.Editor.Services;

namespace Suity.Helpers;

/// <summary>
/// A file system watcher for the editor that supports delayed event raising and automatic pause/resume during file operations.
/// Implements <see cref="IEditorFileSystemWatcher"/> to provide file change notifications.
/// </summary>
public class EditorFileSystemWatcher : IEditorFileSystemWatcher, IDisposable
{

    private object _owner;
    private string _path;
    private bool _enableRisingEvents = true;
    private string _filter;
    private bool _includeSubDirectories = false;

    private FileSystemWatcher _watcher;

    /// <inheritdoc/>
    public event IEditorFileSystemWatcher.PathHandler Created;

    /// <inheritdoc/>
    public event IEditorFileSystemWatcher.PathHandler Deleted;

    /// <inheritdoc/>
    public event IEditorFileSystemWatcher.PathHandler Changed;

    /// <inheritdoc/>
    public event IEditorFileSystemWatcher.PathRenameHandler Renamed;


    /// <summary>
    /// Gets or sets the path of the directory to watch.
    /// </summary>
    public string Path
    {
        get => _watcher?.Path ?? _path;
        set
        {
            _path = value;
            if (_watcher != null)
            {
                _watcher.Path = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the watcher is currently raising events.
    /// </summary>
    public bool EnableRaisingEvents
    {
        get => _watcher?.EnableRaisingEvents ?? _enableRisingEvents;
        set
        {
            _enableRisingEvents = value;
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = value;
            }
            
            EditorServices.SystemLog.AddLog($"set DalayedDirectoryWatcher enabled : {Path} = {value}.");
        }
    }

    /// <summary>
    /// Gets or sets the filter string used to determine which files to watch.
    /// </summary>
    public string Filter
    {
        get => _watcher?.Filter ?? _filter;
        set
        {
            _filter = value;
            if (_watcher != null)
            {
                _watcher.Filter = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories should be watched.
    /// </summary>
    public bool IncludeSubdirectories
    {
        get => _watcher?.IncludeSubdirectories ?? _includeSubDirectories;
        set
        {
            _includeSubDirectories = value;
            if (_watcher != null)
            {
                _watcher.IncludeSubdirectories = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether events should be delayed and queued instead of raised immediately.
    /// </summary>
    public bool Delayed { get; set; }

    /// <summary>
    /// Gets the owner object associated with this watcher.
    /// </summary>
    public object Owner => _owner;


    /// <summary>
    /// Initializes a new instance of the <see cref="EditorFileSystemWatcher"/> class.
    /// </summary>
    /// <param name="path">The directory path to watch.</param>
    /// <param name="owner">The owner object associated with this watcher. Optional.</param>
    /// <param name="enableUnwatch">If true, subscribes to <see cref="FileUnwatchedAction"/> events for automatic pause/resume. Default is true.</param>
    public EditorFileSystemWatcher(string path, object owner = null, bool enableUnwatch = true)
    {
        EditorServices.SystemLog.AddLog($"DalayedDirectoryWatcher creating : {path}...");

        _path = path;
        _owner = owner;

        CreateWatcher();

        if (enableUnwatch)
        {
            FileUnwatchedAction.Pause += EnterUnwatched;
            FileUnwatchedAction.Resume += ExitUnwatched;
            FileUnwatchedAction.Rename += OnCheckRename;
        }

        EditorServices.SystemLog.AddLog($"DalayedDirectoryWatcher created.");
    }

    private void CreateWatcher()
    {
        if (_watcher != null)
        {
            return;
        }

        _watcher = new FileSystemWatcher(_path)
        {
            EnableRaisingEvents = _enableRisingEvents,
            Filter = _filter,
            IncludeSubdirectories = _includeSubDirectories,
        };

        _watcher.Created += _watcher_Created;
        _watcher.Deleted += _watcher_Deleted;
        _watcher.Changed += _watcher_Changed;
        _watcher.Renamed += _watcher_Renamed;
    }

    private void DisposeWatcher()
    {
        if (_watcher is null)
        {
            return;
        }

        _watcher.Created -= _watcher_Created;
        _watcher.Deleted -= _watcher_Deleted;
        _watcher.Changed -= _watcher_Changed;
        _watcher.Renamed -= _watcher_Renamed;

        _watcher.Dispose();
        _watcher = null;
    }

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
        if (_watcher != null)
        {
            _pausing = true;

            try
            {
                DisposeWatcher();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    private void ExitUnwatched()
    {
        if (_watcher == null)
        {
            _pausing = false;

            try
            {
                CreateWatcher();
            }
            catch (Exception err)
            {
                // err.LogError();
            }
        }
    }

    private void OnCheckRename(string oldPath, string newPath)
    {
        if (_path == oldPath)
        {
            _path = newPath;

            if (_watcher != null)
            {
                DisposeWatcher();
                CreateWatcher();
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        FileUnwatchedAction.Pause -= EnterUnwatched;
        FileUnwatchedAction.Resume -= ExitUnwatched;
        FileUnwatchedAction.Rename -= OnCheckRename;

        DisposeWatcher();
    }

    #region Events

    private class CreatedEvent : DelayedNamedAction<EditorFileSystemWatcher>
    {
        public CreatedEvent(EditorFileSystemWatcher watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Created?.Invoke(Name);
        }
    }

    private class DeletedEvent : DelayedNamedAction<EditorFileSystemWatcher>
    {
        public DeletedEvent(EditorFileSystemWatcher watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Deleted?.Invoke(Name);
        }
    }

    private class ChangedEvent : DelayedNamedAction<EditorFileSystemWatcher>
    {
        public ChangedEvent(EditorFileSystemWatcher watcher, string fullPath)
            : base(watcher, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Changed?.Invoke(Name);
        }
    }

    private class RenamedEvent : DelayedNamedAction<EditorFileSystemWatcher>
    {
        public RenamedEvent(EditorFileSystemWatcher watcher, string fullPath, string oldFullPath)
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
