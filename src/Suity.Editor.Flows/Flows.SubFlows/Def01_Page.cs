using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Flows.SubFlows;

#region SubflowDefinitionNode

/// <summary>
/// Sub-flow interactive page definition node that can act as a group and page.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.Page)]
[DisplayText("Sub-flow Page", "*CoreIcon|Page")]
[DisplayOrder(5000)]
[ToolTipsText("Sub-flow interactive page definition")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageDefinitionNode")]
public class SubflowDefinitionNode : SubflowDefNode, IGroupFlowNode, ISubFlowPage
{
    Guid _id;

    FlowNodeConnector _resultConnector;

    readonly AssetListProperty<IToolDefAsset> _tools = new("Tools", "Tool List");
    readonly ValueProperty<bool> _useParentArticle = new("UseParentArticle", "Use Parent Article", false, "Use the parent article as the article record for this page's content.");

    /// <summary>
    /// Initializes a new instance of the <see cref="SubflowDefinitionNode"/> class.
    /// </summary>
    public SubflowDefinitionNode()
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
    public IEnumerable<IToolDefAsset> Tools => _tools.Targets;

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
            this.AddAssociateOutputConnector("Page", typeof(SubFlowDefinitionAsset).FullName, _id);
        }

        var type = TypeDefinition.FromNative<IBranchConnection>();
        _resultConnector = FixedNodeConnector.CreateControlInput("Result", type, "Result Page");
        AddConnector(_resultConnector);
    }

    #region ISubFlowDef

    /// <inheritdoc/>
    public override ISubFlowPage GetPageDefinition() => null;

    /// <inheritdoc/>
    public override ISubFlowPage GetPageResult()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_resultConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as ISubFlowPage;
    }

    /// <inheritdoc/>
    public override object GetDocumentItem() => this.DiagramItem;
    #endregion
}

/// <summary>
/// Diagram item representing a <see cref="SubflowDefinitionNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageDefinitionDiagramItem")]
public class SubFlowDefinitionDiagramItem : FlowDiagramItem<SubflowDefinitionNode, SubFlowDefinitionAssetBuilder>
{
    private DocumentEntry _docEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowDefinitionDiagramItem"/> class.
    /// </summary>
    public SubFlowDefinitionDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowDefinitionDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page definition node.</param>
    public SubFlowDefinitionDiagramItem(SubflowDefinitionNode node)
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
        => SubFlowNode.VerifyName(name);

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

/// <summary>
/// Asset representing a page definition that can be used as a tool.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Definition", Color = FlowColors.Page, Icon = "*CoreIcon|Page")]
public class SubFlowDefinitionAsset : Asset,
    ISubFlowDefAsset,
    IToolDefAsset,
    ISubFlowAsset
{
    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Page;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowDefinitionAsset"/> class.
    /// </summary>
    public SubFlowDefinitionAsset()
    {
        UpdateAssetTypes(typeof(ISubFlowDefAsset), typeof(ISubFlowAsset));
    }

    /// <summary>
    /// Gets the diagram item associated with this asset.
    /// </summary>
    /// <returns>The <see cref="SubFlowDefinitionDiagramItem"/>, or null.</returns>
    public SubFlowDefinitionDiagramItem GetDiagramItem() => this.GetStorageObject(true) as SubFlowDefinitionDiagramItem;

    #region ISubFlowAsset

    /// <inheritdoc/>
    public bool IsStartup => false;

    /// <inheritdoc/>
    public ISubFlowPage GetBaseDefinition() => GetDiagramItem()?.Node;

    /// <inheritdoc/>
    public ISubFlowInstance CreateInstance(PageElementOption option)
    {
        if (GetDiagramItem() is not { } toolPageItem)
        {
            return null;
        }

        return new SubFlowInstance(toolPageItem, option);
    }

    #endregion
}

/// <summary>
/// Asset builder for <see cref="SubFlowDefinitionAsset"/>.
/// </summary>
public class SubFlowDefinitionAssetBuilder : AssetBuilder<SubFlowDefinitionAsset>
{
}

#endregion

#region SubflowBranchNode

/// <summary>
/// Sub-flow interactive page's detached extension sub-page node.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.Page)]
[DisplayText("Sub-flow Branch Page", "*CoreIcon|Page")]
[DisplayOrder(4999)]
[ToolTipsText("Sub-flow interactive page's detached branch page")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageSubNode")]
public class SubflowBranchNode : SubflowDefNode, IGroupFlowNode, ISubFlowPage
{
    readonly AssetProperty<SubFlowDefinitionAsset> _page = new("Page", "Page");

    FlowNodeConnector _resultConnector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubflowBranchNode"/> class.
    /// </summary>
    public SubflowBranchNode()
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
    /// <returns>The <see cref="SubFlowDefinitionDiagramItem"/>, or null.</returns>
    public SubFlowDefinitionDiagramItem GetTargetPageItem() => _page.Target?.GetDiagramItem();

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
            this.AddAssociateInputConnector("Page", typeof(SubFlowDefinitionAsset).FullName, _page.Id);
        }

        var type = TypeDefinition.FromNative<IBranchConnection>();
        _resultConnector = FixedNodeConnector.CreateControlInput("Result", type, "Result Page");
        AddConnector(_resultConnector);
    }

    #region ISubFlowDef

    /// <inheritdoc/>
    public override ISubFlowPage GetPageDefinition() => null;

    /// <inheritdoc/>
    public override ISubFlowPage GetPageResult()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_resultConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as ISubFlowPage;
    }

    /// <inheritdoc/>
    public override object GetDocumentItem() => this.DiagramItem;
    #endregion
}

/// <summary>
/// Diagram item representing a <see cref="SubflowBranchNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageSubDiagramItem")]
public class SubflowBranchDiagramItem : FlowDiagramItem<SubflowBranchNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubflowBranchDiagramItem"/> class.
    /// </summary>
    public SubflowBranchDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubflowBranchDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page sub node.</param>
    public SubflowBranchDiagramItem(SubflowBranchNode node)
        : base(node)
    {
    }
}

#endregion

#region PageResultNode

/// <summary>
/// Sub-flow interactive page's detached result page node.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.Page)]
[DisplayText("Sub-flow Result Page", "*CoreIcon|CheckList")]
[DisplayOrder(4998)]
[ToolTipsText("Sub-flow interactive page's detached result page")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageResultNode")]
public class SubFlowResultNode : SubflowDefNode, IGroupFlowNode, ISubFlowPage
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
    /// Initializes a new instance of the <see cref="SubFlowResultNode"/> class.
    /// </summary>
    public SubFlowResultNode()
    {
        Description = "Result";
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var type = TypeDefinition.FromNative<IBranchConnection>();
        _defConnector = FixedNodeConnector.CreateControlOutput("Definition", type, "Definition Page");
        AddConnector(_defConnector);
    }

    #region ISubFlowDef

    /// <inheritdoc/>
    public override ISubFlowPage GetPageDefinition()
    {
        if (this.Diagram is not { } diagram)
        {
            return null;
        }

        var node = diagram.GetLinkedConnector(_defConnector)?.ParentNode;
        if (node is null)
        {

        }

        return node as ISubFlowPage;
    }

    /// <inheritdoc/>
    public override ISubFlowPage GetPageResult() => null;

    /// <inheritdoc/>
    public override object GetDocumentItem() => this.DiagramItem;
    #endregion
}

/// <summary>
/// Diagram item representing a <see cref="SubFlowResultNode"/> in the flow diagram.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageResultDiagramItem")]
public class SubFlowResultDiagramItem : FlowDiagramItem<SubFlowResultNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowResultDiagramItem"/> class.
    /// </summary>
    public SubFlowResultDiagramItem()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowResultDiagramItem"/> class with the specified node.
    /// </summary>
    /// <param name="node">The page sub node.</param>
    public SubFlowResultDiagramItem(SubflowBranchNode node)
        : base(node)
    {
    }
}

#endregion

#region Converters

/// <summary>
/// Converts a <see cref="SubFlowDefinitionAsset"/> to an <see cref="ISubFlowPage"/>.
/// </summary>
public class SubFlowDefinitionAssetToISubFlowDefConverter : AssetLinkToTypeConverter<SubFlowDefinitionAsset, ISubFlowPage>
{
    /// <inheritdoc/>
    public override ISubFlowPage Convert(SubFlowDefinitionAsset objFrom)
    {
        return objFrom.GetDiagramItem()?.Node;
    }
}

/// <summary>
/// Converts an <see cref="ISubFlowPage"/> to a <see cref="SubFlowDefinitionAsset"/>.
/// </summary>
public class ISubFlowDefToSubFlowDefinitionAssetConverter : TypeToAssetLinkConverter<ISubFlowPage, SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override SubFlowDefinitionAsset Convert(ISubFlowPage objFroms)
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
/// Converts an <see cref="ISubFlowAsset"/> to a text representation of its page instance.
/// </summary>
public class ISubFlowAssetToTextConverter : TypeToTextConverter<ISubFlowAsset>
{
    /// <inheritdoc/>
    public override string Convert(ISubFlowAsset objFrom)
    {
        var item = (objFrom as SubFlowDefinitionAsset)?.GetDiagramItem();
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
/// Converts an array of <see cref="ISubFlowAsset"/> to a combined text representation.
/// </summary>
public class ISubFlowAssetArrayToTextConverter : AssetLinkArrayToTextConverter<ISubFlowAsset>
{
    /// <inheritdoc/>
    public override string Convert(ISubFlowAsset[] objFroms)
    {
        List<string> list = [];

        foreach (var obj in objFroms.SkipNull())
        {
            var option = new PageElementOption
            {
                Mode = PageElementMode.Function,
            };

            var element = obj.CreateInstance(option);
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
