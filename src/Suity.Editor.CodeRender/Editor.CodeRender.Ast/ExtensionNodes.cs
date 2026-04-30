using Suity.Editor.Types;
using Suity.Parser.Ast;
using System;

namespace Suity.Editor.CodeRender.Ast;

/// <summary>
/// Represents a variable declarator with an associated type identifier.
/// </summary>
public class TypedVariableDeclarator : VariableDeclarator
{
    /// <summary>
    /// The type identifier for the variable.
    /// </summary>
    public Identifier TypeId;
}

/// <summary>
/// Represents the type of type operator used in type expressions.
/// </summary>
public enum TypeOperator
{
    /// <summary>
    /// The 'as' operator for safe type casting.
    /// </summary>
    As,
    /// <summary>
    /// The cast operator for explicit type conversion.
    /// </summary>
    Cast,
}

/// <summary>
/// Represents a type expression involving a type operator (as or cast).
/// </summary>
public class TypeExpression : BinaryExpression
{
    /// <summary>
    /// The type operator used in this expression.
    /// </summary>
    public TypeOperator TypeOp;

    /// <summary>
    /// Parses a string representation of a type operator.
    /// </summary>
    /// <param name="op">The string representation of the operator ("as" or "cast").</param>
    /// <returns>The corresponding <see cref="TypeOperator"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operator string is not recognized.</exception>
    public static TypeOperator ParseTypeOperator(string op)
    {
        switch (op)
        {
            case "as":
                return TypeOperator.As;

            case "cast":
                return TypeOperator.Cast;

            default:
                throw new ArgumentOutOfRangeException("Invalid type operator: " + op);
        }
    }
}

/// <summary>
/// Represents an identifier with associated type information.
/// </summary>
public class TypeIdentifier : Identifier
{
    /// <summary>
    /// The type definition associated with this identifier.
    /// </summary>
    public TypeDefinition TypeInfo;
}

/// <summary>
/// Represents a default expression that evaluates to a default value.
/// </summary>
public class DefaultExpression : Expression
{
    /// <summary>
    /// The type definition for which the default value is computed.
    /// </summary>
    public TypeDefinition TypeInfo;
}

/// <summary>
/// Represents a typed array expression with associated type information.
/// </summary>
public class TypeArrayExpression : ArrayExpression
{
    /// <summary>
    /// The type definition of the array elements.
    /// </summary>
    public TypeDefinition TypeInfo;
}