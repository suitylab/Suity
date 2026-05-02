using Suity.Views.NodeGraph;
using Suity.Editor.Services;
using Suity.Views.Graphics;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// ImGUI expandable node that supports expanding/collapsing to show additional content.
/// </summary>
public class ImExpandableNode : ImGraphNode, IDrawContext
{
    /// <summary>
    /// Gets or sets the default content scale factor for expanded view.
    /// </summary>
    public static float DefaultContentScale { get; set; } = 0.4f;

    /// <summary>
    /// Gets or sets a value indicating whether to render snapshots for performance optimization.
    /// </summary>
    public static bool RenderSnapshot { get; set; } = false;

    /// <summary>
    /// Default width for expanded content.
    /// </summary>
    public const int DefaultContentWidth = 470;

    /// <summary>
    /// Default height for expanded content.
    /// </summary>
    public const int DefaultContentHeight = 300;


    private readonly ImGuiNodeRef _nodeRef = new();
    private readonly ImGuiNodeRef _headerNodeRef = new();

    private GuiExpandableValue? _expandValue;
    private IDrawExpandedImGui? _expandedView;
    private readonly ImGuiNodeRef _expandedNode = new();


    /// <summary>
    /// Gets the expanded view instance.
    /// </summary>
    public IDrawExpandedImGui? ExpandedView => _expandedView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImExpandableNode"/> class.
    /// </summary>
    /// <param name="x">The X position of the node.</param>
    /// <param name="y">The Y position of the node.</param>
    /// <param name="p_View">The graph diagram that contains this node.</param>
    /// <param name="p_CanBeSelected">Indicates whether the node can be selected.</param>
    /// <param name="p_CanBeDeleted">Indicates whether the node can be deleted.</param>
    public ImExpandableNode(int x, int y, GraphDiagram p_View, bool p_CanBeSelected, bool p_CanBeDeleted)
        : base(x, y, p_View, p_CanBeSelected, p_CanBeDeleted)
    {
    }

    /// <summary>
    /// Gets the inspector context for this node.
    /// </summary>
    public virtual IInspectorContext? InspectorContext { get; }

    /// <summary>
    /// Gets a value indicating whether the mouse is inside the expanded area.
    /// </summary>
    public bool IsMouseInsideExpandedArea => _expandedNode.Node?.IsMouseInRect == true;

    #region GUI

    /// <inheritdoc/>
    protected override ImGuiNode OnNodeGui(ImGui gui)
    {
        if (_expandValue is null)
        {
            _expandValue = new GuiExpandableValue { Expanded = this.IsExpanded };
            var size = this.PreferredSize;
            _width = Math.Max(60, size.Width);
            _height = Math.Max(30, size.Height);
            UpdateHitRectangle();
        }

        // If expanded
        if (_expandValue?.Expanded == true)
        {
            // If content is resizable
            if (EnsureExpandedView(gui)?.ResizableOnExpand == true)
            {
                // Draw resizable node
                _nodeRef.Node = OnNodeGuiResize(gui);
            }
            else
            {
                // Draw fixed-size node
                _nodeRef.Node = OnNodeGuiFitted(gui);
            }
        }
        else
        {
            // Draw fixed-size node
            _nodeRef.Node = OnNodeGuiFitted(gui);
        }

        return _nodeRef.Node;
    }

    /// <summary>
    /// Draws the node with a fixed size (non-resizable).
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The rendered ImGui node.</returns>
    protected virtual ImGuiNode OnNodeGuiFitted(ImGui gui)
    {
        var editorGui = EditorGui;

        return NodeGraphExternalBK.Instance.FittedNodeFrame(gui, this)
        .OnContent(n =>
        {
            if (_connectors.HasControlOuputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Output, editorGui);
            }

            if (HasHeader)
            {
                OnHeaderGui(gui, editorGui, true);
            }

            editorGui?.Invoke(gui, EditorImGuiPipeline.Begin, this);

            if (_connectors.HasNormalConnector)
            {
                NodeGraphExternalBK.Instance.BodyFrame(gui, "body")
                .OnContent(() =>
                {
                    OnNormalConnectorsGui(gui, editorGui);
                });
            }

            if (Expandable && ExpandedObject is { } obj)
            {
                if (IsExpanded)
                {
                    var node = OnExpandedGui(gui, obj, true);
                    //if (_connectors.HasControlInputConnector)
                    //{
                    //    // Cannot use Adapt here because the final height is unknown
                    //    node?.InitHeightRest(10);
                    //}
                    //else
                    //{
                    //    node?.InitHeightRest(5);
                    //}

                    if (RenderSnapshot)
                    {
                        node?.InitRenderFunctionChain(SnapshotRender);
                    }

                    if (node != null)
                    {
                        gui.VerticalLayout("##footer")
                        .InitFullWidth()
                        .InitHeight(5);
                    }
                }
                else
                {
                    OnCollapsedGui(gui);
                }
            }

            editorGui?.Invoke(gui, EditorImGuiPipeline.End, this);

            if (_connectors.HasControlInputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Input, editorGui);
            }

        });
    }

    /// <summary>
    /// Draws the node with resizable dimensions.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The rendered ImGui node.</returns>
    protected virtual ImGuiNode OnNodeGuiResize(ImGui gui)
    {
        var editorGui = EditorGui;

        return NodeGraphExternalBK.Instance.ResizableNodeFrame(gui, this)
        .SetSize(this._width, this._height)
        .OnPartialContent(n =>
        {
            if (_connectors.HasControlOuputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Output, editorGui);
            }

            if (HasHeader)
            {
                OnHeaderGui(gui, editorGui, false);
            }

            editorGui?.Invoke(gui, EditorImGuiPipeline.Begin, this);

            if (_connectors.HasNormalConnector)
            {
                var connectorFrame = NodeGraphExternalBK.Instance.OverlayBodyFrame(gui, "body")
                .OnContent(() =>
                {
                    OnNormalConnectorsGui(gui, editorGui);
                });
            }

            if (Expandable && ExpandedObject is { } obj)
            {
                if (IsExpanded)
                {
                    var node = OnExpandedGui(gui, obj, true);
                    if (_connectors.HasControlInputConnector)
                    {
                    // Cannot use Adapt here because the final height is unknown
                        //node?.InitHeightRest(10);
                        node?.InitHeightAdapt();
                    }
                    else
                    {
                        node?.InitHeightRest(5);
                    }

                    if (RenderSnapshot)
                    {
                        node?.InitRenderFunctionChain(SnapshotRender);
                    }

                    if (node != null)
                    {
                        gui.VerticalLayout("##footer")
                        .InitFullWidth()
                        .InitHeight(5);
                    }

                    UpdatePreferredSize();
                }
                else
                {
                    OnCollapsedGui(gui);
                }
            }

            editorGui?.Invoke(gui, EditorImGuiPipeline.End, this);

            if (_connectors.HasControlInputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Input, editorGui);
            }
        }, false);
    }

    /// <summary>
    /// Draws the header section of the node.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="editorGui">Optional custom editor GUI drawing function.</param>
    /// <param name="fit">Indicates whether the header should use fitted sizing.</param>
    private void OnHeaderGui(ImGui gui, DrawEditorImGui? editorGui, bool fit)
    {
        _headerNodeRef.Node = NodeGraphExternalBK.Instance.HeaderFrame(gui, this, "header", fit)
        .OnContent(() =>
        {
            NodeGraphExternalBK.Instance.HorizontalBox(gui, "left", true)
            .OnContent(() =>
            {
                if (Expandable)
                {
                    gui.ExpandButton("expand", expandValue: _expandValue)
                    .InitClass("configBtn")
                    .InitInputFunctionChain(ExpandButtonInput)
                    .OnToggleExpand((n, v) =>
                    {
                        // Debug.WriteLine(v);
                        IsExpanded = v;
                    });
                }

                editorGui?.Invoke(gui, EditorImGuiPipeline.Prefix, this);
                NodeGraphExternalBK.Instance.TitleTextSection(gui, this);
            });

            NodeGraphExternalBK.Instance.HorizontalBox(gui, "right", true)
            .OnContent(() =>
            {
                editorGui?.Invoke(gui, EditorImGuiPipeline.Preview, this);
                NodeGraphExternalBK.Instance.TitlePreviewSection(gui, this);
            });
        });
    }

    /// <summary>
    /// Draws the normal (non-control) connectors for input and output.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="editorGui">Optional custom editor GUI drawing function.</param>
    private void OnNormalConnectorsGui(ImGui gui, DrawEditorImGui? editorGui)
    {
        NodeGraphExternalBK.Instance.VerticalBox(gui, "inputs", true)
        .OnContent(() =>
        {
            if ((editorGui?.Invoke(gui, EditorImGuiPipeline.Input, this)) != true)
            {
                var connectors = _connectors.Where(o =>
                    o.ConnectorType.GetIsNormalConnector()
                    && o.Direction == GraphDirection.Input);

                foreach (var connector in connectors)
                {
                    NodeGraphExternalBK.Instance.ConnectorRow(gui, connector, this, true);
                }
            }
        });

        NodeGraphExternalBK.Instance.VerticalBox(gui, "outputs", true)
        .OnContent(() =>
        {
            if ((editorGui?.Invoke(gui, EditorImGuiPipeline.Output, this)) != true)
            {
                var connectors = _connectors.Where(o =>
                    o.ConnectorType.GetIsNormalConnector()
                    && o.Direction == GraphDirection.Output);

                foreach (var connector in connectors)
                {
                    NodeGraphExternalBK.Instance.ConnectorRow(gui, connector, this, true);
                }
            }
        });
    }

    /// <summary>
    /// Draws the control connectors (execution flow) for the specified direction.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="direction">The direction of the connectors (Input or Output).</param>
    /// <param name="editorGui">Optional custom editor GUI drawing function.</param>
    private void OnControlConnectorGui(ImGui gui, GraphDirection direction, DrawEditorImGui? editorGui)
    {
        gui.HorizontalLayout("#control-" + direction.ToString())
        .OnInitialize(n =>
        {
            n.InitClass("debug_draw");
            n.InitFit();
            n.InitHorizontalAlignment(GuiAlignment.Center);
            n.InitPadding(1);
        })
        .OnContent(() =>
        {
            var pipeline = direction == GraphDirection.Input ? EditorImGuiPipeline.Input : EditorImGuiPipeline.Output;

            if (editorGui is null || !editorGui(gui, pipeline, this))
            {
                var connectors = _connectors.Where(o =>
                    o.ConnectorType == ConnectorType.Control &&
                    o.Direction == direction);

                foreach (var connector in connectors)
                {
                    NodeGraphExternalBK.Instance.ConnectorPoint(gui, connector, connector.Name);
                }
            }
        });
    }

    /// <summary>
    /// Draws the expanded content area when the node is expanded.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="obj">The object to display in the expanded view.</param>
    /// <param name="resizable">Indicates whether the expanded content is resizable.</param>
    /// <returns>The rendered ImGui node for the expanded content, or null.</returns>
    protected virtual ImGuiNode? OnExpandedGui(ImGui gui, object obj, bool resizable)
    {
        var view = EnsureExpandedView(gui);
        float scale = view?.ContentScale ?? DefaultContentScale;

        var node = _expandedNode.Node = view?.OnExpandedGui(gui)
        ?.OnInitialize(n =>
        {
            n.InitScale(scale);

            // If resizable, add a content blocker that requires selection before content can be operated
            // Otherwise, the content blocker will intercept all operations to avoid conflicts when dragging nodes, content being dragged, or any other operations
            if (resizable)
            {
                n.InitInputFunctionChain(ExpandContentBlockerInput);
            }
        });

        return node;
    }

    /// <summary>
    /// Handles the GUI when the node is collapsed.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    protected virtual void OnCollapsedGui(ImGui gui)
    {
        _expandedView?.ExitExpandedView();
        _expandedView = null;
        _expandedNode.Node = null;

        DisposeSnapshot();
    }



    /// <summary>
    /// Ensures the expanded view is created and initialized.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The expanded view instance, or null if not available.</returns>
    protected IDrawExpandedImGui? EnsureExpandedView(ImGui gui)
    {
        var obj = ExpandedObject;
        if (obj is null)
        {
            return null;
        }

        if (_expandedView is null)
        {
            _expandedView = CreateExpandedView();
            if (_expandedView is null)
            {
                return null;
            }

            _expandedView.EnterExpandedView(obj, InspectorContext);
            gui.QueueAction(() =>
            {
                // Need to update once more to display TreeView content
                Diagram.ParentControl.RefreshNode(this);
            });
        }

        return _expandedView;
    }

    /// <summary>
    /// Creates the expanded view instance.
    /// </summary>
    /// <returns>A new instance of <see cref="IDrawExpandedImGui"/> for the expanded content.</returns>
    protected virtual IDrawExpandedImGui CreateExpandedView() 
        => new ImSubPropertyGrid("Node", _expandedView?.ResizableOnExpand == true);

    #endregion

    #region Gui Func

    /// <summary>
    /// Input handler for the expand/collapse button.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    private GuiInputState ExpandButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state >= GuiInputState.PartialSync)
        {
            if (_expandValue?.Expanded == true)
            {
                // Initialize size at this position and set the minimum size value
                SetDefaultExpandedSize(node.Gui);

                EnsureExpandedView(node.Gui);
            }

            // Expand/collapse button has been pressed
            // Need to proactively request full refresh from Panel, otherwise DirtyRect will be incorrect due to outer frame size changes
            Diagram?.ParentControl?.RequestOutput();
        }

        return state;
    }

    /// <summary>
    /// Input handler that blocks interaction with expanded content when the node is not highlighted.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    private GuiInputState ExpandContentBlockerInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (!Highlighted)
        {
            baseAction(GuiPipeline.Blocked);
        }
        else
        {
            // Remember to call base
            baseAction(pipeline);
        }

        return GuiInputState.None;
    }


    private ISnapshot? _snapshot;
    private float _snapshotZoom = 0;

    /// <summary>
    /// Creates a snapshot of the specified rectangle for rendering optimization.
    /// </summary>
    /// <param name="output">The graphic output to capture the snapshot from.</param>
    /// <param name="rect">The rectangle area to snapshot.</param>
    private void CreateSnapshot(IGraphicOutput output, RectangleF rect)
    {
        _snapshot?.Dispose();
        _snapshot = output.Snapshot(rect);
        _snapshotZoom = Diagram.ParentControl.Viewport.ViewZoom;
    }

    /// <summary>
    /// Disposes the current snapshot and releases resources.
    /// </summary>
    private void DisposeSnapshot()
    {
        if (_snapshot != null)
        {
            _snapshot.Dispose();
            _snapshot = null;
        }
    }

    /// <summary>
    /// Render function that uses image snapshots for performance optimization during certain operations.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="baseAction">The base render function to call for child rendering.</param>
    private void SnapshotRender(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        if (!RenderSnapshot)
        {
            DisposeSnapshot();
            return;
        }

        //Debug.WriteLine($"SnapshotRender");

        // During partial refresh, do not draw snapshot and clear snapshot
        if (!output.RepaintAll && Diagram.ParentControl.InputManager.EditMode == GraphEditMode.Idle)
        {
            //if (node.NeedRender)
            //{
            DisposeSnapshot();
            //}

            //Debug.WriteLine($"Not repaint all");
            return;
        }

        // If the node is in hover state, meaning it has visual focus, do not draw snapshot
        if (node.IsMouseIn)
        {
            DisposeSnapshot();
            return;
        }

        var rect = node.GlobalRect;
        var panel = Diagram.ParentControl;
        var selectedItems = panel.Diagram.SelectedNodes;
        var onlySelectedMe = selectedItems.Count == 1 && selectedItems[0] == this; // Whether the current item is the only selected one
        bool optimizing;

        // When only one is selected, do not optimize unless it's a Pan operation
        if (onlySelectedMe && panel.InputManager.EditMode != GraphEditMode.Pan)
        {
            optimizing = false;
            DisposeSnapshot();
        }
        else
        {
            // Optimize based on edit mode
            optimizing = panel.InputManager.EditMode switch
            {
                GraphEditMode.Pressing or
                GraphEditMode.Pan or
                GraphEditMode.Zooming or
                GraphEditMode.Selecting or
                GraphEditMode.SelectingBox or
                GraphEditMode.MovingSelection or
                GraphEditMode.Linking => true,
                _ => false,
            };
        }

        // If optimization is needed in the current operation mode, or in Zoom mode, perform snapshot rendering
        if (optimizing || panel.InputManager.ViewZooming)
        {
            if (_snapshot != null)
            {
                // If snapshot already exists, draw it directly
                baseAction(GuiPipeline.Blocked);
                output.DrawSnapshot(_snapshot, rect);
            }
            else
            {
                // Call base draw method to get draw result
                baseAction(pipeline);

                // If draw result is within current viewport, generate snapshot
                if (rect.X >= 0 && rect.Y >= 0 && rect.Right <= output.Width && rect.Bottom <= output.Height)
                {
                    if (!onlySelectedMe)
                    {
                        // When not uniquely selected, draw snapshot
                        CreateSnapshot(output, rect);
                    }
                    //Debug.WriteLine($"Snapshot updated");
                }
            }
        }
        else // No optimization needed
        {
            if (node.NeedRender)
            {
                // When node needs redraw, clear snapshot
                DisposeSnapshot();
                baseAction(pipeline);
            }
            else if (_snapshot != null)
            {
                // When node does not need redraw, if snapshot exists, draw it directly
                baseAction(GuiPipeline.Blocked);
                output.DrawSnapshot(_snapshot, rect);
            }

            //Debug.WriteLine($"other");
        }

        //Debug.WriteLine($"InnerRender : {node.GlobalScale}");
    }


    #endregion

    #region Expand & Resize

    /// <summary>
    /// Sets the default size for the expanded content based on the content scale.
    /// </summary>
    /// <param name="gui">The ImGui instance for getting the content scale.</param>
    private void SetDefaultExpandedSize(ImGui gui)
    {
        float scale = EnsureExpandedView(gui)?.ContentScale ?? DefaultContentScale;

        var size = this.PreferredSize;
        _width = Math.Max((int)(DefaultContentWidth * scale), size.Width);
        _height = Math.Max((int)(DefaultContentHeight * scale), size.Height);
        UpdateHitRectangle();
    }

    /// <inheritdoc/>
    /// <summary>
    /// Gets a value indicating whether the node is resizable. Depends on being expanded and having resizable content.
    /// </summary>
    public override bool Resizable => IsExpanded && _expandedView?.ResizableOnExpand == true;

    /// <summary>
    /// Gets a value indicating whether this node can be expanded.
    /// </summary>
    public virtual bool Expandable => true;

    /// <summary>
    /// Gets the object displayed after expansion (default is FlowNode).
    /// </summary>
    public virtual object? ExpandedObject { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the node is currently expanded.
    /// </summary>
    public virtual bool IsExpanded { get; set; }

    /// <summary>
    /// Sets the expand state of the node.
    /// </summary>
    /// <param name="value">The new expand state.</param>
    public virtual void SetExpand(bool value)
    {
        IsExpanded = value;

        if (_nodeRef.Node is { } node && _expandValue is { } expandValue)
        {
            expandValue.Expanded = value;

            if (value)
            {
                // Initialize size at this position and set the minimum size value
                SetDefaultExpandedSize(node.Gui);

                EnsureExpandedView(node.Gui);
            }

            node.QueueRefresh();
        }
    }

    /// <inheritdoc/>
    protected override void OnSizeUpdated()
    {
        base.OnSizeUpdated();

        if (IsExpanded)
        {
            // After size is updated, save the expanded size
            UpdatePreferredSize();
        }
    }

    /// <summary>
    /// Gets the cached preferred size after expansion.
    /// </summary>
    public virtual Size PreferredSize { get; }

    /// <summary>
    /// Updates the cached preferred size of the expanded content.
    /// </summary>
    public virtual void UpdatePreferredSize() { }


    /// <summary>
    /// Updates the expanded content, mainly used in tree structures. When a node expands, the expanded content needs to be updated.
    /// </summary>
    public void UpdateExpandedObject()
    {
        _expandedView?.UpdateExpandedTarget();
    }

    #endregion

    /// <inheritdoc/>
    public override RectangleF? GetHeaderArea()
    {
        // When node is expanded and content is visible, return Header area; when node content is not visible, return the entire node area
        if (IsExpanded && _headerNodeRef.Node is { } node && node.GlobalScale >= ImGraphExtensions.NodeHiddenScale)
        {
            return node.GlobalRect;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    protected override void OnHighlightedChanged()
    {
        base.OnHighlightedChanged();

        // If highlight state changes, clear expanded content
        _expandedView?.ClearSelection();
    }

    /// <inheritdoc/>
    internal protected override void OnMarkDeleted()
    {
        // If marked for deletion, clear expanded content
        if (_expandedView != null)
        {
            _expandedView.ExitExpandedView();
            _expandedView = null;
        }

        // If marked for deletion, clear cache
        DisposeSnapshot();
    }

    /// <inheritdoc/>
    public override void ClearCache()
    {
        // If clearing cache, remove image snapshot
        if (_snapshot != null)
        {
            // Check the difference between the Zoom when the cache was saved and the current Zoom; if the difference is within a certain range, do not delete
            float d = _snapshotZoom / Diagram.ParentControl.Viewport.ViewZoom;
            if (d < 0.5f || d > 2.0f)
            {
                DisposeSnapshot();
            }
        }
    }

}
