using AvaloniaEdit;
using Suity.Views.Menu;

namespace Suity.Editor.MenuCommands;

class TextEditorMenu : RootMenuCommand
{
    public TextEditorMenu()
        : base(":TextEditor")
    {
        AddCommand(new SimpleMenuCommand("Copy", "Copy", "Ctrl+C", CoreIconCache.Copy, m => HandleCopy(m.Sender as TextEditor)));
        AddCommand(new SimpleMenuCommand("Cut", "Cut", "Ctrl+X", CoreIconCache.Cut, m => HandleCut(m.Sender as TextEditor)));
        AddCommand(new SimpleMenuCommand("Paste", "Paste", "Ctrl+V", CoreIconCache.Paste, m => HandlePaste(m.Sender as TextEditor)));
        AddSeparator();
        AddCommand(new SimpleMenuCommand("SelectAll", "Select All", "Ctrl+A", CoreIconCache.Select, m => HandleSelectAll(m.Sender as TextEditor)));
    }

    private void HandleCopy(TextEditor? editor)
    {
        editor?.Copy();
    }
    private void HandleCut(TextEditor? editor)
    {
        editor?.Cut();
    }
    private void HandlePaste(TextEditor? editor)
    {
        editor?.Paste();
    }
    private void HandleSelectAll(TextEditor? editor)
    {
        editor?.SelectAll();
    }
}
