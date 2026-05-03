using Suity.Drawing;
using Suity.Views.Graphics;
using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating toggle and checkbox controls in ImGui.
/// </summary>
public static class GuiToggleExtensions
{
    /// <summary>
    /// Creates a simple checkbox without text.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the checkbox on.</param>
    /// <param name="id">The unique identifier for the checkbox node.</param>
    /// <param name="value">The optional bound value. If null, the checkbox maintains its own state.</param>
    /// <param name="initValue">The initial checked state when the checkbox is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the checkbox.</returns>
    public static ImGuiNode CheckBox(this ImGui gui, string id, bool? value = null, bool initValue = false)
    {
        CheckState? state = null;
        if (value.HasValue)
        {
            state = value.Value ? CheckState.Checked : CheckState.Unchecked;
        }
        CheckState initState = initValue ? CheckState.Checked : CheckState.Unchecked;
        return gui.CheckBoxAdvanced(id, string.Empty, state, initState);
    }

    /// <summary>
    /// Creates a checkbox with optional text.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the checkbox on.</param>
    /// <param name="id">The unique identifier for the checkbox node.</param>
    /// <param name="initText">The optional display text next to the checkbox.</param>
    /// <param name="value">The optional bound value. If null, the checkbox maintains its own state.</param>
    /// <param name="initValue">The initial checked state when the checkbox is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the checkbox.</returns>
    public static ImGuiNode CheckBox(this ImGui gui, string id, string? initText, bool? value = null, bool initValue = false)
    {
        CheckState? state = null;
        if (value.HasValue)
        {
            state = value.Value ? CheckState.Checked : CheckState.Unchecked;
        }
        CheckState initState = initValue ? CheckState.Checked : CheckState.Unchecked;
        return gui.CheckBoxAdvanced(id, initText, state, initState);
    }

    /// <summary>
    /// Creates an advanced checkbox with three-state support.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the checkbox on.</param>
    /// <param name="id">The unique identifier for the checkbox node.</param>
    /// <param name="value">The optional bound <see cref="CheckState"/> value. Supports Checked, Unchecked, and Indeterminate states.</param>
    /// <param name="initValue">The initial <see cref="CheckState"/> when the checkbox is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the advanced checkbox.</returns>
    public static ImGuiNode CheckBoxAdvanced(this ImGui gui, string id, CheckState? value = null, CheckState initValue = CheckState.Unchecked)
    {
        return gui.CheckBoxAdvanced(id, string.Empty, value, initValue);
    }

    /// <summary>
    /// Creates an advanced checkbox with three-state support and optional text.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the checkbox on.</param>
    /// <param name="id">The unique identifier for the checkbox node.</param>
    /// <param name="initText">The optional display text next to the checkbox.</param>
    /// <param name="value">The optional bound <see cref="CheckState"/> value. Supports Checked, Unchecked, and Indeterminate states.</param>
    /// <param name="initValue">The initial <see cref="CheckState"/> when the checkbox is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the advanced checkbox.</returns>
    public static ImGuiNode CheckBoxAdvanced(this ImGui gui, string id, string? initText, CheckState? value = null, CheckState initValue = CheckState.Unchecked)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(CheckBox);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Both;
            initText ??= string.Empty;
            if (node.Text != initText)
            {
                node.Text = initText;
                node.Fit();
            }
            node.GetOrCreateValue<GuiToggleValue>().Value = value ?? initValue;
        }
        if (value.HasValue && !node.IsEdited)
        {
            var v = node.GetOrCreateValue<GuiToggleValue>();
            if (v.Value != value.Value)
            {
                v.Value = value.Value;
                node.MarkRenderDirty();
            }
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Creates a toggle button with an image.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the toggle button on.</param>
    /// <param name="id">The unique identifier for the toggle button node.</param>
    /// <param name="initImage">The image displayed on the toggle button.</param>
    /// <param name="value">The optional bound value. If null, the toggle button maintains its own state.</param>
    /// <param name="initValue">The initial checked state when the toggle button is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the toggle button.</returns>
    public static ImGuiNode ToggleButton(this ImGui gui, string id, ImageDef? initImage, bool? value = null, bool initValue = false)
    {
        CheckState? state = null;
        if (value.HasValue)
        {
            state = value.Value ? CheckState.Checked : CheckState.Unchecked;
        }
        CheckState initState = initValue ? CheckState.Checked : CheckState.Unchecked;
        return gui.ToggleButton(id, string.Empty, initImage, state, initState);
    }

    /// <summary>
    /// Creates a toggle button with text and image.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the toggle button on.</param>
    /// <param name="id">The unique identifier for the toggle button node.</param>
    /// <param name="initText">The optional display text next to the toggle button.</param>
    /// <param name="initImage">The image displayed on the toggle button.</param>
    /// <param name="value">The optional bound value. If null, the toggle button maintains its own state.</param>
    /// <param name="initValue">The initial checked state when the toggle button is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the toggle button.</returns>
    public static ImGuiNode ToggleButton(this ImGui gui, string id, string? initText, ImageDef? initImage, bool? value = null, bool initValue = false)
    {
        CheckState? state = null;
        if (value.HasValue)
        {
            state = value.Value ? CheckState.Checked : CheckState.Unchecked;
        }
        CheckState initState = initValue ? CheckState.Checked : CheckState.Unchecked;
        return gui.ToggleButton(id, initText, initImage, state, initState);
    }

    /// <summary>
    /// Creates a toggle button with three-state support.
    /// </summary>
    /// <param name="gui">The ImGui instance to create the toggle button on.</param>
    /// <param name="id">The unique identifier for the toggle button node.</param>
    /// <param name="initText">The optional display text next to the toggle button.</param>
    /// <param name="initImage">The image displayed on the toggle button.</param>
    /// <param name="value">The optional bound <see cref="CheckState"/> value. Supports Checked, Unchecked, and Indeterminate states.</param>
    /// <param name="initValue">The initial <see cref="CheckState"/> when the toggle button is first created.</param>
    /// <returns>The created <see cref="ImGuiNode"/> representing the toggle button.</returns>
    public static ImGuiNode ToggleButton(this ImGui gui, string id, string? initText, ImageDef? initImage, CheckState? value = null, CheckState initValue = CheckState.Unchecked)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = nameof(ToggleButton);
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
            initText ??= string.Empty;
            if (node.Text != initText)
            {
                node.Text = initText;
                node.Fit();
            }
            var v = node.GetOrCreateValue<GuiToggleValue>();
            v.Value = value ?? initValue;
            node.Pseudo = v.Value == CheckState.Checked ? ImGuiNode.PseudoActive : null;
        }
        if (value.HasValue && !node.IsEdited)
        {
            var v = node.GetOrCreateValue<GuiToggleValue>();
            if (v.Value != value.Value)
            {
                v.Value = value.Value;
                node.Pseudo = v.Value == CheckState.Checked ? ImGuiNode.PseudoActive : null;
                node.MarkRenderDirty();
            }
        }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Gets the check state of a toggle node.
    /// </summary>
    /// <param name="node">The toggle node to query.</param>
    /// <returns>The current <see cref="CheckState"/> of the node, or null if no toggle value is set.</returns>
    public static CheckState? GetCheckState(this ImGuiNode node)
    {
        return node.GetValue<GuiToggleValue>()?.Value;
    }

    /// <summary>
    /// Gets whether a toggle node is checked.
    /// </summary>
    /// <param name="node">The toggle node to query.</param>
    /// <returns>True if the node's state is <see cref="CheckState.Checked"/>; otherwise, false.</returns>
    public static bool GetIsChecked(this ImGuiNode node)
    {
        return node.GetValue<GuiToggleValue>()?.Value == CheckState.Checked;
    }

    /// <summary>
    /// Sets the check state of a toggle node.
    /// </summary>
    /// <param name="node">The toggle node to update.</param>
    /// <param name="state">The <see cref="CheckState"/> to set on the node.</param>
    /// <returns>The updated <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetCheckState(this ImGuiNode node, CheckState state)
    {
        GuiToggleValue v = node.GetOrCreateValue<GuiToggleValue>();
        if (v.Value == state)
        {
            return node;
        }
        v.Value = state;
        switch (state)
        {
            case CheckState.Checked:
                node.Pseudo = ImGuiNode.PseudoActive;
                break;
            case CheckState.Unchecked:
            case CheckState.Indeterminate:
            default:
                node.Pseudo = null;
                break;
        }
        return node;
    }

    /// <summary>
    /// Executes an action when the checkbox is clicked.
    /// </summary>
    /// <param name="node">The toggle node to attach the click handler to.</param>
    /// <param name="action">The action to execute when the node is clicked. Receives the node and its current checked state.</param>
    /// <returns>The <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode OnChecked(this ImGuiNode node, Action<ImGuiNode, bool> action)
    {
        if (node.GetIsClicked())
        {
            action(node, node.GetIsChecked());
        }
        return node;
    }

    /// <summary>
    /// Registers an input handler for checkbox state changes.
    /// </summary>
    /// <param name="node">The toggle node to attach the input handler to.</param>
    /// <param name="action">The function to execute on mouse up events. Receives the node and its current checked state, returns a <see cref="GuiInputState"/>.</param>
    /// <returns>The <see cref="ImGuiNode"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static ImGuiNode OnCheckedInput(this ImGuiNode node, Func<ImGuiNode, bool, GuiInputState> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);
                if (input.EventType == GuiEventTypes.MouseUp && node.GetIsClicked())
                {
                    var value = node.GetValue<GuiToggleValue>();
                    if (value is { })
                    {
                        var actionState = action(n, value.Value == CheckState.Checked);
                        ImGui.MergeState(ref state, actionState);
                    }
                }
                return state;
            });
        }
        return node;
    }

    /// <summary>
    /// Gets the checkbox color for a node.
    /// </summary>
    /// <param name="node">The node to retrieve the checkbox color from.</param>
    /// <returns>The <see cref="Color"/> used for rendering the checkbox, falling back to the theme's default checkbox color.</returns>
    public static Color GetCheckBoxColor(this ImGuiNode node)
    {
        return node.Color ?? node.Color ?? node.Theme.Colors.GetColor(ColorStyle.CheckBox);
    }
}
