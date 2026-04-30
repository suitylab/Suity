using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Extension methods for creating and configuring property grid UI elements in the ImGui-based editor.
/// </summary>
public static class PropertyGridExtensions
{
    /// <summary>
    /// The width in pixels used for indenting nested property rows.
    /// </summary>
    public static int IndentWidth = 12;

    /// <summary>
    /// The width in pixels reserved for the prefix column (e.g., expand/collapse icons).
    /// </summary>
    public static int PrefixColumnWidth = 28;

    /// <summary>
    /// External delegate that provides the actual implementation for property grid operations.
    /// </summary>
    internal static PropertyGridExternal _external;

    /// <summary>
    /// Creates a new property grid instance with the specified name.
    /// </summary>
    /// <param name="name">The name identifier for the property grid.</param>
    /// <returns>A new <see cref="IPropertyGrid"/> instance.</returns>
    public static IPropertyGrid CreatePropertyGrid(string name) 
        => _external.CreatePropertyGrid(name);

    /// <summary>
    /// Begins a property frame node that can contain multiple property rows.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="scrollable">Whether the property frame should support scrolling.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public static ImGuiNode PropertyFrame(this ImGui gui, bool scrollable = true)
        => _external.PropertyFrame(gui, scrollable);

    /// <summary>
    /// Begins a property frame node with a unique identifier.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the property frame.</param>
    /// <param name="scrollable">Whether the property frame should support scrolling.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public static ImGuiNode PropertyFrame(this ImGui gui, string id, bool scrollable = true)
        => _external.PropertyFrame(gui, id, scrollable);

    /// <summary>
    /// Begins a property frame node with initial grid data and optional resizer state.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the property frame.</param>
    /// <param name="scrollable">Whether the property frame should support scrolling.</param>
    /// <param name="initGridData">The initial data for the property grid layout.</param>
    /// <param name="initResizerState">Optional initial state for the grouped resizer.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property frame.</returns>
    public static ImGuiNode PropertyFrame(this ImGui gui, string id, bool scrollable, PropertyGridData initGridData, GroupedResizerState? initResizerState = null)
        => _external.PropertyFrame(gui, id, scrollable, initGridData, initResizerState);

    /// <summary>
    /// Creates a collapsible property box with a title header.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the property box.</param>
    /// <param name="title">The display title shown in the header.</param>
    /// <param name="initExpand">Whether the box should be expanded by default.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property box.</returns>
    public static ImGuiNode PropertyBox(this ImGui gui, string id, string title, bool initExpand = true)
        => _external.PropertyBox(gui, id, title, initExpand);

    /// <summary>
    /// Creates a property row bound to a target with a custom editor function.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the value and metadata.</param>
    /// <param name="func">Optional custom function to render the property editor.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public static ImGuiNode PropertyRow(this ImGui gui, PropertyTarget target, PropertyEditorFunction? func, PropertyRowAction? rowAction = null)
        => _external.PropertyRow(gui, target, func, rowAction);

    /// <summary>
    /// Creates a property row bound to a target with an optional target action.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the value and metadata.</param>
    /// <param name="targetAction">Optional action for handling target-specific interactions.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public static ImGuiNode PropertyRow(this ImGui gui, PropertyTarget target, PropertyTargetAction? targetAction = null)
        => _external.PropertyRow(gui, target, targetAction);

    /// <summary>
    /// Creates a property row with an explicit identifier bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the property row.</param>
    /// <param name="target">The property target that provides the value and metadata.</param>
    /// <param name="targetAction">Optional action for handling target-specific interactions.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public static ImGuiNode PropertyRow(this ImGui gui, string id, PropertyTarget target, PropertyTargetAction? targetAction = null)
        => _external.PropertyRow(gui, id, target, targetAction);

    /// <summary>
    /// Creates a simple property row displaying a title with an optional status indicator.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the property row.</param>
    /// <param name="title">The display text for the row.</param>
    /// <param name="status">Optional status indicator (e.g., warning, error).</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row.</returns>
    public static ImGuiNode PropertyRow(this ImGui gui, string id, string title, TextStatus? status = null, PropertyRowAction? rowAction = null)
        => _external.PropertyRow(gui, id, title, status, rowAction);

    /// <summary>
    /// Creates the frame container for a property row with optional row action and data.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the row frame.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="value">Optional existing <see cref="PropertyRowData"/> to associate with the row.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property row frame.</returns>
    public static ImGuiNode PropertyRowFrame(this ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? value = null)
        => _external.PropertyRowFrame(gui, id, rowAction, value);

    /// <summary>
    /// Creates a read-only property label bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the value and metadata.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label.</returns>
    public static ImGuiNode PropertyLabel(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
        => _external.PropertyLabel(gui, target, rowAction);

    /// <summary>
    /// Creates a read-only property label with explicit title and optional icon.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the label.</param>
    /// <param name="title">The display text for the label.</param>
    /// <param name="icon">Optional icon displayed alongside the title.</param>
    /// <param name="status">Optional status indicator (e.g., warning, error).</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="value">Optional existing <see cref="PropertyRowData"/> to associate with the label.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label.</returns>
    public static ImGuiNode PropertyLabel(this ImGui gui, string id, string title, Image? icon = null, TextStatus? status = null, PropertyRowAction? rowAction = null, PropertyRowData? value = null)
        => _external.PropertyLabel(gui, id, title, icon, status, rowAction, value);

    /// <summary>
    /// Creates the frame container for a property label with optional row action and data.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the label frame.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="value">Optional existing <see cref="PropertyRowData"/> to associate with the label.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property label frame.</returns>
    public static ImGuiNode PropertyLabelFrame(this ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? value = null)
        => _external.PropertyLabelFrame(gui, id, rowAction, value);

    /// <summary>
    /// Creates a property row that displays tooltip information for a target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the tooltip content.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the tooltips row.</returns>
    public static ImGuiNode PropertyTooltips(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
        => _external.PropertyTooltips(gui, target, rowAction);

    /// <summary>
    /// Creates a property button bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the button metadata.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public static ImGuiNode PropertyButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
        => _external.PropertyButton(gui, target, rowAction);

    /// <summary>
    /// Creates a property button with explicit text and optional icon.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the button.</param>
    /// <param name="text">The display text on the button.</param>
    /// <param name="icon">Optional icon displayed alongside the text.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="onClick">Optional callback invoked when the button is clicked.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public static ImGuiNode PropertyButton(this ImGui gui, string id, string text, Image? icon = null, PropertyRowAction? rowAction = null, Action? onClick = null)
        => _external.PropertyButton(gui, id, text, icon, rowAction, onClick);

    /// <summary>
    /// Creates a property button that supports multiple values (array/collection targets).
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the button metadata.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property button.</returns>
    public static ImGuiNode PropertyMultipleButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
        => _external.PropertyMultipleButton(gui, target, rowAction);

    /// <summary>
    /// Creates a collapsible property group bound to a target.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the group metadata.</param>
    /// <param name="preview">Optional preview text shown in the collapsed header.</param>
    /// <param name="targetAction">Optional action for handling target-specific interactions.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group.</returns>
    public static ImGuiNode PropertyGroup(this ImGui gui, PropertyTarget target, string? preview = null, PropertyTargetAction? targetAction = null)
        => _external.PropertyGroup(gui, target, preview, targetAction);

    /// <summary>
    /// Creates a collapsible property group with an explicit title.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the group.</param>
    /// <param name="title">The display title shown in the header.</param>
    /// <param name="preview">Optional preview text shown in the collapsed header.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="initExpand">Whether the group should be expanded by default.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group.</returns>
    public static ImGuiNode PropertyGroup(this ImGui gui, string id, string title, string? preview = null, PropertyRowAction? rowAction = null, bool initExpand = true)
        => _external.PropertyGroup(gui, id, title, preview, rowAction, initExpand);

    /// <summary>
    /// Creates the frame container for a property group with optional row action and data.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">A unique identifier for the group frame.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="initExpand">Whether the group should be expanded by default.</param>
    /// <param name="value">Optional existing <see cref="PropertyRowData"/> to associate with the group.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property group frame.</returns>
    public static ImGuiNode PropertyGroupFrame(this ImGui gui, string id, PropertyRowAction? rowAction = null, bool? initExpand = true, PropertyRowData? value = null)
        => _external.PropertyGroupFrame(gui, id, rowAction, initExpand, value);

    /// <summary>
    /// Renders a property title for a <see cref="PropertyTarget"/> with optional dark styling.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the title metadata.</param>
    /// <param name="dark">Whether to apply dark text styling.</param>
    public static void PropertyTitle(this ImGui gui, PropertyTarget target, bool dark = false)
        => _external.PropertyTitle(gui, target, dark);

    /// <summary>
    /// Renders a property title for an <see cref="IValueTarget"/> with optional dark styling.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target that provides the title metadata.</param>
    /// <param name="dark">Whether to apply dark text styling.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the property title.</returns>
    public static ImGuiNode PropertyTitle(this ImGui gui, IValueTarget target, bool dark = false)
        => _external.PropertyTitle(gui, target, dark);

    /// <summary>
    /// Registers a callback to be invoked when a property group is expanded or collapsed.
    /// </summary>
    /// <param name="node">The property group node.</param>
    /// <param name="action">The callback to invoke on expand/collapse.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode OnPropertyGroupExpand(this ImGuiNode node, Action action)
        => _external.OnPropertyGroupExpand(node, action);

    /// <summary>
    /// Registers a callback that receives the expanded state when a property group is expanded or collapsed.
    /// </summary>
    /// <param name="node">The property group node.</param>
    /// <param name="action">The callback to invoke, receiving <c>true</c> when expanded and <c>false</c> when collapsed.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode OnPropertyGroupExpand(this ImGuiNode node, Action<bool> action)
        => _external.OnPropertyGroupExpand(node, action);

    /// <summary>
    /// Retrieves or creates a <see cref="PropertyRowData"/> instance associated with the given property target.
    /// </summary>
    /// <param name="target">The property target to get or create row data for.</param>
    /// <returns>The existing or newly created <see cref="PropertyRowData"/>.</returns>
    internal static PropertyRowData GetOrCreatePropertyRowData(this PropertyTarget target)
    {
        if (target.FieldGuiData is PropertyRowData data)
        {
            return data;
        }

        data = new PropertyRowData
        {
            Target = target,
        };

        target.FieldGuiData = data;

        return data;
    }

    /// <summary>
    /// Retrieves or creates a <see cref="PropertyRowData"/> instance associated with the given ImGui node and property target.
    /// </summary>
    /// <param name="node">The ImGui node to get or create row data for.</param>
    /// <param name="target">The property target used to initialize the row data if a new instance is created.</param>
    /// <returns>The existing or newly created <see cref="PropertyRowData"/>.</returns>
    internal static PropertyRowData GetOrCreatePropertyRowData(this ImGuiNode node, PropertyTarget target)
    {
        if (node.GetValue<PropertyRowData>() is { } existing)
        {
            return existing;
        }

        var value = target?.GetOrCreatePropertyRowData();
        if (value is not null)
        {
            return value;
        }

        value = new PropertyRowData();
        node.SetValue(value);
        node.InitializePropertyRowData(value);

        // Logs.LogWarning("PropertyRowData created dynamically.");

        return value;
    }

    /// <summary>
    /// Retrieves or creates a <see cref="PropertyRowData"/> instance associated with the given ImGui node.
    /// </summary>
    /// <param name="node">The ImGui node to get or create row data for.</param>
    /// <returns>The existing or newly created <see cref="PropertyRowData"/>.</returns>
    internal static PropertyRowData GetOrCreatePropertyRowData(this ImGuiNode node)
    {
        var value = node.GetOrCreateValue<PropertyRowData>(out bool created);
        if (created)
        {
            // Logs.LogWarning("PropertyRowData created dynamically.");
        }
        node.InitializePropertyRowData(value);

        return value;
    }

    /// <summary>
    /// Initializes the <see cref="PropertyRowData"/> by locating the parent <see cref="PropertyGridData"/> in the node hierarchy.
    /// </summary>
    /// <param name="node">The ImGui node whose hierarchy is searched for grid data.</param>
    /// <param name="value">The <see cref="PropertyRowData"/> to initialize.</param>
    internal static void InitializePropertyRowData(this ImGuiNode node, PropertyRowData value)
    {
        if (value.GridData is null && !value.IsRootMissing)
        {
            value.GridData = node.FindValueInHierarchy<PropertyGridData>();
            if (value.GridData is null)
            {
                value.IsRootMissing = true;
            }
        }

        if (node.IsInitializing)
        {
            node.SetValue(value);
        }
    }

    /// <summary>
    /// Counts elements in a sequence up to a maximum value, stopping early once the limit is reached.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="values">The sequence to count elements from.</param>
    /// <param name="max">The maximum count to return.</param>
    /// <returns>The number of elements counted, capped at <paramref name="max"/>.</returns>
    internal static int CountWithMaxValue<T>(this IEnumerable<T> values, int max)
    {
        int num = 0;
        foreach (var v in values)
        {
            num++;
            if (num == max)
            {
                break;
            }
        }

        return num;
    }

    /// <summary>
    /// Sets the font and border color of a node based on the error state and custom color of a value target.
    /// </summary>
    /// <param name="node">The ImGui node to style.</param>
    /// <param name="target">The value target providing error state and color information.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetValueEditorColor(this ImGuiNode node, IValueTarget target)
    {
        if (target.ErrorInHierarchy)
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Error);
        }
        else if (target.Color is { } color)
        {
            node.BorderColor = node.FontColor = color;
        }
        else
        {
            node.BorderColor = node.FontColor = null;
        }

        return node;
    }
       
    /// <summary>
    /// Overrides the font color of a node based on the value editor color derived from a target.
    /// </summary>
    /// <param name="node">The ImGui node to style.</param>
    /// <param name="target">The value target providing color information.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode OverrideValueEditorColor(this ImGuiNode node, IValueTarget target)
    {
        var color = target.GetValueEditorColor();
        node.OverrideFont(null, color);

        return node;
    }

    /// <summary>
    /// Determines the appropriate value editor color for a target based on its error state and custom color.
    /// </summary>
    /// <param name="target">The value target to evaluate.</param>
    /// <returns>The color to use for the value editor, or null if no special color applies.</returns>
    private static Color? GetValueEditorColor(this IValueTarget target)
    {
        if (target.ErrorInHierarchy)
        {
            return EditorServices.ColorConfig.GetStatusColor(TextStatus.Error);
        }
        else if (target.Color is { } color)
        {
            return color;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the title color of a property node based on the target's status, expandability, obsolete state, and dark mode preference.
    /// </summary>
    /// <param name="node">The ImGui node to style.</param>
    /// <param name="target">The property target providing status and metadata.</param>
    /// <param name="dark">Whether to apply dark text styling as a fallback.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetPropertyTitleColor(this ImGuiNode node, PropertyTarget target, bool dark = false)
    {
        //if (target.ContainsError)
        //{
        //    node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Error);
        //}
        //else 
        if (target.Status != TextStatus.Normal)
        {
            node.SetFontColor(target.Status);
        }
        else if (!target.CanExpand)
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Disabled);
        }
        else if (target.Styles is { } style && string.Equals(style.GetAttribute("Obsolete"), "true", StringComparison.OrdinalIgnoreCase))
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Disabled);
        }
        else if (dark)
        {
            node.FontColor = Color.Black;
        }
        else
        {
            node.FontColor = null;
        }

        return node;
    }

    /// <summary>
    /// Sets the title color of a property node based on the target's error state, status, obsolete state, and dark mode preference.
    /// </summary>
    /// <param name="node">The ImGui node to style.</param>
    /// <param name="target">The value target providing error state, status, and metadata.</param>
    /// <param name="dark">Whether to apply dark text styling as a fallback.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetPropertyTitleColor(this ImGuiNode node, IValueTarget target, bool dark = false)
    {
        if (target.ErrorInHierarchy)
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Error);
        }
        else if (target.Status != TextStatus.Normal)
        {
            node.SetFontColor(target.Status);
        }
        else if (target.Styles is { } style && string.Equals(style.GetAttribute("Obsolete"), "true", StringComparison.OrdinalIgnoreCase))
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(TextStatus.Disabled);
        }
        else if (dark)
        {
            node.FontColor = Color.Black;
        }
        else
        {
            node.FontColor = null;
        }

        return node;
    }

    /// <summary>
    /// Sets the font color of a node based on a text status value.
    /// </summary>
    /// <param name="node">The ImGui node to style.</param>
    /// <param name="textStatus">The status determining the font color.</param>
    /// <returns>The same <see cref="ImGuiNode"/> for method chaining.</returns>
    public static ImGuiNode SetFontColor(this ImGuiNode node, TextStatus textStatus)
    {
        if (textStatus == TextStatus.Normal)
        {
            node.FontColor = null;
        }
        else
        {
            node.FontColor = DefaultEditorColorConfig.Default.GetStatusColor(textStatus);
        }

        return node;
    }

    /// <summary>
    /// Wraps a <see cref="PropertyEditorFunction"/> into a <see cref="PropertyRowFunction"/> that applies status-based border styling.
    /// </summary>
    /// <param name="func">The editor function to wrap.</param>
    /// <returns>A <see cref="PropertyRowFunction"/> that renders the editor with appropriate border styling.</returns>
    public static PropertyRowFunction MakeRowFunction(this PropertyEditorFunction func)
    {
        return (gui, target, action) =>
        {
            var node = gui.PropertyRow(target, func, action);

            if (target.Status.ShowStatusBorder())
            {
                node?.OverrideBorder(1f, target.Status.ToColor());
            }
            else
            {
                node?.OverrideBorder(null, null);
            }

            return node;
        };
    }

    /// <summary>
    /// Wraps a <see cref="PropertyEditorFunction"/> into a <see cref="PropertyRowFunction"/> that includes a "Go to Definition" button and status-based border styling.
    /// </summary>
    /// <param name="func">The editor function to wrap.</param>
    /// <returns>A <see cref="PropertyRowFunction"/> that renders the editor with a navigation button and appropriate border styling.</returns>
    public static PropertyRowFunction MakeRowFunctionWithGotoDef(this PropertyEditorFunction func)
    {
        return (gui, target, action) =>
        {
            var node = gui.PropertyRow(target, func, (n, c, p) => 
            {
                action?.Invoke(n, c, p);

                if (p.HasFlag(GuiPipeline.Main) && c == PropertyGridColumn.Option)
                {
                    if (n.Parent?.GetIsPropertyFieldSelected() == true)
                    {
                        if ((target.GetValues().CountOne() || !target.ValueMultiple) && target.GetValues().FirstOrDefault() is { } obj)
                        {
                            gui.Button("goto", CoreIconCache.GotoDefination)
                            .InitClass("configBtn")
                            .OnClick(n =>
                            {
                                EditorUtility.NavigateTo(obj);
                            });
                        }
                    }
                }
            });

            if (target.Status.ShowStatusBorder())
            {
                node?.OverrideBorder(1f, target.Status.ToColor());
            }
            else
            {
                node?.OverrideBorder(null, null);
            }

            return node;
        };
    }

    /// <summary>
    /// Wraps a <see cref="PropertyEditorFunction"/> into a <see cref="PropertyRowFunction"/> that renders as a prepositive row when the target is optional.
    /// </summary>
    /// <param name="func">The editor function to wrap.</param>
    /// <returns>A <see cref="PropertyRowFunction"/> that conditionally uses prepositive rendering for optional targets.</returns>
    public static PropertyRowFunction MakePrepositiveRowFunction(this PropertyEditorFunction func)
    {
        return (gui, target, action) =>
        {
            ImGuiNode? node;

            if (target.Optional)
            {
                node = gui.PrepositivePropertyRow(target, action, func);
            }
            else
            {
                node = gui.PropertyRow(target, func, action);
            }

            if (target.Status.ShowStatusBorder())
            {
                node?.OverrideBorder(1f, target.Status.ToColor());
            }
            else
            {
                node?.OverrideBorder(null, null);
            }

            return node;
        };
    }

    /// <summary>
    /// Creates a prepositive property row where the value editor is rendered in the name column instead of the main column.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target that provides the value and metadata.</param>
    /// <param name="rowAction">Optional action invoked during row rendering pipelines.</param>
    /// <param name="func">The editor function used to render the value.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the prepositive property row.</returns>
    public static ImGuiNode PrepositivePropertyRow(this ImGui gui, PropertyTarget target, PropertyRowAction? rowAction, PropertyEditorFunction func)
    {
        return PropertyRow(gui, target, (n, t0, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.PreAction))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                if (column == PropertyGridColumn.Name)
                {
                    func(gui, t0, act => n.DoValueAction(act));
                }
                else if (column == PropertyGridColumn.Main)
                {
                    if (target.ToolTips is { } tooltips && !string.IsNullOrWhiteSpace(tooltips))
                    {
                        gui.Text("#tooltips", L(tooltips))
                        .InitWidthRest();
                    }
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        });
    }


    /// <summary>
    /// Determines whether a <see cref="TextStatus"/> value should display a status border around the property row.
    /// </summary>
    /// <param name="status">The text status to evaluate.</param>
    /// <returns><c>true</c> if the status is Denied, Info, Warning, or Error; otherwise, <c>false</c>.</returns>
    public static bool ShowStatusBorder(this TextStatus status)
    {
        switch (status)
        {
            case TextStatus.Denied:
            case TextStatus.Info:
            case TextStatus.Warning:
            case TextStatus.Error:
                return true;

            default:
                return false;
        }
    }
}