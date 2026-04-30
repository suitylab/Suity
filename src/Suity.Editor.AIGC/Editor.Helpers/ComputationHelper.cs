using Suity.Editor.Flows;
using Suity.Views;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Helpers;

/// <summary>
/// Provides helper methods for computation-related operations with conversation dialog support.
/// </summary>
public static class ComputationHelper
{
    /// <summary>
    /// Displays a pause dialog and waits for user confirmation before continuing.
    /// </summary>
    /// <param name="compute">The asynchronous flow computation context.</param>
    /// <param name="text">The message to display in the dialog.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task that completes with true if the user clicked OK, false otherwise.</returns>
    public static Task<bool> PauseDialog(this IFlowComputationAsync compute, string text, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>();
        if (conversation is null)
        {
            throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");
        }

        var source = new TaskCompletionSource<bool>();
        DisposableDialogItem dialogItem = null;
        IEnumerator dialogCoroutine()
        {
            dialogItem = conversation.AddDebugMessage(text, o =>
            {
                o.AddButton("OK", "Continue");
            });

            yield return null;

            dialogItem?.Dispose();
            conversation.PopAction();

            switch (conversation.InputButton)
            {
                case "OK":
                    source.SetResult(true);
                    break;

                default:
                    source.SetResult(false);
                    break;
            }
        }

        var coroutine = dialogCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            dialogItem?.Dispose();
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }
}