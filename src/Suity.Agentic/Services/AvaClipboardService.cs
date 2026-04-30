using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class AvaClipboardService : ISystemClipboard
{
    public static readonly AvaClipboardService Instance = new();

    // Helper method: Get Clipboard object from current application instance
    private IClipboard? GetClipboard()
    {
        // In Avalonia, clipboard is usually attached to TopLevel (Window)
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }

        return null;
    }

    public async Task<string?> GetText()
    {
        try
        {
            var clipboard = GetClipboard();
            if (clipboard == null) return null;

            return await clipboard.TryGetTextAsync();
        }
        catch (Exception ex)
        {
            // Replace with your error logging
            Console.WriteLine($"Clipboard Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SetText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            // You can keep your prompt logic here
            return false;
        }

        try
        {
            var clipboard = GetClipboard();
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clipboard Error: {ex.Message}");
            return false;
        }
    }
}