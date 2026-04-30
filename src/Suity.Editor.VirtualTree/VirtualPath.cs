using System;
using System.Collections.Generic;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents a path within a virtual tree, composed of property name segments.
/// </summary>
public sealed class VirtualPath
{
    /// <summary>
    /// Gets the array of path segments.
    /// </summary>
    internal string[] Path;

    /// <summary>
    /// Initializes a new instance with the specified path segments.
    /// </summary>
    /// <param name="path">The array of path segments.</param>
    internal VirtualPath(string[] path)
    {
        Path = path ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Path != null)
        {
            return string.Join(".", Path);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Compares two virtual paths for equality.
    /// </summary>
    /// <param name="a">The first path.</param>
    /// <param name="b">The second path.</param>
    /// <returns>True if both paths have identical segments, false otherwise.</returns>
    public static bool SelectionEqual(VirtualPath a, VirtualPath b)
    {
        if (object.Equals(a, b)) return true;
        if (a == null || b == null) return false;

        if (a.Path.Length != b.Path.Length) return false;

        for (int i = 0; i < a.Path.Length; i++)
        {
            if (a.Path[i] != b.Path[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Compares two arrays of virtual paths for equality.
    /// </summary>
    /// <param name="a">The first array of paths.</param>
    /// <param name="b">The second array of paths.</param>
    /// <returns>True if both arrays contain identical paths in the same order, false otherwise.</returns>
    public static bool SelectionEqual(VirtualPath[] a, VirtualPath[] b)
    {
        if (object.Equals(a, b)) return true;
        if (a == null || b == null) return false;

        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (!SelectionEqual(a[i], b[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Creates multiple virtual paths from dot-separated string selections.
    /// </summary>
    /// <param name="selections">The collection of dot-separated path strings.</param>
    /// <returns>An array of virtual paths.</returns>
    public static VirtualPath[] CreateMultiple(IEnumerable<string> selections)
    {
        List<VirtualPath> list = [];

        foreach (var str in selections)
        {
            if (string.IsNullOrEmpty(str))
            {
                continue;
            }
            list.Add(new VirtualPath(str.Split('.')));
        }

        return [.. list];
    }
}