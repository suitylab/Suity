using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// The main control for rendering and interacting with a node graph. Implements <see cref="IGraphicObject"/> for graphics integration.
/// </summary>
public class GraphControl : IGraphicObject
{
    /// <summary>
    /// The key code for the Delete key.
    /// </summary>
    public const string KEY_DELETE = "Delete";
    /// <summary>
    /// The key code for the Escape key.
    /// </summary>
    public const string KEY_ESCAPE = "Escape";
    /// <summary>
    /// The key code for the Insert key.
    /// </summary>
    public const string KEY_INSERT = "Insert";

    /// <summary>
    /// Gets the theme used for rendering the graph.
    /// </summary>
    public GraphControlTheme Theme { get; } = new();
    /// <summary>
    /// Gets the diagram containing nodes and links.
    /// </summary>
    public GraphDiagram Diagram { get; }
    /// <summary>
    /// Gets the viewport for pan and zoom operations.
    /// </summary>
    public GraphViewport Viewport { get; }
    /// <summary>
    /// Gets the input manager for handling user interactions.
    /// </summary>
    public GraphInputManager InputManager { get; }
    /// <summary>
    /// Gets the selection manager for handling node selection.
    /// </summary>
    public GraphSelectionManager SelectionManager { get; }
    /// <summary>
    /// Gets the link manager for handling link operations.
    /// </summary>
    public GraphLinkManager LinkManager { get; }
    /// <summary>
    /// Gets the drawer responsible for rendering the graph.
    /// </summary>
    public GraphDrawer Drawer { get; }


    #region Events

    public event EventHandler? NodeCreateRequesting;
    public event EventHandler? GroupCreateRequesting;
    public event GraphSelectionEventHandler? SelectionChanging;
    public event GraphSelectionEventHandler? SelectionChanged;
    public event GraphSelectionEventHandler? SelectionDeleting;
    public event GraphSelectionEventHandler? SelectionDeleted;
    public event GraphNodeMoveEventHandler? SelectionMoved;
    public event GraphNodeResizeEventHandler? SelectionResized;
    public event GraphLinkEventHandler? LinkCreated;
    public event GraphLinkEventHandler? LinkDestroyed;
    public event EventHandler<GraphicContextEventArgs>? ContextMenuShowing;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphControl"/> class, creating all sub-components.
    /// </summary>
    public GraphControl()
    {
        Theme = CreateTheme() ?? throw new InvalidOperationException("CreateTheme returned null.");
        Diagram = CreateDiagram() ?? throw new InvalidOperationException("CreateDiagram returned null.");
        Viewport = CreateViewport() ?? throw new InvalidOperationException("CreateViewport returned null.");
        InputManager = CreateInputManager() ?? throw new InvalidOperationException("CreateInputManager returned null.");
        SelectionManager = CreateSelectionManager() ?? throw new InvalidOperationException("CreateSelectionManager returned null.");
        LinkManager = CreateLinkManager() ?? throw new InvalidOperationException("CreateLinkManager returned null.");
        Drawer = CreateDrawer() ?? throw new InvalidOperationException("CreateDrawer returned null.");
        
        if (Diagram.ParentControl != this) throw new InvalidOperationException("Diagram's parent control is not set to this instance.");
        if (Viewport.ParentControl != this) throw new InvalidOperationException("Viewport's parent control is not set to this instance.");
        if (InputManager.ParentControl != this) throw new InvalidOperationException("InputManager's parent control is not set to this instance.");
        if (SelectionManager.ParentControl != this) throw new InvalidOperationException("SelectionManager's parent control is not set to this instance.");
        if (LinkManager.ParentControl != this) throw new InvalidOperationException("LinkManager's parent control is not set to this instance.");
        if (Drawer.ParentControl != this) throw new InvalidOperationException("Drawer's parent control is not set to this instance.");

        InputManager.SelectionChanging += (s, e) => SelectionChanging?.Invoke(this, e);
        InputManager.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, e);
        InputManager.SelectionDeleting += (s, e) => SelectionDeleting?.Invoke(this, e);
        InputManager.SelectionDeleted += (s, e) => SelectionDeleted?.Invoke(this, e);
        InputManager.SelectionMoved += (s, e) => SelectionMoved?.Invoke(this, e);
        InputManager.SelectionResized += (s, e) => SelectionResized?.Invoke(this, e);
        InputManager.LinkCreated += (s, e) => LinkCreated?.Invoke(this, e);
        InputManager.LinkDestroyed += (s, e) => LinkDestroyed?.Invoke(this, e);
        InputManager.NodeCreateRequesting += (s, e) => NodeCreateRequesting?.Invoke(this, e);
        InputManager.GroupCreateRequesting += (s, e) => GroupCreateRequesting?.Invoke(this, e);
        InputManager.ContextMenuShowing += (s, e) => ContextMenuShowing?.Invoke(this, e);
    }

    #region IGraphicObject

    private IGraphicContext? _graphicContext;
    /// <summary>
    /// Gets or sets the graphic context associated with this control.
    /// </summary>
    public IGraphicContext? GraphicContext
    {
        get => _graphicContext;
        set => _graphicContext = value;
    }

    /// <inheritdoc/>
    public virtual void HandleGraphicInput(IGraphicInput input)
    {
        InputManager.HandleInput(input);
    }

    /// <inheritdoc/>
    public virtual void HandleGraphicOutput(IGraphicOutput output)
    {
        Viewport.Width = output.Width;
        Viewport.Height = output.Height;
        Viewport.GlobalViewRect = new RectangleF(0, 0, Viewport.Width, Viewport.Height);

        Drawer.PreGraphicOutput(output);
        Drawer.PreGraphicOutput2(output);
        Drawer.PostGraphicOutput(output);
    }

    /// <summary>
    /// Handles input events before the main graphic output. Can be overridden by derived classes.
    /// </summary>
    /// <param name="input">The graphic input to process.</param>
    /// <returns>True if the input was handled; otherwise, false.</returns>
    protected virtual bool PreGraphicInput(IGraphicInput input)
    {
        return false;
    }

    internal bool PreGraphicInputInternal(IGraphicInput input) => PreGraphicInput(input);

    /// <summary>
    /// Updates the font sizes in the theme based on the current zoom level.
    /// </summary>
    internal void UpdateFontSize()
    {
        Theme.UpdateScaledFonts(Viewport.SmoothViewZoom);
    }

    /// <summary>
    /// Requests a redraw of the graphic output.
    /// </summary>
    public void RequestOutput() => _graphicContext?.RequestOutput();

    #endregion

    #region Selection

    /// <summary>
    /// Deletes all currently selected nodes.
    /// </summary>
    /// <returns>The number of nodes deleted.</returns>
    public int DeleteSelected()
    {
        int count = Diagram.SelectedNodes.Count(o => o.CanBeSelected);
        if (count == 0) return 0;

        SelectionDeleting?.Invoke(this, new GraphSelectionEventArgs());

        foreach (GraphNode n in Diagram.SelectedNodes)
        {
            if (!n.CanBeDeleted) continue;

            if (n.Connectors is { } connector)
            {
                foreach (GraphConnector c in connector.ToArray())
                {
                    LinkManager.DeleteLinkConnectors(c, links => LinkDestroyed?.Invoke(this, new GraphLinkEventArgs(links.ToList())));
                }
            }

            Diagram.NodeCollection.Remove(n);
            n.OnMarkDeleted();
            count++;
        }

        SelectionDeleted?.Invoke(this, new GraphSelectionEventArgs());
        Diagram.SelectedNodes.Clear();
        RefreshView();

        return count;
    }

    /// <summary>
    /// Sets the selection to the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to select.</param>
    public void SetNodeSelection(IEnumerable<GraphNode> nodes)
    {
        foreach (GraphNode n in Diagram.NodeCollection)
        {
            n.Highlighted = false;
        }

        int i = 0;
        Diagram.SelectedNodes.Clear();
        foreach (GraphNode node in nodes)
        {
            if (Diagram.NodeCollection.Contains(node))
            {
                node.Highlighted = true;
                Diagram.SelectedNodes.AddRange(nodes);
                i++;
            }
        }

        Diagram.SelectionBringToFront();
        OnSelectionChanged(i, 0);
        RefreshView();
    }

    /// <summary>
    /// Sets the selection to the specified links.
    /// </summary>
    /// <param name="links">The links to select.</param>
    public void SetLinkSelection(IEnumerable<GraphLink> links)
    {
        foreach (GraphLink link in Diagram.Links)
        {
            link.Highlighted = false;
        }

        int i = 0;
        Diagram.SelectedLinks.Clear();
        foreach (GraphLink link in links)
        {
            if (Diagram.Links.Contains(link))
            {
                link.Highlighted = true;
                Diagram.SelectedLinks.Add(link);
                i++;
            }
        }

        Diagram.SelectionBringToFront();
        OnSelectionChanged(0, i);
        RefreshView();
    }

    #endregion

    #region Add/Delete

    /// <summary>
    /// Adds a node to the diagram.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(GraphNode node)
    {
        Diagram.NodeCollection.Add(node);
        LinkManager.UpdateAssociate(node);
        RefreshNode(node);
        OnNodeAdded(node);
    }

    /// <summary>
    /// Deletes a node from the diagram and removes all its associated links.
    /// </summary>
    /// <param name="node">The node to delete.</param>
    public void DeleteNode(GraphNode node)
    {
        if (node is null) return;

        foreach (var c in node.Connectors)
        {
            LinkManager.DeleteLinkConnectors(c, links => LinkDestroyed?.Invoke(this, new GraphLinkEventArgs(links.ToList())));
        }

        Diagram.RemoveNode(node);
        node.OnMarkDeleted();
        RefreshView();
        OnNodeDeleted(node);
    }

    /// <summary>
    /// Deletes all nodes and links from the diagram.
    /// </summary>
    public void DeleteAll()
    {
        foreach (var node in Diagram.NodeCollection)
        {
            node.OnMarkDeleted();
        }

        Diagram.Clear();
        RefreshView();
        OnDiagramCleared();
    }

    #endregion

    #region Refresh

    /// <summary>
    /// Requests a refresh of the entire graph view.
    /// </summary>
    /// <param name="refreshAll">True to force a full refresh; otherwise, false.</param>
    public void RefreshView(bool refreshAll = false)
    {
        if (InputManager.EditMode != GraphEditMode.Idle) return;
        OnRefreshView(refreshAll);
    }

    /// <summary>
    /// Requests a refresh of the specified node.
    /// </summary>
    /// <param name="node">The node to refresh.</param>
    public void RefreshNode(GraphNode node)
    {
        if (InputManager.EditMode != GraphEditMode.Idle) return;
        OnRefreshNode([node]);
    }

    /// <summary>
    /// Requests a refresh of the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to refresh.</param>
    public void RefreshNode(IEnumerable<GraphNode> nodes)
    {
        if (InputManager.EditMode != GraphEditMode.Idle) return;
        OnRefreshNode(nodes);
    }

    /// <summary>
    /// Requests a refresh of the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to refresh.</param>
    public void RefreshNode(params GraphNode[] nodes)
    {
        if (InputManager.EditMode != GraphEditMode.Idle) return;
        OnRefreshNode(nodes);
    }

    #endregion

    #region Factory

    /// <summary>
    /// Creates the theme for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphControlTheme"/> instance.</returns>
    protected virtual GraphControlTheme CreateTheme()
    {
        var theme = new GraphControlTheme();
        theme.InitializeFonts(new FontFamily("Tahoma"));
        return theme;
    }
    /// <summary>
    /// Creates the diagram for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphDiagram"/> instance.</returns>
    protected virtual GraphDiagram CreateDiagram() => new(this);
    /// <summary>
    /// Creates the link manager for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphLinkManager"/> instance.</returns>
    protected virtual GraphLinkManager CreateLinkManager() => new(this);
    /// <summary>
    /// Creates the input manager for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphInputManager"/> instance.</returns>
    protected virtual GraphInputManager CreateInputManager() => new(this);
    /// <summary>
    /// Creates the selection manager for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphSelectionManager"/> instance.</returns>
    protected virtual GraphSelectionManager CreateSelectionManager() => new(this);
    /// <summary>
    /// Creates the viewport for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphViewport"/> instance.</returns>
    protected virtual GraphViewport CreateViewport() => new(this);
    /// <summary>
    /// Creates the drawer for the graph control. Override to customize.
    /// </summary>
    /// <returns>A new <see cref="GraphDrawer"/> instance.</returns>
    protected virtual GraphDrawer CreateDrawer() => new(this);

    #endregion

    #region Virtual

    /// <summary>
    /// Called after a node is added to the diagram. Override to customize.
    /// </summary>
    /// <param name="node">The node that was added.</param>
    protected virtual void OnNodeAdded(GraphNode node) { }

    /// <summary>
    /// Called after a node is deleted from the diagram. Override to customize.
    /// </summary>
    /// <param name="node">The node that was deleted.</param>
    protected virtual void OnNodeDeleted(GraphNode node) { }

    /// <summary>
    /// Called after the diagram is cleared. Override to customize.
    /// </summary>
    protected virtual void OnDiagramCleared() { }


    /// <summary>
    /// Called when the graph view needs to be refreshed. Override to customize.
    /// </summary>
    /// <param name="refreshAll">True if a full refresh is requested; otherwise, false.</param>
    protected virtual void OnRefreshView(bool refreshAll) => _graphicContext?.RequestOutput();

    /// <summary>
    /// Called when specific nodes need to be refreshed. Override to customize.
    /// </summary>
    /// <param name="nodes">The nodes to refresh.</param>
    protected virtual void OnRefreshNode(IEnumerable<GraphNode> nodes) => _graphicContext?.RequestOutput();

    /// <summary>
    /// Called when the selection changes. Override to customize.
    /// </summary>
    /// <param name="count">The number of selected nodes.</param>
    protected virtual void OnSelectionChanged(int nodeCount, int linkCount)
    {
        SelectionChanged?.Invoke(this, new GraphSelectionEventArgs(nodeCount, linkCount));
    }

    /// <summary>
    /// Called when the view zoom changes. Override to customize.
    /// </summary>
    protected virtual void OnViewZoomed() { }

    internal void OnViewZoomedInternal() => OnViewZoomed();

    /// <summary>
    /// Called when a group creation is requested. Override to customize.
    /// </summary>
    protected virtual void OnCreateGroupRequest() { }

    #endregion
}
