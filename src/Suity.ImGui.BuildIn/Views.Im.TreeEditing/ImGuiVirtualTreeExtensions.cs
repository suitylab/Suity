using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.VirtualTree;
using Suity.Editor.VirtualTree.Actions;
using Suity.Helpers;
using Suity.Rex;
using Suity.Synchonizing.Core;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides extension methods for <see cref="ImGuiVirtualTreeView"/> to handle tree operations such as insert, delete, copy, paste, drag-drop, and text editing.
/// </summary>
public static class ImGuiVirtualTreeExtensions
{
    /// <summary>
    /// Handles automatic insert operation based on the selected node type.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleInsertAuto(this ImGuiVirtualTreeView treeView)
    {
        var treeNode = treeView.TreeData?.SelectedNode;

        VirtualNode? currentNode = (treeNode as VisualTreeNode<VirtualNode>)?.Value;
        if (currentNode is null)
        {
            return;
        }

        if (currentNode is IVirtualNodeListOperation)
        {
            HandleArrayAdd(treeView);
        }
        else if (currentNode.Parent is IVirtualNodeListOperation)
        {
            HandleItemInsert(treeView, false);
        }
    }

    /// <summary>
    /// Handles adding items to an array/list node.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="option">Optional creation options for the new items.</param>
    public static async void HandleArrayAdd(this ImGuiVirtualTreeView treeView, ObjectCreationOption? option = null)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return;
        }

        var treeNode = treeView.TreeData?.SelectedNode;

        VirtualNode? currentNode = (treeNode as VisualTreeNode<VirtualNode>)?.Value;
        if (currentNode is null)
        {
            return;
        }

        if (currentNode is not IVirtualNodeListOperation listOp)
        {
            return;
        }

        object[]? newItems = null;
        if (option?.Creation is { } creation)
        {
            var newItem = await creation(option.Type);
        }
        else
        {
            newItems = await listOp.GuiCreateItemsAsync(option?.Type);
        }

        if (newItems is null || newItems.Length == 0)
        {
            await DialogUtility.ShowMessageBoxAsyncL("No item created.");
            return;
        }
        
        QueuedAction.Do(() =>
        {
            try
            {
                model.BeginSetterAction();

                foreach (var newItem in newItems)
                {
                    VirtualNode newNode = listOp.InsertListItem(listOp.Count + 1, newItem, true);
                    if (newNode != null)
                    {
                        //ExpandNode(newNode);
                        newNode.Expand();
                        treeView.SelectNode(newNode);

                        if (newItems.Length == 1)
                        {
                            // Automatically execute default operation
                            //if (newNode.CanEditText)
                            //{
                            //    BeginInvoke(new Action(() => BeginTextLabelEdit(newNode)));
                            //}
                            //else if (newNode.CanEditPreviewText)
                            //{
                            //    BeginInvoke(new Action(() => BeginPreviewTextLabelEdit(newNode)));
                            //}
                            //else
                            //{
                            //    //TODO: Should not execute Action here, will trigger DoubleClick
                            //    //newNode.HandleNodeAction();
                            //}
                        }

                        treeView.BeginEdit(newNode);
                    }
                }
            }
            finally
            {
                model.EndSetterAction();
            }
        });
    }

    /// <summary>
    /// Handles inserting an item before or after the selected node.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="after">True to insert after the selected node; false to insert before.</param>
    /// <param name="option">Optional creation options for the new items.</param>
    public static async void HandleItemInsert(this ImGuiVirtualTreeView treeView, bool after, ObjectCreationOption? option = null)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return;
        }

        var treeNode = treeView.TreeData?.SelectedNode;

        var currentNode = (treeNode as VisualTreeNode<VirtualNode>)?.Value;
        if (currentNode is null)
        {
            return;
        }

        if (currentNode.Parent is not IVirtualNodeListOperation listOp)
        {
            return;
        }

        object[]? newItems = null;
        if (option?.Creation is { } creation)
        {
            var newItem = await creation(option.Type);
        }
        else
        {
            newItems = await listOp.GuiCreateItemsAsync(option?.Type);
        }

        if (newItems is null || newItems.Length == 0)
        {
            await DialogUtility.ShowMessageBoxAsyncL("No item created.");
            return;
        }

        QueuedAction.Do(() =>
        {
            try
            {
                model.BeginSetterAction();

                foreach (var newItem in newItems)
                {
                    VirtualNode newNode = listOp.InsertListItem(after ? currentNode.Index + 1 : currentNode.Index, newItem, true);
                    if (newNode != null)
                    {
                        //ExpandNode(newNode);
                        newNode.Expand();
                        treeView.SelectNode(newNode);

                        if (newItems.Length == 1)
                        {
                            // Automatically execute default operation
                            //if (newNode.CanEditText)
                            //{
                            //    BeginInvoke(new Action(() => BeginTextLabelEdit(newNode)));
                            //}
                            //else if (newNode.CanEditPreviewText)
                            //{
                            //    BeginInvoke(new Action(() => BeginPreviewTextLabelEdit(newNode)));
                            //}
                            //else
                            //{
                            //    //TODO: Should not execute Action here, will trigger DoubleClick
                            //    //newNode.HandleNodeAction();
                            //}
                        }

                        treeView.BeginEdit(newNode);
                    }
                }
            }
            finally
            {
                model.EndSetterAction();
            }
        });
    }

    /// <summary>
    /// Handles removing selected items from the tree.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleItemRemove(this ImGuiVirtualTreeView treeView)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return;
        }

        var treeNodes = treeView.TreeData?.SelectedNodesT ?? [];

        var selectedNodes = treeNodes
            .Select(o => o.Value)
            .OfType<VirtualNode>()
            .ToArray();

        if (selectedNodes.Length == 0)
        {
            return;
        }

        var nodeGroup = selectedNodes.GroupBy(o => o.Parent);

        try
        {
            model.BeginSetterAction();

            treeView.OnSelectionChanged();

            foreach (var nodes in nodeGroup)
            {
                IVirtualNodeListOperation? listOp = nodes.Key as IVirtualNodeListOperation;
                listOp?.RemoveListItems(nodes);
            }

            //TODO: Since VirtualNode.GetId() returns an index ID rather than a real ID, after deletion, the supplemented node reuses the old ImGuiNode
            // Should we add a MarkRemoved or Reset method to ImGuiNode to reset all properties and functions, and return ImGuiNode to Initializing state?

            treeView.ClearSelection();
        }
        finally
        {
            model.EndSetterAction();
        }
    }

    /// <summary>
    /// Handles copying or cutting selected items to the clipboard.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="isCopy">True to copy; false to cut.</param>
    public static void HandleArraySetClipboard(this ImGuiVirtualTreeView treeView, bool isCopy)
    {
        if (treeView.GetSelectedNodeParent() is not IVirtualNodeListOperation)
        {
            return;
        }

        List<ClipboardItem> list = [];

        foreach (VirtualNode node in treeView.SelectedNodes)
        {
            Visitor.Visit<ICrossMove>(node.DisplayedValue, (o, p) =>
            {
                o.ReadyMove();
            });

            list.Add(new ClipboardItem
            {
                Data = node.DisplayedValue,
            });
        }

        Device.Current.GetService<IClipboardService>().SetData(list, isCopy);

        if (!isCopy)
        {
            treeView.HandleItemRemove();
        }
    }

    /// <summary>
    /// Handles pasting items from the clipboard into the tree.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <returns>The operation state indicating success or failure.</returns>
    public static VOpState HandleArrayPaste(this ImGuiVirtualTreeView treeView)
    {
        VOpState state = VOpState.OK;

        if (treeView.GetSelectedNodeParent() is IVirtualNodeListOperation)
        {
            // Search the upper list first
            state = treeView.HandleArrayPasteInsert();
        }
        else if (treeView.SelectedNodes.CountOne() && treeView.SelectedNodes.First() is IVirtualNodeListOperation)
        {
            // Use itself as a list
            state = treeView.HandleArrayPasteAdd();
            if (state == VOpState.NotSupported && treeView.GetSelectedNodeParent() is IVirtualNodeListOperation)
            {
                state = treeView.HandleArrayPasteInsert();
            }
        }

        return state;
    }

    private static readonly LocalRefactor _localRefactor = new();

    private static VOpState HandleArrayPasteAdd(this ImGuiVirtualTreeView treeView)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return VOpState.NullReference;
        }

        if (treeView.SelectedNodes.Count() != 1)
        {
            return VOpState.MultipleNodeSelected;
        }

        var listNode = treeView.SelectedNodes.First();
        if (listNode is not IVirtualNodeListOperation listOp)
        {
            return VOpState.NullReference;
        }

        var clipboard = Device.Current.GetService<IClipboardService>();
        bool isCopy = clipboard.IsCopy;
        IEnumerable<ClipboardItem>? items = null;

        try
        {
            items = clipboard.GetDatas();
        }
        catch (Exception err)
        {
            err.LogError("Failed to get clipboard information.");
            return VOpState.NotSupported;
        }

        var supportedItems = items.Where(o => listOp.CanAddItem(o.Data)).ToArray();

        if (supportedItems.Length == 0 && items.Any())
        {
            return VOpState.NotSupported;
        }

        try
        {
            _localRefactor.Clear();
            _localRefactor.AddObjects(supportedItems.Select(o => o.Data));

            model.BeginSetterAction();
            treeView.ClearSelection();
            treeView.ExpandNode(listNode);

            //bool clearLog = false;

            foreach (var item in supportedItems)
            {
                VirtualNode newNode = listOp.InsertListItem(listOp.Count, item.Data, false);
                if (newNode != null)
                {
                    treeView.ExpandNode(newNode);
                    treeView.SelectNode(newNode, true);

                    Visitor.Visit<ICrossMove>(newNode.DisplayedValue, (o, p) => o.DoMove(_localRefactor));
                }
            }
        }
        finally
        {
            model.EndSetterAction();
            _localRefactor.Clear();
        }

        return VOpState.OK;
    }

    private static VOpState HandleArrayPasteInsert(this ImGuiVirtualTreeView treeView)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return VOpState.NotSupported;
        }

        var listNode = treeView.GetSelectedNodeParent();
        if (listNode is null)
        {
            return VOpState.NullReference;
        }

        if (listNode is not IVirtualNodeListOperation listOp)
        {
            return VOpState.NullReference;
        }

        // Paste after the last one
        int index = treeView.SelectedNodes.Last().Index;

        var clipboard = Device.Current.GetService<IClipboardService>();
        bool isCopy = clipboard.IsCopy;
        IEnumerable<ClipboardItem>? items = null;

        try
        {
            items = clipboard.GetDatas();
        }
        catch (Exception err)
        {
            err.LogError("Failed to get clipboard information.");
            return VOpState.NotSupported;
        }

        foreach (var item in items)
        {
            if (!listOp.CanAddItem(item.Data))
            {
                return VOpState.NotSupported;
            }
        }

        try
        {
            _localRefactor.Clear();
            _localRefactor.AddObjects(items.Select(o => o.Data));

            model.BeginSetterAction();
            treeView.ClearSelection();
            treeView.ExpandNode(listNode);

            //bool clearLog = false;

            foreach (var item in items)
            {
                index++;
                VirtualNode newNode = listOp.InsertListItem(index, item.Data, false);
                if (newNode != null)
                {
                    treeView.ExpandNode(newNode);
                    treeView.SelectNode(newNode, true);

                    Visitor.Visit<ICrossMove>(newNode.DisplayedValue, (o, p) => o.DoMove(_localRefactor));
                }
            }
        }
        finally
        {
            model.EndSetterAction();
            _localRefactor.Clear();
        }

        return VOpState.OK;
    }

    /// <summary>
    /// Handles navigating to the definition of the selected node's displayed value.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleGotoDefinition(this ImGuiVirtualTreeView treeView)
    {
        var node = treeView.TreeData?.SelectedNodesT.FirstOrDefault();

        var obj = node?.Value?.DisplayedValue;

        if (obj is { })
        {
            EditorUtility.GotoDefinition(obj);
        }
    }

    /// <summary>
    /// Handles finding references to the selected node's displayed value.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleFindReference(this ImGuiVirtualTreeView treeView)
    {
        var node = treeView.TreeData?.SelectedNodesT.FirstOrDefault();

        var obj = node?.Value?.DisplayedValue;

        if (obj is { })
        {
            EditorUtility.FindReference(obj);
        }
    }

    /// <summary>
    /// Handles showing analysis problems for the selected object.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleShowProblems(this ImGuiVirtualTreeView treeView)
    {
        var result = (treeView.SelectedObjects.FirstOrDefault() as ISupportAnalysis)?.Analysis;

        if (result is { })
        {
            EditorServices.AnalysisService.ShowProblems(result);
        }
    }

    /// <summary>
    /// Gets a value indicating whether any selected node can be commented.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <returns>True if any selected node supports commenting; otherwise, false.</returns>
    public static bool GetCanComment(this ImGuiVirtualTreeView treeView)
    {
        return treeView.SelectedObjects.OfType<IViewComment>().Any(c => c.CanComment);
    }

    /// <summary>
    /// Gets a value indicating whether all selected nodes are commented.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <returns>True if all selected nodes are commented; otherwise, false.</returns>
    public static bool GetIsComment(this ImGuiVirtualTreeView treeView)
    {
        foreach (var comment in treeView.SelectedNodes.Select(o => o.DisplayedValue).OfType<IViewComment>())
        {
            if (!comment.IsComment)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Handles commenting or uncommenting the selected nodes.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="comment">True to comment; false to uncomment.</param>
    public static void HandleComment(this ImGuiVirtualTreeView treeView, bool comment)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return;
        }

        var comments = treeView.SelectedNodes.Select(o => o.DisplayedValue).OfType<IViewComment>().ToArray();
        if (comments.Length > 0)
        {
            var action = new CommentAction(model, comments, comment);
            model.HandleSetterAction(action);
        }
    }

    /// <summary>
    /// Handles repairing the selected node if it supports advanced editing.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleRepair(this ImGuiVirtualTreeView treeView)
    {
        IViewAdvancedEdit? r = treeView.SelectedNodes.FirstOrDefault()?.GetAdvancedEdit(true);
        r?.Repair();
    }

    /// <summary>
    /// Handles relocating the selected node if it supports advanced editing.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    public static void HandleRelocate(this ImGuiVirtualTreeView treeView)
    {
        IViewAdvancedEdit? r = treeView.SelectedNodes.FirstOrDefault()?.GetAdvancedEdit(true);
        r?.Relocate();
    }

    /// <summary>
    /// Handles copying text from the selected node for the specified feature.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="feature">The advanced edit feature to copy text from.</param>
    public static void HandleCopyText(this ImGuiVirtualTreeView treeView, ViewAdvancedEditFeatures feature)
    {
        IViewAdvancedEdit? r = treeView.SelectedNodes.FirstOrDefault()?.GetAdvancedEdit(true);
        if (r is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        string text = r.GetText(feature);
        EditorUtility.SetSystemClipboardText(text);
    }

    /// <summary>
    /// Handles pasting text to the selected node for the specified feature.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="feature">The advanced edit feature to paste text to.</param>
    public static void HandlePasteText(this ImGuiVirtualTreeView treeView, ViewAdvancedEditFeatures feature)
    {
        IViewAdvancedEdit? r = treeView.SelectedNodes.FirstOrDefault()?.GetAdvancedEdit(true);
        if (r is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        EditorUtility.GetSystemClipboardText().ContinueWith(t => 
        {
            string text = t.Result;
            if (!string.IsNullOrEmpty(text))
            {
                r.SetText(feature, text);
            }
        });
    }

    /// <summary>
    /// Handles editing text for the selected node using a dialog.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="feature">The advanced edit feature to edit.</param>
    public static async void HandleEditText(this ImGuiVirtualTreeView treeView, ViewAdvancedEditFeatures feature)
    {
        IViewAdvancedEdit? r = treeView.SelectedNodes.FirstOrDefault()?.GetAdvancedEdit(true);
        if (r is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    await DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        string text = r.GetText(feature) ?? string.Empty;
        string result = await DialogUtility.ShowTextBlockDialogAsync($"Edit {feature}", text, feature.ToString());
        if (result != null)
        {
            r.SetText(feature, result);
        }
    }

    /// <summary>
    /// Handles exporting text from the selected node to a file.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="feature">The advanced edit feature to export.</param>
    public static async void HandleExportText(this ImGuiVirtualTreeView treeView, ViewAdvancedEditFeatures feature)
    {
        var node = treeView.SelectedNodes.FirstOrDefault();
        IViewAdvancedEdit? r = node?.GetAdvancedEdit(true);
        if (r is null)
        {
            return;
        }

        //if (ImGuiServices._license is { } license && !license.GetCapability(EditorCapabilities.Export))
        //{
        //    await DialogUtility.ShowMessageBoxAsync(license.GetFailedMessage(EditorCapabilities.Export));
        //    return;
        //}

        string text = r.GetText(feature) ?? string.Empty;

        string? initName = null;

        if (node?.DisplayedValue is INamed named)
        {
            initName = named.Name;
        }
        else if (node?.DisplayedValue is Document doc)
        {
            initName = Path.GetFileNameWithoutExtension(doc.FileName.PhysicFileName);
        }

        string? fileName = await GetExportFileName(feature, initName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllText(fileName, text);

            EditorUtility.LocateInPublishView(fileName);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    private static async Task<string?> GetExportFileName(ViewAdvancedEditFeatures feature, string? initName)
    {
        Project project = Project.Current;

        string? shortFileName = null;

        string ext = string.Empty;
        switch (feature)
        {
            case ViewAdvancedEditFeatures.XML:
                ext = ".xml";
                break;

            case ViewAdvancedEditFeatures.Json:
                ext = ".json";
                break;
        }

        if (string.IsNullOrWhiteSpace(initName))
        {
            initName = "ExportText";
        }

        shortFileName = await DialogUtility.ShowSingleLineTextDialogAsync("Enter file name", initName, s =>
        {
            if (!NamingVerifier.VerifyFileName(s))
            {
                DialogUtility.ShowMessageBoxAsync("Invalid file name");
                return false;
            }

            string s2 = project.PublishDirectory.PathAppend(s + ext);
            if (File.Exists(s2))
            {
                return false;
            }

            return true;
        });

        string s2 = project.PublishDirectory.PathAppend(shortFileName + ext);
        if (File.Exists(s2))
        {
            bool result = await DialogUtility.ShowYesNoDialogAsync("File already exists. Overwrite?");
            if (!result)
            {
                return null;
            }
        }

        if (string.IsNullOrEmpty(shortFileName))
        {
            return null;
        }

        return project.PublishDirectory.PathAppend(shortFileName + ext);
    }

    /// <summary>
    /// Gets the advanced edit interface for a virtual node.
    /// </summary>
    /// <param name="node">The virtual node to get advanced edit from.</param>
    /// <param name="msg">True to show a message box if the node does not support advanced editing.</param>
    /// <returns>The <see cref="IViewAdvancedEdit"/> interface or null if not supported.</returns>
    internal static IViewAdvancedEdit? GetAdvancedEdit(this VirtualNode? node, bool msg = false)
    {
        if (node is IViewAdvancedEdit viewRecoverable)
        {
            return viewRecoverable;
        }

        if (msg)
        {
            DialogUtility.ShowMessageBoxAsync("Node does not support serialization.");
        }

        return null;
    }

    /// <summary>
    /// Handles drag-over events for tree nodes, determining if a drop is valid.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="dropEvent">The drag event containing drop data.</param>
    /// <param name="mode">The drag-drop mode (inside, before, after).</param>
    /// <returns>True if the drop is valid; otherwise, false.</returns>
    public static bool HandleDragOver(this ImGuiNode node, ImGuiVirtualTreeView treeView, IDragEvent dropEvent, ImTreeNodeDragDropMode mode)
    {
        if (node.GetValue<VisualTreeNode>() is not VisualTreeNode<VirtualNode> myValue)
        {
            dropEvent.SetNoneEffect();
            return false;
        }

        var dropNode = myValue.Value;
        if (mode != ImTreeNodeDragDropMode.Inside)
        {
            dropNode = dropNode.Parent;
        }
        if (dropNode is null)
        {
            dropEvent.SetNoneEffect();
            return false;
        }

        if (dropEvent.Data.GetDataPresent(typeof(VisualTreeNode[])))
        {
            var input = dropEvent.Data.GetData<VisualTreeNode[]>();
            var idContext = dropEvent.Data.GetData<IHasId>();

            if (input is null && idContext is null)
            {
                dropEvent.SetNoneEffect();
                return false;
            }

            // Filter out nodes that would create circular references
            var vnodes = input
                .OfType<VisualTreeNode<VirtualNode>>()
                .Select(o => o.Value)
                .Where(o => !dropNode.ContainsParent(o))
                .ToArray();

            // Cannot set the dragged node as the target
            if (vnodes.Contains(myValue.Value))
            {
                dropEvent.SetNoneEffect();
                return false;
            }

            // Determine if this is an external drop (from different tree model)
            bool external = vnodes.Length == 0 || vnodes.FirstOrDefault()?.FindModel() != treeView.TreeModel;

            bool canDrop = false;
            if (external)
            {
                // External drop: check if target accepts external values
                if (vnodes.Length > 0)
                {
                    var values = vnodes.Select(n => n?.DisplayedValue);
                    canDrop = values.All(v => dropNode.CanDropInExternal(v));
                }
                else if (idContext != null)
                {
                    canDrop = dropNode.CanDropInExternal(idContext);
                } 
                else
                {
                    var values = input.Select(o => o.ValueObject);
                    canDrop = values.All(v => dropNode.CanDropInExternal(v));
                }
            }
            else
            {
                // Internal move: check if target accepts internal nodes
                canDrop = vnodes.All(o => dropNode.CanDropIn(o));
            }

            if (canDrop)
            {
                dropEvent.SetLinkEffect();
                return true;
            }
            else
            {
                dropEvent.SetNoneEffect();
                return false;
            }
        }
        else if (dropEvent.Data.GetDataPresent(DragEventData.DataFormat_File) && dropNode.DisplayedValue is IDropInCheck dropIn)
        {
            // Handle file system drag-over validation
            string[] files = (string[])dropEvent.Data.GetData(DragEventData.DataFormat_File);

            try
            {
                bool anyDropIn = false;

                foreach (var file in files)
                {
                    var info = new CommonFileInfo(file, string.Empty);
                    if (dropIn.DropInCheck(info))
                    {
                        anyDropIn = true;
                    }
                }

                if (anyDropIn)
                {
                    dropEvent.SetLinkEffect();
                    return true;
                }
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }
        }

        dropEvent.SetNoneEffect();
        return false;
    }

    /// <summary>
    /// Handles drag-drop events for tree nodes, performing the drop operation.
    /// </summary>
    /// <param name="node">The target ImGui node.</param>
    /// <param name="treeView">The tree view instance.</param>
    /// <param name="dropEvent">The drag event containing drop data.</param>
    /// <param name="mode">The drag-drop mode (inside, before, after).</param>
    public static void HandleDragDrop(this ImGuiNode node, ImGuiVirtualTreeView treeView, IDragEvent dropEvent, ImTreeNodeDragDropMode mode)
    {
        if (!(treeView.VirtualModel is { } model))
        {
            return;
        }

        if (node.GetValue<VisualTreeNode>() is not VisualTreeNode<VirtualNode> vNode)
        {
            return;
        }

        var dropNode = vNode.Value;
        if (mode != ImTreeNodeDragDropMode.Inside)
        {
            dropNode = dropNode.Parent;
        }
        if (dropNode is null)
        {
            return;
        }

        if (dropNode is not IVirtualNodeListOperation dropListOp)
        {
            return;
        }

        List<VirtualNode> newNodes = [];

        if (dropEvent.Data.GetDataPresent(typeof(VisualTreeNode[])))
        {
            var input = dropEvent.Data.GetData<VisualTreeNode[]>();
            var idContext = dropEvent.Data.GetData<IHasId>();

            if (input is null && idContext is null)
            {
                return;
            }

            // Filter out nodes that would create circular references (cannot drop parent into its own child)
            var vnodes = input
                .OfType<VisualTreeNode<VirtualNode>>()
                .Select(o => o.Value)
                .Where(o => !dropNode.ContainsParent(o))
                .ToArray();

            // Determine if this is an external drop (from different tree model) or internal move
            bool external = vnodes.Length == 0 || vnodes.FirstOrDefault()?.FindModel() != treeView.TreeModel;

            object[] movingValues = [];

            if (external)
            {
                // External drop: validate and collect values from different tree/model
                if (vnodes.Length > 0)
                {
                    movingValues = vnodes.Where(o => mode != ImTreeNodeDragDropMode.Inside || o != vNode.Value)
                        .Select(o => o.DisplayedValue)
                        .ToArray();

                    if (!movingValues.All(v => dropNode.CanDropInExternal(v)))
                    {
                        return;
                    }
                }
                else if (idContext != null)
                {
                    if (!dropNode.CanDropInExternal(idContext))
                    {
                        return;
                    }
                }
                else
                {
                    movingValues = input.Select(o => o.ValueObject).ToArray();

                    if (!movingValues.All(v => dropNode.CanDropInExternal(v)))
                    {
                        return;
                    }
                }
            }
            else
            {
                // Internal move: validate within same tree model
                movingValues = vnodes.Where(o => mode != ImTreeNodeDragDropMode.Inside || o != vNode.Value)
                    .Select(o => o.DisplayedValue)
                    .ToArray();

                if (!vnodes.All(o => dropNode.CanDropIn(o)))
                {
                    return;
                }
            }

            bool isRefDrop = false;

            // Convert values and detect if this is a reference drop (converted value differs from original)
            for (int i = 0; i < movingValues.Length; i++)
            {
                var convert = dropNode.DropInConvert(movingValues[i]);

                // If the converted value changes, consider this a reference drag-drop. References are not deleted from the original node.
                if (!ReferenceEquals(convert, movingValues[i]))
                {
                    isRefDrop = true;
                }
                movingValues[i] = convert;
            }

            model.SuspendGetValue();
            model.BeginSetterAction("Drag and drop nodes");
            // Create UndoRedo
            treeView.OnSelectionChanged();

            var index = node.GetDropIndex(dropNode, mode);

            if (vnodes.Length > 0)
            {
                var listOps = vnodes.Select(o => o.Parent)
                    .OfType<IVirtualNodeListOperation>()
                    .Distinct()
                    .ToArray();

                // Remove from original location only if this is not a reference drop
                if (!isRefDrop)
                {
                    foreach (var listOp in listOps)
                    {
                        listOp.RemoveListItems(vnodes);
                    }
                }

                // After removal, need to update the insertion index once
                index = node.GetDropIndex(dropNode, mode);

                foreach (var v in movingValues)
                {
                    var result = dropListOp.InsertListItem(index, v, false);
                    if (result != null)
                    {
                        newNodes.Add(result);
                        index++;
                    }
                }
            }
            else if (idContext != null)
            {
                var result = dropListOp.InsertListItem(index, idContext, false);
                if (result is { })
                {
                    newNodes.Add(result);
                }
            }
            else if (movingValues.Length > 0)
            {
                foreach (var v in movingValues)
                {
                    var result = dropListOp.InsertListItem(index, v, false);
                    if (result != null)
                    {
                        newNodes.Add(result);
                        index++;
                    }
                }
            }

            model.ResumeGetValue();
            dropNode.Expand();

            treeView.SetSelection(newNodes, false, true);
            model.EndSetterAction("Drag and drop nodes");
        }
        else if (dropEvent.Data.GetDataPresent(DragEventData.DataFormat_File) && dropNode.DisplayedValue is IDropInCheck dropIn)
        {
            // Handle file system drag-drop
            string[] files = (string[])dropEvent.Data.GetData(DragEventData.DataFormat_File);
            files = files.OrderByDescending(s => s).ToArray();

            try
            {
                List<object> objs = [];

                foreach (var file in files)
                {
                    var info = new CommonFileInfo(file, string.Empty);
                    var obj = dropIn.DropInConvert(info);
                    if (obj is { })
                    {
                        objs.Add(obj);
                    }
                }

                if (objs.Count == 0)
                {
                    return;
                }

                model.SuspendGetValue();
                model.BeginSetterAction("Drag and drop nodes");
                treeView.OnSelectionChanged();

                var index = node.GetDropIndex(dropNode, mode);

                foreach (var obj in objs)
                {
                    var result = dropListOp.InsertListItem(index, obj, false);
                    if (result is { })
                    {
                        newNodes.Add(result);
                    }
                }

                model.ResumeGetValue();
                dropNode.Expand();

                treeView.SetSelection(newNodes, false, true);
                model.EndSetterAction("Drag and drop nodes");
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }
        }

        node.QueueRefresh();
    }

    /// <summary>
    /// Gets the insertion index for a drop operation based on the drop mode.
    /// </summary>
    /// <param name="node">The source ImGui node.</param>
    /// <param name="parent">The parent virtual node for the drop.</param>
    /// <param name="mode">The drag-drop mode.</param>
    /// <returns>The index at which to insert the dropped item.</returns>
    private static int GetDropIndex(this ImGuiNode node, VirtualNode? parent, ImTreeNodeDragDropMode mode)
    {
        if (node.GetValue<VisualTreeNode>() is not VisualTreeNode<VirtualNode> vNode)
        {
            return 0;
        }

        var dropNode = vNode.Value;
        if (mode != ImTreeNodeDragDropMode.Inside)
        {
            dropNode = parent;
        }

        if (dropNode is null)
        {
            return 0;
        }

        if (dropNode is not IVirtualNodeListOperation dropListOp)
        {
            return 0;
        }

        return mode switch
        {
            ImTreeNodeDragDropMode.Inside => dropListOp.Count,
            ImTreeNodeDragDropMode.Previous => vNode.Value.Index,
            ImTreeNodeDragDropMode.Next => vNode.Value.Index + 1,
            _ => 0,
        };
    }

    /// <summary>
    /// Gets the parent of the first selected node.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <returns>The parent virtual node or null if no selection.</returns>
    internal static VirtualNode? GetSelectedNodeParent(this ImGuiVirtualTreeView treeView)
    {
        return treeView.SelectedNodes.FirstOrDefault()?.Parent;
    }

    /// <summary>
    /// Gets the parents of all selected nodes.
    /// </summary>
    /// <param name="treeView">The tree view instance.</param>
    /// <returns>A sequence of parent virtual nodes.</returns>
    internal static IEnumerable<VirtualNode?> GetSelectedNodeParents(this ImGuiVirtualTreeView treeView)
    {
        return treeView.SelectedNodes.Select(o => o?.Parent);
    }
}