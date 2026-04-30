using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Expressions
{
    /// <summary>
    /// Provides extension methods for building and manipulating expression nodes.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Determines if the static mode represents a static member.
        /// </summary>
        /// <param name="staticMode">The static readonly mode to check.</param>
        /// <returns>True if the mode is Static, StaticReadonly, or Const; otherwise, false.</returns>
        public static bool IsStatic(this StaticReadonlyMode staticMode) => staticMode switch
        {
            StaticReadonlyMode.Static or StaticReadonlyMode.StaticReadonly or StaticReadonlyMode.Const => true,
            _ => false,
        };

        /// <summary>
        /// Determines if the static mode represents a readonly member.
        /// </summary>
        /// <param name="staticMode">The static readonly mode to check.</param>
        /// <returns>True if the mode is Readonly, StaticReadonly, or Const; otherwise, false.</returns>
        public static bool IsReadonly(this StaticReadonlyMode staticMode) => staticMode switch
        {
            StaticReadonlyMode.Readonly or StaticReadonlyMode.StaticReadonly or StaticReadonlyMode.Const => true,
            _ => false,
        };

        /// <summary>
        /// Creates an argument expression with a type definition.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="typeInfo">The type definition.</param>
        /// <returns>An expression node representing the argument.</returns>
        public static ExpressionNode Argument(this IExpressionBuilder builder, string name, TypeDefinition typeInfo)
        {
            return builder.Argument(builder.Identifier(name), builder.Type(typeInfo));
        }

        /// <summary>
        /// Creates an argument expression with a type name string.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>An expression node representing the argument.</returns>
        public static ExpressionNode Argument(this IExpressionBuilder builder, string name, string typeName)
        {
            return builder.Argument(builder.Identifier(name), builder.Type(typeName));
        }

        /// <summary>
        /// Creates an assignment expression using object and property names.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="obj">The object name.</param>
        /// <param name="property">The property name.</param>
        /// <param name="target">The target expression to assign.</param>
        /// <returns>An expression node representing the assignment.</returns>
        public static ExpressionNode Assign(this IExpressionBuilder builder, string obj, string property, ExpressionNode target)
        {
            var memberExpr = builder.Member(obj, property);
            return builder.Assign(memberExpr, target, "=");
        }

        /// <summary>
        /// Creates an assignment expression using a type definition and property name.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeInfo">The type definition.</param>
        /// <param name="property">The property name.</param>
        /// <param name="target">The target expression to assign.</param>
        /// <returns>An expression node representing the assignment.</returns>
        public static ExpressionNode Assign(this IExpressionBuilder builder, TypeDefinition typeInfo, string property, ExpressionNode target)
        {
            var memberExpr = builder.Member(typeInfo, property);
            return builder.Assign(memberExpr, target, "=");
        }

        /// <summary>
        /// Creates an assignment expression with the '=' operator.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left-hand side expression.</param>
        /// <param name="right">The right-hand side expression.</param>
        /// <returns>An expression node representing the assignment.</returns>
        public static ExpressionNode AssignEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.Assign(left, right, "=");
        }

        /// <summary>
        /// Creates an assignment expression with the '=' operator.
        /// </summary>
        /// <param name="expr">The left-hand side expression.</param>
        /// <param name="right">The right-hand side expression.</param>
        /// <returns>An expression node representing the assignment.</returns>
        public static ExpressionNode AssignEqual(this ExpressionNode expr, ExpressionNode right)
        {
            return ExpressionNode.Builder.Assign(expr, right, "=");
        }

        /// <summary>
        /// Creates a member access expression using object and property names.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="obj">The object name.</param>
        /// <param name="property">The property name.</param>
        /// <param name="computed">Whether this is a computed property access.</param>
        /// <returns>An expression node representing the member access.</returns>
        public static ExpressionNode Member(this IExpressionBuilder builder, string obj, string property, bool computed = false)
        {
            return builder.Member(builder.Identifier(obj), builder.Identifier(property), computed);
        }

        /// <summary>
        /// Creates a member access expression using an expression node and property name.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="obj">The object expression.</param>
        /// <param name="property">The property name.</param>
        /// <param name="computed">Whether this is a computed property access.</param>
        /// <returns>An expression node representing the member access.</returns>
        public static ExpressionNode Member(this IExpressionBuilder builder, ExpressionNode obj, string property, bool computed = false)
        {
            return builder.Member(obj, builder.Identifier(property), computed);
        }

        /// <summary>
        /// Creates a member access expression using a type definition and property name.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeInfo">The type definition.</param>
        /// <param name="property">The property name.</param>
        /// <param name="computed">Whether this is a computed property access.</param>
        /// <returns>An expression node representing the member access.</returns>
        public static ExpressionNode Member(this IExpressionBuilder builder, TypeDefinition typeInfo, string property, bool computed = false)
        {
            return builder.Member(builder.Type(typeInfo), builder.Identifier(property), computed);
        }

        /// <summary>
        /// Creates a dot member access expression.
        /// </summary>
        /// <param name="expr">The object expression.</param>
        /// <param name="property">The property name.</param>
        /// <returns>An expression node representing the member access.</returns>
        public static ExpressionNode Dot(this ExpressionNode expr, string property)
        {
            return ExpressionNode.Builder.Member(expr, property);
        }

        /// <summary>
        /// Creates a function call expression with no arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The function name.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this IExpressionBuilder builder, string name)
        {
            return builder.Call(builder.Identifier(name), []);
        }

        /// <summary>
        /// Creates a function call expression with no arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The function name expression.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this IExpressionBuilder builder, ExpressionNode name)
        {
            return builder.Call(name, []);
        }

        /// <summary>
        /// Creates a function call expression with arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The function name.</param>
        /// <param name="args">The argument expressions.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this IExpressionBuilder builder, string name, params ExpressionNode[] args)
        {
            return builder.Call(builder.Identifier(name), args);
        }

        /// <summary>
        /// Creates a function call expression with arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The function name expression.</param>
        /// <param name="args">The argument expressions.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this IExpressionBuilder builder, ExpressionNode name, params ExpressionNode[] args)
        {
            return builder.Call(name, args);
        }

        /// <summary>
        /// Creates a method call expression on an object.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="obj">The object expression.</param>
        /// <param name="property">The method name.</param>
        /// <param name="args">The argument expressions.</param>
        /// <returns>An expression node representing the method call.</returns>
        public static ExpressionNode Call(this IExpressionBuilder builder, ExpressionNode obj, string property, params ExpressionNode[] args)
        {
            ExpressionNode memberExpr = builder.Member(obj, property);
            return builder.Call(memberExpr, args);
        }

        /// <summary>
        /// Creates a function call expression with arguments.
        /// </summary>
        /// <param name="expr">The function expression.</param>
        /// <param name="args">The argument expressions.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this ExpressionNode expr, params ExpressionNode[] args)
        {
            return ExpressionNode.Builder.Call(expr, args);
        }

        /// <summary>
        /// Creates a function call expression with arguments.
        /// </summary>
        /// <param name="expr">The function expression.</param>
        /// <param name="args">The argument expressions.</param>
        /// <returns>An expression node representing the function call.</returns>
        public static ExpressionNode Call(this ExpressionNode expr, IEnumerable<ExpressionNode> args)
        {
            return ExpressionNode.Builder.Call(expr, args);
        }

        /// <summary>
        /// Creates a type cast expression.
        /// </summary>
        /// <param name="expr">The expression to cast.</param>
        /// <param name="type">The target type.</param>
        /// <returns>An expression node representing the cast.</returns>
        public static ExpressionNode Cast(this ExpressionNode expr, TypeDefinition type)
        {
            var builder = ExpressionNode.Builder;
            return builder.TypeCast(expr, builder.Type(type));
        }

        /// <summary>
        /// Creates a type 'as' expression.
        /// </summary>
        /// <param name="expr">The expression to convert.</param>
        /// <param name="type">The target type.</param>
        /// <returns>An expression node representing the 'as' conversion.</returns>
        public static ExpressionNode As(this ExpressionNode expr, TypeDefinition type)
        {
            var builder = ExpressionNode.Builder;
            return builder.TypeAs(expr, builder.Type(type));
        }

        /// <summary>
        /// Creates a class method expression with a single statement.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="isStatic">Whether the method is static.</param>
        /// <param name="name">The method name.</param>
        /// <param name="returnType">The return type expression.</param>
        /// <param name="arguments">The argument expressions.</param>
        /// <param name="statement">The statement expression.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the class method.</returns>
        public static ExpressionNode ClassMethod(this IExpressionBuilder builder, AccessState accessState, bool isStatic, string name, ExpressionNode returnType, IEnumerable<ExpressionNode> arguments, ExpressionNode statement, string doc)
        {
            return builder.ClassMethod(accessState, VirtualState.Normal, isStatic, name, returnType, arguments, statement, doc);
        }

        /// <summary>
        /// Creates a class method expression with multiple statements.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="isStatic">Whether the method is static.</param>
        /// <param name="name">The method name.</param>
        /// <param name="returnType">The return type expression.</param>
        /// <param name="arguments">The argument expressions.</param>
        /// <param name="statement">The statement expressions.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the class method.</returns>
        public static ExpressionNode ClassMethod(this IExpressionBuilder builder, AccessState accessState, bool isStatic, string name, ExpressionNode returnType, IEnumerable<ExpressionNode> arguments, IEnumerable<ExpressionNode> statement, string doc)
        {
            var block = builder.Block(statement ?? []);
            return builder.ClassMethod(accessState, VirtualState.Normal, isStatic, name, returnType, arguments, block, doc);
        }

        /// <summary>
        /// Creates a class field expression without an initializer.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="staticMode">The static/readonly mode.</param>
        /// <param name="name">The field name.</param>
        /// <param name="type">The type expression.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the class field.</returns>
        public static ExpressionNode ClassField(this IExpressionBuilder builder, AccessState accessState, StaticReadonlyMode staticMode, string name, ExpressionNode type, string doc)
        {
            return builder.ClassField(accessState, staticMode, name, type, null, doc);
        }

        /// <summary>
        /// Creates a logical AND expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the logical AND.</returns>
        public static ExpressionNode LogicalAnd(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.Logical(left, right, "&&");
        }

        /// <summary>
        /// Creates a logical OR expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the logical OR.</returns>
        public static ExpressionNode LogicalOr(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.Logical(left, right, "||");
        }

        /// <summary>
        /// Creates a binary addition expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the addition.</returns>
        public static ExpressionNode BinaryPlug(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "+");
        }

        /// <summary>
        /// Creates a binary subtraction expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the subtraction.</returns>
        public static ExpressionNode BinaryMinus(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "-");
        }

        /// <summary>
        /// Creates a binary multiplication expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the multiplication.</returns>
        public static ExpressionNode BinaryTimes(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "*");
        }

        /// <summary>
        /// Creates a binary division expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the division.</returns>
        public static ExpressionNode BinaryDivide(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "/");
        }

        /// <summary>
        /// Creates a binary modulo expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the modulo.</returns>
        public static ExpressionNode BinaryModulo(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "%");
        }

        /// <summary>
        /// Creates a binary equality comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the equality comparison.</returns>
        public static ExpressionNode BinaryEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "==");
        }

        /// <summary>
        /// Creates a binary inequality comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the inequality comparison.</returns>
        public static ExpressionNode BinaryNotEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "!=");
        }

        /// <summary>
        /// Creates a binary greater-than comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the greater-than comparison.</returns>
        public static ExpressionNode BinaryGreater(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, ">");
        }

        /// <summary>
        /// Creates a binary greater-than-or-equal comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the greater-than-or-equal comparison.</returns>
        public static ExpressionNode BinaryGreaterOrEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, ">=");
        }

        /// <summary>
        /// Creates a binary less-than comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the less-than comparison.</returns>
        public static ExpressionNode BinaryLess(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "<");
        }

        /// <summary>
        /// Creates a binary less-than-or-equal comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the less-than-or-equal comparison.</returns>
        public static ExpressionNode BinaryLessOrEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "<=");
        }

        /// <summary>
        /// Creates a binary strictly equal comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the strictly equal comparison.</returns>
        public static ExpressionNode BinaryStrictlyEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "===");
        }

        /// <summary>
        /// Creates a binary strictly not equal comparison expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the strictly not equal comparison.</returns>
        public static ExpressionNode BinaryStrictlyNotEqual(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "!==");
        }

        /// <summary>
        /// Creates a binary bitwise AND expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the bitwise AND.</returns>
        public static ExpressionNode BinaryBitwiseAnd(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "&");
        }

        /// <summary>
        /// Creates a binary bitwise OR expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the bitwise OR.</returns>
        public static ExpressionNode BinaryBitwiseOr(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "|");
        }

        /// <summary>
        /// Creates a binary bitwise XOR expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the bitwise XOR.</returns>
        public static ExpressionNode BinaryBitwiseXOr(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "^");
        }

        /// <summary>
        /// Creates a binary left shift expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the left shift.</returns>
        public static ExpressionNode BinaryLeftShift(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "<<");
        }

        /// <summary>
        /// Creates a binary right shift expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the right shift.</returns>
        public static ExpressionNode BinaryRightShift(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, ">>");
        }

        /// <summary>
        /// Creates a binary unsigned right shift expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the unsigned right shift.</returns>
        public static ExpressionNode BinaryUnsignedRightShift(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, ">>>");
        }

        /// <summary>
        /// Creates a binary instanceof expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the instanceof check.</returns>
        public static ExpressionNode BinaryInstanceOf(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "instanceof");
        }

        /// <summary>
        /// Creates a binary 'in' expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>An expression node representing the 'in' check.</returns>
        public static ExpressionNode BinaryIn(this IExpressionBuilder builder, ExpressionNode left, ExpressionNode right)
        {
            return builder.BinaryOperation(left, right, "in");
        }

        /// <summary>
        /// Creates a unary plus expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the unary plus.</returns>
        public static ExpressionNode UnaryPlus(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "+", true);
        }

        /// <summary>
        /// Creates a unary minus expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the unary minus.</returns>
        public static ExpressionNode UnaryMinus(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "-", true);
        }

        /// <summary>
        /// Creates a unary post-increment expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the post-increment.</returns>
        public static ExpressionNode UnaryIncrementRight(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "++", false);
        }

        /// <summary>
        /// Creates a unary post-decrement expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the post-decrement.</returns>
        public static ExpressionNode UnaryDecrementRight(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "--", false);
        }

        /// <summary>
        /// Creates a unary bitwise NOT expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the bitwise NOT.</returns>
        public static ExpressionNode UnaryBitwiseNot(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "~", true);
        }

        /// <summary>
        /// Creates a unary logical NOT expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the logical NOT.</returns>
        public static ExpressionNode UnaryLogicalNot(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "!", true);
        }

        /// <summary>
        /// Creates a unary delete expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the delete operation.</returns>
        public static ExpressionNode UnaryDelete(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "delete", true);
        }

        /// <summary>
        /// Creates a unary void expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the void operation.</returns>
        public static ExpressionNode UnaryVoid(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "void", true);
        }

        /// <summary>
        /// Creates a unary typeof expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="argument">The argument expression.</param>
        /// <returns>An expression node representing the typeof operation.</returns>
        public static ExpressionNode UnaryTypeOf(this IExpressionBuilder builder, ExpressionNode argument)
        {
            return builder.Unary(argument, "typeof", true);
        }

        /// <summary>
        /// Creates a new object expression with no arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>An expression node representing the new object.</returns>
        public static ExpressionNode New(this IExpressionBuilder builder, string typeName)
        {
            return builder.New(builder.Type(typeName), []);
        }

        /// <summary>
        /// Creates a new object expression with no arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeName">The type name expression.</param>
        /// <returns>An expression node representing the new object.</returns>
        public static ExpressionNode New(this IExpressionBuilder builder, ExpressionNode typeName)
        {
            return builder.New(typeName, []);
        }

        /// <summary>
        /// Creates a new object expression with arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeName">The type name.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>An expression node representing the new object.</returns>
        public static ExpressionNode New(this IExpressionBuilder builder, string typeName, params ExpressionNode[] args)
        {
            return builder.New(builder.Type(typeName), args);
        }

        /// <summary>
        /// Creates a new object expression with no arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeInfo">The type definition.</param>
        /// <returns>An expression node representing the new object.</returns>
        public static ExpressionNode New(this IExpressionBuilder builder, TypeDefinition typeInfo)
        {
            return builder.New(builder.Type(typeInfo), []);
        }

        /// <summary>
        /// Creates a new object expression with arguments.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeInfo">The type definition.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>An expression node representing the new object.</returns>
        public static ExpressionNode New(this IExpressionBuilder builder, TypeDefinition typeInfo, params ExpressionNode[] args)
        {
            return builder.New(builder.Type(typeInfo), args);
        }

        /// <summary>
        /// Parses a format string into expression nodes.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arguments">The format arguments.</param>
        /// <returns>An enumerable of parsed expression nodes.</returns>
        public static IEnumerable<ExpressionNode> Parse(this IExpressionBuilder builder, string format, params object[] arguments)
        {
            string code;
            try
            {
                code = string.Format(format, arguments);
            }
            catch (Exception)
            {
                throw;
            }

            return builder.Parse(code);
        }

        /// <summary>
        /// Parses code into a block expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="code">The code string to parse.</param>
        /// <returns>An expression node representing the block.</returns>
        public static ExpressionNode ParseToBlock(this IExpressionBuilder builder, string code)
        {
            var results = builder.Parse(code);
            return builder.Block(results);
        }

        /// <summary>
        /// Parses a format string into a block expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arguments">The format arguments.</param>
        /// <returns>An expression node representing the block.</returns>
        public static ExpressionNode ParseToBlock(this IExpressionBuilder builder, string format, params object[] arguments)
        {
            string code;
            try
            {
                code = string.Format(format, arguments);
            }
            catch (Exception)
            {
                throw;
            }

            var results = builder.Parse(code);
            return builder.Block(results);
        }

        /// <summary>
        /// Creates a source expression with a single type definition.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="typeDefine">The type definition expression.</param>
        /// <param name="customBody">Custom body expressions.</param>
        /// <returns>An expression node representing the source.</returns>
        public static ExpressionNode Source(this IExpressionBuilder builder, string nameSpace, ExpressionNode typeDefine = null, IEnumerable<ExpressionNode> customBody = null)
        {
            return builder.Source(nameSpace, [], [typeDefine], customBody);
        }

        /// <summary>
        /// Creates a source expression with multiple type definitions.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="typeDefines">The type definition expressions.</param>
        /// <returns>An expression node representing the source.</returns>
        public static ExpressionNode Source(this IExpressionBuilder builder, string nameSpace, IEnumerable<ExpressionNode> typeDefines)
        {
            return builder.Source(nameSpace, [], typeDefines, null);
        }

        /// <summary>
        /// Creates a type expression from a type name string.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>An expression node representing the type.</returns>
        public static ExpressionNode Type(this IExpressionBuilder builder, string typeName)
        {
            // Convert to non-path resource name here, otherwise it will be ToLower
            TypeDefinition typeInfo = TypeDefinition.Resolve(typeName, true);
            return builder.Type(typeInfo);
        }

        /// <summary>
        /// Creates a typeof expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="type">The type definition.</param>
        /// <returns>An expression node representing the typeof expression.</returns>
        public static ExpressionNode TypeOf(this IExpressionBuilder builder, TypeDefinition type)
        {
            return builder.TypeOf(builder.Type(type));
        }

        /// <summary>
        /// Creates a type definition expression with full parameters.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="mode">The type kind.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="virtualState">The virtual state.</param>
        /// <param name="isStatic">Whether the type is static.</param>
        /// <param name="name">The type name.</param>
        /// <param name="extends">The base type name.</param>
        /// <param name="implements">The implemented interface names.</param>
        /// <param name="body">The body expression.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the type definition.</returns>
        public static ExpressionNode TypeDefine(this IExpressionBuilder builder, TypeKind mode, AccessState accessState, VirtualState virtualState, bool isStatic, string name, string extends, IEnumerable<string> implements, ExpressionNode body, string doc)
        {
            return builder.TypeDefine(mode, accessState, virtualState, isStatic, name, extends, implements, body, doc);
        }

        /// <summary>
        /// Creates a type definition expression without inheritance.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeKind">The type kind.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="virtualState">The virtual state.</param>
        /// <param name="isStatic">Whether the type is static.</param>
        /// <param name="name">The type name.</param>
        /// <param name="body">The body expression.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the type definition.</returns>
        public static ExpressionNode TypeDefine(this IExpressionBuilder builder, TypeKind typeKind, AccessState accessState, VirtualState virtualState, bool isStatic, string name, ExpressionNode body, string doc)
        {
            return builder.TypeDefine(typeKind, accessState, virtualState, isStatic, name, null, [], body, doc);
        }

        /// <summary>
        /// Creates a type definition expression with a base type.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="typeKind">The type kind.</param>
        /// <param name="accessState">The access state.</param>
        /// <param name="virtualState">The virtual state.</param>
        /// <param name="isStatic">Whether the type is static.</param>
        /// <param name="name">The type name.</param>
        /// <param name="extends">The base type name.</param>
        /// <param name="body">The body expression.</param>
        /// <param name="doc">The documentation string.</param>
        /// <returns>An expression node representing the type definition.</returns>
        public static ExpressionNode TypeDefine(this IExpressionBuilder builder, TypeKind typeKind, AccessState accessState, VirtualState virtualState, bool isStatic, string name, string extends, ExpressionNode body, string doc)
        {
            return builder.TypeDefine(typeKind, accessState, virtualState, isStatic, name, extends, [], body, doc);
        }

        /// <summary>
        /// Creates a variable declaration expression with a type name string.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="typeName">The type name.</param>
        /// <param name="init">The optional initializer expression.</param>
        /// <returns>An expression node representing the variable declaration.</returns>
        public static ExpressionNode Variable(this IExpressionBuilder builder, string name, string typeName, ExpressionNode init = null)
        {
            return builder.Variable(builder.Identifier(name), builder.Type(typeName), init);
        }

        /// <summary>
        /// Creates a variable declaration expression with a type definition.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="type">The type definition.</param>
        /// <param name="init">The optional initializer expression.</param>
        /// <returns>An expression node representing the variable declaration.</returns>
        public static ExpressionNode Variable(this IExpressionBuilder builder, string name, TypeDefinition type, ExpressionNode init = null)
        {
            return builder.Variable(builder.Identifier(name), builder.Type(type), init);
        }

        /// <summary>
        /// Creates a true literal expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <returns>An expression node representing the true literal.</returns>
        public static ExpressionNode True(this IExpressionBuilder builder)
        {
            return builder.Literal(true);
        }

        /// <summary>
        /// Creates a false literal expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <returns>An expression node representing the false literal.</returns>
        public static ExpressionNode False(this IExpressionBuilder builder)
        {
            return builder.Literal(false);
        }

        /// <summary>
        /// Creates a null literal expression.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <returns>An expression node representing the null literal.</returns>
        public static ExpressionNode Null(this IExpressionBuilder builder)
        {
            return builder.Literal(null);
        }

        /// <summary>
        /// Gets the expression representation of an SObject.
        /// </summary>
        /// <param name="obj">The SObject.</param>
        /// <param name="context">The expression context.</param>
        /// <returns>An expression node representing the SObject.</returns>
        public static ExpressionNode GetExpression(this SObject obj, ExpressionContext context)
        {
            return EditorExpressions.EditorObjects.GetEditorExpression(obj, context);
        }

        /// <summary>
        /// Gets the expression representation of an SArray.
        /// </summary>
        /// <param name="ary">The SArray.</param>
        /// <param name="context">The expression context.</param>
        /// <returns>An expression node representing the SArray.</returns>
        public static ExpressionNode GetExpression(this SArray ary, ExpressionContext context)
        {
            return EditorExpressions.EditorObjects.GetEditorExpression(ary, context);
        }

        /// <summary>
        /// Gets the expression representation of an SEnum.
        /// </summary>
        /// <param name="e">The SEnum.</param>
        /// <param name="context">The expression context.</param>
        /// <returns>An expression node representing the SEnum.</returns>
        public static ExpressionNode GetExpression(this SEnum e, ExpressionContext context)
        {
            return EditorExpressions.EditorObjects.GetEditorExpression(e, context);
        }

        /// <summary>
        /// Gets the expression representation of an SKey.
        /// </summary>
        /// <param name="key">The SKey.</param>
        /// <param name="context">The expression context.</param>
        /// <returns>An expression node representing the SKey.</returns>
        public static ExpressionNode GetExpression(this SKey key, ExpressionContext context)
        {
            return EditorExpressions.EditorObjects.GetEditorExpression(key, context);
        }

        /// <summary>
        /// Wraps an expression node into a single-element array.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>An array containing the expression node.</returns>
        public static ExpressionNode[] ToArray(this ExpressionNode expression)
        {
            return [expression];
        }
    }
}