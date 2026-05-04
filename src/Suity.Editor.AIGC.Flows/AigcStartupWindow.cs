using Suity.Drawing;
using Suity.Editor.AIGC.Properties;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.AIGC.TaskPages.Running;
using Suity.Editor.Documents;
using Suity.Editor.Selecting;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

public class AigcStartupWindow : IToolWindow, IDrawImGui, IDrawContext
{
    public static AigcStartupWindow Instance { get; private set; }

    public static BitmapDef Logo128 { get; } = Resources.Logo128.ToBitmap();

    public static Color LogoFilterColor { get; } = Color.FromArgb(128, 128, 128, 128);

    private class StartupSelection : AssetSelection<IAigcToolAsset>
    {
        public StartupSelection()
        {
            this.Filter = StartupPageFilter.Instance;
        }
    }

    private StartupSelection _startupAssetSel = new();
    private readonly PropertyTarget _startupAssetTarget;

    private readonly ImGuiNodeRef _guiRef = new();

    private string _msgInput = "";

    public AigcStartupWindow()
    {
        Instance ??= this;

        _startupAssetSel.Target = _startupAssetSel.GetList()?.GetItems()?.FirstOrDefault() as IAigcToolAsset;
        _startupAssetSel.TargetUpdated += (s, e, ref handled) => { _guiRef.QueueRefresh(); };
        _startupAssetSel.ListenEnabled = true;
        _startupAssetTarget = PropertyTargetUtility.CreatePropertyTarget(_startupAssetSel, "Select Startup Agent");
    }

    public Guid SelectedChatAssetId
    {
        get => _startupAssetSel.Id;
        set
        {
            if (value == Guid.Empty)
            {
                value = (_startupAssetSel.GetList()?.GetItems()?.FirstOrDefault() as Asset)?.Id ?? Guid.Empty;
            }

            _startupAssetSel.Id = value;
        }
    }

    #region Config

    //public void LoadConfig(AigcConfigState state)
    //{
    //    SelectedChatAssetId = state.LastStartupAssetId;
    //}

    //public void SaveConfig(AigcConfigState state)
    //{
    //    state.LastStartupAssetId = SelectedChatAssetId;
    //}

    #endregion

    #region IToolWindow

    public string WindowId => nameof(AigcStartupWindow);

    public string Title => "AI Startup";

    public ImageDef Icon => CoreIconCache.Startup;

    public DockHint DockHint => DockHint.Document;

    public bool CanDockDocument => true;

    public object GetUIObject() => this;

    public void NotifyHide()
    { }

    public void NotifyShow()
    {
        _startupAssetSel.Target = _startupAssetSel.GetList()?.GetItems()?.FirstOrDefault() as IAigcToolAsset;

        _guiRef.QueueRefresh(true);
    }

    #endregion

    #region IDrawImGui

    public void OnGui(ImGui gui)
    {
        _guiRef.Node = gui.Frame()
        .InitClass("editorBg")
        .OnContent(() =>
        {
            gui.HorizontalLayout("hori")
            .InitFullHeight()
            .InitWidthPercentage(75)
            .InitCenter()
            .OnContent(() =>
            {
                gui.VerticalLayout("vert")
                .InitFullWidth()
                .InitHeight(500)
                .InitCenter()
                .OnContent(() =>
                {
                    gui.Image("logo", Logo128, true)
                    .InitImageFilter(LogoFilterColor)
                    .InitCenter()
                    .InitFit();

                    if (_startupAssetSel.Target is null)
                    {
                        gui.HorizontalLayout("#warning")
                        .InitFullWidth()
                        .InitFitVertical()
                        .OnContent(() =>
                        {
                            gui.Image(CoreIconCache.Warning).InitClass("icon");
                            gui.Text("#title", "Please configure startup parameters first.");
                        });
                    }

                    gui.HorizontalLayout("#sel")
                    .InitWidth(400)
                    .InitHeight(32)
                    .InitVerticalAlignment(GuiAlignment.Center)
                    .InitTextAlignment(GuiAlignment.Center)
                    .InitPadding(0)
                    .OnContent(() =>
                    {
                        gui.Text(L("Agent")).InitCenter();
                        gui.PropertyEditor(_startupAssetTarget, act =>
                        {
                            act.DoAction();
                            _startupAssetSel = _startupAssetTarget.GetValues().FirstOrDefault() as StartupSelection ?? _startupAssetSel;

                            //_startupAssetSel.Target ??= MainAssistantChatProvider.Instance;
                            _guiRef.QueueRefresh();
                        });
                    });

                    gui.VerticalLayout("#spacer01")
                    .InitHeight(10);

                    if (_startupAssetSel.Target is IDrawEditorImGui drawEditor)
                    {
                        bool draw = drawEditor.OnEditorGui(gui, EditorImGuiPipeline.Input, this);
                        if (!draw)
                        {
                            DefaultInputGui(gui);
                        }
                    }
                    else
                    {
                        DefaultInputGui(gui);
                    }
                });
            });
        });
    }

    private void DefaultInputGui(ImGui gui)
    {
        var hintText = _startupAssetSel.Target?.GetSkillDefinition()?.PromptHint;
        if (string.IsNullOrWhiteSpace(hintText))
        {
            hintText = L("Prompt input...");
        }

        gui.Text(L("Please input AI prompt"));
        _msgInput = gui.TextAreaInput("input", L(_msgInput), autoFit: false, submitMode: TextBoxEditSubmitMode.Enter)
        .InitFullWidth()
        .InitHeight(270)
        .InitInputFunctionChain(TextInput)
        .SetHintText(hintText)
        .Text;

        gui.HorizontalReverseLayout("input_bar")
        //.SetEnabled(started)
        .InitFullWidth()
        .InitHeight(30)
        .OnContent(() =>
        {
            gui.Button("send", CoreIconCache.Send)
            .InitClass("simpleBtn")
            .InitWidth(50)
            .InitFullHeight()
            .SetToolTipsL("Start Generation")
            .OnClick(() =>
            {
                ProcessInput();
            });
        });

        //gui.TextArea(L("STARTUP_GUIDE"))
        //.InitFullWidth()
        //.InitHeightRest()
        //.InitFontColor(Color.LightGray);
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

    private async void ProcessInput()
    {
        if (!await LLmService.Instance.CheckCurrentModelConfig())
        {
            return;
        }

        var startupPage = _startupAssetSel.Target;
        if (startupPage is null)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Please select a startup agent.");
        }

        string prompt = _msgInput;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Please input a prompt.");
            return;
        }

        var format = DocumentManager.Instance.GetDocumentFormat("AigcTaskPage");
        if (format is null)
        {
            return;
        }

        var docEntry = format.AutoNewDocument("TaskPage");
        if (docEntry is null)
        {
            return;
        }

        var doc = docEntry.Content as AigcTaskPageDocument;
        if (doc is null)
        {
            return;
        }

        var view = doc.ShowView();

        doc.StartupPage = startupPage;
        doc.InitialTaskPrompt = prompt;
        doc.MarkDirtyAndSaveDelayed(this);

        // Waiting for document view to be ready
        await EditorUtility.WaitForNextQueuedAction();

        (view as AigcTaskPageDocumentView)?.Run(prompt);
    }
}