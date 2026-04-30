using Suity.Editor;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Views;

/// <summary>
/// Local naming refactoring utility
/// </summary>
public sealed class LocalRefactor : ILocalRefactor
{
    private readonly HashSet<object> _objects = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRefactor"/> class.
    /// </summary>
    public LocalRefactor()
    {
    }

    /// <summary>
    /// Adds multiple objects to the refactoring scope.
    /// </summary>
    /// <param name="objs">The collection of objects to add.</param>
    public void AddObjects(IEnumerable<object> objs)
    {
        foreach (var obj in objs)
        {
            AddObject(obj);
        }
    }

    /// <summary>
    /// Adds a single object to the refactoring scope, including all contained objects visited recursively.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    public void AddObject(object obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        _objects.Add(obj);

        // Add all contained objects, e.g., states under a state machine should all be included.
        Visitor.Visit(obj, (item, path) =>
        {
            _objects.Add(item);
            return true;
        });
    }

    /// <summary>
    /// Renames all references from <paramref name="oldId"/> to <paramref name="newId"/> within the tracked objects.
    /// </summary>
    /// <param name="oldId">The original identifier to rename from.</param>
    /// <param name="newId">The new identifier to rename to.</param>
    public void Rename(Guid oldId, Guid newId)
    {
        if (oldId == newId)
        {
            return;
        }

        var sync = new LocalRenameSync(oldId, newId);

        foreach (var obj in _objects)
        {
            if (obj is IReferenceHost referenceHost)
            {
                referenceHost.ReferenceSync(SyncPath.Empty, sync);
            }
            else
            {
                Visitor.Visit<IReference>(obj, (referencer, pathContext) =>
                {
                    referencer.ReferenceSync(pathContext.GetPath(), sync);
                });
            }
        }
    }

    /// <summary>
    /// Clears all tracked objects from the refactoring scope.
    /// </summary>
    public void Clear()
    {
        _objects.Clear();
    }
}

/// <summary>
/// Internal synchronization object that redirects references from an old ID to a new ID.
/// </summary>
internal class LocalRenameSync(Guid oldId, Guid newId) : IReferenceSync
{
    private readonly Guid _oldId = oldId;
    private readonly Guid _newId = newId;

    /// <inheritdoc/>
    public ReferenceSyncMode Mode => ReferenceSyncMode.Redirect;

    /// <inheritdoc/>
    public Guid Id => _newId;

    /// <inheritdoc/>
    public Guid OldId => _oldId;

    /// <inheritdoc/>
    public Guid SyncId(SyncPath path, Guid id, string message)
    {
        if (id == _oldId)
        {
            return _newId;
        }
        else
        {
            return id;
        }
    }
}