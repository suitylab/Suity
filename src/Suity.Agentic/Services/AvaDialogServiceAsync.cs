using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Services;

internal class AvaDialogServiceAsync : IDialogServiceAsync
{
    public static readonly AvaDialogServiceAsync Instance = new();

    public static Window? GetMainWindow()
    {
        // Check if the current lifetime is classic desktop lifetime
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task CreateTextWindowAsync(string text, string title, System.Drawing.Image icon)
    {
        // 1. Safely cast the icon
        WindowIcon? avaloniaIcon = null;
        if (icon != null)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    // Use PNG format to preserve transparency
                    icon.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    avaloniaIcon = new WindowIcon(ms);
                }
            }
            catch { /* If conversion fails, ignore the icon and continue */ }
        }

        // 2. Prepare UI controls
        var textBlock = new SelectableTextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            FontFamily = new FontFamily("Cascadia Code, Consolas, Monospace"),
            Margin = new Thickness(12)
        };

        var copyButton = new Button
        {
            Content = "Copy All",
            Width = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        var closeButton = new Button
        {
            Content = "Close",
            IsCancel = true, // Support Esc key to close
            Width = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        // 3. Build the window
        var win = new Window
        {
            Title = title,
            Icon = avaloniaIcon,
            Width = 600,
            Height = 450,
            MinWidth = 400,
            MinHeight = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            // Use Grid to ensure a solid layout: content area takes remaining space, action area is fixed at the bottom
            Content = new Grid
            {
                RowDefinitions = RowDefinitions.Parse("*, Auto"),
                Children =
            {
                [0] = new ScrollViewer
                {
                    Content = textBlock
                },
                [1] = new Border
                {
                    Padding = new Thickness(15, 10),
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0, 1, 0, 0), // Add top thin line for visual separation
                    Child = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 10,
                        Children = { copyButton, closeButton }
                    }
                }
            }
            }
        };

        // 4. Logic binding
        copyButton.Click += async (s, e) =>
        {
            if (win.Clipboard != null)
            {
                await win.Clipboard.SetTextAsync(text);
                var originalContent = copyButton.Content;
                copyButton.Content = "Copied";
                await Task.Delay(1000);
                copyButton.Content = originalContent;
            }
        };

        closeButton.Click += (s, e) => win.Close();

        // 5. Show the window
        var owner = GetMainWindow();
        if (owner != null)
        {
            await win.ShowDialog(owner);
        }
        else
        {
            win.Show();
        }
    }

    // 1. Normal OK dialog
    public async Task ShowDialogAsync(string message)
    {
        // Use Params object for deep configuration
        var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = "Notice",
            ContentMessage = message,
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.None,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,

            // If no WindowProperties, write directly here:
            WindowDecorations = WindowDecorations.Full,
            
            CanResize = false,
        });

        var mainWindow = GetMainWindow();

        if (mainWindow != null)
            await box.ShowWindowDialogAsync(mainWindow);
        else
            await box.ShowAsync(); // If main window not found, show as independent window
    }

    // 2. Yes/No dialog
    public async Task<bool> ShowYesNoDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Please Confirm", message, ButtonEnum.YesNo, windowStartupLocation:WindowStartupLocation.CenterOwner);
        var mainWindow = GetMainWindow();

        ButtonResult result;
        if (mainWindow != null)
            result = await box.ShowWindowDialogAsync(mainWindow);
        else
            result = await box.ShowAsync();

        return result == ButtonResult.Yes;
    }

    // 3. Yes/No/Cancel dialog
    public async Task<bool?> ShowYesNoCancelDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Please Select", message, ButtonEnum.YesNoCancel, windowStartupLocation: WindowStartupLocation.CenterOwner);
        var mainWindow = GetMainWindow();

        ButtonResult result;
        if (mainWindow != null)
            result = await box.ShowWindowDialogAsync(mainWindow);
        else
            result = await box.ShowAsync();

        return result switch
        {
            ButtonResult.Yes => true,
            ButtonResult.No => false,
            _ => null // Cancel or other close cases return null
        };
    }

    public async Task ShowExceptionAsync(Exception exception)
    {
        var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "Program Exception",
            ContentHeader = exception.GetType().Name, // Display exception type, e.g. NullReferenceException
            ContentMessage = exception.Message,
            // Put detailed stack trace below, MsBox will handle long text automatically
            ButtonDefinitions =
            [
                new ButtonDefinition { Name = "Copy Details" },
                new ButtonDefinition { Name = "OK", IsDefault = true }
            ],
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            MaxWidth = 500,
            MaxHeight = 400,
            Icon = Icon.Error,
        });

        var owner = GetMainWindow();
        var result = owner != null ? await box.ShowWindowDialogAsync(owner) : await box.ShowAsync();

        if (result == "Copy Details")
        {
            var fullText = $"{exception.Message}\n\n{exception.StackTrace}";
            if (owner?.Clipboard != null)
            {
                await owner.Clipboard.SetTextAsync(fullText);
            }
        }
    }
   


    // 1. Single-line text dialog
    public Task<string?> ShowSingleLineTextDialogAsync(string title, string text, Predicate<string> validate)
        => ShowInputInternal(title, text, false, validate);

    // 2. Password dialog
    public Task<string?> ShowPasswordTextDialogAsync(string title, string text, Predicate<string> validate)
        => ShowInputInternal(title, text, true, validate);

    // 3. Multi-line text display dialog
    public async Task<string> ShowTextBlockDialogAsync(string title, string text, string format)
    {
        var tcs = new TaskCompletionSource<string>();

        // OK button
        var okButton = new Button
        {
            Content = "OK",
            IsDefault = true,
            Width = 80,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var textBox = new TextBox
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            AcceptsTab = true,
            FontFamily = new FontFamily("Cascadia Code, Consolas, Monospace"),
        };

        // 1. Create child control instances first, for setting attached properties later
        var scrollViewer = new ScrollViewer
        {
            Padding = new Thickness(15),
            Content = textBox,
        };

        var buttonBar = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(15, 10),
            Child = okButton
        };

        // 2. Explicitly set Grid row indices
        Grid.SetRow(scrollViewer, 0);
        Grid.SetRow(buttonBar, 1);

        var win = new Window
        {
            Title = title,
            Width = 600,
            Height = 450,
            MinWidth = 400,
            MinHeight = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Content = new Grid
            {
                RowDefinitions = RowDefinitions.Parse("*, Auto"),
                // Fix: Use standard collection initialization syntax, do not use indexer assignment like [0]=
                Children =
                {
                    scrollViewer,
                    buttonBar
                }
            }
        };

        // Event handling
        okButton.Click += (s, e) =>
        {
            tcs.TrySetResult(textBox.Text);
            win.Close();
        };

        win.Closed += (s, e) =>
        {
            // Ensure the task completes in all cases to avoid caller deadlock
            tcs.TrySetResult(null);
        };

        try
        {
            var owner = GetMainWindow();
            if (owner != null)
            {
                await win.ShowDialog(owner);
            }
            else
            {
                // If no Owner, at least set it to appear in the center of the screen
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                win.Show();
            }
        }
        catch (Exception)
        {
            tcs.TrySetResult("Error");
            throw;
        }

        return await tcs.Task;
    }



    public async Task<string?> ShowOpenFileAsync(string filter, string initPath, string? defaultFile = null)
    {
        var parent = GetMainWindow();
        if (parent is null)
        {
            return null;
        }

        // 1. Parse WinForms style Filter
        // Assume input format is "Text Files|*.txt;*.log|All Files|*.*"
        // or the example you gave "*.*|All Files" (Note: WinForms standard is usually "Label|Extension")
        var fileTypes = ParseWinFormsFilter(filter);

        // 2. Get StorageProvider
        var storageProvider = parent.StorageProvider;

        // 3. Set initial path
        IStorageFolder? startLocation = null;
        if (Directory.Exists(initPath))
        {
            startLocation = await storageProvider.TryGetFolderFromPathAsync(initPath);
        }

        // 4. Open dialog
        var options = new FilePickerOpenOptions
        {
            Title = "Open File",
            FileTypeFilter = fileTypes,
            AllowMultiple = false,
            SuggestedStartLocation = startLocation,
            SuggestedFileName = defaultFile
        };

        var result = await storageProvider.OpenFilePickerAsync(options);

        // 5. Return path
        return result.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<string?> ShowOpenFolderAsync(string initDirectory)
    {
        var parent = GetMainWindow();
        if (parent is null) return null;

        var storageProvider = parent.StorageProvider;

        // Set initial path
        IStorageFolder? startLocation = null;
        if (!string.IsNullOrEmpty(initDirectory) && Directory.Exists(initDirectory))
        {
            startLocation = await storageProvider.TryGetFolderFromPathAsync(initDirectory);
        }

        var options = new FolderPickerOpenOptions
        {
            Title = "Select Folder",
            SuggestedStartLocation = startLocation,
            AllowMultiple = false
        };

        var result = await storageProvider.OpenFolderPickerAsync(options);

        // Return local path string
        return result.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<string?> ShowSaveFileAsync(string filter, string initPath, string? defaultFile = null)
    {
        var parent = GetMainWindow();
        if (parent is null) return null;

        var storageProvider = parent.StorageProvider;

        // 1. Reuse the Filter parsing function you provided
        var fileTypes = ParseWinFormsFilter(filter);

        // 2. Handle initial path
        IStorageFolder? startLocation = null;
        if (!string.IsNullOrEmpty(initPath))
        {
            // If initPath is a file path, try to get its directory
            var dir = Directory.Exists(initPath) ? initPath : Path.GetDirectoryName(initPath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                startLocation = await storageProvider.TryGetFolderFromPathAsync(dir);
            }
        }

        // 3. Configure save options
        var options = new FilePickerSaveOptions
        {
            Title = "Save File",
            FileTypeChoices = fileTypes,
            SuggestedStartLocation = startLocation,
            SuggestedFileName = defaultFile ?? (File.Exists(initPath) ? Path.GetFileName(initPath) : null),
            DefaultExtension = fileTypes.FirstOrDefault()?.Patterns?.FirstOrDefault()?.Replace("*.", "")
        };

        var result = await storageProvider.SaveFilePickerAsync(options);

        // 4. Return the saved file path
        return result?.TryGetLocalPath();
    }



    /// <summary>
    /// Convert WinForms style filter to Avalonia's FilePickerFileType list
    /// </summary>
    private List<FilePickerFileType> ParseWinFormsFilter(string filter)
    {
        var result = new List<FilePickerFileType>();
        if (string.IsNullOrWhiteSpace(filter)) return result;

        var parts = filter.Split('|');
        // WinForms format usually appears in pairs: label|wildcard
        for (int i = 0; i < parts.Length; i += 2)
        {
            if (i + 1 >= parts.Length) break;

            string name = parts[i].Trim();
            string extensionsPart = parts[i + 1].Trim();

            // Convert *.txt;*.log to txt, log
            var extensions = extensionsPart
                .Split(';')
                .Select(x => x.Trim().Replace("*.", ""))
                .ToList();

            result.Add(new FilePickerFileType(name)
            {
                Patterns = extensions.Select(e => "*." + e).ToList(),
                MimeTypes = null // Can be extended as needed
            });
        }

        return result;
    }


    // Core logic: Dynamically build input window
    private async Task<string?> ShowInputInternal(string message, string text, bool isPassword, Predicate<string> validate)
    {
        var tcs = new TaskCompletionSource<string?>();

        var textBox = new TextBox
        {
            PasswordChar = isPassword ? '*' : '\0',
            Margin = new Thickness(0, 10),
            Watermark = "Please enter content...",
            Text = text ?? string.Empty,
        };

        var okButton = new Button { Content = "OK", IsDefault = true, Width = 80 };
        var cancelButton = new Button { Content = "Cancel", IsCancel = true, Width = 80 };

        // Real-time validation logic: disable OK button if validation fails
        if (validate != null)
        {
            okButton.IsEnabled = validate(string.Empty); // Initial state
            textBox.TextChanged += (s, e) => {
                okButton.IsEnabled = validate(textBox.Text ?? string.Empty);
            };
        }

        var win = new Window
        {
            Title = isPassword ? L("Enter Password") : L("Enter Text"),
            SizeToContent = SizeToContent.Height,
            Width = 350,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 5,
                Children =
                {
                    new TextBlock { Text = message, FontWeight = FontWeight.Bold },
                    textBox,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 10,
                        Margin = new Thickness(0, 10, 0, 0),
                        Children = { okButton, cancelButton }
                    }
                }
            }
        };

        //win.KeyDown += (s, e) =>
        //{
        //    if (e.Key == Key.Enter && okButton.IsEnabled)
        //    {
        //        tcs.SetResult(textBox.Text);
        //        win.Close();
        //    }
        //    else if(e.Key == Key.Escape)
        //    {
        //        tcs.SetResult(null);
        //        win.Close();
        //    }
        //};

        textBox.SelectAll();
        textBox.Focus();

        okButton.Click += (s, e) => { tcs.SetResult(textBox.Text); win.Close(); };
        cancelButton.Click += (s, e) => { tcs.SetResult(null); win.Close(); };
        win.Closed += (s, e) => tcs.TrySetResult(null);

        var owner = GetMainWindow();
        if (owner != null) await win.ShowDialog(owner);
        else win.Show();

        return await tcs.Task;
    }
}
