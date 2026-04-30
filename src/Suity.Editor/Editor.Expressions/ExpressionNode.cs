namespace Suity.Editor.Expressions;

/// <summary>
/// Base class for AST expression nodes.
/// </summary>
public abstract class ExpressionNode
{
    private static IExpressionBuilder _builder;

    /// <summary>
    /// Gets the AST expression builder instance.
    /// </summary>
    /// <returns>The expression builder.</returns>
    public static IExpressionBuilder Builder => _builder ??= Device.Current.GetService<IExpressionBuilder>();
}