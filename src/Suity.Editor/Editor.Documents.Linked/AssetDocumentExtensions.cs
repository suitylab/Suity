using Suity.Editor.Design;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Extension methods for AssetDocument and related types.
/// </summary>
public static class AssetDocumentExtensions
{
    /// <summary>
    /// Gets the relative path of the document entry.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The relative path, or null.</returns>
    public static string GetRalativePath(this DocumentEntry entry)
    {
        if (entry.FileName.PhysicFileName is { } fileName)
        {
            string assetDir = EditorServices.CurrentProject.AssetDirectory;
            return fileName.MakeRalativePath(assetDir);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the relative base directory of the document entry.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The relative directory, or null.</returns>
    public static string GetRalativeBaseDirectory(this DocumentEntry entry)
    {
        if (entry.FileName.PhysicFileName is { } fileName)
        {
            string dir = Path.GetDirectoryName(fileName);
            string assetDir = EditorServices.CurrentProject.AssetDirectory;
            return dir.MakeRalativePath(assetDir);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the short type name of the asset.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The short type name.</returns>
    public static string GetShortTypeName(this DocumentEntry entry)
    {
        return entry.GetAsset()?.ShortTypeName;
    }

    /// <summary>
    /// Gets the full type name of the asset.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The full type name.</returns>
    public static string GetFullTypeName(this DocumentEntry entry)
    {
        return entry.GetAsset()?.FullTypeName;
    }

    /// <summary>
    /// Gets the namespace of the asset.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The namespace.</returns>
    public static string GetNameSpace(this DocumentEntry entry)
    {
        Asset asset = entry.GetAsset();
        return asset?.NameSpace ?? string.Empty;
    }

    /// <summary>
    /// Gets the asset filter for the document entry.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <param name="instance">Whether to get instance filter.</param>
    /// <returns>The asset filter.</returns>
    public static IAssetFilter GetAssetFilter(this DocumentEntry entry, bool instance = false)
    {
        Asset asset = GetAsset(entry);
        if (asset != null)
        {
            return asset.GetInstanceFilter(instance);
        }
        else
        {
            return AssetFilters.Default;
        }
    }

    /// <summary>
    /// Gets the asset from the document entry.
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The asset, or null.</returns>
    public static Asset GetAsset(this DocumentEntry entry)
    {
        if (entry is null || entry.IsReleased)
        {
            return null;
        }

        if (entry.FileName.PhysicFileName != null)
        {
            return EditorUtility.GetFileAsset(entry.FileName.PhysicFileName);
        }
        else
        {
            return (entry.Content as AssetDocument)?.AssetBuilder?.TargetAsset;
        }
    }

    /// <summary>
    /// Gets the asset from the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The asset, or null.</returns>
    public static Asset GetAsset(this Document document) => document?.Entry?.GetAsset();

    /// <summary>
    /// Checks if the editor object is in storage.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <returns>True if in storage.</returns>
    public static bool IsInStorage(this EditorObject obj)
    {
        return obj.GetStorageLocation() != null;
    }

    /// <summary>
    /// Gets the document entry for the editor object.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <param name="tryLoadStorage">Whether to try loading from storage.</param>
    /// <returns>The document entry, or null.</returns>
    public static DocumentEntry GetDocumentEntry(this EditorObject obj, bool tryLoadStorage = true)
    {
        return EditorServices.FileAssetManager.GetDocumentEntry(obj, tryLoadStorage);
    }

    /// <summary>
    /// Gets the document as the specified type.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <param name="tryLoadStorage">Whether to try loading from storage.</param>
    /// <returns>The document, or null.</returns>
    public static T GetDocument<T>(this EditorObject obj, bool tryLoadStorage = true) where T : class
    {
        return GetDocumentEntry(obj, tryLoadStorage)?.Content as T;
    }

    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <param name="tryLoadStorage">Whether to try loading from storage.</param>
    /// <returns>The document, or null.</returns>
    public static Document GetDocument(this EditorObject obj, bool tryLoadStorage = true)
    {
        return GetDocumentEntry(obj, tryLoadStorage)?.Content;
    }

    /// <summary>
    /// Shows the document view for the editor object.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <returns>The document view, or null.</returns>
    public static IDocumentView ShowDocumentView(this EditorObject obj)
    {
        if (GetDocumentEntry(obj, true) is not { } doc)
        {
            return null;
        }

        if (doc.ShowView() is not { } view)
        {
            return null;
        }

        // At this time, if the document was just opened, resources are not attached, so sync queue is needed
        QueuedAction.Do(() =>
        {
            // In non-main page
            if (view is IHasSubDocumentView hasSubView && hasSubView.CurrentSubView is { } subView && subView != view)
            {
                if (subView is IObjectView objView && objView.TargetObject == view.TargetObject)
                {
                    if (objView is IServiceProvider s)
                    {
                        s.GetService<IViewSelectable>()?.SetSelection(new ViewSelection(obj.GetStorageObject(true)));
                        return;
                    }
                }

                Logs.LogWarning("Current view is in non-main page, cannot select object.");
            }
            else
            {
                view.GetService<IViewSelectable>()?.SetSelection(new ViewSelection(obj.GetStorageObject(true)));
            }
        });

        return view;
    }

    /// <summary>
    /// Ensures a group exists at the specified path.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="groupPath">The group path.</param>
    /// <returns>The named node.</returns>
    public static INamedNode EnsureGroupByPath(this SNamedDocument doc, string groupPath)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        return doc.ItemCollection.EnsureGroupByPath(groupPath, doc.CreateGroup);
    }

    /// <summary>
    /// Changes the group path of items.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="groupPath">The current group path.</param>
    /// <param name="newGroupPath">The new group path.</param>
    /// <returns>True if changed successfully.</returns>
    public static bool ChangeGroupPath(this SNamedDocument doc, string groupPath, string newGroupPath)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (groupPath == newGroupPath)
        {
            return false;
        }

        var group = doc.ItemCollection.GetGroupByPath(groupPath);
        if (group is null)
        {
            return false;
        }

        var newGroup = doc.EnsureGroupByPath(newGroupPath);

        List<NamedItem> list = [];
        foreach (var item in group.Items)
        {
            list.Add(item);
        }

        foreach (var item in list)
        {
            group.RemoveItem(item);
            newGroup.AddItem(item);
        }

        return true;
    }

    /// <summary>
    /// Locates the editor object in the canvas.
    /// </summary>
    /// <param name="obj">The editor object.</param>
    /// <returns>True if located successfully.</returns>
    public static bool LocateInCanvas(this EditorObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj.GetDocumentEntry(true) is not { } doc)
        {
            return false;
        }

        if (LocateInCanvas(doc) is not { } viewNode)
        {
            return false; 
        }

        viewNode.SetExpand(true);

        if (viewNode.ExpandedView is IViewSelectable selectable)
        {
            selectable.SetSelection(new ViewSelection(obj.Name));
        }

        return true;
    }

    /// <summary>
    /// Locates the document entry in the canvas.
    /// </summary>
    /// <param name="doc">The document entry.</param>
    /// <returns>The flow view node, or null.</returns>
    public static IFlowViewNode LocateInCanvas(this DocumentEntry doc)
    {
        if (DocumentViewManager.Current.ActiveDocument?.Content is not ICanvasDocument canvasDoc)
        {
            return null;
        }

        if ((canvasDoc as Document)?.ShowView() is not IFlowView canvasView)
        {
            return null;
        }

        if (canvasDoc.FindDocumentNodes(doc.Content)?.FirstOrDefault() is not { } node)
        {
            return null;
        }

        if (node.GetViewNode(canvasView) is not { } viewNode)
        {
            return null;
        }

        (canvasView as IViewSelectable)?.SetSelection(new ViewSelection(node));

        return viewNode;
    }
}