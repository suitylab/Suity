using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using System;

namespace Suity.Editor.AIGC;

/// <summary>
/// Tracks and reports progress of a batch task, displaying status updates through a conversation handler.
/// </summary>
public class ProgressCounter : IDisposable
{
    /// <summary>
    /// Gets the conversation handler used to display progress messages.
    /// </summary>
    public IConversationHandler Conversation { get; }

    /// <summary>
    /// Gets the name of the task being tracked.
    /// </summary>
    public string TaskName { get; }

    /// <summary>
    /// Gets the total number of items to process.
    /// </summary>
    public int Total { get; private set; }

    /// <summary>
    /// Gets the number of items processed so far.
    /// </summary>
    public int NumCurrent { get; private set; }

    /// <summary>
    /// Gets the number of items processed successfully.
    /// </summary>
    public int NumOK { get; private set; }

    /// <summary>
    /// Gets the number of items skipped.
    /// </summary>
    public int NumSkip { get; private set; }

    /// <summary>
    /// Gets the number of items that encountered errors.
    /// </summary>
    public int NumError { get; private set; }

    private DisposableDialogItem _currentMsg = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressCounter"/> class.
    /// </summary>
    /// <param name="conversation">The conversation handler to use for displaying progress.</param>
    /// <param name="count">The total number of items to process.</param>
    /// <param name="taskName">Optional name for the task being tracked.</param>
    public ProgressCounter(IConversationHandler conversation, int count, string taskName = null)
    {
        Conversation = conversation;
        TaskName = taskName;

        Reset(count);
    }

    /// <summary>
    /// Resets all counters to zero and sets a new total count.
    /// </summary>
    /// <param name="count">The new total number of items to process.</param>
    public void Reset(int count)
    {
        Total = count;

        NumCurrent = 0;
        NumOK = 0;
        NumSkip = 0;
        NumError = 0;

        Update();
    }

    /// <summary>
    /// Increments the current and successful completion counters, then updates the progress display.
    /// </summary>
    public void IncreaseOK()
    {
        NumCurrent++;
        NumOK++;
        Update();
    }

    /// <summary>
    /// Increments the current and skipped counters, then updates the progress display.
    /// </summary>
    public void IncreaseSkip()
    {
        NumCurrent++;
        NumSkip++;
        Update();
    }

    /// <summary>
    /// Increments the current and error counters, then updates the progress display.
    /// </summary>
    public void IncreaseError()
    {
        NumCurrent++;
        NumError++;
        Update();
    }


    private void Update()
    {
        if (Conversation is not { } c)
        {
            return;
        }

        _currentMsg?.Dispose();

        string msg = L($"Total {Total} tasks, completed:{NumOK}, skipped:{NumSkip}, error:{NumError}.");

        if (!string.IsNullOrWhiteSpace(TaskName))
        {
            msg = $"{TaskName} {msg}";
        }

        _currentMsg = c.AddRunningMessage(msg, m => 
        {
            m.AddProgressBar(NumCurrent, Total);
        });
    }

    private void RemoveMessage()
    {
        if (Conversation is not { } c)
        {
            return;
        }

        _currentMsg?.Dispose();

        _currentMsg = null;
    }

    /// <summary>
    /// Releases resources and removes the progress message from the conversation.
    /// </summary>
    public void Dispose()
    {
        RemoveMessage();
    }
}
