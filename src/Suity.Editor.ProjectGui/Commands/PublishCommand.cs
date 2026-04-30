using Suity.Helpers;
using Suity.Views.Menu;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that publishes a selected asset file.
/// </summary>
public class PublishCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishCommand"/> class.
    /// </summary>
    public PublishCommand()
        : base("Publish", CoreIconCache.Publish.ToIconSmall())
    {
        AcceptOneItemOnly = true;
        AcceptType<IAssetFileNode>(true);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        var asset = GetSelectionAsset();

        Visible = asset != null;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var asset = GetSelectionAsset();
        if (asset is null)
        {
            return;
        }

        try
        {
            asset.Publish();
        }
        catch (Exception err)
        {
            err.LogError("Publish failed");
        }
    }

    private IAssetPublish GetSelectionAsset()
    {
        var node = Selection?.FirstOrDefault() as IAssetFileNode;

        return node?.TargetAsset as IAssetPublish;
    }
}