using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that optimizes the storage of selected file bunch assets by rebuilding them.
/// </summary>
internal class ShrinkFileBunchCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShrinkFileBunchCommand"/> class.
    /// </summary>
    public ShrinkFileBunchCommand()
        : base("Optimize File Bunch Storage", CoreIconCache.FileBunch.ToIconSmall())
    {
        AcceptType<AssetFileNode>(false);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        if (Sender is not IProjectGui view)
        {
            return;
        }

        Visible = view.SelectedNodes.All(o => (o as AssetFileNode)?.GetAsset() is IFileBunch);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var service = Device.Current.GetService<IFileBunchService>();

        long num = 0;

        foreach (var bunch in view.SelectedNodes.OfType<AssetFileNode>().Select(o => o.GetAsset<IFileBunch>()).OfType<IFileBunch>())
        {
            num += bunch.Rebuild();
        }

        if (num > 0)
        {
            DialogUtility.ShowMessageBoxAsyncL("File storage optimized");
        }
    }
}