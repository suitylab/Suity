using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Editor.Types;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Manages and resolves graph data types for flow diagram connectors.
/// Caches types by <see cref="TypeDefinition"/> and <see cref="IFlowDataStyle"/> for efficient lookup.
/// </summary>
public class FlowTypeManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="FlowTypeManager"/>.
    /// </summary>
    public static FlowTypeManager Instance { get; } = new();

    private readonly Dictionary<TypeDefinition, TypeDefinitionDataType> _typesByType = [];
    private readonly Dictionary<IFlowDataStyle, StyledDataType> _typesByStyle = [];

    /// <summary>
    /// Gets the graph data type for the specified type name.
    /// </summary>
    /// <param name="name">The type name to resolve.</param>
    /// <returns>The resolved <see cref="GraphDataType"/>, or <see cref="UnknownNodeGraphDataType.Instance"/> if not found.</returns>
    public GraphDataType GetDataType(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return UnknownNodeGraphDataType.Instance;
        }

        var type = TypeDefinition.Resolve(name);
        if (!TypeDefinition.IsNullOrBroken(type))
        {
            return _typesByType.GetOrAdd(type, _ => new TypeDefinitionDataType(type));
        }

        if (CustomGraphDataType.GetCustomDataType(name) is CustomGraphDataType customType)
        {
            return customType;
        }

        return UnknownNodeGraphDataType.Instance;
    }

    /// <summary>
    /// Gets the graph data type for the specified flow data style.
    /// </summary>
    /// <param name="style">The flow data style to resolve.</param>
    /// <returns>The resolved <see cref="GraphDataType"/>, or <see cref="UnknownNodeGraphDataType.Instance"/> if style is null.</returns>
    public GraphDataType GetDataType(IFlowDataStyle style)
    {
        if (style is null)
        {
            return UnknownNodeGraphDataType.Instance;
        }

        return _typesByStyle.GetOrAdd(style, _ => new(style));
    }
}