using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Parser.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender.Ast;

/// <summary>
/// Abstract base class for rendering code in a specific language from AST nodes.
/// </summary>
public abstract class RenderLanguage : Asset, IRenderLanguage
{
    /// <summary>
    /// Gets a render language by name from the asset manager.
    /// </summary>
    /// <param name="name">The name of the language to retrieve.</param>
    /// <returns>The <see cref="RenderLanguage"/> instance, or null if not found.</returns>
    public static RenderLanguage GetLanguage(string name)
    {
        return AssetManager.Instance.GetAsset<RenderLanguage>("*RenderLanguage:" + name);
    }

    /// <inheritdoc/>
    public int IndentLength => 4;

    /// <inheritdoc/>
    public string LanguageName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderLanguage"/> class.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    public RenderLanguage(string name)
        : base($"*{nameof(RenderLanguage)}:" + name)
    {
        LanguageName = name;

        UpdateAssetTypes(typeof(RenderLanguage), typeof(IRenderLanguage));

        ResolveId(IdResolveType.FullName);
    }

    /// <inheritdoc/>
    public virtual CodeSegmentConfig SegmentConfig => CodeSegmentConfig.CsDefault;

    /// <summary>
    /// Renders a source node.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The source node to render.</param>
    public virtual void RenderSource(IAstExpressionWriter writer, ExpressionContext context, SourceNode node)
    {
    }

    /// <summary>
    /// Renders a type node.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The type node to render.</param>
    public virtual void RenderTypeNode(IAstExpressionWriter writer, ExpressionContext context, TypeNode node)
    {
    }

    /// <summary>
    /// Renders a class field.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The class field node to render.</param>
    public virtual void RenderClassField(IAstExpressionWriter writer, ExpressionContext context, ClassField node)
    {
    }

    /// <summary>
    /// Renders an enum field.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The enum field node to render.</param>
    public virtual void RenderEnumField(IAstExpressionWriter writer, ExpressionContext context, EnumField node)
    {
    }

    /// <summary>
    /// Renders a class method.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The class method node to render.</param>
    public virtual void RenderClassMethod(IAstExpressionWriter writer, ExpressionContext context, ClassMethod node)
    {
    }

    /// <summary>
    /// Renders native code in a class.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The native code node to render.</param>
    public virtual void RenderClassNativeCode(IAstExpressionWriter writer, ExpressionContext context, ClassNativeCode node)
    {
    }

    /// <summary>
    /// Renders a comment node.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The comment node to render.</param>
    public virtual void RenderComment(IAstExpressionWriter writer, ExpressionContext context, CommentNode node)
    {
        writer.String("//" + node.Comment);
        writer.NewLine();
    }

    /// <summary>
    /// Renders an assignment expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The assignment expression to render.</param>
    public virtual void RenderAssignmentExpression(IAstExpressionWriter writer, ExpressionContext context, AssignmentExpression node)
    {
        writer.Syntax(node.Left);
        writer.Space();
        writer.Operator(GetEcmaStandardAssignmentOperator(node.Operator));
        writer.Space();
        writer.Syntax(node.Right);
    }

    /// <summary>
    /// Renders an array expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The array expression to render.</param>
    public virtual void RenderArrayExpression(IAstExpressionWriter writer, ExpressionContext context, ArrayExpression node)
    {
        writer.Operator("[");
        WriteSequenceSyntax(writer, node.Elements, ",");
        writer.Operator("]");
    }

    /// <summary>
    /// Renders a block statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The block statement to render.</param>
    public virtual void RenderBlockStatement(IAstExpressionWriter writer, ExpressionContext context, BlockStatement node)
    {
        writer.Operator("{");
        writer.NewLine();
        writer.IndentBegin();
        foreach (var statement in node.Body)
        {
            writer.Syntax(statement);
            // BlockStatement and CommentNode handle their own semicolons
            if (statement is not BlockStatement && statement is not CommentNode)
            {
                writer.Operator(";");
            }
            writer.NewLine();
        }
        writer.IndentEnd();
        writer.Operator("}");
    }

    /// <summary>
    /// Renders a binary expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The binary expression to render.</param>
    public virtual void RenderBinaryExpression(IAstExpressionWriter writer, ExpressionContext context, BinaryExpression node)
    {
        WriteExpression(writer, node, node.Left);
        writer.Space();
        writer.Operator(GetEcmaStandardBinaryOperator(node.Operator));
        writer.Space();
        WriteExpression(writer, node, node.Right);
    }

    /// <summary>
    /// Renders a break statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The break statement to render.</param>
    public virtual void RenderBreakStatement(IAstExpressionWriter writer, ExpressionContext context, BreakStatement node)
    {
        writer.Keyword("break");
    }

    /// <summary>
    /// Renders a call expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The call expression to render.</param>
    public virtual void RenderCallExpression(IAstExpressionWriter writer, ExpressionContext context, CallExpression node)
    {
        writer.Syntax(node.Callee);
        writer.Operator("(");
        WriteSequenceSyntax(writer, node.Arguments, ",");
        writer.Operator(")");
    }

    /// <summary>
    /// Renders a catch clause.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The catch clause to render.</param>
    public virtual void RenderCatchClause(IAstExpressionWriter writer, ExpressionContext context, CatchClause node)
    {
        writer.Keyword("catch");
        writer.Space();
        writer.Syntax(node.Param);
        writer.NewLine();
        writer.Syntax(node.Body);
    }

    /// <summary>
    /// Renders a conditional (ternary) expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The conditional expression to render.</param>
    public virtual void RenderConditionalExpression(IAstExpressionWriter writer, ExpressionContext context, ConditionalExpression node)
    {
        writer.Syntax(node.Test);
        writer.Space();
        writer.Operator("?");
        writer.Space();
        writer.Syntax(node.Consequent);
        writer.Space();
        writer.Operator(":");
        writer.Space();
        writer.Syntax(node.Alternate);
    }

    /// <summary>
    /// Renders a continue statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The continue statement to render.</param>
    public virtual void RenderContinueStatement(IAstExpressionWriter writer, ExpressionContext context, ContinueStatement node)
    {
        writer.Keyword("continue");
    }

    /// <summary>
    /// Renders a default expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The default expression to render.</param>
    public virtual void RenderDefaultExpression(IAstExpressionWriter writer, ExpressionContext context, DefaultExpression node)
    {
        writer.Keyword("null");
    }

    /// <summary>
    /// Renders a do-while statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The do-while statement to render.</param>
    public virtual void RenderDoWhileStatement(IAstExpressionWriter writer, ExpressionContext context, DoWhileStatement node)
    {
        writer.Keyword("do");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
        writer.NewLine();
        writer.Keyword("while");
        writer.Space();
        writer.Keyword("(");
        writer.Syntax(node.Test);
        writer.Keyword(")");
        writer.Keyword(";");
    }

    /// <summary>
    /// Renders a debugger statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The debugger statement to render.</param>
    public virtual void RenderDebuggerStatement(IAstExpressionWriter writer, ExpressionContext context, DebuggerStatement node)
    {
    }

    /// <summary>
    /// Renders an empty statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The empty statement to render.</param>
    public virtual void RenderEmptyStatement(IAstExpressionWriter writer, ExpressionContext context, EmptyStatement node)
    {
    }

    /// <summary>
    /// Renders an expression statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The expression statement to render.</param>
    public virtual void RenderExpressionStatement(IAstExpressionWriter writer, ExpressionContext context, ExpressionStatement node)
    {
        writer.Syntax(node.Expression);
    }

    /// <summary>
    /// Renders a for statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The for statement to render.</param>
    public virtual void RenderForStatement(IAstExpressionWriter writer, ExpressionContext context, ForStatement node)
    {
        writer.Keyword("for");
        writer.Space();
        writer.Operator("(");
        writer.Syntax(node.Init);
        writer.Operator(";");
        writer.Space();
        writer.Syntax(node.Test);
        writer.Operator(";");
        writer.Space();
        writer.Syntax(node.Update);
        writer.Operator(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
    }

    /// <summary>
    /// Renders a for-in statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The for-in statement to render.</param>
    public virtual void RenderForInStatement(IAstExpressionWriter writer, ExpressionContext context, ForInStatement node)
    {
        writer.Keyword("for");
        writer.Space();
        if (node.Each)
        {
            writer.Keyword("each");
            writer.Space();
        }
        writer.Operator("(");
        writer.Syntax(node.Left);
        writer.Space();
        writer.Keyword("in");
        writer.Space();
        writer.Syntax(node.Right);
        WriteSingleStatement(writer, node.Body);
    }

    /// <summary>
    /// Renders a function declaration.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The function declaration to render.</param>
    public virtual void RenderFunctionDeclaration(IAstExpressionWriter writer, ExpressionContext context, FunctionDeclaration node)
    {
        writer.Keyword("function");
        writer.Space();
        writer.Syntax(node.Id);
        writer.Operator("(");
        WriteSequenceSyntax(writer, node.Parameters, ",");
        writer.Operator(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
    }

    /// <summary>
    /// Renders a function expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The function expression to render.</param>
    public virtual void RenderFunctionExpression(IAstExpressionWriter writer, ExpressionContext context, FunctionExpression node)
    {
        if (node.Id != null)
        {
            writer.Syntax(node.Id);
        }
        writer.Operator("(");
        WriteSequenceSyntax(writer, node.Parameters, ",");
        writer.Operator(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
    }

    /// <summary>
    /// Renders an if statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The if statement to render.</param>
    public virtual void RenderIfStatement(IAstExpressionWriter writer, ExpressionContext context, IfStatement node)
    {
        writer.Keyword("if");
        writer.Space();
        writer.Operator("(");
        writer.Syntax(node.Test);
        writer.Operator(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Consequent);
        if (node.Alternate != null)
        {
            writer.NewLine();
            writer.Keyword("else");
            writer.NewLine();
            WriteSingleStatement(writer, node.Alternate);
        }
    }

    /// <summary>
    /// Renders a literal value.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The literal node to render.</param>
    public virtual void RenderLiteral(IAstExpressionWriter writer, ExpressionContext context, Literal node)
    {
        if (node.Value != null)
        {
            if (node.Value is string)
            {
                writer.DoubleQuotString(node.Value.ToString());
            }
            else if (node.Value is bool v)
            {
                if (v)
                {
                    writer.Keyword("true");
                }
                else
                {
                    writer.Keyword("false");
                }
            }
            else
            {
                writer.String(node.Value.ToString());
            }
        }
        else
        {
            writer.Keyword("null");
        }
    }

    /// <summary>
    /// Renders a logical expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The logical expression to render.</param>
    public virtual void RenderLogicalExpression(IAstExpressionWriter writer, ExpressionContext context, LogicalExpression node)
    {
        writer.Syntax(node.Left);
        writer.Space();
        writer.Operator(GetEcmaStandardLogicalOperator(node.Operator));
        writer.Space();
        writer.Syntax(node.Right);
    }

    /// <summary>
    /// Renders a member expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The member expression to render.</param>
    public virtual void RenderMemberExpression(IAstExpressionWriter writer, ExpressionContext context, MemberExpression node)
    {
        if (!node.Computed)
        {
            WriteExpression(writer, node, node.Object);
            writer.Operator(".");
            writer.Syntax(node.Property);
        }
        else
        {
            WriteExpression(writer, node, node.Object);
            writer.Operator("[");
            writer.Syntax(node.Property);
            writer.Operator("]");
        }
    }

    /// <summary>
    /// Renders a new expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The new expression to render.</param>
    public virtual void RenderNewExpression(IAstExpressionWriter writer, ExpressionContext context, NewExpression node)
    {
        writer.Keyword("new");
        writer.Space();
        writer.TypeInfo(node.Callee);
        writer.Operator("(");
        WriteSequenceSyntax(writer, node.Arguments, ",");
        writer.Operator(")");
    }

    /// <summary>
    /// Renders an object expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The object expression to render.</param>
    public virtual void RenderObjectExpression(IAstExpressionWriter writer, ExpressionContext context, ObjectExpression node)
    {
        writer.Operator("{");
        WriteSequenceSyntax(writer, node.Properties, ",");
        writer.Operator("}");
    }

    /// <summary>
    /// Renders a program node.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The program node to render.</param>
    public virtual void RenderProgram(IAstExpressionWriter writer, ExpressionContext context, Program node)
    {
    }

    /// <summary>
    /// Renders a property in an object expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The property node to render.</param>
    public virtual void RenderProperty(IAstExpressionWriter writer, ExpressionContext context, Property node)
    {
        switch (node.Kind)
        {
            case PropertyKind.Data:
                writer.Identifier(node.Key.GetKey());
                writer.Operator(":");
                writer.Syntax(node.Value);
                break;

            case PropertyKind.Get:
                writer.Keyword("get");
                writer.Space();
                writer.Identifier(node.Key.GetKey());
                writer.Syntax(node.Value);
                break;

            case PropertyKind.Set:
                writer.Keyword("set");
                writer.Space();
                writer.Identifier(node.Key.GetKey());
                writer.Syntax(node.Value);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Renders a return statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The return statement to render.</param>
    public virtual void RenderReturnStatement(IAstExpressionWriter writer, ExpressionContext context, ReturnStatement node)
    {
        writer.Keyword("return");
        if (node.Argument != null)
        {
            writer.Space();
            writer.Syntax(node.Argument);
        }
    }

    /// <summary>
    /// Renders a sequence expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The sequence expression to render.</param>
    public virtual void RenderSequenceExpression(IAstExpressionWriter writer, ExpressionContext context, SequenceExpression node)
    {
        for (int i = 0; i < node.Expressions.Count; i++)
        {
            if (i > 0)
            {
                writer.Operator(",");
                writer.Space();
            }
            writer.Syntax(node.Expressions[i]);
        }
    }

    /// <summary>
    /// Renders a switch statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The switch statement to render.</param>
    public virtual void RenderSwitchStatement(IAstExpressionWriter writer, ExpressionContext context, SwitchStatement node)
    {
        writer.Keyword("switch");
        writer.Space();
        writer.Operator("(");
        writer.Syntax(node.Discriminant);
        writer.Operator(")");
        writer.NewLine();
        writer.Operator("{");
        writer.NewLine();
        writer.IndentBegin();
        foreach (var item in node.Cases)
        {
            writer.Syntax(item);
            writer.NewLine();
        }
        writer.IndentEnd();
        writer.Operator("}");
    }

    /// <summary>
    /// Renders a switch case.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The switch case to render.</param>
    public virtual void RenderSwitchCase(IAstExpressionWriter writer, ExpressionContext context, SwitchCase node)
    {
        if (node.Test != null)
        {
            writer.Keyword("case");
            writer.Space();
            writer.Syntax(node.Test);
        }
        else
        {
            writer.Keyword("default");
        }
        writer.Keyword(":");
        if (node.Consequent.Any())
        {
            writer.NewLine();
            writer.IndentBegin();
            foreach (var item in node.Consequent)
            {
                writer.Syntax(item);
                writer.Operator(";");
                writer.NewLine();
            }
            writer.IndentEnd();
        }
    }

    /// <summary>
    /// Renders a this expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The this expression to render.</param>
    public virtual void RenderThisExpression(IAstExpressionWriter writer, ExpressionContext context, ThisExpression node)
    {
        writer.Keyword("this");
    }

    /// <summary>
    /// Renders a throw statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The throw statement to render.</param>
    public virtual void RenderThrowStatement(IAstExpressionWriter writer, ExpressionContext context, ThrowStatement node)
    {
        writer.Keyword("throw");
        writer.Space();
        writer.Syntax(node.Argument);
    }

    /// <summary>
    /// Renders a try statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The try statement to render.</param>
    public virtual void RenderTryStatement(IAstExpressionWriter writer, ExpressionContext context, TryStatement node)
    {
        writer.Keyword("try");
        writer.NewLine();
        WriteSingleStatement(writer, node.Block);
        writer.NewLine();
        foreach (var item in node.Handlers)
        {
            writer.Syntax(item);
            writer.NewLine();
        }
        if (node.Finalizer != null)
        {
            writer.Keyword("finally");
            writer.NewLine();
            WriteSingleStatement(writer, node.Finalizer);
        }
    }

    /// <summary>
    /// Renders a type expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The type expression to render.</param>
    public virtual void RenderTypeExpression(IAstExpressionWriter writer, ExpressionContext context, TypeExpression node)
    {
        WriteExpression(writer, node, node.Left);
    }

    /// <summary>
    /// Renders a unary expression.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The unary expression to render.</param>
    public virtual void RenderUnaryExpression(IAstExpressionWriter writer, ExpressionContext context, UnaryExpression node)
    {
        if (node.Prefix)
        {
            writer.Operator(GetEcmaStandardUnaryOpeartor(node.Operator));
            switch (node.Operator)
            {
                case UnaryOperator.Delete:
                case UnaryOperator.Void:
                case UnaryOperator.TypeOf:
                    writer.Space();
                    break;

                default:
                    break;
            }
            WriteExpression(writer, node, node.Argument);
        }
        else
        {
            WriteExpression(writer, node, node.Argument);
            writer.Operator(GetEcmaStandardUnaryOpeartor(node.Operator));
        }
    }

    /// <summary>
    /// Renders an update expression (increment/decrement).
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The update expression to render.</param>
    public virtual void RenderUpdateExpression(IAstExpressionWriter writer, ExpressionContext context, UpdateExpression node)
    {
        if (node.Prefix)
        {
            writer.Operator(GetEcmaStandardUnaryOpeartor(node.Operator));
            switch (node.Operator)
            {
                case UnaryOperator.Delete:
                case UnaryOperator.Void:
                case UnaryOperator.TypeOf:
                    writer.Space();
                    break;

                default:
                    break;
            }
            WriteExpression(writer, node, node.Argument);
        }
        else
        {
            WriteExpression(writer, node, node.Argument);
            writer.Operator(GetEcmaStandardUnaryOpeartor(node.Operator));
        }
    }

    /// <summary>
    /// Renders a variable declaration.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The variable declaration to render.</param>
    public virtual void RenderVariableDeclaration(IAstExpressionWriter writer, ExpressionContext context, VariableDeclaration node)
    {
        writer.Keyword(node.Kind);
        writer.Space();
        WriteSequenceSyntax(writer, node.Declarations, ",");
    }

    /// <summary>
    /// Renders a variable declarator.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The variable declarator to render.</param>
    public virtual void RenderVariableDeclarator(IAstExpressionWriter writer, ExpressionContext context, VariableDeclarator node)
    {
        writer.Syntax(node.Id);
        if (node.Init != null)
        {
            writer.Space();
            writer.Operator("=");
            writer.Space();
            writer.Syntax(node.Init);
        }
    }

    /// <summary>
    /// Renders a while statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The while statement to render.</param>
    public virtual void RenderWhileStatement(IAstExpressionWriter writer, ExpressionContext context, WhileStatement node)
    {
        writer.Keyword("while");
        writer.Space();
        writer.Operator("(");
        writer.Syntax(node.Test);
        writer.Operator(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
    }

    /// <summary>
    /// Renders a with statement.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="node">The with statement to render.</param>
    public virtual void RenderWithStatement(IAstExpressionWriter writer, ExpressionContext context, WithStatement node)
    {
        writer.Keyword("with");
        writer.Space();
        writer.Keyword("(");
        writer.Syntax(node.Object);
        writer.Keyword(")");
        writer.NewLine();
        WriteSingleStatement(writer, node.Body);
    }

    /// <inheritdoc/>
    public virtual string ResolveTypeName(TypeDefinition typeInfo, ExpressionContext context, bool forceShortName)
    {
        return GetElementTypeName(context, typeInfo, forceShortName);
    }

    /// <inheritdoc/>
    public virtual string GetCommentLine(string str)
    {
        return $"// {str}";
    }

    /// <summary>
    /// Writes an expression, adding parentheses if needed based on operator precedence.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="parentExp">The parent expression for precedence comparison.</param>
    /// <param name="exp">The expression to write.</param>
    protected virtual void WriteExpression(IAstExpressionWriter writer, Expression parentExp, Expression exp)
    {
        if (GetExpressionLevel(parentExp) > GetExpressionLevel(exp))
        {
            writer.Operator("(");
            writer.Syntax(exp);
            writer.Operator(")");
        }
        else
        {
            writer.Syntax(exp);
        }
    }

    /// <summary>
    /// Writes a single statement, handling indentation and semicolons appropriately.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="statement">The statement to write.</param>
    protected virtual void WriteSingleStatement(IAstExpressionWriter writer, StatementNode statement)
    {
        if ((statement is BlockStatement || statement is CommentNode))
        {
            writer.Syntax(statement);
        }
        else
        {
            writer.IndentBegin();
            writer.Syntax(statement);
            writer.Operator(";");
            writer.IndentEnd();
        }
    }

    /// <summary>
    /// Writes a sequence of syntax nodes separated by a separator.
    /// </summary>
    /// <param name="writer">The AST expression writer.</param>
    /// <param name="nodes">The collection of nodes to write.</param>
    /// <param name="seperator">The separator string between nodes.</param>
    /// <param name="newLine">If true, adds a new line after each separator instead of a space.</param>
    protected virtual void WriteSequenceSyntax(IAstExpressionWriter writer, IEnumerable<SyntaxNode> nodes, string seperator, bool newLine = false)
    {
        int num = 0;
        foreach (var item in nodes)
        {
            if (num > 0)
            {
                writer.Operator(seperator);
                if (newLine)
                {
                    writer.NewLine();
                }
                else
                {
                    writer.Space();
                }
            }
            writer.Syntax(item);
            num++;
        }
    }

    /// <summary>
    /// Gets the ECMA standard string representation of a unary operator.
    /// </summary>
    /// <param name="op">The unary operator.</param>
    /// <returns>The string representation of the operator.</returns>
    protected string GetEcmaStandardUnaryOpeartor(UnaryOperator op)
    {
        switch (op)
        {
            case UnaryOperator.Plus:
                return "+";

            case UnaryOperator.Minus:
                return "-";

            case UnaryOperator.BitwiseNot:
                return "~";

            case UnaryOperator.LogicalNot:
                return "!";

            case UnaryOperator.Delete:
                return "delete";

            case UnaryOperator.Void:
                return "void";

            case UnaryOperator.TypeOf:
                return "typeof";

            case UnaryOperator.Increment:
                return "++";

            case UnaryOperator.Decrement:
                return "--";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gets the ECMA standard string representation of an assignment operator.
    /// </summary>
    /// <param name="op">The assignment operator.</param>
    /// <returns>The string representation of the operator.</returns>
    protected string GetEcmaStandardAssignmentOperator(AssignmentOperator op)
    {
        switch (op)
        {
            case AssignmentOperator.Assign:
                return "=";

            case AssignmentOperator.PlusAssign:
                return "+=";

            case AssignmentOperator.MinusAssign:
                return "-=";

            case AssignmentOperator.TimesAssign:
                return "*=";

            case AssignmentOperator.DivideAssign:
                return "/=";

            case AssignmentOperator.ModuloAssign:
                return "%=";

            case AssignmentOperator.BitwiseAndAssign:
                return "&=";

            case AssignmentOperator.BitwiseOrAssign:
                return "|=";

            case AssignmentOperator.BitwiseXOrAssign:
                return "^=";

            case AssignmentOperator.LeftShiftAssign:
                return "<<=";

            case AssignmentOperator.RightShiftAssign:
                return ">>=";

            case AssignmentOperator.UnsignedRightShiftAssign:
                return ">>>=";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gets the ECMA standard string representation of a binary operator.
    /// </summary>
    /// <param name="op">The binary operator.</param>
    /// <returns>The string representation of the operator.</returns>
    protected string GetEcmaStandardBinaryOperator(BinaryOperator op)
    {
        switch (op)
        {
            case BinaryOperator.Plus:
                return "+";

            case BinaryOperator.Minus:
                return "-";

            case BinaryOperator.Times:
                return "*";

            case BinaryOperator.Divide:
                return "/";

            case BinaryOperator.Modulo:
                return "%";

            case BinaryOperator.Equal:
                return "==";

            case BinaryOperator.NotEqual:
                return "!=";

            case BinaryOperator.Greater:
                return ">";

            case BinaryOperator.GreaterOrEqual:
                return ">=";

            case BinaryOperator.Less:
                return "<";

            case BinaryOperator.LessOrEqual:
                return "<=";

            case BinaryOperator.StrictlyEqual:
                return "===";

            case BinaryOperator.StricltyNotEqual:
                return "!==";

            case BinaryOperator.BitwiseAnd:
                return "&";

            case BinaryOperator.BitwiseOr:
                return "|";

            case BinaryOperator.BitwiseXOr:
                return "^";

            case BinaryOperator.LeftShift:
                return "<<";

            case BinaryOperator.RightShift:
                return ">>";

            case BinaryOperator.UnsignedRightShift:
                return ">>>";

            case BinaryOperator.InstanceOf:
                return "instanceof";

            case BinaryOperator.In:
                return "in";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gets the ECMA standard string representation of a logical operator.
    /// </summary>
    /// <param name="op">The logical operator.</param>
    /// <returns>The string representation of the operator.</returns>
    protected string GetEcmaStandardLogicalOperator(LogicalOperator op)
    {
        switch (op)
        {
            case LogicalOperator.LogicalAnd:
                return "&&";

            case LogicalOperator.LogicalOr:
                return "||";

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gets the precedence level of an expression for determining when parentheses are needed.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The precedence level (higher values bind more tightly).</returns>
    protected int GetExpressionLevel(Expression expression)
    {
        if (expression is UnaryExpression)
        {
            return 12;
        }
        else if (expression is BinaryExpression binaryExpression)
        {
            switch (binaryExpression.Operator)
            {
                case BinaryOperator.Plus:
                case BinaryOperator.Minus:
                    return 9;

                case BinaryOperator.Times:
                case BinaryOperator.Divide:
                case BinaryOperator.Modulo:
                    return 11;

                case BinaryOperator.Equal:
                case BinaryOperator.NotEqual:
                case BinaryOperator.StrictlyEqual:
                case BinaryOperator.StricltyNotEqual:
                    return 6;

                case BinaryOperator.Greater:
                case BinaryOperator.GreaterOrEqual:
                case BinaryOperator.Less:
                case BinaryOperator.LessOrEqual:
                case BinaryOperator.InstanceOf:
                    return 7;

                case BinaryOperator.BitwiseAnd:
                    return 5;

                case BinaryOperator.BitwiseOr:
                    return 3;

                case BinaryOperator.BitwiseXOr:
                    return 4;

                case BinaryOperator.LeftShift:
                case BinaryOperator.RightShift:
                case BinaryOperator.UnsignedRightShift:
                    return 8;

                case BinaryOperator.In:
                    // There is an allowIn issue
                    return 7;

                default:
                    return 0;
            }
        }
        else if (expression is LogicalExpression logicalExpression)
        {
            switch (logicalExpression.Operator)
            {
                case LogicalOperator.LogicalAnd:
                    return 2;

                case LogicalOperator.LogicalOr:
                    return 1;

                default:
                    return 0;
            }
        }
        else if (expression is TypeExpression typeExpression)
        {
            switch (typeExpression.TypeOp)
            {
                case TypeOperator.As:
                case TypeOperator.Cast:
                    return 7;

                default:
                    return 0;
            }
        }
        else if (expression is AssignmentExpression)
        {
            return 0;
        }
        else if (expression is SequenceExpression)
        {
            return -1;
        }
        else if (expression is MemberExpression)
        {
            return 20;
        }
        else
        {
            return 30;
        }
    }

    /// <summary>
    /// Gets the type code from an expression.
    /// </summary>
    /// <param name="expression">The expression to extract type information from.</param>
    /// <returns>A <see cref="TypeDefinition"/> representing the type, or null if not resolvable.</returns>
    public static TypeDefinition GetTypeCode(Expression expression)
    {
        string typeName = string.Empty;

        if (expression is Identifier identifier)
        {
            typeName = identifier.Name;
        }
        else if (expression is Literal literal)
        {
            typeName = literal.Value.ToString();
        }

        return TypeDefinition.Resolve(typeName, true);
    }

    /// <summary>
    /// Gets the full type name from a type string.
    /// </summary>
    /// <param name="type">The type string to resolve.</param>
    /// <returns>The full type name.</returns>
    public static string GetFullTypeName(string type)
    {
        TypeDefinition typeInfo = TypeDefinition.Resolve(type, true);
        return typeInfo.GetFullTypeNameText();
    }

    /// <summary>
    /// Gets the short type name from a type string.
    /// </summary>
    /// <param name="type">The type string to resolve.</param>
    /// <returns>The short type name if available, otherwise the full name.</returns>
    public static string GetShortTypeName(string type)
    {
        TypeDefinition typeInfo = TypeDefinition.Resolve(type, true);
        return typeInfo.GetShortTypeName();
    }

    /// <summary>
    /// Gets the type name from a type string, optionally using short name.
    /// </summary>
    /// <param name="type">The type string to resolve.</param>
    /// <param name="tryShortName">If true, attempts to use the short type name.</param>
    /// <returns>The type name (short or full based on <paramref name="tryShortName"/>).</returns>
    public static string GetTypeName(string type, bool tryShortName)
    {
        TypeDefinition typeInfo = TypeDefinition.Resolve(type, true);

        if (!tryShortName)
        {
            return typeInfo.GetFullTypeNameText();
        }
        else
        {
            if (typeInfo.TryGetShortTypeName(false, out string typeName))
            {
                return typeName;
            }

            return typeInfo.GetFullTypeNameText();
        }
    }

    /// <summary>
    /// Gets the type name from a type string using context settings.
    /// </summary>
    /// <param name="context">The expression context.</param>
    /// <param name="type">The type string to resolve.</param>
    /// <param name="tryShortName">If true, attempts to use the short type name.</param>
    /// <returns>The type name based on context and <paramref name="tryShortName"/>.</returns>
    public static string GetTypeName(ExpressionContext context, string type, bool tryShortName)
    {
        if (context.UseFullName && !tryShortName)
        {
            return GetFullTypeName(type);
        }
        else
        {
            return GetTypeName(type, true);
        }
    }

    /// <summary>
    /// Gets the element type name from a type definition.
    /// </summary>
    /// <param name="context">The expression context.</param>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="tryShortName">If true, attempts to use the short type name.</param>
    /// <returns>The element type name (short or full based on context and <paramref name="tryShortName"/>).</returns>
    public static string GetElementTypeName(ExpressionContext context, TypeDefinition typeInfo, bool tryShortName)
    {
        if (context.UseFullName && !tryShortName)
        {
            return typeInfo.GetFullTypeNameText();
        }
        else
        {
            if (typeInfo.TryGetShortTypeName(false, out string typeName))
            {
                return typeName;
            }

            return typeInfo.GetFullTypeNameText();
        }
    }
}