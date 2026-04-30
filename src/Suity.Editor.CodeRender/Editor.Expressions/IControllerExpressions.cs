using Suity.Editor.Design;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating controller (trigger)-related code.
/// </summary>
public interface IControllerExpressions
{
    /// <summary>
    /// Gets the source expression for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the controller source.</returns>
    ExpressionNode GetSource(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the controller body content.</returns>
    ExpressionNode GetBobyContent(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates the Start method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    void MakeStartBody(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates the Stop method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    void MakeStopBody(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates the Enter method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    void MakeEnterBody(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates the Exit method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    void MakeExitBody(IControllerContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates the Update method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="enableCheck">Whether to include enable state checking.</param>
    void MakeUpdateBody(IControllerContainer owner, ExpressionContext context, bool enableCheck);

    /// <summary>
    /// Generates the DoAction method body for a controller container.
    /// </summary>
    /// <param name="owner">The controller container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="enableCheck">Whether to include enable state checking.</param>
    void MakeDoActionBody(IControllerContainer owner, ExpressionContext context, bool enableCheck);
}