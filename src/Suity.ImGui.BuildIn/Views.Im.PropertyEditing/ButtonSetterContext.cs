using Suity.Synchonizing;
using System;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides a synchronization context and setter context for button property operations
/// within the ImGui property grid.
/// </summary>
public class ButtonSetterContext : ISyncContext, ISetterContext
{
    readonly PropertyTarget _target;
    readonly ImGuiNode _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonSetterContext"/> class.
    /// </summary>
    /// <param name="target">The property target associated with the button.</param>
    /// <param name="node">The ImGui node representing the button in the UI.</param>
    public ButtonSetterContext(PropertyTarget target, ImGuiNode node)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <inheritdoc/>
    public object? Parent => _target?.Parent?.GetValues()?.ToArray() ?? [];

    /// <inheritdoc/>
    public object? GetService(Type serviceType)
    {
        return _target.GetServiceInHierarchy(serviceType) ?? _node.GetValueInHierarchy(serviceType);
    }
}
