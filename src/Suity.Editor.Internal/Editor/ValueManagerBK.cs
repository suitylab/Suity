using Suity.Editor.Types;
using System;
using System.Collections.Concurrent;

namespace Suity.Editor;

/// <summary>
/// A collection of <see cref="ValueAsset"/> items grouped by a specific <see cref="TypeDefinition"/>.
/// </summary>
/// <param name="type">The type definition that categorizes this collection.</param>
public class ValueAssetCollection(TypeDefinition type) : AssetCollection<ValueAsset>, IValueAssetCollection
{
    /// <summary>
    /// Gets the type definition associated with this value asset collection.
    /// </summary>
    public TypeDefinition Type { get; } = type;
}

/// <summary>
/// Internal singleton implementation of <see cref="ValueManager"/> that manages value asset collections by type definition.
/// </summary>
internal class ValueManagerBK : ValueManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ValueManagerBK"/>.
    /// </summary>
    public new static readonly ValueManagerBK Instance = new();

    private readonly ConcurrentDictionary<TypeDefinition, ValueAssetCollection> _values = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueManagerBK"/> class.
    /// </summary>
    private ValueManagerBK()
    { }

    /// <summary>
    /// Initializes the value manager by setting this instance as the active <see cref="ValueManager"/>.
    /// </summary>
    internal void Initialize()
    {
        ValueManager.Instance = this;
    }

    /// <inheritdoc/>
    public override IValueAssetCollection GetValueCollection(TypeDefinition type)
    {
        if (_values.TryGetValue(type ?? TypeDefinition.Empty, out var collection))
        {
            return collection;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Adds a <see cref="ValueAsset"/> to the appropriate type-based collection.
    /// </summary>
    /// <param name="value">The value asset to add. Must have a valid Id and AssetKey.</param>
    /// <returns>A registry handle for the added asset, or null if the asset is invalid.</returns>
    internal override IRegistryHandle<ValueAsset> AddToValueType(ValueAsset value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Id == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrEmpty(value.AssetKey))
        {
            return null;
        }

        var type = value.ValueType ?? TypeDefinition.Empty;

        var collection = _values.GetOrAdd(type, _ => new(type));

        var entry = collection.AddAsset(value.AssetKey, value);

        if (entry != null)
        {
            return new MultipleItemRegHandle<ValueAsset>(entry, value);
        }

        return null;
    }
}