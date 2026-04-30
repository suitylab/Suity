using Suity.Parser.Ast;
using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines an AST-based expression writer with type and syntax information output.
/// </summary>
public interface IAstExpressionWriter : IExpressionWriter
{
    /// <summary>
    /// Writes type information from an identifier.
    /// </summary>
    /// <param name="id">The identifier containing type information.</param>
    void TypeInfo(Identifier id);

    /// <summary>
    /// Writes type information from a literal.
    /// </summary>
    /// <param name="literial">The literal containing type information.</param>
    void TypeInfo(Literal literial);

    /// <summary>
    /// Writes type information from an expression.
    /// </summary>
    /// <param name="expression">The expression containing type information.</param>
    void TypeInfo(Expression expression);

    /// <summary>
    /// Writes a syntax node.
    /// </summary>
    /// <param name="node">The syntax node to write.</param>
    void Syntax(SyntaxNode node);

    /// <summary>
    /// Writes multiple syntax nodes.
    /// </summary>
    /// <param name="nodes">The syntax nodes to write.</param>
    void Syntax(IEnumerable<SyntaxNode> nodes);

    /// <summary>
    /// Writes multiple syntax nodes with a custom separator.
    /// </summary>
    /// <param name="nodes">The syntax nodes to write.</param>
    /// <param name="seperator">The separator string between nodes.</param>
    void Syntax(IEnumerable<SyntaxNode> nodes, string seperator);
}