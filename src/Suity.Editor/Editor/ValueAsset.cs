using Suity.Drawing;
using Suity.Editor.Types;
using System;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Value asset
/// </summary>
[AssetTypeBinding(AssetDefNames.Value, ">Value asset")]
public class ValueAsset : Asset
{
    private TypeDefinition _type = TypeDefinition.Empty;
    private object _value;

    private IRegistryHandle<ValueAsset> _entry;

    public ValueAsset()
    {
    }

    public TypeDefinition ValueType
    {
        get => _type;
        protected internal set
        {
            if (_type == value)
            {
                return;
            }

            _type = value;
            _entry?.Dispose();
            _entry = ValueManager.Instance.AddToValueType(this);

            NotifyPropertyUpdated();
        }
    }

    public virtual object Value
    {
        get => _value;
        protected internal set
        {
            if (ReferenceEquals(_value, value))
            {
                return;
            }

            _value = value;

            NotifyPropertyUpdated();
        }
    }

    public virtual object GetValue(object caller, ICondition resolveContext)
    {
        return _value;
    }

    public override ImageDef DefaultIcon => CoreIconCache.Value;

    internal override void InternalOnAssetActivate(string assetKey)
    {
        _entry?.Dispose();
        _entry = ValueManager.Instance.AddToValueType(this);

        base.InternalOnAssetActivate(assetKey);
    }

    internal override void InternalOnEntryDetached(Guid id)
    {
        var entry = _entry;
        _entry = null;
        entry?.Dispose();

        base.InternalOnEntryDetached(id);
    }
}

/// <summary>
/// Value asset builder.
/// </summary>
public class ValueAssetBuilder : AssetBuilder<ValueAsset>
{
    private TypeDefinition _type;
    private object _value;

    public ValueAssetBuilder()
    {
        AddAutoUpdate(nameof(ValueAsset.ValueType), o => o.ValueType = _type);
        AddAutoUpdate(nameof(ValueAsset.Value), o => o.Value = _value);
    }

    public void SetType(TypeDefinition type)
    {
        _type = type;

        UpdateAuto(nameof(ValueAsset.ValueType));
    }

    public void SetValue(object value)
    {
        _value = value;

        UpdateAuto(nameof(ValueAsset.Value));
    }
}