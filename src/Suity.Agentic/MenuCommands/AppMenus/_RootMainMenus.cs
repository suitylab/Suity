using Suity.Editor.Services;
using Suity.Views.Menu;

namespace Suity.Editor.MenuCommands.AppMenus;

class RootMainMenu : RootMenuCommand
{
    public RootMainMenu()
         : base(":Main")
    {
        AddCommand(new FileMenu());
        AddCommand(new EditMenu());
        AddCommand(new ViewMenu());
        AddCommand(new ToolMenu());
        AddCommand(new HelpMenu());
    }
}

class FileMenu : MainMenuCommand
{
    public FileMenu()
        : base("File", "_File")
    {
        AddCommand(new SaveMenuCommand());
        AddCommand(new SaveAllMenuCommand());
        AddSeparator();
        AddCommand(new ExitCommand());
        AddSeparator();
    }
}

class EditMenu : MainMenuCommand
{
    public EditMenu()
        : base("Edit", "_Edit")
    {
        AddCommand(new LocateMenuCommand());
        AddSeparator();
        AddCommand(new UndoMenuCommand());
        AddCommand(new RedoMenuCommand());
        AddSeparator();
        AddCommand(new BackwardMenuCommand());
        AddCommand(new ForwardMenuCommand());
        AddSeparator();
        AddCommand(new CopyMenuCommand());
        AddCommand(new CutMenuCommand());
        AddCommand(new PasteMenuCommand());
        AddSeparator();
        AddCommand(new FindReferenceMenuCommand());
        AddCommand(new FindImplementMenuCommand());
        AddSeparator();
        AddCommand(new CommentMenuCommand());
        AddCommand(new UncommentMenuCommand());
        AddSeparator();
    }
}

class ViewMenu : MainMenuCommand
{
    public ViewMenu()
        : base("View", "_View")
    {
        foreach (var toolWindow in EditorServices.ToolWindow?.ToolWindows ?? [])
        {
            AddCommand(new ShowToolWindowMenuCommand(toolWindow));
        }

        AddSeparator();

        AddCommand(new ResetLayoutMenuCommand());
        AddCommand(new ToggleDescription());
        AddSeparator();
    }
}

class ToolMenu : MainMenuCommand
{
    public ToolMenu()
        : base("Tool", "_Tool")
    {
        AddCommand(new ProjectSettingMenuCommand());
        AddSeparator();
    }
}

class HelpMenu : MainMenuCommand
{
    public HelpMenu()
        : base("Help", "_Help")
    {
        AddCommand(new AboutMenuCommand());
        AddSeparator();
    }
}