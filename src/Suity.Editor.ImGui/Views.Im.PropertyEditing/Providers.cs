using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Delegate for populating property details for a given target.
/// </summary>
/// <param name="target">The property target to populate.</param>
public delegate void PropertyPopulateFunction(PropertyTarget target);

/// <summary>
/// Delegate for creating a property row node in the ImGui property editor.
/// </summary>
/// <param name="gui">The ImGui context.</param>
/// <param name="target">The property target to render.</param>
/// <param name="action">Optional row action to perform.</param>
/// <returns>An ImGui node representing the property row, or null if no row should be created.</returns>
public delegate ImGuiNode? PropertyRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? action);

/// <summary>
/// Delegate for creating a property editor UI for a given value target.
/// </summary>
/// <param name="gui">The ImGui context.</param>
/// <param name="target">The value target to edit.</param>
/// <param name="handler">Callback handler for value actions.</param>
/// <returns>An ImGui node representing the property editor, or null if no editor should be created.</returns>
public delegate ImGuiNode? PropertyEditorFunction(ImGui gui, IValueTarget target, Action<IValueAction> handler);

/// <summary>
/// Provides property editor functions for ImGui-based property editing.
/// Implement this interface to supply custom populate, row, and editor functions for specific types.
/// </summary>
public interface IImGuiPropertyEditorProvider
{
    /// <summary>
    /// Gets a function that populates property details for the specified type.
    /// </summary>
    /// <param name="commonType">Common base class for all types.</param>
    /// <param name="presetType">Pre-assigned preset types.</param>
    /// <returns>A populate function if one is available for the type; otherwise, null.</returns>
    PropertyPopulateFunction? GetPopulateFunction(Type commonType, Type? presetType);

    /// <summary>
    /// Gets a function that creates a property row node for the specified type.
    /// </summary>
    /// <param name="commonType">Common base class for all types.</param>
    /// <param name="presetType">Pre-assigned preset types.</param>
    /// <returns>A row function if one is available for the type; otherwise, null.</returns>
    PropertyRowFunction? GetRowFunction(Type commonType, Type? presetType);

    /// <summary>
    /// Gets a function that creates a property editor UI for the specified type.
    /// </summary>
    /// <param name="commonType">Common base class for all types.</param>
    /// <param name="presetType">Pre-assigned preset types.</param>
    /// <returns>An editor function if one is available for the type; otherwise, null.</returns>
    PropertyEditorFunction? GetEditorFunction(Type commonType, Type? presetType);

    /// <summary>
    /// Gets an array handler for the specified property target.
    /// </summary>
    /// <param name="target">The property target to handle.</param>
    /// <returns>An array handler if one is available; otherwise, null.</returns>
    ArrayHandler? GetArrayHandler(PropertyTarget target);
}

/// <summary>
/// Default implementation of <see cref="IImGuiPropertyEditorProvider"/> that allows
/// registering custom populate, row, and editor functions for specific types.
/// </summary>
public class ImGuiPropertyEditorProvider : IImGuiPropertyEditorProvider
{
    private readonly Dictionary<Type, PropertyPopulateFunction> _expands = [];
    private readonly Dictionary<Type, PropertyRowFunction> _rows = [];
    private readonly Dictionary<Type, PropertyEditorFunction> _editors = [];

    /// <summary>
    /// Registers a populate function for the specified type.
    /// </summary>
    /// <param name="editedType">The type to associate the populate function with.</param>
    /// <param name="func">The populate function to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editedType"/> or <paramref name="func"/> is null.</exception>
    public void RegisterPopulateFunction(Type editedType, PropertyPopulateFunction func)
    {
        if (editedType is null)
        {
            throw new ArgumentNullException(nameof(editedType));
        }

        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        _expands.Add(editedType, func);
    }

    /// <summary>
    /// Registers a row function for the specified type.
    /// </summary>
    /// <param name="editedType">The type to associate the row function with.</param>
    /// <param name="func">The row function to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editedType"/> or <paramref name="func"/> is null.</exception>
    public void RegisterRowFunction(Type editedType, PropertyRowFunction func)
    {
        if (editedType is null)
        {
            throw new ArgumentNullException(nameof(editedType));
        }

        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        _rows.Add(editedType, func);
    }

    /// <summary>
    /// Registers an editor function for the specified type.
    /// </summary>
    /// <param name="editedType">The type to associate the editor function with.</param>
    /// <param name="func">The editor function to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editedType"/> or <paramref name="func"/> is null.</exception>
    public void RegisterEditorFunction(Type editedType, PropertyEditorFunction func)
    {
        if (editedType is null)
        {
            throw new ArgumentNullException(nameof(editedType));
        }

        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        _editors.Add(editedType, func);
    }

    /// <inheritdoc/>
    public PropertyPopulateFunction? GetPopulateFunction(Type commonType, Type? presetType)
    {
        return _expands.GetValueSafe(commonType) ?? _expands.GetValueSafe(presetType!);
    }

    /// <inheritdoc/>
    public PropertyRowFunction? GetRowFunction(Type commonType, Type? presetType)
    {
        return _rows.GetValueSafe(commonType) ?? _rows.GetValueSafe(presetType!);
    }

    /// <inheritdoc/>
    public PropertyEditorFunction? GetEditorFunction(Type commonType, Type? presetType)
    {
        return _editors.GetValueSafe(commonType) ?? _editors.GetValueSafe(presetType!);
    }

    /// <inheritdoc/>
    public ArrayHandler? GetArrayHandler(PropertyTarget target)
    {
        return null;
    }
}

/// <summary>
/// Represents a custom property editor that provides its own row rendering function.
/// Implement this interface on types that need custom ImGui property row rendering.
/// </summary>
public interface IImGuiCustomPropertyEditor
{
    /// <summary>
    /// Gets the row function used to render this property editor.
    /// </summary>
    /// <returns>A row function for rendering the property row.</returns>
    PropertyRowFunction GetRowFunction();
}