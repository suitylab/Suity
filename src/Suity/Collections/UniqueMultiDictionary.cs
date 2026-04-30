using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Collections;

/// <summary>
/// Simple unique multi-value dictionary
/// </summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TValue">Value type</typeparam>
public class UniqueMultiDictionary<TKey, TValue>
{
    private int _count;
    private readonly Dictionary<TKey, HashSet<TValue>> _dic;

    public UniqueMultiDictionary()
    {
        _dic = [];
    }

    public UniqueMultiDictionary(IEqualityComparer<TKey> comparer)
    {
        _dic = new(comparer);
    }

    /// <summary>
    /// Adds a value to the dictionary under the specified key.
    /// </summary>
    /// <param name="key">The key to add the value under.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the value was added, false if it already existed.</returns>
    public bool Add(TKey key, TValue value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (EnsureSet(key).Add(value))
        {
            _count++;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a value from the dictionary under the specified key.
    /// </summary>
    /// <param name="key">The key to remove the value from.</param>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the value was removed, false if it did not exist.</returns>
    public bool Remove(TKey key, TValue value)
    {
        if (_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            bool removed = set.Remove(value);
            if (removed)
            {
                _count--;
                if (set.Count == 0)
                {
                    _dic.Remove(key);
                }
            }

            return removed;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all values under the specified key.
    /// </summary>
    /// <param name="key">The key to remove all values from.</param>
    public void RemoveAll(TKey key)
    {
        if (_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            int removeCount = set.Count;
            _dic.Remove(key);
            _count -= removeCount;
        }
    }

    /// <summary>
    /// Removes all occurrences of the specified value from the dictionary.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>The number of values removed.</returns>
    public int RemoveAllValues(TValue value)
    {
        TKey[] keys = [.. Keys];
        return keys.Count(key => Remove(key, value));
    }

    /// <summary>
    /// Clears all values from the dictionary.
    /// </summary>
    public void Clear()
    {
        _dic.Clear();
        _count = 0;
    }

    /// <summary>
    /// Renames a key in the dictionary.
    /// </summary>
    /// <param name="oldKey">The old key to rename.</param>
    /// <param name="newKey">The new key to rename to.</param>
    /// <returns>True if the key was renamed, false if the old key does not exist or the new key already exists.</returns>
    public bool RenameKey(TKey oldKey, TKey newKey)
    {
        if (_dic.TryGetValue(oldKey, out HashSet<TValue> set) && !_dic.ContainsKey(newKey))
        {
            _dic.Remove(oldKey);
            _dic.Add(newKey, set);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Combines the values of two keys into one.
    /// </summary>
    /// <param name="oldKey">The old key to combine.</param>
    /// <param name="newKey">The new key to combine into.</param>
    public void RenameCombineKey(TKey oldKey, TKey newKey)
    {
        if (_dic.TryGetValue(oldKey, out HashSet<TValue> oldSet))
        {
            _dic.Remove(oldKey);

            if (_dic.TryGetValue(newKey, out HashSet<TValue> newSet))
            {
                newSet.AddRange(oldSet);
            }
            else
            {
                _dic.Add(newKey, oldSet);
            }
        }
    }

    /// <summary>
    /// Gets the number of keys in the dictionary.
    /// </summary>
    public int Count => _dic.Count;

    /// <summary>
    /// Gets the total number of values in the dictionary.
    /// </summary>
    public int ValueCount => _count;

    /// <summary>
    /// Gets the number of values under the specified key.
    /// </summary>
    /// <param name="key">The key to get the value count for.</param>
    /// <returns>The number of values under the key.</returns>
    public int GetValueCount(TKey key)
    {
        if (_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            return set.Count;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the number of keys in the dictionary.
    /// </summary>
    public int KeyCount => _dic.Count;

    /// <summary>
    /// Checks if the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool ContainsKey(TKey key) => _dic.ContainsKey(key);

    /// <summary>
    /// Checks if the dictionary contains the specified key-value pair.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the key-value pair exists, false otherwise.</returns>
    public bool Contains(TKey key, TValue value)
    {
        if (_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            return set.Contains(value);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the first value under the specified key.
    /// </summary>
    /// <param name="key">The key to get the first value for.</param>
    /// <returns>The first value under the key, or the default value if the key does not exist.</returns>
    public TValue GetFirstOrDefault(TKey key)
    {
        if (_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            return set.FirstOrDefault();
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Gets all keys in the dictionary.
    /// </summary>
    public IEnumerable<TKey> Keys => _dic.Keys;

    /// <summary>
    /// Gets all keys that contain the specified value.
    /// </summary>
    /// <param name="value">The value to get keys for.</param>
    /// <returns>An enumerable of keys that contain the value.</returns>
    public IEnumerable<TKey> GetKeysByValue(TValue value)
    {
        foreach (var pair in _dic)
        {
            if (pair.Value.Contains(value))
            {
                yield return pair.Key;
            }
        }
    }

    /// <summary>
    /// Gets all values under the specified key.
    /// </summary>
    /// <param name="key">The key to get values for.</param>
    /// <returns>An enumerable of values under the key.</returns>
    public IEnumerable<TValue> this[TKey key]
    {
        get
        {
            if (_dic.TryGetValue(key, out HashSet<TValue> set))
            {
                foreach (TValue value in set)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Gets all values in the dictionary.
    /// </summary>
    public IEnumerable<TValue> Values
    {
        get
        {
            foreach (HashSet<TValue> set in _dic.Values)
            {
                foreach (TValue value in set)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Gets all values that are unique (only appear once) in the dictionary.
    /// </summary>
    public IEnumerable<TValue> SoloValues
    {
        get
        {
            foreach (HashSet<TValue> set in _dic.Values)
            {
                if (set.Count == 1)
                {
                    foreach (TValue value in set)
                    {
                        yield return value;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the first value for each key in the dictionary.
    /// </summary>
    public IEnumerable<TValue> FirstValues
    {
        get
        {
            foreach (HashSet<TValue> set in _dic.Values)
            {
                if (set.Count > 0)
                {
                    yield return set.First();
                }
            }
        }
    }

    /// <summary>
    /// Gets all key-value pairs in the dictionary.
    /// </summary>
    public IEnumerable<KeyValuePair<TKey, TValue>> Pairs
    {
        get
        {
            foreach (var pair in _dic)
            {
                foreach (var value in pair.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(pair.Key, value);
                }
            }
        }
    }

    /// <summary>
    /// Ensures that a set exists for the specified key, creating one if necessary.
    /// </summary>
    /// <param name="key">The key to ensure a set for.</param>
    /// <returns>The set for the key.</returns>
    private HashSet<TValue> EnsureSet(TKey key)
    {
        if (!_dic.TryGetValue(key, out HashSet<TValue> set))
        {
            set = [];
            _dic.Add(key, set);
        }

        return set;
    }

    /// <summary>
    /// Creates a clone of the dictionary.
    /// </summary>
    /// <returns>A new instance of UniqueMultiDictionary with the same data.</returns>
    public UniqueMultiDictionary<TKey, TValue> Clone()
    {
        var clone = new UniqueMultiDictionary<TKey, TValue>();

        foreach (var pair in _dic)
        {
            HashSet<TValue> hashSet = [.. pair.Value];
            clone._dic.Add(pair.Key, hashSet);
        }

        return clone;
    }

    /// <summary>
    /// Returns a string representation of the dictionary.
    /// </summary>
    /// <returns>A string representing the dictionary.</returns>
    public override string ToString() => $"{this.GetType().Name}({Count})";
}
