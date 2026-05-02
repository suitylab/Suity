using ICSharpCode.SharpZipLib.Zip;
using Suity;
using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.NodeQuery;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.Libraries;

/// <summary>
/// A library asset implementation that loads and manages content from a zip-based library archive.
/// </summary>
public class LibraryAssetBK : LibraryAsset
{
    /// <summary>
    /// The encryption password for library archives.
    /// </summary>
    internal const string _xx = "Suity.Library.{240CC6F3-0B53-4A8C-85EB-7EF525BD2B15}.{8321EB1B-D989-4788-8BC9-3F6559470360}";

    private string _libraryName;
    private string _libraryVersion;
    private readonly Dictionary<string, LibraryEntry> _contentAssets = [];
    private readonly Dictionary<string, Asset> _contentAssetsByAssetKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, Asset> _contentAssetsByGuid = [];

    /// <inheritdoc/>
    public override Image DefaultIcon => CoreIconCache.Library;

    /// <inheritdoc/>
    public override string LibraryName => _libraryName;

    /// <inheritdoc/>
    public override string LibraryVersion => _libraryVersion;

    /// <inheritdoc/>
    public override IEnumerable<Asset> ContentAssets => _contentAssets.Values.Select(o => o.Asset).OfType<Asset>();

    /// <inheritdoc/>
    public override Asset GetContentAsset(string assetKey)
    {
        return _contentAssetsByAssetKey.GetValueSafe(assetKey);
    }

    /// <inheritdoc/>
    public override Asset GetContentAsset(Guid id)
    {
        return _contentAssetsByGuid.GetValueSafe(id);
    }

    /// <summary>
    /// Gets a storage item for the specified asset key.
    /// </summary>
    /// <param name="assetKey">The asset key to locate.</param>
    /// <returns>The storage item, or null if not found.</returns>
    internal IStorageItem GetStream(string assetKey)
    {
        if (FileName?.PhysicFileName != null && _contentAssets.TryGetValue(assetKey, out LibraryEntry entry))
        {
            return new LibraryStorageItem(FileName.PhysicFileName, entry.Index);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a storage item for the specified archive entry index.
    /// </summary>
    /// <param name="index">The index within the archive.</param>
    /// <returns>The storage item, or null if the file name is not set.</returns>
    internal IStorageItem GetStream(int index)
    {
        if (FileName?.PhysicFileName is null)
        {
            return null;
        }

        return new LibraryStorageItem(FileName.PhysicFileName, index);
    }

    /// <inheritdoc/>
    protected override void OnIdAttached(Guid id)
    {
        base.OnIdAttached(id);

        if (FileName?.PhysicFileName != null)
        {
            try
            {
                ImportLibrary(FileName.PhysicFileName, _xx);
            }
            catch (Exception err)
            {
                err.LogError($"Load library failed : {FileName}");
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnIdDetached(Guid id)
    {
        base.OnIdDetached(id);

        CleanUp();
    }

    /// <summary>
    /// Imports library content from a zip archive file.
    /// </summary>
    /// <param name="fileName">The path to the library archive file.</param>
    /// <param name="password">The password for encrypted archives.</param>
    private void ImportLibrary(string fileName, string password)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        CleanUp();

        // TODO: Need to add Iteration grouping sequential execution, need to add document format type judgment

        using (Stream fs = File.OpenRead(fileName))
        using (var zf = new ZipFile(fs))
        {
            if (!string.IsNullOrEmpty(password))
            {
                // AES encrypted entries are handled automatically
                zf.Password = password;
            }

            int manifestIndex = zf.FindEntry("manifest.xml", false);
            if (manifestIndex < 0)
            {
                throw new InvalidOperationException("Library file manifest not found.");
            }

            var manifestEntry = zf[manifestIndex];

            using var zipStream = zf.GetInputStream(manifestEntry);
            var reader = XmlNodeReader.FromStream(zipStream);
            if (reader.NodeName != "manifest")
            {
                ThrowFormatException();
            }

            var libReader = reader.Node("library");
            if (libReader is null)
            {
                ThrowFormatException();
            }

            _libraryName = libReader.GetAttribute("name");
            _libraryVersion = libReader.GetAttribute("version");

            foreach (var fileReader in libReader.Nodes())
            {
                if (fileReader.NodeName == "file")
                {
                    string name = fileReader.GetAttribute("name");
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    int fileIndex = zf.FindEntry($"Assets/{name}", false);
                    if (fileIndex < 0)
                    {
                        continue;
                    }

                    _contentAssets[name] = new LibraryEntry
                    {
                        Location = name,
                        Index = fileIndex,
                    };
                }
                else if (fileReader.NodeName == "bunch")
                {
                    string bunchName = fileReader.GetAttribute("name");
                    if (string.IsNullOrEmpty(bunchName))
                    {
                        continue;
                    }

                    LibraryEntry entry = new()
                    {
                        Location = bunchName,
                        Index = -1,
                        ChildEntries = [],
                    };
                    _contentAssets[bunchName] = entry;

                    foreach (var bunchReader in fileReader.Nodes("file"))
                    {
                        string name = bunchReader.GetAttribute("name");
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        int fileIndex = zf.FindEntry($"Assets/{bunchName}/{name}", false);
                        if (fileIndex < 0)
                        {
                            continue;
                        }

                        entry.ChildEntries.Add(new LibraryEntry
                        {
                            Location = name,
                            Index = fileIndex,
                        });
                    }
                }
            }
        }

        foreach (var entry in _contentAssets.Values)
        {
            var entryFileName = new StorageLocation(LibraryStorageType, $"{this.Id}/{entry.Location}");
            string assetKey = entry.Location.GetPathId();

            if (entry.Index >= 0)
            {
                if (DocumentManager.Instance.OpenDocument(entryFileName)?.Content is AssetDocument doc)
                {
                    var builder = doc.AssetBuilder?.WithLocalName(assetKey).WithAsset();
                    if (builder != null)
                    {
                        var asset = builder.TargetAsset;

                        if (asset != null)
                        {
                            asset.FileName = entryFileName;
                            asset.Library = this;
                            entry.Asset = asset;

                            if (!string.IsNullOrEmpty(asset.AssetKey))
                            {
                                _contentAssetsByAssetKey[asset.AssetKey] = asset;
                            }
                        }

                        builder.LockedResolveId(IdResolveType.FullName);

                        if (asset != null && asset.Id != Guid.Empty)
                        {
                            _contentAssetsByGuid[asset.Id] = asset;
                        }
                    }
                }
                else
                {
                    string ext = Path.GetExtension(entryFileName.FullPath);
                    AssetActivator assetActivator = AssetActivatorManager.Instance.GetAssetActivator(ext);
                    if (assetActivator != null)
                    {
                        try
                        {
                            var asset = assetActivator.CreateAsset(entryFileName.FullPath, entry.Location);

                            if (asset != null)
                            {
                                asset.Activator = assetActivator;
                                asset.FileName = entryFileName;
                                asset.Library = this;
                                asset.LocalName = assetKey;
                                entry.Asset = asset;

                                if (!string.IsNullOrEmpty(asset.AssetKey))
                                {
                                    _contentAssetsByAssetKey[asset.AssetKey] = asset;
                                }

                                asset.ResolveId(IdResolveType.FullName);

                                if (asset.Id != Guid.Empty)
                                {
                                    _contentAssetsByGuid[asset.Id] = asset;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.LogError($"Library asset load failed : {entryFileName.FullPath}");
                        }
                    }
                }
            }
            else if (entry.ChildEntries != null)
            {
                // Bunch

                var asset = new LibFileBunchAsset(entry)
                {
                    FileName = entryFileName,
                    Library = this
                };
                asset.ResolveId(IdResolveType.FullName);
                entry.Asset = asset;

                if (!string.IsNullOrEmpty(asset.AssetKey))
                {
                    _contentAssetsByAssetKey[asset.AssetKey] = asset;
                }

                if (asset.Id != Guid.Empty)
                {
                    _contentAssetsByGuid[asset.Id] = asset;
                }
            }
        }
    }

    /// <summary>
    /// Cleans up all loaded content assets and resets internal state.
    /// </summary>
    private void CleanUp()
    {
        _libraryName = null;
        _libraryVersion = null;

        try
        {
            foreach (var entry in _contentAssets.Values)
            {
                if (entry.Asset != null)
                {
                    entry.Asset.Entry = null;
                    entry.Asset.FileName = null;
                    entry.Asset.Library = null;
                    Object.DestroyObject(entry.Asset);
                }
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
        finally
        {
            _contentAssets.Clear();
            _contentAssetsByAssetKey.Clear();
            _contentAssetsByGuid.Clear();
        }
    }

    /// <summary>
    /// Throws a format exception indicating an invalid library manifest file.
    /// </summary>
    private void ThrowFormatException()
    {
        throw new FormatException("Library manifest file format is invalid.");
    }

    /// <summary>
    /// Attempts to locate an asset by its full storage path name.
    /// </summary>
    /// <param name="fullName">The full storage path name.</param>
    /// <param name="libraryAsset">When this method returns, contains the library asset if found; otherwise, null.</param>
    /// <param name="contentAsset">When this method returns, contains the content asset if found; otherwise, null.</param>
    /// <returns>True if the asset was found; otherwise, false.</returns>
    public static bool TryGetAssetByFullPath(string fullName, out LibraryAsset libraryAsset, out Asset contentAsset)
    {
        libraryAsset = null;
        contentAsset = null;

        if (!TryParseFullName(fullName, out Guid libraryId, out string contentAssetKey))
        {
            return false;
        }

        if (AssetManager.Instance.GetAsset(libraryId) is not LibraryAsset lib)
        {
            return false;
        }

        contentAsset = lib.GetContentAsset(contentAssetKey);
        return contentAsset != null;
    }

    /// <summary>
    /// Attempts to locate an asset by its location string.
    /// </summary>
    /// <param name="location">The location string in the format "libraryId/assetKey".</param>
    /// <param name="libraryAsset">When this method returns, contains the library asset if found; otherwise, null.</param>
    /// <param name="contentAsset">When this method returns, contains the content asset if found; otherwise, null.</param>
    /// <returns>True if the asset was found; otherwise, false.</returns>
    public static bool TryGetAssetByLocation(string location, out LibraryAsset libraryAsset, out Asset contentAsset)
    {
        libraryAsset = null;
        contentAsset = null;

        if (!TryParseLocation(location, out Guid libraryId, out string contentAssetKey))
        {
            return false;
        }

        if (AssetManager.Instance.GetAsset(libraryId) is not LibraryAsset lib)
        {
            return false;
        }

        contentAsset = lib.GetContentAsset(contentAssetKey);
        return contentAsset != null;
    }

    /// <summary>
    /// Parses a full storage name to extract the library ID and content asset key.
    /// </summary>
    /// <param name="fullName">The full storage name.</param>
    /// <param name="libraryId">When this method returns, contains the parsed library ID.</param>
    /// <param name="contentAssetKey">When this method returns, contains the parsed content asset key.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public static bool TryParseFullName(string fullName, out Guid libraryId, out string contentAssetKey)
    {
        libraryId = Guid.Empty;
        contentAssetKey = null;

        if (!StorageManager.Current.TryParseProvider(fullName, out string name, out string location))
        {
            return false;
        }

        if (name != LibraryStorageType)
        {
            return false;
        }

        return TryParseLocation(location, out libraryId, out contentAssetKey);
    }

    /// <summary>
    /// Parses a location string to extract the library ID and content asset key.
    /// </summary>
    /// <param name="location">The location string in the format "libraryId/assetKey".</param>
    /// <param name="libraryId">When this method returns, contains the parsed library ID.</param>
    /// <param name="contentAssetKey">When this method returns, contains the parsed content asset key.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public static bool TryParseLocation(string location, out Guid libraryId, out string contentAssetKey)
    {
        libraryId = Guid.Empty;
        contentAssetKey = null;

        if (string.IsNullOrEmpty(location))
        {
            return false;
        }

        int index = location.IndexOf('/');
        if (index < 0)
        {
            return false;
        }

        string s = location[..index];
        libraryId = GlobalIdResolver.Resolve(s);
        if (libraryId == Guid.Empty)
        {
            return false;
        }

        contentAssetKey = location.RemoveFromFirst(index + 1);
        return true;
    }
}

/// <summary>
/// An asset activator that creates LibraryAssetBK instances for library files.
/// </summary>
public class LibraryAssetActivatorBK : AssetActivator
{
    private static readonly string[] _extensions = ["suitylibrary"];

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new LibraryAssetBK();
    }

    /// <inheritdoc/>
    public override string[] GetExtensions()
    {
        return _extensions;
    }
}
