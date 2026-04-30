using System;

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
    public static void Do(Action action)
    {
        Device._current.QueueAction(action);
    }
}