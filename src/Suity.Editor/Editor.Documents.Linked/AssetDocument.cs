using Suity.Editor.Services;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Base class for asset documents that support asset building and synchronization.
/// </summary>
public abstract class AssetDocument : Document,
    IViewElementOwner,
    IViewElementEditNotify,
    ITextDisplay,
    IServiceProvider,
    IHasAsset,
    IHasId,
    ILegacy,
    IViewLocateInProject,
    IValidate
{
    private string _nameSpace;
    private string _importName;

    private IReferenceHost _refHost;


    public AssetDocument()
    {
    }

    public AssetDocument(AssetBuilder builder)
        : this()
    {
        AssetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        AssetBuilder.Owner = this;
    }

    public override Image DefaultIcon => AssetBuilder?.TargetAsset?.Icon ?? this.GetType().ToDisplayIcon();


    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    public string NameSpace
    {
        get => _nameSpace;
        set
        {
            if (_nameSpace != value)
            {
                _nameSpace = value;
                AssetBuilder?.SetNameSpace(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the import name.
    /// </summary>
    public string ImportName
    {
        get => _importName;
        set
        {
            if (_importName == value)
            {
                return;
            }

            _importName = value;
            AssetBuilder.SetImportedId(value);
        }
    }

    /// <summary>
    /// Gets the preview field name.
    /// </summary>
    public string PreviewFieldName { get; internal protected set; }

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected internal AssetBuilder AssetBuilder { get; }

    /// <summary>
    /// Gets the reference host.
    /// </summary>
    internal IReferenceHost RefHost => _refHost;

    /// <summary>
    /// Gets the asset key.
    /// </summary>
    public string AssetKey => AssetBuilder?.TargetAsset?.AssetKey;

    /// <summary>
    /// Gets the ID.
    /// </summary>
    public Guid Id => AssetBuilder?.TargetAsset?.Id ?? Guid.Empty;

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    public Asset TargetAsset => AssetBuilder?.TargetAsset;


    protected override void OnViewEdited(object obj, string propertyName)
    {
        AssetBuilder?.NotifyUpdated();
    }

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <returns>True if successful.</returns>
    protected internal override bool NewDocument()
    {
        // Auto-fill NameSpace
        NameSpace = EditorServices.CurrentProject.ProjectName;

        return true;
    }

    #region IViewElementOwner

    public virtual object GetElement(string name) => null;

    #endregion

    #region IViewElementEditNotify

    void IViewElementEditNotify.NotifyViewElementEdited(IEnumerable<object> objs)
    {
        AssetBuilder?.NotifyUpdated();
        OnElementEdited(objs);
    }

    /// <summary>
    /// Called when an element is edited.
    /// </summary>
    /// <param name="objs">The edited objects.</param>
    protected virtual void OnElementEdited(IEnumerable<object> objs)
    {
    }

    #endregion

    #region New Open Save Dirty CleanUp

    /// <summary>
    /// Gets the sync type resolver.
    /// </summary>
    protected virtual ISyncTypeResolver SyncTypeResolver => null;

    /// <summary>
    /// Called when the document is created.
    /// </summary>
    protected internal override void OnCreated()
    {
        base.OnCreated();

        if (!string.IsNullOrEmpty(FileName.PhysicFileName))
        {
            _refHost = EditorUtility.EnsureReferenceHost(FileName.PhysicFileName);
        }
    }

    /// <summary>
    /// Called when the document becomes dirty.
    /// </summary>
    protected internal override void OnDirty()
    {
        base.OnDirty();

        _refHost?.MarkDirty();
    }

    /// <summary>
    /// Called when the document is loaded.
    /// </summary>
    protected internal override void OnLoaded(DocumentLoadingIntent intent)
    {
        base.OnLoaded(intent);

        _refHost?.MarkDirty();
    }

    /// <summary>
    /// Called when the document is saved.
    /// </summary>
    protected internal override void OnSaved()
    {
        base.OnSaved();

        AssetBuilder?.SetIsLegacy(false);
    }

    /// <summary>
    /// Called when the document is reset.
    /// </summary>
    protected internal override void OnReset()
    {
        base.OnReset();

        AssetBuilder?.DetachAsset(false);
        (AssetBuilder as IGroupAssetBuilder)?.Clear();
    }

    /// <summary>
    /// Called when the document is destroyed.
    /// </summary>
    protected internal override void OnDestroy()
    {
        base.OnDestroy();

        AssetBuilder?.DetachAsset(false);
    }

    #endregion

    #region ITextDisplay

    public virtual string DisplayText => Format?.DisplayText ?? string.Empty;

    object ITextDisplay.DisplayIcon => this.Icon;

    public TextStatus DisplayStatus => TextStatus.Normal;

    #endregion

    #region IServiceProvider

    public virtual object GetService(Type serviceType)
    {
        if (serviceType == typeof(ILegacy))
        {
            return this;
        }

        return null;
    }

    #endregion

    #region ILegacy

    void ILegacy.ReportLegacy() => AssetBuilder?.SetIsLegacy(true);

    #endregion

    #region IValidate

    /// <summary>
    /// Searches for text within the document.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="findStr">The search string.</param>
    /// <param name="findOption">The search option.</param>
    public virtual void Find(ValidationContext context, string findStr, SearchOption findOption)
    {
        if (Validator.Compare(_nameSpace, findStr, findOption))
        {
            context.Report(_nameSpace, this);
        }

        if (Validator.Compare(_importName, findStr, findOption))
        {
            context.Report(_importName, this);
        }
    }

    /// <summary>
    /// Validates the document.
    /// </summary>
    /// <param name="context">The validation context.</param>
    public virtual void Validate(ValidationContext context)
    {
    }

    #endregion
}

/// <summary>
/// Generic AssetDocument with a specific asset builder type.
/// </summary>
public abstract class AssetDocument<TAssetBuilder> : AssetDocument
    where TAssetBuilder : AssetBuilder, new()
{
    private readonly TAssetBuilder _builder;

    public AssetDocument()
        : base(new TAssetBuilder())
    {
        _builder = base.AssetBuilder as TAssetBuilder;
    }

    protected internal new TAssetBuilder AssetBuilder => _builder;
}