using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.IO;
using System.Linq;

namespace Suity.Editor.Flows.WorkSpaces;

public static class WorkSpaceFileExtensions
{
    /// <summary>
    /// Writes content to a file within the workspace, resolving the full path relative to the workspace master directory.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="filePath">The relative file path within the workspace.</param>
    /// <param name="content">The content to write to the file.</param>
    public static void WriteWorkSpaceFile(this WorkSpace workSpace, string filePath, string content)
    {
        filePath ??= string.Empty;
        filePath = filePath.Trim().Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"File path is empty.");
        }

        string fileFullPath = workSpace.MakeMasterFullPath(filePath);

        FileUtils.Write(fileFullPath, content, FileUtils.UTF8NoBom);
    }

    /// <summary>
    /// Retrieves all files recursively from a specified base path within the workspace.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="basePath">The relative base directory path within the workspace.</param>
    /// <returns>An array of FileInfo objects representing all files found, or an empty array if the directory does not exist.</returns>
    public static FileInfo[] GetFiles(this WorkSpace workSpace, string basePath)
    {
        basePath ??= string.Empty;
        basePath = basePath.Trim().Replace('\\', '/').TrimStart('.', '/');

        string dirFullPath = workSpace.MakeMasterFullPath(basePath);

        var dirInfo = new DirectoryInfo(dirFullPath);
        if (dirInfo.Exists)
        {
            return dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Retrieves all file paths recursively from a specified base path within the workspace, returned as relative paths.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="basePath">The relative base directory path within the workspace.</param>
    /// <returns>An array of relative file paths, or an empty array if the directory does not exist.</returns>
    public static string[] GetRelativeFilePaths(this WorkSpace workSpace, string basePath)
    {
        basePath ??= string.Empty;
        basePath = basePath.Trim().Replace('\\', '/').TrimStart('.', '/');

        string dirFullPath = workSpace.MakeMasterFullPath(basePath);

        var dirInfo = new DirectoryInfo(dirFullPath);
        if (dirInfo.Exists)
        {
            return dirInfo.GetFiles("*.*", SearchOption.AllDirectories).Select(o =>
            {
                string rPath = workSpace.MakeMasterRelativePath(o.FullName);

                return rPath.Replace('\\', '/');
            }).ToArray();
        }
        else
        {
            return [];
        }
    }
}
