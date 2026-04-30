using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using Suity;
using Suity.Collections;
using Suity.Controls;
using Suity.Editor;
using Suity.Editor.MenuCommands;
using Suity.Editor.Services;
using Suity.Editor.WinformGui;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Controls;

public class DockToolFactory
{
    public required IToolWindow Tool { get; init; }

    public required Func<Control> ViewFactory { get; init; }

    public Func<object>? ViewObjectFactory { get; init; }

    public string? Title { get; init; }

    /// <summary>
    /// This can be geometry data, DrawingImage, Geometry, System.Drawing.Image, etc. The specific type is determined by the implementer.
    /// </summary>
    public object? IconStream { get; init; }

    public DockMode? DockHint { get; init; }
}


public class EditorDockContainer : UserControl
{
    private readonly DockControl _dockControl;
    private readonly Factory _factory;
    private readonly IDockSerializer _serializer;
    private readonly IDockState _dockState;

    // Factory mapping
    private readonly Dictionary<IToolWindow, DockToolFactory> _viewFactories = [];
    private readonly Dictionary<string, DockToolFactory> _toolsById = [];

    private readonly Dictionary<DockMode, string> _dockMapping = new()
    {
        { DockMode.Left, "LeftPane" },
        { DockMode.Right, "PropertiesPane" },
        { DockMode.Bottom, "BottomPane" },
        { DockMode.Center, "DocumentsPane" } // Note: Center usually corresponds to DocumentDock
    };

    private readonly Dictionary<IToolWindow, EditorToolContent> _toolCache = [];
    private readonly Dictionary<IToolWindow, EditorDocumentContent> _centerToolCache = [];
    private readonly Dictionary<Documents.DocumentEntry, EditorDocumentContent> _documentCache = [];

    bool _loaded;
    bool _layoutLoaded;

    private EditorDocumentContent? _lastActiveDocument;

    public event EventHandler? ConfigLayout;

    public EditorDockContainer()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        // 1. Initialize serializer and state manager (consistent with the provided example)
        _serializer = new DockSerializer(typeof(AvaloniaList<>));
        _dockState = new DockState();

        // 2. Initialize Factory and DockControl
        _factory = new Factory
        {
            HideToolsOnClose = false,
            HideDocumentsOnClose = false
        };

        // Subscribe to global focus changes
        _factory.FocusedDockableChanged += (s, e) =>
        {
            // e is the newly focused IDockable
            if (e.Dockable is EditorDocumentDockable docDockable)
            {
                _lastActiveDocument = docDockable.EditorContent;

                if (_loaded && ViewPlugin.Instance.AutoLocateInProject && _lastActiveDocument?.Document?.Content is IViewLocateInProject p)
                {
                    EditorUtility.LocateInProject(p);
                }
            }
            // If a tool is focused, we do nothing, thereby preserving the reference to the last document
        };

        _dockControl = new DockControl
        {
            Factory = _factory,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        Content = _dockControl;


        // 1. Create style: for Document tabs
        var docTabStyle = new Style(x => x.OfType<DocumentTabStripItem>());

        // 2. Wrap ContextMenu with FuncTemplate
        // This way each Tab gets its own menu instance
        docTabStyle.Setters.Add(new Setter(
            DocumentTabStripItem.DocumentContextMenuProperty,
            new FuncTemplate<ContextMenu>(CreateDocumentTabMenu)
        ));
        _dockControl.Styles.Add(docTabStyle);

        var toolTabStyle = new Style(x => x.OfType<ToolTabStripItem>());
        toolTabStyle.Setters.Add(new Setter(
            ToolTabStripItem.TabContextMenuProperty,
            new FuncTemplate<ContextMenu>(CreateToolTabMenu)
        ));
        _dockControl.Styles.Add(toolTabStyle);

        var toolControlStyle = new Style(x => x.OfType<ToolTabStrip>());
        toolControlStyle.Setters.Add(new Setter(
            ToolControl.ContextMenuProperty,
            new FuncTemplate<ContextMenu>(CreateToolControlMenu) // Reuse the method for creating Tool menus
        ));
        _dockControl.Styles.Add(toolControlStyle);

    }

    public string? LayoutConfigFilName { get; set; }

    public Factory DockFactory => _factory;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        LayoutProcess();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _loaded = true;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _loaded = false;
    }

    #region Tool

    /// <summary>
    /// Register tool panel factory (must be called before LoadLayout)
    /// </summary>
    public void RegisterTool(IToolWindow tool, Func<Control> viewFactory, Func<object>? vmFactory = null, string? title = null, object? iconStream = null, DockMode? dockHint = null)
    {
        var factory = new DockToolFactory
        {
            Tool = tool,
            ViewFactory = viewFactory,
            ViewObjectFactory = vmFactory,
            Title = title,
            IconStream = iconStream,
            DockHint = dockHint,
        };

        _viewFactories[tool] = factory;
        _toolsById[tool.WindowId] = factory;
    }

    public void RegisterTool(IToolWindow toolWindow)
    {
        if (toolWindow.GetUIObject() is Control ctrl)
        {
            RegisterTool(toolWindow, () => ctrl, null, toolWindow.Title, toolWindow.Icon, toolWindow.DockHint.ToDockMode());
        }
        else if (toolWindow is IDrawImGui drawImGui)
        {
            RegisterTool(toolWindow, () => new EditorToolContent(toolWindow), null, toolWindow.Title, toolWindow.Icon, toolWindow.DockHint.ToDockMode());
        }
        else if (toolWindow is IGraphicObject graphicObject)
        {
            RegisterTool(toolWindow, () => new EditorToolContent(toolWindow), null, toolWindow.Title, toolWindow.Icon, toolWindow.DockHint.ToDockMode());
        }
    }

    public void ShowAllTools()
    {
        var layout = _dockControl.Layout;
        if (layout == null) return;

        foreach (var f in _viewFactories.Values)
        {
            ShowTool(f.Tool, f.Title, f.DockHint ?? DockMode.Left, false, true);
        }

        _factory.InitLayout(layout);
    }

    public void ShowTool(string id, bool focus)
    {
        if (_toolsById.TryGetValue(id, out var f))
        {
            ShowTool(f.Tool, f.Title, f.DockHint ?? DockMode.Left, focus, true);
        }
    }

    /// <summary>
    /// Dynamically add a tool to the specified ToolDock
    /// </summary>
    public UserControl? ShowTool(IToolWindow tool, string? title, DockMode mode = DockMode.Left, bool focus = true, bool updateLayout = false)
    {
        var layout = _dockControl.Layout;
        if (layout == null)
        {
            return null;
        }

        // 1. Try to find if a dockable with this ID already exists in the global layout tree
        /*        var existingItem = FindDockable(layout, tool.WindowId);
                if (existingItem != null)
                {
                    // --- Case A: Already exists in layout ---

                    // If it's hidden or inactive, we need to make it visible again
                    if (focus && existingItem.Owner is IDock dockExisting)
                    {
                        // Ensure it's in the visible list (for certain specific hiding mechanisms)
                        // If HideToolsOnClose is true, it's usually still in VisibleDockables but not selected
                        dockExisting.ActiveDockable = existingItem;
                    }

                    if (focus)
                    {
                        _factory.SetActiveDockable(existingItem);
                    }

                    return;
                }*/

        // 1. Get target container ID based on mode
        if (!_dockMapping.TryGetValue(mode, out var targetId))
        {
            targetId = "LeftPane"; // Default fallback
        }

        // 2. Find target container
        var xDock = FindDockable(layout, targetId) as IDock;
        if (xDock is null)
        {
            xDock = CreateAndInsertDockContainer(layout, targetId, mode);
        }
        if (xDock is null)
        {
            return null;
        }

        if (xDock is ToolDock toolDock)
        {
            var toolControl = GetOrCreateToolControl(tool, out var created);
            if (toolControl is null)
            {
                return null;
            }

            if (toolControl.Dockable is not { } dockable)
            {
                dockable = new EditorToolDockable { Id = tool.WindowId, Title = title ?? tool.WindowId };
                toolControl.Dockable = dockable;

                // Add to the Dock system
                toolDock.Add(dockable);

                toolDock.ActiveDockable = dockable;
            }

            toolDock.CanFloat = false;
            toolDock.CanDockAsDocument = false;
            toolDock.CanPin = true;


            if (updateLayout)
            {
                UpdateDockLayout();
            }

            return toolControl;
        }
        else if (xDock is DocumentDock docDock)
        {
            var centerToolControl = GetOrCreateCenterToolControl(tool, out var created);
            if (centerToolControl is null)
            {
                return null;
            }

            if (centerToolControl.Dockable is not { } dockable)
            {
                dockable = new EditorDocumentDockable { Id = tool.WindowId, Title = title ?? tool.WindowId };
                centerToolControl.Dockable = dockable;

                // Add to the Dock system
                docDock.AddDocument(dockable);

                docDock.ActiveDockable = dockable;
            }

            docDock.CanFloat = false;
            docDock.CanDockAsDocument = true;
            docDock.CanPin = false;

            if (updateLayout)
            {
                UpdateDockLayout();
            }

            return centerToolControl;
        }

        return null;
    }

    public bool FocusTool(IToolWindow tool)
    {
        if (_toolCache.TryGetValue(tool, out var toolControl))
        {
            var dockable = toolControl.Dockable;
            if (dockable?.Owner is IDock dock)
            {
                dock.FocusedDockable = dockable;
                dock.ActiveDockable = dockable;
                return true;
            }
        }
        else if (_centerToolCache.TryGetValue(tool, out var centerToolControl))
        {
            var dockable = centerToolControl.Dockable;
            if (dockable?.Owner is IDock dock)
            {
                dock.FocusedDockable = dockable;
                dock.ActiveDockable = dockable;
                return true;
            }
        }

        return false;
    }

    public EditorToolContent? GetOrCreateToolControl(IToolWindow tool, out bool created)
    {
        created = false;

        if (tool is null)
        {
            return null;
        }

        // If already open, focus directly
        if (_toolCache.TryGetValue(tool, out var existingControl))
        {
            FocusTool(tool);
            return existingControl;
        }

        var toolControl = new EditorToolContent(tool);

        // Register to cache
        _toolCache[tool] = toolControl;
        created = true;

        return toolControl;
    }

    public EditorDocumentContent? GetOrCreateCenterToolControl(IToolWindow tool, out bool created)
    {
        created = false;

        if (tool is null)
        {
            return null;
        }

        // If already open, focus directly
        if (_centerToolCache.TryGetValue(tool, out var existingControl))
        {
            FocusTool(tool);
            return existingControl;
        }

        var toolControl = new EditorDocumentContent(tool);

        // Register to cache
        _centerToolCache[tool] = toolControl;
        created = true;

        return toolControl;
    }
    #endregion

    #region Dynamic Dock Creation

    public IDock? CreateAndInsertDockContainer(IDock rootLayout, string id, DockMode mode)
    {
        // 1. Handle RootDock recursion
        if (rootLayout is IRootDock root && root.ActiveDockable is IDock mainDock)
        {
            return CreateAndInsertDockContainer(mainDock, id, mode);
        }

        if (rootLayout is IProportionalDock propDock)
        {
            // 2. Determine target orientation
            var targetOrientation = (mode == DockMode.Left || mode == DockMode.Right)
                ? Orientation.Horizontal
                : Orientation.Vertical;

            // 3. Case A: Orientation matches, insert directly
            if (propDock.VisibleDockables == null || propDock.VisibleDockables.Count == 0 || propDock.Orientation == targetOrientation)
            {
                propDock.Orientation = targetOrientation; // Ensure correct orientation for empty container
                return InsertIntoProportionalDock(propDock, id, mode);
            }

            // 4. Case B: Orientation doesn't match, search inward for compatible child containers
            var compatibleSubDock = propDock.VisibleDockables
                .OfType<IProportionalDock>()
                .FirstOrDefault(x => x.Orientation == targetOrientation);

            if (compatibleSubDock != null)
            {
                return CreateAndInsertDockContainer(compatibleSubDock, id, mode);
            }

            // 5. Case C: Orientation mismatch and no child container found -> Execute "layout degradation/reorganization"
            // Logic: Create a new ProportionalDock and move all content from current propDock into it
            return WrapAndInsert(propDock, id, mode, targetOrientation);
        }

        return null;
    }

    private IDock InsertIntoProportionalDock(IProportionalDock propDock, string id, DockMode mode)
    {
        IDock newContainer = mode == DockMode.Center
            ? new DocumentDock { Id = id, Proportion = 0.2 }
            : new ToolDock { Id = id, Proportion = 0.2 };

        newContainer.CanFloat = false;
        //newContainer.CanDockAsDocument = true;

        var dockables = propDock.VisibleDockables ??= _factory.CreateList<IDockable>();
        bool insertAtFirst = (mode == DockMode.Left || mode == DockMode.Top);

        if (dockables.Count == 0)
        {
            dockables.Add(newContainer);
        }
        else
        {
            if (insertAtFirst)
            {
                dockables.Insert(0, new ProportionalDockSplitter());
                dockables.Insert(0, newContainer);
            }
            else
            {
                dockables.Add(new ProportionalDockSplitter());
                dockables.Add(newContainer);
            }
        }
        return newContainer;
    }

    private IDock WrapAndInsert(IProportionalDock parent, string id, DockMode mode, Orientation newOrientation)
    {
        // 1. Create wrapper container, inheriting the old layout content
        var wrapDock = new ProportionalDock
        {
            Orientation = parent.Orientation,
            VisibleDockables = _factory.CreateList<IDockable>(parent.VisibleDockables?.ToArray() ?? [])
        };

        // 2. Clear parent container, change orientation
        parent.VisibleDockables?.Clear();
        parent.Orientation = newOrientation;

        // 3. Put the wrapped old content and new container back into the parent container
        // The mode determines the order of old and new
        IDock newToolDock = mode == DockMode.Center
            ? new DocumentDock { Id = id, Proportion = 0.2 }
            : new ToolDock { Id = id, Proportion = 0.2 };

        bool isFirst = (mode == DockMode.Left || mode == DockMode.Top);

        if (isFirst)
        {
            parent.VisibleDockables.Add(newToolDock);
            parent.VisibleDockables.Add(new ProportionalDockSplitter());
            parent.VisibleDockables.Add(wrapDock);
        }
        else
        {
            parent.VisibleDockables.Add(wrapDock);
            parent.VisibleDockables.Add(new ProportionalDockSplitter());
            parent.VisibleDockables.Add(newToolDock);
        }

        return newToolDock;
    }

    #endregion

    #region Document

    public IEnumerable<EditorDocumentContent> DocumentControls => _documentCache.Values;

    public IEnumerable<EditorDocumentContent> DirtyDocuments => _documentCache.Values.Where(o => o.Document?.IsDirty == true);

    public IEnumerable<Documents.DocumentEntry> OpenedDocuments => DocumentControls.Select(o => o.Document).SkipNull() ?? [];

    public EditorDocumentContent? GetDocumentControl(Documents.DocumentEntry entry) => _documentCache.GetValueSafe(entry);

    public Documents.IDocumentView? GetDocumentView(Documents.DocumentEntry entry) => GetDocumentControl(entry)?.DocumentView;

    // 1. Implement DocumentEntry? ActiveDocument { get; }
    public Documents.DocumentEntry? ActiveDocument
    {
        get
        {
            if (_lastActiveDocument is { } doc)
            {
                if (doc.Document is { } entry && _documentCache.ContainsKey(entry))
                {
                    return entry;
                }
                else
                {
                    _lastActiveDocument = null;
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    // 2. Implement bool CloseDocument(Documents.DocumentEntry entry)
    public bool CloseDocument(Documents.DocumentEntry entry)
    {
        if (_documentCache.TryGetValue(entry, out var docControl))
        {
            // Call Dockable's Owner (i.e. DocumentDock) to close it
            // This triggers the DocumentDockable.OnClose() logic you wrote earlier
            var dockable = docControl.Dockable;
            dockable?.Owner?.Factory?.CloseDockable(dockable);

            return true;
        }
        return false;
    }

    // 3. Implement bool FocusDocument(Documents.DocumentEntry entry)
    public bool FocusDocument(Documents.DocumentEntry entry)
    {
        if (_documentCache.TryGetValue(entry, out var docControl))
        {
            var dockable = docControl.Dockable;
            if (dockable?.Owner is IDock dock)
            {
                dock.ActiveDockable = dockable;
                dock.FocusedDockable = dockable;
                return true;
            }
        }
        return false;
    }

    // 4. Implement IDocumentView? ShowDocumentView(Documents.DocumentEntry entry)
    public Documents.IDocumentView? ShowDocumentView(Documents.DocumentEntry entry)
    {
        // Get the DocumentsPane container
        var layout = _dockControl.Layout;
        if (layout == null)
        {
            return null;
        }

        if (FindDockable(layout, "DocumentsPane") is not DocumentDock docDock)
        {
            return null;
        }

        var docControl = GetOrCreateDocumentControl(entry, out var created);
        if (docControl is null)
        {
            return null;
        }

        if (docControl.Dockable is not { } dockable)
        {
            dockable = new EditorDocumentDockable();
            docControl.Dockable = dockable;

            // Add to the Dock system
            docDock.AddDocument(dockable);
            docDock.ActiveDockable = dockable;
        }

        docDock.CanFloat = false;
        docDock.CanDockAsDocument = true;
        docDock.CanPin = false;

        // Ensure layout and content are synchronized
        _factory.InitLayout(layout);

        return docControl.DocumentView;
    }

    public EditorDocumentContent? GetOrCreateDocumentControl(Documents.DocumentEntry entry, out bool created)
    {
        created = false;

        if (entry is null || entry.Content is null)
        {
            return null;
        }

        if (entry.State != Documents.DocumentState.Loaded)
        {
            return null;
        }

        if (!entry.Format.CanShowView)
        {
            return null;
        }

        entry.MarkVisit();

        // If already open, focus directly
        if (_documentCache.TryGetValue(entry, out var existingControl))
        {
            FocusDocument(entry);
            return existingControl;
        }

        var view = DocumentViewResolver.Instance.CreateView(entry.Content.GetType());
        if (view is null)
        {
            Logs.LogError(L($"Cannot find view corresponding to document {entry}."));
            //DocumentViewResolver.Instance.LogViewTypes();

            return null;
        }

        // If not open, create new DocumentControl and associated Dockable
        var docWatching = new DocumentWatching(entry, view); // Assuming you have this constructor
        var docControl = new EditorDocumentContent(docWatching);
        docControl.Closed += DocControl_Closed;

        // Register to cache
        _documentCache[entry] = docControl;
        created = true;

        return docControl;
    }

    private void DocControl_Closed(object? sender, EventArgs e)
    {
        var docControl = (EditorDocumentContent)sender!;
        docControl.Closed -= DocControl_Closed;

        if (docControl.Document is { } doc)
        {
            _documentCache.Remove(doc);
        }
    }

    #endregion

    #region Layout

    public void LayoutProcess()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(LayoutConfigFilName) && File.Exists(LayoutConfigFilName))
            {
                LoadLayout(LayoutConfigFilName);
                _layoutLoaded = true;
            }
            else
            {
                InitializeDefaultLayout();
            }

            ConfigLayout?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception err)
        {
            err.LogErrorL("Error when config layout.");
        }

        if (!_layoutLoaded)
        {
            // When a project is opened, the DockContainer's ConfigLayout event fires, which loads the DockLayout.json file.
            // But if the project doesn't have a DockLayout.json file, the default layout needs to be initialized in the OnLoaded event, and all tools should be shown.
            ShowAllTools();
        }

        if (_dockControl.Layout is not { } layout)
        {
            return;
        }

        if (_layoutLoaded)
        {
            RestoreContent(layout);
            UpdateLayout();
        }
        else
        {
            ResetLayout();
        }
    }

    /// <summary>
    /// Initialize default layout structure (matching the provided XAML hierarchy)
    /// </summary>
    public void InitializeDefaultLayout(double leftProp = 0.2, double rightProp = 0.2, double bottomProp = 0.2)
    {
        // Key point 1: Use Factory's extension method chain (requires referencing Dock.Model.Mvvm namespace)
        // Even if there are no tools now, we reserve the container IDs

        _factory.ToolDock(out var leftPane, Alignment.Left, d => d
                .WithId("LeftPane")
                .WithTitle("Explorer")
                .WithProportion(leftProp));

        _factory.ToolDock(out var rightPane, Alignment.Right, d => d
                .WithId("PropertiesPane")
                .WithTitle("Properties")
                .WithProportion(rightProp));

        _factory.ToolDock(out var bottomPane, Alignment.Bottom, d => d
                .WithId("BottomPane")
                .WithTitle("Output")
                .WithProportion(bottomProp));

        _factory.DocumentDock(out var docsPane, d => d
                .WithId("DocumentsPane")
                .WithIsCollapsable(false)
                .WithCanCreateDocument(false));

        _factory.ProportionalDockSplitter(out var splitH1);
        _factory.ProportionalDockSplitter(out var splitH2);
        _factory.ProportionalDockSplitter(out var splitV);

        // Combine hierarchy
        _factory.ProportionalDock(out var centerVertical, Orientation.Vertical, d => d
                .Add(docsPane, splitV, bottomPane));

        _factory.ProportionalDock(out var mainLayout, Orientation.Horizontal, d => d
                .Add(leftPane, splitH1, centerVertical, splitH2, rightPane));

        _factory.RootDock(out var root, r => r
                .WithId("Root")
                .WithTitle("Suity")
                .Add(mainLayout)
                .WithActiveDockable(mainLayout)
                .WithDefaultDockable(mainLayout));

        // Key point 2: Assign the constructed root to the control
        _dockControl.Layout = root;

    }

    // Convenience overload: directly read/write file path
    public void SaveLayout(string filePath) { using var fs = File.Create(filePath); SaveLayout(fs); }
    /// <summary>
    /// Save layout to Stream
    /// </summary>
    public void SaveLayout(Stream stream)
    {
        var layout = _dockControl.Layout;
        if (layout == null) return;

        _dockState.Save(layout);      // 1. Save hidden/active state
        _serializer.Save(stream, layout); // 2. Serialize layout tree
    }

    public bool LoadLayout(string filePath) => File.Exists(filePath) && LoadLayout(File.OpenRead(filePath));
    /// <summary>
    /// Load layout from Stream
    /// </summary>
    public bool LoadLayout(Stream stream)
    {
        IDock? layout = null;
        try
        {
            layout = _serializer.Load<IDock?>(stream);
            if (layout == null)
            {
                return false;
            }

            //RestoreDockContent(layout);

            _dockState.Restore(layout);    // Restore panel visibility state
            _dockControl.Layout = layout;
            _factory.InitLayout(layout);   // Key: Rebuild Factory internal events and bindings
        }
        catch (Exception err)
        {
            err.LogErrorL("Error when loading layout.");
            return false;
        }

        //Dispatcher.UIThread.Post(() => 
        //{
        //    var layout = _dockControl.Layout;
        //    if (layout != null)
        //    {
        //        RestoreDockContent(layout);        // Recreate View/ViewModel
        //    }
        //});

        return true;
    }

    /// <summary>
    /// Restore layout: clear current state and reset to default view
    /// </summary>
    public void ResetLayout()
    {
        // 1. Back up currently open document entries
        // We only need to back up DocumentEntry, because View can be rebuilt in ShowDocumentView or restored from cache
        var openedEntries = _documentCache.Keys.ToList();

        // 2. Thoroughly clean up old layout
        _dockControl.Layout = null;

        // 3. Clear cache
        // Tool views are usually singletons or long-lived; clearing the cache ensures they are recreated and mounted to new containers in subsequent AddTool calls
        _toolCache.Clear();
        _centerToolCache.Clear();

        // Note: Don't directly Clear _documentCache, because ShowDocumentView relies on it to identify existing views
        // But we need to clear old Dockable binding relationships
        foreach (var docContent in _documentCache.Values)
        {
            docContent.Dockable = null;
        }

        // 4. Execute default layout initialization (create new LeftPane, DocumentsPane, etc.)
        InitializeDefaultLayout();

        // 5. Re-add all tool panels
        // This will repopulate the toolbar based on _viewFactories
        ShowAllTools();

        // 6. Re-mount all documents to the new DocumentsPane
        foreach (var entry in openedEntries)
        {
            // Internally calls GetOrCreateDocumentControl and handles Dockable rebinding
            ShowDocumentView(entry);
        }

        UpdateDockLayout();
    }


    public void UpdateDockLayout()
    {
        var layout = _dockControl.Layout;
        if (layout != null)
        {
            _factory.InitLayout(layout);
        }
    }

    private bool RestoreContent(IDockable? dockable)
    {
        if (dockable == null) return false;

        // 1. Handle Tool logic
        if (dockable is EditorToolDockable toolDockable)
        {
            if (_toolsById.GetValueSafe(toolDockable.Id)?.Tool is { } tool)
            {
                var toolControl = GetOrCreateToolControl(tool, out var created);
                toolControl?.Dockable = toolDockable;
            }
            else
            {
                return false;
            }
        }
        else if (dockable is EditorDocumentDockable docDockable)
        {
            if (EditorDocumentContent.ResolveDocumentPersistantString(docDockable.Id) is { } entry)
            {
                var docControl = GetOrCreateDocumentControl(entry, out var created);
                docControl?.Dockable = docDockable;
            }
            else if (EditorDocumentContent.ResolveToolWindowPersistantString(docDockable.Id) is { } tool)
            {
                var toolControl = GetOrCreateCenterToolControl(tool, out var created);
                toolControl?.Dockable = docDockable;
            }
            else
            {
                return false;
            }
        }

        // 2. Recursively process child nodes (for IDock containers)
        if (dockable is IDock dock)
        {
            List<IDockable>? removal = null;
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                // Note: Must recursively call itself here to maintain logic independence
                if (!RestoreContent(child))
                {
                    (removal ??= new List<IDockable>()).Add(child);
                }
            }

            if (removal != null)
            {
                foreach (var item in removal)
                {
                    dock.VisibleDockables?.Remove(item);
                }
            }
        }

        dockable.CanFloat = false;

        return true;
    }


    #endregion

    #region Context Menu

    static private ContextMenu CreateDocumentTabMenu()
    {
        var menuCommand = new DocumentDockTabMenu();
        EditorUtility.PrepareMenu(menuCommand);
        var menu = AvaContextMenuBinder.CreateMenuMenu(menuCommand);

        // Use Avalonia's reactive property listening
        var subscription = menu.GetObservable(Control.DataContextProperty).Subscribe(data =>
        {
            if (data is IDockable dockable)
            {
                menuCommand.ApplySender(dockable);
            }
        });

        return menu;
    }

    static private ContextMenu CreateToolTabMenu()
    {
        var menuCommand = new ToolDockTabMenu();
        EditorUtility.PrepareMenu(menuCommand);
        var menu = AvaContextMenuBinder.CreateMenuMenu(menuCommand);

        // Use Avalonia's reactive property listening
        var subscription = menu.GetObservable(Control.DataContextProperty).Subscribe(data =>
        {
            if (data is IDockable dockable)
            {
                menuCommand.ApplySender(dockable);
            }
        });

        return menu;
    }

    static private ContextMenu CreateToolControlMenu()
    {
        var menuCommand = new ToolDockTabMenu();
        EditorUtility.PrepareMenu(menuCommand);
        var menu = AvaContextMenuBinder.CreateMenuMenu(menuCommand);

        // Use Avalonia's reactive property listening
        var subscription = menu.GetObservable(Control.DataContextProperty).Subscribe(data =>
        {
            if (data is IDockable dockable)
            {
                menuCommand.ApplySender(dockable);
            }
        });

        return menu;
    }

    #endregion

    /// <summary>
    /// Helper: Recursively find Dockable by Id
    /// </summary>
    private static IDockable? FindDockable(IDockable root, string id)
    {
        if (root.Id == id) return root;
        if (root is IDock dock)
            return dock.VisibleDockables?.OfType<IDockable>()
                .Select(c => FindDockable(c, id))
                .FirstOrDefault(f => f != null);
        return null;
    }

}