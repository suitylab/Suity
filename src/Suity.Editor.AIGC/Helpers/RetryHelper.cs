using Suity.Editor.AIGC.Assistants;
using Suity.Views;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Suity.Editor.AIGC.Helpers;

/// <summary>
/// Provides helper methods for retrying asynchronous operations with error handling and conversation feedback.
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Executes an asynchronous operation with automatic retry on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="title">The title displayed in conversation messages during retry attempts.</param>
    /// <param name="task">The asynchronous operation to execute.</param>
    /// <param name="acceptNull">Whether to accept a null result as success.</param>
    /// <param name="retry">The number of retry attempts. Uses default from configuration if null.</param>
    /// <param name="conversation">Optional conversation handler for displaying status messages.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>The result of the operation, or null if all attempts fail.</returns>
    public static async Task<T> DoRetryAction<T>(string title, Func<Task<T>> task, bool acceptNull = false, int? retry = null,
        IConversationHandler conversation = null, CancellationToken cancel = default) where T : class
    {
        DisposableDialogItem msgItem = null;

        int vTryCount = retry ?? AIAssistantService.Config?.RetryCount ?? 3;
        if (vTryCount < 1)
        {
            vTryCount = 1;
        }

        for (int i = 0; i < vTryCount; i++)
        {
            cancel.ThrowIfCancellationRequested();

            if (conversation != null)
            {
                msgItem?.Dispose();

                // Only show message when second attempt starts
                if (i > 0)
                {
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        msgItem = conversation?.AddInfoMessage($"{title} (Attempt {i + 1}/{vTryCount})...");
                    }
                    else
                    {
                        msgItem = conversation?.AddInfoMessage($"(Attempt {i + 1}/{vTryCount})...");
                    }
                }
            }

            try
            {
                var result = await task();
                if (result != null || acceptNull)
                {
                    msgItem?.Dispose();

                    return result;
                }
            }
            catch (AigcException llmErr)
            {
// string errMsg = $"Execution failed: {title}";
// Normal errors, message auto-disappears after 3 seconds
                // conversation?.AddException(llmErr, errMsg).RemoveOn(3);
                // llmErr.LogError(errMsg);

                continue;
            }
            catch (OperationCanceledException)
            {
                // Cancel exception directly exits.
                throw;
            }
            catch (Exception err)
            {
// Best to catch all exceptions to prevent unexpected situations
// Unconventional errors, message does not auto-disappear.
                conversation?.AddException(err, $"Execution failed: {title}");
                continue;
            }
        }

        if (conversation != null)
        {
            msgItem?.Dispose();

            if (!string.IsNullOrWhiteSpace(title))
            {
                msgItem = conversation?.AddErrorMessage($"{title}, failed after {vTryCount} attempts: {title}.");
            }
            else
            {
                msgItem = conversation?.AddErrorMessage($"Failed after {vTryCount} attempts: {title}.");
            }
        }

        return null;
    }
}
