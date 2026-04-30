using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Abstract base class for managing workspaces
/// </summary>
public abstract class WorkSpaceManager
{
    private static WorkSpaceManager _current;

    /// <summary>
    /// Gets the current workspace manager instance
    /// </summary>
    public static WorkSpaceManager Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Project.Current?.WorkSpaceManager;
            return _current;
        }
        internal set
        {
            _current = value;
        }
    }

    /// <summary>
    /// Configuration file name for rendering
    /// </summary>
    public const string RenderConfigFileName = "render.config";
    /// <summary>
    /// Asset key for workspace manager
    /// </summary>
    public const string WorkspaceManagerAssetKey = "*WorkSpaceManager";

    /// <summary>
    /// Whether to use absolute external master path
    /// </summary>
    public static bool AbsoluteExternalMasterPath = true;

    #region Property

    /// <summary>
    /// Gets the owning project
    /// </summary>
    public abstract Project OwnerProject { get; }
    /// <summary>
    /// Gets the base path of the workspace manager
    /// </summary>
    public abstract string BasePath { get; }
    /// <summary>
    /// Gets the solution GUID
    /// </summary>
    public abstract Guid SolutionGuid { get; }
    /// <summary>
    /// Gets whether the workspace manager is in released mode
    /// </summary>
    public abstract bool IsReleased { get; }
    /// <summary>
    /// Gets the workspace manager asset
    /// </summary>
    public abstract WorkSpaceManagerAsset Asset { get; }

    #endregion

    #region Event

    /// <summary>
    /// Event raised when a workspace is added
    /// </summary>
    public event EventHandler<WorkSpaceEventArgs> WorkSpaceAdded;

    /// <summary>
    /// Event raised when a workspace is removed
    /// </summary>
    public event EventHandler<WorkSpaceEventArgs> WorkSpaceRemoved;

    /// <summary>
    /// Event raised when a workspace is renamed
    /// </summary>
    public event EventHandler<WorkSpaceRenameEventArgs> WorkSpaceRenamed;

    /// <summary>
    /// Event raised when a workspace render target is updated
    /// </summary>
    public event EventHandler<WorkSpaceEventArgs> WorkSpaceRenderTargetUpdated;

    /// <summary>
    /// Raises the WorkSpaceAdded event
    /// </summary>
    /// <param name="args">Event arguments</param>
    protected void OnWorkSpaceAdded(WorkSpaceEventArgs args)
    {
        WorkSpaceAdded?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the WorkSpaceRemoved event
    /// </summary>
    /// <param name="args">Event arguments</param>
    protected void OnWorkSpaceRemoved(WorkSpaceEventArgs args)
    {
        WorkSpaceRemoved?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the WorkSpaceRenamed event
    /// </summary>
    /// <param name="args">Event arguments</param>
    protected void OnWorkSpaceRenamed(WorkSpaceRenameEventArgs args)
    {
        WorkSpaceRenamed?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the WorkSpaceRenderTargetUpdated event
    /// </summary>
    /// <param name="args">Event arguments</param>
    protected void OnWorkSpaceRenderTargetUpdated(WorkSpaceEventArgs args)
    {
        WorkSpaceRenderTargetUpdated?.Invoke(this, args);
    }

    #endregion

    #region WorkSpace

    /// <summary>
    /// Adds a new workspace with the specified name
    /// </summary>
    /// <param name="name">Name of the workspace</param>
    /// <param name="ctrlInfo">Optional controller info</param>
    /// <returns>The created workspace</returns>
    public abstract WorkSpace AddWorkSpace(string name, WorkSpaceControllerInfo ctrlInfo = null);

    /// <summary>
    /// Deletes a workspace by name
    /// </summary>
    /// <param name="name">Name of the workspace to delete</param>
    /// <returns>True if successful</returns>
    public abstract bool DeleteWorkSpace(string name);

    /// <summary>
    /// Checks whether a workspace can be renamed
    /// </summary>
    /// <param name="oldName">Current name</param>
    /// <param name="newName">New name</param>
    /// <returns>True if rename is allowed</returns>
    public abstract bool CanRenameWorkSpace(string oldName, string newName);

    /// <summary>
    /// Renames a workspace
    /// </summary>
    /// <param name="oldName">Current name</param>
    /// <param name="newName">New name</param>
    /// <returns>True if successful</returns>
    public abstract bool RenameWorkSpace(string oldName, string newName);

    /// <summary>
    /// Gets a workspace by name
    /// </summary>
    /// <param name="name">Workspace name</param>
    /// <returns>The workspace, or null if not found</returns>
    public abstract WorkSpace GetWorkSpace(string name);

    /// <summary>
    /// Checks whether a workspace with the specified name exists
    /// </summary>
    /// <param name="name">Workspace name</param>
    /// <returns>True if exists</returns>
    public abstract bool ContainsWorkSpace(string name);

    /// <summary>
    /// Gets all controllers of the specified type
    /// </summary>
    /// <typeparam name="T">Controller type</typeparam>
    /// <returns>Enumerable of controllers</returns>
    public abstract IEnumerable<T> GetControllers<T>() where T : WorkSpaceController;

    /// <summary>
    /// Gets all workspaces
    /// </summary>
    public abstract IEnumerable<WorkSpace> WorkSpaces { get; }

    /// <summary>
    /// Gets the number of workspaces
    /// </summary>
    public abstract int WorkSpaceCount { get; }

    /// <summary>
    /// Gets whether any workspace requires rendering
    /// </summary>
    public bool RequireRender => WorkSpaces.Any(o => o.RequireRender);

    #endregion

    #region Path

    /// <summary>
    /// Gets full path
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Returns full path</returns>
    public abstract string MakeFullPath(string relativePath);

    /// <summary>
    /// Gets relative path
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>Returns relative path</returns>
    public abstract string MakeRelativePath(string fullPath);

    #endregion

    #region Solution

    /// <summary>
    /// Writes the solution file
    /// </summary>
    public abstract void WriteSolution();

    #endregion


    /// <summary>
    /// Updates the plugin with a delay
    /// </summary>
    public abstract void UpdatePluginDelayed();

    /// <summary>
    /// Raises the render target updated event for the specified workspace
    /// </summary>
    /// <param name="workSpace">The workspace</param>
    internal virtual void RaiseRenderTargetUpdated(WorkSpace workSpace)
    { }
}