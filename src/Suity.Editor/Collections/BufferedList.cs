using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Collections;

/// <summary>
/// Adaptive Double Buffering List with COW (Copy-On-Write) Fallback.
/// Designed for "Read-Heavy, Write-Light" scenarios where read latency is critical.
/// Implements both generic (IList<T>) and non-generic (IList, ICollection) interfaces.
/// </summary>
public class BufferedList<T> : IList<T>, IReadOnlyList<T>, IList
{
    private class Buffer
    {
        public readonly List<T> List;
        // Tracks how many enumerators are currently iterating over this buffer
        public int ActiveReaders;

        public Buffer(int capacity)
        {
            List = new List<T>(capacity);
        }
    }

    // Volatile ensures that read threads always see the most recent buffer reference
    private volatile Buffer _readBuffer;

    // The write buffer, only accessed within the write lock
    private Buffer _writeBuffer;

    // Mutex for write operations to ensure atomicity during buffer switching
    private readonly object _writeLock = new();

    public BufferedList(int capacity = 16)
    {
        _readBuffer = new Buffer(capacity);
        _writeBuffer = new Buffer(capacity);
    }

    #region Core Read/Write Logic

    /// <summary>
    /// Core method to apply any write action.
    /// Handles buffer reuse, allocation, data copying, and pointer swapping.
    /// </summary>
    public void ApplyWriteAction(Action<List<T>> writeAction)
    {
        lock (_writeLock)
        {
            Buffer targetBuffer;

            // Core Logic: Check if the standby buffer (_writeBuffer) is occupied by readers
            if (Volatile.Read(ref _writeBuffer.ActiveReaders) == 0)
            {
                // Perfect Case: No readers are using it. Reuse it directly (Zero GC).
                targetBuffer = _writeBuffer;
                targetBuffer.List.Clear();
            }
            else
            {
                // Conflict Case: A slow reader is still iterating over _writeBuffer.
                // Strategy: Abandon the occupied buffer (let GC reclaim it later) 
                // and create a new Buffer to take its place.
                targetBuffer = new Buffer(_readBuffer.List.Capacity);
            }

            // Perform Copy-On-Write: Copy current data to the target buffer
            targetBuffer.List.AddRange(_readBuffer.List);

            // Execute the specific write operation (Add, Remove, Insert, etc.)
            writeAction(targetBuffer.List);

            // Atomic Pointer Swap:
            _writeBuffer = _readBuffer;
            _readBuffer = targetBuffer;
        }
    }

    #endregion

    #region Generic Read Operations (Lock-Free)

    public T this[int index]
    {
        get => _readBuffer.List[index];
        set => ApplyWriteAction(list => list[index] = value);
    }

    public int Count => _readBuffer.List.Count;
    public bool IsReadOnly => false;

    public bool Contains(T item) => _readBuffer.List.Contains(item);
    public int IndexOf(T item) => _readBuffer.List.IndexOf(item);

    public void CopyTo(T[] array, int arrayIndex) => _readBuffer.List.CopyTo(array, arrayIndex);

    #endregion

    #region Generic Write Operations (Lock-Based)

    public void Add(T item) => ApplyWriteAction(list => list.Add(item));
    public void Clear() => ApplyWriteAction(list => list.Clear());

    public bool Remove(T item)
    {
        bool removed = false;
        ApplyWriteAction(list => removed = list.Remove(item));
        return removed;
    }

    public void RemoveAt(int index) => ApplyWriteAction(list => list.RemoveAt(index));
    public void Insert(int index, T item) => ApplyWriteAction(list => list.Insert(index, item));

    #endregion

    #region Generic Enumerator (Zero Allocation for foreach)

    public IEnumerator<T> GetEnumerator()
    {
        var currentBuffer = _readBuffer;
        Interlocked.Increment(ref currentBuffer.ActiveReaders);
        return new EnumeratorWrapper(currentBuffer);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    // Non-generic IEnumerable.GetEnumerator() implementation
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Custom Struct Enumerator.
    /// Note: Native generic foreach uses Duck Typing (Zero GC).
    /// When cast to non-generic IEnumerator, it will be boxed. 
    /// IMPORTANT: Non-generic foreach does NOT automatically call Dispose. 
    /// See remarks in the non-generic interface region regarding ActiveReaders leakage.
    /// </summary>
    private struct EnumeratorWrapper : IEnumerator<T>
    {
        private readonly Buffer _buffer;
        private List<T>.Enumerator _inner;
        private bool _disposed;

        public EnumeratorWrapper(Buffer buffer)
        {
            _buffer = buffer;
            _inner = buffer.List.GetEnumerator();
            _disposed = false;
        }

        public T Current => _inner.Current;
        object IEnumerator.Current => Current;
        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => ((IEnumerator)_inner).Reset();

        public void Dispose()
        {
            if (!_disposed)
            {
                _inner.Dispose();
                Interlocked.Decrement(ref _buffer.ActiveReaders);
                _disposed = true;
            }
        }
    }

    #endregion

    #region Non-Generic Interfaces (IList, ICollection)

    // --- ICollection Properties ---
    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => _writeLock;

    // --- IList Properties ---
    bool IList.IsFixedSize => false;

    // --- IList Indexer ---
    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = CastValue(value);
    }

    // --- IList Methods ---
    int IList.Add(object value)
    {
        Add(CastValue(value));
        return Count - 1;
    }

    bool IList.Contains(object value)
    {
        return value is T tValue && Contains(tValue);
    }

    int IList.IndexOf(object value)
    {
        return value is T tValue ? IndexOf(tValue) : -1;
    }

    void IList.Insert(int index, object value) => Insert(index, CastValue(value));

    void IList.Remove(object value)
    {
        if (value is T tValue)
        {
            Remove(tValue);
        }
    }

    // --- ICollection.CopyTo ---
    void ICollection.CopyTo(Array array, int index)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (array.Rank != 1) throw new ArgumentException("Only single dimensional arrays are supported.", nameof(array));

        var buffer = _readBuffer;
        // Increment reference count to ensure thread-safe snapshot iteration
        Interlocked.Increment(ref buffer.ActiveReaders);
        try
        {
            // List<T>.CopyTo cannot directly copy to object[] if T is a value type.
            // We manually iterate to ensure type safety and correct boxing behavior.
            for (int i = 0; i < buffer.List.Count; i++)
            {
                array.SetValue(buffer.List[i], index + i);
            }
        }
        finally
        {
            Interlocked.Decrement(ref buffer.ActiveReaders);
        }
    }

    /// <summary>
    /// Helper method to safely cast object to T, matching standard .NET collection behavior.
    /// </summary>
    private static T CastValue(object value)
    {
        if (value == null && default(T) != null)
            throw new ArgumentException("Value cannot be null.", nameof(value));

        try
        {
            return (T)value;
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException($"Value is not of type {typeof(T).Name}.", nameof(value));
        }
    }

    #endregion
}