using Suity.Editor.Flows;

namespace Suity.Editor.Documents.Canvas;

/// <summary>
/// Provides a selection list of available canvas tool nodes for the canvas document.
/// </summary>
internal class CanvasSelectionList : FlowNodeSelectionNode
{
    /// <summary>
    /// Gets the singleton instance of the canvas selection list.
    /// </summary>
    public static CanvasSelectionList Instance { get; } = new();

    private CanvasSelectionList()
    {
        AddDerived<CanvasToolNode>();
    }
}
