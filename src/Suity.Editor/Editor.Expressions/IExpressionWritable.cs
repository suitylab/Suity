namespace Suity.Editor.Expressions;

/// <summary>
/// Represents an expression that can be written to code output.
/// </summary>
public interface IExpressionWritable
{
    /// <summary>
    /// Gets the sorting order for output ordering.
    /// </summary>
    int SortingOrder { get; }

    /// <summary>
    /// Writes this expression to the specified writer.
    /// </summary>
    /// <param name="writer">The expression writer to use.</param>
    void Write(IExpressionWriter writer);

    /// <summary>
    /// Resets the attention state of this expression.
    /// </summary>
    void AttentionReset();

    /// <summary>
    /// Sets the attention collection for this expression.
    /// </summary>
    /// <param name="collection">The node collection to use for attention.</param>
    void Attention(IExpressionNodeCollection collection);
}