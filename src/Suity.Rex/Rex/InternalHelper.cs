using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Rex;

/// <summary>
/// Internal helper methods for dictionary operations and enumeration.
/// </summary>
internal static class InternalHelper
{
    /// <summary>
    /// Safely gets a value from a dictionary, returning the default value if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value associated with the key, or the default value if not found.</returns>
    public static TValue GetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (key == null)
        {
            return default;
        }

        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Gets the value for a key, or adds and returns a default value if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary to search or modify.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to add if the key is not found.</param>
    /// <returns>The existing value or the added default value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
    {
        if (key == null)
        {
            return default;
        }

        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }
        else
        {
            dictionary.Add(key, defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the value for a key, or creates and adds a new value using a factory function if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary to search or modify.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="creation">A function to create the new value.</param>
    /// <param name="added">When this method returns, contains true if a new value was added; otherwise, false.</param>
    /// <returns>The existing value or the newly created value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creation, out bool added)
    {
        if (key == null)
        {
            added = false;
            return default;
        }

        if (dictionary.TryGetValue(key, out TValue value))
        {
            added = false;
            return value;
        }
        else
        {
            value = creation(key);
            dictionary.Add(key, value);

            added = true;
            return value;
        }
    }

    /// <summary>
    /// Gets the value for a key, or creates and adds a new value using a factory function if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary to search or modify.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="creation">A function to create the new value.</param>
    /// <returns>The existing value or the newly created value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creation)
    {
        if (key == null)
        {
            return default;
        }

        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }
        else
        {
            value = creation(key);
            dictionary.Add(key, value);

            return value;
        }
    }

    /// <summary>
    /// Removes and returns the value associated with the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary to modify.</param>
    /// <param name="key">The key to remove.</param>
    /// <returns>The removed value, or the default value if the key was not found.</returns>
    public static TValue RemoveAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (key == null)
        {
            return default;
        }

        if (dictionary.TryGetValue(key, out TValue value))
        {
            dictionary.Remove(key);
            return value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Executes an action for each element in the source enumeration.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source enumeration.</param>
    /// <param name="action">The action to execute for each element.</param>
    public static void Foreach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Returns a pass-through enumerable that yields each element from the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="collection">The source collection.</param>
    /// <returns>An enumerable that yields the same elements as the source.</returns>
    public static IEnumerable<T> Pass<T>(this IEnumerable<T> collection) => collection.Select(o => o);
}
