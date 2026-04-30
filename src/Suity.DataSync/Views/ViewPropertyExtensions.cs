using Suity.NodeQuery;
using System.Drawing;

namespace Suity.Views;

public static class ViewPropertyExtensions
{
    public static ViewProperty WithViewId(this ViewProperty viewProperty, int viewId)
    {
        viewProperty.ViewId = viewId;
        return viewProperty;
    }

    public static ViewProperty WithDescription(this ViewProperty viewProperty, string description)
    {
        viewProperty.Description = description;
        return viewProperty;
    }

    public static ViewProperty WithIcon(this ViewProperty viewProperty, object icon)
    {
        viewProperty.Icon = icon;
        return viewProperty;
    }

    public static ViewProperty WithExpand(this ViewProperty viewProperty)
    {
        viewProperty.Expand = true;
        return viewProperty;
    }

    public static ViewProperty WithStatus(this ViewProperty viewProperty, TextStatus status)
    {
        viewProperty.Status = status;
        return viewProperty;
    }

    public static ViewProperty WithInspectorView(this ViewProperty viewProperty)
    {
        viewProperty.ViewId = ViewIds.Inspector;
        return viewProperty;
    }

    public static ViewProperty WithTreeView(this ViewProperty viewProperty)
    {
        viewProperty.ViewId = ViewIds.TreeView;
        return viewProperty;
    }

    public static ViewProperty WithMainTreeView(this ViewProperty viewProperty)
    {
        viewProperty.ViewId = ViewIds.MainTreeView;
        return viewProperty;
    }

    public static ViewProperty WithDetailTreeView(this ViewProperty viewProperty)
    {
        viewProperty.ViewId = ViewIds.DetailTreeView;
        return viewProperty;
    }

    public static ViewProperty WithReadOnly(this ViewProperty viewProperty)
    {
        viewProperty.ReadOnly = true;
        return viewProperty;
    }

    public static ViewProperty WithReadOnly(this ViewProperty viewProperty, bool isReadOnly)
    {
        viewProperty.ReadOnly = isReadOnly;
        return viewProperty;
    }

    public static ViewProperty WithEnabeld(this ViewProperty viewProperty, bool enabled)
    {
        viewProperty.Disabled = !enabled;
        return viewProperty;
    }

    public static ViewProperty WithDisabled(this ViewProperty viewProperty)
    {
        viewProperty.Disabled = true;
        return viewProperty;
    }

    public static ViewProperty WithDisabled(this ViewProperty viewProperty, bool disabled)
    {
        viewProperty.Disabled = disabled;
        return viewProperty;
    }

    public static ViewProperty WithOptional(this ViewProperty viewProperty)
    {
        viewProperty.Optional = true;
        return viewProperty;
    }

    public static ViewProperty WithOptional(this ViewProperty viewProperty, bool optional)
    {
        viewProperty.Optional = optional;
        return viewProperty;
    }

    public static ViewProperty WithConnector(this ViewProperty viewProperty)
    {
        viewProperty.IsConnector = true;
        return viewProperty;
    }

    public static ViewProperty WithHideTitle(this ViewProperty viewProperty)
    {
        viewProperty.HideTitle = true;
        return viewProperty;
    }

    public static ViewProperty WithHideTitle(this ViewProperty viewProperty, bool hideTitle)
    {
        viewProperty.HideTitle = hideTitle;
        return viewProperty;
    }

    public static ViewProperty WithWriteBack(this ViewProperty viewProperty)
    {
        viewProperty.WriteBack = true;
        return viewProperty;
    }

    public static ViewProperty WithNavigation(this ViewProperty viewProperty)
    {
        viewProperty.Navigation = true;
        return viewProperty;
    }

    public static ViewProperty WithColor(this ViewProperty viewProperty, Color? color)
    {
        viewProperty.Color = color;
        return viewProperty;
    }

    public static ViewProperty WithConfirm(this ViewProperty viewProperty, string message)
    {
        viewProperty.EnsureStyles().SetAttribute("Confirm", message);
        return viewProperty;
    }

    public static ViewProperty WithEmptyListGray(this ViewProperty viewProperty)
    {
        viewProperty.EnsureStyles().SetAttribute("EmptyListGray", true);
        return viewProperty;
    }

    public static ViewProperty WithHeaderStyle(this ViewProperty viewProperty, string style)
    {
        viewProperty.EnsureStyles().SetAttribute("HeaderStyle", style);
        return viewProperty;
    }

    public static ViewProperty WithEmboss(this ViewProperty viewProperty)
    {
        return WithHeaderStyle(viewProperty, "Emboss");
    }

    public static ViewProperty WithToolTips(this ViewProperty viewProperty, string toolTip)
    {
        viewProperty.EnsureStyles().SetAttribute("ToolTip", toolTip);
        return viewProperty;
    }

    public static ViewProperty WithUnit(this ViewProperty viewProperty, string unit)
    {
        viewProperty.EnsureStyles().SetAttribute("Unit", unit);
        return viewProperty;
    }

    public static ViewProperty WithHintText(this ViewProperty viewProperty, string hintText)
    {
        viewProperty.EnsureStyles().SetAttribute("Hint", hintText);
        return viewProperty;
    }

    public static ViewProperty WithInitialHidden(this ViewProperty viewProperty)
    {
        viewProperty.EnsureStyles().SetAttribute("InitialHidden", true);
        return viewProperty;
    }


    public static RawNode EnsureStyles(this ViewProperty viewProperty)
    {
        RawNode styles = viewProperty.Styles as RawNode;
        if (styles == null)
        {
            styles = new RawNode("Styles");
            viewProperty.Styles = styles;
        }

        return styles;
    }

    public static ViewProperty WithObsolete(this ViewProperty viewProperty)
    {
        viewProperty.EnsureStyles().SetAttribute("Obsolete", true);
        return viewProperty;
    }

    public static string GetConfirm(this INodeReader reader)
    {
        return reader.GetAttribute("Confirm");
    }

    public static bool GetEmptyListGray(this INodeReader reader)
    {
        return reader.GetBooleanAttribute("EmptyListGray");
    }

    public static string GetHeaderStyle(this INodeReader reader)
    {
        return reader.GetAttribute("HeaderStyle");
    }

    public static string GetToolTip(this INodeReader reader)
    {
        return reader.GetAttribute("ToolTip");
    }

    public static string GetUnit(this INodeReader reader)
    {
        return reader.GetAttribute("Unit");
    }

    public static string GetHintText(this INodeReader reader)
    {
        return reader.GetAttribute("Hint");
    }

    public static bool GetInitialHidden(this INodeReader reader)
    {
        return reader.GetBooleanAttribute("InitialHidden");
    }
}