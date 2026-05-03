using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.Gui;
using Suity.Views.Im;
using Suity.Views.Im.Logging;
using Suity.Views.Im.TreeEditing;

namespace Suity.Editor.View;

/// <summary>
/// ImGui-based log view implementation with tree view display, log level filtering, and tool window integration.
/// </summary>
public class LogViewImGui : IDrawImGui, IToolWindow
{
    private readonly ConsoleModel _model;
    private readonly HeaderlessPathTreeView _treeView;

    private readonly QueueOnceAction _logAction;

    private readonly ImGuiNodeRef _guiRef = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewImGui"/> class.
    /// Sets up the console model, tree view, log action queue, and registers event listeners for render start and log commands.
    /// </summary>
    public LogViewImGui()
    {
        _model = new ConsoleModel();
        _treeView = new HeaderlessPathTreeView(_model);
        _logAction = new QueueOnceAction(LogAction);

        var menu = new ConsoleViewRootCommand(_treeView);

        _treeView.Menu = menu;
        _treeView.MenuSender = this;

        _treeView.ContentGui = ConsoleModel.ConfigContent;
        _treeView.Theme.SetTwoColorRow();

        EditorRexes.OnRenderStart.AddActionListener(ClearLog);
        EditorCommands.ClearLog.AddActionListener(ClearLog);
        EditorCommands.ShowLogView.AddActionListener(() => EditorUtility.ShowToolWindow("Log"));
    }

    #region IToolWindow

    /// <summary>
    /// Gets the unique identifier for this tool window.
    /// </summary>
    public string WindowId => "Log";

    /// <summary>
    /// Gets the display title of the log window.
    /// </summary>
    public string Title => "Log";

    /// <summary>
    /// Gets the icon displayed for the log window tab.
    /// </summary>
    public ImageDef Icon => CoreIconCache.Log;

    /// <summary>
    /// Gets the default docking position for the log window.
    /// </summary>
    public DockHint DockHint => DockHint.Bottom;

    /// <summary>
    /// Gets a value indicating whether this window can be docked as a document tab.
    /// </summary>
    public bool CanDockDocument => false;

    /// <inheritdoc/>
    public object GetUIObject()
    {
        return this;
    }

    /// <inheritdoc/>
    public void NotifyHide()
    {
        LogCache.LogEntryAdded -= LogCache_LogEntryAdded;
    }

    /// <inheritdoc/>
    public void NotifyShow()
    {
        LogCache.LogEntryAdded += LogCache_LogEntryAdded;

        //Logs.LogInfo("Show\r\nOKOK");

        LogAction();

        _guiRef.QueueRefresh(true);
    }

    #endregion

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _guiRef.Node = gui.Frame("log")
        .InitClass("editorBg")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.HorizontalReverseLayout("header")
            .InitFullWidth()
            .InitHeight(24)
            .OnContent(() =>
            {
                gui.Button("btnClear", CoreIconCache.Delete)
                .InitClass("configBtn")
                .OnClick(() =>
                {
                    _model.Clear();
                });

                gui.ToggleButton("btnLvDebug", CoreIconCache.LogDebug, _model.LvDebug)
                .InitClass("toolBtn")
                .OnChecked((n, v) =>
                {
                    _model.LvDebug = v;
                });

                gui.ToggleButton("btnLvInfo", CoreIconCache.LogInfo, _model.LvInfo)
                .InitClass("toolBtn")
                .OnChecked((n, v) =>
                {
                    _model.LvInfo = v;
                });

                gui.ToggleButton("btnLvWarning", CoreIconCache.LogWarning, _model.LvWarning)
                .InitClass("toolBtn")
                .OnChecked((n, v) =>
                {
                    _model.LvWarning = v;
                });

                gui.ToggleButton("btnLvError", CoreIconCache.LogError, _model.LvError)
                .InitClass("toolBtn")
                .OnChecked((n, v) =>
                {
                    _model.LvError = v;
                });
            });

            _treeView.OnGui(gui, "tree_view", n => n.InitSizeRest().AutoScrollToBottom());
        });
    }

    /// <summary>
    /// Clears all log entries from the console model.
    /// </summary>
    public void ClearLog()
    {
        _model.Clear();
    }

    private void LogCache_LogEntryAdded(LogEntry obj)
    {
        _logAction.DoQueuedAction();
    }

    private void LogAction()
    {
        foreach (var entry in LogCache.PickUp())
        {
            _model.AddLogNow(entry.LogLevel, entry.Message, entry.Indent);
        }
    }
}