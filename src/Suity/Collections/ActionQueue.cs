using System;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// Represents a queue of actions that can be executed.
/// </summary>
public class ActionQueue
{
    // Private field to store the actions
    private readonly List<Action> _actions = [];

    /// <summary>
    /// Queues an action to be executed.
    /// </summary>
    /// <param name="action">The action to be queued.</param>
    public void QueueAction(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException();
        }

        lock (_actions)
        {
            _actions.Add(action);
        }
    }

    /// <summary>
    /// Updates the queue and executes all queued actions.
    /// </summary>
    public void Update()
    {
        lock (_actions)
        {
            if (_actions.Count > 0)
            {
                var actions = _actions.ToArray();
                _actions.Clear();

                foreach (var action in actions)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception err)
                    {
                        err.LogError();
                    }
                }
            }
        }
    }
}
