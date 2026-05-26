using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

internal class DelayCountDown : IDisposable
{
    private readonly IConversationHandler _conversation;
    private CancellationTokenSource _cancelSource;
    private IDisposable _currentMsg;

    private int _secondTotal;
    private int _secondCurrent;

    public DelayCountDown(IConversationHandler conversation)
    {
        _conversation = conversation;
        _cancelSource = new CancellationTokenSource();
    }

    public async Task Run(int seconds, CancellationToken cancellation)
    {
        _secondTotal = seconds;
        _secondCurrent = seconds;

        do
        {
            _currentMsg?.Dispose();
            _currentMsg = _conversation?.AddSystemMessage($"{_secondCurrent}s", msg => 
            {
                msg.AddProgressBar(_secondCurrent, _secondTotal);
            });

            await Task.Delay(1000);

            _secondCurrent--;

            if (cancellation.IsCancellationRequested)
            {
                break;
            }

            if (_secondCurrent <= 0)
            {
                break;
            }
        } while (true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cancelSource?.Cancel();
        _cancelSource = null;

        _currentMsg?.Dispose();
        _currentMsg = null;
    }
}