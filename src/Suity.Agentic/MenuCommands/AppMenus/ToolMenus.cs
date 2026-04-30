using Suity.Views.Menu;

namespace Suity.Editor.MenuCommands.AppMenus;

#region ProjectSettingMenuCommand
class ProjectSettingMenuCommand : MenuCommand
{
    public ProjectSettingMenuCommand()
        : base("Project Setting", CoreIconCache.Project)
    {
    }

    public override void DoCommand() => OpenProjectSetting();

    public static void OpenProjectSetting()
    {
        new ProjectSettingGui().CreateImGuiDialog("Project Setting", 800, 800, false);
    }
}

#endregion
