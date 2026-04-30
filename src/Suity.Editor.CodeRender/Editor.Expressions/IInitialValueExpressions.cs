using Suity.Editor.Types;
using Suity.Editor.Values;
using System;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating initial value assignments.
/// </summary>
public interface IInitialValueExpressions
{
    /// <summary>
    /// Gets an expression node representing a value assignment.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <param name="typeInfo">The type definition of the value.</param>
    /// <param name="value">The initial value object.</param>
    /// <param name="option">The expression context options.</param>
    /// <returns>An expression node representing the value assignment.</returns>
    ExpressionNode GetValueExpression(string name, TypeDefinition typeInfo, object value, ExpressionContext option);

    /// <summary>
    /// Generates initialization function code for an SObject.
    /// </summary>
    /// <param name="name">The name of the initialization function.</param>
    /// <param name="obj">The SObject to initialize.</param>
    /// <param name="option">The expression context options.</param>
    /// <param name="customPropGetter">Optional custom property getter function.</param>
    void MakeInitialFunction(string name, SObject obj, ExpressionContext option, Func<DStructField, object> customPropGetter = null);

    /// <summary>
    /// Generates initialization function code for an SArray.
    /// </summary>
    /// <param name="name">The name of the initialization function.</param>
    /// <param name="ary">The SArray to initialize.</param>
    /// <param name="option">The expression context options.</param>
    void MakeInitialFunction(string name, SArray ary, ExpressionContext option);
}