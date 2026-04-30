using ComputerBeacon.Json;
using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Reflecting;
using Suity.Synchonizing.Core;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Transferring;

#region SNameDocumentDRW

/// <summary>
/// Data transfer for SNamedDocument objects.
/// </summary>
public class SNamedDocumentDRW : DataRWTransfer<SNamedDocument>
{
    public override void Transfer(SNamedDocument doc, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.Preinput:
                HandlePreinput(doc, data, selection);
                break;

            case ContentTransferPipelines.InputProperty:
                HandleInputProperty(doc, data.Reader);
                break;

            case ContentTransferPipelines.InputCollection:
                HandleInputCollection(doc, data.Reader);
                break;

            case ContentTransferPipelines.Delete:
                HandleDelete(doc, selection);
                break;

            case ContentTransferPipelines.OutputProperty:
                HandleOutputProperty(doc, data.Writer, data.Options);
                break;

            case ContentTransferPipelines.OutputCollection:
                if (selection != null)
                {
                    HandleOutputCollection(doc, data.Writer, selection, data.Options);
                }
                else
                {
                    HandleOutputCollection(doc, data.Writer, data.Options);
                }
                break;
        }
    }

    private void HandlePreinput(SNamedDocument doc, DataRW data, ICollection<object> selection)
    {
        var reader = data.Reader;
        var items = doc.ItemCollection;
        var typeResolver = SyncTypes.GlobalResolver;
        var itemAryReader = reader.Nodes(items.FieldName);

        foreach (var itemReader in itemAryReader)
        {
            string typeName = itemReader.Node("@type").ReadString();
            if (string.IsNullOrWhiteSpace(typeName))
            {
                continue;
            }

            string name = itemReader.Node("Name").ReadString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string groupPath = itemReader.Node("@group").ReadString();
            if (string.IsNullOrWhiteSpace(groupPath))
            {
                groupPath = string.Empty;
            }

            bool adding = false;
            string localName = ResolveLocalName(doc, name);

            var item = items.GetItemAll(localName);
            if (item != null)
            {
                // Check if types match
                string itemTypeName = typeResolver.ResolveTypeName(item.GetType(), item);
                if (itemTypeName != typeName)
                {
                    items.RemoveItem(item);
                    item = null;
                }
            }

            if (item is null)
            {
                Type type = typeResolver.ResolveType(typeName, null);
                if (type is null)
                {
                    Logs.LogWarning($"Type resolved failed : {typeName}");
                    continue;
                }

                try
                {
                    item = type.CreateInstanceOf() as NamedItem;
                    adding = true;
                }
                catch (Exception err)
                {
                    err.LogError();
                    continue;
                }
            }

            if (item is null)
            {
                continue;
            }

            item.Name = localName;

            if (GetTransfer(item.GetType()) is { } transfer)
            {
                transfer.PreInput(item, new DataRW { Reader = itemReader });
            }

            selection?.Add(item);

            if (adding)
            {
                doc.EnsureGroupByPath(groupPath).AddItem(item);
                (data.NewObjects ??= []).Add(item);
            }
            else
            {
                if (item.GetGroupPath() != groupPath)
                {
                    item.ParentList?.Remove(item);
                    doc.EnsureGroupByPath(groupPath).AddItem(item);
                }
            }
        }
    }

    private void HandleInputProperty(SNamedDocument doc, IDataReader reader)
    {
        if (reader.Node("ImportName").ReadString() is { } importName && !string.IsNullOrWhiteSpace(importName))
        {
            doc.ImportName = importName;
        }

        if (reader.Node("NameSpace").ReadString() is { } nameSpace && !string.IsNullOrWhiteSpace(nameSpace))
        {
            doc.NameSpace = nameSpace.TrimStart('*');
        }

        if (reader.Node("Description").ReadString() is { } description && !string.IsNullOrWhiteSpace(description))
        {
            doc.Description = description;
        }
    }

    private void HandleInputCollection(SNamedDocument doc, IDataReader reader)
    {
        var items = doc.ItemCollection;
        if (items is null)
        {
            return;
        }

        var itemAryReader = reader.Nodes(items.FieldName);
        foreach (var itemReader in itemAryReader)
        {
            string typeName = itemReader.Node("@type").ReadString();
            if (string.IsNullOrWhiteSpace(typeName))
            {
                continue;
            }

            string name = itemReader.Node("Name").ReadString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var fieldCode = new FieldCode(name);
            string localName;

            if (!string.IsNullOrEmpty(fieldCode.FieldName))
            {
                localName = fieldCode.FieldName;
            }
            else
            {
                localName = fieldCode.MainName;
            }

            // Creation flow is in Preload
            var item = items.GetItemAll(localName);
            if (item is null)
            {
                continue;
            }

            if (GetTransfer(item.GetType()) is { } transfer)
            {
                transfer.Input(item, new DataRW { Reader = itemReader });
            }
        }
    }

    private void HandleDelete(SNamedDocument doc, ICollection<object> selection)
    {
        if (selection is null)
        {
            return;
        }

        var items = doc.ItemCollection;

        foreach (var item in selection.OfType<SNamedItem>())
        {
            items.RemoveItem(item);
        }
    }

    private void HandleOutputProperty(SNamedDocument doc, IDataWriter writer, object options)
    {
        string docTypeName = SyncTypes.GlobalResolver.ResolveTypeName(doc.GetType(), doc);
        if (!string.IsNullOrWhiteSpace(docTypeName))
        {
            writer.Node("@doc").WriteString(docTypeName);
        }

        if (!string.IsNullOrWhiteSpace(doc.ImportName))
        {
            writer.Node("ImportName").WriteString(doc.ImportName);
        }

        if (!string.IsNullOrWhiteSpace(doc.NameSpace))
        {
            writer.Node("NameSpace").WriteString(doc.NameSpace.TrimStart('*'));
        }

        if (!string.IsNullOrWhiteSpace(doc.Description))
        {
            writer.Node("Description").WriteString(doc.Description);
        }
    }

    private void HandleOutputCollection(SNamedDocument doc, IDataWriter writer, object options)
    {
        var items = doc.ItemCollection;

        int totalCount = items.AllItems.Count();

        var typeResolver = SyncTypes.GlobalResolver;

        if (totalCount > 0)
        {
            var aryWriter = writer.Nodes(items.FieldName, totalCount);

            foreach (var item in items.AllItemsSorted)
            {
                if (GetTransfer(item.GetType()) is { } transfer)
                {
                    var itemWriter = aryWriter.Item();

                    var typeName = typeResolver.ResolveTypeName(item.GetType(), item);
                    if (!string.IsNullOrWhiteSpace(typeName))
                    {
                        itemWriter.Node("@type").WriteString(typeName);
                    }

                    var groupPath = item.GetGroupPath();
                    if (!string.IsNullOrWhiteSpace(groupPath))
                    {
                        itemWriter.Node("@group").WriteString(groupPath);
                    }

                    transfer.Output(item, new DataRW { Writer = itemWriter, Options = options, });
                }
            }
            aryWriter.Finish();
        }
    }

    private void HandleOutputCollection(SNamedDocument doc, IDataWriter writer, IEnumerable<object> selection, object options)
    {
        var items = doc.ItemCollection;

        int totalCount = items.AllItems.Count();

        var typeResolver = SyncTypes.GlobalResolver;

        if (totalCount > 0)
        {
            var aryWriter = writer.Nodes(items.FieldName, totalCount);
            selection = selection.Where(o => (o as SNamedItem)?.GetDocument() == items.Document);

            foreach (var item in selection.SkipNull())
            {
                if (GetTransfer(item.GetType()) is { } transfer)
                {
                    var itemWriter = aryWriter.Item();

                    var typeName = typeResolver.ResolveTypeName(item.GetType(), item);
                    if (!string.IsNullOrWhiteSpace(typeName))
                    {
                        itemWriter.Node("@type").WriteString(typeName);
                    }

                    var groupPath = (item as NamedItem)?.GetGroupPath();
                    if (!string.IsNullOrWhiteSpace(groupPath))
                    {
                        itemWriter.Node("@group").WriteString(groupPath);
                    }

                    transfer.Output(item, new DataRW { Writer = itemWriter, Options = options });
                }
            }
            aryWriter.Finish();
        }
    }

    private static string ResolveLocalName(SNamedDocument doc, string name)
    {
        var fieldCode = new FieldCode(name);
        string localName;

        if (!string.IsNullOrEmpty(fieldCode.FieldName))
        {
            localName = fieldCode.FieldName;

            // Check if namespace matches
            if (doc.NameSpace != fieldCode.MainName)
            {
                Logs.LogWarning($"{name} does not match the name space defined in document:{doc.NameSpace}");
            }
        }
        else
        {
            localName = fieldCode.MainName;
        }

        return localName;
    }
}

#endregion

#region SNamedItemDRW

/// <summary>
/// Data transfer for SNamedItem objects.
/// </summary>
public class SNamedItemDRW : DataRWTransfer<SNamedItem>
{
    [ThreadStatic]
    private static HashSet<NamedField> _tempFields;

    public override void Transfer(SNamedItem item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.Preinput:
                HandlePreload(item, data.Reader);
                break;

            case ContentTransferPipelines.InputCollection:
                HandleLoadCollection(item, data);
                break;

            case ContentTransferPipelines.OutputProperty:
                HandleSaveProperty(item, data.Writer, data.Options);
                break;

            case ContentTransferPipelines.OutputCollection:
                HandleSaveCollection(item, data.Writer, data.Options);
                break;
        }
    }

    private void HandlePreload(SNamedItem item, IDataReader reader)
    {
        string name = reader.Node("Name").ReadString();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var fieldCode = new FieldCode(name);
            string localName;

            if (!string.IsNullOrEmpty(fieldCode.FieldName))
            {
                localName = fieldCode.FieldName;

                var doc = item.GetDocument();
                if (doc != null)
                {
                    // Check if namespace matches
                    if (doc.NameSpace != fieldCode.MainName)
                    {
                        Logs.LogWarning($"{name} does not match the name space defined in document:{doc.NameSpace}");
                    }
                }
            }
            else
            {
                localName = fieldCode.MainName;
            }

            item.Name = localName;
        }
    }

    private void HandleLoadCollection(SNamedItem target, DataRW data)
    {
        var fieldList = target.FieldList;
        if (fieldList is null)
        {
            return;
        }

        if (!LoadCollection(target, data, fieldList.FieldName))
        {
            LoadCollection(target, data, "Fields");
        }
    }

    private bool LoadCollection(SNamedItem target, DataRW data, string nodeName)
    {
        var reader = data.Reader;

        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return false;
        }

        if (!reader.HasNode(nodeName))
        {
            return false;
        }

        var fieldList = target.FieldList;
        if (fieldList is null)
        {
            return false;
        }

        (_tempFields ??= []).Clear();

        var fieldAryReader = reader.Nodes(nodeName);
        foreach (var fieldReader in fieldAryReader)
        {
            string fieldName = fieldReader.Node("Name").ReadString();
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                continue;
            }

            bool adding = false;

            var field = fieldList.GetItem(fieldName);
            if (field is null)
            {
                field = fieldList.CreateDefaultItem();
                adding = true;
            }

            if (field is null)
            {
                continue;
            }

            if (GetTransfer(field.GetType()) is { } transfer)
            {
                transfer.Input(field, new DataRW { Reader = fieldReader });
            }

            if (adding)
            {
                fieldList.Add(field);
            }

            _tempFields.Add(field);
        }

        // Remove extra items
        List<NamedField> removes = null;
        foreach (var field in fieldList)
        {
            if (!_tempFields.Contains(field))
            {
                (removes ??= [])?.Add(field);
            }
        }

        if (removes != null)
        {
            foreach (var field in removes)
            {
                fieldList.Remove(field);
            }
        }

        return true;
    }

    private void HandleSaveProperty(SNamedItem item, IDataWriter writer, object options)
    {
        var doc = item.GetDocument();

        string name;
        if (doc?.NameSpace is string ns && !string.IsNullOrWhiteSpace(ns))
        {
            name = $"{ns.TrimStart('*')}.{item.Name}";
        }
        else
        {
            name = item.Name;
        }

        writer.Node("Name").WriteString(name);
    }

    private void HandleSaveCollection(SNamedItem target, IDataWriter writer, object options)
    {
        var fieldList = target?.FieldList;
        if (fieldList?.Count > 0)
        {
            string nodeName = fieldList.FieldName;
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                nodeName = "Fields";
            }

            var listWriter = writer.Nodes(nodeName, fieldList.Count);
            foreach (var field in fieldList)
            {
                if (GetTransfer(field.GetType()) is { } transfer)
                {
                    var fieldWriter = listWriter.Item();
                    transfer.Output(field, new DataRW { Writer = fieldWriter, Options = options });
                }
            }
            listWriter.Finish();
        }
    }
}

#endregion

#region DesignItemDRW

/// <summary>
/// Data transfer for DesignItem objects.
/// </summary>
public class DesignItemDRW : DataRWTransfer<DesignItem>
{
    public override void Transfer(DesignItem item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    item.Description = data.Reader.Node("Description").ReadString() ?? string.Empty;

                    if (data.Reader.Node("ToolTips").ReadString() is { } toolTips
                        && !string.IsNullOrWhiteSpace(toolTips))
                    {
                        item.SetAttribute<ToolTipsAttribute>(o => o.ToolTips = toolTips);
                    }

                    break;
                }

            case ContentTransferPipelines.OutputProperty:
                {
                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        data.Writer.Node("Description").WriteString(item.Description);
                    }

                    if (item.GetAttribute<ToolTipsAttribute>() is { } toolTipsAttribute
                        && !string.IsNullOrWhiteSpace(toolTipsAttribute.ToolTips))
                    {
                        data.Writer.Node("ToolTips").WriteString(toolTipsAttribute.ToolTips);
                    }

                    break;
                }
        }
    }
}

#endregion

#region TypeDesignItemDRW

/// <summary>
/// Data transfer for TypeDesignItem objects.
/// </summary>
public class TypeDesignItemDRW : DataRWTransfer<TypeDesignItem>
{
    public override void Transfer(TypeDesignItem item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("ImportName").ReadString() is { } importName
                        && !string.IsNullOrWhiteSpace(importName))
                    {
                        item.IsImported = true;
                        item.ImportName = importName;
                    }
                    else
                    {
                        item.IsImported = false;
                        item.ImportName = string.Empty;
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    if (item.IsImported && !string.IsNullOrWhiteSpace(item.ImportName))
                    {
                        data.Writer.Node("ImportName").WriteString(item.ImportName);
                    }
                }
                break;
        }
    }
}

#endregion

#region FunctionDesignItemDRW

/// <summary>
/// Data transfer for FunctionDesignItem objects.
/// </summary>
public class FunctionDesignItemDRW : DataRWTransfer<FunctionDesignItem>
{
    public override void Transfer(FunctionDesignItem item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("ReturnType").ReadString() is string returnType && !string.IsNullOrWhiteSpace(returnType))
                    {
                        var typeInfo = TypeDefinition.Resolve(returnType);
                        item.ReturnType.FieldType = typeInfo;
                    }
                    else
                    {
                        item.ReturnType.FieldType = TypeDefinition.Empty;
                    }

                    item.IsUser = data.Reader.Node("IsUser").ReadBoolean();
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    string returnType = item.ReturnType.FieldType.ToExportString(true);
                    data.Writer.Node("ReturnType").WriteString(returnType);

                    if (item.IsUser)
                    {
                        data.Writer.Node("IsUser").WriteBoolean(true);
                    }
                }
                break;
        }
    }
}

#endregion

#region SNamedFieldDRW

/// <summary>
/// Data transfer for SNamedField objects.
/// </summary>
public class SNamedFieldDRW : DataRWTransfer<SNamedField>
{
    public override void Transfer(SNamedField field, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    string name = data.Reader.Node("Name").ReadString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        field.Name = name;
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                data.Writer.Node("Name").WriteString(field.Name);
                break;
        }
    }
}

#endregion

#region DesignTypeFieldDRW

/// <summary>
/// Data transfer for DesignField objects.
/// </summary>
public class DesignTypeFieldDRW : DataRWTransfer<DesignField>
{
    public override void Transfer(DesignField item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("ToolTips").ReadString() is { } toolTips
                        && !string.IsNullOrWhiteSpace(toolTips))
                    {
                        item.Attributes.SetAttribute<ToolTipsAttribute>().ToolTips = toolTips;
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    if (item.Attributes.GetAttribute<ToolTipsAttribute>() is { } toolTipsAttribute
                        && !string.IsNullOrWhiteSpace(toolTipsAttribute.ToolTips))
                    {
                        data.Writer.Node("ToolTips").WriteString(toolTipsAttribute.ToolTips);
                    }
                }
                break;
        }
    }
}

#endregion

#region ParameterFieldItemDRW

/// <summary>
/// Data transfer for ParameterField objects.
/// </summary>
public class ParameterFieldDRW : DataRWTransfer<ParameterField>
{
    public override void Transfer(ParameterField field, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Type").ReadString() is string varType && !string.IsNullOrWhiteSpace(varType))
                    {
                        var typeInfo = TypeDefinition.Resolve(varType);
                        field.VariableType.FieldType = typeInfo;
                    }
                    else
                    {
                        field.VariableType.FieldType = TypeDefinition.Empty;
                    }

                    field.Optional = data.Reader.Node("Nullable").ReadBoolean();
                    field.Description = data.Reader.Node("Description").ReadString() ?? string.Empty;
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    string varType = field.VariableType.FieldType.ToExportString(true);
                    data.Writer.Node("Type").WriteString(varType);

                    if (field.Optional)
                    {
                        data.Writer.Node("Nullable").WriteBoolean(true);
                    }

                    if (!string.IsNullOrWhiteSpace(field.Description))
                    {
                        data.Writer.Node("Description").WriteString(field.Description);
                    }
                }
                break;
        }
    }
}

#endregion

#region FunctionParameterDRW

/// <summary>
/// Data transfer for FunctionParameter objects.
/// </summary>
public class FunctionParameterDRW : DataRWTransfer<FunctionParameter>
{
    public override void Transfer(FunctionParameter parameter, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                parameter.IsParameter = !data.Reader.Node("IsVariable").ReadBoolean();
                parameter.IsConnector = !data.Reader.Node("NoConnector").ReadBoolean();
                break;

            case ContentTransferPipelines.OutputProperty:
                if (!parameter.IsParameter)
                {
                    data.Writer.Node("IsVariable").WriteBoolean(false);
                }

                if (!parameter.IsConnector)
                {
                    data.Writer.Node("NoConnector").WriteBoolean(false);
                }
                break;
        }
    }
}

#endregion

#region SObjectDRW

/// <summary>
/// Data transfer for SObject objects.
/// </summary>
public class SObjectDRW : DataRWTransfer<SObject>
{
    public override void Transfer(SObject obj, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.ReadObject() is JsonObject jobj)
                    {
                        var jsonService = EditorServices.JsonResource;
                        if (jsonService.FromJson(jobj, data.Options as SItemResourceOptions) is SObject sobj)
                        {
                            sobj.MergeTo(obj, true);
                        }
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    var jsonService = EditorServices.JsonResource;
                    var jobj = jsonService.GetJson(obj, options: data.Options as JsonResourceOptions);

                    data.Writer.WriteObject(jobj);
                }
                break;
        }
    }
}

#endregion
