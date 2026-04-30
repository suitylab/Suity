using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines the mapping mode for how a type is resolved.
/// </summary>
public enum RexMappingMode
{
    /// <summary>
    /// Resolved from external service providers.
    /// </summary>
    External = 0,
    /// <summary>
    /// Resolved from a preset instance.
    /// </summary>
    Preset = 1,
    /// <summary>
    /// Resolved as a singleton instance (created once).
    /// </summary>
    Singleton = 2,
    /// <summary>
    /// Resolved as a new instance each time.
    /// </summary>
    Instance = 3,
}

/// <summary>
/// Records the success and failure counts for named resolutions.
/// </summary>
public class RexNameRecord
{
    private readonly string _name;
    private int _counterSuccess;
    private int _counterFailed;

    /// <summary>
    /// Gets the name associated with this record.
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// Gets the number of successful resolutions.
    /// </summary>
    public int CounterSuccess => _counterSuccess;
    /// <summary>
    /// Gets the number of failed resolutions.
    /// </summary>
    public int CounterFailed => _counterFailed;

    internal RexNameRecord(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        _name = name;
    }

    internal void Increase(bool success)
    {
        if (success)
        {
            _counterSuccess++;
        }
        else
        {
            _counterFailed++;
        }
    }
}

/// <summary>
/// Contains information about a type mapping registration.
/// </summary>
public sealed class RexMappingInfo
{
    /// <summary>
    /// Gets the implementation type for this mapping.
    /// </summary>
    public Type ImplementType { get; }
    /// <summary>
    /// Gets the mapping mode (External, Preset, Singleton, or Instance).
    /// </summary>
    public RexMappingMode MappingMode { get; }
    /// <summary>
    /// Gets the preset object instance (only for Preset mode).
    /// </summary>
    public object PresetObject { get; }
    /// <summary>
    /// Gets or sets the singleton object instance (only for Singleton mode).
    /// </summary>
    public object SingletonObject { get; internal set; }
    /// <summary>
    /// Gets the total number of times this mapping has been resolved.
    /// </summary>
    public int Counter => _counter;
    /// <summary>
    /// Gets the number of new instances created.
    /// </summary>
    public int NewCounter => _newCounter;
    /// <summary>
    /// Gets the collection of named resolution records.
    /// </summary>
    public IEnumerable<RexNameRecord> ResolvedNames => _names != null ? _names.Values.Select(o => o) : [];

    private int _counter;
    private int _newCounter;
    private Dictionary<string, RexNameRecord> _names;

    internal RexMappingInfo(Type type)
    {
        ImplementType = type;
        MappingMode = RexMappingMode.External;
    }

    internal RexMappingInfo(Type implementType, bool singleton)
    {
        ImplementType = implementType ?? throw new ArgumentNullException(nameof(implementType));
        MappingMode = singleton ? RexMappingMode.Singleton : RexMappingMode.Instance;
    }

    internal RexMappingInfo(object presetObj)
    {
        if (presetObj == null)
        {
            throw new ArgumentNullException(nameof(presetObj));
        }

        ImplementType = presetObj.GetType();
        MappingMode = RexMappingMode.Preset;
        PresetObject = presetObj;
    }

    internal T Resolve<T>() where T : class
    {
        _counter++;

        switch (MappingMode)
        {
            case RexMappingMode.Preset:
                return PresetObject as T;

            case RexMappingMode.Singleton:
                if (SingletonObject is T t)
                {
                    return t;
                }
                else
                {
                    _newCounter++;
                    T result = (T)Activator.CreateInstance(ImplementType);
                    SingletonObject = result;
                    return result;
                }
            case RexMappingMode.Instance:
                _newCounter++;
                return (T)Activator.CreateInstance(ImplementType);

            default:
                return null;
        }
    }

    internal void IncreaseCounter()
    {
        _counter++;
    }

    internal void IncreaseNewCounter()
    {
        _counter++;
        _newCounter++;
    }

    internal void AddName(string name, bool success)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        (_names ??= []).GetOrAdd(name, _ => new(name)).Increase(success);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (MappingMode == RexMappingMode.External)
        {
            return "[External]";
        }
        else
        {
            return ImplementType.FullName;
        }
    }
}