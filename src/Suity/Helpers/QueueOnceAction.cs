using System;

namespace Suity.Helpers;

/// <summary>
/// Represents an action that can be queued to execute only once.
/// </summary>
public sealed class QueueOnceAction(Action action)
{
    private readonly Action _action = action ?? throw new ArgumentNullException(nameof(action));
    private bool _inQueue;

    public void DoAction()
    {
        _inQueue = false;
        _action();
    }

    public void DoQueuedAction()
    {
        lock (_action)
        {
            if (_inQueue)
            {
                return;
            }

            _inQueue = true;
        }

        QueuedAction.Do(() =>
        {
            lock (_action)
            {
                if (_inQueue)
                {
                    _inQueue = false;
                }
                else
                {
                    return;
                }
            }

            _action();
        });
    }
}