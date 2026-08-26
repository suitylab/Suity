using Suity.Collections;
using Suity.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Conversation;

public class ConversationHost :
    IConversationHost,
    IConversationHostAsync,
    IConversation
{
    private readonly string _id;

    private readonly ValueStore<IConversationClient> _client = new();
    private readonly Stack<Action> _actionStack = new();
    private IEnumerator _coroutine;
    private DisposeCollector _contentListeners;

    private readonly List<DialogItem> _items = [];

    private long _idAlloc;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationImGui"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this conversation handler.</param>
    public ConversationHost(string id)
    {
        _id = id;
    }

    public string Id => _id;
    
    /// <summary>
    /// Gets or sets whether old messages should be disabled during rendering.
    /// </summary>
    public bool DisableOldMessage { get; set; } = true;

    public ConversationLevels Level { get; set; } = ConversationLevels.Main;

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

    protected void FillItems(ICollection<DialogItem> collection)
    {
        lock (_items)
        {
            collection.AddRange(_items);
        }
    }

    #region IConversationHost

    /// <inheritdoc/>
    public void StartConversation(IConversationClient conversation)
    {
        StartConversation(null, conversation);
    }

    /// <inheritdoc/>
    public void StartConversation(string name, IConversationClient conversation)
    {
        StartConversation(name, name, conversation);
    }

    /// <inheritdoc/>
    public void StartConversation(string name, string title, IConversationClient conversation)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        ConversationName = name;

        _client.PickUp()?.StopConversation();
        lock (_items)
        {
            _items.Clear();
        }

        _coroutine = null;
        _actionStack.Clear();

        _client.Set(conversation);

        Title = title ?? name;

        QueuedAction.Do(() =>
        {
            _client.Get().StartConversation(this);

            OnRefreshRequested();
        });
    }

    /// <inheritdoc/>
    public void StopCurrentConversation()
    {
        try
        {
            _client.PickUp()?.StopConversation();
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
                var c = _client.Get();
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
                var c = _client.Get();
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

    #region IConversation

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

        lock (_items)
        {
            _items.Add(item);
        }

        OnScrollToButtonRequested();

        OnMessageAdded(item);

        return item;
    }

    /// <inheritdoc/>
    public void RemoveMessage(IDIalogItem item)
    {
        if (item is null)
        {
            return;
        }

        bool removed = false;
        lock (_items)
        {
            if (item is DialogItem o && _items.Remove(o))
            {
                removed = true;
            }
        }

        if (removed)
        {
            OnRefreshRequested();

            OnMessageRemoved((DialogItem)item);
        }
    }

    /// <inheritdoc/>
    public void RemoveMessages(Predicate<IDIalogItem> predicate)
    {
        DialogItem[] items = null;

        lock (_items)
        {
            items = _items.Where(v => predicate(v)).ToArray();
            _items.RemoveAll(predicate);
        }

        OnRefreshRequested();

        if (items?.Length > 0)
        {
            foreach (var item in items)
            {
                OnMessageRemoved(item);
            }
        }
    }

    /// <inheritdoc/>
    public void RemoveDebugMessages()
    {
        DialogItem[] items = null;

        lock (_items)
        {
            items = _items.Where(o => o.Role == ConversationRole.Debug).ToArray();
            _items.RemoveAll(o => o.Role == ConversationRole.Debug);
        }

        OnRefreshRequested();

        if (items?.Length > 0)
        {
            foreach (var item in items)
            {
                OnMessageRemoved(item);
            }
        }
    }

    /// <inheritdoc/>
    void IConversation.StartCoroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

        MoveCoroutine();
    }

    /// <inheritdoc/>
    void IConversation.StopCoroutine(IEnumerator coroutine)
    {
        if (Equals(coroutine, _coroutine))
        {
            _coroutine = null;
        }
    }

    /// <inheritdoc/>
    void IConversation.PushAction(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _actionStack.Push(action);

        action();
    }

    /// <inheritdoc/>
    void IConversation.PushAction<T>(Action<T> action, T value)
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
    void IConversation.PushCoroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

        void act() => (this as IConversation).StartCoroutine(coroutine);

        _actionStack.Push(act);

        (this as IConversation).StartCoroutine(coroutine);
    }

    /// <inheritdoc/>
    void IConversation.PopAction(bool redoAction)
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
    void IConversation.PeekAction()
    {
        if (_actionStack.Count > 0)
        {
            _actionStack.Peek().Invoke();
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        lock (_items)
        {
            _items.Clear();
        }

        OnRefreshRequested();
    }

    #endregion

    #region Virtual

    protected virtual void OnMessageAdded(DialogItem item)
    {
    }

    protected virtual void OnMessageRemoved(DialogItem item)
    {
    }

    protected virtual void OnRefreshRequested()
    {
    }

    protected virtual void OnScrollToButtonRequested()
    {
    }

    #endregion
}
