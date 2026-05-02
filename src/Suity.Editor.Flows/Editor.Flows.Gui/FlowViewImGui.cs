using Suity;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows.Gui.Actions;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.Named;
using Suity.Views.NodeGraph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Abstract base class for ImGui-based flow diagram views.
/// Provides node graph visualization, selection management, clipboard operations,
/// drag-and-drop support, and integration with the editor's inspection and undo/redo systems.
/// </summary>
public abstract class FlowViewImGui :
    IFlowView,
    IObjectView,
    IInspectorContext,
    IViewClipboard,
    IViewSelectable,
    IViewSelectionInfo,
    IGraphDataTypeProvider,
    IDropTarget
{
    /// <summary>
    /// Default font family used for rendering node titles, connectors, and preview text.
    /// </summary>
    static readonly FontFamily DEFAULT_FONT = ImGuiTheme.DefaultFont; //SystemFonts.DefaultFont.FontFamily;

    private static readonly Dictionary<string, RootFlowMenuCommand> _menus = [];

    private ImGuiGraphControl _graphControl;
    private FlowDocument _document;


    private readonly List<GraphNode> _nodeSelectionBefore = [];

    private readonly List<GraphLink> _linkSelectionBefore = [];

    private readonly ComputeDataAction _computeDataAction;

    private RootFlowMenuCommand _contextMenuCommand;

    private readonly GuiExpandBackupState _expandState = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="FlowViewImGui"/> class.
    /// Sets up the graph panel and compute data action.
    /// </summary>
    public FlowViewImGui()
    {
        InitializeGraphPanel();

        //nodeGraphPanel1.EnableDrawDebug = true;
        _computeDataAction = new ComputeDataAction(this);
    }

    
    /// <summary>
    /// Gets or sets the flow document associated with this view.
    /// Setting this property initializes the diagram, registers event handlers,
    /// and populates the graph panel with nodes and links.
    /// </summary>
    public FlowDocument Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(value, _document))
            {
                return;
            }

            if (_document?.Diagram is { } oldDiagram)
            {
                oldDiagram.NodeAddedOrUpdated -= Diagram_NodeAddedOrUpdated;
                oldDiagram.NodeRemoved -= Diagram_NodeRemoved;
                oldDiagram.ConnectorRenamed -= Diagram_ConnectorRenamed;
                oldDiagram.LinkAdded -= NewDiagram_LinkAdded;
                oldDiagram.LinkRemoved -= Diagram_LinkRemoved;

                oldDiagram.StopView(this);
            }

            _graphControl.DeleteAll();

            _document = value;
            if (_document != null)
            {
                if (_document.Diagram is { } diagram)
                {
                    diagram.NodeAddedOrUpdated += Diagram_NodeAddedOrUpdated;
                    diagram.NodeRemoved += Diagram_NodeRemoved;
                    diagram.ConnectorRenamed += Diagram_ConnectorRenamed;
                    diagram.LinkAdded += NewDiagram_LinkAdded;
                    diagram.LinkRemoved += Diagram_LinkRemoved;

                    diagram.StartView(this);

                    foreach (var node in diagram.Nodes)
                    {
                        AddOrUpdateNode(node);
                    }

                    foreach (var link in diagram.Links)
                    {
                        AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
                    }
                }

                string formatName = _document.Entry?.Format?.FormatName;
                if (!string.IsNullOrWhiteSpace(formatName))
                {
                    _contextMenuCommand = _menus.GetOrAdd("#" + formatName, n => new(n));
                    EditorUtility.PrepareMenu(_contextMenuCommand);
                }

                QueueComputeData();
            }

            OnRebuild();
        }
    }

    /// <summary>
    /// Gets the graph control associated with this view.
    /// </summary>
    public ImGuiGraphControl GraphControl => _graphControl;

    /// <summary>
    /// Called when the view is rebuilt. Override this method to perform custom rebuild logic.
    /// </summary>
    protected virtual void OnRebuild()
    {

    }

    /// <summary>
    /// Initializes the graph panel with default styling, colors, and event handlers.
    /// </summary>
    private void InitializeGraphPanel()
    {
        this._graphControl = new()
        {
            OwnerFlowView = this,
        };

        this._graphControl.Drawer.GridAlpha = 16;
        this._graphControl.Drawer.GridPadding = 100;
        this._graphControl.Drawer.ShowGrid = true;

        var theme = _graphControl.Theme;
        theme.BackColor = EditorColorScheme.Default.Background;
        theme.ConnectorFillColor = Color.FromArgb(0, 0, 0);
        theme.ConnectorSelectedFillColor = Color.FromArgb(32, 32, 32);
        theme.ConnectorOutlineColor = Color.FromArgb(32, 32, 32);
        theme.ConnectorOutlineSelectedColor = Color.FromArgb(64, 64, 64);
        theme.ConnectorTextColor = Color.White;
        theme.LinkColor = Color.FromArgb(180, 180, 180);
        theme.LinkEditableColor = Color.FromArgb(80, 64, 255, 0);
        theme.NodeConnectorFont = new System.Drawing.Font(DEFAULT_FONT, 8F);
        theme.NodeFillColor = Color.FromArgb(80, 80, 80);
        theme.NodeFillSelectedColor = Color.FromArgb(80, 80, 80);
        theme.NodeHeaderColor = Color.FromArgb(50, 0, 0, 0);
        theme.NodeOutlineColor = Color.FromArgb(0, 0, 0, 0);
        theme.NodeOutlineSelectedColor = Color.FromArgb(0, 142, 255);
        theme.NodePreviewFont = new System.Drawing.Font(DEFAULT_FONT, 12F);
        theme.NodeScaledConnectorFont = new System.Drawing.Font(DEFAULT_FONT, 8F);
        theme.NodeScaledPreviewFont = new System.Drawing.Font(DEFAULT_FONT, 12F);
        theme.NodeScaledTitleFont = new System.Drawing.Font(DEFAULT_FONT, 8F);
        theme.NodeSignalInvalidColor = Color.FromArgb(255, 0, 0);
        theme.NodeSignalValidColor = Color.FromArgb(0, 255, 0);
        theme.NodeTextColor = Color.FromArgb(255, 255, 255);
        theme.NodeTextShadowColor = Color.FromArgb(128, 0, 0, 0);
        theme.NodeTitleFont = new System.Drawing.Font(DEFAULT_FONT, 8F);
        theme.SelectionFillColor = Color.FromArgb(64, 0, 142, 255);
        theme.SelectionOutlineColor = Color.FromArgb(192, 0, 142, 255);

        theme.UpdateBrushesAndPens();

        this._graphControl.NodeCreateRequesting += GraphPanel_NodeCreateRequesting;
        this._graphControl.GroupCreateRequesting += GraphPanel_GroupCreateRequesting;
        this._graphControl.SelectionChanging += GraphPanel_SelectionChanging;
        this._graphControl.SelectionChanged += GraphPanel_SelectionChanged;
        this._graphControl.SelectionDeleting += GraphPanel_SelectionDeleting;
        this._graphControl.SelectionDeleted += GraphPanel_SelectionDeleted;
        this._graphControl.SelectionMoved += GraphPanel_SelectionMoved;
        this._graphControl.SelectionResized += GraphPanel_SelectionResized;
        this._graphControl.LinkCreated += GraphPanel_LinkCreated;
        this._graphControl.LinkDestroyed += GraphPanel_LinkDestroyed;
        this._graphControl.ContextMenuShowing += GraphPanel_ContextMenuShowing;

        this._graphControl.Diagram.DataTypeProvider = this;
    }


    #region Diagram Events

    /// <inheritdoc/>
    private void Diagram_NodeAddedOrUpdated(object sender, FlowNode e)
    {
        AddOrUpdateNode(e);

        _graphControl.RefreshView();
    }

    /// <summary>
    /// Handles removal of a node from the diagram.
    /// </summary>
    private void Diagram_NodeRemoved(object sender, FlowNode e)
    {
        RemoveNode(e);

        _graphControl.RefreshView();
    }

    /// <inheritdoc/>
    private void Diagram_ConnectorRenamed(object sender, ConnectorRenamedEventArgs e)
    {
        RenameConnector(e.Node, e.OldName, e.NewName);

        _graphControl.RefreshView();
    }

    /// <inheritdoc/>
    private void NewDiagram_LinkAdded(object sender, NodeLink e)
    {
        AddLink(e.FromNode, e.FromConnector, e.ToNode, e.ToConnector);

        _graphControl.RefreshView();
    }

    private void Diagram_LinkRemoved(object sender, NodeLink e)
    {
        RemoveLink(e.FromNode, e.FromConnector, e.ToNode, e.ToConnector);

        _graphControl.RefreshView();
    } 

    #endregion



    /// <summary>
    /// Gets or sets the grid span size for snapping nodes.
    /// </summary>
    public int GridSpan
    {
        get => _graphControl.Drawer.GridPadding;
        set => _graphControl.Drawer.GridPadding = value;
    }

    /// <summary>
    /// Gets the collection of currently selected flow nodes in the view.
    /// </summary>
    public IEnumerable<FlowNode> SelectedNodes =>
        this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Select(o => o.Node)
            .SkipNull();

    /// <summary>
    /// Rebuilds the entire view from the document data.
    /// </summary>
    public void RebuildView()
    {
        var diagram = _document?.Diagram;
        if (diagram is null)
        {
            return;
        }

        foreach (var node in _graphControl.Diagram.NodeCollection.OfType<ImGraphNode>())
        {
            // Clear ImGuiNode and force rebuild.
            node.GetImGuiNode()?.MarkDeleted = true;
        }

        _graphControl.DeleteAll();
        OnRebuild();

        List<GraphNode> viewNodes = [];

        foreach (var node in diagram.Nodes)
        {
            // Remove old view node by passing self.
            node.StopView(this);

            var viewNode = CreateViewNode(node);
            this._graphControl.AddNode(viewNode);

            viewNodes.Add(viewNode);
        }

        foreach (var link in diagram.Links.ToArray())
        {
            this._graphControl.LinkManager.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        //foreach (var viewNode in viewNodes)
        //{
        //    this._graphPanel.UpdateAssociate(viewNode);
        //}
    }

    /// <summary>
    /// Queues a delayed computation of node data for preview values.
    /// </summary>
    public void QueueComputeData()
    {
        if (_document is null)
        {
            return;
        }

        if (!_document.PreviewComputeEnabled)
        {
            return;
        }

        EditorUtility.AddDelayedAction(_computeDataAction);
    }

    /// <inheritdoc/>
    public void QueueAnalysis()
    {
        EditorUtility.AddDelayedAction(new UpdateAnalysisAction(this));
    }

    /// <summary>
    /// Gets the underlying ImGui graph control.
    /// </summary>
    protected ImGuiGraphControl GraphPanel => _graphControl;

    #region IFlowView

    /// <inheritdoc/>
    public IFlowDiagram Diagram => _document?.Diagram;

    /// <inheritdoc/>
    public virtual object UIObject => this._graphControl;

    /// <summary>
    /// Gets or sets the computation engine for the flow diagram.
    /// </summary>
    public IFlowComputation Computation { get; set; }

    /// <summary>
    /// Gets the last known mouse position in view coordinates.
    /// </summary>
    public Point LastMousePosition => _graphControl.Viewport.ControlToView(_graphControl.LastMousePos);

    /// <summary>
    /// Sets the selection to the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to select.</param>
    public void SetSelection(IEnumerable<FlowNode> nodes)
    {
        var viewNodes = nodes.Select(o => o.GetViewNode(this)).OfType<GraphNode>();
        this._graphControl.SetNodeSelection(viewNodes);
        InspectSelection();
    }

    /// <summary>
    /// Inspects the currently selected nodes in the inspector panel.
    /// </summary>
    public void InspectSelection()
    {
        var nodes = this._graphControl.Diagram.SelectedNodes.OfType<IFlowViewNode>().Select(o => o.Node);

        if (nodes.Any())
        {
            bool handled = (_document as SNamedDocument)?.HandleInspect(nodes, this) == true;
            if (!handled)
            {
                EditorUtility.Inspector.InspectObjects(nodes, this);
                //EditorUtility.Inspector.InspectObjects(nodes, InspectorTreeModes.DetailTree, false, this);
            }
        }
        else
        {
            EditorUtility.Inspector.InspectObject(_document, this);
        }
    }

    /// <summary>
    /// Adds a new node to the view or updates an existing one.
    /// </summary>
    /// <param name="node">The flow node to add or update.</param>
    public void AddOrUpdateNode(FlowNode node)
    {
        if (node.GetViewNode(this) is { } viewNode)
        {
            viewNode.RebuildNode(o => 
            {
                OnFlowDoAction(new DeleteLinkAction(this, o));
            });

            if (viewNode is ImFlowViewNode imViewNode)
            {
                imViewNode.UpdateExpandedObject();
            }

            if (viewNode is GraphNode graphNode)
            {
                _graphControl.RefreshNode(graphNode);
            }
        }
        else
        {
            var graphNode = CreateViewNode(node); // Construction includes Rebuild process.
            _graphControl.AddNode(graphNode);
        }

        QueueComputeData();
    }

    /// <inheritdoc/>
    public void RemoveNode(FlowNode node)
    {
        if (node.GetViewNode(this) is ImGraphNode viewNode)
        {
            _graphControl.DeleteNode(viewNode);
        }

        node.StopView(this);

        QueueComputeData();
    }

    /// <inheritdoc/>
    public void RenameConnector(FlowNode node, string oldName, string newName)
    {
        if (node.GetViewNode(this) is ImGraphNode viewNode)
        {
            viewNode.RenameConnector(oldName, newName);
        }
    }

    /// <inheritdoc/>
    public void AddLink(string fromNodeName, string fromConnectorName, string toNodeName, string toConnectorName)
    {
        bool added = _graphControl.LinkManager.AddLink(fromNodeName, fromConnectorName, toNodeName, toConnectorName);
        // Try to add in the next frame.
        if (!added)
        {
            QueuedAction.Do(() => _graphControl.LinkManager.AddLink(fromNodeName, fromConnectorName, toNodeName, toConnectorName));
        }

        QueueComputeData();
    }

    /// <inheritdoc/>
    public void RemoveLink(string fromNodeName, string fromConnectorName, string toNodeName, string toConnectorName)
    {
        _graphControl.LinkManager.DeleteLink(fromNodeName, fromConnectorName, toNodeName, toConnectorName);

        QueueComputeData();
    }

    /// <inheritdoc/>
    public void SetSelection(FlowNode node)
    {
        if (node?.GetViewNode(this) is not GraphNode viewNode)
        {
            return;
        }

        _nodeSelectionBefore.Clear();
        _nodeSelectionBefore.AddRange(this._graphControl.Diagram.SelectedNodes);
        OnFlowDoAction(new NodeSmartSelectionAction(this, _nodeSelectionBefore, [viewNode]));
        _nodeSelectionBefore.Clear();

        this._graphControl.Viewport.FocusSelection();
    }

    /// <inheritdoc/>
    public void SetSelections(IEnumerable<FlowNode> nodes)
    {
        var viewNodes = nodes.Select(o => o?.GetViewNode(this)).OfType<GraphNode>();

        _nodeSelectionBefore.Clear();
        _nodeSelectionBefore.AddRange(this._graphControl.Diagram.SelectedNodes);
        OnFlowDoAction(new NodeSmartSelectionAction(this, _nodeSelectionBefore, viewNodes));
        _nodeSelectionBefore.Clear();

        this._graphControl.Viewport.FocusSelection();
    }


    //public bool IsDisposed { get; }
    /// <inheritdoc/>
    public virtual void RefreshView() => _graphControl.RefreshView(true);

    /// <summary>
    /// Refreshes the specified view nodes in the diagram.
    /// </summary>
    /// <param name="nodes">The view nodes to refresh.</param>
    public void RefreshNodes(IEnumerable<IFlowViewNode> nodes)
    {
        if (nodes is null)
        {
            return;
        }

        var view = _graphControl.Diagram;

        var graphNodes = nodes
            .OfType<GraphNode>()
            .Where(o => o.Diagram == view)
            .ToArray();

        _graphControl.RefreshNode(graphNodes);
    }

    /// <inheritdoc/>
    public IFlowViewNode GetViewNode(string name)
    {
        return _graphControl.Diagram.FindNode(name) as IFlowViewNode;
    }

    #endregion

    #region IObjectView

    /// <inheritdoc/>
    public object TargetObject => _document;

    #endregion

    #region IServiceProvider

    /// <inheritdoc/>
    public virtual object GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        if (_document?.GetService(serviceType) is object obj)
        {
            return obj;
        }

        if (_document != null && serviceType.IsInstanceOfType(_document))
        {
            return _document;
        }

        return null;
    }

    #endregion

    #region IInspectorContext

    /// <inheritdoc/>
    public virtual void InspectorEnter()
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorExit()
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorBeginMacro(string name)
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorEndMarco(string name)
    {
    }

    /// <inheritdoc/>
    public virtual bool InspectorDoAction(UndoRedoAction action)
    {
        action.Do();
        return true;
    }

    /// <inheritdoc/>
    public virtual void InspectorEditFinish()
    {
        RefreshView();
    }

    /// <inheritdoc/>
    public virtual void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        // Should not Rebuild here.
        /**
        bool rebuild = false;

        foreach (var viewNode in nodeGraphPanel1.View.SelectedItems.OfType<IFlowViewNode>())
        {
            viewNode.Rebuild(o => _undoManager.Do(new RemoveLinkAction(this, o)));
            rebuild = true;
        }

        if (rebuild)
        {
            //GraphicContext?.RequestOutput();
            // Due to the new ImGui introduction, executing Invalidate() will perform ImGui synchronization and also execute RequestOutput
            nodeGraphPanel1?.Invalidate();

            QueueComputeData();
        }
        **/

        QueueComputeData();

        OnDirty();

        QueuedAction.Do(() =>
        {
            (_document as IViewElementEditNotify)?.NotifyViewElementEdited(objs);
        });

        // Update selected nodes.
        _graphControl.RefreshNode(_graphControl.Diagram.SelectedNodes);
    }

    /// <inheritdoc/>
    public virtual object InspectorUserData { get; set; }

    #endregion

    #region FlowContext

    /// <inheritdoc/>
    protected virtual void OnFlowBeginMacro(string name)
    {
    }

    /// <summary>
    /// Ends a macro (grouped undo/redo action) for flow operations.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    protected virtual void OnFlowEndMacro(string name)
    {
    }

    /// <inheritdoc/>
    protected virtual bool OnFlowDoAction(UndoRedoAction action)
    {
        action.Do();
        return true;
    }

    /// <inheritdoc/>
    protected virtual void OnFlowEditFinish()
    {
    }

    #endregion

    #region IViewClipboard

    /// <inheritdoc/>
    public void ClipboardCopy()
    {
        // If a single node is selected, expanded, and supports clipboard operations, pass the operation to the node.
        if (_graphControl.Diagram.SelectedNodes.OnlyOneOfDefault() is ImExpandableNode node
            && node.IsMouseInsideExpandedArea
            && node.ExpandedView is IViewClipboard clipboard)
        {
            clipboard.ClipboardCopy();
            return;
        }

        var selected = this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Where(o => o.CanBeDeleted)
            .Select(o => o.Node?.DiagramItem)
            .SkipNull();

        if (!selected.Any())
        {
            return;
        }

        var items = selected.ToList();
        List<NodeLink> links = [];
        Document.Links.CollectLinks(items.Select(o => o.Name), links);

        var data = new FlowViewClipboardData
        {
            Items = items,
            Links = links,
        };

        EditorServices.ClipboardService.SetData(data, true);
    }

    /// <inheritdoc/>
    public void ClipboardCut()
    {
        // If a single node is selected, expanded, and supports clipboard operations, pass the operation to the node.
        if (_graphControl.Diagram.SelectedNodes.OnlyOneOfDefault() is ImExpandableNode node
            && node.IsMouseInsideExpandedArea
            && node.ExpandedView is IViewClipboard clipboard)
        {
            clipboard.ClipboardCut();
            return;
        }

        var selected = this._graphControl.Diagram.SelectedNodes.OfType<IFlowViewNode>().Where(o => o.CanBeDeleted);
        if (!selected.Any())
        {
            return;
        }

        var sel = selected.Where(o => o.CanBeDeleted).Select(o => o.Node.DiagramItem);
        List<IFlowDiagramItem> items = [.. sel];
        List<NodeLink> links = [];
        Document.Links.CollectLinks(items.Select(o => o.Name), links);

        var data = new FlowViewClipboardData
        {
            Items = items,
            Links = links,
        };

        OnFlowBeginMacro("Delete Node");
        OnFlowDoAction(new DeleteNodeAction(this, items.Select(o => o.Node)));
        OnFlowEndMacro("Delete Node");
        OnDirty();

        EditorServices.ClipboardService.SetData(data, false);
    }

    private static readonly Dictionary<string, string> _pasteNameDic = [];

    /// <inheritdoc/>
    public void ClipboardPaste()
    {
        // If a single node is selected, expanded, and supports clipboard operations, pass the operation to the node.
        if (_graphControl.Diagram.SelectedNodes.OnlyOneOfDefault() is ImExpandableNode node
            && node.IsMouseInsideExpandedArea
            && node.ExpandedView is IViewClipboard clipboard)
        {
            clipboard.ClipboardPaste();
            return;
        }

        bool isCopy = EditorServices.ClipboardService.IsCopy;

        var data = EditorServices.ClipboardService.GetDatas()
            .Select(o => o.Data)
            .FirstOrDefault() as FlowViewClipboardData;

        if (data is null)
        {
            return;
        }

        // Unknown reason - ClipboardService's auto-copy mechanism is invalid, need to copy again.
        HandleCloneNodes(data.Items, data.Links, isCopy);
    }

    #endregion

    #region IViewSelectable

    /// <inheritdoc/>
    public ViewSelection GetSelection()
    {
        List<IFlowDiagramItem> list = this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Select(o => o.Node?.DiagramItem)
            .SkipNull()
            .ToList();

        return new ViewSelection(list);
    }

    /// <inheritdoc/>
    public bool SetSelection(ViewSelection selection)
    {
        switch (selection.Selection)
        {
            case IFlowDiagramItem diagramItem:
                SetSelection(diagramItem.Node);
                return true;

            case List<IFlowDiagramItem> list:
                SetSelections(list.Select(o => o.Node));
                return true;

            case FlowNode node:
                SetSelection(node);
                return true;

            case SyncPath path:
                return SetSelection(path);

            case string name:
                return SetSelection(name);

            default:
                return false;
        }
    }

    #endregion

    #region IViewSelectionInfo

    /// <inheritdoc/>
    public IEnumerable<object> SelectedObjects =>
        this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Select(o => o.Node?.DiagramItem)
            .SkipNull();

    /// <inheritdoc/>
    public IEnumerable<T> FindSelectionOrParent<T>(bool distinct = true) where T : class
    {
        if (distinct)
        {
            return this._graphControl.Diagram.SelectedNodes.OfType<IFlowViewNode>()
                .Select(o => o.Node?.DiagramItem)
                .OfType<T>();
        }
        else
        {
            return this._graphControl.Diagram.SelectedNodes.OfType<IFlowViewNode>()
                .Select(o => o.Node?.DiagramItem)
                .As<T>();
        }
    }

    #endregion

    #region INodeGraphDataTypeProvider

    readonly Dictionary<string, GraphDataType> _cachedDataTypes = [];

    /// <inheritdoc/>
    public GraphDataType ActionDataType => ActionNodeGraphDataType.Instance;

    /// <inheritdoc/>
    public GraphDataType GetDataType(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return UnknownNodeGraphDataType.Instance;
        }

        if (_cachedDataTypes.TryGetValue(name, out var dataType))
        {
            return dataType;
        }

        switch (name)
        {
            case FlowNode.ACTION_TYPE:
                return ActionNodeGraphDataType.Instance;

            case FlowNode.EVENT_TYPE:
                return EventNodeGraphDataType.Instance;

            case FlowNode.UNKNOWN_TYPE:
                return UnknownNodeGraphDataType.Instance;
        }

        var dataStyle = _document?.GetDataStyle(name);

        dataType = dataStyle != null ?
            FlowTypeManager.Instance.GetDataType(dataStyle) : FlowTypeManager.Instance.GetDataType(name);

        if (dataType != null)
        {
            _cachedDataTypes[name] = dataType;
            return dataType;
        }
        else
        {
            _cachedDataTypes[name] = UnknownNodeGraphDataType.Instance;
            return UnknownNodeGraphDataType.Instance;
        }
    }

    /// <inheritdoc/>
    public bool RevertDataArray => _document?.RevertDataArray == true;

    /// <inheritdoc/>
    public bool GetCanConnectTo(GraphDataType fromDataType, GraphDataType toDataType, bool toMultiple, out bool converted)
    {
        converted = false;

        if (fromDataType is null || toDataType is null)
        {
            return false;
        }

        if (fromDataType == toDataType)
        {
            return true;
        }

        bool reverseFlow = _document?.ReverseDataFlow ?? false;

        if (fromDataType is TypeDefinitionDataType fromDef && toDataType is TypeDefinitionDataType toDef)
        {
            if (TypeDefinition.IsNullOrBroken(fromDef.TypeDef) || TypeDefinition.IsNullOrBroken(toDef.TypeDef))
            {
                return false;
            }

            if (fromDef == toDef)
            {
                return true;
            }

            if (reverseFlow)
            {
                if (fromDef.TypeDef.IsAssignableFrom(toDef.TypeDef))
                {
                    return true;
                }

                var state = EditorServices.TypeConvertService.CanConvert(toDef.TypeDef, fromDef.TypeDef, toMultiple);
                if (state != TypeConvertState.Unconvertible)
                {
                    converted = state == TypeConvertState.Convertible;
                    return true;
                }
            }
            else
            {
                if (toDef.TypeDef.IsAssignableFrom(fromDef.TypeDef))
                {
                    return true;
                }

                var state = EditorServices.TypeConvertService.CanConvert(fromDef.TypeDef, toDef.TypeDef, toMultiple);
                if (state != TypeConvertState.Unconvertible)
                {
                    converted = state == TypeConvertState.Convertible;
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool GetCanAssociate(GraphDataType fromDataType, object fromValue, GraphDataType toDataType, object toValue)
    {
        if (fromDataType is null || toDataType is null)
        {
            return false;
        }

        if (fromDataType != toDataType)
        {
            return false;
        }

        if (fromValue is null || toValue is null)
        {
            return false;
        }

        if (fromValue is IHasAsset fromContext && toValue is IHasAsset toContext)
        {
            return fromContext.TargetAsset == toContext.TargetAsset;
        }

        if (fromValue is SItem fromItem && toValue is SItem toItem)
        {
            return SItem.ValueEquals(fromItem, toItem);
        }

        return object.Equals(fromValue, toValue);
    }

    #endregion

    #region State

    /// <inheritdoc/>
    public void RestoreViewState(ImGui gui)
    {
        if (gui is null)
        {
            throw new ArgumentNullException(nameof(gui));
        }

        var asset = _document.GetAsset();
        if (asset != null)
        {
            var config = EditorServices.PluginService.GetPlugin<GuiStatePlugin>().GetGuiState<FlowViewState>(asset);

            if (config != null)
            {
                (this as IInspectorContext).InspectorUserData = config.InspectorUserData;

                _graphControl.Viewport.ViewX = config.ViewX;
                _graphControl.Viewport.ViewY = config.ViewY;
                if (config.ViewZoom > 0)
                {
                    _graphControl.Viewport.ViewZoom = config.ViewZoom;
                }

                if (config.SelectedNodes != null)
                {
                    var nodes = config.SelectedNodes
                        .Select(o => _document.GetDiagramItem(o)?.Node?.GetViewNode(this))
                        .OfType<GraphNode>();

                    foreach (var node in nodes)
                    {
                        node.Highlighted = true;
                        node.Diagram.SelectedNodes.Add(node);
                    }
                }

                if (config.ExpandState != null)
                {
                    _expandState.Clear();
                    _expandState.SetExpandedPaths(config.ExpandState);
                    gui.RestoreState(_expandState);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void SaveViewState(ImGui gui)
    {
        if (gui is null)
        {
            throw new ArgumentNullException(nameof(gui));
        }

        var asset = _document.GetAsset();
        if (asset != null)
        {
            _expandState.Clear();
            gui.BackupState(_expandState);

            var config = new FlowViewState()
            {
                ViewX = _graphControl.Viewport.ViewX,
                ViewY = _graphControl.Viewport.ViewY,
                ViewZoom = _graphControl.Viewport.ViewZoom,
                InspectorUserData = (this as IInspectorContext).InspectorUserData,
                ExpandState = _expandState.GetExpandedPaths().Select(o => o.ToString()).ToArray(),
            };

            foreach (var item in _graphControl.Diagram.SelectedNodes.OfType<IFlowViewNode>().Select(o => o.Node?.DiagramItem).OfType<NamedItem>())
            {
                config.SelectedNodes.Add(item.Name);
            }

            EditorServices.PluginService.GetPlugin<GuiStatePlugin>().SetGuiState<FlowViewState>(asset, config);
        }
    }

    #endregion

    #region IDropTarget

    /// <inheritdoc/>
    public virtual void DragOver(IDragEvent e)
    {
        do
        {
            if (_document is not IDropInCheck docCheck)
            {
                break;
            }

            var idContext = e.Data.GetData<IHasId>();
            if (idContext is null || idContext.Id == Guid.Empty)
            {
                break;
            }

            EditorObject obj = EditorObjectManager.Instance.GetObject(idContext.Id);

            if (obj is null)
            {
                break;
            }

            if (!docCheck.DropInCheck(obj))
            {
                break;
            }

            e.SetLinkEffect();

            return;
        } while (false);

        e.SetNoneEffect();
    }

    /// <inheritdoc/>
    public virtual void DragDrop(IDragEvent e)
    {
        do
        {
            var lastPos = _graphControl.LastMousePos;

            //if (_graphPanel.HitAll(lastPos) != HitType.Nothing)
            //{
            //    break;
            //}

            if (_document is not IDropInCheck docCheck)
            {
                break;
            }

            IHasId idContext = e.Data.GetData<IHasId>();
            if (idContext is null || idContext.Id == Guid.Empty)
            {
                break;
            }

            EditorObject obj = EditorObjectManager.Instance.GetObject(idContext.Id);
            if (obj is null)
            {
                break;
            }

            if (docCheck.DropInConvert(obj) is not FlowNode node)
            {
                break;
            }

            Point pos = _graphControl.Viewport.ControlToView(lastPos);
            OnFlowDoAction(new CreateNodeAction(this, node));
            node.DiagramItem?.SetPosition(pos.X, pos.Y);

            OnDirty();

            return;
        } while (false);
    }

    #endregion

    #region GraphPanel

    private void GraphPanel_NodeCreateRequesting(object sender, EventArgs e)
    {
        HandleCreateNode();
    }

    /// <summary>
    /// Handles the group creation request from the graph panel.
    /// </summary>
    private void GraphPanel_GroupCreateRequesting(object sender, EventArgs e)
    {
        HandleCreateGroup();
    }

    //private void nodeGraphPanel1_MouseMove(object sender, MouseEventArgs e)
    //{
    //    m_MouseLoc = e.Location;
    //}
    private void GraphPanel_SelectionChanging(object sender, GraphSelectionEventArgs args)
    {
        _nodeSelectionBefore.Clear();
        _nodeSelectionBefore.AddRange(this._graphControl.Diagram.SelectedNodes);

        _linkSelectionBefore.Clear();
        _linkSelectionBefore.AddRange(this._graphControl.Diagram.SelectedLinks);
    }

    private void GraphPanel_SelectionChanged(object sender, GraphSelectionEventArgs args)
    {
        if (!_nodeSelectionBefore.ElementEquals(_graphControl.Diagram.SelectedNodes)
            || !_linkSelectionBefore.ElementEquals(_graphControl.Diagram.SelectedLinks)
            )
        {
            OnFlowDoAction(new NodeSmartSelectionAction(this, _nodeSelectionBefore, _linkSelectionBefore));
            _nodeSelectionBefore.Clear();
            _linkSelectionBefore.Clear();

            if (args.NodeCount > 0 || args.LinkCount > 0)
            {
                NavigationService.Current.AddRecord(_document);
            }
        }
    }

    /// <summary>
    /// Handles the selection deleting event, beginning a delete macro.
    /// </summary>
    private void GraphPanel_SelectionDeleting(object sender, GraphSelectionEventArgs args)
    {
    }

    private void GraphPanel_SelectionDeleted(object sender, GraphSelectionEventArgs args)
    {
        var nodes = this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>().Select(o => o.Node)
            .Where(o => o?.CanBeDeleted ?? true)
            .ToArray();

        var links = this._graphControl.Diagram.SelectedLinks
            .ToArray();

        if (nodes.Length == 0 && links.Length == 0)
        {
            return;
        }

        List<UndoRedoAction> actions = [];
        if (nodes.Length > 0)
        {
            actions.Add(new DeleteNodeAction(this, nodes));
        }

        if (links.Length > 0)
        {
            foreach (var link in links)
            {
                actions.Add(new DeleteLinkAction(this, link.Input.Parent.Name, link.Input.Name, link.Output.Parent.Name, link.Output.Name));
            }
        }

        var macroAction = new UndoRedoMacroAction("Delete Selected", actions);
        OnFlowDoAction(macroAction);

        OnDirty();
    }

    private void GraphPanel_SelectionMoved(object sender, GraphNodeMoveEventArgs args)
    {
        var items = this._graphControl.Diagram.SelectedNodes
            .Concat(this._graphControl.Diagram.DrivenNodes)
            .OfType<IFlowViewNode>()
            .Select(o => o.Node?.DiagramItem)
            .SkipNull();

        OnFlowDoAction(new MoveNodeAction(this, items, args.Offset));

        OnDirty();
    }

    /// <summary>
    /// Handles the node resized event, recording the size change as an undoable action.
    /// </summary>
    private void GraphPanel_SelectionResized(object sender, GraphNodeResizeEventArgs args)
    {
        var node = this._graphControl.Diagram.SelectedNodes
            .OnlyOneOfDefault() as IFlowViewNode;

        if (node?.Node?.DiagramItem is { } item)
        {
            OnFlowDoAction(new NodeResizeAction(item, args.OldBound, args.NewBound));
        }
    }

    private void GraphPanel_LinkCreated(object sender, GraphLinkEventArgs args)
    {
        if (args.Links.Count > 1)
        {
            List<UndoRedoAction> actions = [];
            foreach (var link in args.Links)
            {
                actions.Add(new CreateLinkAction(this, link.Input.Parent.Name, link.Input.Name, link.Output.Parent.Name, link.Output.Name));
            }

            var macroAction = new UndoRedoMacroAction("Create Links", actions);
            OnFlowDoAction(macroAction);
        }
        else if (args.Links.Count == 1)
        {
            var link = args.Links[0];
            OnFlowDoAction(new CreateLinkAction(this, link.Input.Parent.Name, link.Input.Name, link.Output.Parent.Name, link.Output.Name));
        }

        if (args.Links.Count > 0)
        {
            OnDirty();
            EditorUtility.Inspector.UpdateInspector();
        }
    }

    private void GraphPanel_LinkDestroyed(object sender, GraphLinkEventArgs args)
    {
        if (args.Links.Count > 1)
        {
            List<UndoRedoAction> actions = [];
            foreach (var link in args.Links)
            {
                actions.Add(new DeleteLinkAction(this, link.Input.Parent.Name, link.Input.Name, link.Output.Parent.Name, link.Output.Name));
            }

            var macroAction = new UndoRedoMacroAction("Delete Links", actions);
            OnFlowDoAction(macroAction);
        }
        else if (args.Links.Count == 1)
        {
            var link = args.Links[0];
            OnFlowDoAction(new DeleteLinkAction(this, link.Input.Parent.Name, link.Input.Name, link.Output.Parent.Name, link.Output.Name));
        }

        if (args.Links.Count > 0)
        {
            OnDirty();
            EditorUtility.Inspector.UpdateInspector();
        }
    }

    //private void nodeGraphPanel1_KeyDown(object sender, KeyEventArgs e)
    //{
    //    if (e.KeyCode == Keys.C && e.Control)
    //    {
    //        ClipboardCopy();
    //    }
    //    else if (e.KeyCode == Keys.X && e.Control)
    //    {
    //        ClipboardCut();
    //    }
    //    else if (e.KeyCode == Keys.V && e.Control)
    //    {
    //        ClipboardPaste();
    //    }
    //    else if (e.KeyCode == Keys.Insert)
    //    {
    //        HandleCreateNode();
    //    }
    //}

    private void GraphPanel_ContextMenuShowing(object sender, GraphicContextEventArgs e)
    {
        var items = this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>();

        _contextMenuCommand.ApplySender(this);
        (_graphControl.Gui?.Context as IGraphicContextMenu)?.ShowContextMenu(_contextMenuCommand, items);
    }

    #endregion

    #region Private & Internal

    internal void InternalComputeData()
    {
        if (_document is null)
        {
            return;
        }

        if (!_document.PreviewComputeEnabled)
        {
            return;
        }

        var compute = _document.CreateComputation();
        if (compute is null)
        {
            return;
        }

        foreach (var node in _document.DiagramItems.Select(o => o.Node).SkipNull())
        {
            if (!node.PreviewValue)
            {
                continue;
            }

            if (!compute.GetNodeRunningState(node).GetIsEnded())
            {
                node.Compute(compute);
            }

            if (node.GetViewNode(this) is { } viewNode)
            {
                object value = compute.GetResult(node);
                viewNode.UpdatePreviewText(_document.GetValuePreviewText(value));
            }
        }

        //GraphicContext?.RequestOutput();
        _graphControl.RefreshView();
    }

    /// <summary>
    /// Shows a node selection dialog and creates the selected node at the current mouse position.
    /// </summary>
    internal async void HandleCreateNode()
    {
        Point pos = _graphControl.Viewport.ControlToView(_graphControl.LastMousePos);

        var nodeList = _document?.GetFactoryNodeList();
        if (nodeList is null)
        {
            return;
        }

        var result = await nodeList.ShowSelectionGUIAsync("Select Node", new SelectionOption { HideEmptySelection = true });
        if (!result.IsSuccess)
        {
            return;
        }

        var rect = new Rectangle(pos.X, pos.Y, 0, 0);

        FlowNode node = _document.CreateFlowNode(result.SelectedKey);

        if (node != null)
        {
            OnFlowDoAction(new CreateNodeAction(this, node, rect));
            //node.DiagramItem?.SetPosition(pos.X, pos.Y);

            OnDirty();
        }
    }

    /// <summary>
    /// Creates a group node around the currently selected items, or at the mouse position if forced.
    /// </summary>
    /// <param name="forceCreate">If <c>true</c>, creates a group even when no items are selected.</param>
    internal void HandleCreateGroup(bool forceCreate = false)
    {
        var selectedItems = _graphControl.Diagram.SelectedNodes;
        if (selectedItems.Count > 0)
        {
            var rect = GraphHelper.GetBoundingBox(selectedItems.Select(o => o.HitRectangle));
            rect.X -= 10;
            rect.Y -= 30;
            rect.Width += 20;
            rect.Height += 40;

            var node = new GroupFlowNode();
            OnFlowDoAction(new CreateNodeAction(this, node, rect));
            OnDirty();
        }
        else if (forceCreate)
        {
            Point pos = _graphControl.Viewport.ControlToView(_graphControl.LastMousePos);
            var rect = new Rectangle(pos.X, pos.Y, 100, 100);

            var node = new GroupFlowNode();
            OnFlowDoAction(new CreateNodeAction(this, node, rect));
            OnDirty();
        }
    }

    internal void HandleCreateComment(bool forceCreate = false)
    {
        Point pos = _graphControl.Viewport.ControlToView(_graphControl.LastMousePos);
        var rect = new Rectangle(pos.X, pos.Y, 100, 100);

        var node = new CommentFlowNode();
        OnFlowDoAction(new CreateNodeAction(this, node, rect));
        OnDirty();
    }

    [Obsolete]
    internal void HandleDeleteNode()
    {
        var nodes = _graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Select(o => o.Node)
            .SkipNull();

        if (nodes.Any())
        {
            OnFlowBeginMacro("Delete Node");
            OnFlowDoAction(new DeleteNodeAction(this, nodes));
            OnFlowEndMacro("Delete Node");

            OnDirty();
        }
    }

    /// <summary>
    /// Clones the currently selected nodes and their links, placing them at the cursor position.
    /// </summary>
    internal void HandleCloneSelectedNodes()
    {
        var selected = this._graphControl.Diagram.SelectedNodes
            .OfType<IFlowViewNode>()
            .Where(o => o.CanBeDeleted)
            .Select(o => o.Node?.DiagramItem)
            .SkipNull();

        if (!selected.Any())
        {
            return;
        }

        var items = selected.ToList();
        List<NodeLink> links = [];
        Document.Links.CollectLinks(items.Select(o => o.Name), links);

        HandleCloneNodes(items, links, true);
    }

    internal void HandleSelectSameType(Type type)
    {
        if (type is null)
        {
            return;
        }

        var nodes = this._graphControl.Diagram.NodeCollection
            .OfType<IFlowViewNode>()
            .Select(o => o.Node)
            .OfType<FlowNode>()
            .Where(o => o.GetType() == type);

        SetSelection(nodes);
    }

    /// <summary>
    /// Finds references to the currently selected node.
    /// </summary>
    internal void HandleFindReference()
    {
        var items = this._graphControl.Diagram.SelectedNodes
        .OfType<IFlowViewNode>()
        .Select(o => o.Node?.DiagramItem)
        .SkipNull();

        if (items.CountOne())
        {
            var item = items.First();

            Guid id = item.Id;
            if (id != Guid.Empty)
            {
                EditorUtility.FindReference(id);
            }
            else if (item is INavigable itemNavi)
            {
                EditorUtility.FindReference(itemNavi.GetNavigationTarget());
            }
            else if (item.Node is INavigable nodeNavi)
            {
                EditorUtility.FindReference(nodeNavi.GetNavigationTarget());
            }
            else
            {
                EditorUtility.ClearLogView();
            }
        }
    }

    internal void HandleGotoDefinition()
    {
        var items = this._graphControl.Diagram.SelectedNodes
        .OfType<IFlowViewNode>()
        .Select(o => o.Node?.DiagramItem)
        .SkipNull();

        if (items.CountOne())
        {
            var item = items.First();

            if (item is INavigable navigable)
            {
                EditorUtility.GotoDefinition(navigable);
                return;
            }

            if (item.Node is INavigable navigable2)
            {
                EditorUtility.GotoDefinition(navigable2);
                return;
            }

            if (item.Id != Guid.Empty)
            {
                EditorUtility.GotoDefinition(item.Id);
                return;
            }
        }
    }



    private void HandleCloneNodes(List<IFlowDiagramItem> items, List<NodeLink> links, bool doClone)
    {
        if (items is null || items.Count == 0)
        {
            return;
        }

        int minX = items.Select(o => o.X).Min();
        int minY = items.Select(o => o.Y).Min();

        int offsetX = _graphControl.InputManager.ViewSpaceCursorLocation.X - minX;
        int offsetY = _graphControl.InputManager.ViewSpaceCursorLocation.Y - minY;

        //var clone = items.Select(o => Cloner.Clone(o)).ToArray();
        var clone = doClone ? items.Select(o => Cloner.Clone(o)).ToArray() : items.ToArray();

        foreach (var item in clone)
        {
            int x = item.X + offsetX;
            int y = item.Y + offsetY;
            item.SetPosition(x, y);

            // Need to update ports immediately.
            item.Node?.UpdateConnector();
        }

        OnFlowBeginMacro("Clone Node");
        OnFlowDoAction(new CreateNodeAction(this, clone.Select(o => o.Node)));

        _pasteNameDic.Clear();
        for (int i = 0; i < clone.Length; i++)
        {
            _pasteNameDic[items[i].Name] = clone[i].Node.Name;
        }

        if (links?.Count > 0)
        {
            foreach (var link in links)
            {
                string fromNode = _pasteNameDic.GetValueSafe(link.FromNode, link.FromNode);
                string toNode = _pasteNameDic.GetValueSafe(link.ToNode, link.ToNode);

                OnFlowDoAction(new CreateLinkAction(this, fromNode, link.FromConnector, toNode, link.ToConnector));
            }
        }

        OnFlowEndMacro("Clone Node");

        OnDirty();
    }

    private GraphNode CreateViewNode(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (node.DiagramItem is null)
        {
            throw new NullReferenceException("DiagramItem is empty.");
        }

        if (node is IGroupFlowNode)
        {
            var groupViewNode = new ImFlowGroupViewNode(node, node.DiagramItem.X, node.DiagramItem.Y, _graphControl.Diagram);

            return groupViewNode;
        }
        if (node is CommentFlowNode)
        {
            var commentViewNode = new ImFlowCommentViewNode(node, node.DiagramItem.X, node.DiagramItem.Y, _graphControl.Diagram);

            return commentViewNode;
        }
        else
        {
            // Creation will also call FlowNode.StartView for registration.
            var viewNode = new ImFlowViewNode(node, node.DiagramItem.X, node.DiagramItem.Y, _graphControl.Diagram)
            {
                CanBeDeleted = node.CanBeDeleted
            };

            return viewNode;
        }
    }

    /// <summary>
    /// Marks the document as dirty to indicate unsaved changes.
    /// </summary>
    protected void OnDirty() => _document?.Diagram?.MarkDirty();

    /// <summary>
    /// Sets the selection using a sync path for deep traversal.
    /// </summary>
    /// <param name="path">The sync path to resolve.</param>
    /// <returns><c>true</c> if the selection was set successfully.</returns>
    private bool SetSelection(SyncPath path)
    {
        if (!Visitor.TryGetValueDeep(_document, path, out FlowNode node, out SyncPath rest))
        {
            return false;
        }

        SetSelection(node);

        if (!SyncPath.IsNullOrEmpty(rest) && rest.Length >= 1)
        {
            EditorUtility.Inspector.SetSelection(rest, out rest);
        }

        return true;
    }

    private bool SetSelection(string name)
    {
        var node = _document.GetDiagramItem(name)?.Node;
        if (node != null)
        {
            SetSelection(node);

            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateAnalysis()
    {
        //Task.Run(() =>
        //{
        //    if (_document != null)
        //    {
        //        EditorUtility.LogCore?.LogDebug($"FlowDocumentView UpdateAnalysis : {_document}...");
        //        EditorManagement.AnalysisService.Analyze(_document, new AnalysisOption { }, () =>
        //        {
        //        });
        //        EditorUtility.LogCore?.LogDebug($"FlowDocumentView UpdateAnalysis OK");
        //    }
        //})
        //.ContinueWith(task =>
        //{
        //    QueuedAction.Do(() =>
        //    {
        //        EditorUtility.LogCore?.LogDebug("FlowDocumentView UpdateFromRootObject");
        //        //treeControl.UpdateDisplayedObject();
        //        EditorUtility.Inspector.UpdateSelectedObjects();

        //        nodeGraphPanel1.Invalidate();
        //    });
        //});

        EditorServices.AnalysisService.QueueAnalyze(_document, new AnalysisOption { }, () =>
        {
            EditorUtility.Inspector.UpdateInspector();
            _graphControl.RefreshView(true);
        });
    }

    #endregion

    #region Actions

    private class UpdateAnalysisAction(FlowViewImGui value) : DelayedAction<FlowViewImGui>(value)
    {
        public override void DoAction()
        {
            if (Value._document is null)
            {
                return;
            }

            Value.UpdateAnalysis();
        }
    }

    private class ComputeDataAction(FlowViewImGui view) : DelayedAction<FlowViewImGui>(view)
    {
        public override void DoAction()
        {
            Value.InternalComputeData();
        }
    }

    #endregion
}