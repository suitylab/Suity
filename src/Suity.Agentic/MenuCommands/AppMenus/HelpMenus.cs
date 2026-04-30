using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Im;
using Suity.Views.Menu;
using System.Drawing;

namespace Suity.Editor.MenuCommands.AppMenus;

#region ProjectSettingMenuCommand
class AboutMenuCommand : MenuCommand
{
    public AboutMenuCommand()
        : base("About Suity", CoreIconCache.Suity)
    {
    }

    public override void DoCommand()
    {
        new AboutWindowImGui().CreateImGuiDialog("Project Setting", 600, 220, true, true);
    }
}

#endregion

#region AboutWindowImGui

class AboutWindowImGui : IDrawImGui
{
    static readonly Image LOGO = Suity.Editor.Properties.Resources.Logo196.ToBitmap();
    static readonly Font BIG_FONT = new Font(ImGuiTheme.DefaultFont, 42, FontStyle.Bold);
    static readonly Font MEDIDUM_FONT = new Font(ImGuiTheme.DefaultFont, 22);
    static readonly Font SMALL_FONT = new Font(ImGuiTheme.DefaultFont, 14);

    public void OnGui(ImGui gui)
    {
        gui.HorizontalLayout()
        .InitFullSize()
        .InitPadding(10)
        .InitChildSpacing(10)
        .OnContent(() => 
        {
            gui.Image(LOGO, true);

            gui.VerticalLayout()
            .InitSizeRest()
            .OnContent(() => 
            {
                gui.HorizontalLayout()
                .InitFit()
                .InitChildSpacing(10)
                .OnContent(() => 
                {
                    gui.Text("Suity Agentic")
                    .InitFont(BIG_FONT)
                    .InitFontColor(Color.White)
                    .InitCenterVertical();

                    gui.HorizontalFrame()
                    .InitClass("refBox")
                    .InitOverrideColor(DefaultEditorColorConfig.Default.GetStatusColor(TextStatus.Info))
                    .InitFit()
                    .InitOverridePadding(0, 0, 5, 5)
                    .InitCenter()
                    .OnContent(() =>
                    {
                        gui.Text(SuityApp.VersionCode);
                    });
                });

                gui.Text("OpenSource Agentic IDE")
                .InitFont(MEDIDUM_FONT)
                .InitFontColor(Color.LightGray);

                gui.VerticalLayout()
                .InitHeight(30);

                gui.Button("Github Page")
                .InitClass("button")
                .InitHorizontalAlignment(GuiAlignment.Near)
                .OnClick(() =>
                {
                    EditorUtility.OpenBrowser(SuityApp.GithubPage);
                });

                gui.VerticalLayout()
                .InitHeight(20);

                gui.Text("Copyright © 2026 Suity Agentic Team")
                .InitFont(SMALL_FONT)
                .InitFontColor(Color.Gray);
            });
        });
    }
}

#endregion