using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Collections;

public static class CollectionExtensions
{
    #region IList<T> && Array

    /// <summary>
    /// Inserts an item into a sorted list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to insert the item into.</param>
    /// <param name="item">The item to insert.</param>
    /// <param name="compare">The comparison function to determine the order.</param>
    public static void InsertSorted<T>(this IList<T> list, T item, Comparison<T> compare)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (compare(item, list[i]) < 0)
            {
                list.Insert(i, item);
                return;
            }
        }
        list.Add(item);
    }

    /// <summary>
    /// Checks if two lists contain the same elements in the same order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the lists.</typeparam>
    /// <param name="list">The first list.</param>
    /// <param name="other">The second list.</param>
    /// <returns>True if the lists are equal, otherwise false.</returns>
    public static bool ElementEquals<T>(this IList<T> list, IList<T> other)
    {
        if (list == null || other == null)
        {
            return false;
        }
        if (list.Count != other.Count)
        {
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (!object.Equals(list[i], other[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Finds the index of an element in a list that matches a predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to search.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <returns>The index of the matching element, or -1 if not found.</returns>
    public static int IndexOf<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Safely gets an item from a list by index, returning a default value if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the default value if the index is out of range.</returns>
    public static T GetListItemSafe<T>(this IList<T> list, int index)
    {
        if (list == null)
        {
            return default;
        }

        if (index < 0 || index >= list.Count)
        {
            return default;
        }
        else
        {
            return list[index];
        }
    }

    /// <summary>
    /// Safely gets an item from a list by index, returning the first or last item if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the first or last item if the index is out of range.</returns>
    public static T GetListItemMinMax<T>(this IList<T> list, int index)
    {
        if (list == null || list.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            return list[0];
        }
        else if (index >= list.Count)
        {
            return list[list.Count - 1];
        }
        else
        {
            return list[index];
        }
    }

    /// <summary>
    /// Gets the last item in a list, or a default value if the list is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to get the last item from.</param>
    /// <returns>The last item in the list, or the default value if the list is empty.</returns>
    public static T LastOrDefault<T>(this IList<T> list)
    {
        if (list is null)
        {
            return default;
        }

        if (list.Count > 0)
        {
            return list[list.Count - 1];
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Gets the last item in an array, or a default value if the array is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="list">The array to get the last item from.</param>
    /// <returns>The last item in the array, or the default value if the array is empty.</returns>
    public static T LastOrDefault<T>(this T[] list)
    {
        if (list is null)
        {
            return default;
        }

        if (list.Length > 0)
        {
            return list[list.Length - 1];
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Ensures that a list has at least a specified size by adding default values if necessary.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to ensure the size of.</param>
    /// <param name="size">The minimum size of the list.</param>
    public static void EnsureListSize<T>(this IList<T> list, int size)
    {
        while (list.Count < size)
        {
            list.Add(default);
        }
    }

    /// <summary>
    /// Ensures that a list has at least a specified size by adding new elements created by a factory function if necessary.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to ensure the size of.</param>
    /// <param name="size">The minimum size of the list.</param>
    /// <param name="creation">The factory function to create new elements.</param>
    public static void EnsureListSize<T>(this IList<T> list, int size, Func<T> creation)
    {
        while (list.Count < size)
        {
            list.Add(creation());
        }
    }

    /// <summary>
    /// Safely gets an item from an array by index, returning the first or last item if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="ary">The array to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the first or last item if the index is out of range.</returns>
    public static T GetArrayItemMinMax<T>(this T[] ary, int index)
    {
        if (ary == null || ary.Length == 0)
        {
            return default;
        }

        if (index < 0)
        {
            return ary[0];
        }
        else if (index >= ary.Length)
        {
            return ary[ary.Length - 1];
        }
        else
        {
            return ary[index];
        }
    }

    /// <summary>
    /// Safely gets an item from an array by index, returning a default value if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="ary">The array to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the default value if the index is out of range.</returns>
    public static T GetArrayItemSafe<T>(this T[] ary, int index)
    {
        if (ary is null)
        {
            return default;
        }

        if (index < 0 || index >= ary.Length)
        {
            return default;
        }
        else
        {
            return ary[index];
        }
    }

    /// <summary>
    /// Gets the last item in an array, or a default value if the array is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="ary">The array to get the last item from.</param>
    /// <returns>The last item in the array, or the default value if the array is empty.</returns>
    public static T LastOfDefault<T>(this T[] ary)
    {
        if (ary is null)
        {
            return default;
        }

        if (ary.Length == 0)
        {
            return ary[ary.Length - 1];
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Finds the index of an element in an array that matches a predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="ary">The array to search.</param>
    /// <param name="condition">The predicate to match.</param>
    /// <returns>The index of the matching element, or -1 if not found.</returns>
    public static int IndexOf<T>(this T[] ary, Predicate<T> condition)
    {
        for (int i = 0; i < ary.Length; i++)
        {
            if (condition(ary[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns an enumerable that iterates over an array in reverse order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="ary">The array to iterate over.</param>
    /// <returns>An enumerable that iterates over the array in reverse order.</returns>
    public static IEnumerable<T> ReverseEnumerable<T>(this T[] ary)
    {
        for (int i = ary.Length - 1; i >= 0; i--)
        {
            yield return ary[i];
        }
    }

    /// <summary>
    /// Gets a random item from a list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="rnd">The random number generator to use.</param>
    /// <param name="list">The list to get the item from.</param>
    /// <returns>A random item from the list, or the default value if the list is empty.</returns>
    public static T GetRandomListItem<T>(this Random rnd, List<T> list)
    {
        if (list.Count == 0)
        {
            return default;
        }

        return list[rnd.Next(list.Count)];
    }

    /// <summary>
    /// Gets a random item from a list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to get the item from.</param>
    /// <param name="rnd">The random number generator to use.</param>
    /// <returns>A random item from the list, or the default value if the list is empty.</returns>
    public static T GetRandomListItem<T>(this IList<T> list, Random rnd)
    {
        if (list.Count == 0)
        {
            return default;
        }

        return list[rnd.Next(list.Count)];
    }

    /// <summary>
    /// Gets a random item from an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="rnd">The random number generator to use.</param>
    /// <param name="list">The array to get the item from.</param>
    /// <returns>A random item from the array, or the default value if the array is empty.</returns>
    public static T GetRandomArrayItem<T>(this Random rnd, T[] list)
    {
        if (list.Length == 0)
        {
            return default;
        }

        return list[rnd.Next(list.Length)];
    }

    /// <summary>
    /// Gets a random item from an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="list">The array to get the item from.</param>
    /// <param name="rnd">The random number generator to use.</param>
    /// <returns>A random item from the array, or the default value if the array is empty.</returns>
    public static T GetRandomArrayItem<T>(this T[] list, Random rnd)
    {
        if (list.Length == 0)
        {
            return default;
        }

        return list[rnd.Next(list.Length)];
    }

    /// <summary>
    /// Takes a specified number of items from a list, optionally filtering by a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="source">The list to take items from.</param>
    /// <param name="count">The number of items to take.</param>
    /// <param name="condition">An optional condition to filter items by.</param>
    /// <returns>An enumerable containing the taken items.</returns>
    public static IEnumerable<T> TakeSafe<T>(this IList<T> source, int count, Predicate<T> condition = null)
    {
        if (count <= 0)
        {
            yield break;
        }

        int num = 0;

        for (int i = 0; i < source.Count; i++)
        {
            T value = source[i];
            if (condition == null || condition(value))
            {
                yield return value;
            }

            num++;
            if (num >= count)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Takes a specified number of items from the end of a list, optionally filtering by a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="source">The list to take items from.</param>
    /// <param name="count">The number of items to take.</param>
    /// <param name="condition">An optional condition to filter items by.</param>
    /// <returns>An enumerable containing the taken items.</returns>
    public static IEnumerable<T> TakeLastSafe<T>(this IList<T> source, int count, Predicate<T> condition = null)
    {
        if (count <= 0)
        {
            yield break;
        }

        int num = 0;

        for (int i = source.Count - 1; i >= 0; i--)
        {
            T value = source[i];
            if (condition == null || condition(value))
            {
                yield return value;
            }

            num++;
            if (num >= count)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Calculates the number of pages required to display a specified number of items per page.
    /// </summary>
    /// <param name="totalItemCount">The total number of items.</param>
    /// <param name="pageItemCount">The number of items per page.</param>
    /// <returns>The number of pages required.</returns>
    public static int GetPageCount(int totalItemCount, int pageItemCount)
    {
        if (totalItemCount == 0)
        {
            return 1;
        }

        return (totalItemCount - 1) / pageItemCount + 1;
    }

    /// <summary>
    /// Gets a specified page of items from a list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to get items from.</param>
    /// <param name="pageIndex">The index of the page to get.</param>
    /// <param name="pageItemCount">The number of items per page.</param>
    /// <returns>An enumerable containing the items on the specified page.</returns>
    public static IEnumerable<T> GetPagedItems<T>(this IList<T> list, int pageIndex, int pageItemCount)
    {
        int startIndex = pageIndex * pageItemCount;

        if (startIndex < 0 || startIndex >= list.Count)
        {
            yield break;
        }

        int endCount = Math.Min(list.Count, startIndex + pageItemCount);

        for (int i = startIndex; i < endCount; i++)
        {
            yield return list[i];
        }
    }

    /// <summary>
    /// Gets a specified page of items from an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to get items from.</param>
    /// <param name="pageIndex">The index of the page to get.</param>
    /// <param name="pageItemCount">The number of items per page.</param>
    /// <returns>An enumerable containing the items on the specified page.</returns>
    public static IEnumerable<T> GetPagedItems<T>(this T[] array, int pageIndex, int pageItemCount)
    {
        int startIndex = pageIndex * pageItemCount;

        if (startIndex < 0 || startIndex >= array.Length)
        {
            yield break;
        }

        int endCount = Math.Min(array.Length, startIndex + pageItemCount);

        for (int i = startIndex; i < endCount; i++)
        {
            yield return array[i];
        }
    }

    /// <summary>
    /// Safely removes an item from the list at the specified index.
    /// </summary>
    /// <param name="list">The list from which to remove the item.</param>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    public static bool RemoveAtSafe<T>(this IList<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            return false;
        }

        list.RemoveAt(index);

        return true;
    }

    /// <summary>
    /// Safely swaps two items in the list.
    /// </summary>
    /// <param name="list">The list in which to swap the items.</param>
    /// <param name="index">The index of the first item.</param>
    /// <param name="indexTo">The index of the second item.</param>
    /// <returns>True if the items were successfully swapped, false otherwise.</returns>
    public static bool SwapListItem<T>(this IList<T> list, int index, int indexTo)
    {
        if (index == indexTo)
        {
            return false;
        }

        if (index < 0 || index >= list.Count || indexTo < 0 || indexTo >= list.Count)
        {
            return false;
        }

        var item = list[index];
        var itemTo = list[indexTo];

        list[index] = itemTo;
        list[indexTo] = item;

        return true;
    }

    /// <summary>
    /// Safely removes an item from the list and inserts it at another position.
    /// </summary>
    /// <param name="list">The list from which to remove and insert the item.</param>
    /// <param name="indexFrom">The index of the item to remove.</param>
    /// <param name="indexInsert">The index at which to insert the item.</param>
    /// <returns>True if the item was successfully removed and inserted, false otherwise.</returns>
    public static bool RemoveInserListItem<T>(this IList<T> list, int indexFrom, int indexInsert)
    {
        if (indexFrom == indexInsert)
        {
            return false;
        }

        if (indexFrom < 0 || indexFrom >= list.Count || indexInsert < 0 || indexInsert >= list.Count + 1)
        {
            return false;
        }

        var path = list[indexFrom];
        if (indexInsert > indexFrom)
        {
            list.Insert(indexInsert, path);
            list.RemoveAt(indexFrom);
        }
        else
        {
            list.RemoveAt(indexFrom);
            list.Insert(indexInsert, path);
        }

        return true;
    }

    /// <summary>
    /// Removes duplicate items from the list.
    /// </summary>
    /// <param name="list">The list from which to remove duplicates.</param>
    /// <returns>True if any duplicates were removed, false otherwise.</returns>
    public static bool RemoveDuplicates<T>(this List<T> list)
    {
        // Create a HashSet<T> to store the elements that have been encountered
        HashSet<T> encountered = [];

        bool removed = false;

        // Iterate over the elements in a list
        for (int i = 0; i < list.Count; i++)
        {
            T current = list[i];

            // If the current element has been encountered, remove it from the list
            if (encountered.Contains(current))
            {
                list.RemoveAt(i);
                i--; // Update the index to process the next element after the removed element
                removed = true;
            }
            else
            {
                encountered.Add(current);
            }
        }

        return removed;
    }

    /// <summary>
    /// Returns an enumerable that iterates over the list in reverse order.
    /// </summary>
    /// <param name="list">The list to iterate over in reverse.</param>
    /// <returns>An enumerable that yields the list items in reverse order.</returns>
    public static IEnumerable<T> ReverseEnumerable<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            yield return list[i];
        }
    }

    #endregion

    #region IDictionary

    /// <summary>
    /// Adds a range of values to the dictionary using a key resolution function.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the values.</param>
    /// <param name="values">The values to add to the dictionary.</param>
    /// <param name="keyResolve">A function that resolves the key for each value.</param>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TValue> values, Func<TValue, TKey> keyResolve)
    {
        foreach (var value in values)
        {
            TKey key = keyResolve(value);
            if (key is null)
            {
                throw new NullReferenceException(nameof(key));
            }

            dictionary[key] = value;
        }
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        foreach (var pair in pairs)
        {
            dictionary[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Adds a range of values to the dictionary using a key resolution function, but skips duplicates.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the values.</param>
    /// <param name="values">The values to add to the dictionary.</param>
    /// <param name="keyResolve">A function that resolves the key for each value.</param>
    public static void AddRangeSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TValue> values, Func<TValue, TKey> keyResolve)
    {
        foreach (var value in values)
        {
            TKey key = keyResolve(value);
            if (key is null)
            {
                continue;
            }

            if (dictionary.ContainsKey(key))
            {
                continue;
            }

            dictionary[key] = value;
        }
    }

    /// <summary>
    /// Adds a range of items to the dictionary using key and value resolution functions.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the items.</param>
    /// <param name="source">The source items to add to the dictionary.</param>
    /// <param name="keyResolve">A function that resolves the key for each item.</param>
    /// <param name="valueResolve">A function that resolves the value for each item.</param>
    public static void AddRange<T, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<T> source, Func<T, TKey> keyResolve, Func<T, TValue> valueResolve)
    {
        foreach (var item in source)
        {
            TKey key = keyResolve(item);
            if (key is null)
            {
                throw new NullReferenceException(nameof(key));
            }

            TValue value = valueResolve(item);
            dictionary[key] = value;
        }
    }

    /// <summary>
    /// Adds a range of items to the dictionary using key and value resolution functions, but skips duplicates.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the items.</param>
    /// <param name="source">The source items to add to the dictionary.</param>
    /// <param name="keyResolve">A function that resolves the key for each item.</param>
    /// <param name="valueResolve">A function that resolves the value for each item.</param>
    public static void AddRangeSafe<T, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<T> source, Func<T, TKey> keyResolve, Func<T, TValue> valueResolve)
    {
        foreach (var item in source)
        {
            TKey key = keyResolve(item);
            if (key is null)
            {
                continue;
            }

            if (dictionary.ContainsKey(key))
            {
                continue;
            }

            TValue value = valueResolve(item);
            dictionary[key] = value;
        }
    }

    /// <summary>
    /// Safely retrieves a value from the dictionary by key, returning the default value if the key is not found.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve the value.</param>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the key, or the default value if the key is not found.</returns>
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
    /// Converts an enumerable to a dictionary, handling invalid keys.
    /// </summary>
    /// <param name="source">The source enumerable to convert.</param>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="elementSelector">A function to extract the element from each item.</param>
    /// <param name="keyInvalid">An optional action to perform when an invalid key is encountered.</param>
    /// <returns>A dictionary containing the elements from the source enumerable.</returns>
    public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        Action<TSource> keyInvalid = null)
    {
        Dictionary<TKey, TElement> dic = [];
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (key is null)
            {
                keyInvalid?.Invoke(item);
                continue;
            }

            if (dic.ContainsKey(key))
            {
                keyInvalid?.Invoke(item);
                continue;
            }

            var value = elementSelector(item);
            dic.Add(key, value);
        }

        return dic;
    }

    /// <summary>
    /// Safely retrieves a value from the dictionary by key, returning a specified default value if the key is not found.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve the value.</param>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="defalutValue">The default value to return if the key is not found.</param>
    /// <returns>The value associated with the key, or the specified default value if the key is not found.</returns>
    public static TValue GetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defalutValue)
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
            return defalutValue;
        }
    }

    [Obsolete]
    /// <summary>
    /// Retrieves a value from the dictionary by key, returning the default value if the key is not found.
    /// (This method is obsolete and should not be used.)
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve the value.</param>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the key, or the default value if the key is not found.</returns>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
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

    [Obsolete]
    /// <summary>
    /// Retrieves a value from the dictionary by key, returning a specified default value if the key is not found.
    /// (This method is obsolete and should not be used.)
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve the value.</param>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="defalutValue">The default value to return if the key is not found.</param>
    /// <returns>The value associated with the key, or the specified default value if the key is not found.</returns>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defalutValue)
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
            return defalutValue;
        }
    }

    [Obsolete]
    /// <summary>
    /// Retrieves a value from the dictionary by key, creating the value if it does not exist.
    /// (This method is obsolete and should not be used.)
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve or create the value.</param>
    /// <param name="key">The key of the value to retrieve or create.</param>
    /// <param name="creation">A function that creates the value if it does not exist.</param>
    /// <returns>The value associated with the key, or the newly created value if the key was not found.</returns>
    public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> creation)
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
            value = creation();
            dictionary.Add(key, value);

            return value;
        }
    }

    [Obsolete]
    /// <summary>
    /// Retrieves a value from the dictionary by key, creating a specified default value if it does not exist.
    /// (This method is obsolete and should not be used.)
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve or create the value.</param>
    /// <param name="key">The key of the value to retrieve or create.</param>
    /// <param name="defaultValue">The default value to create if the key is not found.</param>
    /// <returns>The value associated with the key, or the newly created default value if the key was not found.</returns>
    public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
    {
        return dictionary.GetOrAdd(key, defaultValue);
    }

    /// <summary>
    /// Retrieves a value from the dictionary by key, creating the value if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve or create the value.</param>
    /// <param name="key">The key of the value to retrieve or create.</param>
    /// <param name="creation">A function that creates the value if it does not exist.</param>
    /// <returns>The value associated with the key, or the newly created value if the key was not found.</returns>
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
    /// Retrieves a value from the dictionary by key, creating the value if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve or create the value.</param>
    /// <param name="key">The key of the value to retrieve or create.</param>
    /// <param name="creation">A function that creates the value if it does not exist.</param>
    /// <param name="added">A boolean indicating whether the value was added to the dictionary.</param>
    /// <returns>The value associated with the key, or the newly created value if the key was not found.</returns>
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
    /// Retrieves a value from the dictionary by key, creating a specified default value if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve or create the value.</param>
    /// <param name="key">The key of the value to retrieve or create.</param>
    /// <param name="defaultValue">The default value to create if the key is not found.</param>
    /// <returns>The value associated with the key, or the newly created default value if the key was not found.</returns>
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
    /// Attempts to add a key-value pair to the dictionary if the key does not already exist.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the key-value pair.</param>
    /// <param name="key">The key of the key-value pair to add.</param>
    /// <param name="value">The value of the key-value pair to add.</param>
    /// <returns>True if the key-value pair was added, false if the key already exists.</returns>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to add a key-value pair to the dictionary if the key does not already exist.
    /// </summary>
    /// <param name="dictionary">The dictionary to which to add the key-value pair.</param>
    /// <param name="key">The key of the key-value pair to add.</param>
    /// <param name="factory">A function that creates the value to add if the key does not exist.</param>
    /// <returns>True if the key-value pair was added, false if the key already exists.</returns>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, factory());
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a value from the dictionary by key and returns the removed value.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the value.</param>
    /// <param name="key">The key of the value to remove.</param>
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
    /// Attempts to remove a value from the dictionary by key and returns the removed value.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the value.</param>
    /// <param name="key">The key of the value to remove.</param>
    /// <param name="value">The removed value, or the default value if the key was not found.</param>
    /// <returns>True if the value was removed, false if the key was not found.</returns>
    public static bool TryRemoveAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
    {
        if (dictionary.TryGetValue(key, out value))
        {
            dictionary.Remove(key);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the value satisfies the specified predicate.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="predicate">A predicate that determines whether a key-value pair should be removed.</param>
    public static void RemoveAllByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TValue> predicate)
    {
        List<TKey> removes = null;
        foreach (var pair in dictionary)
        {
            if (predicate(pair.Value))
            {
                (removes ??= []).Add(pair.Key);
            }
        }

        if (removes != null)
        {
            foreach (TKey key in removes)
            {
                dictionary.Remove(key);
            }
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the value satisfies the specified predicate and returns the removed values.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="predicate">A predicate that determines whether a key-value pair should be removed.</param>
    /// <returns>An enumerable containing the removed values.</returns>
    public static IEnumerable<TValue> RemoveAllByValueAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TValue> predicate)
    {
        List<KeyValuePair<TKey, TValue>> removes = null;
        foreach (var pair in dictionary)
        {
            if (predicate(pair.Value))
            {
                (removes ??= []).Add(pair);
            }
        }

        if (removes != null)
        {
            foreach (var pair in removes)
            {
                dictionary.Remove(pair.Key);
            }
            return removes.Select(o => o.Value);
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the key satisfies the specified predicate.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="predicate">A predicate that determines whether a key-value pair should be removed.</param>
    public static void RemoveAllByKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TKey> predicate)
    {
        List<TKey> removes = null;
        foreach (var pair in dictionary)
        {
            if (predicate(pair.Key))
            {
                (removes ??= []).Add(pair.Key);
            }
        }

        if (removes != null)
        {
            foreach (TKey key in removes)
            {
                dictionary.Remove(key);
            }
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the key is in the specified collection.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="keys">A collection of keys to remove from the dictionary.</param>
    public static void RemoveAllByKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
        {
            dictionary.Remove(key);
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the key satisfies the specified predicate and returns the removed values.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="predicate">A predicate that determines whether a key-value pair should be removed.</param>
    /// <returns>An enumerable containing the removed values.</returns>
    public static IEnumerable<TValue> RemoveAllByKeyAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TKey> predicate)
    {
        List<KeyValuePair<TKey, TValue>> removes = null;
        foreach (var pair in dictionary)
        {
            if (predicate(pair.Key))
            {
                (removes ??= []).Add(pair);
            }
        }

        if (removes != null)
        {
            foreach (var pair in removes)
            {
                dictionary.Remove(pair.Key);
            }
            return removes.Select(o => o.Value);
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Removes all key-value pairs from the dictionary where the key is in the specified collection and returns the removed values.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to remove the key-value pairs.</param>
    /// <param name="keys">A collection of keys to remove from the dictionary.</param>
    /// <returns>An enumerable containing the removed values.</returns>
    public static IEnumerable<TValue> RemoveAllByKeyAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
    {
        List<TValue> removes = null;
        foreach (var key in keys)
        {
            if (dictionary.TryRemoveAndGet(key, out TValue value))
            {
                (removes ??= []).Add(value);
            }
        }

        return removes ?? [];
    }

    /// <summary>
    /// Converts the dictionary to an object dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>An object dictionary containing the same key-value pairs as the original dictionary.</returns>
    public static IDictionary<TKey, object> ConvertToObjectDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
    {
        return dictionary.ToDictionary<KeyValuePair<TKey, TValue>, TKey, object>(pair => pair.Key, pair => pair.Value);
    }

    #endregion

    #region HashSet

    /// <summary>
    /// Returns a sequence that contains the elements of the source sequence except for those that are also in the specified HashSet.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence and the HashSet.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="except">The HashSet containing elements to exclude from the result.</param>
    /// <returns>A sequence that contains the elements of the source sequence except for those that are also in the specified HashSet.</returns>
    public static IEnumerable<T> ExceptHashSet<T>(this IEnumerable<T> source, HashSet<T> except)
    {
        foreach (var item in source)
        {
            if (!except.Contains(item))
            {
                yield return item;
            }
        }
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => [.. source];

    #endregion

    #region ICollection

    /// <summary>
    /// Adds the elements of the specified collection to the current collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection and the current collection.</typeparam>
    /// <param name="collection">The current collection.</param>
    /// <param name="other">The collection whose elements should be added to the current collection.</param>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> other)
    {
        if (other != null)
        {
            foreach (T item in other)
            {
                collection.Add(item);
            }
        }
    }

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate from the current collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    /// <param name="collection">The current collection.</param>
    /// <param name="predicate">The predicate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements removed from the collection.</returns>
    public static int RemoveAll<T>(this ICollection<T> collection, Predicate<T> predicate)
    {
        List<T> removes = null;

        foreach (T item in collection)
        {
            if (predicate(item))
            {
                (removes ??= []).Add(item);
            }
        }

        if (removes != null)
        {
            foreach (T item in removes)
            {
                collection.Remove(item);
            }

            return removes.Count;
        }

        return 0;
    }

    #endregion

    #region Stack

    /// <summary>
    /// Pops an element from the stack if it is not empty; otherwise, creates a new element using the specified creation function.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the stack.</typeparam>
    /// <param name="stack">The stack from which to pop an element.</param>
    /// <param name="creation">The function to create a new element if the stack is empty.</param>
    /// <returns>The popped element from the stack or the newly created element.</returns>
    public static T PopOrCreate<T>(this Stack<T> stack, Func<T> creation)
    {
        if (stack.Count > 0)
        {
            return stack.Pop();
        }
        else
        {
            return creation();
        }
    }

    #endregion

    #region IEnumerable

    /// <summary>
    /// Concatenates multiple sequences into a single sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequences.</typeparam>
    /// <param name="collection">The first sequence to concatenate.</param>
    /// <param name="others">The additional sequences to concatenate.</param>
    /// <returns>A sequence that contains the elements of the specified sequences.</returns>
    public static IEnumerable<T> ConcatMultiple<T>(this IEnumerable<T> collection, params IEnumerable<T>[] others)
    {
        foreach (T item in collection)
        {
            yield return item;
        }

        foreach (IEnumerable<T> other in others)
        {
            foreach (T item in other)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Converts the elements of the source sequence to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the elements to.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>A sequence that contains the elements of the source sequence converted to the specified type.</returns>
    public static IEnumerable<T> As<T>(this IEnumerable source) where T : class
    {
        foreach (var s in source)
        {
            yield return s as T;
        }
    }

    /// <summary>
    /// Converts the elements of the source sequence to the specified type, using a default value if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The type to convert the elements to.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="defaultValue">The default value to use if the conversion fails.</param>
    /// <returns>A sequence that contains the elements of the source sequence converted to the specified type, or the default value if the conversion fails.</returns>
    public static IEnumerable<T> SafeCast<T>(this IEnumerable source, T defaultValue = default)
    {
        foreach (var s in source)
        {
            if (s is T t)
            {
                yield return t;
            }
            else
            {
                yield return defaultValue;
            }
        }
    }

    /// <summary>
    /// Returns a sequence that contains the same elements as the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="collection">The source sequence.</param>
    /// <returns>A sequence that contains the same elements as the source sequence.</returns>
    public static IEnumerable<T> Pass<T>(this IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Returns a sequence that contains only the non-null elements of the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="collection">The source sequence.</param>
    /// <returns>A sequence that contains only the non-null elements of the source sequence.</returns>
    public static IEnumerable<T> SkipNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(x => x != null)!;
    }

    /// <summary>
    /// Returns a sequence that contains only the non-null elements of the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="collection">The source sequence.</param>
    /// <returns>A sequence that contains only the non-null elements of the source sequence.</returns>
    public static IEnumerable<T> SkipNull<T>(this IEnumerable<T?> source)
        where T : struct
    {
        return source.Where(x => x.HasValue).Select(x => x.Value);
    }

    /// <summary>
    /// Concatenates the specified sequence with a single element.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequences.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="other">The element to concatenate.</param>
    /// <returns>A sequence that contains the elements of the source sequence followed by the specified element.</returns>
    public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> source, T other)
    {
        foreach (var item in source)
        {
            yield return item;
        }

        yield return other;
    }

    /// <summary>
    /// Applies an action to each element of the sequence and returns a sequence that contains the same elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to apply to each element.</param>
    /// <returns>A sequence that contains the same elements as the source sequence.</returns>
    public static IEnumerable<T> WithAction<T>(this IEnumerable<T> source, Action<T> action) where T : class
    {
        foreach (var s in source)
        {
            action(s);
            yield return s;
        }
    }

    /// <summary>
    /// Returns a sequence that contains only the distinct elements of the source sequence based on the specified key selector.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key to select for comparison.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>A sequence that contains the distinct elements of the source sequence based on the specified key selector.</returns>
    public static IEnumerable<T> DistinctBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

        var seenKeys = new HashSet<TKey>();
        var result = new List<T>();

        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                result.Add(element);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether all elements in the sequence are equal.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="emptyValue">A value to return if the sequence is empty.</param>
    /// <returns>true if all elements in the sequence are equal; otherwise, false.</returns>
    public static bool AllEqual<TSource>(this IEnumerable<TSource> source, bool emptyValue = false)
    {
        if (!source.Any())
        {
            return emptyValue;
        }

        if (source.CountOne())
        {
            return true;
        }

        TSource first = source.First();
        return source.Skip(1).All(o => Equals(o, first));
    }

    /// <summary>
    /// Determines whether all elements in the sequence reference equal each other.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="emptyValue">A value to return if the sequence is empty.</param>
    /// <returns>true if all elements in the sequence reference equal each other; otherwise, false.</returns>
    public static bool AllReferenceEqual<TSource>(this IEnumerable<TSource> source, bool emptyValue = false)
    {
        if (!source.Any())
        {
            return emptyValue;
        }

        if (source.CountOne())
        {
            return true;
        }

        TSource first = source.First();
        return source.Skip(1).All(o => ReferenceEquals(o, first));
    }

    /// <summary>
    /// Executes the specified action on each element of the sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to execute on each element.</param>
    public static void Foreach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Determines whether the sequence contains exactly one element.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <returns>true if the sequence contains exactly one element; otherwise, false.</returns>
    public static bool CountOne(this IEnumerable source)
    {
        if (source is null)
        {
            return false;
        }

        int num = 0;

        foreach (var item in source)
        {
            num++;

            if (num > 1)
            {
                return false;
            }
        }

        return num == 1;
    }

    /// <summary>
    /// Returns the only element of the sequence, or the default value if the sequence is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The only element of the sequence, or the default value if the sequence is empty or contains more than one element.</returns>
    public static T OneOrDefault<T>(this IEnumerable<T> source)
    {
        if (source is null)
        {
            return default;
        }

        int num = 0;
        T result = default;

        foreach (var item in source)
        {
            num++;
            result = item;

            if (num > 1)
            {
                return default;
            }
        }

        return num == 1 ? result : default;
    }

    /// <summary>
    /// Determines whether the sequence contains more than one element.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <returns>true if the sequence contains more than one element; otherwise, false.</returns>
    public static bool CountMoreThanOne(this IEnumerable source)
    {
        if (source is null)
        {
            return false;
        }

        int num = 0;
        foreach (var item in source)
        {
            num++;
            if (num > 1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the common type of the elements in the sequence, or null if the sequence is empty.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <returns>The common type of the elements in the sequence, or null if the sequence is empty.</returns>
    public static Type GetCommonType(this IEnumerable<Type> source)
    {
        if (!source.Any())
        {
            return null;
        }

        Type commonType = source.OfType<Type>().FirstOrDefault();
        if (commonType is null)
        {
            return null;
        }

        foreach (Type o in source.Skip(1))
        {
            Type curType = o;
            while (commonType != curType && !commonType.IsAssignableFrom(curType))
            {
                commonType = commonType.BaseType;
            }
        }

        return commonType;
    }

    /// <summary>
    /// Returns the common type of the elements in the sequence, or null if the sequence is empty.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <returns>The common type of the elements in the sequence, or null if the sequence is empty.</returns>
    public static Type GetCommonType(this IEnumerable<object> source)
    {
        if (!source.Any())
        {
            return null;
        }

        Type commonType = source.OfType<object>().FirstOrDefault()?.GetType();
        if (commonType is null)
        {
            return null;
        }

        foreach (object o in source.Skip(1))
        {
            Type curType = o.GetType();
            while (commonType != curType && !commonType.IsAssignableFrom(curType))
            {
                commonType = commonType.BaseType;
            }
        }

        return commonType;
    }

    /// <summary>
    /// Returns the only element of the collection, or the default value if the collection is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <returns>The only element of the collection, or the default value if the collection is empty or contains more than one element.</returns>
    public static T OnlyOneOfDefault<T>(this IEnumerable<T> collection)
    {
        bool getValue = false;
        T value = default;

        foreach (var item in collection)
        {
            if (getValue)
            {
                value = default;
                break;
            }
            else
            {
                getValue = true;
                value = item;
            }
        }

        return value;
    }

    #endregion

    #region RangeCollection

    /// <summary>
    /// Gets the item at the specified index from the collection, or the default value if the index is out of range or the group is null.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the default value if the index is out of range or the group is null.</returns>
    public static T GetItemSafe<T>(this RangeCollection<T> collection, int index)
    {
        if (index >= 0 && index < collection.Count)
        {
            var group = collection[index];
            if (group != null)
            {
                return group.Value;
            }
            else
            {
                return default;
            }
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Gets the item at the specified index from the collection, or the default value if the index is out of range or the group is null.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the default value if the index is out of range or the group is null.</returns>
    public static T GetItemMinMax<T>(this RangeCollection<T> collection, int index)
    {
        if (collection.Count == 0)
        {
            return default;
        }

        if (index < 0)
        {
            index = 0;
        }
        else if (index >= collection.Count)
        {
            index = collection.Count - 1;
        }

        var group = collection[index];
        if (group != null)
        {
            return group.Value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Finds the index of the group that contains the specified number.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="number">The number to find.</param>
    /// <returns>The index of the group that contains the specified number, or -1 if the number is not found.</returns>
    public static int FindIndexMinMax<T>(this RangeCollection<T> collection, int number)
    {
        if (collection.Count == 0)
        {
            return -1;
        }

        if (number <= collection[0].High)
        {
            return 0;
        }

        if (number >= collection[collection.Count - 1].Low)
        {
            return collection.Count - 1;
        }

        return collection.FindIndex(number);
    }

    #endregion
}