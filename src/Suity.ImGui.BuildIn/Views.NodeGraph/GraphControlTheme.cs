using Suity.Drawing;
using System.Drawing;

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
    public SolidBrushDef NodeText { get; set; }
    /// <summary>
    /// Gets or sets the brush for node text shadow.
    /// </summary>
    public SolidBrushDef NodeTextShadow { get; set; }
    /// <summary>
    /// Gets or sets the brush for node header fill.
    /// </summary>
    public SolidBrushDef NodeHeaderFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for node fill.
    /// </summary>
    public SolidBrushDef NodeFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for selected node fill.
    /// </summary>
    public SolidBrushDef NodeFillSelected { get; set; }
    /// <summary>
    /// Gets or sets the brush for valid node signals.
    /// </summary>
    public SolidBrushDef NodeSignalValid { get; set; }
    /// <summary>
    /// Gets or sets the brush for invalid node signals.
    /// </summary>
    public SolidBrushDef NodeSignalInvalid { get; set; }
    /// <summary>
    /// Gets or sets the brush for connector text.
    /// </summary>
    public SolidBrushDef ConnectorText { get; set; }
    /// <summary>
    /// Gets or sets the brush for connector fill.
    /// </summary>
    public SolidBrushDef ConnectorFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for selected connector fill.
    /// </summary>
    public SolidBrushDef ConnectorFillSelected { get; set; }
    /// <summary>
    /// Gets or sets the brush for selection box fill.
    /// </summary>
    public SolidBrushDef SelectionFill { get; set; }
    /// <summary>
    /// Gets or sets the brush for editable link type hint.
    /// </summary>
    public SolidBrushDef LinkEditbleType { get; set; }
    /// <summary>
    /// Gets or sets the brush for link arrows.
    /// </summary>
    public SolidBrushDef LinkArrow { get; set; }

    /// <summary>
    /// Gets or sets the brush for error link arrows.
    /// </summary>
    public SolidBrushDef LinkArrowError { get; set; }

    // Pens
    /// <summary>
    /// Gets or sets the pen for node outlines.
    /// </summary>
    public PenDef NodeOutline { get; set; }

    /// <summary>
    /// Gets or sets the pen for selected node outlines.
    /// </summary>
    public PenDef NodeOutlineSelected { get; set; }

    /// <summary>
    /// Gets or sets the pen for connector outlines.
    /// </summary>
    public PenDef ConnectorOutline { get; set; }

    /// <summary>
    /// Gets or sets the pen for selected connector outlines.
    /// </summary>
    public PenDef ConnectorOutlineSelected { get; set; }

    /// <summary>
    /// Gets or sets the pen for selection box outlines.
    /// </summary>
    public PenDef SelectionOutline { get; set; }

    /// <summary>
    /// Gets or sets the pen for links.
    /// </summary>
    public PenDef Link { get; set; }

    /// <summary>
    /// Gets or sets the pen for editable links.
    /// </summary>
    public PenDef LinkEditable { get; set; }

    /// <summary>
    /// Gets or sets the pen for error links.
    /// </summary>
    public PenDef LinkError { get; set; }

    /// <summary>
    /// Gets or sets the pen for combined connectors.
    /// </summary>
    public PenDef ConnectorCombinedPen { get; set; }

    /// <summary>
    /// Gets or sets the pen for combined links.
    /// </summary>
    public PenDef LinkCombinedPen { get; set; }

    /// <summary>
    /// Gets or sets the pen for selected links.
    /// </summary>
    public PenDef LinkSelectedPen { get; set; }

    /// <summary>
    /// Gets or sets the pen for selected combined links.
    /// </summary>
    public PenDef LinkSelectedCombinedPen { get; set; }

    // Fonts
    /// <summary>
    /// Gets or sets the font for node titles.
    /// </summary>
    public FontDef NodeTitleFont { get; set; }

    /// <summary>
    /// Gets or sets the font for node preview text.
    /// </summary>
    public FontDef NodePreviewFont { get; set; }

    /// <summary>
    /// Gets or sets the font for connector labels.
    /// </summary>
    public FontDef NodeConnectorFont { get; set; }

    /// <summary>
    /// Gets or sets the scaled font for node titles.
    /// </summary>
    public FontDef NodeScaledTitleFont { get; set; }

    /// <summary>
    /// Gets or sets the scaled font for node preview text.
    /// </summary>
    public FontDef NodeScaledPreviewFont { get; set; }

    /// <summary>
    /// Gets or sets the scaled font for connector labels.
    /// </summary>
    public FontDef NodeScaledConnectorFont { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphControlTheme"/> class.
    /// </summary>
    public GraphControlTheme()
    {
        UpdateBrushesAndPens();
    }

    /// <summary>
    /// Initializes the theme fonts using the specified default font family.
    /// </summary>
    /// <param name="defaultFont">The default font family to use.</param>
    public void InitializeFonts(FontFamilyDef defaultFont)
    {
        NodeTitleFont = new FontDef(defaultFont, 8.0f);
        NodePreviewFont = new FontDef(defaultFont, 16.0f);
        NodeConnectorFont = new FontDef(defaultFont, 7.0f);
        NodeScaledTitleFont = new FontDef(NodeTitleFont.Name, NodeTitleFont.Size);
        NodeScaledPreviewFont = new FontDef(NodePreviewFont.Name, NodePreviewFont.Size);
        NodeScaledConnectorFont = new FontDef(NodeConnectorFont.Name, NodeConnectorFont.Size);
    }

    /// <summary>
    /// Updates the scaled fonts based on the current zoom level.
    /// </summary>
    /// <param name="zoom">The current zoom factor.</param>
    public void UpdateScaledFonts(float zoom)
    {
        NodeScaledTitleFont = new FontDef(NodeTitleFont.Name, NodeTitleFont.Size * zoom);
        NodeScaledPreviewFont = new FontDef(NodePreviewFont.Name, NodePreviewFont.Size * zoom);
        NodeScaledConnectorFont = new FontDef(NodeConnectorFont.Name, NodeConnectorFont.Size * zoom);
    }

    /// <summary>
    /// Updates the brushes and pens based on the current theme colors.
    /// </summary>
    public void UpdateBrushesAndPens()
    {
        NodeText = new SolidBrushDef(NodeTextColor);
        NodeTextShadow = new SolidBrushDef(NodeTextShadowColor);
        NodeHeaderFill = new SolidBrushDef(NodeHeaderColor);
        NodeFill = new SolidBrushDef(NodeFillColor);
        NodeFillSelected = new SolidBrushDef(NodeFillSelectedColor);
        NodeSignalValid = new SolidBrushDef(NodeSignalValidColor);
        NodeSignalInvalid = new SolidBrushDef(NodeSignalInvalidColor);
        ConnectorText = new SolidBrushDef(ConnectorTextColor);
        ConnectorFill = new SolidBrushDef(ConnectorFillColor);
        ConnectorFillSelected = new SolidBrushDef(ConnectorSelectedFillColor);
        SelectionFill = new SolidBrushDef(SelectionFillColor);
        LinkEditbleType = new SolidBrushDef(LinkEditableColor);
        LinkArrow = new SolidBrushDef(LinkColor);
        LinkArrowError = new SolidBrushDef(NodeSignalInvalidColor);

        NodeOutline = new PenDef(NodeOutlineColor, 2);
        NodeOutlineSelected = new PenDef(NodeOutlineSelectedColor, 2);
        ConnectorOutline = new PenDef(ConnectorOutlineColor);
        ConnectorOutlineSelected = new PenDef(ConnectorOutlineSelectedColor);
        SelectionOutline = new PenDef(SelectionOutlineColor);
        Link = new PenDef(LinkColor, 3f);
        LinkEditable = new PenDef(LinkEditableColor, 3f);
        LinkError = new PenDef(NodeSignalInvalidColor, 3);
        ConnectorCombinedPen = new PenDef(Color.White, 2);
        LinkCombinedPen = new PenDef(Color.White, 5);

        LinkSelectedPen = new PenDef(NodeOutlineSelectedColor, 5);
        LinkSelectedCombinedPen = new PenDef(NodeOutlineSelectedColor, 7);
    }

    public void UpdateNodeTextColor(Color color)
    {
        NodeTextColor = color;
        NodeText = new SolidBrushDef(color);
    }

    public void UpdateNodeTextShadowColor(Color color)
    {
        NodeTextShadowColor = color;
        NodeTextShadow = new SolidBrushDef(color);
    }

    public void UpdateNodeFillColor(Color color)
    {
        NodeFillColor = color;
        NodeFill = new SolidBrushDef(color);
    }

    public void UpdateNodeFillSelectedColor(Color color)
    {
        NodeFillSelectedColor = color;
        NodeFillSelected = new SolidBrushDef(color);
    }

    public void UpdateNodeOutlineColor(Color color)
    {
        NodeOutlineColor = color;
        NodeOutline = new PenDef(color, 2);
    }

    public void UpdateNodeOutlineSelectedColor(Color color)
    {
        NodeOutlineSelectedColor = color;
        NodeOutlineSelected = new PenDef(color, 2);
    }

    public void UpdateNodeSignalValidColor(Color color)
    {
        NodeSignalValidColor = color;
        NodeSignalValid = new SolidBrushDef(color);
    }

    public void UpdateNodeSignalInvalidColor(Color color)
    {
        NodeSignalInvalidColor = color;
        NodeSignalInvalid = new SolidBrushDef(color);
        LinkError = new PenDef(color, 3);
        LinkArrowError = new SolidBrushDef(color);
    }

    public void UpdateConnectorTextColor(Color color)
    {
        ConnectorTextColor = color;
        ConnectorText = new SolidBrushDef(color);
    }

    public void UpdateConnectorFillColor(Color color)
    {
        ConnectorFillColor = color;
        ConnectorFill = new SolidBrushDef(color);
    }

    public void UpdateConnectorOutlineColor(Color color)
    {
        ConnectorOutlineColor = color;
        ConnectorOutline = new PenDef(color);
    }

    public void UpdateConnectorFillSelectedColor(Color color)
    {
        ConnectorSelectedFillColor = color;
        ConnectorFillSelected = new SolidBrushDef(color);
    }

    public void UpdateConnectorOutlineSelectedColor(Color color)
    {
        ConnectorOutlineSelectedColor = color;
        ConnectorOutlineSelected = new PenDef(color);
    }

    public void UpdateSelectionFillColor(Color color)
    {
        SelectionFillColor = color;
        SelectionFill = new SolidBrushDef(color);
    }

    public void UpdateSelectionOutlineColor(Color color)
    {
        SelectionOutlineColor = color;
        SelectionOutline = new PenDef(color);
    }

    public void UpdateNodeHeaderColor(Color color)
    {
        NodeHeaderColor = color;
        NodeHeaderFill = new SolidBrushDef(color);
    }

    public void UpdateLinkColor(Color color)
    {
        LinkColor = color;
        Link = new PenDef(color, 3f);
        LinkArrow = new SolidBrushDef(color);
    }

    public void UpdateLinkEditableColor(Color color)
    {
        LinkEditableColor = color;
        LinkEditable = new PenDef(color, 3f);
        LinkEditbleType = new SolidBrushDef(color);
    }
}
