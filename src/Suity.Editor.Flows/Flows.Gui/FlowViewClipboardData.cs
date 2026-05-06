using Suity.Synchonizing;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Holds clipboard data for copy/paste operations in flow views, including diagram items and links.
/// </summary>
internal class FlowViewClipboardData : ISyncObject
{
    /// <summary>
    /// The list of diagram items to be copied or pasted.
    /// </summary>
    public List<IFlowDiagramItem> Items;
    /// <summary>
    /// The list of links between diagram items to be copied or pasted.
    /// </summary>
    public List<NodeLink> Links;

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Items = sync.Sync("Items", Items, SyncFlag.ByRef);
        Links = sync.Sync("Links", Links, SyncFlag.ByRef);
    }
}