using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Suity.Editor.Services;

namespace Suity.Editor;

/// <summary>
/// Provides static utility methods for navigating to various object types including
/// assets, documents, editor objects, workspaces, type definitions, and sync paths.
/// Supports go-to-definition, find-references, find-implementations, and global search.
/// </summary>
public static class Navigator
{
    /// <summary>
    /// Opens the GUI navigation dialog for the specified value, resolving the target
    /// and navigating to it. Shows an error message if navigation fails.
    /// </summary>
    /// <param name="value">The object to navigate to. Supported types include <see cref="Guid"/>,
    /// <see cref="INavigable"/>, <see cref="SObject"/>, <see cref="SKey"/>, <see cref="SAssetKey"/>,
    /// <see cref="SEnum"/>, <see cref="SValue"/>, <see cref="SItem"/>, <see cref="EditorObject"/>,
    /// <see cref="Document"/>, <see cref="TypeDefinition"/>, <see cref="AssetSelection"/>,
    /// <see cref="WorkSpace"/>, <see cref="IHasId"/>, <see cref="IHasAsset"/>, <see cref="KeyCode"/>,
    /// <see cref="RenderFileName"/>, <see cref="LocateWorkSpaceVReq"/>, and <see cref="System.Collections.IEnumerable"/>.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool GuiGotoDefinition(object value)
    {
        if (value is null)
        {
            DialogUtility.ShowMessageBoxAsyncL("No navigation target");

            return false;
        }

        if (value is INavigableRoute route && route.GetNavigableRoute() is object routeObj)
        {
            value = routeObj;
        }

        switch (value)
        {
            case Guid id:
                return GuiNavigate(id);

            case INavigable navigable:
                return GuiNavigate(navigable.GetNavigationTarget());

            case SObject obj:
                if (obj.Controller is INavigable n)
                {
                    return GuiNavigate(n.GetNavigationTarget());
                }
                else if (!TypeDefinition.IsNullOrEmpty(obj.ObjectType))
                {
                    return GuiNavigate(obj.ObjectType);
                }
                else if (!TypeDefinition.IsNullOrEmpty(obj.InputType))
                {
                    return GuiNavigate(obj.InputType);
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsyncL("Navigation not supported");

                    return false;
                }

            case SKey skey:
                if (!string.IsNullOrEmpty(skey.SelectedKey))
                {
                    return GuiNavigate(new RawAssetKey(skey.SelectedKey));
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsyncL("No navigation target");

                    return false;
                }

            case SAssetKey assetKey:
                if (!string.IsNullOrEmpty(assetKey.SelectedKey))
                {
                    return GuiNavigate(new RawAssetKey(assetKey.SelectedKey));
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsyncL("No navigation target");

                    return false;
                }

            case SEnum senum:
                return GuiNavigate(senum.ValueId);

            case SValue svalue:
                return GuiNavigate(svalue.Value);

            case SItem sitem:
                if (!TypeDefinition.IsNullOrEmpty(sitem.InputType))
                {
                    return GuiNavigate(sitem.InputType);
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsyncL("No navigation target");

                    return false;
                }

            case EditorObject editorObj:
                return GuiNavigate(editorObj);

            case Document document:
                return GuiNavigate(document);

            case TypeDefinition typeDef:
                return GuiNavigate(typeDef);

            case AssetSelection s:
                if (s.Id != Guid.Empty)
                {
                    return GuiNavigate(s.Id);
                }
                else if (!string.IsNullOrEmpty(s.SelectedKey))
                {
                    return GuiNavigate(new RawAssetKey(s.SelectedKey));
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsyncL("No navigation target");
                    return false;
                }

            case WorkSpace workSpace:
                return GuiNavigate(workSpace);

            case IHasId idContext:
                return GuiNavigate(idContext.Id);

            case IHasAsset assetContext:
                return GuiNavigate(assetContext.TargetAsset);

            case KeyCode keyCode:
                if (Navigate(new RawAssetKey(keyCode.ToString())))
                {
                    return true;
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsync(L("Cannot navigate to") + ": " + keyCode);

                    return false;
                }

            case RenderFileName fileName:
                if (GuiNavigate(fileName))
                {
                    return true;
                }
                else
                {
                    DialogUtility.ShowMessageBoxAsync(L("Cannot navigate to") + ": " + fileName.PhysicFullPath);

                    return false;
                }

            case string:
                DialogUtility.ShowMessageBoxAsync(L("Type") + ": String");

                return false;

            case System.Collections.IEnumerable enumerable:
                var subValue = enumerable.OfType<object>().FirstOrDefault();

                return GuiGotoDefinition(subValue);

            case LocateWorkSpaceVReq workSpaceVReq:
                return EditorUtility.LocateWorkSpace(workSpaceVReq);

            default:
                DialogUtility.ShowMessageBoxAsync(L("Type") + ": " + value.GetType().GetTypeCSCodeName(true));

                return false;
        }
    }

    /// <summary>
    /// Internal navigation helper that attempts to navigate to the target and shows
    /// an error message with logging if navigation fails.
    /// </summary>
    /// <param name="target">The target object to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    private static bool GuiNavigate(object target)
    {
        if (target != null)
        {
            if (Navigate(target))
            {
                return true;
            }
            else
            {
                string msg = L("Cannot navigate to") + ": " + EditorUtility.GetBriefStringL(target);
                // Easy for user to copy
                Logs.LogError(msg);

                DialogUtility.ShowMessageBoxAsync(msg);

                return false;
            }
        }
        else
        {
            DialogUtility.ShowMessageBoxAsyncL("No navigation target");

            return false;
        }
    }

    /// <summary>
    /// Navigates to the specified context object, resolving the appropriate navigation
    /// strategy based on the object type.
    /// </summary>
    /// <param name="contextObj">The context object to navigate to. Supported types include
    /// <see cref="Guid"/>, <see cref="ReportList"/>, <see cref="INavigable"/>, <see cref="RawAssetKey"/>,
    /// <see cref="DocumentEntry"/>, <see cref="Document"/>, <see cref="EditorObject"/>, <see cref="WorkSpace"/>,
    /// <see cref="IHasId"/>, <see cref="IHasAsset"/>, <see cref="TypeDefinition"/>,
    /// <see cref="SyncPathReportItem"/>, <see cref="Action"/>, <see cref="RenderFileName"/>,
    /// <see cref="StorageLocation"/>, <see cref="Exception"/>, <see cref="KeyCode"/>, and <see cref="LocateWorkSpaceVReq"/>.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool Navigate(object contextObj)
    {
        if (contextObj is INavigableRoute route && route.GetNavigableRoute() is object routeObj)
        {
            contextObj = routeObj;
        }

        switch (contextObj)
        {
            case Guid id:
                return NavigateEditorObject(id);

            case ReportList syncPathReportList:
                return Navigate(syncPathReportList.Owner);

            case INavigable navigable:
                return Navigate(navigable.GetNavigationTarget());

            case RawAssetKey assetKey:
                return NavigateAsset(assetKey.AssetKey);

            case DocumentEntry documentEntry:
                return NavigateDocument(documentEntry);

            case Document document:
                return NavigateDocument(document?.Entry);

            case EditorObject editorObj:
                return NavigateEditorObject(editorObj);

            case WorkSpace workSpace:
                return EditorUtility.LocateWorkSpace(workSpace);

            case IHasId idContext:
                return NavigateEditorObject(EditorObjectManager.Instance.GetObject(idContext.Id));

            case IHasAsset assetContext:
                return NavigateEditorObject(assetContext.TargetAsset);

            case TypeDefinition typeDefinition:
                return NavigateEditorObject(typeDefinition.TargetId);

            case SyncPathReportItem syncPathReportItem:
                return NavigateSyncPath(syncPathReportItem);

            case Action action:
                try
                {
                    action();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            case RenderFileName fileName:
                return EditorUtility.LocateInProject(fileName.PhysicFullPath);

            case StorageLocation location:
                return NavigateDocument(location);

            case Exception err:
                DialogUtility.ShowExceptionAsync(err);
                return true;

            case KeyCode keyCode:
                return NavigateAsset(keyCode.ToString());

            case LocateWorkSpaceVReq workSpaceVReq:
                return EditorUtility.LocateWorkSpace(workSpaceVReq);

            default:
                return false;
        }
    }

    /// <summary>
    /// Opens and navigates to the document at the specified storage location.
    /// </summary>
    /// <param name="location">The storage location of the document to open and navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateDocument(StorageLocation location)
    {
        var document = DocumentManager.Instance.OpenDocument(location);
        return NavigateDocument(document);
    }

    /// <summary>
    /// Navigates to the specified document entry by locating it in the canvas,
    /// showing its view, or locating it in the project file system.
    /// </summary>
    /// <param name="document">The document entry to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateDocument(DocumentEntry document)
    {
        if (document is null)
        {
            return false;
        }

        if (document.LocateInCanvas() != null)
        {
            return true;
        }
        else if (document.ShowView() != null)
        {
            return true;
        }
        else
        {
            if (document.FileName.PhysicFileName != null)
            {
                QueuedAction.Do(() => EditorUtility.LocateInProject(document.FileName.PhysicFileName));

                return true;
            }
            else
            {
                return false;
            }
            //return EditorUtility.LocateInProject(document.FullPath);
        }
    }

    /// <summary>
    /// Navigates to the editor object identified by the specified GUID.
    /// </summary>
    /// <param name="id">The unique identifier of the editor object to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateEditorObject(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        EditorObject obj = EditorObjectManager.Instance.GetObject(id);
        if (obj is null)
        {
            return false;
        }

        return NavigateEditorObject(obj);
    }

    /// <summary>
    /// Navigates to the specified editor object. If the object has ID conflicts,
    /// shows a selection dialog for the user to resolve the conflict.
    /// </summary>
    /// <param name="obj">The editor object to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false (may be false when showing conflict resolution dialog).</returns>
    public static bool NavigateEditorObject(EditorObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is INavigableRoute route && route.GetNavigableRoute() is EditorObject routeObj)
        {
            obj = routeObj;
        }

        if (obj.Entry != null && obj.IdConflict)
        {
            QueuedAction.Do(async () =>
            {
                var list = new SelectionList();
                int index = 0;
                foreach (var item in obj.Entry.Targets)
                {
                    list.Add(new ConflictNaviItem(index, item));
                    index++;
                }

                var result = await list.ShowSelectionGUIAsync("Resolve Conflict", new SelectionOption { SelectedKey = obj.FullName });
                if (!result.IsSuccess)
                {
                    return;
                }

                if (result.Item is EditorObjectNaviItem e)
                {
                    obj = e.Target;
                }

                InternalNavigate(obj);
            });

            return false;
        }
        else
        {
            return InternalNavigate(obj);
        }
    }

    /// <summary>
    /// Internal navigation logic for an editor object. Attempts to locate the object
    /// in the canvas, show its document view, or locate it in the project file system.
    /// </summary>
    /// <param name="obj">The editor object to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    private static bool InternalNavigate(EditorObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj.LocateInCanvas())
        {
            return true;
        }

        if (obj.ShowDocumentView() != null)
        {
            return true;
        }

        var location = obj.GetStorageLocation();
        if (location != null)
        {
            if (location.PhysicFileName != null)
            {
                // Entering via project view double-click needs a one-frame delay to take effect
                QueuedAction.Do(() => EditorUtility.LocateInProject(location.PhysicFileName));

                return true;
            }
            else
            {
                Asset libAsset = location.GetAsset()?.Library;
                if (libAsset != null)
                {
                    return NavigateEditorObject(libAsset);
                }
            }

            //return EditorUtility.LocateInProject(fileName);
        }

        if (obj is WorkSpaceAsset workSpaceAsset && workSpaceAsset.WorkSpace is { } workSpace)
        {
            EditorUtility.LocateWorkSpace(workSpace);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Navigates to the asset identified by the specified asset key.
    /// </summary>
    /// <param name="assetKey">The key of the asset to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateAsset(string assetKey)
    {
        if (string.IsNullOrEmpty(assetKey))
        {
            return false;
        }

        Asset asset = AssetManager.Instance.GetAsset(assetKey);
        if (asset is null)
        {
            return false;
        }

        if (asset is INavigableRoute route && route.GetNavigableRoute() is Asset routeObj)
        {
            asset = routeObj;
        }

        if (asset.LocateInCanvas())
        {
            return true;
        }

        if (asset.ShowDocumentView() != null)
        {
            return true;
        }

        var fileName = asset.GetStorageLocation();
        if (fileName?.PhysicFileName != null)
        {
            QueuedAction.Do(() => EditorUtility.LocateInProject(fileName.PhysicFileName));

            return true;
            //return EditorUtility.LocateInProject(fileName);
        }

        return false;
    }

    /// <summary>
    /// Navigates to a sync path report item, either by locating the sync path in the canvas
    /// or by showing the text search result in the document view.
    /// </summary>
    /// <param name="report">The sync path report item to navigate to.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateSyncPath(SyncPathReportItem report)
    {
        var project = Project.Current;

        if (project is null)
        {
            return false;
        }

        if (report is null)
        {
            return false;
        }

        if (!SyncPath.IsNullOrEmpty(report.Path))
        {
            if (NavigateSyncPath(report.Owner, report.Path))
            {
                return true;
            }
            else
            {
                return Navigate(report.Owner);
            }
        }
        else
        {
            if (report.Info is TextSearchResult textSearchResult && report.Owner is ITextAsset textAsset && textAsset is Asset asset)
            {
                var view = asset.ShowDocumentView();
                if (view is IViewSelectable sel)
                {
                    QueuedAction.Do(() => 
                    {
                        sel.SetSelection(new ViewSelection(textSearchResult));
                    });
                    return true;
                }
                else
                {
                    return Navigate(report.Owner);
                }
            }
            else
            {
                return Navigate(report.Owner);
            }
        }
    }

    /// <summary>
    /// Navigates to the specified sync path within the context of an owner object.
    /// Supports documents, storage locations, assets, and GUIDs as owners.
    /// </summary>
    /// <param name="owner">The owner object that provides the context for navigation.
    /// Supported types include <see cref="IViewGotoDefinitionAction"/>, <see cref="Document"/>,
    /// <see cref="StorageLocation"/>, <see cref="Asset"/>, <see cref="Guid"/>, and <see cref="INavigable"/>.</param>
    /// <param name="path">The sync path to navigate to within the owner.</param>
    /// <returns>True if navigation succeeded; otherwise, false.</returns>
    public static bool NavigateSyncPath(object owner, SyncPath path)
    {
        Asset asset = null;
        DocumentEntry document = null;

        switch (owner)
        {
            case IViewGotoDefinitionAction gotoDefAction:
                QueuedAction.Do(() => gotoDefAction.GotoDefinition(path, out var rest));
                return true;

            case Document doc:
                // Document may be expired, need to ensure single load
                document = DocumentManager.Instance.OpenDocument(doc.FileName);
                asset = document.GetAsset();
                break;

            case StorageLocation storageLocation:
                document = DocumentManager.Instance.OpenDocument(storageLocation);
                asset = document.GetAsset();
                break;

            case Asset a_asset:
                asset = a_asset;
                document = a_asset.GetDocumentEntry(true);
                break;

            case Guid id:
                asset = AssetManager.Instance.GetAsset(id);
                document = asset?.GetDocumentEntry(true);
                break;

            case INavigable navi:
                object target = navi.GetNavigationTarget();
                if (target is INavigable)
                {
                    // avoid nested
                    return false;
                }
                return NavigateSyncPath(target, path);

            default:
                break;
        }

        if (document?.LocateInCanvas() is { } viewNode)
        {
            if (viewNode.ExpandedView is IViewSelectable selectable)
            {
                QueuedAction.Do(() => selectable.SetSelection(new ViewSelection(path)));
            }
            else if (viewNode.ExpandedView is IViewGotoDefinitionAction gotoDef)
            {
                QueuedAction.Do(() => gotoDef.GotoDefinition(path, out var rest));
            }

            return true;
        }
        else if (document?.ShowView() is { } view)
        {
            if (view.GetService<IViewSelectable>() is IViewSelectable selectable)
            {
                QueuedAction.Do(() => selectable.SetSelection(new ViewSelection(path)));
            }
            else if (view.GetService<IViewGotoDefinitionAction>() is IViewGotoDefinitionAction gotoDef)
            {
                QueuedAction.Do(() => gotoDef.GotoDefinition(path, out var rest));
            }

            return true;
        }
        else if (asset is IViewGotoDefinitionAction gotoDef)
        {
            QueuedAction.Do(() => gotoDef.GotoDefinition(path, out var rest));

            return true;
        }
        else if (asset != null)
        {
            var location = asset.GetStorageLocation();
            if (location?.PhysicFileName != null)
            {
                // Entering via project view double-click needs a one-frame delay to take effect
                QueuedAction.Do(() => EditorUtility.LocateInProject(location.PhysicFileName));

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves a GUID from the specified value by checking various interfaces and types.
    /// </summary>
    /// <param name="value">The value to resolve. Supported types include <see cref="Guid"/>,
    /// <see cref="INavigable"/>, <see cref="IHasId"/>, <see cref="DocumentEntry"/>, and types
    /// that can resolve to an asset key.</param>
    /// <returns>The resolved GUID, or <see cref="Guid.Empty"/> if resolution fails.</returns>
    public static Guid ResolveId(object value)
    {
        switch (value)
        {
            case Guid guid:
                return guid;

            case INavigable navigable:
                {
                    object obj = navigable.GetNavigationTarget();
                    if (obj is not INavigable)
                    {
                        return ResolveId(obj);
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                }

            case IHasId context:
                return context.Id;

            case DocumentEntry docEntry:
                return ResolveId(docEntry.Content);
        }

        string assetKey = ResolveAssetKey(value);
        if (!string.IsNullOrEmpty(assetKey))
        {
            return AssetManager.Instance.GetAsset(assetKey)?.Id ?? Guid.Empty;
        }
        else
        {
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Resolves an asset key string from the specified value by checking various interfaces and types.
    /// </summary>
    /// <param name="value">The value to resolve. Supported types include <see cref="string"/>,
    /// <see cref="KeyCode"/>, <see cref="TypeDefinition"/>, <see cref="IFindable"/>, <see cref="Asset"/>,
    /// <see cref="Document"/>, <see cref="AssetSelection"/>, <see cref="SObject"/>, <see cref="SKey"/>,
    /// <see cref="SAssetKey"/>, <see cref="SItem"/>, and <see cref="INavigable"/>.</param>
    /// <returns>The resolved asset key, or null if resolution fails.</returns>
    public static string ResolveAssetKey(object value)
    {
        switch (value)
        {
            case string str:
                return str;

            case KeyCode keyCode:
                return keyCode.ToString();

            case TypeDefinition typeDefinition:
                return typeDefinition.Target?.AssetKey;

            case IFindable findable:
                return findable.GetFindingKey();

            case Asset asset:
                return asset.AssetKey;

            case Document document:
                return document.GetAsset()?.AssetKey;

            case AssetSelection assetSelection:
                return assetSelection.SelectedKey;

            case SObject obj:
                if (obj.Controller is INavigable n)
                {
                    return ResolveAssetKey(n.GetNavigationTarget());
                }
                else if (!TypeDefinition.IsNullOrEmpty(obj.ObjectType))
                {
                    return obj.ObjectType.Target?.AssetKey;
                }
                else if (!TypeDefinition.IsNullOrEmpty(obj.InputType))
                {
                    return obj.InputType.Target?.AssetKey;
                }
                else
                {
                    return null;
                }

            case SKey skey:
                return skey.SelectedKey;

            case SAssetKey assetKey:
                return assetKey.SelectedKey;

            case SItem sitem:
                if (!TypeDefinition.IsNullOrEmpty(sitem.InputType))
                {
                    return sitem.InputType.Target?.AssetKey;
                }
                else
                {
                    return null;
                }

            case INavigable navigable:
                return ResolveAssetKey(navigable.GetNavigationTarget());

            default:
                return null;
        }
    }

    /// <summary>
    /// Performs a global search across all assets in the current project for the specified search string.
    /// Results are logged and grouped by owner.
    /// </summary>
    /// <param name="findStr">The search string to look for.</param>
    /// <param name="findOption">The search options to apply during the search.</param>
    /// <returns>True if the search was initiated; otherwise, false.</returns>
    public static bool GlobalSearch(string findStr, SearchOption findOption)
    {
        var project = Project.Current;

        if (project is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(findStr))
        {
            return false;
        }

        List<SyncPathReportItem> results = [];

        var assets = AssetManager.Instance.Assets.
            Where(o => o.ParentAsset is null && o.IsInStorage()).ToArray();

        var documents = new ConcurrentHashSet<DocumentEntry>();

        EditorUtility.DoProgress(L("Searching..."), p =>
        {
            int index = 0;

            void SearchAsset(Asset asset)
            {
                DocumentEntry document = asset.GetDocumentEntry(true);
                if (document is null || document.Content is null || documents.Contains(document))
                {
                    return;
                }

                p.UpdateProgess(index + 1, assets.Length, document.FileName.FullPath, string.Empty);

                var r = Validator.Find(asset, document.Content, findStr, findOption);
                if (r?.Any() == true)
                {
                    lock (results)
                    {
                        results.AddRange(r);
                    }
                }

                documents.Add(document);

                Interlocked.Increment(ref index);
            }

            Parallel.ForEach(assets, asset => 
            {
                try
                {
                    SearchAsset(asset);
                }
                catch (Exception err)
                {
                    err.LogWarning();
                }
            });

            /*foreach (var asset in assets)
            {
                try
                {
                    SearchAsset(asset);
                }
                catch (Exception err)
                {
                    err.LogWarning();
                }
            }*/

            QueuedAction.Do(() =>
            {
                EditorCommands.ClearLog.Invoke();
                Logs.LogInfo(L("Search") + ": " + findStr);

                var resultList = GroupReportItems(results);
                foreach (var result in resultList)
                {
                    Logs.LogInfo(result);
                }

                //foreach (SyncPathReportItem result in results)
                //{
                //    Logs.LogInfo(result);
                //}

                EditorUtility.ShowLogView();
            });

            p.CompleteProgess();
        });

        return true;
    }

    /// <summary>
    /// Finds and logs all references to the specified value across the project.
    /// </summary>
    /// <param name="value">The object to find references for. Can implement <see cref="IFindReferenceScope"/> to customize the search scope.</param>
    /// <returns>True if references were found and logged; otherwise, false.</returns>
    public static bool FindReference(object value)
    {
        Guid id = ResolveId(value);

        if (id == Guid.Empty)
        {
            DialogUtility.ShowMessageBoxAsyncL("Cannot find references to this object.");

            return false;
        }

        try
        {
            HandleFindReference(id, value);

            return true;
        }
        catch (Exception err)
        {
            err.LogError(L("Error finding references."));

            return false;
        }
    }

    /// <summary>
    /// Internal handler that finds references to the specified ID and logs the results.
    /// Supports scoped reference finding and including child asset IDs.
    /// </summary>
    /// <param name="id">The GUID to find references for.</param>
    /// <param name="value">The original value used for display and scoping.</param>
    private static void HandleFindReference(Guid id, object value)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        List<SyncPathReportItem> results = [];

        if (value is IFindReferenceScope scope)
        {
            results.AddRange(ReferenceManager.Current.FindReference(id).Where(scope.IsInScope));
            if (scope.IncludeChildAssets)
            {
                foreach (var childId in GetChildAssetIds(id))
                {
                    results.AddRange(ReferenceManager.Current.FindReference(childId).Where(scope.IsInScope));
                }
            }
        }
        else
        {
            results.AddRange(ReferenceManager.Current.FindReference(id));
        }

        var resultList = GroupReportItems(results);

        EditorUtility.ClearLogView();

        Logs.LogInfo(new ObjectLogCoreItem(L("Find References") + ": " + EditorUtility.GetBriefStringL(value), value));

        Asset asset = AssetManager.Instance.GetAsset(id);
        if (asset != null && asset.MultipleFullTypeNames?.Count > 1)
        {
            var nameGroup = new ReportList(L("Named Share"));

            foreach (var m in asset.MultipleFullTypeNames.Values)
            {
                nameGroup.List.Add(new ObjectLogCoreItem(L("Named Share") + ": " + m.AssetKey, m));
            }

            Logs.LogInfo(nameGroup);
        }

        foreach (var result in resultList)
        {
            Logs.LogInfo(result);
        }

        EditorUtility.ShowLogView();
    }

    /// <summary>
    /// Redirects all references from an old ID to a new ID and logs the results.
    /// </summary>
    /// <param name="oldId">The old GUID that references should be redirected from.</param>
    /// <param name="newId">The new GUID that references should be redirected to.</param>
    public static void RedirectReference(Guid oldId, Guid newId)
    {
        if (oldId == Guid.Empty)
        {
            return;
        }

        if (oldId == newId)
        {
            return;
        }

        List<SyncPathReportItem> results = [.. ReferenceManager.Current.RedirectReference(oldId, newId)];

        var resultList = GroupReportItems(results);

        EditorUtility.ClearLogView();

        //Logs.LogInfo(new ObjectLogItem($"Find reference : {EditorUtility.GetDisplayString(value)}", value));

        foreach (var result in resultList)
        {
            Logs.LogInfo(result);
        }

        EditorUtility.ShowLogView();
    }

    /// <summary>
    /// Groups and sorts sync path report items by owner and path for display.
    /// </summary>
    /// <param name="results">The list of report items to group.</param>
    /// <returns>An enumerable of grouped report lists.</returns>
    private static IEnumerable<ReportList> GroupReportItems(List<SyncPathReportItem> results)
    {
        results.Sort((a, b) =>
        {
            int compare1 = string.Compare(a.Owner?.ToString(), b.Owner?.ToString());
            if (compare1 != 0)
            {
                return compare1;
            }

            if (a.Path != null)
            {
                return a.Path.CompareTo(b.Path);
            }
            else
            {
                return 1;
            }
        });

        Dictionary<object, ReportList> reportDic = [];
        foreach (SyncPathReportItem result in results)
        {
            if (result.Owner is null)
            {
                continue;
            }

            var reportList = reportDic.GetOrAdd(result.Owner, o => new ReportList(o));
            reportList.List.Add(result);
        }

        return reportDic.Values;
    }

    /// <summary>
    /// Recursively retrieves all child asset IDs from a group asset.
    /// </summary>
    /// <param name="id">The GUID of the asset to get child IDs for.</param>
    /// <returns>An enumerable of child asset GUIDs.</returns>
    private static IEnumerable<Guid> GetChildAssetIds(Guid id)
    {
        if (AssetManager.Instance.GetAsset(id) is GroupAsset groupAsset)
        {
            foreach (var childAsset in groupAsset.ChildAssets)
            {
                yield return childAsset.Id;
            }

            foreach (var childAsset in groupAsset.ChildAssets.OfType<GroupAsset>())
            {
                foreach (var childAsset2 in GetChildAssetIds(childAsset.Id))
                {
                    yield return childAsset2;
                }
            }
        }
        else
        {
            yield break;
        }
    }

    /// <summary>
    /// Finds and logs all implementations of the specified value.
    /// </summary>
    /// <param name="value">The object to find implementations for.</param>
    /// <returns>True if implementations were found and logged; otherwise, false.</returns>
    public static bool FindImplement(object value)
    {
        Guid id = ResolveId(value);

        if (id == Guid.Empty)
        {
            DialogUtility.ShowMessageBoxAsyncL("Cannot find implementation of this object.");
            return false;
        }

        HandleFindImplement(id, value);

        return true;
    }

    /// <summary>
    /// Internal handler that finds implementations of the specified ID and logs the results.
    /// </summary>
    /// <param name="id">The GUID to find implementations for.</param>
    /// <param name="value">The original value used for display.</param>
    private static void HandleFindImplement(Guid id, object value)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        var results = DTypeManager.Instance.GetStructsByBaseType(id);

        var list = results.Assets.Select(o => new EditorObjectNaviItem(o)).ToList();
        list.Sort((a, b) =>
        {
            return string.Compare(a.Target.FullName, b.Target.FullName);
        });

        EditorUtility.ClearLogView();

        Logs.LogInfo(new ObjectLogCoreItem(L("Find Implementation") + ": " + EditorUtility.GetBriefStringL(value), value));

        foreach (var result in list)
        {
            Logs.LogInfo(result);
        }

        EditorUtility.ShowLogView();
    }

    //public static bool HandleFind(string findStr, FindOption findOption)
    //{
    //    var project = EditorUtility.CurrentProject;

    //    if (project is null)
    //    {
    //        return false;
    //    }
    //    if (string.IsNullOrEmpty(findStr))
    //    {
    //        return false;
    //    }

    //    var results = project.Library.Find(findStr, findOption);

    //    AppService.Instance.DispatchEvent(typeof(EditorEnvironment), new CommonNotifyEvent(CommonNotifyEvent.ClearLog));
    //    Logs.LogInfo($"Find : {findStr}");

    //    foreach (SyncPathReportItem result in results)
    //    {
    //        Logs.LogInfo(result);
    //    }

    //    EditorUtility.ShowLogView();

    //    return true;
    //}

    //public static void HandleValidate()
    //{
    //    var project = EditorUtility.CurrentProject;

    //    if (project is null)
    //    {
    //        return;
    //    }

    //    EditorUtility.ClearLog();

    //    SyncPathReportItem[] items = project.Library.ValidateAssets().ToArray();

    //    if (items.Length > 0)
    //    {
    //        foreach (SyncPathReportItem result in items)
    //        {
    //            Logs.LogError(result);
    //        }
    //        EditorUtility.ShowLogView();
    //    }
    //    else
    //    {
    //        Logs.LogInfo("Validation passed");
    //    }

    //}


    /// <summary>
    /// A selection item that wraps an editor object for navigation purposes.
    /// </summary>
    private class EditorObjectNaviItem : ISelectionItem, INavigable
    {
        /// <summary>
        /// The target editor object.
        /// </summary>
        public EditorObject Target { get; }

        /// <inheritdoc/>
        public string SelectionKey => Target.FullName;

        /// <inheritdoc/>
        public string DisplayText => Target.ToString();

        /// <summary>
        /// The icon for the target asset, if available.
        /// </summary>
        public object Icon => (Target as Asset)?.Icon;

        /// <summary>
        /// Creates a new navigation item for the specified editor object.
        /// </summary>
        /// <param name="target">The editor object to wrap.</param>
        public EditorObjectNaviItem(EditorObject target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }


        /// <inheritdoc/>
        public object GetNavigationTarget() => Target;

        /// <inheritdoc/>
        public override string ToString() => Target.ToString();
    }

    /// <summary>
    /// A selection item that wraps an editor object with an index for conflict resolution navigation.
    /// </summary>
    private class ConflictNaviItem : ISelectionItem, INavigable
    {
        /// <summary>
        /// The index of this item in the conflict list.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The target editor object.
        /// </summary>
        public EditorObject Target { get; }

        /// <inheritdoc/>
        public string SelectionKey => $"{Target.FullName}-{Index}";

        /// <inheritdoc/>
        public string DisplayText => Target.ToString();

        /// <summary>
        /// The icon for the target asset, if available.
        /// </summary>
        public object Icon => (Target as Asset)?.Icon;

        /// <summary>
        /// Creates a new conflict navigation item.
        /// </summary>
        /// <param name="index">The index of this item in the conflict list.</param>
        /// <param name="target">The editor object to wrap.</param>
        public ConflictNaviItem(int index, EditorObject target)
        {
            Index = index;
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <inheritdoc/>
        public object GetNavigationTarget() => Target;

        /// <inheritdoc/>
        public override string ToString() => Target.ToString();
    }
}