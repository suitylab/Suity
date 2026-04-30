using static Suity.Helpers.GlobalLocalizer;
using System;

namespace Suity.Editor;

/// <summary>
/// Asset reference
/// </summary>
public class EditorAssetRef : EditorObjectRef<Asset>, IHasAsset
{
    public string AssetKey
    {
        get => Target?.AssetKey ?? GlobalIdResolver.RevertResolve(base.Id);
        set
        {
            base.Id = GlobalIdResolver.Resolve(value);
        }
    }

    public EditorAssetRef()
    {
    }

    public EditorAssetRef(Guid id)
        : base(id)
    {
    }

    public EditorAssetRef(string assetKey)
    {
        base.Id = GlobalIdResolver.Resolve(assetKey);
    }

    public EditorAssetRef(Asset target)
        : base(target?.Id ?? Guid.Empty)
    {
    }

    public Asset TargetAsset => base.Target;

    public override string ToString()
    {
        return L(Id.ToDescriptionText());
    }
}

public sealed class EditorAssetRef<T> : EditorAssetRef
    where T : class
{
    public EditorAssetRef()
    {
    }

    public EditorAssetRef(Guid id)
        : base(id)
    {
    }

    public EditorAssetRef(string assetKey)
        : base(assetKey)
    {
    }

    public EditorAssetRef(Asset target)
        : base(target)
    {
    }

    public new T Target
    {
        get => base.Target as T;
        set => base.Target = value as Asset;
    }
}

public struct RawAssetKey
{
    public string AssetKey;

    public RawAssetKey(string assetKey)
    {
        AssetKey = assetKey;
    }

    public override string ToString()
    {
        return AssetKey ?? string.Empty;
    }
}