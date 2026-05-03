using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

internal class AvaDialogService : IDialogService, IDialogExService
{
    public static readonly AvaDialogService Instance = new();

    public static Window? GetMainWindow()
    {
        // Check if the current lifetime is classic desktop lifetime
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    #region IDialogService
    public void CreateTextWindow(string text, string title, Suity.Drawing.ImageDef icon)
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

    public string ShowOpenFile(string filter, string initPath, string? defaultFile = null)
    {
        if (GetMainWindow() is not { } parent)
        {
            return null;
        }

        string? result = null;

        // Define an async anonymous function
        return ShowOpenFileAsync(parent, filter, [], initPath).GetAwaiter().GetResult();
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


    public async Task<string?> ShowOpenFileAsync(Window parent, string filterName, string[] extensions, string? initPath = null)
    {
        // Get current window's StorageProvider
        var topLevel = TopLevel.GetTopLevel(parent);
        if (topLevel == null) return null;

        // Convert filter (e.g., extensions = new[] { "txt", "pdf" })
        var options = new FilePickerOpenOptions
        {
            Title = "Select File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(filterName)
                {
                    Patterns = extensions.Select(e => $"*.{e}").ToList()
                }
            ]
        };

        // Set initial path
        if (!string.IsNullOrEmpty(initPath))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initPath);
        }

        // Open dialog
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

        // Return the first selected file path
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }



    #endregion

    #region IDialogExService

    public Task<object> ShowSimpleSelectDialogAsync(string title, IEnumerable<KeyValuePair<string, object>> selections)
    {
        throw new NotImplementedException();
    }

    public Task<Color?> ShowColorSelectDialogAsync(Color initColor)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> ShowExportFileNameDialogAsync(string initName, string ext = "txt")
    {
        Project project = Project.Current;

        ext = '.' + ext.TrimStart('.');

        string? shortFileName = null;
        string? packageFileName = null;
        ulong num = 0;

        if (string.IsNullOrEmpty(initName))
        {
            do
            {
                num++;
                shortFileName = KeyIncrementHelper.MakeKey("Export", 2, num);
                packageFileName = project.PublishDirectory.PathAppend(shortFileName + ext);

                if (!File.Exists(packageFileName))
                {
                    break;
                }
            } while (true);
        }
        else
        {
            shortFileName = initName;
        }

        shortFileName = await DialogUtility.ShowSingleLineTextDialogAsync("Enter file name", shortFileName, s =>
        {
            if (!NamingVerifier.VerifyFileName(s))
            {
                //DialogUtility.ShowMessageBoxAsync("Invalid file name");
                return false;
            }

            string s2 = project.PublishDirectory.PathAppend(s + ext);
            if (File.Exists(s2))
            {
                //return DialogUtility.ShowYesNoDialogAsync("File already exists, overwrite?");
                return false;
            }

            return true;
        });

        if (string.IsNullOrWhiteSpace(shortFileName))
        {
            return null;
        }

        return project.PublishDirectory.PathAppend(shortFileName + ext);
    }

    public async Task<string?> ShowExportFolderDialogAsync(string initName)
    {
        Project project = Project.Current;

        string? shortFolderName = null;
        string? packageFolderName = null;
        ulong num = 0;

        if (string.IsNullOrEmpty(initName))
        {
            do
            {
                num++;
                shortFolderName = KeyIncrementHelper.MakeKey("Export", 2, num);
                packageFolderName = project.PublishDirectory.PathAppend(shortFolderName);

                if (!Directory.Exists(packageFolderName))
                {
                    break;
                }
            } while (true);
        }
        else
        {
            shortFolderName = initName;
        }

        shortFolderName = await DialogUtility.ShowSingleLineTextDialogAsync("Enter folder name", shortFolderName, s =>
        {
            if (!NamingVerifier.VerifyFileName(s))
            {
                //DialogUtility.ShowMessageBoxAsync("Invalid folder name");
                return false;
            }

            string s2 = project.PublishDirectory.PathAppend(s);
            if (Directory.Exists(s2))
            {
                //DialogUtility.ShowMessageBoxAsync("Folder already exists");
                return false;
            }

            return true;
        });

        if (string.IsNullOrWhiteSpace(shortFolderName))
        {
            return null;
        }

        return project.PublishDirectory.PathAppend(shortFolderName);
    }



    #endregion
}
