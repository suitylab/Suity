using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Expressions;

/// <summary>
/// Base factory for building expression nodes from objects.
/// </summary>
public abstract class ExpressionFactory
{
    private static readonly Dictionary<Type, ExpressionFactory> _factories = [];

    /// <summary>
    /// Checks if an object can be resolved to an expression.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object can be built into an expression node; otherwise, false.</returns>
    public static bool CanBuild(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is IExpressionObject)
        {
            return true;
        }

        return GetExpressionFactory(obj.GetType()) != null;
    }

    /// <summary>
    /// Builds an expression node from the specified object.
    /// </summary>
    /// <param name="obj">The object to build an expression from.</param>
    /// <param name="context">The expression context.</param>
    /// <param name="withBody">Whether to include the body in the expression.</param>
    /// <returns>The built expression node, or null if no factory is found.</returns>
    public static ExpressionNode BuildNode(object obj, ExpressionContext context, bool withBody)
    {
        if (obj is null)
        {
            throw new ArgumentNullException();
        }

        context.WithBody = withBody;
        if (obj is IExpressionObject expr)
        {
            return expr.GetExpression(ExpressionNode.Builder, context);
        }

        ExpressionFactory factory = GetExpressionFactory(obj.GetType());
        if (factory != null)
        {
            try
            {
                return factory.GetExpression(obj, context);
            }
            catch (Exception err)
            {
                Logs.LogError(err);
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Builds the full source expression including namespace for the specified object.
    /// </summary>
    /// <param name="obj">The object to build full source for.</param>
    /// <param name="body">The body expression nodes.</param>
    /// <returns>The full source expression node.</returns>
    public static ExpressionNode BuildFullSource(object obj, ExpressionNode body)
    {
        string nameSpace = EditorUtility.GetNameSpace(obj);

        return ExpressionNode.Builder.Source(nameSpace, body);
    }

    /// <summary>
    /// Builds the full source expression including namespace for the specified object.
    /// </summary>
    /// <param name="obj">The object to build full source for.</param>
    /// <param name="body">The body expression nodes.</param>
    /// <returns>The full source expression node.</returns>
    public static ExpressionNode BuildFullSource(object obj, IEnumerable<ExpressionNode> body)
    {
        string nameSpace = EditorUtility.GetNameSpace(obj);

        return ExpressionNode.Builder.Source(nameSpace, body);
    }

    private static ExpressionFactory GetExpressionFactory(Type objType)
    {
        if (_factories.TryGetValue(objType, out ExpressionFactory factory))
        {
            return factory;
        }

        // Global search and build factory pool

        // Inheritance chain search
        Type testType = objType;
        while (testType != null)
        {
            Type exprType = typeof(ExpressionFactory<>).GetGenericDerivedType(testType).FirstOrDefault();
            if (exprType != null)
            {
                factory = (ExpressionFactory)exprType.CreateInstanceOf();
                _factories[objType] = factory;
                return factory;
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        // No solution for current type
        _factories[objType] = null;

        return null;
    }

    /// <summary>
    /// Gets the expression node for the specified object.
    /// </summary>
    /// <param name="obj">The object to get an expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>The expression node representing the object.</returns>
    public abstract ExpressionNode GetExpression(object obj, ExpressionContext context);
}

/// <summary>
/// Generic base factory for building expression nodes from objects of a specific type.
/// </summary>
/// <typeparam name="T">The type of object this factory handles.</typeparam>
public abstract class ExpressionFactory<T> : ExpressionFactory
{
    #region ExpressionFactory

    /// <inheritdoc/>
    public override ExpressionNode GetExpression(object obj, ExpressionContext context)
    {
        return GetExpressionT((T)obj, context);
    }

    /// <summary>
    /// Gets the expression node for the specified typed value.
    /// </summary>
    /// <param name="value">The typed value to get an expression for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>The expression node representing the value.</returns>
    protected abstract ExpressionNode GetExpressionT(T value, ExpressionContext context);

    #endregion
}