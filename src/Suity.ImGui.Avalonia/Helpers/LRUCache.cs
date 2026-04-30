namespace Suity.Helpers;

/// <summary>
/// High-performance LRU (Least Recently Used) cache generic class
/// </summary>
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cacheMap;
    private readonly LinkedList<CacheItem> _lruList;
    private readonly Action<TValue>? _onDispose;

    private class CacheItem
    {
        public TKey Key { get; init; } = default!;
        public TValue Value { get; init; } = default!;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="capacity">Maximum cache capacity</param>
    /// <param name="onDispose">Disposal logic when object is removed from cache (e.g., Dispose)</param>
    public LRUCache(int capacity, Action<TValue>? onDispose = null)
    {
        _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));
        _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
        _lruList = new LinkedList<CacheItem>();
        _onDispose = onDispose;
    }

    /// <summary>
    /// Get or add cache item
    /// </summary>
    public TValue GetOrCreate(TKey key, Func<TValue> valueFactory)
    {
        // 1. Quick attempt to get (inside lock)
        lock (_lruList)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                return node.Value.Value;
            }
        }

        // 2. Build new value outside lock
        TValue newValue = valueFactory();

        // Prepare to collect objects that need disposal (may be removed due to full capacity, or discarded due to concurrency conflicts)
        TValue? valueToDispose = default;
        bool hasValueToDispose = false;

        // 3. Write process
        lock (_lruList)
        {
            // Double-check: handle concurrency race conditions
            if (_cacheMap.TryGetValue(key, out var existingNode))
            {
                // If you insist on not wasting newValue, then you need to "update" the old value here
                // But remember: if the replaced old value is not disposed, it will cause memory leaks
                valueToDispose = existingNode.Value.Value;
                hasValueToDispose = true;

                _lruList.Remove(existingNode);
                // Remove old node, prepare to insert new node
                _cacheMap.Remove(key);
            }
            else if (_cacheMap.Count >= _capacity)
            {
                // Normal capacity exceeded handling
                var lastNode = _lruList.Last;
                if (lastNode != null)
                {
                    valueToDispose = lastNode.Value.Value;
                    hasValueToDispose = true;
                    _lruList.RemoveLast();
                    _cacheMap.Remove(lastNode.Value.Key);
                }
            }

            // Insert new value
            var cacheItem = new CacheItem { Key = key, Value = newValue };
            var newNode = _lruList.AddFirst(cacheItem);
            _cacheMap.Add(key, newNode);
        }

        // 4. Execute disposal logic outside lock (unified handling: whether replaced or removed due to capacity)
        if (hasValueToDispose && _onDispose != null)
        {
            _onDispose(valueToDispose!);
        }

        return newValue;
    }

    /// <summary>
    /// Manually clear cache
    /// </summary>
    public void Clear()
    {
        List<TValue>? itemsToDispose = null;
        lock (_lruList)
        {
            foreach (var item in _lruList)
            {
                (itemsToDispose ??=[]).Add(item.Value);
            }
            _lruList.Clear();
            _cacheMap.Clear();
        }

        if (itemsToDispose != null && _onDispose != null)
        {
            foreach (var value in itemsToDispose)
            {
                _onDispose(value);
            }
        }
    }
}
