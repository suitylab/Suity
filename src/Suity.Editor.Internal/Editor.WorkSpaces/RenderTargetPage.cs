using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Represents a collected page of render targets for a workspace, organizing them by file path and directory
/// and tracking their rendering status.
/// </summary>
internal class RenderTargetPage
{
    /// <summary>
    /// The owning workspace.
    /// </summary>
    public readonly WorkSpaceBK WorkSpace;

    /// <summary>
    /// All collected render targets.
    /// </summary>
    public RenderTarget[] AllTargets;

    /// <summary>
    /// Render targets indexed by their full physical path.
    /// </summary>
    public readonly Dictionary<string, RenderTarget> RenderTargets = [];

    /// <summary>
    /// Render targets grouped by their relative file path ID.
    /// Note: RenderTargets can be out of date.
    /// </summary>
    public readonly UniqueMultiDictionary<string, RenderTarget> RenderTargetsByFileId = new();

    /// <summary>
    /// Render targets grouped by their parent directory ID.
    /// Note: RenderTargets can be out of date.
    /// </summary>
    public readonly UniqueMultiDictionary<string, RenderTarget> RenderTargetsByDir = new();

    /// <summary>
    /// Directory information indexed by relative directory path ID.
    /// </summary>
    public readonly Dictionary<string, RenderDirectoryInfo> RenderDirectories = [];

    /// <summary>
    /// Parent-child directory relationships indexed by parent directory ID.
    /// </summary>
    public readonly UniqueMultiDictionary<string, string> RenderDirsByDir = new();

    /// <summary>
    /// Initializes a new instance of <see cref="RenderTargetPage"/> for the specified workspace.
    /// </summary>
    /// <param name="workSpace">The owning workspace.</param>
    public RenderTargetPage(WorkSpaceBK workSpace)
    {
        WorkSpace = workSpace ?? throw new ArgumentNullException(nameof(workSpace));
    }

    /// <summary>
    /// Collects all render targets from the workspace's reference items and organizes them
    /// into the various dictionaries and status tracking structures.
    /// </summary>
    public void Collect()
    {
        RenderTargets.Clear();
        RenderTargetsByFileId.Clear();
        RenderTargetsByDir.Clear();
        RenderDirectories.Clear();
        RenderDirsByDir.Clear();

        AllTargets = WorkSpace.ReferenceItems.SkipNull()
            .Where(r => r.Enabled)
            .SelectMany(o => o.GetRenderTargets()
            .OfType<RenderTarget>())
            .ToArray();

        //TODO: Temporarily using ToArray to solve multi-threading issues
        foreach (var target in AllTargets)
        {
            string rFileName = target.FileName.PhysicRelativePath;
            string fileId = rFileName.GetPathLowId();
            string dirId = fileId.GetParentPath() ?? string.Empty;

            RenderTargets[target.FileName.PhysicFullPath] = target;
            RenderTargetsByFileId.Add(fileId, target);
            if (RenderTargetsByFileId[fileId].CountOne())
            {
                RenderTargetsByDir.Add(dirId, target);
            }

            if (!target.Suspended)
            {
                foreach (var info in EnsureDirectoryInfosBubble(rFileName))
                {
                    info.Rendering = true;
                }
            }
        }

        WorkSpace.RenderRecord.RemoveAllRenderStatus(id => !RenderTargetsByFileId.ContainsKey(id));

        foreach (var id in RenderTargetsByFileId.Keys)
        {
            string rFileName = RenderTargetsByFileId[id].First().FileName.PhysicRelativePath;
            FileState status = GetFileStatus(rFileName);
            foreach (var dirInfo in EnsureDirectoryInfosBubble(rFileName))
            {
                switch (status)
                {
                    case FileState.Add:
                        // Since it's an add, remove it from the rendered collection
                        WorkSpace.RenderRecord.RemoveRenderedFileByRelativePath(rFileName);
                        dirInfo.ContainsAddingFiles = true;
                        break;

                    case FileState.Remove:
                        dirInfo.ContainsRemovingFiles = true;
                        break;

                    case FileState.Update:
                        dirInfo.ContainsUpdatingFiles = true;
                        break;

                    case FileState.Duplicated:
                    case FileState.UserOccupied:
                    case FileState.Warning:
                        dirInfo.ContainsErrorFiles = true;
                        break;

                    case FileState.Modified:
                        dirInfo.ContainsModifiedFiles = true;
                        break;

                    default:
                        break;
                }
            }
        }

        //TODO: Temporarily using ToArray to solve multi-threading issues
        foreach (var record in WorkSpace.RenderRecord.RenderedFiles.ToArray())
        {
            var status = GetFileStatus(record.RelativeFileName);
            foreach (var dirInfo in EnsureDirectoryInfosBubble(record.RelativeFileName))
            {
                switch (status)
                {
                    case FileState.Add:
                        dirInfo.ContainsAddingFiles = true;
                        break;

                    case FileState.Remove:
                        dirInfo.ContainsRemovingFiles = true;
                        break;

                    case FileState.Update:
                        dirInfo.ContainsUpdatingFiles = true;
                        break;

                    case FileState.Duplicated:
                    case FileState.UserOccupied:
                    case FileState.Warning:
                        dirInfo.ContainsErrorFiles = true;
                        break;

                    case FileState.Modified:
                        dirInfo.ContainsModifiedFiles = true;
                        break;

                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Determines the file state for the specified relative path.
    /// </summary>
    /// <param name="relativePath">The relative file path.</param>
    /// <returns>The current file state.</returns>
    public FileState GetFileStatus(string relativePath)
    {
        string fileId = relativePath.GetPathLowId();
        string fullPath = relativePath.MakeFullPath(WorkSpace.MasterDirectory);

        var targets = RenderTargetsByFileId[fileId];
        var record = WorkSpace.RenderRecord.GetRenderedFile(fileId);

        if (!targets.Any())
        {
            // Currently does not exist

            if (record != null)
            {
                return FileState.Remove;
            }
            else if (File.Exists(fullPath))
            {
                if (WorkSpace.RenderRecord.ContainsModifiedFile(fileId))
                {
                    return FileState.Modified;
                }
                else
                {
                    return FileState.User;
                }
            }
            else
            {
                return FileState.Remove;
            }
        }
        else if (targets.CountOne())
        {
            RenderTarget target = targets.First();

            if (target.Suspended)
            {
                return FileState.Exist;
            }
            else if (record != null)
            {
                if (File.Exists(fullPath))
                {
                    if (record.Dirty || target.LastUpdateTime > record.LastUpdateTime)
                    {
                        return FileState.Update;
                    }
                    else if (WorkSpace.RenderRecord.ContainsModifiedFile(fileId))
                    {
                        return FileState.Modified;
                    }
                    else
                    {
                        return FileState.Exist;
                    }
                }
                else
                {
                    return FileState.Add;
                }
            }
            else if (File.Exists(fullPath))
            {
                return FileState.UserOccupied;
            }
            else
            {
                return FileState.Add;
            }
        }
        else
        {
            if (record != null)
            {
                return FileState.Duplicated;
            }
            else if (File.Exists(fullPath))
            {
                return FileState.UserOccupied;
            }
            else
            {
                return FileState.Duplicated;
            }
        }
    }

    /// <summary>
    /// Checks for pre-render errors such as duplicate file names or user-occupied files.
    /// </summary>
    /// <returns>True if any pre-render errors were found.</returns>
    public bool CheckPreRenderError()
    {
        bool preRenderError = false;

        foreach (var pathId in RenderTargetsByFileId.Keys)
        {
            string fileName = RenderTargetsByFileId[pathId].First().FileName.PhysicFullPath;

            if (RenderTargetsByFileId[pathId].CountMoreThanOne())
            {
                var logItem = new ActionLogItem($"Duplicate file name: {fileName}", () => TextFileHelper.NavigateFile(fileName));

                Logs.LogError(logItem);
                preRenderError = true;
            }

            var state = GetFileStatus(pathId);
            if (state == FileState.UserOccupied || state == FileState.User)
            {
                var logItem = new ActionLogItem($"File is occupied by user: {fileName}", () => TextFileHelper.NavigateFile(fileName));

                Logs.LogError(logItem);
                preRenderError = true;
            }
        }

        return preRenderError;
    }

    /// <summary>
    /// Cleans up rendered files that no longer have corresponding render targets.
    /// </summary>
    public void PreRenderCleanUp()
    {
        string basePath = WorkSpace.MasterDirectory;

        foreach (var record in WorkSpace.RenderRecord.RenderedFiles)
        {
            string fileId = record.RelativeFileName.GetPathLowId();
            string fileName = record.RelativeFileName.MakeFullPath(basePath);

            if (!RenderTargetsByFileId.ContainsKey(fileId))
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Delete(fileName);
                        Logs.LogInfo($"Delete file: {fileName}");
                    }
                    catch (Exception e)
                    {
                        e.LogError($"Failed to delete file: {fileName}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ensures directory info entries exist for the specified file path and all its parent directories,
    /// bubbling up from the file's directory to the root.
    /// </summary>
    /// <param name="rFileName">The relative file name.</param>
    /// <returns>An enumeration of directory info entries from the file's directory to the root.</returns>
    public IEnumerable<RenderDirectoryInfo> EnsureDirectoryInfosBubble(string rFileName)
    {
        string rDir = rFileName.GetParentPath() ?? string.Empty;

        while (true)
        {
            var info = EnsureDirectoryInfo(rDir);
            yield return info;

            if (string.IsNullOrEmpty(rDir))
            {
                break;
            }

            string parentDir = rDir.GetParentPath() ?? string.Empty;
            string parentDirId = parentDir.GetPathLowId();
            RenderDirsByDir.Add(parentDirId, rDir);
            rDir = parentDir;
        }
    }

    /// <summary>
    /// Ensures a directory info entry exists for the specified relative directory path.
    /// </summary>
    /// <param name="rDir">The relative directory path.</param>
    /// <returns>The existing or newly created directory info.</returns>
    public RenderDirectoryInfo EnsureDirectoryInfo(string rDir)
    {
        string id = rDir.GetPathLowId();

        if (!RenderDirectories.TryGetValue(id, out RenderDirectoryInfo info))
        {
            info = new RenderDirectoryInfo(rDir);
            RenderDirectories.Add(id, info);
        }

        return info;
    }
}