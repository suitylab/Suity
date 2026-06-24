using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using Suity.Controls;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using System;

namespace Suity.Editor.Controls;

public class EditorToolDockable : Dock.Model.Avalonia.Controls.Tool
{
    private DockBridgeControl<EditorToolContent>? _bridge;

    public object? IconSource { get; internal set; }

    public EditorToolDockable()
    {
        this.CanFloat = false;
        this.CanDockAsDocument = false;
        this.CanPin = true;
    }

    public EditorToolDockable(EditorToolContent content)
        : this()
    {
        SetEditorContent(content);
    }

    [JsonIgnore]
    public DockBridgeControl<EditorToolContent>? Bridge => _bridge;

    [JsonIgnore]
    public EditorToolContent? EditorContent => _bridge?.Target;

    internal void SetEditorBridge(DockBridgeControl<EditorToolContent>? bridge)
    {
        _bridge = bridge;
        base.Content = bridge;
        IconSource = bridge?.Target.ToolWindow?.Icon?.ToAvaloniaBitmapCached();
    }

    internal void SetEditorContent(EditorToolContent? content)
    {
        if (content != null)
        {
            content.RemoveFromVisualTree();
            SetEditorBridge(new DockBridgeControl<EditorToolContent>(content));
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
        var newBridge = new DockBridgeControl<EditorToolContent>(content);
        SetEditorBridge(newBridge);
    }

    public override bool OnClose()
    {
        if (_bridge is not { } bridge)
        {
            return true;
        }

        var ctrl = _bridge.Target;
        ctrl.HandleClosed();

        return true;
    }
}

public class EditorToolContent : UserControl
{
    private readonly IToolWindow? _toolWindow;

    private EditorToolDockable? _dockable;

    public EditorToolContent()
    {
    }

    public EditorToolContent(IToolWindow toolWindow, EditorToolDockable? dockable = null)
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

    [JsonIgnore]
    public EditorToolDockable? Dockable => _dockable;

    internal void SetDockable(EditorToolDockable? value)
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
            dockable.Id = _toolWindow?.WindowId ?? "???";
        }

        UpdateIcon();
        UpdateTitle();
    }

    public IToolWindow? ToolWindow => _toolWindow;

    public event EventHandler? Closed;

    #region Setup

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

        try
        {
            _toolWindow?.NotifyShow();
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        try
        {
            _toolWindow?.NotifyHide();
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    internal void HandleClosed()
    {
        _dockable = null;

        Closed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public void UpdateIcon()
    {
    }

    public void UpdateTitle()
    {
        string title = _toolWindow?.Title ?? _dockable?.Id ?? "???";
        _dockable?.Title = title;
    }
}