using Suity.Drawing;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.Services;

internal class CliDialogService : IDialogService, IDialogExService, IDialogServiceAsync
{
    public static CliDialogService Instance { get; } = new();

    #region IDialogService
    public void CreateTextWindow(string text, string title, ImageDef icon)
    {
    }

    public void ShowDialog(string message)
    {
    }

    public void ShowException(Exception exception)
    {
    }

    public string? ShowOpenFile(string filter, string initPath, string? defaultFile = null) => null;
    public string? ShowOpenFolder(string initDirectory) => null;

    public string? ShowPasswordTextDialog(string title, string text, Predicate<string> validate) => null;

    public string? ShowSaveFile(string filter, string initPath, string? defaultFile = null) => null;

    public string? ShowSingleLineTextDialog(string title, string text, Predicate<string> validate) => null;

    public string? ShowTextBlockDialog(string title, string text, string format) => null;

    public bool? ShowYesNoCancelDialog(string message) => null;

    public bool ShowYesNoDialog(string message) => false;

    #endregion

    #region IDialogExService
    public Task<Color?> ShowColorSelectDialogAsync(Color initColor)
        => Task.FromResult<Color?>(initColor);

    public Task<string> ShowExportFileNameDialogAsync(string initName, string ext = "txt")
        => Task.FromResult<string>(initName);

    public Task<string> ShowExportFolderDialogAsync(string initName)
        => Task.FromResult<string>(initName);

    public Task<object> ShowSimpleSelectDialogAsync(string title, IEnumerable<KeyValuePair<string, object>> selections)
        => Task.FromResult<object>(null);

    #endregion

    #region IDialogServiceAsync
    public Task CreateImGuiDialog(IDrawImGui imGui, DialogOptions option) 
        => Task.CompletedTask;

    public Task CreateTextWindowAsync(string text, string title, ImageDef icon)
        => Task.CompletedTask;

    public Task ShowDialogAsync(string message)
        => Task.CompletedTask;

    public Task ShowExceptionAsync(Exception exception)
        => Task.CompletedTask;

    public Task<string> ShowOpenFileAsync(string filter, string initPath, string defaultFile = null)
        => Task.FromResult<string>(null);

    public Task<string> ShowOpenFolderAsync(string initDirectory)
        => Task.FromResult<string>(null);

    public Task<string> ShowPasswordTextDialogAsync(string title, string text, Predicate<string> validate)
        => Task.FromResult<string>(null);
    
    public Task<string> ShowSaveFileAsync(string filter, string initPath, string defaultFile = null)
        => Task.FromResult<string>(null);

    public Task<string> ShowSingleLineTextDialogAsync(string title, string text, Predicate<string> validate)
        => Task.FromResult<string>(null);

    public Task<string> ShowTextBlockDialogAsync(string title, string text, string format)
        => Task.FromResult<string>(null);

    public Task<bool?> ShowYesNoCancelDialogAsync(string message)
        => Task.FromResult<bool?>(null);

    public Task<bool> ShowYesNoDialogAsync(string message)
        => Task.FromResult<bool>(false);

    #endregion
}
