using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.WorkSpaces;

public static class WorkSpaceExtensions
{
    public static void WriteAllText(this WorkSpace workSpace, string relativePath, string content)
    {
        if (workSpace is null)
        {
            throw new ArgumentNullException(nameof(workSpace));
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Path is not relative.", nameof(relativePath));
        }

        string rootDir = workSpace.MasterDirectory;
        string fullPath = Path.Combine(rootDir, relativePath);
        File.WriteAllText(fullPath, content);
    }

    public static void WriteAllLines(this WorkSpace workSpace, string relativePath, IEnumerable<string> lines)
    {
        if (workSpace is null)
        {
            throw new ArgumentNullException(nameof(workSpace));
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Path is not relative.", nameof(relativePath));
        }

        string rootDir = workSpace.MasterDirectory;
        string fullPath = Path.Combine(rootDir, relativePath);
        File.WriteAllLines(fullPath, lines);
    }

}
