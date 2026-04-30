using Suity.Editor.CodeRender.Replacing;
using Suity.Editor.CodeRender.UserCodeV2;
using System;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Interface for opening a code library database.
/// </summary>
public interface ICodeLibarayOpen
{
    /// <summary>
    /// Opens the code library database.
    /// </summary>
    /// <param name="withTransition">Whether to include transition data.</param>
    /// <returns>The opened code library database.</returns>
    ICodeLibraryDatabase OpenDatabase(bool withTransition);
}

/// <summary>
/// Interface for accessing and managing the code library database.
/// </summary>
public interface ICodeLibraryDatabase : IDisposable
{
    /// <summary>
    /// Gets whether the database is in restore mode.
    /// </summary>
    bool RestoreMode { get; }

    /// <summary>
    /// Incrementally stores a segment document into the database.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="location">The location path.</param>
    /// <param name="collection">The segment document collection.</param>
    /// <param name="skipEmpty">Whether to skip empty segments.</param>
    /// <returns>The code tag collection.</returns>
    CodeTagCollection IncrementalStore(string fileName, string location, SegmentDocument collection, bool skipEmpty);

    /// <summary>
    /// Queries a code tag by its properties.
    /// </summary>
    /// <param name="location">The location path.</param>
    /// <param name="material">The material name.</param>
    /// <param name="renderType">The render type.</param>
    /// <param name="keyString">The key string.</param>
    /// <param name="extension">The file extension.</param>
    /// <returns>The matched code tag, or null if not found.</returns>
    CodeTag QueryTag(string location, string material, string renderType, string keyString, string extension);

    /// <summary>
    /// Creates a checkpoint in the database.
    /// </summary>
    void CheckPoint();
}