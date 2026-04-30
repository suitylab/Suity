using Suity.Collections;
using Suity.Helpers;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Represents the visual state of a text box control.
/// </summary>
[Flags]
public enum TextBoxState
{
    /// <summary>
    /// The text box is disabled.
    /// </summary>
    Disabled = 0x1,

    /// <summary>
    /// The text box is in normal state.
    /// </summary>
    Normal = 0x2,

    /// <summary>
    /// The text box is hovered (hot).
    /// </summary>
    Hot = 0x4,

    /// <summary>
    /// The text box has focus.
    /// </summary>
    Focus = 0x8,

    /// <summary>
    /// The text box is read-only.
    /// </summary>
    ReadOnlyFlag = 0x100
}

/// <summary>
/// Represents the visual style of a text box border.
/// </summary>
public enum TextBoxStyle
{
    /// <summary>
    /// No border style.
    /// </summary>
    Plain,

    /// <summary>
    /// Flat border style.
    /// </summary>
    Flat,

    /// <summary>
    /// Sunken (inset) border style.
    /// </summary>
    Sunken
}

/// <summary>
/// Default render system for ImGui controls, providing rendering functions for common UI elements.
/// </summary>
public class ImGuiRenderSystemBK : ImGuiRenderSystem
{
    private static ImGuiRenderSystemBK? _instance;

    /// <summary>
    /// Gets the singleton instance of the render system.
    /// </summary>
    public static ImGuiRenderSystemBK Instance => _instance ??= new();

    private readonly Dictionary<string, RenderFunction> _functions = [];

    /// <summary>
    /// Initializes a new render system and registers all built-in render functions.
    /// </summary>
    public ImGuiRenderSystemBK()
    {
        _functions[nameof(GuiCommonExtensions.Image)] = RenderImage;
        _functions[nameof(GuiButtonExtensions.Button)] = RenderButton;
        _functions[nameof(GuiButtonExtensions.ExpandButton)] = RenderExpandButton;
        _functions[nameof(GuiToggleExtensions.ToggleButton)] = RenderToggleButton;
        _functions[nameof(GuiButtonExtensions.DropDownButton)] = RenderDropDownButton;
        _functions[nameof(GuiToggleExtensions.CheckBox)] = RenderCheckBox;
        _functions[nameof(GuiTextExtensions.Text)] = RenderText;
        _functions[nameof(GuiTextExtensions.TextArea)] = RenderTextArea;
        _functions[nameof(GuiCommonExtensions.Frame)] = RenderFrame;
        _functions[nameof(GuiScrollableExtensions.ScrollableFrame)] = RenderScrollableFrame;
        _functions[nameof(GuiPanelExtensions.Panel)] = RenderPanel;
        _functions[nameof(GuiPanelExtensions.ExpandablePanel)] = RenderExpandablePanel;
        _functions[nameof(GuiCommonExtensions.HorizontalLine)] = RenderHorizontalLine;
        _functions[nameof(GuiCommonExtensions.VerticalLine)] = RenderVerticalLine;
        _functions[nameof(GuiTextInputExtensions.StringInput)] = RenderStringInput;
        _functions[nameof(GuiTextInputExtensions.NumericInput)] = RenderNumericInput;
        _functions[nameof(GuiTextInputExtensions.TextAreaInput)] = RenderTextAreaInput;
        _functions[nameof(GuiVirtualListExtensions.VirtualList)] = RenderVirtualList;
    }

    /// <inheritdoc/>
    public override RenderFunction? GetRenderFunction(string name)
    {
        var func = _functions.GetValueSafe(name);

        if (func is null)
        {
            Debug.WriteLine($"{nameof(ImGuiRenderSystemBK)} function not found : {name}");
        }

        return func;
    }

    private void RenderImage(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        Image? img = node.Image;
        if (img is null)
        {
            return;
        }

        output.DrawImageCached(img, node.GlobalRect, node.ImageFilterColor);

        baseAction(pipeline);
    }

    private void RenderButton(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        RenderButton(pipeline, node, output, baseAction, false, null);
    }

    private void RenderExpandButton(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var value = node.GetOrCreateValue<GuiExpandableValue>();
        node.Image = value.Expanded ? node.Theme.ExpandImage : node.Theme.CollapseImage;

        RenderButton(pipeline, node, output, baseAction, false, null);
    }

    private void RenderToggleButton(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var v = node.GetValue<GuiToggleValue>();
        bool toggle = v?.Value == CheckState.Checked;

        RenderButton(pipeline, node, output, baseAction, false, toggle);
    }

    private void RenderDropDownButton(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        RenderButton(pipeline, node, output, baseAction, true, null);
    }

    private void RenderText(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var font = ImGuiExternalBK.Instance.GetScaledFont(node);
        var fontColor = node.GetFontColor();

        if (node.IsDisabled)
        {
            fontColor = fontColor.MultiplyAlpha(0.5f);
        }

        var rect = node.GlobalRect;
        //float x = rect.X;
        //float y = rect.Y + rect.Height * 0.5f + font.Size * 0.5f;

        string text = node.Text ?? node.Id;

        var alignment = node.TextAlignment ?? GuiAlignment.Near;

        RenderText(node.Gui.Output, node, text, font, fontColor, rect, alignment);

        baseAction(pipeline);
    }

    private void RenderTextArea(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        Font? font = ImGuiExternalBK.Instance.GetScaledFont(node);
        Color fontColor = node.GetFontColor();

        if (node.IsDisabled)
        {
            fontColor = fontColor.MultiplyAlpha(0.5f);
        }

        string text = node.Text ?? string.Empty;

        output.DrawTextArea(text, font, fontColor, node.GlobalRect);

        baseAction(pipeline);
    }

    private void RenderCheckBox(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var width = node.GlobalScaleValue(node.Theme.CheckBoxWidth);
        var rect = node.GlobalRect;

        float midY = rect.Y + rect.Height * 0.5f;
        float midH = width * 0.5f;

        var boxRect = new RectangleF
        {
            X = rect.X,
            Y = midY - midH,
            Width = width,
            Height = width,
        };

        var color = node.GetCheckBoxColor();
        var fontColor = node.GetFontColor();
        bool disabled = node.IsDisabled;

        if (!node.HasPseudoStyle)
        {
            if (disabled)
            {
                color = color.MultiplyAlpha(0.5f);
                fontColor = fontColor.MultiplyAlpha(0.5f);
            }
            else
            {
                if (node.IsMouseInClickRect)
                {
                    if (node.MouseState == GuiMouseState.Pressed)
                    {
                        color = color.Multiply(0.8f);
                    }
                    else
                    {
                        color = color.MultiplyRevert(0.8f);
                    }
                }
            }
        }

        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetCheckBoxCornerRound(true);

        RenderFrame(output, boxRect, color, corner, border, borderColor);

        var value = node.GetValue<GuiToggleValue>();
        if (value is { })
        {
            Color? imgColor = node.ImageFilterColor;
            if (disabled && imgColor is { })
            {
                imgColor = imgColor.Value.MultiplyAlpha(0.5f);
            }

            switch (value.Value)
            {
                case CheckState.Checked:
                    output.DrawImageCached(node.Theme.CheckBoxCheckedImage, boxRect, imgColor);
                    break;

                case CheckState.Indeterminate:
                    output.DrawImageCached(node.Theme.CheckBoxPendingImage, boxRect, imgColor);
                    break;
            }
        }

        var text = node.Text;
        if (!string.IsNullOrEmpty(text))
        {
            var spacing = node.GlobalScaleValue(node.ChildSpacing ?? 0);

            var textRect = new RectangleF
            {
                X = boxRect.Right + spacing,
                Y = boxRect.Y,
                Width = rect.Width - boxRect.Width - spacing,
                Height = width,
            };

            var alignment = node.TextAlignment ?? GuiAlignment.Near;
            var font = ImGuiExternalBK.Instance.GetScaledFont(node);

            RenderText(node.Gui.Output, node, text!, font, fontColor, textRect, alignment);
        }
    }

    private void RenderFrame(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetFrameCornerRound(true);

        RenderFrame(node.Gui.Output, node.GlobalRect, node.Color, corner, border, borderColor);

        baseAction(pipeline);
    }

    private void RenderScrollableFrame(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetFrameCornerRound(true);

        RenderFrame(node.Gui.Output, node.GlobalRect, node.Color, corner, border, borderColor);

        baseAction(pipeline);

        var value = node.GetValue<GuiScrollableValue>();
        if (value == null)
        {
            return;
        }

        var color = node.ScrollBarColor ?? node.Theme.Colors.GetColor(ColorStyle.ScrollBar);

        RenderScrollBar(node.Gui.Output, node, value, color);

        baseAction(pipeline);
    }

    private void RenderPanel(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetFrameCornerRound(true);

        RenderFrame(node.Gui.Output, node.GlobalRect, node.GetBackgroundColor(), corner, border, borderColor);
        RenderHeaderBar(node, output, node.Expanded);

        baseAction(pipeline);
    }

    private void RenderExpandablePanel(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        Color color = node.GetBackgroundColor();
        Color? headerColor = node.HeaderColor;

        if (!node.HasPseudoStyle)
        {
            if (node.IsMouseInClickRect && headerColor.HasValue)
            {
                if (node.MouseState == GuiMouseState.Pressed)
                {
                    headerColor = headerColor.Value.Multiply(0.9f);
                }
                else
                {
                    headerColor = headerColor.Value.MultiplyRevert(0.9f);
                }
            }
        }

        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetFrameCornerRound(true);

        if (node.GetIsExpanded())
        {
            RenderFrame(node.Gui.Output, node.GlobalRect, color, corner, border, borderColor);

            if (headerColor.HasValue)
            {
                var rect = node.GlobalMouseClickRect ?? node.GlobalRect;
                rect = new RectangleF(rect.X + border, rect.Y + border, rect.Width - border - border, rect.Height - border - border);
                RenderFrame(node.Gui.Output, rect, headerColor, corner, 0, borderColor);
            }

            RenderHeaderBar(node, output, node.Expanded);
        }
        else
        {
            RenderFrame(node.Gui.Output, node.GlobalRect, headerColor, corner, border, borderColor);
            RenderHeaderBar(node, output, node.Expanded);
        }

        baseAction(pipeline);
    }

    private void RenderHorizontalLine(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        float border = node.ScaledBorderWidth;
        if (border <= 0)
        {
            return;
        }

        var rect = node.GlobalRect;
        Color color = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);

        float x1 = rect.Left;
        float x2 = rect.Right;
        float y = rect.Y + rect.Height * 0.5f;

        var pen = new Pen(color, border);

        output.DrawLine(pen, new PointF(x1, y), new PointF(x2, y));

        baseAction(pipeline);
    }

    private void RenderVerticalLine(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        float border = node.ScaledBorderWidth;
        if (border <= 0)
        {
            return;
        }

        var rect = node.GlobalRect;
        Color color = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);

        float y1 = rect.Top;
        float y2 = rect.Bottom;
        float x = rect.X + rect.Width * 0.5f;

        var pen = new Pen(color, border);

        output.DrawLine(pen, new PointF(x, y1), new PointF(x, y2));

        baseAction(pipeline);
    }

    private void RenderStringInput(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        bool disabled = node.IsDisabled;

        if (pipeline.HasFlag(GuiPipeline.Main) || pipeline.HasFlag(GuiPipeline.Background))
        {
            var color = node.GetTextInputBackgroundColor();

            if (disabled)
            {
                color = color.MultiplyAlpha(0.5f);
            }

            float border = node.ScaledBorderWidth;
            Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
            float corner = node.GetTextInputCornerRound(true);

            RenderFrame(output, node.GlobalRect, color, corner, border, borderColor);
        }

        if (pipeline.HasFlag(GuiPipeline.Main) || pipeline.HasFlag(GuiPipeline.Text))
        {
            var alignment = node.TextAlignment ?? GuiAlignment.Near;

            var font = ImGuiExternalBK.Instance.GetScaledFont(node);
            var fontColor = node.GetFontColor();

            var rect = node.GetGlobalTextInputRect(true);
            //float x = rect.X;
            //float y = rect.Y + rect.Height * 0.5f + font.Size * 0.5f;

            var hint = node.GetValue<GuiHintTextStyle>();

            string? text = node.Text;

            if (!string.IsNullOrEmpty(text))
            {
                if (disabled)
                {
                    fontColor = fontColor.MultiplyAlpha(0.5f);
                }

                if (hint?.Password == true)
                {
                    text = new string('*', text!.Length);
                }

                output.SetClipRect(rect);
                RenderText(output, node, text!, font, fontColor, rect, alignment);
                output.RestoreClip();
            }
            else
            {
                text = hint?.HintText ?? string.Empty;
                output.SetClipRect(rect);
                RenderText(output, node, text, font, fontColor.MultiplyAlpha(0.5f), rect, alignment);
                output.RestoreClip();
            }

            Image? img = node.Image;
            if (img is null)
            {
                return;
            }

            var imgRect = rect = node.GlobalRect;
            imgRect.Width = img.Width;
            imgRect.Height = img.Height;
            imgRect.X += (rect.Height - imgRect.Width) * 0.5f;
            imgRect.Y += (rect.Height - imgRect.Height) * 0.5f;

            var imgColor = node.ImageFilterColor;
            if (disabled && imgColor is { })
            {
                imgColor = imgColor.Value.MultiplyAlpha(0.5f);
            }

            output.DrawImageCached(img, imgRect, imgColor);
        }

        baseAction(pipeline);
    }

    private void RenderNumericInput(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        RenderStringInput(GuiPipeline.Background, node, output, dirtyMode, baseAction);

        bool outOfRange = false;

        var value = node.GetValue<GuiNumericValue>();
        if (value is { Min: { } min, Max: { } max } && max > min)
        {
            var v = value.DecimalValue;

            var rect = node.GetGlobalTextInputRect(true);
            var rLeft = rect.Left;
            var rRight = rect.Right;

            decimal span = max - min;
            bool gradient = value is { MinColor: { } minColor, MaxColor: { } maxColor }; // value.MinColor.HasValue && value.MaxColor.HasValue;
            var baseColor = value.Color ?? node.GetProgressColor(() => node.FontColor);

            SolidBrush brush;
            if (gradient)
            {
                float rate = (float)((v - min) / span);

                if (rate < 0) rate = 0;
                if (rate > 1) rate = 1;

                if (value.Color is { } color)
                {
                    color = ColorHelper.Lerp(minColor, color, maxColor, rate);
                }
                else
                {
                    color = ColorHelper.Lerp(minColor, maxColor, rate);
                }

                brush = new SolidBrush(color);
            }
            else
            {
                brush = new SolidBrush(baseColor);
            }

            float py, ph;

            if (!gradient && baseColor == node.FontColor)
            {
                // Conflicts with text, needs to be placed at the bottom
                float padding2 = node.GlobalScaleValue((node.Padding?.Bottom ?? 0) * 0.5f);
                ph = rect.Height * 0.2f;
                py = rect.Y + rect.Height - ph + padding2;
            }
            else
            {
                // As background
                py = rect.Y;
                ph = rect.Height;
            }

            if (min < 0 && max > 0)
            {
                float rZeroRate = (float)((-min) / span);
                float rZero = rect.X + rZeroRate * rect.Width;

                float rValueRate = (float)((-min + v) / span);
                float rValue = rect.X + rValueRate * rect.Width;

                if (v < 0)
                {
                    var pRect = new RectangleF
                    {
                        X = rValue,
                        Y = py,
                        Width = rZero - rValue,
                        Height = ph,
                    };

                    output.FillRectangle(brush, pRect);
                }
                else if (v > 0)
                {
                    var pRect = new RectangleF
                    {
                        X = rZero,
                        Y = py,
                        Width = rValue - rZero,
                        Height = ph,
                    };

                    output.FillRectangle(brush, pRect);
                }
            }
            else if (min >= 0 && max > 0)
            {
                float rZero = rect.X;

                float rValueRate = (float)((v - min) / span);
                float rValue = rect.X + rValueRate * rect.Width;

                var pRect = new RectangleF
                {
                    X = rZero,
                    Y = py,
                    Width = rValue - rZero,
                    Height = ph,
                };

                output.FillRectangle(brush, pRect);
            }
            else if (min < 0 && max <= 0)
            {
                float rZero = rect.Right;

                float rValueRate = (float)((-min + v) / span);
                float rValue = rect.X + rValueRate * rect.Width;

                var pRect = new RectangleF
                {
                    X = rValue,
                    Y = py,
                    Width = rZero - rValue,
                    Height = ph,
                };

                output.FillRectangle(brush, pRect);
            }

            outOfRange = v < min || v > max;
        }

        RenderStringInput(GuiPipeline.Text, node, output, dirtyMode, baseAction);

        if (outOfRange)
        {
            var rect = node.GetGlobalTextInputRect(true);
            var imgRect = new RectangleF(rect.X + rect.Width - rect.Height, rect.Y, rect.Height, rect.Height);
            output.DrawImageCached(node.Theme.Warning, imgRect, null);
        }
    }

    private void RenderTextAreaInput(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        bool disabled = node.IsDisabled;

        if (pipeline.HasFlag(GuiPipeline.Main) || pipeline.HasFlag(GuiPipeline.Background))
        {
            var color = node.GetTextInputBackgroundColor();

            if (disabled)
            {
                color = color.MultiplyAlpha(0.5f);
            }

            float border = node.ScaledBorderWidth;
            Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
            float corner = node.GetTextInputCornerRound(true);

            RenderFrame(output, node.GlobalRect, color, corner, border, borderColor);
        }

        if (pipeline.HasFlag(GuiPipeline.Main) || pipeline.HasFlag(GuiPipeline.Text))
        {
            var font = ImGuiExternalBK.Instance.GetScaledFont(node);
            var fontColor = node.GetFontColor();

            var rect = node.GetGlobalTextInputRect(true);
            //float x = rect.X;
            //float y = rect.Y + rect.Height * 0.5f + font.Size * 0.5f;

            var hint = node.GetValue<GuiHintTextStyle>();

            string? text = node.Text;

            if (!string.IsNullOrEmpty(text))
            {
                if (disabled)
                {
                    fontColor = fontColor.MultiplyAlpha(0.5f);
                }

                if (hint?.Password == true)
                {
                    text = new string('*', text!.Length);
                }

                output.SetClipRect(rect);
                output.DrawTextArea(text!, font, fontColor, rect);
                output.RestoreClip();
            }
            else
            {
                text = hint?.HintText ?? string.Empty;
                output.SetClipRect(rect);
                output.DrawTextArea(text, font, fontColor.MultiplyAlpha(0.5f), rect);
                output.RestoreClip();
            }
        }

        baseAction(pipeline);
    }

    private void RenderVirtualList(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        RenderFrame(pipeline, node, output, dirtyMode, (_, _) => { });

        var value = node.GetValue<VisualListData>();
        if (value is null)
        {
            return;
        }

        float? headerHeight = value.HeaderHeight is { } h ? node.GlobalScaleValue(h) : null;

        var contentRect = node.GlobalInnerRect;
        if (headerHeight.HasValue)
        {
            contentRect.Y += headerHeight.Value;
            contentRect.Height -= headerHeight.Value;
        }

        try
        {
            output.SetClipRect(contentRect);
            baseAction(pipeline);
        }
        finally
        {
            output.RestoreClip();
        }

        if (headerHeight.HasValue)
        {
            var headerRect = node.GlobalInnerRect;
            headerRect.Height = headerHeight.Value;

            output.SetClipRect(headerRect);
            try
            {
                var headerNode = node.GetChildNode(GuiVirtualListExtensions.HeaderId);
                if (headerNode is { })
                {
                    headerNode.Render(GuiPipeline.Header, output);
                }
            }
            finally
            {
                output.RestoreClip();
            }
        }

        var scroll = node.GetValue<GuiScrollableValue>();
        if (scroll is { })
        {
            // var theme = node.Theme;
            var color = node.ScrollBarColor ?? node.Theme.Colors.GetColor(ColorStyle.ScrollBar);
            RenderScrollBar(node.Gui.Output, node, scroll, color);
        }

        //baseAction(pipeline);
    }

    private void RenderHeaderBar(ImGuiNode node, IGraphicOutput output, bool? expand = null)
    {
        var font = ImGuiExternalBK.Instance.GetScaledFont(node);
        var fontColor = node.GetFontColor();

        var rect = node.GlobalRect;
        var headerStyle = node.HeaderStyle;

        float headerHeight = node.GlobalScaleValue(headerStyle?.Height ?? 0);
        if (headerHeight <= 0)
        {
            return;
        }

        float headerIndent = node.GlobalScaleValue(headerStyle?.Padding ?? 0);
        float spacing = node.GlobalScaleValue(headerStyle?.Spacing ?? 0);

        float tx = rect.X + headerIndent;
        float ty = rect.Y + (headerHeight + font.Size) * 0.5f;
        float iconOffset = 0;

        if (expand.HasValue)
        {
            float imgSize = font.Size;
            float yAdd = (headerHeight - imgSize) * 0.5f;

            RectangleF imgRect;

            if (headerStyle?.RightSide == true)
            {
                imgRect = new RectangleF(rect.X + rect.Width - headerIndent - imgSize, rect.Y + yAdd, imgSize, imgSize);
            }
            else
            {
                imgRect = new RectangleF(rect.X + headerIndent, rect.Y + yAdd, imgSize, imgSize);
                iconOffset += imgSize + spacing;
                tx += imgSize + spacing;
            }

            var theme = node.Theme;
            output.DrawImageCached(expand.Value ? theme.ExpandImage : theme.CollapseImage, imgRect, node.ImageFilterColor);
        }

        var img = node.Image;
        if (img is { })
        {
            float imgSize = font.Size;
            float yAdd = (headerHeight - imgSize) * 0.5f;

            var imgRect = new RectangleF(rect.X + headerIndent + iconOffset, rect.Y + yAdd, imgSize, imgSize);
            output.DrawImageCached(img, imgRect, node.ImageFilterColor);

            tx += imgSize + spacing;
        }

        string? text = node.Text;
        if (!string.IsNullOrEmpty(text))
        {
            output.DrawString(
                text,
                font,
                new SolidBrush(fontColor),
                new PointF(tx, ty),
                new StringFormat { Alignment = StringAlignment.Near });
        }
    }

    private void RenderFrame(IGraphicOutput output, RectangleF rect, Color? color, float cornerRound, float borderWidth, Color borderColor)
    {
        if (borderWidth > 0)
        {
            rect = rect.OffsetHalf(-borderWidth);
        }

        if (color is { A: > 0 })
        {
            if (cornerRound > 0)
            {
                output.FillRoundRectangle(new SolidBrush(color.Value), rect, cornerRound);
            }
            else
            {
                output.FillRectangle(new SolidBrush(color.Value), rect);
            }
        }

        if (borderWidth > 0)
        {
            if (cornerRound > 0)
            {
                output.DrawRoundRectangle(new Pen(borderColor, borderWidth), rect, cornerRound);
            }
            else
            {
                output.DrawRectangle(new Pen(borderColor, borderWidth), rect);
            }
        }
    }

    private void RenderText(IGraphicOutput output, string text, Font font, Color color, float x, float y)
    {
        output.DrawString(
            text,
            font,
            new SolidBrush(color),
            new PointF(x, y),
            new StringFormat { Alignment = StringAlignment.Near });
    }

    private void RenderText(IGraphicOutput output, ImGuiNode node, string text, Font font, Color color, RectangleF rect, GuiAlignment alignment)
    {
        float x = 0, y = 0;
        StringAlignment a = StringAlignment.Near;

        float posFix = node.GlobalScaleValue(ImGuiFitSystemBK.TextPosFit_V);

        switch (alignment)
        {
            case GuiAlignment.Near:
                x = rect.X;
                y = rect.Y + (rect.Height + font.Size - posFix) * 0.5f;
                a = StringAlignment.Near;
                break;

            case GuiAlignment.Center:
                x = rect.X + rect.Width * 0.5f;
                y = rect.Y + (rect.Height + font.Size - posFix) * 0.5f;
                a = StringAlignment.Center;
                break;

            case GuiAlignment.Far:
                x = rect.Right;
                y = rect.Y + (rect.Height + font.Size - posFix) * 0.5f;
                a = StringAlignment.Far;
                break;
        }

        //var brush = new SolidBrush(Color.Cyan);
        //output.FillRectangle(brush, rect);

        output.DrawString(
            text,
            font,
            new SolidBrush(color),
            new PointF(x, y),
            new StringFormat { Alignment = a });
    }

    private void RenderScrollBar(IGraphicOutput output, ImGuiNode node, GuiScrollableValue value, Color color)
    {
        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetFrameCornerRound(true);

        if (value.ScrollOrientation.IsVertical() && value.VScrollBarVisible)
        {
            var scrollRect = node.GetVerticalScrollBarRect(value);
            var globalScrollRect = node.GlobalScaleRect(scrollRect);
            RenderFrame(output, globalScrollRect, color, corner, border, borderColor);
        }

        if (value.ScrollOrientation.IsHorizontal() && value.HScrollBarVisible)
        {
            var scrollRect = node.GetHorizontalScrollBarRect(value);
            var globalScrollRect = node.GlobalScaleRect(scrollRect);
            RenderFrame(output, globalScrollRect, color, corner, border, borderColor);
        }
    }

    private void RenderButton(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, ChildRenderFunction baseAction, bool dropDown, bool? toggle)
    {
        var font = ImGuiExternalBK.Instance.GetScaledFont(node);
        var fontColor = node.GetFontColor();

        Color color = node.GetButtonColor();
        Image? image = node.Image;
        Color? imgColor = node.ImageFilterColor;

        var rect = node.GlobalRect;
        var innerRect = node.GlobalInnerRect;
        string? text = node.Text;

        bool disabled = node.IsDisabled;

        // Non-interactive style
        if (!node.HasPseudoStyle)
        {
            if (disabled)
            {
                color = color.MultiplyAlpha(0.5f);
                fontColor = fontColor.MultiplyAlpha(0.5f);
                if (imgColor is { })
                {
                    imgColor = imgColor.Value.MultiplyAlpha(0.5f);
                }
            }
            else
            {
                if (node.IsMouseInClickRect)
                {
                    if (node.MouseState == GuiMouseState.Pressed)
                    {
                        color = color.Multiply(0.8f);
                        if (imgColor is { })
                        {
                            imgColor = imgColor.Value.Multiply(0.8f);
                        }
                    }
                    else
                    {
                        color = color.MultiplyRevert(0.8f);
                        if (imgColor is { })
                        {
                            imgColor = imgColor.Value.MultiplyRevert(0.8f);
                        }
                    }
                }
            }

            if (toggle.HasValue && toggle.Value == true)
            {
                color = color.Multiply(0.5f);
            }
        }

        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float corner = node.GetButtonCornerRound(true);

        RenderFrame(output, rect, color, corner, border, borderColor);

        if (dropDown)
        {
            var innerRect2 = innerRect;
            innerRect2.Width -= innerRect.Height;

            output.SetClipRect(innerRect2);
        }
        else
        {
            output.SetClipRect(innerRect);
        }

        if (!string.IsNullOrEmpty(text)) // With text
        {
            float posFix = node.GlobalScaleValue(ImGuiFitSystemBK.TextPosFit_V);

            // Text aligned left for dropDown, centered for button
            float x = dropDown ? innerRect.X : innerRect.X + innerRect.Width * 0.5f;
            float y = innerRect.Y + (innerRect.Height + font.Size - posFix) * 0.5f;

            if (image is { })
            {
                float paddingTop = node.GlobalScaleValue(node.GetButtonTextPaddingTop());
                float paddingLeft = node.GlobalScaleValue(node.GetButtonTextPaddingLeft());
                float paddingRight = node.GlobalScaleValue(node.GetButtonTextPaddingRight());

                float imgSize = innerRect.Height - paddingLeft - paddingRight;

                var imgRect = new RectangleF(innerRect.X + paddingLeft, innerRect.Y + paddingTop, imgSize, imgSize);
                output.DrawImageCached(image, imgRect, imgColor);

                x += imgSize * 0.5f + node.GlobalScaleValue(node.ChildSpacing ?? 0);
            }

            output.DrawString(
                text,
                font,
                new SolidBrush(fontColor),
                new PointF(x, y),
                new StringFormat { Alignment = dropDown ? StringAlignment.Near : StringAlignment.Center });
        }
        else if (image is { }) // Image only
        {
            float indent = node.GlobalScaleValue(node.GetButtonTextPaddingLeft());
            float imgSize = innerRect.Height - indent * 2;

            float x = innerRect.X + (innerRect.Width - imgSize) * 0.5f;
            float y = innerRect.Y + (innerRect.Height - imgSize) * 0.5f;

            var imgRect = new RectangleF(x, y, imgSize, imgSize);
            output.DrawImageCached(image, imgRect, imgColor);
        }

        output.RestoreClip();

        if (dropDown)
        {
            var imgRect = innerRect;
            imgRect.X = innerRect.Right - innerRect.Height;
            imgRect.Width = innerRect.Height;

            output.DrawImageCached(image ?? node.Theme.DropDownImage, imgRect, imgColor);
        }

        baseAction(pipeline);
    }
}