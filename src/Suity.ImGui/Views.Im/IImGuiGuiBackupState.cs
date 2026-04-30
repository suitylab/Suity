using Suity.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Represents a backup of GUI state for ImGui nodes, allowing state to be saved and restored.
/// </summary>
public interface IImGuiGuiBackupState
{
    /// <summary>
    /// Backs up the state of the specified node.
    /// </summary>
    /// <param name="node">The node to back up.</param>
    void BackupNode(ImGuiNode node);

    /// <summary>
    /// Begins restoring state to the specified node.
    /// </summary>
    /// <param name="node">The node to restore state to.</param>
    /// <returns>True if state was restored; otherwise, false.</returns>
    bool BeginRestoreNode(ImGuiNode node);

    /// <summary>
    /// Ends the restore process for the specified node.
    /// </summary>
    /// <param name="node">The node that was restored.</param>
    void EndRestoreNode(ImGuiNode node);
}

/// <summary>
/// Backs up and restores expand/collapse state for ImGui nodes.
/// </summary>
public class GuiExpandBackupState : IImGuiGuiBackupState
{
    private readonly HashSet<ImGuiPath> _expandState = [];

    /// <inheritdoc/>
    public void BackupNode(ImGuiNode node)
    {
        GuiExpandableValue? value = node.GetValue<GuiExpandableValue>();
        if (value?.Expanded == true)
        {
            _expandState.Add(node.FullPath);
        }
        else
        {
            _expandState.Remove(node.FullPath);
        }
    }

    /// <inheritdoc/>
    public bool BeginRestoreNode(ImGuiNode node)
    {
        if (_expandState.Contains(node.FullPath))
        {
            var value = node.GetOrCreateValue<GuiExpandableValue>();
            value.Expanded = true;

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public void EndRestoreNode(ImGuiNode node)
    {
    }

    /// <summary>
    /// Clears all backed up expand states.
    /// </summary>
    public void Clear()
    {
        _expandState.Clear();
    }

    /// <summary>
    /// Gets all paths that were backed up as expanded.
    /// </summary>
    /// <returns>An enumerable of expanded ImGuiPath instances.</returns>
    public IEnumerable<ImGuiPath> GetExpandedPaths()
    {
        return _expandState;
    }

    /// <summary>
    /// Sets the expanded paths from a collection of ImGuiPath instances.
    /// </summary>
    /// <param name="paths">The paths to set as expanded.</param>
    public void SetExpandedPaths(IEnumerable<ImGuiPath> paths)
    {
        _expandState.Clear();
        _expandState.AddRange(paths.SkipNull());
    }

    /// <summary>
    /// Sets the expanded paths from a collection of string path representations.
    /// </summary>
    /// <param name="paths">The string paths to set as expanded.</param>
    public void SetExpandedPaths(IEnumerable<string> paths)
    {
        _expandState.Clear();
        _expandState.AddRange(paths.SkipNull().Select(s => ImGuiPath.Create(s)));
    }
}
