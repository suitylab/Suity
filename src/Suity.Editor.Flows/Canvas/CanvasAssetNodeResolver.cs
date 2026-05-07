using Suity.Collections;
using Suity.Editor.Flows;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Documents.Canvas;

/// <summary>
/// Resolves and creates canvas asset nodes based on asset types.
/// </summary>
internal sealed class CanvasAssetNodeResolver
{
    /// <summary>
    /// Gets the singleton instance of the canvas asset node resolver.
    /// </summary>
    public static CanvasAssetNodeResolver Instance { get; } = new();

    private static readonly UniqueMultiDictionary<Type, Type> _viewTypes = new();
    private static bool _init;

    private CanvasAssetNodeResolver()
    {
    }

    private void Initialize()
    {
        foreach (Type nodeType in typeof(CanvasAssetNode).GetDerivedTypes())
        {
            var attrs = nodeType.GetAttributesCached<CanvasAssetTypeAttribute>().ToArray();

            foreach (var attr in attrs)
            {
                if (attr.AssetType is null)
                {
                    continue;
                }

                _viewTypes.Add(attr.AssetType, nodeType);
            }
        }

        foreach (Type nodeType in typeof(CanvasAssetNode<>).GetDerivedTypes())
        {
            var assetType = nodeType.BaseType.GetGenericArguments()[0];

            _viewTypes.Add(assetType, nodeType);
        }

        _init = true;
    }

    /// <summary>
    /// Creates a canvas asset node for the specified asset, resolving the appropriate node type.
    /// </summary>
    /// <param name="asset">The asset to create a node for.</param>
    /// <returns>The created canvas asset node.</returns>
    internal CanvasAssetNode CreateNode(Asset asset)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        if (!_init)
        {
            Initialize();
        }

        Type viewType = ResolveViewType(asset.GetType());
        if (viewType != null)
        {
            var node = (CanvasAssetNode)viewType.CreateInstanceOf();
            node.TargetAsset = asset;

            return node;
        }
        else
        {
            return new CanvasAssetNode(asset);
        }
    }

    /// <summary>
    /// Resolves the appropriate canvas asset node type for the given asset type.
    /// </summary>
    /// <param name="assetType">The type of asset to resolve a node type for.</param>
    /// <returns>The resolved node type, or null if no specific type is found.</returns>
    public Type ResolveViewType(Type assetType)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = assetType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_viewTypes[cType]);
            if (viewType != null)
            {
                return viewType;
            }

            cType = cType.BaseType;
        }


        return null;
    }


    private Type ResolveMultipleType(IEnumerable<Type> types)
    {
        if (types.CountOne())
        {
            return types.First();
        }

        Type overrideType = types.FirstOrDefault(o => o.HasAttributeCached<RequestOverrideAttribute>());
        return overrideType ?? types.FirstOrDefault();
    }
}
