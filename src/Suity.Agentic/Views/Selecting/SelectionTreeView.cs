using Suity.Selecting;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.TreeEditing;
using System;

namespace Suity.Editor.Views.Selecting;

public class SelectionTreeView : ImGuiTreeView<ISelectionItem>
{
    private readonly Column3Template<ISelectionItem> _column;

    private readonly ImGuiNodeRef _guiRef = new();

    public Action<ISelectionItem> DoubleClicked;

    public SelectionTreeView(IImGuiTreeModel<ISelectionItem> model) : base(model)
    {
        _column = new Column3Template<ISelectionItem>();
        this.ViewTemplate = _column;

        _column.RowPipeline = RowPipeline;
        _column.NameColumn.RowGui = NameColumnGui;
        _column.PreviewColumn.RowGui = PreviewColumnGui;

        _column.NameColumnWidth = 200;
        _column.PreviewColumnWidth = 600;
    }

    public override ImGuiNode OnGui(ImGui gui, string id, Action<ImGuiNode> config = null)
    {
        var node = _guiRef.Node = base.OnGui(gui, id, config);

        return node;
    }

    private void RowPipeline(ImGuiNode node, ISelectionItem item, EditorImGuiPipeline pipeline)
    {
        if (item is ITextDisplay display)
        {
            node.SetEnabled(display.DisplayStatus == TextStatus.Normal);
        }

        if (pipeline == EditorImGuiPipeline.Normal)
        {
            node.InitInputDoubleClicked(n =>
            {
                DoubleClicked?.Invoke(item);
            });
        }
    }

    private void NameColumnGui(ImGuiNode node, ISelectionItem item)
    {
        var gui = node.Gui;

        var icon = item.ToDisplayIcon();
        if (icon != null)
        {
            gui.Image("##icon", icon)
            .InitClass("icon");
        }

        string text = item.ToDisplayTextL();
        if (string.IsNullOrWhiteSpace(text))
        {
            text = item.SelectionKey;
        }

        gui.Text("##title_text", text)
        .InitVerticalAlignment(GuiAlignment.Center)
        .SetFontColor((item as IViewColor)?.ViewColor);
    }

    private void PreviewColumnGui(ImGuiNode node, ISelectionItem item)
    {
        var gui = node.Gui;

        string previewText = item.ToPreviewText() ?? item.ToToolTipsText() ?? item.SelectionKey;

        gui.Text("##preview_text", previewText)
        .InitVerticalAlignment(GuiAlignment.Center)
        .SetFontColor((item as IViewColor)?.ViewColor);
    }

    public new void QueueRefresh()
    {
        base.QueueRefresh();

        TreeData?.QueueRefresh();
        _guiRef.QueueRefresh();
    }
}
