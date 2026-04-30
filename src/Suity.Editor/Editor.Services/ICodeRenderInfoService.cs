using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for code render information.
/// </summary>
public interface ICodeRenderInfoService
{
    /// <summary>
    /// Gets render targets affected by a specific ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>Affected render targets.</returns>
    IEnumerable<RenderTarget> GetAffectedRenderTargets(Guid id);

    /// <summary>
    /// Gets render targets affected by a specific ID and render host.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="renderHostId">The render host ID.</param>
    /// <returns>Affected render targets.</returns>
    IEnumerable<RenderTarget> GetAffectedRenderTargets(Guid id, Guid renderHostId);

    /// <summary>
    /// Gets the user code count for a specific ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The user code count.</returns>
    int GetUserCodeCount(Guid id);

    /// <summary>
    /// Event raised when render information is updated.
    /// </summary>
    event EventHandler RenderInfoUpdated;
}

/// <summary>
/// Empty implementation of the code render info service.
/// </summary>
public class EmptyCodeRenderInfoService : ICodeRenderInfoService
{
    /// <summary>
    /// Gets the singleton instance of EmptyCodeRenderInfoService.
    /// </summary>
    public static EmptyCodeRenderInfoService Empty { get; } = new();

    /// <inheritdoc/>
    public event EventHandler RenderInfoUpdated;

    /// <inheritdoc/>
    public IEnumerable<RenderTarget> GetAffectedRenderTargets(Guid id) => [];

    /// <inheritdoc/>
    public IEnumerable<RenderTarget> GetAffectedRenderTargets(Guid id, Guid renderHostId) => [];

    /// <inheritdoc/>
    public int GetUserCodeCount(Guid id) => 0;
}

/// <summary>
/// Represents information about a user code segment.
/// </summary>
public class UserCodeInfo
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the material.
    /// </summary>
    public string Material { get; set; }

    /// <summary>
    /// Gets or sets the render type.
    /// </summary>
    public string RenderType { get; set; }

    /// <summary>
    /// Gets or sets the key string.
    /// </summary>
    public string KeyString { get; set; }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    public string Code { get; set; }
}