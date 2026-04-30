using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Base class for named nodes that support asset building and analysis.
/// </summary>
public abstract class SNamedNode : NamedNode, 
    IMember,
    IHasAssetBuilder,
    ICrossMove,
    ISupportAnalysis,
    ICommit,
    IViewEditNotify
{
    private readonly AssetBuilder _builder;
    private SNamedFieldList _fieldList;

    public SNamedNode()
    { }

    public SNamedNode(AssetBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _builder.Owner = this;
    }

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    public Asset TargetAsset => _builder?.TargetAsset;

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected AssetBuilder AssetBuilder => _builder;

    /// <summary>
    /// Gets the field list.
    /// </summary>
    protected SNamedFieldList FieldList => _fieldList;

    /// <summary>
    /// Gets the document containing this node.
    /// </summary>
    public SNamedDocument GetDocument()
    {
        var model = Root as SNamedRootCollection;
        return model?.Document;
    }

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
    }

    #region FieldList

    /// <summary>
    /// Adds a primary field list to this node.
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

    #region IAssetBuilderOwner

    AssetBuilder IHasAssetBuilder.TargetAssetBuilder => _builder;

    #endregion

    #region ICrosstMove

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

    #region INavigationItem

    public string SelectionKey => Name;

    public virtual string DisplayText => this.OnGetDisplayText();

    #endregion

    #region IMember

    public IMemberContainer Container => GetDocument() as IMemberContainer;

    /// <summary>
    /// Gets the ID of the member.
    /// </summary>
    public virtual Guid Id => _builder?.TargetAsset?.Id ?? Guid.Empty;

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
/// Generic SNamedNode with a specific asset builder type.
/// </summary>
public abstract class SNamedNode<TAssetBuilder> : SNamedNode
        where TAssetBuilder : AssetBuilder, new()
{
    private readonly TAssetBuilder _builder;

    public SNamedNode()
        : base(new TAssetBuilder())
    {
        _builder = (TAssetBuilder)base.AssetBuilder;
    }

    protected new TAssetBuilder AssetBuilder => _builder;
}