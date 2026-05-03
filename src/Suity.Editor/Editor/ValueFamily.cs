using Suity.Drawing;

namespace Suity.Editor;

/// <summary>
/// Value family asset that groups value assets.
/// </summary>
[AssetTypeBinding(AssetDefNames.ValueFamily)]
public class ValueFamily : GroupAsset
{
    internal ImageDef _icon = CoreIconCache.Value;

    public override ImageDef DefaultIcon => _icon;
}

/// <summary>
/// Value family builder.
/// </summary>
public class ValueFamilyBuilder : GroupAssetBuilder<ValueFamily>
{
    private readonly bool _isFlow;

    public ValueFamilyBuilder()
    {
    }
    public ValueFamilyBuilder(bool isFlow)
    {
        _isFlow = isFlow;
    }

    public override void UpdateAsset()
    {
        base.UpdateAsset();

        var asset = this.Asset;
        if (asset != null)
        {
            asset._icon = _isFlow ? CoreIconCache.Flow : CoreIconCache.Value;
        }
    }

}