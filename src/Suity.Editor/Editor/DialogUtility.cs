using Suity.Drawing;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor;

/// <summary>
/// Provides utility methods for creating dialogs and message boxes.
/// </summary>
public static class DialogUtility
{
    #region Dialog

    /// <summary>
    /// Creates a text window with the specified text, title, and icon.
    /// </summary>
    /// <param name="text">The content text to display in the window.</param>
    /// <param name="title">The title of the window.</param>
    /// <param name="icon">The icon to display in the window.</param>
    [Obsolete("Use async version intead.", true)]
    public static void CreateTextWindow(string text, string title, ImageDef icon)
        => EditorServices.DialogService?.CreateTextWindow(text, title, icon);

    /// <summary>
    /// Shows a message box with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the message box.</param>
    [Obsolete("Use async version intead.", true)]
    public static void ShowMessageBox(string message)
        => EditorServices.DialogService?.ShowDialog(message);

    /// <summary>
    /// Shows a localized message box with a plain string message.
    /// </summary>
    /// <param name="message">The plain string message to display, which will be localized.</param>
    [Obsolete("Use async version intead.", true)]
    public static void ShowMessageBoxL(PlainString message)
        => EditorServices.DialogService?.ShowDialog(L(message));

    /// <summary>
    /// Shows a localized message box with a formattable string message.
    /// </summary>
    /// <param name="message">The formattable string message to display, which will be localized.</param>
    [Obsolete("Use async version intead.", true)]
    public static void ShowMessageBoxL(FormattableString message)
        => EditorServices.DialogService?.ShowDialog(L(message));

    /// <summary>
    /// Shows a yes/no dialog with the specified message and returns the user's choice.
    /// </summary>
    /// <param name="message">The message to display in the dialog.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    [Obsolete("Use async version intead.", true)]
    public static bool ShowYesNoDialog(string message)
        => EditorServices.DialogService?.ShowYesNoDialog(message) ?? false;

    /// <summary>
    /// Shows a localized yes/no dialog with a plain string message and returns the user's choice.
    /// </summary>
    /// <param name="message">The plain string message to display, which will be localized.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    [Obsolete("Use async version intead.", true)]
    public static bool ShowYesNoDialogL(PlainString message)
        => EditorServices.DialogService?.ShowYesNoDialog(L(message)) ?? false;

    /// <summary>
    /// Shows a localized yes/no dialog with a formattable string message and returns the user's choice.
    /// </summary>
    /// <param name="message">The formattable string message to display, which will be localized.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    [Obsolete("Use async version intead.", true)]
    public static bool ShowYesNoDialogL(FormattableString message)
        => EditorServices.DialogService?.ShowYesNoDialog(L(message)) ?? false;

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    [Obsolete("Use async version intead.", true)]
    public static bool? ShowYesNoCancelDialog(string message)
        => EditorServices.DialogService?.ShowYesNoCancelDialog(message);

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified PlainString message.
    /// </summary>
    /// <param name="message">The PlainString message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    [Obsolete("Use async version intead.", true)]
    public static bool? ShowYesNoCancelDialog(PlainString message)
        => EditorServices.DialogService?.ShowYesNoCancelDialog(L(message));

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified FormattableString message.
    /// </summary>
    /// <param name="message">The FormattableString message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    [Obsolete("Use async version intead.", true)]
    public static bool? ShowYesNoCancelDialog(FormattableString message)
        => EditorServices.DialogService?.ShowYesNoCancelDialog(L(message));


    /// <summary>
    /// Displays a single-line text dialog with a title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowSingleLineTextDialog(string title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowSingleLineTextDialog(title, text, validate);

    /// <summary>
    /// Displays a single-line text dialog with localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowSingleLineTextDialogL(PlainString title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowSingleLineTextDialog(L(title), text, validate);

    /// <summary>
    /// Displays a single-line text dialog with formattable localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowSingleLineTextDialogL(FormattableString title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowSingleLineTextDialog(L(title), text, validate);

    /// <summary>
    /// Displays a password input dialog with title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowPasswordTextDialog(string title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowPasswordTextDialog(title, text, validate);

    /// <summary>
    /// Displays a password input dialog with localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowPasswordTextDialogL(PlainString title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowPasswordTextDialog(L(title), text, validate);

    /// <summary>
    /// Displays a password input dialog with formattable localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowPasswordTextDialogL(FormattableString title, string text, Predicate<string> validate)
        => EditorServices.DialogService?.ShowPasswordTextDialog(L(title), text, validate);

    /// <summary>
    /// Displays a text block dialog with title, text content, and format string
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowTextBlockDialog(string title, string text, string format)
        => EditorServices.DialogService?.ShowTextBlockDialog(title, text, format);

    /// <summary>
    /// Displays a text block dialog with localized title, text content, and format string
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowTextBlockDialogL(PlainString title, string text, string format)
        => EditorServices.DialogService?.ShowTextBlockDialog(L(title), text, format);

    /// <summary>
    /// Displays a text block dialog with formattable localized title, text content, and format string
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowTextBlockDialogL(FormattableString title, string text, string format)
        => EditorServices.DialogService?.ShowTextBlockDialog(L(title), text, format);

    /// <summary>
    /// Displays an open file dialog with filter, initial path, and default file options
    /// </summary>
    /// <param name="filter">The file filter to apply</param>
    /// <param name="initPath">The initial directory path</param>
    /// <param name="defaultFile">The default file name (optional)</param>
    /// <returns>The selected file path, or null if cancelled</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowOpenFile(string filter, string initPath, string defaultFile = null)
        => EditorServices.DialogService?.ShowOpenFile(filter, initPath, defaultFile);

    /// <summary>
    /// Displays a save file dialog with filter, initial path, and default file options
    /// </summary>
    /// <param name="filter">The file filter to apply</param>
    /// <param name="initPath">The initial directory path</param>
    /// <param name="defaultFile">The default file name (optional)</param>
    /// <returns>The selected file path, or null if cancelled</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowSaveFile(string filter, string initPath, string defaultFile = null)
        => EditorServices.DialogService?.ShowSaveFile(filter, initPath, defaultFile);

    /// <summary>
    /// Displays an open folder dialog with initial directory
    /// </summary>
    /// <param name="initDirectory">The initial directory path</param>
    /// <returns>The selected folder path, or null if cancelled</returns>
    [Obsolete("Use async version intead.", true)]
    public static string ShowOpenFolder(string initDirectory)
        => EditorServices.DialogService?.ShowOpenFolder(initDirectory);

    /// <summary>
    /// Displays an exception dialog showing the exception details
    /// </summary>
    /// <param name="exception">The exception to display</param>
    [Obsolete("Use async version intead.", true)]
    public static void ShowException(Exception exception)
        => EditorServices.DialogService?.ShowException(exception);


    #endregion

    #region DialogEx

    public static object ShowSimpleSelectDialog(string title, IEnumerable<KeyValuePair<string, object>> selections)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Displays a simple selection dialog with the given title and selection options
    /// </summary>
    /// <param name="title">The title of the selection dialog</param>
    /// <param name="selections">The collection of key-value pairs representing the selectable options</param>
    /// <returns>The selected object or null if no selection was made</returns>
    public static Task<object> ShowSimpleSelectDialogAsync(string title, IEnumerable<KeyValuePair<string, object>> selections)
    {
        return EditorServices.DialogExService.ShowSimpleSelectDialogAsync(title, selections);
    }

    /// <summary>
    /// Displays a color selection dialog with the specified initial color
    /// </summary>
    /// <param name="initColor">The initial color to be displayed in the color picker</param>
    /// <returns>The selected color as a nullable Color, or null if no color was selected</returns>
    public static Task<Color?> ShowColorSelectDialogAsync(Color initColor)
    {
        return EditorServices.DialogExService.ShowColorSelectDialogAsync(initColor);
    }

    /// <summary>
    /// Displays a dialog for exporting files with the specified initial name and extension
    /// </summary>
    /// <param name="initName">The initial filename to display in the dialog</param>
    /// <param name="ext">The file extension (default is "txt")</param>
    /// <returns>The full path of the file to be exported, or null if the operation was cancelled</returns>
    public static Task<string> ShowExportFileNameDialogAsync(string initName, string ext = "txt")
    {
        return EditorServices.DialogExService.ShowExportFileNameDialogAsync(initName, ext);
    }

    /// <summary>
    /// Displays a dialog for selecting a folder to export to
    /// </summary>
    /// <param name="initName">The initial folder name to display in the dialog</param>
    /// <returns>The selected folder path, or null if the operation was cancelled</returns>
    public static Task<string> ShowExportFolderDialogAsync(string initName)
    {
        return EditorServices.DialogExService.ShowExportFolderDialogAsync(initName);
    }

    #endregion

    #region DialogAsync

    /// <summary>
    /// Creates a text window with the specified text, title, and icon.
    /// </summary>
    /// <param name="text">The content text to display in the window.</param>
    /// <param name="title">The title of the window.</param>
    /// <param name="icon">The icon to display in the window.</param>
    public static Task CreateTextWindowAsync(string text, string title, ImageDef icon)
        => EditorServices.DialogServiceAsync.CreateTextWindowAsync(text, title, icon);

    /// <summary>
    /// Shows a message box with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the message box.</param>
    public static Task ShowMessageBoxAsync(string message)
        => EditorServices.DialogServiceAsync.ShowDialogAsync(message);

    /// <summary>
    /// Shows a localized message box with a plain string message.
    /// </summary>
    /// <param name="message">The plain string message to display, which will be localized.</param>
    public static Task ShowMessageBoxAsyncL(PlainString message)
        => EditorServices.DialogServiceAsync.ShowDialogAsync(L(message));

    /// <summary>
    /// Shows a localized message box with a formattable string message.
    /// </summary>
    /// <param name="message">The formattable string message to display, which will be localized.</param>
    public static Task ShowMessageBoxAsyncL(FormattableString message)
        => EditorServices.DialogServiceAsync.ShowDialogAsync(L(message));

    /// <summary>
    /// Shows a yes/no dialog with the specified message and returns the user's choice.
    /// </summary>
    /// <param name="message">The message to display in the dialog.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    public static Task<bool> ShowYesNoDialogAsync(string message)
        => EditorServices.DialogServiceAsync.ShowYesNoDialogAsync(message);

    /// <summary>
    /// Shows a localized yes/no dialog with a plain string message and returns the user's choice.
    /// </summary>
    /// <param name="message">The plain string message to display, which will be localized.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    public static Task<bool> ShowYesNoDialogAsyncL(PlainString message)
        => EditorServices.DialogServiceAsync.ShowYesNoDialogAsync(L(message));

    /// <summary>
    /// Shows a localized yes/no dialog with a formattable string message and returns the user's choice.
    /// </summary>
    /// <param name="message">The formattable string message to display, which will be localized.</param>
    /// <returns>True if the user selects Yes, false if No or closes the dialog.</returns>
    public static Task<bool> ShowYesNoDialogAsyncL(FormattableString message)
        => EditorServices.DialogServiceAsync.ShowYesNoDialogAsync(L(message));

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified message.
    /// </summary>
    /// <param name="message">The message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    public static Task<bool?> ShowYesNoCancelDialogAsync(string message)
        => EditorServices.DialogServiceAsync.ShowYesNoCancelDialogAsync(message);

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified PlainString message.
    /// </summary>
    /// <param name="message">The PlainString message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    public static Task<bool?> ShowYesNoCancelDialogAsync(PlainString message)
        => EditorServices.DialogServiceAsync.ShowYesNoCancelDialogAsync(L(message));

    /// <summary>
    /// Shows a Yes/No/Cancel dialog box with the specified FormattableString message.
    /// </summary>
    /// <param name="message">The FormattableString message to display in the dialog box.</param>
    /// <returns>
    /// A nullable bool value that indicates the user's choice:
    /// - true if the user clicks Yes
    /// - false if the user clicks No
    /// - null if the user clicks Cancel
    /// </returns>
    public static Task<bool?> ShowYesNoCancelDialogAsync(FormattableString message)
        => EditorServices.DialogServiceAsync.ShowYesNoCancelDialogAsync(L(message));


    /// <summary>
    /// Displays a single-line text dialog with a title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    public static Task<string> ShowSingleLineTextDialogAsync(string title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowSingleLineTextDialogAsync(title, text, validate);

    /// <summary>
    /// Displays a single-line text dialog with localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    public static Task<string> ShowSingleLineTextDialogAsyncL(PlainString title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowSingleLineTextDialogAsync(L(title), text, validate);

    /// <summary>
    /// Displays a single-line text dialog with formattable localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The user input if validation passes, otherwise null</returns>
    public static Task<string> ShowSingleLineTextDialogAsyncL(FormattableString title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowSingleLineTextDialogAsync(L(title), text, validate);

    /// <summary>
    /// Displays a password input dialog with title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    public static Task<string> ShowPasswordTextDialogAsync(string title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowPasswordTextDialogAsync(title, text, validate);

    /// <summary>
    /// Displays a password input dialog with localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    public static Task<string> ShowPasswordTextDialogAsyncL(PlainString title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowPasswordTextDialogAsync(L(title), text, validate);

    /// <summary>
    /// Displays a password input dialog with formattable localized title, text content, and validation predicate
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="validate">A predicate function to validate the input</param>
    /// <returns>The password input if validation passes, otherwise null</returns>
    public static Task<string> ShowPasswordTextDialogAsyncL(FormattableString title, string text, Predicate<string> validate)
        => EditorServices.DialogServiceAsync.ShowPasswordTextDialogAsync(L(title), text, validate);

    /// <summary>
    /// Displays a text block dialog with title, text content, and format string
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    public static Task<string> ShowTextBlockDialogAsync(string title, string text, string format)
        => EditorServices.DialogServiceAsync.ShowTextBlockDialogAsync(title, text, format);

    /// <summary>
    /// Displays a text block dialog with localized title, text content, and format string
    /// </summary>
    /// <param name="title">The localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    public static Task<string> ShowTextBlockDialogAsyncL(PlainString title, string text, string format)
        => EditorServices.DialogServiceAsync.ShowTextBlockDialogAsync(L(title), text, format);

    /// <summary>
    /// Displays a text block dialog with formattable localized title, text content, and format string
    /// </summary>
    /// <param name="title">The formattable localized title of the dialog</param>
    /// <param name="text">The text content to display</param>
    /// <param name="format">The format string for the text content</param>
    /// <returns>The formatted text content</returns>
    public static Task<string> ShowTextBlockDialogAsyncL(FormattableString title, string text, string format)
        => EditorServices.DialogServiceAsync.ShowTextBlockDialogAsync(L(title), text, format);

    /// <summary>
    /// Displays an open file dialog with filter, initial path, and default file options
    /// </summary>
    /// <param name="filter">The file filter to apply</param>
    /// <param name="initPath">The initial directory path</param>
    /// <param name="defaultFile">The default file name (optional)</param>
    /// <returns>The selected file path, or null if cancelled</returns>
    public static Task<string> ShowOpenFileAsync(string filter, string initPath, string defaultFile = null)
        => EditorServices.DialogServiceAsync.ShowOpenFileAsync(filter, initPath, defaultFile);

    /// <summary>
    /// Displays a save file dialog with filter, initial path, and default file options
    /// </summary>
    /// <param name="filter">The file filter to apply</param>
    /// <param name="initPath">The initial directory path</param>
    /// <param name="defaultFile">The default file name (optional)</param>
    /// <returns>The selected file path, or null if cancelled</returns>
    public static Task<string> ShowSaveFileAsync(string filter, string initPath, string defaultFile = null)
        => EditorServices.DialogServiceAsync.ShowSaveFileAsync(filter, initPath, defaultFile);

    /// <summary>
    /// Displays an open folder dialog with initial directory
    /// </summary>
    /// <param name="initDirectory">The initial directory path</param>
    /// <returns>The selected folder path, or null if cancelled</returns>
    public static Task<string> ShowOpenFolderAsync(string initDirectory)
        => EditorServices.DialogServiceAsync.ShowOpenFolderAsync(initDirectory);

    /// <summary>
    /// Displays an exception dialog showing the exception details
    /// </summary>
    /// <param name="exception">The exception to display</param>
    public static Task ShowExceptionAsync(Exception exception)
        => EditorServices.DialogServiceAsync.ShowExceptionAsync(exception);
    #endregion

    #region Selection Async


    /// <summary>
    /// Displays a selection GUI for a selection list
    /// </summary>
    /// <param name="list">The selection list to display</param>
    /// <param name="title">The title of the selection window</param>
    /// <param name="option">Optional selection options</param>
    /// <returns>The selection result</returns>
    public static async Task<SelectionResult> ShowSelectionGUIAsync(this ISelectionList list, string title, SelectionOption option = null)
    {
        ISelectionService service = EditorServices.SelectionService;
        if (service != null)
        {
            return await service.ShowSelectionGUIAsync(list, title, option);
        }
        else
        {
            return SelectionResult.EmptyFailed;
        }
    }

    /// <summary>
    /// Displays a multiple selection GUI for a selection list
    /// </summary>
    /// <param name="list">The selection list to display</param>
    /// <param name="title">The title of the selection window</param>
    /// <param name="option">Optional selection options</param>
    /// <returns>The multiple selection result</returns>
    public static async Task<MultipleSelectionResult> ShowMultipleSelectionGUIAsync(this ISelectionList list, string title, SelectionOption option = null)
    {
        ISelectionService service = EditorServices.SelectionService;
        if (service != null)
        {
            return await service.ShowMultipleSelectionGUIAsync(list, title, option);
        }
        else
        {
            return MultipleSelectionResult.EmptyFailed;
        }
    }

    /// <summary>
    /// Displays a selection GUI for a selection object
    /// </summary>
    /// <param name="selection">The selection object to display</param>
    /// <param name="title">The title of the selection window</param>
    /// <param name="option">Optional selection options</param>
    /// <returns>True if selection was successful, false otherwise</returns>
    public static async Task<bool> ShowSelectionGUIAsync(this ISelection selection, string title, SelectionOption option = null)
    {
        ISelectionService service = EditorServices.SelectionService;
        if (service != null)
        {
            SelectionResult result = await service.ShowSelectionGUIAsync(selection.GetList(), title, option);
            if (result?.IsSuccess == true)
            {
                selection.SelectedKey = result.SelectedKey;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Displays an asset selection GUI for a specific type
    /// </summary>
    /// <typeparam name="T">The type of asset to select</typeparam>
    /// <param name="title">The title of the selection window</param>
    /// <param name="option">Optional selection options</param>
    /// <returns>The selected asset or null if selection was cancelled</returns>
    public static async Task<T> ShowAssetSelectionGUIAsync<T>(string title, SelectionOption option = null) where T : class
    {
        AssetSelection<T> selection = new();
        if (await selection.ShowSelectionGUIAsync(title, option))
        {
            return selection.Target;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Displays an asset selection GUI for a specific type with a filter
    /// </summary>
    /// <typeparam name="T">The type of asset to select</typeparam>
    /// <param name="title">The title of the selection window</param>
    /// <param name="filter">The asset filter to apply</param>
    /// <param name="option">Optional selection options</param>
    /// <returns>The selected asset or null if selection was cancelled</returns>
    public static async Task<T> ShowAssetSelectionGUIAsync<T>(string title, IAssetFilter filter, SelectionOption option = null) where T : class
    {
        AssetSelection<T> selection = new(filter);
        if (await selection.ShowSelectionGUIAsync(title, option))
        {
            return selection.Target;
        }
        else
        {
            return null;
        }
    }

    #endregion
}
