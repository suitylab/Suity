using Suity.Collections;
using Suity.Helpers;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Default input system for ImGui, handling mouse, keyboard, and drag-drop input for all built-in controls.
/// </summary>
public class ImGuiInputSystemBK : ImGuiInputSystem
{
    /// <summary>
    /// Input state used for splitter synchronization.
    /// </summary>
    public const GuiInputState SplitterSync = GuiInputState.PartialSync;


    private static ImGuiInputSystemBK? _instance;

    /// <summary>
    /// Gets the singleton instance of the input system.
    /// </summary>
    public static ImGuiInputSystemBK Instance => _instance ??= new ImGuiInputSystemBK();

    private readonly Dictionary<string, InputFunction> _functions = [];

    /// <summary>
    /// Initializes a new input system and registers all built-in input functions.
    /// </summary>
    public ImGuiInputSystemBK()
    {
        _functions[Click] = ClickInput;
        _functions[MouseInRender] = MouseInRenderInput;
        _functions[MouseInRefresh] = MouseInRefreshInput;
        _functions[Hover] = HoverInput;
        _functions[KeyDown] = KeyDownInput;
        _functions[KeyUp] = KeyUpInput;
        _functions[Resizer] = ResizerInput;
        _functions[ResizerFitter] = ResizerFitterInput;
        _functions[GroupedResizer] = GroupedResizerInput;
        _functions[TreeView] = TreeViewInput;
        _functions[TreeNode] = TreeNodeInput;
        _functions[SimpleStringInput] = SimpleStringInputFunction;
        _functions[DoubleClickStringInput] = DoubleClickStringInputFunction;
        _functions[DragDrop] = DragDropInput;
        _functions[Viewport] = ViewportInput;

        _functions[nameof(GuiButtonExtensions.Button)] = ButtonInput;
        _functions[nameof(GuiButtonExtensions.DropDownButton)] = DropDownButtonInput;
        _functions[nameof(GuiButtonExtensions.ExpandButton)] = ExpandButtonInput;
        _functions[nameof(GuiToggleExtensions.CheckBox)] = CheckBoxInput;
        _functions[nameof(GuiToggleExtensions.ToggleButton)] = CheckBoxInput;
        _functions[nameof(GuiButtonExtensions.SwitchButton)] = SwitchButtonInput;
        _functions[nameof(GuiPanelExtensions.ExpandablePanel)] = ExpandableInput;
        _functions[nameof(GuiScrollableExtensions.ScrollableFrame)] = ScrollableFrameInput;
        _functions[nameof(GuiTextInputExtensions.StringInput)] = TextInput;
        _functions[nameof(GuiTextInputExtensions.TextAreaInput)] = TextInput;
        _functions[nameof(GuiTextInputExtensions.NumericInput)] = NumericInput;
        _functions[nameof(GuiCommonExtensions.Viewport)] = ViewportInput;
    }

    /// <inheritdoc/>
    public override InputFunction? GetInputFunction(string name)
    {
        var func = _functions.GetValueSafe(name);

        if (func is null)
        {
            Debug.WriteLine($"{nameof(ImGuiInputSystemBK)} function not found : {name}");
        }

        return func;
    }

    private GuiInputState ClickInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        // var childState = childNodesAction(pipeline);
        // if (childState != GuiState.None)
        // {
        //     return childState;
        // }
        //
        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState MouseInRenderInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        GuiInputState state;

        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                node.SetPseudoMouseIn(true);
                node.MarkRenderDirty();
                state = GuiInputState.Render;
                //Debug.WriteLine($"mouse in : {node.FullName}");
                break;

            case GuiEventTypes.MouseOut:
                node.SetPseudoMouseIn(false);
                node.MarkRenderDirty();
                state = GuiInputState.Render;
                //Debug.WriteLine($"mouse out : {node.FullName}");
                break;

            default:
                var childState = baseAction(pipeline);
                if (childState != GuiInputState.None)
                {
                    // Tests show forced setting is unnecessary and may interfere with custom logic
                    //node.Pseudo = ImGuiNode.PseudoMouseIn;
                    //node.MarkRenderDirty();
                    return childState;
                }

                state = GuiInputState.None;
                break;
        }

        ImGui.MergeState(ref state, baseAction(pipeline));

        return state;
    }

    private GuiInputState MouseInRefreshInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        GuiInputState state;

        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                node.SetPseudoMouseIn(true);
                node.MarkRenderDirty();
                state = GuiInputState.FullSync;
                break;

            case GuiEventTypes.MouseOut:
                node.SetPseudoMouseIn(false);
                node.MarkRenderDirty();
                state = GuiInputState.FullSync;
                break;

            default:
                var childState = baseAction(pipeline);
                if (childState != GuiInputState.None)
                {
                    // Tests show forced setting is unnecessary and may interfere with custom logic
                    //node.Pseudo = ImGuiNode.PseudoMouseIn;
                    //node.MarkRenderDirty();
                    return childState;
                }

                state = GuiInputState.None;
                break;
        }

        ImGui.MergeState(ref state, baseAction(pipeline));

        return state;
    }

    private GuiInputState HoverInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.HoverIn:
                node.SetPseudoMouseIn(true);
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            case GuiEventTypes.HoverOut:
                node.SetPseudoMouseIn(false);
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            default:
                return baseAction(pipeline);
        }
    }

    private GuiInputState KeyDownInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state != GuiInputState.None)
        {
            // Child node has already responded
            return state;
        }

        return input.EventType switch
        {
            GuiEventTypes.KeyDown => GuiInputState.FullSync,
            _ => GuiInputState.None,
        };
    }

    private GuiInputState KeyUpInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state != GuiInputState.None)
        {
            // Child node has already responded
            return state;
        }

        return input.EventType switch
        {
            GuiEventTypes.KeyUp => GuiInputState.FullSync,
            _ => GuiInputState.None,
        };
    }

    private GuiInputState ButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState ExpandButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        var value = node.GetOrCreateValue<GuiExpandableValue>();

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();

                if (node.GetIsClicked(true))
                {
                    value.Expanded = !value.Expanded;
                }

                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState DropDownButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && !node.IsReadOnly && node.Gui.Context is IGraphicDropDownEdit dropDownEdit)
                {
                    var value = node.GetValue<GuiDropDownValue>();
                    if (value?.Items.Count > 0)
                    {
                        var rect = node.GlobalRect;
                        var dropDownRect = new RectangleF(rect.X, rect.Bottom, rect.Width, value.DropDownHeight ?? 100);
                        if (dropDownRect.Width < 100)
                        {
                            dropDownRect.Width = 100;
                        }
                        dropDownEdit.ShowComboBoxDropDown(dropDownRect.ToInt(), value.Items.OfType<object>(), value.SelectedItem, sel =>
                        {
                            if (sel is GuiDropDownItem item && node.IsInUsage && !node.IsReadOnly && node.GetValue<GuiDropDownValue>() is { } v)
                            {
                                v.SelectedItem = item;
                                node.Text = sel?.ToString() ?? String.Empty;
                                node.IsEdited = true;
                                node.QueueRefresh();
                            }
                        });
                    }
                }
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState CheckBoxInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        var value = node.GetValue<GuiToggleValue>();
        bool active = value?.Value == CheckState.Checked;

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = active ? ImGuiNode.PseudoActiveMouseDown : ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && !node.IsReadOnly)
                {
                    if (value is { })
                    {
                        switch (value.Value)
                        {
                            case CheckState.Unchecked:
                            case CheckState.Indeterminate:
                                value.Value = CheckState.Checked;
                                active = true;
                                break;

                            case CheckState.Checked:
                                value.Value = CheckState.Unchecked;
                                active = false;
                                break;
                        }

                        node.IsEdited = true;
                    }
                }

                node.Pseudo = active ? ImGuiNode.PseudoActiveMouseIn : ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = active ? ImGuiNode.PseudoActiveMouseIn : ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:

                node.Pseudo = active ? ImGuiNode.PseudoActive : null;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState SwitchButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        var selectedValue = node.Parent?.GetOrCreateValue<GuiOptionalValue>();
        if (selectedValue?.ActiveNodeId == node.Id)
        {
            node.Pseudo = ImGuiNode.PseudoActive;
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && selectedValue is { })
                {
                    if (node.Parent?.GetChildNode(selectedValue.ActiveNodeId) is { } otherNode)
                    {
                        otherNode.Pseudo = null;
                        node.MarkRenderDirty();
                    }

                    selectedValue.ActiveNodeId = node.Id;
                    node.Pseudo = ImGuiNode.PseudoActive;
                    node.Gui.SetCursor(GuiCursorTypes.Default);
                    node.MarkRenderDirty();
                }
                else
                {
                    node.Pseudo = ImGuiNode.PseudoMouseIn;
                    node.MarkRenderDirty();
                }

                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.Gui.SetCursor(GuiCursorTypes.Hand);
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.Gui.SetCursor(GuiCursorTypes.Default);
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState ExpandableInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                if (node.IsMouseInClickRect)
                {
                    node.Gui.SetCursor(GuiCursorTypes.Hand);
                    node.MarkRenderDirty(true);
                }
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.MarkRenderDirty(true);
                node.Gui.SetCursor(GuiCursorTypes.Default);
                return GuiInputState.Render;
        }

        var childState = childNodesAction(pipeline);
        if (childState != GuiInputState.None)
        {
            return GuiInputState.Render;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                if (node.IsMouseInClickRect)
                {
                    node.MarkRenderDirty(true);
                }
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked(true))
                {
                    GuiExpandableValue? value = node.GetStyle<GuiExpandableValue>();
                    if (value is { })
                    {
                        var duration = node.Theme.ExpandDuration;
                        if (duration > 0)
                        {
                            var ani = new ExpandAnimation(node, value, !value.Expanded, duration);
                            node.StartAnimation(ani);
                        }
                        else
                        {
                            value.Expanded = !value.Expanded;
                        }
                    }
                }
                return node.IsMouseInClickRect ? GuiInputState.FullSync : GuiInputState.None;

            default:
                return GuiInputState.None;
        }
    }

    private GuiInputState ScrollableFrameInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        var value = node.GetValue<GuiScrollableValue>();
        if (value == null)
        {
            return GuiInputState.None;
        }

        // Set scroll starting position before processing child nodes
        if (pipeline == GuiPipeline.BeginSync)
        {
            // This line may conflict with VirtualListData's SetInitialLayoutPosition execution.
            //node.SetInitialLayoutPosition(new PointF(-value.ScrollX, -value.ScrollY));
            return GuiInputState.None;
        }

        if (!(input.MouseLocation is { } pos))
        {
            return GuiInputState.None;
        }

        var cSize = value.ContentSize;
        var syncMode = value.SyncGui ? GuiInputState.PartialSync : GuiInputState.Layout;

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                // Detect if user clicked on vertical or horizontal scrollbar to start dragging
                if (input.GetMouseButtonDown(GuiMouseButtons.Left))
                {
                    var vRect = value.GetViewRect(node.GlobalInnerRect);

                    // Check vertical scrollbar hit
                    if (cSize.Height > vRect.Height)
                    {
                        var scrollRect = node.GetVerticalScrollBarRect(value);
                        var globalScrollRect = node.GlobalScaleRect(scrollRect);
                        if (globalScrollRect.Contains(pos))
                        {
                            value.VMouseDownPos = node.GlobalReverseScaleValue(pos.Y);
                            value.HMouseDownPos = null;
                            value.CurrentScrollBarRect = scrollRect;
                            node.SetIsControlling(true);
                            return syncMode;
                        }
                    }

                    // Check horizontal scrollbar hit
                    if (cSize.Width > vRect.Width)
                    {
                        var scrollRect = node.GetHorizontalScrollBarRect(value);
                        var globalScrollRect = node.GlobalScaleRect(scrollRect);
                        if (globalScrollRect.Contains(pos))
                        {
                            value.VMouseDownPos = null;
                            value.HMouseDownPos = node.GlobalReverseScaleValue(pos.X);
                            value.CurrentScrollBarRect = scrollRect;
                            node.SetIsControlling(true);
                            return syncMode;
                        }
                    }
                }

                break;

            case GuiEventTypes.MouseMove:
                // Handle scrollbar dragging - calculate offset and update scroll position
                if (input.GetMouseButtonDown(GuiMouseButtons.Left))
                {
                    if (value.VMouseDownPos.HasValue)
                    {
                        // Vertical scrollbar dragging
                        var vRect = value.GetViewRect(node.InnerRect);
                        var scrollRect = value.CurrentScrollBarRect;
                        float posY = node.GlobalReverseScaleValue(pos.Y);

                        float offset = posY - value.VMouseDownPos.Value;
                        scrollRect.Y += offset;
                        ScrollHelper.ClampSliderPositionV(ref scrollRect, vRect);

                        float rate = ScrollHelper.GetSliderRateV(vRect, scrollRect);
                        value.ScrollY = rate * (value.ContentSize.Height - vRect.Height);
                        value.ManualInput = true;
                        node.MarkRenderDirty();

                        return syncMode;
                    }
                    else if (value.HMouseDownPos.HasValue)
                    {
                        // Horizontal scrollbar dragging
                        var vRect = value.GetViewRect(node.InnerRect);
                        var scrollRect = value.CurrentScrollBarRect;
                        float posX = node.GlobalReverseScaleValue(pos.X);

                        float offset = posX - value.HMouseDownPos.Value;
                        scrollRect.X += offset;
                        ScrollHelper.ClampSliderPositionH(ref scrollRect, vRect);

                        float rate = ScrollHelper.GetSliderRateH(vRect, scrollRect);
                        value.ScrollX = rate * (value.ContentSize.Width - vRect.Width);
                        value.ManualInput = true;
                        node.MarkRenderDirty();

                        return syncMode;
                    }
                }

                break;

            case GuiEventTypes.MouseUp:
                // Release scrollbar control
                value.VMouseDownPos = null;
                value.HMouseDownPos = null;
                node.SetIsControlling(false);

                break;
        }

        var childState = childNodesAction(pipeline);
        if (childState != GuiInputState.None)
        {
            return childState;
        }

        // Handle mouse wheel scrolling with animation support
        if (input.EventType == GuiEventTypes.MouseWheel && node.IsMouseInRect)
        {
            value.HMouseDownPos = null;
            value.VMouseDownPos = null;

            var vRect = value.GetViewRect(node.InnerRect);
            float delta = node.Theme.ScrollDelta;

            // Vertical scrolling (without modifier keys)
            if (value.ScrollOrientation.IsVertical() && input.GetNoCompondKey())
            {
                if (cSize.Height > vRect.Height)
                {
                    if (!value.VScrollBarVisible)
                    {
                        value.VScrollBarVisible = true;
                        node.MarkRenderDirty();
                    }

                    float offset = input.MouseDelta * delta;
                    float scroll = value.ScrollY - offset;
                    MathHelper.Clamp(ref scroll, 0, cSize.Height - vRect.Height);
                    bool mod = value.ScrollY != scroll;
                    if (mod)
                    {
                        float fromScroll = value.ScrollY;
                        value.ScrollY = scroll;
                        value.ManualInput = true;
                        node.MarkRenderDirty();

                        // Start scroll animation if enabled
                        if (value.Animation)
                        {
                            var ani = new ScrollAnimation(
                                node,
                                value,
                                GuiOrientation.Vertical,
                                fromScroll,
                                scroll,
                                node.Theme.ScrollDuration);

                            node.StartAnimation(ani);
                        }

                    }

                    // Always return syncMode to prevent input events from propagating upward
                    node.SetIsControlling(true);
                    node.Gui.QueueAction(() => node.SetIsControlling(false));
                    return syncMode;
                }

                // Hide scrollbar and reset scroll if content fits
                if (value.VScrollBarVisible)
                {
                    value.VScrollBarVisible = false;
                    node.MarkRenderDirty();
                }
                if (value.ScrollY != 0)
                {
                    value.ScrollY = 0;
                    node.MarkRenderDirty();
                }

                return syncMode;
            }

            // Horizontal scrolling (with Shift key if vertical is also enabled)
            if (value.ScrollOrientation.IsHorizontal() && (!value.ScrollOrientation.IsVertical() || input.GetOnlyShiftKey()))
            {
                if (cSize.Width > vRect.Width)
                {
                    if (!value.HScrollBarVisible)
                    {
                        value.HScrollBarVisible = true;
                        node.MarkRenderDirty();
                    }

                    float offset = input.MouseDelta * delta;
                    float scroll = value.ScrollX - offset;
                    MathHelper.Clamp(ref scroll, 0, cSize.Width - vRect.Width);
                    bool mod = value.ScrollX != scroll;
                    if (mod)
                    {
                        float fromScroll = value.ScrollX;
                        value.ScrollX = scroll;
                        value.ManualInput = true;
                        node.MarkRenderDirty();

                        // Start scroll animation if enabled
                        if (value.Animation)
                        {
                            var ani = new ScrollAnimation(
                                node,
                                value,
                                GuiOrientation.Vertical,
                                fromScroll,
                                scroll,
                                node.Theme.ScrollDuration);

                            node.StartAnimation(ani);
                        }

                    }

                    // Always return syncMode to prevent input events from propagating upward
                    node.SetIsControlling(true);
                    node.Gui.QueueAction(() => node.SetIsControlling(false));
                    return syncMode;
                }

                // Hide scrollbar and reset scroll if content fits
                if (value.HScrollBarVisible)
                {
                    value.HScrollBarVisible = false;
                    node.MarkRenderDirty();
                }
                if (value.ScrollX != 0)
                {
                    value.ScrollX = 0;
                    node.MarkRenderDirty();
                }

                return syncMode;
            }

            return GuiInputState.None;
        }

        return GuiInputState.None;
    }

    private GuiInputState TextInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;

            return GuiInputState.None;
        }

        if (pipeline == GuiPipeline.BeginEdit)
        {
            BeginTextEdit(node);

            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                node.Gui.SetCursor(GuiCursorTypes.Hand);
                break;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                node.Gui.SetCursor(GuiCursorTypes.Default);
                break;

            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked())
                {
                    BeginTextEdit(node);
                }

                break;

            case GuiEventTypes.FocusOut:
                (node.Gui.Context as IGraphicTextBoxEdit)?.EndTextEdit();
                break;
        }

        return GuiInputState.None;
    }

    private GuiInputState SimpleStringInputFunction(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        if (pipeline == GuiPipeline.BeginEdit)
        {
            BeginTextEdit(node);
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.FocusOut:
                (node.Gui.Context as IGraphicTextBoxEdit)?.EndTextEdit();
                break;
        }

        return GuiInputState.None;
    }

    private GuiInputState DoubleClickStringInputFunction(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        if (pipeline == GuiPipeline.BeginEdit)
        {
            BeginTextEdit(node);
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && node.Gui.IsDoubleClick)
                {
                    BeginTextEdit(node);
                }

                break;

            case GuiEventTypes.FocusOut:
                (node.Gui.Context as IGraphicTextBoxEdit)?.EndTextEdit();
                break;
        }

        return GuiInputState.None;
    }

    private static GuiInputState DragDropInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (node.Gui.Input.DragEvent is { })
        {
            return GuiInputState.FullSync;
        }

        return state;
    }

    private static void BeginTextEdit(ImGuiNode node)
    {
        if (node.Gui.Context is IGraphicTextBoxEdit edit)
        {
            var hint = node.GetValue<GuiHintTextStyle>();

            var option = new TextBoxEditOptions
            {
                IsReadonly = node.IsReadOnly,
                IsPassword = hint?.Password == true,
                MultiLine = hint?.Multiline == true,
                AutoFit = node.FitOrientation.IsHorizontal(),
                SubmitMode = hint?.SubmitMode ?? TextBoxEditSubmitMode.Auto,
                Font = node.GetScaledFont(),
                EditedCallBack = text =>
                {
                    if (text is { } && node.Text != text && node.IsInUsage && !node.IsReadOnly)
                    {
                        node.Text = text;
                        node.IsEdited = true;
                        // Directly calling RequestInput causes Scroll data to become invalid
                        //node.Gui.Context.RequestInput();
                        node.QueueRefresh();
                    }
                },
            };

            edit.BeginTextEdit(node.GetGlobalTextInputRect(false).ToInt(), node.Text ?? string.Empty, option);
        }
    }

    private Decimal _valueOnMouseDown;
    private bool _mouseMoving;
    private Point? _lastMouseDownPos;

    private GuiInputState NumericInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        var value = node.GetValue<GuiNumericValue>();
        if (value is null)
        {
            return GuiInputState.None;
        }

        if (pipeline == GuiPipeline.BeginEdit)
        {
            BeginEditNumericInput(node, value);
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                node.Gui.SetCursor(GuiCursorTypes.SizeWE);
                break;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                node.Gui.SetCursor(GuiCursorTypes.Default);
                break;

            case GuiEventTypes.MouseDown:
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    _valueOnMouseDown = value.DecimalValue;
                    _mouseMoving = false;
                    _lastMouseDownPos = input.MouseLocation;
                    node.SetIsControlling(true);
                }
                break;

            case GuiEventTypes.MouseUp:
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    node.SetIsControlling(false);
                    if (node.IsMouseInClickRect)
                    {
                        node.Gui.SetCursor(GuiCursorTypes.SizeWE);
                    }
                    else
                    {
                        node.Gui.SetCursor(GuiCursorTypes.Default);
                    }

                    _lastMouseDownPos = null;

                    if (_mouseMoving)
                    {
                        // Release after dragging

                        node.IsEdited = true;
                        value.SetText(node, true);

                        return GuiInputState.FullSync;
                    }
                    else if (node.GetIsClicked())
                    {
                        // Direct click

                        BeginEditNumericInput(node, value);
                    }
                }
                break;

            case GuiEventTypes.MouseMove:
                {
                    if (input.MouseLocation is not { } pos)
                    {
                        return GuiInputState.None;
                    }

                    if (node.IsReadOnly)
                    {
                        return GuiInputState.None;
                    }

                    if (!node.IsControlling)
                    {
                        return GuiInputState.None;
                    }

                    if (_lastMouseDownPos is not { } lastDownPos)
                    {
                        return GuiInputState.None;
                    }
                    // Check if mouse movement threshold is reached to distinguish between click and drag
                    if (Math.Abs(pos.X - lastDownPos.X) < 2 && Math.Abs(pos.Y - lastDownPos.Y) < 2)
                    {
                        // Movement distance not reached
                        return GuiInputState.None;
                    }

                    _mouseMoving = true;

                    // Calculate value change based on horizontal mouse offset
                    var offset = pos.X - node.Gui.LastMouseDownLocation.X;
                    if (value.Min is { } min && value.Max is { } max && max > min)
                    {
                        // When min/max are defined, calculate proportional value based on node width
                        var rect = node.GlobalRect;
                        float r = (float)offset / (float)rect.Width;
                        decimal v = (max - min) * (decimal)r;
                        if (value.Increment > 0)
                        {
                            v -= v % value.Increment;
                        }
                        value.DecimalValue = _valueOnMouseDown + v;
                    }
                    else
                    {
                        // Without min/max, use increment as pixel-to-value multiplier
                        var v = offset * value.Increment;
                        value.DecimalValue = _valueOnMouseDown + v;
                    }

                    node.IsEdited = true;
                    value.SetText(node, true);

                    return value.RefreshAtOnce ? GuiInputState.FullSync : GuiInputState.Render;
                }

            case GuiEventTypes.FocusOut:
                (node.Gui.Context as IGraphicTextBoxEdit)?.EndTextEdit();
                break;
        }

        return GuiInputState.None;
    }

    private static void BeginEditNumericInput(ImGuiNode node, GuiNumericValue? value)
    {
        value?.SetText(node, false);

        if (node.Gui.Context is IGraphicTextBoxEdit edit)
        {
            var option = new TextBoxEditOptions
            {
                IsReadonly = node.IsReadOnly,
                AutoFit = node.FitOrientation.IsHorizontal(),
                Font = node.GetScaledFont(),
                EditedCallBack = text =>
                {
                    if (text is { } && node.IsInUsage && !node.IsReadOnly)
                    {
                        node.Text = text;
                        value?.SetValueFromText(node);
                        value?.SetText(node, true);
                        node.IsEdited = true;
                        // Directly calling RequestInput causes Scroll data to become invalid
                        //node.Gui.Context.RequestInput();
                        node.QueueRefresh();
                    }
                },
            };

            edit.BeginTextEdit(node.GetGlobalTextInputRect(false).ToInt(), node.Text ?? string.Empty, option);
        }
    }

    private GuiInputState ResizerInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        var value = node.GetValue<GuiResizerValue>();
        if (value is null)
        {
            return GuiInputState.None;
        }

        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            switch (input.EventType)
            {
                case GuiEventTypes.HoverIn:
                    {
                        if (node.IsControlling)
                        {
                            return GuiInputState.None;
                        }

                        node.Pseudo = ImGuiNode.PseudoMouseIn;
                        node.MarkRenderDirty();

                        GuiCursorTypes cursor = value.Orientation switch
                        {
                            GuiOrientation.Vertical => GuiCursorTypes.HSplit,
                            GuiOrientation.Horizontal => GuiCursorTypes.VSplit,
                            _ => GuiCursorTypes.Default,
                        };

                        node.Gui.SetCursor(cursor);
                        return GuiInputState.Render;
                    }

                case GuiEventTypes.HoverOut:
                    {
                        if (node.IsControlling)
                        {
                            return GuiInputState.None;
                        }

                        node.Pseudo = null;
                        node.Gui.SetCursor(GuiCursorTypes.Default);
                        node.MarkRenderDirty();
                        return GuiInputState.Render;
                    }

                case GuiEventTypes.MouseDown:
                    {
                        if (ResizerBeginDrag(node, input, value))
                        {
                            node.SetIsControlling(true);
                            return GuiInputState.Render;
                        }

                        break;
                    }

                case GuiEventTypes.MouseMove:
                    {
                        if (node.IsControlling && ResizerDragging(node, input, value))
                        {
                            node.Parent?.MarkRenderDirty();
                            // Using Layout here would cause VirtualTree to fail syncing and not display subsequent content.
                            return GuiInputState.PartialSync;
                        }

                        break;
                    }

                case GuiEventTypes.MouseUp:
                    if (input.GetMouseButtonDown(GuiMouseButtons.Left))
                    {
                        value.MouseDownPos = null;
                        node.SetIsControlling(false);
                        if (!node.IsHover)
                        {
                            node.Pseudo = null;
                            node.Gui.SetCursor(GuiCursorTypes.Default);
                        }

                        return GuiInputState.FullSync;
                    }

                    break;

                default:
                    return state;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.BeginDrag))
        {
            if (ResizerBeginDrag(node, input, value))
            {
                return GuiInputState.Render;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Dragging))
        {
            if (ResizerDragging(node, input, value))
            {
                return SplitterSync;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.EndDrag))
        {
            value.MouseDownPos = null;
        }

        return GuiInputState.None;
    }

    private GuiInputState ResizerFitterInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        var value = node.GetValue<GuiResizerValue>();
        if (value is null)
        {
            return GuiInputState.None;
        }

        if (pipeline.HasFlag(GuiPipeline.BeginDrag))
        {
            if (ResizerBeginDrag(node, input, value))
            {
                return GuiInputState.Render;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Dragging))
        {
            if (ResizerDragging(node, input, value))
            {
                return SplitterSync;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.EndDrag))
        {
            value.MouseDownPos = null;
        }

        return GuiInputState.None;
    }

    private bool ResizerBeginDrag(ImGuiNode node, IGraphicInput input, GuiResizerValue value)
    {
        if (input.GetMouseButtonDown(GuiMouseButtons.Left) && node.Previous is { } previous)
        {
            var rect = previous.Rect;
            value.MouseDownPos = input.MouseLocation;
            value.ContentSize = new() { Width = rect.Width, Height = rect.Height };

            if (value.AffectSibling && node.Next is { } next)
            {
                var nextRect = next.Rect;
                value.NextContentSize = new() { Width = nextRect.Width, Height = nextRect.Height };
            }

            return true;
        }

        return false;
    }

    private bool ResizerDragging(ImGuiNode node, IGraphicInput input, GuiResizerValue value)
    {
        if (input.MouseLocation is { } pos && value.MouseDownPos is { } downPos && node.Previous is { } previous)
        {
            float diff;

            var rect = node.Parent?.InnerRect ?? RectangleF.Empty;

            switch (value.Orientation)
            {
                case GuiOrientation.Vertical:
                    {
                        // When affecting sibling, resize both current and next node while respecting min/max constraints
                        if (value.AffectSibling && node.Next is { } next)
                        {
                            diff = node.GlobalReverseScaleValue(pos.Y - downPos.Y);
                            var newLen1 = value.ContentSize.Height + diff;
                            newLen1 = value.GetPositionMinMax(newLen1, out bool limited1);

                            var newLen2 = value.NextContentSize.Height - diff;
                            newLen2 = value.GetPositionMinMax(newLen2, out bool limited2);

                            // Only apply resize if neither node hits its constraint boundary
                            if (!limited1 && !limited2)
                            {
                                previous.Height = GetNewLength(newLen1, rect.Height, previous.Height?.Mode);
                                next.Height = GetNewLength(newLen2, rect.Height, next.Height?.Mode);
                            }
                        }
                        else
                        {
                            // Single node resize without affecting sibling
                            diff = node.GlobalReverseScaleValue(pos.Y - downPos.Y);
                            var newLen = value.ContentSize.Height + diff;
                            newLen = value.GetPositionMinMax(newLen, out _);
                            previous.Height = GetNewLength(newLen, rect.Height, previous.Height?.Mode);
                        }

                        break;
                    }
                case GuiOrientation.Horizontal:
                    {
                        // When affecting sibling, resize both current and next node while respecting min/max constraints
                        if (value.AffectSibling && node.Next is { } next)
                        {
                            diff = node.GlobalReverseScaleValue(pos.X - downPos.X);
                            var newLen1 = value.ContentSize.Width + diff;
                            newLen1 = value.GetPositionMinMax(newLen1, out bool limited1);

                            var newLen2 = value.NextContentSize.Width - diff;
                            newLen2 = value.GetPositionMinMax(newLen2, out bool limited2);

                            // Only apply resize if neither node hits its constraint boundary
                            if (!limited1 && !limited2)
                            {
                                previous.Width = GetNewLength(newLen1, rect.Width, previous.Width?.Mode);
                                next.Width = GetNewLength(newLen2, rect.Width, next.Width?.Mode);
                            }
                        }
                        else
                        {
                            // Single node resize without affecting sibling
                            diff = node.GlobalReverseScaleValue(pos.X - downPos.X);
                            var newLen = value.ContentSize.Width + diff;
                            newLen = value.GetPositionMinMax(newLen, out _);
                            previous.Width = GetNewLength(newLen, rect.Width, previous.Width?.Mode);
                        }

                        break;
                    }
            }

            return true;
        }

        return false;
    }

    private GuiInputState GroupedResizerInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        bool update = false;

        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            switch (input.EventType)
            {
                case GuiEventTypes.MouseDown:
                    if (input.GetMouseButtonDown(GuiMouseButtons.Left))
                    {
                        var value = node.GetValue<GroupedResizerItem>();
                        var managerNode = node.FindNodeInHierarchy(n => n.GetValue<GroupedResizerState>() is { }, false);

                        if (value is { } && managerNode is { })
                        {
                            HandleGroupResizerChildren(GuiPipeline.BeginDrag, node, managerNode, input, value.Index);
                        }
                    }

                    break;

                case GuiEventTypes.MouseMove:
                    if (node.IsControlling)
                    {
                        var value = node.GetValue<GroupedResizerItem>();
                        var managerNode = node.FindNodeInHierarchy(n => n.GetValue<GroupedResizerState>() is { }, false);

                        if (value is { } && managerNode is { })
                        {
                            HandleGroupResizerChildren(GuiPipeline.Dragging, node, managerNode, input, value.Index);

                            // Mark the main object as dirty because it causes chain reactions
                            managerNode.MarkRenderDirty();

                            var orientation = node.GetValue<GuiResizerValue>()?.Orientation ?? GuiOrientation.None;
                            var managerValue = managerNode.GetValue<GroupedResizerState>()!;

                            switch (orientation)
                            {
                                case GuiOrientation.Horizontal:
                                    {
                                        if (node.Previous?.Width is { Mode: GuiLengthMode.Fixed } width)
                                        {
                                            managerValue.SetLength(value.Index, width.Value);
                                        }
                                        update = true;
                                        break;
                                    }

                                case GuiOrientation.Vertical:
                                    {
                                        if (node.Previous?.Height is { Mode: GuiLengthMode.Fixed } height)
                                        {
                                            managerValue.SetLength(value.Index, height.Value);
                                        }
                                        update = true;
                                        break;
                                    }
                            }
                        }
                    }
                    break;

                case GuiEventTypes.MouseUp:
                    if (!input.GetMouseButtonDown(GuiMouseButtons.Left))
                    {
                        var value = node.GetValue<GroupedResizerItem>();
                        var managerNode = node.FindNodeInHierarchy(n => n.GetValue<GroupedResizerState>() is { }, false);

                        if (value is { } && managerNode is { })
                        {
                            HandleGroupResizerChildren(GuiPipeline.EndDrag, node, managerNode, input, value.Index);
                            update = true;
                        }
                    }

                    break;
            }
        }

        return update ? SplitterSync : state;
    }

    private void HandleGroupResizerChildren(GuiPipeline pipeline, ImGuiNode sourceNode, ImGuiNode node, IGraphicInput input, int index)
    {
        if (node == sourceNode)
        {
            return;
        }

        var value = node.GetValue<GroupedResizerItem>();
        if (value?.Index == index)
        {
            //Debug.WriteLine(node.FullName);
            node.InputFunction?.Invoke(pipeline, node, input, (_, _) => GuiInputState.None);
            return;
        }

        foreach (var childNode in node.ChildNodes)
        {
            HandleGroupResizerChildren(pipeline, sourceNode, childNode, input, index);
        }
    }

    internal GuiLength GetNewLength(float value, float fullValue, GuiLengthMode? mode = null)
    {
        switch (mode)
        {
            case GuiLengthMode.Fixed:
                return value;

            case GuiLengthMode.ScaledFixed:
                return value;

            case GuiLengthMode.FullExcept:
                return value;

            case GuiLengthMode.RestExcept:
                return value;

            case GuiLengthMode.Percentage:
                if (fullValue > 0)
                {
                    return new GuiLength(value / fullValue * 100f, GuiLengthMode.Percentage);
                }
                else
                {
                    return value;
                }
            case GuiLengthMode.RestPercentage:
                if (fullValue > 0)
                {
                    return new GuiLength(value / fullValue * 100f, GuiLengthMode.Percentage);
                }
                else
                {
                    return value;
                }
            case GuiLengthMode.Adapt:
                return GuiLength.Adapt;

            default:
                return new GuiLength(value);
        }
    }

    private GuiInputState TreeViewInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state != GuiInputState.None)
        {
            // Child node has already responded
            return state;
        }

        var data = node.GetValue<VisualTreeData>();
        if (data is null)
        {
            return state;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                data.ClearSelection();
                GuiTreeViewExtensions.UpdateTreeNodeSelections(node);
                //node.MarkRenderDirty();
                ImGui.MergeState(ref state, GuiInputState.Layout);
                break;

            default:
                break;
        }

        return state;
    }

    private GuiInputState TreeNodeInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        if (node.Gui.Input.DragEvent is { })
        {
            // Override drag input function, do not execute baseAction
            return TreeNodeDragDropInput(pipeline, node, input, baseAction);
        }

        // Need to check != None here; if child node performed an action, this node should not act, to avoid Resizer operation conflicts.
        var baseState = baseAction(pipeline);
        var state = baseState;
        //if (state == GuiInputState.FullSync)
        //{
        //    return state;
        //}

        var value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return state;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                if (input.MouseButton == GuiMouseButtons.Left || input.MouseButton == GuiMouseButtons.Right) {
                    var selMode = value.Tree.SelectionMode;

                    bool allowMultiple = false;
                    if (selMode == ImTreeViewSelectionMode.Multiple)
                    {
                        allowMultiple = true;
                    }
                    else if (selMode == ImTreeViewSelectionMode.MultipleSameParent &&
                        value.Parent == value.Tree.SelectedNode?.Parent)
                    {
                        allowMultiple = true;
                    }

                    if (input.ControlKey)
                    {
                        if (allowMultiple)
                        {
                            value.Tree.ToggleSelection(value);
                            GuiTreeViewExtensions.UpdateTreeNodeSelection(node, value);
                            //node.Parent?.MarkRenderDirty();
                        }
                    }
                    else if (input.ShiftKey)
                    {
                        if (allowMultiple)
                        {
                            int lastIndex = value.Tree.SelectedNode?.Index ?? -1;
                            int index = value.Index;
                            if (lastIndex == -1)
                            {
                                lastIndex = index;
                            }
                            value.Tree.SetSelections(lastIndex, index);
                            GuiTreeViewExtensions.UpdateTreeNodeSelections(node.Parent);
                            //node.Parent?.MarkRenderDirty();
                        }
                    }
                    else
                    {
                        if (!value.IsSelected)
                        {
                            value.Tree.SetSelection(value);
                            GuiTreeViewExtensions.UpdateTreeNodeSelections(node.Parent);
                            //node.Parent?.MarkRenderDirty();
                        }
                    }

                    //if (node.Gui.ControllingNode is null)
                    //{
                    //    node.SetIsControlling(true);
                    //}

                    // Drag activated only when child node has no action
                    if (baseState == GuiInputState.None)
                    {
                        value.MouseDown = true;
                    }

                    ImGui.MergeState(ref state, GuiInputState.Render);
                }
                break;

            case GuiEventTypes.MouseUp:
                // Do not use GetIsClicked() here; GetIsClicked() checks if child node has input function and ignores if so.
                // Even if child node can act, this node can still be selected.

                if (node.MouseState == GuiMouseState.Clicked /**GetIsClicked()**/)
                {
                    if (input.ControlKey)
                    {
                    }
                    else if (input.ShiftKey)
                    {
                    }
                    else
                    {
                        value.Tree.SetSelection(value);
                        GuiTreeViewExtensions.UpdateTreeNodeSelections(node.Parent);
                        //node.Parent?.MarkRenderDirty();
                    }
                }

                node.SetIsControlling(false);
                value.MouseDown = false;
                value.Tree.ClearDroppingNode();
                //node.Parent?.MarkRenderDirty();
                ImGui.MergeState(ref state, GuiInputState.Render);

                break;

            case GuiEventTypes.MouseMove:
                {
                    // Drag activated only when child node has no action
                    if (baseState != GuiInputState.None)
                    {
                        return GuiInputState.None;
                    }

                    if (!value.MouseDown)
                    {
                        return GuiInputState.None;
                    }

                    //if (node.IsControlling || node.ChildNodes.Any(o => o.IsControlling))
                    //{
                    //    return GuiInputState.None;
                    //}

                    // IsControlling only when mouse is pressed and moved on this node
                    if (input.MouseButton == GuiMouseButtons.Left && input.MouseLocation is { } pos)
                    {
                        int offset = Math.Abs(pos.X - node.Gui.LastMouseDownLocation.X) +
                                     Math.Abs(pos.Y - node.Gui.LastMouseDownLocation.Y);

                        if (offset >= 10)
                        {
                            // Tests show controlling node needs to be cancelled, then let system's DragOver handle it.
                            // If not set as controlling node, OnPartialContent will fail because this flow needs to monitor controlling node.
                            node.SetIsControlling(true);

                            // Get drag value from parent
                            var draggable = node.Parent?.GetOrCreateValue<GuiDraggableValue>();
                            if (draggable is { })
                            {
                                value.DragRequesting = true;
                                draggable.DragRequest = true;
                                ImGui.MergeState(ref state, GuiInputState.PartialSync);
                            }
                        }
                    }
                    else
                    {
                        return GuiInputState.None;
                    }
                }
                break;

            case GuiEventTypes.MouseOut:
                value.Tree.ClearDroppingNode();
                break;
        }

        return state;
    }

    private GuiInputState TreeNodeDragDropInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = GuiInputState.FullSync;

        var value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return state;
        }

        switch (input.EventType)
        {
            //case GuiEventTypes.MouseIn:
            //    node.Pseudo = ImGuiNode.PseudoMouseIn;
            //    node.MarkRenderDirty();
            //    state = GuiState.Refresh;
            //    break;

            case GuiEventTypes.MouseIn:
            case GuiEventTypes.DragOver:
            case GuiEventTypes.DragDrop:
                {
                    if (!(input.MouseLocation is { } pos))
                    {
                        break;
                    }

                    var rect = node.GlobalRect;
                    float h4 = rect.Height * 0.25f;
                    float top = rect.Top + h4;
                    float bottom = rect.Bottom - h4;

                    bool dropAction = input.EventType == GuiEventTypes.DragDrop;

                    if (pos.Y < top)
                    {
                        node.Pseudo = null;
                        value.Tree.SetDroppingNode(value, ImTreeNodeDragDropMode.Previous, dropAction);
                    }
                    else if (pos.Y > bottom)
                    {
                        node.Pseudo = null;
                        value.Tree.SetDroppingNode(value, ImTreeNodeDragDropMode.Next, dropAction);
                    }
                    else
                    {
                        node.Pseudo = ImGuiNode.PseudoMouseIn;
                        value.Tree.SetDroppingNode(value, ImTreeNodeDragDropMode.Inside, dropAction);
                    }

                    node.MarkRenderDirty();
                }
                break;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                value.Tree.ClearDroppingNode();
                break;

            default:
                break;
        }

        return state;
    }

    private GuiInputState ViewportInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childAction)
    {
        if (node.IsDisabled)
        {
            node.Pseudo = null;
            return GuiInputState.None;
        }

        var viewport = node.GetValue<GuiViewportValue>();
        if (viewport is null)
        {
            return GuiInputState.None;
        }

        GuiInputState state;

        switch (input.EventType)
        {
            case GuiEventTypes.Resize:
            case GuiEventTypes.Refresh:
            case GuiEventTypes.BeginSync:
                viewport.ApplyViewportNode(node);
                state = GuiInputState.Render;
                break;
            
            case GuiEventTypes.MouseDown:
                if (input.MouseButton == GuiMouseButtons.Right && input.MouseLocation is Point mouseLocation)
                {
                    viewport.MouseDownPosition = mouseLocation;
                    state = GuiInputState.None;
                }
                else
                {
                    viewport.MouseDownPosition = null;
                    state = GuiInputState.None;
                }
                break;

            case GuiEventTypes.MouseUp:
                if (input.MouseButton == GuiMouseButtons.Right)
                {
                    viewport.MouseDownPosition = null;
                    viewport.UnsetPan();
                }

                state = GuiInputState.None;
                break;

            case GuiEventTypes.MouseMove:
                if (viewport.MouseDownPosition is Point downPosition && input.MouseLocation is Point movingPosition)
                {
                    int ox = movingPosition.X - downPosition.X;
                    int oy = movingPosition.Y - downPosition.Y;
                    viewport.SetPan(ox, oy);

                    viewport.ApplyViewportNode(node);

                    state = GuiInputState.Render;
                    break;
                }
                else
                {
                    state = GuiInputState.None;
                    break;
                }

            case GuiEventTypes.MouseWheel:
                if (input.MouseDelta > 0 && viewport.ZoomIn())
                {
                    viewport.ApplyViewportNode(node);

                    state = GuiInputState.Render;
                    break;
                }
                else if (input.MouseDelta < 0 && viewport.ZoomOut())
                {
                    viewport.ApplyViewportNode(node);

                    state = GuiInputState.Render;
                    break;
                }
                else
                {
                    state = GuiInputState.None;
                    break;
                }

            default:
                state = GuiInputState.None;
                break;
        }

        childAction(pipeline, viewport.InBoundNodes);

        return state;
    }
}