using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Suity.Editor.Views.Selecting;
using Suity.Selecting;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class AvaSelectionService : ISelectionService
{
    public static readonly AvaSelectionService Instance = new();

    public static Window? GetMainWindow()
    {
        // Check if the current lifetime is classic desktop lifetime
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }


    public async Task<SelectionResult> ShowSelectionGUIAsync(ISelectionList list, string title, SelectionOption? option = null)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var mainWindow = GetMainWindow();
        if (mainWindow is null)
        {
            return SelectionResult.EmptyFailed;
        }

        var window = new SelectionWindow(list, title, option);
        await window.ShowDialog(mainWindow);

        SelectionResult result;
        if (window.IsSuccess && string.IsNullOrWhiteSpace(window.SelectedKey) && window.SelectedItem is EmptyGuiSelectionItem)
        {
            result = SelectionResult.EmptySuccess;
        }
        else
        {
            result = new SelectionResult(window.IsSuccess, window.SelectedKey, window.SelectedItem, window.InputText);
        }

        return result;
    }

    public async Task<MultipleSelectionResult> ShowMultipleSelectionGUIAsync(ISelectionList list, string title, SelectionOption option = null)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var mainWindow = GetMainWindow();
        if (mainWindow is null)
        {
            return MultipleSelectionResult.EmptyFailed;
        }

        option ??= new SelectionOption();
        option.Multiple = true;
        option.HideEmptySelection = true;

        var window = new SelectionWindow(list, title, option);
        await window.ShowDialog(mainWindow);

        if (!window.IsSuccess)
        {
            return MultipleSelectionResult.EmptyFailed;
        }

        var result = new MultipleSelectionResult(window.SelectedItems);

        return result;
    }

}
