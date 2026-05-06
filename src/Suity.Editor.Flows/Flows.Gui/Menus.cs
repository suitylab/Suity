using Suity.Helpers;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Root menu command for flow diagram context menus.
/// </summary>
internal class RootFlowMenuCommand : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootFlowMenuCommand"/> class.
    /// </summary>
    /// <param name="name">The menu name.</param>
    public RootFlowMenuCommand(string name)
        : base(name)
    {
        AddCommand(new CreateNodeMenuCommand());
        AddCommand(new SelectSameTypeCommand());
        AddCommand(new CloneNodeCommand());
        AddCommand(new DeleteNodeMenuCommand());
        AddSeparator();

        AddCommand(new CreateGroupMenuCommand());
        AddCommand(new CreateCommentMenuCommand());
        AddSeparator();

        AddCommand(new GotoDefinitionMenuCommand());
        AddCommand(new FindReferenceMenuCommand());
        AddSeparator();
    }
}

/// <summary>
/// Menu command to create a new node in the flow diagram.
/// </summary>
internal class CreateNodeMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNodeMenuCommand"/> class.
    /// </summary>
    public CreateNodeMenuCommand()
        : base("Create", CoreIconCache.New.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleCreateNode();
    }
}

/// <summary>
/// Menu command to clone selected nodes in the flow diagram.
/// </summary>
internal class CloneNodeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloneNodeCommand"/> class.
    /// </summary>
    public CloneNodeCommand()
        : base("Clone", CoreIconCache.Copy.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleCloneSelectedNodes();
    }
}

/// <summary>
/// Menu command to delete selected nodes from the flow diagram.
/// </summary>
internal class DeleteNodeMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNodeMenuCommand"/> class.
    /// </summary>
    public DeleteNodeMenuCommand()
        : base("Delete", CoreIconCache.Delete.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.GraphControl?.DeleteSelected();
    }
}

/// <summary>
/// Menu command to create a group node containing selected items.
/// </summary>
internal class CreateGroupMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGroupMenuCommand"/> class.
    /// </summary>
    public CreateGroupMenuCommand()
        : base("Create Group", CoreIconCache.Group.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleCreateGroup(true);
    }
}

/// <summary>
/// Menu command to create a comment node in the flow diagram.
/// </summary>
internal class CreateCommentMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentMenuCommand"/> class.
    /// </summary>
    public CreateCommentMenuCommand()
        : base("Create Comment", CoreIconCache.Comment.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleCreateComment(true);
    }
}

/// <summary>
/// Menu command to select all nodes of the same type as the currently selected node.
/// </summary>
internal class SelectSameTypeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectSameTypeCommand"/> class.
    /// </summary>
    public SelectSameTypeCommand()
        : base("Select Same Type", CoreIconCache.Select.ToIconSmall())
    {
        base.AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var view = Sender as FlowViewImGui;
        if (view is null)
        {
            return;
        }

        var nodeType = view.SelectedNodes.FirstOrDefault()?.GetType();
        if (nodeType is null)
        {
            return;
        }

        (Sender as FlowViewImGui)?.HandleSelectSameType(nodeType);
    }
}

/// <summary>
/// Menu command to navigate to the definition of the selected node.
/// </summary>
internal class GotoDefinitionMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoDefinitionMenuCommand"/> class.
    /// </summary>
    public GotoDefinitionMenuCommand()
        : base("Go to Definition", CoreIconCache.GotoDefination)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IFlowViewNode);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleGotoDefinition();
    }
}

/// <summary>
/// Menu command to find references to the selected node.
/// </summary>
internal class FindReferenceMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FindReferenceMenuCommand"/> class.
    /// </summary>
    public FindReferenceMenuCommand()
        : base("Find Reference", CoreIconCache.Search)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IFlowViewNode);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as FlowViewImGui)?.HandleFindReference();
    }
}