using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Suity.Editor.AIGC;
using Suity.Editor.MenuCommands.AppMenus;

namespace Suity.Editor.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        ProjectTitleButton.Command = new RelayCommand(ProjectSettingMenuCommand.OpenProjectSetting);
        HomeButton.Command = new RelayCommand(() => EditorUtility.ShowToolWindow(nameof(AigcStartupWindow)));
        SearchButton.Command = new RelayCommand(NavigateMenuCommand.HandleNavigate);
    }
}
