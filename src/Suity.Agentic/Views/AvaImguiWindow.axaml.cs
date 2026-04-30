using Avalonia.Controls;
using Suity.Editor.Services;
using Suity.Views.Graphics;
using Suity.Views.Im;

namespace Suity.Editor.Views;

public partial class AvaImguiWindow : Window, IDrawImGui
{
    private IDrawImGui? _imGui;

    public AvaImguiWindow()
    {
        InitializeComponent();

        var theme = AvaImGuiService.Instance.GetEditorTheme(false);
        ImGuiControl.GuiTheme = theme;
        ImGuiControl.BackgroundColor = theme.Colors.GetColor(ColorStyle.Background);
        ImGuiControl.DrawImGui = this;
    }

    public AvaImguiWindow(IDrawImGui imGui)
        : this()
    {
        _imGui = imGui ?? throw new System.ArgumentNullException(nameof(imGui));
    }

    public IDrawImGui? ImGui
    {
        get => ImGuiControl.DrawImGui;
        set => ImGuiControl.DrawImGui = value;
    }

    public void OnGui(ImGui gui)
    {
        _imGui?.OnGui(gui);

        if (gui.IsClosing)
        {
            Close();
        }
    }
}