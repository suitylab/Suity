using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Syntax element.
/// </summary>
public interface ICodeRenderElement
{
    /// <summary>
    /// The render type.
    /// </summary>
    RenderType RenderType { get; }

    /// <summary>
    /// Gets syntax element property object for specified property name.
    /// </summary>
    /// <param name="property">Property name.</param>
    /// <param name="argument">Argument.</param>
    /// <returns>Returns property object.</returns>
    object GetProperty(CodeRenderProperty property, object argument = null);
}

/// <summary>
/// Empty code render element implementation.
/// </summary>
public sealed class EmptyCodeRenderElement : ICodeRenderElement
{
    /// <summary>
    /// Empty instance.
    /// </summary>
    public static readonly EmptyCodeRenderElement Empty = new EmptyCodeRenderElement();

    private EmptyCodeRenderElement()
    { }

    /// <inheritdoc/>
    public RenderType RenderType => null;

    /// <inheritdoc/>
    public object GetProperty(CodeRenderProperty property, object argument)
    {
        return null;
    }
}

/// <summary>
/// Syntax property name.
/// </summary>
public abstract class CodeRenderProperty
{
    /// <summary>
    /// Property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Property type.
    /// </summary>
    public abstract Type PropertyType { get; }

    /// <summary>
    /// Parameter type.
    /// </summary>
    public virtual Type ParameterType => null;

    /// <summary>
    /// Creates a new code render property.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    public CodeRenderProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(L($"\"{nameof(propertyName)}\" cannot be null or whitespace."), nameof(propertyName));
        }

        PropertyName = propertyName;
    }

    #region Preset

    /// <summary>
    /// Id property name.
    /// </summary>
    public const string Id = nameof(Id);

    /// <summary>
    /// Id property.
    /// </summary>
    public static CodeRenderProperty<Guid> IdProperty = new(nameof(Id));

    /// <summary>
    /// Name property name.
    /// </summary>
    public const string Name = nameof(Name);

    /// <summary>
    /// Name property.
    /// </summary>
    public static CodeRenderProperty<string> NameProperty = new(nameof(Name));

    /// <summary>
    /// Full name property name.
    /// </summary>
    public const string FullName = nameof(FullName);

    /// <summary>
    /// Full name property.
    /// </summary>
    public static CodeRenderProperty<string> FullNameProperty = new(nameof(FullName));

    /// <summary>
    /// Full type name property name.
    /// </summary>
    public const string FullTypeName = nameof(FullTypeName);

    /// <summary>
    /// Full type name property.
    /// </summary>
    public static CodeRenderProperty<string> FullTypeNameProperty = new(nameof(FullTypeName));

    /// <summary>
    /// Namespace property name.
    /// </summary>
    public const string NameSpace = nameof(NameSpace);

    /// <summary>
    /// Namespace property.
    /// </summary>
    public static CodeRenderProperty<string> NameSpaceProperty = new(nameof(NameSpace));

    /// <summary>
    /// Type info property name.
    /// </summary>
    public const string TypeInfo = nameof(TypeInfo);

    /// <summary>
    /// Type info property.
    /// </summary>
    public static CodeRenderProperty<TypeDefinition> TypeInfoProperty = new(nameof(TypeInfo));

    /// <summary>
    /// Base type info property name.
    /// </summary>
    public const string BaseTypeInfo = nameof(BaseTypeInfo);

    /// <summary>
    /// Base type info property.
    /// </summary>
    public static CodeRenderProperty<TypeDefinition> BaseTypeInfoProperty = new(nameof(BaseTypeInfo));

    /// <summary>
    /// Base enum type info property name.
    /// </summary>
    public const string BaseEnumTypeInfo = nameof(BaseEnumTypeInfo);

    /// <summary>
    /// Base enum type info property.
    /// </summary>
    public static CodeRenderProperty<TypeDefinition> BaseEnumTypeInfoProperty = new(nameof(BaseEnumTypeInfo));

    /// <summary>
    /// Base value type info property name.
    /// </summary>
    public const string BaseValueTypeInfo = nameof(BaseValueTypeInfo);

    /// <summary>
    /// Base value type info property.
    /// </summary>
    public static CodeRenderProperty<TypeDefinition> BaseValueTypeInfoProperty = new(nameof(BaseValueTypeInfo));

    /// <summary>
    /// Return type info property name.
    /// </summary>
    public const string ReturnTypeInfo = nameof(ReturnTypeInfo);

    /// <summary>
    /// Return type info property.
    /// </summary>
    public static CodeRenderProperty<TypeDefinition> ReturnTypeInfoProperty = new(nameof(ReturnTypeInfo));

    /// <summary>
    /// Description property name.
    /// </summary>
    public const string Description = nameof(Description);

    /// <summary>
    /// Description property.
    /// </summary>
    public static CodeRenderProperty<string> DescriptionProperty = new(nameof(Description));

    /// <summary>
    /// Imported ID property name.
    /// </summary>
    public const string ImportedId = nameof(ImportedId);

    /// <summary>
    /// Imported ID property.
    /// </summary>
    public static CodeRenderProperty<string> ImportedIdProperty = new(nameof(ImportedId));

    /// <summary>
    /// Path name property name.
    /// </summary>
    public const string PathName = nameof(PathName);

    /// <summary>
    /// Path name property.
    /// </summary>
    public static CodeRenderProperty<string> PathNameProperty = new(nameof(PathName));

    /// <summary>
    /// All child elements property name.
    /// </summary>
    public const string ChildNodes = nameof(ChildNodes);

    /// <summary>
    /// Child nodes property.
    /// </summary>
    public static CodeRenderProperty<IEnumerable<ICodeRenderElement>> ChildNodesProperty = new(nameof(ChildNodes));

    /// <summary>
    /// Child element property name.
    /// </summary>
    public const string ChildNode = nameof(ChildNode);

    /// <summary>
    /// Child node property.
    /// </summary>
    public static CodeRenderProperty<ICodeRenderElement, string> ChildNodeProperty = new(nameof(ChildNode));

    /// <summary>
    /// Parent property name.
    /// </summary>
    public const string Parent = nameof(Parent);

    /// <summary>
    /// Parent property.
    /// </summary>
    public static CodeRenderProperty<ICodeRenderElement> ParentProperty = new(nameof(Parent));

    /// <summary>
    /// Attributes property name.
    /// </summary>
    public const string Attributes = nameof(Attributes);

    /// <summary>
    /// Attributes property.
    /// </summary>
    public static CodeRenderProperty<IEnumerable<SObject>, string> AttributesProperty = new(nameof(Attributes));

    /// <summary>
    /// Version property name.
    /// </summary>
    public const string Version = nameof(Version);

    /// <summary>
    /// Version property.
    /// </summary>
    public static CodeRenderProperty<string> VersionProperty = new(nameof(Version));

    /// <summary>
    /// Supported versions property name.
    /// </summary>
    public const string SupportedVersions = nameof(SupportedVersions);

    /// <summary>
    /// Supported versions property.
    /// </summary>
    public static CodeRenderProperty<string[]> SupportedVersionsProperty = new(nameof(SupportedVersions));

    /// <summary>
    /// Components property name.
    /// </summary>
    public const string Components = nameof(Components);

    /// <summary>
    /// Components property.
    /// </summary>
    public static CodeRenderProperty<IEnumerable<ICodeRenderElement>> ComponentsProperty = new(nameof(Components));

    /// <summary>
    /// IsNullable property name.
    /// </summary>
    public const string IsNullable = nameof(IsNullable);

    /// <summary>
    /// IsNullable property.
    /// </summary>
    public static CodeRenderProperty<bool> IsNullablesProperty = new(nameof(IsNullable));

    #endregion

    private static readonly Dictionary<string, CodeDynamicRenderProperty> _dynamics = [];

    /// <summary>
    /// Gets a property by name.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <returns>The property.</returns>
    public static CodeRenderProperty GetProperty(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(L($"\"{name}\" cannot be null or whitespace."), nameof(name));
        }

        switch (name)
        {
            case Id:
                return IdProperty;

            case Name:
                return NameProperty;

            case FullName:
                return FullNameProperty;

            case FullTypeName:
                return FullTypeNameProperty;

            case NameSpace:
                return NameSpaceProperty;

            case TypeInfo:
                return TypeInfoProperty;

            case BaseTypeInfo:
                return BaseTypeInfoProperty;

            case BaseEnumTypeInfo:
                return BaseEnumTypeInfoProperty;

            case BaseValueTypeInfo:
                return BaseValueTypeInfoProperty;

            case ReturnTypeInfo:
                return ReturnTypeInfoProperty;

            case Description:
                return DescriptionProperty;

            case ImportedId:
                return ImportedIdProperty;

            case PathName:
                return PathNameProperty;

            case ChildNodes:
                return ChildNodesProperty;

            case ChildNode:
                return ChildNodeProperty;

            case Parent:
                return ParentProperty;

            case Attributes:
                return AttributesProperty;

            case Version:
                return VersionProperty;

            case SupportedVersions:
                return SupportedVersionsProperty;

            case Components:
                return ComponentsProperty;

            case IsNullable:
                return IsNullablesProperty;

            default:
                lock (_dynamics)
                {
                    return _dynamics.GetOrAdd(name, _ => new CodeDynamicRenderProperty(name));
                }
        }
    }
}

/// <summary>
/// Generic code render property.
/// </summary>
public class CodeRenderProperty<T> : CodeRenderProperty
{
    /// <summary>
    /// Creates a new code render property.
    /// </summary>
    /// <param name="name">Property name.</param>
    public CodeRenderProperty(string name) : base(name)
    {
    }

    /// <inheritdoc/>
    public override Type PropertyType => typeof(T);
}

/// <summary>
/// Generic code render property with parameter.
/// </summary>
public class CodeRenderProperty<T, TParameter> : CodeRenderProperty<T>
{
    /// <summary>
    /// Creates a new code render property.
    /// </summary>
    /// <param name="name">Property name.</param>
    public CodeRenderProperty(string name) : base(name)
    {
    }

    /// <inheritdoc/>
    public override Type ParameterType => typeof(TParameter);
}

/// <summary>
/// Dynamic code render property.
/// </summary>
public class CodeDynamicRenderProperty : CodeRenderProperty
{
    /// <summary>
    /// Creates a new dynamic render property.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    public CodeDynamicRenderProperty(string propertyName) : base(propertyName)
    {
    }

    /// <inheritdoc/>
    public override Type PropertyType => null;
}