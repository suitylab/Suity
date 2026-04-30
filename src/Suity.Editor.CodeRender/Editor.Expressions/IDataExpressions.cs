using Suity.Editor.Design;
using Suity.Editor.Values;

namespace Suity.Editor.Expressions;

/// <summary>
/// Defines expressions for generating data-related code.
/// </summary>
public interface IDataExpressions
{
    /// <summary>
    /// Gets the source expression for a data container.
    /// </summary>
    /// <param name="owner">The data container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the data source.</returns>
    ExpressionNode GetSource(IDataContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the body content expression for a data container.
    /// </summary>
    /// <param name="owner">The data container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the data body content.</returns>
    ExpressionNode GetBobyContent(IDataContainer owner, ExpressionContext context);

    /// <summary>
    /// Gets the initialization method expression for a data container.
    /// </summary>
    /// <param name="owner">The data container owner.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the initialization method.</returns>
    ExpressionNode GetInitializeMethod(IDataContainer owner, ExpressionContext context);

    /// <summary>
    /// Generates data methods for a data item.
    /// </summary>
    /// <param name="data">The data item to generate methods for.</param>
    /// <param name="context">The expression context.</param>
    void MakeDataMethods(IDataItem data, ExpressionContext context);

    /// <summary>
    /// Gets the creation method expression for a data item.
    /// </summary>
    /// <param name="data">The data item to generate creation method for.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>An expression node representing the data creation method.</returns>
    ExpressionNode GetDataCreationMethod(IDataItem data, ExpressionContext context);

    /// <summary>
    /// Generates data component methods for a data item and its component.
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <param name="comp">The SObject component.</param>
    /// <param name="context">The expression context.</param>
    void MakeDataComponentMethods(IDataItem data, SObject comp, ExpressionContext context);
}