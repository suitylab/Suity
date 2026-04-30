using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.MenuCommands.AppMenus;

public abstract class ActiveDocumentMenuCommand<T> : MenuCommand
    where T : class
{
    protected ActiveDocumentMenuCommand()
    {
    }

    protected ActiveDocumentMenuCommand(string text, Image? icon = null) : base(text, icon)
    {
    }

    protected ActiveDocumentMenuCommand(string key, string text, Image? icon = null) : base(key, text, icon)
    {
    }


    public (DocumentEntry? Doc, T? Service) ResorveService()
    {
        if (DocumentViewManager.Current.ActiveDocument is { } doc)
        {
            if (doc.View is IHasSubDocumentView hasSubView && hasSubView.CurrentSubView is IServiceProvider subView)
            {
                return (null, subView.GetService<T>());
            }
            else
            {
                return (doc, doc.View?.GetService<T>());
            }
        }
        else
        {
            return (null, null);
        }
    }

    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        if (ResorveService() is { } result && result.Service is { } service)
        {
            OnPopUpFound(result.Doc, service);
        }
        else
        {
            OnPopUpNotFound();
        }
    }

    protected virtual void OnPopUpFound(DocumentEntry? doc, T service)
    {
        this.Enabled = true;
    }

    protected virtual void OnPopUpNotFound()
    {
        this.Enabled = false;
    }

    public override void DoCommand()
    {
        if (ResorveService() is { } result && result.Service is { } service)
        {
            DoCommandFound(result.Doc, service);
        }
        else
        {
            DoCommandNotFound();
        }
    }

    protected virtual void DoCommandFound(DocumentEntry? doc, T service)
    {
    }

    protected virtual void DoCommandNotFound()
    {
    }

}
