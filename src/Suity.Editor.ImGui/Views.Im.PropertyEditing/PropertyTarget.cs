using Suity.Drawing;
using Suity.Editor;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Delegate for converting property target values to a different representation.
/// </summary>
/// <param name="target">The property target to convert.</param>
/// <returns>A sequence of converted objects.</returns>
public delegate IEnumerable<object?> TargetConversion(PropertyTarget target);

/// <summary>
/// Delegate for reverting converted values back to the original property target representation.
/// </summary>
/// <param name="target">The property target to revert values for.</param>
/// <param name="values">The converted values to revert.</param>
/// <returns>A sequence of reverted objects.</returns>
public delegate IEnumerable<object?> TargetRevertConversion(PropertyTarget target, IEnumerable<object?> values);

/// <summary>
/// Property target for property editor
/// </summary>
public abstract class PropertyTarget : IValueTarget, ISupportStyle, IDrawContext
{
    #region Parenting

    /// <summary>
    /// Is top-level node
    /// </summary>
    public virtual bool IsRoot => false;

    /// <summary>
    /// Parent property target
    /// </summary>
    public abstract PropertyTarget? Parent { get; set; }

    /// <summary>
    /// Gets the root property target in the hierarchy.
    /// </summary>
    public PropertyTarget? Root
    {
        get
        {
            PropertyTarget? current = this;
            while (current?.Parent is not null)
            {
                current = current.Parent;
            }

            return current;
        }
    }

    #endregion

    #region Function

    /// <summary>
    /// Cached row function
    /// </summary>
    public PropertyRowFunction? RowFunction { get; internal set; }

    /// <summary>
    /// Cached editor function
    /// </summary>
    public PropertyEditorFunction? EditorFunction { get; internal set; }

    #endregion

    #region Array

    /// <summary>
    /// Array property target
    /// </summary>
    public abstract ArrayTarget? ArrayTarget { get; }

    /// <summary>
    /// Index of this property target within its parent array, if applicable.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Sets up this property target as an array element with the specified handler.
    /// </summary>
    /// <param name="handler">The array handler to use for setup.</param>
    internal abstract void SetupArray(ArrayHandler handler);

    #endregion

    #region Expand

    /// <summary>
    /// Indicates whether this property target can be expanded to show child properties.
    /// </summary>
    public bool CanExpand { get; set; } = true;

    private bool _initExpanded;

    /// <summary>
    /// Indicates whether to expand initially
    /// </summary>
    public bool? InitExpanded
    {
        get => CanExpand ? _initExpanded : null;
        set
        {
            if (value is { } v)
            {
                if (_initExpanded != v)
                {
                    _initExpanded = v;
                }
            }
        }
    }

    /// <summary>
    /// Request to expand nodes outside the IMGUI loop
    /// </summary>
    public bool? ExpandRequest { get; set; }

    #endregion

    #region Style

    /// <summary>
    /// Current state
    /// </summary>
    public abstract TextStatus Status { get; set; }

    /// <summary>
    /// View property flags for styling and behavior configuration.
    /// </summary>
    public ViewPropertyFlags Flags { get; set; }

    /// <summary>
    /// When writing properties, write to the parent node at the same time
    /// </summary>
    public bool WriteBack
    {
        get => Flags.HasFlag(ViewPropertyFlags.WriteBack);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.WriteBack;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.WriteBack;
            }
        }
    }

    /// <summary>
    /// Readonly
    /// Since the child nodes in <see cref="PropertyGrid"/> are not real Parent relationships, ReadOnly needs to be inherited from <see cref="PropertyTarget"/>.
    /// </summary>
    public bool ReadOnly
    {
        get => Flags.HasFlag(ViewPropertyFlags.ReadOnly);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.ReadOnly;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.ReadOnly;
            }
        }
    }

    /// <summary>
    /// Indicates whether it is disabled
    /// </summary>
    public bool Disabled
    {
        get => Flags.HasFlag(ViewPropertyFlags.Disabled);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.Disabled;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.Disabled;
            }
        }
    }

    /// <summary>
    /// Optional
    /// </summary>
    public bool Optional
    {
        get => Flags.HasFlag(ViewPropertyFlags.Optional);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.Optional;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.Optional;
            }
        }
    }

    /// <summary>
    /// Represents an abstract structure, refreshing the fields when writing data
    /// </summary>
    public bool IsAbstract
    {
        get => Flags.HasFlag(ViewPropertyFlags.IsAbstract);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.IsAbstract;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.IsAbstract;
            }
        }
    }

    /// <summary>
    /// Indicates the property title is hidden
    /// </summary>
    public bool HideTitle
    {
        get => Flags.HasFlag(ViewPropertyFlags.HideTitle);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.HideTitle;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.HideTitle;
            }
        }
    }

    /// <summary>
    /// Indicates whether navigation is enabled for this property target.
    /// </summary>
    public bool Navigation
    {
        get => Flags.HasFlag(ViewPropertyFlags.Navigation);
        set
        {
            if (value)
            {
                Flags |= ViewPropertyFlags.Navigation;
            }
            else
            {
                Flags &= ~ViewPropertyFlags.Navigation;
            }
        }
    }


    /// <summary>
    /// Property description, should be localized.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon to display for this property.
    /// </summary>
    public ImageDef? Icon { get; set; }

    /// <summary>
    /// Property tooltips, should be localized.
    /// </summary>
    public string? ToolTips { get; set; }

    /// <summary>
    /// Get or set the view ID. The default value is <see cref="ViewIds.Inspector"/>
    /// </summary>
    public int ViewId { get; set; } = ViewIds.Inspector;

    /// <summary>
    /// Field gui data
    /// </summary>
    public object? FieldGuiData { get; internal set; }

    /// <summary>
    /// Property object setup cached info
    /// </summary>
    public object? ObjectSetupCache { get; set; }

    /// <summary>
    /// Service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Property path
    /// </summary>
    public object? Path { get; set; }

    /// <summary>
    /// Property path with type information
    /// </summary>
    public object? TypedPath { get; set; }

    /// <summary>
    /// Path builder function
    /// </summary>
    public Action<SyncPathBuilder>? BuildPathAction { get; set; }

    /// <summary>
    /// Cached theme data for rendering.
    /// </summary>
    public object? CachedTheme { get; set; }

    #endregion

    #region Get Set Edit

    /// <summary>
    /// Indicates whether to cache the value
    /// </summary>
    public bool CacheValues { get; set; } = true;

    /// <summary>
    /// Gets the cached values for this property target.
    /// </summary>
    /// <returns>A sequence of cached objects.</returns>
    protected virtual IEnumerable<object?> OnGetValues()
    {
        return Getter?.Invoke() ?? [];
    }

    /// <summary>
    /// Sets the values for this property target.
    /// </summary>
    /// <param name="objects">The objects to set.</param>
    /// <param name="context">The setter context.</param>
    protected virtual void OnSetValues(IEnumerable<object?> objects, ISetterContext? context)
    {
        Setter?.Invoke(objects ?? [], context);
    }

    /// <summary>
    /// Forces writing property values back to the parent node.
    /// </summary>
    /// <param name="context">The setter context for the write operation.</param>
    public abstract void DoWriteBack(ISetterContext? context);

    /// <summary>
    /// Clears any cached values from the getter.
    /// </summary>
    public abstract void ClearGetterCache();

    /// <summary>
    /// Internal getter function for retrieving property values.
    /// </summary>
    internal Func<IEnumerable<object?>>? Getter { get; set; }

    /// <summary>
    /// Internal setter function for assigning property values.
    /// </summary>
    internal Action<IEnumerable<object?>, ISetterContext?>? Setter { get; set; }

    /// <summary>
    /// Property Edited Event
    /// </summary>
    public event EventHandler? Edited;

    /// <summary>
    /// Raises the edited event and optionally writes back to the parent.
    /// </summary>
    /// <param name="context">The setter context for the edit operation.</param>
    public void RaiseEdited(ISetterContext? context)
    {
        if (WriteBack)
        {
            DoWriteBack(context);
        }

        Edited?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Field

    /// <summary>
    /// Clears all child property targets (fields).
    /// </summary>
    public abstract void ClearFields();

    /// <summary>
    /// Gets a child property target by name.
    /// </summary>
    /// <param name="name">The name of the field to retrieve.</param>
    /// <returns>The property target if found, or null.</returns>
    public abstract PropertyTarget? GetField(string name);

    /// <summary>
    /// Gets or creates a child property target by name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="creation">A function to create the property target if it doesn't exist.</param>
    /// <returns>The existing or newly created property target.</returns>
    public PropertyTarget GetOrCreateField(string name, Func<PropertyTarget> creation)
    {
        return GetOrCreateField(name, creation, out _);
    }

    /// <summary>
    /// Gets or creates a child property target by name, indicating whether it was newly created.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="creation">A function to create the property target if it doesn't exist.</param>
    /// <param name="created">True if the property target was created; false if it already existed.</param>
    /// <returns>The existing or newly created property target.</returns>
    public abstract PropertyTarget GetOrCreateField(string name, Func<PropertyTarget> creation, out bool created);

    /// <summary>
    /// Gets or creates a strongly-typed child property target.
    /// </summary>
    /// <typeparam name="TObject">The type of the parent object.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="name">The name of the field.</param>
    /// <param name="getter">A function to get the property value.</param>
    /// <param name="setter">An optional action to set the property value.</param>
    /// <param name="creationConfig">An optional action to configure the newly created property target.</param>
    /// <returns>The existing or newly created property target.</returns>
    public abstract PropertyTarget GetOrCreateField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Action<TObject, TValue, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null);

    /// <summary>
    /// Gets or creates a strongly-typed child property target with a non-generic value type.
    /// </summary>
    /// <typeparam name="TObject">The type of the parent object.</typeparam>
    /// <param name="name">The name of the field.</param>
    /// <param name="editedType">The type of the property value being edited.</param>
    /// <param name="getter">A function to get the property value.</param>
    /// <param name="setter">An optional action to set the property value.</param>
    /// <param name="creationConfig">An optional action to configure the newly created property target.</param>
    /// <returns>The existing or newly created property target.</returns>
    public abstract PropertyTarget GetOrCreateField<TObject>(
        string name,
        Type editedType,
        Func<TObject, object?> getter,
        Action<TObject, object, ISetterContext?>? setter = null,
        Action<PropertyTarget>? creationConfig = null);

    /// <summary>
    /// Gets or creates a struct field property target that returns the modified parent object.
    /// </summary>
    /// <typeparam name="TObject">The type of the parent struct object.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="name">The name of the field.</param>
    /// <param name="getter">A function to get the property value.</param>
    /// <param name="setter">An optional function to set the property value and return the modified parent.</param>
    /// <param name="creationConfig">An optional action to configure the newly created property target.</param>
    /// <returns>The existing or newly created property target.</returns>
    public abstract PropertyTarget GetOrCreateStructField<TObject, TValue>(
        string name,
        Func<TObject, TValue> getter,
        Func<TObject, TValue, ISetterContext?, TObject>? setter = null,
        Action<PropertyTarget>? creationConfig = null);

    /// <summary>
    /// Gets all child property targets (fields).
    /// </summary>
    public abstract IEnumerable<PropertyTarget> Fields { get; }

    /// <summary>
    /// Gets the number of child property targets.
    /// </summary>
    public abstract int FieldCount { get; }

    #endregion

    #region Column

    /// <summary>
    /// Whether to support multi-column co-editing
    /// </summary>
    public bool SupportMultipleColumn { get; set; } = true;

    /// <summary>
    /// Gets a property target for a specific column index in multi-column editing.
    /// </summary>
    /// <param name="index">The column index.</param>
    /// <returns>The property target for the specified column.</returns>
    public abstract PropertyTarget GetColumnTarget(int index);

    /// <summary>
    /// Creates a new property target that converts values using the provided conversion functions.
    /// </summary>
    /// <param name="convert">The conversion function to transform values.</param>
    /// <param name="revertConvert">The revert conversion function to restore original values.</param>
    /// <returns>A new converted property target.</returns>
    public abstract PropertyTarget CreateConvertedTarget(TargetConversion convert, TargetRevertConversion revertConvert);

    #endregion

    #region ITarget

    /// <summary>
    /// Gets the parent objects for this property target.
    /// </summary>
    /// <returns>A sequence of parent objects.</returns>
    public abstract IEnumerable<object?> GetParentObjects();

    #endregion

    #region IValueTarget

    private IAttributeGetter? _attributes;

    /// <inheritdoc/>
    ITarget? ITarget.Parent => Parent;


    /// <summary>
    /// Property name
    /// </summary>
    public abstract string PropertyName { get; }

    /// <summary>
    /// Property display name
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (EditorUtility.ShowAsDescription.Value && !string.IsNullOrWhiteSpace(Description))
            {
                return Description!;
            }

            return PropertyName;
        }
    }

    /// <summary>
    /// Property Id
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Property preset type
    /// </summary>
    /// <remarks>
    /// Since patterns like 'LabelValue' are virtual types with no actual value provided, 'commonType' cannot obtain the type, and the 'PresetType' mechanism needs to be introduced.
    /// </remarks>
    public Type? PresetType { get; internal set; }

    /// <summary>
    /// Cached resolved property type
    /// </summary>
    public Type? EditedType { get; internal set; }





    /// <summary>
    /// Gets or sets whether multiple values ​​are inconsistent
    /// Even in multi-column mode, colors can be used to distinguish different values.
    /// </summary>
    public virtual bool ValueMultiple { get; set; }

    /// <summary>
    /// Color override for this property target.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Design attributes
    /// </summary>
    public IAttributeGetter? Attributes
    {
        get => _attributes;
        set
        {
            _attributes = value;
        }
    }

    /// <summary>
    /// Get values
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<object?> GetValues();

    /// <summary>
    /// Set values
    /// </summary>
    /// <param name="objects"></param>
    public abstract void SetValues(IEnumerable<object?> objects, ISetterContext? context = null);

    /// <summary>
    /// Additional styls
    /// </summary>
    public INodeReader? Styles { get; set; }


    /// <summary>
    /// Indicates whether there are some errors in the current node.
    /// </summary>
    public abstract bool ErrorSelf { get; set; }

    /// <summary>
    /// Indicates whether there are some errors in the current node or its children.
    /// </summary>
    public abstract bool ErrorInHierarchy { get; }

    #endregion

    /// <summary>
    /// Get service in hierarchy
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>The service instance, or null if not found.</returns>
    public object? GetServiceInHierarchy(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        return ServiceProvider?.GetService(serviceType) ?? Parent?.GetServiceInHierarchy(serviceType);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Id ?? base.ToString();
    }

    /// <summary>
    /// Determines whether the specified value target is null or contains only null values.
    /// </summary>
    /// <param name="target">The value target to check.</param>
    /// <returns>True if the target is null or all its values are null; otherwise, false.</returns>
    public static bool IsNullOrEmpty(IValueTarget? target)
    {
        if (target is null)
        {
            return true;
        }

        if (!target.GetValues().Any())
        {
            return true;
        }

        return target.GetValues().All(o => o is null);
    }
}