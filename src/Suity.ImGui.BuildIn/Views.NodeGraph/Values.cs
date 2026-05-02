using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Current edit mode
/// </summary>
public enum GraphEditMode
{
    /// <summary>No active operation.</summary>
    Idle,
    /// <summary>Right mouse button is being pressed (for context menu).</summary>
    Pressing,
    /// <summary>Panning the viewport.</summary>
    Pan,
    /// <summary>Zooming the viewport.</summary>
    Zooming,
    /// <summary>Starting a selection operation.</summary>
    Selecting,
    /// <summary>Dragging a selection box.</summary>
    SelectingBox,
    /// <summary>Moving selected nodes.</summary>
    MovingSelection,
    /// <summary>Creating a link between connectors.</summary>
    Linking,
    /// <summary>Resizing a node.</summary>
    Resizing,
}

/// <summary>
/// Type of mouse hit
/// </summary>
public enum HitType
{
    /// <summary>Nothing was hit.</summary>
    Nothing,
    /// <summary>A node body was hit.</summary>
    Node,
    /// <summary>A node header (move area) was hit.</summary>
    NodeMoveArea,
    /// <summary>A connector was hit.</summary>
    Connector
}

/// <summary>
/// Visual mode of viewing links
/// </summary>
public enum LinkVisualStyle
{
    /// <summary>Links are drawn as straight lines.</summary>
    Direct,
    /// <summary>Links are drawn as rectangular paths.</summary>
    Rectangle,
    /// <summary>Links are drawn as bezier curves.</summary>
    Curve
}

/// <summary>
/// The type of connector on a node.
/// </summary>
public enum ConnectorType
{
    /// <summary>Data connector for passing values.</summary>
    Data,
    /// <summary>Action connector for execution flow.</summary>
    Action,
    /// <summary>Associate connector for loose associations.</summary>
    Associate,
    /// <summary>Control connector for UI control.</summary>
    Control,
}

/// <summary>
/// The Direction of connector associated to the Node
/// </summary>
public enum GraphDirection
{
    /// <summary>Input connector (receives data).</summary>
    Input,
    /// <summary>Output connector (sends data).</summary>
    Output
}

/// <summary>
/// Represents which side of a node rectangle is being resized.
/// </summary>
public enum GraphResizeSide
{
    /// <summary>Inside the rectangle but not on an edge.</summary>
    Inside,

    /// <summary>Left edge.</summary>
    Left,
    /// <summary>Right edge.</summary>
    Right,
    /// <summary>Top edge.</summary>
    Top,
    /// <summary>Bottom edge.</summary>
    Bottom,

    /// <summary>Bottom-right corner.</summary>
    Corner,

    /// <summary>Outside the rectangle.</summary>
    Outside,
}

// DELEGATES & EVENTARGS

#region GraphSelectionEvent

/// <summary>
/// Delegate for selection change events.
/// </summary>
public delegate void GraphSelectionEventHandler(object sender, GraphSelectionEventArgs args);

/// <summary>
/// Event arguments for selection change events.
/// </summary>
public class GraphSelectionEventArgs : EventArgs
{
    public static GraphSelectionEventArgs Empty { get; } = new();

    /// <summary>
    /// The number of nodes in the new selection.
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// The number of links in the new selection.
    /// </summary>
    public int LinkCount { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="GraphSelectionEventArgs"/> class.
    /// </summary>
    private GraphSelectionEventArgs()
    {
        NodeCount = 0;
        LinkCount = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphSelectionEventArgs"/> class.
    /// </summary>
    /// <param name="nodeCount">The number of selected nodes.</param>
    /// <param name="linkCount">The number of selected links.</param>
    public GraphSelectionEventArgs(int nodeCount, int linkCount)
    {
        this.NodeCount = nodeCount;
        this.LinkCount = linkCount;
    }
}

#endregion

#region GraphLinkEvent

/// <summary>
/// Delegate for link change events.
/// </summary>
public delegate void GraphLinkEventHandler(object sender, GraphLinkEventArgs args);

/// <summary>
/// Event arguments for link change events.
/// </summary>
public class GraphLinkEventArgs : EventArgs
{
    /// <summary>
    /// The list of affected links.
    /// </summary>
    public List<GraphLink> Links;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphLinkEventArgs"/> class with a list of links.
    /// </summary>
    /// <param name="affectedLinks">The affected links.</param>
    public GraphLinkEventArgs(List<GraphLink> affectedLinks)
    {
        this.Links = affectedLinks;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphLinkEventArgs"/> class with a single link.
    /// </summary>
    /// <param name="link">The affected link.</param>
    public GraphLinkEventArgs(GraphLink link)
    {
        Links = [link];
    }
}

#endregion

#region GraphNodeMoveEvent

/// <summary>
/// Delegate for node move events.
/// </summary>
public delegate void GraphNodeMoveEventHandler(object sender, GraphNodeMoveEventArgs args);

/// <summary>
/// Event arguments for node move events.
/// </summary>
public class GraphNodeMoveEventArgs : EventArgs
{
    /// <summary>
    /// The offset by which the nodes were moved.
    /// </summary>
    public Point Offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphNodeMoveEventArgs"/> class.
    /// </summary>
    /// <param name="offset">The movement offset.</param>
    public GraphNodeMoveEventArgs(Point offset)
    {
        Offset = offset;
    }
}

#endregion

#region GraphNodeResizeEvent

/// <summary>
/// Delegate for node resize events.
/// </summary>
public delegate void GraphNodeResizeEventHandler(object sender, GraphNodeResizeEventArgs args);

/// <summary>
/// Event arguments for node resize events.
/// </summary>
public class GraphNodeResizeEventArgs : EventArgs
{
    /// <summary>
    /// The bounding rectangle before resizing.
    /// </summary>
    public Rectangle OldBound;

    /// <summary>
    /// The bounding rectangle after resizing.
    /// </summary>
    public Rectangle NewBound;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphNodeResizeEventArgs"/> class.
    /// </summary>
    /// <param name="oldBound">The old bounding rectangle.</param>
    /// <param name="newBound">The new bounding rectangle.</param>
    public GraphNodeResizeEventArgs(Rectangle oldBound, Rectangle newBound)
    {
        OldBound = oldBound;
        NewBound = newBound;
    }
}

#endregion
