using Suity.Editor.Documents;
using Suity.Editor.Views;
using Suity.Views;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.Helpers;

/// <summary>
/// Menu command to close all background documents that have no active view and are not dirty.
/// </summary>
[InsertInto(":Main/Tool")]
[NotAvailable]
public class CloseBackendDocuments : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloseBackendDocuments"/> class.
    /// </summary>
    public CloseBackendDocuments()
        : base("Close All Background Documents", CoreIconCache.File)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var docs = DocumentManager.Instance.AllOpenedDocuments
            .Where(o => o.View is null && !o.IsDirty)
            .ToArray();

        foreach (var doc in docs)
        {
            DocumentManager.Instance.CloseDocument(doc);
            Logs.LogDebug("Closing document: " + doc.FileName.FullPath);
        }
    }
}

[InsertInto(":Main/Tool")]
public class LayoutDockCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockCommand"/> class.
    /// </summary>
    public LayoutDockCommand()
        : base("Layout Dock", CoreIconCache.File)
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

        dockManager.RebuildContents();
        //dockManager.InvalidateVisual();
    }
}
