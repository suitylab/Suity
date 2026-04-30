using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Collections;

/// <summary>
/// Represents a range group with a value and a low and high boundary.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class RangeGroup<T>(T value, int low, int high)
{
    /// <summary>
    /// Gets or sets the value of the range group.
    /// </summary>
    public T Value { get; set; } = value;

    /// <summary>
    /// Gets the low boundary of the range group.
    /// </summary>
    public int Low { get; } = low;

    /// <summary>
    /// Gets the high boundary of the range group.
    /// </summary>
    public int High { get; } = high;

    /// <summary>
    /// Gets the low boundary of the range group (deprecated).
    /// </summary>
    [Obsolete("Use Low instead.")]
    public int LowValue => Low;

    /// <summary>
    /// Gets the high boundary of the range group (deprecated).
    /// </summary>
    [Obsolete("Use High instead.")]
    public int HighValue => High;

    /// <summary>
    /// Compares the value with the given number and returns -1, 0, or 1.
    /// </summary>
    /// <param name="value">The number to compare with.</param>
    /// <returns>-1 if the number is less than the low boundary, 0 if it is within the range, and 1 if it is greater than the high boundary.</returns>
    public int CompareTo(int value)
    {
        if (value < Low)
        {
            return 1;
        }

        if (value > High)
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// Returns a string representation of the range group.
    /// </summary>
    /// <returns>A string in the format "[Low-High] Value".</returns>
    public override string ToString() => $"[{Low}-{High}] {Value}";
}

/// <summary>
/// Represents a collection of range groups.
/// </summary>
/// <typeparam name="T">The type of the value in the range groups.</typeparam>
public class RangeCollection<T>
{
    /// <summary>
    /// The list of range groups.
    /// </summary>
    private readonly List<RangeGroup<T>> _groups = [];

    /// <summary>
    /// Appends a new range group to the end of the collection.
    /// </summary>
    /// <param name="value">The value of the new range group.</param>
    /// <param name="length">The length of the new range group.</param>
    public void Append(T value, int length)
    {
        if (_groups.Count > 0)
        {
            var last = _groups[_groups.Count - 1];
            int low = last.High + 1;
            int high = last.High + length;
            var group = new RangeGroup<T>(value, low, high);
            _groups.Add(group);
        }
        else
        {
            _groups.Add(new RangeGroup<T>(value, 0, length - 1));
        }
    }

    /// <summary>
    /// Prepends a new range group to the beginning of the collection.
    /// </summary>
    /// <param name="value">The value of the new range group.</param>
    /// <param name="length">The length of the new range group.</param>
    public void Prepend(T value, int length)
    {
        if (_groups.Count > 0)
        {
            var last = _groups[_groups.Count - 1];
            int high = last.Low - 1;
            int low = last.Low - length;
            var group = new RangeGroup<T>(value, low, high);
            _groups.Add(group);
        }
        else
        {
            _groups.Add(new RangeGroup<T>(value, -1, -length));
        }
    }

    /// <summary>
    /// Adds a new range group by the high boundary.
    /// </summary>
    /// <param name="highValue">The high boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    public void AddByHigh(int highValue, T value)
    {
        InternalAddByHigh(highValue, value, false);
    }

    /// <summary>
    /// Adds a new range group by the low boundary.
    /// </summary>
    /// <param name="lowValue">The low boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    public void AddByLow(int lowValue, T value)
    {
        InternalAddByLow(lowValue, value, false);
    }

    /// <summary>
    /// Adds a new range group by the high boundary (deprecated).
    /// </summary>
    /// <param name="highValue">The high boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    [Obsolete]
    public void AddByHighValue(int highValue, T value)
    {
        InternalAddByHigh(highValue, value, false);
    }

    /// <summary>
    /// Adds a new range group by the low boundary (deprecated).
    /// </summary>
    /// <param name="lowValue">The low boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    [Obsolete]
    public void AddByLowValue(int lowValue, T value)
    {
        InternalAddByLow(lowValue, value, false);
    }

    /// <summary>
    /// Gets or adds a new range group by the high boundary.
    /// </summary>
    /// <param name="high">The high boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    public RangeGroup<T> GetOrAddByHigh(int high, T value)
    {
        return InternalAddByHigh(high, value, true);
    }

    /// <summary>
    /// Gets or adds a new range group by the low boundary.
    /// </summary>
    /// <param name="low">The low boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    public RangeGroup<T> GetOrAddByLow(int low, T value)
    {
        return InternalAddByLow(low, value, true);
    }

    /// <summary>
    /// Gets or adds a new range group by the high boundary (deprecated).
    /// </summary>
    /// <param name="highValue">The high boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    [Obsolete]
    public RangeGroup<T> GetOrAddByHighValue(int highValue, T value)
    {
        return InternalAddByHigh(highValue, value, true);
    }

    /// <summary>
    /// Gets or adds a new range group by the low boundary (deprecated).
    /// </summary>
    /// <param name="lowValue">The low boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    [Obsolete]
    public RangeGroup<T> GetOrAddByLowValue(int lowValue, T value)
    {
        return InternalAddByLow(lowValue, value, true);
    }

    /// <summary>
    /// Adds or retrieves a new range group by the high boundary.
    /// </summary>
    /// <param name="high">The high boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <param name="replace">Whether to replace an existing range group with the same high boundary.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    private RangeGroup<T> InternalAddByHigh(int high, T value, bool replace)
    {
        int index = FindIndex(high);
        if (index >= 0)
        {
            RangeGroup<T> current = _groups[index];
            if (current.High == high)
            {
                if (replace)
                {
                    current.Value = value;
                    return current;
                }
                else
                {
                    throw new InvalidOperationException("High value exist : " + high);
                }
            }

            var higherGroup = new RangeGroup<T>(current.Value, high + 1, current.High);
            var lowerGroup = new RangeGroup<T>(value, current.Low, high);
            _groups[index] = higherGroup;
            _groups.Insert(index, lowerGroup);

            return lowerGroup;
        }
        else
        {
            if (_groups.Count == 0)
            {
                var newGroup = new RangeGroup<T>(value, int.MinValue, high);
                _groups.Add(newGroup);

                return newGroup;
            }
            else
            {
                var last = _groups[_groups.Count - 1];
                var newGroup = new RangeGroup<T>(value, last.High + 1, high);
                _groups.Add(newGroup);

                return newGroup;
            }
        }
    }

    /// <summary>
    /// Adds or retrieves a new range group by the low boundary.
    /// </summary>
    /// <param name="low">The low boundary of the new range group.</param>
    /// <param name="value">The value of the new range group.</param>
    /// <param name="replace">Whether to replace an existing range group with the same low boundary.</param>
    /// <returns>The range group that was added or retrieved.</returns>
    private RangeGroup<T> InternalAddByLow(int low, T value, bool replace)
    {
        int index = FindIndex(low);
        if (index >= 0)
        {
            var current = _groups[index];
            if (current.Low == low)
            {
                if (replace)
                {
                    current.Value = value;
                    return current;
                }
                else
                {
                    throw new InvalidOperationException("Low value exist : " + low);
                }
            }

            var higherGroup = new RangeGroup<T>(value, low, current.High);
            var lowerGroup = new RangeGroup<T>(current.Value, current.Low, low - 1);

            _groups[index] = higherGroup;
            _groups.Insert(index, lowerGroup);

            return lowerGroup;
        }
        else
        {
            if (_groups.Count == 0)
            {
                var newGroup = new RangeGroup<T>(value, low, int.MaxValue);
                _groups.Add(newGroup);

                return newGroup;
            }
            else
            {
                var first = _groups[0];
                var newGroup = new RangeGroup<T>(value, low, first.Low - 1);
                _groups.Insert(0, newGroup);

                return newGroup;
            }
        }
    }

    /// <summary>
    /// Finds the value at the given position.
    /// </summary>
    /// <param name="position">The position to find the value at.</param>
    /// <returns>The value at the given position, or the default value if the position is out of range.</returns>
    public T FindValue(int position)
    {
        int index = FindIndex(position);
        if (index >= 0)
        {
            return _groups[index].Value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Finds the range group at the given position.
    /// </summary>
    /// <param name="position">The position to find the range group at.</param>
    /// <returns>The range group at the given position, or null if the position is out of range.</returns>
    public RangeGroup<T> FindRangeGroup(int position)
    {
        int index = FindIndex(position);
        if (index >= 0)
        {
            return _groups[index];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Finds the index of the range group that contains the given number.
    /// </summary>
    /// <param name="number">The number to find the index of.</param>
    /// <returns>The index of the range group that contains the number, or -1 if no such range group exists.</returns>
    public int FindIndex(int number)
    {
        int min = 0;
        int max = _groups.Count - 1;

        while (min <= max)
        {
            int mid = (min + max) / 2;
            int comparison = _groups[mid].CompareTo(number);
            if (comparison == 0)
            {
                return mid;
            }

            if (comparison < 0)
            {
                min = mid + 1;
            }
            else
            {
                max = mid - 1;
            }
        }
        return -1;
    }

    /// <summary>
    /// Finds the index of the range group that contains the given number, or the nearest range group if no exact match is found.
    /// </summary>
    /// <param name="number">The number to find the index of.</param>
    /// <returns>The index of the range group that contains the number, or the index of the nearest range group if no exact match is found.</returns>
    public int FindIndexMinMax(int number)
    {
        if (_groups.Count == 0)
        {
            return -1;
        }

        var first = _groups[0];
        if (number < first.Low)
        {
            return 0;
        }

        var last = _groups[_groups.Count - 1];
        if (number > last.High)
        {
            return _groups.Count - 1;
        }

        return FindIndex(number);
    }

    /// <summary>
    /// Gets an enumerable collection of all range groups.
    /// </summary>
    public IEnumerable<RangeGroup<T>> RangeGroups => _groups.Select(o => o);

    /// <summary>
    /// Gets an enumerable collection of all values in the range groups.
    /// </summary>
    public IEnumerable<T> Values => _groups.Select(o => o.Value);

    /// <summary>
    /// Clears all range groups from the collection.
    /// </summary>
    public void Clear() => _groups.Clear();

    /// <summary>
    /// Gets the number of range groups in the collection.
    /// </summary>
    public int Count => _groups.Count;

    /// <summary>
    /// Gets the total length of all range groups in the collection.
    /// </summary>
    public int TotalLength
    {
        get
        {
            if (_groups.Count > 0)
            {
                return _groups[_groups.Count - 1].High - _groups[0].Low + 1;
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Gets or sets the range group at the specified index.
    /// </summary>
    /// <param name="index">The index of the range group to get or set.</param>
    /// <returns>The range group at the specified index.</returns>
    public RangeGroup<T> this[int index] => _groups[index];

    /// <summary>
    /// Finds the index of the specified range group.
    /// </summary>
    /// <param name="group">The range group to find the index of.</param>
    /// <returns>The index of the specified range group, or -1 if the range group is not found.</returns>
    public int IndexOf(RangeGroup<T> group) => _groups.IndexOf(group);
}
