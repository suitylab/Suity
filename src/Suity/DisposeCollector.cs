using System;
using System.Collections.Generic;

namespace Suity;

/// <summary>
/// Collects objects for batch disposal.
/// Provides operator overloads for convenient object collection.
/// </summary>
public class DisposeCollector : IDisposable
{
    private readonly List<object> _list = [];

    public void Dispose()
    {
        var ary = _list.ToArray();
        _list.Clear();

        foreach (var obj in ary)
        {
            if (obj is Suity.Object o)
            {
                try
                {
                    Suity.Object.DestroyObject(o);
                }
                catch (Exception err)
                {
                    Logs.LogError(err);
                }
            }
            else if (obj is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception err)
                {
                    Logs.LogError(err);
                }
            }
        }
    }

    /// <summary>
    /// Adds an object to the collection.
    /// </summary>
    /// <param name="s">The DisposeCollector instance.</param>
    /// <param name="d">The object to add.</param>
    /// <returns>The updated DisposeCollector.</returns>
    public static DisposeCollector operator +(DisposeCollector s, object d)
    {
        if (d != null)
        {
            s ??= new DisposeCollector();
            s._list.Add(d);
        }

        return s;
    }

    /// <summary>
    /// Adds multiple objects to the collection.
    /// </summary>
    /// <param name="s">The DisposeCollector instance.</param>
    /// <param name="d">The objects to add.</param>
    /// <returns>The updated DisposeCollector.</returns>
    public static DisposeCollector operator +(DisposeCollector s, IEnumerable<object> d)
    {
        if (d != null)
        {
            s ??= new DisposeCollector();

            foreach (var item in d)
            {
                if (item != null)
                {
                    s._list.Add(item);
                }
            }
        }

        return s;
    }

    /// <summary>
    /// Removes an object from the collection.
    /// </summary>
    /// <param name="s">The DisposeCollector instance.</param>
    /// <param name="d">The object to remove.</param>
    /// <returns>The updated DisposeCollector, or null if empty.</returns>
    public static DisposeCollector operator -(DisposeCollector s, object d)
    {
        if (d != null)
        {
            s._list.Remove(d);
        }

        if (s._list.Count == 0)
        {
            return null;
        }

        return s;
    }

    /// <summary>
    /// Removes multiple objects from the collection.
    /// </summary>
    /// <param name="s">The DisposeCollector instance.</param>
    /// <param name="d">The objects to remove.</param>
    /// <returns>The updated DisposeCollector, or null if empty.</returns>
    public static DisposeCollector operator -(DisposeCollector s, IEnumerable<object> d)
    {
        if (d != null)
        {
            foreach (var item in d)
            {
                s._list.Remove(item);
            }
        }

        if (s._list.Count == 0)
        {
            return null;
        }

        return s;
    }
}