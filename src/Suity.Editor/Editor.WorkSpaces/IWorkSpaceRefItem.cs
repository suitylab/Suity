using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Workspace reference item
/// </summary>
public interface IWorkSpaceRefItem
{
    /// <summary>
    /// Workspace
    /// </summary>
    WorkSpace WorkSpace { get; }

    /// <summary>
    /// Gets the unique identifier
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Whether enabled
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Whether suspended and not generating code
    /// </summary>
    bool Suspended { get; }

    /// <summary>
    /// Order
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Upload mode
    /// </summary>
    bool UploadMode { get; }

    /// <summary>
    /// User code library
    /// </summary>
    ICodeLibrary UserCode { get; }

    /// <summary>
    /// Gets whether auto restore user code
    /// </summary>
    bool AutoRestoreUserCode { get; }

    /// <summary>
    /// Sets user code library file
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>True if successful</returns>
    bool SetupUserCodeFile(string fileName);

    /// <summary>
    /// Gets all render targets
    /// </summary>
    /// <returns>Enumerable of render targets</returns>
    IEnumerable<RenderTarget> GetRenderTargets();
}

/// <summary>
/// Renderable workspace reference item
/// </summary>
public interface IRenderableRefItem : IWorkSpaceRefItem
{
    /// <summary>
    /// Adds a material
    /// </summary>
    /// <param name="material">Material to add</param>
    /// <returns>True if successful</returns>
    bool AddMaterial(IMaterial material);

    /// <summary>
    /// Removes a material
    /// </summary>
    /// <param name="material">Material to remove</param>
    /// <returns>True if successful</returns>
    bool RemoveMaterial(IMaterial material);
}