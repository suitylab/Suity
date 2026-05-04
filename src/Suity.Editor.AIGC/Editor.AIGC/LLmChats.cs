using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

#region BaseLLmChat

/// <summary>
/// Base abstract class for LLM chat implementations, providing conversation management, state handling, and ImGui integration.
/// </summary>
public abstract class BaseLLmChat : ILLmChat,
    IConversationAsync
{
    private readonly string _name;
    private readonly string _text;

    protected readonly IConversationImGui _conversation;

    protected readonly FunctionContext _context;

    protected readonly ImGuiNodeRef _guiRef = new();

    private LLmChatStates _state;

    protected CancellationTokenSource _cancel;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLLmChat"/> class with a name and optional text and context.
    /// </summary>
    /// <param name="name">The name of the chat session.</param>
    /// <param name="text">Optional display text for the chat.</param>
    /// <param name="context">Optional function context for the chat.</param>
    public BaseLLmChat(string name, string text = null, FunctionContext context = null)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _text = text;
        _context = context ?? new FunctionContext();

        _conversation = EditorServices.ImGuiService.CreateConversationImGui(name, false);
        (_conversation as IConversationHost)?.StartConversation(this);

        _context.SetArgument<ILLmChat>(this);
        _context.SetArgument<IConversationHandler>(_conversation);
        _context.SetArgument<IConversationHost>(_conversation as IConversationHost);
        _context.SetArgument<IConversationHostAsync>(_conversation as IConversationHostAsync);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLLmChat"/> class with an existing conversation handler.
    /// </summary>
    /// <param name="conversation">The conversation handler to use.</param>
    /// <param name="name">The name of the chat session.</param>
    /// <param name="text">Optional display text for the chat.</param>
    /// <param name="context">Optional function context for the chat.</param>
    protected BaseLLmChat(IConversationHandler conversation, string name, string text = null, FunctionContext context = null)
    {
        
    }

    #region ILLmChat

    /// <summary>
    /// Gets the current state of the LLM chat session.
    /// </summary>
    public LLmChatStates State => _state;


    /// <summary>
    /// Starts a new chat session with an optional message.
    /// </summary>
    /// <param name="msg">The initial message to send, or null to start without a message.</param>
    /// <param name="attachments">Optional attachments to include with the message.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, or null if the chat is not in a stopped state.</returns>
    public async Task<object> Start(string msg, object attachments = null, object option = null)
    {
        if (_state != LLmChatStates.Stopped)
        {
            return null;
        }

        try
        {
            _state = LLmChatStates.Starting;

            _cancel?.Dispose();
            _cancel = new CancellationTokenSource();

            await OnStart(_cancel.Token);

            _state = LLmChatStates.Started;

            if (!string.IsNullOrWhiteSpace(msg))
            {
                _conversation.AddUserMessage(msg, attachments as AttachmentSet[]);
                // return await _conversation.HandleMessageInputAsync(msg, _cancel.Token);
                return await HandleStart(msg, option, _cancel.Token);
            }
            else
            {
                return await HandleStart(null, option, _cancel.Token);
            }
        }
        //catch (ExecuteException err)
        //{
        //    _state = LLmChatStates.Stopped;

        //    string errMsg;
        //    if (EnumHelper.TryParseEnumValue(err.StatusCode, out var v))
        //    {
        //        errMsg = $"Startup failed: {err.Message}({v.ToDisplayText()})";
        //    }
        //    else
        //    {
        //        errMsg = $"Startup failed: {err.Message}({err.StatusCode})";
        //    }

        //    _conversation.AddErrorMessage(errMsg);
        //    DialogUtility.ShowMessageBoxAsync(errMsg);
        //    return null;
        //}
        catch (OperationCanceledException)
        {
            _state = LLmChatStates.Stopped;
            _conversation.AddSystemMessage("Operation cancelled");
            return null;
        }
        catch (AigcException llmErr)
        {
            _state = LLmChatStates.Stopped;
            _conversation.AddException(llmErr, "Start failed.");
            return null;
        }
        catch (Exception err)
        {
            _state = LLmChatStates.Stopped;
            _conversation.AddException(err, "Start failed");
            return null;
        }
        finally
        {
            var cancel = _cancel;
            _cancel = null;

            cancel?.Dispose();

            try
            {
                Stop();
                _guiRef.QueueRefresh();
            }
            catch (Exception stopErr)
            {
                stopErr.LogError();
            }
        }
    }

    /// <summary>
    /// Starts a chat session without an initial message.
    /// </summary>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task<object> StartWithoutMessage(object option = null) => Start(null, option:option);


    /// <summary>
    /// Stops the current chat session and cancels any ongoing operations.
    /// </summary>
    public void Stop()
    {
        if (_state != LLmChatStates.Started)
        {
            return;
        }

        _state = LLmChatStates.Stopped;

        var cancel = _cancel;
        _cancel = null;

        cancel?.Cancel();
        cancel?.Dispose();
        
        OnStop();
    }

    /// <summary>
    /// Clears the current conversation and closes the chat UI.
    /// </summary>
    public void Clear()
    {
        _conversation.Close();

        OnClear();
    }

    /// <summary>
    /// Called to configure or customize the ImGui interface for this chat.
    /// </summary>
    /// <param name="gui">The ImGui instance to configure.</param>
    public virtual void OnSettingGui(ImGui gui)
    {
    }

    /// <summary>
    /// Sends a message to the ongoing chat session or starts a new session if not already started.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="attachments">Optional attachments to include with the message.</param>
    /// <param name="option">Optional configuration for the message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task<object> Send(string msg, object attachments = null, object option = null)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return null;
        }

        if (_cancel != null)
        {
            _conversation.AddUserMessage(msg, attachments as AttachmentSet[]);
            //_conversation.AddErrorMessage($"A task is currently being processed and cannot accept new information.");

            if (_conversation is IConversationHostAsync hostAsync)
            {
                return await hostAsync.HandleMessageInputAsync(msg, _cancel.Token);
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (_state != LLmChatStates.Started)
            {
                throw new InvalidOperationException($"Conversation is not started.");
            }

            return await Start(msg, attachments, option);
        }
    }


    /// <summary>
    /// Called when the chat session is starting. Override to perform initialization logic.
    /// </summary>
    /// <param name="cancel">The cancellation token for the session.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnStart(CancellationToken cancel)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the chat session is stopping. Override to perform cleanup logic.
    /// </summary>
    protected virtual void OnStop()
    {
    }

    /// <summary>
    /// Called when the conversation is being cleared. Override to perform cleanup logic.
    /// </summary>
    protected virtual void OnClear()
    {
    }

    #endregion

    #region IDrawImGuiNode
    /// <summary>
    /// Renders the conversation node in the ImGui interface.
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering.</param>
    /// <returns>The rendered ImGui node.</returns>
    public virtual ImGuiNode OnNodeGui(ImGui gui)
    {
        bool first = _guiRef.Node is null;

        var node = _conversation?.OnNodeGui(gui);
        _guiRef.Node = node;

        if (first)
        {
            _guiRef.QueueRefresh();
        }

        return node;
    }
    #endregion

    #region IDisposable
    /// <summary>
    /// Releases resources used by the chat session and closes the conversation.
    /// </summary>
    public virtual void Dispose()
    {
        _conversation.Close();
        _guiRef.Node = null;
    }
    #endregion

    #region IConversationAsync

    void IConversation.StartConversation(IConversationHandler handler)
    {
    }

    void IConversation.StopConversation()
    {
    }

    void IConversation.HandleInput()
    {
        HandleConversation(_conversation);
    }

    Task<object> IConversationAsync.HandleInputAsync(CancellationToken cancel)
    {
        HandleConversation(_conversation);

        return Task.FromResult<object>(null);
    }

    /// <summary>
    /// Handles the start of a conversation with the given message. Override to implement custom start logic.
    /// </summary>
    /// <param name="msg">The message to process, or null.</param>
    /// <param name="option">Optional configuration for the conversation.</param>
    /// <param name="cancel">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task<object> HandleStart(string msg, object option, CancellationToken cancel)
    {
        return Task.FromResult<object>(null);
    }

    /// <summary>
    /// Handles conversation input. Override to implement custom conversation logic.
    /// </summary>
    /// <param name="handler">The conversation handler to interact with.</param>
    protected virtual void HandleConversation(IConversationHandler handler)
    {
    }

    #endregion


    public override string ToString() => _text ?? _name;
}

#endregion

#region EmptyLLmChat

/// <summary>
/// A placeholder LLM chat implementation that represents an empty/no-op chat session.
/// </summary>
public sealed class EmptyLLmChat : BaseLLmChat
{
    /// <summary>
    /// Gets the singleton instance of the empty LLM chat.
    /// </summary>
    public static EmptyLLmChat Empty { get; } = new();

    private EmptyLLmChat()
        : base("(Empty)")
    {
    }
}

#endregion

#region ManualLLmChat

/// <summary>
/// A manual LLM chat implementation that copies messages to clipboard and allows pasting external conversation results.
/// </summary>
public class ManualLLmChat : BaseLLmChat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManualLLmChat"/> class.
    /// </summary>
    /// <param name="context">Optional function context for the chat.</param>
    public ManualLLmChat(FunctionContext context = null)
        : base("Manual", "Manual", context)
    {
    }

    /// <summary>
    /// Handles the start of a manual chat session by copying the message to clipboard and prompting the user to paste external results.
    /// </summary>
    /// <param name="msg">The message to process.</param>
    /// <param name="option">Optional configuration.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task<object> HandleStart(string msg, object option, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return null;
        }

        bool setOk = await EditorUtility.SetSystemClipboardText(msg);
        if (!setOk)
        {
            _conversation.AddErrorMessage("Copy failed, please manually copy the conversation content.");
            return null;
        }

        _conversation.AddDebugMessage("Please copy external conversation result, and click [Paste] button to input to this conversation.", config =>
        {
            config.AddButton("Paste", () =>
            {
                EditorUtility.GetSystemClipboardText().ContinueWith(t => 
                {
                    string text = t.Result;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        ProcessMarkdown(text);
                    }

                    _conversation.RemoveDebugMessages();
                    
                    _guiRef.QueueRefresh();
                });
            });
        });

        _guiRef.QueueRefresh();

        return null;
    }

    private void ProcessMarkdown(string msg)
    {
        _conversation.AddMessage(string.Empty, ConversationRole.Remote, TextStatus.Normal, config =>
        {
            config.AddMarkdigMessage(msg);
        });
    }
}

#endregion
