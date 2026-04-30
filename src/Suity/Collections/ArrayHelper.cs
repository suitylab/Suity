using System;

namespace Suity.Collections;

/// <summary>
/// Provides helper methods for working with arrays.
/// </summary>
public static class ArrayHelper
{
    /// <summary>
    /// Determines whether two arrays are equal.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the arrays.</typeparam>
    /// <param name="aryA">The first array to compare.</param>
    /// <param name="aryB">The second array to compare.</param>
    /// <returns>true if the arrays are equal; otherwise, false.</returns>
    public static bool ArrayEquals<T>(T[] aryA, T[] aryB)
    {
        if (aryA is null && aryB is null)
        {
            return true;
        }

        if (aryA is null || aryB is null)
        {
            return false;
        }

        if (aryA.Length != aryB.Length)
        {
            return false;
        }

        for (int i = 0; i < aryA.Length; i++)
        {
            if (!Equals(aryA[i], aryB[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether two arrays are equal within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the arrays.</typeparam>
    /// <param name="aryA">The first array to compare.</param>
    /// <param name="aryB">The second array to compare.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="len">The length of the range.</param>
    /// <returns>true if the arrays are equal within the specified range; otherwise, false.</returns>
    public static bool ArrayEquals<T>(T[] aryA, T[] aryB, int start, int len)
    {
        if (aryA is null && aryB is null)
        {
            return true;
        }

        if (aryA is null || aryB is null)
        {
            return false;
        }

        if (aryA.Length < len || aryB.Length < len)
        {
            return false;
        }

        for (int i = start; i < len; i++)
        {
            if (!Object.Equals(aryA[i], aryB[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the item at the specified index in the array, or the first or last item if the index is out of range.
    /// </summary>
    /// <param name="ary">The array to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or the first or last item if the index is out of range.</returns>
    public static object GetArrayItemMinMax(this Array ary, int index)
    {
        if (ary is null || ary.Length is 0)
        {
            return null;
        }

        if (index < 0)
        {
            return ary.GetValue(0);
        }
        else if (index >= ary.Length)
        {
            return ary.GetValue(ary.Length - 1);
        }
        else
        {
            return ary.GetValue(index);
        }
    }

    /// <summary>
    /// Gets the item at the specified index in the array, or null if the index is out of range.
    /// </summary>
    /// <param name="ary">The array to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, or null if the index is out of range.</returns>
    public static object GetArrayItemSafe(this Array ary, int index)
    {
        if (ary is null || ary.Length is 0)
        {
            return null;
        }

        if (index < 0 || index >= ary.Length)
        {
            return null;
        }
        else
        {
            return ary.GetValue(index);
        }
    }
}
