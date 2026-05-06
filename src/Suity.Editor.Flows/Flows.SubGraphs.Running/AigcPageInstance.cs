using Suity;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Flows;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Design;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Flows.Pages;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Represents an instance of an AIGC page within a flow, managing page elements, parameters, and computation context.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Page Instance", Icon = "*CoreIcon|Page")]
public class AigcPageInstance : AigcPageElement, IFlowCallerContext, IAigcPageInstance
{
    /// <summary>
    /// Property key used to store the skill asset reference.
    /// </summary>
    public const string SKILL_PROP = "__skill__";

    private readonly PageDefinitionDiagramItem _pageDefinition;
    private readonly PageDefinitionNode _pageNode;


    private readonly PageDefinitionAsset _asset;

    private readonly List<AigcPageElement> _list = [];
    private readonly List<GroupPageElement> _groups = [];
    private readonly Dictionary<string, AigcPageElement> _dic = [];
    private readonly HashSet<AigcPageElement> _allElements = [];
    private PageEndElement _currentEndElement;

    private readonly AssetProperty<IAigcToolAsset> _skill = new(SKILL_PROP, "Skill");
    private string _skillName;

    private readonly IConversationImGui _conversation;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcPageInstance"/> class.
    /// </summary>
    /// <param name="pageDefinition">The page definition diagram item.</param>
    /// <param name="option">The page element configuration options.</param>
    /// <param name="skill">Optional skill asset to associate with this page instance.</param>
    public AigcPageInstance(PageDefinitionDiagramItem pageDefinition, PageElementOption option, IAigcToolAsset skill = null)
        : base(pageDefinition)
    {
        _pageDefinition = pageDefinition ?? throw new ArgumentNullException(nameof(pageDefinition));
        _pageNode = _pageDefinition.Node ?? throw new ArgumentNullException(nameof(pageDefinition));
        _asset = pageDefinition.TargetAsset as PageDefinitionAsset ?? throw new ArgumentNullException(nameof(pageDefinition.TargetAsset));
        Option = option;
        _skill.Target = skill;

        _conversation = EditorServices.ImGuiService.CreateConversationImGui(pageDefinition.Name, false);

        Build();
    }

    /// <summary>
    /// Occurs when the page title is updated.
    /// </summary>
    public event EventHandler TitleUpdated;
    /// <summary>
    /// Occurs when a refresh of the page is requested.
    /// </summary>
    public event EventHandler RefreshRequesting;
    /// <summary>
    /// Occurs when a result is output from the page.
    /// </summary>
    public event EventHandler ResultOutput;
    /// <summary>
    /// Occurs when a flow computation is being configured.
    /// </summary>
    public event EventHandler<IFlowComputation> ConfigComputation;
    /// <summary>
    /// Occurs when an undo/redo action is requested.
    /// </summary>
    public event EventHandler<UndoRedoAction> DoActionRequesting;
    /// <summary>
    /// Occurs when a page parameter value is set.
    /// </summary>
    public event EventHandler<IPageParameter> ParameterSet;


    #region Core Props

    /// <summary>
    /// Gets the selection for the skill asset associated with this page.
    /// </summary>
    public AssetSelection<IAigcToolAsset> SkillAssetSelection => _skill.Selection;

    /// <summary>
    /// Gets the last computation that was executed on this page.
    /// </summary>
    public IFlowComputation LastComputation { get; private set; }

    /// <summary>
    /// Gets the base page definition asset.
    /// </summary>
    public PageDefinitionAsset BaseAsset => _asset;

    /// <summary>
    /// Gets the condition that determines when page parameters are considered complete.
    /// </summary>
    public ParameterConditions ParameterCondition { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this page should use the parent article.
    /// </summary>
    public bool UseParentArticle { get; private set; }

    /// <inheritdoc/>
    public override string Name => _skillName ?? base.Name;

    /// <summary>
    /// Gets or sets the tooltips text for this page.
    /// </summary>
    public string Tooltips { get; private set; }

    /// <summary>
    /// Gets the conversation interface for this page instance.
    /// </summary>
    public IConversationImGui Conversation => _conversation;

    #endregion

    #region IAigcPageInstance

    /// <summary>
    /// Gets the owner of this page instance.
    /// </summary>
    public object Owner => Option?.Owner;

    /// <summary>
    /// Gets the base page definition that this instance is based on.
    /// </summary>
    public IAigcPage BaseDefinition => _pageNode;

    /// <summary>
    /// Gets the skill definition associated with this page, if any.
    /// </summary>
    /// <returns>The skill definition, or null if no skill is set.</returns>
    public IAigcSkill GetSkill() => _skill.Target?.GetSkillDefinition();

    /// <summary>
    /// Gets the tool asset associated with this page.
    /// </summary>
    /// <returns>The tool asset, falling back to the target asset if no skill is set.</returns>
    public IAigcToolAsset GetToolAsset()
    {
        if (_skill.Target is { } skillAsset)
        {
            return skillAsset;
        }

        return TargetAsset as IAigcToolAsset;
    }

    /// <summary>
    /// Gets all elements contained within this page.
    /// </summary>
    public IEnumerable<IAigcPageElement> Elements => _allElements;

    #endregion

    #region Update

    /// <summary>
    /// Gets or sets the title of this page instance.
    /// </summary>
    public string Title
    {
        get => field;
        set
        {
            value ??= string.Empty;
            if (field != value)
            {
                field = value;
                TitleUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Builds the page structure from the underlying diagram definition.
    /// </summary>
    /// <returns>True if the page was successfully built; otherwise, false.</returns>
    public bool Build()
    {
        if (!IsInDiagram)
        {
            return false;
        }

        OnBuild();

        var page = _pageDefinition;

        var doc = page.GetDocument();
        if (doc is null)
        {
            return false;
        }

        var items = doc.ItemCollection.AllItems
            .OfType<FlowDiagramItem>()
            .Where(o => o.Node is not IAigcPage)
            .Where(o => o.Bound.IntersectsWith(page.Bound))
            .ToList();

        items.Sort(FlowDiagramItemSort);

        Dictionary<FlowDiagramItem, AigcPageElement> dic = [];
        dic.AddRange(_dic.Values, o => o.DiagramItem);

        Clear();

        List<AigcPageElement> pageList = null;


        // Build group structure first
        var groups = items.Where(o => o.Node is GroupFlowNode).ToArray();
        if (groups.Length > 0)
        {
            CollectGroups(groups);
        }

        // Build main result page
        if (page.Node?.GetPageResult() is FlowNode { DiagramItem: FlowDiagramItem mainResultPage })
        {
            CollectResultPages([mainResultPage], ref pageList);
        }

        // Build sub pages
        var subPages = doc.ItemCollection.AllItems
            .OfType<PageSubDiagramItem>()
            .Where(o => o.Node?.GetTargetPageItem() == page)
            .ToList();

        subPages.Sort(FlowDiagramItemSort);

        if (subPages.Count > 0)
        {
            CollectSubPages(subPages, ref pageList);
        }

        // Build result pages of sub pages
        var subResultPages = subPages.Select(o => (o.Node?.GetPageResult() as FlowNode)?.DiagramItem)
            .OfType<FlowDiagramItem>()
            .ToArray();
        if (subResultPages.Length > 0)
        {
            CollectResultPages(subResultPages, ref pageList);
        }


        // Add all non-group nodes
        foreach (var item in items.Where(o => o.Node is not IGroupFlowNode))
        {
            var element = CreateElement(dic, item, out bool reused);
            if (element != null)
            {
                dic[element.DiagramItem] = element;
                element.InternalBuild();
                AddElement(element);
            }
        }

        // Sub pages
        if (pageList != null)
        {
            items = doc.ItemCollection.AllItems
            .OfType<FlowDiagramItem>()
            .Where(o => o.Node is not IAigcPage)
            .Where(o => o.Node is not IGroupFlowNode)
            .Where(o => pageList.Any(page => page.Bound.IntersectsWith(o.Bound)))
            .ToList();

            foreach (var item in items)
            {
                var element = CreateElement(dic, item, out bool reused);
                if (element != null)
                {
                    dic[element.DiagramItem] = element;
                    element.InternalBuild();
                    AddElement(element);
                }
            }
        }

        Sort();

        return true;
    }

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        ParameterCondition = _pageDefinition?.Node?.CompletionCondition ?? ParameterConditions.All;
        UseParentArticle = GetSkill()?.UseParentArticle ?? (_pageDefinition.Node?.UseParentArticle == true);

        //Name
        if (GetSkill()?.SkillName is { } skillName && !string.IsNullOrWhiteSpace(skillName))
        {
            _skillName = skillName;
        }

        //Tooltips
        if (GetSkill()?.SkillTooltips is { } skillTooltips && !string.IsNullOrWhiteSpace(skillTooltips))
        {
            Tooltips = skillTooltips;
        }
        else
        {
            Tooltips = _pageNode.GetAttribute<ToolTipsAttribute>()?.ToolTips;
        }
    }

    private void CollectGroups(IEnumerable<FlowDiagramItem> groups)
    {
        RectNode<FlowDiagramItem> groupRootNode = RectTreeBuilder.BuildTree(groups, o => o.Bound);
        foreach (var subPageNode in groupRootNode.Children)
        {
            var page = new GroupPageElement(subPageNode.Data, 1) { Parent = this, Option = this.Option };

            page.InternalBuild();

            _list.Add(page);
            _groups.Add(page);
            page.AddChildNodes(subPageNode);
        }
    }

    private void CollectSubPages(IEnumerable<FlowDiagramItem> subPages, ref List<AigcPageElement> pages)
    {
        int order = -10;

        RectNode<FlowDiagramItem> groupRootNode = RectTreeBuilder.BuildTree(subPages, o => o.Bound);
        
        // Sorting is required here because Order will be set
        groupRootNode.Children.Sort(RectNode<FlowDiagramItem>.RectNodeSort);

        foreach (var subPageNode in groupRootNode.Children)
        {
            var page = new SubPageElement(subPageNode.Data, 1, order)
            {
                Parent = this,
                Option = this.Option
            };

            page.InternalBuild();

            _list.Add(page);
            _groups.Add(page);
            page.AddChildNodes(subPageNode);

            (pages ??= []).Add(page);

            order -= 10;
        }
    }

    private void CollectResultPages(IEnumerable<FlowDiagramItem> subPages, ref List<AigcPageElement> pages)
    {
        RectNode<FlowDiagramItem> groupRootNode = RectTreeBuilder.BuildTree(subPages, o => o.Bound);
        foreach (var subPageNode in groupRootNode.Children)
        {
            var diagramItem = subPageNode.Data;
            var flowNode = diagramItem.Node;

            var page = new ResultPageElement(diagramItem, 1)
            {
                Parent = this,
                Option = this.Option
            };

            page.InternalBuild();

            _list.Add(page);
            _groups.Add(page);
            page.AddChildNodes(subPageNode);

            (pages ??= []).Add(page);

            if (pages.FirstOrDefault(o => (o.DiagramItem.Node as IAigcPage)?.GetPageResult() == flowNode) is { } defPage)
            {
                defPage.ResultPage = page;
                page.Order = defPage.Order - 5;
            }
            else if ((this.DiagramItem.Node as IAigcPage)?.GetPageResult() == flowNode)
            {
                // RootPage result page
                this.ResultPage = page;
                page.Order = this.Order - 5;
            }
        }
    }

    private void Clear()
    {
        _list.Clear();
        _groups.Clear();
        _dic.Clear();
        _allElements.Clear();
        _currentEndElement = null;
    }

    private bool AddElement(AigcPageElement element)
    {
        if (element is null)
        {
            return false;
        }

        // Check for duplicate addition
        if (!_allElements.Add(element))
        {
            return false;
        }

        bool addedToGroup = false;
        foreach (var group in _groups)
        {
            if (group.Bound.IntersectsWith(element.Bound))
            {
                group.AddElement(element);
                addedToGroup = true;
            }
        }

        if (!addedToGroup)
        {
            _list.Add(element);
            element.Parent = this;
            element.Option = this.Option;

            if (element is GroupPageElement groupElement)
            {
                _groups.Add(groupElement);
            }
        }

        if (element.Name is { } name && !string.IsNullOrWhiteSpace(name))
        {
            _dic[name] = element;
        }
        else
        {
            throw new ArgumentException("Element name is null or empty");
        }

        return true;
    }

    /// <inheritdoc/>
    public override void Sort()
    {
        _list.Sort(PageElementSort);

        foreach (var item in _list)
        {
            item.Sort();
        }
    }



    /// <inheritdoc/>
    public override void UpdateFromOther(IAigcPageElement other)
    {
        if (other is AigcPageInstance otherRoot)
        {
            UpdateFromOther(otherRoot);
        }
    }

    /// <summary>
    /// Updates this page instance's data from another page instance.
    /// </summary>
    /// <param name="otherRoot">The source page instance to update from.</param>
    public void UpdateFromOther(AigcPageInstance otherRoot)
    {
        _skill.TargetAsset = otherRoot._skill.TargetAsset;

        foreach (var other in otherRoot._dic.Values)
        {
            if (_dic.TryGetValue(other.DiagramItem.Name, out var exist))
            {
                exist.UpdateFromOther(other);
            }
        }
    }

    /// <summary>
    /// Creates a clone of this page instance with the specified options.
    /// </summary>
    /// <param name="option">The options to apply to the cloned instance.</param>
    /// <returns>A new <see cref="AigcPageInstance"/> that is a copy of this instance.</returns>
    public AigcPageInstance Clone(PageElementOption option)
    {
        var clone = new AigcPageInstance(_pageDefinition, option);
        clone.UpdateFromOther(this);

        return clone;
    }

    /// <summary>
    /// Sets the value of a named parameter on this page.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value to set.</param>
    public void SetParameter(string name, object value)
    {
        if (_dic.TryGetValue(name, out var element) && element is IPageParameter parameter)
        {
            parameter.SetValue(value);

            ParameterSet?.Invoke(this, parameter);
        }
    }

    /// <summary>
    /// Gets all input parameters defined on this page.
    /// </summary>
    /// <returns>An enumerable of page parameter inputs.</returns>
    public IEnumerable<IPageParameterInput> GetInputParameters()
    {
        return GetAllChildElements().OfType<IPageParameterInput>();
    }

    #endregion

    #region Data Sync, View & Connector
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _skill.Sync(sync);

        foreach (var element in _list)
        {
            element.Sync(sync, context);
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        foreach (var element in _list)
        {
            element.SetupView(setup);
        }
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        foreach (var element in _list)
        {
            element.UpdateConnector(node);
        }
    }
    #endregion

    #region Get

    /// <inheritdoc/>
    public override IEnumerable<AigcPageElement> ChildElements => _list;

    /// <inheritdoc/>
    public override IEnumerable<AigcPageElement> GetAllChildElements(bool sorted = true)
    {
        if (sorted)
        {
            return base.GetAllChildElements(true);
        }
        else
        {
            return _dic.Values;
        }
    }

    /// <summary>
    /// Gets a page element by its name.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <returns>The element if found; otherwise, null.</returns>
    public AigcPageElement GetElement(string name) => _dic.GetValueSafe(name);

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (!GetIsDoneInputs(ParameterCondition).IsTrueOrEmpty())
        {
            return false;
        }

        return ResultPage?.GetIsDone();
    }

    /// <summary>
    /// Gets a value indicating whether all input parameters are done.
    /// </summary>
    /// <returns>True if all inputs are done, false if any is not done, or null if no inputs are defined.</returns>
    public bool? GetIsDoneInputs() => GetIsDoneInputs(ParameterCondition);

    /// <summary>
    /// Gets a value indicating whether all input parameters are done based on the specified condition.
    /// </summary>
    /// <returns>True if all outputs are done, false if any is not done, or null if no outputs are defined.</returns>
    public bool? GetIsDoneOutputs() => GetIsDoneOutputs(ParameterCondition);

    /// <summary>
    /// Gets a value indicating whether all pages (including sub-pages) are done.
    /// </summary>
    /// <returns>True if all pages are done, false if any is not done, or null if no inputs are defined.</returns>
    public bool? GetAllDone()
    {
        if (GetIsDone().IsFalse())
        {
            return false;
        }

        var pages = _groups.OfType<SubPageElement>().OfType<AigcPageElement>().ConcatOne(this);

        bool? v = null;
        foreach (var page in pages)
        {
            if (page.GetIsDone() is { } doneV)
            {
                if (doneV)
                {
                    v ??= true;
                }
                else
                {
                    v = false;
                    break;
                }
            }
        }

        return v;
    }

    /// <summary>
    /// Gets the overall status of all pages as a <see cref="TextStatus"/>.
    /// </summary>
    /// <returns>Checked if all done, Unchecked if any not done, or Normal if undetermined.</returns>
    public TextStatus GetAllStatus()
    {
        bool? done = GetAllDone();
        if (done is { } doneV)
        {
            return doneV ? TextStatus.Checked : TextStatus.Unchecked;
        }
        else
        {
            return TextStatus.Normal;
        }
    }

    /// <summary>
    /// Gets the status icon representing the completion state of all pages.
    /// </summary>
    /// <returns>A check icon if all done, uncheck icon if any not done, or null if undetermined.</returns>
    public ImageDef GetAllStatusIcon()
    {
        bool? done = GetAllDone();
        if (done is { } doneV)
        {
            return doneV ? CoreIconCache.Check : CoreIconCache.Uncheck;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the current end element that was last executed in the flow.
    /// </summary>
    public PageEndElement CurrentEndElement => _currentEndElement;

    /// <summary>
    /// Converts this page instance to a <see cref="SimpleType"/> representation.
    /// </summary>
    /// <returns>A <see cref="SimpleType"/> describing the page's input parameters.</returns>
    public SimpleType ToSimpleType()
    {
        List<SimpleField> fields = [];

        if (_dic.Values.OfType<PageBeginElement>().FirstOrDefault() is { } begin)
        {
            if (begin.ParameterType is { } fieldType && !TypeDefinition.IsNullOrEmpty(fieldType))
            {
                if (fieldType == NativeTypes.TextBlockType)
                {
                    fieldType = NativeTypes.StringType;
                }

                var attr = begin.Node as IAttributeGetter;
                var range = attr?.GetAttribute<NumericRangeAttribute>();
                var selection = attr?.GetAttribute<SelectionDesignAttribute>();
                var tooltips = attr?.GetAttribute<ToolTipsAttribute>();

                var field = new SimpleField
                {
                    Name = begin.Name,
                    Tooltips = tooltips?.ToolTips,
                    Type = fieldType,
                    Range = range,
                    Selection = selection,
                };

                fields.Add(field);
            }
        }

        foreach (var parameter in _dic.Values.OfType<IPageParameterInput>())
        {
            if (parameter.IsSkillInput)
            {
                continue;
            }

            var fieldType = parameter.ParameterType;
            if (fieldType == NativeTypes.TextBlockType)
            {
                fieldType = NativeTypes.StringType;
            }

            var node = (parameter as AigcPageElement)?.DiagramItem?.Node as DesignFlowNode;
            var range = node?.GetAttribute<NumericRangeAttribute>();
            var selection = node?.GetAttribute<SelectionDesignAttribute>();
            var tooltips = node?.GetAttribute<ToolTipsAttribute>();

            var field = new SimpleField
            {
                Name = parameter.Name,
                Tooltips = tooltips?.ToolTips,
                Type = fieldType,
                Range = range,
                Selection = selection,
            };

            fields.Add(field);
        }

        var name = this.Name;
        var typeToolTips = this.Tooltips;

        var type = new SimpleType
        {
            Name = name,
            Tooltips = typeToolTips,
            Fields = [.. fields],
        };

        return type;
    }

    /// <summary>
    /// Converts this page instance to an <see cref="IDataWritable"/> representation.
    /// </summary>
    /// <returns>An <see cref="IDataWritable"/> for serializing the page's data.</returns>
    public IDataWritable ToDataWritable()
    {
        var type = ToSimpleType();

        return type.ToDataWritable();
    }

    /// <summary>
    /// Gets the input chat history formatted as a <see cref="ChatHistoryText"/>.
    /// </summary>
    /// <returns>The formatted input chat history.</returns>
    public ChatHistoryText GetInputChatHistory()
    {
        var builder = new StringBuilder();

        var inputs = GetAllChildElements(true).OfType<IPageParameterInput>()
            .Where(o => o.ChatHistory && o.GetCanOutputHistory(FlowDirections.Input))
            .ToArray();

        foreach (var element in inputs)
        {
            if (element is IPageMessage)
            {
                try
                {
                    var text = element.ResolveChatHistory();
                    builder.AppendLine(text.Text);
                }
                catch (Exception)
                {
                    builder.AppendLine("---");
                }
            }
            else
            {
                string attr = ResolveElementXmlAttr(element as AigcPageElement);
                builder.AppendLine($"<{element.Name}{attr}>");

                try
                {
                    var text = element.ResolveChatHistory();
                    builder.AppendLine(text.Text);
                }
                catch (Exception)
                {
                    builder.AppendLine("---");
                }
                builder.AppendLine($"</{element.Name}>");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the output chat history formatted as a <see cref="ChatHistoryText"/>.
    /// </summary>
    /// <returns>The formatted output chat history.</returns>
    public ChatHistoryText GetOutputChatHistory()
    {
        var builder = new StringBuilder();

        var outputs = GetAllChildElements(true).OfType<IPageParameterOutput>()
            .Where(o => o.ChatHistory && o.GetCanOutputHistory(FlowDirections.Output))
            .ToArray();

        builder.Length = 0;
        foreach (var element in outputs)
        {
            if (element is IPageMessage)
            {
                try
                {
                    var text = element.ResolveChatHistory();
                    builder.AppendLine(text.Text);
                }
                catch (Exception)
                {
                    builder.AppendLine("---");
                }
            }
            else
            {
                string attr = ResolveElementXmlAttr(element as AigcPageElement);
                builder.AppendLine($"<{element.Name}{attr}>");

                try
                {
                    var text = element.ResolveChatHistory();
                    builder.AppendLine(text.Text);
                }
                catch (Exception)
                {
                    builder.AppendLine("---");
                }
                builder.AppendLine($"</{element.Name}>");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the task commit data formatted as a <see cref="ChatHistoryText"/>.
    /// </summary>
    /// <returns>The formatted task commit data.</returns>
    public ChatHistoryText GetTaskCommit()
    {
        var builder = new StringBuilder();

        var outputs = GetAllChildElements(true).OfType<IPageParameter>()
            .Where(o => o.TaskCommit)
            .ToArray();

        builder.Length = 0;
        foreach (var element in outputs)
        {
            string attr = ResolveElementXmlAttr(element as AigcPageElement);
            builder.AppendLine($"<{element.Name}{attr}>");

            try
            {
                var text = element.ResolveChatHistory();
                builder.AppendLine(text.Text);
            }
            catch (Exception)
            {
                builder.AppendLine("---");
            }
            builder.AppendLine($"</{element.Name}>");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private string ResolveElementXmlAttr(AigcPageElement element)
    {
        if (element is null)
        {
            return string.Empty;
        }

        var attr = (element as AigcPageElement)?.Node as IAttributeGetter;
        var tooltips = attr?.GetAttribute<ToolTipsAttribute>();
        string desc = string.Empty;
        if (tooltips != null)
        {
            desc = $" description='{tooltips.ToolTips}'";
        }

        string toolName = (element as IPageParameterTool)?.ToolName;
        if (!string.IsNullOrWhiteSpace(toolName))
        {
            desc += $" tool='{toolName}'";
        }

        return desc;
    }


    /// <summary>
    /// Gets the list of tool assets available to this page.
    /// </summary>
    /// <returns>An array of tool assets from both the page definition and associated skill.</returns>
    public IAigcToolAsset[] GetToolList()
    {
        var tools = _pageNode.Tools.SkipNull();

        if (GetSkill()?.Tools?.ToArray() is { } exTools && exTools.Length > 0)
        {
            tools = tools.Concat(exTools.SkipNull());
        }

        return [.. tools];
    }

    #endregion

    #region Compute

    /// <summary>
    /// Tries to get the value of an outer parameter by name.
    /// </summary>
    /// <param name="outerCompute">The outer computation context.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">When this method returns, contains the parameter value if found; otherwise, null.</param>
    /// <returns>True if the parameter was found; otherwise, false.</returns>
    public bool TryGetOuterParameter(IFlowComputation outerCompute, string name, out object value)
    {
        if (_dic.TryGetValue(name, out var element) && element is IPageParameterInput parameter)
        {
            value = parameter.GetOuterValue(outerCompute);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Sets the value of an outer parameter by name.
    /// </summary>
    /// <param name="outerCompute">The outer computation context.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value to set.</param>
    public void SetOuterParameter(IFlowComputation outerCompute, string name, object value)
    {
        if (_dic.TryGetValue(name, out var element) && element is IPageParameterOutput output)
        {
            output.SetOuterValue(outerCompute, value);
        }
    }

    #endregion

    #region IFlowCallerContext

    /// <inheritdoc/>
    public void OnBeginFlow(IFlowComputation computation, string name)
    {
        if (!_dic.TryGetValue(name, out var element))
        {
            return;
        }

        if (element is not PageBeginElement beginElement)
        {
            return;
        }

        if (beginElement.Parent is not { } parent)
        {
            return;
        }

        if (computation.Context.GetArgument<IConversationHandler>() is { } conversation)
        {
            conversation.AddSystemMessage("Workflow started", msg => 
            {
                msg.AddCode(beginElement.DiagramItem.DisplayText ?? name);
            });
        }

        var inputs = parent.GetAllChildElements(true)
            .OfType<IPageParameterInput>()
            .ToArray();

        foreach (var input in inputs)
        {
            input.IsValueSet = false; // Clear set flag
        }

        if (beginElement.Parent?.ResultPage is { } resultPage)
        {
            var outputs = resultPage.GetAllChildElements(true)
                .OfType<IPageParameterOutput>()
                .ToArray();

            foreach (var output in outputs)
            {
                output.IsValueSet = false; // Clear set flag
            }
        }
    }

    /// <inheritdoc/>
    public string[] GetDatasToCompute(IFlowComputation computation, string name)
    {
        if (!_dic.TryGetValue(name, out var element))
        {
            return null;
        }

        if (element is not PageEndElement endElement)
        {
            return null;
        }

        if (endElement.Parent is not { } parent)
        {
            return null;
        }

        return parent.GetAllChildElements(true)
            .OfType<IPageParameterOutput>()
            .Select(o => o.Name)
            .SkipNull()
            .ToArray();
    }

    /// <inheritdoc/>
    public void OnEndFlow(IFlowComputation computation, string name, object value)
    {
        if (!_dic.TryGetValue(name, out var element))
        {
            return;
        }

        if (element is not PageEndElement endElement)
        {
            return;
        }

        if (endElement.Parent is not { } parent)
        {
            return;
        }

        if (computation.Context.GetArgument<IConversationHandler>() is { } conversation)
        {
            conversation.AddSystemMessage("Workflow ended", msg => 
            {
                msg.AddCode(endElement.DiagramItem.DisplayText ?? name);
            });
        }

        endElement.SetValue(value);

        var outputs = parent.GetAllChildElements(true)
            .OfType<IPageParameterOutput>()
            .Where(o => !o.IsValueSet) // Exclude variables already set in the flow
            .ToArray();

        foreach (var output in outputs)
        {
            if ((output as AigcPageElement)?.DiagramItem?.Node is not { } node)
            {
                continue;
            }

            if (!output.IsValueSet)
            {
                var v = computation.GetResult(node, true);
                // For values not forcibly updated by the flow, perform a passive update
                output.SetValue(v);
            }
        }

        _currentEndElement = endElement;
        ResultOutput?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public bool TryGetParameter(IFlowComputation computation, string name, out object value)
    {
        if (_dic.TryGetValue(name, out var element) && element is IPageValueElement parameter)
        {
            value = parameter.EnsureValue();
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc/>
    public void SetParameter(IFlowComputation computation, string name, object value)
    {
        if (_dic.TryGetValue(name, out var element) && element is IPageParameter parameter)
        {
            parameter.SetValue(value);

            ParameterSet?.Invoke(this, parameter);
        }
    }

    /// <inheritdoc/>
    public async Task<object> CallFunction(IFlowComputation computation, string name, object value, CancellationToken cancel)
    {
        return null;
    }

    /// <summary>
    /// Gets the definition page, preferring the skill asset if available.
    /// </summary>
    /// <returns>The skill asset or the base asset as the definition page.</returns>
    public IAigcToolAsset GetDefinitionPage()
    {
        if (_skill.Target is { } skill)
        {
            return skill;
        }

        return _asset;
    }


    #endregion

    #region Handle

    /// <summary>
    /// Executes an undo/redo action, either by raising the request event or executing directly.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void DoAction(UndoRedoAction action)
    {
        if (DoActionRequesting != null)
        {
            DoActionRequesting(this, action);
        }
        else
        {
            action.Do();
        }
    }


    /// <summary>
    /// Handles the start of a chat flow from a page begin element.
    /// </summary>
    /// <param name="begin">The page begin element that initiated the chat.</param>
    /// <param name="view">Optional flow view for UI integration.</param>
    /// <returns>A task representing the asynchronous chat operation.</returns>
    internal Task<object> HandleBeginChat(PageBeginElement begin, IFlowView view = null)
    {
        if (!IsInDiagram)
        {
            //Logs.LogError(L("This page has expired, please reload."));
            //return Task.FromResult<object>(null);
            throw new InvalidOperationException(L("This page has expired, please reload."));
        }

        //if (this.GetAllDone() == true)
        //{
        //    Logs.LogInfo(L("All flows on this page have completed."));
        //    return Task.FromResult<object>(null);
        //}

        if (begin.FindParentDefPage() is not { } defPage)
        {
            return Task.FromResult<object>(null);
        }

        if (defPage.GetIsDone() == true)
        {
            Logs.LogInfo(L("This flow has completed."));
            return Task.FromResult<object>(null);
        }

        if (begin.Node is not IAigcRunWorkflow runnable)
        {
            return Task.FromResult<object>(null);
        }

        return LLmService.Instance.StartWorkflowChat(runnable, view, OnConfigComputation);
    }

    /// <summary>
    /// Handles the start of a task flow from a page begin element.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="begin">The page begin element that initiated the task.</param>
    /// <param name="view">Optional flow view for UI integration.</param>
    /// <returns>A task representing the asynchronous task operation.</returns>
    public Task<object> HandleBeginTask(AIRequest request, PageBeginElement begin, IFlowView view = null)
    {
        if (!IsInDiagram)
        {
            //Logs.LogError(L("This page has expired, please reload."));
            //return Task.FromResult<object>(null);
            throw new InvalidOperationException(L("This page has expired, please reload."));
        }

        //if (this.GetAllDone() == true)
        //{
        //    Logs.LogInfo(L("All flows on this page have completed."));
        //    return Task.FromResult<object>(null);
        //}

        if (begin.FindParentDefPage() is not { } defPage)
        {
            return Task.FromResult<object>(null);
        }

        if (defPage.GetIsDone() == true)
        {
            Logs.LogInfo(L("This flow has completed."));
            return Task.FromResult<object>(null);
        }

        if (begin.Node is not IAigcRunWorkflow runnable)
        {
            return Task.FromResult<object>(null);
        }

        _currentEndElement = null;
        return LLmService.Instance.StartWorkflowTask(request, runnable, view, OnConfigComputation);
    }

    /// <summary>
    /// Configures the flow computation by setting up context arguments and raising the configuration event.
    /// </summary>
    /// <param name="computation">The flow computation to configure.</param>
    protected virtual void OnConfigComputation(IFlowComputation computation)
    {
        _conversation.RemoveMessages(o => true);

        var context = computation.Context;

        var view = context.GetArgument<IFlowView>();
        context.SetArgument<IFlowCallerContext>(this);
        context.SetArgument<IConversationHandler>(_conversation);
        context.SetArgument<IConversationHost>(_conversation as IConversationHost);
        context.SetArgument<IConversationHostAsync>(_conversation as IConversationHostAsync);

        this.LastComputation = computation;

        ConfigComputation?.Invoke(this, computation);

        if (Option.Owner is FlowNode node && view?.GetViewNode(node.Name) is { } viewNode)
        {
            viewNode.NodeComputation = computation;
        }
    }
    #endregion
}
