using Suity.Editor.Types;
using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// AST expression builder
/// </summary>
public interface IExpressionBuilder
{
    /// <summary>
    /// Creates an argument expression.
    /// </summary>
    /// <param name="id">The argument identifier.</param>
    /// <param name="typeId">The argument type.</param>
    /// <returns>The argument expression node.</returns>
    ExpressionNode Argument(ExpressionNode id, ExpressionNode typeId);

    /// <summary>
    /// Creates an assignment expression.
    /// </summary>
    /// <param name="left">The left side of the assignment.</param>
    /// <param name="right">The right side of the assignment.</param>
    /// <param name="op">The assignment operator.</param>
    /// <returns>The assignment expression node.</returns>
    ExpressionNode Assign(ExpressionNode left, ExpressionNode right, string op);

    /// <summary>
    /// Creates an array literal expression.
    /// </summary>
    /// <param name="type">The element type of the array.</param>
    /// <param name="elements">The array elements.</param>
    /// <returns>The array expression node.</returns>
    ExpressionNode Array(string type, IEnumerable<ExpressionNode> elements);

    /// <summary>
    /// Creates a block expression containing multiple statements.
    /// </summary>
    /// <param name="elements">The statements in the block.</param>
    /// <returns>The block expression node.</returns>
    ExpressionNode Block(IEnumerable<ExpressionNode> elements);

    /// <summary>
    /// Creates a binary operation expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="op">The binary operator.</param>
    /// <returns>The binary operation expression node.</returns>
    ExpressionNode BinaryOperation(ExpressionNode left, ExpressionNode right, string op);

    /// <summary>
    /// Creates a break statement.
    /// </summary>
    /// <returns>The break expression node.</returns>
    ExpressionNode Break();

    /// <summary>
    /// Creates a method call expression.
    /// </summary>
    /// <param name="callee">The method being called.</param>
    /// <param name="arguments">The call arguments.</param>
    /// <returns>The call expression node.</returns>
    ExpressionNode Call(ExpressionNode callee, IEnumerable<ExpressionNode> arguments);

    /// <summary>
    /// Creates a catch clause for exception handling.
    /// </summary>
    /// <param name="param">The exception parameter.</param>
    /// <param name="body">The catch block body.</param>
    /// <returns>The catch expression node.</returns>
    ExpressionNode Catch(ExpressionNode param, ExpressionNode body);

    /// <summary>
    /// Creates a conditional (ternary) expression.
    /// </summary>
    /// <param name="test">The condition to test.</param>
    /// <param name="consequent">The expression when condition is true.</param>
    /// <param name="alternate">The expression when condition is false.</returns>
    ExpressionNode Conditional(ExpressionNode test, ExpressionNode consequent, ExpressionNode alternate);

    /// <summary>
    /// Creates a continue statement.
    /// </summary>
    /// <returns>The continue expression node.</returns>
    ExpressionNode Continue();

    /// <summary>
    /// Creates a class constructor definition.
    /// </summary>
    /// <param name="accessState">The access level of the constructor.</param>
    /// <param name="arguments">The constructor parameters.</param>
    /// <param name="statement">The constructor body.</param>
    /// <param name="doc">The documentation comment.</param>
    /// <returns>The constructor expression node.</returns>
    ExpressionNode ClassConstructor(AccessState accessState, IEnumerable<ExpressionNode> arguments, ExpressionNode statement, string doc);

    /// <summary>
    /// Creates a class field definition.
    /// </summary>
    /// <param name="accessState">The access level of the field.</param>
    /// <param name="staticMode">The static/readonly mode.</param>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    /// <param name="init">The initial value, or null.</param>
    /// <param name="doc">The documentation comment.</param>
    /// <returns>The field expression node.</returns>
    ExpressionNode ClassField(AccessState accessState, StaticReadonlyMode staticMode, string name, ExpressionNode type, ExpressionNode init, string doc);

    /// <summary>
    /// Creates a class method definition.
    /// </summary>
    /// <param name="accessState">The access level of the method.</param>
    /// <param name="virtualState">The virtual state of the method.</param>
    /// <param name="isStatic">Whether the method is static.</param>
    /// <param name="name">The method name.</param>
    /// <param name="returnType">The return type.</param>
    /// <param name="arguments">The method parameters.</param>
    /// <param name="statement">The method body.</param>
    /// <param name="doc">The documentation comment.</param>
    /// <returns>The method expression node.</returns>
    ExpressionNode ClassMethod(AccessState accessState, VirtualState virtualState, bool isStatic, string name, ExpressionNode returnType, IEnumerable<ExpressionNode> arguments, ExpressionNode statement, string doc);

    /// <summary>
    /// Creates a comment expression.
    /// </summary>
    /// <param name="comment">The comment text.</param>
    /// <returns>The comment expression node.</returns>
    ExpressionNode Comment(string comment);

    /// <summary>
    /// Creates a default expression for a type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The default expression node.</returns>
    ExpressionNode Default(string typeName);

    /// <summary>
    /// Creates a do-while loop.
    /// </summary>
    /// <param name="body">The loop body.</param>
    /// <param name="test">The loop condition.</param>
    /// <returns>The do-while expression node.</returns>
    ExpressionNode DoWhile(ExpressionNode body, ExpressionNode test);

    /// <summary>
    /// Creates an enum field definition.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="number">The field value.</param>
    /// <param name="exact">Whether the value is exact.</param>
    /// <param name="doc">The documentation comment.</param>
    /// <returns>The enum field expression node.</returns>
    ExpressionNode EnumField(string name, int number, bool exact, string doc);

    /// <summary>
    /// Creates a for loop.
    /// </summary>
    /// <param name="init">The initialization expression.</param>
    /// <param name="test">The loop condition.</param>
    /// <param name="update">The update expression.</param>
    /// <param name="body">The loop body.</param>
    /// <returns>The for loop expression node.</returns>
    ExpressionNode For(ExpressionNode init, ExpressionNode test, ExpressionNode update, ExpressionNode body);

    /// <summary>
    /// Creates a foreach-in loop.
    /// </summary>
    /// <param name="left">The iteration variable.</param>
    /// <param name="right">The collection to iterate.</param>
    /// <param name="body">The loop body.</param>
    /// <returns>The foreach-in expression node.</returns>
    ExpressionNode ForEachIn(ExpressionNode left, ExpressionNode right, ExpressionNode body);

    /// <summary>
    /// Creates an identifier expression.
    /// </summary>
    /// <param name="id">The identifier name.</param>
    /// <returns>The identifier expression node.</returns>
    ExpressionNode Identifier(string id);

    /// <summary>
    /// Creates an if statement.
    /// </summary>
    /// <param name="test">The condition to test.</param>
    /// <param name="consequent">The branch when condition is true.</param>
    /// <param name="alternate">The branch when condition is false.</param>
    /// <returns>The if expression node.</returns>
    ExpressionNode If(ExpressionNode test, ExpressionNode consequent, ExpressionNode alternate);

    /// <summary>
    /// Creates a literal value expression.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>The literal expression node.</returns>
    ExpressionNode Literal(object value);

    /// <summary>
    /// Creates a logical operation expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="op">The logical operator.</param>
    /// <returns>The logical expression node.</returns>
    ExpressionNode Logical(ExpressionNode left, ExpressionNode right, string op);

    /// <summary>
    /// Gets a member
    /// </summary>
    /// <param name="obj">Object</param>
    /// <param name="property">Member property</param>
    /// <param name="computed">false for direct access, true for indirect access</param>
    /// <returns></returns>
    ExpressionNode Member(ExpressionNode obj, ExpressionNode property, bool computed);

    /// <summary>
    /// Creates an object instantiation expression.
    /// </summary>
    /// <param name="calee">The type or constructor to instantiate.</param>
    /// <param name="arguments">The constructor arguments.</param>
    /// <returns>The new expression node.</returns>
    ExpressionNode New(ExpressionNode calee, IEnumerable<ExpressionNode> arguments);

    /// <summary>
    /// Creates an object literal expression.
    /// </summary>
    /// <param name="properties">The object properties.</param>
    /// <returns>The object expression node.</returns>
    ExpressionNode Object(IEnumerable<ExpressionNode> properties);

    /// <summary>
    /// Parses source code into a collection of expression nodes.
    /// </summary>
    /// <param name="code">The source code to parse.</param>
    /// <returns>The parsed expression nodes.</returns>
    IEnumerable<ExpressionNode> Parse(string code);

    /// <summary>
    /// Parses a code string into a single expression.
    /// </summary>
    /// <param name="code">The code to parse.</param>
    /// <returns>The parsed expression node.</returns>
    ExpressionNode ParseExpression(string code);

    /// <summary>
    /// Creates a property key-value pair.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    /// <returns>The property expression node.</returns>
    ExpressionNode Property(string key, ExpressionNode value);

    /// <summary>
    /// Creates a return statement.
    /// </summary>
    /// <param name="argument">The return value, or null.</param>
    /// <returns>The return expression node.</returns>
    ExpressionNode Return(ExpressionNode argument);

    /// <summary>
    /// Creates a sequence expression (comma-separated expressions).
    /// </summary>
    /// <param name="expressions">The expressions in the sequence.</param>
    /// <returns>The sequence expression node.</returns>
    ExpressionNode SequenceExpression(IEnumerable<ExpressionNode> expressions);

    /// <summary>
    /// Creates a source file expression containing namespace and type definitions.
    /// </summary>
    /// <param name="nameSpace">The namespace.</param>
    /// <param name="imports">The import statements.</param>
    /// <param name="typeDefines">The type definitions.</param>
    /// <param name="customBody">Custom body content.</param>
    /// <returns>The source expression node.</returns>
    ExpressionNode Source(string nameSpace, IEnumerable<ImportItem> imports, IEnumerable<ExpressionNode> typeDefines, IEnumerable<ExpressionNode> customBody);

    /// <summary>
    /// Creates a switch statement.
    /// </summary>
    /// <param name="discriminant">The value to switch on.</param>
    /// <param name="cases">The switch cases.</param>
    /// <returns>The switch expression node.</returns>
    ExpressionNode Switch(ExpressionNode discriminant, IEnumerable<ExpressionNode> cases);

    /// <summary>
    /// Creates a switch case.
    /// </summary>
    /// <param name="test">The case test value.</param>
    /// <param name="consequent">The case body.</param>
    /// <returns>The switch case expression node.</returns>
    ExpressionNode SwitchCase(ExpressionNode test, IEnumerable<ExpressionNode> consequent);

    /// <summary>
    /// Creates a this reference expression.
    /// </summary>
    /// <returns>The this expression node.</returns>
    ExpressionNode This();

    /// <summary>
    /// Creates a throw statement.
    /// </summary>
    /// <param name="argument">The exception to throw.</param>
    /// <returns>The throw expression node.</returns>
    ExpressionNode Throw(ExpressionNode argument);

    /// <summary>
    /// Creates a try-catch-finally block.
    /// </summary>
    /// <param name="block">The try block.</param>
    /// <param name="handlers">The catch handlers.</param>
    /// <param name="finalizer">The finally block.</param>
    /// <returns>The try expression node.</returns>
    ExpressionNode Try(ExpressionNode block, IEnumerable<ExpressionNode> handlers, ExpressionNode finalizer);

    /// <summary>
    /// Creates a type expression.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <returns>The type expression node.</returns>
    ExpressionNode Type(TypeDefinition type);

    /// <summary>
    /// Creates a type 'as' expression.
    /// </summary>
    /// <param name="left">The expression to cast.</param>
    /// <param name="right">The target type.</param>
    /// <returns>The type as expression node.</returns>
    ExpressionNode TypeAs(ExpressionNode left, ExpressionNode right);

    /// <summary>
    /// Creates a type cast expression.
    /// </summary>
    /// <param name="left">The expression to cast.</param>
    /// <param name="right">The target type.</param>
    /// <returns>The type cast expression node.</returns>
    ExpressionNode TypeCast(ExpressionNode left, ExpressionNode right);

    /// <summary>
    /// Creates a type definition.
    /// </summary>
    /// <param name="mode">The kind of type.</param>
    /// <param name="accessState">The access level.</param>
    /// <param name="virtualState">The virtual state.</param>
    /// <param name="isStatic">Whether the type is static.</param>
    /// <param name="name">The type name.</param>
    /// <param name="extends">The base class.</param>
    /// <param name="implements">The interfaces to implement.</param>
    /// <param name="body">The type body.</param>
    /// <param name="doc">The documentation comment.</param>
    /// <returns>The type definition expression node.</returns>
    ExpressionNode TypeDefine(TypeKind mode, AccessState accessState, VirtualState virtualState, bool isStatic, string name, string extends, IEnumerable<string> implements, ExpressionNode body, string doc);

    /// <summary>
    /// Creates a typeof expression.
    /// </summary>
    /// <param name="argument">The type argument.</param>
    /// <returns>The typeof expression node.</returns>
    ExpressionNode TypeOf(ExpressionNode argument);

    /// <summary>
    /// Creates a unary operation expression.
    /// </summary>
    /// <param name="argument">The operand.</param>
    /// <param name="op">The unary operator.</param>
    /// <param name="prefix">Whether the operator is prefix.</param>
    /// <returns>The unary expression node.</returns>
    ExpressionNode Unary(ExpressionNode argument, string op, bool prefix);

    /// <summary>
    /// Creates an update (increment/decrement) expression.
    /// </summary>
    /// <param name="argument">The operand.</param>
    /// <param name="op">The update operator.</param>
    /// <param name="prefix">Whether the operator is prefix.</param>
    /// <returns>The update expression node.</returns>
    ExpressionNode Update(ExpressionNode argument, string op, bool prefix);

    /// <summary>
    /// Creates a variable declaration.
    /// </summary>
    /// <param name="id">The variable name.</param>
    /// <param name="typeId">The variable type.</param>
    /// <param name="init">The initial value, or null.</param>
    /// <returns>The variable expression node.</returns>
    ExpressionNode Variable(ExpressionNode id, ExpressionNode typeId, ExpressionNode init);

    /// <summary>
    /// Creates a while loop.
    /// </summary>
    /// <param name="test">The loop condition.</param>
    /// <param name="body">The loop body.</param>
    /// <returns>The while expression node.</returns>
    ExpressionNode While(ExpressionNode test, ExpressionNode body);
}