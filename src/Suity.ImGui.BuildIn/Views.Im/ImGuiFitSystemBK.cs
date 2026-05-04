using Suity.Collections;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Default fit system for ImGui, providing auto-sizing functions for various control types.
/// </summary>
public class ImGuiFitSystemBK : ImGuiFitSystem
{
    /// <summary>
    /// Vertical position adjustment for text rendering to properly show underlines.
    /// </summary>
    internal const float TextPosFit_V = 3;

    /// <summary>
    /// Horizontal position adjustment for text rendering.
    /// </summary>
    internal const float TextPosFit_H = 0;

    private static ImGuiFitSystemBK? instance;

    /// <summary>
    /// Gets the singleton instance of the fit system.
    /// </summary>
    public static ImGuiFitSystemBK Instance => instance ??= new ImGuiFitSystemBK();

    private readonly Dictionary<string, FitFunction> _functions = [];

    /// <summary>
    /// Initializes a new fit system and registers all built-in fit functions.
    /// </summary>
    public ImGuiFitSystemBK()
    {
        _functions[nameof(GuiButtonExtensions.DropDownButton)] = DropdownButtonFit;
        _functions[nameof(GuiButtonExtensions.Button)] = ButtonFit;
        _functions[nameof(GuiButtonExtensions.ExpandButton)] = ButtonFit;
        _functions[nameof(GuiToggleExtensions.ToggleButton)] = ButtonFit;
        _functions[nameof(GuiToggleExtensions.CheckBox)] = CheckBoxFit;
        _functions[Auto] = AutoFit;
        _functions[Expandable] = ExpandableFit;
        _functions[Overlay] = OverlayFit;
        _functions[Scrollable] = ScrollableFit;
        _functions[nameof(GuiTextExtensions.Text)] = TextFit;
        _functions[nameof(GuiTextExtensions.TextArea)] = TextAreaFit;
        _functions[nameof(GuiTextInputExtensions.StringInput)] = StringInputFit;
        _functions[nameof(GuiTextInputExtensions.NumericInput)] = StringInputFit;
        _functions[nameof(GuiTextInputExtensions.TextAreaInput)] = TextAreaInputFit;
        _functions[nameof(GuiVirtualListExtensions.VirtualList)] = VirtualListFit;
        _functions[nameof(GuiCommonExtensions.Image)] = ImageFit;
    }

    /// <inheritdoc/>
    public override FitFunction? GetFitFunction(string name)
    {
        var func = _functions.GetValueSafe(name);

        if (func is null)
        {
            Debug.WriteLine($"{nameof(ImGuiFitSystemBK)} function not found : {name}");
        }

        return func;
    }

    private void DropdownButtonFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        ButtonFit(pipeline, node, baseAction, true);
    }

    private void ButtonFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        ButtonFit(pipeline, node, baseAction, false);
    }

    private void ButtonFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction, bool dropDown)
    {
        var direction = node.FitOrientation;
        string text = node.Text ?? node.Id;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        SizeF size = node.Gui.Output.MeasureString(text, font);

        if (direction.IsHorizontal())
        {
            float paddingTop = node.GetButtonTextPaddingTop();
            float paddingBottom = node.GetButtonTextPaddingBottom();
            float paddingLeft = node.GetButtonTextPaddingLeft();
            float paddingRight = node.GetButtonTextPaddingRight();

            float width = size.Width + (paddingLeft + paddingRight + TextPosFit_H);
            if (node.Image != null)
            {
                var innerRect = node.InnerRect;

                float imgSize = innerRect.Height - (paddingTop + paddingBottom);

                width += imgSize;

                if (!string.IsNullOrEmpty(text))
                {
                    width += node.ChildSpacing ?? 0;
                }
            }
            node.Width = width;
        }

        if (direction.IsVertical())
        {
            float paddingTop = node.GetButtonTextPaddingTop();
            float paddingBottom = node.GetButtonTextPaddingBottom();

            node.Height = size.Height + (paddingTop + paddingBottom + TextPosFit_V);
        }

        node.Layout();

        baseAction(pipeline);

        switch (node.MouseState)
        {
            case GuiMouseState.Pressed:
            case GuiMouseState.Clicked:
                node.MarkRenderDirty();
                break;
        }
    }

    private void CheckBoxFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var direction = node.FitOrientation;

        string? text = node.Text;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        SizeF? size = !string.IsNullOrEmpty(text) ? node.Gui.Output.MeasureString(text, font) : null;

        if (direction.IsHorizontal())
        {
            float width = (node.Theme.CheckBoxWidth);
            if (size is { } sizeV)
            {
                width += sizeV.Width + (node.ChildSpacing ?? 0) + TextPosFit_H;
            }

            node.Width = width;
        }

        if (direction.IsVertical())
        {
            float height = (node.Theme.CheckBoxWidth);
            if (size is { })
            {
                height = Math.Max(height, size.Value.Height);
                node.Height = height + TextPosFit_V;
            }
        }

        node.Layout();

        baseAction(pipeline);

        switch (node.MouseState)
        {
            case GuiMouseState.Pressed:
            case GuiMouseState.Clicked:
                node.MarkRenderDirty();
                break;
        }
    }

    private void AutoFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var o = node.FitOrientation;
        if (o == GuiOrientation.None)
        {
            node.Layout();
            return;
        }

        var pos = node.LayoutPosition;
        float width = pos.X;
        float height = pos.Y;

        if (o.IsHorizontal())
        {
            if (width < 0)
            {
                width = 0;
            }
            float padding = node.GetPadding(GuiSides.Left) + node.GetPadding(GuiSides.Right) + (node.HeaderWidth ?? 0);
            node.Width = width + padding;
        }

        if (o.IsVertical())
        {
            if (height < 0)
            {
                height = 0;
            }
            float padding = node.GetPadding(GuiSides.Top) + node.GetPadding(GuiSides.Bottom) + (node.HeaderHeight ?? 0);
            node.Height = height + padding;
        }

        node.Layout();
    }

    private void ExpandableFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        node.MouseClickRect = node.GetHeaderRect();

        GuiExpandableValue? value = node.GetValue<GuiExpandableValue>();
        if (value is null)
        {
            return;
        }

        if (value.Expanded)
        {
            // Can be empty on first time
            if (value.ExpandedHeight is { })
            {
                node.BaseHeight = value.ExpandedHeight;
            }

            AutoFit(pipeline, node, baseAction);
        }
        else
        {
            value.ExpandedHeight ??= node.BaseHeight;

            node.BaseHeight = node.HeaderHeight;

            node.ClearContents();
        }

        node.Layout();
    }

    private void OverlayFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var direction = node.FitOrientation;
        if (direction == GuiOrientation.None)
        {
            return;
        }

        var rect = node.InnerRect;
        var size = new SizeF(rect.Width, rect.Height);

        if (direction.IsHorizontal() && node.ChildNodes.Any())
        {
            size.Width = node.ChildNodes.Select(o => o.Rect.Right).Max() - rect.X;
            node.Width = size.Width + (node.GetPadding(GuiSides.Left) + node.GetPadding(GuiSides.Right));
        }
        if (direction.IsVertical() && node.ChildNodes.Any())
        {
            size.Height = node.ChildNodes.Select(o => o.Rect.Bottom).Max() - rect.Y;
            node.Height = size.Height + (node.GetPadding(GuiSides.Top) + node.GetPadding(GuiSides.Bottom));
        }

        node.Layout();
    }

    private void ScrollableFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var value = node.GetValue<GuiScrollableValue>();
        if (value is null)
        {
            return;
        }

        var dir = value.ScrollOrientation;

        var rect = node.InnerRect;
        var cSize = new SizeF(rect.Width, rect.Height);

        if (dir.IsVertical())
        {
            cSize.Height = node.CalculateContentHeight();
        }
        if (dir.IsHorizontal())
        {
            cSize.Width = node.CalculateContentWidth();
        }

        value.ContentSize = cSize;

        // Debug.WriteLine($"Scrollable fit : {node.ChildNodeCount}-{cSize.Height}");

        node.FitScrollBarPosition(value);
    }

    private void TextFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var direction = node.FitOrientation;
        if (direction == GuiOrientation.None)
        {
            return;
        }

        var gui = node.Gui;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        string text = node.Text ?? node.Id;
        SizeF size = gui.Output.MeasureString(text, font);

        if (direction.IsHorizontal())
        {
            node.Width = Mathf.Ceil(size.Width + TextPosFit_H);
        }
        if (direction.IsVertical())
        {
            node.Height = Mathf.Ceil(size.Height + TextPosFit_V);
        }

        node.Layout();
    }

    private void TextAreaFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        // Needs two Layout passes
        //node.Layout();

        var gui = node.Gui;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        string text = node.Text ?? node.Id;
        var size = gui.Output.MeasureTextArea(text, font, node.Rect.Width);

        // var w = node.Width;

        //node.Width = Mathf.Ceil(size.Width);
        node.Height = Mathf.Ceil(size.Height);

        node.Layout();
    }

    private void StringInputFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var direction = node.FitOrientation;
        if (direction == GuiOrientation.None)
        {
            return;
        }

        var gui = node.Gui;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        string text = node.Text ?? node.Id;
        SizeF size = gui.Output.MeasureString(text, font);

        if (direction.IsHorizontal())
        {
            float width = (Mathf.Ceil(size.Width) + (node.GetTextInputPaddingLeft() + node.GetTextInputPaddingRight()));
            if (node.Image != null)
            {
                width += size.Height + TextPosFit_H;
            }
            node.Width = width;
        }

        if (direction.IsVertical())
        {
            float height = (Mathf.Ceil(size.Height) + (node.GetTextInputPaddingTop() + node.GetTextInputPaddingBottom()));
            node.Height = height + TextPosFit_V;
        }

        node.Layout();
    }

    private void TextAreaInputFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var direction = node.FitOrientation;
        if (direction == GuiOrientation.None)
        {
            return;
        }

        var gui = node.Gui;
        var font = ImGuiExternalBK.Instance.GetFont(node);
        string text = node.Text ?? node.Id;
        var size = gui.Output.MeasureTextArea(text, font, float.MaxValue);

        if (direction == GuiOrientation.Horizontal || direction == GuiOrientation.Both)
        {
            float width = (Mathf.Ceil(size.Width) + (node.GetTextInputPaddingLeft() + node.GetTextInputPaddingRight()));
            if (node.Image != null)
            {
                width += size.Height;
            }
            node.Width = width + TextPosFit_H;
        }

        if (direction == GuiOrientation.Vertical || direction == GuiOrientation.Both)
        {
            float height = (Mathf.Ceil(size.Height) + (node.GetTextInputPaddingTop() + node.GetTextInputPaddingBottom()));
            node.Height = height + TextPosFit_V;
        }

        node.Layout();
    }

    private void ImageFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var image = node.Image;

        // Due to the image is only a data representation, it may not be loaded when fitting, so we need to check if it's loaded before measuring it.
        var size = node.Gui.Output.MeasureImage(image);

        if (image is { })
        {
            node.SetSize(size.Width, size.Height);
            node.Layout();
        }
    }

    private static void VirtualListFit(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction)
    {
        var data = node.GetValue<VisualListData>();
        if (data is null)
        {
            return;
        }

        var scroll = node.GetValue<GuiScrollableValue>();
        if (scroll is null)
        {
            return;
        }

        var rect = node.InnerRect;
        float height = data.TotalHeight;
        float width = data.Width ?? rect.Width;

        //float height = node.ScaleValue(data.TotalHeight);
        //float width = data.Width is { } w ? node.ScaleValue(w) : rect.Width;

        var cSize = new SizeF(width, height);

        scroll.ContentSize = cSize;

        node.FitScrollBarPosition(scroll);
    }
}