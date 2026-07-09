using Suity.Editor.Views;
using Suity.Views.Gui;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.MenuCommands.AppMenus;

class ToggleDescription : MenuCommand
{
    public ToggleDescription()
        : base("Toggle Description")
    {
        HotKey = "F7";
    }

    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        bool show = EditorUtility.ShowAsDescription.Value;
        this.Icon = show ? CoreIconCache.Tick : null;
    }

    public override void DoCommand()
    {
        EditorUtility.ShowAsDescription.Value = !EditorUtility.ShowAsDescription.Value;
    }
}

class ShowToolWindowMenuCommand : MenuCommand
{
    readonly IToolWindow _toolWindow;

    public ShowToolWindowMenuCommand(IToolWindow toolWindow)
        : base(toolWindow.Title, toolWindow.Icon)
    {
        _toolWindow = toolWindow ?? throw new ArgumentNullException(nameof(toolWindow));
    }

    public override void DoCommand()
    {
        EditorUtility.ShowToolWindow(_toolWindow);
    }
}

class ResetLayoutMenuCommand : MenuCommand
{
    public ResetLayoutMenuCommand()
        : base("Reset Layout", CoreIconCache.Layout)
    {
    }

    public override void DoCommand()
    {
        (SuityApp.Instance.Window as MainWindow)?.View.DockContainer.ResetLayout();
    }
}

public class RefreshLayoutMenuCommand : MenuCommand
{
    public RefreshLayoutMenuCommand()
        : base("Refresh Layout")
    {
        HotKey = "F5";
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var mainWindow = SuityApp.Instance.Window as MainWindow;
        if (mainWindow is null)
        {
            return;
        }

        var docks = mainWindow.View?.DockContainer;
        if (docks is null)
        {
            return;
        }

        docks.QueueRebuildDocuments();
        docks.QueueRebuildTools();
    }
}