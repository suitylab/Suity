namespace Suity.Editor.Documents;

/// <summary>
/// Interface for text document views that support navigation to specific positions.
/// </summary>
public interface ITextDocumentView
{
    /// <summary>
    /// Navigates to a specific character position in the document.
    /// </summary>
    /// <param name="position">The character position to navigate to.</param>
    void Goto(int position);

    /// <summary>
    /// Navigates to a specific line and column in the document.
    /// </summary>
    /// <param name="line">The line number.</param>
    /// <param name="column">The column number.</param>
    void Goto(int line, int column);
}