using Suity.Editor.Selecting;
using Suity.Editor.Transferring;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Documents.TypeEdit;

#region AbstractTypeDRW

/// <summary>
/// Data read/write transfer for <see cref="AbstractType"/> objects.
/// </summary>
public class AbstractTypeDRW : DataRWTransfer<AbstractType>
{
}
#endregion

#region ClassFunctionDRW

/// <summary>
/// Data read/write transfer for <see cref="ClassFunction"/> objects.
/// </summary>
public class ClassFunctionDRW : DataRWTransfer<ClassFunction>
{
    /// <inheritdoc/>
    public override void Transfer(ClassFunction func, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    bool isPublic = data.Reader.Node("IsPublic").ReadBoolean();

                    func.AccessMode = isPublic ? AssetAccessMode.Public : AssetAccessMode.Private;
                    func.ActionMode = data.Reader.Node("ActionMode").ReadBoolean();
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                if (func.AccessMode == AssetAccessMode.Public)
                {
                    data.Writer.Node("IsPublic").WriteBoolean(true);
                }

                if (func.ActionMode)
                {
                    data.Writer.Node("ActionMode").WriteBoolean(true);
                }
                break;
        }
    }
}
#endregion

#region EnumItemBaseDRW

/// <summary>
/// Data read/write transfer for <see cref="EnumItemBase"/> objects.
/// </summary>
public class EnumItemBaseDRW : DataRWTransfer<EnumItemBase>
{
    /// <inheritdoc/>
    public override void Transfer(EnumItemBase item, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                item.Description = data.Reader.Node("Description").ReadString() ?? string.Empty;
                break;

            case ContentTransferPipelines.OutputProperty:
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    data.Writer.Node("Description").WriteString(item.Description);
                }
                break;
        }
    }
}
#endregion

#region EnumItemDRW

/// <summary>
/// Data read/write transfer for <see cref="EnumItem"/> objects.
/// </summary>
public class EnumItemDRW : DataRWTransfer<EnumItem>
{
    /// <inheritdoc/>
    public override void Transfer(EnumItem e, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                e.Value = data.Reader.Node("Value").ReadInt32();
                break;

            case ContentTransferPipelines.OutputProperty:
                if (e.Value != 0)
                {
                    data.Writer.Node("Value").WriteInt32(e.Value);
                }
                break;
        }
    }
}
#endregion

#region EnumToStructTypeDRW

/// <summary>
/// Data read/write transfer for <see cref="EnumToStructType"/> objects.
/// </summary>
public class EnumToStructTypeDRW : DataRWTransfer<EnumToStructType>
{
    /// <inheritdoc/>
    public override void Transfer(EnumToStructType type, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Enum").ReadString() is string baseEnumStr && !string.IsNullOrWhiteSpace(baseEnumStr))
                    {
                        var denum = AssetManager.Instance.GetAssetByResourceName<DEnum>(baseEnumStr);
                        type.BaseEnum = new AssetSelection<DEnum>(denum);
                    }
                    else
                    {
                        type.BaseEnum = null;
                    }

                    if (data.Reader.Node("Type").ReadString() is string fieldTypeStr && !string.IsNullOrWhiteSpace(fieldTypeStr))
                    {
                        var fieldType = TypeDefinition.Resolve(fieldTypeStr);
                        type.FieldType.FieldType = fieldType;
                    }

                    type.Optional = data.Reader.Node("Optional").ReadBoolean(); //TODO: Change to Optional
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    var baseEnum = type.BaseEnum.Target;
                    if (baseEnum != null)
                    {
                        data.Writer.Node("Enum").WriteString(baseEnum.ToDataId(true));
                    }

                    string typeName = type.FieldType.FieldType.ToExportString(true);
                    data.Writer.Node("Type").WriteString(typeName);

                    if (type.Optional)
                    {
                        data.Writer.Node("Optional").WriteBoolean(true);
                    }
                }
                break;
        }
    }
}
#endregion

#region EnumTypeDRW

/// <summary>
/// Data read/write transfer for <see cref="EnumType"/> objects.
/// </summary>
public class EnumTypeDRW : DataRWTransfer<EnumType>
{
    /// <inheritdoc/>
    public override void Transfer(EnumType e, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                if (data.Reader.Node("IdAutomation").ReadString() is string idAutoStr && Enum.TryParse(idAutoStr, out IdAutomations idAuto))
                {
                    e.IdAutomation = idAuto;
                }
                else
                {
                    e.IdAutomation = IdAutomations.Index;
                }

                if (!HandleSimpleReadItems(e, data.Reader, "Items"))
                {
                    HandleSimpleReadItems(e, data.Reader, "Parameters");
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                if (e.IdAutomation != IdAutomations.Index)
                {
                    data.Writer.Node("IdAutomation").WriteString(e.IdAutomation.ToString());
                }
                break;
        }
    }

    /// <summary>
    /// Handles reading simple enum item entries from the data reader.
    /// </summary>
    /// <param name="e">The enum type to populate.</param>
    /// <param name="reader">The data reader.</param>
    /// <param name="nodeName">The node name to read from.</param>
    /// <returns>True if items were read successfully, false otherwise.</returns>
    private bool HandleSimpleReadItems(EnumType e, IDataReader reader, string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return false;
        }

        if (!reader.HasNode(nodeName))
        {
            return false;
        }

        var fieldList = e.FieldList;

        foreach (var simpleReader in reader.Nodes(nodeName))
        {
            var name = simpleReader.ReadString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            bool adding = false;

            var field = fieldList.GetItem(name);
            if (field is null)
            {
                field = fieldList.CreateDefaultItem();
                field.Name = name;
                adding = true;
            }

            if (field is null)
            {
                continue;
            }

            if (adding)
            {
                fieldList.Add(field);
            }
        }

        return true;
    }
}
#endregion

#region EventTypeDRW

/// <summary>
/// Data read/write transfer for <see cref="EventType"/> objects.
/// </summary>
public class EventTypeDRW : DataRWTransfer<EventType>
{
}
#endregion

#region LogicModuleDRW

/// <summary>
/// Data read/write transfer for <see cref="LogicModule"/> objects.
/// </summary>
public class LogicModuleDRW : DataRWTransfer<LogicModule>
{
}

#endregion

#region LogicModuleComponentDRW

/// <summary>
/// Data read/write transfer for <see cref="LogicModuleComponent"/> objects.
/// </summary>
public class LogicModuleComponentDRW : DataRWTransfer<LogicModuleComponent>
{
    /// <inheritdoc/>
    public override void Transfer(LogicModuleComponent module, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Component").ReadString() is string s && !string.IsNullOrWhiteSpace(s))
                    {
                        var comp = AssetManager.Instance.GetAssetByResourceName<DStruct>(s);
                        module._compSelection = new AssetSelection<DStruct>(comp);
                    }
                    else
                    {
                        module._compSelection = null;
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    var comp = module._compSelection?.Target;
                    if (comp != null)
                    {
                        data.Writer.Node("Component").WriteString(comp.ToDataId());
                    }
                }
                break;
        }
    }
}

#endregion

#region StructFieldBaseDRW

/// <summary>
/// Data read/write transfer for <see cref="StructFieldItem"/> objects.
/// </summary>
public class StructFieldBaseDRW : DataRWTransfer<StructFieldItem>
{
    /// <inheritdoc/>
    public override void Transfer(StructFieldItem field, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Description").ReadString() is string desc && !string.IsNullOrWhiteSpace(desc))
                    {
                        field.Description = desc;
                    }
                    else
                    {
                        field.Description = string.Empty;
                    }
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                if (!string.IsNullOrWhiteSpace(field.Description))
                {
                    data.Writer.Node("Description").WriteString(field.Description);
                }
                break;
        }
    }
}

#endregion

#region StructFieldDRW

/// <summary>
/// Data read/write transfer for <see cref="StructField"/> objects.
/// </summary>
public class StructFieldDRW : DataRWTransfer<StructField>
{
    /// <inheritdoc/>
    public override void Transfer(StructField field, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Type").ReadString() is string typeName && !string.IsNullOrWhiteSpace(typeName))
                    {
                        var typeInfo = TypeDefinition.Resolve(typeName);
                        field.FieldType.FieldType = typeInfo;
                    }
                    else
                    {
                        field.FieldType.FieldType = TypeDefinition.Empty;
                    }

                    field.Optional = data.Reader.Node("Optional").ReadBoolean();
                    field.Unit = data.Reader.Node("Unit").ReadString() ?? string.Empty;
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    string typeName = field.FieldType.FieldType.ToExportString(true);
                    data.Writer.Node("Type").WriteString(typeName);

                    if (field.Optional)
                    {
                        data.Writer.Node("Optional").WriteBoolean(true);
                    }

                    if (!string.IsNullOrWhiteSpace(field.Unit))
                    {
                        data.Writer.Node("Unit").WriteString(field.Unit);
                    }
                }
                break;
        }
    }
}

#endregion

#region StructTypeBaseDRW

/// <summary>
/// Data read/write transfer for <see cref="StructTypeBase"/> objects.
/// </summary>
public class StructTypeBaseDRW : DataRWTransfer<StructTypeBase>
{
    /// <inheritdoc/>
    public override void Transfer(StructTypeBase type, DataRW data, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        switch (pipeline)
        {
            case ContentTransferPipelines.InputProperty:
                {
                    if (data.Reader.Node("Base").ReadString() is string s && !string.IsNullOrWhiteSpace(s))
                    {
                        var baseType = AssetManager.Instance.GetAssetByResourceName<DAbstract>(s);
                        type.BaseType = new AssetSelection<DAbstract>(baseType);
                    }
                    else
                    {
                        type.BaseType = null;
                    }

                    type.IsValueType = data.Reader.Node("IsValueType").ReadBoolean();
                }
                break;

            case ContentTransferPipelines.OutputProperty:
                {
                    var baseType = type.BaseTypeTarget;
                    if (baseType != null)
                    {
                        data.Writer.Node("Base").WriteString(baseType.ToDataId());
                    }

                    if (type.IsValueType)
                    {
                        data.Writer.Node("IsValueType").WriteBoolean(true);
                    }
                }
                break;
        }
    }
}

#endregion

#region StructTypeDRW

/// <summary>
/// Data read/write transfer for <see cref="StructType"/> objects.
/// </summary>
public class StructTypeDRW : DataRWTransfer<StructType>
{
}

#endregion

#region TypeDesignDocumentDRW

/// <summary>
/// Data read/write transfer for <see cref="TypeDesignDocument"/> objects.
/// </summary>
public class TypeDesignDocumentDRW : DataRWTransfer<TypeDesignDocument>
{
}

#endregion
