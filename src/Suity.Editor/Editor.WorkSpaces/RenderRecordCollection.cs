using Suity.Editor.CodeRender;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Collection of render file records
/// </summary>
public abstract class RenderRecordCollection
{
    #region Rendered

    /// <summary>
    /// Gets all rendered file records
    /// </summary>
    public abstract IEnumerable<RenderFileRecord> RenderedFiles { get; }
    /// <summary>
    /// Gets all rendered file IDs
    /// </summary>
    public abstract IEnumerable<string> RenderedFileIds { get; }

    /// <summary>
    /// Gets a rendered file record by ID
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>The render file record, or null if not found</returns>
    public abstract RenderFileRecord GetRenderedFile(string id);

    /// <summary>
    /// Checks whether a rendered file exists
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>True if exists</returns>
    public abstract bool ContainsRenderedFile(string id);

    #endregion

    #region RenderStatus

    /// <summary>
    /// Gets the render status by file ID
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>Render status</returns>
    public abstract RenderStatus GetRenderStatus(string id);

    /// <summary>
    /// Gets the render status by relative path
    /// </summary>
    /// <param name="rPath">Relative path</param>
    /// <returns>Render status</returns>
    public abstract RenderStatus GetRenderStatusByRelativePath(string rPath);

    #endregion

    #region Modify

    /// <summary>
    /// Checks whether a modified file exists
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>True if exists</returns>
    public abstract bool ContainsModifiedFile(string id);

    /// <summary>
    /// Adds a modified file by relative path
    /// </summary>
    /// <param name="rPath">Relative path</param>
    public abstract void AddModifiedFileByRelativePath(string rPath);

    /// <summary>
    /// Checks whether there are any modifying files
    /// </summary>
    /// <returns>True if contains modifying files</returns>
    public abstract bool ContainsModifyingFiles();

    #endregion
}