using System.Collections;

namespace Suity.Collections;

public static class IListExtensions
{
    /// <summary>
    /// Safely retrieves an item from the list at the specified index.
    /// </summary>
    /// <param name="list">The list to retrieve the item from.</param>
    /// <param name="index">The index of the item to retrieve.</param>
    /// <returns>The item at the specified index, or null if the index is out of range or the list is null or empty.</returns>
    public static object GetIListItemSafe(this IList list, int index)
    {
        if (list == null || list.Count == 0)
        {
            return null;
        }

        if (index < 0 || index >= list.Count)
        {
            return null;
        }
        else
        {
            return list[index];
        }
    }

    /// <summary>
    /// Ensures that the list has at least the specified size by adding null elements if necessary.
    /// </summary>
    /// <param name="list">The list to ensure the size of.</param>
    /// <param name="size">The minimum size the list should have.</param>
    public static void EnsureIListSize(this IList list, int size)
    {
        while (list.Count < size)
        {
            list.Add(null);
        }
    }

    /// <summary>
    /// Retrieves an item from the list at the specified index, returning the first or last item if the index is out of range.
    /// </summary>
    /// <param name="list">The list to retrieve the item from.</param>
    /// <param name="index">The index of the item to retrieve.</param>
    /// <returns>The item at the specified index, the first item if the index is negative, or the last item if the index is greater than or equal to the list count.</returns>
    public static object GetIListItemMinMax(this IList list, int index)
    {
        if (list == null || list.Count == 0)
        {
            return null;
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
}
