using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.Views.Startup;

public class StartupStyles
{
    public class NavBtnValue : IValueTransition<NavBtnValue>
    {
        public float Value { get; set; }

        public NavBtnValue Lerp(NavBtnValue v2, float t)
        {
            float v = MathHelper.Lerp(Value, v2.Value, t);
            return new NavBtnValue { Value = v };
        }
    }

    public static StartupStyles Instance { get; } = new StartupStyles();

    public readonly Color _colorTextStatusOnline = Color.FromArgb(124, 197, 118);
    public readonly Color _colorFormBg = Color.FromArgb(43, 49, 52);
    public readonly Color _colorFormHeader = Color.FromArgb(20, 23, 24);
    public readonly Color _colorFormBody = Color.FromArgb(34, 38, 39);


    public Bitmap Icon { get; } = Suity.Editor.Properties.Resources.LogoText48;

    public ImGuiTheme Theme { get; }

    private StartupStyles()
    {
        Theme = SetupTheme();
    }

    private ImGuiTheme SetupTheme()
    {
        var theme = new ImGuiTheme();

        theme.ClassStyle("statusText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), _colorTextStatusOnline);

        theme.ClassStyle("formBG")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(_colorFormBg)
            .SetPadding(0)
            .SetFullSize()
            .SetFitOrientation(GuiOrientation.None);

        theme.ClassStyle("formHeader")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(_colorFormHeader)
            .SetPadding(new GuiThickness { Left = 20 })
            .SetLayoutFunctionChain(ImGuiLayoutSystem.Horizontal)
            .SetFitFunctionChain(ImGuiFitSystem.Auto);

        theme.ClassStyle("formBody")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(_colorFormBody)
            .SetPadding(0);

        theme.ClassStyle("formFooter")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(_colorFormHeader)
            .SetPadding(new GuiThickness { Left = 20 });

        theme.ClassStyle("formFooterNotice")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(Color.Khaki)
            .SetPadding(new GuiThickness { Left = 20 });

        theme.ClassStyle("noticeText")
           .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.Black);

        theme.ClassStyle("navBtn")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetPadding(new GuiThickness { Left = 20, Right = 20 })
            .SetCenter()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.FromArgb(77, 88, 94))
            .SetHeight(50)
            .SetStyleFluent(new NavBtnValue() { Value = 0 })
            .SetRenderFunctionChain(RenderNavBtnActive);
        theme.PseudoMouseIn()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White);
        theme.PseudoMouseDown()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.LightGray);
        theme.PseudoActive()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White)
            .SetStyleFluent(new NavBtnValue() { Value = 1 });

        theme.SetMouseInLinearTransition(0.2f, 0.4f);

        theme.SetLinearTransition(ImGuiNode.PseudoActive, null, 0.2f);

        theme.ClassStyle("bodyHeader")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(25)
            .SetChildSpacing(20)
            .SetLayoutFunctionChain(ImGuiLayoutSystem.Overlay)
            .SetFitFunctionChain(ImGuiFitSystem.Overlay);

        theme.ClassStyle("title")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 28, FontStyle.Bold), Color.White);

        theme.ClassStyle("smallText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White);

        theme.ClassStyle("transButton")
            .SetColor(Color.Transparent)
            .SetBorder(0)
            .SetPadding(0)
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.FromArgb(174, 176, 175));
        theme.PseudoMouseIn()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White);

        theme.ClassStyle("projectFrame")
            .SetBorder(0)
            .SetCornerRound(0);

        theme.ClassStyle("projectTitle")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 16), Color.FromArgb(174, 176, 175));
        theme.PseudoMouseIn()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 16), Color.White);

        theme.ClassStyle("projectText1")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.FromArgb(174, 176, 175));
        theme.PseudoMouseIn()
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White);

        theme.ClassStyle("projectText2")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.FromArgb(152, 152, 152));

        theme.ClassStyle("projectItem")
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(new GuiThickness { Left = 20, Right = 20, Top = 10, Bottom = 10 })
            .SetColor(Color.FromArgb(0, 20, 23, 24))
            .SetSize(new GuiLength(100, GuiLengthMode.Percentage), 60)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRefresh);
        theme.PseudoMouseIn()
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetMouseInLinearTransition(0.2f, 0.2f);
        theme.PseudoMouseDown()
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White)
            .SetBorder(2, _colorTextStatusOnline.MultiplyAlpha(0.5f));
        theme.PseudoActive()
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White)
            .SetBorder(2, _colorTextStatusOnline);
        theme.PseudoActiveMouseIn()
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White)
            .SetBorder(2, _colorTextStatusOnline);
        theme.PseudoActiveMouseDown()
            .SetColor(Color.FromArgb(20, 23, 24))
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.White)
            .SetBorder(2, _colorTextStatusOnline);


        theme.ClassStyle("templateText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 16), Color.White);

        theme.ClassStyle("templateText2")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 12), Color.FromArgb(152, 152, 152));

        theme.ClassStyle("icon32")
            .SetSize(32, 32)
            .SetImageFilterColor(Color.White.MultiplyAlpha(0.5f));

        theme.ChildSpacing = 1;

        return theme;
    }

    private static void RenderNavBtnActive(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeline);

        int h = 1;
        var rect = node.GlobalRect;
        rect.Y += rect.Height - h;
        rect.Height = h;

        var value = node.GetStyle<NavBtnValue>()?.Value ?? 1;

        float mid = rect.Width * 0.5f;
        rect.X = (int)MathHelper.Lerp(rect.X + mid, rect.X, value);
        rect.Width = (int)MathHelper.Lerp(0, rect.Width, value);

        var brush = new SolidBrush(Color.Cyan);
        output.FillRectangle(brush, rect);
    }
}