using System.Linq;
using System;
using Suity.Editor.Types;

namespace Suity.Editor.Design;

/// <summary>
/// Provides extension methods for design objects.
/// </summary>
public static class DesignExtensions
{
    /// <summary>
    /// Gets the data usage mode from the given attribute getter.
    /// </summary>
    public static DataUsageMode GetDataUsageMode(this IAttributeGetter attributes)
    {
        return attributes.GetAttribute<DataUsageAttribute>()?.Usage ?? DataUsageMode.None;
    }

    /// <summary>
    /// Gets the data driven mode from the given struct field.
    /// </summary>
    public static DataDrivenMode GetDataDrivenMode(this DStructField field)
    {
        if (field.GetAttribute<DrivenAttribute>() is { } driven)
        {
            return driven.Mode;
        }

        var type = field.FieldType?.OriginType?.Target;
        if (type != null)
        {
            return type.GetDataDrivenMode();
        }
        else
        {
            return DataDrivenMode.None;
        }
    }

    public static DataDrivenMode GetDataDrivenMode(this IAttributeGetter attributes)
    {
        return attributes.GetAttribute<DrivenAttribute>()?.Mode ?? DataDrivenMode.None;
    }

    /// <summary>
    /// Gets whether the attribute indicates a preview field.
    /// </summary>
    public static bool GetIsPreview(this IAttributeGetter attributes)
    {
        if (attributes?.GetAttributes<PreviewFieldAttribute>().Any() == true)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets whether the attribute indicates a disabled field.
    /// </summary>
    public static bool GetIsDisabled(this IAttributeGetter attributes)
    {
        return attributes?.GetAttributes<DisabledAttribute>().Any() == true;
    }

    /// <summary>
    /// Gets whether the attribute indicates a hidden field.
    /// </summary>
    public static bool GetIsHidden(this IAttributeGetter attributes)
    {
        if (attributes?.GetAttributes<HiddenAttribute>().Any() == true)
        {
            return true;
        }

        //if (attributes?.GetAttributes<ConnectorAttribute>().Any() == true)
        //{
        //    return true;
        //}

        return attributes.GetAutoFieldType().HasValue;
    }

    /// <summary>
    /// Gets whether the attribute indicates a hidden or disabled field.
    /// </summary>
    public static bool GetIsHiddenOrDisabled(this IAttributeGetter attributes)
    {
        return attributes.GetIsHidden() || attributes.GetIsDisabled();
    }

    /// <summary>
    /// Gets the auto field type from the given attribute getter.
    /// </summary>
    public static AutoFieldType? GetAutoFieldType(this IAttributeGetter attributes)
    {
        if (attributes is null)
        {
            return null;
        }

        var autoFieldAttr = attributes.GetAttributes<AutoFieldAttribute>().FirstOrDefault();
        if (autoFieldAttr != null)
        {
            return autoFieldAttr.FieldType;
        }

        if (attributes.GetAttributes<DataIdFieldAttribute>().FirstOrDefault() != null)
        {
            return AutoFieldType.DataId;
        }

        if (attributes.GetAttributes<EditorGuidFieldAttribute>().FirstOrDefault() != null)
        {
            return AutoFieldType.Guid;
        }

        return null;
    }

    public static bool ContainsAttribute<T>(this IAttributeGetter attributes) where T : class
    {
        return attributes?.GetAttributes<T>().Any() == true;
    }

    public static bool ContainsAttribute<T>(this IAttributeGetter attributes, Predicate<T> predicate) where T : class
    {
        return attributes?.GetAttributes<T>().FirstOrDefault(o => predicate(o)) != null;
    }
}
