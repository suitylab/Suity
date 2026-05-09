using Suity.Collections;
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

        var option = new PageElementOption
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

            var option = new PageElementOption
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
    /// <inheritdoc/>
    public override string Convert(IPageAsset objFrom)
    {
        var option = new PageElementOption
        {
            Mode = PageElementMode.Function,
        };

        var element = objFrom.CreatePageInstance(option);

        return element?.ToSimpleType().ToString();
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
            var option = new PageElementOption
            {
                Mode = PageElementMode.Function,
            };

            var element = obj.CreatePageInstance(option);
            if (element is null)
            {
                continue;
            }

            string s = element.ToSimpleType().ToString();

            if (!string.IsNullOrWhiteSpace(s))
            {
                list.Add(s);
            }
        }

        return string.Join("\r\n\r\n", list);
    }
}
