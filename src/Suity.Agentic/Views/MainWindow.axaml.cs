using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Suity.Editor.Controls;
using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Views;
public class WindowSettings
{
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public double Width { get; set; } = 1024;
    public double Height { get; set; } = 768;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
}

public partial class MainWindow : Window
{
    public const string DockLayoutFile = "DockLayout.json";
    public const string WindowSettingFile = "WindowSetting.json";

    readonly HashSet<Documents.DocumentEntry> _abandonedDocuments = [];

    DisposeCollector _listeners;

    public MainWindow()
    {
        InitializeComponent();

        var dockContainer = this.View.DockContainer;
        foreach (var window in EditorServices.ToolWindow.ToolWindows)
        {
            dockContainer.RegisterTool(window);
        }

        this.Title = Project.Current.ProjectName ?? "Suity";
        this.View.ProjectTitle.Text = Project.Current.ProjectName;

        var project = Project.Current;
        string projectDockFile = project.UserDirectory.PathAppend(DockLayoutFile);
        View.DockContainer.LayoutConfigFilName = projectDockFile;

        _listeners += EditorRexes.ShowToolWindow.AddQueuedActionListener(HandleShowToolWindow);
    }


    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        LoadWindowSettings();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        EditorRexes.UIStarted.Invoke();
        EditorRexes.IsAppActive.Value = true;
        EditorRexes.EditorResume.Invoke();

        // Loading dock layout will cause losing icon, so set it after loading.
        this.Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Suity.Agentic/Assets/suity-logo.ico")));

    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel)
        {
            return;
        }

        var stoppingEvent = new CancellableEvent();
        EditorRexes.UIStopping.Invoke(stoppingEvent);
        if (stoppingEvent.Cancel)
        {
            e.Cancel = true;
            return;
        }

        var dockContainer = this.View.DockContainer;
        var dirtyDocuments = dockContainer.DirtyDocuments
            .Where(o => o.Document is { } doc && !_abandonedDocuments.Contains(doc))
            .ToArray();

        if (dirtyDocuments.Length > 0)
        {
            e.Cancel = true;
            HandleAskForSaving(dirtyDocuments);
            return;
        }

        SaveDockLayout();
        SaveWindowSettings();

        SuityApp.Instance.CloseProject();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _listeners?.Dispose();
    }

    private async void HandleAskForSaving(EditorDocumentContent[] dirtyDocuments)
    {
        foreach (var docContent in dirtyDocuments)
        {
            if (docContent.Document is not { } doc)
            {
                continue;
            }

            bool? result = await docContent.AskForAppQuit();
            if (!result.HasValue)
            {
                _abandonedDocuments.Clear();
                return;
            }
            else if (result.Value)
            {
                _abandonedDocuments.Remove(doc);
            }
            else
            {
                _abandonedDocuments.Add(doc);
            }
        }

        QueuedAction.Do(Close);
    }

    private void HandleShowToolWindow(string id, bool focus)
    {
        View.DockContainer.ShowTool(id, focus);
    }


    #region Setting
    public void SaveDockLayout()
    {
        var project = Project.Current;
        string projectDockFile = project.UserDirectory.PathAppend(DockLayoutFile);
        View.DockContainer.SaveLayout(projectDockFile);
    }
    public void SaveWindowSettings()
    {
        var project = Project.Current;
        string windowSettingFile = project.UserDirectory.PathAppend(WindowSettingFile);

        WindowStateManager.SaveSettings(this, windowSettingFile);
    }
    public void LoadWindowSettings()
    {
        var project = Project.Current;
        string windowSettingFile = project.UserDirectory.PathAppend(WindowSettingFile);

        WindowStateManager.LoadSettings(this, windowSettingFile);
    } 
    #endregion
}
