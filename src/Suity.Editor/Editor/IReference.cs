using Suity.Synchonizing.Core;
using System;

namespace Suity.Editor;

/// <summary>
/// Specifies the mode for synchronizing object references during build or runtime.
/// </summary>
public enum ReferenceSyncMode
{
    /// <summary>
    /// Builds new references from scratch.
    /// </summary>
    Build,
    /// <summary>
    /// Redirects references to new locations or objects.
    /// </summary>
    Redirect,
    /// <summary>
    /// Finds existing references in the system.
    /// </summary>
    Find,
}

/// <summary>
/// Specifies types of reference-related problems that can occur.
/// </summary>
public enum ReferenceProblemTypes
{
    /// <summary>
    /// A required reference is missing.
    /// </summary>
    Missing,
    /// <summary>
    /// A reference conflict has been detected.
    /// </summary>
    Conflict,
}

/// <summary>
/// Provides a mechanism for synchronizing object references across different contexts.
/// </summary>
public interface IReference
{
    /// <summary>
    /// Synchronizes references using the provided sync context.
    /// </summary>
    /// <param name="path">The path to the reference in the object hierarchy.</param>
    /// <param name="sync">The sync context for reference synchronization.</param>
    void ReferenceSync(SyncPath path, IReferenceSync sync);
}

/// <summary>
/// Provides a context for synchronizing object references.
/// </summary>
public interface IReferenceSync
{
    /// <summary>
    /// Gets the current synchronization mode.
    /// </summary>
    ReferenceSyncMode Mode { get; }

    /// <summary>
    /// Gets the current unique identifier of the reference.
    /// </summary>
    Guid Id { get; }
    /// <summary>
    /// Gets the original unique identifier of the reference.
    /// </summary>
    Guid OldId { get; }

    /// <summary>
    /// Synchronizes an identifier at the specified path.
    /// </summary>
    /// <param name="path">The path to the reference in the object hierarchy.</param>
    /// <param name="id">The new identifier.</param>
    /// <param name="message">A message describing the synchronization.</param>
    /// <returns>The synchronized identifier.</returns>
    Guid SyncId(SyncPath path, Guid id, string message);
}

/// <summary>
/// Provides a host for object references that can be marked dirty and synchronized.
/// </summary>
public interface IReferenceHost : IEditorObjectListener, IReference
{
    /// <summary>
    /// Marks the host object as dirty, indicating it has been modified.
    /// </summary>
    void MarkDirty();
}

/// <summary>
/// Provides a host for documents that wraps IReferenceHost with document path information.
/// </summary>
public interface IDocumentHost : IReferenceHost
{
    /// <summary>
    /// Gets the path to the document.
    /// </summary>
    string DocumentPath { get; }
}