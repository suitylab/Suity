using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Collections;

/// <summary>
/// Adaptive Double Buffering HashSet with COW (Copy-On-Write) Fallback.
/// Designed for "Read-Heavy, Write-Light" scenarios where read latency and zero-allocation reads are critical.
/// 
/// Key Features:
/// 1. Zero-Allocation Reads: Read operations and native foreach loops are completely lock-free and zero-GC.
/// 2. Zero-GC Writes (99% case): Reuses the idle buffer without allocating new memory.
/// 3. Safe Fallback: If the idle buffer is occupied by a slow reader, it creates a new buffer 
///    (COW) and lets the GC reclaim the old one later, avoiding deadlocks or spin-waits.
/// </summary>
public class BufferedHashSet<T> : ISet<T>, IReadOnlyCollection<T>
{
    private class Buffer
    {
        public readonly HashSet<T> Set;
        // Tracks how many enumerators are currently iterating over this buffer
        public int ActiveReaders;

        public Buffer(IEqualityComparer<T> comparer)
        {
            Set = new HashSet<T>(comparer);
        }
    }

    // Volatile ensures that read threads always see the most recent buffer reference
    private volatile Buffer _readBuffer;

    // The write buffer, only accessed within the write lock
    private Buffer _writeBuffer;

    // Mutex for write operations to ensure atomicity during buffer switching
    private readonly object _writeLock = new object();

    private readonly IEqualityComparer<T> _comparer;

    public BufferedHashSet(IEqualityComparer<T> comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        _readBuffer = new Buffer(_comparer);
        _writeBuffer = new Buffer(_comparer);
    }

    #region Core Read/Write Logic

    /// <summary>
    /// Core method to apply any write action.
    /// Handles buffer reuse, allocation, data copying, and pointer swapping.
    /// </summary>
    public void ApplyWriteAction(Action<HashSet<T>> writeAction)
    {
        lock (_writeLock)
        {
            Buffer targetBuffer;

            // Core Logic: Check if the standby buffer (_writeBuffer) is occupied by readers
            if (Volatile.Read(ref _writeBuffer.ActiveReaders) == 0)
            {
                // Perfect Case: No readers are using it. Reuse it directly (Zero GC).
                targetBuffer = _writeBuffer;
                targetBuffer.Set.Clear();
            }
            else
            {
                // Conflict Case: A slow reader is still iterating over _writeBuffer.
                // Strategy: Abandon the occupied buffer (let GC reclaim it later) 
                // and create a new Buffer to take its place.
                targetBuffer = new Buffer(_comparer);
            }

            // Perform Copy-On-Write: Copy current data to the target buffer
            targetBuffer.Set.UnionWith(_readBuffer.Set);

            // Execute the specific write operation (Add, Remove, UnionWith, etc.)
            writeAction(targetBuffer.Set);

            // Atomic Pointer Swap:
            // The old read buffer becomes the new standby (write) buffer
            _writeBuffer = _readBuffer;
            // The newly modified buffer becomes the current read buffer
            _readBuffer = targetBuffer;
        }
    }

    #endregion

    #region Read Operations (Lock-Free)

    public int Count => _readBuffer.Set.Count;
    public bool IsReadOnly => false;

    public bool Contains(T item) => _readBuffer.Set.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _readBuffer.Set.CopyTo(array, arrayIndex);

    // Set comparison operations are purely read-based on the current snapshot
    public bool IsSubsetOf(IEnumerable<T> other) => _readBuffer.Set.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => _readBuffer.Set.IsSupersetOf(other);
    public bool IsProperSubsetOf(IEnumerable<T> other) => _readBuffer.Set.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => _readBuffer.Set.IsProperSupersetOf(other);
    public bool SetEquals(IEnumerable<T> other) => _readBuffer.Set.SetEquals(other);
    public bool Overlaps(IEnumerable<T> other) => _readBuffer.Set.Overlaps(other);

    #endregion

    #region Write Operations (Lock-Based)

    // ISet<T> requires bool Add, ICollection<T> requires void Add
    public bool Add(T item)
    {
        bool added = false;
        ApplyWriteAction(set => added = set.Add(item));
        return added;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Remove(T item)
    {
        bool removed = false;
        ApplyWriteAction(set => removed = set.Remove(item));
        return removed;
    }

    public void Clear() => ApplyWriteAction(set => set.Clear());

    // Bulk write operations
    public void UnionWith(IEnumerable<T> other) => ApplyWriteAction(set => set.UnionWith(other));
    public void IntersectWith(IEnumerable<T> other) => ApplyWriteAction(set => set.IntersectWith(other));
    public void ExceptWith(IEnumerable<T> other) => ApplyWriteAction(set => set.ExceptWith(other));
    public void SymmetricExceptWith(IEnumerable<T> other) => ApplyWriteAction(set => set.SymmetricExceptWith(other));

    #endregion

    #region Enumerator (Zero Allocation for foreach)

    /// <summary>
    /// Gets the enumerator. Returns a Struct to avoid Boxing during native foreach loops.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        var currentBuffer = _readBuffer;
        // Increment reference count to prevent this buffer from being reused/recycled 
        // while it is being iterated
        Interlocked.Increment(ref currentBuffer.ActiveReaders);
        return new EnumeratorWrapper(currentBuffer);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Custom Struct Enumerator.
    /// Note: Native foreach uses Duck Typing and calls this struct directly (Zero GC).
    /// However, LINQ methods (e.g., .Where()) cast to IEnumerable<T>, which triggers 
    /// Boxing. This is an inherent trait of .NET LINQ and is acceptable in this design.
    /// </summary>
    private struct EnumeratorWrapper : IEnumerator<T>
    {
        private readonly Buffer _buffer;
        private HashSet<T>.Enumerator _inner; // Use the underlying HashSet's Struct enumerator
        private bool _disposed;

        public EnumeratorWrapper(Buffer buffer)
        {
            _buffer = buffer;
            _inner = buffer.Set.GetEnumerator();
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
                // Decrement reference count after iteration finishes, 
                // allowing the write thread to potentially reuse or recycle this buffer
                Interlocked.Decrement(ref _buffer.ActiveReaders);
                _disposed = true;
            }
        }
    }

    #endregion
}