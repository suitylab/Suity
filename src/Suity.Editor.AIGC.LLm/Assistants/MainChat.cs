using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Main chat implementation that manages workflow-based LLM conversations.
/// Acts as a proxy to the selected workflow chat provider.
/// </summary>
internal class MainChat : ILLmChat
{
    /// <summary>
    /// Selection type for choosing an LLM chat provider asset.
    /// </summary>
    private class ChatSelection : AssetSelection<ILLmChatProvider>
    {
    }

    //TODO: Add startup mechanism here. When not started, there is no UI. When started, selection is disabled

    private ChatSelection _chatAssetSel = new();
    private readonly PropertyTarget _chatAssetTarget;

    private ILLmChat _innerChat;
    private ILLmChat _lastChat;

    /// <summary>
    /// Reference to the main ImGui node for the chat conversation.
    /// </summary>
    protected readonly ImGuiNodeRef _guiRef = new();
    /// <summary>
    /// Reference to the ImGui node for the chat settings panel.
    /// </summary>
    protected readonly ImGuiNodeRef _guiSettingRef = new();

    /// <summary>
    /// Current state of the LLM chat session.
    /// </summary>
    protected LLmChatStates _state;

    private readonly IConversationImGui _conversation;
    private readonly FunctionContext _context;

    /// <summary>
    /// Gets or sets the selected LLM chat provider asset.
    /// </summary>
    public ILLmChatProvider SelectedAsset
    {
        get => _chatAssetSel.Target;
        set => _chatAssetSel.Target = value;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the selected chat asset.
    /// </summary>
    public Guid SelectedChatAssetId
    {
        get => _chatAssetSel.Id;
        set { _chatAssetSel.Id = value; }
    }

    /// <summary>
    /// Gets the function context associated with this chat.
    /// </summary>
    public FunctionContext Context => _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainChat"/> class with a default conversation.
    /// </summary>
    public MainChat()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainChat"/> class with the specified conversation.
    /// </summary>
    /// <param name="conversation">The conversation UI handler. If null, a default one is created.</param>
    public MainChat(IConversationImGui conversation)
    {
        conversation ??= EditorServices.ImGuiService.CreateConversationImGui(nameof(MainChat), false);

        _conversation = conversation ?? throw new ArgumentNullException(nameof(conversation));

        _chatAssetTarget = PropertyTargetUtility.CreatePropertyTarget(_chatAssetSel, "Select workflow");
        _chatAssetSel.Target = MainAssistantChatProvider.Instance;

        _context = new();
        _context.SetArgument<ILLmChat>(this);
        _context.SetArgument<IConversationHandler>(_conversation);
        _context.SetArgument<IConversationHost>(_conversation as IConversationHost);
        _context.SetArgument<IConversationHostAsync>(_conversation as IConversationHostAsync);
    }

    #region ILLmChat

    /// <summary>
    /// Gets the current state of the LLM chat session.
    /// </summary>
    public LLmChatStates State => _state;

    /// <summary>
    /// Starts a new conversation with the specified message.
    /// </summary>
    /// <param name="msg">The initial message to send.</param>
    /// <param name="attachments">Optional attachments to include.</param>
    /// <param name="option">Optional configuration options.</param>
    /// <returns>The response object from the chat provider, or null if the operation failed.</returns>
    public virtual async Task<object> Start(string msg, object attachments = null, object option = null)
    {
        if (_state != LLmChatStates.Stopped)
        {
            return null;
        }

        _state = LLmChatStates.Starting;

        try
        {
            _innerChat?.Dispose();
            _innerChat = null;
        }
        catch (Exception err)
        {
            err.LogError();
        }

        try
        {
            var chat = await GetOrCreateChat(option);
            if (chat is null)
            {
                await DialogUtility.ShowMessageBoxAsyncL("No workflow selected.");
                return null;
            }

            _chatAssetTarget.ReadOnly = true;
            _state = LLmChatStates.Started;

            return await chat.Start(msg, attachments, option);
        }
        catch (Exception err)
        {
            err.LogErrorL("Failed to start workflow.");
            return null;
        }
        finally
        {
            _chatAssetTarget.ReadOnly = false;
            _state = LLmChatStates.Stopped;
        }
    }

    /// <summary>
    /// Stops the current conversation.
    /// </summary>
    public virtual void Stop()
    {
        if (_state != LLmChatStates.Started)
        {
            return;
        }

        _state = LLmChatStates.Stopped;

        _innerChat?.Stop();

        _chatAssetTarget.ReadOnly = false;
    }

    /// <summary>
    /// Clears the conversation history.
    /// </summary>
    public virtual void Clear()
    {
        _innerChat?.Clear();
    }

    /// <summary>
    /// Renders the settings GUI for selecting and configuring the chat workflow.
    /// </summary>
    /// <param name="gui">The ImGui context to render into.</param>
    public void OnSettingGui(ImGui gui)
    {
        _chatAssetTarget.Description = L("Select Workflow");

        _guiSettingRef.Node = gui.HorizontalLayout("#chat_asset_setting")
        .InitFullHeight()
        .InitFit(GuiOrientation.Horizontal)
        .OnContent(() =>
        {
            gui.HorizontalLayout("#sel")
            .InitWidth(200)
            .InitFullHeight()
            .OnContent(() =>
            {
                _chatAssetTarget.ReadOnly = _state != LLmChatStates.Stopped;

                gui.PropertyEditor(_chatAssetTarget, act =>
                {
                    act.DoAction();
                    _chatAssetSel = _chatAssetTarget.GetValues().FirstOrDefault() as ChatSelection ?? _chatAssetSel;

                    _innerChat?.Dispose();
                    _innerChat = null;

                    _chatAssetSel.Target ??= MainAssistantChatProvider.Instance;

                    _guiSettingRef.QueueRefresh();
                })
                ?.SetEnabled(_state == LLmChatStates.Stopped);
            });

            gui.Button("gotoAsset", CoreIconCache.GotoDefination)
            .InitClass("toolBtn")
            .SetToolTipsL("Go to Definition")
            .OnClick(() =>
            {
                EditorUtility.GotoDefinition(_chatAssetSel.Id);
            });
            
            _innerChat?.OnSettingGui(gui);
        });
    }

    /// <summary>
    /// Sends a message in the current conversation. If not started, starts a new conversation.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="attachments">Optional attachments to include.</param>
    /// <param name="option">Optional configuration options.</param>
    /// <returns>The response object from the chat provider, or null if the operation failed.</returns>
    public virtual async Task<object> Send(string msg, object attachments = null, object option = null)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return null;
        }

        if (_state != LLmChatStates.Started)
        {
            return await Start(msg, attachments, option);
            //throw new InvalidOperationException($"Conversation is not started.");
        }

        // Start process already includes GetOrCreateChat(), no need to call again
        var chat = _innerChat; // GetOrCreateChat();
        if (chat is null)
        {
            throw new AigcException(L("No workflow selected."));
        }

        try
        {
            return await chat.Send(msg, attachments, option);
        }
        catch (Exception err)
        {
            _conversation.AddException(err);

            return null;
        }
        finally
        {
            if (chat.State == LLmChatStates.Stopped)
            {
                this.Stop();
            }
            
            this._guiRef.QueueRefresh();
        }
    }

    /// <summary>
    /// Starts the conversation without an initial message.
    /// </summary>
    /// <param name="option">Optional configuration options.</param>
    /// <returns>The response object from the chat provider, or null if the operation failed.</returns>
    public virtual Task<object> StartWithoutMessage(object option = null) => Start(null, option:option);

    #endregion

    #region IDrawImGuiNode

    /// <summary>
    /// Renders the main chat node GUI by delegating to the inner chat implementation.
    /// </summary>
    /// <param name="gui">The ImGui context to render into.</param>
    /// <returns>The ImGui node representing the chat UI.</returns>
    public ImGuiNode OnNodeGui(ImGui gui)
    {
        _guiRef.Node = _innerChat?.OnNodeGui(gui);

        return _guiRef.Node;
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases resources used by this chat instance by stopping the current conversation.
    /// </summary>
    public virtual void Dispose() => Stop();

    #endregion

    private async Task<ILLmChat> GetOrCreateChat(object option)
    {
        if (_innerChat != null)
        {
            return _innerChat;
        }

        var asset = _chatAssetSel.Target;
        if (asset is null)
        {
            if (!await _chatAssetSel.ShowSelectionGUIAsync(L("Select Workflow")))
            {
                return null;
            }

            asset = _chatAssetSel.Target;
            if (asset is null)
            {
                return null;
            }
        }

        var ctx = new FunctionContext(_context);

        if (option != null)
        {
            ctx.SetArgument(option.GetType().FullName, option);
        }

        _innerChat = asset?.CreateChat(ctx);

        return _innerChat;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Workflow Chat");
}
