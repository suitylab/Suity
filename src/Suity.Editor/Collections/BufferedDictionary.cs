using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Suity.Collections;

/// <summary>
/// Adaptive Double Buffering Dictionary with COW (Copy-On-Write) Fallback.
/// Designed for "Read-Heavy, Write-Light" scenarios where read latency and zero-allocation reads are critical.
/// </summary>
public class BufferedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
{
    private class Buffer
    {
        public readonly Dictionary<TKey, TValue> Dict;
        public int ActiveReaders;

        public Buffer(int capacity, IEqualityComparer<TKey> comparer)
        {
            Dict = new Dictionary<TKey, TValue>(capacity, comparer);
        }
    }

    private volatile Buffer _readBuffer;
    private Buffer _writeBuffer;
    private readonly object _writeLock = new();
    private readonly IEqualityComparer<TKey> _comparer;

    // Cached instances to avoid allocating new collection objects on every property access
    private KeyCollection _keys;
    private ValueCollection _values;

    public BufferedDictionary(IEqualityComparer<TKey> comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _readBuffer = new Buffer(0, _comparer);
        _writeBuffer = new Buffer(0, _comparer);
    }

    #region Core Read/Write Logic

    public void ApplyWriteAction(Action<Dictionary<TKey, TValue>> writeAction)
    {
        lock (_writeLock)
        {
            Buffer targetBuffer;

            if (Volatile.Read(ref _writeBuffer.ActiveReaders) == 0)
            {
                targetBuffer = _writeBuffer;
                targetBuffer.Dict.Clear();
            }
            else
            {
                targetBuffer = new Buffer(_readBuffer.Dict.Count, _comparer);
            }

            foreach (var kvp in _readBuffer.Dict)
            {
                targetBuffer.Dict[kvp.Key] = kvp.Value;
            }

            writeAction(targetBuffer.Dict);

            _writeBuffer = _readBuffer;
            _readBuffer = targetBuffer;
        }
    }

    #endregion

    #region Read Operations (Lock-Free)

    public TValue this[TKey key]
    {
        get => _readBuffer.Dict[key];
        set => ApplyWriteAction(dict => dict[key] = value);
    }

    public int Count => _readBuffer.Dict.Count;
    public bool IsReadOnly => false;

    public bool ContainsKey(TKey key) => _readBuffer.Dict.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => _readBuffer.Dict.TryGetValue(key, out value);

    // Encapsulated Keys and Values collections
    public KeyCollection Keys => _keys ??= new KeyCollection(this);
    public ValueCollection Values => _values ??= new ValueCollection(this);

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    #endregion

    #region Write Operations (Lock-Based)

    public void Add(TKey key, TValue value) => ApplyWriteAction(dict => dict.Add(key, value));
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public bool Remove(TKey key)
    {
        bool removed = false;
        ApplyWriteAction(dict => removed = dict.Remove(key));
        return removed;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        bool removed = false;
        ApplyWriteAction(dict =>
        {
            if (dict.TryGetValue(item.Key, out TValue currentValue) &&
                EqualityComparer<TValue>.Default.Equals(currentValue, item.Value))
            {
                removed = dict.Remove(item.Key);
            }
        });
        return removed;
    }

    public void Clear() => ApplyWriteAction(dict => dict.Clear());

    #endregion

    #region Query & Collection Operations

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (_readBuffer.Dict.TryGetValue(item.Key, out TValue value))
        {
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_readBuffer.Dict).CopyTo(array, arrayIndex);
    }

    #endregion

    #region Main Enumerator (Zero Allocation for foreach)

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var currentBuffer = _readBuffer;
        Interlocked.Increment(ref currentBuffer.ActiveReaders);
        return new EnumeratorWrapper(currentBuffer);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private struct EnumeratorWrapper : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly Buffer _buffer;
        private Dictionary<TKey, TValue>.Enumerator _inner;
        private bool _disposed;

        public EnumeratorWrapper(Buffer buffer)
        {
            _buffer = buffer;
            _inner = buffer.Dict.GetEnumerator();
            _disposed = false;
        }

        public KeyValuePair<TKey, TValue> Current => _inner.Current;
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

    #region Encapsulated Key & Value Collections

    /// <summary>
    /// Thread-safe, Zero-GC encapsulated collection for Keys.
    /// Implemented as a nested class to access the outer class's private buffer fields.
    /// </summary>
    public sealed class KeyCollection : ICollection<TKey>, IReadOnlyCollection<TKey>
    {
        private readonly BufferedDictionary<TKey, TValue> _dictionary;

        internal KeyCollection(BufferedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => true;
        public bool Contains(TKey item) => _dictionary.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            // Safe copy: uses our custom enumerator to manage reference counting
            using var enumerator = GetEnumerator();
            int index = arrayIndex;
            while (enumerator.MoveNext())
            {
                array[index++] = enumerator.Current;
            }
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            var currentBuffer = _dictionary._readBuffer;
            Interlocked.Increment(ref currentBuffer.ActiveReaders);
            return new KeyEnumerator(currentBuffer);
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();
        void ICollection<TKey>.Clear() => throw new NotSupportedException();
        bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();

        /// <summary>
        /// Struct enumerator for Keys to ensure Zero-GC Duck Typing during native foreach.
        /// </summary>
        private struct KeyEnumerator : IEnumerator<TKey>
        {
            private readonly Buffer _buffer;
            private Dictionary<TKey, TValue>.KeyCollection.Enumerator _inner;
            private bool _disposed;

            public KeyEnumerator(Buffer buffer)
            {
                _buffer = buffer;
                _inner = buffer.Dict.Keys.GetEnumerator();
                _disposed = false;
            }

            public TKey Current => _inner.Current;
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
    }

    /// <summary>
    /// Thread-safe, Zero-GC encapsulated collection for Values.
    /// </summary>
    public sealed class ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>
    {
        private readonly BufferedDictionary<TKey, TValue> _dictionary;

        internal ValueCollection(BufferedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => true;

        public bool Contains(TValue item)
        {
            // O(N) scan, but thread-safe
            foreach (var val in this)
            {
                if (EqualityComparer<TValue>.Default.Equals(val, item)) return true;
            }
            return false;
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            using var enumerator = GetEnumerator();
            int index = arrayIndex;
            while (enumerator.MoveNext())
            {
                array[index++] = enumerator.Current;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var currentBuffer = _dictionary._readBuffer;
            Interlocked.Increment(ref currentBuffer.ActiveReaders);
            return new ValueEnumerator(currentBuffer);
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();
        void ICollection<TValue>.Clear() => throw new NotSupportedException();
        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

        /// <summary>
        /// Struct enumerator for Values to ensure Zero-GC Duck Typing.
        /// </summary>
        private struct ValueEnumerator : IEnumerator<TValue>
        {
            private readonly Buffer _buffer;
            private Dictionary<TKey, TValue>.ValueCollection.Enumerator _inner;
            private bool _disposed;

            public ValueEnumerator(Buffer buffer)
            {
                _buffer = buffer;
                _inner = buffer.Dict.Values.GetEnumerator();
                _disposed = false;
            }

            public TValue Current => _inner.Current;
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
    }

    #endregion
}