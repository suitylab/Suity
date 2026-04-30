using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Utility class for code rendering operations.
/// </summary>
public static class CodeRenderUtility
{
    private static readonly ServiceStore<ICodeRenderService> _renderService = new();

    /// <summary>
    /// Renders all code.
    /// </summary>
    /// <param name="incremental">Whether to use incremental rendering.</param>
    public static void RenderAll(bool incremental)
        => _renderService.Get()?.RenderAll(incremental);

    /// <summary>
    /// Renders the workspace.
    /// </summary>
    /// <param name="workSpaces">The workspaces.</param>
    /// <param name="incremental">Whether to use incremental rendering.</param>
    /// <returns>True if successful.</returns>
    public static bool RenderWorkSpace(IEnumerable<WorkSpace> workSpaces, bool incremental)
        => _renderService.Get()?.RenderWorkSpace(workSpaces, incremental) ?? false;

    /// <summary>
    /// Renders the workspace with targets.
    /// </summary>
    /// <param name="targets">The render targets.</param>
    /// <returns>True if successful.</returns>
    public static bool RenderWorkSpace(UniqueMultiDictionary<WorkSpace, RenderTarget> targets)
        => _renderService.Get()?.RenderWorkSpace(targets) ?? false;

    /// <summary>
    /// Restores the workspace.
    /// </summary>
    /// <param name="space">The workspace.</param>
    /// <param name="refItem">The reference item.</param>
    /// <param name="userCode">User code library.</param>
    /// <returns>True if successful.</returns>
    public static bool RestoreWorkSpace(WorkSpace space, IWorkSpaceRefItem refItem = null, ICodeLibrary userCode = null)
        => _renderService.Get()?.RestoreWorkSpace(space, refItem, userCode) ?? false;

    /// <summary>
    /// Uploads user code to the workspace.
    /// </summary>
    /// <param name="workSpace">The workspace.</param>
    /// <param name="refItem">The reference item.</param>
    /// <returns>True if successful.</returns>
    public static bool UploadWorkSpaceUserCode(WorkSpace workSpace, IWorkSpaceRefItem refItem)
        => _renderService.Get()?.UploadWorkSpaceUserCode(workSpace, refItem) ?? false;

    /// <summary>
    /// Creates a user code file.
    /// </summary>
    /// <param name="workSpace">The workspace.</param>
    /// <param name="refItem">The reference item.</param>
    /// <returns>True if successful.</returns>
    public static bool CreateUserCodeFile(WorkSpace workSpace, IWorkSpaceRefItem refItem)
        => _renderService.Get()?.CreateUserCodeFile(workSpace, refItem) ?? false;
}