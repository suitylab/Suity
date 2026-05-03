using static Suity.Helpers.GlobalLocalizer;
using Suity;
using Suity.Collections;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using System;
using System.Drawing;
using System.Linq;
using Suity.Editor;
using Suity.Drawing;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides built-in property field rendering and editing functionality for the ImGui-based property editor.
/// Handles arrays, enums, text blocks, and drag-and-drop operations for array items.
/// </summary>
internal class PropertyFieldExternalBK : PropertyFieldExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="PropertyFieldExternalBK"/>.
    /// </summary>
    public static PropertyFieldExternalBK Instance { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether detailed tool buttons (clone, sort up, sort down) are shown for array elements.
    /// </summary>
    public bool DetailedArrayElementToolButton { get; set; } = false;

    private PropertyFieldExternalBK()
    {
    }

    /// <inheritdoc/>
    public override ImGuiNode? PropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
    {
        Type? commonType = target.GetValues().SkipNull().GetCommonType() ?? target.PresetType;
        if (commonType is null)
        {
            return null;
        }

        // Initialize Provider or commonType has changed
        if (target.RowFunction is null || target.EditedType != commonType)
        {
            target.EditedType = commonType;

            var provider = gui.GetPropertyEditorProvider();

            // Initialize ArrayTarget
            if (target.ArrayTarget is null)
            {
                var arrayHandler =
                    provider?.GetArrayHandler(target)
                    ?? PropertyEditorProviderBK.Instance.GetArrayHandler(target);

                if (arrayHandler is { })
                {
                    target.SetupArray(arrayHandler);
                }
            }

            do
            {
                target.RowFunction = null;

                if (target.ArrayTarget != null)
                {
                    target.RowFunction = ArrayPropertyField;
                    break;
                }

                //Type commonType = target.GetValues().SkipNull().GetCommonType() ?? target.EditedType;
                if (commonType.IsEnum)
                {
                    target.RowFunction = EnumPropertyField;
                    break;
                }

                if (target.Attributes?.GetAttribute<IImGuiCustomPropertyEditor>() is { } custom)
                {
                    target.RowFunction = custom.GetRowFunction();
                }

                target.RowFunction ??=
                        provider?.GetRowFunction(commonType, target.PresetType)
                        ?? PropertyEditorProviderBK.Instance.GetRowFunction(commonType, target.PresetType);
            } while (false);

            target.RowFunction ??= NullPropertyField;
        }

        ImGuiNode? node = null;
        if (target.RowFunction is { } func)
        {
            node = func.Invoke(gui, target, rowAction);
        }

        if (!target.IsRoot && node is null)
        {
            node = gui.PropertyRow(target.Id, target.DisplayName, target.Status, rowAction);
        }

        if (node is { })
        {
            node.IsDisabled = target.Disabled;
            node.IsReadOnly = target.ReadOnly;

            // Because PropertyRowData can be dynamically created externally and assigned to FieldExData, prioritize reading the information stored in FieldExData
            if (target.FieldGuiData is PropertyRowData value)
            {
                node.SetValue(value);
            }
            else
            {
                // Create one if reading fails
                value = node.GetOrCreateValue<PropertyRowData>();
                value.Target = target;
                target.FieldGuiData = value;
            }

            node.UpdatePropertyFieldSelection(value);

            return node;
        }

        return null;
    }

    /// <inheritdoc/>
    public override ImGuiNode? NullPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction) => null;

    /// <inheritdoc/>
    public override ImGuiNode EnumPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        return gui.PropertyRow(target, EditorTemplates.EnumEditor, rowAction);
    }

    /// <inheritdoc/>
    public override ImGuiNode? ArrayPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        var arrayTarget = target.ArrayTarget;
        // Cannot get ArrayTarget
        if (arrayTarget is null)
        {
            return target.IsRoot ? null : gui.PropertyRow(target);
        }

        // ArrayTarget is not displayable
        if (!arrayTarget.CanDisplay())
        {
            return target.IsRoot ? null : gui.PropertyRow(target);
        }

        // Cannot get length
        int? len = arrayTarget.GetArrayLengthMax();
        if (!len.HasValue)
        {
            return target.IsRoot ? null : gui.PropertyRow(target);
        }

        var groupNode = gui.PropertyGroup(target, null, (n, inner, column, pipeline) =>
        {
            bool expanded = n.Parent?.GetIsExpanded() == true;

            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        //gui.Text(target.DisplayName).InitClass(PropertyEditorTheme.ClassPropertyInput);
                        break;

                    case PropertyGridColumn.Main:
                        {
                            var innerArrayTarget = inner.ArrayTarget;
                            if (innerArrayTarget is null)
                            {
                                break;
                            }

                            int innerCount = innerArrayTarget.GetMaxArrayLength();

                            //Debug.WriteLine($"count={count}");

                            gui.Button("##add", ImGuiIcons.Add)
                            .InitClass("configBtn")
                            .OnClick(() =>
                            {
                                if (target.ReadOnly || n.IsReadOnly)
                                {
                                    return;
                                }

                                //TODO: When SArray creates SKey, directly pop up multi-selection
                                //if (target.ArrayTarget?.ElementType is { } elementType && typeof(ISelection).IsAssignableFrom(elementType))
                                //{

                                //}

                                var act = innerArrayTarget.SetCountAction([innerCount + 1]);
                                n.DoValueAction(act);
                                len = arrayTarget.GetArrayLengthMax();

                                target.ExpandRequest = true;
                            });

                            gui.Text("text_count", L("Count")).InitClass(PropertyGridThemes.ClassPropertyInput);
                            var countNode = gui.NumericInput("#count", innerCount, 0, 0)
                            .SetEnabled(expanded);

                            if (!expanded && arrayTarget.GetArrays().CountOne())
                            {
                                string? brief = GetArrayBrief(arrayTarget);
                                if (brief != null)
                                {
                                    gui.Text("text_brief", brief).InitClass(PropertyGridThemes.ClassPropertyInput);
                                }
                            }

                            if (countNode.IsEdited && countNode.GetNumericValue<int>() is int newCount)
                            {
                                if (newCount > PropertyFieldExtensions.ArrayMaxCount)
                                {
                                    newCount = PropertyFieldExtensions.ArrayMaxCount;
                                    countNode.SetNumericValue(newCount);
                                }

                                if (newCount != innerCount)
                                {
                                    var act = innerArrayTarget.SetCountAction([newCount]);
                                    n.DoValueAction(act);
                                    len = arrayTarget.GetArrayLengthMax();
                                }

                                countNode.SetClass(PropertyGridThemes.ClassPropertyInput);
                            }
                            else
                            {
                                countNode.SetClass(target.GetPropertyInputClass());
                            }
                        }
                        break;

                    case PropertyGridColumn.Option:
                        {
                            var innerArrayTarget = inner.ArrayTarget;
                            if (innerArrayTarget is null)
                            {
                                break;
                            }

                            int innerCount = innerArrayTarget.GetMaxArrayLength();
                        }
                        break;

                    default:
                        break;
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }

            if (len > PropertyFieldExtensions.ArrayPagingCount && pipeline.HasFlag(GuiPipeline.PostAction) && column == PropertyGridColumn.Option)
            {
                bool mouseIn = gui.CurrentNode?.Parent?.IsMouseIn == true;

                if (mouseIn && expanded)
                {
                    gui.HorizontalLayout("#index_frame")
                    .InitFullWidth()
                    .OnContent(() =>
                    {
                        gui.Text("Start position").InitClass(PropertyGridThemes.ClassPropertyInput);
                        var countNode = gui.NumericInput("#index", value: arrayTarget.StartIndex);
                        var countNumeric = countNode.GetValue<GuiNumericValue>();

                        if (countNumeric is { })
                        {
                            countNumeric.Min = 0;
                            countNumeric.Max = len.Value - PropertyFieldExtensions.ArrayPagingCount;
                            countNumeric.RefreshAtOnce = true;
                        }

                        if (countNode.IsEdited)
                        {
                            arrayTarget.StartIndex = countNode.GetNumericValue<int>() ?? 0;
                        }
                    });
                }
            }
        })
        .OnPropertyGroupExpand(() =>
        {
            if (len <= 0)
            {
                return;
            }

            ImGuiNode? elementNode = null;
            ArrayElementOp? elementOp = null;
            int elementIndex = 0;

            int startIndex = arrayTarget.StartIndex;
            int endIndex = startIndex + PropertyFieldExtensions.ArrayPagingCount;

            for (int i = startIndex; i < endIndex; i++)
            {
                var elementTarget = arrayTarget.GetOrCreateElementTarget(i);
                if (elementTarget is null || PropertyTarget.IsNullOrEmpty(elementTarget))
                {
                    continue;
                }

                elementTarget.Path = new SyncPath(i);

                ImGuiNode? node = PropertyField(gui, elementTarget, (n, column, pipeline) =>
                {
                    bool isReadOnly = n.Parent?.IsReadOnly == true;

                    if (pipeline.HasFlag(GuiPipeline.Main) && column == PropertyGridColumn.Option && !isReadOnly)
                    {
                        // bool mouseIn = gui.CurrentNode?.Parent?.IsMouseIn == true;
                        if (n.Parent?.GetIsPropertyFieldSelected() == true)
                        {
                            gui.Button("delete", ImGuiIcons.Delete)
                            .InitClass("configBtn")
                            .OnClick(n =>
                            {
                                elementNode = n;
                                elementOp = ArrayElementOp.Delete;
                                elementIndex = i;
                            });

                            if (DetailedArrayElementToolButton)
                            {

                                gui.Button("clone", ImGuiIcons.Clone)
                                .InitClass("configBtn")
                                .OnClick(n =>
                                {
                                    elementNode = n;
                                    elementOp = ArrayElementOp.Clone;
                                    elementIndex = i;
                                });

                                gui.Button("sortUp", ImGuiIcons.SortUp)
                                .InitClass("configBtn")
                                .InitEnabled(i > 0)
                                .OnClick(n =>
                                {
                                    elementNode = n;
                                    elementOp = ArrayElementOp.MoveUp;
                                    elementIndex = i;
                                });

                                gui.Button("sortDown", ImGuiIcons.SortDown)
                                .InitClass("configBtn")
                                .InitEnabled(i < len - 1)
                                .OnClick(n =>
                                {
                                    elementNode = n;
                                    elementOp = ArrayElementOp.MoveDown;
                                    elementIndex = i;
                                });
                            }
                        }
                    }
                });

                //node?.InitInputFunctionChain(ImGuiInputSystem.MouseInRefresh);

                if (node != null)
                {
                    node.InitInputFunctionChain(ArrayItemDragDropInput)
                    .InitRenderFunctionChain(ArrayItemDragDropRender);

                    var aryIndex = node.GetOrCreateValue<ArrayIndexValue>();
                    aryIndex.Target = arrayTarget;
                    aryIndex.Index = i;
                }
            }

            if (elementOp.HasValue && elementNode != null)
            {
                IValueAction? act = null;

                switch (elementOp.Value)
                {
                    case ArrayElementOp.Delete:
                        act = arrayTarget.RemoveItemAtAction(elementIndex);
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
                    elementNode.DoValueAction(act);
                    elementNode.QueueRefresh();
                }
            }
        });

        //groupNode.InitInputFunctionChain(ImGuiInputSystem.MouseInRefresh);

        return groupNode;
    }

    /// <summary>
    /// Gets a brief string representation of the first few elements in the array for display purposes.
    /// </summary>
    /// <param name="arrayTarget">The array target to generate the brief description for.</param>
    /// <returns>A string containing the first up to 3 elements separated by commas, or null if the array cannot be summarized.</returns>
    private string? GetArrayBrief(ArrayTarget arrayTarget)
    {
        if (!arrayTarget.GetArrays().CountOne())
        {
            return null;
        }

        var ary = arrayTarget.GetArrays().FirstOrDefault();
        if (ary is null)
        {
            return null;
        }

        var handler = arrayTarget.Handler;
        if (!(handler.GetLength(ary) is { } len) || len <= 0)
        {
            return null;
        }

        bool moreThen3 = false;
        if (len > 3)
        {
            len = 3;
            moreThen3 = true;
        }

        var builder = ImGuiBK.StringBuilderPool.Acquire();
        builder.Clear();

        for (int i = 0; i < len; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            var obj = handler.GetItemAt(ary, i);
            builder.Append((obj?.ToString() ?? string.Empty).ToShortcutString());
        }

        if (moreThen3)
        {
            builder.Append($"...(total {len} items)");
        }

        string result = builder.ToString();
        builder.Clear();
        ImGuiBK.StringBuilderPool.Release(builder);

        return result;
    }

    /// <inheritdoc/>
    public override ImGuiNode? TextBlockPropertyField(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        var values = target.GetValues();
        CheckState state = CheckState.Checked;
        if (target.Optional)
        {
            if (values.OfType<TextBlock>().All(o => o.Text != null))
            {
                state = CheckState.Checked;
            }
            else if (values.OfType<TextBlock>().All(o => o.Text is null))
            {
                state = CheckState.Unchecked;
            }
            else
            {
                state = CheckState.Indeterminate;
            }
        }

        PropertyEditorFunction? func = state == CheckState.Checked ? EditorTemplateExternalBK.Instance.TextBlockEditor : null;
        var node = gui.PropertyRow(target, func, (n, c, p) => 
        {
            rowAction?.Invoke(n, c, p);

            if (p.HasFlag(GuiPipeline.Main))
            {
                if (c == PropertyGridColumn.Prefix && target.Optional)
                {
                    gui.CheckBoxAdvanced("##nullable", state)
                    .SetClass(target.GetPropertyInputClass())
                    .OnChecked((n, v) =>
                    {
                        if (target.ReadOnly)
                        {
                            return;
                        }

                        if (v)
                        {
                            var valueAry = values.As<TextBlock>().ToArray();
                            var newValueAry = new TextBlock[valueAry.Length];
                            for (int i = 0; i < valueAry.Length; i++)
                            {
                                newValueAry[i] = new TextBlock(valueAry[i]?.Text ?? string.Empty);
                            }

                            n.DoValueAction(target.SetValuesAction(newValueAry));
                        }
                        else
                        {
                            TextBlock[] empty = [new TextBlock(null)];
                            n.DoValueAction(target.SetValuesAction(empty));
                        }
                    });
                }
                else if (c == PropertyGridColumn.Option && !n.IsReadOnly && !target.ValueMultiple && state == CheckState.Checked)
                {
                    // bool mouseIn = gui.CurrentNode?.Parent?.IsMouseIn == true;
                    if (n.Parent?.GetIsPropertyFieldSelected() == true)
                    {
                        gui.Button("edit", ImGuiIcons.Open)
                        .InitClass("configBtn")
                        .OnClick(async n =>
                        {
                            var textBlock = values.FirstOrDefault() as TextBlock;
                            if (textBlock != null)
                            {
                                string edited = await DialogUtility.ShowTextBlockDialogAsync(L(target.DisplayName), textBlock.Text, null);
                                if (edited != null)
                                {
                                    object[] value = [new TextBlock(edited)];
                                    n.DoValueAction(target.SetValuesAction(value));
                                }
                            }
                        });
                    }
                }
            }
            else
            {

            }
        });

        if (target.Status != TextStatus.Normal)
        {
            node?.OverrideBorder(1f, target.Status.ToColor());
        }
        else
        {
            node?.OverrideBorder(null, null);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? PropertyEditor(ImGui gui, PropertyTarget target, Action<IValueAction> handler)
    {
        Type? commonType = target.GetValues().SkipNull().GetCommonType() ?? target.PresetType;
        if (commonType is null)
        {
            return null;
        }

        // Initialize Provider or commonType has changed
        if (target.EditorFunction is null || target.EditedType != commonType)
        {
            target.EditedType = commonType;

            var provider = gui.GetPropertyEditorProvider();

            // Initialize ArrayTarget
            if (target.ArrayTarget is null)
            {
                var arrayHandler =
                    provider?.GetArrayHandler(target)
                    ?? PropertyEditorProviderBK.Instance.GetArrayHandler(target);

                if (arrayHandler is { })
                {
                    target.SetupArray(arrayHandler);
                }
            }

            do
            {
                if (target.ArrayTarget != null)
                {
                    target.EditorFunction = ArrayPreviewPropertyEditor;
                    break;
                }

                //Type commonType = target.GetValues().SkipNull().GetCommonType() ?? target.EditedType;
                if (commonType.IsEnum)
                {
                    target.EditorFunction = EditorTemplates.EnumEditor;
                    break;
                }

                target.EditorFunction = provider?.GetEditorFunction(commonType, target.PresetType)
                    ?? PropertyEditorProviderBK.Instance.GetEditorFunction(commonType, target.PresetType);
            } while (false);

            target.EditorFunction ??= NullPropertyEditor;
        }

        ImGuiNode? node = null;
        if (target.EditorFunction is { } func)
        {
            node = func.Invoke(gui, target, handler);
        }

        if (node is { })
        {
            node.IsDisabled = target.Disabled;
            node.IsReadOnly = target.ReadOnly;

            return node;
        }

        return null;
    }

    /// <inheritdoc/>
    public override ImGuiNode? NullPropertyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return null;
    }

    /// <inheritdoc/>
    public override ArrayTarget? SetupArrayTarget(PropertyTarget target, ImGui gui)
    {
        if (target.ArrayTarget is { })
        {
            return target.ArrayTarget;
        }

        var provider = gui.GetPropertyEditorProvider();

        var arrayHandler =
            provider?.GetArrayHandler(target)
            ?? PropertyEditorProviderBK.Instance.GetArrayHandler(target);

        if (arrayHandler is { })
        {
            target.SetupArray(arrayHandler);
        }

        return target.ArrayTarget;
    }

    /// <inheritdoc/>
    public override SyncPathBuilder GetSyncPathBuilder(PropertyTarget? target)
    {
        var builder = new SyncPathBuilder();

        while (target != null && !target.IsRoot)
        {
            if (target.BuildPathAction is { } action)
            {
                action(builder);
                // No need to access upper levels, jump out directly
                break;
            }

            if (target.TypedPath != null)
            {
                builder.Prepend(target.TypedPath);
            }
            else if (target.Path != null)
            {
                builder.Prepend(target.Path);
            }
            else
            {
                builder.Prepend(target.PropertyName);
            }

            target = target.Parent;
        }

        return builder;
    }

    /// <inheritdoc/>
    public override void SetupArrayItemDragDrop(ImGuiNode node, PropertyTarget target, int index)
    {
        var gui = node.Gui ?? throw new NullReferenceException(nameof(node.Gui));
        var arrayTarget = SetupArrayTarget(target, gui);

        node.InitInputFunctionChain(ArrayItemDragDropInput)
        .InitRenderFunctionChain(ArrayItemDragDropRender);

        var aryIndex = node.GetOrCreateValue<ArrayIndexValue>();
        aryIndex.Target = arrayTarget;
        aryIndex.Index = index;
    }

    #region Array System Chain

    /// <summary>
    /// Handles input events for array item drag-and-drop operations, including mouse down, mouse up, and mouse move.
    /// </summary>
    /// <param name="pipeline">The current GUI rendering pipeline stage.</param>
    /// <param name="node">The ImGui node receiving the input event.</param>
    /// <param name="input">The input event data containing mouse state and location.</param>
    /// <param name="baseAction">The base input function to call for child nodes.</param>
    /// <returns>The resulting input state after processing the event.</returns>
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

                    var aryItem = node.Gui.MouseInNodes
                        .Select(o => o.GetValue<ArrayIndexValue>())
                        .OfType<ArrayIndexValue>()
                        .FirstOrDefault();

                    if (aryItem is null)
                    {
                        break;
                    }

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

                        var act = aryTarget.RemoveInsertItemAction(indexFrom, indexTo);
                        if (act != null)
                        {
                            node.DoValueAction(act);
                            node.QueueRefresh();
                        }
                        //RemoveInsertPreviewPath(indexFrom, indexTo);
                    }
                }
                break;

            case GuiEventTypes.MouseMove:
                if (node.IsControlling && input.MouseLocation is { } pos)
                {
                    //Logs.LogDebug("Move");

                    int offset = Math.Abs(pos.X - node.Gui.LastMouseDownLocation.X) +
                                Math.Abs(pos.Y - node.Gui.LastMouseDownLocation.Y);

                    if (offset >= 10)
                    {
                        //node.SetIsControlling(false);

                        var aryIndex = node.GetValue<ArrayIndexValue>();
                        var draggable = node.Parent?.GetOrCreateValue<ArrayDragValue>();
                        if (aryIndex is { } && draggable is { })
                        {
                            // value.DragRequesting = true;
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

    /// <summary>
    /// Renders visual feedback for array item drag-and-drop operations, drawing an insertion indicator line.
    /// </summary>
    /// <param name="pipeline">The current GUI rendering pipeline stage.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output interface for drawing operations.</param>
    /// <param name="dirtyMode">Indicates whether the node needs to be redrawn.</param>
    /// <param name="baseAction">The base render function to call for child nodes.</param>
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

        float h2 = rect.Height * 0.5f;
        float top = rect.Top + h2;
        float bottom = rect.Bottom - h2;

        // +1 is reserved for the expand button position
        float indent = 1;
        float x = rect.X + indent;
        float w = rect.Width - indent;

        if (pos.Y < top)
        {
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Y, w, 3));
            draggable.After = false;
        }
        else if (pos.Y > bottom)
        {
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Bottom - 3, w, 3));
            draggable.After = true;
        }
    }

    /// <summary>
    /// Stores the array index and target reference for a node, used during drag-and-drop operations.
    /// </summary>
    private class ArrayIndexValue
    {
        /// <summary>
        /// Gets or sets the index of the array element.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the array target that owns this element.
        /// </summary>
        public ArrayTarget? Target { get; set; }
    }

    /// <summary>
    /// Stores drag state information for array item reordering operations.
    /// </summary>
    private class ArrayDragValue
    {
        /// <summary>
        /// Gets or sets the array item currently being dragged.
        /// </summary>
        public ArrayIndexValue? DraggingArrayItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the drop target is after the current item.
        /// </summary>
        public bool After { get; set; }

        /// <summary>
        /// Determines whether the specified index value belongs to the same array target as the dragging item.
        /// </summary>
        /// <param name="indexValue">The index value to compare against.</param>
        /// <returns>True if both items belong to the same array target; otherwise, false.</returns>
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

    #endregion

    /// <summary>
    /// Renders a brief preview of array contents for the property editor view.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="target">The value target containing the array data.</param>
    /// <param name="handler">The action handler for value changes.</param>
    /// <returns>Always returns null as this is a preview-only editor.</returns>
    private ImGuiNode? ArrayPreviewPropertyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var arrayTarget = (target as PropertyTarget)?.ArrayTarget;
        // Cannot get ArrayTarget
        if (arrayTarget is null)
        {
            return null;
        }

        if (arrayTarget.GetArrays().CountOne())
        {
            string? brief = GetArrayBrief(arrayTarget);
            if (brief != null)
            {
                gui.Text("text_brief", brief).InitClass(PropertyGridThemes.ClassPropertyInput);
            }
        }

        return null;
    }
}
