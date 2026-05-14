using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Selecting;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Provides extension methods for type definitions.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Creates a type definition from a DType.
    /// </summary>
    internal static TypeDefinition MakeDefinition(this DType type)
        => TypesExternal._external.MakeDefinition(type);

    /// <summary>
    /// Gets the exported type of the type.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <returns>The exported type.</returns>
    public static TypeDefinition GetExportedType(this TypeDefinition type)
    {
        if (type.GetIsStringLikeType())
        {
            return NativeTypes.StringType;
        }
        else if (type.IsArray && type.ElementType.GetIsStringLikeType())
        {
            return NativeTypes.StringType.MakeArrayType();
        }
        else
        {
            return type;
        }
    }

    /// <summary>
    /// Gets the namespace of the type.
    /// </summary>
    public static string GetNameSpace(this TypeDefinition typeInfo)
    {
        return typeInfo.Target?.NameSpace ?? string.Empty;
    }

    /// <summary>
    /// Gets the short type name.
    /// </summary>
    public static string GetShortTypeName(this TypeDefinition typeInfo, bool alias = false)
        => TypesExternal._external.GetShortTypeName(typeInfo, alias);

    /// <summary>
    /// Tries to get the short type name.
    /// </summary>
    public static bool TryGetShortTypeName(this TypeDefinition typeInfo, bool alias, out string shortTypeName)
        => TypesExternal._external.TryGetShortTypeName(typeInfo, alias, out shortTypeName);

    /// <summary>
    /// Gets the full type name text.
    /// </summary>
    public static string GetFullTypeNameText(this TypeDefinition typeInfo, bool alias = false)
        => TypesExternal._external.GetFullTypeName(typeInfo, alias);

    /// <summary>
    /// Gets the full type name.
    /// </summary>
    public static string GetFullTypeName(this TypeDefinition typeInfo, bool alias = false)
        => TypesExternal._external.GetFullTypeName(typeInfo, alias);

    /// <summary>
    /// Gets the implementations of a type.
    /// </summary>
    public static ISelectionList GetImplementationList(this TypeDefinition type, IAssetFilter filter = null)
        => TypesExternal._external.GetImplementationList(type, filter);

    /// <summary>
    /// Gets the icon for a type.
    /// </summary>
    public static ImageDef GetIcon(this TypeDefinition typeInfo)
        => TypesExternal._external.GetIcon(typeInfo);

    /// <summary>
    /// Resolves the definition for a DType.
    /// </summary>
    public static TypeDefinition ResolveDefinition(this DType type)
    {
        type.ResolveId();
        return type.Definition;
    }

    /// <summary>
    /// Determines whether the type can show in detail view.
    /// </summary>
    public static bool CanShowInDetailView(this TypeDefinition info)
    {
        return info.IsAbstract || info.IsAbstractArray || info == NativeTypes.DelegateType;
    }

    /// <summary>
    /// Determines whether the type is defined as a value struct.
    /// </summary>
    public static bool GetIsDefinedAsValueStruct(this TypeDefinition typeInfo)
    {
        if (typeInfo.Target is not DCompond pStruct)
        {
            return false;
        }

        if (pStruct.IsValueStruct)
        {
            return true;
        }

        return pStruct.Attributes.GetAttributes<ValueTypeStructAttribute>().Any();
    }

    public static bool GetIsLink(this TypeDefinition type, bool includeArray)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return false;
        }

        if (type.IsLink)
        {
            return true;
        }

        if (includeArray && type.IsArray)
        {
            return type.ElementType.GetIsLink(false);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the type is a data link.
    /// </summary>
    public static bool GetIsDataLink(this TypeDefinition type, bool includeArray)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return false;
        }

        if (type.IsDataLink)
        {
            return true;
        }

        if (includeArray && type.IsArray)
        {
            return type.ElementType.GetIsDataLink(false);
        }
        else
        {
            return false;
        }
    }

    public static bool GetIsNormalDataLink(this TypeDefinition type, bool includeArray)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return false;
        }

        if (type.IsDataLink && type.ElementType.IsStruct)
        {
            return true;
        }

        if (includeArray && type.IsArray)
        {
            return type.ElementType.GetIsNormalDataLink(false);
        }
        else
        {
            return false;
        }
    }

    public static bool GetIsAbstractDataLink(this TypeDefinition type, bool includeArray)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return false;
        }

        if (type.IsDataLink && type.ElementType.IsAbstractStruct)
        {
            return true;
        }

        if (includeArray && type.IsArray)
        {
            return type.ElementType.GetIsAbstractDataLink(false);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the type is string-like.
    /// </summary>
    public static bool GetIsStringLikeType(this TypeDefinition type) => type.IsLink || type == NativeTypes.TextBlockType;

    /// <summary>
    /// Checks if the field is public.
    /// </summary>
    internal static T CheckPublic<T>(this T field) where T : DField => (field?.AccessMode == AssetAccessMode.Public) ? field : null;

    /// <summary>
    /// Gets the documentation for a DType.
    /// </summary>
    public static string GetDocumentation(this DType type) => GetDocumentation(type.Description, type.ToolTips);

    /// <summary>
    /// Gets the documentation for a DTypeFamily.
    /// </summary>
    public static string GetDocumentation(this DTypeFamily family) => GetDocumentation(family.Description, family.ToolTips);

    /// <summary>
    /// Gets the documentation for a DField.
    /// </summary>
    public static string GetDocumentation(this DField field) => GetDocumentation(field.Description, field.ToolTips);

    /// <summary>
    /// Combines description and tooltips into documentation.
    /// </summary>
    private static string GetDocumentation(string description, string tooltips)
    {
        if (!string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(tooltips))
        {
            return $"{description}\r\n{tooltips}";
        }

        if (!string.IsNullOrWhiteSpace(tooltips))
        {
            return tooltips;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            return description;
        }

        return null;
    }
}