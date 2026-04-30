using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Synchronous service interface for displaying dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Creates a text window.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="title">The window title.</param>
    /// <param name="icon">Optional icon.</param>
    void CreateTextWindow(string text, string title, Image icon);

    /// <summary>
    /// Shows a simple message dialog.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void ShowDialog(string message);

    /// <summary>
    /// Shows a Yes/No dialog.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>True if Yes was selected.</returns>
    bool ShowYesNoDialog(string message);

    /// <summary>
    /// Shows a Yes/No/Cancel dialog.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>True if Yes, false if No, or null if Cancel.</returns>
    bool? ShowYesNoCancelDialog(string message);

    /// <summary>
    /// Shows a single-line text input dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="validate">Optional validation predicate.</param>
    /// <returns>The entered text, or null if cancelled.</returns>
    string ShowSingleLineTextDialog(string title, string text, Predicate<string> validate);

    /// <summary>
    /// Shows a password input dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="validate">Optional validation predicate.</param>
    /// <returns>The entered text, or null if cancelled.</returns>
    string ShowPasswordTextDialog(string title, string text, Predicate<string> validate);

    /// <summary>
    /// Shows a multi-line text input dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="format">Optional format string.</param>
    /// <returns>The entered text, or null if cancelled.</returns>
    string ShowTextBlockDialog(string title, string text, string format);

    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="filter">The file filter.</param>
    /// <param name="initPath">The initial path.</param>
    /// <param name="defaultFile">Optional default file name.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    string ShowOpenFile(string filter, string initPath, string defaultFile = null);

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="filter">The file filter.</param>
    /// <param name="initPath">The initial path.</param>
    /// <param name="defaultFile">Optional default file name.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    string ShowSaveFile(string filter, string initPath, string defaultFile = null);

    /// <summary>
    /// Shows an open folder dialog.
    /// </summary>
    /// <param name="initDirectory">The initial directory.</param>
    /// <returns>The selected folder path, or null if cancelled.</returns>
    string ShowOpenFolder(string initDirectory);

    /// <summary>
    /// Shows an exception dialog.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    void ShowException(Exception exception);
}

/// <summary>
/// Asynchronous service interface for displaying dialogs.
/// </summary>
public interface IDialogServiceAsync
{
    /// <summary>
    /// Creates a text window asynchronously.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="title">The window title.</param>
    /// <param name="icon">Optional icon.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateTextWindowAsync(string text, string title, Image icon);

    /// <summary>
    /// Shows a dialog asynchronously.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowDialogAsync(string message);

    /// <summary>
    /// Shows a Yes/No dialog asynchronously.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>A task containing true if Yes was selected.</returns>
    Task<bool> ShowYesNoDialogAsync(string message);

    /// <summary>
    /// Shows a Yes/No/Cancel dialog asynchronously.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>A task containing true if Yes, false if No, or null if Cancel.</returns>
    Task<bool?> ShowYesNoCancelDialogAsync(string message);

    /// <summary>
    /// Shows a single-line text input dialog asynchronously.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="validate">Optional validation predicate.</param>
    /// <returns>A task containing the entered text, or null if cancelled.</returns>
    Task<string> ShowSingleLineTextDialogAsync(string title, string text, Predicate<string> validate);

    /// <summary>
    /// Shows a password input dialog asynchronously.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="validate">Optional validation predicate.</param>
    /// <returns>A task containing the entered text, or null if cancelled.</returns>
    Task<string> ShowPasswordTextDialogAsync(string title, string text, Predicate<string> validate);

    /// <summary>
    /// Shows a multi-line text input dialog asynchronously.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="text">The initial text.</param>
    /// <param name="format">Optional format string.</param>
    /// <returns>A task containing the entered text, or null if cancelled.</returns>
    Task<string> ShowTextBlockDialogAsync(string title, string text, string format);

    /// <summary>
    /// Shows an open file dialog asynchronously.
    /// </summary>
    /// <param name="filter">The file filter.</param>
    /// <param name="initPath">The initial path.</param>
    /// <param name="defaultFile">Optional default file name.</param>
    /// <returns>A task containing the selected file path, or null if cancelled.</returns>
    Task<string> ShowOpenFileAsync(string filter, string initPath, string defaultFile = null);

    /// <summary>
    /// Shows a save file dialog asynchronously.
    /// </summary>
    /// <param name="filter">The file filter.</param>
    /// <param name="initPath">The initial path.</param>
    /// <param name="defaultFile">Optional default file name.</param>
    /// <returns>A task containing the selected file path, or null if cancelled.</returns>
    Task<string> ShowSaveFileAsync(string filter, string initPath, string defaultFile = null);

    /// <summary>
    /// Shows an open folder dialog asynchronously.
    /// </summary>
    /// <param name="initDirectory">The initial directory.</param>
    /// <returns>A task containing the selected folder path, or null if cancelled.</returns>
    Task<string> ShowOpenFolderAsync(string initDirectory);

    /// <summary>
    /// Shows an exception dialog asynchronously.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowExceptionAsync(Exception exception);
}