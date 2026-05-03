using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.Graphics;
using System.Drawing;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Theme definition for the ImGui property grid, providing visual styling for all UI elements
/// including property rows, buttons, inputs, and layout components.
/// </summary>
public class PropertyGridTheme : ThemeBase
{
    /// <summary>
    /// Gets the default theme instance using the default color scheme.
    /// </summary>
    public static PropertyGridTheme Default => new();

    /// <summary>
    /// Gets a preview theme instance using the preview color scheme.
    /// </summary>
    public static PropertyGridTheme Preview => new(PreviewColorScheme.Default);

    public PropertyGridTheme()
    {
    }

    public PropertyGridTheme(EditorColorScheme colorScheme)
        : base(colorScheme)
    {
    }

    /// <inheritdoc/>
    protected override void OnBuildTheme()
    {
        base.OnBuildTheme();

        this.ClassStyle("componentBar")
            .SetPadding(5)
            .SetFullWidth();

        this.ClassStyle("headerBar")
            .SetPadding(5)
            .SetFullWidth();

        this.ClassStyle("componentTreeView")
            .SetPadding(5)
            .SetBorder(0, ColorScheme.ScrollBar)
            .SetColor(ColorScheme.ComponentBG);

        this.ClassStyle("nameInput")
            .SetWidth(new GuiLength(150, GuiLengthMode.RestExcept))
            .SetRectangleFrame(Color.Transparent)
            .SetPadding(0)
            .SetCenter();

        this.ClassStyle("mainBtn")
            .SetColor(ColorScheme.ToolButton)
            .SetCornerRound(5)
            .SetPadding(5)
            .SetCenter();

        this.ClassStyle("toolBtn")
            .SetColor(ColorScheme.ToolButton)
            .SetCornerRound(3)
            .SetCenter();

        this.ClassStyle("toolBtnTrans")
            .SetColor(Color.Transparent)
            .SetBorder(0)
            .SetCornerRound(3)
            .SetCenter();
        this.PseudoMouseIn()
            .SetColor(ColorScheme.ToolButton);
        this.PseudoMouseDown()
            .SetColor(ColorScheme.ToolButton.Multiply(0.8f));

        this.ClassStyle("searchBox")
            .SetBorder(2, ColorScheme.ToolButton)
            .SetColor(ColorScheme.StringInput)
            .SetCornerRound(15)
            .SetCenter();
        this.PseudoMouseIn()
             .SetBorder(2, ColorScheme.StringInputMouseIn);

        this.ClassStyle("configBtn")
            .SetColor(Color.Transparent)
            .SetSize(20, 20)
            .SetPadding(1)
            .SetBorder(0)
            .SetCornerRound(3)
            .SetCenterVertical()
            .SetImageFilterColor(ColorScheme.ButtonText);
        this.PseudoMouseIn()
            .SetImageFilterColor(Color.White);
        this.PseudoMouseDown()
            .SetImageFilterColor(ColorScheme.ButtonText.Multiply(0.8f));
        this.PseudoActive()
            .SetColor(Color.White.MultiplyAlpha(0.15f));
        this.PseudoActiveMouseIn()
            .SetColor(Color.White.MultiplyAlpha(0.15f))
            .SetImageFilterColor(Color.White);
        this.PseudoActiveMouseDown()
            .SetColor(Color.White.MultiplyAlpha(0.15f))
            .SetImageFilterColor(ColorScheme.ButtonText.Multiply(0.8f));

        this.ClassStyle("propScroll")
            .SetRectangleFrame(Color.Transparent)
            .SetBorder(0, ColorScheme.ScrollBar)
            .SetHeightRest();

        this.ClassStyle("resizer")
            .SetBorder(3, ColorScheme.EditorBG)
            .SetHeight(5);
        this.PseudoMouseIn()
            .SetBorder(3, ColorScheme.EditorMouseIn);

        this.ClassStyle("resizer_w")
            .SetBorder(3, ColorScheme.EditorBG)
            .SetHeight(5);
        this.PseudoMouseIn()
            .SetBorder(3, ColorScheme.EditorMouseIn);

        this.ClassStyle("resizer_h")
            .SetBorder(3, ColorScheme.EditorBG)
            .SetWidth(5);
        this.PseudoMouseIn()
            .SetBorder(3, ColorScheme.EditorMouseIn);

        this.ClassStyle("propResizer")
            .SetBorder(1, ColorScheme.ComponentBG)
            .SetFullHeight()
            .SetWidth(5)
            .SetHeight(10)
            .SetCenter(true)
            .SetSiblingSpacing(0);

        this.ClassStyle("propHeader")
            .SetHeaderHeight(30, 10, 12)
            .SetHeaderColor(ColorScheme.Header);

        this.ClassStyle("propBox")
            .SetColor(ColorScheme.ComponentBG)
            .SetBorder(0, ColorScheme.ScrollBar)
            .SetCornerRound(0)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.ButtonText)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(0)
            .SetChildSpacing(1);

        this.ClassStyle("propBoxShadow")
            .SetBorder(1, ColorScheme.ComponentBG)
            .SetFullWidth()
            .SetHeight(1)
            .SetSiblingSpacing(0);

        this.ClassStyle(PropertyGridThemes.ClassPropertyLine)
            .SetColor(ColorScheme.EditorBG)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(0)
            .SetChildSpacing(1)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorMouseIn);

        this.ClassStyle(PropertyGridThemes.ClassLabel)
            .SetColor(ColorScheme.EditorBG)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(0)
            .SetChildSpacing(1)
            .SetHeight(50)
            .SetPadding(2, 2, 0, 0)
            .SetRenderFunctionChain(RenderLabel);

        this.ClassStyle(PropertyGridThemes.ClassLabelCell)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(2)
            .SetColor(Color.Transparent)
            .SetVerticalAlignment(GuiAlignment.Far, false)
            .SetFitOrientation(GuiOrientation.Vertical);

        this.ClassStyle(PropertyGridThemes.ClassPropertyLineSel)
            .SetColor(ColorScheme.EditorSelection)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(0)
            .SetChildSpacing(1)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorSelectionMouseIn);

        this.ClassStyle(PropertyGridThemes.ClassPropertyEmboss)
            .SetColor(ColorScheme.ComponentBG)
            .SetHeight(36)
            .SetFullWidth()
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(0)
            .SetChildSpacing(1)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);

        this.ClassStyle(PropertyGridThemes.ClassBG)
            .SetColor(ColorScheme.EditorBG)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(0)
            .SetChildSpacing(1);

        this.ClassStyle("propLabelText")
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(2)
            .SetColor(ColorScheme.ButtonBG)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(5)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold), Color.White);

        this.ClassStyle(PropertyGridThemes.ClassPropertyCell)
            .SetBorder(0)
            .SetCornerRound(0)
            .SetPadding(2)
            .SetColor(Color.Transparent)
            .SetCenterVertical(false)
            .SetFitOrientation(GuiOrientation.Vertical);

        this.ClassStyle("propCell1")
            .SetPadding(new GuiThickness { Left = 5, Right = 2, Top = 2, Bottom = 2 });

        this.ClassStyle(PropertyGridThemes.ClassPropertyInput)
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(2)
            .SetColor(ColorScheme.ButtonBG)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(5)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.ButtonText)
            .SetCenterVertical(true);

        this.ClassStyle(PropertyGridThemes.ClassPropertyInputMultiple)
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(2)
            .SetColor(ColorScheme.ValueMultiple)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(5)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.ButtonText)
            .SetCenterVertical(true);

        this.ClassStyle(PropertyGridThemes.ClassPropertyInputReadonly)
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(2)
            .SetColor(ColorScheme.ValueReadonly)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(5)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.ButtonText)
            .SetCenterVertical(true);

        this.ClassStyle("axisX")
            .SetPadding(new GuiThickness { Left = 15, Right = 5, Bottom = 5, Top = 5 })
            .SetAxisNumericInput("x", Color.Red)
            .SetProgressColor(Color.DarkRed);

        this.ClassStyle("axisY")
            .SetPadding(new GuiThickness { Left = 15, Right = 5, Bottom = 5, Top = 5 })
            .SetAxisNumericInput("y", Color.Green)
            .SetProgressColor(Color.DarkGreen);

        this.ClassStyle("axisZ")
            .SetPadding(new GuiThickness { Left = 15, Right = 5, Bottom = 5, Top = 5 })
            .SetAxisNumericInput("z", Color.Blue)
            .SetProgressColor(Color.DarkBlue);

        this.ClassStyle("progressBar")
            .SetBorder(1, ColorScheme.ButtonBorder)
            .SetCornerRound(2)
            .SetColor(ColorScheme.ButtonBG)
            .SetImageFilterColor(ColorScheme.ButtonText)
            .SetPadding(5)
            .SetFont(new FontDef(ImGuiTheme.DefaultFont, 12), ColorScheme.ButtonText)
            .SetCenterVertical(true)
            .SetTextAlignment(GuiAlignment.Center);

        this.ClassStyle("refBox")
            .SetFitOrientation(GuiOrientation.Both)
            .SetCenterVertical()
            .SetBorder(0)
            .SetCornerRound(10)
            .SetPadding(0, 0, 2, 2);
    }

    private static readonly SolidBrushDef _labelBrush = new(Color.Black.MultiplyAlpha(0.15f));

    /// <summary>
    /// Renders a subtle label indicator at the top of label nodes.
    /// </summary>
    /// <param name="pipeline">The GUI pipeline context.</param>
    /// <param name="node">The node being rendered.</param>
    /// <param name="output">The graphic output surface.</param>
    /// <param name="dirtyMode">Indicates if rendering in dirty mode.</param>
    /// <param name="baseAction">The base render function to call first.</param>
    private void RenderLabel(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeline);

        var rect = node.GlobalRect;
        rect.Height = 5;

        output.FillRectangle(_labelBrush, rect);
    }
}
