using Suity.Drawing;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// Draw node context
/// </summary>
public interface IDrawNodeContext : IDrawContext, ITextDisplay
{
    /// <summary>
    /// Gets the ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    int X { get; }

    /// <summary>
    /// Gets the Y position.
    /// </summary>
    int Y { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    int Height { get; }


    /// <summary>
    /// Gets the background color.
    /// </summary>
    Color? BackgroundColor { get; }

    /// <summary>
    /// Gets the title color.
    /// </summary>
    Color? TitleColor { get; }

    /// <summary>
    /// Gets the preview text.
    /// </summary>
    string PreviewText { get; }

    /// <summary>
    /// Gets whether it has header.
    /// </summary>
    bool HasHeader { get; }

    #region Legacy

    /// <summary>
    /// Gets the node fill brush.
    /// </summary>
    BrushDef NodeFillBrush { get; }

    /// <summary>
    /// Gets the node header fill brush.
    /// </summary>
    BrushDef NodeHeaderFillBrush { get; }

    /// <summary>
    /// Gets the node outline pen.
    /// </summary>
    PenDef NodeOutlinePen { get; }

    /// <summary>
    /// Gets the node outline when selected.
    /// </summary>
    PenDef NodeOutlineSelected { get; }

    /// <summary>
    /// Gets the scaled preview font.
    /// </summary>
    FontDef NodeScaledPreviewFont { get; }

    /// <summary>
    /// Gets the scaled title font.
    /// </summary>
    FontDef NodeScaledTitleFont { get; }

    /// <summary>
    /// Gets the node text brush.
    /// </summary>
    BrushDef NodeText { get; }

    /// <summary>
    /// Gets the node text shadow brush.
    /// </summary>
    BrushDef NodeTextShadow { get; }

    /// <summary>
    /// Draws shadow.
    /// </summary>
    /// <param name="output">Graphic output.</param>
    void DrawShadow(IGraphicOutput output);

    /// <summary>
    /// Draws panel.
    /// </summary>
    /// <param name="output">Graphic output.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="rect">Rectangle.</param>
    void DrawPanel(IGraphicOutput output, float zoom, Rectangle rect);

    /// <summary>
    /// Draws preview text.
    /// </summary>
    /// <param name="output">Graphic output.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="rect">Rectangle.</param>
    /// <param name="text">Text to draw.</param>
    void DrawPreviewText(IGraphicOutput output, float zoom, Rectangle rect, string text);

    /// <summary>
    /// Draws header.
    /// </summary>
    /// <param name="output">Graphic output.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="rect">Rectangle.</param>
    void DrawHeader(IGraphicOutput output, float zoom, Rectangle rect);

    /// <summary>
    /// Draws connectors.
    /// </summary>
    /// <param name="output">Graphic output.</param>
    void DrawConnectors(IGraphicOutput output);
    #endregion
}

/// <summary>
/// Flow node draw delegate
/// </summary>
public delegate void FlowNodeDrawDelegate(IGraphicOutput output, IDrawNodeContext context, float zoom, Point pos, Rectangle rect, bool drawText);

/// <summary>
/// Draw flow node ImGui delegate
/// </summary>
public delegate ImGuiNode DrawFlowNodeImGui(ImGui gui, IDrawNodeContext context);