using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Views;

#region ConversationRole
/// <summary>
/// Specifies the role of a participant in a conversation.
/// </summary>
public enum ConversationRole
{
    Debug,
    System,
    Remote,
    User,
}
#endregion

#region IConversationHandler
/// <summary>
/// Defines an interface for handling conversations.
/// </summary>
public interface IConversationHandler
{
    string ConversationName { get; }
    string Title { get; set; }
    string InputMessage { get; }
    string InputButton { get; }
    Stream InputStream { get; }

    IDialogMessage LastMessage { get; }

    IDIalogItem AddMessage(string content, ConversationRole role, TextStatus status, Action<IDialogMessage> config = null);

    void RemoveMessage(IDIalogItem item);

    void RemoveMessages(Predicate<IDIalogItem> predicate);

    void StartCoroutine(IEnumerator coroutine);

    void StopCoroutine(IEnumerator coroutine);

    void PushAction(Action action);

    void PushAction<T>(Action<T> action, T value);

    void PushCoroutine(IEnumerator coroutine);

    void PopAction(bool redoAction = true);

    void PeekAction();

    void Close();
}
#endregion

#region IConversationHost
/// <summary>
/// Defines an interface for hosting conversations.
/// </summary>
public interface IConversationHost : IConversationHandler
{
    void StartConversation(IConversation conversation);
    void StartConversation(string name, IConversation conversation);
    void StopCurrentConversation();
    void HandleMessageInput(string message);
    void HandleButtonClick(string button);
    void HandleFileStream(Stream stream);
}
#endregion

#region IConversationHostAsync
/// <summary>
/// Defines an async interface for hosting conversations.
/// </summary>
public interface IConversationHostAsync : IConversationHost
{
    Task<object> HandleMessageInputAsync(string message, CancellationToken cancel);
    Task<object> HandleButtonClickAsync(string button, CancellationToken cancel);
    Task<object> HandleFileStreamAsync(Stream stream, CancellationToken cancel);
}
#endregion

#region IConversation
/// <summary>
/// Defines an interface for a conversation.
/// </summary>
public interface IConversation
{
    void StartConversation(IConversationHandler handler);

    void StopConversation();

    void HandleInput();
}
#endregion

#region IConversationAsync
/// <summary>
/// Defines an async interface for a conversation.
/// </summary>
public interface IConversationAsync : IConversation
{
    Task<object> HandleInputAsync(CancellationToken cancel);
}
#endregion

#region IDIalogItem
/// <summary>
/// Defines an interface for dialog items.
/// </summary>
public interface IDIalogItem
{
}

/// <summary>
/// Represents an empty dialog item.
/// </summary>
public class EmptyDialogItem : IDIalogItem
{
    public static EmptyDialogItem Empty { get; } = new();

    private EmptyDialogItem()
    {
    }
}

/// <summary>
/// Represents a disposable dialog item that can be removed.
/// </summary>
public class DisposableDialogItem : IDisposable
{
    public IConversationHandler Handler { get; }
    public IDIalogItem DialogItem { get; }

    public DisposableDialogItem(IConversationHandler handler, IDIalogItem dialogItem)
    {
        Handler = handler;
        DialogItem = dialogItem;
    }

    public void Dispose()
    {
        Handler.RemoveMessage(DialogItem);
    }
}

#endregion

#region IDialogMessage
/// <summary>
/// Defines an interface for dialog messages.
/// </summary>
public interface IDialogMessage : IDIalogItem
{
    TextStatus Status { get; }
    ConversationRole Role { get; }
    string Message { get; }


    void AddText(string text);

    void AddCode(string code);

    void AddButton(string key, string text, Action callBack = null);

    void AddButtons(string title, IEnumerable<ConversationButton> buttons);

    void AddProgressBar(float progress, float max);

    void AddLine();
}
#endregion

#region ConversationButton
/// <summary>
/// Represents a button in a conversation.
/// </summary>
public class ConversationButton
{
    public string Key { get; set; }
    public string Text { get; set; }
    public Action CallBack { get; set; }
}
#endregion