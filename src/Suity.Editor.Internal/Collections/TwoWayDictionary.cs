using System;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// A bidirectional dictionary that maintains a one-to-one mapping between keys and values,
/// allowing efficient lookup in both directions.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public class TwoWayDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private readonly Dictionary<TKey, TValue> _dic = [];
    private readonly Dictionary<TValue, TKey> _dicRev = [];

    /// <summary>
    /// Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="key"/> or <paramref name="value"/> already exists in the dictionary.</exception>
    public void Add(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (_dic.ContainsKey(key)) throw new ArgumentException("key exists.");
        if (_dicRev.ContainsKey(value)) throw new ArgumentException("value exists.");

        _dic[key] = value;
        _dicRev[value] = key;
    }

    /// <summary>
    /// Removes the entry with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns><c>true</c> if the entry was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(TKey key)
    {
        if (_dic.TryGetValue(key, out TValue value))
        {
            _dic.Remove(key);
            _dicRev.Remove(value);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the entry with the specified value from the dictionary.
    /// </summary>
    /// <param name="value">The value of the entry to remove.</param>
    /// <returns><c>true</c> if the entry was found and removed; otherwise, <c>false</c>.</returns>
    public bool RemoveValue(TValue value)
    {
        if (_dicRev.TryGetValue(value, out TKey key))
        {
            _dicRev.Remove(value);
            _dic.Remove(key);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all entries from the dictionary.
    /// </summary>
    public void Clear()
    {
        _dic.Clear();
        _dicRev.Clear();
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">When this method returns, contains the value associated with the key, if found; otherwise, the default value for the type.</param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        return _dic.TryGetValue(key, out value);
    }

    /// <summary>
    /// Attempts to get the key associated with the specified value.
    /// </summary>
    /// <param name="value">The value to look up.</param>
    /// <param name="key">When this method returns, contains the key associated with the value, if found; otherwise, the default value for the type.</param>
    /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
    public bool TryGetKey(TValue value, out TKey key)
    {
        return _dicRev.TryGetValue(value, out key);
    }

    /// <summary>
    /// Gets the number of key-value pairs in the dictionary.
    /// </summary>
    public int Count => _dic.Count;

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(TKey key) => _dic.ContainsKey(key);

    /// <summary>
    /// Determines whether the dictionary contains the specified value.
    /// </summary>
    /// <param name="value">The value to locate.</param>
    /// <returns><c>true</c> if the value exists; otherwise, <c>false</c>.</returns>
    public bool ContainsValue(TValue value) => _dicRev.ContainsKey(value);

    /// <summary>
    /// Gets a collection containing all keys in the dictionary.
    /// </summary>
    public IEnumerable<TKey> Keys => _dic.Keys;

    /// <summary>
    /// Gets a collection containing all values in the dictionary.
    /// </summary>
    public IEnumerable<TValue> Values => _dicRev.Keys;

    /// <summary>
    /// Changes the key associated with the specified value to a new key.
    /// </summary>
    /// <param name="value">The value whose key should be changed.</param>
    /// <param name="newKey">The new key to associate with the value.</param>
    /// <returns><c>true</c> if the key was changed successfully; otherwise, <c>false</c> if the value was not found or the new key already exists.</returns>
    public bool ChangeKey(TValue value, TKey newKey)
    {
        if (!_dicRev.TryGetValue(value, out TKey oldKey))
        {
            return false;
        }

        if (_dic.ContainsKey(newKey))
        {
            return false;
        }

        _dic.Remove(oldKey);
        _dicRev.Remove(value);

        _dic.Add(newKey, value);
        _dicRev.Add(value, newKey);

        return true;
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return _dic.GetEnumerator();
    }

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _dic.GetEnumerator();
    }
}
