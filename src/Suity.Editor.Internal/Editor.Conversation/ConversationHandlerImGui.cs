using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Conversation;

/// <summary>
/// Handles conversation UI rendering using ImGui and manages conversation lifecycle, input handling, and message display.
/// </summary>
public class ConversationHandlerImGui :
    IConversationImGui,
    IConversationHandler,
    IConversationHost,
    IConversationHostAsync,
    IDrawImGuiNode
{
    private readonly string _id;

    private readonly ImGuiNodeRef _guiRef = new();

    private readonly ValueStore<IConversation> _conversation = new();
    private readonly Stack<Action> _actionStack = new();
    private IEnumerator _coroutine;
    private DisposeCollector _contentListeners;

    private readonly List<DialogItem> _items = [];

    private bool _scrollToButtom;

    private readonly DialogMenuRootCommand _menu = new();

    private long _idAlloc;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationHandlerImGui"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this conversation handler.</param>
    public ConversationHandlerImGui(string id)
    {
        _id = id;
    }

    /// <summary>
    /// Gets or sets whether old messages should be disabled during rendering.
    /// </summary>
    public bool DisableOldMessage { get; set; } = true;

    /// <summary>
    /// Scrolls the conversation view to the bottom.
    /// </summary>
    public void ScrollToBottom()
    {
        _guiRef.Node?.SetScrollRateY(1);

        _scrollToButtom = true;
    }

    /// <summary>
    /// Advances the current coroutine and returns its current yield value.
    /// </summary>
    /// <returns>The current yield value of the coroutine, or null if no coroutine is running or it has completed.</returns>
    public object MoveCoroutine()
    {
        var coroutine = _coroutine;

        if (coroutine is null)
        {
            return null;
        }

        try
        {
            if (coroutine.MoveNext())
            {
                return coroutine.Current;
            }
            else
            {
                // Coroutine was replaced during operation
                if (ReferenceEquals(_coroutine, coroutine))
                {
                    _coroutine = null;
                }

                return null;
            }
        }
        catch (Exception err)
        {
            Logs.LogError(err);

            return null;
        }
    }

    /// <inheritdoc/>
    public ImGuiNode OnNodeGui(ImGui gui)
    {
        var node =_guiRef.Node = gui.ScrollableFrame($"#conversation_{_id}", GuiOrientation.Vertical)
        .InitFullWidth()
        .InitHeightRest()
        .InitChildSpacing(5)
        .OnPartialContent(() =>
        {
            if (DisableOldMessage)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    item.OnGui(gui, i, i == _items.Count - 1, _menu, this);
                }
            }
            else
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    item.OnGui(gui, i, true, _menu, this);
                }
            }
        })
        .AutoScrollToBottom();

        return node;
    }

    #region IConversationHost

    /// <inheritdoc/>
    public void StartConversation(IConversation conversation)
    {
        StartConversation(null, conversation);
    }

    /// <inheritdoc/>
    public void StartConversation(string name, IConversation conversation)
    {
        StartConversation(name, name, conversation);
    }

    /// <inheritdoc/>
    public void StartConversation(string name, string title, IConversation conversation)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        ConversationName = name;

        _conversation.PickUp()?.StopConversation();
        _items.Clear();
        _coroutine = null;
        _actionStack.Clear();

        _conversation.Set(conversation);

        Title = title ?? name;

        QueuedAction.Do(() =>
        {
            _conversation.Get().StartConversation(this);

            _guiRef.QueueRefresh();
        });
    }

    /// <inheritdoc/>
    public void StopCurrentConversation()
    {
        try
        {
            _conversation.PickUp()?.StopConversation();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        _actionStack.Clear();
        _coroutine = null;
        _contentListeners?.Dispose();
        _contentListeners = null;

        InputMessage = null;
        InputButton = null;
        InputStream?.Dispose();
        InputStream = null;
    }

    /// <inheritdoc/>
    public void HandleMessageInput(string message)
    {
        InputMessage = message;
        InputButton = null;
        InputStream?.Dispose();
        InputStream = null;

        HandleNextInput();
    }

    /// <inheritdoc/>
    public void HandleButtonClick(string button)
    {
        InputMessage = null;
        InputButton = button;
        InputStream?.Dispose();
        InputStream = null;

        HandleNextInput();
    }

    /// <inheritdoc/>
    public void HandleFileStream(Stream stream)
    {
        InputMessage = null;
        InputButton = null;
        InputStream?.Dispose();
        InputStream = stream;

        HandleNextInput();
    }

    private void HandleNextInput()
    {
        if (_coroutine != null)
        {
            MoveCoroutine();
        }
        else
        {
            try
            {
                var c = _conversation.Get();
                c?.HandleInput();
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }
        }
    }

    #endregion

    #region IConversationHostAsync

    /// <inheritdoc/>
    public Task<object> HandleMessageInputAsync(string message, CancellationToken cancel)
    {
        InputMessage = message;
        InputButton = null;
        InputStream?.Dispose();
        InputStream = null;

        return HandleNextInputAsync(cancel);
    }

    /// <inheritdoc/>
    public Task<object> HandleButtonClickAsync(string button, CancellationToken cancel)
    {
        InputMessage = null;
        InputButton = button;
        InputStream?.Dispose();
        InputStream = null;

        return HandleNextInputAsync(cancel);
    }

    /// <inheritdoc/>
    public Task<object> HandleFileStreamAsync(Stream stream, CancellationToken cancel)
    {
        InputMessage = null;
        InputButton = null;
        if (!ReferenceEquals(stream, InputStream))
        {
            InputStream?.Dispose();
            InputStream = stream;
        }

        return HandleNextInputAsync(cancel);
    }

    private Task<object> HandleNextInputAsync(CancellationToken cancel)
    {
        if (_coroutine != null)
        {
            if (MoveCoroutine() is Task<object> task)
            {
                return task;
            }
            else
            {
                return Task.FromResult<object>(null);
            }
        }
        else
        {
            try
            {
                var c = _conversation.Get();
                if (c is IConversationAsync cAsync)
                {
                    return cAsync.HandleInputAsync(cancel);
                }
                else
                {
                    c?.HandleInput();
                    return Task.FromResult<object>(null);
                }
            }
            catch (Exception err)
            {
                Logs.LogError(err);

                return Task.FromResult<object>(null);
            }
        }
    }

    #endregion

    #region IConversationHandler

    /// <inheritdoc/>
    public string ConversationName { get; private set; }

    /// <inheritdoc/>
    public string Title { get; set; }

    /// <inheritdoc/>
    public string InputMessage { get; private set; }

    /// <inheritdoc/>
    public string InputButton { get; private set; }

    /// <inheritdoc/>
    public Stream InputStream { get; private set; }

    /// <inheritdoc/>
    public IDialogMessage LastMessage { get; private set; }

    /// <inheritdoc/>
    public IDIalogItem AddMessage(string content, ConversationRole role, TextStatus status, Action<IDialogMessage> config = null)
    {
        DialogItem item = new DialogItem
        {
            Role = role,
            Message = content,
            Status = status,
            Id = (++_idAlloc).ToString(),
        };

        config?.Invoke(item);

        _items.Add(item);

        ScrollToBottom();
        _guiRef.QueueRefresh();

        return item;
    }

    /// <inheritdoc/>
    public void RemoveMessage(IDIalogItem item)
    {
        if (item is null)
        {
            return;
        }

        if (item is DialogItem o && _items.Remove(o))
        {
            _guiRef.QueueRefresh();
        }
    }

    /// <inheritdoc/>
    public void RemoveMessages(Predicate<IDIalogItem> predicate)
    {
        _items.RemoveAll(predicate);
        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    public void RemoveDebugMessages()
    {
        _items.RemoveAll(o => o.Role == ConversationRole.Debug);
        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    void IConversationHandler.StartCoroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

        MoveCoroutine();
    }

    /// <inheritdoc/>
    void IConversationHandler.StopCoroutine(IEnumerator coroutine)
    {
        if (Equals(coroutine, _coroutine))
        {
            _coroutine = null;
        }
    }

    /// <inheritdoc/>
    void IConversationHandler.PushAction(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _actionStack.Push(action);

        action();
    }

    /// <inheritdoc/>
    void IConversationHandler.PushAction<T>(Action<T> action, T value)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        void act() => action(value);

        _actionStack.Push(act);

        action(value);
    }

    /// <inheritdoc/>
    void IConversationHandler.PushCoroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

        void act() => (this as IConversationHandler).StartCoroutine(coroutine);

        _actionStack.Push(act);

        (this as IConversationHandler).StartCoroutine(coroutine);
    }

    /// <inheritdoc/>
    void IConversationHandler.PopAction(bool redoAction)
    {
        if (_actionStack.Count > 0)
        {
            _actionStack.Pop();
        }

        if (redoAction && _actionStack.Count > 0)
        {
            _actionStack.Peek().Invoke();
        }
    }

    /// <inheritdoc/>
    void IConversationHandler.PeekAction()
    {
        if (_actionStack.Count > 0)
        {
            _actionStack.Peek().Invoke();
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        _items.Clear();
        _guiRef.QueueRefresh();
    }

    #endregion
}