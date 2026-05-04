using Suity.Drawing;
using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Views.Im;

public abstract class ThemeBase : ImGuiTheme
{
    public EditorColorScheme ColorScheme { get; }

    public ThemeBase()
        : this(EditorColorScheme.Default)
    {
    }

    public ThemeBase(EditorColorScheme colorScheme)
    {
        ColorScheme = colorScheme ?? throw new ArgumentNullException(nameof(colorScheme));
    }

    protected override void OnBuildTheme()
    {
        this.ClassStyle("bg")
            .SetRectangleFrame(ColorScheme.Background)
            .SetPadding(0);

        this.ClassStyle("frameBg")
            .SetRectangleFrame(ColorScheme.EditorBG)
            .SetPadding(0);

        this.ClassStyle("editorBg")
            .SetRectangleFrame(ColorScheme.EditorBG)
            .SetPadding(0)
            .SetSizeRest();

        this.ClassStyle("toolBar")
            .SetRectangleFrame(ColorScheme.ComponentBG)
            .SetPadding(new GuiThickness { Top = 5, Bottom = 5 });

        this.ClassStyle("icon")
            .SetSize(16, 16)
            .SetVerticalAlignment(GuiAlignment.Center);

        this.ClassStyle("iconDark")
            .SetSize(16, 16)
            .SetImageFilterColor(ColorScheme.EditorBG)
            .SetVerticalAlignment(GuiAlignment.Center);

        this.ClassStyle("numBox")
            .SetHeight(13)
            .SetFitOrientation(GuiOrientation.Horizontal)
            .SetCenterVertical()
            .SetBorder(0)
            .SetCornerRound(10)
            .SetPadding(0, 0, 5, 5)
            .SetChildSpacing(5)
            .SetColor(Color.FromArgb(20, 255, 255, 255));

        this.ClassStyle("numBoxDark")
            .SetHeight(13)
            .SetFitOrientation(GuiOrientation.Horizontal)
            .SetCenterVertical()
            .SetBorder(0)
            .SetCornerRound(10)
            .SetPadding(0, 0, 5, 5)
            .SetChildSpacing(5)
            .SetColor(ColorScheme.EditorBG);

        this.ClassStyle("numBoxText")
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 10), Color.White)
            .SetHorizontalAlignment(GuiAlignment.Center)
            .SetVerticalAlignment(GuiAlignment.Center);

        this.ClassStyle("button")
            .SetColor(ColorScheme.EditorBG2)
            .SetBorder(0)
            .SetPadding(8)
            .SetCornerRound(7);

        this.ClassStyle("selBtn")
            .SetColor(ColorScheme.EditorSelection)
            .SetCornerRound(3);

        this.ClassStyle("smallBtn")
            .SetSize(18, 18)
            .SetPadding(1)
            .SetBorder(0)
            .SetColor(ColorScheme.ToolButton)
            .SetCornerRound(2);

        this.ClassStyle("simpleBtn")
            .SetColor(ColorScheme.ToolButton)
            .SetCornerRound(3);

        this.ClassStyle("simpleFrame")
            .SetColor(ColorScheme.EditorBG)
            .SetPadding(8)
            .SetCornerRound(4)
            .SetBorder(0);


        this.ClassStyle("toolTipFrame")
            .SetColor(ColorScheme.ToolButton.MultiplyAlpha(0.5f))
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(6)
            .SetCenter()
            .SetPadding(5);

        this.ClassStyle("toolTipText")
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 10), ColorScheme.ButtonText);

        this.ClassStyle("placement")
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.PlacementText);

        this.ClassStyle("brief")
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.BriefText);

        this.ClassStyle("briefMultiple")
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.BriefTextMultiple);
    }
}
