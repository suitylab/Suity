using Suity.Views.Gui;
using System;
using System.Collections.Generic;
using Suity.Collections;
using Suity.Helpers;

namespace Suity.Editor.Services;

internal class AvaToolWindowService : IToolWindowService
{
    public static readonly AvaToolWindowService Instance = new();

    private readonly Dictionary<string, IToolWindow> _toolWindows = [];
    private readonly Dictionary<Type, IToolWindow> _toolWindowsByType = [];

    private bool _init;

    private AvaToolWindowService()
    {
        // Need to execute various scanning in Start, Awake
        EditorRexes.EditorStart.AddActionListener(Initialize);
    }

    private void Initialize()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        EditorServices.SystemLog.AddLog("ToolWindowService Initializing...");
        EditorServices.SystemLog.PushIndent();

        foreach (var type in typeof(IToolWindow).GetAvailableClassTypes())
        {
            //try
            //{
            if (Activator.CreateInstance(type) is IToolWindow toolWindow)
            {
                RegisterToolWindow(toolWindow);
            }
            //}
            //catch (Exception err)
            //{
            //    Logs.LogError(new ExceptionLogItem("Failed to create tool window : " + type.Name, err));

            //    continue;
            //}
        }

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("ToolWindowService Initialized.");
    }

    private void RegisterToolWindow(IToolWindow toolWindow)
    {
        if (toolWindow is null)
        {
            throw new ArgumentNullException(nameof(toolWindow));
        }

        if (string.IsNullOrEmpty(toolWindow.WindowId))
        {
            throw new ArgumentNullException(nameof(toolWindow.WindowId));
        }

        if (_toolWindows.ContainsKey(toolWindow.WindowId))
        {
            throw new InvalidOperationException();
        }

        EditorServices.SystemLog.AddLog($"Register tool window : ${toolWindow.WindowId} for ${toolWindow.GetType().Name}");

        _toolWindows.Add(toolWindow.WindowId, toolWindow);
        _toolWindowsByType.Add(toolWindow.GetType(), toolWindow);
    }

    #region IToolWindowService
    public IEnumerable<IToolWindow> ToolWindows => _toolWindows.Values;

    public IWindowHandle CreateViewObjectWindow(IViewObjectWindow window)
    {
        return null;
    }

    public IToolWindow GetToolWindow(string windowId)
        => _toolWindows.GetValueSafe(windowId);

    public IToolWindow GetToolWindow(Type toolWindowType) 
        => _toolWindowsByType.GetValueSafe(toolWindowType);
    #endregion
}
