using System;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for managing file updates.
/// </summary>
public abstract class FileUpdateService
{
    private static FileUpdateService _current;

    /// <summary>
    /// Gets or sets the current file update service instance.
    /// </summary>
    public static FileUpdateService Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Device.Current.GetService<FileUpdateService>();
            return _current;
        }
        internal set
        {
            _current = value;
        }
    }

    /// <summary>
    /// Updates files with a delay.
    /// </summary>
    public abstract void UpdateFileDelayed();

    /// <summary>
    /// Updates files immediately.
    /// </summary>
    public abstract void UpdateFileNow();

    /// <summary>
    /// Adds a listener for file update events.
    /// </summary>
    /// <param name="iteration">The loading iteration.</param>
    /// <param name="callBack">The callback to invoke.</param>
    public abstract void AddFileUpdateListener(LoadingIterations iteration, Action<IProgress> callBack);

    /// <summary>
    /// Event raised when file update is finished.
    /// </summary>
    public event EventHandler UpdateFinished;

    /// <summary>
    /// Raises the UpdateFinished event.
    /// </summary>
    protected void RaiseUpdateFinished()
    {
        UpdateFinished?.Invoke(this, EventArgs.Empty);
    }
}