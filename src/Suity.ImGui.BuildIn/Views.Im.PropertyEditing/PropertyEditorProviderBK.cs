using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views.Im.PropertyEditing.ViewObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides property editors, row functions, and populate functions for the ImGui-based property grid.
/// Scans available assemblies to discover and register property editors, and resolves editors
/// based on type matching for built-in and custom types.
/// </summary>
internal class PropertyEditorProviderBK : IImGuiPropertyEditorProvider
{
    /// <summary>
    /// Gets the singleton instance of <see cref="PropertyEditorProviderBK"/>.
    /// </summary>
    public static PropertyEditorProviderBK Instance { get; } = new();

    private readonly HashSet<object> _internalObjects = [];

    private readonly Dictionary<Type, PropertyPopulateFunction> _populates = [];
    private readonly Dictionary<Type, PropertyRowFunction> _rows = [];
    private readonly Dictionary<Type, PropertyEditorFunction> _editors = [];

    private readonly IListArrayHandler _ilistHandler = new();
    private readonly ViewListArrayHandler _viewListHandler = new();
    private readonly SyncListArrayHandler _syncListHandler = new();
    private readonly SArrayHandler _sarrayHandler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyEditorProviderBK"/> class.
    /// Registers built-in row and editor functions for primitive types and common value types,
    /// and schedules assembly scanning for additional editor discovery.
    /// </summary>
    public PropertyEditorProviderBK()
    {
        AddInternalRowFunction<byte>(new NumericPropertyField<byte>().RowFunction);
        AddInternalRowFunction<sbyte>(new NumericPropertyField<sbyte>().RowFunction);
        AddInternalRowFunction<short>(new NumericPropertyField<short>().RowFunction);
        AddInternalRowFunction<int>(new NumericPropertyField<int>().RowFunction);
        AddInternalRowFunction<uint>(new NumericPropertyField<uint>().RowFunction);
        AddInternalRowFunction<long>(new NumericPropertyField<long>().RowFunction);
        AddInternalRowFunction<ulong>(new NumericPropertyField<ulong>().RowFunction);
        AddInternalRowFunction<float>(new NumericPropertyField<float>().RowFunction);
        AddInternalRowFunction<double>(new NumericPropertyField<double>().RowFunction);
        AddInternalRowFunction<decimal>(new NumericPropertyField<decimal>().RowFunction);

        AddInternalEditorFunction<byte>(new NumericPropertyField<byte>().EditorFunction);
        AddInternalEditorFunction<sbyte>(new NumericPropertyField<sbyte>().EditorFunction);
        AddInternalEditorFunction<short>(new NumericPropertyField<short>().EditorFunction);
        AddInternalEditorFunction<ushort>(new NumericPropertyField<ushort>().EditorFunction);
        AddInternalEditorFunction<int>(new NumericPropertyField<int>().EditorFunction);
        AddInternalEditorFunction<uint>(new NumericPropertyField<uint>().EditorFunction);
        AddInternalEditorFunction<long>(new NumericPropertyField<long>().EditorFunction);
        AddInternalEditorFunction<ulong>(new NumericPropertyField<ulong>().EditorFunction);
        AddInternalEditorFunction<float>(new NumericPropertyField<float>().EditorFunction);
        AddInternalEditorFunction<double>(new NumericPropertyField<double>().EditorFunction);
        AddInternalEditorFunction<decimal>(new NumericPropertyField<decimal>().EditorFunction);

        AddInternalRowFunction<bool>(PropertyGridExtensions.MakePrepositiveRowFunction(EditorTemplateExternalBK.Instance.BooleanEditor));
        AddInternalRowFunction<string>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.StringEditor));
        AddInternalRowFunction<Guid>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.GuidEditor));
        AddInternalRowFunction<DateTime>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.DateTimeEditor));
        //AddInternalRowFunction<TextBlock>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.TextBlockEditor));
        AddInternalRowFunction<TextBlock>(PropertyFieldExternalBK.Instance.TextBlockPropertyField);
        AddInternalRowFunction<Color>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.ColorEditor));
        AddInternalRowFunction<EmptyValue>(PropertyGridExtensions.MakeRowFunction(EditorTemplateExternalBK.Instance.EmptyValueEditor));

        AddInternalEditorFunction<bool>(EditorTemplateExternalBK.Instance.BooleanEditor);
        AddInternalEditorFunction<string>(EditorTemplateExternalBK.Instance.StringEditor);
        AddInternalEditorFunction<Guid>(EditorTemplateExternalBK.Instance.GuidEditor);
        AddInternalEditorFunction<DateTime>(EditorTemplateExternalBK.Instance.DateTimeEditor);
        AddInternalEditorFunction<TextBlock>(EditorTemplateExternalBK.Instance.TextBlockEditor);
        AddInternalEditorFunction<Color>(EditorTemplateExternalBK.Instance.ColorEditor);
        AddInternalEditorFunction<EmptyValue>(EditorTemplateExternalBK.Instance.EmptyValueEditor);

        AddInternalRowFunction<LabelValue>(PropertyGridExtensions.PropertyLabel);

        //ScanAssemblies();

        // Wait for all Dlls to finish loading before scanning
        EditorRexes.EditorBeforeAwake.AddActionListener(ScanAssemblies);
    }

    /// <summary>
    /// Registers an internal row function for a specific type and tracks it to prevent override.
    /// </summary>
    /// <typeparam name="T">The type to associate the row function with.</typeparam>
    /// <param name="func">The row function to register.</param>
    private void AddInternalRowFunction<T>(PropertyRowFunction func)
    {
        _rows.Add(typeof(T), func);
        _internalObjects.Add(func);
    }

    /// <summary>
    /// Registers an internal editor function for a specific type and tracks it to prevent override.
    /// </summary>
    /// <typeparam name="T">The type to associate the editor function with.</typeparam>
    /// <param name="func">The editor function to register.</param>
    private void AddInternalEditorFunction<T>(PropertyEditorFunction func)
    {
        _editors.Add(typeof(T), func);
        _internalObjects.Add(func);
    }


    /// <summary>
    /// Scans available assemblies for classes derived from <see cref="ImGuiPropertyEditor"/> and registers them.
    /// Skips types that are already registered by internal functions.
    /// </summary>
    private void ScanAssemblies()
    {
        foreach (Type editorType in typeof(ImGuiPropertyEditor).GetAvailableClassTypes())
        {
            try
            {
                if (Activator.CreateInstance(editorType) is not ImGuiPropertyEditor editor)
                {
                    Logs.LogError($"Create editor instance failed : {editorType.Name}");
                    continue;
                }

                if (editor.EditedType is null)
                {
                    continue;
                }

                if (_rows.TryGetValue(editor.EditedType, out var current) && !_internalObjects.Contains(current))
                {
                    Logs.LogWarning($"Property editor type is defined : {editor.EditedType.Name}, Can not register editor : {editorType.Name}");
                    continue;
                }

                _rows[editor.EditedType] = editor.RowFunction;
                _populates[editor.EditedType] = editor.PopulateFunction;
                _editors[editor.EditedType] = editor.EditorFunction;
            }
            catch (Exception err)
            {
                err.LogError($"Create editor instance failed : {editorType.Name}");
                continue;
            }
        }
    }

    /// <inheritdoc/>
    public PropertyPopulateFunction? GetPopulateFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        var func = _populates.GetValueSafe(commonType);
        if (func is { })
        {
            return func;
        }

        func = ResolvePopulateFunction(commonType, presetType);
        if (func is { })
        {
            _populates[commonType] = func;
            _internalObjects.Add(func);
        }

        return func;
    }

    /// <inheritdoc/>
    public PropertyRowFunction? GetRowFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        var func = _rows.GetValueSafe(commonType);
        if (func is { })
        {
            return func;
        }

        func = ResolveRowFunction(commonType, presetType);
        if (func is { })
        {
            _rows[commonType] = func;
            _internalObjects.Add(func);
        }

        return func;
    }

    /// <inheritdoc/>
    public PropertyEditorFunction? GetEditorFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        var func = _editors.GetValueSafe(commonType);
        if (func is { })
        {
            return func;
        }

        func = ResolveEditorFunction(commonType, presetType);
        if (func is { })
        {
            _editors[commonType] = func;
            _internalObjects.Add(func);
        }

        return func;
    }

    /// <inheritdoc/>
    public ArrayHandler? GetArrayHandler(PropertyTarget target)
    {
        var type = target.EditedType ?? target.PresetType;

        // Need to check in the following order
        // SArray implements IList interface, so SArray should be checked first

        if (typeof(SArray).IsAssignableFrom(type))
        {
            return _sarrayHandler;
        }

        if (typeof(IViewList).IsAssignableFrom(type))
        {
            return _viewListHandler;
        }

        if (typeof(ISyncList).IsAssignableFrom(type))
        {
            return _syncListHandler;
        }

        if (IsIListType(type))
        {
            return _ilistHandler;
        }

        return null;
    }

    /// <summary>
    /// Resolves a populate function for the given type by checking against known base types.
    /// </summary>
    /// <param name="commonType">The type to find a populate function for.</param>
    /// <param name="presetType">An optional preset type that may influence the resolution.</param>
    /// <returns>A populate function if one is found; otherwise, <c>null</c>.</returns>
    private PropertyPopulateFunction? ResolvePopulateFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        if (typeof(SObject).IsAssignableFrom(commonType))
        {
            return SObjectPropertyFunctions.SObjectPopulateFunction;
        }

        if (typeof(IViewObject).IsAssignableFrom(commonType))
        {
            return ViewObjectPropertyFunctions.ViewObjectPopulateFunction;
        }

        return null;
    }

    /// <summary>
    /// Resolves a row function for the given type by matching against built-in and custom type hierarchies.
    /// </summary>
    /// <param name="commonType">The type to find a row function for.</param>
    /// <param name="presetType">An optional preset type that may influence the resolution.</param>
    /// <returns>A row function if one is found; otherwise, <c>null</c>.</returns>
    private PropertyRowFunction? ResolveRowFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        if (typeof(SObject).IsAssignableFrom(commonType))
        {
            return SObjectPropertyFunctions.SObjectRowFunction;
        }
        if (typeof(SNull).IsAssignableFrom(commonType))
        {
            return SObjectPropertyFunctions.SObjectRowFunction;
        }
        if (typeof(SEnum).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SEnumEditor);
        }
        if (typeof(SBoolean).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakePrepositiveRowFunction(SValueEditorExternalBK.Instance.SBooleanEditor);
        }
        if (typeof(SString).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SStringEditor);
        }
        if (typeof(STextBlock).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.STextBlockEditor);
        }
        if (typeof(SNumeric).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SNumericEditor);
        }
        if (typeof(SDateTime).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SDateTimeEditor);
        }
        if (typeof(SDynamic).IsAssignableFrom(commonType))
        {
            return SObjectPropertyFunctions.SDynamicRowFunction;
        }
        if (typeof(SKey).IsAssignableFrom(commonType))
        {
            //return PropertyGridExtensions.MakeRowFunction(SValueEditorBackend.Instance.SKeyEditor);
            return SValueEditorExternalBK.Instance.SKeyRowFunction;
        }
        if (typeof(SAssetKey).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SAssetKeyEditor);
        }
        if (typeof(SUnknownValue).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(SValueEditorExternalBK.Instance.SPendingValueEditor);
        }

        if (typeof(TextBlock).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(EditorTemplates.TextBlockEditor);
        }
        if (typeof(DesignValue).IsAssignableFrom(commonType))
        {
            return DesignObjectSetups.DesignValueRowFunction;
        }
        if (typeof(FlowNode).IsAssignableFrom(commonType))
        {
            return FlowNodePropertyFunctions.FlowNodeRowFunction;
        }
        if (typeof(IDesignObject).IsAssignableFrom(commonType))
        {
            return DesignObjectSetups.DesignObjectRowFunction;
        }
        if (typeof(IViewObject).IsAssignableFrom(commonType))
        {
            return ViewObjectPropertyFunctions.ViewObjectRowFunction;
        }
        if (typeof(AssetSelection).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(EditorTemplates.AssetSelectionEditor);
        }
        if (typeof(ITypeDesignSelection).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(EditorTemplates.TypeDesignSelectionEditor);
        }
        if (typeof(ISelection).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(EditorTemplates.SelectionEditor);
        }
        if (typeof(EnumSelection).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.MakeRowFunction(EditorTemplates.EnumSelectionEditor);
        }
        if (typeof(LabelValue).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.PropertyLabel;
        }
        if (typeof(TooltipsValue).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.PropertyTooltips;
        }
        if (typeof(ButtonValue).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.PropertyButton;
        }
        if (typeof(MultipleButtonValue).IsAssignableFrom(commonType))
        {
            return PropertyGridExtensions.PropertyMultipleButton;
        }

        return null;
    }

    /// <summary>
    /// Resolves an editor function for the given type by matching against built-in and custom type hierarchies.
    /// </summary>
    /// <param name="commonType">The type to find an editor function for.</param>
    /// <param name="presetType">An optional preset type that may influence the resolution.</param>
    /// <returns>An editor function if one is found; otherwise, <c>null</c>.</returns>
    private PropertyEditorFunction? ResolveEditorFunction(Type commonType, Type? presetType)
    {
        if (commonType is null)
        {
            return null;
        }

        if (typeof(SObject).IsAssignableFrom(commonType))
        {
            //return ViewObjectExtensions.SObjectFieldFunction;
            //return null;
            return SObjectPropertyFunctions.HoriSObjectEditorFunction;
        }
        if (typeof(SEnum).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SEnumEditor;
        }
        if (typeof(SBoolean).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SBooleanEditor;
        }
        if (typeof(SString).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SStringEditor;
        }
        if (typeof(SNumeric).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SNumericEditor;
        }
        if (typeof(SDateTime).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SDateTimeEditor;
        }
        if (typeof(SDynamic).IsAssignableFrom(commonType))
        {
            //return ViewObjectExtensions.SDynamicFieldFunction;
            return null;
        }
        if (typeof(SKey).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SKeyEditor;
        }
        if (typeof(SAssetKey).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SAssetKeyEditor;
        }
        if (typeof(SUnknownValue).IsAssignableFrom(commonType))
        {
            return SValueEditorTemplates.SPendingValueEditor;
        }

        if (typeof(TextBlock).IsAssignableFrom(commonType))
        {
            return EditorTemplates.TextBlockEditor;
        }
        if (typeof(IDesignObject).IsAssignableFrom(commonType))
        {
            //return ViewObjectExtensions.ViewDesignObjectFieldFunction;
            return null;
        }
        if (typeof(DesignValue).IsAssignableFrom(commonType))
        {
            //return ViewObjectExtensions.DesignValueFieldFunction;
            return null;
        }
        if (typeof(IViewObject).IsAssignableFrom(commonType))
        {
            //return ViewObjectExtensions.ViewObjectFieldFunction;
            return null;
        }
        if (typeof(AssetSelection).IsAssignableFrom(commonType))
        {
            return EditorTemplates.AssetSelectionEditor;
        }
        if (typeof(ITypeDesignSelection).IsAssignableFrom(commonType))
        {
            return EditorTemplates.TypeDesignSelectionEditor;
        }
        if (typeof(ISelection).IsAssignableFrom(commonType))
        {
            return EditorTemplates.SelectionEditor;
        }
        if (typeof(EnumSelection).IsAssignableFrom(commonType))
        {
            return EditorTemplates.EnumSelectionEditor;
        }
        if (typeof(LabelValue).IsAssignableFrom(commonType))
        {
            //return PropertyGridExtensions.PropertyLabel;
            return null;
        }

        if (commonType.IsEnum)
        {
            return EditorTemplates.EnumEditor;
        }

        return null;
    }

    /// <summary>
    /// Determines whether the specified type represents a list-like collection (array or <see cref="IList"/>).
    /// </summary>
    /// <param name="editedType">The type to check.</param>
    /// <returns><c>true</c> if the type is an array or implements <see cref="IList"/>; otherwise, <c>false</c>.</returns>
    private bool IsIListType(Type? editedType)
    {
        if (editedType is null)
        {
            return false;
        }

        if (editedType.IsArray)
        {
            return true;
        }

        if (typeof(IList).IsAssignableFrom(editedType))
        {
            return true;
        }

        return false;
    }
}
