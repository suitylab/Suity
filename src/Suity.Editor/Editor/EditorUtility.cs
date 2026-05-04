using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.ProjectGui;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Rex;
using Suity.Rex.VirtualDom;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Gui;
using Suity.Views.Im;
using Suity.Views.Menu;
using Suity.Views.Named;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor;

/// <summary>
/// Provides a collection of utility methods, properties, and events for interacting with the editor environment,
/// including project management, dialogs, selection, file operations, UI services, and other editor-related
/// functionality.
/// </summary>
/// <remarks>The EditorUtility class exposes static members to facilitate common editor operations such as
/// invoking actions on the main thread, managing dialogs, handling project and document views, clipboard operations,
/// asset and icon retrieval, and more. It serves as a central access point for editor services and events, streamlining
/// integration with the editor's UI and project system. All members are thread-safe only if explicitly documented;
/// callers should ensure thread safety when interacting with UI or project state. This class is not intended to be
/// instantiated.</remarks>
public static class EditorUtility
{
    /// <summary>
    /// Gets the thread that was designated as the application's main thread.
    /// </summary>
    /// <remarks>The main thread is typically the thread that starts the application and may be used for
    /// operations that require execution on the primary thread, such as UI updates in single-threaded environments. The
    /// value is set internally and should not be modified by user code.</remarks>
    public static Thread MainThread { get; internal set; }

    /// <summary>
    /// Invokes the specified action on the main thread. If called from the main thread, the action is executed
    /// immediately; otherwise, it is scheduled to run on the main thread.
    /// </summary>
    /// <remarks>Use this method to ensure that code interacting with thread-affine resources, such as UI
    /// components, runs on the main thread. This is especially important in applications where certain operations must
    /// be performed on the main thread to avoid threading issues.</remarks>
    /// <param name="action">The action to execute on the main thread. Cannot be null.</param>
    public static void InvokeInMainThread(Action action)
    {
        if (Thread.CurrentThread == MainThread)
        {
            action();
        }
        else
        {
            QueuedAction.Do(action);
        }
    }

    /// <summary>
    /// Moves the specified file to the system Recycle Bin instead of permanently deleting it.
    /// </summary>
    /// <remarks>The file is not permanently deleted and can be restored from the Recycle Bin using standard
    /// system tools. If the file does not exist, no action is taken.</remarks>
    /// <param name="fileName">The full path of the file to move to the Recycle Bin. Cannot be null or an empty string.</param>
    public static void SendToRecycleBin(string fileName)
    {
        EditorRexes.SendToRecycleBin.Invoke(fileName);
    }

    #region Startup

    /// <summary>
    /// Gets the event that is raised when the editor initializes or awakens.
    /// </summary>
    public static IRexEvent EditorAwake { get; } = new RexActionWrapperEvent(EditorRexes.EditorAwake);

    /// <summary>
    /// Gets the event that is raised when the editor starts.
    /// </summary>
    public static IRexEvent EditorStart { get; } = new RexActionWrapperEvent(EditorRexes.EditorStart);

    /// <summary>
    /// Gets the event that is triggered when the editor enters or exits pause mode.
    /// </summary>
    public static IRexEvent EditorPause { get; } = new RexActionWrapperEvent(EditorRexes.EditorPause);

    /// <summary>
    /// Gets the event that is raised when the editor resumes from a paused state.
    /// </summary>
    public static IRexEvent EditorResume { get; } = new RexActionWrapperEvent(EditorRexes.EditorResume);

    /// <summary>
    /// Gets the event that is raised when a project is opened.
    /// </summary>
    /// <remarks>Subscribe to this event to be notified whenever a project is opened in the application. Event
    /// handlers receive the opened project as an argument.</remarks>
    public static IRexEvent<Project> ProjectOpened { get; } = new RexActionWrapperEvent<Project>(EditorRexes.ProjectOpened);

    /// <summary>
    /// Gets an event that is raised when a project is about to close.
    /// </summary>
    /// <remarks>Subscribers can use this event to perform cleanup or save state before the project is closed.
    /// Handlers are invoked prior to the project being fully closed.</remarks>
    public static IRexEvent<Project> ProjectClosing { get; } = new RexActionWrapperEvent<Project>(EditorRexes.ProjectClosing);

    /// <summary>
    /// Gets the currently active document in the editor, if any.
    /// </summary>
    public static IRexValue<DocumentEntry> ActiveDocument { get; } = new RexPropertyWrapperValue<DocumentEntry>(EditorRexes.ActiveDocument);

    /// <summary>
    /// Gets the language associated with the current context or resource.
    /// </summary>
    public static IRexValue<string> Language { get; } = new RexPropertyWrapperValue<string>(EditorRexes.Language);

    #endregion

    #region Project

    /// <summary>
    /// Converts an absolute file path to a path relative to the current project's base directory.
    /// </summary>
    /// <remarks>If no project is currently loaded, the method returns the input path unchanged. This method
    /// is typically used to display or store file paths in a project-relative format.</remarks>
    /// <param name="fullPath">The absolute file path to convert. This value should be a fully qualified path.</param>
    /// <returns>A path relative to the current project's base directory if a project is loaded; otherwise, returns the original
    /// absolute path.</returns>
    public static string MakeProjectRelativePath(this string fullPath)
    {
        var project = Project.Current;
        if (project != null)
        {
            return fullPath.MakeRalativePath(project.ProjectBasePath);
        }
        else
        {
            return fullPath;
        }
    }

    /// <summary>
    /// Refresh the project view.
    /// </summary>
    public static void RefreshProjectView()
    {
        EditorRexes.RefreshProjectView.Invoke();
    }

    #endregion

    #region AppConfig

    /// <summary>
    /// Gets the application configuration for the current context.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetAppSetting(string name)
    {
        return EditorServices.AppConfig?.GetSetting(name);
    }

    #endregion

    #region Icon

    /// <summary>
    /// Retrieves an icon image by using the specified asset key.
    /// </summary>
    /// <param name="assetKey">The key of the asset to retrieve the icon for.</param>
    /// <returns>An Image object representing the icon, or null if not found.</returns>
    public static ImageDef GetIconByAssetKey(string assetKey)
    {
        return EditorServices.IconService?.GetIconById(GlobalIdResolver.Resolve(assetKey));
    }

    /// <summary>
    /// Retrieves an icon image by using the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the icon to retrieve.</param>
    /// <returns>An Image object representing the icon, or null if not found.</returns>
    public static ImageDef GetIconById(Guid id)
    {
        return EditorServices.IconService?.GetIconById(id);
    }

    /// <summary>
    /// Retrieves an exact icon image for the specified file path.
    /// </summary>
    /// <param name="path">The file path to retrieve the icon for.</param>
    /// <returns>An Image object representing the file icon, or null if not found.</returns>
    public static ImageDef GetIconForFileExact(string path)
    {
        return EditorServices.IconService?.GetIconForFileExact(path);
    }

    /// <summary>
    /// Retrieves an icon image for the specified file path.
    /// </summary>
    /// <param name="path">The file path to retrieve the icon for.</param>
    /// <returns>An Image object representing the file icon, or null if not found.</returns>
    public static ImageDef GetIconForFile(string path)
    {
        return EditorServices.IconService?.GetIconForFile(path);
    }

    /// <summary>
    /// Retrieves an icon based on the provided icon object.
    /// The method handles various types of icon objects including direct Image references,
    /// Assets, GUIDs, objects with IDs, and strings that can be resolved to IDs.
    /// </summary>
    /// <param name="iconObject">The object from which to retrieve the icon. Can be of various types.</param>
    /// <returns>
    /// An Image representing the icon if found; otherwise, null if:
    /// - The input object is null
    /// - The object type is not supported
    /// - No icon can be resolved from the provided object
    /// </returns>
    public static ImageDef GetIcon(object iconObject)
    {
        if (iconObject is null)
        {
            return null;
        }

        if (iconObject is ImageDef image)
        {
            return image;
        }

        if (iconObject is Asset asset)
        {
            return asset.Icon;
        }
        else if (iconObject is Guid id)
        {
            return GetIconById(id);
        }
        else if (iconObject is IHasId idContext)
        {
            return GetIcon(AssetManager.Instance.GetAsset(idContext.Id));
        }
        else if (iconObject is string s)
        {
            if (GlobalIdResolver.TryResolve(s, out id))
            {
                return GetIconById(id);
            }
        }

        return null;
    }

    #endregion

    #region Inspector

    private static readonly ServiceStore<IInspector> _inspector = new();

    /// <summary>
    /// Retrieves the inspector service.
    /// </summary>
    public static IInspector Inspector => _inspector.Get(EmptyInspector.Empty);

    #endregion

    #region RunDelayed

    /// <summary>
    /// Adds a delayed action to the execution queue.
    /// This method allows scheduling actions to be executed after a specified delay.
    /// </summary>
    /// <param name="action">The DelayedAction to be added to the queue</param>
    public static void AddDelayedAction(DelayedAction action)
    {
        var runDelayed = EditorServices.RunDelayed;
        if (runDelayed != null)
        {
            runDelayed.AddAction(action);
        }
        else
        {
            //QueuedAction.Do(() => action.DoAction());
        }
    }

    /// <summary>
    /// Removes a delayed action from the execution queue.
    /// This method prevents a previously scheduled action from being executed.
    /// </summary>
    /// <param name="action">The DelayedAction to be removed from the queue</param>
    public static void RemoveDelayedAction(DelayedAction action)
    {
        EditorServices.RunDelayed?.RemoveAction(action);
    }

    /// <summary>
    /// Processes all pending delayed actions immediately.
    /// This method forces the execution of all actions that were scheduled with a delay.
    /// </summary>
    public static void FlushDelayedActions()
    {
        EditorServices.RunDelayed?.ProccessActions();
    }

    /// <summary>
    /// Waits for the next queued action to be processed.
    /// </summary>
    /// <returns>A Task that completes when the next queued action is processed.</returns>
    public static Task WaitForNextQueuedAction()
    {
        TaskCompletionSource<bool> tcs = new();

        QueuedAction.Do(() => 
        {
            tcs.SetResult(true);
        });

        return tcs.Task;
    }

    #endregion

    #region Progress

    

    /// <summary>
    /// Executes a progress operation with the specified title and progress action.
    /// </summary>
    /// <param name="title">The title to display for the progress operation.</param>
    /// <param name="progressAction">The action to execute for progress reporting.</param>
    /// <param name="finishedAction">Optional action to execute when the progress operation completes.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task DoProgress(string title, Action<IProgress> progressAction, Action finishedAction = null)
    {
        var request = new ProgressRequest
        {
            Title = title,
            ProgressAction = progressAction,
            FinishedAction = finishedAction,
        };

        return EditorServices.ProgressService?.DoProgress(request);
    }

    /// <summary>
    /// Executes a progress operation using the provided ProgressRequest.
    /// </summary>
    /// <param name="request">The ProgressRequest containing all necessary information for the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    public static Task DoProgress(ProgressRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return EditorServices.ProgressService?.DoProgress(request);
    }

    /// <summary>
    /// Executes multiple progress operations using the provided array of ProgressRequests.
    /// </summary>
    /// <param name="requests">An array of ProgressRequests containing all necessary information for the operations.</param>
    /// <returns>An array of Tasks representing the asynchronous operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the requests array is null.</exception>
    public static Task[] DoProgress(ProgressRequest[] requests)
    {
        if (requests is null)
        {
            throw new ArgumentNullException(nameof(requests));
        }

        return EditorServices.ProgressService?.DoProgress(requests);
    }

    /// <summary>
    /// Gets a value indicating whether a progress operation is currently running.
    /// </summary>
    public static bool ProgressRunning => EditorServices.ProgressService?.ProgressRunning == true;

    /// <summary>
    /// Checks if any build or progress operations are currently running.
    /// </summary>
    /// <returns>True if no operations are running, false otherwise.</returns>
    public static bool CheckBuildStatus()
    {
        if (ProgressRunning)
        {
            Logs.LogError(L("BACKEND_PROCESSING"));

            return false;
        }

        if (BuildTaskRunning)
        {
            Logs.LogError(L("BACKEND_PROCESSING"));

            return false;
        }

        return true;
    }

    #endregion

    #region FileUnwatchedAction

    /// <summary>
    /// Executes the specified action when no files are being watched.
    /// This is a static method that delegates the execution to FileUnwatchedAction class.
    /// </summary>
    /// <param name="action">The action to be executed when no files are being watched.</param>
    public static void DoFileUnwatchedAction(Action action)
    {
        FileUnwatchedAction.Do(action);
    }

    /// <summary>
    /// Gets a value indicating whether files are currently being watched.
    /// This property provides a convenient way to check the file watching status.
    /// </summary>
    /// <value>
    /// true if files are being watched; otherwise, false.
    /// Returns the negation of FileUnwatchedAction.IsUnwatched.
    /// </value>
    public static bool IsFileWatching => !FileUnwatchedAction.IsUnwatched;


    #endregion

    #region ProjectView & Definition & Reference


    /// <summary>
    /// Navigates to the definition of the specified value.
    /// </summary>
    /// <param name="value">The object whose definition should be found.</param>
    public static void GotoDefinition(object value)
    {
        // InternalRexes.GotoDefinition.Invoke(value);

        if (!LocateInProject(value))
        {
            GotoSource(value);
        }
    }

    public static void GotoSource(object value)
    {
        EditorRexes.GotoDefinition.Invoke(value);
    }

    /// <summary>
    /// Finds all references to the specified value.
    /// </summary>
    /// <param name="value">The object whose references should be found.</param>
    public static void FindReference(object value)
    {
        EditorRexes.FindReference.Invoke(value);
    }

    /// <summary>
    /// Finds implementations of the specified value.
    /// </summary>
    /// <param name="value">The object whose implementations should be found.</param>
    public static void FindImplement(object value)
    {
        EditorRexes.FindImplement.Invoke(value);
    }

    /// <summary>
    /// Navigates to the specified target.
    /// </summary>
    /// <param name="target">The target object to navigate to.</param>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    public static bool NavigateTo(object target)
    {
        var vo = new NavigateVReq { Target = target };
        EditorCommands.Mapper.Handle(vo);

        return vo.Successful;
    }

    /// <summary>
    /// Locates a file in the project by its name.
    /// </summary>
    /// <param name="fileName">The name of the file to locate.</param>
    /// <returns>True if the file was located successfully; otherwise, false.</returns>
    public static bool LocateInProject(string fileName, object item = null)
    {
        var vo = new LocateInProjectVReq
        {
            FileName = fileName,
            Item = item,
        };

        EditorCommands.Mapper.Handle(vo);

        return vo.Successful;
    }

    /// <summary>
    /// Locates a project view node in the project.
    /// </summary>
    /// <param name="viewNode">The view node to locate.</param>
    /// <returns>True if the view node was located successfully; otherwise, false.</returns>
    public static bool LocateInProject(IProjectViewNode viewNode)
    {
        var vo = new LocateProjectNodeVReq
        {
            ViewNode = viewNode,
        };

        EditorCommands.Mapper.Handle(vo);

        return vo.Successful;
    }

    /// <summary>
    /// Locates an object in the project. This method handles various types of objects
    /// including GUIDs, objects with IDs, view nodes, file names, and navigable objects.
    /// </summary>
    /// <param name="obj">The object to locate in the project.</param>
    /// <returns>True if the object was located successfully; otherwise, false.</returns>
    public static bool LocateInProject(object obj)
    {
        if (obj is Guid guid)
        {
            return LocateInProject(guid);
        }
        else if (obj is INavigable navi)
        {
            return LocateInProject(navi.GetNavigationTarget());
        }
        else if (obj is IHasId context)
        {
            return LocateInProject(context.Id);
        }
        else if (obj is IProjectViewNode viewNode)
        {
            return LocateInProject(viewNode);
        }
        else if (obj is DocumentEntry docEntry)
        {
            return LocateInProject(docEntry.FileName.PhysicFileName);
        }
        else if (obj is Document doc && doc.Entry is { } docEntry2)
        {
            return LocateInProject(docEntry2.FileName.PhysicFileName);
        }
        else if (obj is string fileName)
        {
            return LocateInProject(fileName);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Locates an object in the project by its ID.
    /// </summary>
    /// <param name="id">The ID of the object to locate.</param>
    /// <param name="autoGetParent">Whether to automatically get the parent object.</param>
    /// <returns>True if the object was located successfully; otherwise, false.</returns>
    public static bool LocateInProject(Guid id, bool autoGetParent = true)
    {
        var obj = EditorObjectManager.Instance.GetObject(id);

        StorageLocation fileName = autoGetParent ? obj?.GetStorageLocation() : (obj as Asset)?.FileName;

        if (fileName?.PhysicFileName != null)
        {
            return LocateInProject(fileName.PhysicFileName, obj);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Locates an object in the project by its ID, or navigates to its definition if not found.
    /// </summary>
    /// <param name="id">The ID of the object to locate.</param>
    public static void LocateInProjectOrDefinition(Guid id)
    {
        if (!LocateInProject(id, false))
        {
            EditorRexes.GotoDefinition.Invoke(id);
        }
    }

    /// <summary>
    /// Locates a workspace using the provided request parameters.
    /// </summary>
    /// <param name="req">The request object containing workspace location parameters.</param>
    /// <returns>True if the workspace was successfully located; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request object is null.</exception>
    public static bool LocateWorkSpace(LocateWorkSpaceVReq req)
    {
        if (req is null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        if (req.WorkSpace is null)
        {
            return false;
        }

        EditorCommands.Mapper.Handle(req);

        return req.Successful;
    }

    /// <summary>
    /// Locates a workspace using the provided workspace object.
    /// </summary>
    /// <param name="workSpace">The workspace object to locate.</param>
    /// <returns>True if the workspace was successfully located; otherwise, false.</returns>
    public static bool LocateWorkSpace(WorkSpace workSpace)
    {
        var req = new LocateWorkSpaceVReq
        {
            WorkSpace = workSpace,
        };

        EditorCommands.Mapper.Handle(req);

        return req.Successful;
    }

    /// <summary>
    /// Locates a workspace using the provided workspace object and relative file name.
    /// </summary>
    /// <param name="workSpace">The workspace object to locate.</param>
    /// <param name="relativeFileName">The relative file name to locate within the workspace.</param>
    /// <returns>True if the workspace was successfully located; otherwise, false.</returns>
    public static bool LocateWorkSpace(WorkSpace workSpace, string relativeFileName)
    {
        var req = new LocateWorkSpaceVReq
        {
            WorkSpace = workSpace,
            RelativeFileName = relativeFileName,
        };

        EditorCommands.Mapper.Handle(req);

        return req.Successful;
    }

    /// <summary>
    /// Locates an item in the publish view using the provided file name.
    /// </summary>
    /// <param name="fileName">The name of the file to locate in the publish view.</param>
    /// <returns>True if the item was successfully located; otherwise, false.</returns>
    public static bool LocateInPublishView(string fileName)
    {
        EditorCommands.ShowPublishView.Invoke();
        return EditorCommands.Mapper.Handle(new LocateInPublishVReq { FileName = fileName });
    }

    /// <summary>
    /// Locates an item in the canvas using the provided ID.
    /// </summary>
    /// <param name="id">The GUID of the item to locate in the canvas.</param>
    /// <returns>True if the item was successfully located; otherwise, false.</returns>
    public static bool LocateInCanvas(Guid id)
    {
        return EditorCommands.Mapper.Handle(new LocateInCanvasVReq { Id = id });
    }

    /// <summary>
    /// Gets the redirected object for a given view ID, handling potential redirection cycles and depth limits.
    /// </summary>
    /// <param name="obj">The original object to check for redirection.</param>
    /// <param name="viewId">The ID of the view for which to get the redirected object.</param>
    /// <returns>The redirected object or the original object if no redirection is possible.</returns>
    public static object GetViewRedirectedObject(object obj, int viewId)
    {
        if (obj is null)
        {
            return null;
        }

        var current = obj;
        int depth = 0;

        while (current is IViewRedirect viewRedirect)
        {
            current = viewRedirect.GetRedirectedObject(viewId);
            if (ReferenceEquals(current, viewRedirect))
            {
                // Returned itself
                return current;
            }

            if (ReferenceEquals(current, obj))
            {
                // Cycle navigation
                Logs.LogError("IViewRedirect cycle detected.");
                return obj;
            }

            depth++;
            if (depth > 10)
            {
                // Exceeded depth
                Logs.LogError("IViewRedirect depth exceeded.");
                return current;
            }
        }

        return current;
    }

    #endregion

    #region IToolWindowService

    /// <summary>
    /// Retrieves a tool window of the specified type T.
    /// </summary>
    /// <typeparam name="T">The type of tool window to retrieve, which must implement IToolWindow.</typeparam>
    /// <returns>An instance of the tool window of type T, or null if not found.</returns>
    public static T GetToolWindow<T>() where T : class, IToolWindow 
        => EditorServices.ToolWindow?.GetToolWindow(typeof(T)) as T;

    /// <summary>
    /// Retrieves a tool window by its unique identifier.
    /// </summary>
    /// <param name="windowId">The unique identifier of the tool window to retrieve.</param>
    /// <returns>An instance of the tool window with the specified ID, or null if not found.</returns>
    public static IToolWindow GetToolWindow(string windowId)
        => EditorServices.ToolWindow?.GetToolWindow(windowId);

    /// <summary>
    /// Creates a new view object window with the specified UI object, title, and icon.
    /// </summary>
    /// <param name="uiObject">The UI object to be displayed in the window.</param>
    /// <param name="title">The title of the window.</param>
    /// <param name="icon">The icon to be displayed in the window.</param>
    /// <param name="menu">Optional root menu command for the window.</param>
    /// <returns>A window handle for the created window, or an empty handle if creation fails.</returns>
    public static IWindowHandle CreateViewObjectWindow(object uiObject, string title, ImageDef icon, RootMenuCommand menu = null)
    {
        var window = new SimpleViewObjectWindow(uiObject, title, icon, menu);
        return EditorServices.ToolWindow?.CreateViewObjectWindow(window) ?? EmptyWindowHandle.Empty;
    }

    /// <summary>
    /// Creates a new view object window using an existing IViewObjectWindow instance.
    /// </summary>
    /// <param name="window">The view object window instance to create.</param>
    /// <returns>A window handle for the created window, or an empty handle if the input is null or creation fails.</returns>
    public static IWindowHandle CreateViewObjectWindow(IViewObjectWindow window)
    {
        if (window is null)
        {
            return EmptyWindowHandle.Empty;
        }

        return EditorServices.ToolWindow?.CreateViewObjectWindow(window) ?? EmptyWindowHandle.Empty;
    }

    #endregion

    #region FileAsset

    /// <summary>
    /// Retrieves a file asset by the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file to retrieve the asset for.</param>
    /// <returns>The Asset object associated with the given file name, or null if the current FileAssetManager is not available.</returns>
    public static Asset GetFileAsset(string fileName)
    {
        return FileAssetManager.Current?.GetAsset(fileName);
    }

    /// <summary>
    /// Creates an asset key for the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file to create an asset key for.</param>
    /// <returns>The asset key string for the given file name, or null if the current FileAssetManager is not available.</returns>
    public static string MakeFileAssetKey(string fileName)
    {
        return FileAssetManager.Current?.MakeAssetKey(fileName);
    }

    internal static Asset GetOrUpdateFileAsset(string fileName)
    {
        return FileAssetManager.Current?.GetOrUpdateAsset(fileName);
    }

    internal static Asset LockedResolveId(this AssetBuilder builder, IdResolveType resolveType = IdResolveType.Auto)
    {
        lock (builder)
        {
            return builder.ResolveId(resolveType);
        }
    }

    internal static IReferenceHost GetReferenceHost(StorageLocation location)
    {
        return FileAssetManager.Current?.GetReferenceHost(location.PhysicFileName);
    }

    internal static IReferenceHost EnsureReferenceHost(StorageLocation location)
    {
        return FileAssetManager.Current?.EnsureReferenceHost(location.PhysicFileName);
    }

    internal static IReferenceHost GetReferenceHost(string fileName)
    {
        return FileAssetManager.Current?.GetReferenceHost(fileName);
    }

    internal static IReferenceHost EnsureReferenceHost(string fileName)
    {
        return FileAssetManager.Current?.EnsureReferenceHost(fileName);
    }

    internal static void RemoveReferenceHost(string fileName)
    {
        FileAssetManager.Current?.RemoveReferenceHost(fileName);
    }

    #endregion

    #region Storage

    /// <summary>
    /// Extension method to retrieve an Asset from a StorageLocation
    /// </summary>
    /// <param name="location">The StorageLocation to retrieve the asset from</param>
    /// <returns>The retrieved Asset, or null if not found</returns>
    public static Asset GetAsset(this StorageLocation location)
    {
        if (location.PhysicFileName != null)
        {
            return FileAssetManager.Current?.GetAsset(location.PhysicFileName);
        }
        else if (location.StorageType != null)
        {
            return StorageManager.Current.GetProvider(location.StorageType)?.GetAsset(location.Location);
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Document

    /// <summary>
    /// Opens a file and returns its document view
    /// </summary>
    /// <param name="fileName">The name of the file to open</param>
    /// <returns>IDocumentView instance if successful, null otherwise</returns>
    public static IDocumentView OpenFile(string fileName)
    {
        var doc = DocumentManager.Instance.OpenDocument(fileName);
        if (doc?.ShowView() is IDocumentView view)
        {
            return view;
        }

        TextFileHelper.NavigateFile(fileName);

        return null;
    }

    /// <summary>
    /// Creates a new document with specified format and default name
    /// </summary>
    /// <param name="format">The format of the new document</param>
    /// <param name="defaultName">The default name for the new document</param>
    /// <returns>DocumentEntry instance representing the new document</returns>
    public static DocumentEntry AutoNewDocument(this DocumentFormat format, string defaultName)
    {
        string basePath = EditorServices.CurrentProject.AssetDirectory;
        string canvasFileName = EditorServices.FileNameService.GetIncrementalFileName(basePath, defaultName, format.Extension);
        string fullName = Path.Combine(basePath, canvasFileName);

        DocumentEntry docEntry = DocumentManager.Instance.NewDocument(fullName, format);

        return docEntry;
    }

    #endregion

    #region Views

    /// <summary>
    /// Displays an error message with the provided message and exception details.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="exception">The exception that occurred.</param>
    public static void ShowError(string message, Exception exception)
    {
        EditorRexes.ShowError.Invoke(message, exception);
    }

    /// <summary>
    /// Shows a tool window with the specified IToolWindow object.
    /// </summary>
    /// <param name="toolWindow">The tool window to show.</param>
    /// <param name="focus">Whether to focus the window when shown. Default is true.</param>
    public static void ShowToolWindow(IToolWindow toolWindow, bool focus = true)
    {
        if (toolWindow is null)
        {
            throw new ArgumentNullException(nameof(toolWindow));
        }

        EditorRexes.ShowToolWindow.Invoke(toolWindow.WindowId, focus);
    }

    /// <summary>
    /// Shows a tool window with the specified name.
    /// </summary>
    /// <param name="name">The name of the tool window to show.</param>
    /// <param name="focus">Whether to focus the window when shown. Default is true.</param>
    public static void ShowToolWindow(string name, bool focus = true)
    {
        EditorRexes.ShowToolWindow.Invoke(name, focus);
    }

    /// <summary>
    /// Closes a tool window using the specified IToolWindow object.
    /// </summary>
    /// <param name="toolWindow">The tool window to close.</param>
    public static void CloseToolWindow(IToolWindow toolWindow)
    {
        if (toolWindow is null)
        {
            throw new ArgumentNullException(nameof(toolWindow));
        }

        EditorRexes.CloseToolWindow.Invoke(toolWindow.WindowId);
    }

    /// <summary>
    /// Closes a tool window using the specified name.
    /// </summary>
    /// <param name="name">The name of the tool window to close.</param>
    public static void CloseToolWindow(string name)
    {
        EditorRexes.CloseToolWindow.Invoke(name);
    }

    /// <summary>
    /// Displays text in a window with the specified title.
    /// </summary>
    /// <param name="title">The title of the window.</param>
    /// <param name="text">The text content to display.</param>
    public static void ShowText(string title, string text)
    {
        EditorRexes.ShowText.Invoke(title, text);
    }

    /// <summary>
    /// Clears the log view in the editor.
    /// </summary>
    public static void ClearLogView()
    {
        EditorCommands.ClearLog.Invoke();
    }

    /// <summary>
    /// Shows the log view in the editor.
    /// </summary>
    public static void ShowLogView()
    {
        EditorCommands.ShowLogView.Invoke();
    }

    /// <summary>
    /// Shows the publish view in the editor.
    /// </summary>
    public static void ShowPublishView()
    {
        EditorCommands.ShowPublishView.Invoke();
    }

    /// <summary>
    /// Shows the chat view in the editor.
    /// </summary>
    public static void ShowChatView()
    {
        EditorCommands.ShowChatView.Invoke();
    }


    /// <summary>
    /// Gets or sets a value indicating whether to show as description.
    /// </summary>
    public static RexValue<bool> ShowAsDescription { get; } = new RexValue<bool>(true);

    /// <summary>
    /// Gets a brief string representation of the specified value.
    /// </summary>
    /// <param name="value">The value to convert to a brief string.</param>
    /// <returns>A brief string representation of the value.</returns>
    public static string GetBriefString(object value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        try
        {
            switch (value)
            {
                case SObject sobj:
                    return sobj.ToBriefString();

                case ITextDisplay textDisplay:
                    return textDisplay.DisplayText;

                case Guid id:
                    {
                        var obj = EditorObjectManager.Instance.GetObject(id);
                        if (obj != null)
                        {
                            return GetBriefString(obj);
                        }

                        string s = GlobalIdResolver.RevertResolve(id);
                        if (s != null)
                        {
                            return s;
                        }
                        else
                        {
                            return id.ToString();
                        }
                    }
                case SyncPath path:
                    return path.ToString(id => GetBriefString(id));

                case ISyncPathObject syncPathObject:
                    return GetBriefString(syncPathObject.GetPath());

                default:
                    {
                        var type = value.GetType();
                        if (type == typeof(string) || type == typeof(bool) || type.GetIsNumeric())
                        {
                            return value.ToString();
                        }
                    }
                    
                    //return value.ToString();
                    return string.Empty;
            }
        }
        catch (Exception)
        {
            return $"?ERROR:{value.GetType().FullName}?";
        }
    }

    /// <summary>
    /// Gets a localized brief string representation of the specified value.
    /// </summary>
    /// <param name="value">The value to convert to a localized brief string.</param>
    /// <returns>A localized brief string representation of the value.</returns>
    public static string GetBriefStringL(object value) => L(GetBriefString(value));

    /// <summary>
    /// Shows the project settings window.
    /// </summary>
    public static void ShowProjectSetting() => EditorRexes.ShowProjectSetting.Invoke();

    #endregion

    #region Path & NameSpace

    /// <summary>
    /// Retrieves the namespace of the provided object based on its type
    /// </summary>
    /// <param name="value">The object to get the namespace from</param>
    /// <returns>The namespace of the object, or an empty string if not found</returns>
    public static string GetNameSpace(object value)
    {
        string nameSpace = null;
        switch (value)
        {
            case Asset asset:
                nameSpace = asset.NameSpace;
                break;

            case Document document:
                nameSpace = document.Entry?.GetNameSpace();
                break;

            case DocumentEntry documentEntry:
                nameSpace = documentEntry.GetNameSpace();
                break;

            case TypeDefinition typeDef:
                nameSpace = typeDef.GetNameSpace();
                break;

            case IHasId hasId:
                nameSpace = AssetManager.Instance.GetAsset(hasId.Id)?.NameSpace;
                break;

            default:
                break;
        }

        nameSpace ??= string.Empty;

        return nameSpace.Replace('/', '.').Trim('.', ':', '*');
    }

    /// <summary>
    /// Converts a namespace string to a path format
    /// </summary>
    /// <param name="nameSpace">The namespace string to convert</param>
    /// <returns>The converted path string</returns>
    public static string NameSpaceToPath(string nameSpace)
    {
        if (string.IsNullOrEmpty(nameSpace))
        {
            return string.Empty;
        }

        if (nameSpace.StartsWith("*"))
        {
            nameSpace = nameSpace.TrimStart('*');
        }

        nameSpace = nameSpace.Replace('.', '/');

        return nameSpace;
    }

    /// <summary>
    /// Converts a path to a namespace format
    /// </summary>
    /// <param name="path">The path to convert</param>
    /// <returns>The converted namespace string</returns>
    public static string PathToNameSpace(string path)
    {
        string nameSpace = path;
        nameSpace = nameSpace.Replace('\\', '.');
        nameSpace = nameSpace.Replace('/', '.');

        return nameSpace;
    }

    /// <summary>
    /// Combines two namespace strings
    /// </summary>
    /// <param name="baseNameSpace">The base namespace</param>
    /// <param name="nameSpace">The namespace to append</param>
    /// <returns>The combined namespace string</returns>
    public static string CombineNameSpace(string baseNameSpace, string nameSpace)
    {
        nameSpace = (nameSpace ?? string.Empty).Trim('*');
        baseNameSpace = (baseNameSpace ?? string.Empty).Trim('*');

        if (string.IsNullOrEmpty(baseNameSpace))
        {
            return nameSpace;
        }

        if (nameSpace.StartsWith("."))
        {
            if (!string.IsNullOrEmpty(baseNameSpace))
            {
                return $"{baseNameSpace.Trim('.')}.{nameSpace.Trim('.')}";
            }
            else
            {
                return nameSpace.Trim('.');
            }
        }
        else
        {
            return nameSpace.Trim('.');
        }
    }

    /// <summary>
    /// Combines a namespace and a name
    /// </summary>
    /// <param name="nameSpace">The namespace</param>
    /// <param name="name">The name to append</param>
    /// <returns>The combined namespace and name</returns>
    public static string CombineName(string nameSpace, string name)
    {
        name = (name ?? string.Empty).Trim('*', ',').GetNameTerminal();
        nameSpace = (nameSpace ?? string.Empty).Trim('*', '.');

        if (string.IsNullOrEmpty(nameSpace))
        {
            return name;
        }

        if (!string.IsNullOrEmpty(nameSpace))
        {
            return $"{nameSpace}.{name}";
        }
        else
        {
            return name;
        }
    }

    /// <summary>
    /// Extracts the terminal part (after last dot) of a namespace or name
    /// </summary>
    /// <param name="name">The string to extract from</param>
    /// <returns>The terminal part of the string</returns>
    public static string GetNameTerminal(this string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        int index = name.LastIndexOf('.');
        if (index >= 0)
        {
            return name.Substring(index + 1, name.Length - index - 1);
        }
        else
        {
            return name;
        }
    }

    /// <summary>
    /// Normalize asset name by replacing backslashes with forward slashes
    /// </summary>
    /// <param name="assetName">Asset name to normalize</param>
    /// <returns>Returns the normalized asset name</returns>
    public static string FixSlash(string assetName)
    {
        assetName ??= string.Empty;
        // Convert to lowercase, convert to backslash
        return assetName.Replace('\\', '/');
    }

    #endregion

    #region FileBunch


    /// <summary>
    /// Creates or updates a file bunch with the specified files
    /// </summary>
    /// <param name="bunchFiles">Collection of FileBunchUpdate objects to process</param>
    /// <param name="bunchFileFullName">Full name of the bunch file</param>
    /// <returns>True if operation succeeded, false otherwise</returns>
    public static bool CreateOrUpdate(IEnumerable<FileBunchUpdate> bunchFiles, string bunchFileFullName)
    {
        return EditorServices.FileBunchService?.CreateOrUpdate(bunchFiles, bunchFileFullName) == true;
    }

    /// <summary>
    /// Creates or updates a file bunch with the specified files
    /// </summary>
    /// <param name="bunchFiles">Collection of FileBunchUpdate objects to process</param>
    /// <param name="bunchFileFullName">Full name of the bunch file</param>
    /// <returns>True if operation succeeded, false otherwise</returns>
    public static void Download(string bunchFileFullName, string targetDirectory, IEnumerable<FileBunchReplace> replaces)
    {
        EditorServices.FileBunchService?.Download(bunchFileFullName, targetDirectory, replaces);
    }

    /// <summary>
    /// Shrinks a file bunch to optimize its size
    /// </summary>
    /// <param name="bunchFileFullName">Full name of the bunch file to shrink</param>
    public static void Shrink(string bunchFileFullName)
    {
        EditorServices.FileBunchService?.Shrink(bunchFileFullName);
    }

    /// <summary>
    /// Extension method to create a file bunch file ID from a full path
    /// </summary>
    /// <param name="workSpace">Current workspace instance</param>
    /// <param name="fullName">Full path of the file</param>
    /// <returns>Relative path ID for the file bunch</returns>
    public static string MakeFileBunchFileId(this WorkSpace workSpace, string fullName)
    {
        return fullName.MakeRalativePath(workSpace.MasterDirectory);
    }

    /// <summary>
    /// Extension method to get a file bunch by its ID from the workspace
    /// </summary>
    /// <param name="workSpace">Current workspace instance</param>
    /// <param name="fileBunchId">ID of the file bunch to retrieve</param>
    /// <returns>FileBunch instance if found, null otherwise</returns>
    public static IFileBunch GetFileBunch(this WorkSpace workSpace, string fileBunchId)
    {
        return workSpace.GetRenderTargets(fileBunchId).FirstOrDefault()?.FileBunch;
    }

    #endregion

    #region Clipboard

    /// <summary>
    /// Sets the system clipboard text to the specified value.
    /// </summary>
    /// <param name="text">The text to be set to the clipboard. If null, an empty string will be used.</param>
    public static Task<bool> SetSystemClipboardText(string text)
    {
        if (EditorServices.SystemClipboard is { } clipboard)
        {
            return clipboard.SetText(text ?? string.Empty);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Retrieves the current text content from the system clipboard.
    /// </summary>
    /// <returns>The text content from the clipboard, or null if the clipboard is empty or inaccessible.</returns>
    public static Task<string> GetSystemClipboardText()
    {
        if (EditorServices.SystemClipboard is { } clipboard)
        {
            return clipboard.GetText();
        }

        return Task.FromResult<string>(null);
    }

    #endregion

    #region Menu

    /// <summary>
    /// Prepares the menu by using the IMenuService to set up menu commands
    /// </summary>
    /// <param name="menu">The root menu command to be prepared</param>
    public static void PrepareMenu(RootMenuCommand menu)
    {
        Device.Current.GetService<IMenuService>()?.PrepareMenu(menu);
    }

    #endregion

    #region ImGuiService

    /// <summary>
    /// Creates an ImGui dialog with the specified parameters.
    /// </summary>
    /// <param name="drawImGui">The interface for drawing ImGui content</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="width">The width of the dialog</param>
    /// <param name="height">The height of the dialog</param>
    /// <param name="isDialog">Flag indicating whether to create as a dialog (default: true)</param>
    /// <returns>An object representing the created ImGui dialog</returns>
    public static Task CreateImGuiDialog(this IDrawImGui drawImGui, string title, int width, int height, bool isDialog = true, bool fixedSize = false) 
        => EditorServices.ImGuiService.CreateImGuiDialog(drawImGui, new DialogOptions { Title = title, Width = width, Height = height, IsDialog = isDialog, FixedSize = fixedSize });

    public static Task CreateImGuiDialog(this IDrawImGui drawImGui, DialogOptions option)
        => EditorServices.ImGuiService.CreateImGuiDialog(drawImGui, option);

    /// <summary>
    /// Creates an ImGui dialog with the specified parameters, wrapping the DrawImGui implementation.
    /// </summary>
    /// <param name="drawImGui">The implementation for drawing ImGui content</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="width">The width of the dialog</param>
    /// <param name="height">The height of the dialog</param>
    /// <param name="isDialog">Flag indicating whether to create as a dialog (default: true)</param>
    /// <returns>An object representing the created ImGui dialog</returns>
    public static Task CreateImGuiDialog(this DrawImGui drawImGui, string title, int width, int height, bool isDialog = true, bool fixedSize = false) 
        => EditorServices.ImGuiService.CreateImGuiDialog(new DrawImguiWrapper(drawImGui), new DialogOptions { Title = title, Width = width, Height = height, IsDialog = isDialog, FixedSize = fixedSize });

    public static Task CreateImGuiDialog(this DrawImGui drawImGui, DialogOptions option)
        => EditorServices.ImGuiService.CreateImGuiDialog(new DrawImguiWrapper(drawImGui), option);

    /// <summary>
    /// Creates an ImGui control with the specified drawing interface.
    /// </summary>
    /// <param name="drawImGui">The interface for drawing ImGui content</param>
    /// <returns>An object representing the created ImGui control</returns>
    public static object CreateImGuiControl(this IDrawImGui drawImGui) 
        => EditorServices.ImGuiService.CreateImGuiControl(drawImGui);

    /// <summary>
    /// Creates an ImGui control with the specified drawing implementation, wrapping it.
    /// </summary>
    /// <param name="drawImGui">The implementation for drawing ImGui content</param>
    /// <returns>An object representing the created ImGui control</returns>
    public static object CreateImGuiControl(this DrawImGui drawImGui) 
        => EditorServices.ImGuiService.CreateImGuiControl(new DrawImguiWrapper(drawImGui));

    public static IUndoableViewObjectImGui CreateSimpleTreeImGui(string menuName = null)
    {
        var option = new HeaderlessTreeOptions
        {
            MenuName = menuName,
        };

        return EditorServices.ImGuiService.CreateSimpleTreeImGui(option);
    }

    public static IUndoableViewObjectImGui CreateSimpleTreeImGui(HeaderlessTreeOptions option)
        => EditorServices.ImGuiService.CreateSimpleTreeImGui(option);

    /// <summary>
    /// Creates a view object ImGui with an optional menu name.
    /// </summary>
    /// <param name="menuName">Optional name for the menu (default: null)</param>
    /// <returns>An IUndoableViewObjectImGui instance</returns>
    public static IUndoableViewObjectImGui CreateColumnTreeImGui(string menuName = null)
    {
        var option = new ColumnTreeOptions
        {
            MenuName = menuName,
        };

        return EditorServices.ImGuiService.CreateColumnTreeImGui(option);
    }

    public static IUndoableViewObjectImGui CreateColumnTreeImGui(ColumnTreeOptions option)
        => EditorServices.ImGuiService.CreateColumnTreeImGui(option);

    /// <summary>
    /// Gets the editor's ImGui theme, with an option to preview.
    /// </summary>
    /// <param name="preview">Flag indicating whether to get the preview theme (default: false)</param>
    /// <returns>An ImGuiTheme instance representing the current or preview theme</returns>
    public static ImGuiTheme GetEditorImGuiTheme(bool preview = false) 
        => EditorServices.ImGuiService.GetEditorTheme(preview);

    #endregion

    #region Packaging

    /// <summary>
    /// Displays the export package dialog with specified files and workspaces.
    /// </summary>
    /// <param name="files">An enumerable collection of file paths to be included in the package.</param>
    /// <param name="workSpaces">An enumerable collection of workspace paths to be included in the package.</param>
    /// <param name="onComplete">Optional callback action to be executed when the export operation completes.</param>
    public static void ShowExportPackage(IEnumerable<string> files, IEnumerable<string> workSpaces, Action onComplete = null)
    {
        Device.Current.GetService<IPackageExport>()?.ShowExportPackage(files, workSpaces, onComplete);
    }

    /// <summary>
    /// Displays the import package GUI dialog for importing a package.
    /// </summary>
    /// <param name="fileName">The path to the package file to be imported.</param>
    /// <param name="packageFullName">Optional full name of the package.</param>
    /// <param name="onComplete">Optional callback action to be executed when the import operation completes.</param>
    public static void ShowImportPackage(string fileName, string packageFullName = null, Action onComplete = null)
    {
        Device.Current.GetService<IPackageImport>()?.ShowImportPackageGui(fileName, packageFullName, onComplete);
    }

    /// <summary>
    /// Imports a package programmatically without showing the GUI.
    /// </summary>
    /// <param name="fileName">The path to the package file to be imported.</param>
    /// <param name="packageFullName">Optional full name of the package.</param>
    /// <param name="onComplete">Optional callback action to be executed when the import operation completes.</param>
    public static Task ImportPackage(string fileName, string packageFullName = null)
    {
        if (Device.Current.GetService<IPackageImport>() is { } importer)
        {
            return importer.ImportPackage(fileName, packageFullName);
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    #endregion

    #region Build task

    /// <summary>
    /// Invokes the solution compilation command
    /// </summary>
    public static void CompileSolution()
    {
        EditorCommands.CompileSolution?.Invoke();
    }

    private static Task<bool> _buildTask;

    /// <summary>
    /// Gets a value indicating whether a build task is currently running
    /// </summary>
    public static bool BuildTaskRunning => _buildTask != null;

    /// <summary>
    /// Starts a new build task in the background
    /// </summary>
    /// <param name="action">The action to execute as part of the build</param>
    /// <param name="clearLog">Whether to clear the log view before starting the build</param>
    /// <returns>A Task representing the build operation</returns>
    public static Task<bool> StartBuildTask(Action action, bool clearLog = true)
    {
        if (_buildTask != null)
        {
            DialogUtility.ShowMessageBoxAsyncL("There is currently a build task in progress.");
            return Task.FromResult(false);
        }

        if (ProgressRunning)
        {
            DialogUtility.ShowMessageBoxAsyncL("There is currently background progress being processed.");
            return Task.FromResult(false);
        }

        _buildTask = Task.Run<bool>(() =>
        {
            try
            {
                ShowLogView();
                if (clearLog)
                {
                    ClearLogView();
                }

                action?.Invoke();
            }
            catch (Exception err)
            {
                HandleException(err);

                return false;
            }
            finally
            {
                _buildTask = null;
            }

            return true;
        });

        return _buildTask;
    }

    private static void HandleException(Exception exception)
    {
        //if (exception is ExecuteException exe)
        //{
        //    string msg;

        //    if (EnumHelper.TryParseEnumValue(exe.StatusCode, out var v))
        //    {
        //        msg = L("Start failed :") + $"{exe.Message}({v.ToDisplayText()})";
        //    }
        //    else
        //    {
        //        msg = L("Start failed :") + $"{exe.Message}({exe.StatusCode})";
        //    }

        //    DialogUtility.ShowMessageBoxAsync(msg);
        //}
        //else 
        if (exception is AggregateException aggr && aggr.InnerExceptions.Count > 0)
        {
            HandleException(aggr.InnerExceptions[0]);
        }
        else
        {
            exception.LogError();
        }
    }

    #endregion

    #region To

    class TypeDisplayInfo(string displayText, ImageDef icon, string previewText, string toolTips)
    {
        public string DisplayText { get; } = displayText;
        public ImageDef Icon { get; } = icon;
        public string PreviewText { get; } = previewText;
        public string ToolTips { get; } = toolTips;
    }

    readonly static ConcurrentDictionary<Type, TypeDisplayInfo> _staticDisplayInfo = new();
    private static TypeDisplayInfo GetTypeDisplayInfo(Type type)
    {
        if (type is null)
        {
            return null;
        }

        return _staticDisplayInfo.GetOrAdd(type, t =>
        {
            string displayText = null;
            ImageDef icon = null;
            string previewText = null;
            string toolTipsText = null;

            do
            {
                var display = t.GetAttributeCached<DisplayTextAttribute>();
                if (display != null)
                {
                    displayText ??= display.DisplayText;
                    icon ??= GetIcon(display.Icon);
                }

                var preview = t.GetAttributeCached<PreviewTextAttribute>();
                if (preview != null)
                {
                    previewText ??= preview.PreviewText;
                }

                var toolTips = t.GetAttributeCached<ToolTipsTextAttribute>();
                if (toolTips != null)
                {
                    toolTipsText ??= toolTips.ToolTips;
                }

                var native = type.GetAttributeCached<NativeTypeAttribute>();
                if (native != null)
                {
                    displayText ??= native.Description;
                    icon ??= GetIcon(native.Icon);
                }

                var bind = type.GetAttributeCached<AssetTypeBindingAttribute>();
                if (bind != null)
                {
                    displayText ??= bind.Description;
                }

                var format = type.GetAttributeCached<DocumentFormatAttribute>();
                if (format != null)
                {
                    displayText ??= format.DisplayText;
                    icon ??= GetIcon(format.Icon);
                }
            } while (false);

            if (displayText is null && icon is null && previewText is null && toolTipsText is null)
            {
                return null;
            }

            if (icon is null && t.BaseType is { } baseType)
            {
                // Get the Icon of the base class
                icon = baseType.ToDisplayIcon();
            }

            return new TypeDisplayInfo(displayText, icon, previewText, toolTipsText);
        });
    }

    /// <summary>
    /// Converts an object to its name representation.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The name of the object if it implements INamed or Suity.Object, otherwise null.</returns>
    public static string ToName(this object obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is INamed n)
        {
            return n.Name;
        }

        if (obj is Suity.Object o)
        {
            return o.Name;
        }

        return null;
    }

    /// <summary>
    /// Gets the display text of an enum value based on its DisplayTextAttribute.
    /// </summary>
    /// <param name="value">The enum value to process.</param>
    /// <returns>The display text from attribute or the enum string value if no attribute is present.</returns>
    public static string ToDisplayText(this Enum value)
    {
        Type enumType = value.GetType();
        FieldInfo fieldInfo = enumType.GetField(value.ToString());

        var attr = fieldInfo?.GetAttributeCached<DisplayTextAttribute>();

        return attr?.DisplayText ?? value.ToString();
    }

    /// <summary>
    /// Gets the category of an enum value based on its DisplayCategoryAttribute.
    /// </summary>
    /// <param name="value">The enum value to process.</param>
    /// <returns>The category from attribute or an empty string if no attribute is present.</returns>
    public static string GetCategory(this Enum value)
    {
        Type enumType = value.GetType();
        FieldInfo fieldInfo = enumType.GetField(value.ToString());

        var attr = fieldInfo?.GetAttributeCached<DisplayCategoryAttribute>();

        return attr?.Category ?? string.Empty;
    }

    /// <summary>
    /// Gets the preview text of an enum value based on its PreviewTextAttribute.
    /// </summary>
    /// <param name="value">The enum value to process.</param>
    /// <returns>The preview text from attribute or null if no attribute is present.</returns>
    public static string ToPreviewText(this Enum value)
    {
        Type enumType = value.GetType();
        FieldInfo fieldInfo = enumType.GetField(value.ToString());

        var attr = fieldInfo?.GetAttributeCached<PreviewTextAttribute>();

        return attr?.PreviewText;
    }

    /// <summary>
    /// Gets the tooltip text of an enum value based on its ToolTipsTextAttribute.
    /// </summary>
    /// <param name="value">The enum value to process.</param>
    /// <returns>The tooltip text from attribute or null if no attribute is present.</returns>
    public static string ToToolTipsText(this Enum value)
    {
        Type enumType = value.GetType();
        FieldInfo fieldInfo = enumType.GetField(value.ToString());

        var attr = fieldInfo?.GetAttributeCached<ToolTipsTextAttribute>();

        return attr?.ToolTips;
    }

    /// <summary>
    /// Converts an object to its display text representation.
    /// Handles different types of objects including null, ITextDisplay, EditorObject, and Suity.Object.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The display text of the object, or an empty string if no suitable representation is found.</returns>
    public static string ToDisplayText(this object obj)
    {
        if (obj is null)
        {
            return string.Empty;
        }

        if (obj is ITextDisplay display)
        {
            return display.DisplayText ?? string.Empty;
        }

        if (obj is EditorObject editorObj)
        {
            return editorObj.DisplayText ?? string.Empty;
        }

        if (ToDisplayText(obj.GetType()) is string s && !string.IsNullOrWhiteSpace(s))
        {
            return s ?? string.Empty;
        }

        return (obj as Suity.Object)?.Name ?? obj.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the localized display text of an enum value.
    /// </summary>
    /// <param name="value">The enum value to process.</param>
    /// <returns>The localized display text.</returns>
    public static string ToDisplayTextL(this Enum value)
    {
        var text = ToDisplayText(value);

        return GlobalLocalizer.L(text);
    }

    /// <summary>
    /// Gets the localized display text of an object.
    /// </summary>
    /// <param name="obj">The object to process.</param>
    /// <returns>The localized display text.</returns>
    public static string ToDisplayTextL(this object obj)
    {
        var text = ToDisplayText(obj);

        return GlobalLocalizer.L(text);
    }

    /// <summary>
    /// Gets the localized display text of an object with a default fallback text.
    /// </summary>
    /// <param name="obj">The object to process.</param>
    /// <param name="defaultText">The default text to return if no suitable display text is found.</param>
    /// <returns>The localized display text or the default text.</returns>
    public static string ToDisplayTextL(this object obj, string defaultText)
    {
        if (obj is null)
        {
            return defaultText;
        }

        if (obj is ITextDisplay display && !string.IsNullOrWhiteSpace(display.DisplayText))
        {
            return L(display.DisplayText);
        }

        if (obj is EditorObject editorObj && !string.IsNullOrWhiteSpace(editorObj.DisplayText))
        {
            return L(editorObj.DisplayText);
        }

        return defaultText;
    }

    /// <summary>
    /// Converts an object to a display icon.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>An Image representing the object's icon, or null if the object is null.</returns>
    public static ImageDef ToDisplayIcon(this object obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is ITextDisplay textDisplay)
        {
            return GetIcon(textDisplay.DisplayIcon);
        }

        return ToDisplayIcon(obj.GetType());
    }

    /// <summary>
    /// Converts an object to preview text.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>A string representing the object's preview text, or null if the object is null.</returns>
    public static string ToPreviewText(this object obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is IPreviewDisplay preview)
        {
            return preview.PreviewText;
        }

        return ToPreviewText(obj.GetType());
    }

    /// <summary>
    /// Converts an object to tooltip text.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>A string representing the object's tooltip text, or null if the object is null.</returns>
    public static string ToToolTipsText(this object obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is IHasToolTips hasToolTips)
        {
            return hasToolTips.ToolTips;
        }

        if (obj is SObject sobj)
        {
            return sobj.ToToolTipsText();
        }

        if (obj is IAttributeGetter hasAttr)
        {
            return hasAttr.GetAttribute<ToolTipsTextAttribute>()?.ToolTips;
        }

        return ToToolTipsText(obj.GetType());
    }

    /// <summary>
    /// Converts an object to localized tooltip text.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>A localized string representing the object's tooltip text.</returns>
    public static string ToToolTipsTextL(this object obj) => L(obj.ToToolTipsText());

    /// <summary>
    /// Converts an SObject to tooltip text.
    /// </summary>
    /// <param name="sobj">The SObject to convert.</param>
    /// <returns>A string representing the SObject's tooltip text, or null if the SObject is null.</returns>
    public static string ToToolTipsText(this SObject sobj)
    {
        if (sobj is null)
        {
            return null;
        }

        if (sobj.Controller is { } controller)
        {
            return controller.ToToolTipsText();
        }

        if (sobj.ObjectType?.Target is { } dtype)
        {
            return dtype.ToolTips;
        }

        return null;
    }

    /// <summary>
    /// Converts an SObject to localized tooltip text.
    /// </summary>
    /// <param name="sobj">The SObject to convert.</param>
    /// <returns>A localized string representing the SObject's tooltip text.</returns>
    public static string ToToolTipsTextL(this SObject sobj) => L(sobj.ToToolTipsText());

    /// <summary>
    /// Extension method to get the display text of a Type
    /// </summary>
    /// <param name="type">The Type to get display text for</param>
    /// <returns>The display text of the Type, or null if no display info is available</returns>
    public static string ToDisplayText(this Type type)
    {
        return GetTypeDisplayInfo(type)?.DisplayText;
    }

    /// <summary>
    /// Extension method to get the localized display text of a Type
    /// </summary>
    /// <param name="type">The Type to get localized display text for</param>
    /// <returns>The localized display text of the Type</returns>
    public static string ToDisplayTextL(this Type type) => L(type.ToDisplayText());

    /// <summary>
    /// Extension method to get the display icon of a Type
    /// </summary>
    /// <param name="type">The Type to get display icon for</param>
    /// <returns>The display icon of the Type, or null if no display info is available</returns>
    public static ImageDef ToDisplayIcon(this Type type)
    {
        return GetTypeDisplayInfo(type)?.Icon;
    }

    /// <summary>
    /// Extension method to get the preview text of a Type
    /// </summary>
    /// <param name="type">The Type to get preview text for</param>
    /// <returns>The preview text of the Type, or null if no display info is available</returns>
    public static string ToPreviewText(this Type type)
    {
        var info = GetTypeDisplayInfo(type);

        return info?.PreviewText;
    }

    /// <summary>
    /// Extension method to get the tooltip text of a Type
    /// </summary>
    /// <param name="type">The Type to get tooltip text for</param>
    /// <returns>The tooltip text of the Type, or null if no display info is available</returns>
    public static string ToToolTipsText(this Type type)
    {
        var info = GetTypeDisplayInfo(type);

        return info?.ToolTips;
    }

    /// <summary>
    /// Extension method to convert TextStatus to a Color object
    /// </summary>
    /// <param name="status">The TextStatus to convert</param>
    /// <returns>A Color object representing the status</returns>
    public static Color ToColor(this TextStatus status)
    {
        return EditorServices.ColorConfig.GetStatusColor(status);
    }

    /// <summary>
    /// Extension method to convert various object types to their DataId representation
    /// </summary>
    /// <param name="obj">The object to convert</param>
    /// <returns>A string representing the DataId of the object</returns>
    public static string ToDataId(this object obj)
    {
        switch (obj)
        {
            case Asset asset:
                return asset.ToDataId();

            case IDataItem row:
                return row.ToDataId();

            case SKey key:
                return SItemExtensions.ToDataId(key);

            case SAssetKey key:
                return SItemExtensions.ToDataId(key);

            case Enum value:
                return EnumHelper.ToDataId(value);

            case IHasId idContext:
                return ToDataId(EditorObjectManager.Instance.GetObject(idContext.Id));

            case Guid id:
                return ToDataId(EditorObjectManager.Instance.GetObject(id));

            default:
                return null;
        }
    }

    /// <summary>
    /// Extension method to convert IDataItem to its DataId representation
    /// </summary>
    /// <param name="row">The IDataItem to convert</param>
    /// <returns>A string representing the DataId of the IDataItem</returns>
    public static string ToDataId(this IDataItem row)
    {
        if (row.DataContainer is { } table)
        {
            return $"{table.TableId}.{row.DataLocalId}";
        }
        else
        {
            return row.DataLocalId;
        }
    }

    /// <summary>
    /// Extension method to convert SKey to its DataId representation
    /// </summary>
    /// <param name="key">The SKey to convert</param>
    /// <returns>A string representing the DataId of the SKey</returns>
    public static string ToDataId(this SKey key) => SItemExtensions.ToDataId(key);

    /// <summary>
    /// Extension method to convert SAssetKey to its DataId representation
    /// </summary>
    /// <param name="key">The SAssetKey to convert</param>
    /// <returns>A string representing the DataId of the SAssetKey</returns>
    public static string ToDataId(this SAssetKey key) => SItemExtensions.ToDataId(key);

    /// <summary>
    /// Extension method to convert Enum to its DataId representation
    /// </summary>
    /// <param name="value">The Enum to convert</param>
    /// <returns>A string representing the DataId of the Enum</returns>
    public static string ToDataId(this Enum value) => EnumHelper.ToDataId(value);

    /// <summary>
    /// Extension method to create a shortened version of a string with ellipsis
    /// </summary>
    /// <param name="s">The string to shorten</param>
    /// <param name="length">Maximum length of the resulting string (default: 50)</param>
    /// <returns>A shortened version of the input string</returns>
    public static string ToShortcutString(this string s, int length = 50)
    {
        if (s.Length > length)
        {
            s = s[..length] + "...";
        }

        return s;
    }

    public static ImageDef ToStatusIcon(this TextStatus status) => status switch
    {
        TextStatus.Info => CoreIconCache.LogInfo,
        TextStatus.Warning => CoreIconCache.Warning,
        TextStatus.Error => CoreIconCache.Error,
        TextStatus.Comment => CoreIconCache.Comment,
        TextStatus.Anonymous => null,
        TextStatus.Disabled => null,
        TextStatus.Import => CoreIconCache.Import,
        TextStatus.Tag => CoreIconCache.Tag,
        TextStatus.UserCode => null,
        TextStatus.ResourceUse => null,
        TextStatus.Reference => CoreIconCache.Reference,
        TextStatus.FileReference => CoreIconCache.Reference,
        TextStatus.EnumReference => CoreIconCache.Reference,
        TextStatus.Add => CoreIconCache.New,
        TextStatus.Remove => CoreIconCache.Disable,
        TextStatus.Modify => CoreIconCache.Receive,
        TextStatus.Denied => CoreIconCache.Denied,
        TextStatus.Checked => CoreIconCache.Check,
        TextStatus.Unchecked => CoreIconCache.Uncheck,
        _ => null,
    };

    public static Color? ToNativeColor(this Type type)
    {
        var attr = type?.GetAttributeCached<NativeTypeAttribute>();
        if (attr != null && ColorHelper.TryParseHtmlColor(attr.Color, out Color color))
        {
            return color;
        }

        return null;
    }

    /// <summary>
    /// Extension method to convert TextStatus to LogMessageType
    /// </summary>
    /// <param name="status">The TextStatus to convert</param>
    /// <returns>The corresponding LogMessageType based on the input TextStatus</returns>
    public static LogMessageType ToLogMessageType(this TextStatus status) => status switch
    {
        TextStatus.Normal or TextStatus.Reference or TextStatus.FileReference or TextStatus.EnumReference or TextStatus.Add or TextStatus.Remove or TextStatus.Modify or TextStatus.Disabled or TextStatus.Comment or TextStatus.Anonymous or TextStatus.Import => LogMessageType.Info,
        TextStatus.Info => LogMessageType.Info,
        TextStatus.Warning => LogMessageType.Warning,
        TextStatus.Error => LogMessageType.Error,
        _ => LogMessageType.Info,
    };


    #endregion

    #region Browser

    /// <summary>
    /// Opens the specified URL in the default web browser.
    /// Attempts to use shell execute first, then falls back to Windows Explorer if that fails.
    /// Silently handles exceptions and logs the URL if both methods fail.
    /// </summary>
    /// <param name="url">The URL to open. If null, empty, or whitespace, the method returns without action.</param>
    public static void OpenBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        Process result1 = null;
        try
        {
            result1 = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // Important: Ensure that NET Core or Used in NET 5+
            });
        }
        catch (Exception)
        {
        }

        if (result1 is null)
        {
            try
            {
                result1 = Process.Start("explorer.exe", url);
            }
            catch (Exception)
            {
            }
        }

        if (result1 is null)
        {
            Logs.LogInfo(url);
        }
    }


    #endregion

    #region Selection

    [Obsolete]
    public static SelectionResult ShowSelectionGUI(this ISelectionList list, string title, SelectionOption option = null)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public static MultipleSelectionResult ShowMultipleSelectionGUI(this ISelectionList list, string title, SelectionOption option = null)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public static bool ShowSelectionGUI(this ISelection selection, string title, SelectionOption option = null)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public static T ShowAssetSelectionGUI<T>(string title, SelectionOption option = null) where T : class
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public static T ShowAssetSelectionGUI<T>(string title, IAssetFilter filter, SelectionOption option = null) where T : class
    {
        throw new NotImplementedException();
    }

    #endregion
}