using Suity.Collections;
using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Helpers;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Provides property row rendering functions for <see cref="FlowNode"/> types, including computation state visualization.
/// </summary>
public static class FlowNodePropertyFunctions
{
    /// <summary>
    /// Creates a property row editor for flow nodes with computation state display and design object properties.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null.</returns>
    public static ImGuiNode? FlowNodeRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        if (!target.IsRoot)
        {
            return ViewObjectPropertyFunctions.ViewObjectRowFunction(gui, target, rowAction);
        }

        ViewObjectPropertyFunctions.ViewObjectRowFunctionInner(gui, target, rowAction);

        var values = target.GetValues();
        if (!values.CountOne())
        {
            return null;
        }

        if (values.FirstOrDefault() is not FlowNode node)
        {
            return null;
        }

        if (node.GetFlowDocument() is not { } doc)
        {
            return null;
        }

        // Real-time computation visualization
        var view = target.ServiceProvider?.GetService<IFlowView>();
        if (view?.Computation is { } computation && computation.GetNodeRunningState(node) is { } state)
        {
            gui.VerticalLayout("#computationFrame")
            .InitFullWidth()
            .SetPadding(10)
            .OnContent(n =>
            {
                switch (state.State)
                {
                    case FlowComputationStates.Running:
                        gui.Text("Running")
                        .InitClass("propLabelText")
                        .InitCenterHorizontal()
                        .OverrideFont(new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold), Color.Cyan);
                        break;

                    case FlowComputationStates.Finished:
                        {
                            string formattedTime = GetFormattedTime(state.ElapsedTime);

                            gui.Text($"Completed ({formattedTime})")
                            .InitClass("propLabelText")
                            .InitCenterHorizontal()
                            .OverrideFont(new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold), Color.DarkGreen);
                        }
                        break;

                    case FlowComputationStates.Error:
                        {
                            var err = state.Exception;
                            string formattedTime = GetFormattedTime(state.ElapsedTime);

                            gui.Text($"Error: {err?.GetType().Name} ({formattedTime})")
                            .InitClass("propLabelText")
                            .InitCenterHorizontal()
                            .OverrideFont(new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold), Color.Red);

                            string msg = string.Empty;
                            if (err != null)
                            {
                                msg = err.Message;
                            }

                            gui.TextAreaInput("#error_msg", msg ?? string.Empty)
                            .InitReadonly(true)
                            .InitFullWidth()
                            .InitHeight(100);

                            gui.VerticalResizer(30, null)
                            .InitFullWidth()
                            .InitClass("resizer");
                            break;
                        }

                    case FlowComputationStates.Cancelled:
                        gui.Text("Cancelled")
                        .InitClass("propLabelText")
                        .InitCenterHorizontal()
                        .OverrideFont(new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold), Color.Gray);
                        break;
                }
            });

            var inputs = node.Connectors.Where(o => o.Direction == FlowDirections.Input && o.DataTypeName != FlowNode.ACTION_TYPE);
            if (inputs.Any())
            {
                gui.PropertyLabel("##input-connectors", "Input Ports", CoreIconCache.Connect);
                foreach (var c in inputs)
                {
                    string propName = $"#computed-{c.Name}";

                    var t = target.GetOrCreateField<FlowNode, object>(propName, CreateComputedValueGetter(computation, c.Name));
                    t.Description = c.Description ?? c.DisplayName;
                    t.ReadOnly = true;

                    gui.PropertyField(t);
                }
            }

            var outputs = node.Connectors.Where(o => o.Direction == FlowDirections.Output && o.DataTypeName != FlowNode.ACTION_TYPE);
            if (outputs.Any())
            {
                gui.PropertyLabel("##output-connectors", "Output Ports", CoreIconCache.Connect);
                foreach (var c in outputs)
                {
                    string propName = $"#computed-{c.Name}";

                    var t = target.GetOrCreateField<FlowNode, object>(propName, CreateComputedValueGetter(computation, c.Name));
                    t.Description = c.Description ?? c.DisplayName;
                    t.ReadOnly = true;

                    gui.PropertyField(t);
                }
            }
        }

        // Design object properties
        if (values.All(o => o is IDesignObject))
        {
            DesignObjectSetups.DesignObjectGui(gui, target, [.. values.OfType<IDesignObject>()]);
        }

        return null;
    }

    private static string GetFormattedTime(TimeSpan duration)
    {
        string formattedTime;

        if (duration.TotalSeconds < 180)
        {
            formattedTime = string.Format("{0:F4}s", duration.TotalSeconds);
        }
        else if (duration.TotalMinutes < 60)
        {
            formattedTime = string.Format("{0:F4}m", duration.TotalMinutes);
        }
        else
        {
            formattedTime = string.Format("{0:F4}h", duration.TotalHours);
        }

        return formattedTime;
    }

    private static Func<FlowNode, object?> CreateComputedValueGetter(IFlowComputation computation, string name)
    {
        return new Func<FlowNode, object?>(o =>
        {
            var c = o.GetConnector(name);
            if (c != null)
            {
                var value = computation.GetValue(c);
                if (value is string str)
                {
                    // Automatically wrap as text block
                    return new TextBlock(str);
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return null;
            }
        });
    }
}