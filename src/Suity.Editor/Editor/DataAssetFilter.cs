using Suity.Editor.Types;

namespace Suity.Editor;

public class DataAssetFilter : IAssetFilter
{
    private readonly TypeDefinition _type;

    public DataAssetFilter(string type)
    {
        _type = TypeDefinition.Resolve(type) ?? TypeDefinition.Empty;
    }

    public DataAssetFilter(TypeDefinition type)
    {
        _type = type ?? TypeDefinition.Empty;
    }

    public bool FilterAsset(Asset asset)
    {
        if (asset is null || asset.ParentAsset is null)
        {
            return false;
        }

        if (asset.ParentAsset.InstanceMode == AssetInstanceMode.Instance)
        {
            return false;
        }

        if (asset.AccessMode != AssetAccessMode.Public)
        {
            return false;
        }

        if (asset is IDataAsset dataAsset)
        {
            return dataAsset.SupportType(_type);
        }
        else
        {
            return asset.GetType().ResolveAssetTypeName() == _type.TypeCode;
        }
    }

    public override string ToString()
    {
        return _type.ToString();
    }
}