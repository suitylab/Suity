using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Values;

/// <summary>
/// Selection for data keys that supports abstract struct types.
/// </summary>
public class SKeySelection : AssetSelection
{
    private TypeDefinition _baseType;
    private SKeyAbstractSelectionList _abstractList;

    /// <summary>
    /// Creates an empty SKeySelection.
    /// </summary>
    public SKeySelection()
    { }

    /// <summary>
    /// Creates an SKeySelection with the specified base type.
    /// </summary>
    /// <param name="baseType">The base type definition.</param>
    public SKeySelection(TypeDefinition baseType)
    {
        BaseType = baseType;
    }

    /// <summary>
    /// Gets or sets the base type definition.
    /// </summary>
    public TypeDefinition BaseType
    {
        get => _baseType;
        set
        {
            _baseType = value;
            if (value?.IsAbstractStruct == true)
            {
                if (_abstractList is null)
                {
                    _abstractList = new SKeyAbstractSelectionList();
                }
                _abstractList.UpdateBaseType(_baseType);
                _abstractList.UpdateFilter(Filter);
            }
            else
            {
                _abstractList = null;
            }
        }
    }

    /// <summary>
    /// Gets the content type ID.
    /// </summary>
    public override Guid ContentTypeId => _baseType?.Target?.Id ?? Guid.Empty;

    /// <summary>
    /// Gets the content type name.
    /// </summary>
    public override string ContentTypeName => _baseType?.Target?.AssetKey ?? _baseType?.TypeCode;

    /// <summary>
    /// Gets or sets the asset filter.
    /// </summary>
    public override IAssetFilter Filter
    {
        get => base.Filter;
        set
        {
            base.Filter = value;
            _abstractList?.UpdateFilter(base.Filter);
        }
    }

    /// <summary>
    /// Gets the selection list, returning the abstract list for abstract struct types.
    /// </summary>
    public override ISelectionList GetSelectionList()
    {
        if (_baseType != null && _baseType.IsAbstractStruct && _abstractList != null)
        {
            return _abstractList;
        }

        return base.GetSelectionList();
    }

    /// <summary>
    /// Gets whether the current selection is valid.
    /// </summary>
    public override bool IsValid
    {
        get
        {
            if (_baseType != null && _baseType.IsAbstractStruct && _abstractList != null)
            {
                var target = this.TargetAsset;

                if (target != null)
                {
                    return _abstractList.GetItem(target.AssetKey) != null;
                }
                else
                {
                    return true;
                }
            }

            return base.IsValid;
        }
    }

    /// <summary>
    /// Checks whether the specified asset is valid for this selection.
    /// </summary>
    /// <param name="asset">The asset to validate.</param>
    public override bool GetIsValid(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (_baseType != null && _baseType.IsAbstractStruct && _abstractList != null)
        {
            return _abstractList.GetItem(asset.AssetKey) != null;
        }

        return base.GetIsValid(asset);
    }

    /// <summary>
    /// Synchronizes the selection properties.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    /// <param name="context">The sync context.</param>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            BaseType = sync.Sync("BaseType", BaseType, SyncFlag.AttributeMode | SyncFlag.ByRef);
        }
    }
}

/// <summary>
/// Selection list for abstract struct types.
/// </summary>
public class SKeyAbstractSelectionList : ISelectionList
{
    private TypeDefinition _baseType;
    private IAssetFilter _filter;

    /// <summary>
    /// Creates an empty SKeyAbstractSelectionList.
    /// </summary>
    public SKeyAbstractSelectionList()
    { }

    /// <summary>
    /// Updates the base type.
    /// </summary>
    /// <param name="baseType">The base type.</param>
    public void UpdateBaseType(TypeDefinition baseType) => _baseType = baseType;

    /// <summary>
    /// Updates the filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    public void UpdateFilter(IAssetFilter filter) => _filter = filter;

    /// <summary>
    /// Gets the selection item by key.
    /// </summary>
    /// <param name="key">The key.</param>
    public ISelectionItem GetItem(string key)
    {
        if (_baseType?.Target is not DAbstract baseType)
        {
            return null;
        }

        var collection = DTypeManager.Instance.GetStructsByBaseType(baseType);
        if (collection is null)
        {
            return null;
        }

        foreach (var type in collection.Assets)
        {
            var assets = AssetManager.Instance.GetAssetCollection(type.Id);
            var asset = assets?.GetAsset(key, _filter);

            if (asset != null)
            {
                return asset;
            }
        }

        return null;
    }

    public IEnumerable<ISelectionItem> GetItems()
    {
        if (_baseType?.Target is not DAbstract baseType)
        {
            yield break;
        }

        var collection = DTypeManager.Instance.GetStructsByBaseType(baseType);
        if (collection is null)
        {
            yield break;
        }

        foreach (var type in collection.Assets)
        {
            var assets = AssetManager.Instance.GetAssetCollection(type.Id);
            if (assets is null)
            {
                continue;
            }

            foreach (var asset in assets.Assets)
            {
                if (_filter is null || _filter.FilterAsset(asset))
                {
                    yield return asset;
                }
            }
        }
    }
}