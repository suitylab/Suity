using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Flows.Gui;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

public enum PageViewCategory
{
    Page,
    Chat,
    Context,
}

/// <summary>
/// Document view for <see cref="AigcTaskPageDocument"/>, providing tree-based navigation, property editing, and AI task interaction UI.
/// </summary>
[DocumentViewUsage(typeof(AigcTaskPageDocument))]
public class AigcTaskPageDocumentView : IDocumentView,
    IHasSubDocumentView,
    IDrawImGui,
    IDrawContext, 
    IViewRefresh, 
    ISyncStateRecord
{
    private readonly ImGuiNodeRef _guiRef = new();
    private readonly ImGuiNodeRef _guiNaviContainerRef = new();

    private readonly ImGuiTheme _theme;
    private readonly IUndoableViewObjectImGui _treeView;
    private readonly IInspectorContext _inspectorContext;
    private readonly SubDocumentView _subView;

    private UndoRedoManager _undoManager;
    private AigcTaskPageDocument _document;
    private PropertyTarget _startupPageTarget;

    private readonly IPropertyGrid _propGrid;

    private AigcTaskPage _currentPage;
    private readonly GuiOptionalValue _pageNaviOption = new();
    private PageViewCategory _pageCategory;
    private AigcTaskPageRunner _currentRunner;
    private string _msgInput = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcTaskPageDocumentView"/> class.
    /// </summary>
    public AigcTaskPageDocumentView()
    {
        _theme = CreateTheme();

        var option = new HeaderlessTreeOptions
        {
            ShowDisplayText = true,
        };
        _treeView = EditorUtility.CreateSimpleTreeImGui(option);

        _treeView.SelectionChanged += _treeView_SelectionChanged;
        _treeView.Dirty += View_Dirty;
        _treeView.Edited += View_Edited;

        _inspectorContext = _treeView.GetService<IInspectorContext>();

        _subView = new(this);

        _propGrid = PropertyGridExtensions.CreatePropertyGrid("AIBuilder");

        _propGrid.ShowContextMenu = false;
        _propGrid.ShowToolBar = false;

        _propGrid.AddService<IViewRefresh>(this);
        _propGrid.AddService<IViewSave>(_subView);
        _propGrid.Edited += _grid_Edited;
    }

    public bool IsRunning => _currentRunner?.IsRunning == true;

    #region IHasSubDocumentView

    /// <summary>
    /// Gets the currently active sub-view object.
    /// </summary>
    public object CurrentSubView => _subView.CurrentSubView;

    /// <summary>
    /// Opens a sub-view for the specified document.
    /// </summary>
    /// <param name="document">The document to open a sub-view for.</param>
    /// <returns>The opened sub-view object.</returns>
    public object OpenSubView(Document document) => _subView.OpenSubView(document);

    #endregion

    #region IServiceProvider

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        if (serviceType.IsInstanceOfType(_subView))
        {
            return _subView;
        }

        return _treeView.GetService(serviceType);
    }

    #endregion

    #region IDocumentView

    /// <summary>
    /// Gets the target document object displayed by this view.
    /// </summary>
    public object TargetObject => _document;


    /// <inheritdoc/>
    public void StartView(Document document, IDocumentViewHost host)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (document is not AssetDocument)
        {
            throw new ArgumentException("Document must be a LinkedDocument", nameof(document));
        }

        if (_document != null)
        {
            throw new InvalidOperationException();
        }

        _document = (AigcTaskPageDocument)document;
        _subView.Document = document;
        _startupPageTarget = PropertyTargetUtility.CreatePropertyTarget(_document.StartupPageSelection);

        if (_document.StartupPage is null)
        {
            _document.StartupPage = _document.StartupPageSelection?.GetList()?.GetItems()?.FirstOrDefault() as ISubFlowAsset;
        }

        _undoManager = host.GetService<UndoRedoManager>() ?? new UndoRedoManager();

        _treeView.UndoManager = _undoManager;
        _treeView.Target = _document;
        _treeView.ExpandAll();

        string formatName = _document.Entry?.Format?.FormatName;
        if (!string.IsNullOrWhiteSpace(formatName))
        {
            _treeView.CreateMenu("#" + formatName);
        }

        RestoreDocumentViewState(_document);
    }

    /// <inheritdoc/>
    public void StopView()
    {
        SaveDocumentViewState(_document);

        _treeView.Target = null;
        _document = null;
        _startupPageTarget = null;
        _subView.Document = null;
        _undoManager = null;
        _treeView.UndoManager = null;
        _guiRef.Node = null;

        _subView.ExitAll();
        _subView.ClearGui();
    }

    /// <inheritdoc/>
    public object GetUIObject()
    {
        return this;
    }

    /// <inheritdoc/>
    public void SetDataToDocument()
    {
    }

    /// <inheritdoc/>
    public void GetDataFromDocument()
    {
        _treeView.Target = _document;
    }

    /// <inheritdoc/>
    public void ActivateView(bool focus)
    {
        if (focus)
        {
            _treeView.FocusView(false);
        }

        _treeView.UpdateAnalysis();
    }

    /// <inheritdoc/>
    public void RefreshView()
    {
        _treeView?.UpdateDisplayedObject();
    }

    #endregion

    #region IDrawImGui

    private ImGuiTheme CreateTheme()
    {
        var colorScheme = EditorColorScheme.Default;

        var font = new FontDef(ImGuiTheme.DefaultFont, 16);
        var fontBold = new FontDef(ImGuiTheme.DefaultFont, 16, FontStyle.Bold);
        var fontSmall = new FontDef(ImGuiTheme.DefaultFont, 12);

        var theme = new ImGuiTheme();

        theme.ClassStyle("titleText")
            .SetFont(font, Color.White);

        theme.ClassStyle("titleIcon")
            .SetSize(32, 32)
            .SetVerticalAlignment(GuiAlignment.Center);

        theme.ClassStyle("descText")
        .SetFont(fontSmall, Color.White);

        theme.ClassStyle("mainBtn")
            .SetCornerRound(5)
            .SetPadding(7);

        theme.ClassStyle("naviBtn")
            .SetHeight(40)
            .SetColor(Color.Transparent)
            .SetFont(fontBold, Color.White.MultiplyAlpha(0.5f))
            .SetCornerRound(0)
            .SetPadding(4, 4, 4, 10)
            .SetBorder(0)
            .SetCenter();
        theme.PseudoMouseIn()
            .SetColor(colorScheme.ToolButton);
        theme.PseudoMouseDown()
            .SetColor(colorScheme.ToolButton.Multiply(0.8f));
        theme.PseudoActive()
            .SetColor(colorScheme.ToolButton)
            .SetFont(fontBold, Color.White);

        return theme;
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _subView.OnGui(gui, n => { }, MainGui);
    }

    private void MainGui(ImGui gui)
    {
        _guiRef.Node = gui.HorizontalFrame("main_ui")
        .OnInitialize(n =>
        {
            n.InitTheme(_subView.Theme);
            n.InitClass("editorBg");
            n.InitFullSize();
        })
        .OnContent(() =>
        {
            gui.VerticalLayout("left")
            .InitFullHeight()
            .InitWidthPercentage(30)
            .OnContent(() => 
            {
                gui.Frame("toolBar")
                .InitClass("toolBar")
                .InitOverridePadding(3)
                .InitFullWidth()
                .InitFitVertical()
                .OnContent(() => 
                {
                    if (IsRunning)
                    {
                        gui.Button("btnStop", "Stop", CoreIconCache.Stop)
                        .InitClass("simpleBtn")
                        .SetToolTipsL("Stop task")
                        .OnClick(() =>
                        {
                            _currentRunner?.RequestCancel();
                            _currentRunner = null;
                            _guiRef.QueueRefresh();
                        });
                    }
                    else
                    {
                        gui.Button("btnResume", "Resume", CoreIconCache.Play)
                        .InitClass("simpleBtn")
                        .SetToolTipsL("Start task")
                        .SetEnabled(_document?.GetUnfinishedChildTaskDeep() != null)
                        .OnClick(() =>
                        {
                            Run("-resume");
                        });
                    }
                });

                _treeView.OnNodeGui(gui)
                .InitHeightRest();
            });

            gui.HorizontalResizer(40, null)
            .InitFullHeight()
            .InitClass("resizer_h");

            var sel = _treeView.SelectedObjects;

            if (sel.CountOne() && sel.FirstOrDefault() is AigcTaskPageDocument doc)
            {
                OnDocumentGui(gui, doc);
            }
            else if (sel.Any())
            {
                if (sel.CountOne() && sel.FirstOrDefault() is AigcTaskPage page)
                {
                    OnTaskPageGui(gui, page);
                }
                else
                {
                    gui.VerticalLayout("#blank")
                    .InitFullHeight()
                    .InitWidthRest();
                }
            }
            else if (_document != null)
            {
                OnDocumentGui(gui, _document);
            }
        });
    }

    private void OnDocumentGui(ImGui gui, AigcTaskPageDocument doc)
    {
        if (doc.IsTaskEmpty)
        {
            OnStartupGui(gui, doc);
        }
        else if (doc.GetUnfinishedChildTaskDeep() is null)
        {
            // Previous task completed.
            OnStartupGui(gui, doc);
        }
        else
        {
            // OnResumeGui(gui, doc);
        }
    }

    private void OnStartupGui(ImGui gui, AigcTaskPageDocument doc)
    {
        gui.Frame("#startup")
        .InitClass("editorBg")
        .InitSizeRest()
        .OnContent(() =>
        {
            gui.HorizontalLayout()
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
                    bool configured = doc.IsStartupConfigured == true;

                    if (!configured)
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
                    .OnContent(() => 
                    {
                        gui.Text(L("Startup")).InitFit();
                        gui.PropertyEditor(_startupPageTarget, act =>
                        {
                            if (doc.Count == 0)
                            {
                                act.DoAction();
                                doc.StartupPageSelection = _startupPageTarget.GetValues().FirstOrDefault() as AssetSelection<ISubFlowAsset> ?? doc.StartupPageSelection;

                                // doc.StartupPageSelection.Target ??= null;
                                _guiRef.QueueRefresh();
                            }
                        })
                        .SetEnabled(doc.Count == 0);
                    });

                    gui.Text("#title", L("Please enter AI prompt words"));
                    doc.InitialTaskPrompt = gui.TextAreaInput("input", doc.InitialTaskPrompt, autoFit: false, submitMode: TextBoxEditSubmitMode.Enter)
                    .InitFullWidth()
                    .InitHeight(270)
                    .InitInputFunctionChain(TextInput)
                    .SetHintText(L("Prompt words input"))
                    .SetEnabled(configured)
                    .Text;

                    gui.HorizontalReverseLayout("input_bar")
                    //.SetEnabled(started)
                    .InitFullWidth()
                    .InitHeight(30)
                    .OnContent(() =>
                    {
                        gui.Button("btnRun", "Run", CoreIconCache.Send)
                        .InitClass("simpleBtn")
                        .InitFullHeight()
                        .SetToolTipsL("Start running")
                        .SetEnabled(configured)
                        .OnClick(() =>
                        {
                            if (doc.InitialTaskPrompt is { } prompt && !string.IsNullOrWhiteSpace(prompt))
                            {
                                Run(prompt);
                            }
                        });
                    });
                });
            });
        });
    }

    private void OnResumeGui(ImGui gui, AigcTaskPageDocument doc)
    {
        gui.Frame("#resume")
        .InitClass("editorBg")
        .InitSizeRest()
        .OnContent(() =>
        {
            gui.HorizontalLayout()
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
                    gui.Button("btnResume", "Resume", CoreIconCache.Play)
                    .InitClass("simpleBtn")
                    .InitHeight(30)
                    .InitCenterHorizontal()
                    .SetToolTipsL("Start generating")
                    .OnClick(() =>
                    {
                        Run("resume");
                    });
                });
            });
        });
    }

    private GuiInputState TextInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (input.EventType == GuiEventTypes.KeyDown && input.KeyCode == "Return" && node.IsMouseInClickRect)
        {
            Run(_document?.InitialTaskPrompt);
        }

        return state;
    }


    private void OnTaskPageGui(ImGui gui, AigcTaskPage page)
    {
        gui.VerticalLayout("#task_page-" + page.Name)
        .OnInitialize(n =>
        {
            n.InitTheme(_theme);
            n.InitFullHeight();
            n.InitWidthRest();
        })
        .OnContent(() =>
        {
            _guiNaviContainerRef.Node = gui.HorizontalFrame("title")
            .InitFullWidth()
            //.InitHeight(80)
            .InitFitVertical()
            .OnContent(() =>
            {
                //_guiNaviContainerRef.Node = gui.HorizontalLayout("#left")
                //.InitWidthRest(64)
                //.InitPadding(0)
                ////.InitVerticalAlignment(GuiAlignment.Center)
                //.OnContent(() =>
                //{

                //});

                NaviButton(gui, PageViewCategory.Page, CoreIconCache.Task, "Page view");
                NaviButton(gui, PageViewCategory.Chat, CoreIconCache.Chat, "LLm chat view");
                NaviButton(gui, PageViewCategory.Context, CoreIconCache.Text, "Chat history context");

                //TaskTitleGui(gui, page);

                gui.VerticalLine("#spacing01")
                .OnInitialize(n =>
                {
                    //n.InitOverrideMargin(0, 0, 10, 10);
                    n.InitHeight(40);
                    n.InitWidth(20);
                    n.InitOverrideBorder(1, Color.White.MultiplyAlpha(0.2f));
                });

                gui.Button("#workflowBtn", "Workflow", CoreIconCache.Workflow)
                .InitClass("naviBtn")
                .InitToolTips("Go to workflow")
                .OnClick(() =>
                {
                    HandleGotoWorkflow();
                });
            });

            gui.VerticalLayout("#content")
            .InitSizeRest()
            .OnContent(() => 
            {
                switch (_pageCategory)
                {
                    case PageViewCategory.Page:
                        _propGrid.OnGui(gui);
                        break;

                    case PageViewCategory.Chat:
                        ChatGui(gui, page);
                        break;

                    case PageViewCategory.Context:
                        ContextGui(gui, page);
                        break;
                }
            });
        });
    }

    private void ChatGui(ImGui gui, AigcTaskPage page)
    {
        if (page.Instance is not { } instance)
        {
            return;
        }

        gui.VerticalLayout("#dialog")
        .InitFullWidth()
        .InitHeightRest(120)
        .OnContent(() =>
        {
            instance.Conversation.OnNodeGui(gui);
        });

        bool started = IsRunning;

        gui.VerticalResizer(30, null)
        .InitFullWidth()
        .InitClass("resizer");

/*        if (_attachments.Count > 0)
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
        }*/

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
                if (IsRunning && !string.IsNullOrWhiteSpace(_msgInput))
                {
                    if (page.Instance?.Conversation is { } conversation && conversation is IConversationHost host)
                    {
                        conversation.AddUserMessage(_msgInput);
                        host.HandleMessageInput(_msgInput);
                        _msgInput = string.Empty;
                        _guiRef.QueueRefresh();
                    }
                }
            });
        });
    }

    private void ContextGui(ImGui gui, AigcTaskPage page)
    {
        gui.ScrollableFrame("#chat-history-" + page.Name, GuiOrientation.Vertical)
        .InitFullSize()
        .OnContent(() =>
        {
            gui.HorizontalLayout("#input-title")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() =>
            {
                gui.Image("icon", CoreIconCache.Input).InitClass("titleIcon");
                gui.Text("input", "Input chat history")
                .InitClass("titleText")
                .InitCenter();
            });


            gui.TextAreaInput("#input", null, page.Instance?.GetInputChatHistory())
            .InitFullWidth()
            .InitFitVertical()
            .InitReadonly(true);

            gui.HorizontalLayout("#output-title")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() =>
            {
                gui.Image("icon", CoreIconCache.Output).InitClass("titleIcon");
                gui.Text("output", "Output chat history")
                .InitClass("titleText")
                .InitCenter();
            });

            gui.TextAreaInput("#output", null, page.Instance?.GetOutputChatHistory())
            .InitFullWidth()
            .InitFitVertical()
            .InitReadonly(true);

            gui.HorizontalLayout("#commit-title")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() =>
            {
                gui.Image("icon", CoreIconCache.Complete).InitClass("titleIcon");
                gui.Text("commit", "Commit to parent task")
                .InitClass("titleText")
                .InitCenter();
            });

            gui.TextAreaInput("#commit", null, page.Instance?.GetTaskCommit())
            .InitFullWidth()
            .InitFitVertical()
            .InitReadonly(true);
        });
    }

    private void TaskTitleGui(ImGui gui, AigcTaskPage page)
    {
        if (page.Instance?.GetAllStatusIcon() is { } statusIcon)
        {
            gui.Image("#statusIcon", statusIcon).InitClass("icon");
        }

        if (page.Icon is { } icon)
        {
            gui.Image("#icon", icon).InitClass("icon");
        }

        gui.Text($"#title", page.DisplayText ?? "---")
        .InitClass("titleText")
        .InitCenter();

        if (page.Instance is SkillSubFlowInstance instance && instance.SkillAssetSelection?.Target is { } skill)
        {
            gui.Text($" (Skill: {skill.ToDisplayText()})");
        }
    }

    private ImGuiNode NaviButton(ImGui gui, PageViewCategory category, ImageDef icon, string toolTips = null)
    {
        var node = gui.SwitchButton($"#{category}", category.ToDisplayTextL(), icon, _pageNaviOption)
        .OnInitialize(n =>
        {
            n.InitClass("naviBtn");
            n.InitFullHeight();
            n.InitFitHorizontal();
            n.InitChildSpacing(5);
            n.InitOptionActive(category == _pageCategory);
            //n.InitPadding(0, 0, 10, 10);
        })
        .OnClick(() =>
        {
            _pageCategory = category;
            gui.QueueRefresh();
        });

        if (!string.IsNullOrWhiteSpace(toolTips))
        {
            node.SetToolTips(L(toolTips));
        }
        else
        {
            node.SetToolTips(category.ToDisplayTextL());
        }

        return node;
    }

    private void SetCurrentCategory(PageViewCategory category)
    {
        if (_pageCategory == category)
        {
            return;
        }

        _pageCategory = category;

        if (_guiNaviContainerRef.Node is { } container)
        {
            var node = container.GetChildNode($"#{category}");
            node?.SetActiveSwitchButton();
            _guiNaviContainerRef.QueueRefresh();
        }
    }


    #endregion

    #region IDropTarget

    /// <inheritdoc/>
    public void DragOver(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route
        if (_subView.CurrentSubView is IDropTarget dropTarget && dropTarget != this)
        {
            dropTarget.DragOver(e);
        }
        else
        {
            //TODO
        }
    }

    /// <inheritdoc/>
    public void DragDrop(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route
        if (_subView.CurrentSubView is IDropTarget dropTarget && dropTarget != this)
        {
            dropTarget.DragDrop(e);
        }
        else
        {
            //TODO
        }
    }

    #endregion

    #region IViewRefresh

    /// <inheritdoc/>
    public void QueueRefreshView() => this.RefreshView();

    #endregion

    #region ISyncStateRecord

    /// <inheritdoc/>
    void ISyncStateRecord.Record(ISyncObject obj)
    {
        _undoManager?.Do(new SnapshotObjectUndoAction(obj, null, this));
    }

    /// <inheritdoc/>
    void ISyncStateRecord.Record(ISyncList list)
    {
        _undoManager?.Do(new SnapshotListUndoAction(list, null, this));
    }

    #endregion

    #region Event handlers

    private void View_Dirty(object sender, EventArgs e)
    {
        _document?.MarkDirty(this);
    }

    private void View_Edited(object sender, object[] objs)
    {
        //_edit.QueueRefresh();
        _guiRef.QueueRefresh();
    }

    #endregion

    #region Document state

    private void RestoreDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            object config = EditorServices.PluginService.GetPlugin<GuiStatePlugin>().GetGuiState<object>(asset);

            _treeView.RestoreViewState(config);
        }
    }

    private void SaveDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            var config = _treeView.SaveViewState();
            if (config != null)
            {
                EditorServices.PluginService.GetPlugin<GuiStatePlugin>().SetGuiState<object>(asset, config);
            }
        }
    }

    #endregion

    #region Run

    /// <summary>
    /// Runs the specified message on the current task.
    /// </summary>
    /// <param name="msg">The message to run.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<object> Run(string msg)
    {
        var runner = _currentRunner;

        if (runner != null)
        {
            runner.RequestCancel();
        }

        if (_document is not { } doc)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(msg))
        {
            return null;
        }

        runner = _currentRunner = new AigcTaskPageRunner(doc);
        SetCurrentCategory(PageViewCategory.Chat);

        var result = await LLmService.Instance.InputMainChat(msg, runner);

        _currentRunner = null;
        SetCurrentCategory(PageViewCategory.Page);

        return result;
    }


    #endregion

    private void _treeView_SelectionChanged(object sender, EventArgs e)
    {
        if (_treeView.SelectedObjects.CountMoreThanOne())
        {
            _currentPage = null;
            _propGrid.InspectObjects([], context: _inspectorContext);
        }
        else if (_treeView.SelectedObjects.FirstOrDefault() is AigcTaskPage pageNode && pageNode.Instance is { } root)
        {
            _currentPage = pageNode;
            _propGrid.InspectObjects([root], context: _inspectorContext);
        }
        else
        {
            _currentPage = null;
            _propGrid.InspectObjects([], context: _inspectorContext);
        }

        _treeView.QueueRefresh();
        _guiRef.QueueRefresh();
    }

    private void _grid_Edited(object sender, ObjectPropertyEventArgs e)
    {
    }

    /// <summary>
    /// Navigates to the diagram associated with the currently selected task.
    /// </summary>
    internal void HandleGotoWorkflow() => HandleGotoWorkflow(_currentPage);

    /// <summary>
    /// Navigates to the diagram associated with the specified task.
    /// </summary>
    /// <param name="task">The task whose diagram to navigate to.</param>
    internal void HandleGotoWorkflow(AigcTaskPage task)
    {
        if (task is null)
        {
            return;
        }

        if (task.GetDocument() != _document)
        {
            return;
        }

        if (task.GetDefinitionItem()?.GetDocument() is not { } diagramDoc)
        {
            return;
        }

        if (_document is not { } document)
        {
            return;
        }

        var currentView = _subView.CurrentSubView as IFlowView;
        if (_subView.OpenSubView(diagramDoc) is not { } view)
        {
            return;
        }

        if (view is IFlowView flowView)
        {
            if (currentView?.GetViewNode(task.Name) is { } viewNode && viewNode.NodeComputation is { } nodeCompute)
            {
                flowView.Computation = nodeCompute;
            }
            else if (task.Instance?.LastComputation is { } lastCompute)
            {
                flowView.Computation = lastCompute;
            }
        }

        if ((view as IServiceProvider)?.GetService<IViewSelectable>() is { } sel && task.GetDefinitionItem() is { } page)
        {
            QueuedAction.Do(() =>
            {
                sel.SetSelection(new ViewSelection(page));
            });
        }

        _guiRef.QueueRefresh();
    }


}
