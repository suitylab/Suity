using Suity.Helpers;
using System;

namespace Suity;

/// <summary>
/// A custom attribute that can be applied to any target to specify display text and optional icon.
/// This attribute is not inheritable and can only be applied once to a target.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DisplayTextAttribute : Attribute
{
    /// <summary>
    /// Gets the display text associated with the target element.
    /// </summary>
    public string DisplayText { get; }
    
    /// <summary>
    /// Gets the icon associated with the target element (optional).
    /// </summary>
    public string Icon { get; }

    /// <summary>
    /// Initializes a new instance of the DisplayTextAttribute class with display text.
    /// </summary>
    /// <param name="displayText">The text to display for the target element.</param>
    public DisplayTextAttribute(string displayText)
    {
        DisplayText = displayText ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the DisplayTextAttribute class with display text and icon.
    /// </summary>
    /// <param name="displayText">The text to display for the target element.</param>
    /// <param name="icon">The icon to display for the target element.</param>
    public DisplayTextAttribute(string displayText, string icon)
    {
        DisplayText = displayText ?? string.Empty;
        Icon = icon;
    }
}


/// <summary>
/// Custom attribute that provides preview text for elements it's applied to.
/// This attribute can be used to display tooltips or preview information in various UI contexts.
/// </summary>
/// <remarks>
/// - The attribute is not inheritable (Inherited = false)
/// - Can only be applied once to a target (AllowMultiple = false)
/// - Can be applied to any code element (AttributeTargets.All)
/// </remarks>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class PreviewTextAttribute : Attribute
{
    /// <summary>
    /// Gets the preview text to be displayed.
    /// </summary>
    public string PreviewText { get; }

    /// <summary>
    /// Initializes a new instance of the PreviewTextAttribute class.
    /// </summary>
    /// <param name="toolTips">The preview text to be displayed. If null, will be set to empty string.</param>
    public PreviewTextAttribute(string toolTips)
    {
        PreviewText = toolTips ?? string.Empty;
    }
}


/// <summary>
/// Custom attribute that provides tooltip text for elements it's applied to.
/// This attribute can be applied to any code element (targets all) and is not inheritable.
/// It cannot be applied multiple times to the same element.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class ToolTipsTextAttribute : Attribute
{
    /// <summary>
    /// Gets the tooltip text associated with the element this attribute is applied to.
    /// </summary>
    public string ToolTips { get; }

    /// <summary>
    /// Initializes a new instance of the ToolTipsTextAttribute class with the specified tooltip text.
    /// </summary>
    /// <param name="toolTips">The tooltip text to be associated with the element.</param>
    public ToolTipsTextAttribute(string toolTips)
    {
        // Assign the tooltip text, using empty string if null is provided
        ToolTips = toolTips ?? string.Empty;
    }
}


/// <summary>
/// Custom attribute that can be applied to elements to specify display category information.
/// This attribute is not inherited and can only be applied once to a target.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DisplayCategoryAttribute : Attribute
{
    /// <summary>
    /// Gets the category name for display purposes.
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// Gets the icon associated with the category.
    /// </summary>
    public string Icon { get; }

    /// <summary>
    /// Initializes a new instance of the DisplayCategoryAttribute class with a category name.
    /// </summary>
    /// <param name="category">The name of the category.</param>
    public DisplayCategoryAttribute(string category)
    {
        Category = category ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the DisplayCategoryAttribute class with a category name and icon.
    /// </summary>
    /// <param name="category">The name of the category.</param>
    /// <param name="icon">The icon associated with the category.</param>
    public DisplayCategoryAttribute(string category, string icon)
    {
        Category = category ?? string.Empty;
        Icon = icon;
    }
}


/// <summary>
/// Custom attribute that specifies the display order of elements.
/// This attribute can be applied to any target (AttributeTargets.All) and is not inheritable.
/// It cannot be applied multiple times to the same target.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DisplayOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the display order value.
    /// Higher values indicate higher priority (appear earlier in display).
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a new instance of the DisplayOrderAttribute class.
    /// </summary>
    /// <param name="order">The display order value. Higher values appear first.</param>
    public DisplayOrderAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Compares two types based on their display order.
    /// </summary>
    /// <param name="a">The first type.</param>
    /// <param name="b">The second type.</param>
    /// <returns>A comparison result.</returns>
    public static int Compare(Type a, Type b)
    {
        var aOrder = a.GetAttributeCached<DisplayOrderAttribute>()?.Order ?? 0;
        var bOrder = b.GetAttributeCached<DisplayOrderAttribute>()?.Order ?? 0;

        return -aOrder.CompareTo(bOrder);
    }

    /// <summary>
    /// Compares two DisplayOrderAttribute instances.
    /// </summary>
    /// <param name="a">The first attribute.</param>
    /// <param name="b">The second attribute.</param>
    /// <returns>A comparison result.</returns>
    public static int Compare(DisplayOrderAttribute a, DisplayOrderAttribute b)
    {
        if (a is null && b is null)
        {
            return 0;
        }

        if (a is null)
        {
            return 1;
        }

        if (b is null)
        {
            return -1;
        }

        return a.Order.CompareTo(b.Order);
    }
}

/// <summary>
/// Marks a class as not available.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NotAvailableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the NotAvailableAttribute class.
    /// </summary>
    public NotAvailableAttribute()
    {
    }
}

/// <summary>
/// Marks a class as non-preferred.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NonPreferredAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the NonPreferredAttribute class.
    /// </summary>
    public NonPreferredAttribute()
    {
    }
}