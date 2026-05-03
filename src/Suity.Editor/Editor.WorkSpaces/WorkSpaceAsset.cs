using Suity.Drawing;
using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Asset representing the workspace manager
/// </summary>
public class WorkSpaceManagerAsset : Asset
{
    private readonly WorkSpaceManager _workSpaceManager;

    /// <summary>
    /// Gets the workspace manager
    /// </summary>
    public WorkSpaceManager WorkSpaceManager => _workSpaceManager;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceManagerAsset"/>
    /// </summary>
    /// <param name="workSpaceManager">The workspace manager</param>
    public WorkSpaceManagerAsset(WorkSpaceManager workSpaceManager)
        : base(WorkSpaceManager.WorkspaceManagerAssetKey)
    {
        _workSpaceManager = workSpaceManager;
        ResolveId();
    }

    /// <summary>
    /// Gets the default icon for this asset
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.WorkSpace;

    /// <summary>
    /// Releases this asset
    /// </summary>
    internal void Release() => Entry = null;

    /// <summary>
    /// Gets the display text
    /// </summary>
    public override string DisplayText => "Workspace Manager";
}

/// <summary>
/// Asset representing a workspace
/// </summary>
[DisplayText("Workspace Asset")]
public class WorkSpaceAsset : Asset, ICodeLibrary
{
    private readonly WorkSpace _workSpace;

    /// <summary>
    /// Gets the workspace
    /// </summary>
    public WorkSpace WorkSpace => _workSpace;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceAsset"/>
    /// </summary>
    /// <param name="workSpace">The workspace</param>
    public WorkSpaceAsset(WorkSpace workSpace)
        : base(KeyCode.Combine(WorkSpace.WorkspaceAssetKeyPrefix, workSpace.Name))
    {
        _workSpace = workSpace;

        UpdateAssetTypes(typeof(ICodeLibrary));
        ResolveId(IdResolveType.FullName);
    }

    /// <summary>
    /// Gets the icon for this asset
    /// </summary>
    /// <returns>The workspace icon</returns>
    public override ImageDef GetIcon() => base.GetIcon() ?? _workSpace.Icon;

    /// <summary>
    /// Releases this asset
    /// </summary>
    internal void Release() => Entry = null;

    #region ICodeLibrary

    StorageLocation ICodeLibrary.FileName => StorageLocation.Create(_workSpace.DbFileName);
    CodeLibraryStorageModes ICodeLibrary.StorageMode => CodeLibraryStorageModes.DB;
    IEnumerable<string> ICodeLibrary.IncludedFiles => [];

    /// <summary>
    /// Creates the build configuration for this code library
    /// </summary>
    /// <returns>The render configuration</returns>
    RenderConfig ICodeLibrary.CreateBuildConfig() => _workSpace.CreateRenderConfig();

    /// <summary>
    /// Checks whether this library contains the specified dependency
    /// </summary>
    /// <param name="id">Dependency ID</param>
    /// <returns>True if contains</returns>
    bool ICodeLibrary.ContainsDependency(Guid id) => true;

    /// <summary>
    /// Gets the default storage, returns null
    /// </summary>
    public object DefaultStorage => null;

    #endregion

    /// <summary>
    /// Gets the display text from the workspace name
    /// </summary>
    public override string DisplayText => _workSpace?.Name ?? base.DisplayText;
}