using Suity.Helpers;
using System.Drawing;

namespace Suity.Views.PathTree;

/// <summary>
/// A path node that displays arbitrary text with an optional image and text status coloring.
/// </summary>
public class TextNode : PathNode
{
    /// <summary>
    /// Gets a value indicating whether the user can drag this node. Always returns false for text nodes.
    /// </summary>
    public override bool CanUserDrag => false;

    private string _text;
    private Image _image;
    private TextStatus _textStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextNode"/> class with default values.
    /// </summary>
    public TextNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextNode"/> class with the specified text, image, and status.
    /// </summary>
    /// <param name="text">The display text for the node.</param>
    /// <param name="image">An optional image to display alongside the text.</param>
    /// <param name="status">The text status used for coloring.</param>
    public TextNode(string text, Image image = null, TextStatus status = TextStatus.Normal)
    {
        _text = text ?? string.Empty;
        _image = image?.ToIconSmall();
        _textStatus = status;
    }

    /// <summary>
    /// Sets the image displayed by this node.
    /// </summary>
    /// <param name="image">The image to display.</param>
    public void SetImage(Image image)
    {
        _image = image.ToIconSmall();
    }

    /// <summary>
    /// Sets the display text for this node.
    /// </summary>
    /// <param name="value">The new display text.</param>
    public void SetString(string value)
    {
        _text = value;
    }

    /// <summary>
    /// Sets the text status used for coloring the node's text.
    /// </summary>
    /// <param name="status">The new text status.</param>
    public void SetTextStatus(TextStatus status)
    {
        _textStatus = status;
    }

    /// <summary>
    /// Returns the display text for this node.
    /// </summary>
    /// <returns>The current text value.</returns>
    protected override string OnGetText()
    {
        return _text;
    }

    /// <summary>
    /// Gets the image displayed by this node.
    /// </summary>
    public override Image Image => _image;

    /// <summary>
    /// Gets the text color status for this node.
    /// </summary>
    public override TextStatus TextColorStatus => _textStatus;

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    /// <returns>The display text of the node.</returns>
    public override string ToString()
    {
        return _text ?? string.Empty;
    }
}
