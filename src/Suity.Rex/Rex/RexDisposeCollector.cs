using System;
using System.Collections.Generic;

namespace Suity.Rex;

/// <summary>
/// Collects disposable objects and disposes them all when this collector is disposed.
/// Supports adding and removing items via + and - operators.
/// </summary>
public class RexDisposeCollector : IDisposable
{
    private readonly List<object> _list = [];

    /// <inheritdoc/>
    public void Dispose()
    {
        var ary = _list.ToArray();
        _list.Clear();

        foreach (var obj in ary)
        {
            if (obj is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception err)
                {
                    RexGlobalResolve.Current?.LogException(err);
                }
            }
        }
    }

    /// <summary>
    /// Adds an object to the collector. Creates a new collector if the existing one is null.
    /// </summary>
    /// <param name="s">The collector to add to.</param>
    /// <param name="d">The object to add.</param>
    /// <returns>The collector instance with the object added.</returns>
    public static RexDisposeCollector operator +(RexDisposeCollector s, object d)
    {
        if (d != null)
        {
            s ??= new RexDisposeCollector();
            s._list.Add(d);
        }

        return s;
    }

    /// <summary>
    /// Adds a collection of objects to the collector. Creates a new collector if the existing one is null.
    /// </summary>
    /// <param name="s">The collector to add to.</param>
    /// <param name="d">The collection of objects to add.</param>
    /// <returns>The collector instance with the objects added.</returns>
    public static RexDisposeCollector operator +(RexDisposeCollector s, IEnumerable<object> d)
    {
        if (d != null)
        {
            s ??= new RexDisposeCollector();

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
    /// Removes an object from the collector. Returns null if the collector becomes empty.
    /// </summary>
    /// <param name="s">The collector to remove from.</param>
    /// <param name="d">The object to remove.</param>
    /// <returns>The collector instance, or null if empty.</returns>
    public static RexDisposeCollector operator -(RexDisposeCollector s, object d)
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
    /// Removes a collection of objects from the collector. Returns null if the collector becomes empty.
    /// </summary>
    /// <param name="s">The collector to remove from.</param>
    /// <param name="d">The collection of objects to remove.</param>
    /// <returns>The collector instance, or null if empty.</returns>
    public static RexDisposeCollector operator -(RexDisposeCollector s, IEnumerable<object> d)
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
