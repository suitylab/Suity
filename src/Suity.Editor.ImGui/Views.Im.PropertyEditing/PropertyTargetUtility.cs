using Suity.Collections;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides utility methods for creating and manipulating property targets in the property editor.
/// </summary>
public static class PropertyTargetUtility
{
    /// <summary>
    /// External implementation that handles the actual property target operations.
    /// </summary>
    internal static PropertyTargetExternal _external;

    /// <summary>
    /// Creates a property target for a single object.
    /// </summary>
    /// <param name="obj">The object to create a property target for.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public static PropertyTarget CreatePropertyTarget(object obj)
        => _external.CreatePropertyTarget([obj]);

    /// <summary>
    /// Creates a property target for a single object, focused on a specific property.
    /// </summary>
    /// <param name="obj">The object to create a property target for.</param>
    /// <param name="propertyName">The name of the property to focus on.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public static PropertyTarget CreatePropertyTarget(object obj, string propertyName)
        => _external.CreatePropertyTarget([obj], propertyName);

    /// <summary>
    /// Creates a property target for multiple objects.
    /// </summary>
    /// <param name="objs">The collection of objects to create a property target for.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public static PropertyTarget CreatePropertyTarget(IEnumerable<object> objs)
        => _external.CreatePropertyTarget(objs);

    /// <summary>
    /// Creates a property target for multiple objects, focused on a specific property.
    /// </summary>
    /// <param name="objs">The collection of objects to create a property target for.</param>
    /// <param name="propertyName">The name of the property to focus on.</param>
    /// <returns>A new <see cref="PropertyTarget"/> instance.</returns>
    public static PropertyTarget CreatePropertyTarget(IEnumerable<object> objs, string propertyName)
        => _external.CreatePropertyTarget(objs, propertyName);

    /// <summary>
    /// Populates the property target by navigating the specified synchronization path.
    /// </summary>
    /// <param name="target">The property target to populate.</param>
    /// <param name="path">The synchronization path to navigate.</param>
    /// <param name="forceRepopulate">If true, forces repopulation even if the target is already populated.</param>
    /// <returns>The populated property target, or null if population failed.</returns>
    public static PropertyTarget? PopulatePath(this PropertyTarget target, SyncPath path, bool forceRepopulate)
        => _external.PopulatePath(target, path, forceRepopulate);

    /// <summary>
    /// Populates the properties of the target using the specified provider.
    /// </summary>
    /// <param name="target">The property target to populate properties for.</param>
    /// <param name="provider">Optional provider for ImGui property editors.</param>
    /// <returns>True if properties were successfully populated; otherwise, false.</returns>
    public static bool PopulateProperties(this PropertyTarget target, IImGuiPropertyEditorProvider? provider = null)
        => _external.PopulateProperties(target, provider);

    /// <summary>
    /// Gets the SItem field information associated with the property target.
    /// </summary>
    /// <param name="target">The property target to get field information from.</param>
    /// <returns>The SItem field information, or null if not available.</returns>
    public static object? GetSItemFieldInfomation(this PropertyTarget target)
        => _external.GetSItemFieldInfomation(target);

    /// <summary>
    /// Repairs the SItem associated with the property target.
    /// </summary>
    /// <param name="target">The property target to repair the SItem for.</param>
    /// <returns>A value action representing the repair operation, or null if repair was not needed.</returns>
    public static IValueAction? RepairSItem(this PropertyTarget target)
        => _external.RepairSItem(target);

    /// <summary>
    /// Repairs the SContainer associated with the property target.
    /// </summary>
    /// <param name="target">The property target to repair the SContainer for.</param>
    /// <returns>A value action representing the repair operation, or null if repair was not needed.</returns>
    public static IValueAction? RepairSContainer(this PropertyTarget target)
        => _external.RepairSContainer(target);

    /// <summary>
    /// Gets the text representation of the specified edit feature for the property target.
    /// </summary>
    /// <param name="target">The property target to get text from.</param>
    /// <param name="feature">The advanced edit feature to retrieve text for.</param>
    /// <returns>The text representation, or null if not available.</returns>
    public static string? GetText(this PropertyTarget target, ViewAdvancedEditFeatures feature)
        => _external.GetText(target, feature);

    /// <summary>
    /// Sets the text value for the specified edit feature on the property target.
    /// </summary>
    /// <param name="target">The property target to set text on.</param>
    /// <param name="feature">The advanced edit feature to set text for.</param>
    /// <param name="text">The text value to set.</param>
    /// <returns>A value action representing the set operation, or null if the operation was not performed.</returns>
    internal static IValueAction? SetText(this PropertyTarget target, ViewAdvancedEditFeatures feature, string text)
        => _external.SetText(target, feature, text);

    /// <summary>
    /// Sets a dynamic action type for the property target.
    /// </summary>
    /// <param name="target">The property target to set the dynamic action for.</param>
    /// <param name="dynamicType">The type to use for the dynamic action, or null to clear it.</param>
    /// <returns>A value action representing the set operation, or null if the operation was not performed.</returns>
    public static IValueAction? SetDynamicAction(this PropertyTarget target, Type? dynamicType)
        => _external.SetDynamicAction(target, dynamicType);

    /// <summary>
    /// Converts the property target to a preview path for display purposes.
    /// </summary>
    /// <param name="target">The property target to convert.</param>
    /// <returns>A <see cref="PreviewPath"/> representing the target's path.</returns>
    public static PreviewPath ToPreviewPath(this PropertyTarget target)
        => _external.ToPreviewPath(target);

    /// <summary>
    /// Gets the maximum array length across all arrays in the array target.
    /// </summary>
    /// <param name="target">The array target to get the maximum length from.</param>
    /// <returns>
    /// The maximum array length. If all arrays have the same length, returns that length.
    /// If arrays have different lengths, returns the maximum. Returns 0 if no arrays exist.
    /// </returns>
    public static int GetMaxArrayLength(this ArrayTarget target)
    {
        var innerCounts = target.GetArrayLength();
        if (innerCounts.AllEqual())
        {
            return innerCounts.First();
        }
        else if (innerCounts.Any())
        {
            return innerCounts.Max();
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the CSS input class for the property based on its value multiplicity and read-only state.
    /// </summary>
    /// <param name="target">The value target to get the input class for.</param>
    /// <returns>The CSS class name for the property input.</returns>
    public static string GetPropertyInputClass(this IValueTarget target) 
        => PropertyGridThemes.GetPropertyInputClass(target.ValueMultiple, target.ReadOnly);
}