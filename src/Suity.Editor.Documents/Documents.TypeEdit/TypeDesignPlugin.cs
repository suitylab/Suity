using Suity.Editor.Design;
using Suity.Helpers;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Plugin for type design functionality, manages available design item types.
/// </summary>
public class TypeDesignPlugin : EditorPlugin
{
    /// <summary>
    /// Gets the description of this plugin.
    /// </summary>
    public override string Description => "Type Design";

    /// <summary>
    /// Gets the list of available design item types sorted by display order.
    /// </summary>
    public List<Type> DesignItemTypes { get; } = [];

    /// <summary>
    /// Called when the plugin is initialized. Populates and sorts the design item types.
    /// </summary>
    /// <param name="context">The plugin context.</param>
    protected override void Awake(PluginContext context)
    {
        base.Awake(context);

        DesignItemTypes.Clear();
        DesignItemTypes.AddRange(typeof(TypeDesignItem).GetAvailableClassTypes());
        DesignItemTypes.Sort((a, b) =>
        {
            int orderA = a.GetAttributeCached<DisplayOrderAttribute>()?.Order ?? 0;
            int orderB = b.GetAttributeCached<DisplayOrderAttribute>()?.Order ?? 0;
            return orderA.CompareTo(orderB);
        });
    }
}
