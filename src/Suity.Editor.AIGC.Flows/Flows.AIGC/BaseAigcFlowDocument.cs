using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Documents;
using Suity.Editor.Flows.Nodes;
using Suity.Editor.Flows.SubFlows;
using Suity.Selecting;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// Base class for AIGC flowchart documents, providing common functionality for agent and tool flowcharts.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of asset builder used for this document.</typeparam>
public class BaseAigcFlowDocument<TAssetBuilder> : FlowDocument<TAssetBuilder>, IDropInCheck
    where TAssetBuilder : AssetBuilder, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAigcFlowDocument{TAssetBuilder}"/> class.
    /// </summary>
    public BaseAigcFlowDocument()
    {
        ItemCollection.FieldIcon = CoreIconCache.Flow;
    }

    /// <summary>
    /// Gets a value indicating whether preview computation is enabled. Always returns false for AIGC flows.
    /// </summary>
    public override bool PreviewComputeEnabled => false;

    /// <summary>
    /// Gets the selection list of available flow nodes for this document.
    /// </summary>
    /// <returns>An instance of <see cref="AigcFlowSelectionList"/> containing available nodes.</returns>
    public override ISelectionList GetFactoryNodeList() => AigcFlowSelectionList.Instance;

    /// <summary>
    /// Gets the data style for the specified data type.
    /// </summary>
    /// <param name="dataType">The data type to get the style for.</param>
    /// <returns>The flow data style for the specified type, or the base implementation result if not found.</returns>
    public override IFlowDataStyle GetDataStyle(string dataType)
    {
        if (TypeFlowDataStyle.GetDataStyle(dataType) is { } type)
        {
            return type;
        }

        return base.GetDataStyle(dataType);
    }

    #region IDropInCheck

    /// <summary>
    /// Checks whether the specified value can be dropped into the flow diagram.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is an <see cref="Asset"/>; otherwise, false.</returns>
    public virtual bool DropInCheck(object value)
    {
        if (value is Asset)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts a dropped value into a flow node or input value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A flow node or input value representing the dropped asset, or null if conversion fails.</returns>
    public virtual object DropInConvert(object value)
    {
        if (value is SubFlowDefinitionAsset page)
        {
            return new PageFunctionNode(page);
        }

        if (value is ArticleAsset article)
        {
            return new GetArticle(article);
        }

        //if (value is TextAsset textAsset)
        //{
        //    if (textAsset.PageCount > 1)
        //    {
        //        return new PagedTextAssetRef(textAsset);
        //    }
        //    else
        //    {
        //        return new TextAssetRef(textAsset);
        //    }
        //}

        if (value is Asset asset)
        {
            try
            {
                return new InputValue(asset);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    #endregion

    /// <summary>
    /// Determines whether a node of the specified type can be created.
    /// </summary>
    /// <param name="type">The type of node to check.</param>
    /// <returns>Always returns true, allowing all node types to be created.</returns>
    protected override bool GetCanCreateNode(Type type)
    {
        return true;
    }
}