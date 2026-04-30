using ICSharpCode.SharpZipLib.Zip;
using Suity.Collections;
using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Libraries;

/// <summary>
/// A render target that extracts and renders file content from a library archive.
/// </summary>
internal class LibFileBunchRenderTarget : RenderTarget
{
    /// <summary>
    /// Gets the storage location of the database file.
    /// </summary>
    public StorageLocation DbFile { get; }

    /// <summary>
    /// Gets or sets the file identifier within the bunch.
    /// </summary>
    public string FileId { get; set; }

    private readonly LibFileBunchAsset _bunch;
    private readonly LibFileBunchElementAsset _element;

    /// <summary>
    /// Initializes a new instance of the render target.
    /// </summary>
    /// <param name="bunch">The file bunch asset.</param>
    /// <param name="element">The specific file element to render.</param>
    /// <param name="fileName">The render file name.</param>
    /// <param name="dbFile">The storage location.</param>
    /// <param name="fileId">The file identifier.</param>
    public LibFileBunchRenderTarget(LibFileBunchAsset bunch, LibFileBunchElementAsset element, RenderFileName fileName, StorageLocation dbFile, string fileId)
        : base(bunch.Id, fileName, bunch.LastUpdateTime)
    {
        _bunch = bunch;
        _element = element;
        DbFile = dbFile;
        FileId = fileId;
    }

    /// <inheritdoc/>
    public override IFileBunch FileBunch => _bunch;

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        using var fs = File.OpenRead(_bunch.Library.FileName.PhysicFileName);
        using var zf = new ZipFile(fs);
        if (!string.IsNullOrEmpty(LibraryAssetBK._xx))
        {
            zf.Password = LibraryAssetBK._xx;
        }

        using var inputStream = zf.GetInputStream(_element.Index);
        using var stream = new MemoryStream();

        inputStream.CopyTo(stream);

        return new BinaryRenderResult(RenderStatus.Success, stream.ToArray());
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> AffectedIds => base.AffectedIds.ConcatOne(_element.Id);
}
