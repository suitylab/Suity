using Suity.Drawing;
using Suity.Views.Graphics;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Represents a base node for use in a GraphDiagram
/// </summary>
public abstract class GraphNode
{
    internal protected int _height;
    internal protected int _width;
    private bool _highlighted;
    private bool _canBeSelected;
    private bool _canBeDeleted;
    private string _comment;
    private BrushDef _previewBrush;


    protected string _name;

    protected GraphConnectorCollection _connectors;
    protected GraphDiagram _diagram;

    internal Point _movingPoint;
    internal Size _resizingSize;

    /// <summary>
    /// Whether the node can be selected.
    /// </summary>
    public bool CanBeSelected { get => _canBeSelected; set => _canBeSelected = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the node can be deleted.
    /// </summary>
    public bool CanBeDeleted { get => _canBeDeleted; set => _canBeDeleted = value; }

    /// <summary>
    /// Gets the display name of the node.
    /// </summary>
    public string Name
    {
        get => GetName();
        set => _name = value;
    }


    #region ITextDisplay

    /// <inheritdoc/>
    public virtual string DisplayText => Name;

    /// <inheritdoc/>
    public virtual object DisplayIcon => Icon;

    /// <inheritdoc/>
    public virtual TextStatus DisplayStatus { get; } 

    #endregion

    /// <summary>
    /// New preview value
    /// </summary>

    /// <summary>
    /// Gets the preview text displayed on the node.
    /// </summary>
    public virtual string PreviewText => string.Empty;

    
    /// <summary>
    /// Gets the icon displayed on the node.
    /// </summary>
    public virtual ImageDef Icon => null;

    /// <summary>
    /// Gets a value indicating whether this node is a group node.
    /// </summary>
    public virtual bool IsGroup => false;
    
    /// <summary>
    /// Gets a value indicating whether this node has a header area.
    /// </summary>
    public virtual bool HasHeader => true;

    /// <summary>
    /// Gets a value indicating whether multiple connectors of the specified direction should be rendered.
    /// </summary>
    /// <param name="type">The connector direction to check.</param>
    /// <returns>True if multiple connectors should be rendered; otherwise, false.</returns>
    public virtual bool RenderMutiple(GraphDirection type) => false;

    internal protected int _x;
    internal protected int _y;

    /// <summary>
    /// X Position (ViewSpace) of the Node
    /// </summary>
    public int X => _x;

    /// <summary>
    /// Y Position (ViewSpace) of the Node
    /// </summary>
    public int Y => _y;

    /// <summary>
    /// Width (ViewSpace) of the node
    /// </summary>
    public int Width
    {
        get => _width;
        set { _width = value; UpdateHitRectangle(); }
    }

    /// <summary>
    /// Height (ViewSpace) of the node
    /// </summary>
    public int Height
    { 
        get => _height; 
        set { _height = value; UpdateHitRectangle(); }
    }

    /// <summary>
    /// The Diagram associated to this node
    /// </summary>
    public GraphDiagram Diagram => _diagram;

    /// <summary>
    /// Whether the node is highlighted
    /// </summary>
    public bool Highlighted
    {
        get => _highlighted;
        set
        {
            if (_highlighted != value)
            {
                _highlighted = value;
                OnHighlightedChanged();
            }
        }
    }

    /// <summary>
    /// The Hit (Mouse Click) rectangle of the Node
    /// </summary>
    public Rectangle HitRectangle;

    /// <summary>
    /// The list of NodeGraphConnectors owned by this Node
    /// </summary>
    public GraphConnectorCollection Connectors => _connectors;

    /// <summary>
    /// The displayed Commentary of the node
    /// </summary>
    public string Comment { get => _comment; set => _comment = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the node can be resized by the user.
    /// </summary>
    public virtual bool Resizable => false;


    /// <summary>
    /// Gets the brush used for drawing the preview text background.
    /// </summary>
    protected BrushDef PreviewBrush => _previewBrush;

    /// <summary>
    /// Occurs after the node is drawn, allowing custom post-draw rendering.
    /// </summary>
    public event EventHandler<GraphicOutputEventArgs> onPostDraw;

    /// <summary>
    /// Creates a new NodeGraphNode into the NodeGraphView, given coordinates and ability to be selected
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="diagram"></param>
    /// <param name="canBeSelected"></param>
    public GraphNode(GraphDiagram diagram, int x, int y, bool canBeSelected, bool canBeDeleted)
    {
        _x = x;
        _y = y;
        _diagram = diagram;
        _width = 140;
        _height = 64;
        _name = "Test Void Node";
        _canBeSelected = canBeSelected;
        _canBeDeleted = canBeDeleted;
        Highlighted = false;
        _comment = "";

        UpdateHitRectangle();

        _connectors = new();

        _previewBrush = new SolidBrushDef(Color.FromArgb(90, 0, 0, 0));
    }

    /// <summary>
    /// Gets the connector index, given the connector object reference
    /// </summary>
    /// <param name="connector">the connector reference</param>
    /// <returns>the connector index</returns>
    [Obsolete]
    public int GetConnectorIndex(GraphConnector connector)
    {
        for (int i = 0; i < _connectors.Count; i++)
        {
            if (_connectors[i] == connector)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds a connector by its name.
    /// </summary>
    /// <param name="name">The name of the connector.</param>
    /// <returns>The connector if found; otherwise, null.</returns>
    public GraphConnector FindConnector(string name) => Connectors.FirstOrDefault(o => o.Name == name);

    /// <summary>
    /// Returns the name of the node: can be overriden to match custom names.
    /// </summary>
    /// <returns></returns>
    protected virtual string GetName() => _name;

    /// <summary>
    /// Updates HitRectangle (when moved)
    /// </summary>
    public virtual void UpdateHitRectangle(bool notifyPosition = false, bool notifySize = false)
    {
        HitRectangle = new Rectangle(_x, _y, _width, _height);

        if (notifyPosition)
        {
            OnPositionUpdated();
        }

        if (notifySize)
        {
            OnSizeUpdated();
        }
    }

    /// <summary>
    /// Intercepts a mouse hit and returns a Connector if hit by the mouse, null otherwise
    /// </summary>
    /// <param name="screenPosition"></param>
    /// <returns></returns>
    public GraphConnector GetConnectorMouseHit(Point screenPosition)
    {
        RectangleF hitRectangle = new RectangleF(screenPosition, Size.Empty);

        foreach (GraphConnector connector in _connectors.Where(o => o.ConnectorType != ConnectorType.Associate))
        {
            if (hitRectangle.IntersectsWith(connector.GetHitArea()))
            {
                return connector;
            }
        }

        return null;
    }

    /// <summary>
    /// Draws the node
    /// </summary>
    /// <param name="e"></param>
    public virtual void Draw(IGraphicOutput output) { }

    /// <summary>
    /// Raises the post-draw event for custom rendering after the node is drawn.
    /// </summary>
    /// <param name="output">The graphic output surface.</param>
    protected void RaisePostDraw(IGraphicOutput output)
    {
        onPostDraw?.Invoke(this, new GraphicOutputEventArgs(output));
    }

    #region Draw Func

    /// <summary>
    /// Gets the brush used for filling the node body.
    /// </summary>
    public virtual BrushDef NodeFillBrush => _diagram.ParentControl.Theme.NodeFill;
    /// <summary>
    /// Gets the brush used for filling the node header.
    /// </summary>
    public virtual BrushDef NodeHeaderFillBrush => _diagram.ParentControl.Theme.NodeHeaderFill;
    /// <summary>
    /// Gets the pen used for drawing the node outline.
    /// </summary>
    public virtual PenDef NodeOutlinePen => _diagram.ParentControl.Theme.NodeOutline;
    /// <summary>
    /// Gets the pen used for drawing the node outline when selected.
    /// </summary>
    public virtual PenDef NodeOutlineSelected => _diagram.ParentControl.Theme.NodeOutlineSelected;
    /// <summary>
    /// Gets the scaled font used for preview text.
    /// </summary>
    public virtual FontDef NodeScaledPreviewFont => _diagram.ParentControl.Theme.NodeScaledPreviewFont;
    /// <summary>
    /// Gets the scaled font used for the node title.
    /// </summary>
    public virtual FontDef NodeScaledTitleFont => _diagram.ParentControl.Theme.NodeScaledTitleFont;
    /// <summary>
    /// Gets the brush used for drawing node text.
    /// </summary>
    public virtual BrushDef NodeText => _diagram.ParentControl.Theme.NodeText;
    /// <summary>
    /// Gets the brush used for drawing node text shadow.
    /// </summary>
    public virtual BrushDef NodeTextShadow => _diagram.ParentControl.Theme.NodeTextShadow;


    #endregion

    #region Area & Position Func

    /// <summary>
    /// Gets the header area rectangle for hit testing. Returns null if the node has no header.
    /// </summary>
    /// <returns>The header area rectangle, or null.</returns>
    public virtual RectangleF? GetHeaderArea() => null;

    /// <inheritdoc/>
    public virtual RectangleF GetConnectorArea(GraphConnector connector) => RectangleF.Empty;

    /// <summary>
    /// Gets the position of the specified connector in view space.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>The connector position in view space.</returns>
    public virtual PointF GetConnectorPosition(GraphConnector connector) => PointF.Empty;

    /// <inheritdoc/>
    public virtual RectangleF GetConnectorHitArea(GraphConnector connector) => RectangleF.Empty;

    #endregion


    /// <summary>
    /// Handles a double-click event on the node. Override to customize.
    /// </summary>
    public virtual void HandleDoubleClick()
    {
    }

    /// <summary>
    /// Called when the highlighted state changes. Override to customize.
    /// </summary>
    protected virtual void OnHighlightedChanged()
    {
    }
    /// <summary>
    /// Called when the node position is updated. Override to customize.
    /// </summary>
    protected virtual void OnPositionUpdated()
    {
    }
    /// <summary>
    /// Called when the node size is updated. Override to customize.
    /// </summary>
    protected virtual void OnSizeUpdated()
    {

    }
    /// <summary>
    /// Called when the node is marked for deletion. Override to customize.
    /// </summary>
    internal protected virtual void OnMarkDeleted()
    {
    }

    /// <summary>
    /// Stores the current position for move operations.
    /// </summary>
    /// <param name="snapping">True to snap to grid; otherwise, false.</param>
    internal void MarkMovingPosition(bool snapping)
    {
        if (snapping)
        {
            _movingPoint = new Point((int)Math.Round(_x * 0.1f) * 10, (int)Math.Round(_y * 0.1f) * 10);
        }
        else
        {
            _movingPoint = new Point(_x, _y);
        }
    }

    /// <summary>
    /// Stores the current size for resize operations.
    /// </summary>
    /// <param name="snapping">True to snap to grid; otherwise, false.</param>
    internal void MarkResizingSize(bool snapping)
    {
        if (snapping)
        {
            _resizingSize = new Size((int)Math.Round(_width * 0.1f) * 10, (int)Math.Round(_height * 0.1f) * 10);
        }
        else
        {
            _resizingSize = new Size(_width, _height);
        }
    }

    public override string ToString() => Name;
}