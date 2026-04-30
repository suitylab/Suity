using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Services;

/// <summary>
/// Manages a collection of file-based assets within a base directory, handling CRUD operations and reference hosts.
/// </summary>
internal class FileAssetCollection
{
    private readonly string _basePath;

    private readonly Dictionary<string, Asset> _assets = [];
    private readonly Dictionary<string, SyncReferenceHost> _refHosts = [];

    private readonly object _syncRoot = new();

    /// <summary>
    /// Creates a new file asset collection for the specified base path.
    /// </summary>
    /// <param name="basePath">The root directory for assets.</param>
    public FileAssetCollection(string basePath)
    {
        EditorServices.SystemLog.AddLog($"Creating FileAssetCollection : {basePath}...");

        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
    }

    /// <summary>
    /// Gets an asset by its file path.
    /// </summary>
    /// <param name="path">The file path to look up.</param>
    /// <returns>The asset, or null if not found.</returns>
    public Asset Get(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        StorageLocation fileName = StorageLocation.Create(path);
        if (fileName.PhysicFileName is null)
        {
            return null;
        }

        string rPath = MakeAssetKey(path);
        if (string.IsNullOrEmpty(rPath))
        {
            // File is outside asset folder, ignore
            return null;
        }

        lock (_syncRoot)
        {
            return _assets.GetValueSafe(rPath);
        }
    }

    /// <summary>
    /// Gets an existing asset or updates it if not found.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The existing or newly updated asset.</returns>
    public Asset GetOrUpdate(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        StorageLocation fileName = StorageLocation.Create(path);
        if (fileName.PhysicFileName is null)
        {
            return null;
        }

        string rPath = MakeAssetKey(path);
        if (string.IsNullOrEmpty(rPath))
        {
            // File is outside asset folder, ignore asset update
            return null;
        }

        lock (_syncRoot)
        {
            Asset currentAsset = _assets.GetValueSafe(rPath);
            if (currentAsset != null)
            {
                return currentAsset;
            }
        }

        return Update(path);
    }

    /// <summary>
    /// Creates or updates an asset from the given file path.
    /// </summary>
    /// <param name="path">The file path to load from.</param>
    /// <returns>The created or updated asset, or null if loading failed.</returns>
    public Asset Update(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        StorageLocation fileName = StorageLocation.Create(path);
        if (fileName.PhysicFileName is null)
        {
            return null;
        }

        string rPath = MakeAssetKey(path);
        if (string.IsNullOrEmpty(rPath))
        {
            // File is outside asset folder, ignore asset update
            return null;
        }

        Asset asset = null;

        string ext = Path.GetExtension(path);

        if (ext.IgnoreCaseEquals(Asset.MetaExtension))
        {
            asset = Get(path.RemoveExtension());
            asset?.LoadMetaFile(path);
            return asset;
        }
        else
        {
            // Get asset factory by file extension
            // Process triggers document loading, document loading triggers update action
            //TODO: Bind DocumentFactory with AssetActivator
            AssetActivator assetActivator = AssetActivatorManager.Instance.GetAssetActivator(ext);

            if (assetActivator is null)
            {
                return null;
            }
            else
            {
                try
                {
                    asset = assetActivator.CreateAsset(path, rPath);
                }
                catch (Exception ex)
                {
                    ex.LogError($"Asset load failed : {path}");
                    return null;
                }

                if (asset is null)
                {
                    return null;
                }

                asset.Activator = assetActivator;

                Asset currentAsset = null;
                bool removed = false;
                lock (_syncRoot)
                {
                    currentAsset = _assets.GetValueSafe(rPath);
                    if (currentAsset != null)
                    {
                        if (ReferenceEquals(currentAsset, asset))
                        {
                            return asset;
                        }
                        else
                        {
                            _assets.Remove(rPath);
                            removed = true;
                        }
                    }

                    if (removed)
                    {
                        currentAsset.Entry = null;
                        currentAsset.FileName = null;
                        Suity.Object.DestroyObject(currentAsset);
                    }

                    _assets.Add(rPath, asset);
                }

                // Set FileName first, setting LocalName will trigger event
                asset.FileName = fileName;
                asset.LocalName = rPath;
                asset.ResolveId();

                EditorServices.SystemLog.AddLog($"File asset created : {path}");
                // Can notify asset view, project view update
                QueuedAction.Do(() => AssetManager.Instance.NotifyAssetUpdated(asset, EntryEventArgs.Empty));

                return asset;
            }
        }
    }

    /// <summary>
    /// Removes an asset by its file path.
    /// </summary>
    /// <param name="path">The file path of the asset to remove.</param>
    /// <returns>True if the asset was found and removed.</returns>
    public bool Remove(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        StorageLocation fileName = StorageLocation.Create(path);
        if (fileName.PhysicFileName is null)
        {
            return false;
        }

        string rPath = MakeAssetKey(path);

        string ext = Path.GetExtension(path);

        Asset asset;
        if (ext.IgnoreCaseEquals(Asset.MetaExtension))
        {
            asset = Get(path.RemoveExtension());
            asset?.RemoveMetaFile();

            return true;
        }
        else
        {
            lock (_syncRoot)
            {
                SyncReferenceHost refHost = _refHosts.RemoveAndGet(rPath);
                refHost?.Remove();

                asset = _assets.RemoveAndGet(rPath);
                if (asset != null)
                {
                    asset.Entry = null;
                    asset.LocalName = null;
                    asset.FileName = null;
                    Suity.Object.DestroyObject(asset);

                    EditorServices.SystemLog.AddLog($"File asset removed : {path}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Changes the path of an asset from oldPath to newPath.
    /// </summary>
    /// <param name="oldPath">The current file path.</param>
    /// <param name="newPath">The new file path.</param>
    /// <returns>True if the path was changed successfully.</returns>
    public bool ChangePath(string oldPath, string newPath)
    {
        if (oldPath is null)
        {
            return false;
        }
        if (newPath is null)
        {
            return false;
        }
        if (oldPath == newPath)
        {
            return false;
        }

        string rOldPath = MakeAssetKey(oldPath);
        string rNewPath = MakeAssetKey(newPath);

        lock (_syncRoot)
        {
            // In order to move from old path to new path, the asset occupying the new path is forcibly removed first.
            Remove(newPath);

            SyncReferenceHost refHost = _refHosts.RemoveAndGet(rOldPath);
            if (refHost is DocumentReferenceHost docRefHost)
            {
                // Change path
                docRefHost.DocumentPath = newPath;
                _refHosts.Add(rNewPath, docRefHost);
                docRefHost.MarkDirty();
            }
            else
            {
                // Some documents have no external references, for example ImageAsset, so SyncReferenceHost will not be created
            }

            Asset currentAsset = _assets.RemoveAndGet(rOldPath);
            if (currentAsset != null)
            {
                // Set FileName first, setting LocalName will trigger event
                currentAsset.FileName = StorageLocation.Create(newPath);
                // Fix: AssetBuilder's LocalName also needs to be set, otherwise it will cause LocalName value to rollback to original value.
                currentAsset.Builder?.SetLocalName(rNewPath);
                currentAsset.LocalName = rNewPath;
                currentAsset._ex.UpdateAssetKey();
                _assets.Add(rNewPath, currentAsset);

                EditorServices.SystemLog.AddLog($"File asset changed : {oldPath} -> {newPath}");
                QueuedAction.Do(() => AssetManager.Instance.NotifyAssetUpdated(currentAsset, EntryEventArgs.Empty));

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the reference host for a given path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The reference host, or null if not found.</returns>
    public SyncReferenceHost GetReferenceHost(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        SyncReferenceHost host;

        string rPath = MakeAssetKey(path);
        if (rPath is null)
        {
            // External
            return null;
        }

        lock (_syncRoot)
        {
            host = _refHosts.GetValueSafe(rPath);
        }

        return host;
    }

    /// <summary>
    /// Gets or creates a reference host for the given path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The reference host, or null if the path is external.</returns>
    public SyncReferenceHost EnsureReferenceHost(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        SyncReferenceHost host;

        string rPath = MakeAssetKey(path);
        if (rPath is null)
        {
            // External
            return null;
        }

        lock (_syncRoot)
        {
            host = _refHosts.GetOrAdd(rPath, _ => new DocumentReferenceHost(path));
        }

        host.MarkDirty();
        return host;
    }

    /// <summary>
    /// Removes the reference host for the given path.
    /// </summary>
    /// <param name="path">The file path.</param>
    public void RemoveReferenceHost(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string rPath = MakeAssetKey(path);

        lock (_syncRoot)
        {
            SyncReferenceHost refHost = _refHosts.RemoveAndGet(rPath);
            refHost?.Remove();
        }
    }

    /// <summary>
    /// Converts a full file path to an asset key relative to the base path.
    /// </summary>
    /// <param name="fullPath">The full file path.</param>
    /// <returns>The asset key, or null if the path is outside the base directory.</returns>
    public string MakeAssetKey(string fullPath)
    {
        try
        {
            string rPath = fullPath.MakeRalativePath(_basePath).GetPathId();
            if (rPath.StartsWith("../"))
            {
                return null;
            }

            return rPath;
        }
        catch (Exception)
        {
            return fullPath;
        }
    }
}
