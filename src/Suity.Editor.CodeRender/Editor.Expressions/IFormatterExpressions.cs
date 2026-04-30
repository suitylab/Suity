using Suity.Editor.Types;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating formatter-related code.
/// </summary>
public interface IFormatterExpressions
{
    /// <summary>
    /// Gets the source expression for a type family formatter.
    /// </summary>
    /// <param name="family">The type family to format.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the formatter source.</returns>
    ExpressionNode GetFamilyFormatterSource(DTypeFamily family, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a type family formatter.
    /// </summary>
    /// <param name="family">The type family to format.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the formatter body content.</returns>
    ExpressionNode GetFamilyFormatterBodyContent(DTypeFamily family, ExpressionContext context);

    /// <summary>
    /// Gets the full source expression for a struct formatter.
    /// </summary>
    /// <param name="s">The compound struct to format.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the full struct formatter source.</returns>
    ExpressionNode GetStructFormatterFullSource(DCompond s, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a struct formatter.
    /// </summary>
    /// <param name="s">The compound struct to format.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the struct formatter body content.</returns>
    ExpressionNode GetStructFormatterBodyContent(DCompond s, ExpressionContext context);
}