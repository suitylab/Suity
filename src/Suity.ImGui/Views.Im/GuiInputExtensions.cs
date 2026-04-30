using Suity.Views.Graphics;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for handling input events, mouse interactions, keyboard events, and context menus in ImGui.
/// </summary>
public static class GuiInputExtensions
{
    #region System

    /// <summary>
    /// Executes an action when the node is being initialized.
    /// </summary>
    public static ImGuiNode OnInitialize(this ImGuiNode node, Action<ImGuiNode> action)
    {
        if (node.IsInitializing)
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Searches up the node hierarchy for a value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to find.</typeparam>
    /// <param name="node">The starting node.</param>
    /// <param name="includeMe">Whether to include the starting node in the search.</param>
    /// <returns>The found value, or null if not found.</returns>
    public static T? FindValueInHierarchy<T>(this ImGuiNode node, bool includeMe = true) where T : class
    {
        var current = includeMe ? node : node.Parent;

        while (current is { })
        {
            var value = current.GetValue<T>();
            if (value is { })
            {
                return value;
            }
            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Searches up the node hierarchy for a node matching the specified predicate.
    /// </summary>
    /// <param name="node">The starting node.</param>
    /// <param name="predicate">The predicate to match nodes against.</param>
    /// <param name="includeMe">Whether to include the starting node in the search.</param>
    /// <returns>The found node, or null if not found.</returns>
    public static ImGuiNode? FindNodeInHierarchy(this ImGuiNode node, Predicate<ImGuiNode> predicate, bool includeMe = true)
    {
        var current = includeMe ? node : node.Parent;

        while (current is { })
        {
            if (predicate(current))
            {
                return current;
            }

            current = current.Parent;
        }

        return null;
    }

    #endregion

    #region Mouse

    /// <summary>
    /// Gets whether the node has been clicked.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <param name="ignoreEnabled">Whether to ignore the disabled state.</param>
    /// <returns>True if the node was clicked.</returns>
    public static bool GetIsClicked(this ImGuiNode node, bool ignoreEnabled = false)
    {
        if (ignoreEnabled)
        {
            return node.IsHover && node.MouseState == GuiMouseState.Clicked;
        }
        else
        {
            return (!node.IsDisabled) && node.IsHover && node.MouseState == GuiMouseState.Clicked;
        }
    }

    /// <summary>
    /// Gets whether the node has been double-clicked.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <param name="ignoreEnabled">Whether to ignore the disabled state.</param>
    /// <returns>True if the node was double-clicked.</returns>
    public static bool GetIsDoubleClicked(this ImGuiNode node, bool ignoreEnabled = false)
    {
        //if (ignoreEnabled)
        //{
        //    return node.IsHover && node.MouseState == GuiMouseState.Clicked && node.Gui.IsDoubleClick;
        //}
        //else
        //{
        //    return (!node.IsDisabled) && node.IsHover && node.MouseState == GuiMouseState.Clicked && node.Gui.IsDoubleClick;
        //}

        // Temporarily not using Hover, instead using the more lenient IsMouseInClickRect for detection.

        if (ignoreEnabled)
        {
            return node.IsMouseInClickRect && node.MouseState == GuiMouseState.Clicked && node.Gui.IsDoubleClick;
        }
        else
        {
            return (!node.IsDisabled) && node.IsMouseInClickRect && node.MouseState == GuiMouseState.Clicked && node.Gui.IsDoubleClick;
        }
    }

    /// <summary>
    /// Executes an action when the node is clicked.
    /// </summary>
    public static ImGuiNode OnClick(this ImGuiNode node, Action action, bool ignoreEnabled = false)
    {
        if (node.GetIsClicked(ignoreEnabled))
        {
            action();
        }

        return node;
    }

    /// <summary>
    /// Executes an async action when the node is clicked.
    /// </summary>
    public static ImGuiNode OnClick(this ImGuiNode node, Func<Task> action, bool ignoreEnabled = false)
    {
        if (node.GetIsClicked(ignoreEnabled))
        {
            action();
        }

        return node;
    }

    /// <summary>
    /// Executes an action with node access when the node is clicked.
    /// </summary>
    public static ImGuiNode OnClick(this ImGuiNode node, Action<ImGuiNode> action, bool ignoreEnabled = false)
    {
        if (node.GetIsClicked(ignoreEnabled))
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Executes an async action with node access when the node is clicked.
    /// </summary>
    public static ImGuiNode OnClick(this ImGuiNode node, Func<ImGuiNode, Task> action, bool ignoreEnabled = false)
    {
        if (node.GetIsClicked(ignoreEnabled))
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Executes an action when the node is double-clicked.
    /// </summary>
    public static ImGuiNode OnDoubleClick(this ImGuiNode node, Action action, bool ignoreEnabled = false)
    {
        if (node.GetIsDoubleClicked(ignoreEnabled))
        {
            action();
        }

        return node;
    }

    /// <summary>
    /// Executes an async action when the node is double-clicked.
    /// </summary>
    public static ImGuiNode OnDoubleClick(this ImGuiNode node, Func<Task> action, bool ignoreEnabled = false)
    {
        if (node.GetIsDoubleClicked(ignoreEnabled))
        {
            action();
        }

        return node;
    }

    /// <summary>
    /// Executes an action with node access when the node is double-clicked.
    /// </summary>
    public static ImGuiNode OnDoubleClick(this ImGuiNode node, Action<ImGuiNode> action, bool ignoreEnabled = false)
    {
        if (node.GetIsDoubleClicked(ignoreEnabled))
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Executes an async action with node access when the node is double-clicked.
    /// </summary>
    public static ImGuiNode OnDoubleClick(this ImGuiNode node, Func<ImGuiNode, Task> action, bool ignoreEnabled = false)
    {
        if (node.GetIsDoubleClicked(ignoreEnabled))
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Initializes click input handling with a custom action that returns an input state.
    /// </summary>
    public static ImGuiNode InitInputClick(this ImGuiNode node, Func<ImGuiNode, GuiInputState> action, bool ignoreEnabled = false)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseUp && node.GetIsClicked(ignoreEnabled))
                {
                    var actionState = action(n);
                    ImGui.MergeState(ref state, actionState);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes mouse down input handling with an action that receives the mouse button.
    /// </summary>
    public static ImGuiNode InitInputMouseDown(this ImGuiNode node, Func<ImGuiNode, GuiMouseButtons, GuiInputState> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseDown)
                {
                    var actionState = action(n, input.MouseButton);
                    ImGui.MergeState(ref state, actionState);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes mouse down input handling for a specific button.
    /// </summary>
    public static ImGuiNode InitInputMouseDown(this ImGuiNode node, GuiMouseButtons button, Func<ImGuiNode, GuiInputState> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseDown && input.MouseButton == button)
                {
                    var actionState = action(n);
                    ImGui.MergeState(ref state, actionState);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes mouse up input handling for a specific button.
    /// </summary>
    public static ImGuiNode InitInputMouseUp(this ImGuiNode node, GuiMouseButtons button, Func<ImGuiNode, GuiInputState> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseUp && input.MouseButton == button)
                {
                    var actionState = action(n);
                    ImGui.MergeState(ref state, actionState);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes mouse up input handling with an action that receives the mouse button.
    /// </summary>
    public static ImGuiNode InitInputMouseUp(this ImGuiNode node, Func<ImGuiNode, GuiMouseButtons, GuiInputState> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseUp)
                {
                    var actionState = action(n, input.MouseButton);
                    // Set handle to true to prevent further processing.
                    input.Handled = true;

                    ImGui.MergeState(ref state, actionState);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes double-click input handling.
    /// </summary>
    /// <param name="node">The node to attach the double-click handler to.</param>
    /// <param name="action">The action to execute on double-click, receiving the node as a parameter.</param>
    /// <returns>The node instance for method chaining.</returns>
    public static ImGuiNode InitInputDoubleClicked(this ImGuiNode node, Action<ImGuiNode> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseUp && node.Gui.IsDoubleClick)
                {
                    return GuiInputState.FullSync;
                }

                return state;
            });
        }

        if (node.GetIsDoubleClicked())
        {
            action(node);
        }

        return node;
    }

    /// <summary>
    /// Initializes mouse in/out synchronization to trigger full sync on hover state changes.
    /// </summary>
    public static ImGuiNode InitInputMouseInSync(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                if (input.EventType == GuiEventTypes.MouseIn || input.EventType == GuiEventTypes.MouseOut)
                {
                    ImGui.MergeState(ref state, GuiInputState.FullSync);
                }

                return state;
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes hyperlink behavior that opens a URL on click and changes cursor on hover.
    /// </summary>
    public static ImGuiNode InitInputHyperLink(this ImGuiNode node, string url)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);

                switch (input.EventType)
                {
                    case GuiEventTypes.MouseIn:
                        node.Gui.SetCursor(GuiCursorTypes.Hand);
                        ImGui.MergeState(ref state, GuiInputState.Render);
                        break;

                    case GuiEventTypes.MouseOut:
                        node.Gui.SetCursor(GuiCursorTypes.Default);
                        ImGui.MergeState(ref state, GuiInputState.Render);
                        break;

                    case GuiEventTypes.MouseClick:
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception)
                        {
                        }
                        break;
                }

                return state;
            });
        }

        return node;
    }

    #endregion

    #region Key

    /// <summary>
    /// Initializes key down input handling with optional focus requirement.
    /// </summary>
    public static ImGuiNode InitKeyDownInput(this ImGuiNode node, Func<ImGuiNode, IGraphicInput, GuiInputState?> func, bool requireFocus = false)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                if (requireFocus)
                {
                    if (input.EventType == GuiEventTypes.MouseDown)
                    {
                        node.SetIsFocused(true);
                    }
                }

                var state = baseAction(pipeline);
                if (state != GuiInputState.None)
                {
                    // The child node has responded
                    return state;
                }

                // The mouse needs to be inside the node
                if (!node.IsMouseInRect)
                {
                    return GuiInputState.None;
                }

                switch (input.EventType)
                {
                    case GuiEventTypes.KeyDown:
                        if (requireFocus && !node.IsFocused)
                        {
                            return GuiInputState.None;
                        }

                        try
                        {
                            return func(n, input) ?? GuiInputState.None;
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                            return GuiInputState.None;
                        }
                    default:
                        return GuiInputState.None;
                }
            });
        }

        return node;
    }

    /// <summary>
    /// Initializes key up input handling with a custom predicate function.
    /// </summary>
    public static ImGuiNode InitKeyUpInput(this ImGuiNode node, Func<ImGuiNode, IGraphicInput, bool> func, GuiInputState returnState = GuiInputState.Layout)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((pipeline, n, input, baseAction) =>
            {
                var state = baseAction(pipeline);
                if (state != GuiInputState.None)
                {
                    // The child node has responded
                    return state;
                }

                switch (input.EventType)
                {
                    case GuiEventTypes.KeyUp:
                        try
                        {
                            return func(n, input) ? returnState : GuiInputState.None;
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                            return GuiInputState.None;
                        }
                    default:
                        return GuiInputState.None;
                }
            });
        }

        return node;
    }

    /// <summary>
    /// Executes an action when a key is pressed down.
    /// </summary>
    [Obsolete]
    public static ImGuiNode OnKeyDown(this ImGuiNode node, Action<ImGuiNode, IGraphicInput> action)
    {
        var input = node.Gui.Input;

        if (/*node.IsMouseInRect && */input.EventType == GuiEventTypes.KeyDown)
        {
            action(node, input);
        }

        return node;
    }

    /// <summary>
    /// Executes an action when a key is released.
    /// </summary>
    [Obsolete]
    public static ImGuiNode OnKeyUp(this ImGuiNode node, Action<ImGuiNode, IGraphicInput> action)
    {
        var input = node.Gui.Input;

        if (/*node.IsMouseInRect && */input.EventType == GuiEventTypes.KeyUp)
        {
            action(node, input);
        }

        return node;
    }

    #endregion

    #region Menu

    /// <summary>
    /// Initializes a context menu on the node that appears on right-click.
    /// </summary>
    public static ImGuiNode InitMenu(this ImGuiNode node, RootMenuCommand menu, object? sender = null, Func<IEnumerable<object>?>? selFunc = null)
    {
        if (node.IsInitializing)
        {
            var gui = node.Gui;

            (gui.Context as IGraphicContextMenu)?.RegisterContextMenu(menu);

            node.InitInputMouseUp((n, btn) =>
            {
                if (btn == GuiMouseButtons.Right)
                {
                    var sel = selFunc?.Invoke();
                    menu.ApplySender(sender);

                    (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu, sel);
                }

                return GuiInputState.None;
            });
        }

        return node;
    }

    #endregion
}