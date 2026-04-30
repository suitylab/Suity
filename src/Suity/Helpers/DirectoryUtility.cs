using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Helpers;

/// <summary>
/// Provides directory utility methods.
/// </summary>
public static class DirectoryUtility
{
    public static IEnumerable<FileInfo> GetAllFiles(this DirectoryInfo dirInfo, bool recursive = true)
    {
        foreach (FileInfo fileInfo in dirInfo.GetFiles())
        {
            yield return fileInfo;
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                foreach (FileInfo subFileInfo in GetAllFiles(subDirInfo))
                {
                    yield return subFileInfo;
                }
            }
        }
    }

    public static IEnumerable<FileInfo> GetAllFiles(string dir, bool recursive = true)
    {
        return GetAllFiles(new DirectoryInfo(dir), recursive);
    }

    public static bool CopyDirectory(string sourcePath, string targetPath, bool overwrite = false, Predicate<string> filter = null)
    {
        if (!Directory.Exists(sourcePath)) return false;
        if (!overwrite && Directory.Exists(targetPath)) return false;

        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        foreach (string file in Directory.GetFiles(sourcePath))
        {
            if (filter != null && !filter(file)) continue;
            File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)), overwrite);
        }

        foreach (string subDir in Directory.GetDirectories(sourcePath))
        {
            if (filter != null && !filter(subDir)) continue;
            CopyDirectory(subDir, Path.Combine(targetPath, Path.GetFileName(subDir)), overwrite, filter);
        }

        return true;
    }

    public static void CleanUpDirectory(string srcPath)
    {
        try
        {
            var dir = new DirectoryInfo(srcPath);
            var fileinfos = dir.GetFileSystemInfos();  // Returns all files and subdirectories in a directory
            foreach (var i in fileinfos)
            {
                if (i is DirectoryInfo)            // Determine whether it is a folder
                {
                    DirectoryInfo subdir = new(i.FullName);
                    subdir.Delete(true);          // Deleting subdirectories and files
                }
                else
                {
                    File.Delete(i.FullName);      // Delete the specified file
                }
            }
        }
        catch (Exception e)
        {
            //throw;
            Logs.LogError(e);
        }
    }

    public static void EnsureDirectory(string fullName)
    {
        if (File.Exists(fullName))
        {
            File.Delete(fullName);
        }

        if (!Directory.Exists(fullName))
        {
            Directory.CreateDirectory(fullName);
        }
    }
}