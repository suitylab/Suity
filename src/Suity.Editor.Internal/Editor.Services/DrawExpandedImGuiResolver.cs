using Suity.Editor.Flows;
using Suity.Reflecting;
using Suity.Views.Im;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Resolves custom expanded ImGui views for flow node types based on attributes.
/// </summary>
public sealed class DrawExpandedImGuiResolver 
    : BaseServiceTypeResolver<IDrawExpandedImGui, FlowExpandedViewUsageAttribute, DefaultFlowExpandedViewAttribute>
{
    /// <summary>
    /// Singleton instance of the resolver.
    /// </summary>
    public static readonly DrawExpandedImGuiResolver Instance = new();

    private DrawExpandedImGuiResolver()
        : base("flow expanded view")
    {
    }

    /// <summary>
    /// Creates an expanded ImGui view instance for the specified object type.
    /// </summary>
    /// <param name="objectType">The object type to create a view for.</param>
    /// <returns>The created view, or null if no matching view type is found.</returns>
    public IDrawExpandedImGui CreateView(Type objectType)
    {
        if (objectType is null)
        {
            return null;
        }

        if (!IsInitialized)
        {
            Initialize();
        }

        Type viewType = ResolveServiceType(objectType);
        if (viewType != null)
        {
            return (IDrawExpandedImGui)viewType.CreateInstanceOf();
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    protected override Type GetTargetType(FlowExpandedViewUsageAttribute attribute)
    {
        return attribute.ObjectType;
    }
}
