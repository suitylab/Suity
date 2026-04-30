using ComputerBeacon.Json;
using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender.Json;

/// <summary>
/// Delegate used to retrieve the value of a property from an <see cref="SObject"/> for a given struct field during JSON serialization.
/// </summary>
/// <param name="obj">The source object to read the property from.</param>
/// <param name="field">The struct field describing the property to retrieve.</param>
/// <returns>The property value as an object, or null if not available.</returns>
public delegate object JsonPropertyGetter(SObject obj, DStructField field);

/// <summary>
/// Provides helper methods for converting <see cref="SObject"/> and <see cref="SArray"/> instances to and from JSON representations.
/// Supports recursive serialization of nested objects, arrays, keys, enums, and other custom value types.
/// </summary>
public static class SValueJsonHelper
{
    /// <summary>
    /// Converts an <see cref="SObject"/> to a <see cref="JsonObject"/> representation.
    /// Serializes all properties of the object, handling nested objects, arrays, keys, enums, and other value types recursively.
    /// </summary>
    /// <param name="obj">The source object to serialize.</param>
    /// <param name="userDataId">When true, serializes keys and asset keys as data IDs; otherwise uses target IDs.</param>
    /// <param name="writeTypeName">When true, includes the @type property in the JSON output.</param>
    /// <param name="condition">Optional condition used when evaluating property values.</param>
    /// <param name="propGetter">Optional custom property getter; if null, uses the object's default property retrieval.</param>
    /// <returns>A <see cref="JsonObject"/> representing the serialized object, or null if the input is null or empty.</returns>
    public static JsonObject ToJson(
        this SObject obj, bool userDataId, bool writeTypeName = true,
        ICondition condition = null, JsonPropertyGetter propGetter = null)
    {
        //When obj.ObjectiType is empty, it also means that obj is empty
        if (SObject.IsNullOrEmpty(obj))
        {
            return null;
        }

        var jsonObj = new JsonObject();

        //Clearly write type | | Abstract structure
        if (writeTypeName || obj.InputType != obj.ObjectType)
        {
            jsonObj["@type"] = obj.ObjectType.GetFullTypeName();
        }

        var s = obj.GetStruct(AssetFilters.All);
        if (s != null)
        {
            foreach (DStructField field in s.AllStructFields)
            {
                var value = propGetter != null ? propGetter(obj, field) : obj.GetPropertyFormatted(field.Name);
                if (value != null)
                {
                    WriteJsonField(jsonObj, field.Name, value, userDataId, writeTypeName, condition, propGetter);
                }
                else
                {
                    // Do nothing.
                }
            }
        }
        else
        {
            foreach (var name in obj.GetPropertyNames())
            {
                var item = obj.GetPropertyFormatted(name);
                if (item != null)
                {
                    WriteJsonField(jsonObj, name, item, userDataId, writeTypeName, condition, propGetter);
                }
                else
                {
                    // Do nothing.
                }
            }
        }

        return jsonObj;
    }

    /// <summary>
    /// Converts an <see cref="SArray"/> to a <see cref="JsonArray"/> representation.
    /// Serializes each element in the array, handling primitives, strings, nested objects, arrays, keys, enums, and other value types.
    /// </summary>
    /// <param name="ary">The source array to serialize.</param>
    /// <param name="userDataId">When true, serializes keys and asset keys as data IDs; otherwise uses target IDs.</param>
    /// <param name="writeTypeName">When true, includes the @type property in nested object JSON output.</param>
    /// <param name="condition">Optional condition used when evaluating property values.</param>
    /// <param name="propGetter">Optional custom property getter for nested objects.</param>
    /// <returns>A <see cref="JsonArray"/> representing the serialized array, or null if the input is null.</returns>
    public static JsonArray ToJson(
        this SArray ary, bool userDataId, bool writeTypeName = true,
        ICondition condition = null, JsonPropertyGetter propGetter = null)
    {
        if (ary is null)
        {
            return null;
        }

        var jsonAry = new JsonArray();

        foreach (var item in ary.GetValues())
        {
            if (item is null)
            {
                jsonAry.Add(null);
            }
            else if (item.GetType().IsPrimitive || item is string)
            {
                jsonAry.Add(item);
            }
            else
            {
                switch (item)
                {
                    case DateTime dateTime:
                        jsonAry.Add(item.ToString());
                        break;

                    case SObject sObject:
                        jsonAry.Add(sObject.ToJson(userDataId, writeTypeName, condition, propGetter));
                        break;

                    case SArray sArray:
                        jsonAry.Add(sArray.ToJson(userDataId, writeTypeName, condition, propGetter));
                        break;

                    case SKey sKey:
                        jsonAry.Add(userDataId ? sKey.ToDataId() : sKey.TargetId.ToString());
                        break;

                    case SEnum sEnum:
                        jsonAry.Add(sEnum.Value);
                        break;

                    case SAssetKey sAssetKey:
                        jsonAry.Add(userDataId ? sAssetKey.ToDataId() : sAssetKey.TargetId.ToString());
                        break;

                    case SValue sValue:
                        jsonAry.Add(sValue.GetValue(condition));
                        break;

                    default:
                        jsonAry.Add(null);
                        break;
                }
            }
        }

        return jsonAry;
    }

    private static void WriteJsonField(
        JsonObject jsonObj, string name, object item, bool userDataId, bool writeTypeName = true,
        ICondition condition = null, JsonPropertyGetter propGetter = null)
    {
        if (item is null)
        {
            jsonObj.Add(name, null);
        }
        else if (item.GetType().IsPrimitive || item is string)
        {
            jsonObj.Add(name, item);
        }
        else
        {
            switch (item)
            {
                case DateTime dateTime:
                    jsonObj.Add(name, item.ToString());
                    break;

                case SObject sObject:
                    jsonObj.Add(name, sObject.ToJson(userDataId, writeTypeName, condition, propGetter));
                    break;

                case SArray sArray:
                    jsonObj.Add(name, sArray.ToJson(userDataId, writeTypeName, condition, propGetter));
                    break;

                case SKey sKey:
                    jsonObj.Add(name, userDataId ? sKey.ToDataId() : sKey.TargetId.ToString());
                    break;

                case SEnum sEnum:
                    jsonObj.Add(name, sEnum.Value);
                    break;

                case SAssetKey sAssetKey:
                    jsonObj.Add(name, userDataId ? sAssetKey.ToDataId() : sAssetKey.TargetId.ToString());
                    break;

                case SValue sValue:
                    jsonObj.Add(name, sValue.GetValue(condition));
                    break;

                default:
                    jsonObj.Add(name, null);
                    break;
            }
        }
    }

    /// <summary>
    /// Deserializes a <see cref="JsonObject"/> into an <see cref="SObject"/> instance.
    /// Resolves the object type from the @type property or a type hint, then recursively deserializes all child properties.
    /// </summary>
    /// <param name="jsonObj">The JSON object to deserialize.</param>
    /// <param name="options">Optional resource options providing type hints and enum auto-add behavior.</param>
    /// <returns>An <see cref="SObject"/> populated with the deserialized data, or null if the input is null.</returns>
    public static SObject FromJson(this JsonObject jsonObj, SItemResourceOptions options = null)
    {
        if (jsonObj is null)
        {
            return null;
        }

        string typeId = jsonObj["@type"] as string;
        SObject sobj;
        TypeDefinition typeDef;

        if (GlobalIdResolver.TryResolve(typeId, out Guid id))
        {
            typeDef = TypeDefinition.Resolve(id) ?? TypeDefinition.Empty;
            sobj = new SObject(typeDef);
        }
        else if (!TypeDefinition.IsNullOrEmpty(options?.TypeHint))
        {
            typeDef = options.TypeHint;
            sobj = new SObject(typeDef);
        }
        else
        {
            typeDef = null;
            sobj = new SObject();
        }

        var stype = typeDef?.GetTarget(AssetFilters.Default) as DCompond;

        foreach (var pair in jsonObj)
        {
            if (pair.Key.StartsWith("@"))
            {
                continue;
            }

            var field = stype?.GetPublicStructFieldFromBase(pair.Key);
            var subOptions = new SItemResourceOptions
            {
                TypeHint = field?.FieldType,
                AutoAddNewEnumValue = options?.AutoAddNewEnumValue == true
            };
            var item = FromJson(pair.Value, subOptions);
            sobj.SetProperty(pair.Key, item);
        }

        // Fix data once
        foreach (var pair in jsonObj)
        {
            if (!pair.Key.StartsWith("@"))
            {
                sobj.GetPropertyFormatted(pair.Key);
            }
        }

        return sobj;
    }

    /// <summary>
    /// Deserializes a <see cref="JsonObject"/> into a dictionary of property values based on a <see cref="SimpleType"/> definition.
    /// Maps JSON properties to their corresponding simple field types and recursively deserializes nested values.
    /// </summary>
    /// <param name="jsonObj">The JSON object to deserialize.</param>
    /// <param name="type">The simple type definition used to map field names and types.</param>
    /// <param name="options">Optional resource options providing type hints and enum auto-add behavior.</param>
    /// <returns>A dictionary mapping property names to their deserialized values, or null if the input is null.</returns>
    public static Dictionary<string, object> FromJson(this JsonObject jsonObj, SimpleType type, SItemResourceOptions options = null)
    {
        if (jsonObj is null)
        {
            return null;
        }

        Dictionary<string, SimpleField> fieldMap = [];
        foreach (var field in (type.Fields ?? []).SkipNull())
        {
            if (!string.IsNullOrWhiteSpace(field.Name))
            {
                fieldMap[field.Name] = field;
            }
        }

        string typeId = jsonObj["@type"] as string;
        Dictionary<string, object> dic = [];

        foreach (var pair in jsonObj)
        {
            if (pair.Key.StartsWith("@"))
            {
                continue;
            }

            var field = fieldMap.GetValueSafe(pair.Key);
            var subOptions = new SItemResourceOptions
            {
                TypeHint = field?.Type,
                AutoAddNewEnumValue = options?.AutoAddNewEnumValue == true
            };
            var item = FromJson(pair.Value, subOptions);
            dic[pair.Key] = item;
        }

        return dic;
    }

    /// <summary>
    /// Deserializes a <see cref="JsonArray"/> into an <see cref="SArray"/> instance.
    /// Recursively deserializes each element using the provided type hint for element type resolution.
    /// </summary>
    /// <param name="jsonArray">The JSON array to deserialize.</param>
    /// <param name="options">Optional resource options providing type hints and enum auto-add behavior.</param>
    /// <returns>An <see cref="SArray"/> populated with the deserialized elements, or null if the input is null.</returns>
    public static SArray FromJson(this JsonArray jsonArray, SItemResourceOptions options = null)
    {
        if (jsonArray is null)
        {
            return null;
        }

        var sary = new SArray();

        var typeHint = options?.TypeHint;
        if (typeHint?.IsArray == true)
        {
            typeHint = typeHint.ElementType;
        }

        var subOptions = new SItemResourceOptions
        {
            TypeHint = typeHint,
            AutoAddNewEnumValue = options?.AutoAddNewEnumValue == true
        };

        foreach (var item in jsonArray)
        {
            sary.Add(FromJson(item, subOptions));
        }

        // Fix data once
        for (int i = 0; i < sary.Count; i++)
        {
            sary.GetItemFormatted(i);
        }

        return sary;
    }


    private static object FromJson(object obj, SItemResourceOptions options = null)
    {
        if (obj is null)
        {
            return null;
        }
        else if (obj is JsonObject jsonObj)
        {
            return FromJson(jsonObj, options);
        }
        else if (obj is JsonArray jsonArray)
        {
            return FromJson(jsonArray, options);
        }
        else if (options?.TypeHint is { } typeHint)
        {
            if (typeHint.IsEnum)
            {
                string name = obj.ToString();

                var sEnum = new SEnum(typeHint)
                {
                    Value = name
                };

                if (sEnum.IsValid)
                {
                    return sEnum;
                }

                if (options?.AutoAddNewEnumValue == true && AutoAddNewEnumValue(typeHint, name))
                {
                    sEnum.Value = name;

                    if (sEnum.IsValid)
                    {
                        return sEnum;
                    }
                }

                //Invalid enumeration value, return missing prompt
                //return new SAIGeneration(name);
                return null;
            }
            else
            {
                var sItem = typeHint.CreateValue();
                if (sItem is SValue sValue)
                {
                    sValue.Value = obj;
                    sValue.AutoConvertValue();
                    return sValue;
                }
                else if (sItem is SKey sKey)
                {
                    var dataRow = AssetManager.Instance.GetAssetByResourceName<IDataAsset>(obj?.ToString());
                    dataRow ??= AssetManager.Instance.GetAsset<IDataAsset>(obj?.ToString());
                    sKey.TargetId = dataRow?.Id ?? Guid.Empty;
                    return sKey;
                }
                else if (sItem is SAssetKey sAssetKey)
                {
                    var assetType = sAssetKey.InputType?.NativeType;
                    var asset = AssetManager.Instance.GetAssetByResourceName(assetType, obj?.ToString());
                    asset ??= AssetManager.Instance.GetAsset(obj?.ToString());
                    sAssetKey.TargetId = asset?.Id ?? Guid.Empty;
                    return sAssetKey;
                }
                else if (sItem != null)
                {
                    return sItem;
                }

                //The created object is not SValue, unable to set value, return original value
                //return new SAIGeneration(obj.ToString());
                return null;
            }
        }
        else
        {
            return obj;
        }
    }

    private static bool AutoAddNewEnumValue(TypeDefinition type, string name)
    {
        if (TypeDefinition.IsNullOrEmpty(type) || !type.IsEnum)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if ((type.Target as DEnum)?.GetStorageObject(true) is not TypeDesignItem item)
        {
            return false;
        }

        if (item.GetDocument() is not ITypeDesignDocument doc)
        {
            return false;
        }

        return doc.AddField(item, name) != null;
    }
}