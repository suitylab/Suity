using Suity.Editor.Design;
using Suity.Editor.Types;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating function-related code.
/// </summary>
public interface IFuntionExpressions
{
    /// <summary>
    /// Gets the source expression for a function container.
    /// </summary>
    /// <param name="owner">The function container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the function source.</returns>
    ExpressionNode GetSource(IFunctionContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the body expression for a function container.
    /// </summary>
    /// <param name="owner">The function container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the function body.</returns>
    ExpressionNode GetBoby(IFunctionContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the source expression for a specific function.
    /// </summary>
    /// <param name="function">The function to generate source for.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="access">The access state of the function.</param>
    /// <returns>An expression node representing the function source.</returns>
    ExpressionNode GetFunctionSource(IFunction function, ExpressionContext context, AccessState access);

    /// <summary>
    /// Gets the body expression for a DFunction.
    /// </summary>
    /// <param name="function">The DFunction to generate body for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the function body.</returns>
    ExpressionNode GetFunctionBody(DFunction function, ExpressionContext context);
}