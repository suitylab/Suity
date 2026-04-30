using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Reference manager
/// </summary>
public abstract class ReferenceManager
{
    /// <summary>
    /// Gets or sets the current reference manager instance.
    /// </summary>
    public static ReferenceManager Current { get; internal set; }

    /// <summary>
    /// Occurs when references have been updated.
    /// </summary>
    public event EventHandler ReferenceUpdated;

    /// <summary>
    /// Marks the specified reference host as dirty.
    /// </summary>
    /// <param name="host">The reference host to mark as dirty.</param>
    public abstract void MarkDirty(IReferenceHost host);

    /// <summary>
    /// Removes the specified reference host from the manager.
    /// </summary>
    /// <param name="host">The reference host to remove.</param>
    public abstract void Remove(IReferenceHost host);

    /// <summary>
    /// Updates all references managed by this manager.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Gets a value indicating whether any references have been modified.
    /// </summary>
    public abstract bool IsDirty { get; }

    /// <summary>
    /// Finds all reference path reports for the specified reference ID.
    /// </summary>
    /// <param name="id">The reference ID to search for.</param>
    /// <returns>An enumerable collection of sync path report items.</returns>
    public abstract IEnumerable<SyncPathReportItem> FindReference(Guid id);

    /// <summary>
    /// Finds all reference path reports for the specified reference ID within a specific host.
    /// </summary>
    /// <param name="host">The reference host to search within.</param>
    /// <param name="id">The reference ID to search for.</param>
    /// <returns>An enumerable collection of sync path report items.</returns>
    public abstract IEnumerable<SyncPathReportItem> FindReference(IReferenceHost host, Guid id);

    /// <summary>
    /// Finds all reference hosts that reference the specified ID.
    /// </summary>
    /// <param name="id">The reference ID to search for.</param>
    /// <returns>An enumerable collection of reference hosts.</returns>
    public abstract IEnumerable<IReferenceHost> FindReferenceHosts(Guid id);

    /// <summary>
    /// Gets the count of references for the specified ID.
    /// </summary>
    /// <param name="id">The reference ID to count.</param>
    /// <returns>The number of references to the specified ID.</returns>
    public abstract int GetReferenceCount(Guid id);

    /// <summary>
    /// Gets all dependency IDs for the specified reference host.
    /// </summary>
    /// <param name="host">The reference host to get dependencies for.</param>
    /// <returns>An enumerable collection of dependency GUIDs.</returns>
    public abstract IEnumerable<Guid> GetDependencies(IReferenceHost host);

    /// <summary>
    /// Collects all dependency IDs from the specified object.
    /// </summary>
    /// <param name="obj">The object to collect dependencies from.</param>
    /// <returns>A collection of dependency GUIDs.</returns>
    public abstract ICollection<Guid> CollectDependencies(object obj);

    /// <summary>
    /// Collects all problem dependencies from the specified object.
    /// </summary>
    /// <param name="obj">The object to collect problem dependencies from.</param>
    /// <returns>A dictionary mapping dependency IDs to problem types.</returns>
    public abstract IDictionary<Guid, ReferenceProblemTypes> CollectProblemDependencies(object obj);

    /// <summary>
    /// Redirects all references from an old ID to a new ID.
    /// </summary>
    /// <param name="oldId">The old reference ID to redirect from.</param>
    /// <param name="newId">The new reference ID to redirect to.</param>
    /// <returns>An enumerable collection of sync path report items for the redirected references.</returns>
    public abstract IEnumerable<SyncPathReportItem> RedirectReference(Guid oldId, Guid newId);

    /// <summary>
    /// Raises the <see cref="ReferenceUpdated"/> event.
    /// </summary>
    protected void RaiseReferenceUpdated()
    {
        ReferenceUpdated?.Invoke(this, EventArgs.Empty);
    }
}