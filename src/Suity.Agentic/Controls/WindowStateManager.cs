using Avalonia;
using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Suity.Editor.Controls;

/// <summary>
/// Window state configuration model
/// </summary>
public class WindowSettings
{
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public double Width { get; set; } = 800;
    public double Height { get; set; } = 600;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
}

public static class WindowStateManager
{
    /// <summary>
    /// Serialize and save window state
    /// </summary>
    /// <param name="window">Target window</param>
    /// <param name="path">Configuration file path</param>
    public static void SaveSettings(Window window, string path)
    {
        var settings = new WindowSettings { WindowState = window.WindowState };

        // Only capture position and size in normal state
        if (window.WindowState == WindowState.Normal)
        {
            settings.Width = window.Width;
            settings.Height = window.Height;
            settings.X = window.Position.X;
            settings.Y = window.Position.Y;
        }
        else
        {
            // If maximized, try to preserve size data from old file to avoid losing Normal state memory
            var existing = LoadRawSettings(path);
            settings.Width = existing?.Width ?? 800;
            settings.Height = existing?.Height ?? 600;
            settings.X = existing?.X ?? 100;
            settings.Y = existing?.Y ?? 100;
        }

        try
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save window configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserialize and restore window state
    /// </summary>
    public static void LoadSettings(Window window, string path)
    {
        var settings = LoadRawSettings(path);
        if (settings != null)
        {
            try
            {
                // 1. Set startup mode to manual to apply coordinates
                window.WindowStartupLocation = WindowStartupLocation.Manual;

                // 2. Restore position (convert to physical pixels)
                window.Position = new PixelPoint((int)settings.X, (int)settings.Y);

                // 3. Restore size
                window.Width = settings.Width;
                window.Height = settings.Height;

                // 4. Finally apply window state
                window.WindowState = settings.WindowState;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load window configuration: {ex.Message}");
            }
        }
        else
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    private static WindowSettings? LoadRawSettings(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<WindowSettings>(json);
        }
        catch
        {
            return null;
        }
    }
}