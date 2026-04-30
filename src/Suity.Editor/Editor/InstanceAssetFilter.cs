using System;

namespace Suity.Editor;

internal class InstanceAssetFilter : IAssetFilter
{
    private readonly Asset _myAsset;
    private readonly bool _instance;

    internal InstanceAssetFilter(Asset myAsset, bool instance)
    {
        _myAsset = myAsset ?? throw new ArgumentNullException();
        _instance = instance;
    }

    public bool FilterAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (asset.ShareSameRoot(_myAsset))
        {
            if (asset.AccessMode == AssetAccessMode.NoAccess)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            if (_instance)
            {
                if (asset.InstanceMode != AssetInstanceMode.Instance)
                {
                    return false;
                }
            }
            else
            {
                if (asset.InstanceMode == AssetInstanceMode.Instance)
                {
                    return false;
                }
            }

            if (asset.AccessMode != AssetAccessMode.Public)
            {
                return false;
            }

            return true;
        }
    }

    public override string ToString()
    {
        if (_instance)
        {
            return "SelfAssetFilter";
        }
        else
        {
            return "SelfAssetFilter(Instance)";
        }
    }
}