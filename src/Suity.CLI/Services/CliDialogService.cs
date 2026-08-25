using Suity.Drawing;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Suity.Editor.Services;

internal class CliDialogService : IDialogService, IDialogExService, IDialogServiceAsync
{
    public static CliDialogService Instance { get; } = new();

    #region IDialogService
    public void CreateTextWindow(string text, string title, ImageDef icon)
    {
        throw new NotImplementedException();
    }

    public void ShowDialog(string message)
    {
        throw new NotImplementedException();
    }

    public void ShowException(Exception exception)
    {
        throw new NotImplementedException();
    }

    public string ShowOpenFile(string filter, string initPath, string defaultFile = null)
    {
        throw new NotImplementedException();
    }

    public string ShowOpenFolder(string initDirectory)
    {
        throw new NotImplementedException();
    }

    public string ShowPasswordTextDialog(string title, string text, Predicate<string> validate)
    {
        throw new NotImplementedException();
    }

    public string ShowSaveFile(string filter, string initPath, string defaultFile = null)
    {
        throw new NotImplementedException();
    }

    public string ShowSingleLineTextDialog(string title, string text, Predicate<string> validate)
    {
        throw new NotImplementedException();
    }

    public string ShowTextBlockDialog(string title, string text, string format)
    {
        throw new NotImplementedException();
    }

    public bool? ShowYesNoCancelDialog(string message)
    {
        throw new NotImplementedException();
    }

    public bool ShowYesNoDialog(string message)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IDialogExService
    public Task<Color?> ShowColorSelectDialogAsync(Color initColor)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowExportFileNameDialogAsync(string initName, string ext = "txt")
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowExportFolderDialogAsync(string initName)
    {
        throw new NotImplementedException();
    }

    public Task<object> ShowSimpleSelectDialogAsync(string title, IEnumerable<KeyValuePair<string, object>> selections)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IDialogServiceAsync
    public Task CreateImGuiDialog(IDrawImGui imGui, DialogOptions option)
    {
        throw new NotImplementedException();
    }

    public Task CreateTextWindowAsync(string text, string title, ImageDef icon)
    {
        throw new NotImplementedException();
    }

    public Task ShowDialogAsync(string message)
    {
        throw new NotImplementedException();
    }

    public Task ShowExceptionAsync(Exception exception)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowOpenFileAsync(string filter, string initPath, string defaultFile = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowOpenFolderAsync(string initDirectory)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowPasswordTextDialogAsync(string title, string text, Predicate<string> validate)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowSaveFileAsync(string filter, string initPath, string defaultFile = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowSingleLineTextDialogAsync(string title, string text, Predicate<string> validate)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShowTextBlockDialogAsync(string title, string text, string format)
    {
        throw new NotImplementedException();
    }

    public Task<bool?> ShowYesNoCancelDialogAsync(string message)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ShowYesNoDialogAsync(string message)
    {
        throw new NotImplementedException();
    } 
    #endregion
}
