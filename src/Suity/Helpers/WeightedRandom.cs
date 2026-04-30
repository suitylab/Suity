using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Helpers;

/// <summary>
/// Provides weighted random selection from a collection.
/// </summary>
/// <typeparam name="T">The type of elements.</typeparam>
public class WeightedRandom<T> where T : class
{
    private class WeightedItem
    {
        public T Value;
        public double Weight;
    }

    private readonly List<WeightedItem> _items = [];
    private double _totalWeight;

    public bool AddValue(T value, double weight)
    {
        if (value is null)
        {
            return false;
        }
        if (weight <= 0)
        {
            return false;
        }

        _items.Add(new WeightedItem { Value = value, Weight = weight });

        _totalWeight += weight;

        return true;
    }

    public T GetRandomValue(Random rnd)
    {
        if (_items.Count == 0)
        {
            return default;
        }

        double v = rnd.NextDouble() * _totalWeight;

        for (int i = 0; i < _items.Count - 1; i++)
        {
            var item = _items[i];
            if (v < item.Weight)
            {
                return item.Value;
            }

            v -= item.Weight;
        }

        return _items[_items.Count - 1].Value;
    }

    public void Clear()
    {
        _items.Clear();
        _totalWeight = 0;
    }

    public T this[int index]
    {
        get => _items.GetListItemSafe(index)?.Value ?? default;
    }

    public IEnumerable<T> AllItems => _items.Select(o => o.Value);
}
