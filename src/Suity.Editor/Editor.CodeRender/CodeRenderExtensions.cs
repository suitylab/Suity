using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Syntax property extensions.
/// </summary>
public static class CodeRenderExtensions
{
    /// <summary>
    /// Gets the name of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The name.</returns>
    public static string GetName(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.NameProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the short name of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The short name.</returns>
    public static string GetShortName(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.NameProperty, null) as string ?? string.Empty;
    }


    /// <summary>
    /// Gets the full name of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The full name.</returns>
    public static string GetFullName(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.FullNameProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the full type name of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The full type name.</returns>
    public static string GetFullTypeName(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.FullTypeNameProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the type info of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetTypeInfo(this ICodeRenderElement element)
    {
        object obj = element.GetProperty(CodeRenderProperty.TypeInfoProperty, null);
        return ResolveType(obj);
    }

    /// <summary>
    /// Gets the base type info of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetBaseTypeInfo(this ICodeRenderElement element)
    {
        object obj = element.GetProperty(CodeRenderProperty.BaseTypeInfoProperty, null);
        return ResolveType(obj);
    }

    /// <summary>
    /// Gets the base enum type info of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetBaseEnumTypeInfo(this ICodeRenderElement element)
    {
        object obj = element.GetProperty(CodeRenderProperty.BaseEnumTypeInfoProperty, null);
        return ResolveType(obj);
    }

    /// <summary>
    /// Gets the base value type info of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetBaseValueTypeInfo(this ICodeRenderElement element)
    {
        object obj = element.GetProperty(CodeRenderProperty.BaseValueTypeInfoProperty, null);
        return ResolveType(obj);
    }

    /// <summary>
    /// Gets the return type info of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetReturnTypeInfo(this ICodeRenderElement element)
    {
        object obj = element.GetProperty(CodeRenderProperty.ReturnTypeInfoProperty, null);
        return ResolveType(obj);
    }

    /// <summary>
    /// Gets the description of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The description.</returns>
    public static string GetDescription(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.DescriptionProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the path name of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The path name.</returns>
    public static string GetPathName(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.PathNameProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the imported id of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The imported id.</returns>
    public static string GetImportedId(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.ImportedIdProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the id of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The id.</returns>
    public static Guid GetId(this ICodeRenderElement element)
    {
        var obj = element.GetProperty(CodeRenderProperty.IdProperty, null);
        if (obj is Guid guid)
        {
            return guid;
        }
        else
        {
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Gets the child nodes of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>Child nodes.</returns>
    public static IEnumerable<object> GetChildNodes(this ICodeRenderElement element)
    {
        var e = element.GetProperty(CodeRenderProperty.ChildNodesProperty, null) as IEnumerable<object>;
        return e ?? [];
    }

    /// <summary>
    /// Gets a child node by name.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <param name="name">Child node name.</param>
    /// <returns>The child node.</returns>
    public static object GetChildNode(this ICodeRenderElement element, string name)
    {
        return element.GetProperty(CodeRenderProperty.ChildNodeProperty, name);
    }

    /// <summary>
    /// Gets the parent of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The parent.</returns>
    public static object GetParent(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.ParentProperty, null);
    }

    /// <summary>
    /// Gets the attributes of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <param name="name">Attribute name.</param>
    /// <returns>Attributes.</returns>
    public static IEnumerable<SObject> GetAttributes(this ICodeRenderElement element, string name)
    {
        IEnumerable<SObject> e = element.GetProperty(CodeRenderProperty.AttributesProperty, name) as IEnumerable<SObject>;
        return e ?? [];
    }

    /// <summary>
    /// Gets an attribute by name.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <param name="name">Attribute name.</param>
    /// <returns>The attribute, or null if not found.</returns>
    public static SObject GetAttribute(this ICodeRenderElement element, string name)
    {
        return GetAttributes(element, name).FirstOrDefault();
    }

    /// <summary>
    /// Gets the version of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>The version.</returns>
    public static string GetVersion(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.VersionProperty, null) as string ?? string.Empty;
    }

    /// <summary>
    /// Gets the supported versions of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>Supported versions.</returns>
    public static string[] GetSupportedVersions(this ICodeRenderElement element)
    {
        return element.GetProperty(CodeRenderProperty.SupportedVersionsProperty, null) as string[] ?? [];
    }

    /// <summary>
    /// Gets the components of the element.
    /// </summary>
    /// <param name="element">The code render element.</param>
    /// <returns>Components.</returns>
    public static IEnumerable<object> GetComponents(this ICodeRenderElement element)
    {
        var e = element.GetProperty(CodeRenderProperty.ComponentsProperty, null) as IEnumerable<object>;
        return e ?? [];
    }

    private static TypeDefinition ResolveType(object obj)
    {
        if (obj is TypeDefinition typeDefinition)
        {
            return typeDefinition;
        }
        else if (obj is string s)
        {
            return TypeDefinition.Resolve(s, true);
        }
        else if (obj is Guid id)
        {
            return (AssetManager.Instance.GetAsset(id) as DType)?.Definition ?? TypeDefinition.Empty;
        }
        else
        {
            return TypeDefinition.Empty;
        }
    }
}