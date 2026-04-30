using static Suity.Helpers.GlobalLocalizer;
using Suity.Views.Graphics;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating text input controls in ImGui.
/// </summary>
public static class GuiTextInputExtensions
{
    //public static ImGuiNode StringInput(this ImGui gui, string? value, string? initValue = null, string? hintText = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    //    => StringInput(gui, $"##string_input#{member}#{line}", value, initValue, hintText);

    /// <summary>
    /// Creates a string text input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current text value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode StringInput(this ImGui gui, string id, string? value, string? initValue = null, 
        string? hintText = null, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(StringInput);
            node.Text = value ?? initValue ?? string.Empty;
            node.SetPadding(node.Theme.TextInputPadding);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Vertical;
            if (!string.IsNullOrEmpty(hintText) || submitMode != TextBoxEditSubmitMode.Auto)
            {
                node.SetValue(new GuiHintTextStyle 
                {
                    HintText = hintText,
                    SubmitMode = submitMode,
                });
            }
        }

        if (value != null && !node.IsEdited)
        {
            node.Text = value;
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a password input control that masks entered text.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current password value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode PasswordInput(this ImGui gui, string id, string? value, string? initValue = null, 
        string? hintText = null, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(StringInput);
            node.Text = value ?? initValue ?? string.Empty;
            node.SetPadding(node.Theme.TextInputPadding);
            node.SetInputFunction();
            node.SetRenderFunction();
            node.SetFitFunction();
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetValue(new GuiHintTextStyle 
            {
                HintText = hintText, 
                Password = true,
                SubmitMode = submitMode,
            });
        }

        if (value != null && !node.IsEdited)
        {
            node.Text = value;
        }

        node.Layout();

        return node;
    }

    //public static ImGuiNode DoubleClickStringInput(this ImGui gui, string? value, string? initValue = null, string? hintText = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    //    => DoubleClickStringInput(gui, $"##d_string_input#{member}#{line}", value, initValue, hintText);
    /// <summary>
    /// Creates a string input that requires double-click to activate editing.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current text value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode DoubleClickStringInput(this ImGui gui, string id, string? value, string? initValue = null, 
        string? hintText = null, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.StringInput(id, value, initValue, hintText, submitMode);

        if (node.IsInitializing)
        {
            node.SetInputFunction(ImGuiInputSystem.DoubleClickStringInput);
        }

        return node;
    }

    //public static ImGuiNode ManualStringInput(this ImGui gui, string? value, string? initValue = null, string? hintText = null, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    //    => ManualStringInput(gui, $"##m_string_input#{member}#{line}", value, initValue, hintText);
    /// <summary>
    /// Creates a string input that allows manual activation for editing.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current text value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode ManualStringInput(this ImGui gui, string id, string? value, string? initValue = null, 
        string? hintText = null, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.StringInput(id, value, initValue, hintText, submitMode);

        if (node.IsInitializing)
        {
            node.SetInputFunction(ImGuiInputSystem.SimpleStringInput);
        }

        return node;
    }

    /// <summary>
    /// Creates a multi-line text area input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current text value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="autoFit">Whether to automatically fit the size to content.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode TextAreaInput(this ImGui gui, string id, string? value, string? initValue = null, 
        string? hintText = null, bool autoFit = true, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(TextAreaInput);
            node.Text = value ?? initValue ?? string.Empty;
            node.SetPadding(node.Theme.TextInputPadding);
            node.SetInputFunction();
            node.SetRenderFunction();
            if (autoFit)
            {
                node.SetFitFunction();
            }
            node.FitOrientation = GuiOrientation.Vertical;
            node.SetValue(new GuiHintTextStyle { HintText = hintText, Multiline = true, SubmitMode = submitMode });
        }

        if (value != null && !node.IsEdited)
        {
            node.Text = value;
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a multi-line text area input that requires double-click to activate editing.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current text value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="hintText">Hint text displayed when the input is empty.</param>
    /// <param name="autoFit">Whether to automatically fit the size to content.</param>
    /// <param name="submitMode">The mode that determines when text changes are submitted.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode DoubleClickTextAreaInput(this ImGui gui, string id, string? value, string? initValue = null,
        string? hintText = null, bool autoFit = true, TextBoxEditSubmitMode submitMode = TextBoxEditSubmitMode.Auto)
    {
        ImGuiNode node = gui.TextAreaInput(id, value, initValue, hintText, autoFit, submitMode);

        if (node.IsInitializing)
        {
            node.SetInputFunction(ImGuiInputSystem.DoubleClickStringInput);
        }

        return node;
    }

    /// <summary>
    /// Creates a numeric input control using a generic numeric value.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="initValue">The initial numeric value configuration.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode NumericInput(this ImGui gui, string id, GuiNumericValue initValue)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupNumericInput(initValue);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates an integer numeric input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="initMin">Initial minimum value constraint.</param>
    /// <param name="initMax">Initial maximum value constraint.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode NumericInput(this ImGui gui, string id, int? value = null, int initValue = 0, int? initMin = null, int? initMax = null, int initIncrement = 1)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupNumericInput(new GuiNumericValue<int>(value ?? initValue)
            {
                Min = initMin.HasValue ? (decimal)initMin.Value : null,
                Max = initMax.HasValue ? (decimal)initMax.Value : null,
                Increment = initIncrement,
            });
        }

        if (value.HasValue && !node.IsEdited && node.GetValue<GuiNumericValue>() is GuiNumericValue<int> v)
        {
            v.Value = value.Value;
            v.SetText(node, true);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a long numeric input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="initMin">Initial minimum value constraint.</param>
    /// <param name="initMax">Initial maximum value constraint.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode NumericInput(this ImGui gui, string id, long? value = null, long initValue = 0, long? initMin = null, long? initMax = null, long initIncrement = 1)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupNumericInput(new GuiNumericValue<long>(value ?? initValue)
            {
                Min = initMin.HasValue ? (decimal)initMin.Value : null,
                Max = initMax.HasValue ? (decimal)initMax.Value : null,
                Increment = initIncrement,
            });
        }

        if (value.HasValue && !node.IsEdited && node.GetValue<GuiNumericValue>() is GuiNumericValue<long> v)
        {
            v.Value = value.Value;
            v.SetText(node, true);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a float numeric input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="initMin">Initial minimum value constraint.</param>
    /// <param name="initMax">Initial maximum value constraint.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <param name="clamp">Whether to clamp values to min/max bounds.</param>
    /// <param name="refreshAtOnce">Whether to refresh the value immediately on change.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode NumericInput(this ImGui gui, string id, float? value = null, float initValue = 0, 
        float? initMin = null, float? initMax = null, float initIncrement = 1, bool clamp = false, bool refreshAtOnce = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupNumericInput(new GuiNumericValue<float>(value ?? initValue)
            {
                Min = initMin.HasValue ? (decimal)initMin.Value : null,
                Max = initMax.HasValue ? (decimal)initMax.Value : null,
                Increment = (decimal)initIncrement,
                ClampMax = clamp,
                ClampMin = clamp,
                RefreshAtOnce = refreshAtOnce,
            });
        }

        if (value.HasValue && !node.IsEdited && node.GetValue<GuiNumericValue>() is GuiNumericValue<float> v)
        {
            v.Value = value.Value;
            v.SetText(node, true);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a double numeric input control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current value.</param>
    /// <param name="initValue">Initial value when the control is first created.</param>
    /// <param name="initMin">Initial minimum value constraint.</param>
    /// <param name="initMax">Initial maximum value constraint.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode NumericInput(this ImGui gui, string id, double? value = null, double initValue = 0, double? initMin = null, double? initMax = null, double initIncrement = 1)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupNumericInput(new GuiNumericValue<double>(value ?? initValue)
            {
                Min = initMin.HasValue ? (decimal)initMin.Value : null,
                Max = initMax.HasValue ? (decimal)initMax.Value : null,
                Increment = (decimal)initIncrement,
            });
        }

        if (value.HasValue && !node.IsEdited && node.GetValue<GuiNumericValue>() is GuiNumericValue<double> v)
        {
            v.Value = value.Value;
            v.SetText(node, true);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Creates a progress bar control with auto-generated ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="value">The current progress value.</param>
    /// <param name="initMax">The maximum value of the progress bar.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <param name="line">Caller line number for auto-generated ID.</param>
    /// <param name="member">Caller member name for auto-generated ID.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode ProgressBar(this ImGui gui, float? value, float initMax = 1f, float initIncrement = 0.1f, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => ProgressBar(gui, $"##progress_bar#{member}#{line}", value, initMax, initIncrement);

    /// <summary>
    /// Creates a progress bar control with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The current progress value.</param>
    /// <param name="initMax">The maximum value of the progress bar.</param>
    /// <param name="initIncrement">The increment step value.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode ProgressBar(this ImGui gui, string id, float? value, float initMax = 1f, float initIncrement = 0.1f)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.SetupProgressBar(new GuiNumericValue<float>(value ?? 0f)
            {
                Min = 0,
                Max = (decimal)initMax,
                Increment = (decimal)initIncrement,
            });
        }

        if (value.HasValue && !node.IsEdited && node.GetValue<GuiNumericValue>() is GuiNumericValue<float> v)
        {
            v.Value = value.Value;
            v.SetText(node, true);
        }

        node.Layout();

        return node;
    }

    /// <summary>
    /// Sets the minimum and maximum values for a numeric input node.
    /// </summary>
    /// <param name="node">The numeric input node.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetNumericValueMinMax(this ImGuiNode node, decimal min, decimal max)
    {
        var value = node.GetValue<GuiNumericValue>();
        if (value is { })
        {
            value.Min = min;
            value.Max = max;
        }

        return node;
    }

    /// <summary>
    /// Configures a node as a numeric input with the specified initial value.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    /// <param name="initValue">The initial numeric value configuration.</param>
    public static void SetupNumericInput(this ImGuiNode node, GuiNumericValue initValue)
    {
        node.TypeName = nameof(NumericInput);
        node.SetPadding(node.Theme.TextInputPadding);
        node.SetInputFunction();
        node.SetRenderFunction();
        node.SetFitFunction();
        node.FitOrientation = GuiOrientation.Vertical;

        node.SetValue(initValue);
        initValue.SetText(node, true);

        node.Layout();
    }

    /// <summary>
    /// Configures a node as a progress bar with the specified initial value.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    /// <param name="initValue">The initial numeric value configuration.</param>
    public static void SetupProgressBar(this ImGuiNode node, GuiNumericValue initValue)
    {
        node.TypeName = nameof(ProgressBar);
        node.SetPadding(node.Theme.TextInputPadding);
        node.SetRenderFunction(nameof(NumericInput));
        node.SetFitFunction(nameof(NumericInput));
        node.FitOrientation = GuiOrientation.Vertical;

        node.SetValue(initValue);
        initValue.SetText(node, true);

        node.Layout();
    }

    /// <summary>
    /// Sets the unit string for a numeric input node during initialization.
    /// </summary>
    /// <param name="node">The numeric input node.</param>
    /// <param name="unit">The unit string to display (e.g., "px", "%").</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitNumericUnit(this ImGuiNode node, string unit)
    {
        if (node.IsInitializing)
        {
            var value = node.GetValue<GuiNumericValue>();
            if (value is { })
            {
                value.Unit = unit;
                value.SetText(node, true);
            }
        }

        return node;
    }

    /// <summary>
    /// Gets the current numeric value from a numeric input node.
    /// </summary>
    /// <typeparam name="T">The numeric type to retrieve.</typeparam>
    /// <param name="node">The numeric input node.</param>
    /// <returns>The current value, or null if not available.</returns>
    public static T? GetNumericValue<T>(this ImGuiNode node) where T : struct
    {
        var v = node.GetValue<GuiNumericValue>();
        if (v is { })
        {
            if (v is GuiNumericValue<T> vt)
            {
                return vt.Value;
            }
            else
            {
                return v.GetValue<T>();
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the current numeric value on a numeric input node.
    /// </summary>
    /// <typeparam name="T">The numeric type to set.</typeparam>
    /// <param name="node">The numeric input node.</param>
    /// <param name="value">The value to set.</param>
    public static void SetNumericValue<T>(this ImGuiNode node, T value) where T : struct
    {
        var v = node.GetValue<GuiNumericValue>() as GuiNumericValue<T>;
        if (v is { })
        {
            v.Value = value;
        }
        else
        {
            v = new GuiNumericValue<T>(value);
            node.SetValue<GuiNumericValue>(v);
        }
    }

    /// <summary>
    /// Gets the background color used for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The background color.</returns>
    public static Color GetTextInputBackgroundColor(this ImGuiNode node)
    {
        return node.Color ?? node.Theme.Colors.GetColor(ColorStyle.MdiBackground);
    }

    /// <summary>
    /// Gets the corner roundness value for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <param name="scaled">Whether to return the globally scaled value.</param>
    /// <returns>The corner roundness value.</returns>
    public static float GetTextInputCornerRound(this ImGuiNode node, bool scaled)
    {
        float v = node.CornerRound ?? node.Theme.ButtonCornerRound;
        if (scaled)
        {
            v = node.GlobalScaleValue(v);
        }

        return v;
    }

    /// <summary>
    /// Gets the global rectangle for the text input area.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <param name="withPadding">Whether to include padding in the calculation.</param>
    /// <returns>The global rectangle of the text input area.</returns>
    public static RectangleF GetGlobalTextInputRect(this ImGuiNode node, bool withPadding)
    {
        var rect = node.GlobalRect;
        float h = rect.Height;

        if (withPadding)
        {
            var padding = node.Padding;
            var p = padding ?? node.Theme.TextInputPadding;
            rect = p.Shrink(rect, node.GlobalScale);
        }

        if (node.Image != null)
        {
            rect.X += h;
            rect.Width -= h;
        }

        return rect;
    }

    /// <summary>
    /// Gets the top padding for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The top padding value.</returns>
    public static float GetTextInputPaddingTop(this ImGuiNode node)
        => node.Padding?.Top ?? node.Theme.TextInputPadding;

    /// <summary>
    /// Gets the bottom padding for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The bottom padding value.</returns>
    public static float GetTextInputPaddingBottom(this ImGuiNode node)
        => node.Padding?.Bottom ?? node.Theme.TextInputPadding;

    /// <summary>
    /// Gets the left padding for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The left padding value.</returns>
    public static float GetTextInputPaddingLeft(this ImGuiNode node)
        => node.Padding?.Left ?? node.Theme.TextInputPadding;

    /// <summary>
    /// Gets the right padding for text input controls.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The right padding value.</returns>
    public static float GetTextInputPaddingRight(this ImGuiNode node)
        => node.Padding?.Right ?? node.Theme.TextInputPadding;

    /// <summary>
    /// Begins the editing state for a text input node.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode BeginEdit(this ImGuiNode node)
    {
        node.InputFunction?.Invoke(GuiPipeline.BeginEdit, node, node.Gui.Input, (_, _) => GuiInputState.None);

        return node;
    }

    /// <summary>
    /// Sets the hint text for a text input node.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <param name="hintText">The hint text to display when the input is empty.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHintText(this ImGuiNode node, string hintText)
    {
        if (!string.IsNullOrEmpty(hintText))
        {
            var style = node.GetOrCreateValue<GuiHintTextStyle>();
            if (style.HintText != hintText)
            {
                style.HintText = hintText;
                node.MarkRenderDirty();
            }
        }
        else
        {
            node.RemoveValue<GuiHintTextStyle>();
        }

        return node;
    }

    /// <summary>
    /// Sets the localized hint text for a text input node.
    /// </summary>
    /// <param name="node">The text input node.</param>
    /// <param name="hintText">The hint text key to localize and display.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHintTextL(this ImGuiNode node, string hintText)
    {
        if (!string.IsNullOrEmpty(hintText))
        {
            var style = node.GetOrCreateValue<GuiHintTextStyle>();
            string hintTextL = L(hintText);
            if (style.HintText != hintTextL)
            {
                style.HintText = hintTextL;
                node.MarkRenderDirty();
            }
        }
        else
        {
            node.RemoveValue<GuiHintTextStyle>();
        }

        return node;
    }
}

/// <summary>
/// Base class for numeric value storage and manipulation in numeric input controls.
/// Handles value clamping, validation, and text conversion.
/// </summary>
public abstract class GuiNumericValue
{
    private decimal _value;

    /// <summary>
    /// Gets or sets the numeric value as a decimal with clamping applied.
    /// </summary>
    public decimal DecimalValue
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;
            }

            value = Math.Max(value, TypeMin);
            value = Math.Min(value, TypeMax);

            var v = value;
            if (Min.HasValue && ClampMin)
            {
                v = Math.Max(v, Min.Value);
            }

            if (Max.HasValue && ClampMax)
            {
                v = Math.Min(v, Max.Value);
            }

            IsValueClamped = value != v;

            _value = v;
        }
    }

    /// <summary>
    /// Gets the minimum value for the underlying numeric type.
    /// </summary>
    public decimal TypeMin { get; protected set; }
    /// <summary>
    /// Gets the maximum value for the underlying numeric type.
    /// </summary>
    public decimal TypeMax { get; protected set; }

    /// <summary>
    /// Gets or sets the minimum value constraint.
    /// </summary>
    public decimal? Min { get; set; }
    /// <summary>
    /// Gets or sets the maximum value constraint.
    /// </summary>
    public decimal? Max { get; set; }

    /// <summary>
    /// Gets or sets whether to clamp values to the minimum bound.
    /// </summary>
    public bool ClampMin { get; set; }
    /// <summary>
    /// Gets or sets whether to clamp values to the maximum bound.
    /// </summary>
    public bool ClampMax { get; set; }

    /// <summary>
    /// Gets or sets the number of decimal places for display.
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Gets or sets the increment step value.
    /// </summary>
    public decimal Increment { get; set; } = 1;
    /// <summary>
    /// Gets or sets whether the current text input is valid.
    /// </summary>
    public bool IsTextValid { get; set; } = true;
    /// <summary>
    /// Gets or sets whether the current value has been clamped.
    /// </summary>
    public bool IsValueClamped { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to refresh the value immediately on change.
    /// </summary>
    public bool RefreshAtOnce { get; set; }

    /// <summary>
    /// Gets or sets the unit string to display alongside the value.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets the color for the numeric value display.
    /// </summary>
    public Color? Color { get; set; }
    /// <summary>
    /// Gets or sets the color to use when the value is at minimum.
    /// </summary>
    public Color? MinColor { get; set; }
    /// <summary>
    /// Gets or sets the color to use when the value is at maximum.
    /// </summary>
    public Color? MaxColor { get; set; }


    /// <summary>
    /// Converts the current numeric value to display text on the node.
    /// </summary>
    /// <param name="node">The target ImGuiNode.</param>
    /// <param name="preview">Whether to format the text for preview display.</param>
    public virtual void SetText(ImGuiNode node, bool preview = true)
    {
    }

    /// <summary>
    /// Parses the text from the node and updates the numeric value.
    /// </summary>
    /// <param name="node">The source ImGuiNode.</param>
    public virtual void SetValueFromText(ImGuiNode node)
    {
    }

    /// <summary>
    /// Converts the decimal value to the specified numeric type.
    /// </summary>
    /// <typeparam name="TValue">The target numeric type.</typeparam>
    /// <returns>The converted value.</returns>
    public TValue GetValue<TValue>() where TValue : struct
    {
        return (TValue)Convert.ChangeType(DecimalValue, typeof(TValue));
    }

    /// <summary>
    /// Safely converts an object to a decimal value, handling NaN and infinity.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <returns>The decimal representation, with bounds checking.</returns>
    public static decimal SafeToDecimal(object? o)
    {
        //Mod by simage
        if (o is float)
        {
            float v = (float)o;
            if (float.IsNaN(v))
                return decimal.Zero;
            else if (v <= (float)decimal.MinValue || float.IsNegativeInfinity(v))
                return decimal.MinValue;
            else if (v >= (float)decimal.MaxValue || float.IsPositiveInfinity(v))
                return decimal.MaxValue;
            else
                return (decimal)v;
        }
        else if (o is decimal dec)
        {
            return dec;
        }
        else if (o is null)
        {
            return 0;
        }
        else
        {
            double v = Convert.ToDouble(o);
            if (double.IsNaN(v))
                return decimal.Zero;
            else if (v <= (double)decimal.MinValue || double.IsNegativeInfinity(v))
                return decimal.MinValue;
            else if (v >= (double)decimal.MaxValue || double.IsPositiveInfinity(v))
                return decimal.MaxValue;
            else
                return (decimal)v;
        }
    }
}

/// <summary>
/// Generic implementation of GuiNumericValue for a specific numeric type.
/// Handles type-specific initialization, text formatting, and parsing.
/// </summary>
/// <typeparam name="T">The numeric type (e.g., int, float, double).</typeparam>
public sealed class GuiNumericValue<T> : GuiNumericValue where T : struct
{
    /// <summary>
    /// Creates a new instance with default values.
    /// </summary>
    public GuiNumericValue()
    {
        UpdateType();
    }

    /// <summary>
    /// Creates a new instance with the specified initial value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public GuiNumericValue(T value)
    {
        UpdateType();
        DecimalValue = SafeToDecimal(value);
    }

    /// <summary>
    /// Creates a new instance with the specified decimal value.
    /// </summary>
    /// <param name="value">The initial decimal value.</param>
    public GuiNumericValue(decimal value)
    {
        UpdateType();
        DecimalValue = value;
    }

    /// <summary>
    /// Gets or sets the typed numeric value.
    /// </summary>
    public T Value
    {
        get => (T)Convert.ChangeType(DecimalValue, typeof(T));
        set => DecimalValue = SafeToDecimal(value);
    }

    private void UpdateType()
    {
        var type = typeof(T);

        if (type == typeof(byte))
        {
            DecimalPlaces = 0;
            TypeMin = byte.MinValue;
            TypeMax = byte.MaxValue;
        }
        else if (type == typeof(sbyte))
        {
            DecimalPlaces = 0;
            TypeMin = sbyte.MinValue;
            TypeMax = sbyte.MaxValue;
        }
        else if (type == typeof(short))
        {
            DecimalPlaces = 0;
            TypeMin = short.MinValue;
            TypeMax = short.MaxValue;
        }
        else if (type == typeof(ushort))
        {
            DecimalPlaces = 0;
            TypeMin = ushort.MinValue;
            TypeMax = ushort.MaxValue;
        }
        else if (type == typeof(int))
        {
            DecimalPlaces = 0;
            TypeMin = int.MinValue;
            TypeMax = int.MaxValue;
        }
        else if (type == typeof(uint))
        {
            DecimalPlaces = 0;
            TypeMin = uint.MinValue;
            TypeMax = uint.MaxValue;
        }
        else if (type == typeof(long))
        {
            DecimalPlaces = 0;
            TypeMin = long.MinValue;
            TypeMax = long.MaxValue;
        }
        else if (type == typeof(ulong))
        {
            DecimalPlaces = 0;
            TypeMin = ulong.MinValue;
            TypeMax = ulong.MaxValue;
        }
        else if (type == typeof(float))
        {
            DecimalPlaces = 2; //Mod by simage 2 -> 6
            TypeMin = decimal.MinValue;
            TypeMax = decimal.MaxValue;
        }
        else if (type == typeof(double))
        {
            DecimalPlaces = 2; //Mod by simage 2 -> 10
            TypeMin = decimal.MinValue;
            TypeMax = decimal.MaxValue;
        }
        else if (type == typeof(decimal))
        {
            DecimalPlaces = 2; //Mod by simage 2 -> 10
            TypeMin = decimal.MinValue;
            TypeMax = decimal.MaxValue;
        }
    }

    /// <inheritdoc/>
    public override void SetText(ImGuiNode node, bool preview = true)
    {
        if (preview)
        {
            string text;

            if (this.DecimalPlaces > 0)
            {
                decimal beforeSep = this.DecimalValue >= 0m
                    ? Math.Floor(this.DecimalValue)
                    : Math.Ceiling(this.DecimalValue);
                decimal afterSep = Math.Abs(this.DecimalValue - beforeSep);
                decimal placesMult = (decimal)Math.Pow(10.0d, this.DecimalPlaces);

                beforeSep = Math.Abs(beforeSep);
                afterSep = Math.Round(afterSep * placesMult);

                while (afterSep >= placesMult)
                {
                    beforeSep++;
                    afterSep -= placesMult;
                }

                text = (this.DecimalValue < 0m ? "-" : "") + beforeSep.ToString() + "." +
                       afterSep.ToString().PadLeft(this.DecimalPlaces, '0');
            }
            else
            {
                text = Math.Round(this.DecimalValue).ToString();
            }

            if (!string.IsNullOrWhiteSpace(Unit))
            {
                node.Text = $"{text} {Unit}";
            }
            else
            {
                node.Text = text;
            }
        }
        else
        {
            node.Text = this.DecimalValue.ToString();
        }

        IsTextValid = true;
        IsValueClamped = false;
    }

    /// <inheritdoc/>
    public override void SetValueFromText(ImGuiNode node)
    {
        IsTextValid = decimal.TryParse(node.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valResult);
        if (!IsTextValid && string.IsNullOrWhiteSpace(node.Text))
        {
            IsTextValid = true;
            valResult = 0m;
        }

        if (IsTextValid)
        {
            var v = valResult;

            if (Min.HasValue && ClampMin)
            {
                v = Math.Max(v, Min.Value);
            }

            if (Max.HasValue && ClampMax)
            {
                v = Math.Min(v, Max.Value);
            }

            this.IsValueClamped = valResult != v;
            this.DecimalValue = v;
            node.IsEdited = true;
        }
        else
        {
            this.IsValueClamped = false;
        }
    }
}