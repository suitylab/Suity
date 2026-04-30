using Avalonia.Controls;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.MenuCommands.AppMenus;

#region LocateMenuCommand

class LocateMenuCommand : MenuCommand
{
    public LocateMenuCommand()
        : base("Locate", CoreIconCache.GotoDefination)
    {
        AddCommand(new NavigateMenuCommand());
        AddCommand(new GotoMenuCommand());
        AddCommand(new SearchMenuCommand());
    }
}

#endregion

#region UndoMenuCommand
class UndoMenuCommand : ActiveDocumentMenuCommand<IViewUndo>
{
    public UndoMenuCommand()
        : base("Undo", CoreIconCache.Undo)
    {
        HotKey = "Ctrl+Z";
    }

    protected override void OnPopUpFound(DocumentEntry doc, IViewUndo viewUndo)
    {
        if (viewUndo.CanUndo)
        {
            Enabled = true;
            Text = L("Undo") + " " + viewUndo.UndoText;
        }
        else
        {
            OnPopUpNotFound();
        }
    }

    protected override void OnPopUpNotFound()
    {
        Enabled = false;
        Text = L("Undo");
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewUndo viewUndo)
    {
        if (viewUndo.CanUndo)
        {
            viewUndo.Undo();
        }
    }
}
#endregion

#region RedoMenuCommand
class RedoMenuCommand : ActiveDocumentMenuCommand<IViewUndo>
{
    public RedoMenuCommand()
        : base("Redo", CoreIconCache.Redo)
    {
        HotKey = "Ctrl+Shift+Z";
    }

    protected override void OnPopUpFound(DocumentEntry doc, IViewUndo viewUndo)
    {
        if (viewUndo.CanRedo)
        {
            Enabled = true;
            Text = L("Redo") + " " + viewUndo.RedoText;
        }
        else
        {
            OnPopUpNotFound();
        }
    }

    protected override void OnPopUpNotFound()
    {
        Enabled = false;
        Text = L("Redo");
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewUndo viewUndo)
    {
        if (viewUndo.CanRedo)
        {
            viewUndo.Redo();
        }
    }
}

#endregion

#region BackwardMenuCommand

public class BackwardMenuCommand : MenuCommand
{
    public BackwardMenuCommand()
        : base("Backward", CoreIconCache.Backward)
    {
        HotKey = "Ctrl+-";
    }

    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        this.Enabled = NavigationService.Current.HasBackward;
    }

    public override void DoCommand()
    {
        NavigationService.Current.BackwardNavigation();
    }
}

#endregion

#region ForwardMenuCommand

public class ForwardMenuCommand : MenuCommand
{
    public ForwardMenuCommand()
        : base("Forward", CoreIconCache.Forward)
    {
        HotKey = "Ctrl+Shift+-";
    }

    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        this.Enabled = NavigationService.Current.HasForward;
    }

    public override void DoCommand()
    {
        NavigationService.Current.ForwardNavigation();
    }
}

#endregion

#region CopyMenuCommand

class CopyMenuCommand : ActiveDocumentMenuCommand<IViewClipboard>
{
    public CopyMenuCommand()
        : base("Copy", CoreIconCache.Copy)
    {
        HotKey = "Ctrl+C";
    }

    protected override void OnPopUpNotFound()
    {
        this.Enabled = true;
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewClipboard clipboard)
    {
        if (HandleGlobalTextCopy())
        {
            return;
        }

        clipboard.ClipboardCopy();
    }

    protected override void DoCommandNotFound()
    {
        HandleGlobalTextCopy();
    }

    public static bool HandleGlobalTextCopy()
    {
        if (SuityApp.Instance.Window is { } mainWindow)
        {
            // Get the currently focused control
            var focusManager = TopLevel.GetTopLevel(mainWindow)?.FocusManager;
            var focusedElement = focusManager?.GetFocusedElement();

            if (focusedElement is TextBox textBox)
            {
                // Manually call TextBox copy function
                textBox.Copy();
                return true;
            }
        }

        return false;
    }
}

#endregion

#region CutMenuCommand

class CutMenuCommand : ActiveDocumentMenuCommand<IViewClipboard>
{
    public CutMenuCommand()
        : base("Cut", CoreIconCache.Cut)
    {
        HotKey = "Ctrl+X";
    }

    protected override void OnPopUpNotFound()
    {
        this.Enabled = true;
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewClipboard clipboard)
    {
        if (HandleGlobalTextCut())
        {
            return;
        }

        clipboard.ClipboardCut();
    }

    protected override void DoCommandNotFound()
    {
        HandleGlobalTextCut();
    }

    public static bool HandleGlobalTextCut()
    {
        if (SuityApp.Instance.Window is { } mainWindow)
        {
            // Get the currently focused control
            var focusManager = TopLevel.GetTopLevel(mainWindow)?.FocusManager;
            var focusedElement = focusManager?.GetFocusedElement();

            if (focusedElement is TextBox textBox)
            {
                // Manually call TextBox cut function
                textBox.Cut();
                return true;
            }
        }

        return false;
    }
}

#endregion

#region PasteMenuCommand

class PasteMenuCommand : ActiveDocumentMenuCommand<IViewClipboard>
{
    public PasteMenuCommand()
        : base("Paste", CoreIconCache.Paste)
    {
        HotKey = "Ctrl+V";
    }

    protected override void OnPopUpNotFound()
    {
        this.Enabled = true;
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewClipboard clipboard)
    {
        if (HandleGlobalTextPaste())
        {
            return;
        }

        clipboard.ClipboardPaste();
    }

    protected override void DoCommandNotFound()
    {
        HandleGlobalTextPaste();
    }

    public static bool HandleGlobalTextPaste()
    {
        if (SuityApp.Instance.Window is { } mainWindow)
        {
            // Get the currently focused control
            var focusManager = TopLevel.GetTopLevel(mainWindow)?.FocusManager;
            var focusedElement = focusManager?.GetFocusedElement();

            if (focusedElement is TextBox textBox)
            {
                // Manually call TextBox paste function
                textBox.Paste();
                return true;
            }
        }

        return false;
    }
}

#endregion

#region FindReferenceMenuCommand

class FindReferenceMenuCommand : ActiveDocumentMenuCommand<IViewSelectionInfo>
{
    public FindReferenceMenuCommand()
        : base("Find Reference", CoreIconCache.Reference)
    {
        HotKey = "Shift+F12";
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewSelectionInfo selInfo)
    {
        var obj = selInfo.SelectedObjects.FirstOrDefault();
        if (obj != null)
        {
            EditorUtility.FindReference(obj);
        }
    }

}

#endregion

#region FindImplementMenuCommand

class FindImplementMenuCommand : ActiveDocumentMenuCommand<IViewSelectionInfo>
{
    public FindImplementMenuCommand()
        : base("Find Implement", CoreIconCache.Abstract)
    {
        HotKey = "Ctrl+F12";
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewSelectionInfo selInfo)
    {
        var obj = selInfo.SelectedObjects.FirstOrDefault();
        if (obj != null)
        {
            EditorUtility.FindImplement(obj);
        }
    }

}

#endregion

#region CommentMenuCommand

class CommentMenuCommand : ActiveDocumentMenuCommand<IViewComment>
{
    public CommentMenuCommand()
        : base("Comment", CoreIconCache.Comment)
    {
        HotKey = "Alt+C";
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewComment comment)
    {
        comment.IsComment = true;
    }
}

#endregion

#region UncommentMenuCommand

class UncommentMenuCommand : ActiveDocumentMenuCommand<IViewComment>
{
    public UncommentMenuCommand()
        : base("Uncomment")
    {
        HotKey = "Ctrl+Alt+C";
    }

    protected override void DoCommandFound(DocumentEntry doc, IViewComment comment)
    {
        comment.IsComment = false;
    }
}

#endregion
