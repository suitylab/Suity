using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Collections;

/// <summary>
/// Represents a circular array with a fixed size.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public class CappedArray<T> : IEnumerable<T>
{
    private readonly T[] _array;

    private int _cursorIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="CappedArray{T}"/> class with the specified length.
    /// </summary>
    /// <param name="length">The length of the array.</param>
    /// <exception cref="ArgumentException">Thrown when the length is less than 1.</exception>
    public CappedArray(int length)
    {
        if (length < 1)
        {
            throw new ArgumentException("length < 1", nameof(length));
        }
        _array = new T[length];
    }

    /// <summary>
    /// Adds a value to the front of the array, replacing the last value and moving the cursor backward.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public void AddFirst(T value)
    {
        MoveCursorBackward();
        _array[_cursorIndex] = value;
    }

    /// <summary>
    /// Adds a value to the end of the array, replacing the first value and moving the cursor forward.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public void AddLast(T value)
    {
        _array[_cursorIndex] = value;
        MoveCursorForward();
    }

    /// <summary>
    /// Moves the cursor forward by one position.
    /// </summary>
    public void MoveCursorForward()
    {
        _cursorIndex++;
        if (_cursorIndex >= _array.Length)
        {
            _cursorIndex = 0;
        }
    }

    /// <summary>
    /// Moves the cursor backward by one position.
    /// </summary>
    public void MoveCursorBackward()
    {
        _cursorIndex--;
        if (_cursorIndex < 0)
        {
            _cursorIndex = _array.Length - 1;
        }
    }

    /// <summary>
    /// Gets or sets the value at the cursor position.
    /// </summary>
    public T FirstValue
    {
        get => _array[_cursorIndex];
        set => _array[_cursorIndex] = value;
    }

    /// <summary>
    /// Gets or sets the value at the last position.
    /// </summary>
    public T LastValue
    {
        get => _array[GetRealIndex(_array.Length - 1)];
        set => _array[GetRealIndex(_array.Length - 1)] = value;
    }

    /// <summary>
    /// Gets or sets the value at the specified index.
    /// </summary>
    /// <param name="index">The index of the value to get or set.</param>
    /// <returns>The value at the specified index.</returns>
    public T this[int index]
    {
        get => _array[GetRealIndex(index)];
        set => _array[GetRealIndex(index)] = value;
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length => _array.Length;

    /// <summary>
    /// Gets the raw values of the array.
    /// </summary>
    public IEnumerable<T> RawValues => _array;

    /// <summary>
    /// Gets the real index of the specified index in the array.
    /// </summary>
    /// <param name="index">The index to get the real index of.</param>
    /// <returns>The real index of the specified index.</returns>
    private int GetRealIndex(int index)
    {
        int i = index + _cursorIndex;
        if (i >= _array.Length)
        {
            i -= _array.Length;
        }

        return i;
    }

    /// <summary>
    /// Clears the array.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_array, 0, _array.Length);
    }

    /// <summary>
    /// Gets an enumerator for the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    private IEnumerable<T> GetEnumerable()
    {
        for (int i = 0; i < _array.Length; i++)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Gets an enumerator for the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return GetEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Gets an enumerator for the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerable().GetEnumerator();
    }
}
