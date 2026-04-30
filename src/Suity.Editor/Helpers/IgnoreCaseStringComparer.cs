using System;
using System.Collections.Generic;

namespace Suity.Helpers;

/// <summary>
/// Provides a case-insensitive string comparer that implements <see cref="IEqualityComparer{T}"/>.
/// Uses ordinal case-insensitive comparison for equality checks and generates consistent hash codes.
/// </summary>
public class IgnoreCaseStringComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="IgnoreCaseStringComparer"/>.
    /// </summary>
    public static IgnoreCaseStringComparer Instance { get; } = new();

    private IgnoreCaseStringComparer()
    {
    }

    /// <summary>
    /// Determines whether two strings are equal using case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns><c>true</c> if the strings are equal ignoring case; otherwise, <c>false</c>.</returns>
    public bool Equals(string x, string y)
    {
        return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a hash code for the specified string, computed using its lowercase representation.
    /// </summary>
    /// <param name="obj">The string for which to compute a hash code.</param>
    /// <returns>A hash code that is consistent for strings that are equal ignoring case.</returns>
    public int GetHashCode(string obj)
    {
        return obj?.ToLower().GetHashCode() ?? 0;
    }
}