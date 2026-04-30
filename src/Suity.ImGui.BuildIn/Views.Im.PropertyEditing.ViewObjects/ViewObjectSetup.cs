using Suity.Editor;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Base implementation of <see cref="IViewObjectSetup"/> that manages child property targets for view objects.
/// </summary>
/// <param name="target">The property target this setup belongs to.</param>
internal abstract class BaseViewObjectSetup(PropertyTarget target) : MarshalByRefObject, IViewObjectSetup
{
    private readonly PropertyTarget _target = target ?? throw new ArgumentNullException(nameof(target));
    private readonly List<PropertyTarget> _childTargets = [];

    /// <inheritdoc/>
    public override object? InitializeLifetimeService()
    {
        return null;
    }

    /// <summary>
    /// Gets the property target this setup belongs to.
    /// </summary>
    public PropertyTarget Target => _target;

    /// <summary>
    /// Clears all child targets from this setup.
    /// </summary>
    public void Clear()
    {
        _childTargets.Clear();
    }

    /// <summary>
    /// Gets the collection of child property targets.
    /// </summary>
    public IEnumerable<PropertyTarget> ChildTargets => _childTargets;

    /// <summary>
    /// Gets the number of child property targets.
    /// </summary>
    public int Count => _childTargets.Count;

    #region IViewObjectSetup

    /// <inheritdoc/>
    public object? Parent => _target;

    /// <inheritdoc/>
    public void AddField(Type type, ViewProperty property)
    {
        // Filter non-view IDs
        if (_target.ViewId != 0 && property.ViewId != 0 && property.ViewId != _target.ViewId)
        {
            return;
        }

        // Filter connector ports
        if (property.IsConnector && _target.Styles?.GetAttribute(ViewProperty.HideConnectorAttribute) != null)
        {
            return;
        }

        var childTarget = GetOrCreateChildTarget(type, property);

        childTarget.Description = L(property.Description);

        // Enabled and ReadOnly have inheritance; check both self and parent

        childTarget.Disabled = property.Disabled || _target.Disabled;
        childTarget.ReadOnly = property.ReadOnly || _target.ReadOnly;
        childTarget.Optional = property.Optional; // Does not inherit from parent
        childTarget.Styles = property.Styles;
        // Only accept expand setting; otherwise, it will affect internal expand operations
        if (property.Expand)
        {
            childTarget.InitExpanded = true;
        }
        childTarget.Status = property.Status;
        childTarget.Icon = property.Icon != null ? EditorUtility.GetIcon(property.Icon) : null;
        childTarget.Color = property.Color;
        childTarget.WriteBack = property.WriteBack;
        childTarget.IsAbstract = property.IsAbstract;
        childTarget.Navigation = property.Navigation;

        childTarget.HideTitle = property.HideTitle;

        childTarget.ToolTips = L(property.Styles?.GetToolTip());
        childTarget.ToolTips ??= L(type.ToToolTipsText());

        childTarget.Attributes = property.Attributes;

        _childTargets.Add(childTarget);
    }

    /// <inheritdoc/>
    public IEnumerable<object> GetObjects()
    {
        return _target.GetValues().ToArray()!;
    }

    /// <inheritdoc/>
    public virtual object? GetService(Type serviceType)
    {
        return _target.ServiceProvider?.GetService(serviceType);
    }

    /// <inheritdoc/>
    public bool IsTypeSupported(Type type)
    {
        return true;
    }

    /// <inheritdoc/>
    public bool IsViewIdSupported(int viewId)
    {
        return _target.ViewId == 0 || viewId == 0 || viewId == _target.ViewId;
    }

    /// <inheritdoc/>
    public INodeReader? Styles => _target.Styles;

    #endregion

    /// <summary>
    /// Creates or retrieves a child property target for the specified type and view property.
    /// </summary>
    /// <param name="type">The type of the child property.</param>
    /// <param name="property">The view property definition.</param>
    /// <returns>The created or existing child property target.</returns>
    protected abstract PropertyTarget GetOrCreateChildTarget(Type type, ViewProperty property);
}

/// <summary>
/// View object setup implementation for <see cref="IViewObject"/> types.
/// </summary>
/// <param name="target">The property target this setup belongs to.</param>
internal class ViewObjectSetup(PropertyTarget target) : BaseViewObjectSetup(target)
{

    /// <inheritdoc/>
    protected override PropertyTarget GetOrCreateChildTarget(Type type, ViewProperty property)
    {
        var childTarget = Target.GetOrCreateField<IViewObject>(
            property.Name,
            type,
            o => o.GetProperty(property.Name),
            (o, v, ctx) => o.SetProperty(property.Name, v, ctx as ISyncContext ?? this));

        childTarget.CanExpand = childTarget.GetValues().Any(o => o != null);

        return childTarget;
    }
}

/// <summary>
/// View object setup implementation for <see cref="SObject"/> types.
/// </summary>
/// <param name="target">The property target this setup belongs to.</param>
internal class SObjectSetup(PropertyTarget target) : BaseViewObjectSetup(target)
{
    /// <inheritdoc/>
    protected override PropertyTarget GetOrCreateChildTarget(Type type, ViewProperty property)
    {
        var childTarget = Target.GetOrCreateField<SObject>(
            property.Name,
            type,
            o => o.GetItemFormatted(property.Name),
            (o, v, ctx) => o.SetProperty(property.Name, v));

        childTarget.CanExpand = childTarget.GetValues().Any(o =>
        {
            if (o is null)
            {
                return false;
            }

            if (o is SObject obj && TypeDefinition.IsNullOrEmpty(obj.ObjectType))
            {
                return false;
            }

            return true;
        });

        return childTarget;
    }
}