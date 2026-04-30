using Suity.Editor.Documents;
using Suity.Editor.Views;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Menu;
using System;
using System.Linq;

namespace Suity.Editor.MenuCommands.AppMenus;

#region SaveMenuCommand
class SaveMenuCommand : MenuCommand
{
    public SaveMenuCommand()
        : base("Save", CoreIconCache.Save)
    {
        this.HotKey = "Ctrl+S";
    }

    public override void DoCommand()
    {
        var doc = DocumentViewManager.Current.ActiveDocument;
        HandleSave(doc);
    }

    public static void HandleSave(DocumentEntry? doc)
    {
        if (doc is null)
        {
            return;
        }
        ;

        if (doc.View?.GetService<IViewSave>() is { } viewSave)
        {
            try
            {
                viewSave.SaveView();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        else if (doc.IsDirty)
        {
            doc.Save();
        }
    }
}
#endregion

#region SaveAllMenuCommand
class SaveAllMenuCommand : MenuCommand
{
    public SaveAllMenuCommand()
        : base("Save All", CoreIconCache.Save)
    {
        this.HotKey = "Ctrl+Shift+S";
    }

    public override void DoCommand()
    {
        foreach (var doc in DocumentManager.Instance.AllOpenedDocuments.Where(o => o.IsDirty))
        {
            SaveMenuCommand.HandleSave(doc);
        }
    }
}
#endregion

#region ExitCommand
class ExitCommand : MenuCommand
{
    public ExitCommand()
        : base("Exit", CoreIconCache.Close)
    {
    }

    public override void DoCommand()
    {
        if (SuityApp.Instance.Window is MainWindow mainWindow)
        {
            mainWindow.Close();
        }
    }
} 
#endregion