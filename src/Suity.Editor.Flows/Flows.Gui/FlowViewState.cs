using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Represents the persisted state of a flow view, including viewport position, zoom, selection, and expand state.
/// </summary>
public class FlowViewState
{
    /// <summary>
    /// Gets or sets the horizontal scroll position of the viewport.
    /// </summary>
    public int ViewX { get; set; }

    /// <summary>
    /// Gets or sets the vertical scroll position of the viewport.
    /// </summary>
    public int ViewY { get; set; }

    /// <summary>
    /// Gets or sets the zoom level of the viewport.
    /// </summary>
    public float ViewZoom { get; set; }

    /// <summary>
    /// Gets or sets the list of selected node names.
    /// </summary>
    public List<string> SelectedNodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the user data associated with the inspector context.
    /// </summary>
    public object InspectorUserData { get; set; }

    /// <summary>
    /// Gets or sets the expanded state paths for the UI hierarchy.
    /// </summary>
    public string[] ExpandState { get; set; }
}