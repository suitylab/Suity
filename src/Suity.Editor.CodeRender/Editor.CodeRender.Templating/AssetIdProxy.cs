using Suity.Editor.Types;
using System;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that provides dynamic access to an asset identified by a GUID.
/// Exposes the asset's type definition and target model through dynamic member access.
/// </summary>
internal class AssetIdProxy : RenderProxy
{
    private readonly Guid _id;

    /// <summary>
    /// Gets the GUID identifier of the asset.
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Initializes a new instance with a base proxy, additional expression code, and an asset GUID.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="id">The GUID identifier of the asset.</param>
    public AssetIdProxy(RenderProxy baseProxy, string exCode, Guid id)
        : base(baseProxy, exCode)
    {
        _id = id;
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (!IsContentValid())
        {
            return base.TryGetMember(binder, out result);
        }

        switch (binder.Name)
        {
            case "Type":
                {
                    DType type = AssetManager.Instance.GetAsset<DType>(_id);
                    if (type != null)
                    {
                        result = new TypeDefinitionProxy(this, ".Type", type.Definition);
                        return true;
                    }
                    break;
                }

            case "Target":
            case "TypeInfo":
                {
                    var asset = AssetManager.Instance.GetAsset(_id);
                    if (asset != null)
                    {
                        result = new RenderModelProxy(this, ".Target", asset);
                        return true;
                    }
                    break;
                }
            default:
                break;
        }

        return base.TryGetMember(binder, out result);
    }

    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (!IsContentValid())
        {
            return base.TryInvokeMember(binder, args, out result);
        }

        switch (binder.Name)
        {
            case "Type":
                // Type(string) - resolve the asset type and return its type name string
                if (args.Length == 1 && args[0] is string)
                {
                    DType type = AssetManager.Instance.GetAsset<DType>(_id);
                    if (type != null)
                    {
                        result = GetTypeString(type.Definition);
                        return true;
                    }
                }
                break;

            default:
                break;
        }

        return base.TryInvokeMember(binder, args, out result);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _id.ToString();
    }
}
