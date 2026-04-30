using Suity.Editor.Design;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating component-related code.
/// </summary>
public interface IComponentExpressions
{
    /// <summary>
    /// Gets the source expression for a component container.
    /// </summary>
    /// <param name="owner">The component container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the component source.</returns>
    ExpressionNode GetSource(IComponentContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the body expression for a component container.
    /// </summary>
    /// <param name="owner">The component container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the component body.</returns>
    ExpressionNode GetBoby(IComponentContainer owner, ExpressionContext context);
}