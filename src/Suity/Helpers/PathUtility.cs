using System;
using System.IO;

namespace Suity.Helpers;

/// <summary>
/// Provides utility methods for path manipulation.
/// </summary>
public static class PathUtility
{
    private static readonly char[] Slashes = ['\\', '/'];

    public static string MakeRalativePath(this string path, string basePath)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(basePath))
        {
            return path;
        }

        if (!basePath.EndsWith("/") && !basePath.EndsWith("\\"))
        {
            basePath += '/';
        }

        if (!Uri.TryCreate(path, UriKind.Absolute, out Uri pathUri))
        {
            return string.Empty;
        }

        var baseUri = new Uri(basePath);

        // You cannot use IsBaseOf because it resolves to ../
        try
        {
            var resultUri = baseUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(resultUri.ToString());
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string MakeFullPath(this string path, string basePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return path;
        }

        if (!basePath.EndsWith("/") && !basePath.EndsWith("\\"))
        {
            basePath += '/';
        }

        var baseUri = new Uri(basePath);
        var resultUri = new Uri(baseUri, path);
        string result = Uri.UnescapeDataString(resultUri.ToString());

        if (result.StartsWith("file:///"))
        {
            return result.Substring(8, result.Length - 8);
        }
        else
        {
            return result;
        }
    }

    public static string NormalizeDirectoryName(this string path)
    {
        path = path.Replace('/', '\\');
        if (!path.EndsWith("\\"))
        {
            path += "\\";
        }

        return path;
    }

    public static string GetPathTerminal(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        path = path.TrimEnd('\\', '/');

        int index = path.LastIndexOfAny(Slashes);
        if (index >= 0)
        {
            return path.Substring(index + 1, path.Length - index - 1);
        }
        else
        {
            return path;
        }
    }

    public static string GetPathRoot(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        path = path.TrimStart('\\', '/');

        int index = path.IndexOfAny(Slashes);
        if (index >= 0)
        {
            return path.Substring(0, index);
        }
        else
        {
            return path;
        }
    }

    public static string GetParentPath(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        path = path.TrimEnd('\\', '/');

        int index = path.LastIndexOfAny(Slashes);
        if (index >= 0)
        {
            return path.Substring(0, index);
        }
        else
        {
            return string.Empty;
        }
    }

    public static string GetPathLowId(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // Convert backslashes, remove the trailing slash, and convert to lowercase
        return path.Replace('\\', '/').TrimEnd('\\', '/').ToLower();
    }

    public static string GetPathId(this string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // Convert backslashes, remove the trailing slash, and convert to lowercase
        return path.Replace('\\', '/').TrimEnd('\\', '/');
    }

    public static string PathAppend(this string path, string suffixPath)
    {
        return Path.Combine(path, suffixPath);
    }

    public static string PathPreppend(this string path, string prefixPath)
    {
        return Path.Combine(prefixPath, path);
    }


    [Obsolete]
    public static string UrlPathAppend(this string path, string suffixPath)
        => UrlAppend(path, suffixPath);

    public static string UrlAppend(this string path, string suffixPath)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return suffixPath;
        }

        if (string.IsNullOrWhiteSpace(suffixPath))
        {
            return path;
        }

        return $"{path.TrimEnd('/', '\\')}/{suffixPath.TrimStart('/', '\\')}";
    }

    public static bool IsPathValid(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Check for invalid characters
        if (path.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            return false;

        // Check for trailing characters
        if (path.EndsWith(" ") || path.EndsWith("."))
            return false;

        return true;
    }

    public static bool FileExists(this string path)
    {
        return File.Exists(path);
    }

    public static bool DirectoryExists(this string path)
    {
        return Directory.Exists(path);
    }

    public static bool FileOrDirectoryExists(this string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    /// <summary>
    /// Compare strings for equality regardless of case
    /// </summary>
    /// <param name="str"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool IgnoreCaseEquals(this string str, string other)
    {
        return string.Equals(str, other, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Ignore case and determine whether the file name extension is the specified extension
    /// </summary>
    /// <param name="str"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static bool FileExtensionEquals(this string str, string extension)
    {
        string ext = Path.GetExtension(str);

        return string.Equals(ext, extension, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Remove the file name extension
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveExtension(this string path)
    {
        string ext = Path.GetExtension(path);
        if (ext.Length > 0)
        {
            return path.RemoveFromLast(ext.Length);
        }
        else
        {
            return path;
        }
    }
}