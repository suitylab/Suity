using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using System;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

#region ViewObjectSetups

/// <summary>
/// Provides property population and row rendering functions for <see cref="IViewObject"/> types.
/// </summary>
public static class ViewObjectPropertyFunctions
{
    /// <summary>
    /// Maximum number of fields that can be displayed in horizontal layout mode.
    /// </summary>
    public const int MaxHoriFieldCount = 8;

    /// <summary>
    /// Populates the property target with child fields based on the view object setup.
    /// </summary>
    /// <param name="target">The property target to populate.</param>
    public static void ViewObjectPopulateFunction(PropertyTarget target)
    {
        var first = target.GetValues().OfType<IViewObject>().FirstOrDefault();

        var setup = target.GetViewObjectSetup();
        setup.Clear();

        first?.SetupView(setup);
    }

    /// <summary>
    /// Creates a property row editor for view objects with expandable groups and optional value support.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null if the target is root.</returns>
    public static ImGuiNode? ViewObjectRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        if (target.IsRoot)
        {
            ViewObjectRowFunctionInner(gui, target, rowAction);

            return null;
        }

        var node = gui.PropertyGroup(target, targetAction: (n, t, c, p) =>
        {
            if (p.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, c, GuiPipeline.PreAction);

                if (c == PropertyGridColumn.Main)
                {
                    var childValues = t.GetValues();
                    var childFirstVal = childValues.OfType<IViewObject>().FirstOrDefault();
                    bool one = childValues.CountOne();
                    var inputType = target.PresetType;
                    var objType = target.EditedType;

                    string brief = string.Empty;

                    if (!n.GetIsExpanded() && one && childFirstVal is { })
                    {
                        brief = childFirstVal.ToString();
                    }

                    bool selection = inputType?.IsAbstract == true;
                    if (selection)
                    {
                        if (one && childFirstVal?.ToDisplayIcon() is { } icon)
                        {
                            gui.Image("#icon", icon)
                            .InitClass("icon")
                            .InitCenterVertical();
                        }

                        if (!string.IsNullOrEmpty(brief))
                        {
                            gui.Text("#brief", brief)
                            .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                            .InitWidthRest(24)
                            .InitCenterVertical();
                        }

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

                            var (success, obj) = await inputType.GuiCreateObjectAsync(inputType.ToDisplayText());
                            if (!success)
                            {
                                return;
                            }

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
                        if (one && childFirstVal?.ToDisplayIcon() is { } icon)
                        {
                            gui.Image("#icon", icon)
                            .InitClass("icon")
                            .InitCenterVertical();
                        }

                        if (!string.IsNullOrEmpty(brief))
                        {
                            gui.Text("#brief", brief)
                            .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                            .InitFullWidth()
                            .InitCenterVertical();
                        }
                    }
                }
                else if (c == PropertyGridColumn.Prefix)
                {
                    var childValues = t.GetValues();
                    var childFirstVal = childValues.OfType<IViewObject>().FirstOrDefault();

                    if (childFirstVal is IViewOptional optional)
                    {
                        CheckState state = optional.IsOptional ? CheckState.Checked : CheckState.Unchecked;
                        if (!childValues.As<IViewOptional>().Select(o => o?.IsOptional ?? false).AllEqual())
                        {
                            state = CheckState.Indeterminate;
                        }

                        gui.CheckBoxAdvanced("##nullable", state)
                        .SetClass(target.GetPropertyInputClass())
                        .OnChecked((n, v) =>
                        {
                            try
                            {
                                var action = new OptionalSetterAction(t, v);
                                n.DoValueAction(action);
                            }
                            catch (Exception err)
                            {
                                Logs.LogError(err);
                            }

                            t.ExpandRequest = v;
                        });
                    }
                    else if (t.Optional)
                    {
                        gui.CheckBox("##nullable", childFirstVal != null)
                        .SetClass(target.GetPropertyInputClass())
                        .OnChecked((n, v) =>
                        {
                            object? obj = null;

                            if (v && t.EditedType is { } editedType)
                            {
                                try
                                {
                                    if (editedType.IsInterface || editedType.IsAbstract)
                                    {
                                        obj = ObjectCreationNotice.Instance;
                                    }
                                    else
                                    {
                                        obj = Activator.CreateInstance(editedType);
                                    }
                                }
                                catch (Exception err)
                                {
                                    obj = null;
                                    err.LogError($"Failed to create {t.EditedType?.Name}");
                                }
                            }
                            else
                            {
                                obj = null;
                            }

                            int count = childValues.Count();
                            object?[] newObjs = new object?[count];
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

                            t.ExpandRequest = v;
                        });
                    }
                }

                rowAction?.Invoke(n, c, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        })
        .OnPropertyGroupExpand(() =>
        {
            var childValues = target.GetValues();
            if (childValues.FirstOrDefault() is IViewOptional)
            {
                bool show = childValues.As<IViewOptional>().All(o => o?.IsOptional ?? false);
                if (show)
                {
                    ViewObjectRowFunctionInner(gui, target, rowAction);
                }
            }
            else
            {
                ViewObjectRowFunctionInner(gui, target, rowAction);
            }
        });

        return node;
    }

    /// <summary>
    /// Creates the inner property row content for view objects, populating and rendering child fields.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to render.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    internal static void ViewObjectRowFunctionInner(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        ViewObjectPopulateFunction(target);

        var setup = target.GetViewObjectSetup();

        foreach (var childTarget in setup.ChildTargets)
        {
            // Do not pass parent rowAction
            gui.PropertyField(childTarget, null);
        }
    }

    /// <summary>
    /// Gets or creates a <see cref="ViewObjectSetup"/> cache for the specified property target.
    /// </summary>
    /// <param name="target">The property target to get the setup for.</param>
    /// <returns>The view object setup instance.</returns>
    private static ViewObjectSetup GetViewObjectSetup(this PropertyTarget target)
    {
        if (target.ObjectSetupCache is not ViewObjectSetup setup)
        {
            setup = new ViewObjectSetup(target);
            target.ObjectSetupCache = setup;
        }

        return setup;
    }

    /// <summary>
    /// Creates a child property target for the specified view property.
    /// </summary>
    /// <param name="target">The parent property target.</param>
    /// <param name="type">The type of the child property.</param>
    /// <param name="property">The view property definition.</param>
    /// <returns>The created child property target.</returns>
    internal static PropertyTarget CreateChildTarget(this PropertyTarget target, Type type, ViewProperty property)
    {
        var childTarget = target.GetOrCreateField<IViewObject>(
            property.Name,
            type,
            o => o.GetProperty(property.Name),
            (o, v, ctx) => o.SetProperty(property.Name, v, ctx as ISyncContext));

        return childTarget;
    }
}

#endregion
