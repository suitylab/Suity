using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Internal sealed implementation of <see cref="AssetKeyEntry"/> that manages multiple asset targets by key
/// with conflict detection and thread-safe add/remove operations.
/// </summary>
internal sealed class AssetKeyEntryBK : AssetKeyEntry
{
    private readonly MultipleItem<Asset> _item = new();

    /// <inheritdoc/>
    public override Asset Target => _item.Value;

    /// <inheritdoc/>
    internal override IEnumerable<Asset> Targets => _item.Values;

    /// <inheritdoc/>
    internal override int TargetCount => _item.Count;

    /// <inheritdoc/>
    public override bool AssetKeyConflict => _item.Count > 1;

    /// <inheritdoc/>
    public override Asset GetTarget(IAssetFilter filter)
    {
        Asset asset = _item.Value;
        if (asset != null && filter.FilterAsset(asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    internal override void Add(Asset value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        lock (_item)
        {
            _item.Add(value);
        }
    }

    /// <inheritdoc/>
    internal override bool Remove(Asset value)
    {
        if (value is null)
        {
            return false;
        }

        lock (_item)
        {
            return _item.Remove(value);
        }
    }
}