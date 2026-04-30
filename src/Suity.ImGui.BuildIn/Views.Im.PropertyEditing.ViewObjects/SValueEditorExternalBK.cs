using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.Views.Im.PropertyEditing.Targets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing.ViewObjects;

/// <summary>
/// External implementation of <see cref="SValueEditorExternal"/> providing custom editors for SKey, SAssetKey, SEnum, and other Suity value types.
/// </summary>
internal class SValueEditorExternalBK : SValueEditorExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="SValueEditorExternalBK"/>.
    /// </summary>
    public static SValueEditorExternalBK Instance { get; } = new SValueEditorExternalBK();

    /// <summary>
    /// Creates a property row editor for <see cref="SKey"/> values with optional expansion and navigation capabilities.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="target">The property target to edit.</param>
    /// <param name="rowAction">Optional action invoked during row rendering.</param>
    /// <returns>The created ImGui node, or null if editing is not possible.</returns>
    public ImGuiNode? SKeyRowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        var keys = target.GetValues().OfType<SKey>();
        var type = keys.FirstOrDefault()?.InputType.ElementType?.Target as DCompond;
        Guid id = keys.FirstOrDefault()?.Id ?? Guid.Empty;
        bool hasPreview = type?.PreviewStructFields?.Any() ?? false;
        bool canExpand = hasPreview && id != Guid.Empty && !target.ValueMultiple;

        ImGuiNode? node = null;

        if (canExpand)
        {
            node = gui.PropertyGroup(target, targetAction: (n, t0, c, p) =>
            {
                if (p.HasFlag(GuiPipeline.Main))
                {
                    rowAction?.Invoke(n, c, GuiPipeline.PreAction);

                    if (c == PropertyGridColumn.Main)
                    {
                        SKeyEditor(gui, t0, act => n.DoValueAction(act));
                    }

                    rowAction?.Invoke(n, c, GuiPipeline.Main);

                    if (c == PropertyGridColumn.Option)
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

                    rowAction?.Invoke(n, c, GuiPipeline.PostAction);
                }
            })
            .OnPropertyGroupExpand(() =>
            {
                var key = keys.FirstOrDefault();
                if (key is null)
                {
                    return;
                }

                if (key.TargetAsset is not IDataAsset dataRowAsset)
                {
                    return;
                }

                var dataRow = dataRowAsset.GetData(true);
                if (dataRow is null)
                {
                    return;
                }

                var type = key.InputType.ElementType;
                var obj = dataRow.Components.Where(o => o.ObjectType == type).FirstOrDefault();
                if (obj is null)
                {
                    return;
                }

                var previewTarget = new RootPropertyTarget("Preview", [obj])
                {
                    ReadOnly = true,
                    CachedTheme = PropertyGridTheme.Preview,
                    Parent = target
                };

                SObjectPropertyFunctions.SObjectRowFunctionPreview(gui, previewTarget, null);
            });
        }
        else
        {
            node = gui.PropertyRow(target, SKeyEditor, (n, c, p) => 
            {
                rowAction?.Invoke(n, c, p);

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
        }

        if (target.Status != TextStatus.Normal)
        {
            node?.OverrideBorder(1f, target.Status.ToColor());
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? SKeyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return EditorTemplates.SelectionEditorTemplate(gui, target, handler,
            placementTextAction: (n, val) =>
            {
                if (val.OfType<ISupportAnalysis>().Select(o => o.Analysis).SkipNull().Any(o => o.Status == TextStatus.Error))
                {
                    target.ErrorSelf = true;
                }

                string? p = null;

                var keys = val.OfType<SKey>();
                if (keys.CountOne() && keys.FirstOrDefault() is { } k && k.TargetAsset is not null)
                {
                    p = EditorUtility.GetBriefStringL(k);
                }
                else
                {
                    SKey? key = keys.FirstOrDefault();
                    if (key is not null)
                    {
                        var field = key.GetField() as DStructField;

                        if (field?.Optional == true)
                        {
                            p = $"{L(key.InputType.ToDisplayString())} (Optional)";
                        }
                        else
                        {
                            p = L(key.InputType.ToDisplayString());
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(p))
                {
                    gui.Text("##placement", p!)
                    .InitClass("placement");
                }
            },
            dragDropFunc: (e, val) =>
            {
                IHasId context = e.Data.GetData<IHasId>();
                if (context is null)
                {
                    return null;
                }

                Asset asset = AssetManager.Instance.GetAsset(context.Id);
                if (asset is null)
                {
                    return null;
                }

                if (val is not SKey key)
                {
                    return null;
                }

                if (!key.GetIsValid(asset))
                {
                    return null;
                }

                return asset.AssetKey;
            });
    }

    /// <inheritdoc/>
    public override ImGuiNode? SAssetKeyEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return EditorTemplates.SelectionEditorTemplate(gui, target, handler,
            placementTextAction: (n, val) =>
            {
                if (val.OfType<ISupportAnalysis>().Select(o => o.Analysis).SkipNull().Any(o => o.Status == TextStatus.Error))
                {
                    target.ErrorSelf = true;
                }

                string? p = null;

                var keys = val.OfType<SAssetKey>();
                if (keys.CountOne() && keys.FirstOrDefault() is { } k && k.TargetAsset is not null)
                {
                    p = EditorUtility.GetBriefStringL(k);
                }
                else
                {
                    SAssetKey? key = keys.FirstOrDefault();
                    if (key is not null)
                    {
                        var field = key.GetField() as DStructField;

                        if (field?.Optional == true)
                        {
                            p = $"{L(key.InputType.ToDisplayString())} (Optional)";
                        }
                        else
                        {
                            p = L(key.InputType.ToDisplayString());
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(p))
                {
                    gui.Text("##placement", p!)
                    .InitClass("placement");
                }
            },
            dragDropFunc: (e, val) =>
            {
                IHasId context = e.Data.GetData<IHasId>();
                if (context is null)
                {
                    return null;
                }

                Asset asset = AssetManager.Instance.GetAsset(context.Id);
                if (asset is null)
                {
                    return null;
                }

                if (val is not SAssetKey key)
                {
                    return null;
                }

                if (!key.GetIsValid(asset))
                {
                    return null;
                }

                return asset.AssetKey;
            });
    }

    /// <inheritdoc/>
    public override ImGuiNode? SEnumEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        SEnum?[] values = target.GetValues().As<SEnum>().ToArray();
        IEnumerable<SEnum> valuesPresent = values.OfType<SEnum>();

        if (!valuesPresent.Any())
        {
            return null;
        }

        var node = gui.DropDownButton($"{target.PropertyName}#enum")
            .InitWidthRest(24)
            .SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target);

        void SetResult(DEnumField field)
        {
            var result = new SEnum(field.ParentType.Definition, field);

            List<object> newValues = [];

            foreach (var _ in values)
            {
                var newValue = Cloner.Clone(result);
                newValues.Add(newValue);
            }

            try
            {
                var action = target.SetValuesAction(newValues);
                handler(action);
            }
            catch (Exception)
            {
            }

            node?.SetClass(PropertyGridThemes.ClassPropertyInput);
        }

        if (node.IsEdited)
        {
            if (node.GetValue<GuiDropDownValue>() is { } v && v.SelectedValue is DEnumField field)
            {
                SetResult(field);
            }

            return node;
        }

        SEnum val = valuesPresent.First();

        // Type mismatch
        if (values.Any(o => o is null || o.InputType != val.InputType))
        {
            return null;
        }

        // Cannot get type
        DEnum? e = val.GetEnum();
        if (e is null)
        {
            return null;
        }

        GuiDropDownValue dropDownValue = node.GetOrCreateValue<GuiDropDownValue>();
        if (!object.ReferenceEquals(dropDownValue.Tag, e))
        {
            dropDownValue.Tag = e;
            dropDownValue.Items.Clear();
            dropDownValue.AddValues(e.EnumFields);
        }

        DEnumField? selectedDEnum = e.GetPublicField(val.Value) as DEnumField;
        dropDownValue.SelectedValue = selectedDEnum;

        node.Text = val.ToString();
        node.SetClass(target.GetPropertyInputClass());

        var enumColor = e?.ViewColor;
        var fieldColor = selectedDEnum?.ViewColor;

        node.OverrideBorder(1, enumColor);
        node.SetFontColor(fieldColor);

        gui.Button("##more", ImGuiIcons.More)
            .InitClass("configBtn")
            .OnClick(async () =>
            {
                if (target.ReadOnly || node.IsReadOnly)
                {
                    return;
                }

                var result = await SimpleSelectionList.ShowSelectionGUIAsync(
                    e.PublicFields.Select(o => new SelectionItem(o.Name) { DisplayText = o.DisplayText, DisplayIcon = o.DisplayIcon }),
                    e.DisplayText,
                    new SelectionOption { Icon = e.Icon, SelectedKey = val.Value, HideEmptySelection = true });

                if (result.IsSuccess && !string.IsNullOrEmpty(result.SelectedKey))
                {
                    if (e.GetPublicField(result.SelectedKey) is DEnumField field)
                    {
                        SetResult(field);
                    }
                }
            });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? SBooleanEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var cTarget = new ConvertedValueTarget<SBoolean, bool>(
            target,
            o => o?.Value is bool b ? b : false,
            v => new SBoolean(v));

        return EditorTemplates.BooleanEditor(gui, cTarget, handler);
    }

    /// <inheritdoc/>
    public override ImGuiNode? SStringEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var cTarget = new ConvertedValueTarget<SString, string>(
            target,
            o => o?.Value?.ToString() ?? string.Empty,
            v => new SString(v));

        return EditorTemplates.StringEditor(gui, cTarget, handler);
    }

    /// <inheritdoc/>
    public override ImGuiNode? STextBlockEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        STextBlock[] values = target.GetValues().As<STextBlock>().ToArray();
        var valuePresets = values.OfType<STextBlock>();

        if (!valuePresets.Any())
        {
            return null;
        }

        var val = valuePresets.First();

        foreach (var v in valuePresets)
        {
            v.TextValue ??= string.Empty;
        }

        var layout = gui.VerticalLayout($"#layout")
            .InitFullWidth()
            .OnContent(() =>
            {
                var node = gui.TextAreaInput($"{target.PropertyName}#textArea", val.TextValue)
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
                            newValue.TextValue = n.Text ?? String.Empty;
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
    public override ImGuiNode? SNumericEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var cTarget = new ConvertedValueTarget<SNumeric, object>(
            target,
            o => o?.Value,
            v => new SNumeric(v));

        Type? type = cTarget.GetValues().FirstOrDefault()?.GetType();
        if (type is null)
        {
            return null;
        }

        var func = PropertyEditorProviderBK.Instance.GetEditorFunction(type, null);

        return func?.Invoke(gui, cTarget, handler);
    }

    /// <inheritdoc/>
    public override ImGuiNode? SDateTimeEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var cTarget = new ConvertedValueTarget<SDateTime, DateTime>(
            target,
            o => o?.Value is DateTime dateTime ? dateTime : default,
            v => new SDateTime(v));

        return EditorTemplates.DateTimeEditor(gui, cTarget, handler);
    }

    /// <inheritdoc/>
    public override ImGuiNode? SPendingValueEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        SUnknownValue? value = target.GetValues().OfType<SUnknownValue>().FirstOrDefault();
        if (value is null)
        {
            return null;
        }

        TypeDefinition? inputType = value.InputType;
        if (inputType is null)
        {
            return null;
        }

        if (inputType?.Target is DPrimative pType)
        {
            switch (pType.TypeCode)
            {
                case TypeCode.Boolean:
                    {
                        var cTarget = new ConvertedValueTarget<SUnknownValue, bool>(
                            target,
                            o => o.Value is bool b ? b : false,
                            v => new SUnknownValue(inputType, v));

                        return EditorTemplates.BooleanEditor(gui, cTarget, handler);
                    }

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    {
                        ConvertedValueTarget<SUnknownValue, object> cTarget = new(
                            target,
                            o => o.Value,
                            v => new SUnknownValue(inputType, v));

                        Type? type = cTarget.GetValues().FirstOrDefault()?.GetType();
                        if (type is null)
                        {
                            return null;
                        }

                        var func = PropertyEditorProviderBK.Instance.GetEditorFunction(type, null);

                        return func?.Invoke(gui, cTarget, handler);
                    }

                case TypeCode.DateTime:
                    {
                        ConvertedValueTarget<SUnknownValue, DateTime> cTarget = new(
                            target,
                            o => o.Value is DateTime dateTime ? dateTime : default,
                            v => new SUnknownValue(inputType, v));

                        return EditorTemplates.DateTimeEditor(gui, cTarget, handler);
                    }

                case TypeCode.String:
                case TypeCode.Char:
                    {
                        ConvertedValueTarget<SUnknownValue, string> cTarget = new(
                            target,
                            o => o.Value?.ToString() ?? string.Empty,
                            v => new SUnknownValue(inputType, v));

                        return EditorTemplates.StringEditor(gui, cTarget, handler);
                    }

                case TypeCode.Object:
                    return null;

                case TypeCode.Empty:
                case TypeCode.DBNull:
                default:
                    return null;
            }
        }

        switch (value.Value)
        {
            case AssetSelection:
                {
                    var cTarget = new ConvertedValueTarget<SUnknownValue, AssetSelection>(
                        target,
                        o => o?.Value as AssetSelection,
                        v => new SUnknownValue(inputType, v));

                    return EditorTemplates.AssetSelectionEditor(gui, cTarget, handler);
                }
            case ITypeDesignSelection:
                {
                    var cTarget = new ConvertedValueTarget<SUnknownValue, ITypeDesignSelection>(
                        target,
                        o => o?.Value as ITypeDesignSelection,
                        v => new SUnknownValue(inputType, v));

                    return EditorTemplates.TypeDesignSelectionEditor(gui, cTarget, handler);
                }
            case ISelection:
                {
                    var cTarget = new ConvertedValueTarget<SUnknownValue, ISelection>(
                        target,
                        o => o?.Value as ISelection,
                        v => new SUnknownValue(inputType, v));

                    return EditorTemplates.SelectionEditor(gui, cTarget, handler);
                }
            case Color:
                {
                    var cTarget = new ConvertedValueTarget<SUnknownValue, Color>(
                        target,
                        o => o?.Value is Color color ? color : Color.White,
                        v => new SUnknownValue(inputType, v));

                    return EditorTemplates.ColorEditor(gui, cTarget, handler);
                }
            default:
                return null;
        }
    }
}