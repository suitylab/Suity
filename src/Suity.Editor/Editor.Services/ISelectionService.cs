using Suity.Selecting;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for displaying selection dialogs.
/// </summary>
public interface ISelectionService
{
    /// <summary>
    /// Shows a selection dialog for a single item.
    /// </summary>
    /// <param name="list">The selection list.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="option">Optional selection options.</param>
    /// <returns>A task representing the asynchronous operation, containing the selection result.</returns>
    Task<SelectionResult> ShowSelectionGUIAsync(ISelectionList list, string title, SelectionOption option = null);

    /// <summary>
    /// Shows a selection dialog for multiple items.
    /// </summary>
    /// <param name="list">The selection list.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="option">Optional selection options.</param>
    /// <returns>A task representing the asynchronous operation, containing the multiple selection result.</returns>
    Task<MultipleSelectionResult> ShowMultipleSelectionGUIAsync(ISelectionList list, string title, SelectionOption option = null);
}