using Suity.Editor.Types;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating DType-related code (structs, enums, abstract types, functions).
/// </summary>
public interface IDTypeExpressions
{
    /// <summary>
    /// Gets the source expression for a type family.
    /// </summary>
    /// <param name="family">The type family to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the type family source.</returns>
    ExpressionNode GetFamilySource(DTypeFamily family, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a type family.
    /// </summary>
    /// <param name="family">The type family to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the type family body content.</returns>
    ExpressionNode GetFamilyBodyContent(DTypeFamily family, ExpressionContext context);

    /// <summary>
    /// Gets the full source expression for a struct.
    /// </summary>
    /// <param name="dstruct">The struct to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the full struct source.</returns>
    ExpressionNode GetStructFullSource(DCompond dstruct, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a struct.
    /// </summary>
    /// <param name="dstruct">The struct to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the struct body content.</returns>
    ExpressionNode GetStructBodyContent(DCompond dstruct, ExpressionContext context);

    /// <summary>
    /// Gets the full source expression for an abstract type.
    /// </summary>
    /// <param name="abstractType">The abstract type to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the full abstract type source.</returns>
    ExpressionNode GetAbstractFullSource(DAbstract abstractType, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for an abstract type.
    /// </summary>
    /// <param name="abstractType">The abstract type to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the abstract type body content.</returns>
    ExpressionNode GetAbstractBodyContent(DAbstract abstractType, ExpressionContext context);

    /// <summary>
    /// Gets the full source expression for an enum.
    /// </summary>
    /// <param name="denum">The enum to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the full enum source.</returns>
    ExpressionNode GetEnumFullSource(DEnum denum, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for an enum.
    /// </summary>
    /// <param name="denum">The enum to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the enum body content.</returns>
    ExpressionNode GetEnumBodyContent(DEnum denum, ExpressionContext context);

    /// <summary>
    /// Gets the source expression for a function.
    /// </summary>
    /// <param name="function">The function to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="access">The access state of the function.</param>
    /// <returns>An expression node representing the function source.</returns>
    ExpressionNode GetFunction(DFunction function, ExpressionContext context, AccessState access);

    /// <summary>
    /// Gets the body expression for a function.
    /// </summary>
    /// <param name="function">The function to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the function body.</returns>
    ExpressionNode GetFunctionBody(DFunction function, ExpressionContext context);
}