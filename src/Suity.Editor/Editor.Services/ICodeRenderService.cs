using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Expressions;
using Suity.Editor.WorkSpaces;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for code rendering operations.
/// </summary>
public interface ICodeRenderService
{
    /// <summary>
    /// Renders multiple render targets.
    /// </summary>
    /// <param name="config">The render configuration.</param>
    /// <param name="renderTargets">The render targets.</param>
    /// <param name="results">The render results.</param>
    /// <returns>True if rendering was successful.</returns>
    bool RenderTargets(RenderConfig config, IEnumerable<RenderTarget> renderTargets, out IEnumerable<TargetFileRenderResult> results);

    /// <summary>
    /// Renders a target in memory.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>The render result.</returns>
    RenderResult RenderTargetInMemory(RenderTarget target, ExpressionContext context);

    /// <summary>
    /// Restores multiple render targets from generated files.
    /// </summary>
    /// <param name="config">The render configuration.</param>
    /// <param name="renderTargets">The render targets.</param>
    /// <param name="results">The restore results.</param>
    /// <returns>True if restoration was successful.</returns>
    bool RestoreTargets(RenderConfig config, IEnumerable<RenderTarget> renderTargets, out IEnumerable<TargetFileRenderResult> results);

    /// <summary>
    /// Replaces user code in rendered code.
    /// </summary>
    /// <param name="config">The render configuration.</param>
    /// <param name="renderTarget">The render target.</param>
    /// <param name="renderedCode">The rendered code.</param>
    /// <returns>The code with user code replaced.</returns>
    string ReplaceUserCode(RenderConfig config, RenderTarget renderTarget, string renderedCode);

    /// <summary>
    /// Replaces user code in rendered code with specific user code.
    /// </summary>
    /// <param name="config">The render configuration.</param>
    /// <param name="renderTarget">The render target.</param>
    /// <param name="userCode">The user code.</param>
    /// <param name="renderedCode">The rendered code.</param>
    /// <returns>The code with user code replaced.</returns>
    string ReplaceUserCode(RenderConfig config, RenderTarget renderTarget, string userCode, string renderedCode);

    /// <summary>
    /// Removes tags from code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="userTagConfig">The user tag configuration.</param>
    /// <returns>The code with tags removed.</returns>
    string RemoveTags(string code, CodeSegmentConfig userTagConfig);

    /// <summary>
    /// Shows the user code editor.
    /// </summary>
    /// <param name="config">The render configuration.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="userTagConfig">The user tag configuration.</param>
    void ShowUserCodeEditor(RenderConfig config, RenderFileName fileName, CodeSegmentConfig userTagConfig);

    /// <summary>
    /// Renames key strings in user code.
    /// </summary>
    /// <param name="userCode">The code library.</param>
    /// <param name="renames">The rename operations.</param>
    void RenameKeyString(ICodeLibrary userCode, IEnumerable<UserCodeRename> renames);

    /// <summary>
    /// Renames materials in user code.
    /// </summary>
    /// <param name="userCode">The code library.</param>
    /// <param name="renames">The rename operations.</param>
    void RenameMaterial(ICodeLibrary userCode, IEnumerable<UserCodeRename> renames);

    /// <summary>
    /// Gets a render language by name.
    /// </summary>
    /// <param name="name">The language name.</param>
    /// <returns>The render language.</returns>
    IRenderLanguage GetLanguage(string name);

    // WorkSpace

    /// <summary>
    /// Renders all targets.
    /// </summary>
    /// <param name="incremental">Whether to use incremental rendering.</param>
    void RenderAll(bool incremental);

    /// <summary>
    /// Renders a workspace.
    /// </summary>
    /// <param name="workSpaces">The workspaces to render.</param>
    /// <param name="incremental">Whether to use incremental rendering.</param>
    /// <returns>True if rendering was successful.</returns>
    bool RenderWorkSpace(IEnumerable<WorkSpace> workSpaces, bool incremental);

    /// <summary>
    /// Renders a workspace with specific targets.
    /// </summary>
    /// <param name="targets">The targets to render.</param>
    /// <returns>True if rendering was successful.</returns>
    bool RenderWorkSpace(UniqueMultiDictionary<WorkSpace, RenderTarget> targets);

    /// <summary>
    /// Restores a workspace.
    /// </summary>
    /// <param name="workSpace">The workspace to restore.</param>
    /// <param name="refItem">Optional reference item.</param>
    /// <param name="userCode">Optional code library.</param>
    /// <returns>True if restoration was successful.</returns>
    bool RestoreWorkSpace(WorkSpace workSpace, IWorkSpaceRefItem refItem = null, ICodeLibrary userCode = null);

    /// <summary>
    /// Uploads workspace user code.
    /// </summary>
    /// <param name="workSpace">The workspace.</param>
    /// <param name="refItem">The reference item.</param>
    /// <returns>True if upload was successful.</returns>
    bool UploadWorkSpaceUserCode(WorkSpace workSpace, IWorkSpaceRefItem refItem);

    /// <summary>
    /// Creates a user code file.
    /// </summary>
    /// <param name="workSpace">The workspace.</param>
    /// <param name="refItem">The reference item.</param>
    /// <returns>True if creation was successful.</returns>
    bool CreateUserCodeFile(WorkSpace workSpace, IWorkSpaceRefItem refItem);

    // Result

    /// <summary>
    /// Creates a text render result.
    /// </summary>
    /// <param name="result">The render status.</param>
    /// <param name="text">The text content.</param>
    /// <returns>A render result.</returns>
    RenderResult CreateTextRenderResult(RenderStatus result, string text);

    /// <summary>
    /// Creates a binary render result.
    /// </summary>
    /// <param name="result">The render status.</param>
    /// <param name="data">The binary data.</param>
    /// <returns>A render result.</returns>
    RenderResult CreateBinaryRenderResult(RenderStatus result, byte[] data);
}