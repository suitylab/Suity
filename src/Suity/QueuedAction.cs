using System;
using System.Threading.Tasks;

namespace Suity;

/// <summary>
/// Support action queue
/// </summary>
public static class QueuedAction
{
    /// <summary>
    /// Execute actions in a queue
    /// </summary>
    /// <param name="action"></param>
    public static void Do(Action action) => Device._current.QueueAction(action);

    public static void DoSuspendedAction(Action action) => Device._current.DoSuspendedAction(action);

    public static Task DoSuspendedAction(Func<Task> action) => Device._current.DoSuspendedAction(action);

    public static void FlushQueuedActions() => Device._current.FlushQueuedActions();

    public static bool IsQueueSuspended => Device._current.IsQueueSuspended;
}