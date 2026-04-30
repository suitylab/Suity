using ComputerBeacon.Json;
using Suity;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Services;
using Suity.Editor.Transferring;
using Suity.Editor.Values;
using Suity.Json;
using Suity.NodeQuery;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// External implementation of <see cref="PropertyTargetExternal"/> providing property target creation, population, and serialization capabilities.
/// </summary>
internal class PropertyTargetExternalBK : PropertyTargetExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="PropertyTargetExternalBK"/>.
    /// </summary>
    public static PropertyTargetExternalBK Instance { get; } = new();

    /// <inheritdoc/>
    public override PropertyTarget CreatePropertyTarget(IEnumerable<object> objs)
    {
        return new RootPropertyTarget(objs);
    }

    /// <inheritdoc/>
    public override PropertyTarget CreatePropertyTarget(IEnumerable<object> objs, string propertyName)
    {
        return new RootPropertyTarget(objs, propertyName);
    }

    /// <inheritdoc/>
    public override PropertyTarget? PopulatePath(PropertyTarget target, SyncPath path, bool forceRepopulate)
    {
        PropertyTarget? current = target;
        if (current is null)
        {
            return null;
        }

        if (SyncPath.IsNullOrEmpty(path))
        {
            if (forceRepopulate)
            {
                PopulateProperties(current);
            }

            return current;
        }

        foreach (var item in path)
        {
            if (current is null)
            {
                return null;
            }

            if (current.FieldCount == 0 || forceRepopulate)
            {
                if (!PopulateProperties(current))
                {
                    return null;
                }
            }

            switch (item)
            {
                case string name:
                    current = current.GetField(name);
                    break;

                case int index:
                    current = current.ArrayTarget?.GetOrCreateElementTarget(index);
                    break;

                case Guid guid:
                    // This is a workaround and can only retrieve a single value.
                    current = (current.ArrayTarget?.Elements ?? current.Fields)
                        .Where(o => o.GetValues().OfType<ISyncPathIdObject>().Any(o => o.Id == guid))
                        .FirstOrDefault();
                    break;

                default:
                    return null;
            }
        }

        return current;
    }

    /// <inheritdoc/>
    public override bool PopulateProperties(PropertyTarget target, IImGuiPropertyEditorProvider? provider = null)
    {
        Type? commonType = target.GetValues().SkipNull().GetCommonType() ?? target.PresetType;
        if (commonType is null)
        {
            return false;
        }

        if (target.ArrayTarget is { })
        {
            return true;
        }

        var arrayHandler =
            provider?.GetArrayHandler(target)
            ?? PropertyEditorProviderBK.Instance.GetArrayHandler(target);

        if (arrayHandler is { })
        {
            target.SetupArray(arrayHandler);
            return true;
        }

        var func = provider?.GetPopulateFunction(commonType, target.PresetType)
            ?? PropertyEditorProviderBK.Instance.GetPopulateFunction(commonType, target.PresetType);

        if (func is { })
        {
            func(target);

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public override object? GetSItemFieldInfomation(PropertyTarget target)
    {
        SItem firstObj = target.GetValues().OfType<SItem>().FirstOrDefault();
        if (firstObj == null)
        {
            return null;
        }

        if (firstObj.FieldId != Guid.Empty)
        {
            return firstObj.FieldId;
        }
        else
        {
            return firstObj.InputType;
        }
    }

    /// <inheritdoc/>
    public override IValueAction? RepairSItem(PropertyTarget target)
    {
        if (target.ReadOnly) return null;

        var values = target.GetValues();

        if (!values.Any())
        {
            DialogUtility.ShowMessageBoxAsync(IPropertyGridExtensions.MsgObjectNoSelected);
            return null;
        }

        SItem[] oldObjs = values.As<SItem>().ToArray();
        SItem[] newObjs = new SItem[oldObjs.Length];

        for (int i = 0; i < oldObjs.Length; i++)
        {
            if (oldObjs[i] == null)
            {
                continue;
            }

            newObjs[i] = Cloner.Clone(oldObjs[i]);

            var type = oldObjs[i].GetTypeFromParent();
            newObjs[i].EnsureInputType(type);
        }

        return target.SetValuesAction(newObjs.OfType<object>());
    }

    /// <inheritdoc/>
    public override IValueAction? RepairSContainer(PropertyTarget target)
    {
        if (target.ReadOnly) return null;

        SContainer[] oldObjs = target.GetValues().As<SContainer>().ToArray();

        if (oldObjs.Length == 0)
        {
            DialogUtility.ShowMessageBoxAsync(IPropertyGridExtensions.MsgObjectNoSelected);
            return null;
        }

        SContainer[] newObjs = new SContainer[oldObjs.Length];

        for (int i = 0; i < oldObjs.Length; i++)
        {
            newObjs[i] = Cloner.Clone(oldObjs[i]);

            var type = oldObjs[i].GetTypeFromParent();
            newObjs[i].RepairDeep(type);
        }

        return target.SetValuesAction(newObjs.OfType<object>());
    }

    /// <inheritdoc/>
    public override string? GetText(PropertyTarget target, ViewAdvancedEditFeatures feature)
    {
        var firstObj = target.GetValues().FirstOrDefault();
        if (firstObj is null)
        {
            return null;
        }

        switch (feature)
        {
            case ViewAdvancedEditFeatures.XML:
                {
                    var writer = new XmlNodeWriter("SuityFragment");
                    Suity.Synchonizing.Core.Serializer.Serialize(firstObj, writer, SyncIntent.DataExport);

                    return writer.ToString();
                }

            case ViewAdvancedEditFeatures.Json:
                if (firstObj is SItem sitem)
                {
                    var obj = EditorServices.JsonResource.GetJson(sitem);

                    switch (obj)
                    {
                        case JsonObject jobj:
                            return jobj.ToString(true);

                        case JsonArray jary:
                            return jary.ToString(true);
                    }
                }
                else if (ContentTransfer<DataRW>.GetTransfer(firstObj?.GetType()) is { } transfer)
                {
                    var writer = new JsonDataWriter();
                    writer.Node("@format").WriteString("SuityJson");
                    transfer.Output(firstObj, new DataRW { Writer = writer });

                    return writer.ToString(true);
                }

                return null;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    internal override IValueAction? SetText(PropertyTarget target, ViewAdvancedEditFeatures feature, string text)
    {
        if (target.ReadOnly)
        {
            return null;
        }

        var values = target.GetValues();
        int count = values.Count();

        if (count == 0)
        {
            return null;
        }

        var firstObj = target.GetValues().FirstOrDefault();
        if (firstObj is null)
        {
            return null;
        }

        switch (feature)
        {
            case ViewAdvancedEditFeatures.XML:
                {
                    var reader = XmlNodeReader.FromXml(text);
                    if (!reader.Exist)
                    {
                        DialogUtility.ShowMessageBoxAsync(IPropertyGridExtensions.MsgFragmentInvalid);
                        return null;
                    }

                    if (reader.NodeName != "SuityFragment")
                    {
                        DialogUtility.ShowMessageBoxAsync(IPropertyGridExtensions.MsgFragmentInvalid);
                        return null;
                    }

                    object? obj = Suity.Synchonizing.Core.Serializer.Deserialize(reader, firstObj.GetType()) as SItem;
                    if (obj is null)
                    {
                        return null;
                    }

                    object[] newObjs = new object[count];
                    newObjs[0] = obj;

                    for (int i = 1; i < count; i++)
                    {
                        newObjs[i] = Cloner.Clone(obj);
                    }

                    return target.SetValuesAction(newObjs);
                }

            case ViewAdvancedEditFeatures.Json:
                if (firstObj is SItem sitem)
                {
                    var typeHint = (sitem as SObject)?.ObjectType ?? sitem?.InputType;

                    SItem? item;
                    try
                    {
                        object? o = Parser.Parse(text);
                        item = EditorServices.JsonResource.FromJson(o, new() { TypeHint = typeHint }); // Add type hint functionality
                    }
                    catch (Exception)
                    {
                        DialogUtility.ShowMessageBoxAsync(IPropertyGridExtensions.MsgJsonFormatInvalid);

                        return null;
                    }

                    if (item is null)
                    {
                        return null;
                    }

                    SItem[] newObjs = new SItem[count];
                    newObjs[0] = item;

                    for (int i = 1; i < count; i++)
                    {
                        newObjs[i] = Cloner.Clone(item);
                    }

                    return target.SetValuesAction(newObjs.OfType<object>());
                }
                else if (ContentTransfer<DataRW>.GetTransfer(firstObj?.GetType()) is { } transfer)
                {
                    JsonDataReader reader;

                    try
                    {
                        reader = new JsonDataReader(text);
                    }
                    catch (Exception err)
                    {
                        err.LogError("Json parsing failed");

                        return null;
                    }

                    try
                    {
                        transfer.Input(firstObj, new DataRW { Reader = reader }, true);
                    }
                    catch (Exception err)
                    {
                        err.LogError("Json reading failed");
                    }

                    object[] newObjs = new object[count];
                    newObjs[0] = firstObj;

                    try
                    {
                        for (int i = 1; i < count; i++)
                        {
                            newObjs[i] = Cloner.Clone(firstObj);
                        }
                    }
                    catch (Exception err)
                    {
                        err.LogError("Object cloning failed");
                    }

                    return target.SetValuesAction(newObjs.OfType<object>());
                }

                return null;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override IValueAction? SetDynamicAction(PropertyTarget target, Type? dynamicType)
    {
        object[] values = target.GetValues().Select(o => SItem.ResolveOriginValue(o)).ToArray();
        if (values.Length == 0)
        {
            return null;
        }

        IValueAction act;

        if (dynamicType is { })
        {
            SDynamic[] dynamics = new SDynamic[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                SDynamic d = (SDynamic)dynamicType.CreateInstanceOf();
                d.Value = values[i];
                dynamics[i] = d;
            }

            act = target.SetValuesAction(dynamics.OfType<object>());
        }
        else
        {
            SItem[] items = values.Select(v => SItem.ResolveSItem(SItem.ResolveOriginValue(v))).ToArray();
            act = target.SetValuesAction(items.OfType<object>());
        }

        return act;
    }

    /// <inheritdoc/>
    public override PreviewPath ToPreviewPath(PropertyTarget target)
    {
        SyncPathBuilder builder = target.GetSyncPathBuilder().Trim();

        bool isArrayElement = target.Parent?.ArrayTarget != null;

        var path = new PreviewPath
        (
            builder.ToSyncPath(),
            isArrayElement ? $"{target.Parent?.PropertyName}[{target.Index}]" : target.PropertyName,
            isArrayElement ? $"{target.Parent?.DisplayName}[{target.Index}]" : target.DisplayName
        );

        return path;
    }
}