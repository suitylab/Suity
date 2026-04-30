namespace Suity.Views.Im;

/// <summary>
/// Extension methods for configuring ImGui themes with fluent API, including style targeting, function assignment, and transition setup.
/// </summary>
public static class GuiThemeExtensions
{
    /// <summary>
    /// Sets the current style target to a type name with optional pseudo state.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="name">The type name to target.</param>
    /// <param name="pseudo">Optional pseudo state (e.g., "hover", "active").</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme TypeNameStyle(this ImGuiTheme theme, string name, string? pseudo = null)
    {
        theme.CurrentName = name;
        theme.CurrentPseudo = pseudo;
        return theme;
    }

    /// <summary>
    /// Sets the current style target to a CSS-like class with optional pseudo state.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="class">The class name to target (without the leading dot).</param>
    /// <param name="pseudo">Optional pseudo state.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme ClassStyle(this ImGuiTheme theme, string @class, string? pseudo = null)
    {
        theme.CurrentName = "." + @class;
        theme.CurrentPseudo = pseudo;
        return theme;
    }

    /// <summary>
    /// Sets the current style target to a specific node ID with optional pseudo state.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="id">The node ID to target (without the leading hash).</param>
    /// <param name="pseudo">Optional pseudo state.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme IdStyle(this ImGuiTheme theme, string id, string? pseudo = null)
    {
        theme.CurrentName = "#" + id;
        theme.CurrentPseudo = pseudo;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state for style targeting.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="pseudo">The pseudo state to set, or null to clear.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme Pseudo(this ImGuiTheme theme, string? pseudo = null)
    {
        theme.CurrentPseudo = pseudo;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state to mouse-in.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme PseudoMouseIn(this ImGuiTheme theme)
    {
        theme.CurrentPseudo = ImGuiNode.PseudoMouseIn;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state to mouse-down.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme PseudoMouseDown(this ImGuiTheme theme)
    {
        theme.CurrentPseudo = ImGuiNode.PseudoMouseDown;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state to active.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme PseudoActive(this ImGuiTheme theme)
    {
        theme.CurrentPseudo = ImGuiNode.PseudoActive;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state to active mouse-in.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme PseudoActiveMouseIn(this ImGuiTheme theme)
    {
        theme.CurrentPseudo = ImGuiNode.PseudoActiveMouseIn;
        return theme;
    }

    /// <summary>
    /// Sets the pseudo state to active mouse-down.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme PseudoActiveMouseDown(this ImGuiTheme theme)
    {
        theme.CurrentPseudo = ImGuiNode.PseudoActiveMouseDown;
        return theme;
    }

    /// <summary>
    /// Clears the current style target and pseudo state.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme EndStyle(this ImGuiTheme theme)
    {
        theme.CurrentName = null;
        theme.CurrentPseudo = null;
        return theme;
    }

    /// <summary>
    /// Sets a style value using fluent API.
    /// </summary>
    /// <typeparam name="T">The type of the style value (must be a reference type).</typeparam>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="value">The style value to set.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetStyleFluent<T>(this ImGuiTheme theme, T value) where T : class
    {
        theme.SetStyle(value);
        return theme;
    }

    #region Function

    /// <summary>
    /// Sets an input function chain for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="func">The input function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetInputFunctionChain(this ImGuiTheme theme, InputFunction func)
    {
        theme.SetStyle(new GuiInputFunctionStyle { Function = func });
        return theme;
    }

    /// <summary>
    /// Sets an input function chain by name for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="funcName">The name of the input function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetInputFunctionChain(this ImGuiTheme theme, string funcName)
    {
        theme.SetStyle(new GuiInputFunctionStyle { FunctionName = funcName });
        return theme;
    }

    /// <summary>
    /// Sets a layout function chain for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="func">The layout function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetLayoutFunctionChain(this ImGuiTheme theme, LayoutFunction func)
    {
        theme.SetStyle(new GuiLayoutFunctionStyle() { Function = func });
        return theme;
    }

    /// <summary>
    /// Sets a layout function chain by name for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="funcName">The name of the layout function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetLayoutFunctionChain(this ImGuiTheme theme, string funcName)
    {
        theme.SetStyle(new GuiLayoutFunctionStyle { FunctionName = funcName });
        return theme;
    }

    /// <summary>
    /// Sets a fit function chain for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="func">The fit function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetFitFunctionChain(this ImGuiTheme theme, FitFunction func)
    {
        theme.SetStyle(new GuiFitFunctionStyle() { Function = func });
        return theme;
    }

    /// <summary>
    /// Sets a fit function chain by name for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="funcName">The name of the fit function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetFitFunctionChain(this ImGuiTheme theme, string funcName)
    {
        theme.SetStyle(new GuiFitFunctionStyle { FunctionName = funcName });
        return theme;
    }

    /// <summary>
    /// Sets a render function chain for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="func">The render function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetRenderFunctionChain(this ImGuiTheme theme, RenderFunction func)
    {
        theme.SetStyle(new GuiRenderFunctionStyle() { Function = func });
        return theme;
    }

    /// <summary>
    /// Sets a render function chain by name for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="funcName">The name of the render function to assign.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetRenderFunctionChain(this ImGuiTheme theme, string funcName)
    {
        theme.SetStyle(new GuiRenderFunctionStyle { FunctionName = funcName });
        return theme;
    }

    #endregion

    #region Transition

    /// <summary>
    /// Sets a linear transition on an ImGui instance.
    /// </summary>
    /// <param name="gui">The ImGui instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="duration">The transition duration in seconds.</param>
    /// <returns>The same <see cref="ImGui"/> instance for fluent chaining.</returns>
    public static ImGui SetLinearTransition(this ImGui gui, string name, string pseudo, string targetPseudo, float duration)
    {
        gui.SetCurrentTransition(name, pseudo, targetPseudo, ImGuiExternal._external.CreateEase(duration));
        return gui;
    }

    /// <summary>
    /// Sets separate linear transitions for entering and exiting a state.
    /// </summary>
    /// <param name="gui">The ImGui instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="durationIn">The duration for entering the state in seconds.</param>
    /// <param name="durationOut">The duration for exiting the state in seconds.</param>
    /// <returns>The same <see cref="ImGui"/> instance for fluent chaining.</returns>
    public static ImGui SetInOutLinearTransition(this ImGui gui, string name, string pseudo, string targetPseudo, float durationIn, float durationOut)
    {
        gui.SetCurrentTransition(name, pseudo, targetPseudo, ImGuiExternal._external.CreateEase(durationIn));
        gui.SetCurrentTransition(name, targetPseudo, pseudo, ImGuiExternal._external.CreateEase(durationOut));
        return gui;
    }

    /// <summary>
    /// Sets linear transitions for mouse-in and mouse-out states.
    /// </summary>
    /// <param name="gui">The ImGui instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="durationIn">The duration for mouse-in transition in seconds.</param>
    /// <param name="durationOut">The duration for mouse-out transition in seconds.</param>
    /// <returns>The same <see cref="ImGui"/> instance for fluent chaining.</returns>
    public static ImGui SetMouseInLinearTransition(this ImGui gui, string name, float durationIn, float durationOut)
    {
        gui.SetCurrentTransition(name, string.Empty, "mouse-in", ImGuiExternal._external.CreateEase(durationIn));
        gui.SetCurrentTransition(name, "mouse-in", string.Empty, ImGuiExternal._external.CreateEase(durationOut));
        return gui;
    }

    /// <summary>
    /// Sets a linear transition for the current style target.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="duration">The transition duration in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetLinearTransition(this ImGuiTheme theme, string name, string? pseudo, string? targetPseudo, float duration)
    {
        theme.SetTransition(name, pseudo, targetPseudo, ImGuiExternal._external.CreateEase(duration));
        return theme;
    }

    /// <summary>
    /// Sets a linear transition using the current style target name.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="duration">The transition duration in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetLinearTransition(this ImGuiTheme theme, string? pseudo, string? targetPseudo, float duration)
    {
        theme.SetTransition(pseudo, targetPseudo, ImGuiExternal._external.CreateEase(duration));
        return theme;
    }

    /// <summary>
    /// Sets separate linear transitions for entering and exiting a state.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="durationIn">The duration for entering the state in seconds.</param>
    /// <param name="durationOut">The duration for exiting the state in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetInOutLinearTransition(this ImGuiTheme theme, string name, string? pseudo, string? targetPseudo, float durationIn, float durationOut)
    {
        theme.SetTransition(name, pseudo, targetPseudo, ImGuiExternal._external.CreateEase(durationIn));
        theme.SetTransition(name, targetPseudo, pseudo, ImGuiExternal._external.CreateEase(durationOut));
        return theme;
    }

    /// <summary>
    /// Sets separate linear transitions using the current style target name.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="pseudo">The source pseudo state.</param>
    /// <param name="targetPseudo">The target pseudo state.</param>
    /// <param name="durationIn">The duration for entering the state in seconds.</param>
    /// <param name="durationOut">The duration for exiting the state in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetInOutLinearTransition(this ImGuiTheme theme, string? pseudo, string? targetPseudo, float durationIn, float durationOut)
    {
        theme.SetTransition(pseudo, targetPseudo, ImGuiExternal._external.CreateEase(durationIn));
        theme.SetTransition(targetPseudo, pseudo, ImGuiExternal._external.CreateEase(durationOut));
        return theme;
    }

    /// <summary>
    /// Sets linear transitions for mouse-in and mouse-out states.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="name">The style target name.</param>
    /// <param name="durationIn">The duration for mouse-in transition in seconds.</param>
    /// <param name="durationOut">The duration for mouse-out transition in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetMouseInLinearTransition(this ImGuiTheme theme, string name, float durationIn, float durationOut)
    {
        theme.SetTransition(name, string.Empty, "mouse-in", ImGuiExternal._external.CreateEase(durationIn));
        theme.SetTransition(name, "mouse-in", string.Empty, ImGuiExternal._external.CreateEase(durationOut));
        return theme;
    }

    /// <summary>
    /// Sets linear transitions for mouse-in and mouse-out states using the current style target name.
    /// </summary>
    /// <param name="theme">The theme instance to configure.</param>
    /// <param name="durationIn">The duration for mouse-in transition in seconds.</param>
    /// <param name="durationOut">The duration for mouse-out transition in seconds.</param>
    /// <returns>The same <see cref="ImGuiTheme"/> instance for fluent chaining.</returns>
    public static ImGuiTheme SetMouseInLinearTransition(this ImGuiTheme theme, float durationIn, float durationOut)
    {
        theme.SetTransition(string.Empty, "mouse-in", ImGuiExternal._external.CreateEase(durationIn));
        theme.SetTransition("mouse-in", string.Empty, ImGuiExternal._external.CreateEase(durationOut));
        return theme;
    }

    #endregion
}
