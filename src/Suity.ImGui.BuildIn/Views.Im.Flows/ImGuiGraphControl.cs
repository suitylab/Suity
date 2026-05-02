using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Views.NodeGraph;
using Suity.Views.Graphics;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Main ImGui-based graph control that manages node rendering, input handling, and viewport synchronization.
/// </summary>
public class ImGuiGraphControl : GraphControl, IDrawContext
{
    private readonly ImGuiGraphViewport _viewport;
    private readonly GuiViewportValue _guiViewport = new();
    private readonly ClearCacheAction _clearCacheAction;

    private Point _lastMousePos;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiGraphControl"/> class.
    /// </summary>
    public ImGuiGraphControl()
    {
        _clearCacheAction = new ClearCacheAction(this);
        _viewport = (ImGuiGraphViewport)base.Viewport;
    }

    /// <summary>
    /// Gets or sets the main flowchart control view that owns this control.
    /// </summary>
    public IFlowView? OwnerFlowView { get; set; }

    /// <summary>
    /// Gets a value indicating whether partial GUI sync mode is active.
    /// </summary>
    public bool PartialGuiSync { get; private set; }

    /// <summary>
    /// Gets the set of nodes that are being updated during partial sync.
    /// </summary>
    public HashSet<GraphNode> PartialUpdatingNodes { get; } = [];

    /// <summary>
    /// Gets the ImGui instance associated with the viewport.
    /// </summary>
    public ImGui? Gui => _viewport.Gui;

    /// <summary>
    /// Gets the last recorded mouse position.
    /// </summary>
    public Point LastMousePos => _lastMousePos;


    #region Partial Gui Sync

    /// <summary>
    /// Begins partial GUI sync mode, clearing the list of nodes to update.
    /// </summary>
    public void BeginPartialGuiSync()
    {
        PartialGuiSync = true;
        PartialUpdatingNodes.Clear();
    }

    /// <summary>
    /// Begins partial GUI sync mode for a single node.
    /// </summary>
    /// <param name="node">The node to include in partial sync.</param>
    public void BeginPartialGuiSync(ImGraphNode node)
    {
        PartialGuiSync = true;
        PartialUpdatingNodes.Add(node);
    }

    /// <summary>
    /// Begins partial GUI sync mode for a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to include in partial sync.</param>
    public void BeginPartialGuiSync(IEnumerable<ImGraphNode> nodes)
    {
        PartialGuiSync = true;
        PartialUpdatingNodes.AddRange(nodes);
    }

    /// <summary>
    /// Begins partial GUI sync mode for the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to include in partial sync.</param>
    public void BeginPartialGuiSync(params ImGraphNode[] nodes)
    {
        PartialGuiSync = true;
        PartialUpdatingNodes.AddRange(nodes);
    }

    #endregion

    #region ImGui

    readonly List<ImGraphNode> _tempNodeList = [];

    /// <summary>
    /// Builds the GUI for all nodes in the viewport.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The viewport ImGui node.</returns>
    public ImGuiNode OnNodeGui(ImGui gui)
    {
        var viewport = _viewport.Node = gui.BeginCurrentNode("Viewport")
        .OnInitialize(n =>
        {
            n.SetValue(_guiViewport);
            n.SetLayoutFunction(ImGuiLayoutSystem.Viewport);
            n.SetInputFunction(ViewPortInput);
            n.SetRenderFunction(ViewportRender);
            n.InitFullSize();
            n.InitOverlapped(true);
            n.IsMouseDragOutSideEvent = true;
            n.IsNoTransform = true;

            _guiViewport.ApplyViewportNode(n);
        })
        .OnContent(viewport =>
        {
            bool filtered = PartialGuiSync;
            var filteredNodes = PartialUpdatingNodes;

            // Currently controlled node, find the containing ImGraphNode
            if (GetOwnerGraphNode(viewport, gui.ControllingNode) is { } controllingOwner)
            {
                filtered = true;
                filteredNodes.Add(controllingOwner);
            }

            // Currently refreshing node, find the containing ImGraphNode
            if (GetOwnerGraphNode(viewport, gui.RefreshingNode) is { } refresingOwner)
            {
                filtered = true;
                filteredNodes.Add(refresingOwner);
            }

            if (filtered)
            {
                // Node filtered sync mode
                // In some cases, collection modifications may occur, requiring ToArray()
                _tempNodeList.Clear();
                _tempNodeList.AddRange(Diagram.NodeCollection.OfType<ImGraphNode>());

                try
                {
                    foreach (var node in _tempNodeList)
                    {
                        // Nodes to sync update || nodes not yet initialized
                        if (filteredNodes.Contains(node) || node.ImGuiNodeId is null)
                        {
                            // External request for partial update, may produce large changes
                            // Therefore double layout is needed to ensure layout correctness.
                            node.DoubleLayout();

                            node.OnGui(gui);
                            //Debug.WriteLine($"sync:{node.GuiId}");
                        }
                        else
                        {
                            // Bypass execution
                            gui.PassCurrentNode(node.ImGuiNodeId);
                            //Debug.WriteLine($"skip:{node.GuiId}");
                        }
                    }
                }
                finally
                {
                    _tempNodeList.Clear();
                }

                // Debug.WriteLine("OnNodeGui2: " + string.Join(", ", viewport.ChildNodes.Select(o => $"{o.Id} {o.Index}")));

                // Reset
                PartialGuiSync = false;
                filteredNodes.Clear();
            }
            else
            {
                // Full sync mode

                //foreach (var node in View.NodeCollection.OfType<ImGraphNode>())
                //{
                //    node.OnGui(gui);
                //}

                // In some cases, collection modifications may occur, requiring ToArray()
                _tempNodeList.Clear();
                _tempNodeList.AddRange(Diagram.NodeCollection.OfType<ImGraphNode>());

                try
                {
                    foreach (var node in _tempNodeList)
                    {
                        node.OnGui(gui);
                    }
                }
                finally
                {
                    _tempNodeList.Clear();
                }

                //Debug.WriteLine($"sync all, controlling={gui.ControllingNode?.FullPath}");
            }
        });

        // Resolve the ordering issue of buffered nodes
        _guiViewport.ApplyViewportNode(viewport);
        //_guiViewport.ApplyOrder();

        return viewport;
    }

    /// <summary>
    /// Finds the ImGraphNode that owns the specified ImGui node.
    /// </summary>
    /// <param name="viewportNode">The viewport node to search within.</param>
    /// <param name="guiNode">The ImGui node to find the owner for.</param>
    /// <returns>The owning ImGraphNode, or null if not found.</returns>
    private ImGraphNode? GetOwnerGraphNode(ImGuiNode viewportNode, ImGuiNode? guiNode)
    {
        if (guiNode is null)
        {
            return null;
        }

        while (guiNode?.Parent is { } parentNode)
        {
            if (parentNode == viewportNode && guiNode.GetValue<ImGraphNode>() is { } owner)
            {
                return owner;
            }

            guiNode = guiNode.Parent;
        }

        return null;
    }

    /// <summary>
    /// Handles input events for the viewport node.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The viewport ImGui node.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    private GuiInputState ViewPortInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (input.MouseLocation is Point pos)
        {
            _lastMousePos = pos;
        }

        ApplyScreenSize(node);

        if (PreGraphicInput(input))
        {
            OnPostGraphicInput(input);
            _viewport.Node?.MarkRenderDirty();

            baseAction(GuiPipeline.Blocked);
            return GuiInputState.None;
        }

        baseAction(pipeline, _guiViewport.RevertInBoundNodes);

        bool prehandled = input.Handled || node.Gui.ControllingNode != null;
        if (prehandled && input.EventType != GuiEventTypes.MouseDown && input.EventType != GuiEventTypes.MouseUp)
        {
            base.InputManager.SetIdle();
            return GuiInputState.None;
        }

        HandleGraphicInput(input);
        if (this.InputManager.EditMode != GraphEditMode.Idle)
        {
            _viewport.Node?.MarkRenderDirty();
        }

        OnPostGraphicInput(input);

        return GuiInputState.None;
    }

    /// <summary>
    /// Synchronizes viewport position data after input processing.
    /// </summary>
    /// <param name="input">The graphic input event.</param>
    protected void OnPostGraphicInput(IGraphicInput input)
    {
        if (input.EventType != GuiEventTypes.Timer && _viewport.Node is { } node)
        {
            // Sync view information
            var pos = _guiViewport.ViewportPosition;
            if (pos.X != Viewport.ViewX || pos.Y != Viewport.ViewY || _guiViewport.Zoom != Viewport.ViewZoom)
            {
                _guiViewport.ViewportPosition = new Point(Viewport.ViewX, Viewport.ViewY);
                _guiViewport.Zoom = Viewport.ViewZoom;
                // This is a computational bottleneck; the workload grows linearly with the number of nodes
                // Can be optimized using a quadtree
                _guiViewport.ApplyViewportNode(node);

                GraphicContext?.RequestOutput();
            }
        }
    }

    private readonly List<ImGuiNode> _tempGroups = [];
    private readonly List<ImGuiNode> _tempNodes = [];

    /// <summary>
    /// Renders the viewport, separating groups and nodes for correct layering.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The viewport ImGui node.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="baseAction">The base render function to call for child rendering.</param>
    private void ViewportRender(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        ApplyScreenSize(node);

        bool renderViewport = _viewport.Node?.IsRenderDirty == true || !dirtyMode;
        if (renderViewport)
        {
            base.Drawer.PreGraphicOutput(output);
        }

        _tempGroups.Clear();
        _tempNodes.Clear();

        _tempGroups.AddRange(_guiViewport.InBoundNodes.Where(o => o.GetValue<ImGraphNode>()?.IsGroup == true));
        _tempNodes.AddRange(_guiViewport.InBoundNodes.Where(o => !(o.GetValue<ImGraphNode>()?.IsGroup == true)));

        // Render groups
        baseAction(pipeline, _tempGroups);

        // Render connection lines
        // When groups are involved, connection lines must be forced to render to ensure correct group rendering
        if (renderViewport || _tempGroups.Count > 0)
        {
            base.Drawer.PreGraphicOutput2(output);
        }

        // Render nodes
        baseAction(pipeline, _tempNodes);

        //baseAction(pipeline, _guiViewport.InBoundNodes);

        _tempGroups.Clear();
        _tempNodes.Clear();

        if (renderViewport)
        {
            base.Drawer.PostGraphicOutput(output);
        }
    }

    /// <summary>
    /// Applies the viewport node's dimensions to the underlying Viewport.
    /// </summary>
    /// <param name="viewport">The viewport ImGui node.</param>
    private void ApplyScreenSize(ImGuiNode viewport)
    {
        var rect = viewport.Rect;
        var globalRect = viewport.GlobalRect;

        Viewport.GlobalViewRect = globalRect;
        Viewport.Width = rect.Width;
        Viewport.Height = rect.Height;
    }


    #endregion

    #region Factory

    /// <inheritdoc/>
    protected override GraphControlTheme CreateTheme()
    {
        var theme = new GraphControlTheme();
        theme.InitializeFonts(ImGuiTheme.DefaultFont);
        return theme;
    }

    /// <inheritdoc/>
    protected override GraphViewport CreateViewport() => new ImGuiGraphViewport(this);

    #endregion

    #region Virtual

    /// <inheritdoc/>
    protected override void OnNodeDeleted(GraphNode node)
    {
        base.OnNodeDeleted(node);

        PartialUpdatingNodes.Remove(node);
    }

    /// <inheritdoc/>
    protected override void OnDiagramCleared()
    {
        base.OnDiagramCleared();

        PartialUpdatingNodes.Clear();
    }

    /// <inheritdoc/>
    // Handle refresh input and output
    protected override void OnRefreshView(bool refreshAll)
    {
        if (!refreshAll)
        {
            // If partial update is indicated, only update to the node's outer layer to avoid internal node updates
            PartialGuiSync = true;
        }

        GraphicContext?.RequestRefreshInput(false);
        //GraphicContext?.RequestOutput();
        _viewport.Node?.MarkRenderDirty();
    }

    /// <inheritdoc/>
    // Handle partial node refresh
    protected override void OnRefreshNode(IEnumerable<GraphNode> nodes)
    {
        PartialGuiSync = true;
        PartialUpdatingNodes.AddRange(nodes.SkipNull());

        GraphicContext?.RequestRefreshInput(false);
        //GraphicContext?.RequestOutput();
        _viewport.Node?.MarkRenderDirty();
    }

    /// <inheritdoc/>
    // Resolve setting node order to front after rendering
    protected override void OnSelectionChanged(int nodeCount, int linkCount)
    {
        base.OnSelectionChanged(nodeCount, linkCount);

        if (Diagram.SelectedNodes.Count > 0 || Diagram.SelectedLinks.Count > 0)
        {
            //TODO: How to achieve PartialSync to improve rendering efficiency?

            // Re-sync to resolve node ordering issues
            BeginPartialGuiSync();
            base.GraphicContext?.RequestRefreshInput(false);
        }
    }

    /// <inheritdoc/>
    // Zoom in/out triggers cache clearing
    protected override void OnViewZoomed()
    {
        EditorUtility.AddDelayedAction(_clearCacheAction);
    }

    #endregion

    #region ClearCache

    /// <summary>
    /// Clears the cache for all nodes in the diagram.
    /// </summary>
    public void ClearCache()
    {
        foreach (var node in Diagram.NodeCollection.OfType<ImGraphNode>())
        {
            node.ClearCache();
        }

        _viewport.Node?.MarkRenderDirty();
    }


    /// <summary>
    /// Delayed action that clears the node cache when zooming is idle.
    /// </summary>
    class ClearCacheAction(ImGuiGraphControl value) : DelayedAction<ImGuiGraphControl>(value)
    {
        private readonly ImGuiGraphControl _panel = value;

        /// <inheritdoc/>
        public override void DoAction()
        {
            if (_panel.InputManager.EditMode == GraphEditMode.Idle)
            {
                _panel.ClearCache();

                // Set the ViewZoom flag to let nodes establish image cache
                _panel.InputManager.ViewZooming = true;
                _panel.RequestOutput();
            }
            else
            {
                EditorUtility.AddDelayedAction(this);
            }
        }
    }

    #endregion
}
