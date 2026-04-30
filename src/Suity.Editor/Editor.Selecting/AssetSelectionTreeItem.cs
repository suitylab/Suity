using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;

namespace Suity.Editor.Selecting;

/// <summary>
/// Represents a tree item in an asset selection tree list, containing an asset reference and optional value.
/// </summary>
/// <typeparam name="TAsset">The type of asset.</typeparam>
/// <typeparam name="TValue">The type of value.</typeparam>
public class AssetSelectionTreeItem<TAsset, TValue> : IViewObject, ITextDisplay, INavigable, IReference, IHasId
    where TAsset : class
    where TValue : class, new()
{
    internal readonly EditorAssetRef<TAsset> _assetRef = new();
    internal TValue _value;

    /// <summary>
    /// Initializes a new instance of the AssetSelectionTreeItem class.
    /// </summary>
    public AssetSelectionTreeItem()
    {
        if (typeof(TValue) != typeof(EmptyValue))
        {
            _value = new TValue();
        }
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelectionTreeItem class with the specified key.
    /// </summary>
    /// <param name="key">The asset key.</param>
    public AssetSelectionTreeItem(string key)
        : this()
    {
        _assetRef.AssetKey = key;
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelectionTreeItem class with the specified id.
    /// </summary>
    /// <param name="id">The asset id.</param>
    public AssetSelectionTreeItem(Guid id)
        : this()
    {
        _assetRef.Id = id;
    }

    /// <summary>
    /// Gets the selected content asset.
    /// </summary>
    public TAsset SelectedContent => _assetRef.Target;

    /// <summary>
    /// Gets or sets the value associated with this item.
    /// </summary>
    public TValue Value => _value;

    /// <summary>
    /// Gets the asset key.
    /// </summary>
    public string Key => _assetRef.AssetKey;

    /// <summary>
    /// Gets the asset id.
    /// </summary>
    public Guid Id => _assetRef.Id;

    #region IViewObject

    /// <inheritdoc />
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        sync.SyncAssetRef(_assetRef, context);
        if (typeof(TValue) != typeof(EmptyValue))
        {
            sync.Sync("Value", _value, SyncFlag.GetOnly);
        }
    }

    /// <inheritdoc />
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        if (typeof(TValue) != typeof(EmptyValue))
        {
            setup.InspectorField(_value, new ViewProperty("Value", "Value") { Expand = true });
        }
    }

    #endregion

    #region IVisionTreeTextDisplay

    /// <inheritdoc />
    string ITextDisplay.DisplayText
    {
        get
        {
            return SelectedContent is Asset content ? content.DisplayText : "-";
        }
    }

    /// <inheritdoc />
    object ITextDisplay.DisplayIcon
    {
        get
        {
            Asset content = SelectedContent as Asset;
            return content?.Icon;
        }
    }

    /// <inheritdoc />
    TextStatus ITextDisplay.DisplayStatus => SelectedContent != null ? TextStatus.Normal : TextStatus.Error;

    #endregion

    #region INavigable

    /// <inheritdoc />
    object INavigable.GetNavigationTarget() => _assetRef.Id;

    #endregion

    #region IAssetReferencer

    /// <inheritdoc />
    void IReference.ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        _assetRef.Id = sync.SyncId(path, _assetRef.Id, null);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString()
    {
        return SelectedContent is Asset content ? content.DisplayText : "-";
    }
}