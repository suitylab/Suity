using Suity.Collections;
using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// Provides property row rendering and GUI setup functions for <see cref="IDesignObject"/> types.
/// </summary>
public static class DesignObjectSetups
{
    /// <summary>
    /// Creates a property row editor for design objects with design item management.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null.</returns>
    public static ImGuiNode? DesignObjectRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        if (!target.IsRoot)
        {
            return ViewObjectPropertyFunctions.ViewObjectRowFunction(gui, target, rowAction);
        }

        ViewObjectPropertyFunctions.ViewObjectRowFunctionInner(gui, target, rowAction);

        IDesignObject[] ds = target.GetValues().OfType<IDesignObject>().ToArray();
        if (ds.Length == 0)
        {
            return null;
        }

        DesignObjectGui(gui, target, ds);

        return null;
    }

    /// <summary>
    /// Renders the design object GUI including design items and add property button.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target.</param>
    /// <param name="ds">The array of design objects to render.</param>
    public static void DesignObjectGui(ImGui gui, PropertyTarget target, IDesignObject[] ds)
    {
        IDesignObject? first = ds[0];
        if (first is null)
        {
            return;
        }

        string? propName = first?.DesignPropertyName;
        if (string.IsNullOrEmpty(propName))
        {
            return;
        }

        // Spacer
        gui.VerticalLayout("#space1").InitFullWidth().InitHeight(20);

        // Create array target for design items
        var designTarget = target.GetOrCreateField<IDesignObject, SArray>("#SArray", o => o.DesignItems);
        designTarget.SetupArrayTarget(gui);

        if (ds.Length == 1)
        {
            // Single object: render each design item individually by index
            for (int i = 0; i < first!.DesignItems.Count; i++)
            {
                var obj = first.DesignItems.GetValue(i) as SObject;

                DCompond? s = obj?.GetStruct();
                if (s is null)
                {
                    continue;
                }

                var t = designTarget.GetOrCreateField<SArray, DesignValue>(
                    $"#{s.Id}#{i}", CreateDesignObjectGetter(i), CreateDesignObjectSetter(i));

                t.Description = s.ToDisplayTextL();
                t.Icon = obj.GetIcon();
                t.ToolTips = obj.ToToolTipsTextL();

                ImGuiNode? node = gui.PropertyField(t);

                target.Path = new SyncPath(propName, $"[{i}]");
                target.TypedPath = new SyncPath(propName, $"{{{s?.Id ?? Guid.Empty}}}");

                node?.SetupArrayItemDragDrop(designTarget, i);
            }
        }
        else
        {
            // Multiple objects: render only common design item types across all objects
            HashSet<DCompond> types = [];

            // ToArray is needed; previously detected collection modification errors
            var objs = first.DesignItems.GetValues().OfType<SObject>().ToArray();

            foreach (SObject obj in objs)
            {
                DCompond? s = obj.GetStruct();
                if (s is null || types.Contains(s))
                {
                    continue;
                }
                // Only show types that exist in all selected design objects
                if (!ds.All(o => o.DesignItems.ContainsObjectOfType(s)))
                {
                    continue;
                }

                types.Add(s);

                var t = designTarget.GetOrCreateField<SArray, DesignValue>(
                    $"#{s.Id}", CreateDesignObjectGetter(s), CreateDesignObjectSetter(s));

                t.Description = s.ToDisplayTextL();
                t.Icon = obj.GetIcon();
                t.ToolTips = obj.ToToolTipsTextL();

                gui.PropertyField(t);
            }
        }

        // Render "Add Property" button
        gui.VerticalLayout("#designBtnFrame")
        .InitFullWidth()
        .SetPadding(10)
        .OnContent(n =>
        {
            gui.Button("#addDesignBtn", L("Add Property"))
            .InitClass("toolBtn")
            .InitWidth(120)
            .OnClick(async () =>
            {
                TypeDefinition? typeDefinition = first.DesignItems?.InputType?.ElementType;

                if (typeDefinition is null)
                {
                    return;
                }

                var obj = await typeDefinition.GuiCreateObject(AssetFilters.Default, L("Create Property"));
                if (obj is null)
                {
                    return;
                }

                var arrayTarget = designTarget.SetupArrayTarget(gui);
                if (arrayTarget is null)
                {
                    return;
                }

                // Clone the created object for each selected design object
                int count = arrayTarget.GetArrays().Count();
                SObject[] objs = new SObject[count];
                for (int i = 0; i < count; i++)
                {
                    objs[i] = Cloner.Clone(obj);
                }

                var act = arrayTarget.PushItemAtAction(objs);

                var gridData = n.FindValueInHierarchy<PropertyGridData>();
                n.DoValueAction(act, gridData);
            });
        });
    }

    private static RawNode EnsureStyle(this PropertyTarget target)
    {
        if (target.Styles is not RawNode style)
        {
            style = new RawNode();
            target.Styles = style;
        }

        return style;
    }

    //private static PropertyRowAction CreateDesignObjectRowAction(this ImGui gui, PropertyTarget target, IViewDesignObject[] ds, DObjectType type)
    //{
    //    return (n, c, p) =>
    //    {
    //        if (c == PropertyGridColumn.Name && p.HasFlag(GuiPipeLine.PreAction))
    //        {
    //            gui.CheckBox("Enabled", true, true)
    //            .SetClass(PropertyGridTheme.ClassPropertyInput)
    //            .OnEdited(n =>
    //            {
    //                bool enabledNew = n.GetIsChecked();

    //            });
    //        }
    //    };
    //}

    private static Func<SArray, DesignValue?> CreateDesignObjectGetter(DCompond type)
    {
        return new Func<SArray, DesignValue?>(ary =>
        {
            var obj = ary.FindObjectOfType(type);

            return obj != null ? new DesignValue(obj) : null;
        });
    }

    private static Func<SArray, DesignValue?> CreateDesignObjectGetter(int index)
    {
        return new Func<SArray, DesignValue?>(ary =>
        {
            return ary[index] is SObject obj ? new DesignValue(obj) : null;
        });
    }

    private static Action<SArray, DesignValue, ISetterContext?> CreateDesignObjectSetter(DCompond type)
    {
        return new Action<SArray, DesignValue, ISetterContext?>((ary, v, ctx) =>
        {
            int index = ary.IndexOfType(type);
            if (index >= 0)
            {
                ary[index] = v.Value;
            }
        });
    }

    private static Action<SArray, DesignValue, ISetterContext?> CreateDesignObjectSetter(int index)
    {
        return new Action<SArray, DesignValue, ISetterContext?>((ary, v, ctx) =>
        {
            ary[index] = v.Value;
        });
    }

    /// <summary>
    /// Creates a property row editor for <see cref="DesignValue"/> with enable/disable, delete, clone, and reorder capabilities.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node.</returns>
    public static ImGuiNode? DesignValueRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        string? key = target.GetValues().OfType<DesignValue>().FirstOrDefault()?.Value?.ObjectType?.GetShortTypeName() ?? "#SObject";

        var valueTarget = target.GetOrCreateField<DesignValue, SObject>(key, o => o.Value);
        valueTarget.Description = target.Description;
        valueTarget.ToolTips = target.ToolTips;

        // Set custom path
        valueTarget.BuildPathAction = builder =>
        {
            var obj = valueTarget.GetValues().OfType<SObject>().FirstOrDefault();

            builder.Prepend(obj?.ObjectType?.TargetId ?? Guid.Empty);
            builder.Prepend("Attributes");
        };

        if (target.IsRoot)
        {
            SObjectPropertyFunctions.SObjectRowFunctionInner(gui, valueTarget, rowAction);

            return null;
        }

        ArrayElementOp? elementOp = null;
        bool countOne = target.GetValues().CountOne();
        int elementLen = 0;
        int elementIndex = 0;
        if (countOne)
        {
            elementLen = target.Parent?.ArrayTarget?.GetArrayLengthMax() ?? 0;
            elementIndex = valueTarget.GetValues().OfType<SObject>().FirstOrDefault()?.Index ?? 0;
        }
        target.Index = elementIndex;

        valueTarget.EnsureStyle().SetAttribute("HeaderStyle", "Emboss");

        ImGuiNode node = gui.PropertyGroup(valueTarget, targetAction: (n, t, c, p) =>
        {
            //n.InitInputFunctionChain(ImGuiInputSystem.MouseInRefresh);

            if (p.HasFlag(GuiPipeline.PreAction))
            {
                rowAction?.Invoke(n, c, GuiPipeline.PreAction);

                const string propName = "Enabled";

                if (c == PropertyGridColumn.Name)
                {
                    PropertyTarget enabledTarget = valueTarget.GetOrCreateField<SObject, bool>(
                        propName,
                        o => !o.IsComment,
                        (o, v, ctx) => o.IsComment = !v);

                    var enableValues = enabledTarget.GetValues().OfType<bool>().ToArray();
                    var state = CheckState.Indeterminate;
                    if (enableValues.Length == 0)
                    {
                        state = CheckState.Unchecked;
                    }

                    if (enableValues.AllEqual())
                    {
                        state = enableValues.First() ? CheckState.Checked : CheckState.Unchecked;
                    }
                    else
                    {
                        state = CheckState.Indeterminate;
                    }

                    gui.CheckBoxAdvanced(propName, state, state)
                    .SetClass(PropertyGridThemes.ClassPropertyInput)
                    .OnEdited(n =>
                    {
                        bool enabledNew = n.GetIsChecked();
                        var act = enabledTarget.SetValuesAction([enabledNew]);
                        n.DoValueAction(act);
                        n.QueueRefresh();
                    });

                    if (target.Icon is { } icon)
                    {
                        gui.Image("#property_icon", icon)
                            .InitClass("icon");
                    }
                }
            }
            else if (p.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, c, GuiPipeline.Main);

                if (c == PropertyGridColumn.Main)
                {
                    var childValues = t.GetValues();
                    var childFirstVal = childValues.OfType<SObject>().FirstOrDefault();

                    string? brief = null;

                    if (!n.GetIsExpanded() && childValues.CountOne() && childFirstVal is { })
                    {
                        brief = childFirstVal.ToBriefString();
                    }

                    if (!string.IsNullOrWhiteSpace(brief))
                    {
                        gui.Text("#brief", brief!)
                        .InitClass(PropertyGridThemes.GetBriefClass(target.ValueMultiple))
                        .InitFullWidth()
                        .InitCenterVertical();
                    }
                }
                else if (c == PropertyGridColumn.Option && !n.IsReadOnly)
                {
                    if (n.Parent?.GetIsPropertyFieldSelected() == true)
                    {
                        gui.Button("delete", ImGuiIcons.Delete)
                        .InitClass("configBtn")
                        .OnClick(n =>
                        {
                            elementOp = ArrayElementOp.Delete;
                        });

                        if (countOne)
                        {
                            gui.Button("clone", ImGuiIcons.Clone)
                            .InitClass("configBtn")
                            .OnClick(n =>
                            {
                                elementOp = ArrayElementOp.Clone;
                            });

                            gui.Button("sortUp", ImGuiIcons.SortUp)
                            .InitClass("configBtn")
                            .InitEnabled(elementIndex > 0)
                            .OnClick(n =>
                            {
                                elementOp = ArrayElementOp.MoveUp;
                            });

                            gui.Button("sortDown", ImGuiIcons.SortDown)
                            .InitClass("configBtn")
                            .InitEnabled(elementIndex < elementLen - 1)
                            .OnClick(n =>
                            {
                                elementOp = ArrayElementOp.MoveDown;
                            });
                        }
                    }
                }

                rowAction?.Invoke(n, c, GuiPipeline.PostAction);
            }
        })
        .OnPropertyGroupExpand(() =>
        {
            SObjectPropertyFunctions.SObjectRowFunctionInner(gui, valueTarget, rowAction);
        })
        .OnPropertyFieldKeyDown((n, key) =>
        {
            switch (key)
            {
                case "Delete":
                    elementOp = ArrayElementOp.Delete;
                    break;

                default:
                    break;
            }
        });

        if (elementOp.HasValue && target.Parent?.ArrayTarget is { } arrayTarget)
        {
            IValueAction? act = null;

            switch (elementOp.Value)
            {
                case ArrayElementOp.Delete:
                    {
                        var objs = target.GetValues().As<DesignValue>().ToArray();
                        var indexes = objs.Select(o => o?.Value?.Index ?? -1).ToArray();

                        if (indexes.Length > 0)
                        {
                            act = arrayTarget.RemoveItemAtAction(indexes);
                        }
                    }
                    break;

                case ArrayElementOp.Clone:
                    act = arrayTarget.CloneItemAtAction(elementIndex);
                    break;

                case ArrayElementOp.MoveUp:
                    act = arrayTarget.SwapItemAtAction(elementIndex - 1);
                    break;

                case ArrayElementOp.MoveDown:
                    act = arrayTarget.SwapItemAtAction(elementIndex);
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



    private static GuiInputState ArrayItemDragDropInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state == GuiInputState.FullSync)
        {
            return state;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                // Start drag control when left mouse button is pressed
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    if (node.Gui.ControllingNode is null)
                    {
                        node.SetIsControlling(true);
                    }

                    ImGui.MergeState(ref state, GuiInputState.Render);
                }
                break;

            case GuiEventTypes.MouseUp:
                // End drag and calculate drop target when mouse is released
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    node.SetIsControlling(false);
                    ImGui.MergeState(ref state, GuiInputState.Render);

                    var draggable = node.Parent?.GetValue<ArrayDragValue>();
                    if (draggable is null || draggable.DraggingArrayItem is null)
                    {
                        break;
                    }

                    node.Parent?.RemoveValue<ArrayDragValue>();

                    // Find the target array item under the mouse cursor
                    var aryItem = node.Gui.MouseInNodes
                        .Select(o => o.GetValue<ArrayIndexValue>())
                        .OfType<ArrayIndexValue>()
                        .FirstOrDefault();

                    if (aryItem is null)
                    {
                        break;
                    }

                    // Only allow reordering within the same array target
                    if (!draggable.GetIsInSameTarget(aryItem))
                    {
                        break;
                    }

                    if (!(aryItem.Target is { } aryTarget))
                    {
                        break;
                    }

                    if (draggable.DraggingArrayItem != aryItem)
                    {
                        int indexFrom = draggable.DraggingArrayItem.Index;
                        int indexTo = aryItem.Index;
                        if (draggable.After)
                        {
                            indexTo++;
                        }

                        // Execute remove-insert action to reorder items
                        var act = aryTarget.RemoveInsertItemAction(indexFrom, indexTo);
                        if (act != null)
                        {
                            node.DoValueAction(act);
                            node.QueueRefresh();
                        }
                    }
                }
                break;

            case GuiEventTypes.MouseMove:
                // Initiate drag when mouse moves beyond threshold distance
                if (node.IsControlling && input.MouseLocation is { } pos)
                {
                    int offset = Math.Abs(pos.X - node.Gui.LastMouseDownLocation.X) +
                                Math.Abs(pos.Y - node.Gui.LastMouseDownLocation.Y);

                    if (offset >= 10)
                    {
                        var aryIndex = node.GetValue<ArrayIndexValue>();
                        var draggable = node.Parent?.GetOrCreateValue<ArrayDragValue>();
                        if (aryIndex is { } && draggable is { })
                        {
                            draggable.DraggingArrayItem = aryIndex;
                            ImGui.MergeState(ref state, GuiInputState.Layout);
                        }
                    }
                }
                break;

            default:
                break;
        }

        return state;
    }

    private static readonly BrushDef _dragDropBrush = new SolidBrushDef(ImGuiTheme.DefaultDragColor);

    private static void ArrayItemDragDropRender(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeline);

        var arrayItem = node.GetValue<ArrayIndexValue>();
        if (arrayItem is null)
        {
            return;
        }

        var draggable = node.Parent?.GetValue<ArrayDragValue>();
        if (draggable is null)
        {
            return;
        }

        // Only render drop indicator for items in the same target array
        if (!draggable.GetIsInSameTarget(arrayItem))
        {
            return;
        }

        if (draggable.DraggingArrayItem == arrayItem)
        {
            return;
        }

        if (!(node.Gui.Input.MouseLocation is { } pos))
        {
            return;
        }

        var rect = node.GlobalRect;

        if (!rect.Contains(pos))
        {
            return;
        }

        // Calculate drop indicator position based on mouse Y relative to item center
        float h2 = rect.Height * 0.5f;
        float top = rect.Top + h2;
        float bottom = rect.Bottom - h2;

        // +1 reserves space for the expand button
        float indent = 1;
        float x = rect.X + indent;
        float w = rect.Width - indent;

        if (pos.Y < top)
        {
            // Draw indicator above this item
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Y, w, 3));
            draggable.After = false;
        }
        else if (pos.Y > bottom)
        {
            // Draw indicator below this item
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Bottom - 3, w, 3));
            draggable.After = true;
        }
    }

    private class ArrayIndexValue
    {
        public int Index { get; set; }
        public ArrayTarget? Target { get; set; }
    }

    private class ArrayDragValue
    {
        public ArrayIndexValue? DraggingArrayItem { get; set; }
        public bool After { get; set; }

        public bool GetIsInSameTarget(ArrayIndexValue? indexValue)
        {
            var myTarget = DraggingArrayItem?.Target;
            if (myTarget is null)
            {
                return false;
            }

            var target = indexValue?.Target;
            if (target is null)
            {
                return false;
            }

            return myTarget == target;
        }
    }
}

internal class DesignValueContextMenu : RootMenuCommand
{
    public static readonly DesignValueContextMenu Instance = new();

    public DesignValueContextMenu()
    {
        AddCommand("Delete", icon: CoreIconCache.Delete, action: m =>
        {
            ImGuiNode? node = m.Sender as ImGuiNode;
            PropertyTarget? target = node?.GetValue<PropertyRowData>()?.Target;
            if (node is null || target is null)
            {
                return;
            }

            var obj = target.GetValues().As<DesignValue>().FirstOrDefault()?.Value;
            if (obj is null)
            {
                return;
            }

            if (target.Parent?.ArrayTarget is { } arrayTarget)
            {
            }
        });
    }
}