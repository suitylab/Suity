using Suity;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using Suity.Views.Im.PropertyEditing.ViewObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Extension methods for <see cref="IPropertyGrid"/> that provide common property grid operations
/// such as navigation, clipboard handling, array manipulation, and value editing.
/// </summary>
public static class IPropertyGridExtensions
{
    internal const string MsgObjectNoSelected = "No object selected";
    internal const string MsgJsonFormatInvalid = "Invalid JSON format";
    internal const string MsgFragmentInvalid = "Invalid XML fragment format";
    internal const string MsgPreviewNotSupported = "Previewing this property is not supported";

    /// <summary>
    /// Handles the "Go to Definition" action for the currently selected property in the grid.
    /// Navigates to the source definition of the selected object or file.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleGotoDefinition(this IPropertyGrid grid)
    {
        var obj = grid.GridData.SelectedField?.Target?.GetValues().FirstOrDefault();
        if (obj is { })
        {
            if (obj is SItem sitem && sitem.GetField() is DField field)
            {
                if (field.GetAttribute<FileSelectionEditorAttribute>() is FileSelectionEditorAttribute fileSelection)
                {
                    string file = sitem.ToString();
                    fileSelection.Navigate(file);
                    return;
                }
            }

            EditorUtility.GotoDefinition(obj);
        }
    }

    /// <summary>
    /// Handles the "Find Reference" action for the currently selected property in the grid.
    /// Searches for all references to the selected object.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleFindReference(this IPropertyGrid grid)
    {
        var obj = grid.GridData.SelectedField?.Target?.GetValues().FirstOrDefault();
        if (obj is { })
        {
            EditorUtility.FindReference(obj);
        }
    }

    /// <summary>
    /// Handles array element operations (delete, clone, move up/down) for the selected array item.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <param name="elementOp">The array element operation to perform.</param>
    public static void HandleArrayOp(this IPropertyGrid grid, ArrayElementOp elementOp)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return;
        }

        var arrayTarget = target.Parent?.ArrayTarget;
        if (arrayTarget is null)
        {
            return;
        }

        int elementIndex = target.Index;
        if (elementIndex < 0)
        {
            return;
        }

        IValueAction? act = null;

        switch (elementOp)
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
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Adds the currently selected property path to the column preview.
    /// Shows a message dialog if previewing is not supported for the selected property.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleAddPreview(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return;
        }

        PreviewPath path = target.ToPreviewPath();

        var preview = grid.Context?.GetService(typeof(IColumnPreview)) as IColumnPreview;
        bool handled = preview?.AddPreviewPath(path) == true;

        if (!handled)
        {
            DialogUtility.ShowMessageBoxAsync(MsgPreviewNotSupported);
        }
    }

    /// <summary>
    /// Removes the currently selected property path from the column preview.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleRemovePreview(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return;
        }

        PreviewPath path = target.ToPreviewPath();

        var preview = grid.Context?.GetService(typeof(IColumnPreview)) as IColumnPreview;
        preview?.RemovePreviewPath(path);
    }

    /// <summary>
    /// Removes all property paths from the column preview.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleRemoveAllPreview(this IPropertyGrid grid)
    {
        var preview = grid.Context?.GetService(typeof(IColumnPreview)) as IColumnPreview;
        preview?.ClearPreviewPaths();
    }

    /// <summary>
    /// Handles the "Go to Field Definition" action for the currently selected property.
    /// Navigates to the source definition of the field metadata.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleGotoFieldDefinition(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;
        object? obj = target?.GetSItemFieldInfomation();

        if (obj is { })
        {
            EditorUtility.GotoDefinition(obj);
        }
    }

    /// <summary>
    /// Handles the repair action for the currently selected property.
    /// Attempts to repair corrupted or invalid SItem or SContainer data.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleRepair(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return;
        }

        IValueAction? act;
        if (target.GetValues().OfType<SItem>().FirstOrDefault() is SContainer)
        {
            act = target?.RepairSContainer();
        }
        else
        {
            act = target?.RepairSItem();
        }

        if (act != null)
        {
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Handles the relocate action for the currently selected SObject property.
    /// Opens a selection dialog to choose a new asset type and updates all selected values.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static async void HandleRelocate(this IPropertyGrid grid)
    {
        var target = grid.GetSItemPropertyTarget();
        if (target is null)
        {
            return;
        }

        if (target.ReadOnly) return;

        int count = target.GetValues().Count();
        if (count == 0) return;

        SObject? firstObj = target.GetValues().OfType<SObject>().FirstOrDefault();
        if (firstObj is null)
        {
            return;
        }

        var inputType = firstObj.InputType;
        var filter = firstObj.GetAssetFilter();

        var result = await firstObj.GetSelectionList(filter).ShowSelectionGUIAsync("Redirect");
        if (!result.IsSuccess || string.IsNullOrEmpty(result.SelectedKey))
        {
            return;
        }

        SObject[] oldObjs = target.GetValues().As<SObject>().ToArray();
        SObject[] newObjs = new SObject[oldObjs.Length];

        for (int i = 0; i < oldObjs.Length; i++)
        {
            newObjs[i] = Cloner.Clone(oldObjs[i]);
            if (newObjs[i] != null)
            {
                newObjs[i].ObjectType = TypeDefinition.Resolve(result.SelectedKey);
            }
        }

        var act = target.SetValuesAction(newObjs.OfType<object>());

        grid.DoAction(act);
    }

    /// <summary>
    /// Handles the copy action for the currently selected property.
    /// Copies the property values to the editor clipboard.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleCopy(this IPropertyGrid grid)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        var values = target.GetValues();
        if (values is null)
        {
            return;
        }

        //PropertyGridClipboardData data = new PropertyGridClipboardData(values);

        EditorServices.Clipboard.SetData(values, true);
    }

    /// <summary>
    /// Handles the paste action for the currently selected property.
    /// Pastes values from the editor clipboard to the property.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandlePaste(this IPropertyGrid grid)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        var items = EditorServices.Clipboard.GetDatas();
        if (items is null)
        {
            return;
        }

        var values = items.Select(o => o.Data).ToArray();

        var act = target.SetValuesAction(values);
        if (act is { })
        {
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Handles the copy text action for the currently selected property.
    /// Copies the property value as text to the system clipboard.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <param name="feature">The advanced edit feature specifying the text format.</param>
    public static void HandleCopyText(this IPropertyGrid grid, ViewAdvancedEditFeatures feature)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        if (!target.GetValues().CountOne())
        {
            DialogUtility.ShowMessageBoxAsync("Please select only one item.");
            return;
        }

        string text = target.GetText(feature) ?? string.Empty;
        EditorUtility.SetSystemClipboardText(text);
    }

    /// <summary>
    /// Handles the paste text action for the currently selected property.
    /// Reads text from the system clipboard and applies it to the property.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <param name="feature">The advanced edit feature specifying the text format.</param>
    public static void HandlePasteText(this IPropertyGrid grid, ViewAdvancedEditFeatures feature)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        EditorUtility.GetSystemClipboardText().ContinueWith(t => 
        {
            string? text = t.Result;
            if (text is null)
            {
                return;
            }

            var act = target.SetText(feature, text);
            if (act is { })
            {
                grid.DoAction(act);
            }
        });
    }

    /// <summary>
    /// Handles the edit text action for the currently selected property.
    /// Opens a dialog to edit the property value as text (JSON, XML, etc.).
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <param name="feature">The advanced edit feature specifying the text format.</param>
    public static async void HandleEditText(this IPropertyGrid grid, ViewAdvancedEditFeatures feature)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    await DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        string text = target.GetText(feature) ?? string.Empty;
        string result = await DialogUtility.ShowTextBlockDialogAsync($"Edit {feature}", text, feature.ToString());
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        var act = target.SetText(feature, result);
        if (act is { })
        {
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Handles the fill random value action for the currently selected property.
    /// Generates random values within the defined numeric range or enum fields.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleFillRandomValue(this IPropertyGrid grid)
    {
        var target = grid.GetSItemPropertyTarget();
        if (target is null)
        {
            DialogUtility.ShowMessageBoxAsync("Only SItem is supported");

            return;
        }

        var first = target.GetValues().FirstOrDefault() as SItem;
        if (first is null)
        {
            return;
        }

        var field = first.GetField();
        if (field is null)
        {
            return;
        }

        //var range = field.GetAttribute<NumericRangeAttribute>();
        //if (range is null)
        //{
        //    DialogUtility.ShowMessageBoxAsync("Need to add numeric range attribute");

        //    return;
        //}

        try
        {
            var rnd = new Random();

            List<object> values = [];

            foreach (var item in target.GetValues().As<SItem>())
            {
                SItem itemNew = Cloner.Clone(item);
                FillRandomValue(rnd, itemNew, field);

                values.Add(itemNew);
            }

            var act = target.SetValuesAction(values);
            if (act is { })
            {
                grid.DoAction(act);
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    private static bool FillRandomValue(Random rnd, SItem item, FieldObject? field) => item switch
    {
        SNumeric num => FillRandomValue(rnd, num, field),
        SEnum sEnum => FillRandomValue(rnd, sEnum, field),
        SObject obj => FillRandomValue(rnd, obj, field),
        SArray ary => FillRandomValue(rnd, ary, field),
        _ => false,
    };

    private static bool FillRandomValue(Random rnd, SNumeric value, FieldObject? field)
    {
        if (field is null)
        {
            return false;
        }

        var rangeAttr = (field as IAttributeGetter)?.GetAttribute<NumericRangeAttribute>();
        if (rangeAttr is null)
        {
            return false;
        }

        double v = rnd.Range((double)rangeAttr.Min, (double)rangeAttr.Max);
        value.Value = v;

        return true;
    }

    private static bool FillRandomValue(Random rnd, SEnum sEnum, FieldObject? field)
    {
        var fieldTypeDef = (field as DStructField)?.FieldType;
        if (fieldTypeDef is null)
        {
            return false;
        }

        var enumType = fieldTypeDef.Target as DEnum;
        if (enumType is null)
        {
            return false;
        }
        
        if (enumType.FieldCount == 0)
        {
            return false;
        }

        if (enumType.FieldCount == 1)
        {
            sEnum.ValueId = enumType.EnumFields.First().Id;
            return true;
        }

        var fields = enumType.EnumFields.ToArray();
        var rndField = fields.GetRandomArrayItem(rnd);

        sEnum.ValueId = rndField.Id;
        return true;
    }

    private static bool FillRandomValue(Random rnd, SObject obj, DField? field)
    {
        if (obj is null)
        {
            return false;
        }

        var objType = obj.ObjectType?.Target as DStruct;
        if (objType is null)
        {
            return false;
        }

        foreach (var innerField in objType.PublicStructFields)
        {
            var innerItem = obj.GetItem(innerField);
            if (innerItem is not null)
            {
                FillRandomValue(rnd, innerItem, innerField);
            }
        }

        return true;
    }

    private static bool FillRandomValue(Random rnd, SArray ary, DField? field)
    {
        if (ary is null)
        {
            return false;
        }

        foreach (var item in ary.Items)
        {
            FillRandomValue(rnd, item, field);
        }

        return true;
    }


    /// <summary>
    /// Handles setting a dynamic action on the currently selected property.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <param name="dynamicType">The type of the dynamic action to set.</param>
    public static void HandleSetDynamcAction(this IPropertyGrid grid, Type? dynamicType)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return;
        }

        if (target.ReadOnly)
        {
            return;
        }

        IValueAction? act = target.SetDynamicAction(dynamicType);

        if (act is { })
        {
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Handles the replace action for the currently selected string property.
    /// Opens a dialog to find and replace text within string values.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleReplace(this IPropertyGrid grid)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        var first = target.GetValues().FirstOrDefault();
        if (first is string || first is SString)
        {
            EditorUtility.CreateImGuiDialog(new ReplaceStringImGui(grid), "Replace String", 600, 300);
        }
        else
        {
            DialogUtility.ShowMessageBoxAsync("Only strings are supported");
        }
    }

    /// <summary>
    /// Handles the fill from asset action for the currently selected property.
    /// Opens a selection dialog to choose a data asset and copies its data to the property.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static async void HandleFillFromAsset(this IPropertyGrid grid)
    {
        var target = grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        var first = target.GetValues().FirstOrDefault();
        if (first is not SObject sobj)
        {
            await DialogUtility.ShowMessageBoxAsync("Only objects are supported");
            return;
        }

        if (TypeDefinition.IsNullOrEmpty(sobj.ObjectType))
        {
            await DialogUtility.ShowMessageBoxAsync("Object type is empty");
            return;
        }

        var selection = new SKeySelection(sobj.ObjectType);
        var result = await selection.GetSelectionList().ShowSelectionGUIAsync("Select data source");
        if (result is null || !result.IsSuccess)
        {
            return;
        }

        if (result.Item is not IDataAsset rowAsset)
        {
            await DialogUtility.ShowMessageBoxAsync("The selected resource is not a data resource");
            return;
        }

        var row = rowAsset.GetData(true);
        if (row is null)
        {
            await DialogUtility.ShowMessageBoxAsync("Data resource has no data");
            return;
        }

        var comp = row.Components.FirstOrDefault(o => o.ObjectType == sobj.ObjectType);
        if (comp is null)
        {
            await DialogUtility.ShowMessageBoxAsync("Data resource does not contain the corresponding data");
            return;
        }

        List<object> values = [Cloner.Clone(comp)];
        var act = target.SetValuesAction(values);
        if (act is { })
        {
            grid.DoAction(act);
        }
    }

    /// <summary>
    /// Gets the property target for the currently selected field in the grid.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <returns>The property target of the selected field, or null if no field is selected.</returns>
    public static PropertyTarget? GetPropertyTarget(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;

        return target;
    }

    /// <summary>
    /// Gets the SItem property target for the currently selected field.
    /// Returns the target only if it is an SItem type or a DesignValue wrapping an SItem.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    /// <returns>The SItem property target, or null if the selection is not an SItem type.</returns>
    public static PropertyTarget? GetSItemPropertyTarget(this IPropertyGrid grid)
    {
        var target = grid.GridData.SelectedField?.Target;
        if (target is null)
        {
            return null;
        }

        if (target.EditedType is { } type)
        {
            if (typeof(SItem).IsAssignableFrom(type))
            {
                return target;
            }

            if (typeof(DesignValue).IsAssignableFrom(type))
            {
                var valueTarget = target.GetOrCreateField<DesignValue, SObject>(
                    nameof(DesignValue.Value),
                    v => v.Value,
                    (v, o, ctx) => v.Value = o);

                return valueTarget;
            }
        }

        return null;
    }



    /// <summary>
    /// Handles removing all attachments from the currently selected SItem property.
    /// Recursively clears attachments from all nested SContainer objects.
    /// </summary>
    /// <param name="grid">The property grid instance.</param>
    public static void HandleRemoveAllAttachments(this IPropertyGrid grid)
    {
        var target = grid.GetSItemPropertyTarget();
        if (target is null)
        {
            DialogUtility.ShowMessageBoxAsync("Only SItem is supported");

            return;
        }

        try
        {
            List<object> values = [];
            foreach (var item in target.GetValues().OfType<SItem>())
            {
                if (item is SContainer container)
                {
                    var clone = Cloner.Clone(container);
                    RemoveAllAttachments(clone);
                    values.Add(clone);
                }
                else
                {
                    values.Add(item);
                }
            }

            var act = target.SetValuesAction(values);
            if (act is { })
            {
                grid.DoAction(act);
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    private static void RemoveAllAttachments(SContainer obj)
    {
        if (obj is null)
        {
            return;
        }

        (obj as SObject)?.ClearAttachments();

        foreach (var childObj in obj.Items.OfType<SContainer>())
        {
            RemoveAllAttachments(childObj);
        }
    }

}
