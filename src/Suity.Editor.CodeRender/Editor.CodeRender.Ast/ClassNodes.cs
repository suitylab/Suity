using Suity.Editor.Expressions;
using Suity.Parser.Ast;
using Suity.Synchonizing;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender.Ast;

/// <summary>
/// Represents a comment node in the AST.
/// </summary>
public class CommentNode : StatementNode
{
    /// <summary>
    /// The comment text content.
    /// </summary>
    public string Comment;
}

/// <summary>
/// Represents the root source node containing types, imports, and namespace information.
/// </summary>
public class SourceNode : SyntaxNode
{
    /// <summary>
    /// The namespace of the source file.
    /// </summary>
    public string NameSpace;
    /// <summary>
    /// The collection of type nodes defined in this source.
    /// </summary>
    public ICollection<TypeNode> Body;
    /// <summary>
    /// The collection of import items in this source.
    /// </summary>
    public ICollection<ImportItem> Imports;
    /// <summary>
    /// Custom statement nodes for the source body.
    /// </summary>
    public ICollection<StatementNode> CustomBody;

    /// <summary>
    /// The target language for code rendering.
    /// </summary>
    public string Language;
    /// <summary>
    /// The raw code content.
    /// </summary>
    public string Code;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceNode"/> class.
    /// </summary>
    public SourceNode()
    {
    }
}

/// <summary>
/// Represents a type definition node (class, enum, struct, or interface).
/// </summary>
public class TypeNode : SyntaxNode
{
    /// <summary>
    /// The kind of type (class, enum, struct, or interface).
    /// </summary>
    public TypeKind Kind;
    /// <summary>
    /// The access level of this type.
    /// </summary>
    public AccessState AccessMode;
    /// <summary>
    /// The virtual state of this type.
    /// </summary>
    public VirtualState VirtualMode;
    /// <summary>
    /// Indicates whether this type is static.
    /// </summary>
    public bool IsStatic;
    /// <summary>
    /// The identifier of this type.
    /// </summary>
    public Identifier Id;

    /// <summary>
    /// The name of the base type this type extends.
    /// </summary>
    public string Extends;
    /// <summary>
    /// The list of interface names this type implements.
    /// </summary>
    public IList<string> Implements;

    /// <summary>
    /// The collection of statement nodes in the type body.
    /// </summary>
    public ICollection<StatementNode> Body;
    /// <summary>
    /// Indicates whether this type is generated from AST.
    /// </summary>
    public bool Ast;

    /// <summary>
    /// The target language for code rendering.
    /// </summary>
    public string Language;
    /// <summary>
    /// The raw code content.
    /// </summary>
    public string Code;

    /// <summary>
    /// The documentation comment for this type.
    /// </summary>
    public string Document;
}

/// <summary>
/// Represents a statement that is a member of a type node.
/// </summary>
public class TypeNodeStatement : StatementNode
{
}

/// <summary>
/// Represents a declaration of a type member (field or method).
/// </summary>
public class TypeMemberDeclaration : TypeNodeStatement
{
    /// <summary>
    /// The identifier of the member.
    /// </summary>
    public Identifier Id;
    /// <summary>
    /// The access level of this member.
    /// </summary>
    public AccessState AccessMode;
    /// <summary>
    /// The static/readonly mode of this member.
    /// </summary>
    public StaticReadonlyMode StaticMode;

    /// <summary>
    /// The documentation comment for this member.
    /// </summary>
    public string Document;
}

/// <summary>
/// Represents a field declaration in a class.
/// </summary>
public class ClassField : TypeMemberDeclaration
{
    /// <summary>
    /// The type name of the field.
    /// </summary>
    public string TypeName;
    /// <summary>
    /// The initialization expression for the field.
    /// </summary>
    public Expression Init;
}

/// <summary>
/// Represents a field in an enum.
/// </summary>
public class EnumField : TypeMemberDeclaration
{
    /// <summary>
    /// The numeric value of the enum field.
    /// </summary>
    public int Number;
    /// <summary>
    /// Indicates whether the number is explicitly specified.
    /// </summary>
    public bool ExactNumber;
}

/// <summary>
/// Represents native code embedded in a class.
/// </summary>
public class ClassNativeCode : TypeNodeStatement
{
    /// <summary>
    /// The target language for the native code.
    /// </summary>
    public string Language;
    /// <summary>
    /// The native code content.
    /// </summary>
    public string Code;
}

/// <summary>
/// Represents a method declaration in a class.
/// </summary>
public class ClassMethod : TypeMemberDeclaration
{
    /// <summary>
    /// The virtual state of this method.
    /// </summary>
    public VirtualState VirtualMode;
    /// <summary>
    /// Indicates whether this method is a constructor.
    /// </summary>
    public bool IsConstructor;
    /// <summary>
    /// The return type name of the method.
    /// </summary>
    public string TypeName;
    /// <summary>
    /// The list of parameter definitions.
    /// </summary>
    public IList<ParameterDefination> Parameters;
    /// <summary>
    /// The list of default value expressions for parameters.
    /// </summary>
    public IList<Expression> Defaults;

    /// <summary>
    /// The target language for code rendering.
    /// </summary>
    public string Language;
    /// <summary>
    /// The raw code content.
    /// </summary>
    public string Code;
    /// <summary>
    /// The element identifier associated with this method.
    /// </summary>
    public string Element;
    /// <summary>
    /// Indicates whether this method is generated from AST.
    /// </summary>
    public bool Ast;

    /// <summary>
    /// The list of statement nodes in the method body.
    /// </summary>
    public IList<StatementNode> Statements;
}

/// <summary>
/// Represents a parameter definition for a method.
/// </summary>
public class ParameterDefination : ISyncObject
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name;
    /// <summary>
    /// The type name of the parameter.
    /// </summary>
    public string TypeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterDefination"/> class.
    /// </summary>
    public ParameterDefination()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterDefination"/> class with the specified name and type.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="typeName">The type name of the parameter.</param>
    public ParameterDefination(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Name = sync.Sync("name", Name, SyncFlag.AttributeMode);
        TypeName = sync.Sync("typeName", TypeName, SyncFlag.AttributeMode);
    }
}