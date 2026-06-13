using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Dock.Model.Controls;
using Newtonsoft.Json;
using Suity.Controls;
using Suity.Editor.Analyzing;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.WinformGui;
using Suity.Helpers;
using Suity.Rex;
using Suity.Rex.VirtualDom;
using Suity.UndoRedos;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Controls;

public class EditorDocumentDockable : Dock.Model.Avalonia.Controls.Document
{
    private DockBridgeControl<EditorDocumentContent>? _bridge;

    public object? IconSource { get; internal set; }

    public EditorDocumentDockable()
    {
        this.CanFloat = false;
        this.CanDockAsDocument = true;
        this.CanPin = false;
        base.CanClose = true;
    }

    public EditorDocumentDockable(EditorDocumentContent content)
        : this()
    {
        SetEditorContent(content);
    }

    [JsonIgnore]
    public DockBridgeControl<EditorDocumentContent>? Bridge => _bridge;

    [JsonIgnore] 
    public EditorDocumentContent? DocumentContent => _bridge?.Target;

    internal void SetEditorBridge(DockBridgeControl<EditorDocumentContent>? bridge)
    {
        _bridge = bridge;
        base.Content = bridge;
        IconSource = bridge?.Target.GetIcon();
    }

    internal void SetEditorContent(EditorDocumentContent? content)
    {
        if (content != null)
        {
            SetEditorBridge(new DockBridgeControl<EditorDocumentContent>(content));
        }
        else
        {
            SetEditorBridge(null);
        }
    }

    internal void Rebuild()
    {
        if (_bridge is null)
        {
            return;
        }

        var content = _bridge.Target;
        this.Content = null;

        content.RemoveFromVisualTree();
        var newBridge = new DockBridgeControl<EditorDocumentContent>(content);
        SetEditorBridge(newBridge);
    }


    public override bool OnClose()
    {
        if (_bridge is not { } bridge)
        {
            return true;
        }

        var ctrl = _bridge.Target;

        if (ctrl.Document is { } doc)
        {
            if (ctrl._canClose || !doc.IsDirty)
            {
                ctrl.HandleClosed();
                return true;
            }
            else
            {
                ctrl.HandleClosing();
                return false;
            }
        }

        ctrl.HandleClosed();
        return true;
    }
}

public class EditorDocumentContent : UserControl, IDocumentViewHost
{
    private readonly IToolWindow? _toolWindow;

    private readonly DocumentWatching? _docInstance;
    private DisposeCollector? _listeners;

    private EditorDocumentDockable? _dockable;
    private UndoRedoManager? _undoRedoManager;

    internal bool _canClose;
    private bool _closingHandling;

    public EditorDocumentContent()
    {
    }

    public EditorDocumentContent(IToolWindow toolWindow, EditorDocumentDockable? dockable = null)
    {
        _toolWindow = toolWindow ?? throw new ArgumentNullException(nameof(toolWindow));
        this.Name = _toolWindow.WindowId;

        if (toolWindow.GetUIObject() is Control control)
        {
            this.Content = control;
        }
        if (toolWindow is IDrawImGui drawImGui)
        {
            SetupImGui(drawImGui);
        }
        else if (toolWindow is IGraphicObject graphicObject)
        {
            SetupImGui(graphicObject);
        }

        //var text = new TextBox { Text = "OKOK" };
        //this.Content = text;

        SetDockable(dockable);
        UpdateIcon();
        UpdateTitle();

        Dispatcher.UIThread.Post(() =>
        {
            _toolWindow.NotifyShow();
        });
    }

    public EditorDocumentContent(DocumentWatching instance, EditorDocumentDockable? dockable = null)
    {
        _docInstance = instance ?? throw new ArgumentNullException();
        this.Name = Path.GetFileName(instance.Document.FileName.PhysicFileName);

        instance.Document.DirtyChanged += document_DocumentDirtyChanged;
        instance.Document.Renamed += document_Renamed;
        instance.Document.IconChanged += document_iconChanged;

        _listeners += EditorRexes.DocumentAnalyzeEnabled.AsRexListener().Where(v => v).Subscribe(_ =>
        {
            (_docInstance.View as IAnalysable)?.RequestAnalyze();
        }).Push();

        if (_docInstance.View is Control control)
        {
            this.Content = control;
        }
        else if (_docInstance.View is IDrawImGui drawImGui)
        {
            SetupImGui(drawImGui);
        }
        else if (_docInstance.View is IGraphicObject graphicObject)
        {
            SetupImGui(graphicObject);
        }

        SetDockable(dockable);
        UpdateIcon();
        UpdateTitle();

        Dispatcher.UIThread.Post(() =>
        {
            // Delay to avoid double execution updates with View.GetDataFromDocument().
            _docInstance.View.StartView(_docInstance.Document.Content, this);
        });
    }

    [JsonIgnore]
    public EditorDocumentDockable? Dockable => _dockable;

    internal void SetDockable(EditorDocumentDockable? value)
    {
        if (ReferenceEquals(_dockable, value))
        {
            return;
        }

        _dockable?.SetEditorContent(null);
        _dockable = value;

        if (_dockable is { } dockable)
        {
            dockable.SetEditorContent(this);
            dockable.Id = GetPersistString();
        }

        UpdateIcon();
        UpdateTitle();
    }

    public IToolWindow? ToolWindow => _toolWindow;

    public DocumentWatching? DocumentInstance => _docInstance;
    public DocumentEntry? Document => _docInstance?.Document;
    public IDocumentView? DocumentView => _docInstance?.View;
    public IServiceProvider? ViewObject
    {
        get
        {
            if (_docInstance is { } instance)
            {
                if (instance.View is IHasSubDocumentView hasSubView && hasSubView.CurrentSubView is { } subView)
                {
                    return subView as IServiceProvider;
                }
                else
                {
                    return instance.View;
                }
            }
            else if (_toolWindow is IServiceProvider s)
            {
                return s;
            }

            return null;
        }
    }

    public bool ClosedEntirely { get; private set; }

    public event EventHandler? Closed;

    public string GetPersistString()
    {
        if (_docInstance?.Document is { } doc)
        {
            string? rPath = doc.FileName.FullPath?.MakeRalativePath(Project.Current.ProjectBasePath);
            return $":{rPath}";
        }
        else if (_toolWindow is { } toolWindow)
        {
            return toolWindow.WindowId;
        }
        else
        {
            return string.Empty;
        }
    }

    #region Setup Document

    public void SetupImGui(IDrawImGui drawImGui)
    {
        var control = new AvaImGuiControl();

        var theme = AvaImGuiService.Instance.GetEditorTheme(false);
        control.GuiTheme = theme;
        control.BackgroundColor = theme.Colors.GetColor(ColorStyle.Background);
        control.DrawImGui = drawImGui;

        this.Content = control;
    }

    public void SetupImGui(IGraphicObject graphicObject)
    {
        var control = new AvaImGuiControl();

        var theme = AvaImGuiService.Instance.GetEditorTheme(false);
        control.GuiTheme = theme;
        control.BackgroundColor = theme.Colors.GetColor(ColorStyle.Background);
        control.GraphicObject = graphicObject;

        this.Content = control;
    }

    #endregion

    #region UI Events

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        DocumentView?.ActivateView(false);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        DocumentView?.ActivateView(true);
    }


    private void document_DocumentDirtyChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateTitle();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTitle);
        }
    }

    private void document_Renamed(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            _dockable?.Id = GetPersistString();
            UpdateTitle();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTitle);
        }
    }

    private void document_iconChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateIcon();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(UpdateIcon);
        }
    }


    internal async void HandleClosing()
    {
        if (ClosedEntirely || _closingHandling)
        {
            return;
        }

        if (_docInstance is not { } instance)
        {
            _canClose = true;
            return;
        }

        if (_docInstance.Document is not { } doc)
        {
            _canClose = true;
            return;
        }

        try
        {
            _closingHandling = true;

            if (doc.IsDirty)
            {
                var result = await DialogUtility.ShowYesNoCancelDialogAsync(L($"Save {Path.GetFileName(doc.FileName.FullPath)}?"));
                if (!result.HasValue)
                {
                    // cancel
                    return;
                }
                else if (result.Value)
                {
                    // yes
                    if (doc.Save())
                    {
                        _canClose = true;
                        QueuedAction.Do(CloseThisDockable);
                    }
                    else
                    {
                        await DialogUtility.ShowMessageBoxAsyncL($"Failed to save document: {Path.GetFileName(doc.FileName.FullPath)}");
                        return;
                    }
                }
                else
                {
                    // no
                    _canClose = true;

                    string fileName = doc.FileName.FullPath;
                    QueuedAction.Do(() =>
                    {
                        DocumentManager.Instance.CloseDocument(fileName);
                        // Restore resources
                        DocumentManager.Instance.OpenDocument(fileName);
                        DocumentManager.Instance.CloseDocument(doc);
                    });

                    QueuedAction.Do(CloseThisDockable);
                }
            }
        }
        finally
        {
            _closingHandling = false;
        }
    }

    internal async Task<bool?> AskForAppQuit()
    {
        if (_docInstance?.Document is not { } doc)
        {
            return true;
        }

        if (!_docInstance.Document.IsDirty)
        {
            return true;
        }

        try
        {
            var result = await DialogUtility.ShowYesNoCancelDialogAsync(L($"Save {Path.GetFileName(doc.FileName.FullPath)}?"));
            if (!result.HasValue)
            {
                // cancel
                return null;
            }
            else if (result.Value)
            {
                // yes
                if (doc.Save())
                {
                    UpdateTitle();
                    return true;
                }
                else
                {
                    await DialogUtility.ShowMessageBoxAsyncL($"Failed to save document: {Path.GetFileName(doc.FileName.FullPath)}");
                    return null;
                }
            }
            else
            {
                // do not save
                return false;
            }
        }
        catch (Exception err)
        {
            err.LogError();
            await DialogUtility.ShowMessageBoxAsyncL($"Failed to save document: {Path.GetFileName(doc.FileName.FullPath)}");
            return null;
        }
    }

    #endregion

    #region Close & Rebuild

    internal void HandleClosed()
    {
        if (ClosedEntirely)
        {
            _listeners?.Dispose();
            _listeners = null;
            _dockable = null;

            return;
        }

        ClosedEntirely = true;

        if (_docInstance is { } instance)
        {
            DetachDocumentInstance();

            instance.View?.StopView();
            instance.Dispose();
        }

        Closed?.Invoke(this, EventArgs.Empty);

        _listeners?.Dispose();
        _listeners = null;
        _dockable = null;

        //var content = this.Content;
        //this.Content = null;
    }

    internal void DetachDocumentInstance()
    {
        if (_docInstance is not { } instance)
        {
            return;
        }

        if (instance.Document is not { } doc)
        {
            return;
        }

        doc.DirtyChanged -= document_DocumentDirtyChanged;
        doc.Renamed -= document_Renamed;
        doc.IconChanged -= document_iconChanged;
    }

    // Extract a generic close trigger method
    private void CloseThisDockable()
    {
        // Use Dock's Factory to completely remove this component from the UI tree
        _dockable?.Owner?.Factory?.CloseDockable(_dockable);
    }


    #endregion

    #region IDocumentViewHost

    object? IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(UndoRedoManager))
        {
            _undoRedoManager ??= new UndoRedoManager();

            return _undoRedoManager;
        }

        return null;
    }

    #endregion

    public void UpdateIcon()
    {
    }

    public void UpdateTitle()
    {
        string title = string.Empty;

        if (_docInstance?.Document is { } doc)
        {
            if (!string.IsNullOrEmpty(doc.FileName.FullPath))
            {
                title = Path.GetFileNameWithoutExtension(doc.FileName.FullPath);
            }
            else
            {
                title = "Untitled";
            }

            if (doc.IsDirty)
            {
                title = "●" + title; // '●' '*' '•' '·' '⚫' '⚪'
            }
        }
        else if (_toolWindow is { } tool)
        {
            title = tool.Title;
        }

        _dockable?.Title = title;
    }

    public Avalonia.Media.Imaging.Bitmap? GetIcon()
    {
        if (Document is { } doc)
        {
            return doc.Icon?.ToAvaloniaBitmapCached();
        }
        else if (_toolWindow is { } tool)
        {
            return tool.Icon?.ToAvaloniaBitmapCached();
        }
        else
        {
            return null;
        }
    }

    public void LocateInProject()
    {
        if (Document is { } doc)
        {
            string? fileName = doc.FileName?.PhysicFileName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                EditorUtility.LocateInProject(fileName);
            }
        }
    }

    

    public static DocumentEntry? ResolveDocumentPersistantString(string id)
    {
        if (Project.Current is not { } project)
        {
            return null;
        }

        id = id?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (id.StartsWith(':'))
        {
            id = id[1..];

            var fullPath = project.ProjectBasePath.PathAppend(id);

            return DocumentManager.Instance.OpenDocument(fullPath);
        }

        return null;
    }

    public static IToolWindow? ResolveToolWindowPersistantString(string id)
    {
        if (Project.Current is not { } project)
        {
            return null;
        }

        id = id?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (!id.StartsWith(':'))
        {
            return EditorServices.ToolWindow.GetToolWindow(id);
        }

        return null;
    }
}
