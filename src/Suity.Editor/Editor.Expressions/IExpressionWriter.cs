using Suity.Editor.Types;
using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// Interface for writing expression nodes to code output.
/// </summary>
public interface IExpressionWriter
{
    /// <summary>
    /// Writes a keyword to the output.
    /// </summary>
    /// <param name="keyword">The keyword to write.</param>
    void Keyword(string keyword);

    /// <summary>
    /// Writes an identifier to the output.
    /// </summary>
    /// <param name="id">The identifier to write.</param>
    void Identifier(string id);

    /// <summary>
    /// Writes an operator to the output.
    /// </summary>
    /// <param name="op">The operator to write.</param>
    void Operator(string op);

    /// <summary>
    /// Writes a string literal to the output.
    /// </summary>
    /// <param name="str">The string to write.</param>
    void String(string str);

    /// <summary>
    /// Writes arbitrary code to the output.
    /// </summary>
    /// <param name="str">The code to write.</param>
    void Code(string str);

    /// <summary>
    /// Writes a double-quoted string literal to the output.
    /// </summary>
    /// <param name="str">The string to write.</param>
    void DoubleQuotString(string str);

    /// <summary>
    /// Writes a space to the output.
    /// </summary>
    void Space();

    /// <summary>
    /// Writes a newline to the output.
    /// </summary>
    void NewLine();

    /// <summary>
    /// Writes a type definition to the output.
    /// </summary>
    /// <param name="type">The type definition to write.</param>
    void TypeInfo(TypeDefinition type);

    /// <summary>
    /// Writes a type name to the output.
    /// </summary>
    /// <param name="typeName">The type name to write.</param>
    void TypeInfo(string typeName);

    /// <summary>
    /// Writes an expression node to the output.
    /// </summary>
    /// <param name="expression">The expression to write.</param>
    void Expression(ExpressionNode expression);

    /// <summary>
    /// Writes a writable expression to the output.
    /// </summary>
    /// <param name="writable">The writable expression to write.</param>
    void Expression(IExpressionWritable writable);

    /// <summary>
    /// Writes multiple writable expressions to the output.
    /// </summary>
    /// <param name="writables">The writable expressions to write.</param>
    void Expression(IEnumerable<IExpressionWritable> writables);

    /// <summary>
    /// Marks the beginning of a user-defined code block.
    /// </summary>
    /// <param name="name">The name of the user code block.</param>
    void UserCodeBegin(string name);

    /// <summary>
    /// Marks the end of a user-defined code block.
    /// </summary>
    /// <param name="name">The name of the user code block.</param>
    void UserCodeEnd(string name);

    /// <summary>
    /// Marks the beginning of a generated code block.
    /// </summary>
    /// <param name="name">The name of the generated code block.</param>
    void GenCodeBegin(string name);

    /// <summary>
    /// Marks the end of a generated code block.
    /// </summary>
    /// <param name="name">The name of the generated code block.</param>
    void GenCodeEnd(string name);

    /// <summary>
    /// Begins an indented block.
    /// </summary>
    void IndentBegin();

    /// <summary>
    /// Ends an indented block.
    /// </summary>
    void IndentEnd();
}