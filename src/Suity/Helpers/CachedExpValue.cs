using System;
using System.Collections.Generic;

namespace Suity.Helpers;

/// <summary>
/// Provides cached exponential value calculation based on level.
/// </summary>
public class CachedExpValue
{
    double _baseValue;

    readonly List<double> _items = [];

    public CachedExpValue(double baseValue)
    {
        _baseValue = baseValue;
    }

    public double GetValue(int level)
    {
        int index = level - 1;
        if (index < 0)
        {
            return 0;
        }

        while (_items.Count <= index)
        {
            double v = Math.Pow(_baseValue, _items.Count);
            _items.Add(v);
        }

        return _items[index];
    }
}