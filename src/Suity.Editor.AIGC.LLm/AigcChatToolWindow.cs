using Pathoschild.Http.Client;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Flows;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Tool window that provides the AI generation chat interface.
/// </summary>
public class AigcChatToolWindow : IToolWindow, IDrawImGui
{
    private const string WINDOW_ID = nameof(AigcChatToolWindow);

    /// <summary>
    /// Gets the singleton instance of the AI chat tool window.
    /// </summary>
    public static AigcChatToolWindow Instance { get; private set; }
    private static readonly ServiceStore<IAigcWorkflowRunner> _workflowRunner = new();


    private readonly ImGuiNodeRef _guiRef = new();

    private readonly FluentClient _client;

    private readonly GuiDropDownValue _modelSelect;

    private readonly MainChat _mainChat = new();
    private ILLmChat _currentChat;

    private string _msgInput = string.Empty;
    private object _msgOption;
    private readonly Dictionary<DocumentEntry, AttachmentSet> _attachments = [];

    public AigcChatToolWindow()
    {
        Instance ??= this;

        var chatManual = new ManualLLmChat();

        _currentChat = _mainChat;

        _modelSelect = new GuiDropDownValue(_mainChat, chatManual)
        {
            SelectedValue = _mainChat
        };

        EditorCommands.ShowChatView.AddActionListener(() => EditorUtility.ShowToolWindow(WINDOW_ID));
    }

    /// <summary>
    /// Gets or sets the selected chat provider.
    /// </summary>
    public ILLmChatProvider SelectedChatProvider
    {
        get
        {
            return _mainChat.SelectedAsset;
        }
        set
        {
            var id = (value as Asset)?.Id ?? Guid.Empty;
            if (_mainChat.SelectedChatAssetId != id)
            {
                _mainChat.SelectedChatAssetId = id;
                _guiRef.QueueRefresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets the ID of the selected chat asset.
    /// </summary>
    public Guid SelectedChatAssetId
    {
        get => _mainChat.SelectedChatAssetId;
        set => _mainChat.SelectedChatAssetId = value;
    }

    #region Config

    /// <summary>
    /// Sets the default configuration for the chat window.
    /// </summary>
    public void SetDefaultConfig()
    {
        _mainChat.SelectedChatAssetId = MainAssistantChatProvider.Instance?.Id ?? Guid.Empty;
    }

    /// <summary>
    /// Loads the configuration from the specified state.
    /// </summary>
    /// <param name="state">The configuration state to load from.</param>
    public void LoadConfig(AigcConfigState state)
    {
        _mainChat.SelectedChatAssetId = state.LastChatAssetId;
    }

    /// <summary>
    /// Saves the current configuration to the specified state.
    /// </summary>
    /// <param name="state">The configuration state to save to.</param>
    public void SaveConfig(AigcConfigState state)
    {
        state.LastChatAssetId = _mainChat.SelectedChatAssetId;
    }

    #endregion

    #region IToolWindow

    string IToolWindow.WindowId => WINDOW_ID;

    string IToolWindow.Title => "AI Generation";

    ImageDef IToolWindow.Icon => CoreIconCache.AI;

    DockHint IToolWindow.DockHint => DockHint.Left;

    bool IToolWindow.CanDockDocument => false;

    object IToolWindow.GetUIObject() => this;

    void IToolWindow.NotifyHide() { }

    void IToolWindow.NotifyShow() 
    {
        _guiRef.QueueRefresh(true);
    }

    #endregion

    #region IDrawImGui

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _guiRef.Node = gui.Frame()
        .InitClass("bg")
        .InitSizeRest()
        .InitPadding(3)
        .OnContent(() =>
        {
            gui.HorizontalLayout("tool_bar")
            .InitFullWidth()
            .InitHeight(32)
            .OnContent(() =>
            {
                //gui.DropDownButton("model_select", _modelSelect)
                //.InitWidth(200)
                //.InitClass("propInput")
                //.OnEdited(n =>
                //{
                //    _currentChat = _modelSelect.SelectedValue as IChatConversation;
                //});

                if (_currentChat != null)
                {
                    if (_currentChat.State == LLmChatStates.Started)
                    {
                        gui.Button("stop", CoreIconCache.Stop)
                        .InitClass("toolBtn")
                        .SetToolTipsL("Stop")
                        .OnClick(HandleStop);
                    }
                    else
                    {
                        gui.Button("start", CoreIconCache.Start)
                        .InitClass("toolBtn")
                        .SetToolTipsL("Start")
                        .OnClick(HandleStart)
                        .SetEnabled(_currentChat.State == LLmChatStates.Stopped);
                    }

                    _currentChat.OnSettingGui(gui);
                }

                gui.Button("clear", CoreIconCache.Delete)
                .InitClass("toolBtn")
                .SetToolTipsL("Clear Chat History")
                .OnClick(() =>
                {
                    _currentChat?.Clear();
                    _guiRef.QueueRefresh();
                });
            });

            gui.VerticalLayout("#dialog")
            .InitFullWidth()
            .InitHeightRest(120)
            .OnContent(() =>
            {
                _currentChat?.OnNodeGui(gui);
            });

            bool started = _currentChat?.State == LLmChatStates.Started;

            gui.VerticalResizer(30, null)
            .InitFullWidth()
            .InitClass("resizer");

            if (_attachments.Count > 0)
            {
                foreach (var attachment in _attachments.Values)
                {
                    attachment.OnGui(gui, att =>
                    {
                        QueuedAction.Do(() =>
                        {
                            _attachments.Remove(att.Document);
                            _guiRef.QueueRefresh();
                        });
                    });
                }
            }
            
            _msgInput = gui.TextAreaInput("msgInput", _msgInput, autoFit: false, submitMode: TextBoxEditSubmitMode.Enter)
            //.SetEnabled(started)
            .InitMargin(5)
            .InitHeightRest(30)
            .InitFullWidth()
            .InitInputFunctionChain(TextInput)
            .SetHintTextL("Input message...")
            .Text;

            gui.HorizontalReverseLayout("input_bar")
            //.SetEnabled(started)
            .InitFullWidth()
            .InitHeightRest()
            .OnContent(() =>
            {
                gui.Button("send", CoreIconCache.Send)
                .InitClass("simpleBtn")
                .InitWidth(50)
                .InitFullHeight()
                .SetToolTipsL("Send")
                .OnClick(() =>
                {
                    ProcessInput();
                });

                // Temporarily disabled adding attachments
                //gui.Button("+")
                //.InitClass("simpleBtn")
                //.InitWidth(50)
                //.InitFullHeight()
                //.InitToolTips("Add Materials")
                //.OnClicked(() =>
                //{
                //    var menu = AigcAttachMenu.Instance;
                //    menu.ApplySender(this);
                //    (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu);
                //});
            });
        });
    }

    private void HandleStart()
    {
        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.AigcWorkflow))
        //{
        //    DialogUtility.ShowMessageBoxAsync(ServiceInternals._license.GetFailedMessage(EditorCapabilities.AigcWorkflow));
        //    return;
        //}

        //if (ServiceInternals._license.GetMaxUsageReach())
        //{
        //    DialogUtility.ShowMessageBoxAsync(ServiceInternals._license.GetUsageFailedMessage());
        //    return;
        //}

        StartCurrentChat().GetAwaiter().OnCompleted(() => { });
    }

    private void HandleStop()
    {
        try
        {
            _currentChat.Stop();
        }
        catch (Exception err)
        {
            err.LogError();
        }

        _guiRef.QueueRefresh();
    }

    private GuiInputState TextInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (input.EventType == GuiEventTypes.KeyDown && input.KeyCode == "Return" && node.IsMouseInClickRect)
        {
            ProcessInput();
        }

        return state;
    }


    #endregion

    #region Input & Attachment

    /// <summary>
    /// Inputs a chat message to the specified chat provider and processes it.
    /// </summary>
    /// <param name="chatProvider">The chat provider to use.</param>
    /// <param name="msgInput">The message input.</param>
    /// <param name="option">Optional chat options.</param>
    /// <returns>A task representing the chat operation.</returns>
    public Task<object> InputChat(ILLmChatProvider chatProvider, string msgInput, object option = null)
    {
        if (string.IsNullOrWhiteSpace(msgInput))
        {
            return null;
        }

        if (chatProvider is null)
        {
            return null;
        }

        if (_currentChat != null && _currentChat.State != LLmChatStates.Stopped)
        {
            DialogUtility.ShowMessageBoxAsyncL("Can't start a new chat while the current one is not stopped.");
            return null;
        }

        SelectedChatProvider = chatProvider;
        if (_mainChat.SelectedAsset is null)
        {
            return null;
        }

        _currentChat?.Stop();
        _currentChat?.Clear();

        _msgInput = msgInput;
        _msgOption = option;

        return ProcessInput();
    }

    /// <summary>
    /// Starts a chat session with the specified chat provider.
    /// </summary>
    /// <param name="chatProvider">The chat provider to use.</param>
    /// <param name="option">Optional chat options.</param>
    /// <returns>A task representing the chat operation.</returns>
    public Task<object> StartChat(ILLmChatProvider chatProvider, object option = null)
    {
        if (chatProvider is null)
        {
            return null;
        }

        if (_currentChat != null && _currentChat.State != LLmChatStates.Stopped)
        {
            DialogUtility.ShowMessageBoxAsyncL("Can't start a new chat while the current one is not stopped.");
            return null;
        }

        SelectedChatProvider = chatProvider;
        if (_mainChat.SelectedAsset is null)
        {
            return null;
        }

        _currentChat?.Stop();
        _currentChat?.Clear();

        _msgInput = string.Empty;
        _msgOption = option;

        return ProcessInput();
    }

    private async Task<object> ProcessInput()
    {
        //if (string.IsNullOrWhiteSpace(_msgInput))
        //{
        //    return null;
        //}

        string msg = _msgInput;
        var attachments = _attachments.Values.ToArray();

        _msgInput = string.Empty;
        _attachments.Clear();

        _guiRef.QueueRefresh();

        try
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                return await _currentChat?.Send(msg, attachments, _msgOption);
            }
            else
            {
                return await _currentChat?.Start(msg, attachments, _msgOption);
            }
        }
        catch (Exception err)
        {
            err.LogErrorL("Error executing AI chat.");
            _msgInput = msg;

            return null;
        }
        finally
        {
            _guiRef.QueueRefresh();
        }
    }

    /// <summary>
    /// Sets the input text and attachments in the chat window.
    /// </summary>
    /// <param name="msg">The message text to set.</param>
    /// <param name="attachments">Optional attachments to add.</param>
    public void SetInput(string msg, IEnumerable<AttachmentSet> attachments = null)
    {
        _msgInput = msg ?? string.Empty;

        _attachments.Clear();
        if (attachments != null)
        {
            foreach (var attachment in attachments.SkipNull())
            {
                _attachments[attachment.Document] = attachment;
            }
        }

        _guiRef.QueueRefresh();
    }

    /// <summary>
    /// Adds an attachment from the current document selection.
    /// </summary>
    /// <param name="doc">The document entry.</param>
    /// <param name="selection">The selected item names.</param>
    public void AddAttachment(DocumentEntry doc, IEnumerable<string> selection)
    {
        if (doc is null)
        {
            return;
        }

        var attachment = _attachments.GetOrAdd(doc, key => new AttachmentSet(key));
        attachment.AddAttachments(selection);

        _guiRef.QueueRefresh();
    }

    /// <summary>
    /// Clears all attachments from the chat window.
    /// </summary>
    public void ClearAttachment()
    {
        _attachments.Clear();

        _guiRef.QueueRefresh();
    }

    #endregion

    #region Funcs

    /// <summary>
    /// Starts a workflow chat with the specified runnable workflow.
    /// </summary>
    /// <param name="runnable">The workflow to run.</param>
    /// <param name="view">Optional flow view for visualization.</param>
    /// <param name="config">Optional configuration action for the flow computation.</param>
    /// <returns>A task representing the workflow chat operation.</returns>
    public async Task<object> StartWorkflowChat(IAigcRunWorkflow runnable, IFlowView view = null, Action<IFlowComputation> config = null)
    {
        if (runnable is null)
        {
            return false;
        }

        if (_currentChat != null && _currentChat.State != LLmChatStates.Stopped)
        {
            DialogUtility.ShowMessageBoxAsyncL("Other workflow is running.");

            return false;
        }

        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.AigcWorkflow))
        //{
        //    DialogUtility.ShowMessageBoxAsync(ServiceInternals._license.GetFailedMessage(EditorCapabilities.AigcWorkflow));
        //    return false;
        //}

        //if (ServiceInternals._license.GetMaxUsageReach())
        //{
        //    DialogUtility.ShowMessageBoxAsync(ServiceInternals._license.GetUsageFailedMessage());
        //    return false;
        //}

        _currentChat = _mainChat;
        _mainChat.SelectedAsset = _workflowRunner.Get()?.ChatProvider;

        var option = new AigcWorkflowOption
        {
            Runnable = runnable,
            View = view,
            Config = config,
        };

        try
        {
            _guiRef.QueueRefresh();
            var result = await _currentChat.StartWithoutMessage(option);
            _guiRef.QueueRefresh();

            return result;
        }
        catch (Exception err)
        {
            _guiRef.QueueRefresh();
            err.LogErrorL("Failed to start chat.");

            return null;
        }
    }

    public async Task StartCurrentChat()
    {
        if (_currentChat.State != LLmChatStates.Stopped)
        {
            return;
        }

        try
        {
            _guiRef.QueueRefresh();
            await _currentChat.StartWithoutMessage();
            _guiRef.QueueRefresh();
        }
        catch (Exception err)
        {
            _guiRef.QueueRefresh();
            err.LogErrorL("Failed to start chat.");
        }
    }

    #endregion
}