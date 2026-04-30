using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Editor.Services;

/// <summary>
/// Provides delayed action execution with debouncing support.
/// </summary>
internal class RunDelayed : IRunDelayed
{
    /// <summary>
    /// Default singleton instance.
    /// </summary>
    public static readonly RunDelayed Default = new();

    private readonly DelayedActionQueue _queue;

    /// <summary>
    /// Creates a new RunDelayed instance with a default 800ms delay.
    /// </summary>
    public RunDelayed()
    {
        _queue = new DelayedActionQueue(800);
    }

    /// <inheritdoc/>
    public void AddAction(DelayedAction action)
    {
        _queue.AddAction(action);
    }

    /// <inheritdoc/>
    public void RemoveAction(DelayedAction action)
    {
        _queue.RemoveAction(action);
    }

    /// <inheritdoc/>
    public void ProccessActions()
    {
        _queue.DoEventsAtOnce();
    }
}

/// <summary>
/// Queue that manages delayed actions with timer-based execution.
/// </summary>
internal class DelayedActionQueue
{
    private readonly int _delay;
    private readonly LinkedList<DelayedAction> _actionQueue = new();
    private readonly Dictionary<DelayedAction, LinkedListNode<DelayedAction>> _actionLookup = [];
    private readonly object _sync = new();
    private readonly Timer _timer;

    /// <summary>
    /// Creates a new delayed action queue.
    /// </summary>
    /// <param name="delayMs">The delay in milliseconds before executing actions.</param>
    public DelayedActionQueue(int delayMs)
    {
        _delay = delayMs;
        _timer = new Timer(ProcessTimer);
    }

    /// <summary>
    /// Adds an action to the queue, resetting the timer. If the action already exists, it is moved to the end.
    /// </summary>
    /// <param name="action">The action to add.</param>
    public void AddAction(DelayedAction action)
    {
        if (action == null) throw new ArgumentNullException();

        lock (_sync)
        {
            if (_actionLookup.TryGetValue(action, out LinkedListNode<DelayedAction> node))
            {
                _actionQueue.Remove(node);
                _actionQueue.AddLast(node);
                node.Value.DelayCount = action.DelayCount;
            }
            else
            {
                node = _actionQueue.AddLast(action);
                _actionLookup.Add(action, node);
            }

            _timer.Change(_delay, Timeout.Infinite);
        }
    }

    /// <summary>
    /// Removes an action from the queue.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    /// <returns>True if the action was found and removed.</returns>
    public bool RemoveAction(DelayedAction action)
    {
        if (action == null) throw new ArgumentNullException();

        lock (_sync)
        {
            if (_actionLookup.TryGetValue(action, out LinkedListNode<DelayedAction> node))
            {
                _actionQueue.Remove(node);
                _actionLookup.Remove(action);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Timer callback that triggers immediate execution of all queued actions.
    /// </summary>
    /// <param name="state">Timer state (unused).</param>
    private void ProcessTimer(object state)
    {
        DoEventsAtOnce();
    }

    /// <summary>
    /// Executes all queued actions immediately on the main thread.
    /// </summary>
    public void DoEventsAtOnce()
    {
        DelayedAction[] actions = null;
        lock (_sync)
        {
            if (_actionQueue.Count > 0)
            {
                actions = [.. _actionQueue];
                _actionQueue.Clear();
                _actionLookup.Clear();
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        if (actions != null)
        {
            // Transfer to main thread processing
            QueuedAction.Do(() =>
            {
                foreach (DelayedAction action in actions)
                {
                    if (action.DelayCount <= 0)
                    {
                        try
                        {
                            if (EditorPlugin.RuntimeLogging)
                            {
                                EditorServices.SystemLog.AddLog($"*** Do delayed action : {action.GetType().GetTypeCSCodeName()}");
                            }
                            action.DoAction();
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    }
                    else
                    {
                        action.DelayCount--;
                        AddAction(action);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Disposes the queue and stops the timer.
    /// </summary>
    public void Dispose()
    {
        lock (_sync)
        {
            _actionQueue.Clear();
            _actionLookup.Clear();
            _timer.Dispose();
        }
    }
}
