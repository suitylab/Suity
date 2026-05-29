using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Services;
using System.Collections.Generic;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Converts a <see cref="SubFlowDefinitionAsset"/> to an <see cref="ISubFlow"/>.
/// </summary>
public class SubFlowDefinitionAssetToISubFlowDefConverter : AssetLinkToTypeConverter<SubFlowDefinitionAsset, ISubFlow>
{
    /// <inheritdoc/>
    public override ISubFlow Convert(SubFlowDefinitionAsset objFrom)
    {
        return objFrom.GetDiagramItem()?.Node;
    }
}

/// <summary>
/// Converts an <see cref="ISubFlow"/> to a <see cref="SubFlowDefinitionAsset"/>.
/// </summary>
public class ISubFlowDefToSubFlowDefinitionAssetConverter : TypeToAssetLinkConverter<ISubFlow, SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override SubFlowDefinitionAsset Convert(ISubFlow objFroms)
    {
        if (objFroms is SubflowDefinitionNode node)
        {
            return node.GetAsset() as SubFlowDefinitionAsset;
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// Converts a <see cref="SubFlowDefinitionAsset"/> to a text representation of its page instance.
/// </summary>
public class SubFlowDefinitionAssetToTextConverter : TypeToTextConverter<SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override string Convert(SubFlowDefinitionAsset objFrom)
    {
        var item = objFrom.GetDiagramItem();
        if (item is null)
        {
            return null;
        }

        var option = new PageCreateOption
        {
            Mode = PageElementMode.Function,
        };

        var element = new SubFlowInstance(item, option);

        return element.ToSimpleType().ToString();
    }
}

/// <summary>
/// Converts an array of <see cref="SubFlowDefinitionAsset"/> to a combined text representation.
/// </summary>
public class SubFlowDefinitionAssetArrayToTextConverter : AssetLinkArrayToTextConverter<SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override string Convert(SubFlowDefinitionAsset[] objFroms)
    {
        List<string> list = [];

        foreach (var obj in objFroms)
        {
            if (obj.GetDiagramItem() is not { } item)
            {
                continue;
            }

            var option = new PageCreateOption
            {
                Mode = PageElementMode.Function,
            };

            var element = new SubFlowInstance(item, option);

            string s = element.ToSimpleType().ToString();

            if (!string.IsNullOrWhiteSpace(s))
            {
                list.Add(s);
            }
        }

        return string.Join("\r\n\r\n", list);
    }
}

/// <summary>
/// Converts an <see cref="IPageAsset"/> to a text representation of its page instance.
/// </summary>
public class IPageAssetToTextConverter : TypeToTextConverter<IPageAsset>
{
    public static bool MinimalFormat { get; set; } = true;

    /// <inheritdoc/>
    public override string Convert(IPageAsset objFrom)
    {
        var option = new PageCreateOption
        {
            Mode = PageElementMode.Function,
        };

        var instance = objFrom.CreatePageInstance(option);

        return instance?.ToSimpleType().ToString(!MinimalFormat);
    }
}

/// <summary>
/// Converts an array of <see cref="IPageAsset"/> to a combined text representation.
/// </summary>
public class IPageAssetArrayToTextConverter : AssetLinkArrayToTextConverter<IPageAsset>
{
    /// <inheritdoc/>
    public override string Convert(IPageAsset[] objFroms)
    {
        List<string> list = [];

        foreach (var obj in objFroms.SkipNull())
        {
            var option = new PageCreateOption
            {
                Mode = PageElementMode.Function,
            };

            var instance = obj.CreatePageInstance(option);
            if (instance is null)
            {
                continue;
            }

            string s = instance.ToSimpleType().ToString(!IPageAssetToTextConverter.MinimalFormat);

            if (!string.IsNullOrWhiteSpace(s))
            {
                list.Add(s);
            }
        }

        return string.Join("\r\n\r\n", list);
    }
}

public class IArticleToScratchPadItemConverter : TypeConverter<IArticle, ScratchPadItem>
{
    /// <inheritdoc/>
    public override ScratchPadItem Convert(IArticle objFrom)
    {
        return ScratchPadItem.FromArticle(objFrom);
    }
}

public class IArticleArrayToScratchPadItemArrayConverter : TypeConverter<IArticle[], ScratchPadItem[]>
{
    /// <inheritdoc/>
    public override ScratchPadItem[] Convert(IArticle[] objFrom)
    {
        if (objFrom is null)
        {
            return null;
        }

        var list = new List<ScratchPadItem>();
        foreach (var article in objFrom)
        {
            var item = ScratchPadItem.FromArticle(article);
            if (item != null)
            {
                list.Add(item);
            }
        }

        return list.ToArray();
    }
}

public class ScratchPadItemToTextConverter : TypeToTextConverter<ScratchPadItem>
{
    /// <inheritdoc/>
    public override string Convert(ScratchPadItem objFrom)
    {
        if (objFrom is null)
        {
            return null;
        }

        return objFrom.ToString();
    }
}

public class ScratchPadItemArrayToTextConverter : TypeToTextConverter<ScratchPadItem[]>
{
    /// <inheritdoc/>
    public override string Convert(ScratchPadItem[] objFrom)
    {
        if (objFrom is null)
        {
            return null;
        }
        List<string> list = [];
        foreach (var item in objFrom)
        {
            string s = item.ToString();
            list.Add(s);
        }
        return string.Join("\r\n\r\n", list);
    }
}