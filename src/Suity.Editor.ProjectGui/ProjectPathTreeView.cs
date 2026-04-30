using Suity.Views.Im.TreeEditing;
using Suity.Views.PathTree;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// A specialized path tree view for the project explorer without a header row.
/// </summary>
internal class ProjectPathTreeView : HeaderlessPathTreeView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPathTreeView"/> class with a default model.
    /// </summary>
    public ProjectPathTreeView()
        : this(new ImGuiPathTreeModel())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPathTreeView"/> class with the specified model.
    /// </summary>
    /// <param name="model">The tree model providing path nodes.</param>
    public ProjectPathTreeView(IImGuiTreeModel<PathNode> model)
        : base(model)
    {
    }

    //protected override void ConfigRow(ImGuiNode node, VirtualTreeNode<PathNode> item)
    //{
    //    node.InitOverlayLayout()
    //    .InitFullWidth()
    //    .OnContent(() =>
    //    {
    //        var gui = node.Gui;

    //        gui.Text("aa", "aa");
    //        //var innerMain = node.Gui.BeginNode("#main");
    //        //base.ConfigRow(innerMain, item);
    //    });
    //}
}