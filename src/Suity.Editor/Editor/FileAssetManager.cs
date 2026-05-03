using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// File assst manager
/// </summary>
public abstract class FileAssetManager
{
    private static FileAssetManager _current;

    public static FileAssetManager Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Project.Current?.FileAssetManager;

            return _current;
        }
        internal set
        {
            _current = value;
        }
    }

    public FileAssetManager(string basePath)
    {
        if (basePath is null)
        {
            throw new ArgumentNullException(nameof(basePath));
        }

        DirectoryBasePath = basePath.NormalizeDirectoryName();
    }

    /// <summary>
    /// Directory base path
    /// </summary>
    public string DirectoryBasePath { get; }

    /// <summary>
    /// Make the full path
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Return the full path</returns>
    public string MakeFullPath(string relativePath)
    {
        return relativePath.MakeFullPath(DirectoryBasePath);
    }

    /// <summary>
    /// Make the relative path
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>Return the relative path</returns>
    public string MakeRelativePath(string fullPath)
    {
        return fullPath.MakeRalativePath(DirectoryBasePath);
    }

    /// <summary>
    /// Get the asset name in short format through the full path
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>Return the asset name in short format</returns>
    public string MakeAssetKey(string fullPath)
    {
        return MakeRelativePath(fullPath).GetPathId();
    }

    /// <summary>
    /// Get the asset name in short format through the full path
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Return the asset name in short format</returns>
    public string MakeAssetKey(StorageLocation fileName)
    {
        return MakeRelativePath(fileName.FullPath).GetPathId();
    }


    public abstract void EnsureStorage(Asset asset);

    public abstract Asset GetAsset(string fullPath);

    public abstract ImageDef GetIcon(string fullPath);

    internal abstract Asset GetOrUpdateAsset(string fullPath);

    internal abstract Asset UpdateAsset(string fullPath);

    internal abstract IReferenceHost GetReferenceHost(string fullPath);

    internal abstract IReferenceHost EnsureReferenceHost(string fullPath);

    internal abstract void RemoveReferenceHost(string fullPath);

    internal abstract DocumentEntry GetDocumentEntry(EditorObject obj, bool tryLoadStorage);

    internal abstract TAssetBuilder LockedWithFileAsset<TAssetBuilder>(TAssetBuilder builder, StorageLocation location)
        where TAssetBuilder : AssetBuilder;
}