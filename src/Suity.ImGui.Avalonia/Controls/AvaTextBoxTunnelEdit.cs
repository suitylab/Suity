using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Suity.Views.Graphics;

namespace Suity.Controls;

/// <summary>
/// Provides an overlay text box editing control for Avalonia using Tunneling events for light dismiss.
/// </summary>
public class AvaTextBoxTunnelEdit : IGraphicTextBoxEdit
{
    private readonly Control _control;
    private readonly TextBox _textBox;
    private readonly Canvas _container; // Container for positioning

    private Action<string>? _currentCallback;
    private TextBoxEditSubmitMode _submitNode;
    private System.Drawing.Rectangle _initRect;
    private bool _multiLine;
    private bool _autoFit;
    private bool _editing;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaTextBoxTunnelEdit"/> class.
    /// </summary>
    /// <param name="control">The parent control.</param>
    public AvaTextBoxTunnelEdit(Control control)
    {
        _control = control;

        _textBox = new TextBox
        {
            Focusable = true,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(2),
            VerticalContentAlignment = VerticalAlignment.Center
        };

        _container = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            // Background 默认为 null，允许鼠标事件穿透空白区域
            ClipToBounds = false
        };

        _container.Children.Add(_textBox);

        _textBox.TextChanged += (s, e) =>
        {
            if (_submitNode == TextBoxEditSubmitMode.TextChanged)
            {
                _currentCallback?.Invoke(_textBox.Text ?? string.Empty);
            }

            if (_autoFit && !_multiLine)
            {
                Fit();
            }
        };

        _textBox.KeyDown += OnEditorKeyDown;
    }

    /// <summary>
    /// Gets a value indicating whether the text box is currently in editing mode.
    /// </summary>
    public bool Editing => _editing;

    /// <inheritdoc/>
    public void BeginTextEdit(System.Drawing.Rectangle rect, string text, TextBoxEditOptions option)
    {
        if (_editing) return;

        // 1. Get top-level window and OverlayLayer
        var topLevel = TopLevel.GetTopLevel(_control);
        if (topLevel == null) return;

        var overlay = OverlayLayer.GetOverlayLayer(topLevel);
        if (overlay == null) return;

        _currentCallback = option.EditedCallBack;
        _submitNode = option.SubmitMode;
        _autoFit = option.AutoFit;

        // 2. Configure TextBox properties
        _textBox.Text = text;
        _textBox.IsReadOnly = option.IsReadonly;
        _textBox.AcceptsReturn = _multiLine = option.MultiLine;
        _textBox.PasswordChar = option.IsPassword ? '●' : '\0';

        // 3. Solve multiline centering issue
        if (option.MultiLine)
        {
            _textBox.VerticalContentAlignment = VerticalAlignment.Top;
            _textBox.TextWrapping = TextWrapping.Wrap;
            _textBox.Padding = new Thickness(4, 4);
        }
        else
        {
            _textBox.VerticalContentAlignment = VerticalAlignment.Center;
            _textBox.TextWrapping = TextWrapping.NoWrap;
            _textBox.Padding = new Thickness(4, 0);
        }

        // 4. Set font
        if (option.Font is { } font)
        {
            _textBox.FontFamily = new FontFamily(font.Name);
            _textBox.FontSize = font.Size;
        }

        // 5. Layout and coordinate transformation
        _container.Width = topLevel.Bounds.Width;
        _container.Height = topLevel.Bounds.Height;

        var controlOrigin = _control.TranslatePoint(new Point(0, 0), topLevel) ?? new Point(0, 0);

        _initRect = rect;
        _textBox.Width = rect.Width;
        _textBox.Height = rect.Height;
        Canvas.SetLeft(_textBox, controlOrigin.X + rect.X);
        Canvas.SetTop(_textBox, controlOrigin.Y + rect.Y);

        // 6. Mount and display
        if (!overlay.Children.Contains(_container))
        {
            overlay.Children.Add(_container);
        }

        _editing = true;
        _container.IsVisible = true;

        // 核心改动：在顶层窗口注册隧道（Tunnel）级别的点击监听
        topLevel.AddHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed, RoutingStrategies.Tunnel);

        // 7. Force refresh and focus
        _textBox.ApplyTemplate();
        _textBox.UpdateLayout();

        Dispatcher.UIThread.Post(() =>
        {
            _textBox.Focus();
            if (!option.MultiLine)
            {
                _textBox.SelectAll();
            }
            else
            {
                _textBox.ScrollToLine(0);
            }
        }, DispatcherPriority.Input);
    }

    /// <inheritdoc/>
    public void EndTextEdit()
    {
        if (!_editing) return;
        _editing = false;

        _container.IsVisible = false;

        var topLevel = TopLevel.GetTopLevel(_control);
        if (topLevel != null)
        {
            OverlayLayer.GetOverlayLayer(topLevel)?.Children.Remove(_container);

            // 核心改动：清理编辑状态，注销全局隧道监听，防止内存泄漏
            topLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed);
        }

        // --- Core fix: Normalize line breaks ---
        string resultText = _textBox.Text ?? string.Empty;
        resultText = resultText.Replace("\r\n", "\n").Replace("\r", "\n");

        _currentCallback?.Invoke(resultText);
        _currentCallback = null;
        _control.Focus();
    }

    private void OnTopLevelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_editing) return;

        // 检查点击事件源是否在 TextBox 内部（包括其子元素）
        bool isInsideTextBox = false;
        var current = e.Source as Visual;

        while (current != null)
        {
            if (current == _textBox)
            {
                isInsideTextBox = true;
                break;
            }
            current = current.GetVisualParent();
        }

        // 如果点击发生在 TextBox 外部，关闭编辑，但【不拦截】事件，允许它继续穿透传递给底层 UI
        if (!isInsideTextBox)
        {
            EndTextEdit();
        }
    }

    private void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (!_textBox.AcceptsReturn || _submitNode == TextBoxEditSubmitMode.Enter)
            {
                e.Handled = true;
                EndTextEdit();
            }
        }
        else if (e.Key == Key.Escape)
        {
            _currentCallback = null;
            EndTextEdit();
        }
    }

    private void Fit()
    {
        var textBox = _textBox;

        var typeface = new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight);
        var fontSize = textBox.FontSize;
        var text = textBox.Text ?? "";

        var textLayout = new TextLayout(
            text,
            typeface,
            fontSize,
            textBox.Foreground,
            TextAlignment.Left,
            maxWidth: double.PositiveInfinity);

        var horizontalPadding = textBox.Padding.Left + textBox.Padding.Right;

        double width = textLayout.Width + horizontalPadding + 5;
        if (width < _initRect.Width)
        {
            width = _initRect.Width;
        }

        textBox.Width = width;
    }
}