using Suity.Editor.AIGC;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Theme definition for the node graph control, providing styles for nodes, connectors, and UI elements.
/// </summary>
public class NodeGraphTheme : ThemeBase
{
    /// <summary>
    /// Gets the default node graph theme instance.
    /// </summary>
    public static NodeGraphTheme Default { get; } = new();

    /// <summary>
    /// Gets the preselection highlight color.
    /// </summary>
    public static Color ColorPreslect { get; } = Color.White.MultiplyAlpha(0.5f);

    /// <inheritdoc/>
    protected override void OnBuildTheme()
    {
        base.OnBuildTheme();

        this.SetColllection(PropertyGridTheme.Default);

        this.ClassStyle("node")
            .SetColor(ColorScheme.ToolButton)
            .SetPadding(1)
            .SetCornerRound(6)
            .SetBorder(0)
            .SetLayoutFunctionChain(LeftRightAlignLayout)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender)
            .SetChildSpacing(0);
        this.PseudoMouseIn()
            //.SetColor(ColorScheme.ToolButtonMouseIn)
            .SetBorder(2, ColorPreslect, false);
        this.PseudoActive()
            .SetBorder(3, ColorScheme.Highlight, scaled: false);
        this.PseudoActiveMouseIn()
            //.SetColor(ColorScheme.ToolButtonMouseIn)
            .SetBorder(3, ColorScheme.Highlight, scaled: false);

        this.ClassStyle("headerFrame")
            .SetFitOrientation(GuiOrientation.Vertical)
            .SetFullWidth()
            .SetColor(AigcColors.NodeHaderColor)
            .SetBorder(0)
            .SetCornerRound(6)
            .SetPadding(4);

        this.ClassStyle("headerFrameFit")
            .SetFitOrientation(GuiOrientation.Both)
            .SetColor(AigcColors.NodeHaderColor)
            .SetBorder(0)
            .SetCornerRound(6)
            .SetPadding(4);

        this.ClassStyle("commentText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 6), Color.White)
            .SetBorder(0)
            .SetPadding(0);

        this.ClassStyle("titleText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 10), Color.White)
            .SetVerticalAlignment(GuiAlignment.Center)
            .SetInputFunctionChain(ImGraphExtensions.ScaleInputBlockerMedium)
            .SetRenderFunctionChain(ImGraphExtensions.ScaleRenderBlockerMedium);

        this.ClassStyle("bodyFit")
            .SetFitOrientation(GuiOrientation.Both)
            .SetPadding(2);

        this.ClassStyle("body")
            .SetPadding(2);

        this.ClassStyle("sideBoxHori")
            .SetFitOrientation(GuiOrientation.Vertical)
            .SetFullWidth()
            .SetPadding(0);

        this.ClassStyle("sideBoxVert")
            .SetFitOrientation(GuiOrientation.Horizontal)
            .SetFullHeight()
            .SetPadding(0)
            .SetChildSpacing(0);

        this.ClassStyle("sideBoxFit")
            .SetFitOrientation(GuiOrientation.Both)
            .SetPadding(0)
            .SetChildSpacing(0);

        this.ClassStyle("inputRow")
            .SetFitOrientation(GuiOrientation.Both)
            .SetHorizontalAlignment(GuiAlignment.Near);

        this.ClassStyle("outputRow")
            .SetFitOrientation(GuiOrientation.Both)
            .SetHorizontalAlignment(GuiAlignment.Far);

        this.ClassStyle("connectorText")
            .SetFont(new Font(ImGuiTheme.DefaultFont, 6), Color.White)
            .SetCenterVertical()
            .SetInputFunctionChain(ImGraphExtensions.ScaleInputBlockerNear)
            .SetRenderFunctionChain(ImGraphExtensions.ScaleRenderBlockerNear);

        this.ClassStyle("connectorPoint")
            .SetSize(6, 6)
            .SetBorder(0)
            .SetCenterVertical()
            .SetColor(Color.Green)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetBorder(2, Color.LightGray, false);

        this.ClassStyle("connectorFrame")
            .SetFitOrientation(GuiOrientation.Both)
            .SetCenterVertical()
            .SetBorder(0)
            .SetCornerRound(3)
            .SetPadding(2)
            .SetColor(ColorScheme.ToolButton);
        this.PseudoActive()
            .SetColor(ColorScheme.EditorBG)
            .SetBorder(1, TypeDefinition.FromNative<IDataTransport>()?.Target?.ViewColor ?? ColorScheme.Highlight);

        this.ClassStyle("iconSmall")
            .SetSize(8, 8)
            .SetVerticalAlignment(GuiAlignment.Center);

        this.ClassStyle("refBox")
            .SetFitOrientation(GuiOrientation.Both)
            .SetCenterVertical()
            .SetBorder(0)
            .SetCornerRound(10)
            .SetPadding(1)
            .SetColor(ColorScheme.EditorBG);
        this.PseudoActive()
            .SetColor(ColorScheme.Highlight);

        this.ClassStyle("configBtn")
            .SetColor(Color.Transparent)
            .SetSize(16, 16)
            .SetPadding(0)
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


        this.ClassStyle("scaleHiddenMedium")
            .SetInputFunctionChain(ImGraphExtensions.ScaleInputBlockerMedium)
            .SetRenderFunctionChain(ImGraphExtensions.ScaleRenderBlockerMedium);

        this.ClassStyle("scaleHiddenFar")
            .SetInputFunctionChain(ImGraphExtensions.ScaleInputBlockerFar)
            .SetRenderFunctionChain(ImGraphExtensions.ScaleRenderBlockerFar);

        this.ClassStyle("scaleHiddenVeryFar")
            .SetInputFunctionChain(ImGraphExtensions.ScaleInputBlockerVeryFar)
            .SetRenderFunctionChain(ImGraphExtensions.ScaleRenderBlockerVeryFar);

        this.ClassStyle("toolBtn")
            .SetSize(12, 12)
            .SetColor(ColorScheme.ToolButton)
            .SetBorder(0)
            .SetPadding(1)
            .SetCornerRound(1)
            .SetCenter();

        this.ClassStyle("toolBtnTrans")
            .SetSize(12, 12)
            .SetColor(Color.Transparent)
            .SetBorder(0)
            .SetPadding(1)
            .SetCornerRound(1)
            .SetCenter();
        this.PseudoMouseIn()
            .SetColor(ColorScheme.ToolButton);
        this.PseudoMouseDown()
            .SetColor(ColorScheme.ToolButton.Multiply(0.8f));

        this.ClassStyle("debug_draw")
            .SetRenderFunctionChain(RenderDebug);
    }

    /// <summary>
    /// Left-right alignment layout function that extends nodes to fill parent width and aligns the last child to the right edge.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being laid out.</param>
    /// <param name="position">The layout position information.</param>
    /// <param name="childAction">The base layout function to call for child layout.</param>
    private void LeftRightAlignLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition position, ChildLayoutFunction childAction)
    {
        childAction(pipeline);

        if (pipeline == GuiPipeline.Align && node.Classes.Contains("left-right"))
        {
            var rect = node.Rect;
            var pRect = node.Parent!.InnerRect;

            if (rect.Right < pRect.Right)
            {
                float ox = pRect.Right - rect.Right;

                rect.Width += ox;
                node.Rect = rect;
            }

            // If the node has more than one child, align the last child to the right
            if (node.ChildNodeCount > 1)
            {
                var child = node.GetChildNode(node.ChildNodeCount - 1)!;

                if (child.Rect.Width > 0)
                {
                    float cx = node.InnerRect.Right - child.Rect.Width;
                    float ox2 = cx - child.Rect.X;

                    if (ox2 != 0)
                    {
                        child.OffsetPositionDeep(ox2, 0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Debug render function that draws green bounding boxes and node information when global debug drawing is enabled.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    private void RenderDebug(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        if (!ImGuiBK.GlobalDebugDraw)
        {
            childAction(pipeline);
            return;
        }

        childAction(pipeline);

        var rect = node.GlobalRect;

        var pen = new Pen(Color.Green, 1);
        output.DrawRectangle(pen, rect);

        var font = new Font(ImGuiTheme.DefaultFont, 12);
        var brush = new SolidBrush(Color.White);
        output.DrawString($"{node.Id} {(int)rect.Width}x{(int)rect.Height}", font, brush, new PointF(rect.X, rect.Y + 12));
    }
}
