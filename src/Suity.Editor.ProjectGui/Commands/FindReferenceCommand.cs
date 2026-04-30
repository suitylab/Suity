using Suity.Editor;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that finds all references to a selected asset or element.
/// </summary>
internal class FindReferenceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FindReferenceCommand"/> class.
    /// </summary>
    public FindReferenceCommand()
        : base("Find References", CoreIconCache.Search.ToIconSmall())
    {
        AcceptType<AssetFileNode>(false);
        AcceptType<AssetElementNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var selectedNode = view.SelectedNodes.FirstOrDefault();
        if (selectedNode is AssetFileNode projectFileNode)
        {
            var asset = projectFileNode.GetAsset();
            if (asset != null)
            {
                EditorUtility.FindReference(asset);
            }
        }
        else if (selectedNode is AssetElementNode projectElementNode)
        {
            var asset = projectElementNode.GetAsset();
            if (asset != null)
            {
                EditorUtility.FindReference(asset);
            }
        }
    }
}