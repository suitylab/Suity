namespace Suity.Views;

/// <summary>
/// Represents a selection in a view, which can be any object or path information.
/// </summary>
public class ViewSelection
{
    /// <summary>
    /// Gets an empty selection instance.
    /// </summary>
    public static readonly ViewSelection Empty = new(null);

    /// <summary>
    /// Gets a root selection instance.
    /// </summary>
    public static readonly ViewSelection Root = new(string.Empty);

    /// <summary>
    /// Gets the selected object or path information.
    /// </summary>
    public object Selection { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ViewSelection"/> with the specified selection.
    /// </summary>
    /// <param name="selection">The selected object or path information.</param>
    public ViewSelection(object selection)
    {
        Selection = selection;
    }

    /// <summary>
    /// Returns a string representation of the selection.
    /// </summary>
    /// <returns>A string representing the selection.</returns>
    public override string ToString() => Selection?.ToString() ?? base.ToString();
}