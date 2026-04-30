namespace Suity.Editor.Expressions;

/// <summary>
/// Represents an object that can provide its own expression representation.
/// </summary>
public interface IExpressionObject
{
    /// <summary>
    /// Gets the expression node representing this object.
    /// </summary>
    /// <param name="builder">The expression builder to use.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing this object.</returns>
    ExpressionNode GetExpression(IExpressionBuilder builder, ExpressionContext context);
}