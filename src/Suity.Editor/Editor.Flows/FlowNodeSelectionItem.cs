using Suity.Drawing;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Flowchart node selection item
/// </summary>
public class FlowNodeSelectionItem : TypedSelectionItem
{
    private readonly FlowNodeStyle _style;

    /// <summary>
    /// Build flowchart node selection item
    /// </summary>
    /// <param name="node">Flowchart node instance, this instance is only used to obtain style, does not participate in work.</param>
    public FlowNodeSelectionItem(Type type, Predicate<Type> condition = null)
        : base(type, condition)
    {
        _style = FlowNodeStyle.GetStyle(type);
    }

    public override object DisplayIcon => _style?.Icon ?? base.DisplayIcon;
}

public class FlowNodeSelectionItem<T> : FlowNodeSelectionItem
    where T : class
{
    public FlowNodeSelectionItem(Predicate<Type> condition = null) : base(typeof(T), condition)
    {
    }
}

public class FlowNodeSelectionNode : SelectionNode
{
    public FlowNodeSelectionNode(string key = null, string displayText = null, ImageDef icon = null)
        : base(key, displayText, icon)
    {
    }

    public void AddDerivedType(Type baseType, Predicate<Type> condition = null)
    {
        var derivedtypes = baseType.GetAvailableClassTypes();
        if (condition != null)
        {
            derivedtypes = derivedtypes.Where(o => condition(o));
        }

        var categoryGroups = derivedtypes.GroupBy(t => t.GetAttributeCached<SimpleFlowNodeStyleAttribute>()?.Category)
            .OrderBy(g => g.Key);

        foreach (var group in categoryGroups.Where(g => g.Key != null))
        {
            string category = $"[{group.Key}]";

            var selGroup = new CategorySelectionGroup(category, group.Select(o => new FlowNodeSelectionItem(o)));
            Add(selGroup);
        }

        var types = categoryGroups.FirstOrDefault(g => g.Key is null)?.ToList() ?? [];
        types.Sort((a, b) => 
        {
            int c = DisplayOrderAttribute.Compare(a, b);
            if (c != 0)
            {
                return c;
            }

            return a.Name.CompareTo(b.Name);
        });

        foreach (Type type in types)
        {
            if (type.HasAttributeCached<NotAvailableAttribute>())
            {
                continue;
            }

            Add(new FlowNodeSelectionItem(type, condition));
        }
    }

    public void Add<T>(Predicate<ISelectionItem> condition = null) where T : FlowNode
    {
        if (typeof(T).HasAttributeCached<NotAvailableAttribute>())
        {
            return;
        }

        base.Add(new FlowNodeSelectionItem<T>(), condition);
    }

    public void AddDerived<T>()
        // where T : FlowNode
    {
        Add(new FlowNodeSelectionNode<T>());
    }

    public void Add(Type type, Predicate<ISelectionItem> condition = null)
    {
        if (type is null)
        {
            return;
        }

        if (type.HasAttributeCached<NotAvailableAttribute>())
        {
            return;
        }

        if (!typeof(FlowNode).IsAssignableFrom(type))
        {
            // throw new ArgumentException($"Type {type} is not a FlowNode.");
            return;
        }

        base.Add(new FlowNodeSelectionItem(type), condition);
    }
}

public class FlowNodeSelectionNode<T> : FlowNodeSelectionNode, IPreviewDisplay
    //where T : FlowNode
{
    public FlowNodeSelectionNode(string key = null, string displayText = null, ImageDef icon = null, Predicate<Type> condition = null)
         : base(key, displayText, icon)
    {
        AddDerivedType(typeof(T),  t => 
        {
            if (!typeof(FlowNode).IsAssignableFrom(t))
            {
                return false;
            }

            if (condition != null)
            {
                return condition(t);
            }

            return true;
        });
    }

    public override string SelectionKey => base.SelectionKey ?? typeof(T).FullName;
    public override string DisplayText => base.DisplayText ?? typeof(T).ToDisplayText();
    public override object DisplayIcon => base.DisplayIcon ?? typeof(T).ToDisplayIcon();

    public string PreviewText => typeof(T).ToPreviewText() ?? typeof(T).ToToolTipsText() ?? string.Empty;

    public object PreviewIcon => null;
}