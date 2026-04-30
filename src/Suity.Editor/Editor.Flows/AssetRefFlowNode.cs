using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;

namespace Suity.Editor.Flows;

/// <summary>
/// Asset reference flow node
/// </summary>
public abstract class AssetRefFlowNode : FlowNode
{
    private AssetSelection<Asset> _assetRef = new();

    private DocumentEntry _currentDocEntry;
    private readonly DocumentUsageToken _docUseToken = new(nameof(CanvasAssetNode));
    private bool _occupyDocumentUsage = false;


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
    /// Initializes a new instance of the AssetRefFlowNode.
    /// </summary>
    protected AssetRefFlowNode()
    {
        _assetRef.Filter = AssetFilters.All;
        _assetRef.TargetUpdated += _assetRef_TargetUpdated;

        base.EditorGui = DrawEditorGui;
    }

    /// <summary>
    /// Initializes a new instance of the AssetRefFlowNode with an asset.
    /// </summary>
    protected AssetRefFlowNode(Asset asset)
        : this()
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        _assetRef.Target = asset;
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

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        AssetRef = sync.Sync(nameof(AssetRef), AssetRef, SyncFlag.NotNull) ?? new AssetSelection<Asset>();
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

    /// <summary>
    /// Asset target has been updated
    /// </summary>
    protected virtual void OnAssetTargetUpdated()
    {
    }

    protected virtual void OnNavigationButtonClicked(IHasSubDocumentView subView)
    {
    }

    /// <summary>
    /// Sets listening enabled. Currently called by <see cref="CanvasAssetDiagramItem"/>
    /// </summary>
    /// <param name="enabled"></param>
    internal void SetListenEnabled(bool enabled)
    {
        _assetRef.ListenEnabled = enabled;
    }
}

public abstract class AssetRefFlowNode<TAsset> : AssetRefFlowNode
    where TAsset : Asset
{
    protected AssetRefFlowNode() : base()
    {
    }

    protected AssetRefFlowNode(TAsset asset) : base(asset)
    {
    }

    public override Type AssetType => typeof(TAsset);

    public TAsset Target => TargetAsset as TAsset;
}

public class AssetRefDiagramItem : FlowDiagramItem<AssetRefFlowNode>,
    IInspectorRoute,
    INavigable
{
    public AssetRefDiagramItem()
    { }

    public AssetRefDiagramItem(AssetRefFlowNode node) : base(node)
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