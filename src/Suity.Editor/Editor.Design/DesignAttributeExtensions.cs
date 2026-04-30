using Suity.Editor.Types;

namespace Suity.Editor.Design;

/// <summary>
/// Provides extension methods for design attributes.
/// </summary>
public static class DesignAttributeExtensions
{
    /// <summary>
    /// Gets the usage attribute value from the given attribute getter.
    /// </summary>
    public static string GetUsage(this IAttributeGetter hasAttr)
    {
        return hasAttr.GetAttribute<UsageAttribute>()?.Usage;
    }

    /// <summary>
    /// Gets whether source code automation is enabled for the given attribute getter.
    /// </summary>
    public static bool GetSourceCodeAutomation(this IAttributeGetter hasAttr)
    {
        bool codeAutomation = true;
        if (hasAttr.GetAttribute<SourceCodeAutomationAttribute>() is SourceCodeAutomationAttribute attr && !attr.DataStorage)
        {
            codeAutomation = false;
        }

        return codeAutomation;
    }


    public static bool GetIsSaveData(this IAttributeGetter hasAttr)
    {
        if (hasAttr.GetAttribute<SaveDataAttribute>() is not null)
        {
            return true;
        }

        if (hasAttr.GetAttribute<DataUsageAttribute>() ?.Usage == DataUsageMode.EntityData)
        {
            return true;
        }

        return false;
    }

    public static bool GetIsSaveData(this TypeDefinition dataType)
    {
        var targetType = dataType?.Target;

        return targetType?.GetIsSaveData() == true;
    }

    public static bool GetIsSaveData(this DType dataType)
    {
        IAttributeGetter hasAttr = dataType;

        return hasAttr?.GetIsSaveData() == true || (dataType?.ParentAsset as IAttributeGetter)?.GetIsSaveData() == true;
    }


    public static bool GetIsCompond(this DataStructureType type)
    {
        switch (type)
        {
            case DataStructureType.Struct:
            case DataStructureType.Abstract:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the given data usage mode represents a data-filling mode.
    /// </summary>
    public static bool GetIsDataFilling(this DataUsageMode usage)
    {
        switch (usage)
        {
            case DataUsageMode.DataGrid:
            case DataUsageMode.FlowGraph:
            case DataUsageMode.TreeGraph:
                return true;

            default:
                return false;
        }
    }

    public static bool GetIsLinked(this DataStructureType type, DataUsageMode usage)
    {
        if (type.GetIsCompond())
        {
            return usage.GetIsLinked();
        }

        return false;
    }

    public static bool GetIsLinked(this DataUsageMode usage)
    {
        switch (usage)
        {
            case DataUsageMode.DataGrid:
            case DataUsageMode.FlowGraph:
            case DataUsageMode.TreeGraph:
            case DataUsageMode.EntityData:
            case DataUsageMode.Entity:
                return true;

            default:
                return false;
        }
    }


    public static bool GetIsEntity(this TypeDefinition dataType) 
        => dataType?.Target?.GetIsEntity() == true;

    public static bool GetIsEntity(this IAttributeGetter hasAttr)
        => hasAttr.GetAttribute<DataUsageAttribute>()?.Usage == DataUsageMode.Entity;

    public static bool GetIsGraph(this DataUsageMode usage)
    {
        switch (usage)
        {
            case DataUsageMode.FlowGraph:
            case DataUsageMode.TreeGraph:
                return true;

            default:
                return false;
        }
    }

}