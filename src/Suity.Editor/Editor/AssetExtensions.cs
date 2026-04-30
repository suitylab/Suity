using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using static Suity.Helpers.GlobalLocalizer;
using Suity.Helpers;
using Suity.Selecting;
using System;
using System.Drawing;
using System.IO;
using Suity.Editor.Services;

namespace Suity.Editor;

/// <summary>
/// Provides extension methods for working with assets and editor objects.
/// </summary>
public static class AssetExtensions
{
    /// <summary>
    /// Converts a Guid to the name of the corresponding EditorObject.
    /// </summary>
    /// <param name="id">The Guid to convert.</param>
    /// <returns>The name of the EditorObject, or the Guid string if not found.</returns>
    public static string ToName(this Guid id)
    {
        if (id == Guid.Empty)
        {
            return string.Empty;
        }

        EditorObject obj = EditorObjectManager.Instance.GetObject(id);
        if (obj != null)
        {
            return obj.Name;
        }

        string revert = GlobalIdResolver.RevertResolve(id);
        if (!string.IsNullOrEmpty(revert))
        {
            return revert;
        }

        return id.ToString();
    }

    /// <summary>
    /// Converts a Guid to a description text for display.
    /// </summary>
    /// <param name="id">The Guid to convert.</param>
    /// <returns>The description text of the EditorObject, or the Guid string if not found.</returns>
    public static string ToDescriptionText(this Guid id)
    {
        if (id == Guid.Empty)
        {
            return string.Empty;
        }

        EditorObject obj = EditorObjectManager.Instance.GetObject(id);

        if (obj is Asset asset)
        {
            return asset.DisplayText;
        }
        else if (obj != null)
        {
            return obj.Name;
        }

        string revert = GlobalIdResolver.RevertResolve(id);
        if (!string.IsNullOrEmpty(revert))
        {
            return revert;
        }

        return id.ToString();
    }

    /// <summary>
    /// Converts an Asset to a description text for display.
    /// </summary>
    /// <param name="asset">The asset to convert.</param>
    /// <returns>The description text or name of the asset.</returns>
    public static string ToDescriptionText(this Asset asset)
    {
        if (EditorUtility.ShowAsDescription.Value)
        {
            // Ensured 'Description' defaults to null
            return asset.Description ?? asset.Name;
        }
        else
        {
            return asset.Name;
        }
    }

    /// <summary>
    /// Converts a Guid to a display text for the corresponding asset.
    /// </summary>
    /// <param name="id">The Guid to convert.</param>
    /// <returns>The display text of the asset, or the Guid string if not found.</returns>
    public static string ToDisplayText(this Guid id)
    {
        if (id == Guid.Empty)
        {
            return string.Empty;
        }

        EditorObject obj = EditorObjectManager.Instance.GetObject(id);

        if (obj is Asset asset)
        {
            if (EditorUtility.ShowAsDescription.Value)
            {
                return asset.DisplayText ?? asset.Name;
            }
            else
            {
                return asset.Name;
            }
        }
        else if (obj != null)
        {
            return obj.Name;
        }

        string revert = GlobalIdResolver.RevertResolve(id);
        if (!string.IsNullOrEmpty(revert))
        {
            return revert;
        }

        return id.ToString();
    }

    /// <summary>
    /// Converts an Asset to a display text.
    /// </summary>
    /// <param name="asset">The asset to convert.</param>
    /// <returns>The display text or name of the asset, or empty string if asset is null.</returns>
    public static string ToDisplayText(this Asset asset)
    {
        if (asset is null)
        {
            return string.Empty;
        }

        if (EditorUtility.ShowAsDescription.Value)
        {
            return asset.DisplayText ?? asset.Name;
        }

        return asset.Name;
    }

    /// <summary>
    /// Converts an Asset to a localized display text.
    /// </summary>
    /// <param name="asset">The asset to convert.</param>
    /// <returns>The localized display text or name of the asset, or empty string if asset is null.</returns>
    public static string ToDisplayTextL(this Asset asset)
    {
        if (asset is null)
        {
            return string.Empty;
        }

        if (EditorUtility.ShowAsDescription.Value)
        {
            if (!string.IsNullOrWhiteSpace(asset.DisplayText))
            {
                return L(asset.DisplayText);
            }
        }

        return asset.Name;
    }

    /// <summary>
    /// Gets the EditorObject corresponding to the given Guid.
    /// </summary>
    /// <param name="id">The Guid to look up.</param>
    /// <returns>The EditorObject, or null if not found.</returns>
    public static EditorObject ToEditorObject(this Guid id)
    {
        return EditorObjectManager.Instance.GetObject(id);
    }

    /// <summary>
    /// Gets the EditorObject corresponding to the given Guid.
    /// </summary>
    /// <param name="id">The Guid to look up.</param>
    /// <returns>The EditorObject, or null if not found.</returns>
    public static EditorObject ToAsset(this Guid id)
    {
        return AssetManager.Instance.GetAsset(id);
    }

    /// <summary>
    /// Determines whether the asset name is a system asset name (starts with "*").
    /// </summary>
    /// <param name="assetName">The asset name to check.</param>
    /// <returns>True if the asset name starts with "*", otherwise false.</returns>
    public static bool IsSystemAssetName(string assetName)
    {
        return assetName?.StartsWith("*") ?? false;
    }

    /// <summary>
    /// Gets the storage location for the EditorObject corresponding to the given Guid.
    /// </summary>
    /// <param name="id">The Guid to look up.</param>
    /// <returns>The storage location, or null if not found.</returns>
    public static StorageLocation GetStorageLocation(this Guid id)
    {
        var obj = EditorObjectManager.Instance.GetObject(id);
        if (obj != null)
        {
            return obj.GetStorageLocation();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the storage location for an editor object by traversing up the hierarchy.
    /// </summary>
    /// <param name="obj">The editor object to get the storage location for.</param>
    /// <returns>The storage location, or null if not found.</returns>
    public static StorageLocation GetStorageLocation(this EditorObject obj)
    {
        while (obj != null)
        {
            StorageLocation fileName = (obj as Asset)?.FileName;
            if (fileName != null)
            {
                return fileName;
            }

            obj = obj.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets the file asset containing this editor object by traversing up the hierarchy.
    /// </summary>
    /// <param name="obj">The editor object to get the file asset for.</param>
    /// <returns>The file asset, or null if not found.</returns>
    public static Asset GetFileAsset(this EditorObject obj)
    {
        while (obj != null)
        {
            Asset asset = obj as Asset;
            StorageLocation fileName = asset?.FileName;
            if (fileName != null)
            {
                return asset;
            }

            obj = obj.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets the root Asset of the editor object hierarchy.
    /// </summary>
    /// <param name="obj">The editor object to get the root asset for.</param>
    /// <returns>The root asset, or null if not found.</returns>
    public static Asset GetRootAsset(this EditorObject obj)
    {
        while (obj != null)
        {
            if (obj.Parent is Asset asset && asset.ParentAsset is null)
            {
                return asset;
            }

            obj = obj.Parent;
        }

        return null;
    }


    /// <summary>
    /// Gets the code library attached to the asset.
    /// </summary>
    /// <param name="asset">The asset to get the attached code library for.</param>
    /// <returns>The code library, or null if not found.</returns>
    public static ICodeLibrary GetAttachedUserLibrary(this Asset asset)
    {
        string userCodeFile = asset.GetAttachedUserLibraryFileName();
        if (!File.Exists(userCodeFile))
        {
            return null;
        }

        return EditorServices.FileAssetManager.GetAsset(userCodeFile) as ICodeLibrary;
    }

    /// <summary>
    /// Gets the file name of the code library attached to the asset.
    /// </summary>
    /// <param name="asset">The asset to get the attached code library file name for.</param>
    /// <returns>The code library file name, or null if not found.</returns>
    public static string GetAttachedUserLibraryFileName(this Asset asset)
    {
        StorageLocation fileName = asset.GetStorageLocation();
        if (fileName?.PhysicFileName is null)
        {
            return null;
        }

        return fileName.PhysicFileName + Asset.CodeLibraryExtension;
    }

    /// <summary>
    /// Gets the metadata file name attached to the asset.
    /// </summary>
    /// <param name="asset">The asset to get the attached metadata file name for.</param>
    /// <returns>The metadata file name, or null if not found.</returns>
    public static string GetAttachedMetaFileName(this Asset asset)
    {
        StorageLocation fileName = asset.GetStorageLocation();
        if (fileName?.PhysicFileName is null)
        {
            return null;
        }

        return fileName.PhysicFileName + Asset.MetaExtension;
    }

    /// <summary>
    /// Determines whether the file is an attached file (e.g., code library, metadata, or log file).
    /// </summary>
    /// <param name="fileInfo">The file information.</param>
    /// <returns>True if the file is an attached file.</returns>
    public static bool GetIsAttachedFile(this FileInfo fileInfo)
    {
        string ext = fileInfo.Extension;

        //return ext.IgnoreCaseEquals(Asset.CodeLibraryExtension)
        //    || ext.IgnoreCaseEquals(Asset.MetaExtension)
        //    || ext.IgnoreCaseEquals(Asset.LogExtension);

        foreach (var attachedExt in AssetManager.Instance.AttachedAssetExtensions)
        {
            if (ext.IgnoreCaseEquals(attachedExt))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the file is an attached file (e.g., code library, metadata, or log file).
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>True if the file is an attached file.</returns>
    public static bool GetIsAttachedFile(this string fileName)
    {
        return fileName.FileExtensionEquals(Asset.CodeLibraryExtension)
            || fileName.FileExtensionEquals(Asset.MetaExtension)
            || fileName.FileExtensionEquals(Asset.LogExtension);
    }

    /// <summary>
    /// Determines whether the file is a metadata file.
    /// </summary>
    /// <param name="fileInfo">The file information.</param>
    /// <returns>True if the file is a metadata file.</returns>
    public static bool GetIsMetaFile(this FileInfo fileInfo)
    {
        string ext = fileInfo.Extension;

        return ext.IgnoreCaseEquals(Asset.MetaExtension);
    }

    /// <summary>
    /// Determines whether the file name represents a metadata file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>True if the file name is a metadata file.</returns>
    public static bool GetIsMetaFile(this string fileName)
    {
        return fileName.FileExtensionEquals(Asset.MetaExtension);
    }

    /// <summary>
    /// Creates a filtered selection list from an asset collection.
    /// </summary>
    /// <typeparam name="TAsset">The type of assets in the collection.</typeparam>
    /// <param name="collection">The asset collection.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>A selection list with the filter applied.</returns>
    public static ISelectionList WithFilter<TAsset>(this IAssetCollection<TAsset> collection, IAssetFilter filter)
        where TAsset : Asset
    {
        collection ??= EmptyAssetCollection<TAsset>.Empty;

        return new AssetSelectionList<TAsset>(collection, filter);
    }

    /// <summary>
    /// Gets the small icon for the specified asset.
    /// </summary>
    /// <param name="asset">The asset to get the icon for.</param>
    /// <returns>The small icon image, or null if not found.</returns>
    public static Image ToIconSmall(this Asset asset)
    {
        if (AssetManager.Instance.GetAsset(asset.IconId, AssetFilters.Default) is ImageAsset imgRef)
        {
            return imgRef.GetIconSmall();
        }

        return null;
    }

    /// <summary>
    /// Adds the builder to a group asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <param name="group">The group asset builder.</param>
    /// <returns>The asset builder.</returns>
    public static T WithGroupBuilder<T>(this T builder, GroupAssetBuilder group) where T : AssetBuilder
    {
        group.AddOrUpdate(builder);
        return builder;
    }

    /// <summary>
    /// Adds the builder to a group asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <typeparam name="TGroupAsset">The type of group asset.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <param name="group">The group asset builder.</param>
    /// <returns>The asset builder.</returns>
    public static T WithGroupBuilder<T, TGroupAsset>(this T builder, GroupAssetBuilder<TGroupAsset> group)
        where T : AssetBuilder
        where TGroupAsset : GroupAsset, new()
    {
        group.AddOrUpdate(builder);
        return builder;
    }

    /// <summary>
    /// Sets the local name for the asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <param name="name">The local name to set.</param>
    /// <returns>The asset builder.</returns>
    public static T WithLocalName<T>(this T builder, string name) where T : AssetBuilder
    {
        builder.SetLocalName(name);
        return builder;
    }

    /// <summary>
    /// Sets the namespace for the asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <param name="nameSpace">The namespace to set.</param>
    /// <returns>The asset builder.</returns>
    public static T WithNameSpace<T>(this T builder, string nameSpace) where T : AssetBuilder
    {
        builder.SetNameSpace(nameSpace);
        return builder;
    }

    /// <summary>
    /// Adds the asset to a group asset.
    /// </summary>
    /// <typeparam name="T">The type of asset.</typeparam>
    /// <param name="asset">The asset to add to the group.</param>
    /// <param name="group">The group asset.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    /// <returns>The asset.</returns>
    internal static T WithGroup<T>(this T asset, GroupAsset group, IdResolveType resolveType = IdResolveType.Auto) where T : Asset
    {
        group.AddOrUpdateChildAsset(asset, resolveType);
        return asset;
    }

    /// <summary>
    /// Adds the asset to a group asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset.</typeparam>
    /// <param name="asset">The asset to add to the group.</param>
    /// <param name="group">The group asset builder.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    /// <returns>The asset.</returns>
    internal static T WithGroup<T>(this T asset, GroupAssetBuilder group, IdResolveType resolveType = IdResolveType.Auto) where T : Asset
    {
        asset.SetParent(group?.Asset, resolveType);
        return asset;
    }

    /// <summary>
    /// Adds the asset to a group asset builder.
    /// </summary>
    /// <typeparam name="T">The type of asset.</typeparam>
    /// <typeparam name="TGroupAsset">The type of group asset.</typeparam>
    /// <param name="asset">The asset to add to the group.</param>
    /// <param name="group">The group asset builder.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    /// <returns>The asset.</returns>
    internal static T WithGroup<T, TGroupAsset>(this T asset, GroupAssetBuilder<TGroupAsset> group, IdResolveType resolveType = IdResolveType.Auto) where T : Asset
        where TGroupAsset : GroupAsset, new()
    {
        asset.SetParent(group?.Asset, resolveType);
        return asset;
    }

    /// <summary>
    /// Resolves the ID for the asset.
    /// </summary>
    /// <typeparam name="T">The type of asset.</typeparam>
    /// <param name="asset">The asset to resolve the ID for.</param>
    /// <returns>The asset.</returns>
    internal static T WithId<T>(this T asset) where T : Asset
    {
        asset.ResolveId();
        return asset;
    }

    /// <summary>
    /// Creates a new asset from the builder if not already created.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <returns>The asset builder.</returns>
    internal static T WithAsset<T>(this T builder) where T : AssetBuilder
    {
        if (builder.TargetAsset is null)
        {
            builder.NewAsset();
        }

        return builder;
    }

    /// <summary>
    /// Resolves the builder ID if not already resolved.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <returns>The asset builder.</returns>
    internal static T WithBuilderId<T>(this T builder) where T : AssetBuilder
    {
        if (builder.TargetAsset is null)
        {
            builder.TargetAsset.ResolveId();
        }

        return builder;
    }

    /// <summary>
    /// Creates a new asset from the builder and resolves its ID.
    /// </summary>
    /// <typeparam name="T">The type of asset builder.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    /// <returns>The asset builder.</returns>
    internal static T WithResolvedProduct<T>(this T builder, IdResolveType resolveType = IdResolveType.Auto) where T : AssetBuilder
    {
        if (builder.TargetAsset is null)
        {
            builder.NewAsset();
        }

        builder.ResolveId(resolveType);
        return builder;
    }

    /// <summary>
    /// Gets the asset filter for the given context.
    /// </summary>
    /// <param name="context">The context containing the asset.</param>
    /// <param name="instance">Whether to get the instance filter.</param>
    /// <returns>The asset filter.</returns>
    public static IAssetFilter GetAssetFilter(this IHasAsset context, bool instance = false)
    {
        if (context is null)
        {
            throw new ArgumentNullException();
        }

        Asset asset = context.TargetAsset;
        if (asset != null)
        {
            return asset.GetInstanceFilter(instance);
        }
        else
        {
            return AssetFilters.All;
        }
    }

    /// <summary>
    /// Gets the asset filter for the given asset.
    /// </summary>
    /// <param name="asset">The asset to get the filter for.</param>
    /// <param name="instance">Whether to get the instance filter.</param>
    /// <returns>The asset filter.</returns>
    public static IAssetFilter GetAssetFilter(this Asset asset, bool instance = false)
    {
        if (asset is null)
        {
            throw new ArgumentNullException();
        }
        return asset.GetInstanceFilter(instance);
    }

    /// <summary>
    /// Sets the parent of the asset to the specified group asset.
    /// </summary>
    /// <param name="asset">The asset to set the parent for.</param>
    /// <param name="parent">The parent group asset.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    internal static void SetParent(this Asset asset, GroupAsset parent, IdResolveType resolveType = IdResolveType.Auto)
    {
        if (parent == asset.ParentAsset)
        {
            parent.AddOrUpdateChildAsset(asset, resolveType);
        }
        else
        {
            (asset.ParentAsset as GroupAsset)?.RemoveChildAsset(asset);
            parent?.AddOrUpdateChildAsset(asset, resolveType);
        }
    }

    /// <summary>
    /// Determines whether the asset is the same as or a parent of the given asset.
    /// </summary>
    /// <param name="asset">The asset to check.</param>
    /// <param name="parent">The potential parent asset.</param>
    /// <returns>True if asset is the same as or is a parent of the given asset.</returns>
    public static bool IsMeOrParent(this Asset asset, Asset parent)
    {
        if (parent is null)
        {
            return false;
        }

        Asset current = asset;
        while (current != null)
        {
            if (ReferenceEquals(current, parent))
            {
                return true;
            }

            current = current.ParentAsset;
        }

        return false;
    }

    /// <summary>
    /// Determines whether two assets share the same root asset.
    /// </summary>
    /// <param name="asset1">The first asset.</param>
    /// <param name="asset2">The second asset.</param>
    /// <returns>True if both assets have the same root asset.</returns>
    public static bool ShareSameRoot(this Asset asset1, Asset asset2)
    {
        if (asset1 is null || asset2 is null)
        {
            return false;
        }

        return ReferenceEquals(asset1.RootAsset, asset2.RootAsset);
    }

    /// <summary>
    /// Resolves the type name for an asset type.
    /// </summary>
    /// <param name="type">The type to resolve the name for.</param>
    /// <returns>The resolved type name.</returns>
    public static string ResolveAssetTypeName(this Type type) => AssetManager.Instance.ResolveAssetTypeName(type);

    /// <summary>
    /// Gets the data row from a data asset.
    /// </summary>
    /// <param name="dataRowAsset">The data asset.</param>
    /// <param name="tryLoadStorage">Whether to try loading from storage.</param>
    /// <returns>The data item, or null if not found.</returns>
    public static IDataItem GetDataRow(this IDataAsset dataRowAsset, bool tryLoadStorage)
    {
        if (dataRowAsset is not Asset asset)
        {
            return null;
        }

        if (asset.ParentAsset is not IDataTableAsset parentAsset)
        {
            return null;
        }

        var table = parentAsset.GetDataContainer(tryLoadStorage);
        if (table is null)
        {
            return null;
        }

        return table.GetData(asset.LocalName);
    }
}