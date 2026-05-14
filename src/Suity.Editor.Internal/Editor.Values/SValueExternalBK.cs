using Suity;
using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Reflecting;
using Suity.Selecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Back-end implementation of <see cref="SValueExternal"/> that provides value creation,
/// type resolution, repair, conversion, preview, and GUI object creation functionality.
/// </summary>
internal sealed class SValueExternalBK : SValueExternal
{
    private static readonly Dictionary<Type, Func<TypeDefinition, SItem>> _sItemActivators = [];

    /// <summary>
    /// Gets the singleton instance of <see cref="SValueExternalBK"/>.
    /// </summary>
    public static readonly SValueExternalBK Instance = new();

    private SValueExternalBK()
    { }

    /// <summary>
    /// Initializes the value external system by registering SItem activators for all supported types.
    /// </summary>
    public void Initialize()
    {
        _sItemActivators.Add(typeof(DPrimative), def =>
        {
            TypeCode typeCode = (def.Target as DPrimative)?.TypeCode ?? TypeCode.Empty;

            return typeCode switch
            {
                TypeCode.Boolean => new SBoolean(false),
                TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => new SNumeric(def),
                TypeCode.DateTime => new SDateTime(DateTime.MinValue),
                TypeCode.String => new SString(),
                TypeCode.Char => new SString(),
                TypeCode.Object => new SUnknownValue(),
                _ => new SNull(),
            };
        });

        _sItemActivators.Add(typeof(DNativeStruct), def =>
        {
            if (typeof(SObjectController).IsAssignableFrom(def.NativeType))
            {
                return new SObject(def);
            }

            if (def.NativeType == typeof(TextBlock))
            {
                return new STextBlock(def);
            }

            return new SUnknownValue(def);
        });

        _sItemActivators.Add(typeof(DStruct), def =>
        {
            if (def.Target is DStruct s)
            {
                return CreateObject(s);
            }
            else
            {
                return new SObject(def);
            }
        });

        _sItemActivators.Add(typeof(DFunction), def =>
        {
            if (def.Target is DFunction s)
            {
                return CreateObject(s);
            }
            else
            {
                return new SObject(def);
            }
        });

        _sItemActivators.Add(typeof(DAbstract), def =>
        {
            if (def.PrimaryType?.Target is DStruct s)
            {
                return CreateObject(s, def);
            }
            else
            {
                return new SObject(def, def.PrimaryType);
            }
        });

        _sItemActivators.Add(typeof(DEnum), def => new SEnum(def));
        _sItemActivators.Add(typeof(DDelegate), def => new SDelegate(def));
        _sItemActivators.Add(typeof(DataLinkTypeDefinition), def => new SKey(def));
        _sItemActivators.Add(typeof(AssetLinkTypeDefinition), def => new SAssetKey(def));
        _sItemActivators.Add(typeof(AbstractFunctionTypeDefinition), def => new SObject(def, def.PrimaryType));
        _sItemActivators.Add(typeof(ArrayTypeDefinition), def => new SArray(def));

        SValueExternal._external = this;
    }

    #region Resolve

    /// <inheritdoc/>
    public override Type GetNativeType(TypeDefinition typeInfo)
    {
        if (typeInfo is null)
        {
            return null;
        }

        switch (typeInfo.Relationship)
        {
            case TypeRelationships.Value:
                return typeInfo.Target?.NativeType;

            case TypeRelationships.Array:
                return typeof(SArray);

            case TypeRelationships.DataLink:
                return typeof(SKey);

            case TypeRelationships.Enum:
                return typeof(SEnum);

            case TypeRelationships.Delegate:
                return typeof(SDelegate);

            case TypeRelationships.AbstractStruct:
            case TypeRelationships.AbstractFunction:
                return typeof(SObject);

            case TypeRelationships.AbstractNumeric:
                return typeof(Decimal);

            case TypeRelationships.AssetLink:
                return typeof(SAssetKey);

            case TypeRelationships.Struct:
                // Native modifier
                Type editedType = NativeTypes.GetNativeTypeByLocalName(typeInfo);
                return editedType ?? typeof(SObject);

            case TypeRelationships.None:
                return typeof(void);

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override Type GetFieldEditedType(IEnumerable<SObject> objs, DStructField field)
    {
        var firstValue = objs.FirstOrDefault()?.GetItemFormatted(field.Id);
        if (firstValue is null)
        {
            return SItem.ResolveSType(GetEditedType(field.FieldType));
        }

        Type editedType = SItem.ResolveSType(firstValue.GetType());
        if (editedType is null)
        {
            return SItem.ResolveSType(GetEditedType(field.FieldType));
        }

        foreach (var value in objs.Skip(1).Select(o => o.GetItemFormatted(field.Id)))
        {
            Type valueType = SItem.ResolveSType(value.GetType());
            if (valueType != editedType)
            {
                editedType = null;
                break;
            }
        }

        if (editedType != null)
        {
            return editedType;
        }
        else
        {
            return SItem.ResolveSType(GetEditedType(field.FieldType));
        }
    }

    /// <inheritdoc/>
    public override Type GetArrayEditedType(IEnumerable<SArray> arys, int elementIndex)
    {
        var firstAry = arys.FirstOrDefault();
        if (firstAry is null)
        {
            return null;
        }

        var firstObj = firstAry.GetItemFormatted(elementIndex);
        if (firstObj is null)
        {
            return GetNativeType(firstAry.InputType.ElementType);
        }

        Type editedType = SItem.ResolveSType(firstObj.GetType());
        if (editedType is null)
        {
            return GetNativeType(firstAry.InputType.ElementType);
        }

        foreach (var value in arys.Skip(1).Select(o => o.GetItemFormatted(elementIndex)))
        {
            Type valueType = SItem.ResolveSType(value.GetType());
            if (valueType != editedType)
            {
                editedType = null;
                break;
            }
        }

        if (editedType != null)
        {
            return editedType;
        }
        else
        {
            return GetNativeType(firstAry.InputType.ElementType);
        }
    }

    /// <inheritdoc/>
    public override DStructField GetParentField(SItem item, IAssetFilter filter)
    {
        SContainer parent = item?.Parent;
        if (parent is null)
        {
            return null;
        }

        TypeDefinition parentType;

        if (parent is SArray sArray)
        {
            // If it's an array, jump one level up
            parentType = (sArray.Parent as SObject)?.ObjectType;
            if (parentType != null)
            {
                DCompond s = parentType.GetStruct(filter);
                return s?.GetPublicStructFieldFromBase(sArray.FieldId);
            }
            else
            {
                return null;
            }
        }
        else if (parent is SObject sObject)
        {
            parentType = sObject.ObjectType;
            if (parentType != null)
            {
                DCompond s = parentType.GetStruct(filter);
                return s?.GetPublicStructFieldFromBase(item.FieldId);
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override TypeDefinition GetTypeFromParent(SItem item, IAssetFilter filter)
    {
        SContainer parent = item?.Parent;
        if (parent is null)
        {
            return null;
        }

        TypeDefinition inputType = null;

        if (parent is SArray sArray)
        {
            inputType = sArray.InputType.ElementType;
        }
        else if (parent is SObject sObject)
        {
            var parentType = sObject.ObjectType;
            DCompond s = parentType.GetStruct(filter);
            var field = s?.GetPublicStructFieldFromBase(item.FieldId);
            if (field != null)
            {
                inputType = field.FieldType;
            }
        }

        return inputType;
    }

    #endregion

    #region Create

    /// <inheritdoc/>
    public override SItem CreateValue(TypeDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (definition.IsOrigin)
        {
            return _sItemActivators.GetValueSafe(definition.Target?.GetType())?.Invoke(definition);
        }
        else
        {
            return _sItemActivators.GetValueSafe(definition.GetType())?.Invoke(definition);
        }
    }

    /// <inheritdoc/>
    public override SItem CreateValue(DType type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return _sItemActivators.GetValueSafe(type.GetType())?.Invoke(type.Definition);
    }

    /// <inheritdoc/>
    public override SObject CreateObject(DCompond objectType)
    {
        if (objectType is null)
        {
            throw new ArgumentNullException(nameof(objectType));
        }

        return CreateObject(objectType, objectType.Definition);
    }

    /// <inheritdoc/>
    public override SObject CreateObject(DCompond objectType, TypeDefinition inputType, string inputText = null)
    {
        if (objectType is null)
        {
            throw new ArgumentNullException(nameof(objectType));
        }

        SObject obj;

        if (objectType is INativeType nType)
        {
            Type nativeType = nType.NativeType;
            //if (nativeType == typeof(TextBlock))
            //{
            //    throw new InvalidOperationException("Can not create TextBlock in SObject creation pipeline.");
            //}

            if (typeof(SObjectController).IsAssignableFrom(nativeType))
            {
                obj = new SObject(inputType, objectType.Definition, (SObjectController)nativeType.CreateInstanceOf());
            }
            else
            {
                Logs.LogError(L($"Cannot create SObject of type {nativeType.FullName}"));
                obj = null;
                //throw new InvalidOperationException("Can not create SObject with type : " + nativeType.FullName);
            }
        }
        else
        {
            obj = new SObject(inputType, objectType.Definition);
            foreach (DStructField field in objectType.PublicFields.OfType<DStructField>())
            {
                obj.SetProperty(field.Name, CreateDefaultValue(field));
            }
        }

        if (inputText != null && obj?.Controller is SObjectController ctrl)
        {
            try
            {
                ctrl.InputText(inputText);
                ctrl.Commit();
            }
            catch (Exception)
            {
            }
        }

        return obj;
    }

    /// <inheritdoc/>
    public override SObject CreateObject(TypeDefinition objectType, TypeDefinition inputType, IAssetFilter filter, string inputText = null)
    {
        if (objectType is null)
        {
            throw new ArgumentNullException(nameof(objectType));
        }

        if (objectType.GetTarget(filter) is DCompond dObjType)
        {
            return CreateObject(dObjType, inputType, inputText);
        }

        var obj = new SObject(inputType, objectType);
        if (inputText != null && obj?.Controller is SObjectController ctrl)
        {
            try
            {
                ctrl.InputText(inputText);
                ctrl.Commit();
            }
            catch (Exception)
            {
            }
        }

        return obj;
    }

    /// <inheritdoc/>
    public override SObject CreateDefaultObject(TypeDefinition inputType, IAssetFilter filter, string inputText = null)
    {
        if (inputType.IsAbstract)
        {
            if (inputType.PrimaryType?.GetTarget(filter) is DCompond primary)
            {
                return CreateObject(primary, inputType, inputText);
            }
            else
            {
                return new SObject(inputType, TypeDefinition.Empty);
            }
        }
        else if (inputType == NativeTypes.ObjectType)
        {
            return new SObject(inputType, TypeDefinition.Empty);
        }
        else if (inputType.IsStruct)
        {
            if (inputType.GetTarget(filter) is DCompond s)
            {
                return CreateObject(s, inputType, inputText);
            }
            else
            {
                return new SObject(inputType, inputType);
            }
        }
        else
        {
            return new SObject(TypeDefinition.Empty, TypeDefinition.Empty);
        }
    }

    /// <inheritdoc/>
    public override SArray CreateArray(TypeDefinition typeInfo)
    {
        if (!typeInfo.IsArray)
        {
            typeInfo = typeInfo.MakeArrayType();
        }

        return new SArray(typeInfo);
    }

    /// <inheritdoc/>
    public override SObject CreateEmptyObject(TypeDefinition inputType)
    {
        if (inputType.IsArray)
        {
            inputType = inputType.ElementType;
        }

        if (inputType.IsAbstract)
        {
            return new SObject(inputType, TypeDefinition.Empty);
        }
        else
        {
            return new SObject(inputType, inputType);
        }
    }

    #endregion

    #region Repair

    /// <inheritdoc/>
    public override void Repair(SObject obj, TypeDefinition type = null)
    {
        var dObjType = obj.GetStruct(AssetFilters.All);

        // If no type, get from parent
        type ??= GetTypeFromParent(obj, AssetFilters.All);

        if (type != null)
        {
            if (type.IsAbstract)
            {
                // When type is abstract, repair input type
                if (obj.InputType != type)
                {
                    obj.RepairInputTypeForce(type);
                }
            }
            else
            {
                // When type is implementation type, get struct
                if (type != dObjType?.Definition)
                {
                    dObjType = type.GetStruct(AssetFilters.All);
                }
            }
        }

        obj.RepairIds();

        if (dObjType != null)
        {
            // Repair through struct
            RepairObject(dObjType, obj);
        }
    }

    /// <inheritdoc/>
    public override void RepairDeep(SContainer container, TypeDefinition type = null, int limit = 100)
    {
        if (limit == 0)
        {
            Logs.LogWarning($"Cyclic structure detected : {container?.InputType?.GetFullTypeNameText()}");
            return;
        }
        else if (limit > 0)
        {
            limit--;
        }

        if (container is SObject sObj)
        {
            Repair(sObj, type);
        }

        if (container != null)
        {
            var items = container.Items.OfType<SContainer>().ToArray();

            // Infinite loop overflow occurs when child struct is same as struct type
            foreach (SContainer item in items)
            {
                TypeDefinition innerFixType = GetTypeFromParent(item, AssetFilters.All);
                RepairDeep(item, innerFixType, limit);
            }
        }
    }

    /// <inheritdoc/>
    public override void RepairObject(DCompond objType, SObject obj)
    {
        if (objType is null)
        {
            throw new ArgumentNullException(nameof(objType));
        }

        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        bool nullable = obj.GetParentField()?.Optional == true;
        // Nullable case
        if (nullable && TypeDefinition.IsNullOrEmpty(obj.ObjectType))
        {
            obj.Clear();
            return;
        }

        obj.ObjectType = objType.Definition;

        foreach (var field in objType.PublicStructFields)
        {
            object value = obj.GetItem(field.Name);
            object fixedValue = CreateOrRepairValue(field, value);
            obj.SetProperty(field, fixedValue);
        }

        if (!objType.UseNativeFields)
        {
            List<Guid> lagacy = null;
            foreach (Guid id in obj.GetStoredPropertyIds())
            {
                if (objType.GetPublicField(id) is null)
                {
                    (lagacy ??= []).Add(id);
                }
            }

            if (lagacy != null)
            {
                foreach (var id in lagacy)
                {
                    obj.RemoveProperty(id);
                }
            }
        }
    }

    [ThreadStatic]
    private static List<SItem> _tempSItems;

    /// <inheritdoc/>
    public override void RemoveReadonlyDeep(SContainer container)
    {
        _tempSItems ??= [];

        if (container is SObject obj)
        {
            try
            {
                _tempSItems.Clear();
                _tempSItems.AddRange(obj.Items.Where(o => o.ReadOnly));

                foreach (var item in _tempSItems)
                {
                    obj.RemoveItem(item);
                }
            }
            finally
            {
                _tempSItems.Clear();
            }
        }

        foreach (var childContainer in container.Items.OfType<SContainer>())
        {
            RemoveReadonlyDeep(childContainer);
        }
    }

    /// <inheritdoc/>
    public override void UnsetReadonlyDeep(SItem item)
    {
        item.ReadOnly = false;

        if (item is SContainer container)
        {
            foreach (var childItem in container.Items)
            {
                UnsetReadonlyDeep(childItem);
            }
        }
    }

    /// <inheritdoc/>
    public override Type GetEditedType(TypeDefinition typeInfo)
    {
        if (typeInfo is null)
        {
            return null;
        }

        Type editedType;

        switch (typeInfo.Relationship)
        {
            case TypeRelationships.Value:
                // Native modifier
                editedType = typeInfo.NativeType;
                return SItem.ResolveSType(editedType); // editedType ?? typeof(SValue);
            case TypeRelationships.Struct:
                // Native modifier
                editedType = typeInfo.NativeType;
                return editedType ?? typeof(SObject);

            case TypeRelationships.Array:
                return typeof(SArray);

            case TypeRelationships.Enum:
                return typeof(SEnum);

            case TypeRelationships.DataLink:
                return typeof(SKey);

            case TypeRelationships.AssetLink:
                return typeof(SAssetKey);

            case TypeRelationships.Delegate:
                return typeof(SDelegate);

            case TypeRelationships.AbstractFunction:
                return typeof(SObject);

            case TypeRelationships.AbstractStruct:
                return typeof(SObject);

            case TypeRelationships.None:
            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override bool SupportValue(TypeDefinition typeInfo, object value, bool nullable)
    {
        if (typeInfo is null || value is null)
        {
            return false;
        }

        switch (typeInfo.Relationship)
        {
            case TypeRelationships.Value:
                if (value is SDynamic)
                {
                    return true;
                }
                if (value is SValue svalue)
                {
                    Type editedType = GetEditedType(typeInfo);
                    if (value.GetType() != editedType)
                    {
                        return false;
                    }
                    if (svalue.Value is null)
                    {
                        return false;
                    }

                    return svalue.Value?.GetType() == typeInfo.NativeType;
                }
                else
                {
                    return value.GetType() == typeInfo.NativeType;
                }
            case TypeRelationships.Struct:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    SObject obj = value as SObject;

                    if (typeInfo == NativeTypes.ObjectType)
                    {
                        return obj != null;
                    }
                    else
                    {
                        // Regular struct requires exact type match
                        if (nullable)
                        {
                            return obj != null && obj.InputType == typeInfo && (TypeDefinition.IsNullOrEmpty(obj.ObjectType) || obj.ObjectType == typeInfo);
                        }
                        else
                        {
                            return obj != null && obj.InputType == typeInfo && obj.ObjectType == typeInfo;
                        }
                    }
                }
            case TypeRelationships.Array:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    return value is SArray ary && ary.InputType == typeInfo;
                }
            case TypeRelationships.Enum:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    if (value is not SEnum en || en.InputType != typeInfo)
                    {
                        return false;
                    }

                    if (typeInfo.Target is DEnum enumInfo)
                    {
                        return enumInfo.GetPublicField(en.Value) != null;
                    }
                    else
                    {
                        //TODO: When cannot get DEnum, validation SEnum returns true
                        return true;
                    }
                }
            case TypeRelationships.DataLink:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    return value is SKey key && key.InputType == typeInfo;
                }
            case TypeRelationships.AssetLink:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    return value is SAssetKey assetKey && assetKey.InputType == typeInfo;
                }
            case TypeRelationships.Delegate:
                {
                    return value is SDelegate d && d.InputType == typeInfo;
                }
            case TypeRelationships.AbstractFunction:
                {
                    string afuncKey = (value as SObject)?.ObjectType.Target?.AssetKey;
                    if (string.IsNullOrEmpty(afuncKey))
                    {
                        return false;
                    }

                    return DTypeManager.Instance.GetFunctionsByReturnType(typeInfo).GetAsset(afuncKey) != null;
                }
            case TypeRelationships.AbstractStruct:
                {
                    if (value is SDynamic)
                    {
                        return true;
                    }

                    if (value is not SObject obj)
                    {
                        return false;
                    }

                    // Abstract struct requires input type match, and nullable||base type match
                    if (obj.InputType != typeInfo)
                    {
                        return false;
                    }

                    return TypeDefinition.IsNullOrEmpty(obj.ObjectType)
                        || typeInfo.Target == (obj.ObjectType.Target as DStruct)?.BaseType;
                }
            case TypeRelationships.None:
            default:
                return false;
        }
    }

    /// <inheritdoc/>
    public override object ConvertValue(TypeDefinition typeInfo, object value, bool nullable)
    {
        if (TryConvertValue(typeInfo, value, nullable, out object result))
        {
            return result;
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc/>
    public override bool TryConvertValue(TypeDefinition typeInfo, object value, bool nullable, out object result)
    {
        if (typeInfo is null)
        {
            result = null;
            return false;
        }

        // Fix InputType
        if (value is SItem item && item.InputType != typeInfo)
        {
            item.RepairInputTypeForce(typeInfo);
        }

        if (SupportValue(typeInfo, value, nullable))
        {
            result = value;
            return true;
        }

        switch (typeInfo.Relationship)
        {
            case TypeRelationships.None:
                break;

            case TypeRelationships.Value:
                try
                {
                    // Convert TextBlock value
                    if (value is STextBlock textBlock)
                    {
                        value = textBlock.TextValue;
                    }

                    result = (typeInfo.Target as DPrimative)?.ConvertValue(SItem.ResolveValue(value));
                    return true;
                }
                catch (Exception)
                {
                    break;
                }
            case TypeRelationships.Struct:
                if (typeInfo.Target?.NativeType == typeof(TextBlock))
                {
                    if (value is STextBlock)
                    {
                        result = value;
                    }
                    else
                    {
                        result = new STextBlock(value?.ToString() ?? string.Empty);
                    }

                    return true;
                }
                else if (nullable && (value is null || value is SNull))
                {
                    // Handle null case
                    result = new SObject(typeInfo, TypeDefinition.Empty);
                    return true;
                }
                break;

            case TypeRelationships.Array:
                {
                    if (value is SArray)
                    {
                        break;
                    }
                    else if (value is not string && value is System.Collections.IEnumerable items)
                    {
                        // Try to build an array but failed to check elements.
                        var sary = new SArray(typeInfo);
                        foreach (var item2 in items)
                        {
                            sary.Add(item2);
                        }
                        result = sary;
                        return true;
                    }
                }
                break;

            case TypeRelationships.Enum:
                if (typeInfo.Target is DEnum enumInfo)
                {
                    DEnumField field = enumInfo.ResolveField(value, true) ?? enumInfo.FirstField as DEnumField;
                    result = new SEnum(typeInfo, field);
                    return true;
                }
                break;

            case TypeRelationships.DataLink:
                {
                    if (GlobalIdResolver.TryResolve(value, out Guid id))
                    {
                        result = new SKey(typeInfo, id);
                    }
                    else
                    {
                        result = new SKey(typeInfo, value?.ToString() ?? string.Empty);
                    }
                }
                return true;

            case TypeRelationships.AssetLink:
                {
                    if (GlobalIdResolver.TryResolve(value, out Guid id))
                    {
                        result = new SAssetKey(typeInfo, id);
                    }
                    else
                    {
                        result = new SAssetKey(typeInfo, value?.ToString() ?? string.Empty);
                    }
                }
                return true;

            case TypeRelationships.Delegate:
                break;

            case TypeRelationships.AbstractFunction:
                break;

            case TypeRelationships.AbstractStruct:
                break;

            default:
                break;
        }

        result = null;

        return false;
    }

    /// <inheritdoc/>
    public override object CreateDefaultValue(TypeDefinition typeDef, IAssetFilter filter = null)
    {
        if (typeDef is null)
        {
            return null;
        }

        switch (typeDef.Relationship)
        {
            case TypeRelationships.Value:
                return (typeDef.Target as DPrimative)?.GetDefautValue();

            case TypeRelationships.Struct:
                if (typeDef == NativeTypes.ObjectType)
                {
                    // Abstract Object
                    return new SObject(typeDef, TypeDefinition.Empty);
                }
                else if (typeDef.IsNative)
                {
                    if (typeof(SObjectController).IsAssignableFrom(typeDef.NativeType))
                    {
                        return new SObject(typeDef);
                    }
                    else if (typeDef.NativeType == typeof(TextBlock))
                    {
                        return new STextBlock();
                    }
                    else
                    {
                        return CreateValue(typeDef);
                    }
                }
                else
                {
                    return new SObject(typeDef);
                }
            case TypeRelationships.Array:
                return new SArray(typeDef);

            case TypeRelationships.Enum:
                return new SEnum(typeDef);

            case TypeRelationships.DataLink:
                return new SKey(typeDef);

            case TypeRelationships.AssetLink:
                return new SAssetKey(typeDef);

            case TypeRelationships.Delegate:
                return new SDelegate(typeDef);

            case TypeRelationships.AbstractFunction:
                return new SObject(typeDef, typeDef.PrimaryType);

            case TypeRelationships.AbstractStruct:
                return new SObject(typeDef, typeDef.PrimaryType);

            case TypeRelationships.None:
            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override object CreateOrRepairValue(TypeDefinition typeInfo, object value, bool nullable)
    {
        if (TryConvertValue(typeInfo, value, nullable, out object result))
        {
            return result;
        }

        return CreateDefaultValue(typeInfo);
    }

    /// <inheritdoc/>
    public override object CreateOrRepairValue(DStructField field, object value)
    {
        TypeDefinition fieldType = field?.FieldType;
        if (fieldType is null)
        {
            return null;
        }

        // Null value auto-converts to field default value
        if (value is null && !field.Optional)
        {
            value = field._defaultValue;
        }

        if (TryConvertValue(fieldType, value, field.Optional, out object result2))
        {
            return result2;
        }

        if (TryConvertValue(fieldType, field._defaultValue, field.Optional, out var result1))
        {
            return result1;
        }

        return CreateDefaultValue(fieldType);
    }

    /// <inheritdoc/>
    public override object CreateDefaultValue(DStructField field, IAssetFilter filter = null)
    {
        TypeDefinition fieldType = field?.FieldType;
        if (fieldType is null)
        {
            return null;
        }

        if (TryConvertValue(fieldType, field._defaultValue, field.Optional, out object result))
        {
            return result;
        }

        return CreateDefaultValue(fieldType);
    }

    #endregion

    #region Update

    /// <inheritdoc/>
    public override SObject UpdateOrCreateSObject(TypeDefinition type, SObject obj, IAssetFilter filter)
    {
        if (obj is null)
        {
            if (type.IsAbstract)
            {
                return CreateDefaultObject(type, filter);
            }
            else
            {
                return CreateObject(type, type, filter);
            }
        }
        else
        {
            if (type.IsAbstract)
            {
                obj.InputType = type;
            }
            else
            {
                obj.InputType = type;
                obj.ObjectType = type;
            }

            return obj;
        }
    }

    #endregion

    #region Preview & Icon

    /// <inheritdoc/>
    public override string GetBrief(DCompond type, SObject obj, int depth = 10)
    {
        if (type is null)
        {
            return string.Empty;
        }

        if (obj is null)
        {
            return type.ToString();
        }

        depth--;
        if (depth < 0)
        {
            return type.ToString();
        }

        //var customBrief = type.GetAttribute<CustomBriefAttribute>()?.CustomBrief
        //    ?? type.BaseType?.GetAttribute<CustomBriefAttribute>()?.CustomBrief;

        var customBriefs = (type.BaseType?.GetAttributes<IBrief>() ?? [])
            .Concat(type.GetAttributes<IBrief>());

        string origin = GetBriefOrigin(type, obj, depth);
        string result = origin;

        if (customBriefs.Any())
        {
            foreach (var b in customBriefs)
            {
                result = b.GetBrief(obj, depth, () => result ?? string.Empty, () => origin ?? string.Empty);
            }

            return result ?? string.Empty;
        }

        return result;

        //if (customBrief != null)
        //{
        //    try
        //    {
        //        return customBrief.GetBrief(obj, depth, () => GetBriefOrigin(type, obj, depth)) ?? string.Empty;
        //    }
        //    catch (Exception err)
        //    {
        //        err.LogError();
        //        return string.Empty;
        //    }
        //}

        //return GetBriefOrigin(type, obj, depth);
    }

    /// <summary>
    /// Gets the brief text for an object based on its type's brief template.
    /// </summary>
    /// <param name="type">The compound type.</param>
    /// <param name="obj">The object to get the brief for.</param>
    /// <param name="depth">The recursion depth limit.</param>
    /// <returns>The brief text representation.</returns>
    internal string GetBriefOrigin(DCompond type, SObject obj, int depth = 10)
    {
        var brief = type.Brief;
        if (string.IsNullOrWhiteSpace(brief))
        {
            brief = type.BaseType?.Brief;
        }

        if (!string.IsNullOrWhiteSpace(brief))
        {
            StringBuilder builder = CorePlugin.StringBuilderPool.Acquire();
            builder.Clear();
            builder.Append(brief);

            foreach (DField field in type.PublicFields)
            {
                object value = obj.GetPropertyFormatted(field.Name);

                string identifier = $"{{{field.Name}}}";
                string fieldBrief = GetBrief(value, depth, true);

                builder.Replace(identifier, fieldBrief);
            }

            string str = builder.ToString().Trim();
            builder.Clear();
            CorePlugin.StringBuilderPool.Release(builder);

            // Since Brief is defined, don't return anything else.

            return str;
        }

        if (type is DFunction)
        {
            List<string> list = [];
            foreach (var field in type.PublicFields)
            {
                object value = obj.GetPropertyFormatted(field.Name);
                list.Add(GetBrief(value, depth, true));
            }

            return $"{type.DisplayText}({string.Join(",", list)})";
        }
        else
        {
            //return type.ToString();
            return String.Empty;
        }
    }

    [ThreadStatic] private static StringBuilder _builder;

    /// <summary>
    /// Gets a brief text representation of an object with limited recursion depth.
    /// </summary>
    /// <param name="obj">The object to get the brief for.</param>
    /// <param name="depth">The remaining recursion depth.</param>
    /// <param name="returnTypeName">Whether to return the type name if the brief is empty.</param>
    /// <returns>The brief text representation.</returns>
    private static string GetBrief(object obj, int depth, bool returnTypeName = false)
    {
        if (depth <= 0)
        {
            return string.Empty;
        }

        if (obj is SObject sobj)
        {
            string s = sobj.GetBrief(depth);
            if (string.IsNullOrEmpty(s) && returnTypeName)
            {
                s = sobj.ObjectType?.ToDisplayString();
            }

            return s;
        }
        else if (obj is SArray ary)
        {
            int len = ary.Count;

            bool moreThen3 = false;
            if (ary.Count > 3)
            {
                len = 3;
                moreThen3 = true;
            }

            _builder ??= new();

            _builder.Length = 0;

            for (int i = 0; i < len; i++)
            {
                if (i > 0)
                {
                    _builder.Append(", ");
                }

                var childObj = ary[i];
                _builder.Append(GetBrief(childObj, depth - 1, returnTypeName) ?? string.Empty);
            }

            if (moreThen3)
            {
                _builder.Append(L($"...({len} items)"));
            }

            string str = _builder.ToString();
            _builder.Length = 0;

            return str;
        }
        else
        {
            return obj?.ToString() ?? string.Empty;
        }
    }

    #endregion

    #region CreateObject GUI

    /// <inheritdoc/>
    public override async Task<SObject> GuiCreateObject(TypeDefinition type, SObject parent, string title)
    {
        SObject obj = null;
        SelectionResult result;
        IAssetFilter filter = parent.GetAssetFilter();

        ISelectionList list = EditorServices.SObjectService?.GetShortcutSelectionList(type, parent, filter);
        if (list != null)
        {
            result = await list.ShowSelectionGUIAsync(title);
        }
        else
        {
            result = await type.GetImplementationList(filter).ShowSelectionGUIAsync(title);
        }

        if (!result.IsSuccess)
        {
            return null;
        }

        if (result.Item is ISObjectCreate sObjectCreate)
        {
            obj = sObjectCreate.CreateSObject();
        }
        else if (!string.IsNullOrEmpty(result.SelectedKey))
        {
            var objType = TypeDefinition.Resolve(result.SelectedKey);
            obj = CreateObject(objType, type, filter);
        }
        else
        {
            // Ensure creating empty object, creating default object may create priority object
            //obj = CreateObject(type, null, AssetFilters.Default, result.Text);
            obj = CreateDefaultObject(type, filter, result.Text);
        }

        if (obj != null)
        {
            return obj;
        }

        // Ensure to create an empty object, creating a default object may create a priority object
        //obj = CreateObject(type, null, AssetFilters.Default, result.Text);
        obj = CreateDefaultObject(type, filter, result.Text);

        return obj;
    }

    /// <inheritdoc/>
    public override Task<SObject> GuiCreateObject(TypeDefinition type, SArray parent, string title)
    {
        return GuiCreateObject(type, parent.GetAssetFilter(), title);
    }

    /// <inheritdoc/>
    public override async Task<SObject> GuiCreateObject(TypeDefinition type, IAssetFilter filter, string title)
    {
        SObject obj = null;

        ISelectionList selList = type.GetSelectionList(filter);
        var result = await selList.ShowSelectionGUIAsync(null);
        if (result.IsSuccess)
        {
            if (AssetManager.Instance.GetAsset(result.SelectedKey, filter) is DCompond s)
            {
                obj = CreateObject(s, type);
                return obj;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override async Task GuiConfigObject(SObject obj, string title)
    {
        if (obj != null)
        {
            if (obj.Controller != null)
            {
                await obj.Controller.GuiConfig(title);
            }
            else
            {
                if (obj.ObjectType.Target is DCompond s)
                {
                    foreach (var field in s.PublicFields)
                    {
                        var value = obj.GetPropertyFormatted(field.Name);

                        if (value is SObject childObj && childObj.InputType.IsAbstractFunction)
                        {
                            string childTitle = title + " > " + field.DisplayText;
                            var newChildObj = await GuiCreateObject(childObj.InputType, obj, childTitle);
                            if (newChildObj != null)
                            {
                                obj.SetProperty(field.Name, newChildObj);
                                await GuiConfigObject(newChildObj, childTitle);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var value in obj.Items.ToArray())
                    {
                        if (value is SObject childObj && childObj.InputType.IsAbstractFunction)
                        {
                            string childTitle = title + " > " + childObj.Name;
                            var newChildObj = await GuiCreateObject(childObj.InputType, obj, childTitle);
                            if (newChildObj != null)
                            {
                                obj.SetProperty(childObj.Name, newChildObj);
                                await GuiConfigObject(newChildObj, childTitle);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Setup

    /// <inheritdoc/>
    public override void SetupObjects(SObject[] objs, IViewObjectSetup setup, bool preview, out DCompond sharedType)
    {
        sharedType = null;

        if (objs.Length == 0)
        {
            return;
        }

        SObject firstObj = objs.FirstOrDefault(o => o != null);
        if (firstObj is null)
        {
            return;
        }

        do
        {
            var types = objs.Select(o => o?.ObjectType).ToArray();
            if (types.AllEqual())
            {
                sharedType = types.Select(o => o?.Target as DCompond).FirstOrDefault();
                break;
            }

            types = objs.Select(o => o?.ObjectType?.BaseAbstractType).ToArray();
            if (types.AllEqual())
            {
                sharedType = types.Select(o => o?.Target as DCompond).FirstOrDefault();
                break;
            }
        } while (false);

        if (sharedType is null)
        {
            return;
        }

        if (sharedType != null)
        {
            if (firstObj.Controller != null)
            {
                SetupFieldsByController(setup, objs, firstObj, sharedType, preview);
            }
            else
            {
                SetupFieldsByType(setup, objs, firstObj, sharedType, preview);
            }
        }
        else if (objs.Length == 1)
        {
            if (firstObj.Controller != null)
            {
                SetupFieldsByController(setup, objs, firstObj, sharedType, preview);
            }
            else
            {
                SetupFieldsByType(setup, objs, firstObj, firstObj.ObjectType?.Target as DCompond, preview);
            }
        }
    }

    /// <summary>
    /// Sets up fields based on the type definition.
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="objs">The objects being edited.</param>
    /// <param name="firstObj">The first non-null object.</param>
    /// <param name="sharedType">The shared type definition.</param>
    /// <param name="preview">Whether this is a preview setup.</param>
    private void SetupFieldsByType(IViewObjectSetup setup, SObject[] objs, SObject firstObj, DCompond sharedType, bool preview)
    {
        if (sharedType is DNativeStruct || sharedType is null)
        {
            SetupFieldsByObject(setup, firstObj);
        }
        else
        {
            if (preview)
            {
                SetupFieldsByStructPreview(setup, objs, sharedType);
            }
            else
            {
                SetupFieldsByStruct(setup, objs, sharedType);
            }
        }
    }

    /// <summary>
    /// Sets up fields based on the full struct definition.
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="objs">The objects being edited.</param>
    /// <param name="sharedType">The shared compound type.</param>
    private void SetupFieldsByStruct(IViewObjectSetup setup, SObject[] objs, DCompond sharedType)
    {
        string hiddenFieldTypeStr = setup.Styles?.GetAttribute(ViewProperty.HiddenFieldTypeAttribute);
        bool hideDataLink = setup.Styles?.GetAttribute(ViewProperty.HideDataLinkAttribute) != null;
        bool hideConnector = setup.Styles?.GetAttribute(ViewProperty.HideConnectorAttribute) != null;

        TypeDefinition hiddenFieldType = TypeDefinition.Resolve(hiddenFieldTypeStr);
        TypeDefinition hiddenFieldAryType = hiddenFieldType?.MakeArrayType();

        if (ViewPlugin.Instance.SeparateBaseFields)
        {
            var baseType = sharedType.BaseType;
            if (baseType?.VisibleStructFields.Any() == true)
            {
                setup.AddField(typeof(LabelValue),
                    new ViewProperty(baseType.FullTypeName, baseType.ToDisplayText())
                        .WithIcon(baseType.IconId)
                    );

                foreach (var field in baseType.VisibleStructFields)
                {
                    SetupField(setup, field, objs, hiddenFieldType, hiddenFieldAryType, hideDataLink, hideConnector);
                }

                setup.AddField(typeof(LabelValue),
                    new ViewProperty(sharedType.FullTypeName, sharedType.ToDisplayText())
                        .WithIcon(sharedType.IconId)
                    );
            }

            foreach (var field in sharedType.GetPublicStructFields(false))
            {
                SetupField(setup, field, objs, hiddenFieldType, hiddenFieldAryType, hideDataLink, hideConnector);
            }
        }
        else
        {
            foreach (var field in sharedType.GetPublicStructFields(true))
            {
                SetupField(setup, field, objs, hiddenFieldType, hiddenFieldAryType, hideDataLink, hideConnector);
            }
        }
    }

    /// <summary>
    /// Sets up fields based on the preview struct definition (limited fields for display).
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="objs">The objects being edited.</param>
    /// <param name="sharedType">The shared compound type.</param>
    private void SetupFieldsByStructPreview(IViewObjectSetup setup, SObject[] objs, DCompond sharedType)
    {
        string hiddenFieldTypeStr = setup.Styles?.GetAttribute(ViewProperty.HiddenFieldTypeAttribute);
        bool hideDataLink = setup.Styles?.GetAttribute(ViewProperty.HideDataLinkAttribute) != null;
        bool hideConnector = setup.Styles?.GetAttribute(ViewProperty.HideConnectorAttribute) != null;

        TypeDefinition hiddenFieldType = TypeDefinition.Resolve(hiddenFieldTypeStr);
        TypeDefinition hiddenFieldAryType = hiddenFieldType?.MakeArrayType();

        foreach (var field in sharedType.PreviewStructFields)
        {
            SetupField(setup, field, objs, hiddenFieldType, hiddenFieldAryType, hideDataLink, hideConnector);
        }
    }

    /// <summary>
    /// Sets up a single field in the view.
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="field">The struct field to set up.</param>
    /// <param name="objs">The objects being edited.</param>
    /// <param name="hiddenFieldType">The hidden field type to skip.</param>
    /// <param name="hiddenFieldAryType">The hidden array field type to skip.</param>
    /// <param name="hideDataLink">Whether to hide data link fields with connectors.</param>
    /// <param name="hideConnector">Whether to hide connector fields.</param>
    private void SetupField(IViewObjectSetup setup, DStructField field, SObject[] objs,
        TypeDefinition hiddenFieldType, TypeDefinition hiddenFieldAryType, bool hideDataLink, bool hideConnector)
    {
        if (!IsFieldVisible(field))
        {
            return;
        }

        // Extension to node view ignore
        if (field.ShowInDetail)
        {
            return;
        }

        var fieldType = field.FieldType;

        if (fieldType == NativeTypes.DelegateType)
        {
            return;
        }

        if (hiddenFieldType != null && fieldType == hiddenFieldType)
        {
            return;
        }

        if (hiddenFieldAryType != null && fieldType == hiddenFieldAryType)
        {
            return;
        }

        if (hideDataLink && fieldType.GetIsDataLink(true) && field.ContainsAttribute<ConnectorAttribute>())
        {
            return;
        }

        if (hideConnector && field.ContainsAttribute<ConnectorAttribute>())
        {
            return;
        }

        if (!string.IsNullOrEmpty(field.Label))
        {
            setup.AddField(typeof(LabelValue), new ViewProperty($"#Label-{field.Label}", field.Label));
        }

        var viewProperty = new ViewProperty(field.Name, field.Description, field.DisplayIcon);
        if (field.IsHiddenOrDisabled)
        {
            viewProperty.WithObsolete();
        }

        if (objs.Any(o => o.GetItem(field.Name)?.ReadOnly == true))
        {
            viewProperty.WithReadOnly();
        }

        if (field.Optional)
        {
            viewProperty.WithOptional();
        }

        string toolTips = field.GetAttribute<ToolTipsAttribute>()?.ToolTips ?? field.ToolTips;
        if (!string.IsNullOrWhiteSpace(toolTips))
        {
            viewProperty.WithToolTips(toolTips);
        }

        if (field.Unit is string unit && !string.IsNullOrWhiteSpace(unit))
        {
            viewProperty.WithUnit(unit);
        }

        viewProperty.Disabled = field.IsDisabled;

        viewProperty.Color = field.ViewColor;

        viewProperty.Attributes = field;

        viewProperty.IsAbstract = field.FieldType.IsAbstract || field.FieldType.IsAbstractArray;

        if (objs.Length > 0 && objs[0]?.GetItem(field) is SObject childObj && childObj.ObjectType?.Target?.ViewColor is Color objColor)
        {
            if (objs.Length == 1 || objs.Select(o => (o.GetItem(field) as SObject)?.ObjectType).AllEqual())
            {
                viewProperty.Color = objColor;
            }
        }

        // Try to use specific type, as type can be converted to other dynamic link types
        Type editedType = GetFieldEditedType(objs, field);

        setup.AddField(editedType, viewProperty);
    }

    /// <summary>
    /// Sets up fields directly from the object's stored properties (for native structs without type info).
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="firstObj">The first object to set up fields from.</param>
    private void SetupFieldsByObject(IViewObjectSetup setup, SObject firstObj)
    {
        foreach (string name in firstObj.GetPropertyNames())
        {
            SItem item = firstObj.GetItem(name);
            if (item != null)
            {
                if (item is SObject o && o.InputType == NativeTypes.DelegateType)
                {
                    continue;
                }

                Type editedType = SItem.ResolveSType(item.GetType());
                var viewProperty = new ViewProperty(name) { ReadOnly = item.ReadOnly };

                setup.AddField(editedType, viewProperty);
            }
        }
    }

    /// <summary>
    /// Sets up fields using the object's controller.
    /// </summary>
    /// <param name="setup">The view setup interface.</param>
    /// <param name="objs">The objects being edited.</param>
    /// <param name="firstObj">The first non-null object.</param>
    /// <param name="sharedType">The shared type definition.</param>
    /// <param name="preview">Whether this is a preview setup.</param>
    private void SetupFieldsByController(IViewObjectSetup setup, SObject[] objs, SObject firstObj, DCompond sharedType, bool preview)
    {
        var controller = firstObj.Controller;

        if (controller != null)
        {
            controller.SetupView(setup);
        }
        else
        {
            SetupFieldsByType(setup, objs, firstObj, sharedType, preview);
        }
    }

    /// <summary>
    /// Checks whether a field should be visible in the editor.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns>True if the field should be visible.</returns>
    private bool IsFieldVisible(DField field)
    {
        bool hidden = (field as DStructField)?.IsHidden ?? true;

        return !hidden;
    }

    #endregion
}
