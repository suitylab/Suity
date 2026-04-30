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
    private EditorToolContent? _toolControl;

    public object? IconSource { get; internal set; }

    public EditorToolDockable()
    {
        this.CanFloat = false;
        this.CanDockAsDocument = false;
        this.CanPin = true;

        //IconSource = "M12,2L1,21H23L12,2M12,6L19.53,19H4.47L12,6M11,10V14H13V10H11M11,16V18H13V16H11Z";
    }

    public EditorToolDockable(EditorToolContent toolControl)
        : this()
    {
        _toolControl = toolControl ?? throw new ArgumentNullException(nameof(toolControl));

        base.Content = toolControl;
        base.CanClose = true;
    }

    [JsonIgnore]
    public EditorToolContent? EditorContent
    {
        get => _toolControl;
        internal set
        {
            _toolControl = value;
            base.Content = value;
            IconSource = value?.ToolWindow?.Icon?.ToAvaloniaBitmapCached();
        }
    }

    public override bool OnClose()
    {
        _toolControl?.HandleClosed();
        _toolControl = null;

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

        Dockable = dockable;
        UpdateIcon();
        UpdateTitle();

        Dispatcher.UIThread.Post(() =>
        {
            _toolWindow.NotifyShow();
        });
    }

    [JsonIgnore]
    public EditorToolDockable? Dockable
    {
        get => _dockable;
        internal set
        {
            if (ReferenceEquals(_dockable, value))
            {
                return;
            }

            _dockable?.EditorContent = null;
            _dockable = value;

            if (_dockable is { } dockable)
            {
                dockable.EditorContent = this;
                dockable.Id = _toolWindow?.WindowId ?? "???";
            }

            UpdateIcon();
            UpdateTitle();
        }
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