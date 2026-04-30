using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Views.Named;

/// <summary>
/// Interface for a synchronized named list that supports name-based access and conflict resolution.
/// </summary>
/// <typeparam name="TValue">The type of items in the list, must be a class implementing <see cref="ISyncObject"/>.</typeparam>
public interface INamedSyncList<TValue> : ISyncList, IList<TValue>, IEnumerable<TValue>, IHasObjectCreationGUI
    where TValue : class, ISyncObject
{
    /// <summary>
    /// Event raised when an item is added to the list.
    /// </summary>
    event Action<TValue, bool> ItemAdded;

    /// <summary>
    /// Event raised when an item is removed from the list.
    /// </summary>
    event Action<TValue> ItemRemoved;

    /// <summary>
    /// Gets or sets the GUI-based value creator for this list.
    /// </summary>
    GuiObjectCreation ValueCreaterGUI { get; set; }

    /// <summary>
    /// Gets or sets the function that suggests a prefix for naming new items.
    /// </summary>
    Func<TValue, string> PrefixSuggest { get; set; }

    /// <summary>
    /// Gets or sets the function that resolves name conflicts.
    /// </summary>
    Func<string, string> ConflictResolver { get; set; }

    /// <summary>
    /// Gets or sets the predicate that checks if an item can be added.
    /// </summary>
    Predicate<TValue> AddItemChecker { get; set; }

    /// <summary>
    /// Gets or sets the default prefix used for naming new items.
    /// </summary>
    string DefaultPrefix { get; set; }

    /// <summary>
    /// Removes an item by name.
    /// </summary>
    /// <param name="name">The name of the item to remove.</param>
    /// <returns>True if the item was found and removed; otherwise, false.</returns>
    bool RemoveByName(string name);

    /// <summary>
    /// Gets an item by name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The item with the specified name, or null if not found.</returns>
    TValue this[string name] { get; }

    /// <summary>
    /// Generates a suggested name with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 1).</param>
    /// <returns>A suggested name string.</returns>
    string GetSuggestedName(string prefix, int digiLen = 1);

    /// <summary>
    /// Checks if an item with the specified name exists in the list.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if an item with the name exists; otherwise, false.</returns>
    bool ContainsName(string name);

    /// <summary>
    /// Gets an item by name, returning the default value if not found.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The item with the specified name, or the default value if not found.</returns>
    TValue GetValueOrDefault(string name);

    /// <summary>
    /// Gets the count of items in the list.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// Changes the name of an item in the list.
    /// </summary>
    /// <param name="item">The item to rename.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="setNameProperty">Whether to set the name property on the item.</param>
    /// <returns>True if the rename was successful; otherwise, false.</returns>
    bool ChangeName(TValue item, string newName, bool setNameProperty);
}
