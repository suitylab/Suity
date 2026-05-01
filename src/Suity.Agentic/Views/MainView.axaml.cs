using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Suity.Editor.AIGC;
using Suity.Editor.MenuCommands.AppMenus;
using System.Linq;

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

    private void TitleBar_OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var window = this.GetVisualAncestors().OfType<Window>().FirstOrDefault();
            if (window == null) return;

            if (e.ClickCount == 2 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ToggleWindowState(window);
                return;
            }

            if (e.Source is Visual visual && visual is not Button && visual is not MenuItem)
            {
                window.BeginMoveDrag(e);
            }
        }
    }

    private void ToggleWindowState(Window window)
    {
        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
