namespace Suity.Editor.Types;

internal class DataTypeAssetFilter : IAssetFilter
{
    public static readonly DataTypeAssetFilter Instance = new();

    public bool FilterAsset(Asset asset)
    {
        if (!AssetFilters.Default.FilterAsset(asset))
        {
            return false;
        }
        if (asset is DStruct || asset is DEnum)
        {
            return true;
        }
        if (asset.ParentAsset?.AssetKey == "*System")
        {
            return false;
        }

        return false;
    }

    public override string ToString() => "Type Definition Filter";
}