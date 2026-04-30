using ICSharpCode.SharpZipLib.Zip;
using Suity.Editor.CodeRender;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.Libraries;

/// <summary>
/// Represents a file bunch asset that groups multiple files within a library archive.
/// </summary>
public sealed class LibFileBunchAsset : GroupAsset, IFileBunch
{
    private readonly LibraryEntry _libEntry;

    /// <summary>
    /// Initializes a new instance from a library entry.
    /// </summary>
    /// <param name="libEntry">The library entry containing file bunch data.</param>
    internal LibFileBunchAsset(LibraryEntry libEntry)
    {
        _libEntry = libEntry ?? throw new ArgumentNullException(nameof(libEntry));
        this.LocalName = libEntry.Location;

        if (libEntry.ChildEntries != null)
        {
            foreach (var item in libEntry.ChildEntries)
            {
                var element = new LibFileBunchElementAsset(item);
                AddOrUpdateChildAsset(element, IdResolveType.FullName);
            }
        }
    }

    /// <inheritdoc/>
    public override Image DefaultIcon => CoreIconCache.FileBunch;

    /// <inheritdoc/>
    public IEnumerable<RenderTarget> GetRenderTargets(RenderFileName basePath, bool uploadMode)
    {
        var dbFile = this.GetStorageLocation();
        List<RenderTarget> targets = [];

        if (uploadMode)
        {
            foreach (var element in ChildAssets.OfType<LibFileBunchElementAsset>())
            {
                targets.Add(new LibFileBunchEmptyRenderTarget(this, element, basePath.Append(element.FileId), dbFile, element.FileId));
            }
        }
        else
        {
            foreach (var element in ChildAssets.OfType<LibFileBunchElementAsset>())
            {
                targets.Add(new LibFileBunchRenderTarget(this, element, basePath.Append(element.FileId), dbFile, element.FileId));
            }
        }

        return targets;
    }

    /// <inheritdoc/>
    public bool DeleteFile(string fileId)
    {
        return false;
    }

    /// <inheritdoc/>
    public void CommitFiles(WorkSpace workSpace)
    {
    }

    /// <inheritdoc/>
    public IEnumerable<IFileBunchElement> Files => ChildAssets.OfType<LibFileBunchElementAsset>();

    /// <inheritdoc/>
    public void SaveToFiles(IEnumerable<IFileBunchElement> files, string directory)
    {
        using var fs = File.OpenRead(Library.FileName.PhysicFileName);
        using var zf = new ZipFile(fs);

        if (!string.IsNullOrEmpty(LibraryAssetBK._xx))
        {
            zf.Password = LibraryAssetBK._xx;
        }

        foreach (var file in files.OfType<LibFileBunchElementAsset>())
        {
            string fileName = directory.PathAppend(file.FileId);
            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var inputStream = zf.GetInputStream(file.Index);
            using var stream = File.Create(fileName);
            inputStream.CopyTo(stream);
        }
    }

    /// <inheritdoc/>
    public long Rebuild()
    {
        return 0;
    }

    /// <inheritdoc/>
    string IFileBunch.FileName => null;

    /// <inheritdoc/>
    public ICodeLibrary GetCodeLibrary()
    {
        return this.GetAttachedUserLibrary();
    }
}

/// <summary>
/// Represents a single file element within a file bunch asset.
/// </summary>
public sealed class LibFileBunchElementAsset : Asset, IFileBunchElement
{
    private readonly LibraryEntry _entry;

    /// <summary>
    /// Initializes a new instance from a library entry.
    /// </summary>
    /// <param name="entry">The library entry for this file element.</param>
    internal LibFileBunchElementAsset(LibraryEntry entry)
    {
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));

        this.LocalName = _entry.Location.Replace('/', '.').Replace('\\', '.');
    }

    /// <inheritdoc/>
    public IFileBunch FileBunch => ParentAsset as LibFileBunchAsset;

    /// <inheritdoc/>
    public string FileId => _entry.Location;

    /// <inheritdoc/>
    public int Index => _entry.Index;

    /// <inheritdoc/>
    public override Image GetIcon() => EditorUtility.GetIconForFileExact(_entry.Location);

    /// <inheritdoc/>
    public void SaveToFile(string fileName)
    {
        using var fs = File.OpenRead(ParentAsset.Library.FileName.PhysicFileName);
        using var zf = new ZipFile(fs);
        if (!string.IsNullOrEmpty(LibraryAssetBK._xx))
        {
            zf.Password = LibraryAssetBK._xx;
        }

        string dir = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var inputStream = zf.GetInputStream(_entry.Index);
        using var stream = File.Create(fileName);
        inputStream.CopyTo(stream);
    }

    /// <inheritdoc/>
    public new IStorageItem GetStream()
    {
        return (ParentAsset?.Library as LibraryAssetBK)?.GetStream(_entry.Index);
    }
}
