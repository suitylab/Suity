using Dock.Model.Core;
using Suity.Editor.Controls;
using Suity.Editor.Views;
using Suity.Views.Menu;

namespace Suity.Editor.MenuCommands;

class DocumentDockTabMenu : RootMenuCommand
{
    public DocumentDockTabMenu()
        : base(":DocumentDockTab")
    {
        AddCommand(new SimpleMenuCommand("Save", CoreIconCache.Save, cmd => HandleSave(cmd.Sender as EditorDocumentDockable)));
        AddSeparator();
        AddCommand(new SimpleMenuCommand("Close", CoreIconCache.Close, cmd => HandleClose(cmd.Sender as EditorDocumentDockable)));
        AddCommand(new SimpleMenuCommand("Close others", null, cmd => HandleCloseOthers(cmd.Sender as EditorDocumentDockable)));
        AddCommand(new SimpleMenuCommand("Close all", null, cmd => HandleCloseAll(cmd.Sender as EditorDocumentDockable)));
        AddCommand(new SimpleMenuCommand("Close left", null, cmd => HandleCloseLeft(cmd.Sender as EditorDocumentDockable)));
        AddCommand(new SimpleMenuCommand("Close right", null, cmd => HandleCloseRight(cmd.Sender as EditorDocumentDockable)));
        AddSeparator();
        AddCommand(new SimpleMenuCommand("Locate in project", CoreIconCache.GotoDefination, cmd => HandleLocateInProject(cmd.Sender as EditorDocumentDockable)));
    }

    public static void HandleSave(EditorDocumentDockable? dockable)
    {
        if (dockable?.EditorContent?.Document is { } doc)
        {
            doc.Save();
        }
    }

    public static void HandleClose(EditorDocumentDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseDockable(dockable);
        }
    }

    public static void HandleCloseOthers(EditorDocumentDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseOtherDockables(dockable);
        }
    }

    public static void HandleCloseAll(EditorDocumentDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseAllDockables(dockable);
        }
    }

    public static void HandleCloseLeft(EditorDocumentDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseLeftDockables(dockable);
        }
    }

    public static void HandleCloseRight(EditorDocumentDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseRightDockables(dockable);
        }
    }

    public static void HandleLocateInProject(EditorDocumentDockable? dockable)
    {
        if (dockable?.EditorContent?.Document is { } doc)
        {
            EditorUtility.LocateInProject(doc);
        }
    }

    public static EditorDockContainer? ResolveDock()
    {
        return (SuityApp.Instance.Window as MainWindow)?.View.DockContainer;
    }
}

class ToolDockTabMenu : RootMenuCommand
{
    public ToolDockTabMenu()
         : base(":DocumentDockTab")
    {
        AddCommand(new SimpleMenuCommand("Close", CoreIconCache.Close, cmd => HandleClose(cmd.Sender as IDockable)));
    }

    public static void HandleClose(IDockable? dockable)
    {
        if (dockable != null)
        {
            ResolveDock()?.DockFactory.CloseDockable(dockable);
        }
    }

    public static EditorDockContainer? ResolveDock()
    {
        return (SuityApp.Instance.Window as MainWindow)?.View.DockContainer;
    }
}