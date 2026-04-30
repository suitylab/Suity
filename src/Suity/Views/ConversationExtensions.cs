using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views;

/// <summary>
/// Provides extension methods for conversation handling.
/// </summary>
public static class ConversationExtensions
{
    public static DisposableDialogItem AddMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.Remote, TextStatus.Info, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddMessage(this IConversationHandler handler, string content, TextStatus status, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.Remote, status, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddSystemMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Normal, config);
        return new DisposableDialogItem(handler, msg);
    }


    public static DisposableDialogItem AddSystemMessage(this IConversationHandler handler, string content, TextStatus status, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, status, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddInfoMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Info, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddRunningMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.ResourceUse, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddDisabledMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Disabled, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddTitleMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Tag, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddWarningMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Warning, config);
        return new DisposableDialogItem(handler, msg);
    }


    public static DisposableDialogItem AddErrorMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.System, TextStatus.Error, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddDebugMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.Debug, TextStatus.Normal, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddDebugMessage(this IConversationHandler handler, string content, TextStatus status, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.Debug, status, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static void RemoveDebugMessages(this IConversationHandler handler)
    {
        handler.RemoveMessages(o => o is IDialogMessage { Role: ConversationRole.Debug });
    }

    public static DisposableDialogItem AddMyMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.User, TextStatus.Normal, config);
        return new DisposableDialogItem(handler, msg);
    }

    public static DisposableDialogItem AddUserMessage(this IConversationHandler handler, string content, Action<IDialogMessage> config = null)
    {
        var msg = handler.AddMessage(content, ConversationRole.User, TextStatus.Normal, config);
        return new DisposableDialogItem(handler, msg);
    }


    public static DisposableDialogItem AddException(this IConversationHandler handler, Exception err, string message = null, bool showInner = true, bool addLog = true)
    {
        if (err is OperationCanceledException)
        {
            return new DisposableDialogItem(handler, EmptyDialogItem.Empty);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message = L("An error occurred.");
        }

        var item = handler.AddMessage(message, TextStatus.Error, m =>
        {
            m.AddCode($"{err.GetType().Name}\n{err.Message}");

            if (showInner)
            {
                var inner = err.InnerException;
                while (inner != null)
                {
                    m.AddCode($"{inner.GetType().Name}\n{inner.Message}");
                    inner = inner.InnerException;
                }
            }

            m.AddButton(L("Show error"), () => err.LogError(message));
        });

        //if (addLog)
        //{
        //    err.LogError(message);
        //}

        return item;
    }

    public static void AddButton(this IDialogMessage config, string text, Action callBack)
    {
        config.AddButton(text, text, callBack);
    }

    public static void AddButtons(this IDialogMessage config, string title, params ConversationButton[] buttons)
    {
        config.AddButtons(title, buttons);
    }

    public static Task<string> WaitForTextInput(this IConversationHandler conversation, CancellationToken cancel)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var source = new TaskCompletionSource<string>();

        IEnumerator chatCoroutine()
        {
            yield return null;

            string s = conversation.InputMessage ?? string.Empty;
            source.SetResult(s);
        }

        var coroutine = chatCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }

    public static Task<string> WaitForButtonInput(this IConversationHandler conversation, CancellationToken cancel)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var source = new TaskCompletionSource<string>();

        IEnumerator chatCoroutine()
        {
            yield return null;

            string s = conversation.InputButton ?? string.Empty;
            source.SetResult(s);
        }

        var coroutine = chatCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }

    public static Task<string> WaitForTextOrButtonInput(this IConversationHandler conversation, CancellationToken cancel)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var source = new TaskCompletionSource<string>();

        IEnumerator chatCoroutine()
        {
            yield return null;

            string s = conversation.InputButton ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(s))
            {
                source.SetResult(s);
            }
            else
            {
                s = conversation.InputButton ?? string.Empty;
                source.SetResult(s);
            }
        }

        var coroutine = chatCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }

    public static Task<string> WaitForButtonInput(this IConversationHandler conversation, string[] buttons, CancellationToken cancel)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        if (buttons is null)
        {
            throw new ArgumentNullException(nameof(buttons));
        }

        if (buttons.Length == 0)
        {
            throw new ArgumentException("Buttons cannot be empty.", nameof(buttons));
        }

        var source = new TaskCompletionSource<string>();

        IEnumerator chatCoroutine()
        {
            while (true)
            {
                yield return null;

                string s = conversation.InputButton ?? string.Empty;

                if (buttons.Contains(s))
                {
                    source.SetResult(s);
                    break;
                }
            }
        }

        var coroutine = chatCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }

    public static Task RemoveOn(this DisposableDialogItem dialogItem, float secs)
        => dialogItem.RemoveOn(TimeSpan.FromSeconds(secs));

    public static async Task RemoveOn(this DisposableDialogItem dialogItem, TimeSpan duration)
    {
        // Delay for a specified time
        await Task.Delay(duration);

        QueuedAction.Do(() =>
        {
            // Call Dispose method
            dialogItem.Dispose();
        });
    }
}