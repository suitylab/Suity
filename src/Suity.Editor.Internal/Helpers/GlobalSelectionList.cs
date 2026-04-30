using Suity.Editor.Documents.Linked;
using Suity.Editor.WorkSpaces;
using Suity.Selecting;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Helpers;

/// <summary>
/// A global selection list that aggregates assets from the asset manager and render targets from all workspaces.
/// Implements <see cref="ISelectionList"/> to provide a unified selection interface.
/// </summary>
public class GlobalSelectionList : ISelectionList
{
    /// <inheritdoc/>
    public ISelectionItem GetItem(string key)
    {
        Asset result = AssetManager.Instance.GetAsset(key);
        if (result != null)
        {
            return result;
        }

        foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
        {
            var renderTarget = workSpace.GetRenderTargetByFullPath(key);
            if (renderTarget != null)
            {
                return renderTarget.FileName;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public IEnumerable<ISelectionItem> GetItems()
    {
        IEnumerable<ISelectionItem> result = EditorObjectManager.Instance
            .AllObjects
            .Where(o => o.IsInStorage())
            .OfType<ISelectionItem>();

        foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
        {
            result = result.Concat(
                workSpace.RenderTargets.Where(o => !string.IsNullOrEmpty(o.FileName.PhysicFullPath)).Select(o => o.FileName)
                );
        }

        return result;
    }
}
