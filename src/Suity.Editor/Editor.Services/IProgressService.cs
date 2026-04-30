using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Represents a progress request for the progress service.
/// </summary>
public class ProgressRequest
{
    /// <summary>
    /// Gets or sets the title of the progress operation.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the action to perform during progress.
    /// </summary>
    public Action<IProgress> ProgressAction { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when finished.
    /// </summary>
    public Action FinishedAction { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Title ?? base.ToString();
    }
}

/// <summary>
/// Service interface for displaying progress windows.
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Shows the progress window.
    /// </summary>
    void ShowProgressWindow();

    /// <summary>
    /// Executes a progress request.
    /// </summary>
    /// <param name="request">The progress request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DoProgress(ProgressRequest request);

    /// <summary>
    /// Executes multiple progress requests.
    /// </summary>
    /// <param name="requests">The array of progress requests.</param>
    /// <returns>An array of tasks representing the asynchronous operations.</returns>
    Task[] DoProgress(ProgressRequest[] requests);

    /// <summary>
    /// Gets a value indicating whether a progress operation is running.
    /// </summary>
    bool ProgressRunning { get; }
}

/// <summary>
/// Interface for updating progress information.
/// </summary>
public interface IProgress
{
    /// <summary>
    /// Updates progress by percentage.
    /// </summary>
    /// <param name="percentage">The percentage value (0-100).</param>
    /// <param name="mainMessage">The main message.</param>
    /// <param name="subMessage">The sub message.</param>
    void UpdateProgess(int percentage, string mainMessage, string subMessage);

    /// <summary>
    /// Updates progress by rate.
    /// </summary>
    /// <param name="rate">The rate value (0.0-1.0).</param>
    /// <param name="mainMessage">The main message.</param>
    /// <param name="subMessage">The sub message.</param>
    void UpdateProgess(float rate, string mainMessage, string subMessage);

    /// <summary>
    /// Updates progress by index and count.
    /// </summary>
    /// <param name="index">The current index.</param>
    /// <param name="count">The total count.</param>
    /// <param name="mainMessage">The main message.</param>
    /// <param name="subMessage">The sub message.</param>
    void UpdateProgess(int index, int count, string mainMessage, string subMessage);

    /// <summary>
    /// Marks the progress as complete.
    /// </summary>
    void CompleteProgess();
}

/// <summary>
/// Empty implementation of the progress interface that does nothing.
/// </summary>
public sealed class EmptyProgress : IProgress
{
    /// <summary>
    /// Gets the singleton instance of EmptyProgress.
    /// </summary>
    public static readonly EmptyProgress Empty = new();

    private EmptyProgress()
    {
    }

    /// <inheritdoc/>
    public void CompleteProgess()
    {
    }

    /// <inheritdoc/>
    public void UpdateProgess(int percentage, string mainMessage, string subMessage)
    {
    }

    /// <inheritdoc/>
    public void UpdateProgess(float rate, string mainMessage, string subMessage)
    {
    }

    /// <inheritdoc/>
    public void UpdateProgess(int index, int count, string mainMessage, string subMessage)
    {
    }
}