using System.Drawing;
using System.Drawing.Drawing2D;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Defines the visual theme for the graph control, including colors, brushes, pens, and fonts.
/// </summary>
public class GraphControlTheme
{
    /// <summary>
    /// Gets or sets the color for node text.
    /// </summary>
    public Color NodeTextColor { get; set; } = Color.FromArgb(255, 255, 255, 255);
    /// <summary>
    /// Gets or sets the color for node text shadow.
    /// </summary>
    public Color NodeTextShadowColor { get; set; } = Color.FromArgb(128, 0, 0, 0);
    /// <summary>
    /// Gets or sets the fill color for nodes.
    /// </summary>
    public Color NodeFillColor { get; set; } = Color.FromArgb(255, 128, 128, 128);
    /// <summary>
    /// Gets or sets the fill color for selected nodes.
    /// </summary>
    public Color NodeFillSelectedColor { get; set; } = Color.FromArgb(255, 160, 128, 100);
    /// <summary>
    /// Gets or sets the outline color for nodes.
    /// </summary>
    public Color NodeOutlineColor { get; set; } = Color.FromArgb(255, 180, 180, 180);
    /// <summary>
    /// Gets or sets the outline color for selected nodes.
    /// </summary>
    public Color NodeOutlineSelectedColor { get; set; } = Color.FromArgb(255, 192, 160, 128);
    /// <summary>
    /// Gets or sets the text color for connectors.
    /// </summary>
    public Color ConnectorTextColor { get; set; } = Color.FromArgb(255, 64, 64, 64);
    /// <summary>
    /// Gets or sets the fill color for connectors.
    /// </summary>
    public Color ConnectorFillColor { get; set; } = Color.FromArgb(255, 0, 0, 0);
    /// <summary>
    /// Gets or sets the outline color for connectors.
    /// </summary>
    public Color ConnectorOutlineColor { get; set; } = Color.FromArgb(255, 32, 32, 32);
    /// <summary>
    /// Gets or sets the fill color for selected connectors.
    /// </summary>
    public Color ConnectorSelectedFillColor { get; set; } = Color.FromArgb(255, 32, 32, 32);
    /// <summary>
    /// Gets or sets the outline color for selected connectors.
    /// </summary>
    public Color ConnectorOutlineSelectedColor { get; set; } = Color.FromArgb(255, 64, 64, 64);
    /// <summary>
    /// Gets or sets the fill color for the selection box.
    /// </summary>
    public Color SelectionFillColor { get; set; } = Color.FromArgb(64, 128, 90, 30);
    /// <summary>
    /// Gets or sets the outline color for the selection box.
    /// </summary>
    public Color SelectionOutlineColor { get; set; } = Color.FromArgb(192, 255, 180, 60);
    /// <summary>
    /// Gets or sets the color for links.
    /// </summary>
    public Color LinkColor { get; set; } = Color.FromArgb(255, 180, 180, 180);
    /// <summary>
    /// Gets or sets the color for editable (dragging) links.
    /// </summary>
    public Color LinkEditableColor { get; set; } = Color.FromArgb(255, 64, 255, 0);
    /// <summary>
    /// Gets or sets the color for valid node signals.
    /// </summary>
    public Color NodeSignalValidColor { get; set; } = Color.FromArgb(255, 0, 255, 0);
    /// <summary>
    /// Gets or sets the color for invalid node signals.
    /// </summary>
    public Color NodeSignalInvalidColor { get; set; } = Color.FromArgb(255, 255, 0, 0);
    /// <summary>
    /// Gets or sets the color for node headers.
    /// </summary>
    public Color NodeHeaderColor { get; set; } = Color.FromArgb(128, 0, 0, 0);
    /// <summary>
    /// Gets or sets the background color of the graph.
    /// </summary>
    public Color BackColor { get; set; } = Color.FromArgb(255, 50, 50, 50);

    // Brushes
    /// <summary>
    /// Gets or sets the brush for node text.
    /// </summary>
    public SolidBrush NodeText { get; set; }
    /// <summary>
    /// Gets or sets the brush for node text shadow.
    /// </summary>
    public SolidBrush NodeTextShadow { get; set; }
    /// <summary>
    /// Gets or sets the brush for node header fill.
    /// </summary>
    public SolidBrush NodeHeaderFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for node fill.
    /// </summary>
    public SolidBrush NodeFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for selected node fill.
    /// </summary>
    public SolidBrush NodeFillSelected { get; set; }
    /// <summary>
    /// Gets or sets the brush for valid node signals.
    /// </summary>
    public SolidBrush NodeSignalValid { get; set; }
    /// <summary>
    /// Gets or sets the brush for invalid node signals.
    /// </summary>
    public SolidBrush NodeSignalInvalid { get; set; }
    /// <summary>
    /// Gets or sets the brush for connector text.
    /// </summary>
    public SolidBrush ConnectorText { get; set; }
    /// <summary>
    /// Gets or sets the brush for connector fill.
    /// </summary>
    public SolidBrush ConnectorFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for selected connector fill.
    /// </summary>
    public SolidBrush ConnectorFillSelected { get; set; }
    /// <summary>
    /// Gets or sets the brush for selection box fill.
    /// </summary>
    public SolidBrush SelectionFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for editable link type hint.
    /// </summary>
    public SolidBrush LinkEditbleType { get; set; }
    /// <summary>
    /// Gets or sets the brush for link arrows.
    /// </summary>
    public SolidBrush LinkArrow { get; set; }
    /// <summary>
    /// Gets or sets the brush for error link arrows.
    /// </summary>
    public SolidBrush LinkArrowError { get; set; }

    // Pens
    /// <summary>
    /// Gets or sets the pen for node outlines.
    /// </summary>
    public Pen NodeOutline { get; set; }
    /// <summary>
    /// Gets or sets the pen for selected node outlines.
    /// </summary>
    public Pen NodeOutlineSelected { get; set; }
    /// <summary>
    /// Gets or sets the pen for connector outlines.
    /// </summary>
    public Pen ConnectorOutline { get; set; }
    /// <summary>
    /// Gets or sets the pen for selected connector outlines.
    /// </summary>
    public Pen ConnectorOutlineSelected { get; set; }
    /// <summary>
    /// Gets or sets the pen for selection box outlines.
    /// </summary>
    public Pen SelectionOutline { get; set; }
    /// <summary>
    /// Gets or sets the pen for links.
    /// </summary>
    public Pen Link { get; set; }
    /// <summary>
    /// Gets or sets the pen for editable links.
    /// </summary>
    public Pen LinkEditable { get; set; }
    /// <summary>
    /// Gets or sets the pen for error links.
    /// </summary>
    public Pen LinkError { get; set; }
    /// <summary>
    /// Gets or sets the pen for combined connectors.
    /// </summary>
    public Pen CombinedConnectorPen { get; set; }
    /// <summary>
    /// Gets or sets the pen for combined links.
    /// </summary>
    public Pen CombinedLinkPen { get; set; }

    // Fonts
    /// <summary>
    /// Gets or sets the font for node titles.
    /// </summary>
    public Font NodeTitleFont { get; set; }
    /// <summary>
    /// Gets or sets the font for node preview text.
    /// </summary>
    public Font NodePreviewFont { get; set; }
    /// <summary>
    /// Gets or sets the font for connector labels.
    /// </summary>
    public Font NodeConnectorFont { get; set; }
    /// <summary>
    /// Gets or sets the scaled font for node titles.
    /// </summary>
    public Font NodeScaledTitleFont { get; set; }
    /// <summary>
    /// Gets or sets the scaled font for node preview text.
    /// </summary>
    public Font NodeScaledPreviewFont { get; set; }
    /// <summary>
    /// Gets or sets the scaled font for connector labels.
    /// </summary>
    public Font NodeScaledConnectorFont { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphControlTheme"/> class.
    /// </summary>
    public GraphControlTheme()
    {
        InitializeBrushesAndPens();
    }

    /// <summary>
    /// Initializes the theme fonts using the specified default font family.
    /// </summary>
    /// <param name="defaultFont">The default font family to use.</param>
    public void InitializeFonts(FontFamily defaultFont)
    {
        NodeTitleFont = new Font(defaultFont, 8.0f);
        NodePreviewFont = new Font(defaultFont, 16.0f);
        NodeConnectorFont = new Font(defaultFont, 7.0f);
        NodeScaledTitleFont = new Font(NodeTitleFont.Name, NodeTitleFont.Size);
        NodeScaledPreviewFont = new Font(NodePreviewFont.Name, NodePreviewFont.Size);
        NodeScaledConnectorFont = new Font(NodeConnectorFont.Name, NodeConnectorFont.Size);
    }

    /// <summary>
    /// Updates the scaled fonts based on the current zoom level.
    /// </summary>
    /// <param name="zoom">The current zoom factor.</param>
    public void UpdateScaledFonts(float zoom)
    {
        NodeScaledTitleFont = new Font(NodeTitleFont.Name, NodeTitleFont.Size * zoom);
        NodeScaledPreviewFont = new Font(NodePreviewFont.Name, NodePreviewFont.Size * zoom);
        NodeScaledConnectorFont = new Font(NodeConnectorFont.Name, NodeConnectorFont.Size * zoom);
    }

    private void InitializeBrushesAndPens()
    {
        NodeText = new SolidBrush(NodeTextColor);
        NodeTextShadow = new SolidBrush(NodeTextShadowColor);
        NodeHeaderFill = new SolidBrush(NodeHeaderColor);
        NodeFill = new SolidBrush(NodeFillColor);
        NodeFillSelected = new SolidBrush(NodeFillSelectedColor);
        NodeSignalValid = new SolidBrush(NodeSignalValidColor);
        NodeSignalInvalid = new SolidBrush(NodeSignalInvalidColor);
        ConnectorText = new SolidBrush(ConnectorTextColor);
        ConnectorFill = new SolidBrush(ConnectorFillColor);
        ConnectorFillSelected = new SolidBrush(ConnectorSelectedFillColor);
        SelectionFill = new SolidBrush(SelectionFillColor);
        LinkEditbleType = new SolidBrush(LinkEditableColor);
        LinkArrow = new SolidBrush(LinkColor);
        LinkArrowError = new SolidBrush(NodeSignalInvalidColor);

        NodeOutline = new Pen(NodeOutlineColor, 2);
        NodeOutlineSelected = new Pen(NodeOutlineSelectedColor, 2);
        ConnectorOutline = new Pen(ConnectorOutlineColor);
        ConnectorOutlineSelected = new Pen(ConnectorOutlineSelectedColor);
        SelectionOutline = new Pen(SelectionOutlineColor);
        Link = new Pen(LinkColor, 3f);
        LinkEditable = new Pen(LinkEditableColor, 3f);
        LinkError = new Pen(NodeSignalInvalidColor, 3);
        CombinedConnectorPen = new Pen(Color.White, 2);
        CombinedLinkPen = new Pen(Color.White, 5);
    }

    public void UpdateNodeTextColor(Color color)
    {
        NodeTextColor = color;
        NodeText = new SolidBrush(color);
    }

    public void UpdateNodeTextShadowColor(Color color)
    {
        NodeTextShadowColor = color;
        NodeTextShadow = new SolidBrush(color);
    }

    public void UpdateNodeFillColor(Color color)
    {
        NodeFillColor = color;
        NodeFill = new SolidBrush(color);
    }

    public void UpdateNodeFillSelectedColor(Color color)
    {
        NodeFillSelectedColor = color;
        NodeFillSelected = new SolidBrush(color);
    }

    public void UpdateNodeOutlineColor(Color color)
    {
        NodeOutlineColor = color;
        NodeOutline = new Pen(color, 2);
    }

    public void UpdateNodeOutlineSelectedColor(Color color)
    {
        NodeOutlineSelectedColor = color;
        NodeOutlineSelected = new Pen(color, 2);
    }

    public void UpdateNodeSignalValidColor(Color color)
    {
        NodeSignalValidColor = color;
        NodeSignalValid = new SolidBrush(color);
    }

    public void UpdateNodeSignalInvalidColor(Color color)
    {
        NodeSignalInvalidColor = color;
        NodeSignalInvalid = new SolidBrush(color);
        LinkError = new Pen(color, 3);
        LinkArrowError = new SolidBrush(color);
    }

    public void UpdateConnectorTextColor(Color color)
    {
        ConnectorTextColor = color;
        ConnectorText = new SolidBrush(color);
    }

    public void UpdateConnectorFillColor(Color color)
    {
        ConnectorFillColor = color;
        ConnectorFill = new SolidBrush(color);
    }

    public void UpdateConnectorOutlineColor(Color color)
    {
        ConnectorOutlineColor = color;
        ConnectorOutline = new Pen(color);
    }

    public void UpdateConnectorFillSelectedColor(Color color)
    {
        ConnectorSelectedFillColor = color;
        ConnectorFillSelected = new SolidBrush(color);
    }

    public void UpdateConnectorOutlineSelectedColor(Color color)
    {
        ConnectorOutlineSelectedColor = color;
        ConnectorOutlineSelected = new Pen(color);
    }

    public void UpdateSelectionFillColor(Color color)
    {
        SelectionFillColor = color;
        SelectionFill = new SolidBrush(color);
    }

    public void UpdateSelectionOutlineColor(Color color)
    {
        SelectionOutlineColor = color;
        SelectionOutline = new Pen(color);
    }

    public void UpdateNodeHeaderColor(Color color)
    {
        NodeHeaderColor = color;
        NodeHeaderFill = new SolidBrush(color);
    }

    public void UpdateLinkColor(Color color)
    {
        LinkColor = color;
        Link = new Pen(color, 3f);
        LinkArrow = new SolidBrush(color);
    }

    public void UpdateLinkEditableColor(Color color)
    {
        LinkEditableColor = color;
        LinkEditable = new Pen(color, 3f);
        LinkEditbleType = new SolidBrush(color);
    }
}
