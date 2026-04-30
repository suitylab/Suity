using Suity.Editor.Values;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating editor object-related code.
/// </summary>
public interface IEditorObjectExpressions
{
    /// <summary>
    /// Gets the expression for a generic editor object.
    /// </summary>
    /// <param name="obj">The object to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the editor object.</returns>
    ExpressionNode GetEditorExpression(object obj, ExpressionContext context);

    /// <summary>
    /// Gets the expression for an IExpressionObject.
    /// </summary>
    /// <param name="getter">The expression object to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the editor object.</returns>
    ExpressionNode GetEditorExpression(IExpressionObject getter, ExpressionContext context);

    /// <summary>
    /// Gets the expression for an SObject.
    /// </summary>
    /// <param name="obj">The SObject to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the SObject.</returns>
    ExpressionNode GetEditorExpression(SObject obj, ExpressionContext context);

    /// <summary>
    /// Gets the expression for an SArray.
    /// </summary>
    /// <param name="ary">The SArray to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the SArray.</returns>
    ExpressionNode GetEditorExpression(SArray ary, ExpressionContext context);

    /// <summary>
    /// Fills the expression for an SArray with its elements.
    /// </summary>
    /// <param name="ary">The SArray to fill expression for.</param>
    /// <param name="context">The expression context.</param>
    void FillEdtiorExpression(SArray ary, ExpressionContext context);

    /// <summary>
    /// Gets the expression for an SEnum.
    /// </summary>
    /// <param name="e">The SEnum to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the SEnum.</returns>
    ExpressionNode GetEditorExpression(SEnum e, ExpressionContext context);

    /// <summary>
    /// Gets the expression for an SKey link.
    /// </summary>
    /// <param name="link">The SKey link to generate expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the SKey link.</returns>
    ExpressionNode GetEditorExpression(SKey link, ExpressionContext context);
}