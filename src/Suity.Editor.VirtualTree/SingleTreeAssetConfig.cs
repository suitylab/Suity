using Suity.Collections;
using Suity.Views;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Configuration for a single tree asset, including selections, expansions, and preview presets.
/// </summary>
public class SingleTreeAssetConfig
{
    /// <summary>
    /// Gets or sets the list of selected node paths.
    /// </summary>
    public List<string> Selections { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of expanded node paths.
    /// </summary>
    public List<string> Expands { get; set; } = [];

    /// <summary>
    /// Gets or sets the name of the currently selected preview preset.
    /// </summary>
    public string SelectedPreviewPreset { get; set; }

    /// <summary>
    /// Gets or sets the list of preview presets.
    /// </summary>
    public List<PreviewPreset> PreviewPresets { get; set; } = [];

    /// <summary>
    /// Gets or sets custom user data associated with this config.
    /// </summary>
    public object UserData { get; set; }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public SingleTreeAssetConfig()
    {
    }

    /// <summary>
    /// Adds virtual paths to the selections list.
    /// </summary>
    /// <param name="selection">The array of virtual paths to add.</param>
    public void AddBySelection(VirtualPath[] selection)
    {
        foreach (var sel in selection)
        {
            Selections.Add(sel.ToString());
        }
    }

    /// <summary>
    /// Gets the currently selected preview preset.
    /// </summary>
    /// <returns>The selected preset, or null if none matches.</returns>
    public PreviewPreset GetSelectedPreviewPreset()
    {
        if (string.IsNullOrWhiteSpace(SelectedPreviewPreset))
        {
            return PreviewPresets?.Where(o => string.IsNullOrWhiteSpace(o.Name)).FirstOrDefault();
        }
        else
        {
            return PreviewPresets?.Where(o => o.Name == SelectedPreviewPreset).FirstOrDefault();
        }
    }
}

/// <summary>
/// Represents a preset for preview column configuration.
/// </summary>
public class PreviewPreset
{
    /// <summary>
    /// Gets or sets the name of this preset.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of preview path states.
    /// </summary>
    public List<PreviewPathState> PreviewPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets the column widths for the preview.
    /// </summary>
    public List<float> ColumnWidths { get; set; } = [];

    /// <summary>
    /// Sets the preview paths from a collection of PreviewPath objects.
    /// </summary>
    /// <param name="paths">The collection of preview paths to set.</param>
    public void SetPreviewPaths(IEnumerable<PreviewPath> paths)
    {
        PreviewPaths.Clear();
        var p = paths?.Select(o => new PreviewPathState(o))
            .ToArray() ?? [];

        if (p.Length > 0)
        {
            PreviewPaths.AddRange(p);
        }
    }

    /// <summary>
    /// Checks whether this preset has any preview paths.
    /// </summary>
    /// <returns>True if there are preview paths, false otherwise.</returns>
    public bool HasPreviewPath()
    {
        return PreviewPaths?.Count > 0;
    }

    /// <summary>
    /// Gets the preview paths as an array of PreviewPath objects.
    /// </summary>
    /// <returns>An array of preview paths.</returns>
    public PreviewPath[] GetPreviewPaths()
    {
        if (PreviewPaths?.Count > 0)
        {
            return PreviewPaths.Select(o => o?.CreatePreviewPath()).SkipNull().ToArray();
        }
        else
        {
            return [];
        }
    }
}

/// <summary>
/// Interface for objects that support preview preset management.
/// </summary>
public interface IHasPreviewPreset
{
    /// <summary>
    /// Gets the name of the current preset.
    /// </summary>
    string CurrentPresetName { get; }

    /// <summary>
    /// Creates a new preset from the current state.
    /// </summary>
    /// <returns>The created preset.</returns>
    PreviewPreset CreatePreset();

    /// <summary>
    /// Applies a preset to the current state.
    /// </summary>
    /// <param name="preset">The preset to apply.</param>
    void ApplyPreset(PreviewPreset preset);

    /// <summary>
    /// Restores all presets from a collection.
    /// </summary>
    /// <param name="presets">The collection of presets to restore.</param>
    void RestorePresets(IEnumerable<PreviewPreset> presets);

    /// <summary>
    /// Gets all available presets.
    /// </summary>
    /// <returns>An enumerable of all presets.</returns>
    IEnumerable<PreviewPreset> GetAllPresets();

    /// <summary>
    /// Marks a preset as the current selection.
    /// </summary>
    /// <param name="name">The name of the preset to mark.</param>
    void MarkPreset(string name);

    /// <summary>
    /// Marks the current preset.
    /// </summary>
    void MarkCurrentPreset();

    /// <summary>
    /// Changes to a different preset.
    /// </summary>
    /// <param name="name">The name of the preset to switch to.</param>
    /// <param name="markCurrent">Whether to mark it as the current preset.</param>
    void ChangePreset(string name, bool markCurrent);

    /// <summary>
    /// Removes a preset by name.
    /// </summary>
    /// <param name="name">The name of the preset to remove.</param>
    void RemovePreset(string name);
}

/// <summary>
/// Represents the serializable state of a preview path.
/// </summary>
public class PreviewPathState
{
    /// <summary>
    /// Gets or sets the name of the preview path.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name of the preview path.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the sync path string.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public PreviewPathState()
    {
    }

    /// <summary>
    /// Initializes a new instance from a PreviewPath.
    /// </summary>
    /// <param name="path">The preview path to copy from.</param>
    public PreviewPathState(PreviewPath path)
    {
        Name = path.Name;
        DisplayName = path.DisplayName;
        Path = path.Path.ToString();
    }

    /// <summary>
    /// Creates a PreviewPath from this state.
    /// </summary>
    /// <returns>A new PreviewPath instance, or null if the path is empty.</returns>
    public PreviewPath CreatePreviewPath()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }

        return new PreviewPath(Path, Name, DisplayName);
    }
}