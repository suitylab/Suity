using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Resolves the appropriate diagram item type for a given flow node type.
/// </summary>
internal sealed class FlowDiagramItemResolver
{
    /// <summary>
    /// Gets the singleton instance of the resolver.
    /// </summary>
    public static readonly FlowDiagramItemResolver Instance = new();

    private static readonly UniqueMultiDictionary<Type, Type> _nodeTypes = new();
    private static bool _init;

    private FlowDiagramItemResolver()
    {
    }

    /// <summary>
    /// Initializes the resolver by scanning all derived types of <see cref="FlowDiagramItem{T}"/>.
    /// </summary>
    private void Initialize()
    {
        foreach (Type itemType in typeof(FlowDiagramItem<>).GetDerivedTypes())
        {
            var nodeType = itemType.BaseType.GetGenericArguments()[0];

            _nodeTypes.Add(nodeType, itemType);
        }

        _init = true;
    }

    /// <summary>
    /// Creates a diagram item for the specified flow node.
    /// </summary>
    /// <param name="node">The flow node to create a diagram item for.</param>
    /// <returns>A new <see cref="FlowDiagramItem"/> instance wrapping the node.</returns>
    internal FlowDiagramItem CreateNode(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (!_init)
        {
            Initialize();
        }

        Type viewType = ResolveDiagramItemType(node.GetType());
        if (viewType != null)
        {
            var item = (FlowDiagramItem)viewType.CreateInstanceOf();
            item.Node = node;

            return item;
        }
        else
        {
            return new FlowDiagramItem(node);
        }
    }

    public Type ResolveDiagramItemType(Type assetType)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = assetType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_nodeTypes[cType]);
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
