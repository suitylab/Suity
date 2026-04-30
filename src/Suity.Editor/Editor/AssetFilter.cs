namespace Suity.Editor;

/// <summary>
/// Asset filter interface
/// </summary>
public interface IAssetFilter
{
    bool FilterAsset(Asset asset);
}

public static class AssetFilters
{
    public static readonly IAssetFilter RootAsset = new RootAssetFilter();

    /// <summary>
    /// Default filter
    /// </summary>
    public static readonly IAssetFilter Default = new DefaultAssetFilter();

    /// <summary>
    /// Private filter
    /// </summary>
    public static readonly IAssetFilter Private = new PrivateAssetFilter();

    /// <summary>
    /// All filter
    /// </summary>
    public static readonly IAssetFilter All = new AllAssetFilter();

    public static readonly IAssetFilter None = new NoneAssetFilter();
    public static readonly IAssetFilter Pending = new PendingAssetFilter();
}

internal class RootAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        return asset.ParentAsset is null;
    }
}

internal class DefaultAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (asset.InstanceMode == AssetInstanceMode.Instance)
        {
            return false;
        }

        if (asset.AccessMode != AssetAccessMode.Public)
        {
            return false;
        }

        return true;
    }

    public override string ToString() => "Default asset filter";
}

internal class PrivateAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        if (asset.InstanceMode == AssetInstanceMode.Instance)
        {
            return false;
        }

        if (asset.AccessMode != AssetAccessMode.Public && asset.AccessMode != AssetAccessMode.Private)
        {
            return false;
        }

        return true;
    }

    public override string ToString() => "Private asset filter";
}

internal class AllAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset asset)
    {
        return asset != null;
    }

    public override string ToString() => "All asset filter";
}

internal class NoneAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset content)
    {
        return false;
    }

    public override string ToString() => "None asset filter";
}

internal class PendingAssetFilter : IAssetFilter
{
    public bool FilterAsset(Asset content)
    {
        if (content is null)
        {
            return false;
        }

        if (content.InstanceMode == AssetInstanceMode.Instance)
        {
            return false;
        }

        return true;
    }

    public override string ToString() => "Pending asset filter";
}