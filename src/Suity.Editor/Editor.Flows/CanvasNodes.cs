using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows;

#region ICanvasCompute

/// <summary>
/// Canvas compute interface
/// </summary>
public interface ICanvasCompute
{
    /// <summary>
    /// Invalidates the node for recomputation.
    /// </summary>
    /// <param name="node">The node.</param>
    void InvalidateNodeComputation(FlowNode node);

    /// <summary>
    /// Gets the connector value.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>The value.</returns>
    object GetConnectorValue(FlowNodeConnector connector);
}
#endregion

#region CanvasFlowNode

/// <summary>
/// Canvas flow node
/// </summary>
[DisplayText("Canvas Node", "*CoreIcon|Canvas")]
public abstract class CanvasFlowNode : FlowNode
{
    /// <summary>
    /// Gets the parent document.
    /// </summary>
    public SNamedDocument ParentDocument
        => (DiagramItem as FlowDiagramItem)?.GetDocument();

    /// <summary>
    /// Gets the canvas document.
    /// </summary>
    public ICanvasDocument Canvas => ParentDocument as ICanvasDocument;

    /// <summary>
    /// Gets the inspector context.
    /// </summary>
    /// <returns>The inspector context.</returns>
    public IInspectorContext GetInspectorContext()
        => ParentDocument?.View?.GetService<IInspectorContext>();

    /// <summary>
    /// Gets the icon.
    /// </summary>
    public override Image Icon => CoreIconCache.Canvas;

    /// <summary>
    /// Gets the connector value.
    /// </summary>
    protected object GetConnectorValue(FlowNodeConnector connector)
        => (ParentDocument as ICanvasCompute)?.GetConnectorValue(connector);

    /// <summary>
    /// Gets the connector value as a specific type.
    /// </summary>
    protected T GetConnectorValue<T>(FlowNodeConnector connector) where T : class
        => (ParentDocument as ICanvasCompute)?.GetConnectorValue(connector) as T;

    /// <summary>
    /// Invalidate node and all its output nodes so they need to recompute.
    /// </summary>
    protected void InvalidateNodeComputation()
    {
        (ParentDocument as ICanvasCompute)?.InvalidateNodeComputation(this);
    }

    //public abstract void SetExpand(bool expand);
}
#endregion

#region CanvasToolNode

/// <summary>
/// Canvas tool node
/// </summary>
[DisplayText("Canvas Tool Node", "*CoreIcon|Canvas")]
public abstract class CanvasToolNode : CanvasFlowNode
{
}

#endregion

#region CanvasAssetNode

/// <summary>
/// Canvas asset node
/// </summary>
[DisplayText("Canvas Resource Node", "*CoreIcon|Kanban")]
public class CanvasAssetNode : CanvasFlowNode,
    IInspectorRoute,
    IInspectorContext,
    INavigable
{
    private AssetSelection<Asset> _assetRef = new();

    private DocumentEntry _currentDocEntry;
    private readonly DocumentUsageToken _docUseToken = new(nameof(CanvasAssetNode));
    private bool _occupyDocumentUsage = true;


    /// <summary>
    /// Initializes a new instance of the CanvasAssetNode.
    /// </summary>
    public CanvasAssetNode()
    {
        _assetRef.Filter = AssetFilters.All;
        _assetRef.TargetUpdated += _assetRef_TargetUpdated;

        base.EditorGui = DrawEditorGui;
    }

    /// <summary>
    /// Initializes a new instance of the CanvasAssetNode with an asset.
    /// </summary>
    public CanvasAssetNode(Asset asset)
        : this()
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        _assetRef.Target = asset;
    }

    /// <summary>
    /// Gets or sets the target asset.
    /// </summary>
    public Asset TargetAsset
    {
        get => _assetRef.Target;
        set
        {
            if (ReferenceEquals(_assetRef.Target, value))
            {
                return;
            }

            _assetRef.Target = value;

            // Update connectors when referenced asset changes
            UpdateConnectorQueued();
        }
    }

    /// <summary>
    /// Gets or sets the asset reference.
    /// </summary>
    internal AssetSelection<Asset> AssetRef
    {
        get => _assetRef;
        set
        {
            if (ReferenceEquals(_assetRef, value))
            {
                return;
            }

            bool listen = false;

            if (_assetRef != null)
            {
                listen = _assetRef.ListenEnabled;

                _assetRef.ListenEnabled = false;
                _assetRef.TargetUpdated -= _assetRef_TargetUpdated;
            }

            _assetRef = value;

            if (_assetRef != null)
            {
                _assetRef.TargetUpdated += _assetRef_TargetUpdated;
                _assetRef.ListenEnabled = listen;
            }

            // Update connectors when referenced asset changes
            UpdateConnectorQueued();

            // Callback
            OnAssetTargetUpdated();
        }
    }

    /// <summary>
    /// Gets the asset type.
    /// </summary>
    public virtual Type AssetType => null;

    /// <summary>
    /// Indicates whether to display as read-only in the inspector.
    /// </summary>
    public virtual bool InspectorReadonly => false;

    /// <summary>
    /// Gets the icon.
    /// </summary>
    public override Image Icon => _assetRef?.Target?.Icon;

    /// <summary>
    /// Gets the title color.
    /// </summary>
    public override Color? TitleColor => (_assetRef?.Target as IViewColor)?.ViewColor;

    /// <summary>
    /// Gets or sets whether to occupy document usage.
    /// </summary>
    protected bool OccupyDocumentUsage
    {
        get => _occupyDocumentUsage;
        set
        {
            if (_occupyDocumentUsage != value)
            {
                _occupyDocumentUsage = value;
                if (_occupyDocumentUsage)
                {
                    _currentDocEntry?.MarkUsage(_docUseToken);
                }
                else
                {
                    _currentDocEntry?.UnmarkUsage(_docUseToken);
                }
            }
        }
    }


    /// <summary>
    /// Gets the target document.
    /// </summary>
    /// <returns>The document.</returns>
    public Document GetTargetDocument() => _assetRef.Target?.GetDocument(true);

    /// <summary>
    /// Gets the target document as a specific type.
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    /// <returns>The document.</returns>
    public T GetTargetDocument<T>() where T : class
        => _assetRef.Target?.GetDocument<T>(true);

    /// <summary>
    /// Gets the target object.
    /// </summary>
    /// <returns>The target object.</returns>
    public object GetTargetObject()
    {
        var asset = _assetRef.Target;
        if (asset is null)
        {
            return null;
        }

        var doc = asset.GetDocument(true);
        if (doc is null)
        {
            return null;
        }

        if (asset.FileName != null)
        {
            if (doc is Document sdoc)
            {
                return sdoc;
            }
            else if (doc is IViewObject obj)
            {
                return obj;
            }
        }
        else if ((doc as IMemberContainer)?.GetMember(asset.LocalName) is IViewObject member)
        {
            return member;
        }

        return null;
    }

    /// <summary>
    /// Gets the target inspector context.
    /// </summary>
    /// <returns>The inspector context.</returns>
    public IInspectorContext GetTargetInspectorContext()
    {
        return GetTargetDocument()?.View?.GetService<IInspectorContext>();
    }


    /// <summary>
    /// The object to display when expanded. If the target object doesn't exist, displays itself
    /// </summary>
    public override object ExpandedViewObject => GetTargetObject() ?? this;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        AssetRef = sync.Sync(nameof(AssetRef), AssetRef, SyncFlag.NotNull) ?? new AssetSelection<Asset>();

        if (sync.Intent == SyncIntent.View)
        {
            if (sync.IsGetterOf("Content"))
            {
                var vobj = GetTargetObject();
                if (vobj != null)
                {
                    sync.Sync("Content", vobj);
                }
            }

            if (sync.Intent == SyncIntent.View && sync.IsSetterOf("#EditButton"))
            {
                var vobj = GetTargetObject();
                if (vobj != null)
                {
                    EditorUtility.Inspector.UpdateInspector();
                }
            }
        }
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        var asset = _assetRef.Target;
        if (asset is null)
        {
            return;
        }

        var vobj = GetTargetObject();
        if (vobj is null)
        {
            // Document not opened
            setup.Label(new ViewProperty("#EditLabel", "Document not opened"));
            setup.Button(new ViewProperty("#EditButton", "Edit"));

            return;
        }

        setup.InspectorFieldOf<IViewObject>(new ViewProperty("Content", "Content").WithReadOnly());
    }

    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var type = TargetAsset?.GetType() ?? AssetType;
        if (type != null)
        {
            var connector = AddAssociateOutputConnector("#USAGE", type.FullName);
            if (_assetRef.Target is { } asset)
            {
                connector.AssociateValue = asset.Id;
            }
        }
    }

    // Updates node sync computation data
    protected internal override void OnUpdated()
    {
        base.OnUpdated();

        // When executing UpdateConnectorQueued, NotifyUpdateQueued is also executed and enters this flow
        // This flow ensures that after connection ports are updated, the port data is also synced
        InvalidateNodeComputation();
    }

    /// <summary>
    /// Asset target has been updated
    /// </summary>
    protected virtual void OnAssetTargetUpdated()
    {
    }

    // Temporarily disabled double-click as it may cause accidental operations
    internal protected override void OnDoubleClick()
    {
        //var asset = _assetRef.Target;
        //if (asset != null)
        //{
        //    EditorUtility.GotoDefinition(asset);
        //}
    }

    /// <summary>
    /// Sets listening enabled. Currently called by <see cref="CanvasAssetDiagramItem"/>
    /// </summary>
    /// <param name="enabled"></param>
    internal void SetListenEnabled(bool enabled)
    {
        _assetRef.ListenEnabled = enabled;
    }

    /// <summary>
    /// Target asset has been updated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="handled"></param>
    private void _assetRef_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        // Update target document usage
        UpdateDocumentUsage();

        // Update connection ports, this will also trigger OnUpdated and sync compute port data
        UpdateConnectorQueued();

        // Callback
        OnAssetTargetUpdated();
    }

    // Update target document usage when node is added
    protected internal override void OnAdded()
    {
        base.OnAdded();

        // Update target document usage
        UpdateDocumentUsage();
    }

    // Remove target document usage when node is removed
    protected internal override void OnRemoved()
    {
        base.OnRemoved();

        // Remove target document usage
        _currentDocEntry?.UnmarkUsage(_docUseToken);
        _currentDocEntry = null;
    }

    protected virtual bool DrawEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        switch (pipeline)
        {
            case EditorImGuiPipeline.Preview:
                {
                    gui.Button("#open", ImGuiIcons.Open)
                    .InitClass("configBtn")
                    .OnClick(n => 
                    {
                        if (context is IFlowViewNode { FlowView: IHasSubDocumentView subView })
                        {
                            OnNavigationButtonClicked(subView);
                        }
                        else
                        {
                            subView = gui.GetValue<IHasSubDocumentView>();
                            //var currentView = n.GetValue<IFlowView>();
                            OnNavigationButtonClicked(subView);
                        }
                    });

                    return true;
                }
        }

        return false;
    }

    protected virtual void OnNavigationButtonClicked(IHasSubDocumentView subView)
    {
        //if (TargetAsset?.GetStorageObject(true) is Document doc)
        //{
        //    doc.ShowView();
        //}

        EditorUtility.LocateInProject(TargetAsset);
    }

    /// <summary>
    /// Update target document usage
    /// </summary>
    private void UpdateDocumentUsage()
    {
        var oldEntry = _currentDocEntry;
        var newEntry = GetTargetDocument()?.Entry;

        if (!ReferenceEquals(oldEntry, newEntry))
        {
            oldEntry?.UnmarkUsage(_docUseToken);

            if (OccupyDocumentUsage)
            {
                newEntry?.MarkUsage(_docUseToken);
            }

            _currentDocEntry = newEntry;
        }
    }

    /// <summary>
    /// Returns Error when the document resource behind the target asset doesn't exist
    /// </summary>
    /// <returns></returns>
    public override TextStatus DisplayStatus => GetTargetObject() != null ? TextStatus.Normal : TextStatus.Error;


    #region IInspectorRoute

    object IViewRedirect.GetRedirectedObject(int viewId) => GetTargetObject() ?? this;

    InspectorTreeModes? IInspectorRoute.GetRoutedTreeMode() => null;

    bool IInspectorRoute.GetRoutedReadonly() => false;

    INodeReader IInspectorRoute.GetRoutedStyles() => null;

    #endregion

    #region IInspectorContext

    object IInspectorContext.InspectorUserData
    {
        get => GetInspectorContext()?.InspectorUserData;
        set
        {
            if (GetInspectorContext() is { } context)
            {
                context.InspectorUserData = value;
            }
        }
    }

    void IInspectorContext.InspectorEnter() => GetInspectorContext()?.InspectorEnter();

    void IInspectorContext.InspectorExit() => GetInspectorContext()?.InspectorExit();

    void IInspectorContext.InspectorBeginMacro(string name) => GetInspectorContext()?.InspectorBeginMacro(name);

    bool IInspectorContext.InspectorDoAction(UndoRedoAction action)
    {
        var doc = GetTargetDocument();
        if (doc is null)
        {
            return false;
        }

        var context = GetInspectorContext();
        if (context != null)
        {
            if (!context.InspectorDoAction(action))
            {
                action.Do();
            }
        }
        else
        {
            action.Do();
        }

        // If this is a modifying action, execute the modification flow
        if (action.Modifying)
        {
            doc.MarkDirty(this);

            // If document has no view, auto-save
            if (doc.View is null)
            {
                doc.Save();
            }
        }

        return true;
    }

    void IInspectorContext.InspectorEndMarco(string name)
    {
        GetInspectorContext()?.InspectorEndMarco(name);
    }

    void IInspectorContext.InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        GetInspectorContext()?.InspectorObjectEdited(objs, propertyName);

        // The routed document is loaded but missing a view, forward notification to the target selection object
        var viewSelection = ParentDocument?.View?.GetService<IViewSelectionInfo>();
        if (viewSelection != null)
        {
            QueuedAction.Do(() =>
            {
                var e = viewSelection.FindSelectionOrParent<IInspectorRoute>();

                var routeSelection = e
                    .Select(o => EditorUtility.GetViewRedirectedObject(o, ViewIds.Inspector))
                    .SkipNull()
                    .ToArray();

                var objAry = objs.ToArray();

                for (int i = 0; i < routeSelection.Length; i++)
                {
                    // Members of the two arrays correspond 1-to-1
                    (routeSelection[i] as IViewEditNotify)?.NotifyViewEdited(objAry.GetArrayItemSafe(i), propertyName);
                }
            });
        }

    }

    void IInspectorContext.InspectorEditFinish()
    {
        GetInspectorContext()?.InspectorEditFinish();
    }

    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (GetInspectorContext()?.GetService(serviceType) is { } service)
        {
            return service;
        }

        if (serviceType.IsAssignableFrom(this.GetType()))
        {
            return this;
        }

        return null;
    }
    #endregion

    #region INavigable

    object INavigable.GetNavigationTarget() => _assetRef.Target;

    #endregion

    public override string ToString()
    {
        return _assetRef?.Target?.ToDisplayText() ?? _assetRef?.ToString() ?? DisplayText ?? "???";
    }
}
#endregion

#region CanvasAssetNode<TAsset>

public abstract class CanvasAssetNode<TAsset> : CanvasAssetNode
    where TAsset : Asset
{
    protected CanvasAssetNode() : base()
    {
    }

    protected CanvasAssetNode(TAsset asset) : base(asset)
    {
    }

    public override Type AssetType => typeof(TAsset);

    public TAsset Target => TargetAsset as TAsset;
}

#endregion

#region ExpandedCanvasAssetNode<TAsset>

public abstract class ExpandedCanvasAssetNode<TAsset> : CanvasAssetNode<TAsset>, IDrawExpandedImGui
    where TAsset : Asset
{
    public IInspectorContext TargetContext { get; protected set; }

    protected ExpandedCanvasAssetNode() : base()
    {
    }

    protected ExpandedCanvasAssetNode(TAsset asset) : base(asset)
    {
    }

    #region IDrawExpandedImGui

    public virtual bool ResizableOnExpand => false;

    public virtual float? ContentScale => null;

    public virtual void ClearSelection()
    {
    }

    public virtual void EnterExpandedView(object target, IInspectorContext context = null)
    {
        TargetContext = context;
    }

    public virtual void ExitExpandedView()
    {
    }

    public virtual void UpdateExpandedTarget()
    {
    }

    public virtual ImGuiNode OnExpandedGui(ImGui gui)
    {
        return null;
    }

    #endregion
}

#endregion

#region CanvasAssetDiagramItem
public class CanvasAssetDiagramItem : FlowDiagramItem<CanvasAssetNode>,
    IInspectorRoute,
    INavigable
{
    public CanvasAssetDiagramItem()
    { }

    public CanvasAssetDiagramItem(CanvasAssetNode node) : base(node)
    {
    }

    protected override void OnAdded()
    {
        base.OnAdded();

        Node?.SetListenEnabled(true);
    }

    protected override void OnRemoved(NamedRootCollection model)
    {
        base.OnRemoved(model);

        Node?.SetListenEnabled(false);
    }

    protected override string OnGetDisplayText()
    {
        return Node?.ToDisplayText() ?? "???";
    }

    protected override TextStatus OnGetTextStatus()
    {
        if (Node?.TargetAsset is null)
        {
            return TextStatus.Error;
        }
        else
        {
            return base.OnGetTextStatus();
        }
    }

    #region IInspectorRoute

    object IViewRedirect.GetRedirectedObject(int viewId) => (Node as IViewRedirect)?.GetRedirectedObject(viewId) ?? this;

    InspectorTreeModes? IInspectorRoute.GetRoutedTreeMode() => (Node as IInspectorRoute)?.GetRoutedTreeMode();

    bool IInspectorRoute.GetRoutedReadonly() => (Node as IInspectorRoute)?.GetRoutedReadonly() ?? false;

    INodeReader IInspectorRoute.GetRoutedStyles() => (Node as IInspectorRoute)?.GetRoutedStyles();

    #endregion

    #region INavigable
    object INavigable.GetNavigationTarget()
        => Node?.AssetRef?.TargetAsset?.Id ?? base.Id;
    #endregion
}

//public class CanvasAssetNodeStyle : FlowNodeStyle<CanvasAssetNode>
//{
//    public override bool HasHeader => true;

//    public CanvasAssetNodeStyle()
//    {
//        base.CustomDraw = Draw;
//    }

//    private void Draw(IGraphicOutput output, IFlowNodeDrawContext context, float zoom, Point pos, Rectangle rect, bool drawText)
//    {
//        context.DrawShadow(output);

//        context.DrawPanel(output, zoom, rect);

//        if (drawText)
//        {
//            context.DrawHeader(output, zoom, rect);
//        }
//    }
//} 
#endregion
