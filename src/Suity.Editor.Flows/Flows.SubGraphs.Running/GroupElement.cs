using Suity.Editor.AIGC.Flows.Pages;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Flows.SubGraphs.Running;

/// <summary>
/// Represents a group page element that can contain nested child elements and sub-groups within an AIGC flow diagram.
/// </summary>
public class GroupElement : SubGraphElement
{
    private readonly FlowDiagramItem _groupItem;
    private readonly List<SubGraphElement> _list = [];
    private readonly List<GroupElement> _groups = [];
    private readonly int _depth;

    private string _groupName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupElement"/> class.
    /// </summary>
    /// <param name="groupItem">The flow diagram item representing this group.</param>
    /// <param name="depth">The nesting depth of this group element.</param>
    /// <param name="order">The display order of this element. Defaults to 0.</param>
    public GroupElement(FlowDiagramItem groupItem, int depth, int order = 0)
        : base(groupItem)
    {
        _groupItem = groupItem ?? throw new ArgumentNullException(nameof(groupItem));
        _depth = depth;
        Order = order;
    }

    /// <summary>
    /// Gets the nesting depth of this group element.
    /// </summary>
    public int Depth => _depth;

    /// <summary>
    /// Gets or sets a value indicating whether to draw the group label in the view.
    /// </summary>
    public bool DrawLabel { get; set; } = true;

    /// <summary>
    /// Gets the name of this group.
    /// </summary>
    public string GroupName => _groupName;

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        _groupName = (_groupItem.Node as IGroupFlowNode)?.GroupName;
    }

    /// <summary>
    /// Adds a child element to this group, automatically nesting it into a sub-group if bounds intersect.
    /// </summary>
    /// <param name="element">The page element to add.</param>
    internal void AddElement(SubGraphElement element)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
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

            if (element is GroupElement groupElement)
            {
                _groups.Add(groupElement);
            }
        }
    }

    /// <summary>
    /// Recursively adds child nodes from the diagram tree as group elements.
    /// </summary>
    /// <param name="node">The rectangle node containing child diagram items.</param>
    internal void AddChildNodes(RectNode<FlowDiagramItem> node)
    {
        int newDepth = _depth + 1;

        foreach (var child in node.Children)
        {
            var groupPage = new GroupElement(child.Data, newDepth);
            _groups.Add(groupPage);
            _list.Add(groupPage);
            groupPage.AddChildNodes(child);
        }
    }


    /// <inheritdoc/>
    public override void Sort()
    {
        _list.Sort(SubGraphInstance.PageElementSort);

        foreach (var item in _list)
        {
            item.Sort();
        }
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        foreach (var item in _list)
        {
            item.Sync(sync, context);
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        if (DrawLabel && _depth == 1 && _groupName is { } groupName)
        {
            if (Icon is { } icon)
            {
                setup.LabelWithIcon(Name, groupName, icon);
            }
            else
            {
                setup.Label(Name, groupName);
            }
        }

        foreach (var item in _list)
        {
            item.SetupView(setup);
        }
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        foreach (var item in _list)
        {
            item.UpdateConnector(node);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<SubGraphElement> ChildElements => _list;
}
