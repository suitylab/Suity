using Avalonia.Controls;
using Avalonia.Interactivity;
using Suity.Editor.Services;
using Suity.Views.Graphics;
using Suity.Views.Im;

namespace Suity.Editor.Views.Startup;

public partial class StartupWindow : Window, IDrawImGui
{
    StartupProjectGui? _projectGui;

    public StartupWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _projectGui ??= new();

        var theme = ImGuiServiceBK.Instance.GetEditorTheme(false);

        ImGuiControl.GuiTheme = theme;
        ImGuiControl.BackgroundColor = theme.Colors.GetColor(ColorStyle.Background);

        ImGuiControl.DrawImGui = this;

        this.Title = "Suity Agentic - " + SuityApp.VersionCode;
    }

    public void OnGui(ImGui gui)
    {
        _projectGui?.OnNodeGui(gui);
    }

}