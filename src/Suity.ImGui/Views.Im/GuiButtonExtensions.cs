using static Suity.Helpers.GlobalLocalizer;
using Suity.Views.Graphics;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating button controls in ImGui.
/// </summary>
public static class GuiButtonExtensions
{
    /// <summary>
    /// Creates a button with auto-generated ID.
    /// </summary>
    public static ImGuiNode Button(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => Button(gui, $"##button#{member}#{line}");

    /// <summary>
    /// Creates a button with text and auto-generated ID.
    /// </summary>
    public static ImGuiNode Button(this ImGui gui, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => gui.Button($"##button#{member}#{line}", text);

    /// <summary>
    /// Creates a button with an image and auto-generated ID.
    /// </summary>
    public static ImGuiNode Button(this ImGui gui, Image? initImage, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => Button(gui, $"##button#{member}#{line}", null, initImage);

    /// <summary>
    /// Creates a button with text and image with auto-generated ID.
    /// </summary>
    public static ImGuiNode Button(this ImGui gui, string text, Image? initImage, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => Button(gui, $"##button#{member}#{line}", text, initImage);

    /// <summary>
    /// Creates a button with an image.
    /// </summary>
    public static ImGuiNode Button(this ImGui gui, string id, Image? initImage)
        => gui.Button(id, null, initImage);

    /// <summary>
    /// Creates a button with the specified ID, text, and optional image.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the button.</param>
    /// <param name="text">The text to display on the button.</param>
    /// <param name="initImage">The optional image to display on the button.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the button.</returns>
    public static ImGuiNode Button(this ImGui gui, string id, string? text, Image? initImage = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Button);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            if (initImage != null)
            {
                var imgValue = node.GetOrCreateValue<GuiImageValue>();
                imgValue.Image = initImage;
            }
            else
            {
                node.RemoveValue<GuiImageValue>();
            }
        }
        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a horizontal button with auto-generated ID.
    /// </summary>
    public static ImGuiNode HorizontalButton(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => HorizontalButton(gui, $"##h_button#{member}#{line}");

    /// <summary>
    /// Creates a button with horizontal layout.
    /// </summary>
    public static ImGuiNode HorizontalButton(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Button);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.FitOrientation = GuiOrientation.Both;
            node.InitHorizontalLayout(true);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a vertical button with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for generating a unique ID.</param>
    /// <param name="member">The caller member name, used for generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the vertical button.</returns>
    public static ImGuiNode VerticalButton(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => VerticalButton(gui, $"##v_button#{member}#{line}");

    /// <summary>
    /// Creates a button with vertical layout.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the button.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the vertical button.</returns>
    public static ImGuiNode VerticalButton(this ImGui gui, string id)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(Button);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.FitOrientation = GuiOrientation.Both;
            node.InitVerticalLayout(true);
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates an expand/collapse button with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="line">The caller line number, used for generating a unique ID.</param>
    /// <param name="member">The caller member name, used for generating a unique ID.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the expand button.</returns>
    public static ImGuiNode ExpandButton(this ImGui gui, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => ExpandButton(gui, $"##e_button#{member}#{line}", null, null, false);

    /// <summary>
    /// Creates an expand/collapse button with the specified ID.
    /// </summary>
    public static ImGuiNode ExpandButton(this ImGui gui, string id, string? text = null, Image? initImage = null, bool initExpand = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            var expandValue = new GuiExpandableValue { Expanded = initExpand };
            node.TypeName = nameof(ExpandButton);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            node.Image = expandValue.Expanded ? node.Theme.ExpandImage : node.Theme.CollapseImage;
            node.SetValue(expandValue);
        }
        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates an expand/collapse button with a shared expandable value.
    /// </summary>
    public static ImGuiNode ExpandButton(this ImGui gui, string id, string? text = null, Image? initImage = null, GuiExpandableValue? expandValue = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            expandValue ??= new();
            node.TypeName = nameof(ExpandButton);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            node.Image = expandValue.Expanded ? node.Theme.ExpandImage : node.Theme.CollapseImage;
            node.SetValue(expandValue);
        }
        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a switch button that toggles active state among siblings.
    /// </summary>
    public static ImGuiNode SwitchButton(this ImGui gui, string id, string? text, Image? initImage = null, GuiOptionalValue? value = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(SwitchButton);
            node.SetInputFunction(nameof(SwitchButton));
            node.SetRenderFunction(nameof(Button));
            node.SetFitFunction(nameof(Button));
            node.FitOrientation = GuiOrientation.Both;
            if (value is { })
            {
                node.Parent?.SetValue(value);
            }
            else
            {
                node.Parent?.GetOrCreateValue<GuiOptionalValue>();
            }
            if (initImage != null)
            {
                var imgValue = node.GetOrCreateValue<GuiImageValue>();
                imgValue.Image = initImage;
            }
            else
            {
                node.RemoveValue<GuiImageValue>();
            }
        }
        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a switch button with auto-generated ID.
    /// </summary>
    public static ImGuiNode SwitchButton(this ImGui gui, GuiOptionalValue? value = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => SwitchButton(gui, $"##s_button#{member}#{line}", value);

    /// <summary>
    /// Creates a switch button without text.
    /// </summary>
    public static ImGuiNode SwitchButton(this ImGui gui, string id, GuiOptionalValue? value = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(SwitchButton);
            node.SetInputFunction(nameof(SwitchButton));
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            node.SetRenderFunction(nameof(Button));
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Both;
            if (value is { })
            {
                node.Parent?.SetValue(value);
            }
            else
            {
                node.Parent?.GetOrCreateValue<GuiOptionalValue>();
            }
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a dropdown button with the specified text and image.
    /// </summary>
    public static ImGuiNode DropDownButton(this ImGui gui, string? text, Image? initImage, GuiDropDownValue? initValue = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => DropDownButton(gui, $"##d_button#{member}#{line}", text, initImage, initValue);

    /// <summary>
    /// Creates a dropdown button with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the button.</param>
    /// <param name="text">The text to display on the button.</param>
    /// <param name="initImage">The optional image to display on the button.</param>
    /// <param name="initValue">The optional initial dropdown value.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the dropdown button.</returns>
    public static ImGuiNode DropDownButton(this ImGui gui, string id, string? text, Image? initImage = null, GuiDropDownValue? initValue = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(DropDownButton);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            if (initValue is { })
            {
                node.SetValue(initValue);
            }
            else
            {
                node.GetOrCreateValue<GuiDropDownValue>();
            }
            if (initImage != null)
            {
                var imgValue = node.GetOrCreateValue<GuiImageValue>();
                imgValue.Image = initImage;
            }
            else
            {
                node.RemoveValue<GuiImageValue>();
            }
        }
        text ??= string.Empty;
        if (node.Text != text)
        {
            node.Text = text;
            node.Fit();
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a dropdown button with auto-generated ID.
    /// </summary>
    public static ImGuiNode DropDownButton(this ImGui gui, GuiDropDownValue? value = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => DropDownButton(gui, $"##s_button#{member}#{line}", value);

    /// <summary>
    /// Creates a dropdown button without text or image.
    /// </summary>
    public static ImGuiNode DropDownButton(this ImGui gui, string id, GuiDropDownValue? initValue = null)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(DropDownButton);
            node.SetInputFunction(nameof(DropDownButton));
            node.SetRenderFunction(nameof(DropDownButton));
            node.SetFitFunction(nameof(Button));
            node.FitOrientation = GuiOrientation.Both;
            if (initValue is { })
            {
                node.SetValue(initValue);
                if (initValue.SelectedItem is { })
                {
                    node.Text = initValue.SelectedItem.ToString() ?? string.Empty;
                }
            }
            else
            {
                node.GetOrCreateValue<GuiDropDownValue>();
            }
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the dropdown value on a node.
    /// </summary>
    public static ImGuiNode SetDropDownValue(this ImGuiNode node, GuiDropDownValue value)
    {
        node.SetValue(value);
        node.Text = value.SelectedItem?.ToString() ?? string.Empty;
        return node;
    }

    /// <summary>
    /// Initializes dropdown items only during node initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="objs">The collection of objects to add as dropdown items.</param>
    /// <param name="selected">The optional initially selected item.</param>
    /// <param name="dropDownHeight">The optional maximum height for the dropdown list.</param>
    /// <returns>The node for method chaining.</returns>
    public static ImGuiNode InitDropDownItems(this ImGuiNode node, IEnumerable<object> objs, object? selected = null, int? dropDownHeight = null)
    {
        if (node.IsInitializing)
        {
            var value = node.GetOrCreateValue<GuiDropDownValue>();
            value.Clear();
            if (objs != null)
            {
                value.AddValues(objs);
            }
            if (selected != null)
            {
                value.SelectedItem = new GuiDropDownItem(selected);
            }
            else
            {
                value.SelectedItem = null;
            }
            value.DropDownHeight = dropDownHeight;
            if (selected is { })
            {
                node.Text = selected.ToString() ?? string.Empty;
            }
        }
        return node;
    }

    /// <summary>
    /// Gets the currently selected dropdown item.
    /// </summary>
    public static object? GetDropDownSelectedItem(this ImGuiNode node)
    {
        return node.GetValue<GuiDropDownValue>()?.SelectedItem;
    }

    /// <summary>
    /// Gets the button color for a node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The button color, falling back to the theme's default button color.</returns>
    public static Color GetButtonColor(this ImGuiNode node)
    {
        return node.Color ?? node.Color ?? node.Theme.Colors.GetColor(ColorStyle.Button);
    }

    /// <summary>
    /// Gets the button corner roundness, optionally scaled.
    /// </summary>
    public static float GetButtonCornerRound(this ImGuiNode node, bool scaled)
    {
        float v = node.CornerRound ?? node.Theme.ButtonCornerRound;
        if (scaled)
        {
            v = node.GlobalScaleValue(v);
        }
        return v;
    }

    /// <summary>
    /// Gets the top padding for button text.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The top padding value, falling back to the theme's default.</returns>
    public static float GetButtonTextPaddingTop(this ImGuiNode node)
        => node.Padding?.Top ?? node.Theme.ButtonTextPadding;

    /// <summary>
    /// Gets the bottom padding for button text.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The bottom padding value, falling back to the theme's default.</returns>
    public static float GetButtonTextPaddingBottom(this ImGuiNode node)
        => node.Padding?.Bottom ?? node.Theme.ButtonTextPadding;

    /// <summary>
    /// Gets the left padding for button text.
    /// </summary>
    public static float GetButtonTextPaddingLeft(this ImGuiNode node)
        => node.Padding?.Left ?? node.Theme.ButtonTextPadding;

    /// <summary>
    /// Gets the right padding for button text.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The right padding value, falling back to the theme's default.</returns>
    public static float GetButtonTextPaddingRight(this ImGuiNode node)
        => node.Padding?.Right ?? node.Theme.ButtonTextPadding;
}
