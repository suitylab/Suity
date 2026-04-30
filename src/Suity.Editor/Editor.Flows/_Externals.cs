using Suity.Editor.Documents;
using Suity.Editor.Values;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Flows;

internal abstract class FlowsExternal
{
    internal static FlowsExternal _external;

    public abstract FlowDiagramItem CreateFlowDiagramItem(FlowNode node);

    public abstract FlowDocumentExternal CreateFlowDocumentEx(FlowDocument document);

    public abstract void CollectOutputNetwork(FlowDocument document, FlowNode node, HashSet<FlowNode> collection);

    public abstract void CollectInputNetwork(FlowDocument document, FlowNode node, HashSet<FlowNode> collection);

    public abstract void SetFlowAutoValue(SObject obj, FlowDiagramItem item, PositionAutomationMode mode);

    public abstract IFlowComputation CreateComputation(FlowDocument document, FunctionContext context = null);

    public abstract string ResolveConnectorName(Type nodeType, string aliasName, out bool renamed, out bool resolved);
}

internal abstract class FlowDocumentExternal
{
    public abstract IFlowDiagram Diagram { get; }

    public abstract int GridSpan { get; set; }

    public abstract LinkCollection Links { get; }

    public abstract void Sync(IPropertySync sync, ISyncContext context);

    public abstract IFlowDiagramItem GetFlowItem(string name);

    public abstract IFlowDiagramItem AddFlowNode(FlowNode node, Rectangle? rect = null);

    public abstract bool RemoveFlowNode(FlowNode node);

    public abstract bool RemoveFlowNode(IFlowDiagramItem item);

    public abstract bool GetIsConnectorLinked(FlowNodeConnector connector);

    public abstract int GetLinkedConnectorCount(FlowNodeConnector connector);

    public abstract FlowNodeConnector GetLinkedConnector(FlowNodeConnector connector);

    public abstract FlowNodeConnector[] GetLinkedConnectors(FlowNodeConnector connector, bool sort);

    public abstract void OnLoaded(DocumentLoadingIntent intent = DocumentLoadingIntent.Normal);

    public abstract void OnShowView();

    public abstract void RemoveInvalidConnectors();

    public abstract List<NodeLink> CollectInvalidConnectors(bool report = true);
}