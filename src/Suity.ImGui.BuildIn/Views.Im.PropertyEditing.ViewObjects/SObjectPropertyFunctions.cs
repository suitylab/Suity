using Suity;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Provides property population and row rendering functions for <see cref="SObject"/> types.
/// </summary>
public static class SObjectPropertyFunctions
{
    /// <summary>
    /// Populates the property target with child fields based on the SObject structure.
    /// </summary>
    /// <param name="target">The property target to populate.</param>
    public static void SObjectPopulateFunction(PropertyTarget target)
    {
        SObject[] objs = target.GetValues().As<SObject>().ToArray();

        var setup = target.GetSObjectSetup();
        setup.Clear();

        objs.SetupObjects(setup, false, out var objType);

        // If Attributes is not null and field attributes were already set, do not override.
        // This sets type attributes, not field attributes
        target.Attributes ??= objType;
    }

    /// <summary>
    /// Populates the property target with child fields for preview mode, showing only preview fields.
    /// </summary>
    /// <param name="target">The property target to populate.</param>
    public static void SObjectPopulateFunctionPreview(PropertyTarget target)
    {
        SObject[] objs = target.GetValues().As<SObject>().ToArray();

        var setup = target.GetSObjectSetup();
        setup.Clear();

        objs.SetupObjects(setup, true, out var objType);

        // If Attributes is not null and field attributes were already set, do not override.
        // This sets type attributes, not field attributes
        target.Attributes ??= objType;
    }

    /// <summary>
    /// Creates a property row editor for SObject values with expandable groups, type selection, and attachment display.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null if the target is root.</returns>
    public static ImGuiNode? SObjectRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        DCompond? type = target.GetValues().OfType<SObject>().FirstOrDefault()?.ObjectType?.Target as DCompond;

        // If Attributes is not null and field attributes were already set, do not override.
        // This sets type attributes, not field attributes
        target.Attributes ??= type;

        if (target.IsRoot)
        {
            SObjectRowFunctionInner(gui, target, rowAction);

            return null;
        }

        ImGuiNode? node = gui.PropertyGroup(target, targetAction: (n, t, c, p) =>
        {
            if (p.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, c, GuiPipeline.PreAction);

                if (c == PropertyGridColumn.Main)
                {
                    var childValues = t.GetValues();
                    var childObjs = childValues.OfType<SObject>();
                    var childFirstVal = childObjs.FirstOrDefault();
                    var inputType = GetInputType(childObjs);
                    var objType = GetObjectType(childObjs);

                    string typeName = objType?.ToDisplayString() ?? inputType?.ToDisplayString() ?? string.Empty;
                    string brief = typeName;

                    // Show brief string when collapsed, otherwise show type name
                    if (!n.GetIsExpanded() && childValues.CountOne() && childFirstVal is { })
                    {
                        brief = childFirstVal.ToBriefString();
                        if (string.IsNullOrWhiteSpace(brief))
                        {
                            brief = typeName;
                        }
                    }

                    // Render attachment icons if the SObject has any attachments
                    if (childObjs.Any(o => o.HasAttachments))
                    {
                        string title = L("Attachment");

                        if (childObjs.CountOne() && childFirstVal is { })
                        {
                            foreach (var pair in childFirstVal.GetAttachments().Where(o => o.Value != null))
                            {
                                string aid = $"#attachment_{pair.Key}";
                                string tooltip;

                                Type type = pair.Value.GetType();
                                if (type == typeof(string))
                                {
                                    tooltip = $"{title}: {pair.Key}=\"{pair.Value.ToString().ToShortcutString()}\"";
                                    gui.Image(aid, CoreIconCache.Attachment).InitClass("icon").InitToolTips(pair.Key).InitToolTips(tooltip);
                                }
                                else if (type.IsPrimitive)
                                {
                                    tooltip = $"{title}: {pair.Key}={pair.Value}";
                                    gui.Image(aid, CoreIconCache.Value).InitClass("icon").InitToolTips(pair.Key).InitToolTips(tooltip);
                                }
                                else if (pair.Value is ITextDisplay display)
                                {
                                    tooltip = $"{title}: {pair.Key}={display.ToDisplayTextL().ToShortcutString()}";
                                    gui.Image(aid, display.ToDisplayIcon() ?? CoreIconCache.Attachment).InitClass("icon").InitToolTips(pair.Key).InitToolTips(tooltip);
                                }
                                else
                                {
                                    tooltip = $"{title}: {pair.Key}=[{type.Name}]";
                                    gui.Image(aid, CoreIconCache.Attachment).InitClass("icon").InitToolTips(pair.Key);
                                }
                            }
                        }
                        else
                        {
                            gui.Image("#attachment", CoreIconCache.Attachment).InitClass("icon").InitToolTips($"{title}...");
                        }
                    }

                    // Check if type selection is needed (abstract type or generic Object type)
                    bool selection = inputType is not null && (inputType.IsAbstract || inputType == NativeTypes.ObjectType);
                    if (selection)
                    {
                        if (objType?.GetIcon() is { } icon)
                        {
                            gui.Image("#property_icon", icon).InitClass("icon");
                        }

                        if (n.Parent?.GetIsExpanded() == true)
                        {
                            gui.Text("#brief", typeName)
                            .InitClass(PropertyGridThemes.GetBriefClass(t.ValueMultiple))
                            .InitWidthRest(24)
                            .InitCenterVertical();
                        }
                        else if (!t.HoriMode())
                        {
                            gui.Text("#brief", brief)
                            .InitClass(PropertyGridThemes.GetBriefClass(t.ValueMultiple))
                            .InitWidthRest(24)
                            .InitCenterVertical();
                        }
                        else
                        {
                            gui.HorizontalLayout("#hori")
                            .InitWidthRest(24)
                            .InitCenterVertical()
                            .OnContent(() =>
                            {
                                HoriSObjectEditorFunction(gui, t, act => n.DoValueAction(act));
                            });
                        }

                        // "More" button to change the SObject type
                        gui.Button("##more", ImGuiIcons.More)
                        .InitClass("configBtn")
                        .OnClick(async () =>
                        {
                            if (n.IsReadOnly || target.ReadOnly)
                            {
                                return;
                            }

                            if (inputType is null)
                            {
                                return;
                            }

                            var obj = await inputType.GuiCreateObject(childObjs.FirstOrDefault(), inputType.ToDisplayString());
                            if (obj is null)
                            {
                                return;
                            }

                            // Clone the new object for all selected values
                            int count = childValues.Count();
                            object[] newObjs = new object[count];
                            newObjs[0] = obj;
                            for (int i = 1; i < count; i++)
                            {
                                newObjs[i] = Cloner.Clone(obj);
                            }

                            try
                            {
                                var action = t.SetValuesAction(newObjs);
                                n.DoValueAction(action);
                            }
                            catch (Exception err)
                            {
                                Logs.LogError(err);
                            }
                        });
                    }
                    else
                    {
                        // Non-selectable type: just display brief/type name
                        if (n.Parent?.GetIsExpanded() == true)
                        {
                            gui.Text("#brief", typeName)
                            .InitClass(PropertyGridThemes.GetBriefClass(t.ValueMultiple))
                            .InitFullWidth()
                            .InitCenterVertical();
                        }
                        else if (!t.HoriMode())
                        {
                            gui.Text("#brief", brief)
                            .InitClass(PropertyGridThemes.GetBriefClass(t.ValueMultiple))
                            .InitFullWidth()
                            .InitCenterVertical();
                        }
                        else
                        {
                            HoriSObjectEditorFunction(gui, t, act => n.DoValueAction(act));
                        }
                    }
                }
                else if (c == PropertyGridColumn.Prefix)
                {
                    var childValues = t.GetValues();
                    var childObjs = childValues.OfType<SObject>();
                    var childFirstVal = childObjs.FirstOrDefault();
                    var inputType = GetInputType(childObjs);
                    var objType = GetObjectType(childObjs);
                    
                    // Render nullable checkbox for optional SObject properties
                    if (t.Optional)
                    {
                        gui.CheckBox("##nullable", !SObject.IsNullOrEmpty(childFirstVal))
                        .SetClass(target.GetPropertyInputClass())
                        .OnChecked((n, v) =>
                        {
                            SObject obj;

                            if (v)
                            {
                                // Create normal SObject with inputType as objectType
                                obj = new SObject(inputType, inputType);
                            }
                            else
                            {
                                // Create empty SObject to represent null
                                obj = new SObject(inputType, TypeDefinition.Empty);
                            }

                            // Clone for all selected values
                            int count = childValues.Count();
                            object[] newObjs = new object[count];
                            newObjs[0] = obj;
                            for (int i = 1; i < count; i++)
                            {
                                newObjs[i] = Cloner.Clone(obj);
                            }

                            try
                            {
                                var action = t.SetValuesAction(newObjs);
                                n.DoValueAction(action);
                            }
                            catch (Exception err)
                            {
                                Logs.LogError(err);
                            }
                        });
                    }
                }

                rowAction?.Invoke(n, c, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        })
        .OnPropertyGroupExpand(() =>
        {
            SObjectRowFunctionInner(gui, target, rowAction);
        });

        return node;
    }

    /// <summary>
    /// Creates a property row editor for <see cref="SDynamic"/> values with dynamic type display and cancellation support.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null if the target is root.</returns>
    public static ImGuiNode? SDynamicRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        target.Status = TextStatus.Reference;

        if (target.IsRoot)
        {
            ViewObjectPropertyFunctions.ViewObjectRowFunctionInner(gui, target, rowAction);

            return null;
        }

        ArrayElementOp? elementOp = null;

        var node = gui.PropertyGroup(target, targetAction: (n, t, c, p) =>
        {
            if (p.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, c, GuiPipeline.PreAction);

                if (c == PropertyGridColumn.Main)
                {
                    var childValues = t.GetValues();
                    var childFirstVal = childValues.OfType<SDynamic>().FirstOrDefault();

                    string brief = string.Empty;

                    if (!n.GetIsExpanded() && childValues.CountOne() && childFirstVal is { })
                    {
                        if (childFirstVal is ITextDisplay display)
                        {
                            if (EditorUtility.GetIcon(display.DisplayIcon) is { } icon)
                            {
                                gui.Image("#icon", icon)
                                .InitClass("icon")
                                .InitCenterVertical();
                            }

                            gui.Text("#brief", L(childFirstVal.GetType().ToDisplayText()) + " ")
                            .InitCenterVertical()
                            .SetFontColor(display.DisplayStatus);

                            gui.Text("#display", L(display.DisplayText))
                            .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                            .InitWidthRest()
                            .InitCenterVertical();
                        }
                        else
                        {
                            //brief = $"{childFirstVal.TypeDisplayName} : {EditorUtility.GetDisplayString(childFirstVal.GetValue())}";
                            brief = childFirstVal.ToString();

                            gui.Text("#brief", brief)
                            .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                            .InitFullWidth()
                            .InitCenterVertical();
                        }
                    }
                }
                else if (c == PropertyGridColumn.Option && !n.IsReadOnly)
                {
                    if (n.GetIsPropertyFieldSelected())
                    {
                        gui.Button("cancel_dynamic", ImGuiIcons.Cancel)
                        .InitClass("configBtn")
                        .OnClick(n =>
                        {
                            elementOp = ArrayElementOp.Delete;
                        });
                    }
                }

                rowAction?.Invoke(n, c, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        })
        .OnPropertyGroupExpand(() =>
        {
            ViewObjectPropertyFunctions.ViewObjectRowFunctionInner(gui, target, rowAction);
        });

        if (elementOp.HasValue)
        {
            IValueAction? act = null;

            switch (elementOp.Value)
            {
                case ArrayElementOp.Delete:
                    act = target.SetDynamicAction(null);
                    break;

                default:
                    break;
            }

            if (act != null)
            {
                node.DoValueAction(act);
                node.QueueRefresh();
            }
        }

        return node;
    }

    private static TypeDefinition? GetInputType(IEnumerable<SObject> objs)
    {
        var types = objs.Select(o => o.InputType);

        if (types.Any() && types.AllReferenceEqual())
        {
            return types.FirstOrDefault();
        }

        return null;
    }

    private static TypeDefinition? GetObjectType(IEnumerable<SObject> objs)
    {
        var types = objs.Select(o => o.ObjectType);

        if (types.Any() && types.AllReferenceEqual())
        {
            return types.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Creates the inner property row content for SObject values, populating and rendering child fields.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <param name="config">Optional configuration action applied to each child node.</param>
    internal static void SObjectRowFunctionInner(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction, Action<ImGuiNode>? config = null)
    {
        SObjectPopulateFunction(target);
        var setup = target.GetSObjectSetup();

        foreach (var childTarget in setup.ChildTargets)
        {
            // Do not pass parent rowAction
            var node = gui.PropertyField(childTarget, null);
            if (node != null)
            {
                config?.Invoke(node);
            }
        }
    }

    /// <summary>
    /// Creates the inner property row content for SObject values in preview mode.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <param name="config">Optional configuration action applied to each child node.</param>
    internal static void SObjectRowFunctionPreview(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction, Action<ImGuiNode>? config = null)
    {
        SObjectPopulateFunctionPreview(target);
        var setup = target.GetSObjectSetup();

        foreach (var childTarget in setup.ChildTargets)
        {
            // Do not pass parent rowAction
            var node = gui.PropertyField(childTarget, null);
            if (node != null)
            {
                config?.Invoke(node);
            }
        }
    }

    #region Hori Layout

    private static bool HoriMode(this PropertyTarget target)
    {
        DCompond? type = target.GetValues().OfType<SObject>().FirstOrDefault()?.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return false;
        }

        var attr = type.GetAttribute<HorizontalLayoutAttribute>();
        if (attr is null)
        {
            return false;
        }

        int fieldCount = type.GetHoriFields(attr.PreviewFieldOnly).Count();
        if (fieldCount > ViewObjectPropertyFunctions.MaxHoriFieldCount)
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<DStructField> GetHoriFields(this DCompond type, bool previewFieldOnly)
    {
        if (previewFieldOnly)
        {
            return type.PublicStructFields
                .Where(o => o.GetAttribute<PreviewFieldAttribute>() != null);
        }
        else
        {
            return type.PublicStructFields;
        }
    }

    private static IEnumerable<PropertyTarget> GetHoriFields(this PropertyTarget target, bool previewFieldOnly)
    {
        if (previewFieldOnly)
        {
            return target.Fields
                .Where(o => o.Attributes?.GetAttribute<PreviewFieldAttribute>() != null);
        }
        else
        {
            return target.Fields;
        }
    }

    private static ImGuiNode? HoriSObjectRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        SObjectPopulateFunction(target);
        var setup = target.GetSObjectSetup();
        if (setup.Count == 0)
        {
            return gui.PropertyRow(target);
        }

        foreach (var childTarget in target.Fields)
        {
            // Not sure why we need to get values here to refresh ValueMultiple
            childTarget.GetValues();
        }

        float firstPercentage = target.Attributes?.GetAttribute<HorizontalLayoutAttribute>()?.FirstColumnPercentage ?? 0f;

        return gui.PropertyRow(target, (n, inner, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        rowAction?.Invoke(n, column, pipeline);
                        break;

                    case PropertyGridColumn.Name:
                        rowAction?.Invoke(n, column, pipeline);
                        break;

                    case PropertyGridColumn.Main:
                        {
                            if (inner.FieldCount > 0)
                            {
                                (float pFirst, float p) = GetFieldPercentages(inner.FieldCount, firstPercentage);
                                int i = 0;

                                foreach (var childTarget in inner.Fields)
                                {
                                    gui.HorizontalLayout($"#column_{childTarget.Id}")
                                    .InitOverrideSiblingSpacing(0)
                                    .SetWidthPercentage(i == 0 ? pFirst : p)
                                    .SetToolTipsL(L(childTarget.DisplayName))
                                    .OnContent(() =>
                                    {
                                        gui.PropertyEditor(childTarget, act => n.DoValueAction(act));
                                    });

                                    i++;
                                }
                            }
                        }
                        break;

                    case PropertyGridColumn.Option:
                        gui.Button("toggleHori", ImGuiIcons.Row)
                        .InitClass("configBtn")
                        .OnClick(n =>
                        {
                        });

                        rowAction?.Invoke(n, column, pipeline);
                        break;

                    default:
                        break;
                }
            }
        });
    }

    /// <summary>
    /// Creates a horizontal layout editor for SObject values with multiple fields displayed side by side.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The value target to edit.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>null as horizontal editors render inline.</returns>
    public static ImGuiNode? HoriSObjectEditorFunction(this ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        DCompond? type = target.GetValues().OfType<SObject>().FirstOrDefault()?.ObjectType?.Target as DCompond;
        PropertyTarget? propTarget = target as PropertyTarget;
        if (propTarget is null)
        {
            return null;
        }

        // Check if type has HorizontalLayoutAttribute and field count is within limit
        if (type is { FieldCount: <= ViewObjectPropertyFunctions.MaxHoriFieldCount } && type.GetAttribute<HorizontalLayoutAttribute>() is { } attr)
        {
            float firstPercentage = attr.FirstColumnPercentage;

            SObjectPopulateFunction(propTarget);
            var setup = propTarget.GetSObjectSetup();
            if (setup.Count == 0)
            {
                return null;
            }

            // Get fields to display (preview-only or all based on attribute)
            var fields = propTarget.GetHoriFields(attr.PreviewFieldOnly).ToArray();

            foreach (var childTarget in fields)
            {
                // Refresh ValueMultiple state for each child target
                childTarget.GetValues();
            }

            // Calculate column width percentages
            (float pFirst, float p) = GetFieldPercentages(fields.Length, firstPercentage);
            int i = 0;

            // Render each field in a horizontal layout with calculated width
            foreach (var childTarget in fields)
            {
                gui.HorizontalLayout($"#column_{childTarget.Id}")
                .InitCompact(true)
                .InitOverrideSiblingSpacing(0)
                .SetWidthPercentage(i == 0 ? pFirst : p)
                .SetToolTipsL(L(childTarget.DisplayName))
                .OnContent(() =>
                {
                    gui.PropertyEditor(childTarget, act => handler(act));
                });

                i++;
            }
        }
        else
        {
            // Fallback: display brief string when horizontal layout is not applicable
            var childValues = propTarget.GetValues();
            var childFirstVal = childValues.OfType<SObject>().FirstOrDefault();

            if (childValues.CountOne() && childFirstVal is { })
            {
                string brief = childFirstVal.ToBriefString();
                gui.Text("#brief", brief)
                .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                .InitFullWidth()
                .InitCenterVertical();
            }
        }

        return null;
    }

    #endregion

    private static (float, float) GetFieldPercentages(int fieldCount, float firstPercentage)
    {
        if (fieldCount <= 0)
        {
            return (100, 0);
        }

        float p;
        float pFirst;

        if (firstPercentage > 0)
        {
            if (fieldCount > 1)
            {
                pFirst = firstPercentage;
                p = (100 - pFirst) / (fieldCount - 1);
            }
            else
            {
                pFirst = 100;
                p = 0;
            }
        }
        else
        {
            pFirst = p = 100 / fieldCount;
        }

        return (pFirst, p);
    }

    private static SObjectSetup GetSObjectSetup(this PropertyTarget target)
    {
        if (target.ObjectSetupCache is not SObjectSetup setup)
        {
            setup = new SObjectSetup(target);
            target.ObjectSetupCache = setup;
        }

        return setup;
    }
}
