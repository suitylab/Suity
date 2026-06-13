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
        AddCommand(new DebugRebuildContents());
    }
}

public class DebugRebuildContents : MenuCommand
{
    public DebugRebuildContents()
        : base("Rebuild Contents")
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

        var docks = mainWindow.View?.DockContainer;
        if (docks is null)
        {
            return;
        }

        docks.QueueRebuildDocuments();
        docks.QueueRebuildTools();
    }
}