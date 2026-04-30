using System;
using System.Threading;

namespace Suity.Editor;

/// <summary>
/// Provides actions to temporarily pause and resume file watching operations.
/// </summary>
public static class FileUnwatchedAction
{
    /// <summary>
    /// Delegate for handling file rename operations.
    /// </summary>
    /// <param name="oldPath">The original file path.</param>
    /// <param name="newPath">The new file path.</param>
    public delegate void RenameAction(string oldPath, string newPath);

    /// <summary>
    /// Event raised when file watching is paused.
    /// </summary>
    public static event Action Pause;

    /// <summary>
    /// Event raised when file watching is resumed.
    /// </summary>
    public static event Action Resume;

    /// <summary>
    /// Event raised when a file is renamed.
    /// </summary>
    public static event RenameAction Rename;
    

    private static readonly object _sync = new();
    private static int _num = 0;

    /// <summary>
    /// Executes an action while temporarily pausing file watching.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void Do(Action action)
    {
        //lock (_sync)
        //{

        //}

        try
        {
            Interlocked.Increment(ref _num);
            //_num++;

            if (_num == 1)
            {
                try
                {
                    Pause?.Invoke();
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }
            action();
        }
        finally
        {
            Interlocked.Decrement(ref _num);
            //_num--;

            if (_num == 0)
            {
                try
                {
                    Resume?.Invoke();
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }
        }
    }

    /// <summary>
    /// Notifies that a file has been renamed.
    /// </summary>
    /// <param name="oldPath">The original file path.</param>
    /// <param name="newPath">The new file path.</param>
    /// <exception cref="ArgumentNullException">Thrown when oldPath or newPath is null or whitespace.</exception>
    public static void NotifyRenamed(string oldPath, string newPath)
    {
        if (string.IsNullOrWhiteSpace(oldPath))
        {
            throw new ArgumentNullException(nameof(oldPath));
        }
        if (string.IsNullOrWhiteSpace(newPath))
        {
            throw new ArgumentNullException(nameof(newPath));
        }

        try
        {
            Rename?.Invoke(oldPath.Replace('/', '\\').Trim(), newPath.Replace('/', '\\').Trim());
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Gets a value indicating whether file watching is currently unwatched (suspended).
    /// </summary>
    public static bool IsUnwatched => _num > 0;
}