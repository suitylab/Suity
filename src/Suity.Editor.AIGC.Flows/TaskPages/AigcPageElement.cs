using Suity.Drawing;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.AIGC.TaskPages;

/// <summary>
/// Represents an abstract base class for AIGC page elements that integrate with the flow diagram system.
/// </summary>
public abstract class AigcPageElement : IViewObject, IAigcPageElement
{
    string _elementName;
    string _displayText;
    ImageDef _elementIcon;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcPageElement"/> class.
    /// </summary>
    /// <param name="diagramItem">The flow diagram item associated with this element.</param>
    protected AigcPageElement(FlowDiagramItem diagramItem)
    {
        DiagramItem = diagramItem ?? throw new ArgumentNullException(nameof(diagramItem));
    }

    #region Core Props

    /// <summary>
    /// Gets or sets the configuration options for this page element.
    /// </summary>
    public PageElementOption Option { get; internal set; }

    /// <summary>
    /// Gets the name of this element.
    /// </summary>
    public virtual string Name => _elementName;

    /// <summary>
    /// Gets the flow diagram item associated with this element.
    /// </summary>
    public FlowDiagramItem DiagramItem { get; }

    /// <summary>
    /// Gets the target asset associated with this element.
    /// </summary>
    public Asset TargetAsset { get; private set; }

    /// <summary>
    /// Gets the flow node associated with this element.
    /// </summary>
    public FlowNode Node { get; private set; }

    /// <summary>
    /// Gets the bounding rectangle of this element within the diagram.
    /// </summary>
    public Rectangle Bound => DiagramItem.Bound;

    /// <summary>
    /// Gets or sets the display order of this element.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the result page element associated with this element.
    /// </summary>
    public AigcPageElement ResultPage { get; set; }

    /// <summary>
    /// Gets a value indicating whether this element is currently present in a diagram.
    /// </summary>
    public bool IsInDiagram => DiagramItem?.Diagram != null;

    /// <summary>
    /// Gets the display text shown for this element.
    /// </summary>
    public virtual string DisplayText => _displayText;

    /// <summary>
    /// Gets the icon image shown for this element.
    /// </summary>
    public virtual ImageDef Icon => _elementIcon;

    #endregion

    #region Parenting

    /// <summary>
    /// Gets or sets the parent page element of this element.
    /// </summary>
    public AigcPageElement Parent { get; internal set; }

    /// <summary>
    /// Gets the root page instance that contains this element.
    /// </summary>
    public AigcPageInstance Root
    {
        get
        {
            var parent = Parent;
            while (parent != null)
            {
                if (parent is AigcPageInstance root)
                {
                    return root;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the direct child elements of this element.
    /// </summary>
    public virtual IEnumerable<AigcPageElement> ChildElements => [];

    /// <summary>
    /// Gets all descendant child elements recursively.
    /// </summary>
    /// <param name="sorted">Whether to return elements in sorted order.</param>
    public virtual IEnumerable<AigcPageElement> GetAllChildElements(bool sorted = true)
    {
        foreach (var item in ChildElements)
        {
            yield return item;

            foreach (var child in item.GetAllChildElements(sorted))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Sorts the child elements of this element.
    /// </summary>
    public virtual void Sort() { }

    /// <summary>
    /// Finds the nearest parent element that has a result page defined.
    /// </summary>
    /// <returns>The parent element with a result page, or null if none is found.</returns>
    public AigcPageElement FindParentDefPage()
    {
        var page = this;
        while (page != null)
        {
            if (page.ResultPage != null)
            {
                return page;
            }

            page = page.Parent;
        }

        return null;
    }

    #endregion

    #region Sync View Connector

    /// <summary>
    /// Gets the outer connector for synchronizing with the view.
    /// </summary>
    public virtual FlowNodeConnector OuterConnector => null;


    /// <inheritdoc/>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <inheritdoc/>
    public virtual void SetupView(IViewObjectSetup setup)
    {
    }

    /// <summary>
    /// Updates the connector for the specified function node.
    /// </summary>
    /// <param name="node">The function node to update the connector for.</param>
    public virtual void UpdateConnector(PageFunctionNode node)
    {
    }
    #endregion


    /// <summary>
    /// Performs internal building of this element by invoking <see cref="OnBuild"/>.
    /// </summary>
    internal void InternalBuild() => OnBuild();

    /// <summary>
    /// Called during the build process to initialize element properties from the diagram item.
    /// </summary>
    protected virtual void OnBuild()
    {
        TargetAsset = DiagramItem.TargetAsset;
        Node = DiagramItem.Node;

        _elementName = DiagramItem.Name;
        if (string.IsNullOrWhiteSpace(_elementName))
        {
            throw new InvalidOperationException("Element name is empty.");
        }

        _displayText = DiagramItem.Node?.DisplayText;
        _elementIcon = DiagramItem.Node?.Icon;
    }

    /// <summary>
    /// Updates this element's state from another page element.
    /// </summary>
    /// <param name="other">The other page element to update from.</param>
    public virtual void UpdateFromOther(IAigcPageElement other)
    {
    }

    #region IAigcPageElement

    /// <summary>
    /// Gets whether this element is done/completed.
    /// </summary>
    /// <returns>True if done, false if not done, or null if indeterminate.</returns>
    public virtual bool? GetIsDone() => false;

    /// <summary>
    /// Gets the text status representing the completion state of this element.
    /// </summary>
    public TextStatus GetStatus()
    {
        bool? done = GetIsDone();
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
    /// Gets the status icon representing the completion state of this element.
    /// </summary>
    public ImageDef GetStatusIcon()
    {
        bool? done = GetIsDone();
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
    /// Gets the completion status of all input parameter child elements.
    /// </summary>
    /// <param name="condition">The condition to apply when evaluating multiple inputs.</param>
    /// <returns>True if inputs are done, false if not, or null if indeterminate.</returns>
    protected bool? GetIsDoneInputs(ParameterConditions condition)
    {
        var inputs = GetAllChildElements(false)
            .Where(o => o is IPageParameterInput)
            .ToArray();

        if (!inputs.Any())
        {
            return null;
        }

        if (inputs.All(o => o.GetIsDone() is null))
        {
            return null;
        }

        switch (condition)
        {
            case ParameterConditions.Any:
                {
                    var v = inputs.Any(o => o.GetIsDone().IsTrueOrEmpty());
                    return v;
                }

            case ParameterConditions.All:
            default:
                {
                    var v = inputs.All(o => o.GetIsDone().IsTrueOrEmpty());
                    return v;
                }
        }
    }

    /// <summary>
    /// Gets the completion status of all output parameter child elements.
    /// </summary>
    /// <param name="condition">The condition to apply when evaluating multiple outputs.</param>
    /// <returns>True if outputs are done, false if not, or null if indeterminate.</returns>
    protected bool? GetIsDoneOutputs(ParameterConditions condition)
    {
        var outputs = GetAllChildElements(false)
            .Where(o => o is IPageParameterOutput)
            .ToArray();

        if (!outputs.Any())
        {
            return null;
        }

        if (outputs.All(o => o.GetIsDone() is null))
        {
            return null;
        }

        switch (condition)
        {
            case ParameterConditions.Any:
                {
                    // In Any mode, empty results need to be excluded, so use IsTrue()
                    var v = outputs.Any(o => o.GetIsDone().IsTrue());
                    return v;
                }

            case ParameterConditions.All:
            default:
                {
                    var v = outputs.All(o => o.GetIsDone().IsTrueOrEmpty());
                    return v;
                }
        }
    }

    /// <summary>
    /// Gets whether this element can output history in the specified direction.
    /// </summary>
    /// <param name="diraction">The flow direction to check.</param>
    public virtual bool GetCanOutputHistory(FlowDirections diraction)
    {
        var isDone = GetIsDone();

        return !isDone.HasValue || isDone.Value == true;
    }
    #endregion

    /// <summary>
    /// Finds the nearest parent element matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match parent elements against.</param>
    /// <returns>The matching parent element, or null if none is found.</returns>
    protected AigcPageElement FindParent(Predicate<AigcPageElement> predicate)
    {
        var element = this;
        while (element != null)
        {
            if (predicate(element))
            {
                return element;
            }

            element = element.Parent;
        }

        return null;
    }



    /// <inheritdoc/>
    public override string ToString() => this.Name;



    /// <summary>
    /// Creates a page element for the specified diagram item, reusing an existing element if available.
    /// </summary>
    /// <param name="dic">The dictionary of existing diagram item to element mappings.</param>
    /// <param name="item">The diagram item to create an element for.</param>
    /// <param name="reused">When this method returns, contains whether an existing element was reused.</param>
    /// <returns>The created or reused page element, or null if creation is not possible.</returns>
    public static AigcPageElement CreateElement(Dictionary<FlowDiagramItem, AigcPageElement> dic, FlowDiagramItem item, out bool reused)
    {
        if (dic.TryGetValue(item, out var element))
        {
            reused = true;
            return element;
        }
        else
        {
            reused = false;

            if (item is IPageElementCreator creator)
            {
                return creator.CreatePageElement();
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Compares two diagram items for sorting by position (Y then X).
    /// </summary>
    /// <param name="a">The first diagram item.</param>
    /// <param name="b">The second diagram item.</param>
    /// <returns>A negative value if a comes before b, positive if after, zero if equal.</returns>
    public static int FlowDiagramItemSort(FlowDiagramItem a, FlowDiagramItem b)
    {
        int v = a.Y.CompareTo(b.Y);
        if (v != 0)
        {
            return v;
        }

        return a.X.CompareTo(b.X);
    }

    /// <summary>
    /// Compares two page elements for sorting by order, then position (Y then X).
    /// </summary>
    /// <param name="a">The first page element.</param>
    /// <param name="b">The second page element.</param>
    /// <returns>A negative value if a comes before b, positive if after, zero if equal.</returns>
    public static int PageElementSort(AigcPageElement a, AigcPageElement b)
    {
        if (a.Order != b.Order)
        {
            return -a.Order.CompareTo(b.Order);
        }

        var rectA = a.Bound;
        var rectB = b.Bound;

        int v = rectA.Y.CompareTo(rectB.Y);
        if (v != 0)
        {
            return v;
        }

        return rectA.X.CompareTo(rectB.X);
    }


    /// <summary>
    /// Converts a value to a chat history text representation using the type conversion service.
    /// </summary>
    /// <param name="type">The type definition of the value.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted chat history text, or the original value's string representation.</returns>
    public static ChatHistoryText ConvertChatHistoryText(TypeDefinition type, object value)
    {
        var historyText = TypeDefinition.FromNative<ChatHistoryText>();
        var state = EditorServices.TypeConvertService.TryConvert(type, historyText, false, value, out var converted);
        var result = state == TypeConvertState.Unconvertible ? value : converted;

        return result?.ToString();
    }


}
