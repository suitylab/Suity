using Suity.Editor;
using Suity.Editor.CodeRender.Templating;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Expressions;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Provides binding functions for code rendering templates, resolving objects and extracting type information, names, and other metadata.
/// </summary>
public class CodeBinder
{
    /// <summary>
    /// Singleton instance of the code binder.
    /// </summary>
    public static readonly CodeBinder Instance = new();

    #region Main

    /// <summary>
    /// Gets the language name from a render target.
    /// </summary>
    /// <param name="value">The value to extract language from.</param>
    /// <returns>The language name, or null if not available.</returns>
    public string Language(object value)
    {
        if (value is RenderTarget renderTarget)
        {
            return renderTarget.Language;
        }

        return null;
    }

    /// <summary>
    /// Resolves the type definition from a value.
    /// </summary>
    /// <param name="value">The value to extract type information from.</param>
    /// <returns>The type definition, or empty if not available.</returns>
    public TypeDefinition TypeInfo(object value)
    {
        if (value is TypeDefinition typeDefinition)
        {
            return typeDefinition;
        }

        value = ResolveObject(value);

        return value switch
        {
            DType type => type.Definition ?? TypeDefinition.Empty,
            DStructField field => field.FieldType ?? TypeDefinition.Empty,
            ICodeRenderElement compileNode => compileNode.GetTypeInfo() ?? TypeDefinition.Empty,
            _ => TypeDefinition.Empty,
        };
    }

    /// <summary>
    /// Resolves the base type definition from a code render element.
    /// </summary>
    /// <param name="value">The value to extract base type information from.</param>
    /// <returns>The base type definition, or empty if not available.</returns>
    public TypeDefinition BaseTypeInfo(object value)
    {
        value = ResolveObject(value);

        TypeDefinition typeInfo = null;

        if (value is ICodeRenderElement compileNode)
        {
            typeInfo = compileNode.GetBaseTypeInfo();
        }

        return typeInfo ?? TypeDefinition.Empty;
    }

    /// <summary>
    /// Resolves the return type definition from a code render element.
    /// </summary>
    /// <param name="value">The value to extract return type information from.</param>
    /// <returns>The return type definition, or empty if not available.</returns>
    public TypeDefinition ReturnTypeInfo(object value)
    {
        value = ResolveObject(value);

        TypeDefinition typeInfo = null;

        if (value is ICodeRenderElement compileNode)
        {
            typeInfo = compileNode.GetReturnTypeInfo();
        }

        return typeInfo ?? TypeDefinition.Empty;
    }

    /// <summary>
    /// Gets the name from a value.
    /// </summary>
    /// <param name="value">The value to extract name from.</param>
    /// <returns>The name string, or empty if not available.</returns>
    public string Name(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            string str => str,
            ICodeRenderElement compileNode => compileNode.GetName(),
            Document document => document.Entry?.GetShortTypeName(),
            DocumentEntry documentEntry => documentEntry.GetShortTypeName(),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Gets a chain of names from a collection of values.
    /// </summary>
    /// <param name="value">The value or collection to extract names from.</param>
    /// <param name="seperator">The separator between names.</param>
    /// <returns>The joined name string.</returns>
    public string NameChain(object value, string seperator = ", ")
    {
        if (value is IEnumerable<object> e)
        {
            return string.Join(seperator, e.Select(TypeName));
        }
        else if (value != null)
        {
            return Name(value);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the namespace from a value.
    /// </summary>
    /// <param name="value">The value to extract namespace from.</param>
    /// <returns>The namespace string.</returns>
    public string NameSpace(object value)
    {
        value = ResolveObject(value);

        return EditorUtility.GetNameSpace(value);
    }

    /// <summary>
    /// Gets the namespace as a path format (dots replaced with slashes).
    /// </summary>
    /// <param name="value">The value to extract namespace from.</param>
    /// <returns>The namespace path string.</returns>
    public string NamePath(object value)
    {
        return NameSpace(value).Replace('.', '/');
    }

    /// <summary>
    /// Gets the type code string from a value.
    /// </summary>
    /// <param name="value">The value to extract type code from.</param>
    /// <returns>The type code string.</returns>
    public string TypeCode(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);

        return typeInfo.ToTypeName();
    }

    /// <summary>
    /// Gets the short type name from a value.
    /// </summary>
    /// <param name="value">The value to extract type name from.</param>
    /// <returns>The short type name.</returns>
    public string TypeName(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.GetShortTypeName(false);
    }

    /// <summary>
    /// Gets a chain of full type names from a collection of values.
    /// </summary>
    /// <param name="value">The value or collection to extract type names from.</param>
    /// <param name="seperator">The separator between type names.</param>
    /// <returns>The joined full type name string.</returns>
    public string TypeNameChain(object value, string seperator = ", ")
    {
        if (value is IEnumerable<object> e)
        {
            return string.Join(seperator, e.Select(TypeName));
        }
        else if (value != null)
        {
            return FullTypeName(value);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the short type name with alias from a value.
    /// </summary>
    /// <param name="value">The value to extract type name from.</param>
    /// <returns>The short type name with alias.</returns>
    public string TypeNameAlias(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.GetShortTypeName(true);
    }

    /// <summary>
    /// Gets the full type name from a value.
    /// </summary>
    /// <param name="value">The value to extract full type name from.</param>
    /// <returns>The full type name, or empty if not available.</returns>
    public string FullTypeName(object value)
    {
        if (value is ICodeRenderElement compileNode)
        {
            return compileNode.GetFullTypeName();
        }

        Guid id = Id(value);
        Asset asset = AssetManager.Instance.GetAsset(id);
        if (asset != null)
        {
            return asset.FullTypeName;
        }
        else
        {
            return string.Empty;
        }

        //TypeDefinition typeInfo = TypeInfo(value);
        //return typeInfo.GetFullTypeName();
    }

    /// <summary>
    /// Gets a chain of full type names from a collection of values.
    /// </summary>
    /// <param name="value">The value or collection to extract full type names from.</param>
    /// <param name="seperator">The separator between full type names.</param>
    /// <returns>The joined full type name string.</returns>
    public string FullTypeNameChain(object value, string seperator = ", ")
    {
        if (value is IEnumerable<object> e)
        {
            return string.Join(seperator, e.Select(FullTypeName));
        }
        else if (value != null)
        {
            return FullTypeName(value);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the full type name with dots replaced by underscores.
    /// </summary>
    /// <param name="value">The value to extract type name from.</param>
    /// <returns>The plain type name string.</returns>
    public string PlainTypeName(object value)
    {
        return FullTypeName(value).Replace('.', '_');
    }

    /// <summary>
    /// Gets the asset key from a value.
    /// </summary>
    /// <param name="value">The value to extract asset key from.</param>
    /// <returns>The asset key string, or empty if not available.</returns>
    public string AssetKey(object value)
    {
        if (value == null)
        {
            return null;
        }

        value = ResolveObject(value);

        return value switch
        {
            Asset asset => asset.AssetKey ?? string.Empty,
            ICodeRenderElement compileNode => compileNode.GetPathName(),
            TypeDefinition typeDef => typeDef.ToTypeName(),
            KeyCode keyCode => keyCode.ToString(),
            Document document => document.Entry?.GetAsset()?.AssetKey ?? string.Empty,
            DocumentEntry documentEntry => documentEntry.GetAsset()?.AssetKey ?? string.Empty,
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Gets the unique identifier from a value.
    /// </summary>
    /// <param name="value">The value to extract ID from.</param>
    /// <returns>The GUID, or empty if not available.</returns>
    public Guid Id(object value)
    {
        if (value == null)
        {
            return Guid.Empty;
        }

        value = ResolveObject(value);

        return value switch
        {
            Guid id => id,
            EditorObject editorObj => editorObj.Id,
            ICodeRenderElement compileNode => compileNode.GetId(),
            TypeDefinition typeDef => typeDef.TargetId,
            Document document => document.Entry?.GetAsset()?.Id ?? Guid.Empty,
            DocumentEntry documentEntry => documentEntry.GetAsset()?.Id ?? Guid.Empty,
            IHasId idContext => idContext.Id,
            IHasAsset assetContext => assetContext.TargetAsset?.Id ?? Guid.Empty,
            _ => Guid.Empty,
        };
    }

    /// <summary>
    /// Gets the data ID string from a value's associated asset.
    /// </summary>
    /// <param name="value">The value to extract data ID from.</param>
    /// <returns>The data ID string, or empty if not available.</returns>
    public string DataId(object value)
    {
        Guid id = Id(value);
        Asset asset = AssetManager.Instance.GetAsset(id);
        if (asset != null)
        {
            return asset.ToDataId();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if the value represents an array type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an array.</returns>
    public bool IsArrayType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsArray;
    }

    /// <summary>
    /// Checks if the value represents a key link type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is a data link.</returns>
    public bool IsKeyLinkType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsDataLink;
    }

    /// <summary>
    /// Checks if the value represents an array of key link types.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an array of data links.</returns>
    public bool IsKeyLinkArrayType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsArray && typeInfo.ElementType.IsDataLink;
    }

    /// <summary>
    /// Checks if the value represents a data link type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is a data link.</returns>
    public bool IsDataLinkType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsDataLink;
    }

    /// <summary>
    /// Checks if the value represents an array of data link types.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an array of data links.</returns>
    public bool IsDataLinkArrayType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsArray && typeInfo.ElementType.IsDataLink;
    }

    /// <summary>
    /// Checks if the value represents an asset link type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an asset link.</returns>
    public bool IsAssetLinkType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsAssetLink;
    }

    /// <summary>
    /// Checks if the value represents an array of asset link types.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an array of asset links.</returns>
    public bool IsAssetLinkArrayType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsArray && typeInfo.ElementType.IsAssetLink;
    }

    /// <summary>
    /// Checks if the value represents any link type (data link or asset link).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is any kind of link.</returns>
    public bool IsAnyLinkType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsLink;
    }

    /// <summary>
    /// Checks if the value represents an array of any link types.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an array of any kind of link.</returns>
    public bool IsAnyLinkArrayType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsArray && typeInfo.ElementType.IsLink;
    }

    /// <summary>
    /// Checks if the value represents a native type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is native.</returns>
    public bool IsNativeType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsNative;
    }

    /// <summary>
    /// Checks if the value represents an enum type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is an enum.</returns>
    public bool IsEnumType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsEnum;
    }

    /// <summary>
    /// Checks if the value represents a dynamic (abstract) type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is abstract.</returns>
    public bool IsDynamicType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsAbstract;
    }

    /// <summary>
    /// Checks if the value represents an abstract type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is abstract.</returns>
    public bool IsAbstract(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo.IsAbstract;
    }

    /// <summary>
    /// Checks if the value represents an empty or null type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the type is null or empty.</returns>
    public bool IsEmptyType(object value)
    {
        TypeDefinition typeInfo = TypeInfo(value);
        return typeInfo == null || typeInfo == TypeDefinition.Empty;
    }

    /// <summary>
    /// Gets the description from a code render element.
    /// </summary>
    /// <param name="value">The value to extract description from.</param>
    /// <returns>The description string, or empty if not available.</returns>
    public string Description(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetDescription() ?? string.Empty,
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Gets the path name from a code render element.
    /// </summary>
    /// <param name="value">The value to extract path name from.</param>
    /// <returns>The path name string, or empty if not available.</returns>
    public string PathName(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetPathName(),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Gets the imported ID from a code render element.
    /// </summary>
    /// <param name="value">The value to extract imported ID from.</param>
    /// <returns>The imported ID string, or empty if not available.</returns>
    public string ImportedId(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetImportedId(),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Gets the child elements from a code render element.
    /// </summary>
    /// <param name="value">The value to extract child elements from.</param>
    /// <returns>The collection of child elements, or empty if not available.</returns>
    public IEnumerable<object> Elements(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetChildNodes(),
            _ => [],
        };
    }

    /// <summary>
    /// Gets a specific child element by name from a code render element.
    /// </summary>
    /// <param name="value">The value to search in.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The child element, or null if not found.</returns>
    public object GetChildElement(object value, string name)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetChildNode(name),
            _ => null,
        };
    }

    /// <summary>
    /// Gets the parent element from a code render element.
    /// </summary>
    /// <param name="value">The value to get parent from.</param>
    /// <returns>The parent element, or null if not available.</returns>
    public object Parent(object value)
    {
        value = ResolveObject(value);

        return value switch
        {
            ICodeRenderElement compileNode => compileNode.GetParent(),
            _ => null,
        };
    }

    /// <summary>
    /// Checks if the value is null or represents an empty/error proxy.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is null, ErrorProxy, or DynamicProxy.</returns>
    public bool IsNull(object value)
    {
        if (value == null)
        {
            return true;
        }
        if (value is ErrorProxy || value.GetType() == typeof(DynamicProxy))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the value is empty (same as IsNull).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is null or empty.</returns>
    public bool IsEmpty(object value)
    {
        return IsNull(value);
    }

    /// <summary>
    /// Gets the name of the first child element from a code render element.
    /// </summary>
    /// <param name="value">The value to extract first element name from.</param>
    /// <returns>The name of the first child element, or empty if not available.</returns>
    public string FirstElementName(object value)
    {
        if (value is RenderItem item)
        {
            value = item.Object;
        }

        if (value is ICodeRenderElement compileNode)
        {
            var first = compileNode.GetChildNodes().FirstOrDefault();
            if (first != null)
            {
                return Name(first);
            }
            else
            {
                return string.Empty;
            }
        }
        else
        {
            return string.Empty;
        }
    }

    #endregion

    #region CodeBlock

    /// <summary>
    /// Creates a remark line code block.
    /// </summary>
    /// <param name="text">The remark text.</param>
    /// <returns>A remark line code block.</returns>
    public CodeBlock RemarkLine(string text)
    {
        return $"<RemarkLine text='{text}'/>";
    }

    /// <summary>
    /// Creates a region begin code block.
    /// </summary>
    /// <param name="code">The region code (unused, kept for compatibility).</param>
    /// <returns>A region begin code block.</returns>
    public CodeBlock RegionBegin(string code)
    {
        return "<Region>";
    }

    /// <summary>
    /// Creates a region end code block.
    /// </summary>
    /// <param name="code">The region code (unused, kept for compatibility).</param>
    /// <returns>A region end code block.</returns>
    public CodeBlock RegionEnd(string code = null)
    {
        return "</Region>";
    }

    /// <summary>
    /// Creates a block begin code block.
    /// </summary>
    /// <returns>A block begin code block.</returns>
    public CodeBlock Begin()
    {
        return "<Block>";
    }

    /// <summary>
    /// Creates a block end code block.
    /// </summary>
    /// <returns>A block end code block.</returns>
    public CodeBlock End()
    {
        return "</Block>";
    }

    /// <summary>
    /// Creates a class begin code block with the current modifier state.
    /// </summary>
    /// <param name="name">The class name.</param>
    /// <returns>A class begin code block.</returns>
    public CodeBlock ClassBegin(object name)
    {
        ModiferState state = PickupState();

        return $"<Class name='{Name(name)}' {state.GetCode()}>";
    }

    /// <summary>
    /// Creates a class end code block.
    /// </summary>
    /// <returns>A class end code block.</returns>
    public CodeBlock ClassEnd()
    {
        return "</Class>";
    }

    /// <summary>
    /// Creates a function declaration begin code block with the current modifier state.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>A function declaration begin code block.</returns>
    public CodeBlock FunctionBegin(object name)
    {
        ModiferState state = PickupState();

        return $"<FunctionDeclaration name='{Name(name)}' {state.GetCode()}>";
    }

    /// <summary>
    /// Creates a function parameter code block.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The parameter type.</param>
    /// <returns>A parameter code block.</returns>
    public CodeBlock FunctionParameter(object name, object type)
    {
        return $"<Parameter name='{name}' type='{type}'/>";
    }

    /// <summary>
    /// Creates a function declaration end code block.
    /// </summary>
    /// <returns>A function declaration end code block.</returns>
    public CodeBlock FunctionEnd()
    {
        return "</FunctionDeclaration>";
    }

    /// <summary>
    /// Creates a function call begin code block.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>A function call begin code block.</returns>
    public CodeBlock FunctionCallBegin(string name)
    {
        return $"<FunctionCall name='{name}'>";
    }

    /// <summary>
    /// Creates an argument code block.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>An argument code block.</returns>
    public CodeBlock Argument(string name)
    {
        return $"<Argument name='{name}'/>";
    }

    /// <summary>
    /// Creates a function call end code block.
    /// </summary>
    /// <returns>A function call end code block.</returns>
    public CodeBlock FunctionCallEnd()
    {
        return "</FunctionCall>";
    }

    /// <summary>
    /// Creates a property code block with the current modifier state.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    /// <returns>A property code block.</returns>
    public CodeBlock Property(object name, object type)
    {
        var state = PickupState();
        return $"<Property name='{Name(name)}' {state.GetCode()}/>";
    }

    /// <summary>
    /// Creates a property begin code block with the current modifier state.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    /// <returns>A property begin code block.</returns>
    public CodeBlock PropertyBegin(object name, object type)
    {
        var state = PickupState();
        return $"<Property name='{Name(name)}' {state.GetCode()}>";
    }

    /// <summary>
    /// Creates a getter begin code block.
    /// </summary>
    /// <returns>A getter begin code block.</returns>
    public CodeBlock GetterBegin()
    {
        return "<Getter>";
    }

    /// <summary>
    /// Creates a getter end code block.
    /// </summary>
    /// <returns>A getter end code block.</returns>
    public CodeBlock GetterEnd()
    {
        return "</Getter>";
    }

    /// <summary>
    /// Creates a setter begin code block with the current modifier state.
    /// </summary>
    /// <returns>A setter begin code block.</returns>
    public CodeBlock SetterBegin()
    {
        var state = PickupState();
        return string.Format("<Setter {0}/>", state.GetCode());
    }

    /// <summary>
    /// Creates a setter end code block.
    /// </summary>
    /// <returns>A setter end code block.</returns>
    public CodeBlock SetterEnd()
    {
        return "</Setter>";
    }

    /// <summary>
    /// Creates a constant declaration code block with the current modifier state.
    /// </summary>
    /// <param name="name">The constant name.</param>
    /// <param name="type">The constant type.</param>
    /// <param name="value">The constant value.</param>
    /// <returns>A constant code block.</returns>
    public CodeBlock Const(object name, object type, object value)
    {
        var state = PickupState();
        return $"<Const name='{name}' type='{type}' value='{value}' {state.GetCode()}/>";
    }

    /// <summary>
    /// Creates a variable declaration code block with the current modifier state.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The variable type.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>A variable code block.</returns>
    public CodeBlock Variable(object name, object type, object value)
    {
        var state = PickupState();
        return $"<Variable name='{name}' type='{type}' value='{value}' {state.GetCode()}/>";
    }

    /// <summary>
    /// Creates a user code placeholder code block.
    /// </summary>
    /// <param name="value">The value to extract path name from.</param>
    /// <param name="name">The user code block name.</param>
    /// <returns>A user code placeholder code block.</returns>
    public CodeBlock UserCode(dynamic value, string name = "Default")
    {
        return $"<UserCode pathName='{PathName(value)}' name='{name}'/>";
    }

    /// <summary>
    /// Creates a user code begin code block.
    /// </summary>
    /// <param name="value">The value to extract path name from.</param>
    /// <param name="name">The user code block name.</param>
    /// <returns>A user code begin code block.</returns>
    public CodeBlock UserCodeBegin(dynamic value, string name = "Default")
    {
        return $"<UserCode pathName='{PathName(value)}' name='{name}'>";
    }

    /// <summary>
    /// Creates a user code end code block.
    /// </summary>
    /// <returns>A user code end code block.</returns>
    public CodeBlock UserCodeEnd()
    {
        return "</UserCode>";
    }

    #endregion

    #region State

    private ModiferState _state = new ModiferState();

    /// <summary>
    /// Sets the access modifier to public.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Public()
    {
        _state.AccessState = AccessState.Public; return this;
    }

    /// <summary>
    /// Sets the access modifier to protected.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Protected()
    {
        _state.AccessState = AccessState.Protected; return this;
    }

    /// <summary>
    /// Sets the access modifier to internal.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Internal()
    {
        _state.AccessState = AccessState.Internal; return this;
    }

    /// <summary>
    /// Sets the virtual state to virtual.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Virtual()
    {
        _state.VirtualState = VirtualState.Virtual; return this;
    }

    /// <summary>
    /// Sets the virtual state to abstract.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Abstract()
    {
        _state.VirtualState = VirtualState.Abstract; return this;
    }

    /// <summary>
    /// Sets the virtual state to override.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Override()
    {
        _state.VirtualState = VirtualState.Override; return this;
    }

    /// <summary>
    /// Sets the static state to true.
    /// </summary>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Static()
    {
        _state.StaticState = true; return this;
    }

    /// <summary>
    /// Sets the return type for the current modifier state.
    /// </summary>
    /// <param name="typeName">The return type name.</param>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder ReturnType(string typeName)
    {
        _state.ReturnType = TypeCode(typeName);
        return this;
    }

    /// <summary>
    /// Adds a generic type to the current modifier state.
    /// </summary>
    /// <param name="typeName">The generic type name.</param>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder GenericType(string typeName)
    {
        _state.GendericTypeState.Add(TypeCode(typeName));
        return this;
    }

    /// <summary>
    /// Sets the generic constraint for the current modifier state.
    /// </summary>
    /// <param name="constrain">The constraint string.</param>
    /// <returns>The current code binder instance for chaining.</returns>
    public CodeBinder Constrain(string constrain)
    {
        _state.ConstrainState = constrain;
        return this;
    }

    /// <summary>
    /// Extracts and resets the current modifier state.
    /// </summary>
    /// <returns>The current modifier state, and resets internal state to new.</returns>
    protected ModiferState PickupState()
    {
        ModiferState state = _state;
        _state = new ModiferState();
        return state;
    }

    #endregion

    /// <summary>
    /// Resolves proxy objects to their underlying values.
    /// </summary>
    /// <param name="obj">The object to resolve.</param>
    /// <returns>The resolved object.</returns>
    private object ResolveObject(object obj) => obj switch
    {
        RenderModelProxy typeInfoNodeProxy => typeInfoNodeProxy.Model,
        TypeDefinitionProxy dTypeCodeProxy => dTypeCodeProxy.TypeCode,
        AssetIdProxy assetKeyProxy => EditorObjectManager.Instance.GetObject(assetKeyProxy.Id),
        RenderTarget renderTarget => renderTarget.Item?.Object,
        RenderItem renderItem => renderItem?.Object,
        _ => obj,
    };

    /// <summary>
    /// Gets the type name (passthrough method).
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The same type name.</returns>
    protected string GetTypeName(string typeName)
    {
        return typeName;
    }

    /// <summary>
    /// Represents the state of code modifiers for code generation.
    /// </summary>
    public class ModiferState
    {
        /// <summary>
        /// The access modifier state.
        /// </summary>
        public AccessState AccessState = AccessState.Public;
        /// <summary>
        /// The virtual modifier state.
        /// </summary>
        public VirtualState VirtualState = VirtualState.Normal;
        /// <summary>
        /// Whether the member is static.
        /// </summary>
        public bool StaticState = false;
        /// <summary>
        /// The return type string.
        /// </summary>
        public string ReturnType;
        /// <summary>
        /// The list of generic type names.
        /// </summary>
        public readonly List<string> GendericTypeState = new List<string>();
        /// <summary>
        /// The generic constraint string.
        /// </summary>
        public string ConstrainState;

        /// <summary>
        /// Generates the code representation of the modifier state.
        /// </summary>
        /// <returns>A string containing the modifier attributes in XML-like format.</returns>
        public string GetCode()
        {
            string str = string.Format("access='{0}'", AccessState);

            if (VirtualState != VirtualState.Normal)
            {
                str += string.Format(" virtual='{0}'", VirtualState);
            }
            if (StaticState)
            {
                str += " static='true'";
            }
            if (!string.IsNullOrEmpty(ReturnType))
            {
                str += string.Format(" return='{0}'", ReturnType);
            }
            if (GendericTypeState.Count > 0)
            {
                str += string.Format(" generic='{0}'", string.Join(",", GendericTypeState));
            }
            if (!string.IsNullOrEmpty(ConstrainState))
            {
                str += string.Format(" constrain='{0}'", ConstrainState);
            }

            return str;
        }
    }
}