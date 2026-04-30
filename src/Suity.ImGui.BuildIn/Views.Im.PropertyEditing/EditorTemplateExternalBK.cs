using Suity;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides external editor templates for property editing in the ImGui-based property grid.
/// This class defines UI editors for various data types such as booleans, strings, numerics, enums, colors, and selections.
/// </summary>
internal class EditorTemplateExternalBK : EditorTemplateExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="EditorTemplateExternalBK"/>.
    /// </summary>
    public static EditorTemplateExternalBK Instance { get; } = new();

    /// <inheritdoc/>
    public override ImGuiNode BooleanEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues();

        bool? value;
        if (!target.ValueMultiple)
        {
            value = values.OfType<bool>().FirstOrDefault();
        }
        else if (values.Any())
        {
            if (values.CountOne())
            {
                value = values.OfType<bool>().FirstOrDefault();
            }
            else
            {
                value = null;
            }
        }
        else
        {
            value = null;
        }

        CheckState state;
        if (value.HasValue)
        {
            state = value.Value ? CheckState.Checked : CheckState.Unchecked;
        }
        else
        {
            state = CheckState.Indeterminate;
        }

        var node = gui.CheckBoxAdvanced($"{target.PropertyName}#check_box", state)
        .SetClass(target.GetPropertyInputClass())
        .OnEdited(n =>
        {
            bool v = n.GetIsChecked();
            var action = target.SetValuesAction([v]);
            handler(action);
            n.SetClass(PropertyGridThemes.ClassPropertyInput);
        });

        if (node.Parent?.IsCompact == true)
        {
            gui.PropertyTitle(target);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode StringEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues();

        string? value;
        if (!target.ValueMultiple || values.Any())
        {
            value = values.First()?.ToString() ?? string.Empty;
        }
        else
        {
            value = string.Empty;
        }

        var node = gui.StringInput($"{target.PropertyName}#string", value)
        .InitFullWidth()
        .SetClass(target.GetPropertyInputClass())
        .SetValueEditorColor(target)
        .OnEdited(n =>
        {
            string v = n.Text ?? string.Empty;
            var action = target.SetValuesAction([v]);
            handler(action);
            n.SetClass(PropertyGridThemes.ClassPropertyInput);
        });

        if (target.Styles?.GetHintText() is { } hintText && !string.IsNullOrWhiteSpace(hintText))
        {
            node.SetHintText(hintText);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? TextBlockEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues().As<TextBlock>().ToArray();
        var valuePresets = values.OfType<TextBlock>();

        if (!valuePresets.Any())
        {
            return null;
        }

        var val = valuePresets.First();

        //foreach (var v in valuePresets)
        //{
        //    v.Text ??= string.Empty;
        //}

        var layout = gui.VerticalLayout($"#layout")
        .InitFullWidth()
        .OnContent(() =>
        {
            var node = gui.TextAreaInput($"{target.PropertyName}#textArea", val.Text ?? string.Empty)
            .InitFullWidth()
            .InitHeight(100)
            .SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target)
            .OnEdited(n =>
            {
                object[] newValues = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    var newValue = Cloner.Clone(val);
                    newValue.Text = n.Text;   /* ?? string.Empty*/
                    newValues[i] = newValue;
                }

                var action = target.SetValuesAction(newValues);
                handler(action);
                n.SetClass(PropertyGridThemes.ClassPropertyInput);
            });

            gui.VerticalResizer(30, null)
            .InitFullWidth()
            .InitClass("resizer");
        });

        return layout;
    }

    /// <inheritdoc/>
    public override ImGuiNode NumericEditor<T>(ImGui gui, IValueTarget target, Action<IValueAction> handler) where T : struct
    {
        var node = gui.BeginCurrentNode($"{target.PropertyName}#numeric_{typeof(T).Name}")
            .InitFullWidth();

        if (node.IsEdited)
        {
            if (node.GetValue<GuiNumericValue>() is GuiNumericValue<T> v)
            {
                var action = target.SetValuesAction([v.Value]);
                handler(action);
                node.SetClass(PropertyGridThemes.ClassPropertyInput);
            }

            return node;
        }

        var values = target.GetValues();
        string? unit = target.Styles?.GetUnit();

        decimal value;
        if (!target.ValueMultiple)
        {
            value = GuiNumericValue.SafeToDecimal(values.First());
        }
        else if (values.Any())
        {
            value = values.Select(o => GuiNumericValue.SafeToDecimal(o)).Average();
        }
        else
        {
            value = 0;
        }

        if (node.GetValue<GuiNumericValue>() is GuiNumericValue<T> numericValue)
        {
            numericValue.DecimalValue = value;
            numericValue.Unit = unit;
            numericValue.SetText(node);
        }
        else
        {
            numericValue = new(value);
            // Support inheriting numeric range from parent nodes
            var rangeAttr = target.Attributes?.GetAttribute<NumericRangeAttribute>()
                ?? (target.Parent as IValueTarget)?.Attributes?.GetAttribute<NumericRangeAttribute>();

            if (rangeAttr is not null)
            {
                numericValue.Min = rangeAttr.Min;
                numericValue.Max = rangeAttr.Max;
                numericValue.ClampMin = rangeAttr.ClampMin;
                numericValue.ClampMax = rangeAttr.ClampMax;
                numericValue.Increment = rangeAttr.Increment;

                if (rangeAttr.HasColor)
                {
                    numericValue.Color = rangeAttr.Color;
                }

                if (rangeAttr.HasMinMaxColor)
                {
                    numericValue.MinColor = rangeAttr.MinColor;
                    numericValue.MaxColor = rangeAttr.MaxColor;
                }
            }

            numericValue.Unit = unit;
            node.SetupNumericInput(numericValue);
        }

        node.SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target);

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? EnumEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        Type? enumType = target.GetValues()
            .Select(o => o?.GetType())
            .Where(o => o?.IsEnum == true)
            .FirstOrDefault();

        if (enumType is null)
        {
            return null;
        }

        var node = gui.DropDownButton($"{target.PropertyName}#enum_{enumType.Name}")
            .SetFullWidth()
            .SetClass(target.GetPropertyInputClass());

        if (node.IsEdited)
        {
            if (node.GetValue<GuiDropDownValue>() is { } v)
            {
                try
                {
                    var enumValue = v.SelectedValue is Enum m ? m : enumType.GetEnumValues().GetValue(0);
                    var action = target.SetValuesAction([enumValue]);
                    handler(action);
                }
                catch (Exception err)
                {
                    err.LogError();
                }

                node.SetClass(PropertyGridThemes.ClassPropertyInput);
            }

            return node;
        }

        Enum? val = null;
        var values = target.GetValues().Select(o => o is Enum en ? en : null).ToArray();
        IEnumerable<Enum> valuesNotNull = values.SkipNull()!;

        // Apply values to editors
        if (!valuesNotNull.Any())
        {
            val = null;
        }
        else
        {
            Enum firstVal = valuesNotNull.First();
            val = firstVal;
        }

        var dropDownValue = node.GetOrCreateValue<GuiDropDownValue>();
        if (!object.ReferenceEquals(dropDownValue.Tag, enumType))
        {
            dropDownValue.Tag = enumType;
            dropDownValue.SetupEnumType(enumType);
        }

        dropDownValue.SelectedValue = val;

        node.Text = dropDownValue.SelectedItem?.ToString() ?? string.Empty;
        node.SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target);

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? GuidEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues().ToArray();
        var valuePresent = values.OfType<Guid>();

        Guid val = valuePresent.FirstOrDefault();

        //var node = gui.HorizontalFrame($"{target.PropertyName}#value")
        //    .InitInputFunctionChain(SelectionInput)
        //    .SetClass(PropertyGridTheme.GetPropertyInputClass(target.ValueMultiple))
        //    .InitFullWidth();

        var node = gui.StringInput($"{target.PropertyName}#string", val.ToString())
            .InitFullWidth()
            .InitReadonly(true)
            .SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target);

        //node.OnContent(() =>
        //{
        //    var textNode = gui.Text("##val", val.ToString());
        //});

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode DateTimeEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues().Select(o => o is DateTime time ? time : default).ToArray();

        string value;
        if (!target.ValueMultiple || values.Any())
        {
            value = values.First().ToString();
        }
        else
        {
            value = default(DateTime).ToString();
        }

        var node = gui.StringInput($"{target.PropertyName}#datetime", value)
        .InitFullWidth()
        .SetClass(target.GetPropertyInputClass())
        .SetValueEditorColor(target)
        .OnEdited(n =>
        {
            string v = n.Text ?? string.Empty;

            if (DateTime.TryParse(v, out DateTime result))
            {
                var action = target.SetValuesAction([result]);
                handler(action);
                n.SetClass(PropertyGridThemes.ClassPropertyInput);
                n.Text = result.ToString();
            }
            else
            {
                n.Text = value;
            }
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode ColorEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues().Select(o => o is Color c ? c : default).ToArray();

        Color val = values.Length > 0 ? values[0] : Color.White;

        string str;
        if (!target.ValueMultiple || values.Any())
        {
            str = ColorTranslator.ToHtml(values.First());
        }
        else
        {
            str = string.Empty;
        }

        var node = gui.StringInput($"{target.PropertyName}#color", str)
        .InitWidthRest(48)
        .SetClass(target.GetPropertyInputClass())
        .SetValueEditorColor(target)
        .OnEdited(n =>
        {
            string v = n.Text ?? string.Empty;

            Color c = Color.Empty;

            if (!string.IsNullOrWhiteSpace(v))
            {
                try
                {
                    c = ColorTranslator.FromHtml(v);
                }
                catch (Exception err)
                {
                    err.LogError();

                    n.Text = string.Empty;
                }
            }

            var action = target.SetValuesAction([c]);
            handler(action);
            n.SetClass(PropertyGridThemes.ClassPropertyInput);

            n.Text = c != Color.Empty ? ColorTranslator.ToHtml(c) : string.Empty;
        });

        void ApplyValue(Color c, bool final)
        {
            List<object> newValues = [c];

            var action = target.SetValuesAction(newValues, values.OfType<object>().ToArray());
            action.Preview = !final;

            handler(action);

            node.SetClass(PropertyGridThemes.ClassPropertyInput);
        }

        gui.Button("##more", ImGuiIcons.More)
        .InitClass("configBtn")
        .OnClick(async n =>
        {
            if (target.ReadOnly || node.IsReadOnly)
            {
                return;
            }

            if (n.Gui.Context is IGraphicColorPicker colorPicker)
            {
                var rect = n.GlobalRect;
                var dropDownRect = new RectangleF(rect.X, rect.Bottom, rect.Width, 100);
                if (dropDownRect.Width < 100)
                {
                    dropDownRect.Width = 100;
                }

                colorPicker.ShowColorPicker(dropDownRect.ToInt(), val, (result, final) => 
                {
                    //if (values.Any(v => v != result))
                    //{
                        ApplyValue(result, final);
                    //}
                });
            }

            //var result = await DialogUtility.ShowColorSelectDialogAsync(val);
            //if (result.HasValue)
            //{
            //    ApplyValue(result.Value);
            //}
        });

        gui.Button("##reset", ImGuiIcons.Delete)
        .InitClass("configBtn")
        .OnClick(() =>
        {
            if (target.ReadOnly || node.IsReadOnly)
            {
                return;
            }

            ApplyValue(Color.Empty, true);
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode EmptyValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var node = gui.VerticalLayout("empty")
        .InitFullSize();

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode ButtonValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var node = gui.Button($"{target.PropertyName}#button", target.DisplayName)
        .InitFullWidth()
        .SetClass("toolBtn")
        .SetValueEditorColor(target)
        .OnClick(async () =>
        {
            if (target is ISupportStyle style && style.Styles?.GetConfirm() is { } confirmMsg)
            {
                bool ok = await DialogUtility.ShowYesNoDialogAsync(confirmMsg);
                if (ok)
                {
                    target.SetValues([ButtonValue.Clicked]);
                }
            }
            else
            {
                target.SetValues([ButtonValue.Clicked]);
            }
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? SelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return SelectionEditorTemplate(gui, target, handler, null);
    }

    /// <inheritdoc/>
    public override ImGuiNode? AssetSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return SelectionEditorTemplate(gui, target, handler,
            placementTextAction: (n, val) =>
            {
                string? p;
                if (val.CountOne())
                {
                    p = EditorUtility.GetBriefStringL(val.FirstOrDefault());
                }
                else
                {
                    p = (val.FirstOrDefault() as AssetSelection)?.ContentTypeName;
                }

                if (!string.IsNullOrWhiteSpace(p))
                {
                    gui.Text("##placement", p!)
                    .InitClass("placement");
                }
            },
            dragDropFunc: (e, val) =>
            {
                IHasId idContext = e.Data.GetData<IHasId>();
                if (idContext is null)
                {
                    return null;
                }

                Asset? asset = AssetManager.Instance.GetAsset(idContext.Id);
                if (asset is null)
                {
                    return null;
                }

                if (val is not AssetSelection selection)
                {
                    return null;
                }

                if (!selection.GetIsValid(asset))
                {
                    return null;
                }

                return asset.AssetKey;
            });
    }

    /// <inheritdoc/>
    public override ImGuiNode? TypeDesignSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return SelectionEditorTemplate(gui, target, handler,
            placementTextAction: (n, val) =>
            {
                var placement = target.Optional ? "Type (Optional)" : "Type";
                gui.Text("##placement", placement)
                .InitClass("placement");
            },
            dragDropFunc: (e, val) =>
            {
                IHasId context = e.Data.GetData<IHasId>();
                if (context is null)
                {
                    return null;
                }

                Asset? asset = AssetManager.Instance.GetAsset(context.Id);
                if (asset is null)
                {
                    return null;
                }

                if (asset.ParentAsset is not DTypeFamily)
                {
                    return null;
                }

                if (val is not ITypeDesignSelection)
                {
                    return null;
                }

                return asset.AssetKey;
            });
    }

    /// <inheritdoc/>
    /// <param name="gui">The ImGui instance used for rendering UI elements.</param>
    /// <param name="target">The value target representing the property being edited.</param>
    /// <param name="handler">The action to invoke when a value change occurs.</param>
    /// <param name="placementTextAction">Optional callback to render additional placement text next to the selection.</param>
    /// <param name="dragDropFunc">Optional callback to handle drag-and-drop operations for setting selection values.</param>
    /// <returns>An <see cref="ImGuiNode"/> representing the selection editor UI, or null if the selection is invalid.</returns>
    public override ImGuiNode? SelectionEditorTemplate(
        ImGui gui,
        IValueTarget target,
        Action<IValueAction> handler,
        SelectionPlacementTextFunc? placementTextAction = null,
        SelectionDragDropFunc? dragDropFunc = null)
    {
        target.ErrorSelf = false;

        var values = target.GetValues().As<ISelection>();

        ISelection? val = values.FirstOrDefault(o => o is not null);
        if (val is null)
        {
            try
            {
                val = (target.EditedType ?? target.PresetType)?.CreateInstanceOf() as ISelection;
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        if (val is null)
        {
            target.ErrorSelf = true;
            return null;
        }

        if (!values.All(o => o?.IsValid == true))
        {
            target.ErrorSelf = true;
        }

        var node = gui.HorizontalFrame($"{target.PropertyName}#value")
            .InitInputFunctionChain(SelectionInput)
            .SetClass(target.GetPropertyInputClass())
            .InitWidthRest(24);

        void ApplyKey(string key)
        {
            List<object> newValues = [];

            foreach (var _ in values)
            {
                var newValue = Cloner.Clone(val);
                newValue.SelectedKey = key;

                newValues.Add(newValue);
            }

            var action = target.SetValuesAction(newValues);
            handler(action);

            node.SetClass(PropertyGridThemes.ClassPropertyInput);
        }

        node.OnContent(() =>
        {
            Image? icon = EditorUtility.GetIcon(val)
                ?? EditorUtility.GetIcon(val.GetList()?.GetItem(val.SelectedKey)?.ToDisplayIcon());

            if (icon is { })
            {
                gui.Image("##icon", icon)
                .InitSize(16, 16);
            }

            var textNode = gui.Text("##val", val?.ToString() ?? string.Empty);

            placementTextAction?.Invoke(node, values.SkipNull());

            textNode.SetValueEditorColor(target);

            if (val is IHasId idc && EditorObjectManager.Instance.GetObject(idc.Id) is IViewColor c)
            {
                textNode.FontColor = c.ViewColor;
            }
        }).OnDragOver(dropEvent =>
        {
            if (node.IsReadOnly || target.ReadOnly)
            {
                dropEvent.SetNoneEffect();

                return;
            }

            string? result = dragDropFunc?.Invoke(dropEvent, val);
            if (!string.IsNullOrEmpty(result))
            {
                dropEvent.SetLinkEffect();
            }
            else
            {
                dropEvent.SetNoneEffect();
            }
        }).OnDragDrop(data =>
        {
            if (node.IsReadOnly || target.ReadOnly)
            {
                return;
            }

            string? result = dragDropFunc?.Invoke(data, val);
            if (!string.IsNullOrEmpty(result))
            {
                ApplyKey(result!);
            }
        }).OnDoubleClick(() =>
        {
            EditorUtility.GotoDefinition(val);
        });

        gui.Button("##more", ImGuiIcons.More)
        .InitClass("configBtn")
        .OnClick(async () =>
        {
            if (node.IsReadOnly || target.ReadOnly)
            {
                return;
            }

            var list = val.GetList();

            if (list is null)
            {
                return;
            }

            var result = await list.ShowSelectionGUIAsync(target.DisplayName,
                new SelectionOption
                {
                    SelectedKey = val.SelectedKey,
                    InitialHideItems = target.Styles?.GetInitialHidden() == true,
                });

            if (result.IsSuccess)
            {
                ApplyKey(result.SelectedKey);
            }
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? EnumSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        EnumSelection? val = null;
        var values = target.GetValues().Select(o => o is EnumSelection ? (EnumSelection?)o : null).ToArray();

        IEnumerable<EnumSelection> valuesNotNull = values.Where(o => o.HasValue).Select(o => o!.Value);

        // Apply values to editors
        if (!valuesNotNull.Any())
        {
            val = null;
        }
        else
        {
            EnumSelection firstVal = valuesNotNull.First();
            val = firstVal;
        }

        if (!val.HasValue)
        {
            return null;
        }

        var node = gui.DropDownButton($"{target.PropertyName}#enumSelection")
            .SetFullWidth()
            .SetClass(target.GetPropertyInputClass());

        if (node.IsEdited)
        {
            if (node.GetValue<GuiDropDownValue>() is { } v)
            {
                try
                {
                    string key = (v.SelectedItem?.Value as ISelectionItem)?.SelectionKey ?? string.Empty;
                    var enumValue = new EnumSelection(val.Value.TypeName, val.Value.List, key);
                    var action = target.SetValuesAction([enumValue]);
                    handler(action);
                }
                catch (Exception err)
                {
                    err.LogError();
                }

                node.SetClass(PropertyGridThemes.ClassPropertyInput);
            }

            return node;
        }

        GuiDropDownValue dropDownValue = node.GetOrCreateValue<GuiDropDownValue>();
        if (!Object.Equals(dropDownValue.Tag, val.Value.List))
        {
            dropDownValue.Tag = val.Value.List;
            dropDownValue.Items.Clear();
            dropDownValue.Items.AddRange(val.Value.List.GetItems().Select(o => new GuiDropDownItem(o, o.ToDisplayText())));
        }

        dropDownValue.SelectedValue = val.Value.EnumKey;

        node.Text = dropDownValue.SelectedItem?.ToString() ?? string.Empty;
        node.SetClass(target.GetPropertyInputClass());

        return node;
    }

    /// <inheritdoc/>
    /// <param name="pipeline">The GUI pipeline processing the input.</param>
    /// <param name="node">The ImGui node receiving the input event.</param>
    /// <param name="input">The input event data.</param>
    /// <param name="baseAction">The base input function to call for default processing.</param>
    /// <returns>The input state indicating how the input should be synchronized.</returns>
    public override GuiInputState SelectionInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (node.Gui.Input.DragEvent is { })
        {
            return GuiInputState.FullSync;
        }

        if (input.EventType == GuiEventTypes.MouseUp && node.Gui.IsDoubleClick)
        {
            return GuiInputState.FullSync;
        }

        return state;
    }
}
