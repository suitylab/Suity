using Suity.Editor.Analyzing;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Base class for named fields that support asset references and analysis.
/// </summary>
public abstract class SNamedField : NamedField,
    IHasAsset,
    IFindable,
    IHasId,
    ICrossMove,
    ISupportAnalysis
{
    public Guid _recordedId;

    public SNamedField()
    { }

    public SNamedField(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Gets the asset field.
    /// </summary>
    public virtual EditorObject AssetField => null;

    /// <summary>
    /// Gets the key string for finding.
    /// </summary>
    public string KeyString => AssetField?.FullName ?? Name;

    /// <summary>
    /// Gets the recorded field ID.
    /// </summary>
    public Guid RecorededFieldId
    {
        get
        {
            Guid id = AssetField?.Id ?? Guid.Empty;
            if (id == Guid.Empty)
            {
                id = _recordedId;
            }

            return id;
        }
    }


    #region IAssetContext Members

    /// <summary>
    /// Gets the parent item.
    /// </summary>
    public NamedItem ParentItem => (List as SNamedFieldList)?.ParentItem;

    /// <summary>
    /// Gets the parent SNamedItem.
    /// </summary>
    public SNamedItem ParentSItem => (List as SNamedFieldList)?.ParentItem as SNamedItem;

    /// <summary>
    /// Gets the parent SNamedNode.
    /// </summary>
    public SNamedNode ParentSNode => (List as SNamedFieldList)?.ParentItem as SNamedNode;

    /// <summary>
    /// Gets the document containing this field.
    /// </summary>
    public SNamedDocument GetDocument() => ParentSItem?.GetDocument() ?? ParentSNode?.GetDocument();

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    public Asset TargetAsset => ParentSItem?.TargetAsset;

    #endregion

    #region IFindable Members

    string IFindable.GetFindingKey() => KeyString;

    #endregion

    #region IHasId

    /// <summary>
    /// Gets the ID.
    /// </summary>
    public Guid Id => AssetField?.Id ?? Guid.Empty;

    #endregion

    #region ICrossMove

    private Guid _oldId;

    void ICrossMove.ReadyMove() => _oldId = Id;

    void ICrossMove.DoMove(ILocalRefactor refactor)
    {
        Guid oldId = _oldId;
        Guid newId = Id;

        if (oldId == newId)
        {
            return;
        }

        // Initiate local refactor operation
        refactor?.Rename(oldId, newId);

        _oldId = newId;
    }

    #endregion

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    public virtual AnalysisResult Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems.
    /// </summary>
    /// <param name="problems">The problem collector.</param>
    /// <param name="intent">The analysis intent.</param>
    public virtual void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    { }

    #endregion

    /// <summary>
    /// Gets the parent of the specified type.
    /// </summary>
    /// <typeparam name="T">The parent type.</typeparam>
    /// <returns>The parent, or null.</returns>
    public T GetParent<T>() where T : class => (List as SNamedFieldList)?.ParentItem as T;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        if (sync.Intent == SyncIntent.Serialize && sync.IsNameOf("AssetId"))
        {
            if (sync.IsGetter())
            {
                Guid id = this.RecorededFieldId;
                if (id != Guid.Empty)
                {
                    sync.Sync("AssetId", this.Id);
                }
            }
            else if (sync.IsSetter())
            {
                Guid id = sync.Sync("AssetId", this.Id);
                _recordedId = id;
            }
        }

        if (sync.Intent == SyncIntent.Clone)
        {
            _oldId = sync.Sync("_oldId", _oldId);
        }
    }

}