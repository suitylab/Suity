using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Internal backend implementation of <see cref="AssetManager"/>, handling asset registration,
/// lookup by key/id/type, asset collections, type bindings, resource names, and meta file operations.
/// Creates external objects for assets and manages asset lifecycle initialization.
/// </summary>
internal class AssetManagerBK : AssetManager
{
    /// <summary>
    /// Prefix string used for asset link keys.
    /// </summary>
    private static readonly string AssetLinkPrefix = AssetDefNames.AssetLinkPrefix + "|";

    /// <summary>
    /// Represents a binding between a type and its asset metadata including name, description, and asset link.
    /// </summary>
    private class BindingItem
    {
        /// <summary>
        /// The bound type.
        /// </summary>
        public Type Type { get; init; }
        /// <summary>
        /// The asset type name.
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// The masked type name used for asset identification.
        /// </summary>
        public string TypeName { get; init; }
        /// <summary>
        /// Description of the asset type binding.
        /// </summary>
        public string Description { get; init; }
        /// <summary>
        /// The asset link definition, if created.
        /// </summary>
        public DAssetLink AssetLink { get; set; }

        /// <inheritdoc/>
        public override string ToString() => TypeName;
    }

    /// <summary>
    /// The singleton instance of the backend asset manager.
    /// </summary>
    public new static readonly AssetManagerBK Instance = new();

    private readonly ConcurrentDictionary<string, AssetKeyEntry> _allAssets = new();
    private readonly ConcurrentDictionary<Guid, GeneralAssetCollection> _assetCollections = new();
    private readonly ConcurrentDictionary<string, NamedMultipleItem<Asset>> _assetsByResName = new();
    private readonly Dictionary<Type, DAssetLink> _assetLinks = [];
    private readonly Dictionary<Type, ITypedAssetResolver> _typedAssetResolvers = [];

    /// <summary>
    /// Creates a new instance of the backend asset manager. Private constructor enforces singleton pattern.
    /// </summary>
    private AssetManagerBK()
    {
    }

    /// <summary>
    /// Initializes the asset manager, setting up asset links, auto-start assets, and typed asset resolvers.
    /// This method should only be called once.
    /// </summary>
    internal void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        AssetManager.Instance = this;

        EditorRexes.EditorStart.AddActionListener(() => 
        {
            InitializeAssetLink();
            InitializeAutoStart();
            InitializeResolver();
        });
    }

    /// <summary>
    /// Initializes asset link definitions for all types derived from <see cref="Asset"/> and registered interface types.
    /// </summary>
    private void InitializeAssetLink()
    {
        _group = new GroupAssetBuilder()
            .WithLocalName(AssetDefNames.AssetLinkPrefix)
            .WithAsset()
            .WithBuilderId();

        var types = typeof(Asset).GetDerivedTypes()
            .Concat(NativeTypeReflector.Instance.InterfaceTypes);

        foreach (var type in types)
        {
            var item = GetAssetTypeBinding(type);
            if (item is null)
            {
                continue;
            }

            // Only types inheriting from Asset will automatically create
            item.AssetLink ??= new DAssetLink(type, item.Name, item.Description).WithGroup(_group).WithId();

            _assetLinks[type] = item.AssetLink;
        }
    }

    /// <summary>
    /// Initializes auto-start assets by finding all types with the <see cref="AssetAutoCreateAttribute"/>
    /// and creating instances of them.
    /// </summary>
    private void InitializeAutoStart()
    {
        var autoStarts = typeof(Asset).GetAvailableClassTypes()
            .Where(o => o.GetAttributeCached<AssetAutoCreateAttribute>() != null);

        foreach (var type in autoStarts)
        {
            try
            {
                var asset = Activator.CreateInstance(type) as Asset;
                asset?.ResolveId();
            }
            catch (Exception err)
            {
                err.LogError($"Auto create asset failed : {type.Name}");
            }
        }
    }

    /// <summary>
    /// Initializes all typed asset resolvers found in the assembly.
    /// </summary>
    private void InitializeResolver()
    {
        foreach (var resolver in DerivedTypeHelper.CreateDerivedClasses<ITypedAssetResolver>())
        {
            if (resolver.AssetType is not { } assetType)
            {
                continue;
            }

            if (_typedAssetResolvers.ContainsKey(assetType))
            {
                Logs.LogWarning($"Duplicate {nameof(ITypedAssetResolver)} : {resolver.GetType().FullName}, type={assetType.FullName}");
                continue;
            }

            _typedAssetResolvers.Add(assetType, resolver);
        }
    }


    /// <inheritdoc/>
    public override IEnumerable<string> AttachedAssetExtensions => AssetActivatorManager.Instance.AttachedAssetExtensions;

    /// <inheritdoc/>
    internal override AssetKeyEntry AddAsset(Asset asset)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        if (asset.Id == Guid.Empty)
        {
            throw new ArgumentException("Asset id is empty.");
        }

        var entry = EnsureAssetEntry(asset.AssetKey);
        entry.Add(asset);

        return entry;
    }

    /// <inheritdoc/>
    internal override IMultipleItem<string, Asset> AddAsset(EditorAssetRef assetType, Asset asset)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }
        if (asset.Id == Guid.Empty)
        {
            throw new ArgumentException("Asset id is empty.");
        }

        Guid typeId = assetType.Id;
        if (typeId != Guid.Empty)
        {
            var collection = _assetCollections.GetOrAdd(typeId, _ => new GeneralAssetCollection());
            var result = collection.AddAsset(asset.AssetKey, asset);
            return result;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    internal override IMultipleItem<string, Asset> AddAsset(Type assetType, Asset asset)
    {
        if (asset is null)
        {
            //throw new ArgumentNullException(nameof(asset));
            return null;
        }

        if (asset.Id == Guid.Empty && asset.Entry is null)
        {
            //throw new ArgumentException("Asset id is empty.");
            return null;
        }

        string typeName = ResolveAssetTypeName(assetType);
        Guid typeId = ResolveTypeName(typeName);

        if (typeId != Guid.Empty)
        {
            var collection = _assetCollections.GetOrAdd(typeId, _ => new GeneralAssetCollection());
            var result = collection.AddAsset(asset.AssetKey, asset);
            return result;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    internal override IEnumerable<Asset> Assets => _allAssets.SelectMany(pair => pair.Value.Targets);

    /// <inheritdoc/>
    internal override IEnumerable<Asset> GetAssetsByPrefix(string prefix)
    {
        return _allAssets.Where(pair => pair.Key.StartsWith(prefix)).SelectMany(pair => pair.Value.Targets);
    }

    /// <inheritdoc/>
    public override Asset GetAsset(Guid id)
    {
        return EditorObjectManager.Instance.GetObject(id) as Asset;
    }

    /// <inheritdoc/>
    public override Asset GetAsset(Guid id, IAssetFilter filter)
    {
        filter ??= AssetFilters.Default;

        if (EditorObjectManager.Instance.GetObject(id) is Asset asset && filter.FilterAsset(asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override T GetAsset<T>(Guid id)
    {
        //TODO : Consider filtering in Cargo
        return GetAsset(id) as T;
    }

    /// <inheritdoc/>
    public override T GetAsset<T>(Guid id, IAssetFilter filter)
    {
        //TODO : Consider filtering in Cargo
        return GetAsset(id, filter) as T;
    }

    /// <inheritdoc/>
    public override Asset GetAsset(string assetKey)
    {
        if (string.IsNullOrEmpty(assetKey))
        {
            return null;
        }

        return _allAssets.GetValueSafe(assetKey)?.Target;
    }

    /// <inheritdoc/>
    public override Asset GetAsset(string assetKey, IAssetFilter filter)
    {
        if (string.IsNullOrEmpty(assetKey))
        {
            return null;
        }

        Asset asset = _allAssets.GetValueSafe(assetKey)?.Target;
        if (asset != null && filter.FilterAsset(asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override T GetAsset<T>(string assetKey)
    {
        //TODO : Consider filtering in Cargo
        return GetAsset(assetKey) as T;
    }

    /// <inheritdoc/>
    public override T GetAsset<T>(string assetKey, IAssetFilter filter)
    {
        //TODO : Consider filtering in Cargo
        return GetAsset(assetKey, filter) as T;
    }

    /// <inheritdoc/>
    internal override AssetKeyEntry GetAssetEntry(string assetKey)
    {
        return _allAssets.GetValueSafe(assetKey);
    }

    /// <inheritdoc/>
    internal override AssetKeyEntry EnsureAssetEntry(string assetKey)
    {
        return _allAssets.GetOrAdd(assetKey, s => new AssetKeyEntryBK());
    }

    /// <inheritdoc/>
    public override Asset GetAsset(Guid assetTypeId, string assetKey)
    {
        if (assetTypeId == Guid.Empty || string.IsNullOrEmpty(assetKey))
        {
            return null;
        }

        return _assetCollections.GetValueSafe(assetTypeId)?.GetAsset(assetKey);
    }

    /// <inheritdoc/>
    public override Asset GetAsset(Guid assetTypeId, string assetKey, IAssetFilter filter)
    {
        Asset asset = GetAsset(assetTypeId, assetKey);

        if (asset != null && filter.FilterAsset(asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override Asset GetAsset(string assetType, string assetKey)
    {
        if (string.IsNullOrEmpty(assetType) || string.IsNullOrEmpty(assetKey))
        {
            return null;
        }

        Guid typeId = ResolveTypeName(assetType);

        if (typeId != Guid.Empty)
        {
            return GetAsset(typeId, assetKey);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override Asset GetAsset(string assetType, string assetKey, IAssetFilter filter)
    {
        Asset asset = GetAsset(assetType, assetKey, filter);

        if (asset != null && filter.FilterAsset(asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override IGeneralAssetCollection GetAssetCollection(Guid assetTypeId)
    {
        if (assetTypeId == Guid.Empty)
        {
            return null;
        }

        return _assetCollections.GetValueSafe(assetTypeId) as IGeneralAssetCollection ?? EmptyBaseAssetCollection.Empty;
    }

    /// <inheritdoc/>
    public override IGeneralAssetCollection GetAssetCollection(string assetType)
    {
        if (string.IsNullOrEmpty(assetType))
        {
            return null;
        }

        Guid typeId = ResolveTypeName(assetType);

        if (typeId != Guid.Empty)
        {
            return _assetCollections.GetValueSafe(typeId) as IGeneralAssetCollection ?? EmptyBaseAssetCollection.Empty;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override IGeneralAssetCollection GetAssetCollection(Type type)
    {
        string assetType = ResolveAssetTypeName(type);
        return GetAssetCollection(assetType) ?? EmptyBaseAssetCollection.Empty;
    }

    /// <inheritdoc/>
    public override IGeneralAssetCollection GetAssetCollection<T>()
    {
        string assetType = ResolveAssetTypeName(typeof(T));
        return GetAssetCollection(assetType) ?? EmptyBaseAssetCollection.Empty;
    }

    /// <inheritdoc/>
    public override IEnumerable<Asset> GetAssets(Guid assetTypeId, IAssetFilter filter)
    {
        if (assetTypeId == Guid.Empty)
        {
            return [];
        }

        return _assetCollections.GetValueSafe(assetTypeId).Assets.Where(o => filter.FilterAsset(o));
    }

    /// <inheritdoc/>
    public override IEnumerable<Asset> GetAssets(string assetType, IAssetFilter filter)
    {
        if (string.IsNullOrEmpty(assetType))
        {
            return [];
        }

        Guid typeId = ResolveTypeName(assetType);

        if (typeId != Guid.Empty)
        {
            return _assetCollections.GetValueSafe(typeId).Assets.Where(o => filter.FilterAsset(o));
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<Asset> GetAssets(Type type, IAssetFilter filter)
    {
        string assetType = ResolveAssetTypeName(type);
        var collection = GetAssetCollection(assetType);
        if (collection != null)
        {
            return collection.Assets.Where(o => filter.FilterAsset(o));
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<T> GetAssets<T>()
    {
        string assetType = ResolveAssetTypeName(typeof(T));
        var collection = GetAssetCollection(assetType);
        if (collection != null)
        {
            return collection.Assets.OfType<T>();
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<T> GetAssets<T>(IAssetFilter filter)
    {
        string assetType = ResolveAssetTypeName(typeof(T));

        if (GetAssetCollection(assetType) is { } collection)
        {
            return collection.Assets.Where(filter.FilterAsset).OfType<T>();
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public override DAssetLink GetAssetLink<T>()
    {
        return _assetLinks.GetValueSafe(typeof(T));
    }

    /// <inheritdoc/>
    public override DAssetLink GetAssetLink(Type type)
    {
        return _assetLinks.GetValueSafe(type);
    }


    /// <inheritdoc/>
    internal override AssetExternal CreateAssetExternal(Asset asset)
    {
        return new AssetExternalBK(asset);
    }

    /// <inheritdoc/>
    internal override GroupAssetExternal CreateGroupAssetExternal(GroupAsset groupAsset)
    {
        return new GroupAssetExternalBK(groupAsset);
    }

    /// <inheritdoc/>
    internal override AssetBuilderExternal<TAsset> CreateBuilderExternal<TAsset>(AssetBuilder<TAsset> builder)
    {
        return new AssetBuilderExternalBK<TAsset>(builder);
    }

    /// <inheritdoc/>
    internal override GroupAssetBuilderExternal<TGroupAsset> CreateGroupBuilderExternal<TGroupAsset>(GroupAssetBuilder<TGroupAsset> groupBuilder)
    {
        return new GroupAssetBuilderExternalBK<TGroupAsset>(groupBuilder);
    }

    #region AssetTypeBindings

    private static readonly Dictionary<Type, BindingItem> _typeNameBindings = [];
    private static readonly Dictionary<string, BindingItem> _typeNameStrs = [];

    private static readonly object _sync = new();
    private static bool _init;

    private static GroupAssetBuilder _group;

    /// <inheritdoc/>
    public override string ResolveAssetTypeName(Type type)
    {
        return GetAssetTypeBinding(type)?.TypeName;
    }

    /// <inheritdoc/>
    public override Type GetAssetType(string name)
    {
        return _typeNameStrs.GetValueSafe(name)?.Type;
    }

    private BindingItem GetAssetTypeBinding(Type type)
    {
        if (type is null)
        {
            return null;
        }

        string name;
        string description = null;

        lock (_sync)
        {
            if (_typeNameBindings.TryGetValue(type, out BindingItem item))
            {
                return item;
            }

            if (Attribute.GetCustomAttribute(type, typeof(AssetTypeBindingAttribute)) is AssetTypeBindingAttribute attr && !string.IsNullOrWhiteSpace(attr.TypeName))
            {
                name = attr.TypeName;
                description = attr.Description;
            }
            else
            {
                name = DefaultSyncTypeResolver.Instance.ResolveTypeName(type, null);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                string typeName = AssetDefNames.MaskAssetTypeName(name);

                item = new BindingItem
                {
                    Type = type,
                    Name = name,
                    TypeName = typeName,
                    Description = description,
                };

                _typeNameBindings[type] = item;
                _typeNameStrs[typeName] = item;

                if (_group != null)
                {
                    // Executing before initialization causes infinite loop; after initialization, each new Binding creates a DAssetLink automatically
                    // Disabling dynamic DAssetLink creation - only types inheriting from Asset create automatically
                    //item.AssetLink = new DAssetLink(name, description).WithGroup(_group).WithId();
                }

                return item;
            }
            else
            {
                return null;
            }
        }
    }

    #endregion

    #region CustomName & LocalName

    internal override INamedMultipleItem<Asset> UpdateResourceName(Asset asset, string fullTypeName)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        if (string.IsNullOrEmpty(fullTypeName))
        {
            return null;
        }

        var item = _assetsByResName.GetOrAdd(fullTypeName, _ => new NamedMultipleItem<Asset>(fullTypeName));
        item.Add(asset);

        return item;
    }

    /// <inheritdoc/>
    public override Asset GetAssetByResourceName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _assetsByResName.GetValueSafe(name)?.Value;
    }

    /// <inheritdoc/>
    public override Asset GetAssetByResourceName(Type type, string name)
    {
        if (type is null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (_typedAssetResolvers.TryGetValue(type, out var resolver))
        {
            var asset = resolver.ResolveAsset(name) as Asset;
            if (asset is not null)
            {
                return asset;
            }
        }

        var item = _assetsByResName.GetValueSafe(name);
        return item?.Value;
    }

    /// <inheritdoc/>
    public override T GetAssetByResourceName<T>(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (_typedAssetResolvers.TryGetValue(typeof(T), out var resolver))
        {
            var asset = resolver.ResolveAsset(name) as T;
            if (asset is not null)
            {
                return asset;
            }
        }

        var item = _assetsByResName.GetValueSafe(name);
        return item?.GetValue(o => o is T) as T;
    }

    /// <inheritdoc/>
    public override INamedMultipleItem<Asset> GetAssetsByResourceName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _assetsByResName.GetValueSafe(name);
    }

    #endregion

    private Guid ResolveTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return Guid.Empty;
        }

        if (typeName.StartsWith(AssetLinkPrefix))
        {
            string t = typeName[AssetLinkPrefix.Length..];
            if (_assetsByResName.TryGetValue(t, out var n) && n.Value is Asset asset)
            {
                return asset.Id;
            }
        }

        return GlobalIdResolver.Resolve(typeName);
    }

    /// <summary>
    /// Starts up all registered assets by calling their OnStartup method.
    /// Called after the project is fully initialized.
    /// </summary>
    internal void StartUp()
    {
        foreach (var entry in _allAssets.Values)
        {
            foreach (var asset in entry.Targets)
            {
                asset.OnStartup();
            }
        }
    }
}