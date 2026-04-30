using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Graphics;

/// <summary>
/// Graphic device context
/// </summary>
public interface IGraphicContext
{
    /// <summary>
    /// Graphic input
    /// </summary>
    IGraphicInput Input { get; }

    /// <summary>
    /// Graphic output
    /// </summary>
    IGraphicOutput Output { get; }

    /// <summary>
    /// Drawing area size
    /// </summary>
    Size GraphicSize { get; set; }

    bool RepaintAll { get; }

    /// <summary>
    /// Reset drawing device
    /// </summary>
    void ResetContext();

    /// <summary>
    /// Request the device to perform an input refresh once
    /// </summary>
    /// <param name="atOnce">Execute immediately, if false, delay to the next frame for execution</param>
    void RequestRefreshInput(bool atOnce);

    /// <summary>
    /// Request the device to perform an output drawing once
    /// </summary>
    void RequestOutput();

    /// <summary>
    /// Requests that output be generated for the specified clipping rectangles.
    /// </summary>
    /// <param name="clipRects">A collection of <see cref="RectangleF"/> structures that define the regions to be included in the output. Cannot
    /// be null or contain null elements.</param>
    void RequestOutput(IEnumerable<RectangleF> clipRects);

    void UpdateNow();

    /// <summary>
    /// Request device to obtain focus
    /// </summary>
    void RequestFocus();

    /// <summary>
    /// Sets the mouse cursor to the specified cursor type for the GUI.
    /// </summary>
    /// <param name="cursor">The cursor type to display, specified as a value of the <see cref="GuiCursorTypes"/> enumeration.</param>
    void SetCursor(GuiCursorTypes cursor);
}

/// <summary>
/// Empty implementation of IGraphicContext that performs no operations.
/// </summary>
public class EmptyGraphicContext : IGraphicContext
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static EmptyGraphicContext Empty { get; } = new();

    private EmptyGraphicContext()
    { }

    /// <inheritdoc/>
    public IGraphicInput Input => CommonGraphicInput.Empty;

    /// <inheritdoc/>
    public IGraphicOutput Output => EmptyGraphicOutput.Empty;

    /// <inheritdoc/>
    public Size GraphicSize
    {
        get => Size.Empty;
        set { }
    }

    /// <inheritdoc/>
    public bool RepaintAll => false;

    /// <inheritdoc/>
    public void ResetContext()
    { }

    /// <inheritdoc/>
    public void RequestRefreshInput(bool atOnce)
    { }

    /// <inheritdoc/>
    public void RequestOutput()
    { }

    /// <inheritdoc/>
    public void RequestOutput(IEnumerable<RectangleF> clipRects)
    { }

    /// <inheritdoc/>
    public void UpdateNow()
    { }

    /// <inheritdoc/>
    public void RequestFocus()
    { }

    /// <inheritdoc/>
    public void ShowContextMenu(object rootMenu, IEnumerable<object> selectedItems = null)
    { }

    /// <inheritdoc/>
    public void ShowComboBoxDropDown(IEnumerable<object> items, Rectangle rect)
    { }

    /// <inheritdoc/>
    public void SetCursor(GuiCursorTypes cursor)
    { }
}