using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Internal backend implementation of <see cref="AssetExternal"/>, managing asset properties
/// including asset key, local name, namespace, file name, parent relationships, asset type entries,
/// resource names, and metadata file operations.
/// </summary>
internal class AssetExternalBK(Asset asset) : AssetExternal
{
    private readonly Asset _asset = asset ?? throw new ArgumentNullException(nameof(asset));

    private string _localName = string.Empty;
    private string _assetKey;
    private EditorAssetRef[] _assetTypes;
    private AssetKeyEntry _assetEntry;
    private List<IMultipleItemOp<string, Asset>> _assetEntriesByType;

    private INamedMultipleItemOp<Asset> _assetEntryByResourceName;
    private INamedMultipleItemOp<Asset> _assetEntryByLocalName;

    private StorageLocation _fileName;
    private string _nameSpace;
    private string _importedId;

    private MetaDataInfo _metaInfo;
    private bool _metaInfoLoaded;

    private InstanceAssetFilter _filter;
    private InstanceAssetFilter _filterInstance;

    private Asset _parentAsset;

    /// <inheritdoc/>
    public override string AssetKey => _assetKey;
    /// <inheritdoc/>
    public override AssetKeyEntry AssetEntry => _assetEntry;
    /// <inheritdoc/>
    public override IEnumerable<string> AssetTypeNames => _assetTypes?.Select(o => o.AssetKey) ?? [];

    /// <inheritdoc/>
    public override string ResourceName => _assetEntryByResourceName?.Name;
    /// <inheritdoc/>
    public override INamedMultipleItem<Asset> MultipleResourceNames => _assetEntryByResourceName;

    /// <inheritdoc/>
    public override string LocalName
    {
        get => _localName ?? string.Empty;
        internal set
        {
            value ??= string.Empty;
            string oldName = _localName ?? string.Empty;

            if (value == oldName)
            {
                return;
            }

            _localName = value;

            (_parentAsset as GroupAsset)?.ChangeChildName(_asset, oldName);

            UpdateAssetKey();
            UpdateResourceName();

            _asset.NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc/>
    public override string NameSpace
    {
        get
        {
            // Returns the explicitly set namespace, or inherits from parent asset
            if (!string.IsNullOrEmpty(_nameSpace))
            {
                return _nameSpace;
            }

            // Automatically get parent namespace
            return _parentAsset?.NameSpace ?? string.Empty;
        }
        internal set
        {
            if (_nameSpace == value)
            {
                return;
            }
            _nameSpace = value;

            UpdateResourceName();

            _asset.NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc/>
    public override string ShortTypeName
    {
        get
        {
            // Return file name without extension if available, otherwise use local name
            if (_fileName != null)
            {
                return Path.GetFileNameWithoutExtension(_fileName.FullPath);
            }
            else
            {
                return _localName ?? string.Empty;
            }
        }
    }

    /// <inheritdoc/>
    public override StorageLocation FileName
    {
        get => _fileName;
        internal set
        {
            _fileName = value;
            _asset.NotifyPropertyUpdated();
            UpdateResourceName();

            try
            {
                _asset.OnFileNameUpdated();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <inheritdoc/>
    public override string ImportedId
    {
        get => _importedId ?? string.Empty;
        internal set
        {
            if (value != null)
            {
                value = value?.Trim('.', '*', ':');
            }

            if (_importedId == value)
            {
                return;
            }

            _importedId = value;

            UpdateResourceName();

            _asset.NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc/>
    public override IAssetFilter GetInstanceFilter(bool instance)
    {
        // Returns a filter that matches or excludes instances of this asset type
        if (instance)
        {
            return _filterInstance ??= new InstanceAssetFilter(_asset, true);
        }
        else
        {
            return _filter ??= new InstanceAssetFilter(_asset, false);
        }
    }
    /// <inheritdoc/>
    public override bool IsIdDocumented
    {
        get
        {
            // If parent asset is an ID-documented asset, then this is a child asset of the ID-documented asset, and the ID is already documented
            if (_asset.Parent?.IsIdDocumented == true)
            {
                return true;
            }

            return _asset.Activator?.IsIdDocumented == true;
        }
    }


    #region Parent

    /// <inheritdoc/>
    public override Asset ParentAsset
    {
        get => _parentAsset;
        internal set
        {
            if (_parentAsset == value)
            {
                return;
            }

            _parentAsset = value;

            UpdateAssetKey();
            UpdateResourceName();

            try
            {
                _asset.OnParentChanged();
            }
            catch (Exception err)
            {
                err.LogError();
            }

            _asset.NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc/>
    public override Asset RootAsset => _parentAsset?._ex.RootAsset ?? _asset;

    /// <inheritdoc/>
    public override bool ContainsParent(Asset asset)
    {
        if (asset == _asset)
        {
            return true;
        }

        return _parentAsset?._ex.ContainsParent(asset) ?? false;
    }

    #endregion

    #region Asset key & type

    /// <inheritdoc/>
    public override void UpdateAssetKey()
    {
        // Rebuilds the asset key based on either parent or local name
        if (_parentAsset != null)
        {
            // AssetKey will be controlled by parent and shortName
            UpdateAssetKeyByParent();
        }
        else
        {
            UpdateAssetKeyByLocalName();
        }
    }

    /// <summary>
    /// Updates the asset key using only the local name of this asset.
    /// </summary>
    private void UpdateAssetKeyByLocalName()
    {
        UpdateAssetKey(_asset.LocalName);
    }

    /// <summary>
    /// Updates the asset key by combining the parent asset key with the local name.
    /// </summary>
    private void UpdateAssetKeyByParent()
    {
        var parentAssetKey = _parentAsset?.AssetKey;
        if (string.IsNullOrEmpty(parentAssetKey))
        {
            return;
        }

        string localName = _asset.LocalName;
        if (!string.IsNullOrEmpty(localName))
        {
            UpdateAssetKey(KeyCode.Combine(parentAssetKey, localName));
        }
        else
        {
            UpdateAssetKey($"{parentAssetKey}|");
        }
    }

    /// <summary>
    /// Updates the asset key to the specified value, handling entry removal/creation and activation/deactivation events.
    /// </summary>
    /// <param name="assetKey">The new asset key.</param>
    private void UpdateAssetKey(string assetKey)
    {
        if (assetKey == string.Empty)
        {
            assetKey = null;
        }

        if (_assetKey == assetKey)
        {
            return;
        }

        string oldKey = _assetKey;

        RemoveAssetEntry(false);
        RemoveAssetTypeEntry(false);

        if (_asset.Entry != null && !string.IsNullOrEmpty(oldKey))
        {
            _asset.InternalOnAssetDeactivate(oldKey);
        }

        _assetKey = assetKey;

        if (_asset.Entry != null && !string.IsNullOrEmpty(assetKey))
        {
            UpdateAssetEntry(false);
            UpdateAssetTypeEntry(false);

            _asset.InternalOnAssetActivate(assetKey);
        }

        if (oldKey != null && assetKey != null && _asset.Id != Guid.Empty)
        {
            GlobalIdResolver.Rename(oldKey, assetKey);
            _asset.NotifyUpdated(new RenameAssetEventArgs(oldKey, assetKey));
        }
    }

    /// <inheritdoc/>
    public override void UpdateAssetEntry(bool notify = true)
    {
        // Re-registers the asset in the asset manager's primary lookup
        if (_asset.Entry is null || string.IsNullOrEmpty(_assetKey))
        {
            return;
        }

        RemoveAssetEntry(notify);
        _assetEntry = AssetManager.Instance.AddAsset(_asset);
        if (notify)
        {
            _asset.NotifyUpdated();
        }
    }

    /// <inheritdoc/>
    public override void RemoveAssetEntry(bool notify = true)
    {
        // Removes the asset from the primary lookup
        var entry = _assetEntry;
        _assetEntry = null;
        entry?.Remove(_asset);
        if (notify)
        {
            _asset.NotifyUpdated();
        }
    }

    /// <inheritdoc/>
    public override void UpdateAssetTypeEntry(bool notify = true)
    {
        // Registers the asset in type-based collections (from concrete type up to Asset base)
        if (_asset.Entry is null || _asset.Id == Guid.Empty || string.IsNullOrEmpty(_assetKey))
        {
            return;
        }

        RemoveAssetTypeEntry(notify);
        List<IMultipleItemOp<string, Asset>> items = [];

        // Add entry from current type all the way up to Asset base class
        Type current = _asset.GetType();
        while (current != null && typeof(Asset).IsAssignableFrom(current))
        {
            if (AssetManager.Instance.AddAsset(current, _asset) is IMultipleItemOp<string, Asset> selfEntry)
            {
                items.Add(selfEntry);
            }
            current = current.BaseType;
        }

        if (_assetTypes != null)
        {
            foreach (var type in _assetTypes)
            {
                if (AssetManager.Instance.AddAsset(type, _asset) is IMultipleItemOp<string, Asset> entry)
                {
                    items.Add(entry);
                }
            }
        }

        _assetEntriesByType = items;
        if (notify)
        {
            _asset.NotifyUpdated();
        }
    }

    /// <inheritdoc/>
    public override void RemoveAssetTypeEntry(bool notify = true)
    {
        // Removes the asset from all type-based collections
        var items = _assetEntriesByType;
        _assetEntriesByType = null;
        if (items != null)
        {
            foreach (var item in items)
            {
                item.Remove(_asset);
            }
        }

        if (notify)
        {
            _asset.NotifyUpdated();
        }
    }

    #endregion

    #region Asset type

    /// <inheritdoc/>
    public override void UpdateAssetTypes(IEnumerable<string> assetTypes)
    {
        // Updates the asset type references from string keys
        _assetTypes = assetTypes.Select(str => new EditorAssetRef(str)).ToArray();

        UpdateAssetTypeEntry();
    }

    /// <inheritdoc/>
    public override void UpdateAssetTypes(IEnumerable<Type> types)
    {
        // Updates the asset type references from CLR types
        var assetTypes = types.Select(t => t.ResolveAssetTypeName());
        _assetTypes = assetTypes.Select(str => new EditorAssetRef(str)).ToArray();

        UpdateAssetTypeEntry();
    }

    #endregion

    #region Resource name

    /// <inheritdoc/>
    public override void UpdateResourceName()
    {
        // Recomputes and registers the resource name for this asset
        if (_asset.Entry is null || string.IsNullOrEmpty(_assetKey))
        {
            return;
        }

        string resourceName = _asset.ResolveResourceName();

        if (string.IsNullOrEmpty(resourceName))
        {
            return;
        }

        if (_assetEntryByResourceName?.Name == resourceName)
        {
            return;
        }

        RemoveResourceNameEntry(false);
        _assetEntryByResourceName = AssetManager.Instance.UpdateResourceName(_asset, resourceName) as INamedMultipleItemOp<Asset>;
        _assetEntryByLocalName = AssetManager.Instance.UpdateResourceName(_asset, _localName) as INamedMultipleItemOp<Asset>;

        _asset.NotifyPropertyUpdated(nameof(ResourceName));

        try
        {
            _asset.OnResourceNameUpdated();
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <inheritdoc/>
    public override void RemoveResourceNameEntry(bool notify = true)
    {
        // Removes the asset from resource name and local name lookups
        var entry = _assetEntryByResourceName;
        _assetEntryByResourceName = null;
        entry?.Remove(_asset);

        entry = _assetEntryByLocalName;
        _assetEntryByLocalName = null;
        entry?.Remove(_asset);

        if (notify)
        {
            _asset.NotifyPropertyUpdated(nameof(ResourceName));

            try
            {
                _asset.OnResourceNameUpdated();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <inheritdoc/>
    public override string ResolveDefaultResourceName()
    {
        // Resolves the default resource name from imported ID, namespace, or short type name
        string importedId = ImportedId;

        if (!string.IsNullOrEmpty(importedId))
        {
            return importedId;
        }

        string nameSpace = NameSpace;
        string shortTypeName = ShortTypeName ?? string.Empty;

        string fullTypeName;

        if (!string.IsNullOrEmpty(nameSpace))
        {
            string terminal = shortTypeName.GetPathTerminal();
            fullTypeName = EditorUtility.CombineName(nameSpace, terminal);
        }
        else
        {
            // No namespace configured
            fullTypeName = null;
            // return shortTypeName;
        }

        return fullTypeName;
    }

    #endregion

    #region Meta

    /// <inheritdoc/>
    public override MetaDataInfo MetaInfo => _metaInfo;

    /// <inheritdoc/>
    public override void CheckLoadMetaFile()
    {
        // Loads the meta file if it hasn't been loaded yet
        if (!_metaInfoLoaded)
        {
            LoadMetaFile();
        }
    }

    /// <inheritdoc/>
    public override void LoadMetaFile()
    {
        // Loads the meta file associated with the asset's physical file
        var fileName = FileName;
        if (fileName?.PhysicFileName is null)
        {
            return;
        }

        string metaFile = fileName.PhysicFileName + Asset.MetaExtension;
        LoadMetaFile(metaFile);
    }

    /// <inheritdoc/>
    public override void LoadMetaFile(string fileName)
    {
        // Loads a meta file from the specified path
        _metaInfo = null;
        _metaInfoLoaded = false;

        if (!File.Exists(fileName))
        {
            return;
        }

        //INodeReader reader = XmlNodeReader.FromFile(fileName, false);

        try
        {
            _metaInfo = MetaDataInfo.Load(fileName);
            _metaInfoLoaded = true;
            _asset.OnMetaDataUpdated();
        }
        catch (Exception err)
        {
            _metaInfo = null;
            _metaInfoLoaded = false;
            err.LogError($"Load meta file failed : {fileName}.");
            return;
        }
    }

    /// <inheritdoc/>
    public override void SaveMetaFile(ISyncObject metadata)
    {
        // Saves the specified metadata to the meta file
        _metaInfo ??= new MetaDataInfo
            {
                _metadata = metadata
            };

        SaveMetaFile();
    }

    /// <inheritdoc/>
    public override void SaveMetaFile()
    {
        // Saves the current meta info to the asset's meta file
        var fileName = FileName;
        if (fileName?.PhysicFileName is null)
        {
            return;
        }

        _metaInfo ??= new MetaDataInfo();

        string metaFile = fileName.PhysicFileName + Asset.MetaExtension;

        EditorUtility.DoFileUnwatchedAction(() =>
        {
            try
            {
                MetaDataInfo.Save(_metaInfo, metaFile);
            }
            catch (Exception err)
            {
                err.LogError($"Save meta file failed : {metaFile}.");
            }
        });
    }

    /// <inheritdoc/>
    public override void SaveMetaFileDelayed()
    {
        // Queues a delayed meta file save action
        EditorUtility.AddDelayedAction(new DelaySaveMetaDataAction(this));

        // Need to notify asset update after metadata update
        _asset.NotifyUpdated();
    }

    /// <inheritdoc/>
    public override void RemoveMetaFile()
    {
        // Clears the meta info and notifies the asset
        _metaInfo = null;
        _asset.OnMetaDataUpdated();
    }

    /// <inheritdoc/>
    public override ISyncObject MetaData
    {
        get
        {
            // Gets the metadata from the file asset's meta info
            Asset fileAsset = _asset.GetFileAsset();
            if (fileAsset is null)
            {
                return null;
            }

            var metaStore = fileAsset._ex as AssetExternalBK;
            if (metaStore is null)
            {
                return null;
            }

            fileAsset.CheckLoadMetaFile();

            return metaStore._metaInfo?.MetaData;
        }
        set
        {
            // System initialization sets a null by default, need to forcefully ignore
            if (value is null)
            {
                return;
            }

            var fileAsset = _asset.GetFileAsset();
            if (fileAsset is null)
            {
                return;
            }

            fileAsset.SaveMetaFile(value);

        // Need to notify asset update after metadata update
            _asset.NotifyUpdated();
        }
    }

    /// <inheritdoc/>
    public override T GetMetaData<T>()
    {
        // Gets typed metadata, creating a new instance if none exists
        Asset fileAsset = _asset.GetFileAsset();
        if (fileAsset is null)
        {
            //Logs.LogWarning($"Cannot get file path for {this}.");
            return null;
        }

        var metaStore = fileAsset._ex as AssetExternalBK;
        if (metaStore is null)
        {
            return null;
        }

        fileAsset.CheckLoadMetaFile();

        metaStore._metaInfo ??= new MetaDataInfo();

        if (metaStore._metaInfo.MetaData is T metaData)
        {
            return metaData;
        }
        else
        {
            metaStore._metaInfo.MetaData = metaData = new T();
            fileAsset.SaveMetaFile();

            return metaData;
        }
    }

    /// <inheritdoc/>
    public override string PackageFullName
    {
        get
        {
            // Gets the package full name from the file asset's meta info
            var fileAsset = _asset.GetFileAsset();
            if (fileAsset is null)
            {
                return null;
            }

            var metaStore = fileAsset._ex as AssetExternalBK;
            if (metaStore is null)
            {
                return null;
            }

            fileAsset.CheckLoadMetaFile();

            return metaStore._metaInfo?.PackageFullName;
        }
        internal set
        {
            // Sets the package full name in the file asset's meta info
            var fileAsset = _asset.GetFileAsset();
            if (fileAsset is null)
            {
                return;
            }

            var metaStore = fileAsset._ex as AssetExternalBK;
            if (metaStore is null)
            {
                return;
            }

            metaStore._metaInfo ??= new()
                {
                    PackageFullName = value
                };

            fileAsset.SaveMetaFile();
        }
    }

    #endregion

    #region DelaySaveMetaDataAction

    private class DelaySaveMetaDataAction(AssetExternal value) : DelayedAction<AssetExternal>(value)
    {
        public override void DoAction()
        {
            Value.SaveMetaFile();
        }
    }

    #endregion
}