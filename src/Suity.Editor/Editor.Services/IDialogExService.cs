using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Extended dialog service interface.
/// </summary>
public interface IDialogExService
{
    /// <summary>
    /// Shows a simple selection dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="selections">The selection options.</param>
    /// <returns>A task containing the selected value.</returns>
    Task<object> ShowSimpleSelectDialogAsync(string title, IEnumerable<KeyValuePair<string, object>> selections);

    /// <summary>
    /// Shows a color selection dialog.
    /// </summary>
    /// <param name="initColor">The initial color.</param>
    /// <returns>A task containing the selected color, or null if cancelled.</returns>
    Task<Color?> ShowColorSelectDialogAsync(Color initColor);

    /// <summary>
    /// Shows an export file name dialog.
    /// </summary>
    /// <param name="initName">The initial file name.</param>
    /// <param name="ext">The file extension.</param>
    /// <returns>A task containing the file name, or null if cancelled.</returns>
    Task<string> ShowExportFileNameDialogAsync(string initName, string ext = "txt");

    /// <summary>
    /// Shows an export folder dialog.
    /// </summary>
    /// <param name="initName">The initial folder name.</param>
    /// <returns>A task containing the folder path, or null if cancelled.</returns>
    Task<string> ShowExportFolderDialogAsync(string initName);
}