using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using Suity.Views.Graphics;
using Suity.Views.Menu;
using Suity.Helpers;
using System.Diagnostics;
using System.Drawing;
using Suity.Views;
using Suity.Controls;

namespace Suity.Contexts;

/// <summary>
/// Specifies operations to perform when a frame has no dirty regions to repaint.
/// </summary>
public enum EmptyFrameOperations
{
    /// <summary>
    /// Skip rendering for this frame.
    /// </summary>
    Bypass,
    /// <summary>
    /// Force a full repaint of the entire surface.
    /// </summary>
    RepaintAll,
}

/// <summary>
/// Abstract base class for Avalonia graphics contexts implementing various graphic interfaces.
/// </summary>
internal abstract class AvaGraphicBaseContext :
    IGraphicContext,
    IGraphicTextBoxEdit,
    IGraphicDropDownEdit,
    IGraphicToolTip,
    IGraphicContextMenu,
    IGraphicDragDrop,
    IGraphicColorPicker
{
    private readonly AvaGraphicInput _input;
    private readonly AvaGraphicOutput _output;
    private readonly AvaSKGraphicControl _control;
    private AvaContextMenuBinder _contextMenuBinder;
    private IGraphicObject? _graphicObject;

    private bool _repaintAll = true;
    private RectangleF[]? _dirtyRects;
    private readonly AvaTextBoxOverlayEdit _textBoxEdit;
    private readonly AvaDropDownContextMenuEdit _dropDownEdit;
    private readonly AvaColorPickerEdit _colorPickerEdit;
    private int _toolTipShowing;

    private bool _refreshAction = false;
    private bool _supportDirtyRect;
    private bool _mouseIn;

    private PointerPressedEventArgs _lastPointerPressed;
    private EmptyFrameOperations _emptyFrameOperations = EmptyFrameOperations.Bypass;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicBaseContext"/> class.
    /// </summary>
    /// <param name="control">The parent Avalonia control.</param>
    protected AvaGraphicBaseContext(AvaSKGraphicControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        _input = new AvaGraphicInput(control);
        _output = new AvaGraphicOutput(control);

        _textBoxEdit = new AvaTextBoxOverlayEdit(control);
        _dropDownEdit = new AvaDropDownContextMenuEdit(control);
        _colorPickerEdit = new AvaColorPickerEdit(control);

        DragDrop.SetAllowDrop(control, true);
        DragDrop.AddDragEnterHandler(control, OnDragEnter);
        DragDrop.AddDragLeaveHandler(control, OnDragLeave);
        DragDrop.AddDragOverHandler(control, OnDragOver);
        DragDrop.AddDropHandler(control, OnDragDrop);
    }

    /// <summary>
    /// Gets the Avalonia-specific graphic input handler.
    /// </summary>
    public AvaGraphicInput AvaInput => _input;

    /// <summary>
    /// Gets the Avalonia-specific graphic output handler.
    /// </summary>
    public AvaGraphicOutput AvaOutput => _output;

    /// <summary>
    /// Gets the parent Avalonia control.
    /// </summary>
    protected AvaSKGraphicControl Control => _control;

    /// <summary>
    /// Gets or sets a value indicating whether to repaint all.
    /// </summary>
    protected bool RepaintAllFlag
    {
        get => _repaintAll;
        set => _repaintAll = value;
    }

    /// <summary>
    /// Gets or sets the dirty rectangles.
    /// </summary>
    protected RectangleF[]? DirtyRects
    {
        get => _dirtyRects;
        set => _dirtyRects = value;
    }

    /// <summary>
    /// Gets the text box edit overlay.
    /// </summary>
    protected AvaTextBoxOverlayEdit TextBoxEdit => _textBoxEdit;

    /// <summary>
    /// Gets the drop down context menu edit.
    /// </summary>
    protected AvaDropDownContextMenuEdit DropDownEdit => _dropDownEdit;

    /// <summary>
    /// Event raised when a cleanup of the view context is requested.
    /// </summary>
    public event EventHandler? CleanUpViewContextRequest;

    /// <summary>
    /// Gets or sets the graphic object associated with this context.
    /// </summary>
    public IGraphicObject? GraphicObject
    {
        get => _graphicObject;
        set
        {
            if (_graphicObject == value)
            {
                return;
            }

            if (_graphicObject != null)
            {
                _graphicObject.GraphicContext = null;
            }

            _graphicObject = value;

            if (_graphicObject != null)
            {
                _graphicObject.GraphicContext = this;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether dirty rectangle optimization is supported.
    /// </summary>
    public bool SupportDirtyRect
    {
        get => _supportDirtyRect;
        set => _supportDirtyRect = value;
    }

    /// <summary>
    /// Gets or sets the operation to perform when a frame has no dirty regions.
    /// </summary>
    public EmptyFrameOperations EmptyFrameOperation
    {
        get => _emptyFrameOperations;
        set => _emptyFrameOperations = value;
    }

    #region IGraphicContext

    /// <inheritdoc/>
    public IGraphicInput Input => _input;

    /// <inheritdoc/>
    public IGraphicOutput Output => _output;

    /// <inheritdoc/>
    public Size GraphicSize
    {
        get => new((int)_control.Width, (int)_control.Height);
        set
        {
            _control.Width = value.Width;
            _control.Height = value.Height;
        }
    }

    /// <inheritdoc/>
    public bool RepaintAll => _repaintAll;

    /// <inheritdoc/>
    public void RequestFocus()
    {
        _control.Focus();
    }

    /// <inheritdoc/>
    public void RequestOutput()
    {
        _dirtyRects = null;
        _repaintAll = true;
        _control.Invalidate();
    }

    /// <inheritdoc/>
    public abstract void RequestOutput(IEnumerable<RectangleF> clipRects);

    /// <inheritdoc/>
    public void RequestRefreshInput(bool atOnce)
    {
        var stackTrace = new StackTrace();

        void action()
        {
            _refreshAction = false;
            var stack = stackTrace.GetFrames();

            if (_graphicObject != null)
            {
                if (!ReferenceEquals(_graphicObject.GraphicContext, this))
                {
                    _graphicObject.GraphicContext = this;
                }
                _graphicObject.HandleGraphicInput(CommonGraphicInput.Refresh);
            }
        }

        if (atOnce)
        {
            action();
        }
        else
        {
            if (_refreshAction)
            {
                return;
            }

            _refreshAction = true;
            Dispatcher.UIThread.Post(action);
        }
    }

    /// <inheritdoc/>
    public void ResetContext()
    {
        CleanUpViewContextRequest?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void SetCursor(GuiCursorTypes cursor)
    {
        _control.Cursor = new Cursor(GetStandardCursor(cursor));
    }

    /// <inheritdoc/>
    public void UpdateNow()
    {
        _control.Invalidate();
    }

    #endregion

    #region IGraphicTextBoxEdit

    /// <inheritdoc/>
    public void BeginTextEdit(Rectangle rect, string text, TextBoxEditOptions option)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _dirtyRects = null;
            _repaintAll = true;
            _textBoxEdit.BeginTextEdit(rect, text, option);
        });
    }

    /// <inheritdoc/>
    public void EndTextEdit()
    {
        _dirtyRects = null;
        _repaintAll = true;
        _control.Invalidate();
        _textBoxEdit.EndTextEdit();
    }

    #endregion

    #region IGraphicDropDownEdit

    /// <inheritdoc/>
    public void ShowComboBoxDropDown(Rectangle rect, IEnumerable<object> items, object selectedItem, Action<object> callBack)
    {
        _dropDownEdit.ShowMenu(rect, items, selectedItem, callBack);
    }

    /// <inheritdoc/>
    public void HideDropDown()
    {
        //_dropDownEdit.EndEdit();
    }

    #endregion

    #region IGraphicToolTip

    /// <inheritdoc/>
    public void ShowToolTip(string toolTip, int x, int y)
    {
        ToolTip.SetTip(_control, toolTip);
        ToolTip.SetPlacement(_control, PlacementMode.Pointer);
        ToolTip.SetIsOpen(_control, true);
        _toolTipShowing = 10;
    }

    #endregion

    #region IGraphicContextMenu

    /// <inheritdoc/>
    public void RegisterContextMenu(object menuCommand)
    {
        if (menuCommand is not RootMenuCommand m)
        {
            return;
        }

        var binder = _contextMenuBinder ??= new();
        binder.EnsureContextMenu(m);
    }

    /// <inheritdoc/>
    public void ShowContextMenu(object menuCommand, IEnumerable<object> selectedItems = null)
    {
        if (menuCommand is not RootMenuCommand m)
        {
            return;
        }

        if (_input.MouseLocation is not Point pos)
        {
            return;
        }

        var binder = _contextMenuBinder ??= new();
        var menu = binder.PrepareContextMenu(m, selectedItems);
        if (menu is null)
        {
            return;
        }

        menu.PlacementTarget = _control;
        menu.Placement = PlacementMode.Pointer;
        menu.HorizontalOffset = 0;
        menu.VerticalOffset = 0;
        menu.Open(_control);
    }

    #endregion

    #region IGraphicDragDrop

    /// <inheritdoc/>
    public void DoDragDrop(object obj)
    {
        if (obj is null)
        {
            return;
        }

        var eventArgs = _lastPointerPressed;
        if (eventArgs is null)
        {
            return;
        }

        string dataId = AvaDragEvent.Global.SetInternalData(obj);
        var dataTransfer = new DataTransfer();
        dataTransfer.Add(DataTransferItem.CreateText(dataId));
        var effect = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var result = await DragDrop.DoDragDropAsync(eventArgs, dataTransfer, effect);
            }
            catch (Exception err)
            {
                err.LogError();
            }
            finally
            {
                AvaDragEvent.Global.UpdateEventArgs(null);
            }
        });
    }

    #endregion

    #region IGraphicColorPicker

    /// <inheritdoc/>
    public void ShowColorPicker(Rectangle rect, System.Drawing.Color color, Action<System.Drawing.Color, bool> selected)
    {
        _colorPickerEdit.Show(rect, color.ToAvaloniaColor(), (c, final) => selected.Invoke(c.ToSystemDrawingColor(), final));
    }

    #endregion

    /// <summary>
    /// Handles the control created event, initializing output size and input refresh.
    /// </summary>
    public void HandleCreated()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var bound = _control.Bounds;
            _output.SetSize((int)bound.Width, (int)bound.Height);
            _input.SetRefreshEvent();
            _graphicObject?.HandleGraphicInput(_input);
        });
    }

    /// <summary>
    /// Handles the control released event, releasing internal resources.
    /// </summary>
    public abstract void HandleReleased();

    /// <summary>
    /// Renders the graphic object to the specified canvas.
    /// </summary>
    /// <param name="context">The Avalonia immediate drawing context.</param>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="rect">The bounding rectangle for rendering.</param>
    public abstract void Paint(ImmediateDrawingContext? context, SKCanvas canvas, Avalonia.Rect rect);

    /// <summary>
    /// Clears all pending input events.
    /// </summary>
    public void ClearInput()
    {
        _input.Clear();
    }

    /// <summary>
    /// Clears the stored mouse location.
    /// </summary>
    public void ClearMouseLocation()
    {
        _input.ClearMouseLocation();
    }

    /// <summary>
    /// Processes a graphic input event and forwards it to the associated graphic object.
    /// </summary>
    /// <param name="input">The graphic input event to process.</param>
    public void HandleInput(IGraphicInput input)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                _mouseIn = true;
                break;

            case GuiEventTypes.MouseOut:
                _mouseIn = false;
                break;
        }

        _graphicObject?.HandleGraphicInput(input);
    }

    /// <summary>
    /// Handles a pointer event and updates the graphic input state.
    /// </summary>
    /// <param name="e">The pointer event arguments.</param>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="focus">Whether to request focus for the control.</param>
    /// <param name="endPopUp">Whether to close any active popups.</param>
    public void HandlePointerEvent(PointerEventArgs e, GuiEventTypes eventType, bool focus = false, bool endPopUp = false)
    {
        if (endPopUp)
        {
            _textBoxEdit.EndTextEdit();
            HideDropDown();
        }

        _input.SetPointerEvent(eventType, e);
        _graphicObject?.HandleGraphicInput(_input);

        if (focus)
        {
            if (!_control.IsFocused)
            {
                _control.Focus();
            }
        }

        if (eventType == GuiEventTypes.MouseMove && _toolTipShowing > 0)
        {
            _toolTipShowing--;
            if (_toolTipShowing == 0)
            {
                ToolTip.SetIsOpen(_control, false);
                ToolTip.SetTip(_control, null);
            }
        }

        if (e is PointerPressedEventArgs pressedArgs)
        {
            _lastPointerPressed = pressedArgs;
        }
    }

    /// <summary>
    /// Handles a tapped event and updates the graphic input state.
    /// </summary>
    /// <param name="e">The tapped event arguments.</param>
    /// <param name="eventType">The type of GUI event.</param>
    /// <param name="focus">Whether to request focus for the control.</param>
    /// <param name="endPopUp">Whether to close any active popups.</param>
    public void HandleTappedEvent(TappedEventArgs e, GuiEventTypes eventType, bool focus = false, bool endPopUp = false)
    {
        if (endPopUp)
        {
            _textBoxEdit.EndTextEdit();
            HideDropDown();
        }

        _input.SetTappedEvent(eventType, e);
        _graphicObject?.HandleGraphicInput(_input);

        if (focus)
        {
            if (!_control.IsFocused)
            {
                _control.Focus();
            }
        }

        if (eventType == GuiEventTypes.MouseMove && _toolTipShowing > 0)
        {
            _toolTipShowing--;
            if (_toolTipShowing == 0)
            {
                ToolTip.SetIsOpen(_control, false);
                ToolTip.SetTip(_control, null);
            }
        }
    }

    /// <summary>
    /// Handles a key event and forwards it to the graphic object if not editing text.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    /// <param name="eventType">The type of GUI event.</param>
    public void HandleKeyEvent(KeyEventArgs e, GuiEventTypes eventType)
    {
        if (_textBoxEdit.Editing)
        {
            return;
        }

        _input.SetKeyEvent(eventType, e);
        _graphicObject?.HandleGraphicInput(_input);
    }

    /// <summary>
    /// Handles the resize event, ending text editing and updating output size.
    /// </summary>
    public void HandleResizeEvent()
    {
        _textBoxEdit.EndTextEdit();
        _input.SetResizeEvent();
        var bounds = _control.Bounds;
        _output.SetSize((int)bounds.Width, (int)bounds.Height);
        _repaintAll = true;
        _graphicObject?.HandleGraphicInput(_input);
    }

    /// <summary>
    /// Handles the timer event, checking mouse state and updating input.
    /// </summary>
    public void HandleTimerEvent()
    {
        if (_mouseIn)
        {
            if (!_control.IsPointerOver)
            {
                _mouseIn = false;
                HandleInput(CommonGraphicInput.MouseOut);
            }
        }

        _input.SetTimerEvent();
        _graphicObject?.HandleGraphicInput(_input);
    }

    #region Drag Drop

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        _input.DragEvent = AvaDragEvent.Global;
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        _input.DragEvent = null;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        AvaDragEvent? dropEvent = null;
        try
        {
            AvaDragEvent.Global.UpdateEventArgs(e);
            dropEvent = _input.SetDragEvent(GuiEventTypes.DragOver, _control, e);
            _graphicObject?.HandleGraphicInput(_input);

            if (!_control.IsFocused)
            {
                _control.Focus();
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
        finally
        {
            //dropEvent?.UpdateEventArgs(null);
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        AvaDragEvent? dropEvent = null;
        try
        {
            AvaDragEvent.Global.UpdateEventArgs(e);
            dropEvent = _input.SetDragEvent(GuiEventTypes.DragDrop, _control, e);
            _graphicObject?.HandleGraphicInput(_input);

            if (!_control.IsFocused)
            {
                _control.Focus();
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
        finally
        {
            dropEvent?.UpdateEventArgs(null);
            _input.DragEvent = null;
        }
    }

    #endregion

    /// <summary>
    /// Converts a GUI cursor type to an Avalonia standard cursor type.
    /// </summary>
    /// <param name="cursor">The GUI cursor type.</param>
    /// <returns>The corresponding Avalonia standard cursor type.</returns>
    public static StandardCursorType GetStandardCursor(GuiCursorTypes cursor)
    {
        switch (cursor)
        {
            case GuiCursorTypes.Default: return StandardCursorType.Arrow;
            case GuiCursorTypes.Hand: return StandardCursorType.Hand;
            case GuiCursorTypes.IBeam: return StandardCursorType.Ibeam;
            case GuiCursorTypes.HSplit: return StandardCursorType.SizeNorthSouth;
            case GuiCursorTypes.VSplit: return StandardCursorType.SizeWestEast;
            case GuiCursorTypes.NoMoveVert: return StandardCursorType.No;
            case GuiCursorTypes.NoMoveHoriz: return StandardCursorType.No;
            case GuiCursorTypes.SizeAll: return StandardCursorType.SizeAll;
            case GuiCursorTypes.SizeNS: return StandardCursorType.SizeNorthSouth;
            case GuiCursorTypes.SizeWE: return StandardCursorType.SizeWestEast;
            case GuiCursorTypes.SizeNWSE: return StandardCursorType.BottomRightCorner;
            case GuiCursorTypes.SizeNESW: return StandardCursorType.BottomLeftCorner;
            default: return StandardCursorType.Arrow;
        }
    }

    /// <summary>
    /// Calculates the bounding box that encompasses all specified rectangles.
    /// </summary>
    /// <param name="clipRects">The collection of rectangles.</param>
    /// <returns>The bounding rectangle, or Empty if the collection is null or empty.</returns>
    public static RectangleF GetBoundingBox(IEnumerable<RectangleF> clipRects)
    {
        if (clipRects == null || !clipRects.Any())
        {
            return RectangleF.Empty;
        }

        RectangleF bounds = clipRects.First();
        foreach (var rect in clipRects.Skip(1))
        {
            bounds = RectangleF.Union(bounds, rect);
        }

        return bounds;
    }
}
