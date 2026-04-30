using Suity.Editor.CodeRender;
using Suity.Editor.Expressions;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for rendering expressions to code.
/// </summary>
public interface IExpressionRenderService
{
    /// <summary>
    /// Renders an expression node to code.
    /// </summary>
    /// <param name="expression">The expression node to render.</param>
    /// <param name="language">The target language.</param>
    /// <param name="renderOption">The render context options.</param>
    /// <param name="elementContext">Optional element context.</param>
    /// <returns>The render result.</returns>
    RenderResult Render(ExpressionNode expression, string language, ExpressionContext renderOption, object elementContext = null);

    /// <summary>
    /// Renders an expression writable node to code.
    /// </summary>
    /// <param name="node">The expression writable node.</param>
    /// <param name="language">The target language.</param>
    /// <param name="renderOption">The render context options.</param>
    /// <param name="elementContext">Optional element context.</param>
    /// <returns>The render result.</returns>
    RenderResult Render(IExpressionWritable node, string language, ExpressionContext renderOption, object elementContext = null);
}