using Suity.Collections;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for core ImGui node operations including classes, themes, content management, drag-drop, and function assignment.
/// </summary>
public static class GuiCoreExtensions
{
    #region Class & Theme

    /// <summary>
    /// Sets the CSS-like classes on a node, replacing existing classes.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="classes">The class names to set.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetClass(this ImGuiNode node, params string[] classes)
    {
        var c2 = (node.Classes ?? []).Union(classes).ToArray();
        if (!ArrayHelper.ArrayEquals(node.Classes, c2))
        {
            node.Classes = classes;
        }
        return node;
    }

    /// <summary>
    /// Adds classes to a node without removing existing ones.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="classes">The class names to add.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode UnionClass(this ImGuiNode node, params string[] classes)
    {
        string[] value = node.Classes ?? [];
        node.Classes = value.Union(classes).ToArray();
        return node;
    }

    /// <summary>
    /// Swaps one class for another on a node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="removeClass">The class name to remove.</param>
    /// <param name="addClass">The class name to add.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SwapClass(this ImGuiNode node, string removeClass, string addClass)
    {
        string[] value = node.Classes ?? [];
        bool contains = value.Contains(addClass);
        if (contains)
        {
            int removeIndex = Array.IndexOf(value, removeClass);
            if (removeIndex > 0)
            {
                node.Classes = value.Where((s, idx) => idx != removeIndex).ToArray();
            }
        }
        else
        {
            IEnumerable<string> newAry = value;
            int removeIndex = Array.IndexOf(value, removeClass);
            if (removeIndex > 0)
            {
                newAry = value.Where((s, idx) => idx != removeIndex);
            }
            node.Classes = newAry.ConcatOne(addClass).ToArray();
        }
        return node;
    }

    /// <summary>
    /// Sets classes on a node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="classes">The class names to set.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitClass(this ImGuiNode node, params string[] classes)
    {
        if (node.IsInitializing)
        {
            node.Classes = classes;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Adds classes to a node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="classes">The class names to add.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitUnionClass(this ImGuiNode node, params string[] classes)
    {
        if (node.IsInitializing)
        {
            node.UnionClass(classes);
        }
        return node;
    }

    /// <summary>
    /// Swaps a class on a node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="removeClass">The class name to remove.</param>
    /// <param name="addClass">The class name to add.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitSwapClass(this ImGuiNode node, string removeClass, string addClass)
    {
        if (node.IsInitializing)
        {
            node.SwapClass(removeClass, addClass);
        }
        return node;
    }

    /// <summary>
    /// Sets the theme for a node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetTheme(this ImGuiNode node, ImGuiTheme theme)
    {
        node.Theme = theme;
        return node;
    }

    /// <summary>
    /// Sets the theme for a node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitTheme(this ImGuiNode node, ImGuiTheme theme)
    {
        if (node.IsInitializing)
        {
            node.Theme = theme;
        }
        return node;
    }

    /// <summary>
    /// Updates the pseudo state based on mouse-in status.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="mouseIn">Whether the mouse is currently over the node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetPseudoMouseIn(this ImGuiNode node, bool mouseIn)
    {
        if (mouseIn)
        {
            switch (node.Pseudo)
            {
                case ImGuiNode.PseudoActive:
                    node.Pseudo = ImGuiNode.PseudoActiveMouseIn;
                    break;
                case null:
                    node.Pseudo = ImGuiNode.PseudoMouseIn;
                    break;
            }
        }
        else
        {
            switch (node.Pseudo)
            {
                case ImGuiNode.PseudoActiveMouseIn:
                    node.Pseudo = ImGuiNode.PseudoActive;
                    break;
                case ImGuiNode.PseudoMouseIn:
                    node.Pseudo = null;
                    break;
            }
        }
        return node;
    }

    /// <summary>
    /// Updates the pseudo state based on active status.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="active">Whether the node is in an active state.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetPseudoActive(this ImGuiNode node, bool active)
    {
        if (active)
        {
            node.Pseudo = node.Pseudo switch
            {
                ImGuiNode.PseudoMouseIn => ImGuiNode.PseudoActiveMouseIn,
                ImGuiNode.PseudoMouseDown => ImGuiNode.PseudoActiveMouseDown,
                _ => ImGuiNode.PseudoActive,
            };
        }
        else
        {
            switch (node.Pseudo)
            {
                case ImGuiNode.PseudoActiveMouseIn:
                    node.Pseudo = ImGuiNode.PseudoMouseIn;
                    break;
                case ImGuiNode.PseudoActiveMouseDown:
                    node.Pseudo = ImGuiNode.PseudoMouseDown;
                    break;
                case ImGuiNode.PseudoActive:
                    node.Pseudo = null;
                    break;
            }
        }
        return node;
    }

    /// <summary>
    /// Sets whether pseudo style changes affect child nodes.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="affectsChildren">Whether pseudo styles propagate to children.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetPseudoAffectsChildren(this ImGuiNode node, bool affectsChildren)
    {
        node.PseudoAffectsChildren = affectsChildren;
        return node;
    }

    /// <summary>
    /// Sets whether pseudo style changes affect child nodes only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="affectsChildren">Whether pseudo styles propagate to children.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitPseudoAffectsChildren(this ImGuiNode node, bool affectsChildren)
    {
        if (node.IsInitializing)
        {
            node.PseudoAffectsChildren = affectsChildren;
        }
        return node;
    }

    #endregion

    #region Basics

    /// <summary>
    /// Sets the type name of a node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="typeName">The type name to assign.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitTypeName(this ImGuiNode node, string typeName)
    {
        if (node.IsInitializing)
        {
            node.TypeName = typeName;
        }
        return node;
    }

    /// <summary>
    /// Sets whether a node is enabled only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="enabled">Whether the node should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitEnabled(this ImGuiNode node, bool enabled)
    {
        if (node.IsInitializing)
        {
            node.IsDisabled = !enabled;
        }
        return node;
    }

    /// <summary>
    /// Sets whether a node is enabled.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="enabled">Whether the node should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetEnabled(this ImGuiNode node, bool enabled)
    {
        node.IsDisabled = !enabled;
        return node;
    }

    /// <summary>
    /// Sets whether a node is disabled only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="disabled">Whether the node should be disabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitDisabled(this ImGuiNode node, bool disabled)
    {
        if (node.IsInitializing)
        {
            node.IsDisabled = disabled;
        }
        return node;
    }

    /// <summary>
    /// Sets whether a node is disabled.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="disabled">Whether the node should be disabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetDisabled(this ImGuiNode node, bool disabled)
    {
        node.IsDisabled = disabled;
        return node;
    }

    /// <summary>
    /// Sets whether a node is read-only only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="readOnly">Whether the node should be read-only.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitReadonly(this ImGuiNode node, bool readOnly)
    {
        if (node.IsInitializing)
        {
            node.IsReadOnly = readOnly;
        }
        return node;
    }

    /// <summary>
    /// Sets whether a node is read-only.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="readOnly">Whether the node should be read-only.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetReadonly(this ImGuiNode node, bool readOnly)
    {
        node.IsReadOnly = readOnly;
        return node;
    }

    /// <summary>
    /// Sets whether a node overrides disabled style inheritance only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="reverse">Whether to override the disabled style inheritance.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitOverrideDisabled(this ImGuiNode node, bool reverse)
    {
        if (node.IsInitializing)
        {
            node.OverrideDisabled = reverse;
        }
        return node;
    }

    /// <summary>
    /// Sets whether a node overrides disabled style inheritance.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="reverse">Whether to override the disabled style inheritance.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetOverrideDisabled(this ImGuiNode node, bool reverse)
    {
        node.OverrideDisabled = reverse;
        return node;
    }

    /// <summary>
    /// Marks this node and all ancestors as render dirty.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode MarkRenderDirtyInHierarchy(this ImGuiNode node)
    {
        ImGuiNode? n = node;
        while (n != null)
        {
            n.MarkRenderDirty();
            n = n.Parent;
        }
        return node;
    }

    /// <summary>
    /// Sets compact mode only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="compact">Whether compact mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitCompact(this ImGuiNode node, bool compact)
    {
        if (node.IsInitializing)
        {
            node.IsCompact = compact;
        }
        return node;
    }

    /// <summary>
    /// Sets compact mode.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="compact">Whether compact mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetCompact(this ImGuiNode node, bool compact)
    {
        node.IsCompact = compact;
        return node;
    }

    /// <summary>
    /// Sets overlapped mode only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="overlapped">Whether overlapped mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitOverlapped(this ImGuiNode node, bool overlapped)
    {
        if (node.IsInitializing)
        {
            node.IsOverlapped = overlapped;
        }
        return node;
    }

    /// <summary>
    /// Sets overlapped mode.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="overlapped">Whether overlapped mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetOverlapped(this ImGuiNode node, bool overlapped)
    {
        node.IsOverlapped = overlapped;
        return node;
    }

    /// <summary>
    /// Sets the scale transform only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="scale">The scale factor to apply.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitScale(this ImGuiNode node, float scale)
    {
        if (node.IsInitializing)
        {
            node.Transform = new GuiTransform(0, 0, scale);
        }
        return node;
    }

    /// <summary>
    /// Sets the scale transform.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="scale">The scale factor to apply.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetScale(this ImGuiNode node, float scale)
    {
        node.Transform = new GuiTransform(0, 0, scale);
        return node;
    }

    #endregion

    #region Contents

    /// <summary>
    /// Starts drawing inside the content of this node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode BeginContent(this ImGuiNode node)
    {
        node.Gui.BeginContent(node);
        return node;
    }

    /// <summary>
    /// Executes content drawing within this node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="contentAction">The action to execute for content drawing.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnContent(this ImGuiNode node, Action contentAction)
    {
        node.Gui.OnContent(node, contentAction);
        return node;
    }

    /// <summary>
    /// Executes content drawing within this node with node access.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="contentAction">The action to execute for content drawing, receiving the node as parameter.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnContent(this ImGuiNode node, Action<ImGuiNode> contentAction)
    {
        node.Gui.OnContent(node, contentAction);
        return node;
    }

    /// <summary>
    /// Executes content drawing conditionally based on synchronization state.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="contentAction">The action to execute for content drawing.</param>
    /// <param name="layout">Whether to perform layout when skipping full sync.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnPartialContent(this ImGuiNode node, Action contentAction, bool layout = true)
    {
        var ctrl = node.Gui.ControllingNode;
        if (node.Gui.LastInputState == GuiInputState.FullSync || 
            ctrl == node ||
            ctrl?.ContainsParent(node) == true || 
            node.IsInitializing)
        {
            node.Gui.OnContent(node, contentAction);
        }
        else
        {
            if (layout)
            {
                node.Gui.OnContent(node, () => node.Gui.LayoutCurrentContents());
            }
            else
            {
                node.Gui.OnContent(node, () => node.Gui.PassCurrentContents());
            }
        }

        //if (node.Gui.LastInputState == GuiInputState.FullSync || node.IsInitializing)
        //{
        //    node.Gui.OnContent(node, contentAction);
        //}
        //else
        //{
        //    node.Gui.OnContent(node, () => node.Gui.LayoutContents());
        //}

        return node;
    }

    /// <summary>
    /// Executes content drawing conditionally based on synchronization state with node access.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="contentAction">The action to execute for content drawing, receiving the node as parameter.</param>
    /// <param name="layout">Whether to perform layout when skipping full sync.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnPartialContent(this ImGuiNode node, Action<ImGuiNode> contentAction, bool layout = true)
    {
        var ctrl = node.Gui.ControllingNode;
        if (node.Gui.LastInputState == GuiInputState.FullSync || 
            ctrl == node || 
            ctrl?.ContainsParent(node) == true ||
            node.IsInitializing)
        {
            node.Gui.OnContent(node, contentAction);
        }
        else
        {
            if (layout)
            {
                node.Gui.OnContent(node, () => node.Gui.LayoutCurrentContents());
            }
            else
            {
                node.Gui.OnContent(node, () => node.Gui.PassCurrentContents());
            }
        }

        //if (node.Gui.LastInputState == GuiInputState.FullSync || node.IsInitializing)
        //{
        //    node.Gui.OnContent(node, contentAction);
        //}
        //else
        //{
        //    node.Gui.OnContent(node, () => node.Gui.LayoutContents());
        //}

        return node;
    }

    /// <summary>
    /// Lays out the contents of this node only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="fit">Whether to fit the node before and after layout.</param>
    /// <param name="align">Whether to align contents during layout.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutContents(this ImGuiNode node, bool fit = true, bool align = true)
    {
        if (node.IsInitializing)
        {
            LayoutContents(node, fit, align);
        }
        return node;
    }

    /// <summary>
    /// Lays out the contents of this node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="fit">Whether to fit the node before and after layout.</param>
    /// <param name="align">Whether to align contents during layout.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode LayoutContents(this ImGuiNode node, bool fit = true, bool align = true)
    {
        if (fit)
        {
            node.Fit();
        }
        node.Gui.OnContent(node, () => 
        {
            node.Gui.LayoutCurrentContents(fit, align);
        }, true);
        if (fit)
        {
            node.Fit();
        }
        return node;
    }

    /// <summary>
    /// Passes (skips) the content of this node without updating.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode PassContents(this ImGuiNode node)
    {
        node.Gui.OnContent(node, () =>
        {
            node.Gui.PassCurrentContents();
        }, false);
        return node;
    }

    /// <summary>
    /// Applies a content template to this node.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="template">The content template to apply.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnContentTemplate(this ImGuiNode node, ContentTemplate template)
    {
        var gui = node.Gui;
        node.Gui.OnContent(node, () => template(node));
        return node;
    }

    /// <summary>
    /// Applies a typed content template to this node.
    /// </summary>
    /// <typeparam name="T">The type of the template value.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="template">The typed content template to apply.</param>
    /// <param name="value">The value to pass to the template.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnContentTemplate<T>(this ImGuiNode node, ContentTemplate<T> template, T value)
    {
        var gui = node.Gui;
        node.Gui.OnContent(node, () => template(node, value));
        return node;
    }

    #endregion

    #region Drag

    /// <summary>
    /// Handles drag start events by calling the provided function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The function that returns the drag data object.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnDragStart(this ImGuiNode node, Func<ImGuiNode, object?> func)
    {
        var value = node.GetValue<GuiDraggableValue>();
        if (value is { DragRequest: true })
        {
            value.DragRequest = false;
            var obj = func(node);
            if (obj is { })
            {
                (node.Gui.Context as IGraphicDragDrop)?.DoDragDrop(obj);
            }
        }
        return node;
    }

    /// <summary>
    /// Handles drag over events.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="action">The action to execute when a drag over event occurs.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnDragOver(this ImGuiNode node, Action<IDragEvent> action)
    {
        var input = node.Gui.Input;
        if (node.IsMouseIn &&
            input.EventType == GuiEventTypes.DragOver &&
            input.DragEvent is { } dragEvent)
        {
            action(dragEvent);
            input.Handled = dragEvent.Handled;
        }
        return node;
    }

    /// <summary>
    /// Handles drag drop events.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="action">The action to execute when a drag drop event occurs.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnDragDrop(this ImGuiNode node, Action<IDragEvent> action)
    {
        var input = node.Gui.Input;
        if (node.IsMouseIn &&
            input.EventType == GuiEventTypes.DragDrop &&
            input.DragEvent is { } dragEvent)
        {
            action(dragEvent);
            node.Gui.QueueAction(() =>
            {
                node.MarkRenderDirty();
                node.QueueRefresh();
            });
        }
        return node;
    }

    #endregion

    #region Update

    /// <summary>
    /// Registers an update callback that runs every frame.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="update">The update action to execute each frame.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnUpdate(this ImGuiNode node, Action<ImGuiNode> update)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((p, node2, input, baseAction) =>
            {
                var state = baseAction(p);
                update(node2);
                ImGui.MergeState(ref state, GuiInputState.KeepListening);
                return state;
            });
            node.Gui.AddTimerNode(node);
        }
        return node;
    }

    /// <summary>
    /// Registers an update callback that returns an input state.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="update">The update function that returns an input state.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnUpdate(this ImGuiNode node, Func<ImGuiNode, GuiInputState> update)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain((p, node2, input, baseAction) =>
            {
                var state = baseAction(p);
                var state2 = update(node2);
                ImGui.MergeState(ref state, state2);
                return state;
            });
            node.Gui.AddTimerNode(node);
        }
        return node;
    }

    #endregion

    #region Edit

    /// <summary>
    /// Executes an action when the node has been edited.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="action">The action to execute when the node is edited.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode OnEdited(this ImGuiNode node, Action<ImGuiNode> action)
    {
        if (node.IsEdited)
        {
            action.Invoke(node);
        }
        return node;
    }

    #endregion

    #region ToolTip

    /// <summary>
    /// Sets up tooltip with static text only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="toolTips">The static tooltip text to display.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitToolTips(this ImGuiNode node, string toolTips)
    {
        if (node.IsInitializing)
        {
            node.GetOrCreateValue<GuiToolTipValue>().ToolTipText = toolTips;
            node.InitInputFunctionChain(ToolTipInput);
        }
        return node;
    }

    /// <summary>
    /// Sets up tooltip with a getter function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="getter">The function that returns the tooltip text dynamically.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitToolTips(this ImGuiNode node, Func<string> getter)
    {
        if (node.IsInitializing)
        {
            node.GetOrCreateValue<GuiToolTipValue>().ToolTipGetter = getter;
            node.InitInputFunctionChain(ToolTipInput);
        }
        return node;
    }

    /// <summary>
    /// Sets up tooltip with static text.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="toolTips">The static tooltip text to display.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetToolTips(this ImGuiNode node, string toolTips)
    {
        node.GetOrCreateValue<GuiToolTipValue>().ToolTipText = toolTips;
        node.InitInputFunctionChain(ToolTipInput);
        return node;
    }

    /// <summary>
    /// Sets up tooltip with a getter function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="getter">The function that returns the tooltip text dynamically.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetToolTips(this ImGuiNode node, Func<string> getter)
    {
        node.GetOrCreateValue<GuiToolTipValue>().ToolTipGetter = getter;
        node.InitInputFunctionChain(ToolTipInput);
        return node;
    }

    /// <summary>
    /// Sets up tooltip with localized static text.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="toolTips">The localization key for the tooltip text.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetToolTipsL(this ImGuiNode node, string toolTips)
    {
        node.GetOrCreateValue<GuiToolTipValue>().ToolTipText = L(toolTips);
        node.InitInputFunctionChain(ToolTipInput);
        return node;
    }

    private static GuiInputState ToolTipInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (input.EventType == GuiEventTypes.ToolTip)
        {
            if (node.GetValue<GuiToolTipValue>() is { } v)
            {
                string? toolTip = null;
                try
                {
                    toolTip = v.ToolTipGetter?.Invoke() ?? v.ToolTipText;
                }
                catch (Exception err)
                {
                    err.LogError();
                }
                if (!string.IsNullOrWhiteSpace(toolTip))
                {
                    var rect = node.GlobalRect;
                    (node.Gui.Context as IGraphicToolTip)?.ShowToolTip(toolTip, (int)rect.X, (int)rect.Bottom);
                    input.Handled = true;
                }
            }
        }
        return state;
    }

    #endregion

    #region OptionActive

    /// <summary>
    /// Sets this node as the active option within its parent's optional group.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetOptionActive(this ImGuiNode node)
    {
        if (node.Parent?.GetOrCreateValue<GuiOptionalValue>() is { } value)
        {
            value.ActiveNodeId = node.Id;
            node.Pseudo = ImGuiNode.PseudoActive;
        }
        return node;
    }

    /// <summary>
    /// Sets this node as the active option only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="isActive">Whether this node should be set as active.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitOptionActive(this ImGuiNode node, bool isActive = true)
    {
        if (node.IsInitializing && isActive)
        {
            if (node.Parent?.GetOrCreateValue<GuiOptionalValue>() is { } value)
            {
                value.ActiveNodeId = node.Id;
                node.Pseudo = ImGuiNode.PseudoActive;
            }
        }
        return node;
    }

    #endregion

    #region Floating

    /// <summary>
    /// Sets floating layout mode only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="floating">Whether floating mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFloating(this ImGuiNode node, bool floating)
    {
        if (node.IsInitializing)
        {
            node.IsFloating = floating;
        }
        return node;
    }

    /// <summary>
    /// Sets floating layout mode.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="floating">Whether floating mode should be enabled.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetFloating(this ImGuiNode node, bool floating)
    {
        node.IsFloating = floating;
        return node;
    }

    #endregion

    #region Value fluent

    /// <summary>
    /// Sets a value only during initialization.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitValueFluent<T>(this ImGuiNode node, T value) where T : class
    {
        if (node.IsInitializing)
        {
            node.SetValue(value);
        }
        return node;
    }

    /// <summary>
    /// Creates and configures a value only during initialization.
    /// </summary>
    /// <typeparam name="T">The type of the value to create.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="action">The action to configure the created value.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitValueFluent<T>(this ImGuiNode node, Action<T> action) where T : class, new()
    {
        if (node.IsInitializing)
        {
            var value = node.GetOrCreateValue<T>();
            action(value);
        }
        return node;
    }

    /// <summary>
    /// Sets a value on the node.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetValueFluent<T>(this ImGuiNode node, T value) where T : class
    {
        node.SetValue(value);
        return node;
    }

    /// <summary>
    /// Gets or creates a value and configures it.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or create.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="action">The action to configure the value.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetValueFluent<T>(this ImGuiNode node, Action<T> action) where T : class, new()
    {
        var value = node.GetOrCreateValue<T>();
        action(value);
        return node;
    }

    /// <summary>
    /// Sets a new value only during initialization.
    /// </summary>
    /// <typeparam name="T">The type of the value to create.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitValue<T>(this ImGuiNode node) where T : class, new()
    {
        if (node.IsInitializing)
        {
            node.SetValue(new T());
        }
        return node;
    }

    /// <summary>
    /// Sets a value only during initialization.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitValue<T>(this ImGuiNode node, T value) where T : class
    {
        if (node.IsInitializing)
        {
            node.SetValue(value);
        }
        return node;
    }

    /// <summary>
    /// Sets a value from a factory function only during initialization.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="factory">The factory function that creates the value.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitValue<T>(this ImGuiNode node, Func<T> factory) where T : class
    {
        if (node.IsInitializing)
        {
            var value = factory();
            if (value is { })
            {
                node.SetValue(value);
            }
        }
        return node;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Sets the base input function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The input function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitInputFunction(this ImGuiNode node, InputFunction func)
    {
        if (node.IsInitializing)
        {
            node.BaseInputFunction = func;
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base input function by type name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitInputFunction(this ImGuiNode node)
    {
        if (node.IsInitializing && node.TypeName is { } funcName)
        {
            node.BaseInputFunction = ImGuiExternal._external.ResolveInputFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base input function by name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the input function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitInputFunction(this ImGuiNode node, string funcName)
    {
        if (node.IsInitializing)
        {
            node.BaseInputFunction = ImGuiExternal._external.ResolveInputFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Sets the base input function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The input function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetInputFunction(this ImGuiNode node, InputFunction func)
    {
        node.BaseInputFunction = func;
        return node;
    }

    /// <summary>
    /// Resolves and sets the base input function by type name.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetInputFunction(this ImGuiNode node)
    {
        if (node.TypeName is { } funcName)
        {
            node.BaseInputFunction = ImGuiExternal._external.ResolveInputFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base input function by name.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the input function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetInputFunction(this ImGuiNode node, string funcName)
    {
        node.BaseInputFunction = ImGuiExternal._external.ResolveInputFunction(node, funcName);
        return node;
    }

    /// <summary>
    /// Sets the base layout function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The layout function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutFunction(this ImGuiNode node, LayoutFunction func)
    {
        if (node.IsInitializing)
        {
            node.BaseLayoutFunction = func;
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base layout function by type name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutFunction(this ImGuiNode node)
    {
        if (node.IsInitializing && node.TypeName is { } funcName)
        {
            node.BaseLayoutFunction = ImGuiExternal._external.ResolveLayoutFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base layout function by name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the layout function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutFunction(this ImGuiNode node, string funcName)
    {
        if (node.IsInitializing)
        {
            node.BaseLayoutFunction = ImGuiExternal._external.ResolveLayoutFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Sets the base layout function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The layout function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetLayoutFunction(this ImGuiNode node, LayoutFunction func)
    {
        node.BaseLayoutFunction = func;
        return node;
    }

    /// <summary>
    /// Resolves and sets the base layout function by type name.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetLayoutFunction(this ImGuiNode node)
    {
        if (node.TypeName is { } funcName)
        {
            node.BaseLayoutFunction = ImGuiExternal._external.ResolveLayoutFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base layout function by name.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the layout function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetLayoutFunction(this ImGuiNode node, string funcName)
    {
        node.BaseLayoutFunction = ImGuiExternal._external.ResolveLayoutFunction(node, funcName);
        return node;
    }

    /// <summary>
    /// Sets the base fit function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The fit function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFitFunction(this ImGuiNode node, FitFunction func)
    {
        if (node.IsInitializing)
        {
            node.BaseFitFunction = func;
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base fit function by type name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFitFunction(this ImGuiNode node)
    {
        if (node.IsInitializing && node.TypeName is { } funcName)
        {
            node.BaseFitFunction = ImGuiExternal._external.ResolveFitFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base fit function by name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the fit function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFitFunction(this ImGuiNode node, string funcName)
    {
        if (node.IsInitializing)
        {
            node.BaseFitFunction = ImGuiExternal._external.ResolveFitFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Sets the base fit function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The fit function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetFitFunction(this ImGuiNode node, FitFunction func)
    {
        node.BaseFitFunction = func;
        return node;
    }

    /// <summary>
    /// Resolves and sets the base fit function by type name.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetFitFunction(this ImGuiNode node)
    {
        if (node.TypeName is { } funcName)
        {
            node.BaseFitFunction = ImGuiExternal._external.ResolveFitFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base fit function by name.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the fit function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetFitFunction(this ImGuiNode node, string funcName)
    {
        node.BaseFitFunction = ImGuiExternal._external.ResolveFitFunction(node, funcName);
        return node;
    }

    /// <summary>
    /// Sets the base render function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The render function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitRenderFunction(this ImGuiNode node, RenderFunction func)
    {
        if (node.IsInitializing)
        {
            node.BaseRenderFunction = func;
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base render function by type name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitRenderFunction(this ImGuiNode node)
    {
        if (node.IsInitializing && node.TypeName is { } funcName)
        {
            node.BaseRenderFunction = ImGuiExternal._external.ResolveRenderFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base render function by name only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the render function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitRenderFunction(this ImGuiNode node, string funcName)
    {
        if (node.IsInitializing)
        {
            node.BaseRenderFunction = ImGuiExternal._external.ResolveRenderFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Sets the base render function.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The render function to set as base.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetRenderFunction(this ImGuiNode node, RenderFunction func)
    {
        node.BaseRenderFunction = func;
        return node;
    }

    /// <summary>
    /// Resolves and sets the base render function by type name.
    /// </summary>
    /// <param name="node">The target ImGui node. The type name is taken from <see cref="ImGuiNode.TypeName"/>.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetRenderFunction(this ImGuiNode node)
    {
        if (node.TypeName is { } funcName)
        {
            node.BaseRenderFunction = ImGuiExternal._external.ResolveRenderFunction(node, funcName);
        }
        return node;
    }

    /// <summary>
    /// Resolves and sets the base render function by name.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the render function to resolve.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode SetRenderFunction(this ImGuiNode node, string funcName)
    {
        node.BaseRenderFunction = ImGuiExternal._external.ResolveRenderFunction(node, funcName);
        return node;
    }

    /// <summary>
    /// Resolves and chains an input function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the input function to resolve and chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitInputFunctionChain(this ImGuiNode node, string funcName)
    {
        var func = ImGuiExternal._external.ResolveInputFunction(node, funcName);
        if (func is { })
        {
            node.InitInputFunctionChain(func);
        }
        return node;
    }

    /// <summary>
    /// Resolves and chains a layout function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the layout function to resolve and chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutFunctionChain(this ImGuiNode node, string funcName)
    {
        var func = ImGuiExternal._external.ResolveLayoutFunction(node, funcName);
        if (func is { })
        {
            node.InitLayoutFunctionChain(func);
        }
        return node;
    }

    /// <summary>
    /// Resolves and chains a fit function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the fit function to resolve and chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFitFunctionChain(this ImGuiNode node, string funcName)
    {
        var func = ImGuiExternal._external.ResolveFitFunction(node, funcName);
        if (func is { })
        {
            node.InitFitFunctionChain(func);
        }
        return node;
    }

    /// <summary>
    /// Resolves and chains a render function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="funcName">The name of the render function to resolve and chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitRenderFunctionChain(this ImGuiNode node, string funcName)
    {
        var func = ImGuiExternal._external.ResolveRenderFunction(node, funcName);
        if (func is { })
        {
            node.InitRenderFunctionChain(func);
        }
        return node;
    }

    /// <summary>
    /// Chains an input function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The input function to chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitInputFunctionChain(this ImGuiNode node, InputFunction func)
    {
        if (node.IsInitializing)
        {
            ImGuiExternal._external.SetInputFunctionChain(node, func);
        }
        return node;
    }

    /// <summary>
    /// Chains a layout function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The layout function to chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitLayoutFunctionChain(this ImGuiNode node, LayoutFunction func)
    {
        if (node.IsInitializing)
        {
            ImGuiExternal._external.SetLayoutFunctionChain(node, func);
        }
        return node;
    }

    /// <summary>
    /// Chains a fit function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The fit function to chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitFitFunctionChain(this ImGuiNode node, FitFunction func)
    {
        if (node.IsInitializing)
        {
            ImGuiExternal._external.SetFitFunctionChain(node, func);
        }
        return node;
    }

    /// <summary>
    /// Chains a render function only during initialization.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="func">The render function to chain.</param>
    /// <returns>The same node for method chaining.</returns>
    public static ImGuiNode InitRenderFunctionChain(this ImGuiNode node, RenderFunction func)
    {
        if (node.IsInitializing)
        {
            ImGuiExternal._external.SetRenderFunctionChain(node, func);
        }
        return node;
    }

    #endregion
}
