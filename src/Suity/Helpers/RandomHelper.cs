using System;
using System.Collections.Generic;

namespace Suity.Helpers;

/// <summary>
/// Provides random number generation helper methods.
/// </summary>
public static class RandomHelper
{
    public static int Range(this Random rnd, int min, int max)
    {
        return rnd.Next(min, max);
    }

    public static float Range(this Random rnd, float min, float max)
    {
        return min + (float)(rnd.NextDouble() * (max - min));
    }

    public static double Range(this Random rnd, double min, double max)
    {
        return min + rnd.NextDouble() * (max - min);
    }

    /// <summary>
    /// Shuffle the elements in a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this List<T> list, SeededRandom rnd)
    {
        int len = list.Count;

        for (int i = 0; i < len; i++)
        {
            T a = list[i];
            int rndIndex = rnd.Range(0, len);
            T b = list[rndIndex];
            // Swap position
            list[rndIndex] = a;
            list[i] = b;
        }
    }

    /// <summary>
    /// Shuffle the elements in a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this List<T> list, Random rnd)
    {
        int len = list.Count;

        for (int i = 0; i < len; i++)
        {
            T a = list[i];
            int rndIndex = rnd.Range(0, len);
            T b = list[rndIndex];
            // Swap position
            list[rndIndex] = a;
            list[i] = b;
        }
    }

    /// <summary>
    /// Shuffle the elements in a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this T[] list, SeededRandom rnd)
    {
        int len = list.Length;

        for (int i = 0; i < len; i++)
        {
            T a = list[i];
            int rndIndex = rnd.Range(0, len);
            T b = list[rndIndex];
            // Swap position
            list[rndIndex] = a;
            list[i] = b;
        }
    }

    /// <summary>
    /// Shuffle the elements in a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this T[] list, Random rnd)
    {
        int len = list.Length;

        for (int i = 0; i < len; i++)
        {
            T a = list[i];
            int rndIndex = rnd.Range(0, len);
            T b = list[rndIndex];
            // Swap position
            list[rndIndex] = a;
            list[i] = b;
        }
    }
}