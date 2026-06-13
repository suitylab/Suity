using Dock.Model.Controls;
using Suity.Editor.Views;
using Suity.Views;
using Suity.Views.Menu;

namespace Suity.Editor.Controls;


[InsertInto(":Main/Tool")]
public class DockDebugCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockDebugCommand"/> class.
    /// </summary>
    public DockDebugCommand()
        : base("Dock Debug", CoreIconCache.File)
    {
        AddCommand(new DebugDetachContent());
        AddCommand(new DebugAttachContent());
    }
}

public class DebugDetachContent : MenuCommand
{
    public DebugDetachContent()
        : base("Detach Content")
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var mainWindow = SuityApp.Instance.Window as MainWindow;
        if (mainWindow is null)
        {
            return;
        }

        var dockManager = mainWindow.View?.DockContainer;
        if (dockManager is null)
        {
            return;
        }

        if (dockManager.ActiveDocumentControl is not { } actDocCtrl)
        {
            return;
        }

        if (actDocCtrl.Dockable is not { } dockable)
        {
            return;
        }

        dockable.Content = null;

        if (dockable.Owner is IDocumentDock dock)
        {
            dock.ActiveDockable = null;
            dock.ActiveDockable = dockable;
        }
    }
}

public class DebugAttachContent : MenuCommand
{
    public DebugAttachContent()
        : base("Attach Content")
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var mainWindow = SuityApp.Instance.Window as MainWindow;
        if (mainWindow is null)
        {
            return;
        }

        var dockManager = mainWindow.View?.DockContainer;
        if (dockManager is null)
        {
            return;
        }

        if (dockManager.ActiveDocumentControl is not { } actDocCtrl)
        {
            return;
        }

        if (actDocCtrl.Dockable is not { } dockable)
        {
            return;
        }

        dockable.Content = actDocCtrl;

        if (dockable.Owner is IDocumentDock dock)
        {
            dock.ActiveDockable = null;
            dock.ActiveDockable = dockable;
        }
    }
}