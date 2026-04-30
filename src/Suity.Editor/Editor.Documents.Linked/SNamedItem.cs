using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Base class for named items that support asset building and analysis.
/// </summary>
public abstract class SNamedItem : NamedItem,
    IMember,
    IHasAssetBuilder,
    ICrossMove,
    ISupportAnalysis,
    ICommit,
    IViewEditNotify
{
    private readonly AssetBuilder _builder;
    private SNamedFieldList _fieldList;
    private INamedRenderTargetList _renderTargetList;
    private INamedUsingList _usingList;

    public SNamedItem()
    { }

    public SNamedItem(AssetBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _builder.Owner = this;
    }

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected AssetBuilder AssetBuilder => _builder;

    /// <summary>
    /// Gets the field list.
    /// </summary>
    public SNamedFieldList FieldList => _fieldList;

    /// <summary>
    /// Gets the recorded ID.
    /// </summary>
    public Guid RecorededId => _builder?.RecordedId ?? Guid.Empty;

    /// <summary>
    /// Gets the document containing this item.
    /// </summary>
    public SNamedDocument GetDocument()
    {
        return (Root as SNamedRootCollection)?.Document;
    }

    #region FieldList

    [Obsolete("")]
    protected SNamedFieldList<TItem> CreatePrimaryFieldList<TItem>(string fieldName, Func<TItem> factory = null)
        where TItem : SNamedField
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("Must provide a field name", nameof(fieldName));
        }
        if (_fieldList != null)
        {
            throw new InvalidOperationException("Primary field list is already added.");
        }

        var list = new SNamedFieldList<TItem>(fieldName, this, factory);

        _fieldList = list;

        return list;
    }

    /// <summary>
    /// Adds a primary field list to this item.
    /// </summary>
    /// <param name="list">The field list to add.</param>
    protected void AddPrimaryFieldList(SNamedFieldList list)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (_fieldList != null)
        {
            throw new InvalidOperationException("Primary field list is already added.");
        }

        if (list.ParentItem != this)
        {
            throw new InvalidOperationException($"{nameof(list)}.{nameof(list.ParentItem)} must set to this.");
        }

        _fieldList = list;
    }

    #endregion

    #region Virtual

    /// <summary>
    /// Gets or sets whether to show render targets.
    /// </summary>
    protected virtual bool ShowRenderTargets { get; set; }

    /// <summary>
    /// Gets or sets whether to show usings.
    /// </summary>
    protected virtual bool ShowUsings { get; set; }

    protected override void OnNameUpdated(string oldName, string newName)
    {
        base.OnNameUpdated(oldName, newName);

        _builder?.SetLocalName(this.Name);
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            _oldId = sync.Sync("_oldId", _oldId);
        }

        if (sync.Intent == SyncIntent.Serialize && sync.IsNameOf("AssetId"))
        {
            if (sync.IsGetter())
            {
                Guid id = this.Id;
                if (id != Guid.Empty)
                {
                    sync.Sync("AssetId", this.Id);
                }
            }
            else if (sync.IsSetter())
            {
                if (_builder != null)
                {
                    Guid id = sync.Sync("AssetId", this.Id);
                    _builder.SetRecordedId(id);
                }
            }
        }

        if (sync.Intent == SyncIntent.View && ShowUsings)
        {
            sync.Sync("Usings", _usingList, SyncFlag.GetOnly | SyncFlag.ByRef | SyncFlag.NoSerialize);
        }

        if (sync.Intent == SyncIntent.View && ShowRenderTargets && Analysis?.RenderTargets.Count > 0 && _renderTargetList != null)
        {
            sync.Sync("RenderTargets", _renderTargetList, SyncFlag.GetOnly | SyncFlag.ByRef | SyncFlag.NoSerialize);
        }
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (ShowUsings && Analysis != null && setup.SupportDetailTreeView())
        {
            if (_usingList is null)
            {
                if (Analysis.DependencyObjects.Count > 0)
                {
                    var ids = OnFilterUsingList(Analysis.DependencyObjects);
                    _usingList = NamedExternal._external.CreateUsingList("Using", ids);
                }
            }

            if (_usingList != null)
            {
                var prop = new ViewProperty("Usings", "Using") 
                {
                    ReadOnly = true,
                    Expand = false,
                    Status = TextStatus.Reference ,
                };
                setup.DetailTreeViewField(_usingList, prop);
            }
        }

        if (ShowRenderTargets && Analysis?.RenderTargets.Count > 0 && setup.SupportDetailTreeView())
        {
            _renderTargetList ??= NamedExternal._external.CreateRenderTargetList(this);

            var prop = new ViewProperty("RenderTargets", "Render Targets")
            { 
                ReadOnly = true, 
                Expand = false, 
                Status = TextStatus.Reference,
            };
            setup.DetailTreeViewField(_renderTargetList, prop);
        }
    }

    protected override TextStatus OnGetTextStatus()
    {
        if (Analysis != null)
        {
            return Analysis.Status;
        }

        return base.OnGetTextStatus();
    }

    /// <summary>
    /// Filters the using list.
    /// </summary>
    /// <param name="ids">The IDs to filter.</param>
    /// <returns>The filtered IDs.</returns>
    protected virtual IEnumerable<Guid> OnFilterUsingList(IEnumerable<Guid> ids)
    {
        return ids;
    }

    #endregion

    #region IAssetBuilderOwner

    AssetBuilder IHasAssetBuilder.TargetAssetBuilder => _builder;

    #endregion

    #region IAssetContext

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    public Asset TargetAsset => _builder?.TargetAsset;

    #endregion

    #region INavigationIte

    public string SelectionKey => Name;

    public virtual string DisplayText => this.OnGetDisplayText();

    #endregion

    #region IMember

    public IMemberContainer Container => GetDocument();

    /// <summary>
    /// Gets the ID of the member.
    /// </summary>
    public virtual Guid Id => _builder?.TargetAsset?.Id ?? Guid.Empty;

    #endregion

    #region ICrossMove

    private Guid _oldId;

    void ICrossMove.ReadyMove() => _oldId = this.Id;

    void ICrossMove.DoMove(ILocalRefactor refactor)
    {
        Guid oldId = _oldId;
        Guid newId = this.Id;

        if (oldId == newId)
        {
            return;
        }

        //Initiate local refactor operation
        refactor?.Rename(oldId, newId);

        _oldId = newId;
    }

    #endregion

    #region ISupportAnalysis

    private AnalysisResult _analysisResult;

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    public AnalysisResult Analysis
    {
        get => _analysisResult;
        set
        {
            _analysisResult = value;
            AssetBuilder?.SetProblem(_analysisResult.Problems);

            _usingList = null;
            _renderTargetList = null;
        }
    }

    /// <summary>
    /// Collects analysis problems.
    /// </summary>
    /// <param name="problems">The problem collector.</param>
    /// <param name="intent">The analysis intent.</param>
    public virtual void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    { }

    #endregion

    #region ICommit

    Task ICommit.Commit(object marker)
    {
        if (this.GetDocument() is ICommit commit)
        {
            return commit.Commit(marker);
        }

        throw new NullReferenceException($"Document is null");
    }

    #endregion

    #region IViewEditNotify

    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        _builder?.NotifyUpdated();
        OnViewEdited(obj, propertyName);
    }

    /// <summary>
    /// Called when the view is edited.
    /// </summary>
    /// <param name="obj">The edited object.</param>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnViewEdited(object obj, string propertyName)
    { }

    #endregion
}

/// <summary>
/// Generic SNamedItem with a specific asset builder type.
/// </summary>
public abstract class SNamedItem<TAssetBuilder> : SNamedItem
    where TAssetBuilder : AssetBuilder, new()
{
    private readonly TAssetBuilder _builder;

    public SNamedItem()
        : base(new TAssetBuilder())
    {
        _builder = (TAssetBuilder)base.AssetBuilder;
    }

    public SNamedItem(string name)
    {
        this.Name = name;
    }

    protected new TAssetBuilder AssetBuilder => _builder;
}