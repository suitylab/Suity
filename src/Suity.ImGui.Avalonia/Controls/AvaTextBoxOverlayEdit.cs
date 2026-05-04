using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Suity.Views.Graphics;

namespace Suity.Controls;

/// <summary>
/// Provides an overlay text box editing control for Avalonia.
/// </summary>
public class AvaTextBoxOverlayEdit : IGraphicTextBoxEdit
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
    /// Initializes a new instance of the <see cref="AvaTextBoxOverlayEdit"/> class.
    /// </summary>
    /// <param name="control">The parent control.</param>
    public AvaTextBoxOverlayEdit(Control control)
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
            Background = Brushes.Transparent,
            IsHitTestVisible = true
        };

        _container.Children.Add(_textBox);

        // Click background to close
        _container.PointerPressed += (s, e) =>
        {
            if (e.Source == _container) EndTextEdit();
        };

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
        _textBox.PasswordChar = option.IsPassword ? '●' : '\0';  // '●' '*' '•' '·' '⚫' '⚪'

        // 3. Solve multiline centering issue: multiline aligns to top, single line centers
        if (option.MultiLine)
        {
            _textBox.VerticalContentAlignment = VerticalAlignment.Top;
            _textBox.TextWrapping = TextWrapping.Wrap;
            // Add some padding to prevent multiline text from appearing cramped against the top edge
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
        // Ensure the mask container covers the entire window to capture external clicks
        _container.Width = topLevel.Bounds.Width;
        _container.Height = topLevel.Bounds.Height;

        // Convert rect (relative to _control) to coordinates relative to topLevel
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
        OverlayLayer.GetOverlayLayer(topLevel)?.Children.Remove(_container);

        // --- Core fix: Normalize line breaks ---
        string resultText = _textBox.Text ?? string.Empty;

        // Replace Windows-style \r\n with standard \n
        // This eliminates the "box" characters that appear in some rendering engines
        resultText = resultText.Replace("\r\n", "\n").Replace("\r", "\n");

        _currentCallback?.Invoke(resultText);
        _currentCallback = null;
        _control.Focus();
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

        // 1. 获取 TextBox 的字体相关参数
        var typeface = new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight);
        var fontSize = textBox.FontSize;
        var text = textBox.Text ?? "";

        // 2. 使用 TextLayout 精确测量文本宽度
        // 这里的 Canvas.Infinite 是为了测量不受限的完整宽度
        var textLayout = new TextLayout(
            text,
            typeface,
            fontSize,
            textBox.Foreground,
            TextAlignment.Left,
            maxWidth: double.PositiveInfinity);

        // 3. 计算最终宽度：文本宽度 + 左右 Padding + 光标预留空间
        var horizontalPadding = textBox.Padding.Left + textBox.Padding.Right;

        double width = textLayout.Width + horizontalPadding + 5;
        if (width < _initRect.Width)
        {
            width = _initRect.Width;
        }

        // 注意：TextBox 内部通常还有个几个像素的默认偏移，建议额外 + 2 到 5 像素防止文字抖动
        textBox.Width = width;
    }

}
