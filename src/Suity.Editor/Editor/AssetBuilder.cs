using Suity.Editor.Analyzing;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Provides a base class for building and managing assets in the Suity system.
/// </summary>
public abstract class AssetBuilder
{
    // TODO: AssetBuilder needs to be optimized: AssetBuilder should not save specific data, but keep a reference to the document?
    // When a request is made to build a resource, the document is opened directly to read the data in it.
    // Otherwise, the process will be too complicated. Currently, the document pushes data/fields to the Builder, and the Builder pushes them to the resource, adding an additional storage step in between.
    // The storage part should be removed and the document data should be written directly to the resource.

    private string _localName = string.Empty;
    private AssetAccessMode _accessMode = AssetAccessMode.Public;
    private AssetInstanceMode _instanceMode = AssetInstanceMode.Normal;
    private string _description;
    private string _previewText;
    private Guid _iconId;
    private Color? _color;
    private AnalysisProblem _problems;
    private bool _isLegacy;
    private bool _isPrimary;
    private string _nameSpace;
    private string _importedId;
    private ISyncObject _metaData;
    private IGroupAssetBuilder _parent;

    private Guid _recordedId;

    internal AssetBuilder()
    { }

    /// <summary>
    /// Gets or sets the parent group asset builder.
    /// </summary>
    public IGroupAssetBuilder Parent { get => _parent; internal set => _parent = value; }

    /// <summary>
    /// Gets the local name of the asset.
    /// </summary>
    public string LocalName => _localName;

    /// <summary>
    /// Gets the target asset being built.
    /// </summary>
    public abstract Asset TargetAsset { get; }

    /// <summary>
    /// Gets the type of the asset.
    /// </summary>
    public abstract Type AssetType { get; }

    /// <summary>
    /// Gets or sets the owner of the asset builder.
    /// </summary>
    public object Owner { get; set; }

    /// <summary>
    /// Gets the recorded ID of the asset.
    /// </summary>
    public Guid RecordedId => _recordedId;

    /// <summary>
    /// Occurs when the asset has been replaced.
    /// </summary>
    public event EventHandler<ReplaceEntryEventArgs> AssetChanged;

    /// <summary>
    /// Sets the local name of the asset.
    /// </summary>
    /// <param name="localName">The local name to set.</param>
    public void SetLocalName(string localName)
    {
        localName ??= string.Empty;

        if (_localName == localName)
        {
            return;
        }

        _localName = localName;

        DoUpdateAction(o => o.LocalName = localName);
        OnNameUpdated();
    }

    /// <summary>
    /// Sets the access mode of the asset.
    /// </summary>
    /// <param name="accessMode">The access mode to set.</param>
    public void SetAccessMode(AssetAccessMode accessMode)
    {
        _accessMode = accessMode;
        DoUpdateAction(o => o.AccessMode = accessMode);
    }

    /// <summary>
    /// Sets the instance mode of the asset.
    /// </summary>
    /// <param name="instanceMode">The instance mode to set.</param>
    public void SetInstanceMode(AssetInstanceMode instanceMode)
    {
        _instanceMode = instanceMode;
        DoUpdateAction(o => o.InstanceMode = instanceMode);
    }

    /// <summary>
    /// Sets the description of the asset.
    /// </summary>
    /// <param name="description">The description to set.</param>
    public void SetDescription(string description)
    {
        _description = description;
        DoUpdateAction(o => o.Description = description);
    }

    /// <summary>
    /// Sets the preview text of the asset.
    /// </summary>
    /// <param name="previewText">The preview text to set.</param>
    public void SetPreviewText(string previewText)
    {
        _previewText = previewText;
        DoUpdateAction(o => o.PreviewText = previewText);
    }

    /// <summary>
    /// Sets the icon ID of the asset.
    /// </summary>
    /// <param name="id">The icon ID to set.</param>
    public void SetIconId(Guid id)
    {
        _iconId = id;
        DoUpdateAction(o => o.IconId = _iconId);
    }

    /// <summary>
    /// Sets the icon key of the asset.
    /// </summary>
    /// <param name="iconKey">The icon key to set.</param>
    public void SetIconKey(string iconKey)
    {
        _iconId = GlobalIdResolver.Resolve(iconKey);
        DoUpdateAction(o => o.IconId = _iconId);
    }

    /// <summary>
    /// Sets the view color of the asset.
    /// </summary>
    /// <param name="color">The color to set.</param>
    public void SetColor(Color? color)
    {
        if (color == Color.Empty)
        {
            color = null;
        }

        _color = color;
        DoUpdateAction(o => o.ViewColor = _color);
    }

    /// <summary>
    /// Sets the analysis problems associated with the asset.
    /// </summary>
    /// <param name="problems">The problems to set.</param>
    public void SetProblem(AnalysisProblem problems)
    {
        _problems = problems;
        DoUpdateAction(o => o.Problems = problems);
    }

    /// <summary>
    /// Sets whether the asset is a legacy asset.
    /// </summary>
    /// <param name="isLegacy">Whether the asset is legacy.</param>
    public void SetIsLegacy(bool isLegacy)
    {
        _isLegacy = isLegacy;
        DoUpdateAction(o => o.IsLegacy = isLegacy);
    }

    /// <summary>
    /// Sets whether the asset is a primary asset.
    /// </summary>
    /// <param name="isPrimary">Whether the asset is primary.</param>
    public void SetIsPrimary(bool isPrimary)
    {
        _isPrimary = isPrimary;
        DoUpdateAction(o => o.IsPrimary = isPrimary);
    }

    /// <summary>
    /// Sets the namespace of the asset.
    /// </summary>
    /// <param name="nameSpace">The namespace to set.</param>
    public void SetNameSpace(string nameSpace)
    {
        _nameSpace = nameSpace;
        DoUpdateAction(o => o.NameSpace = nameSpace);
    }

    /// <summary>
    /// Sets the imported ID of the asset.
    /// </summary>
    /// <param name="importedId">The imported ID to set.</param>
    public void SetImportedId(string importedId)
    {
        _importedId = importedId;
        DoUpdateAction(o => o.ImportedId = importedId);
    }

    /// <summary>
    /// Sets the metadata of the asset.
    /// </summary>
    /// <param name="metaData">The metadata to set.</param>
    public void SetMetaData(ISyncObject metaData)
    {
        _metaData = metaData;
        DoUpdateAction(o => o.MetaData = _metaData);
    }

    /// <summary>
    /// Sets the recorded ID of the asset.
    /// </summary>
    /// <param name="id">The recorded ID to set.</param>
    public void SetRecordedId(Guid id)
    {
        _recordedId = id;
    }

    /// <summary>
    /// Creates a new asset.
    /// </summary>
    public abstract void NewAsset();

    /// <summary>
    /// Detaches the asset from the builder.
    /// </summary>
    /// <param name="detachId">Whether to detach the asset ID.</param>
    public abstract void DetachAsset(bool detachId);

    /// <summary>
    /// Notifies that the asset has been updated.
    /// </summary>
    public void NotifyUpdated()
    {
        TargetAsset?.NotifyUpdated();
    }

    /// <summary>
    /// Notifies that the asset has been updated.
    /// </summary>
    /// <param name="delayed">Whether the notification is delayed.</param>
    public void NotifyUpdated(bool delayed)
    {
        TargetAsset?.NotifyUpdated(delayed);
    }

    /// <summary>
    /// Gets a value indicating whether the asset ID has been resolved.
    /// </summary>
    public bool IsIdResolved => TargetAsset?.Entry != null;

    /// <summary>
    /// Updates the asset.
    /// </summary>
    public abstract void UpdateAsset();

    /// <summary>
    /// Called when the local name has been updated.
    /// </summary>
    protected virtual void OnNameUpdated()
    {
    }

    /// <summary>
    /// Resolves the asset ID.
    /// </summary>
    /// <param name="resolveType">The type of ID resolution.</param>
    /// <returns>The resolved asset.</returns>
    internal Asset ResolveId(IdResolveType resolveType = IdResolveType.Auto)
    {
        var asset = TargetAsset;
        asset?.ResolveId(resolveType);
        return asset;
    }

    /// <summary>
    /// Detaches the asset ID.
    /// </summary>
    internal void DetachId()
    {
        var asset = TargetAsset;
        asset?.Entry = null;
    }

    /// <summary>
    /// Attaches an asset to the builder.
    /// </summary>
    /// <param name="asset">The asset to attach.</param>
    /// <returns>True if the asset was attached successfully; otherwise, false.</returns>
    internal abstract bool AttachAsset(Asset asset);

    /// <summary>
    /// Updates the base properties of the asset.
    /// </summary>
    /// <param name="o">The asset to update.</param>
    internal void OnUpdateAssetBase(Asset o)
    {
        if (o is null)
        {
            return;
        }

        o.LocalName = _localName;
        o.AccessMode = _accessMode;
        o.InstanceMode = _instanceMode;
        o.Description = _description;
        o.IconId = _iconId;
        o.ViewColor = _color;
        o.PreviewText = _previewText;
        o.IconId = _iconId;
        o.Problems = _problems;
        o.IsLegacy = _isLegacy;
        o.IsPrimary = _isPrimary;
        o.NameSpace = _nameSpace;
        o.ImportedId = _importedId;
        o.MetaData = _metaData;
    }

    /// <summary>
    /// Raises the AssetChanged event internally.
    /// </summary>
    /// <param name="oldAsset">The old asset.</param>
    internal void InternalRaiseAssetChanged(Asset oldAsset)
    {
        AssetChanged?.Invoke(this, new ReplaceEntryEventArgs(oldAsset, TargetAsset));
    }

    private void DoUpdateAction(Action<Asset> action)
    {
        if (TargetAsset is { } asset)
        {
            action(asset);
        }
    }
}

/// <summary>
/// Provides a generic base class for building and managing assets of a specific type.
/// </summary>
/// <typeparam name="TAsset">The type of asset to build.</typeparam>
public abstract class AssetBuilder<TAsset> : AssetBuilder
    where TAsset : Asset, new()
{
    private readonly AssetBuilderExternal<TAsset> _ex;

    /// <summary>
    /// Gets the type of the asset.
    /// </summary>
    public override Type AssetType => typeof(TAsset);

    /// <summary>
    /// Gets the target asset being built.
    /// </summary>
    public override Asset TargetAsset => _ex.Asset;

    /// <summary>
    /// Gets the asset instance.
    /// </summary>
    public TAsset Asset => _ex.Asset;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetBuilder{TAsset}"/> class.
    /// </summary>
    public AssetBuilder()
    {
        _ex = AssetManager.Instance.CreateBuilderExternal(this);
    }

    /// <summary>
    /// Creates a new asset.
    /// </summary>
    public override void NewAsset()
    {
        SetAsset(new TAsset());
    }

    /// <summary>
    /// Detaches the asset from the builder.
    /// </summary>
    /// <param name="detachId">Whether to detach the asset ID.</param>
    public override void DetachAsset(bool detachId)
    {
        _ex.DetachAsset(detachId);
    }

    /// <summary>
    /// Ensures that an asset exists, creating one if necessary.
    /// </summary>
    /// <returns>The asset instance.</returns>
    public TAsset EnsureAsset() => _ex.EnsureAsset();

    /// <summary>
    /// Sets the asset internally.
    /// </summary>
    /// <param name="value">The asset to set.</param>
    internal virtual void SetAsset(TAsset value) => _ex.SetAsset(value);

    /// <summary>
    /// Resolves the asset with the specified resolve type.
    /// </summary>
    /// <param name="resolveType">The type of ID resolution.</param>
    /// <returns>The resolved asset.</returns>
    public TAsset ResolveAsset(IdResolveType resolveType = IdResolveType.Auto) => _ex.ResolveAsset(resolveType);

    /// <summary>
    /// Updates the asset.
    /// </summary>
    public override void UpdateAsset() => _ex.UpdateAsset();

    #region Updater

    /// <summary>
    /// Adds an auto-update action for the asset.
    /// </summary>
    /// <param name="name">The name of the update action.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value update action.</returns>
    protected IValueUpdateAction AddAutoUpdate(string name, Action<TAsset> action)
        => _ex.AddAutoUpdate(name, action);

    /// <summary>
    /// Adds a value auto-update action for the asset.
    /// </summary>
    /// <typeparam name="TValue">The type of value to update.</typeparam>
    /// <param name="name">The name of the update action.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value update action.</returns>
    protected IValueUpdateAction<TValue> AddValueAutoUpdate<TValue>(string name, Action<TAsset, TValue> action)
        => _ex.AddValueAutoUpdate(name, action);

    /// <summary>
    /// Adds a reference auto-update action for the asset.
    /// </summary>
    /// <typeparam name="TRef">The type of reference to update.</typeparam>
    /// <param name="name">The name of the update action.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>The reference update action.</returns>
    protected IRefUpdateAction<TRef> AddRefAutoUpdate<TRef>(string name, Action<TAsset, Guid> action)
        where TRef : class
        => _ex.AddRefAutoUpdate<TRef>(name, action);

    /// <summary>
    /// Adds an element collector for the asset.
    /// </summary>
    /// <typeparam name="TValue">The type of element to collect.</typeparam>
    /// <param name="name">The name of the collector.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>The element collector.</returns>
    protected IAssetElementCollector<TValue> AddElementCollector<TValue>(string name, Action<TAsset, ICollection<TValue>> action)
        => _ex.AddElementCollector(name, action);

    /// <summary>
    /// Adds an element collector for the asset with key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="name">The name of the collector.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>The element collector.</returns>
    protected IAssetElementCollector<TKey, TValue> AddElementCollector<TKey, TValue>(string name, Action<TAsset, IDictionary<TKey, TValue>> action)
        => _ex.AddElementCollector(name, action);

    /// <summary>
    /// Adds a field collector for the asset.
    /// </summary>
    /// <typeparam name="TField">The type of field to collect.</typeparam>
    /// <param name="name">The name of the collector.</param>
    /// <param name="getCollection">Function to get the collection.</param>
    /// <param name="addOrUpdateAction">Action to add or update a field.</param>
    /// <param name="updateAction">Action to update a field.</param>
    /// <returns>The field collector.</returns>
    protected IAssetFieldCollector<TField> AddFieldCollector<TField>(
        string name,
        Func<TAsset, FieldObjectCollection<TField>> getCollection,
        FieldAddOrUpdateAction<TAsset, TField> addOrUpdateAction = null,
        FieldUpdateAction<TAsset, TField> updateAction = null)
        where TField : FieldObject, new()
        => _ex.AddFieldCollector(name, getCollection, addOrUpdateAction, updateAction);

    /// <summary>
    /// Tries to update the asset immediately.
    /// </summary>
    /// <param name="action">The action to perform on the asset.</param>
    /// <returns>True if the asset was updated; otherwise, false.</returns>
    protected internal bool TryUpdateNow(Action<TAsset> action)
    {
        var asset = _ex.Asset;
        if (asset != null)
        {
            action(asset);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Triggers an auto-update by name.
    /// </summary>
    /// <param name="name">The name of the update action.</param>
    protected internal void UpdateAuto(string name)
    {
        var asset = _ex.Asset;
        if (asset != null)
        {
            _ex.GetUpdater(name)?.DoAction(asset);
        }
    }

    #endregion

    /// <summary>
    /// Attaches an asset to the builder.
    /// </summary>
    /// <param name="asset">The asset to attach.</param>
    /// <returns>True if the asset was attached successfully; otherwise, false.</returns>
    internal override bool AttachAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (asset.GetType() != typeof(TAsset))
        {
            return false;
        }

        asset.Builder?.DetachAsset(true);
        SetAsset((TAsset)asset);

        return true;
    }

    /// <summary>
    /// Called internally when the asset is being updated.
    /// </summary>
    /// <param name="o">The asset to update.</param>
    internal virtual void OnUpdateAssetInternal(TAsset o)
    { }

    /// <summary>
    /// Called when the asset is being updated.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    protected internal virtual void OnUpdateAsset(TAsset asset)
    { }

    /// <summary>
    /// Called internally when the asset has changed.
    /// </summary>
    /// <param name="o">The asset that changed.</param>
    /// <param name="detachId">Whether to detach the ID.</param>
    internal virtual void OnAssetChangedInternal(TAsset o, bool detachId)
    { }

    /// <summary>
    /// Called when the asset has changed.
    /// </summary>
    /// <param name="old">The old asset.</param>
    /// <param name="detachId">Whether to detach the ID.</param>
    protected internal virtual void OnAssetChanged(TAsset old, bool detachId)
    { }

}