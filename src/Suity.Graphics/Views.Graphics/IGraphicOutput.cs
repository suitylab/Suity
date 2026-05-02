using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Graphics;

/// <summary>
/// Interface for graphic output operations including drawing shapes, text, and images.
/// </summary>
public interface IGraphicOutput
{
    /// <summary>
    /// Gets a value indicating whether the output is empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets the width of the output surface.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the output surface.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets a value indicating whether a full repaint is required.
    /// </summary>
    bool RepaintAll { get; }

    /// <summary>
    /// Gets a value indicating whether clipping is active.
    /// </summary>
    bool IsClipped { get; }

    /// <summary>
    /// Clears the output surface with the specified color.
    /// </summary>
    /// <param name="color">The color to fill the surface.</param>
    void Clear(Color color);

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    /// <param name="pen">The pen used to draw the line.</param>
    /// <param name="pt1">The starting point.</param>
    /// <param name="pt2">The ending point.</param>
    void DrawLine(Pen pen, PointF pt1, PointF pt2);

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="pen">The pen used to draw the rectangle.</param>
    /// <param name="rect">The rectangle to draw.</param>
    void DrawRectangle(Pen pen, RectangleF rect);

    /// <summary>
    /// Draws an ellipse.
    /// </summary>
    /// <param name="pen">The pen used to draw the ellipse.</param>
    /// <param name="rect">The bounding rectangle for the ellipse.</param>
    void DrawEllipse(Pen pen, RectangleF rect);

    /// <summary>
    /// Draws an arc.
    /// </summary>
    /// <param name="pen">The pen used to draw the arc.</param>
    /// <param name="rect">The bounding rectangle for the arc.</param>
    /// <param name="startAngle">The starting angle in degrees.</param>
    /// <param name="sweepAngle">The sweep angle in degrees.</param>
    /// <param name="useCenter">Whether to connect to the center.</param>
    void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle, bool useCenter);

    /// <summary>
    /// Draws a polygon outline.
    /// </summary>
    /// <param name="pen">The pen to use.</param>
    /// <param name="points">The points defining the polygon.</param>
    void DrawPolygon(Pen pen, PointF[] points);

    /// <summary>
    /// Draws a Bezier curve.
    /// </summary>
    /// <param name="pen">The pen used to draw the curve.</param>
    /// <param name="pt1">The first control point.</param>
    /// <param name="pt2">The second control point.</param>
    /// <param name="pt3">The third control point.</param>
    /// <param name="pt4">The fourth control point.</param>
    void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4);

    /// <summary>
    /// Draws a Bezier curve with gradient colors.
    /// </summary>
    /// <param name="color1">The starting color.</param>
    /// <param name="color2">The ending color.</param>
    /// <param name="width">The width of the curve.</param>
    /// <param name="pt1">The first control point.</param>
    /// <param name="pt2">The second control point.</param>
    /// <param name="pt3">The third control point.</param>
    /// <param name="pt4">The fourth control point.</param>
    /// <param name="dashStyle">The dash style (default: Solid).</param>
    /// <param name="dashPattern">The custom dash pattern.</param>
    void DrawBezier(Color color1, Color color2, float width, PointF pt1, PointF pt2, PointF pt3, PointF pt4, DashStyle dashStyle = DashStyle.Solid, float[] dashPattern = null);

    /// <summary>
    /// Draws a string at the specified point.
    /// </summary>
    /// <param name="s">The string to draw.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="brush">The brush to use.</param>
    /// <param name="point">The position to draw at.</param>
    void DrawString(string s, Font font, Brush brush, PointF point);

    /// <summary>
    /// Draws a string at the specified coordinates.
    /// </summary>
    /// <param name="s">The string to draw.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="brush">The brush to use.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    void DrawString(string s, Font font, Brush brush, float x, float y);

    /// <summary>
    /// Draws a string with a specific format.
    /// </summary>
    /// <param name="s">The string to draw.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="brush">The brush to use.</param>
    /// <param name="point">The position to draw at.</param>
    /// <param name="format">The string format.</param>
    void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format);

    /// <summary>
    /// Draws text within a rectangular area.
    /// </summary>
    /// <param name="s">The string to draw.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="rect">The bounding rectangle.</param>
    void DrawTextArea(string s, Font font, Color color, RectangleF rect);

    /// <summary>
    /// Draws an image with caching support.
    /// </summary>
    /// <param name="bitmap">The image to draw.</param>
    /// <param name="rect">The destination rectangle.</param>
    /// <param name="color">Optional tint color.</param>
    void DrawImageCached(Image bitmap, RectangleF rect, Color? color);

    /// <summary>
    /// Draws an image.
    /// </summary>
    /// <param name="bitmap">The image to draw.</param>
    /// <param name="rect">The destination rectangle.</param>
    /// <param name="color">Optional tint color.</param>
    void DrawImage(Image bitmap, RectangleF rect, Color? color);

    /// <summary>
    /// Fills a rounded rectangle.
    /// </summary>
    /// <param name="brush">The brush to use.</param>
    /// <param name="rect">The rectangle to fill.</param>
    /// <param name="cornerRadius">The corner radius.</param>
    void FillRoundRectangle(Brush brush, RectangleF rect, float cornerRadius);

    /// <summary>
    /// Draws a rounded rectangle outline.
    /// </summary>
    /// <param name="pen">The pen to use.</param>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="cornerRadius">The corner radius.</param>
    void DrawRoundRectangle(Pen pen, RectangleF rect, float cornerRadius);

    /// <summary>
    /// Fills a rectangle.
    /// </summary>
    /// <param name="brush">The brush to use.</param>
    /// <param name="rect">The rectangle to fill.</param>
    void FillRectangle(Brush brush, RectangleF rect);

    /// <summary>
    /// Fills an ellipse.
    /// </summary>
    /// <param name="brush">The brush to use.</param>
    /// <param name="rect">The bounding rectangle for the ellipse.</param>
    void FillEllipse(Brush brush, RectangleF rect);

    /// <summary>
    /// Fills a polygon.
    /// </summary>
    /// <param name="brush">The brush to use.</param>
    /// <param name="points">The points defining the polygon.</param>
    void FillPolygon(Brush brush, PointF[] points);

    /// <summary>
    /// Sets a clipping rectangle.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    void SetClipRect(RectangleF rect);

    /// <summary>
    /// Sets multiple clipping rectangles.
    /// </summary>
    /// <param name="rects">The collection of clipping rectangles.</param>
    void SetClipRects(IEnumerable<RectangleF> rects);

    /// <summary>
    /// Restores the previous clipping region.
    /// </summary>
    void RestoreClip();

    /// <summary>
    /// Restores the clipping region to a specific count.
    /// </summary>
    /// <param name="count">The number of clipping regions to restore to.</param>
    void RestoreClipTo(int count);

    /// <summary>
    /// Measures the size of a string.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="font">The font to use.</param>
    /// <returns>The measured size.</returns>
    SizeF MeasureString(string text, Font font);

    /// <summary>
    /// Measures the size of text within a constrained width.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="maxLineWidth">The maximum line width.</param>
    /// <returns>The measured size.</returns>
    SizeF MeasureTextArea(string text, Font font, float maxLineWidth);

    /// <summary>
    /// Measures the size of an image.
    /// </summary>
    /// <param name="image">The image to measure.</param>
    /// <returns>The measured size.</returns>
    SizeF MeasureImage(Image image);

    /// <summary>
    /// Creates a snapshot of the entire output surface.
    /// </summary>
    /// <returns>The snapshot object.</returns>
    ISnapshot Snapshot();

    /// <summary>
    /// Creates a snapshot of a specific region.
    /// </summary>
    /// <param name="rect">The region to capture.</param>
    /// <returns>The snapshot object.</returns>
    ISnapshot Snapshot(RectangleF rect);

    /// <summary>
    /// Draws a previously captured snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to draw.</param>
    /// <param name="rect">The destination rectangle.</param>
    void DrawSnapshot(ISnapshot snapshot, RectangleF rect);
}

/// <summary>
/// Interface representing a snapshot of the graphic output surface.
/// </summary>
public interface ISnapshot : IDisposable
{
}

/// <summary>
/// Empty implementation of IGraphicOutput that performs no operations.
/// </summary>
public class EmptyGraphicOutput : IGraphicOutput
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static readonly EmptyGraphicOutput Empty = new EmptyGraphicOutput();

    private EmptyGraphicOutput()
    { }

    /// <inheritdoc/>
    public bool IsEmpty => true;

    /// <inheritdoc/>
    public int Width => 0;

    /// <inheritdoc/>
    public int Height => 0;

    /// <inheritdoc/>
    public bool RepaintAll => true;

    /// <inheritdoc/>
    public bool IsClipped => false;

    /// <inheritdoc/>
    public void Clear(Color color)
    { }

    /// <inheritdoc/>
    public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
    { }

    /// <inheritdoc/>
    public void DrawBezier(Color color1, Color color2, float width, PointF pt1, PointF pt2, PointF pt3, PointF pt4, DashStyle dashStyle = DashStyle.Solid, float[] dashPattern = null)
    { }

    /// <inheritdoc/>
    public void DrawImage(Image bitmap, RectangleF rect, Color? color)
    { }

    /// <inheritdoc/>
    public void DrawImageCached(Image bitmap, RectangleF rect, Color? color)
    { }

    /// <inheritdoc/>
    public void DrawLine(Pen pen, PointF pt1, PointF pt2)
    { }

    /// <inheritdoc/>
    public void DrawRectangle(Pen pen, RectangleF rect)
    { }

    /// <inheritdoc/>
    public void DrawEllipse(Pen pen, RectangleF rect)
    { }

    /// <inheritdoc/>
    public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle, bool useCenter)
    { }

    /// <inheritdoc/>
    public void DrawRoundRectangle(Pen pen, RectangleF rect, float cornerRadius)
    { }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, PointF point)
    { }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, float x, float y)
    { }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
    { }

    /// <inheritdoc/>
    public void DrawTextArea(string s, Font font, Color color, RectangleF rect)
    { }

    /// <inheritdoc/>
    public void FillEllipse(Brush brush, RectangleF rect)
    { }

    /// <inheritdoc/>
    public void DrawPolygon(Pen pen, PointF[] points)
    { }

    /// <inheritdoc/>
    public void FillPolygon(Brush brush, PointF[] points)
    { }

    /// <inheritdoc/>
    public void FillRectangle(Brush brush, RectangleF rect)
    { }

    /// <inheritdoc/>
    public void FillRoundRectangle(Brush brush, RectangleF rect, float cornerRadius)
    { }

    /// <inheritdoc/>
    public SizeF MeasureString(string text, Font font) => SizeF.Empty;

    /// <inheritdoc/>
    public SizeF MeasureTextArea(string text, Font font, float maxLineWidth) => SizeF.Empty;

    /// <inheritdoc/>
    public SizeF MeasureImage(Image image) => SizeF.Empty;

    /// <inheritdoc/>
    public void SetClipRect(RectangleF rect)
    { }

    /// <inheritdoc/>
    public void SetClipRects(IEnumerable<RectangleF> rects)
    { }

    /// <inheritdoc/>
    public void RestoreClip()
    { }

    /// <inheritdoc/>
    public void RestoreClipTo(int count)
    { }

    /// <inheritdoc/>
    public ISnapshot Snapshot() => null;

    /// <inheritdoc/>
    public ISnapshot Snapshot(RectangleF rect) => null;

    /// <inheritdoc/>
    public void DrawSnapshot(ISnapshot snapshot, RectangleF rect) { }
}