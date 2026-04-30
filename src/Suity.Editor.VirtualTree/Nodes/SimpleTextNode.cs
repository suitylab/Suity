using System.Drawing;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A simple virtual tree node that displays static text with optional icons.
/// This node does not bind to a value source; it displays fixed text and preview content.
/// </summary>
public class SimpleTextNode : VirtualNode
{
    private string _text = string.Empty;
    private readonly Image _icon;
    private readonly string _previewText = string.Empty;
    private readonly Image _previewIcon;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTextNode"/> class with the specified text.
    /// </summary>
    /// <param name="text">The display text.</param>
    public SimpleTextNode(string text)
        : this(text, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTextNode"/> class with the specified text and icon.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="icon">The main icon.</param>
    public SimpleTextNode(string text, Image icon)
        : this(text, icon, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTextNode"/> class with full configuration.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="icon">The main icon.</param>
    /// <param name="previewText">The preview text.</param>
    /// <param name="previewIcon">The preview icon.</param>
    public SimpleTextNode(string text, Image icon, string previewText, Image previewIcon)
    {
        _text = text ?? string.Empty;
        _icon = icon;
        _previewText = previewText ?? string.Empty;
        _previewIcon = previewIcon;
    }

    /// <inheritdoc/>
    public override object DisplayedValue => null;

    /// <inheritdoc/>
    protected override string GetText()
    {
        return _text;
    }

    /// <inheritdoc/>
    protected override void SetText(string value)
    {
        _text = value;
    }

    /// <inheritdoc/>
    protected override Image GetMainIcon()
    {
        return _icon;
    }
}
