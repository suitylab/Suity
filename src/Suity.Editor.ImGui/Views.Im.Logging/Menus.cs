using Suity.Editor;
using Suity.Editor.View.ViewModel;
using Suity.Helpers;
using Suity.Views.Im.TreeEditing;
using Suity.Views.Menu;

namespace Suity.Views.Im.Logging;

/// <summary>
/// Represents the root menu command for the console view.
/// </summary>
public class ConsoleViewRootCommand : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleViewRootCommand"/> class.
    /// </summary>
    /// <param name="treeView">The tree view associated with this command.</param>
    public ConsoleViewRootCommand(ImGuiPathTreeView treeView)
        : base(":ConsoleView")
    {
        AddCommand(new CopyLogItemCommand(treeView));
    }
}

/// <summary>
/// Represents a menu command that copies the selected log item text to the clipboard.
/// </summary>
public class CopyLogItemCommand(ImGuiPathTreeView treeView) : MenuCommand<ImGuiPathTreeView>(treeView, "Copy", CoreIconCache.Copy.ToIconSmall())
{
    /// <inheritdoc/>
    public override void DoCommand()
    {
        string s;

        if (Target.SelectedNode is LogNode mNode)
        {
            s = mNode.OriginText ?? mNode.Text;
        }
        else
        {
            s = Target.SelectedNode?.Text;
        }

        if (s != null)
        {
            EditorUtility.SetSystemClipboardText(s);
        }
    }
}
