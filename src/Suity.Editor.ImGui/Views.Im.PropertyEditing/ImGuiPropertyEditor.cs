using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Abstract base class for property editors in ImGui system
/// </summary>
public abstract class ImGuiPropertyEditor
{
    /// <summary>
    /// Internal constructor to prevent direct instantiation of this abstract class
    /// </summary>
    internal ImGuiPropertyEditor()
    { }

    /// <summary>
    /// Gets the type that this editor can handle
    /// </summary>
    public abstract Type EditedType { get; }

    /// <summary>
    /// Populates the editor with functions (virtual method with no implementation)
    /// </summary>
    /// <param name="target">The property target to populate</param>
    public virtual void PopulateFunction(PropertyTarget target)
    {
    }

    /// <summary>
    /// Creates a property row in the ImGui interface
    /// </summary>
    /// <param name="gui">The ImGui interface instance</param>
    /// <param name="target">The property target to display</param>
    /// <param name="rowAction">Optional action to perform on the row</param>
    /// <returns>An ImGuiNode representing the created property row, or null if not created</returns>
    public virtual ImGuiNode? RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        return gui.PropertyRow(target, EditorFunction, rowAction);
    }

    /// <summary>
    /// Creates the editor UI for the property
    /// </summary>
    /// <param name="gui">The ImGui interface instance</param>
    /// <param name="target">The value target to edit</param>
    /// <param name="handler">Action handler for value changes</param>
    /// <returns>An ImGuiNode representing the editor, or null if not created</returns>
    public virtual ImGuiNode? EditorFunction(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return null;
    }
}


/// <summary>
/// Abstract base class for property editors that work with a specific type T.
/// This class extends ImGuiPropertyEditor and provides type-specific implementation.
/// </summary>
/// <typeparam name="T">The type of property that this editor can handle</typeparam>
public abstract class ImGuiPropertyEditor<T> : ImGuiPropertyEditor
{
    /// <summary>
    /// Gets the type that this property editor can handle.
    /// This property is sealed and overrides the base class implementation.
    /// </summary>
    /// <returns>The type T that this editor is designed for</returns>
    public override sealed Type EditedType => typeof(T);
}


/// <summary>
/// Abstract base class for ImGui property fields
/// </summary>
public abstract class ImGuiPropertyField
{
    /// <summary>
    /// Gets the name of the property field
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Called when the GUI is rendered for this property field
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering</param>
    /// <param name="target">The target property to be displayed</param>
    /// <returns>The ImGui node that was created, or null</returns>
    public abstract ImGuiNode? OnGui(ImGui gui, PropertyTarget target);

    /// <summary>
    /// Internal constructor for the abstract class
    /// </summary>
    internal ImGuiPropertyField()
    {
    }
}


internal sealed class ImGuiPropertyField<T, TValue> : ImGuiPropertyField
{
    public ImGuiPropertyEditor<T> Editor { get; }
    public override string Name { get; }
    public Func<T, TValue> Getter { get; }
    public Action<T, TValue, ISetterContext?>? Setter { get; }

    internal ImGuiPropertyField(ImGuiPropertyEditor<T> editor, string name, Func<T, TValue> getter, Action<T, TValue, ISetterContext?>? setter = null)
    {
        Editor = editor ?? throw new ArgumentNullException(nameof(editor));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        Setter = setter;
    }

    public override ImGuiNode? OnGui(ImGui gui, PropertyTarget target)
    {
        var fieldTarget = target.GetOrCreateField<T, TValue>(Name, Getter, Setter);
        return gui.PropertyField(fieldTarget);
    }
}

/// <summary>
/// Abstract base class for grouped property editors in ImGui
/// Implements a generic editor that can handle properties of type T
/// </summary>
/// <typeparam name="T">The type of object being edited</typeparam>
public abstract class ImGuiGroupedPropertyEditor<T> : ImGuiPropertyEditor<T>
{
    /// <summary>
    /// Creates a new property field for editing values of type TValue
    /// </summary>
    /// <typeparam name="TValue">The type of value being edited</typeparam>
    /// <param name="name">The display name of the property</param>
    /// <param name="getter">Function to get the current value</param>
    /// <param name="setter">Optional function to set the value</param>
    /// <returns>A new ImGuiPropertyField instance</returns>
    protected ImGuiPropertyField CreateField<TValue>(string name, Func<T, TValue> getter, Action<T, TValue, ISetterContext?>? setter = null)
    {
        return new ImGuiPropertyField<T, TValue>(this, name, getter, setter);
    }

    /// <summary>
    /// Overrides the base RowFunction to implement grouped property display logic
    /// </summary>
    /// <param name="gui">The ImGui instance</param>
    /// <param name="target">The property target being edited</param>
    /// <param name="rowAction">Optional action to perform on the row</param>
    /// <returns>ImGuiNode if property is grouped, null otherwise</returns>
    public override sealed ImGuiNode? RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        // Check if the target type can be assigned to T
        if (!typeof(T).IsAssignableFrom(target.EditedType))
        {
            return null;
        }

        // Handle root properties differently from grouped properties
        if (target.IsRoot)
        {
            RowFunctionInner(gui, target, rowAction);

            return null;
        }
        else
        {
            // Create a property group that expands to show inner properties
            var node = gui.PropertyGroup(target).OnPropertyGroupExpand(() =>
            {
                RowFunctionInner(gui, target, rowAction);
            });

            return node;
        }
    }

    /// <summary>
    /// Virtual method to be implemented by derived classes
    /// Handles the actual property row creation for root and expanded groups
    /// </summary>
    /// <param name="gui">The ImGui instance</param>
    /// <param name="target">The property target being edited</param>
    /// <param name="rowAction">Optional action to perform on the row</param>
    protected virtual void RowFunctionInner(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
    }
}
