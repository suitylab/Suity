using Suity.Rex;
using Suity.Rex.VirtualDom;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Service that manages file update operations with iteration-based listeners and delayed execution.
/// </summary>
internal class FileUpdateServiceBK : FileUpdateService
{
    /// <summary>
    /// Singleton instance of the file update service.
    /// </summary>
    public static FileUpdateServiceBK Instance { get; } = new();

    private readonly List<List<Action<IProgress>>> _listeners = [];

    private FileUpdateAction _updateAction;
    private bool _isAppActivated = true;
    private bool _fileUpdating;
    private bool _fileUpdateRequesting;

    private FileUpdateServiceBK()
    {
        _updateAction = new FileUpdateAction(this);

        for (int i = 0; i < 5; i++)
        {
            _listeners.Add([]);
        }

        EditorRexes.IsAppActive.AsRexListener().Subscribe(OnAppActivated);
    }

    /// <inheritdoc/>
    public override void AddFileUpdateListener(LoadingIterations iteration, Action<IProgress> callBack)
    {
        if (callBack is null)
        {
            throw new ArgumentNullException(nameof(callBack));
        }

        _listeners[(int)iteration].Add(callBack);
    }

    /// <inheritdoc/>
    public override void UpdateFileDelayed()
    {
        lock (this)
        {
            if (!_fileUpdating && _isAppActivated)
            {
                EditorUtility.AddDelayedAction(_updateAction);
            }
            else
            {
                _fileUpdateRequesting = true;
            }
        }
    }

    /// <inheritdoc/>
    public override void UpdateFileNow()
    {
        lock (this)
        {
            if (_fileUpdating)
            {
                return;
            }

            _fileUpdating = true;

            DoFileUpdateProgress(EmptyProgress.Empty);

            _fileUpdating = false;
            RaiseUpdateFinished();
        }
    }

    /// <summary>
    /// Handles application activation state changes, triggering pending updates when the app becomes active.
    /// </summary>
    /// <param name="activated">Whether the application is now active.</param>
    private void OnAppActivated(bool activated)
    {
        lock (this)
        {
            _isAppActivated = activated;

            if (activated && _fileUpdateRequesting)
            {
                _fileUpdateRequesting = false;
                EditorUtility.AddDelayedAction(_updateAction);
            }
        }
    }

    /// <summary>
    /// Executes file updates asynchronously on a background thread.
    /// </summary>
    private void DoFileUpdate()
    {
        lock (this)
        {
            if (_fileUpdating)
            {
                return;
            }

            _fileUpdating = true;

            Task.Run(() =>
            {
                DoFileUpdateProgress(EmptyProgress.Empty);

                QueuedAction.Do(() =>
                {
                    _fileUpdating = false;
                    RaiseUpdateFinished();
                });
            });
        }
    }

    /// <summary>
    /// Invokes all registered file update listeners for each iteration.
    /// </summary>
    /// <param name="p">Progress reporter for the update operation.</param>
    private void DoFileUpdateProgress(IProgress p)
    {
        for (int i = 0; i < _listeners.Count; i++)
        {
            var list = _listeners[i];
            foreach (var listener in list)
            {
                try
                {
                    listener(p);
                }
                catch (Exception err)
                {
                    err.LogError($"Update failed in iteraction : {i}");
                }
            }
        }
    }

    /// <summary>
    /// Delayed action wrapper that triggers file update operations.
    /// </summary>
    private class FileUpdateAction : DelayedAction<FileUpdateServiceBK>
    {
        /// <summary>
        /// Creates a new file update delayed action.
        /// </summary>
        /// <param name="value">The file update service instance.</param>
        public FileUpdateAction(FileUpdateServiceBK value) : base(value)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.DoFileUpdate();
        }
    }
}
