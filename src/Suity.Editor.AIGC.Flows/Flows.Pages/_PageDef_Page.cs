using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Defines conditions under which parameters are considered complete.
/// </summary>
public enum ParameterConditions
{
    /// <summary>
    /// All parameters must be met.
    /// </summary>
    All,
    /// <summary>
    /// Only one parameter needs to be met.
    /// </summary>
    Any,
}

/// <summary>
/// Abstract base class for AIGC page definition nodes that support parameter completion conditions.
/// </summary>
public abstract class AigcPageDefPageNode : AigcPageDefNode
{
    readonly ValueProperty<ParameterConditions> _completionCondition = new("CompletionCondition", "Completion Condition", ParameterConditions.All, "Condition for parameter completion. All means all must be met, Any means only one needs to be met.");

    /// <summary>
    /// Gets the condition for parameter completion.
    /// </summary>
    public ParameterConditions CompletionCondition => _completionCondition.Value;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _completionCondition.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);
        
        _completionCondition.InspectorField(setup);
    }
}


#region PageDefinitionNode

/// <summary>
/// AIGC interactive page definition node that can act as a group and page.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page)]
[DisplayText("AIGC Page", "*CoreIcon|Page")]
[DisplayOrder(5000)]
[ToolTipsText("AIGC interactive page definition")]
public class PageDefinitionNode : AigcPageDefPageNode, IGroupFlowNode, IAigcPage
{
    Guid _id;

    FlowNodeConnector _resultConnector;

    readonly AssetListProperty<IAigcToolAsset> _tools = new("Tools", "Tool List");
    readonly ValueProperty<bool> _useParentArticle = new("UseParentArticle", "Use Parent Article", false, "Use the parent article as the article record for this page's content.");

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDefinitionNode"/> class.
    /// </summary>
    public PageDefinitionNode()
        : base()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Page;

    /// <summary>
    /// Gets the group name, using description if available, otherwise the node name.
    /// </summary>
    public string GroupName
    {
        get
        {
            var desc = base.Description;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }
            else
            {
                return base.Name;
            }
        }
    }

    /// <inheritdoc/>
    public override Color? TitleColor => GroupFlowNode.GroupHeaderColor;

    /// <inheritdoc/>
    public override Color? BackgroundColor => base.TitleColor;

    /// <summary>
    /// Gets the collection of tools associated with this page.
    /// </summary>
    public IEnumerable<IAigcToolAsset> Tools => _tools.Targets;

    /// <summary>
    /// Gets a value indicating whether to use the parent article as the article record for this page's content.
    /// </summary>
    public bool UseParentArticle => _useParentArticle.Value;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tools.Sync(sync);
        _useParentArticle.Sync(sync);

        if (this.GetAsset() is { } asset && asset.Id != _id)
        {
            _id = asset.Id;
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _useParentArticle.InspectorField(setup);
        _tools.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        if (_id != Guid.Empty)
        {
            this.AddAssociateOutputConnector("Page", typeof(PageDefinitionAsset).FullName, _id);
        }

        var type = TypeDefinition.FromNative<IPageConnection>();
        _resultConnector = FixedNodeConnector.CreateControlInput("Result", type, "Result Page");
        AddConnector(_resultConnector);
    }

    #region IAigcPage

    /// <inheritdoc/>
    public IAigcPage GetPageDefinition() => null;

    /// <inheritdoc/>
    public IAigcPage GetPageResult()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_resultConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as IAigcPage;
    }

    /// <inheritdoc/>
    public object GetDocumentItem() => this.DiagramItem;
    #endregion
}

#endregion

#region PageDefinitionDiagramItem

/// <summary>
/// Diagram item representing a <see cref="PageDefinitionNode"/> in the flow diagram.
/// </summary>
public class PageDefinitionDiagramItem : FlowDiagramItem<PageDefinitionNode, PageDefinitionAssetBuilder>
{
    private DocumentEntry _docEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDefinitionDiagramItem"/> class.
    /// </summary>
    public PageDefinitionDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDefinitionDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page definition node.</param>
    public PageDefinitionDiagramItem(PageDefinitionNode node)
        : base(node)
    {
    }

    /// <summary>
    /// Gets or sets the document entry associated with this diagram item.
    /// </summary>
    public DocumentEntry DocEntry
    {
        get => _docEntry;
        protected set
        {
            if (_docEntry == value)
            {
                return;
            }

            if (_docEntry != null)
            {
                _docEntry.DirtyMarked -= _docEntry_DirtyMarked;
            }

            _docEntry = value;

            if (_docEntry != null)
            {
                _docEntry.DirtyMarked += _docEntry_DirtyMarked;
            }
        }
    }

    private void _docEntry_DirtyMarked(object sender, DirtyEventArgs e)
    {
        this.TargetAsset?.NotifyUpdated();
    }

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "Page";

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
        => AigcPageDefNode.VerifyName(name);

    /// <inheritdoc/>
    protected override void OnAdded()
    {
        base.OnAdded();

        DocEntry = this.GetDocument()?.Entry;
    }

    /// <inheritdoc/>
    protected override void OnRemoved(NamedRootCollection model)
    {
        base.OnRemoved(model);

        DocEntry = null;
    }
}
#endregion

#region PageDefinitionAsset

/// <summary>
/// Asset representing a page definition that can be used as a tool.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Page Definition", Color = AigcColors.Page, Icon = "*CoreIcon|Page")]
public class PageDefinitionAsset : Asset, IAigcPageDefinitionAsset, IAigcToolAsset
{
    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Page;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageDefinitionAsset"/> class.
    /// </summary>
    public PageDefinitionAsset()
    {
        UpdateAssetTypes(typeof(IAigcPageDefinitionAsset), typeof(IAigcToolAsset));
    }

    /// <summary>
    /// Gets the diagram item associated with this asset.
    /// </summary>
    /// <returns>The <see cref="PageDefinitionDiagramItem"/>, or null.</returns>
    public PageDefinitionDiagramItem GetDiagramItem() => this.GetStorageObject(true) as PageDefinitionDiagramItem;

    #region IAigcToolAsset

    /// <inheritdoc/>
    public bool IsStartupPage => false;

    /// <inheritdoc/>
    public IAigcPage GetBaseDefinition() => GetDiagramItem()?.Node;

    /// <inheritdoc/>
    public IAigcSkill GetSkillDefinition() => null;

    /// <inheritdoc/>
    public IAigcPageInstance CreatePageInstance(PageElementOption option)
    {
        if (GetDiagramItem() is not { } toolPageItem)
        {
            return null;
        }

        return new AigcPageInstance(toolPageItem, option);
    }

    #endregion
}

/// <summary>
/// Asset builder for <see cref="PageDefinitionAsset"/>.
/// </summary>
public class PageDefinitionAssetBuilder : AssetBuilder<PageDefinitionAsset>
{
}

#endregion

#region PageSubNode

/// <summary>
/// AIGC interactive page's detached extension sub-page node.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page)]
[DisplayText("AIGC Sub Page", "*CoreIcon|Page")]
[DisplayOrder(4999)]
[ToolTipsText("AIGC interactive page's detached extension page")]
public class PageSubNode : AigcPageDefPageNode, IGroupFlowNode, IAigcPage
{
    readonly AssetProperty<PageDefinitionAsset> _page = new("Page", "Page");

    FlowNodeConnector _resultConnector;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubNode"/> class.
    /// </summary>
    public PageSubNode()
    {
        Description = "Sub Page";

        _page.Filter = new SameDocFilter(this);

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Page;

    /// <summary>
    /// Gets the group name, using description if available, otherwise the node name.
    /// </summary>
    public string GroupName
    {
        get
        {
            var desc = base.Description;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }
            else
            {
                return base.Name;
            }
        }
    }

    /// <summary>
    /// Gets the target page diagram item referenced by this sub-page.
    /// </summary>
    /// <returns>The <see cref="PageDefinitionDiagramItem"/>, or null.</returns>
    public PageDefinitionDiagramItem GetTargetPageItem() => _page.Target?.GetDiagramItem();

    /// <inheritdoc/>
    public override Color? TitleColor => GroupFlowNode.GroupHeaderColor;

    /// <inheritdoc/>
    public override Color? BackgroundColor => base.TitleColor;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _page.Sync(sync);
        if (sync.IsSetterOf(_page.Property.Name))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _page.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        if (_page.Id != Guid.Empty)
        {
            this.AddAssociateInputConnector("Page", typeof(PageDefinitionAsset).FullName, _page.Id);
        }

        var type = TypeDefinition.FromNative<IPageConnection>();
        _resultConnector = FixedNodeConnector.CreateControlInput("Result", type, "Result Page");
        AddConnector(_resultConnector);
    }

    #region IAigcPage

    /// <inheritdoc/>
    public IAigcPage GetPageDefinition() => null;

    /// <inheritdoc/>
    public IAigcPage GetPageResult()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_resultConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as IAigcPage;
    }

    /// <inheritdoc/>
    public object GetDocumentItem() => this.DiagramItem;
    #endregion
}

#endregion

#region PageSubDiagramItem

/// <summary>
/// Diagram item representing a <see cref="PageSubNode"/> in the flow diagram.
/// </summary>
public class PageSubDiagramItem : FlowDiagramItem<PageSubNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubDiagramItem"/> class.
    /// </summary>
    public PageSubDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSubDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page sub node.</param>
    public PageSubDiagramItem(PageSubNode node)
        : base(node)
    {
    }
}

#endregion

#region PageResultNode

/// <summary>
/// AIGC interactive page's detached result page node.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.Page)]
[DisplayText("AIGC Result Page", "*CoreIcon|CheckList")]
[DisplayOrder(4998)]
[ToolTipsText("AIGC interactive page's detached result page")]
public class PageResultNode : AigcPageDefPageNode, IGroupFlowNode, IAigcPage
{
    FlowNodeConnector _defConnector;

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.CheckList;

    /// <summary>
    /// Gets the group name, using description if available, otherwise the node name.
    /// </summary>
    public string GroupName
    {
        get
        {
            var desc = base.Description;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }
            else
            {
                return base.Name;
            }
        }
    }

    /// <inheritdoc/>
    public override Color? TitleColor => GroupFlowNode.GroupHeaderColor;

    /// <inheritdoc/>
    public override Color? BackgroundColor => base.TitleColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageResultNode"/> class.
    /// </summary>
    public PageResultNode()
    {
        Description = "Result";
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var type = TypeDefinition.FromNative<IPageConnection>();
        _defConnector = FixedNodeConnector.CreateControlOutput("Definition", type, "Definition Page");
        AddConnector(_defConnector);
    }

    #region IAigcPage

    /// <inheritdoc/>
    public IAigcPage GetPageDefinition()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_defConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as IAigcPage;
    }

    /// <inheritdoc/>
    public IAigcPage GetPageResult() => null;

    /// <inheritdoc/>
    public object GetDocumentItem() => this.DiagramItem;
    #endregion
}

#endregion

#region PageResultDiagramItem

/// <summary>
/// Diagram item representing a <see cref="PageResultNode"/> in the flow diagram.
/// </summary>
public class PageResultDiagramItem : FlowDiagramItem<PageResultNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageResultDiagramItem"/> class.
    /// </summary>
    public PageResultDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageResultDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page sub node.</param>
    public PageResultDiagramItem(PageSubNode node)
        : base(node)
    {
    }
}

#endregion

#region Converters

/// <summary>
/// Converts a <see cref="PageDefinitionAsset"/> to an <see cref="IAigcPage"/>.
/// </summary>
public class PageDefinitionAssetToIAigcPageConverter : AssetLinkToTypeConverter<PageDefinitionAsset, IAigcPage>
{
    /// <inheritdoc/>
    public override IAigcPage Convert(PageDefinitionAsset objFrom)
    {
        return objFrom.GetDiagramItem()?.Node;
    }
}

/// <summary>
/// Converts an <see cref="IAigcPage"/> to a <see cref="PageDefinitionAsset"/>.
/// </summary>
public class IAigcPageToPageDefinitionAssetConverter : TypeToAssetLinkConverter<IAigcPage, PageDefinitionAsset>
{
    /// <inheritdoc/>
    public override PageDefinitionAsset Convert(IAigcPage objFroms)
    {
        if (objFroms is PageDefinitionNode node)
        {
            return node.GetAsset() as PageDefinitionAsset;
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// Converts a <see cref="PageDefinitionAsset"/> to a text representation of its page instance.
/// </summary>
public class PageDefinitionAssetToTextConverter : TypeToTextConverter<PageDefinitionAsset>
{
    /// <inheritdoc/>
    public override string Convert(PageDefinitionAsset objFrom)
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

        var element = new AigcPageInstance(item, option);

        return element.ToSimpleType().ToString();
    }
}

/// <summary>
/// Converts an array of <see cref="PageDefinitionAsset"/> to a combined text representation.
/// </summary>
public class PageDefinitionAssetArrayToTextConverter : AssetLinkArrayToTextConverter<PageDefinitionAsset>
{
    /// <inheritdoc/>
    public override string Convert(PageDefinitionAsset[] objFroms)
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

            var element = new AigcPageInstance(item, option);

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
/// Converts an <see cref="IAigcToolAsset"/> to a text representation of its page instance.
/// </summary>
public class IAigcSkillToTextConverter : TypeToTextConverter<IAigcToolAsset>
{
    /// <inheritdoc/>
    public override string Convert(IAigcToolAsset objFrom)
    {
        var item = (objFrom as PageDefinitionAsset)?.GetDiagramItem();
        if (item is null)
        {
            return null;
        }

        var option = new PageElementOption
        {
            Mode = PageElementMode.Function,
        };

        var element = new AigcPageInstance(item, option);

        return element.ToSimpleType().ToString();
    }
}

/// <summary>
/// Converts an array of <see cref="IAigcToolAsset"/> to a combined text representation.
/// </summary>
public class IAigcSkillArrayToTextConverter : AssetLinkArrayToTextConverter<IAigcToolAsset>
{
    /// <inheritdoc/>
    public override string Convert(IAigcToolAsset[] objFroms)
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
#endregion
