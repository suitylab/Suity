using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Reflecting;
using Suity.Selecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Values;

/// <summary>
/// Extension methods for SItem and related types.
/// </summary>
public static class SItemExtensions
{
    internal static SObjectController CreateController(this DCompond nativeClass)
    {
        if (typeof(SObjectController).IsAssignableFrom(nativeClass.NativeType))
        {
            return (SObjectController)nativeClass.NativeType.CreateInstanceOf();
        }
        else
        {
            return null;
        }
    }

    #region Asset

    /// <summary>
    /// Gets the context asset for an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public static Asset GetContextAsset(this SItem item)
    {
        IHasAsset context = item?.RootContext as IHasAsset;
        return context?.TargetAsset;
    }

    /// <summary>
    /// Gets the asset filter for an SItem.
    /// </summary>
    /// <param name="value">The SItem.</param>
    public static IAssetFilter GetAssetFilter(this SItem value)
    {
        bool instance = value.GetInstanceAccessMode();

        Asset asset = GetContextAsset(value);
        if (asset != null)
        {
            return asset.GetInstanceFilter(instance);
        }
        else
        {
            return AssetFilters.Default;
        }
    }

    /// <summary>
    /// Gets the data row from an SKey.
    /// </summary>
    /// <param name="key">The SKey.</param>
    /// <param name="tryLoadStorage">Whether to try loading storage.</param>
    public static IDataItem GetDataRow(this SKey key, bool tryLoadStorage = true)
    {
        return (key.TargetAsset as IDataAsset)?.GetData(tryLoadStorage);
    }

    /// <summary>
    /// Gets the data from an SKey.
    /// </summary>
    /// <param name="key">The SKey.</param>
    public static SObject GetData(this SKey key)
    {
        var type = key.InputType;
        if (type.IsDataLink)
        {
            type = type.ElementType;
        }

        return key.GetDataRow()?.Components.FirstOrDefault(o => o.ObjectType == type);
    }

    /// <summary>
    /// Gets the component from an IDataAsset.
    /// </summary>
    /// <typeparam name="T">The type of SObjectController.</typeparam>
    /// <param name="row">The IDataAsset.</param>
    /// <param name="tryLoadStorage">Whether to try loading storage.</param>
    public static T GetComponent<T>(this IDataAsset row, bool tryLoadStorage = true) where T : SObjectController
    {
        return row.GetData(tryLoadStorage)?.GetComponent<T>();
    }

    /// <summary>
    /// Gets the component from an IDataItem.
    /// </summary>
    /// <typeparam name="T">The type of SObjectController.</typeparam>
    /// <param name="row">The IDataItem.</param>
    public static T GetComponent<T>(this IDataItem row) where T : SObjectController
    {
        return row.Components
            .Select(o => o.Controller)
            .OfType<T>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the component from an SKey.
    /// </summary>
    /// <typeparam name="T">The type of SObjectController.</typeparam>
    /// <param name="key">The SKey.</param>
    /// <param name="tryLoadStorage">Whether to try loading storage.</param>
    public static T GetComponent<T>(this SKey key, bool tryLoadStorage = true) where T : SObjectController
    {
        return key.GetDataRow(tryLoadStorage)?.GetComponent<T>();
    }

    #endregion

    #region Resolve

    /// <summary>
    /// Gets the native type from a TypeDefinition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    public static Type GetNativeType(this TypeDefinition typeInfo)
        => SValueExternal._external.GetNativeType(typeInfo);

    /// <summary>
    /// Gets the DStruct from an SObject with the specified filter.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    /// <param name="filter">The asset filter.</param>
    public static DCompond GetStruct(this SObject obj, IAssetFilter filter)
    {
        if (obj is null)
        {
            return null;
        }

        return GetStruct(obj.ObjectType, filter);
    }

    /// <summary>
    /// Gets the DStruct from an SObject.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    public static DCompond GetStruct(this SObject obj)
    {
        if (obj is null)
        {
            return null;
        }

        IAssetFilter filter = obj.GetAssetFilter();

        return GetStruct(obj.ObjectType, filter);
    }

    /// <summary>
    /// Gets the DStruct from a TypeDefinition.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="filter">The asset filter.</param>
    public static DCompond GetStruct(this TypeDefinition type, IAssetFilter filter)
    {
        return type?.GetTarget(filter) as DCompond;
    }

    /// <summary>
    /// Gets the DEnum from an SEnum.
    /// </summary>
    /// <param name="enum">The SEnum.</param>
    public static DEnum GetEnum(this SEnum @enum)
    {
        return GetEnum(@enum?.InputType);
    }

    /// <summary>
    /// Gets the DEnum from a TypeDefinition.
    /// </summary>
    /// <param name="type">The type definition.</param>
    public static DEnum GetEnum(this TypeDefinition type)
    {
        return type?.Target as DEnum;
    }

    /// <summary>
    /// Gets the parent field of an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public static DStructField GetParentField(this SItem item)
    {
        if (item is null)
        {
            return null;
        }

        var filter = item.GetAssetFilter();

        return GetParentField(item, filter);
    }

    /// <summary>
    /// Gets the parent field of an SItem with the specified filter.
    /// </summary>
    /// <param name="item">The SItem.</param>
    /// <param name="filter">The asset filter.</param>
    public static DStructField GetParentField(this SItem item, IAssetFilter filter)
        => SValueExternal._external.GetParentField(item, filter);

    /// <summary>
    /// Gets the type from the parent of an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public static TypeDefinition GetTypeFromParent(this SItem item)
    {
        if (item is null)
        {
            return null;
        }

        var filter = item.GetAssetFilter();

        return GetTypeFromParent(item, filter);
    }

    /// <summary>
    /// Gets the type from the parent of an SItem with the specified filter.
    /// </summary>
    /// <param name="item">The SItem.</param>
    /// <param name="filter">The asset filter.</param>
    public static TypeDefinition GetTypeFromParent(this SItem item, IAssetFilter filter)
        => SValueExternal._external.GetTypeFromParent(item, filter);

    /// <summary>
    /// Finds an owner of the specified type by searching upward in the hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of owner to find.</typeparam>
    /// <param name="value">The SItem to start from.</param>
    public static T FindOwnerUpward<T>(this SItem value)
    {
        if (value is SContainer { Context: T t })
        {
            return t;
        }

        SContainer container = value.Parent;

        while (container != null)
        {
            if (container.Context is T t2)
            {
                return t2;
            }
            else
            {
                container = container.Parent;
            }
        }

        return default;
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a value item from a type definition.
    /// </summary>
    /// <param name="definition">The type definition.</param>
    public static SItem CreateValue(this TypeDefinition definition)
        => SValueExternal._external.CreateValue(definition);

    /// <summary>
    /// Creates a value item from a DType.
    /// </summary>
    /// <param name="type">The DType.</param>
    public static SItem CreateValue(this DType type)
        => SValueExternal._external.CreateValue(type);

    /// <summary>
    /// Creates an SObject from a DCompond.
    /// </summary>
    /// <param name="objectType">The DCompond object type.</param>
    public static SObject CreateObject(this DCompond objectType)
        => SValueExternal._external.CreateObject(objectType);

    /// <summary>
    /// Creates an SObject from a DCompond with input type and optional text.
    /// </summary>
    /// <param name="objectType">The DCompond object type.</param>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="inputText">Optional input text.</param>
    public static SObject CreateObject(this DCompond objectType, TypeDefinition inputType, string inputText = null)
        => SValueExternal._external.CreateObject(objectType, inputType, inputText);

    /// <summary>
    /// Creates an SObject from type definitions with filter and optional text.
    /// </summary>
    /// <param name="objectType">The object type definition.</param>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="filter">The asset filter.</param>
    /// <param name="inputText">Optional input text.</param>
    public static SObject CreateObject(this TypeDefinition objectType, TypeDefinition inputType, IAssetFilter filter, string inputText = null)
        => SValueExternal._external.CreateObject(objectType, inputType, filter, inputText);

    /// <summary>
    /// Creates a default SObject from an input type with filter and optional text.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="filter">The asset filter.</param>
    /// <param name="inputText">Optional input text.</param>
    public static SObject CreateDefaultObject(this TypeDefinition inputType, IAssetFilter filter, string inputText = null)
        => SValueExternal._external.CreateDefaultObject(inputType, filter, inputText);

    /// <summary>
    /// Creates an SArray from a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    public static SArray CreateArray(this TypeDefinition typeInfo)
        => SValueExternal._external.CreateArray(typeInfo);

    /// <summary>
    /// Creates an empty SObject from an input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public static SObject CreateEmptyObject(this TypeDefinition inputType)
        => SValueExternal._external.CreateEmptyObject(inputType);

    /// <summary>
    /// Creates a controller SObject with the specified controller type.
    /// </summary>
    /// <typeparam name="T">The type of SObjectController.</typeparam>
    /// <param name="inputType">The input type definition.</param>
    public static T CreateControllerObject<T>(this TypeDefinition inputType)
        where T : SObjectController, new()
    {
        var type = typeof(T);

        var objType = TypeDefinition.FromNative(type);

        // Requiring DCompond will cause failure in early system structure construction, because DCompond hasn't been resolved yet.
        var dtype = objType?.Target as DCompond;
            // ?? throw new InvalidOperationException($"Can not get {nameof(DCompond)} from {type.Name}");

        SObject obj;
        if (dtype != null)
        {
            obj = dtype.CreateObject(inputType);
        }
        else if (objType != null)
        {
            obj = new SObject(objType, inputType, (SObjectController)type.CreateInstanceOf());
        }
        else
        {
            obj = null;
        }

        return obj?.Controller as T;
    }

    #endregion

    #region Repair

    /// <summary>
    /// Ensures the input type of an SItem matches the specified type, converting the value if necessary.
    /// </summary>
    /// <param name="item">The SItem.</param>
    /// <param name="type">The target type definition.</param>
    public static void EnsureInputType(this SItem item, TypeDefinition type)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (type is null)
        {
            return;
        }

        item.RepairInputTypeForce(type);
        item.AutoConvertValue();
    }

    /// <summary>
    /// Repairs an SObject to match the specified type.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    /// <param name="type">The target type definition.</param>
    public static void Repair(this SObject obj, TypeDefinition type = null)
        => SValueExternal._external.Repair(obj, type);

    /// <summary>
    /// Repairs a container and all its children recursively.
    /// </summary>
    /// <param name="container">The SContainer.</param>
    /// <param name="type">The target type definition.</param>
    /// <param name="limit">The recursion limit.</param>
    public static void RepairDeep(this SContainer container, TypeDefinition type = null, int limit = 100)
        => SValueExternal._external.RepairDeep(container, type, limit);

    /// <summary>
    /// Repairs an SObject to match its DCompond type definition.
    /// </summary>
    /// <param name="objType">The DCompond object type.</param>
    /// <param name="obj">The SObject to repair.</param>
    public static void RepairObject(this DCompond objType, SObject obj)
        => SValueExternal._external.RepairObject(objType, obj);

    /// <summary>
    /// Removes read-only flags from all items in a container recursively.
    /// </summary>
    /// <param name="container">The SContainer.</param>
    public static void RemoveReadonlyDeep(this SContainer container)
        => SValueExternal._external.RemoveReadonlyDeep(container);

    /// <summary>
    /// Unsets the read-only flag from an item and all its children recursively.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public static void UnsetReadonlyDeep(this SItem item)
        => SValueExternal._external.UnsetReadonlyDeep(item);

    /// <summary>
    /// Gets the edited type for a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    public static Type GetEditedType(this TypeDefinition typeInfo)
        => SValueExternal._external.GetEditedType(typeInfo);

    /// <summary>
    /// Determines if a type definition supports a specific value.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="nullable">Whether the type is nullable.</param>
    public static bool SupportValue(this TypeDefinition typeInfo, object value, bool nullable)
        => SValueExternal._external.SupportValue(typeInfo, value, nullable);

    /// <summary>
    /// Converts a value to match the type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullable">Whether the type is nullable.</param>
    public static object ConvertValue(this TypeDefinition typeInfo, object value, bool nullable)
        => SValueExternal._external.ConvertValue(typeInfo, value, nullable);

    /// <summary>
    /// Tries to convert a value to match the type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullable">Whether the type is nullable.</param>
    /// <param name="result">The converted result.</param>
    public static bool TryConvertValue(this TypeDefinition typeInfo, object value, bool nullable, out object result)
        => SValueExternal._external.TryConvertValue(typeInfo, value, nullable, out result);

    /// <summary>
    /// Creates a default value for a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="filter">Optional asset filter.</param>
    public static object CreateDefaultValue(this TypeDefinition typeInfo, IAssetFilter filter = null)
        => SValueExternal._external.CreateDefaultValue(typeInfo, filter);

    // FixConvertibleValue
    /// <summary>
    /// Creates or repairs a value to match the type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="value">The value to create or repair.</param>
    /// <param name="nullable">Whether the type is nullable.</param>
    public static object CreateOrRepairValue(this TypeDefinition typeInfo, object value, bool nullable)
        => SValueExternal._external.CreateOrRepairValue(typeInfo, value, nullable);

    /// <summary>
    /// Creates or repairs a value to match the struct field.
    /// </summary>
    /// <param name="field">The struct field.</param>
    /// <param name="value">The value to create or repair.</param>
    public static object CreateOrRepairValue(this DStructField field, object value)
        => SValueExternal._external.CreateOrRepairValue(field, value);

    /// <summary>
    /// Creates a default value for a struct field.
    /// </summary>
    /// <param name="field">The struct field.</param>
    /// <param name="filter">Optional asset filter.</param>
    public static object CreateDefaultValue(this DStructField field, IAssetFilter filter = null)
        => SValueExternal._external.CreateDefaultValue(field, filter);

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing SObject or creates a new one based on the type definition.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="obj">The existing SObject, or null to create new.</param>
    /// <param name="filter">The asset filter.</param>
    public static SObject UpdateOrCreateSObject(this TypeDefinition type, SObject obj, IAssetFilter filter)
        => SValueExternal._external.UpdateOrCreateSObject(type, obj, filter);

    #endregion

    #region Preivew & Icon


    /// <summary>
    /// Converts an SObject to a brief string representation.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    public static string ToBriefString(this SObject obj)
    {
        if (obj is null)
        {
            return string.Empty;
        }

        if (obj.Controller != null)
        {
            if (obj.Controller is IPreviewDisplay previewDisplay)
            {
                return previewDisplay.PreviewText;
            }
            else
            {
                return obj.Controller.ToString();
            }
        }
        else
        {
            DCompond s = obj.GetStruct(AssetFilters.All);
            if (s != null)
            {
                string brief = s.GetBrief(obj);
                if (!string.IsNullOrEmpty(brief))
                {
                    return brief;
                }
                else
                {
                    return $"({obj.ObjectType.ToDisplayString()})";
                }
            }
            else
            {
                return obj.ObjectType.TypeCode;
            }
        }
    }

    /// <summary>
    /// Converts an SObject to a display string.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    public static string ToDisplayString(this SObject obj)
    {
        if (obj is null)
        {
            return string.Empty;
        }

        if (obj.Controller != null)
        {
            if (obj.Controller is IPreviewDisplay previewDisplay)
            {
                return previewDisplay.PreviewText;
            }
            else
            {
                return obj.Controller.ToString();
            }
        }
        else
        {
            return obj.ObjectType.ToDisplayString();
        }
    }

    //public static string GetBrief(this SObject obj, int depth = 10)
    //{
    //    if (obj is null)
    //    {
    //        return string.Empty;
    //    }

    //    DBaseStruct type = obj.ObjectType.Target as DBaseStruct;
    //    if (type is null)
    //    {
    //        return string.Empty;
    //    }

    //    return GetBrief(type, obj, depth);
    //}

    /// <summary>
    /// Gets a brief string representation of a DCompond object.
    /// </summary>
    /// <param name="type">The DCompond type.</param>
    /// <param name="obj">The SObject.</param>
    /// <param name="depth">The depth for traversal.</param>
    public static string GetBrief(this DCompond type, SObject obj, int depth = 10)
    {
        return SValueExternal._external.GetBrief(type, obj, depth);
    }

    /// <summary>
    /// Gets the icon for an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public static ImageDef GetIcon(this SItem item)
    {
        if (item is SObject sobj)
        {
            return GetIcon(sobj);
        }
        
        if (item is SArray sary)
        {
            return GetIcon(sary);
        }

        if (item.InputType?.OriginType.GetTarget(AssetFilters.All) is DType type)
        {
            return type.ToIconSmall();
        }

        return null;
    }

    /// <summary>
    /// Gets the icon for an SObject.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    public static ImageDef GetIcon(this SObject obj)
    {
        if (obj.Controller is { } controller && controller.ToDisplayIcon() is { } icon)
        {
            return icon;
        }

        DCompond s = obj.GetStruct(AssetFilters.All);
        if (s != null)
        {
            return s.ToIconSmall();
        }

        return null;
    }

    /// <summary>
    /// Gets the icon for an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    public static ImageDef GetIcon(this SArray ary)
    {
        if (ary.InputType.OriginType.GetTarget(AssetFilters.All) is DCompond s)
        {
            return s.ToIconSmall();
        }

        return CoreIconCache.Array;
    }

    #endregion

    #region Selection

    /// <summary>
    /// Gets the selection list for an SObject.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    public static ISelectionList GetSelectionList(this SObject obj)
    {
        IAssetFilter filter = obj.GetAssetFilter();
        return obj?.InputType?.GetImplementationList(filter);
    }

    /// <summary>
    /// Gets the selection list for an SObject with a specific filter.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    /// <param name="filter">The asset filter.</param>
    public static ISelectionList GetSelectionList(this SObject obj, IAssetFilter filter)
    {
        return obj?.InputType?.GetImplementationList(filter);
    }

    /// <summary>
    /// Gets the selection list of implementations for a type definition.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="filter">The asset filter.</param>
    public static ISelectionList GetSelectionList(this TypeDefinition type, IAssetFilter filter)
    {
        return type?.GetImplementationList(filter);
    }

    #endregion

    #region CreateObject GUI

    /// <summary>
    /// Opens a GUI dialog to create an SObject with a parent object context.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="parent">The parent SObject.</param>
    /// <param name="title">The dialog title.</param>
    public static Task<SObject> GuiCreateObject(this TypeDefinition type, SObject parent, string title)
        => SValueExternal._external.GuiCreateObject(type, parent, title);

    /// <summary>
    /// Opens a GUI dialog to create an SObject with a parent array context.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="parent">The parent SArray.</param>
    /// <param name="title">The dialog title.</param>
    public static Task<SObject> GuiCreateObject(this TypeDefinition type, SArray parent, string title)
        => SValueExternal._external.GuiCreateObject(type, parent, title);

    /// <summary>
    /// Opens a GUI dialog to create an SObject with a filter.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="filter">The asset filter.</param>
    /// <param name="title">The dialog title.</param>
    public static Task<SObject> GuiCreateObject(this TypeDefinition type, IAssetFilter filter, string title)
        => SValueExternal._external.GuiCreateObject(type, filter, title);

    /// <summary>
    /// Opens a GUI dialog to configure an SObject.
    /// </summary>
    /// <param name="obj">The SObject to configure.</param>
    /// <param name="title">The dialog title.</param>
    public static Task GuiConfigObject(this SObject obj, string title)
        => SValueExternal._external.GuiConfigObject(obj, title);

    #endregion

    #region DataId

    /// <summary>
    /// Converts an SKey to a data ID string.
    /// </summary>
    /// <param name="key">The SKey.</param>
    public static string ToDataId(this SKey key)
    {
        return key.TargetAsset?.ToDataId();
    }

    /// <summary>
    /// Converts an SAssetKey to a data ID string.
    /// </summary>
    /// <param name="key">The SAssetKey.</param>
    public static string ToDataId(this SAssetKey key)
    {
        return key.TargetAsset?.ToDataId();
    }

    #endregion

    #region Find object in array

    /// <summary>
    /// Finds an object controller by name in an SArray.
    /// </summary>
    /// <typeparam name="T">The type of SObjectController.</typeparam>
    /// <param name="ary">The SArray.</param>
    /// <param name="name">The name to search for.</param>
    public static T FindObjectController<T>(this SArray ary, string name) where T : SObjectController
    {
        var obj = FindObject(ary, name);
        return obj != null ? obj.Controller as T : null;
    }

    /// <summary>
    /// Finds an SObject by name in an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="name">The name to search for.</param>
    public static SObject FindObject(this SArray ary, string name)
    {
        return FindObjects(ary, name).FirstOrDefault();
    }

    /// <summary>
    /// Checks if an SArray contains an object with the specified name.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="name">The name to search for.</param>
    public static bool HasObject(this SArray ary, string name)
    {
        return FindObjects(ary, name).Any();
    }

    /// <summary>
    /// Finds all SObjects matching the specified name in an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="name">The name to search for.</param>
    public static IEnumerable<SObject> FindObjects(this SArray ary, string name)
    {
        foreach (SObject obj in ary.Items.OfType<SObject>())
        {
            DType type = obj.ObjectType.GetTarget(AssetFilters.All);
            if (type != null)
            {
                if (string.IsNullOrEmpty(name) || type.FullTypeName == name)
                {
                    yield return obj;
                }
            }
        }
    }

    /// <summary>
    /// Finds the first SObject of the specified DCompond type in an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="type">The DCompond type.</param>
    public static SObject FindObjectOfType(this SArray ary, DCompond type)
    {
        return FindObjectsOfType(ary, type).FirstOrDefault();
    }

    /// <summary>
    /// Checks if an SArray contains an object of the specified DCompond type.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="type">The DCompond type.</param>
    public static bool ContainsObjectOfType(this SArray ary, DCompond type)
    {
        return FindObjectsOfType(ary, type).Any();
    }

    /// <summary>
    /// Finds all SObjects of the specified DCompond type in an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="type">The DCompond type.</param>
    public static IEnumerable<SObject> FindObjectsOfType(this SArray ary, DCompond type)
    {
        foreach (SObject obj in ary.Items.OfType<SObject>())
        {
            if (obj.GetStruct() == type)
            {
                yield return obj;
            }
        }
    }

    /// <summary>
    /// Gets the index of the first object of the specified DCompond type in an SArray.
    /// </summary>
    /// <param name="ary">The SArray.</param>
    /// <param name="type">The DCompond type.</param>
    public static int IndexOfType(this SArray ary, DCompond type)
    {
        for (int i = 0; i < ary.Count; i++)
        {
            if (ary[i] is SObject obj && obj.GetStruct() == type)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion

    #region AutoField

    /// <summary>
    /// Ensures an auto-field property is set on an SObject based on the data row.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    /// <param name="field">The struct field.</param>
    /// <param name="row">The data item.</param>
    /// <param name="condition">Optional condition context.</param>
    public static object EnsureAutoFieldProperty(this SObject obj, DStructField field, IDataItem row, ICondition condition = null)
    {
        var owner = row?.DataContainer;
        if (owner is null)
        {
            //return null;
            return obj.GetPropertyFormatted(field.Name, condition);
        }

        return obj.EnsureAutoFieldProperty(field, autoField =>
        {
            switch (autoField)
            {
                case AutoFieldType.Guid:
                    return row.DataGuid;

                case AutoFieldType.DataId:
                    return $"{owner.TableId}.{row.DataLocalId}";

                case AutoFieldType.TableId:
                    return owner.TableId;

                case AutoFieldType.LocalId:
                    return row.DataLocalId;

                case AutoFieldType.Name:
                    return row.DataLocalId;

                case AutoFieldType.Description:
                    return row.Description;

                case AutoFieldType.Index:
                    return row.Index;

                default:
                    return obj.GetPropertyFormatted(field.Name, condition);
            }
        }, condition);
    }

    /// <summary>
    /// Ensures an auto-field property is set on an SObject using a custom getter function.
    /// </summary>
    /// <param name="obj">The SObject.</param>
    /// <param name="field">The struct field.</param>
    /// <param name="autoFieldGetter">The function to get auto field values.</param>
    /// <param name="condition">Optional condition context.</param>
    public static object EnsureAutoFieldProperty(this SObject obj, DStructField field, Func<AutoFieldType, object> autoFieldGetter, ICondition condition = null)
    {
        var autoField = field.AutoFieldType;
        if (autoField.HasValue)
        {
            object autoValue = autoFieldGetter(autoField.Value);
            if (autoValue != null)
            {
                obj.SetProperty(field.Name, autoValue);
            }
        }

        return obj.GetPropertyFormatted(field.Name, condition);
    }

    #endregion

    #region Setup

    /// <summary>
    /// Sets up view objects for an array of SObjects.
    /// </summary>
    /// <param name="objs">The array of SObjects.</param>
    /// <param name="setup">The view object setup.</param>
    /// <param name="preview">Whether to show preview.</param>
    /// <param name="sharedType">The shared DCompond type.</param>
    public static void SetupObjects(this SObject[] objs, IViewObjectSetup setup, bool preview, out DCompond sharedType)
       => SValueExternal._external.SetupObjects(objs, setup, preview, out sharedType);

    #endregion

    /// <summary>
    /// Adds an inspector field for a type definition to the view setup.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="property">The view property.</param>
    public static void InspectorFieldOfType(this IViewObjectSetup setup, TypeDefinition typeInfo, ViewProperty property)
    {
        property.ViewId = ViewIds.Inspector;
        setup.AddField(typeInfo.GetNativeType(), property);
    }
}