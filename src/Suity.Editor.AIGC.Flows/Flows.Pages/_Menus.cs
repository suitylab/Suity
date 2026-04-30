using Suity;
using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.Gui;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Menu;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.AIGC.Flows.Pages;

#region CreateFunctionMenuItem

/// <summary>
/// Menu item for creating a new function node group in the AIGC flow diagram.
/// </summary>
[InsertInto("#AigcFlow")]
public class CreateFunctionMenuItem : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFunctionMenuItem"/> class.
    /// </summary>
    public CreateFunctionMenuItem()
        : base("Create Function", CoreIconCache.Function)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        Visible = selectionCount == 0;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IFlowView flowView) return;
        var diagram = flowView.Diagram;

        // 1. Pop up dialog to get function name
        string funcName = "NewFunction";
        funcName = await DialogUtility.ShowSingleLineTextDialogAsyncL("Enter Function Name", funcName, k =>
        {
            if (string.IsNullOrWhiteSpace(k)) return false;
            if (!NamingVerifier.VerifyIdentifier(k))
            {
                //DialogUtility.ShowMessageBoxAsync("Invalid name, please enter a valid identifier.");
                return false;
            }
            if (diagram.GetNode(k) != null)
            {
                //DialogUtility.ShowMessageBoxAsync("Name already exists, please enter a different name.");
                return false;
            }
            return true;
        });

        if (string.IsNullOrWhiteSpace(funcName)) return;

        // 2. Get current mouse coordinates as starting position
        var position = flowView.LastMousePosition;

        // 3. Use undo/redo manager to execute creation action
        if ((flowView as IServiceProvider)?.GetService<UndoRedoManager>() is { } undoRedo)
        {
            undoRedo.Do(new CreateFunctionAction(diagram, funcName, position.X, position.Y));
        }
    }
}

/// <summary>
/// Undoable action that creates a function node group including definition page, result page, and internal nodes.
/// </summary>
public class CreateFunctionAction : UndoRedoAction
{
    private readonly IFlowDiagram _diagram;
    private readonly string _funcName;
    private readonly int _startX;
    private readonly int _startY;

    // Record all created items for undo
    private readonly List<IFlowDiagramItem> _createdItems = new();

    private string _startPageName;
    private string _resultPageName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFunctionAction"/> class.
    /// </summary>
    /// <param name="diagram">The flow diagram to create the function in.</param>
    /// <param name="funcName">The name of the function.</param>
    /// <param name="x">The starting X position.</param>
    /// <param name="y">The starting Y position.</param>
    public CreateFunctionAction(IFlowDiagram diagram, string funcName, int x, int y)
    {
        _diagram = diagram;
        _funcName = funcName;
        _startX = x;
        _startY = y;
    }

    /// <inheritdoc/>
    public override string Name => "Create Function Node Group";

    /// <inheritdoc/>
    public override void Do()
    {
        _createdItems.Clear();

        // Layout constants
        int spacing = 300; // Spacing between two pages
        int nodeOffsetY = 30; // Offset of internal nodes relative to page top

        // 1. Create function definition page (Definition Page)
        var startPage = _diagram.AddNode(new PageDefinitionNode() { Name = _funcName });
        startPage.SetBound(new Rectangle(_startX, _startY, 140, 120));
        _createdItems.Add(startPage);

        // 2. Create function result page (Result Page)
        var resultPage = _diagram.AddNode(new PageResultNode() { Name = _funcName + "_End" });
        resultPage.SetBound(new Rectangle(_startX + spacing, _startY, 140, 120));
        _createdItems.Add(resultPage);

        // 3. Establish framework connection between pages
        _diagram.AddLink(resultPage.Name, "Definition", startPage.Name, "Result");

        // 4. Create start action node (Begin)
        var beginNode = _diagram.AddNode(new PageBeginNode() { Name = "OnStart" });
        beginNode.SetBound(new Rectangle(_startX + 10, _startY + nodeOffsetY, 10, 10));
        _createdItems.Add(beginNode);

        // 5. Create start parameter node (Parameter)
        var paramNode = _diagram.AddNode(new PageParameterInputNode() { Name = "Input" });
        paramNode.SetBound(new Rectangle(_startX + 10, _startY + nodeOffsetY + 20, 10, 10));
        _createdItems.Add(paramNode);

        // 6. Create end action node (End)
        var endNode = _diagram.AddNode(new PageEndNode() { Name = "OnComplete" });
        endNode.SetBound(new Rectangle(_startX + spacing + 10, _startY + nodeOffsetY + 10, 10, 10));
        _createdItems.Add(endNode);

        // 7. Create output parameter node (Output)
        var outputNode = _diagram.AddNode(new PageParameterOutputNode() { Name = "Output" });
        outputNode.SetBound(new Rectangle(_startX + spacing + 10, _startY + nodeOffsetY + 30, 10, 10));
        _createdItems.Add(outputNode);

        // Refresh document state
        _diagram.GetFlowDocument()?.FlushQueuedConnection();

        _startPageName = startPage.Name;
        _resultPageName = resultPage.Name;
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _diagram.RemoveLink(_resultPageName, "Definition", _startPageName, "Result");

        // Remove all created nodes in reverse order
        // Note: Suity's RemoveNode usually automatically removes associated Links
        for (int i = _createdItems.Count - 1; i >= 0; i--)
        {
            _diagram.RemoveNode(_createdItems[i].Node);
        }
        _createdItems.Clear();

        _diagram.GetFlowDocument()?.FlushQueuedConnection();
    }
}
#endregion


#region ExtractFunctionMenuItem

/// <summary>
/// Menu item for extracting selected nodes into a reusable function in the AIGC flow diagram.
/// </summary>
[InsertInto("#AigcFlow")]
public class ExtractFunctionMenuItem : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractFunctionMenuItem"/> class.
    /// </summary>
    public ExtractFunctionMenuItem()
        : base("Extract Function", CoreIconCache.Function)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        Visible = selectionCount > 0;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IFlowView flowView)
        {
            return;
        }

        if (flowView.Diagram is not { } diagram)
        {
            return;
        }

        var nodes = Selection.OfType<IFlowViewNode>().Select(o => o.Node).ToArray();
        if (nodes.Length == 0)
        {
            return;
        }

        FlowFunctionCollector collector = null;

        try
        {
            collector = new FlowFunctionCollector(nodes, flowView);
        }
        catch (Exception err)
        {
            err.LogError();
            return;
        }

        if (!collector.IsValid)
        {
            return;
        }

        if (collector.Diagram.GetFlowDocument() is not { } doc)
        {
            return;
        }

        string fileFullName = doc.FileName?.PhysicFileName;
        if (string.IsNullOrWhiteSpace(fileFullName))
        {
            return;
        }

        string dir = Path.GetDirectoryName(fileFullName);
        if (string.IsNullOrWhiteSpace(dir))
        {
            return;
        }

        string newFileName = KeyIncrementHelper.MakeKey("Function", 2, k =>
        {
            string test = $"{dir}/{k}.sasset";
            return !File.Exists(test);
        });

        newFileName = await DialogUtility.ShowSingleLineTextDialogAsyncL("Enter Function File Name", newFileName, k =>
        {
            if (string.IsNullOrWhiteSpace(k)) return false;
            if (!NamingVerifier.VerifyIdentifier(k))
            {
                DialogUtility.ShowMessageBoxAsync("Invalid name, please enter a valid identifier.");
                return false;
            }

            string test = $"{dir}/{k}.sasset";
            if (File.Exists(test))
            {
                DialogUtility.ShowMessageBoxAsyncL("File already exists, please enter a different file name.");
                return false;
            }

            return true;
        });

        if (string.IsNullOrWhiteSpace(newFileName))
        {
            return;
        }

        fileFullName = $"{dir}/{newFileName}.sasset";

        var newDoc = collector.CreateDocument(fileFullName, newFileName);


        if ((flowView as IServiceProvider)?.GetService<UndoRedoManager>() is { } undoRedo)
        {
            undoRedo.Do(collector);
        }
    }
}

/// <summary>
/// Collects and manages the extraction of selected flow nodes into a separate function document.
/// </summary>
public class FlowFunctionCollector : UndoRedoAction
{
    /// <summary>
    /// Maps a connector to its external links and provides definition metadata.
    /// </summary>
    public class ConnectorMap
    {
        /// <summary>
        /// Gets or sets the definition name for this connector mapping.
        /// </summary>
        public string DefName { get; set; }

        /// <summary>
        /// Gets or sets the description for this connector mapping.
        /// </summary>
        public string DefDescription { get; set; }


        /// <summary>
        /// Gets or sets the name of the node containing this connector.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector.
        /// </summary>
        public string ConnectorName { get; set; }

        /// <summary>
        /// Gets or sets the direction of the connector.
        /// </summary>
        public FlowDirections Direction { get; set; }

        /// <summary>
        /// Gets the list of linked nodes connected to this connector.
        /// </summary>
        public List<ConnectorNodeLink> LinkedNodes { get; } = [];

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DefName} ({NodeName}-{ConnectorName})";
        }
    }

    /// <summary>
    /// Represents a link between a node and a connector.
    /// </summary>
    public class ConnectorNodeLink
    {
        /// <summary>
        /// Gets or sets the name of the linked node.
        /// </summary>
        public string NodeName { get; init; }

        /// <summary>
        /// Gets or sets the name of the linked connector.
        /// </summary>
        public string ConnectorName { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{NodeName}-{ConnectorName}";
        }
    }

    readonly IFlowView _flowView;
    readonly IFlowDiagram _diagram;
    readonly HashSet<FlowNode> _nodes = [];
    readonly Rectangle _bound;
    readonly List<NodeLink> _internalLinks = [];
    readonly List<ConnectorMap> _connectorMaps = [];

    private bool _isValid = false;
    private EditorAssetRef<PageDefinitionAsset> _pageDefAsset = new();
    private string _functionItemName;


    /// <summary>
    /// Initializes a new instance of the <see cref="FlowFunctionCollector"/> class.
    /// </summary>
    /// <param name="nodes">The collection of nodes to extract into a function.</param>
    /// <param name="flowView">The flow view context, optional.</param>
    public FlowFunctionCollector(IEnumerable<FlowNode> nodes, IFlowView flowView = null)
    {
        nodes ??= [];

        _nodes.AddRange(nodes.SkipNull().Where(o => o is not AigcPageDefNode));
        if (_nodes.Count == 0)
        {
            return;
        }

        _diagram = _nodes.FirstOrDefault()?.Diagram;
        if (_diagram is null)
        {
            return;
        }

        _flowView = flowView;

        _bound = FlowDocument.GetBound(_nodes.Select(o => o.DiagramItem).SkipNull());

        Collect();

        _isValid = true;
    }

    private void Collect()
    {
        Dictionary<string, ConnectorMap> connectorMaps = [];

        foreach (var node in _nodes)
        {
            foreach (var connector in node.Connectors)
            {
                var linkedConnectors = _diagram.GetLinkedConnectors(connector, false);
                foreach (var linkedConnector in linkedConnectors)
                {
                    if (_nodes.Contains(linkedConnector.ParentNode))
                    {
                        if (connector.Direction == FlowDirections.Input)
                        {
                            _internalLinks.Add(new NodeLink(linkedConnector.ParentNode.Name, linkedConnector.Name, node.Name, connector.Name));
                        }
                        else
                        {
                            _internalLinks.Add(new NodeLink(node.Name, connector.Name, linkedConnector.ParentNode.Name, linkedConnector.Name));
                        }
                        // Internal connection
                        continue;
                    }

                    // Found an external connection
                    string defName = connector.Name;
                    if (connectorMaps.ContainsKey(defName))
                    {
                        defName = KeyIncrementHelper.MakeKey(defName, 1, k => !connectorMaps.ContainsKey(k));
                    }

                    var connectorMap = new ConnectorMap
                    {
                        DefName = defName,
                        DefDescription = connector.Description,
                        NodeName = node.Name,
                        ConnectorName = connector.Name,
                        Direction = connector.Direction,
                    };

                    connectorMap.LinkedNodes.Add(new ConnectorNodeLink
                    {
                        NodeName = linkedConnector.ParentNode.Name,
                        ConnectorName = linkedConnector.Name,
                    });

                    connectorMaps.Add(defName, connectorMap);
                }
            }
        }

        _connectorMaps.Clear();
        _connectorMaps.AddRange(connectorMaps.Values);
    }

    /// <summary>
    /// Gets the flow diagram associated with this collector.
    /// </summary>
    public IFlowDiagram Diagram => _diagram;

    /// <summary>
    /// Gets a value indicating whether the collector is valid and ready for extraction.
    /// </summary>
    public bool IsValid => _isValid;

    /// <summary>
    /// Gets the page definition asset created during document creation.
    /// </summary>
    public PageDefinitionAsset PageDefAsset => _pageDefAsset.Target;

    /// <summary>
    /// Creates a new document from the collected nodes and sets up the function structure.
    /// </summary>
    /// <param name="fileName">The full path for the new document file.</param>
    /// <param name="name">The name for the function.</param>
    /// <returns>The created flow diagram item, or null if creation failed.</returns>
    public IFlowDiagramItem CreateDocument(string fileName, string name)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        if (File.Exists(fileName))
        {
            return null;
        }

        var format = DocumentManager.Instance.GetDocumentFormat("AigcFlow");
        if (format is null)
        {
            return null;
        }

        var entry = DocumentManager.Instance.NewDocument(fileName, format);
        if (entry is null)
        {
            return null;
        }

        var doc = entry.Content as AigcFlowDocument;
        if (doc is null)
        {
            return null;
        }

        var diagram = doc.Diagram;
        if (diagram is null)
        {
            return null;
        }

        foreach (var node in _nodes)
        {
            var item = node.DiagramItem as NamedItem;
            if (item is null)
            {
                continue;
            }

            var newItem = Cloner.Clone(item);
            doc.ItemCollection.AddItem(newItem);
        }

        foreach (var link in _internalLinks)
        {
            diagram.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        var b = doc.GetBound();
        var startPage = diagram.AddNode(new PageDefinitionNode() { Name = name });
        startPage.SetBound(new Rectangle(b.X - 180, b.Y, 140, 200));

        var resultPage = diagram.AddNode(new PageResultNode() { Name = "End" });
        resultPage.SetBound(new Rectangle(b.Right + 40, b.Y, 140, 200));

        diagram.AddLink(resultPage.Name, "Definition", startPage.Name, "Result");

        doc.FlushQueuedConnection();

        int inputIndex = 0;
        int outputIndex = 0;

        foreach (var c in _connectorMaps)
        {
            var node = diagram.GetNode(c.NodeName);
            var connector = node?.GetConnector(c.ConnectorName);
            if (connector is null)
            {
                continue;
            }

            TypeDefinition typeDef = null;
            if (FlowTypeManager.Instance.GetDataType(connector.DataTypeName) is TypeDefinitionDataType dataType)
            {
                typeDef = dataType.TypeDef;
            }

            if (connector.ConnectionType == FlowConnectorTypes.Action)
            {
                if (connector.Direction == FlowDirections.Input)
                {
                    var begin = diagram.AddNode(new PageBeginNode() { Name = c.DefName, Description = c.DefDescription, TypeDef = typeDef });
                    begin.SetBound(new Rectangle(startPage.X + 10, startPage.Y + 30 + inputIndex * 20, 10, 10));
                    diagram.AddLink(begin.Name, "Out", c.NodeName, c.ConnectorName);
                    c.DefName = begin.Name; // Update name
                    inputIndex++;
                }
                else
                {
                    var end = diagram.AddNode(new PageEndNode() { Name = c.DefName, Description = c.DefDescription, TypeDef = typeDef });
                    end.SetBound(new Rectangle(resultPage.X + 10, resultPage.Y + 40 + outputIndex * 20, 10, 10));
                    diagram.AddLink(c.NodeName, c.ConnectorName, end.Name, "In");
                    c.DefName = end.Name; // Update name
                    outputIndex++;
                }
            }
            else if (connector.ConnectionType == FlowConnectorTypes.Data)
            {
                if (connector.Direction == FlowDirections.Input)
                {
                    var parameter = diagram.AddNode(new PageParameterInputNode() { Name = c.DefName, Description = c.DefDescription, TypeDef = typeDef });
                    parameter.SetBound(new Rectangle(startPage.X + 10, startPage.Y + 30 + inputIndex * 20, 10, 10));
                    diagram.AddLink(parameter.Name, "Out", c.NodeName, c.ConnectorName);
                    c.DefName = parameter.Name; // Update name
                    inputIndex++;
                }
                else
                {
                    var output = diagram.AddNode(new PageParameterOutputNode() { Name = c.DefName, Description = c.DefDescription, TypeDef = typeDef });
                    output.SetBound(new Rectangle(resultPage.X + 10, resultPage.Y + 40 + outputIndex * 20, 10, 10));
                    diagram.AddLink(c.NodeName, c.ConnectorName, output.Name, "In");
                    c.DefName = output.Name; // Update name
                    outputIndex++;
                }
            }
        }

        startPage.SetSize(startPage.Width, 40 + inputIndex * 20, false);
        resultPage.SetSize(resultPage.Width, 50 + outputIndex * 20, false);

        doc.ForceSave();

        _pageDefAsset.Target = (startPage as PageDefinitionDiagramItem)?.TargetAsset as PageDefinitionAsset;

        return startPage;
    }




    /// <inheritdoc/>
    public override string Name => "Extract Function Node";

    /// <inheritdoc/>
    public override void Do()
    {
        var pageDefAsset = _pageDefAsset.Target;
        if (pageDefAsset is null)
        {
            return;
        }

        foreach (var link in _internalLinks)
        {
            _diagram.RemoveLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        foreach (var map in _connectorMaps)
        {
            if (map.Direction == FlowDirections.Input)
            {
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.RemoveLink(link.NodeName, link.ConnectorName, map.NodeName, map.ConnectorName);
                }
            }
            else
            {
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.RemoveLink(map.NodeName, map.ConnectorName, link.NodeName, link.ConnectorName);
                }
            }
        }

        foreach (var node in _nodes)
        {
            _diagram.RemoveNode(node);
        }

        var functionNode = new PageFunctionNode(pageDefAsset)
        {
            Name = pageDefAsset.LocalName,
        };
        var functionItem = _diagram.AddNode(functionNode);

        functionNode.FlushQueuedConnection();
        functionNode.FlushQueuedUpdate();

        _functionItemName = functionItem.Name;

        foreach (var map in _connectorMaps)
        {
            if (map.Direction == FlowDirections.Input)
            {
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.AddLink(link.NodeName, link.ConnectorName, functionItem.Name, map.DefName);
                }
            }
            else
            {
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.AddLink(functionItem.Name, map.DefName, link.NodeName, link.ConnectorName);
                }
            }
        }

        int boundCenterX = _bound.X + _bound.Width / 2;
        int boundCenterY = _bound.Y + _bound.Height / 2;
        functionItem.SetPosition(boundCenterX - 50, boundCenterY - 50);
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_diagram == null)
        {
            return;
        }

        // 1. Handle function node and its unique connection lines
        if (!string.IsNullOrEmpty(_functionItemName))
        {
            var functionNode = _diagram.GetNode(_functionItemName);
            if (functionNode != null)
            {
                // Explicitly remove function node's connection lines based on _connectorMaps
                // Function node's port names correspond to map.DefName
                foreach (var map in _connectorMaps)
                {
                    if (map.Direction == FlowDirections.Input)
                    {
                        // Remove: external node -> function node (Input)
                        foreach (var link in map.LinkedNodes)
                        {
                            _diagram.RemoveLink(link.NodeName, link.ConnectorName, functionNode.Name, map.DefName);
                        }
                    }
                    else
                    {
                        // Remove: function node (Output) -> external node
                        foreach (var link in map.LinkedNodes)
                        {
                            _diagram.RemoveLink(functionNode.Name, map.DefName, link.NodeName, link.ConnectorName);
                        }
                    }
                }

                // Remove the function node itself
                _diagram.RemoveNode(functionNode);
            }
            _functionItemName = null;
        }

        // 2. Restore original nodes
        foreach (var node in _nodes)
        {
            _diagram.AddNode(node);
        }

        // 3. Restore internal connections (originally internal links within the selected node set)
        foreach (var link in _internalLinks)
        {
            _diagram.AddLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector);
        }

        // 4. Restore original external connections (restore topology before extraction)
        foreach (var map in _connectorMaps)
        {
            if (map.Direction == FlowDirections.Input)
            {
                // Restore: external node -> original node
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.AddLink(link.NodeName, link.ConnectorName, map.NodeName, map.ConnectorName);
                }
            }
            else
            {
                // Restore: original node -> external node
                foreach (var link in map.LinkedNodes)
                {
                    _diagram.AddLink(map.NodeName, map.ConnectorName, link.NodeName, link.ConnectorName);
                }
            }
        }

        // Refresh document to ensure UI synchronization
        if (_diagram.GetFlowDocument() is { } doc)
        {
            doc.FlushQueuedConnection();
        }
    }
}

#endregion
