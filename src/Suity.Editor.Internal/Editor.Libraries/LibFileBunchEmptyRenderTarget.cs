using Suity.Collections;
using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Libraries;

/// <summary>
/// An empty render target used in upload mode that does not render actual file content.
/// </summary>
internal class LibFileBunchEmptyRenderTarget : RenderTarget
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
    /// Initializes a new instance of the empty render target.
    /// </summary>
    /// <param name="bunch">The file bunch asset.</param>
    /// <param name="element">The specific file element.</param>
    /// <param name="fileName">The render file name.</param>
    /// <param name="dbFile">The storage location.</param>
    /// <param name="fileId">The file identifier.</param>
    public LibFileBunchEmptyRenderTarget(LibFileBunchAsset bunch, LibFileBunchElementAsset element, RenderFileName fileName, StorageLocation dbFile, string fileId)
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
        return UploadRenderResult.Instance;
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> AffectedIds => base.AffectedIds.ConcatOne(_element.Id);
}
