using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Editor;


internal abstract class AssetExternal
{
    public AssetExternal()
    { }

    public abstract string AssetKey { get; }
    public abstract AssetKeyEntry AssetEntry { get; }
    public abstract IEnumerable<string> AssetTypeNames { get; }

    public abstract string ResourceName { get; }
    public abstract INamedMultipleItem<Asset> MultipleResourceNames { get; }

    public abstract string LocalName { get; internal set; }
    public abstract string NameSpace { get; internal set; }
    public abstract string ShortTypeName { get; }
    public abstract StorageLocation FileName { get; internal set; }
    public abstract string ImportedId { get; internal set; }

    public abstract IAssetFilter GetInstanceFilter(bool instance);

    public abstract bool IsIdDocumented { get; }


    #region Parent

    public abstract Asset ParentAsset { get; internal set; }

    public abstract Asset RootAsset { get; }

    public abstract bool ContainsParent(Asset asset);

    #endregion

    #region Asset key & type

    public abstract void UpdateAssetKey();

    public abstract void UpdateAssetEntry(bool notify = true);

    public abstract void RemoveAssetEntry(bool notify = true);

    public abstract void UpdateAssetTypeEntry(bool notify = true);

    public abstract void RemoveAssetTypeEntry(bool notify = true);

    #endregion

    #region Asset type

    public abstract void UpdateAssetTypes(IEnumerable<string> assetTypes);

    public abstract void UpdateAssetTypes(IEnumerable<Type> types);

    #endregion

    #region Custom name

    public abstract void UpdateResourceName();

    public abstract void RemoveResourceNameEntry(bool notify = true);

    public abstract string ResolveDefaultResourceName();

    #endregion

    #region Meta

    public abstract MetaDataInfo MetaInfo { get; }

    public abstract void CheckLoadMetaFile();

    public abstract void LoadMetaFile();

    public abstract void LoadMetaFile(string fileName);

    public abstract void SaveMetaFile(ISyncObject metadata);

    public abstract void SaveMetaFile();

    public abstract void SaveMetaFileDelayed();

    public abstract void RemoveMetaFile();

    public abstract ISyncObject MetaData { get; set; }

    public abstract T GetMetaData<T>() where T : class, ISyncObject, new();

    public abstract string PackageFullName { get; internal set; }

    #endregion
}

internal abstract class AssetBuilderExternal<TAsset>
    where TAsset : Asset, new()
{
    public abstract TAsset Asset { get; }

    public abstract void SetAsset(TAsset value);

    public abstract TAsset EnsureAsset();

    public abstract void DetachAsset(bool detachId);

    public abstract TAsset ResolveAsset(IdResolveType resolveType);

    public abstract void UpdateAsset();

    public abstract IAssetUpdateAction<TAsset> GetUpdater(string name);

    public abstract IValueUpdateAction AddAutoUpdate(string name, Action<TAsset> action);

    public abstract IValueUpdateAction<TValue> AddValueAutoUpdate<TValue>(string name, Action<TAsset, TValue> action);

    public abstract IRefUpdateAction<TRef> AddRefAutoUpdate<TRef>(string name, Action<TAsset, Guid> action)
        where TRef : class;

    public abstract IAssetElementCollector<TValue> AddElementCollector<TValue>(string name, Action<TAsset, ICollection<TValue>> action);

    public abstract IAssetElementCollector<TKey, TValue> AddElementCollector<TKey, TValue>(string name, Action<TAsset, IDictionary<TKey, TValue>> action);

    public abstract IAssetFieldCollector<TField> AddFieldCollector<TField>(
        string name,
        Func<TAsset, FieldObjectCollection<TField>> getCollection,
        FieldAddOrUpdateAction<TAsset, TField> addOrUpdateAction = null,
        FieldUpdateAction<TAsset, TField> updateAction = null)
        where TField : FieldObject, new();
}

internal abstract class GroupAssetExternal
{
    internal GroupAssetExternal()
    { }

    public abstract IEnumerable<Asset> ChildAssets { get; }

    public abstract IEnumerable<string> ChildAssetLocalNames { get; }

    public abstract IEnumerable<Asset> GetChildAssets(IAssetFilter filter);

    public abstract IEnumerable<string> GetChildAssetLocalNames(IAssetFilter filter);

    public abstract int ChildAssetCount { get; }

    public abstract Asset GetChildAsset(string localName);

    public abstract Asset GetChildAsset(string localName, IAssetFilter filter);

    public abstract bool AddOrUpdateChildAsset(Asset asset, IdResolveType resolveType = IdResolveType.Auto);

    public abstract bool RemoveChildAsset(Asset asset);

    public abstract void ClearChildAssets();

    public abstract bool ChangeChildName(Asset childAsset, string oldName);

    public abstract void ActivateChildAssets();

    public abstract void DeactivateChildAssets();

    public abstract void AttachChildAssets();

    public abstract void DetachChildAssets();
}

internal abstract class GroupAssetBuilderExternal<TGroupAsset>
    where TGroupAsset : GroupAsset, new()
{
    public abstract void AddOrUpdate(AssetBuilder builder, IdResolveType resolveType);

    public abstract bool Remove(AssetBuilder builder);

    public abstract void UpdateProduct(TGroupAsset groupAsset);

    public abstract void Clear();

    public abstract void DetachChildProducts(bool detachId);

    public abstract AssetBuilder[] ChildBuilders { get; }

    public abstract int ChildCount { get; }
}