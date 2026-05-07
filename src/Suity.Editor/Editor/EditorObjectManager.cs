using System;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Editor;

/// <summary>
/// Editor object manager
/// </summary>
public abstract class EditorObjectManager
{
    /// <summary>
    /// Gets the singleton instance of the EditorObjectManager.
    /// </summary>
    public static EditorObjectManager Instance { get; internal set; }

    /// <summary>
    /// Occurs when watching is paused.
    /// </summary>
    public event Action WatchingPaused;

    /// <summary>
    /// Occurs when watching is resumed.
    /// </summary>
    public event Action WatchingResume;

    private int _num = 0;
    internal bool _watchingDisabled;

    internal EditorObjectManager()
    { }

    /// <summary>
    /// Gets a value indicating whether watching is disabled.
    /// </summary>
    public bool IsWatchingDisabled => _watchingDisabled || _num > 0;

    /// <summary>
    /// Creates an editor object using the specified creation function.
    /// </summary>
    /// <typeparam name="T">The type of editor object to create.</typeparam>
    /// <param name="creation">The creation function.</param>
    /// <returns>The created editor object.</returns>
    internal abstract T Create<T>(Func<T> creation) where T : EditorObject;

    /// <summary>
    /// Creates an editor object using the specified creation function and returns the object entry.
    /// </summary>
    /// <typeparam name="T">The type of editor object to create.</typeparam>
    /// <param name="creation">The creation function.</param>
    /// <param name="entry">The created object entry.</param>
    /// <returns>The created editor object.</returns>
    internal abstract T Create<T>(Func<T> creation, out ObjectEntry entry) where T : EditorObject;

    /// <summary>
    /// Creates a new object entry.
    /// </summary>
    /// <returns>The new object entry.</returns>
    internal abstract ObjectEntry NewEntry();

    /// <summary>
    /// Gets the editor object with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the object.</param>
    /// <returns>The editor object, or null if not found.</returns>
    public abstract EditorObject GetObject(Guid id);

    /// <summary>
    /// Gets the object entry with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>The object entry, or null if not found.</returns>
    public abstract ObjectEntry GetEntry(Guid id);

    /// <summary>
    /// Ensures an object entry exists for the specified ID, creating one if necessary.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>The object entry.</returns>
    internal abstract ObjectEntry EnsureEntry(Guid id);

    /// <summary>
    /// Gets all object entries.
    /// </summary>
    internal abstract IEnumerable<ObjectEntry> Entries { get; }

    /// <summary>
    /// Gets all editor objects that are currently active.
    /// </summary>
    internal abstract IEnumerable<EditorObject> Objects { get; }

    /// <summary>
    /// Gets all editor objects including inactive ones.
    /// </summary>
    internal abstract IEnumerable<EditorObject> AllObjects { get; }

    /// <summary>
    /// Gets the count of editor objects.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Resolves an object entry based on the specified resolution type and parameters.
    /// </summary>
    /// <param name="resolveType">The type of resolution to use.</param>
    /// <param name="fullName">The full name of the object.</param>
    /// <param name="lastId">The last known ID.</param>
    /// <returns>The resolved object entry.</returns>
    internal abstract ObjectEntry ResolveEntry(IdResolveType resolveType, string fullName, Guid lastId);

    /// <summary>
    /// Creates a field object collection for the specified owner.
    /// </summary>
    /// <typeparam name="T">The type of field object in the collection.</typeparam>
    /// <param name="owner">The owner editor object.</param>
    /// <returns>A new field object collection.</returns>
    public abstract FieldObjectCollection<T> CreateFieldCollection<T>(EditorObject owner)
        where T : FieldObject, new();

    internal abstract void RegisterSystemAlias(Guid id, EditorObject obj);

    /// <summary>
    /// Executes an action while temporarily disabling watching.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void DoUnwatchedAction(Action action)
    {
        try
        {
            int n = Interlocked.Increment(ref _num);
            if (n == 1)
            {
                WatchingPaused?.Invoke();
            }

            action();
        }
        finally
        {
            int n = Interlocked.Decrement(ref _num);
            if (n == 0)
            {
                WatchingResume?.Invoke();
            }
        }
    }
}