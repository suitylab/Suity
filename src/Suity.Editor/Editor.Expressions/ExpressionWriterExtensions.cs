using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Expressions;

public static class ExpressionWriterExtensions
{
    public static IExpressionWriter KW(this IExpressionWriter writer, string keyword)
    {
        writer.Keyword(keyword);
        return writer;
    }

    public static IExpressionWriter ID(this IExpressionWriter writer, string identifier)
    {
        writer.Identifier(identifier);
        return writer;
    }

    public static IExpressionWriter OP(this IExpressionWriter writer, string @operator)
    {
        writer.Operator(@operator);
        return writer;
    }

    public static IExpressionWriter S(this IExpressionWriter writer, string str)
    {
        writer.String(str);
        return writer;
    }

    public static IExpressionWriter C(this IExpressionWriter writer, string code)
    {
        writer.Code(code);
        return writer;
    }

    public static IExpressionWriter CS(this IExpressionWriter writer, string doubleQuotString)
    {
        writer.DoubleQuotString(doubleQuotString);
        return writer;
    }

    public static IExpressionWriter SP(this IExpressionWriter writer)
    {
        writer.Space();
        return writer;
    }

    public static IExpressionWriter NL(this IExpressionWriter writer)
    {
        writer.NewLine();
        return writer;
    }

    public static IExpressionWriter T(this IExpressionWriter writer, TypeDefinition type)
    {
        writer.TypeInfo(type);
        return writer;
    }

    public static IExpressionWriter T(this IExpressionWriter writer, string type)
    {
        writer.TypeInfo(type);
        return writer;
    }

    public static IExpressionWriter E(this IExpressionWriter writer, ExpressionNode expression)
    {
        writer.Expression(expression);
        return writer;
    }

    public static IExpressionWriter E(this IExpressionWriter writer, IExpressionWritable expression)
    {
        writer.Expression(expression);
        return writer;
    }

    public static IExpressionWriter E(this IExpressionWriter writer, IEnumerable<IExpressionWritable> expression)
    {
        writer.Expression(expression);
        return writer;
    }

    public static IExpressionWriter UB(this IExpressionWriter writer, string userCodeName)
    {
        writer.UserCodeBegin(userCodeName);
        return writer;
    }

    public static IExpressionWriter UN(this IExpressionWriter writer, string userCodeName)
    {
        writer.UserCodeEnd(userCodeName);
        return writer;
    }

    public static IExpressionWriter U(this IExpressionWriter writer, string userCodeName)
    {
        writer.UserCodeBegin(userCodeName);
        writer.UserCodeEnd(userCodeName);
        return writer;
    }

    public static IExpressionWriter GB(this IExpressionWriter writer, string genCodeName)
    {
        writer.GenCodeBegin(genCodeName);
        return writer;
    }

    public static IExpressionWriter GN(this IExpressionWriter writer, string genCodeName)
    {
        writer.GenCodeEnd(genCodeName);
        return writer;
    }

    public static IExpressionWriter G(this IExpressionWriter writer, string genCodeName)
    {
        writer.GenCodeBegin(genCodeName);
        writer.GenCodeEnd(genCodeName);
        return writer;
    }

    public static IExpressionWriter IB(this IExpressionWriter writer)
    {
        writer.IndentBegin();
        return writer;
    }

    public static IExpressionWriter IN(this IExpressionWriter writer)
    {
        writer.IndentEnd();
        return writer;
    }
}