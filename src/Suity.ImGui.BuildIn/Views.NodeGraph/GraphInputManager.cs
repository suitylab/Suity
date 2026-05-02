using Suity.Views.Graphics;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Manages user input for the graph control, including mouse and keyboard interactions for selecting, moving, linking, and resizing nodes.
/// </summary>
public class GraphInputManager
{
    private readonly GraphControl _control;
    private GraphDiagram Diagram => _control.Diagram;
    private GraphViewport View => _control.Viewport;

    private int _scrollLastX;
    private int _scrollLastY;
    private Point _mouseDownCtrlPosition;
    private Point _mouseDownPosition;
    private bool _scrolling;
    private Point _moveLastPosition;
    private bool _movingSelection;
    private bool _altPressed;
    private bool _ctrlPressed;
    private bool _shiftPressed;
    private bool _viewZooming;

    /// <summary>
    /// Gets the parent graph control.
    /// </summary>
    public GraphControl ParentControl => _control;
    /// <summary>
    /// Gets the current edit mode of the graph.
    /// </summary>
    public GraphEditMode EditMode { get; private set; } = GraphEditMode.Idle;
    /// <summary>
    /// Gets the current resize side being interacted with.
    /// </summary>
    public GraphResizeSide ResizeSide { get; private set; }
    /// <summary>
    /// Gets the last resize side that was hovered.
    /// </summary>
    public GraphResizeSide LastResizeSide { get; private set; }
    /// <summary>
    /// Gets the origin point of the selection box in view space.
    /// </summary>
    public Point SelectBoxOrigin { get; private set; }
    /// <summary>
    /// Gets the current point of the selection box in view space.
    /// </summary>
    public Point SelectBoxCurrent { get; private set; }
    /// <summary>
    /// Gets the cursor location in control (screen) space.
    /// </summary>
    public Point ScreenSpaceCursorLocation { get; private set; }
    /// <summary>
    /// Gets the cursor location in view space.
    /// </summary>
    public Point ViewSpaceCursorLocation { get; private set; }
    /// <summary>
    /// Gets the connector from which a link is being dragged.
    /// </summary>
    public GraphConnector? FromConnector { get; private set; }
    /// <summary>
    /// Gets the connector to which a link is being dragged.
    /// </summary>
    public GraphConnector? ToConnector { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether a zoom operation is in progress.
    /// </summary>
    public bool ViewZooming
    {
        get => _viewZooming;
        set => _viewZooming = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether node position snapping is enabled.
    /// </summary>
    public bool Snapping { get; set; } = true;

    /// <summary>
    /// Occurs when a node creation is requested.
    /// </summary>
    public event EventHandler? NodeCreateRequesting;
    /// <summary>
    /// Occurs when a group creation is requested.
    /// </summary>
    public event EventHandler? GroupCreateRequesting;
    /// <summary>
    /// Occurs before the selection changes.
    /// </summary>
    public event GraphSelectionEventHandler? SelectionChanging;
    /// <summary>
    /// Occurs after the selection has changed.
    /// </summary>
    public event GraphSelectionEventHandler? SelectionChanged;
    /// <summary>
    /// Occurs before the selection is deleted.
    /// </summary>
    public event GraphSelectionEventHandler? SelectionDeleting;
    /// <summary>
    /// Occurs after the selection has been deleted.
    /// </summary>
    public event GraphSelectionEventHandler? SelectionDeleted;
    /// <summary>
    /// Occurs when the selected nodes are moved.
    /// </summary>
    public event GraphNodeMoveEventHandler? SelectionMoved;
    /// <summary>
    /// Occurs when the selected nodes are resized.
    /// </summary>
    public event GraphNodeResizeEventHandler? SelectionResized;
    /// <summary>
    /// Occurs when a new link is created.
    /// </summary>
    public event GraphLinkEventHandler? LinkCreated;
    /// <summary>
    /// Occurs when a link is destroyed.
    /// </summary>
    public event GraphLinkEventHandler? LinkDestroyed;
    /// <summary>
    /// Occurs when the context menu is about to be shown.
    /// </summary>
    public event EventHandler<GraphicContextEventArgs>? ContextMenuShowing;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphInputManager"/> class.
    /// </summary>
    /// <param name="control">The parent graph control.</param>
    public GraphInputManager(GraphControl control)
    {
        _control = control;
    }

    /// <summary>
    /// Processes input events and routes them to the appropriate handler.
    /// </summary>
    /// <param name="input">The graphic input to process.</param>
    public void HandleInput(IGraphicInput input)
    {
        if (_control.PreGraphicInputInternal(input))
        {
            return;
        }

        PostGraphicInput(input);
    }

    /// <summary>
    /// Resets the edit mode to idle.
    /// </summary>
    public void SetIdle()
    {
        EditMode = GraphEditMode.Idle;
    }

    /// <summary>
    /// Handles input events before the main graphic output. Can be overridden by derived classes.
    /// </summary>
    /// <param name="input">The graphic input to process.</param>
    /// <returns>True if the input was handled; otherwise, false.</returns>
    protected internal virtual bool PreGraphicInput(IGraphicInput input)
    {
        return false;
    }

    private void PostGraphicInput(IGraphicInput input)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.MouseOut:
                ScreenSpaceCursorLocation = new Point(int.MinValue, int.MinValue);
                break;

            case GuiEventTypes.MouseDown:
                HandleMouseDown(input);
                break;

            case GuiEventTypes.MouseUp:
                HandleMouseUp(input);
                break;

            case GuiEventTypes.MouseMove:
                HandleMouseMove(input);
                break;

            case GuiEventTypes.MouseDoubleClick:
                if (Diagram.SelectedNodes.Count == 1)
                {
                    Diagram.SelectedNodes[0].HandleDoubleClick();
                }
                break;

            case GuiEventTypes.KeyDown:
                HandleKeyDown(input);
                break;

            case GuiEventTypes.KeyUp:
                HandleKeyUp(input);
                break;

            case GuiEventTypes.MouseWheel:
                HandleMouseWheel(input);
                break;
        }
    }

    private void HandleMouseDown(IGraphicInput input)
    {
        if (input.MouseLocation is not Point pos)
        {
            return;
        }

        var viewport = _control.Viewport;

        ScreenSpaceCursorLocation = _mouseDownCtrlPosition = pos;
        ViewSpaceCursorLocation = _mouseDownPosition = viewport.ControlToView(pos);

        switch (EditMode)
        {
            case GraphEditMode.Idle:
                switch (input.MouseButton)
                {
                    case GuiMouseButtons.Left:
                        if (_control.Viewport.HitAll(pos) == HitType.Connector)
                        {
                            if (!_altPressed)
                            {
                                EditMode = GraphEditMode.Linking;
                                FromConnector = _control.Viewport.GetHitConnector(pos);
                                ToConnector = null;
                            }
                            else
                            {
                                if (_control.Viewport.GetHitConnector(pos) is { } c)
                                {
                                    DeleteLinkConnectors(c);
                                }
                            }
                        }
                        else if (input.GetOnlyAltKey() && _control.Viewport.HitSelected(pos, out var node).GetIsNodeOrMoveArea())
                        {
                            node?.Highlighted = false;
                            EditMode = GraphEditMode.Selecting;
                        }
                        else if (!_shiftPressed && _control.Viewport.HitSide(pos) is GraphResizeSide side && side.NeedResize())
                        {
                            EditMode = GraphEditMode.Resizing;
                            ResizeSide = side;
                            _moveLastPosition = viewport.ControlToView(pos);

                            Diagram.SelectedNodes[0].MarkMovingPosition(Snapping);
                            Diagram.SelectedNodes[0].MarkResizingSize(Snapping);
                        }
                        else if (!_shiftPressed && Diagram.SelectedNodes.Count > 0 && _control.Viewport.HitSelected(pos, out node) == HitType.NodeMoveArea)
                        {
                            EditMode = GraphEditMode.MovingSelection;
                            _moveLastPosition = viewport.ControlToView(pos);

                            _control.SelectionManager.CollectDrivenItems();

                            foreach (GraphNode n in Diagram.SelectedNodes.Concat(Diagram.DrivenNodes))
                            {
                                n.MarkMovingPosition(Snapping);
                            }
                        }
                        else
                        {
                            EditMode = GraphEditMode.Selecting;

                            SelectBoxOrigin = SelectBoxCurrent = viewport.ControlToView(pos);

                            bool selected = _control.SelectionManager.UpdateNodeHighlights(false, _shiftPressed, SelectBoxOrigin, SelectBoxCurrent, true, false);
                            if (selected && !_shiftPressed)
                            {
                                _control.SelectionManager.ClearLinkHighlights();
                            }

                            if (!selected)
                            {
                                selected = _control.SelectionManager.UpdateLinkHighlights(false, _shiftPressed, pos);
                                if (selected && !_shiftPressed)
                                {
                                    _control.SelectionManager.ClearNodeHighlights();
                                }
                            }

                            if (!selected)
                            {
                                selected = _control.SelectionManager.UpdateNodeHighlights(false, _shiftPressed, SelectBoxOrigin, SelectBoxCurrent, false, true);
                                if (selected && !_shiftPressed)
                                {
                                    _control.SelectionManager.ClearLinkHighlights();
                                }
                            }
                            
                            CreateSelection();

                            if (Diagram.SelectedNodes.Count > 0 && !_shiftPressed && Diagram.SelectedNodes.Any(o => _control.Viewport.HitMovableArea(pos, o)))
                            {
                                EditMode = GraphEditMode.MovingSelection;
                                _moveLastPosition = viewport.ControlToView(pos);

                                _control.SelectionManager.CollectDrivenItems();

                                foreach (GraphNode n in Diagram.SelectedNodes.Concat(Diagram.DrivenNodes))
                                {
                                    n.MarkMovingPosition(Snapping);
                                }
                            }
                        }
                        break;

                    case GuiMouseButtons.Middle:
                        EditMode = GraphEditMode.Pan;
                        _scrolling = false;
                        _scrollLastX = pos.X;
                        _scrollLastY = pos.Y;
                        View.MarkMovingPosition();
                        break;

                    case GuiMouseButtons.Right:
                        EditMode = GraphEditMode.Pressing;
                        break;
                }
                break;
        }

        _control.RequestOutput();
    }

    private void HandleMouseWheel(IGraphicInput input)
    {
        if (input.MouseLocation is not Point pos)
        {
            return;
        }

        input.Handled = true;

        float newViewZoom;

        if (input.MouseDelta != 0)
        {
            if (input.MouseDelta > 0)
            {
                newViewZoom = View.ViewZoom * 1.2f;
            }
            else
            {
                newViewZoom = View.ViewZoom / 1.2f;
            }

            if (newViewZoom > 0.1f && newViewZoom < 20.0f)
            {
                View.ViewZoom = newViewZoom;
            }
        }

        if (EditMode == GraphEditMode.SelectingBox)
        {
            SelectBoxCurrent = _control.Viewport.ControlToView(pos);
        }

        _viewZooming = true;

        _control.UpdateFontSize();

        _control.RequestOutput();

        _control.OnViewZoomedInternal();
    }

    private void HandleMouseUp(IGraphicInput input)
    {
        if (input.MouseLocation is not Point pos)
        {
            return;
        }

        bool draw = true;

        switch (EditMode)
        {
            case GraphEditMode.Pan:
                if (input.MouseButton == GuiMouseButtons.Middle)
                {
                    EditMode = GraphEditMode.Idle;
                    _control.RequestOutput();
                }
                draw = false;
                break;

            case GraphEditMode.Pressing:
                if (input.MouseButton == GuiMouseButtons.Right && ContextMenuShowing != null)
                {
                    EditMode = GraphEditMode.Idle;
                    if (!input.Handled)
                    {
                        ContextMenuShowing?.Invoke(this, new GraphicContextEventArgs(_control.GraphicContext));
                    }
                }
                draw = false;
                break;

            case GraphEditMode.Selecting:
            case GraphEditMode.SelectingBox:
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    CreateSelection();
                    EditMode = GraphEditMode.Idle;
                }
                break;

            case GraphEditMode.MovingSelection:
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    if (_movingSelection)
                    {
                        _movingSelection = false;

                        Point currentCursorLoc = _control.Viewport.ControlToView(pos);

                        int deltaX = currentCursorLoc.X - _mouseDownPosition.X;
                        int deltaY = currentCursorLoc.Y - _mouseDownPosition.Y;

                        if (deltaX != 0 || deltaY != 0)
                        {
                            SelectionMoved?.Invoke(this, new GraphNodeMoveEventArgs(new Point(deltaX, deltaY)));
                        }

                        draw = false;

                        Diagram.DrivenNodes.Clear();
                    }

                    EditMode = GraphEditMode.Idle;
                }
                break;

            case GraphEditMode.Linking:
                ToConnector = _control.Viewport.GetHitConnector(pos);
                ValidateLink();
                EditMode = GraphEditMode.Idle;
                break;

            case GraphEditMode.Resizing:
                {
                    _control.GraphicContext?.SetCursor(GuiCursorTypes.Default);

                    _control.RefreshNode(Diagram.SelectedNodes);

                    if (Diagram.SelectedNodes.Count > 0)
                    {
                        var node = Diagram.SelectedNodes[0];

                        var oldBound = new Rectangle(node._movingPoint, node._resizingSize);
                        var newBound = node.HitRectangle;
                        if (oldBound != newBound)
                        {
                            SelectionResized?.Invoke(this, new GraphNodeResizeEventArgs(oldBound, newBound));
                        }
                    }

                    EditMode = GraphEditMode.Idle;
                }
                break;
        }

        input.Handled = true;

        if (draw)
        {
            _control.RequestOutput();
        }
    }

    private void HandleMouseMove(IGraphicInput input)
    {
        if (input.MouseLocation is not Point pos)
        {
            return;
        }

        var viewport = _control.Viewport;

        ScreenSpaceCursorLocation = pos;
        ViewSpaceCursorLocation = viewport.ControlToView(pos);

        switch (EditMode)
        {
            case GraphEditMode.Pan:
                if (!_scrolling)
                {
                    int dx = _mouseDownCtrlPosition.X - pos.X;
                    int dy = _mouseDownCtrlPosition.Y - pos.Y;
                    if (Math.Abs(dx) + Math.Abs(dy) > 10)
                    {
                        _scrolling = true;
                    }
                    else
                    {
                        break;
                    }
                }

                float zoom = viewport.ScaledViewZoom;

                View._movingViewX += (pos.X - _scrollLastX) / zoom;
                View._movingViewY += (pos.Y - _scrollLastY) / zoom;

                View.ViewX = (int)View._movingViewX;
                View.ViewY = (int)View._movingViewY;

                _scrollLastX = pos.X;
                _scrollLastY = pos.Y;

                _control.RequestOutput();
                input.Handled = true;
                break;

            case GraphEditMode.Selecting:
                EditMode = GraphEditMode.SelectingBox;
                SelectBoxCurrent = viewport.ControlToView(pos);
                _control.SelectionManager.UpdateNodeHighlights(false, _shiftPressed, SelectBoxOrigin, SelectBoxCurrent);

                _control.RequestOutput();
                break;

            case GraphEditMode.SelectingBox:
                if (viewport.IsInScrollArea(pos))
                {
                    viewport.UpdateScrollInternal(pos);
                }

                SelectBoxCurrent = viewport.ControlToView(pos);
                _control.SelectionManager.UpdateNodeHighlights(true, _shiftPressed, SelectBoxOrigin, SelectBoxCurrent);

                _control.RequestOutput();
                break;

            case GraphEditMode.MovingSelection:
                {
                    if (viewport.IsInScrollArea(pos))
                    {
                        viewport.UpdateScrollInternal(pos);
                    }

                    Point currentCursorLoc = viewport.ControlToView(pos);

                    if (!_movingSelection)
                    {
                        int dx = _mouseDownCtrlPosition.X - pos.X;
                        int dy = _mouseDownCtrlPosition.Y - pos.Y;
                        if (Math.Abs(dx) + Math.Abs(dy) > 10)
                        {
                            _movingSelection = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    _control.SelectionManager.MoveSelection(_mouseDownPosition, currentCursorLoc, Snapping);
                    _moveLastPosition = currentCursorLoc;

                    _control.RequestOutput();
                    input.Handled = true;
                    break;
                }

            case GraphEditMode.Linking:
                if (viewport.IsInScrollArea(pos))
                {
                    viewport.UpdateScrollInternal(pos);
                }

                _control.RequestOutput();
                input.Handled = true;
                break;

            case GraphEditMode.Resizing:
                {
                    if (viewport.IsInScrollArea(pos))
                    {
                        viewport.UpdateScrollInternal(pos);
                    }

                    Point currentCursorLoc = viewport.ControlToView(pos);

                    _control.SelectionManager.ResizeSelection(_mouseDownPosition, currentCursorLoc, ResizeSide, Snapping);
                    _moveLastPosition = currentCursorLoc;

                    _control.RequestOutput();
                    input.Handled = true;
                    break;
                }

            default:
                if (Diagram.SelectedNodes.Count == 1)
                {
                    var side = _control.Viewport.HitSide(pos);
                    if (side != LastResizeSide)
                    {
                        LastResizeSide = side;
                        var cursor = side.GetCursor();
                        _control.GraphicContext?.SetCursor(cursor);
                    }
                }
                else
                {
                    if (LastResizeSide != GraphResizeSide.Outside)
                    {
                        LastResizeSide = GraphResizeSide.Outside;
                    }
                }
                break;
        }
    }

    private void HandleKeyDown(IGraphicInput input)
    {
        if (input.AltKey) _altPressed = true;
        if (input.ControlKey) _ctrlPressed = true;
        if (input.ShiftKey) _shiftPressed = true;

        switch (input.KeyCode)
        {
            case GraphControl.KEY_DELETE:
                if (_control.Viewport.IsMouseInside)
                {
                    _control.DeleteSelected();
                    _control.RequestOutput();
                    input.Handled = true;
                }
                break;

            case GraphControl.KEY_ESCAPE:
                _control.SetNodeSelection([]);
                break;

            case GraphControl.KEY_INSERT:
                NodeCreateRequesting?.Invoke(this, EventArgs.Empty);
                input.Handled = true;
                break;

            case "F":
                if (_control.Viewport.IsMouseInside)
                {
                    if (Diagram.SelectedNodes.Count > 0)
                    {
                        var rect = GraphHelper.GetBoundingBox(Diagram.SelectedNodes.Select(o => o.HitRectangle));
                        _control.Viewport.FocusToRect(rect);
                    }
                    else
                    {
                        var rect = GraphHelper.GetBoundingBox(Diagram.NodeCollection.Select(o => o.HitRectangle));
                        _control.Viewport.FocusToRect(rect);
                    }

                    input.Handled = true;
                }
                break;

            case "G" when input.GetOnlyControlKey():
                if (Diagram.SelectedNodes.Count > 0)
                {
                    GroupCreateRequesting?.Invoke(this, EventArgs.Empty);
                    input.Handled = true;
                }
                break;
        }
    }

    private void HandleKeyUp(IGraphicInput input)
    {
        if (!input.AltKey) _altPressed = false;
        if (!input.ControlKey) _ctrlPressed = false;
        if (!input.ShiftKey) _shiftPressed = false;
    }

    private void DeleteLinkConnectors(GraphConnector connector)
    {
        var linksToDelete = Diagram.Links.GetLinks(connector).ToList();

        foreach (var link in linksToDelete)
        {
            Diagram.Links.Remove(link);
        }

        LinkDestroyed?.Invoke(this, new GraphLinkEventArgs(linksToDelete));

        _control.RefreshView();
    }

    private void ValidateLink()
    {
        if (FromConnector != null &&
            ToConnector != null &&
            FromConnector != ToConnector &&
            FromConnector.Direction != ToConnector.Direction &&
            !_control.LinkManager.IsLinked(FromConnector, ToConnector))
        {
            bool canConnect = _control.LinkManager.GetCanConnect(FromConnector, ToConnector, out bool converted) == true;
            if (canConnect)
            {
                if (FromConnector.Direction == GraphDirection.Output)
                {
                    if (!ToConnector.GetAllowMultipleToConnection() && _control.LinkManager.IsLinked(ToConnector))
                    {
                        DeleteLinkConnectors(ToConnector);
                    }

                    if (!FromConnector.GetAllowMultipleFromConnection() && _control.LinkManager.IsLinked(FromConnector))
                    {
                        DeleteLinkConnectors(FromConnector);
                    }

                    var link = new GraphLink(FromConnector, ToConnector, FromConnector.ConnectorType, FromConnector.DataType)
                    {
                        IsConverted = converted,
                    };

                    Diagram.Links.Add(link);

                    LinkCreated?.Invoke(this, new GraphLinkEventArgs(link));
                }
                else
                {
                    if (!FromConnector.GetAllowMultipleToConnection() && _control.LinkManager.IsLinked(FromConnector))
                    {
                        DeleteLinkConnectors(FromConnector);
                    }

                    if (!ToConnector.GetAllowMultipleFromConnection() && _control.LinkManager.IsLinked(ToConnector))
                    {
                        DeleteLinkConnectors(ToConnector);
                    }

                    var link = new GraphLink(ToConnector, FromConnector, ToConnector.ConnectorType, ToConnector.DataType)
                    {
                        IsConverted = converted,
                    };

                    Diagram.Links.Add(link);

                    LinkCreated?.Invoke(this, new GraphLinkEventArgs(link));
                }

                _control.RequestOutput();
            }
        }

        FromConnector = null;
        ToConnector = null;
    }

    private void CreateSelection()
    {
        SelectionChanging?.Invoke(this, new GraphSelectionEventArgs(Diagram.SelectedNodes.Count, Diagram.SelectedLinks.Count));

        Diagram.SelectedNodes.Clear();
        int numNodes = 0;
        foreach (GraphNode node in Diagram.NodeCollection)
        {
            if (node.Highlighted)
            {
                numNodes++;
                Diagram.SelectedNodes.Add(node);
            }
        }

        Diagram.SelectedLinks.Clear();
        int numLinks = 0;
        foreach (GraphLink link in Diagram.Links)
        {
            if (link.Highlighted)
            {
                numLinks++;
                Diagram.SelectedLinks.Add(link);
            }
        }
        
        Diagram.SelectionBringToFront();

        SelectionChanged?.Invoke(this, new GraphSelectionEventArgs(numNodes, numLinks));
    }
}
