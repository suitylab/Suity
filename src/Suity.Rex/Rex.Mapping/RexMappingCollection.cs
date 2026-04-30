using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Mapping;

/// <summary>
/// Represents a collection of mapping information for a specific request type.
/// </summary>
public sealed class RexMappingCollection
{
    private readonly Type _requestType;
    private readonly LinkedList<RexMappingInfo> _infos = [];
    private readonly Dictionary<object, LinkedListNode<RexMappingInfo>> _objToInfo = [];
    private RexMappingInfo _externalResolve;

    private Predicate<RexMappingInfo> _filter;

    /// <summary>
    /// Gets the type being requested for this collection.
    /// </summary>
    public Type RequestType => _requestType;

    internal Predicate<RexMappingInfo> Filter
    {
        get => _filter;
        set => _filter = value;
    }

    internal RexMappingCollection(Type requestType)
    {
        _requestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
    }

    internal RexMappingCollection(Type requestType, Predicate<RexMappingInfo> filter)
        : this(requestType)
    {
        _filter = filter;
    }

    internal void Add(RexMappingInfo info)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        LinkedListNode<RexMappingInfo> node;

        switch (info.MappingMode)
        {
            case RexMappingMode.Preset:
                if (info.PresetObject is null)
                {
                    throw new NullReferenceException(nameof(info.PresetObject));
                }

                if (_objToInfo.ContainsKey(info.PresetObject))
                {
                    throw new InvalidOperationException("Object exist.");
                }

                node = SortedAdd(info);
                _objToInfo.Add(info.PresetObject, node);
                break;

            case RexMappingMode.Singleton:
            case RexMappingMode.Instance:
                if (_objToInfo.ContainsKey(info.ImplementType))
                {
                    throw new InvalidOperationException("Object exist.");
                }

                node = SortedAdd(info);
                _objToInfo.Add(info.ImplementType, node);
                break;

            default:
                throw new InvalidOperationException();
        }
    }

    internal bool Remove(object obj)
    {
        LinkedListNode<RexMappingInfo> node = _objToInfo.RemoveAndGet(obj);
        if (node != null)
        {
            _infos.Remove(node);

            return true;
        }
        else
        {
            return false;
        }
    }

    internal void Clear()
    {
        _infos.Clear();
        _objToInfo.Clear();
    }

    internal RexMappingInfo IncreaseExternalResolved()
    {
        (_externalResolve ??= new(_requestType)).IncreaseCounter();

        return _externalResolve;
    }

    /// <summary>
    /// Checks if the collection contains the specified object.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is registered, false otherwise.</returns>
    public bool Contains(object obj)
    {
        return _objToInfo.ContainsKey(obj);
    }

    /// <summary>
    /// Gets the first mapping info that matches the filter (if any).
    /// </summary>
    /// <returns>The first matching RexMappingInfo, or null if none found.</returns>
    public RexMappingInfo First()
    {
        if (_filter != null)
        {
            var node = _infos.First;
            while (node != null)
            {
                if (_filter(node.Value))
                {
                    return node.Value;
                }

                node = node.Next;
            }

            return null;
        }
        else
        {
            var first = _infos.First;

            return first?.Value;
        }
    }

    /// <summary>
    /// Gets all mapping infos in this collection, including external resolves if present.
    /// </summary>
    public IEnumerable<RexMappingInfo> Infos
    {
        get
        {
            IEnumerable<RexMappingInfo> infos;

            if (_filter != null)
            {
                infos = _infos.Where(o => _filter(o));
            }
            else
            {
                infos = _infos.Select(o => o);
            }

            if (_externalResolve != null)
            {
                return infos.Concat([_externalResolve]);
            }
            else
            {
                return infos;
            }
        }
    }

    private LinkedListNode<RexMappingInfo> SortedAdd(RexMappingInfo info)
    {
        var node = _infos.First;

        while (node != null)
        {
            if (info.MappingMode > node.Value.MappingMode)
            {
                return _infos.AddBefore(node, info);
            }

            node = node.Next;
        }

        return _infos.AddLast(info);
    }
}