using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Represents a hierarchical path used to identify ImGui nodes within the UI tree.
/// Paths are immutable and support various manipulation operations.
/// </summary>
public abstract class ImGuiPath : IEquatable<ImGuiPath>, IComparable<ImGuiPath>
{
    /// <summary>
    /// Gets an empty ImGuiPath instance.
    /// </summary>
    /// <value>An empty <see cref="ImGuiPath"/> with no segments.</value>
    public static ImGuiPath Empty { get; internal set; }

    private int? _hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiPath"/> class.
    /// This constructor is internal to restrict instantiation to derived types within the assembly.
    /// </summary>
    internal ImGuiPath()
    { }

    /// <summary>
    /// Gets the number of segments in this path.
    /// </summary>
    /// <value>The count of segments in the path.</value>
    public abstract int Length { get; }

    /// <summary>
    /// Gets whether this path is empty.
    /// </summary>
    /// <value>True if the path contains no segments; otherwise, false.</value>
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// Gets the path segment at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the segment.</param>
    /// <returns>The segment string at the specified index.</returns>
    public abstract string this[int index] { get; }

    /// <summary>
    /// Gets the path segment at the specified index, or null if the index is out of range.
    /// </summary>
    /// <param name="index">The zero-based index of the segment.</param>
    /// <returns>The segment string, or null.</returns>
    public abstract string? GetStringAt(int index);

    /// <summary>
    /// Creates a new path that is a substring of this path.
    /// </summary>
    /// <param name="index">The starting index.</param>
    /// <param name="length">The number of segments to include.</param>
    /// <returns>A new ImGuiPath representing the sub path.</returns>
    public abstract ImGuiPath SubPath(int index, int length);

    /// <summary>
    /// Creates a new path with the specified item appended to the end.
    /// </summary>
    /// <param name="pathItem">The path item to append.</param>
    /// <returns>A new ImGuiPath with the appended item.</returns>
    public abstract ImGuiPath Append(string pathItem);

    /// <summary>
    /// Creates a new path with the specified item prepended to the beginning.
    /// </summary>
    /// <param name="pathItem">The path item to prepend.</param>
    /// <returns>A new ImGuiPath with the prepended item.</returns>
    public abstract ImGuiPath Prepend(string pathItem);

    /// <summary>
    /// Creates a new path with the first segment removed.
    /// </summary>
    /// <returns>A new ImGuiPath without the first segment.</returns>
    public abstract ImGuiPath RemoveFirst();

    /// <summary>
    /// Creates a new path with the last segment removed.
    /// </summary>
    /// <returns>A new ImGuiPath without the last segment.</returns>
    public abstract ImGuiPath RemoveLast();

    /// <summary>
    /// Creates a new path with the segment at the specified index replaced.
    /// </summary>
    /// <param name="index">The index of the segment to replace.</param>
    /// <param name="pathItem">The new path item.</param>
    /// <returns>A new ImGuiPath with the edited segment.</returns>
    public abstract ImGuiPath EditPath(int index, string pathItem);

    /// <inheritdoc/>
    public abstract bool Equals(ImGuiPath? other);

    /// <inheritdoc/>
    public abstract int CompareTo(ImGuiPath other);

    /// <summary>
    /// Determines whether this path matches another path starting at the specified index.
    /// </summary>
    /// <param name="index">The starting index in this path.</param>
    /// <param name="other">The path to match against.</param>
    /// <returns>True if the paths match; otherwise, false.</returns>
    public abstract bool Match(int index, ImGuiPath other);

    /// <summary>
    /// Determines whether the segment at the specified index matches the given path item.
    /// </summary>
    /// <param name="index">The index of the segment to check.</param>
    /// <param name="pathItem">The path item to match against.</param>
    /// <returns>True if the segment matches; otherwise, false.</returns>
    public abstract bool Match(int index, string pathItem);

    /// <summary>
    /// Computes the hash code for this path.
    /// </summary>
    /// <returns>The hash code.</returns>
    protected abstract int _GetHashCode();

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _hashCode ??= _GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (Object.ReferenceEquals(this, obj))
        {
            return true;
        }

        ImGuiPath? other = obj as ImGuiPath;
        return Equals(other);
    }

    /// <summary>
    /// Determines whether two ImGuiPath instances are equal.
    /// </summary>
    /// <param name="v1">The first path to compare.</param>
    /// <param name="v2">The second path to compare.</param>
    /// <returns>True if the paths are equal; otherwise, false.</returns>
    public static bool operator ==(ImGuiPath v1, ImGuiPath v2)
    {
        if (ReferenceEquals(v1, null))
        {
            return ReferenceEquals(v2, null);
        }
        else
        {
            return v1.Equals(v2);
        }
    }

    /// <summary>
    /// Determines whether two ImGuiPath instances are not equal.
    /// </summary>
    /// <param name="v1">The first path to compare.</param>
    /// <param name="v2">The second path to compare.</param>
    /// <returns>True if the paths are not equal; otherwise, false.</returns>
    public static bool operator !=(ImGuiPath v1, ImGuiPath v2)
    {
        if (ReferenceEquals(v1, null))
        {
            return !ReferenceEquals(v2, null);
        }
        else
        {
            return !v1.Equals(v2);
        }
    }

    /// <summary>
    /// Implicitly converts a string path chain to an ImGuiPath.
    /// </summary>
    /// <param name="pathChain">The path chain string.</param>
    /// <returns>A new <see cref="ImGuiPath"/> created from the path chain.</returns>
    public static implicit operator ImGuiPath(string pathChain)
    {
        return Create(pathChain);
    }

    /// <summary>
    /// Determines whether the specified path is null or empty.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty(ImGuiPath? path) => ImGuiExternal._external.IsPathNullOrEmpty(path);

    /// <summary>
    /// Combines two paths into a single path.
    /// </summary>
    /// <param name="a">The first path.</param>
    /// <param name="b">The second path.</param>
    /// <returns>A new ImGuiPath representing the combined path.</returns>
    public static ImGuiPath Combine(ImGuiPath a, ImGuiPath b) => ImGuiExternal._external.CombinePath(a, b);

    /// <summary>
    /// Creates a new path from the specified path segments.
    /// </summary>
    /// <param name="path">The path segments.</param>
    /// <returns>A new ImGuiPath.</returns>
    public static ImGuiPath Create(params string[] path) => ImGuiExternal._external.CreatePath(path);

    /// <summary>
    /// Creates a new path from a path chain string.
    /// </summary>
    /// <param name="pathChain">The path chain string.</param>
    /// <returns>A new ImGuiPath.</returns>
    public static ImGuiPath Create(string pathChain) => ImGuiExternal._external.CreatePath(pathChain);

    /// <summary>
    /// Attempts to create a path from a path chain string.
    /// </summary>
    /// <param name="pathChain">The path chain string.</param>
    /// <param name="path">When this method returns, contains the created path, or null if creation failed.</param>
    /// <returns>True if the path was created successfully; otherwise, false.</returns>
    public static bool TryCreate(string pathChain, out ImGuiPath? path) => ImGuiExternal._external.TryCreatePath(pathChain, out path);

    /// <summary>
    /// Attempts to create a path from a collection of path segments.
    /// </summary>
    /// <param name="pathChain">The collection of path segments.</param>
    /// <param name="path">When this method returns, contains the created path, or null if creation failed.</param>
    /// <returns>True if the path was created successfully; otherwise, false.</returns>
    public static bool TryCreate(IEnumerable<string> pathChain, out ImGuiPath? path) => ImGuiExternal._external.TryCreatePath(pathChain, out path);
}
