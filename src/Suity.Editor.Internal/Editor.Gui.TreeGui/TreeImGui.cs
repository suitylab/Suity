using Suity.Collections;
using Suity.Editor.Analyzing;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.Editor.VirtualTree;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Im.TreeEditing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Gui.TreeGui;

/// <summary>
/// Provides an ImGui-based tree view implementation with support for selection, inspection, clipboard operations, and analysis.
/// </summary>
public class TreeImGui : IViewObjectImGui,
    IObjectView,
    IInspectorContext,
    IColumnPreview,
    IViewSelectable,
    IViewSelectionInfo,
    IViewClipboard,
    IViewComment,
    IAnalysable
{
    /// <summary>
    /// Initial height for the resizer.
    /// </summary>
    public const int InitResizerHeight = 400;

    private readonly ImGuiNodeRef _guiRef = new();
    private bool _lastEnabled = true;

    private readonly ImGuiTheme _theme;
    private readonly ImGuiVirtualTreeView _treeView;

    private object _target;

    private readonly VirtualNodeExpandState _expandState = new();

    private readonly DelayRefreshAction _delayRefresh;


    /// <summary>
    /// Gets the underlying virtual tree view.
    /// </summary>
    protected ImGuiVirtualTreeView TreeView => _treeView;


    /// <summary>
    /// Initializes a new instance with headerless tree options.
    /// </summary>
    /// <param name="option">The headerless tree configuration options.</param>
    public TreeImGui(HeaderlessTreeOptions option)
        : this()
    {
        var treeView = new HeaderlessVirtualTreeView(ViewIds.MainTreeView, option?.MenuName)
        {
            SelectionActionEnabled = true,
            ShowDisplayText = option?.ShowDisplayText ?? false,
            StatusIconAtTheEnd = option?.StatusIconAtTheEnd ?? false
        };

        _treeView = treeView;

        ConfigTreeViewEvent();
    }

    /// <summary>
    /// Initializes a new instance with column tree options.
    /// </summary>
    /// <param name="option">The column tree configuration options.</param>
    public TreeImGui(ColumnTreeOptions option)
        : this()
    {
        var treeView = new ColumnVirtualTreeView(ViewIds.MainTreeView, option?.MenuName)
        {
            SelectionActionEnabled = true,
            ShowDisplayText = option?.ShowDisplayText ?? false,
            StatusIconAtTheEnd = option?.StatusIconAtTheEnd ?? false
        };

        treeView.Column.FullColumnResizer = option?.FullColumnResizer ?? true;
        treeView.Column.ResizerMax = option?.ResizerMax ?? 1000;

        treeView.Column.NameColumn.Enabled = option?.NameColumn ?? true;
        treeView.Column.DescriptionColumn.Enabled = option?.DescriptionColumn ?? false;
        treeView.Column.PreviewColumn.Enabled = option?.PreviewColumn ?? true;

        _treeView = treeView;

        ConfigTreeViewEvent();
    }

    /// <summary>
    /// Initializes a new instance with an existing virtual tree view.
    /// </summary>
    /// <param name="treeView">The virtual tree view to use.</param>
    protected TreeImGui(ImGuiVirtualTreeView treeView)
        : this()
    {
        _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));

        ConfigTreeViewEvent();
    }

    /// <summary>
    /// Initializes a new instance with default settings.
    /// </summary>
    protected TreeImGui()
    {
        _theme = EditorUtility.GetEditorImGuiTheme();
        _delayRefresh = new DelayRefreshAction(this);
    }

    /// <summary>
    /// Configures event handlers for the tree view and its underlying model.
    /// </summary>
    private void ConfigTreeViewEvent()
    {
        var treeView = _treeView ?? throw new NullReferenceException(nameof(_treeView));
        var model = treeView.VirtualModel ?? throw new NullReferenceException(nameof(treeView.VirtualModel));

        treeView.SelectionChanged += TreeView_SelectionChanged;
        treeView.RequestDoAction += TreeView_RequestDoAction;

        model.RequestDoAction += TreeModel_RequestDoAction;
        model.BeginSetValue += TreeModel_BeginSetValue;
        model.EndSetValue += TreeModel_EndSetValue;
        model.ValueEdited += TreeModel_ValueEdited;
        model.ListEdited += TreeModel_ListEdited;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this view.
    /// </summary>
    public int ViewId
    {
        get => _treeView.ViewId;
        set => _treeView.ViewId = value;
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        OnNodeGui(gui);
    }

    /// <summary>
    /// Renders the tree view node within the ImGui context.
    /// </summary>
    /// <param name="gui">The ImGui rendering context.</param>
    /// <returns>The rendered ImGui node.</returns>
    public virtual ImGuiNode OnNodeGui(ImGui gui)
    {
        var frame = _guiRef.Node = gui.Frame("treeViewBody")
        .OnInitialize(n =>
        {
            n.InitTheme(_theme);
            n.InitClass("frameBg");
            n.InitFullWidth();
            n.InitHeightRest();
        })
        .OnContent(() =>
        {
            if (_target != null)
            {
                _treeView.OnGui(gui, "vision_tree_view", n =>
                {
                    n.InitFullWidth();
                    n.InitFullHeight();
                });
            }
        });

        _lastEnabled = !frame.IsDisabled;

        return frame;
    }

    #region IObjectView

    /// <inheritdoc/>
    public object TargetObject => _target;

    #endregion

    #region IViewObjectImGui

    /// <inheritdoc/>
    public event EventHandler SelectionChanged;

    /// <inheritdoc/>
    public event EventHandler Dirty;

    /// <inheritdoc/>
    public event EventHandler<object[]> Edited;

    /// <inheritdoc/>
    public event EventHandler<object[]> RequestInspect;

    /// <summary>
    /// Gets or sets the target object displayed by the tree view.
    /// </summary>
    public object Target
    {
        get => _target;
        set
        {
            if (ReferenceEquals(_target, value))
            {
                return;
            }

            _target = value;
            _treeView.DisplayedObject = value;
            OnTargetUpdated();
            QueueRefresh();
        }
    }

    /// <inheritdoc/>
    public void FocusView(bool inspect)
    {
        UpdateDisplayedObject();

        if (inspect && _lastEnabled)
        {
            var selection = SelectedObjects.Select(o => GetInspectorViewObject(o)).SkipNull().ToArray();
            EditorUtility.Inspector.InspectObjects(selection, this);
        }
    }

    /// <inheritdoc/>
    public void ExpandRoot() => _treeView.ExpandRoot();

    /// <inheritdoc/>
    public void ExpandAll() => _treeView.ExpandAll();

    /// <inheritdoc/>
    public void QueueRefresh(bool refreshAll = false)
    {
        if (refreshAll)
        {
            _guiRef.QueueRefresh(true);
            return;
        }

        if (_treeView.FindTreeViewNode() is { } node)
        {
            node.QueueRefresh();
        }
        else
        {
            _guiRef.QueueRefresh();
        }
    }

    /// <inheritdoc/>
    public void UpdateDisplayedObject()
    {
        if (_lastEnabled)
        {
            _treeView.UpdateDisplayedObject();
            EditorUtility.Inspector.UpdateInspector();
            QueueRefresh();
        }
        else
        {
            EditorUtility.Inspector.InspectObject(null);
        }
    }

    /// <inheritdoc/>
    public void RestoreViewState(object configObj = null)
    {
        if (_treeView.VirtualModel is null)
        {
            return;
        }

        if (configObj is SingleTreeAssetConfig config)
        {
            (this as IInspectorContext).InspectorUserData = config.UserData;

            _expandState.SetExpandedPaths(config.Expands);
            _expandState.Restore(_treeView.VirtualModel);

            if (config.PreviewPresets != null && _treeView is IHasPreviewPreset p)
            {
                p.RestorePresets(config.PreviewPresets);
                p.ChangePreset(config.SelectedPreviewPreset, false);
            }

            VirtualPath[] selection = VirtualPath.CreateMultiple(config.Selections);
            _treeView.SetSelection(selection, false);
        }
        else
        {
            _treeView.ExpandRoot();
        }
    }

    /// <inheritdoc/>
    public object SaveViewState()
    {
        if (_treeView.VirtualModel is null)
        {
            return null;
        }

        _expandState.Backup(_treeView.VirtualModel);
        var state = _expandState.GetExpandedPaths().ToArray();
        var selection = _treeView.LastSelection;

        var config = new SingleTreeAssetConfig()
        {
            UserData = (this as IInspectorContext).InspectorUserData,
        };

        config.Expands.AddRange(state);
        config.AddBySelection(selection);

        if (_treeView is IHasPreviewPreset p)
        {
            p.MarkCurrentPreset();
            config.PreviewPresets.AddRange(p.GetAllPresets());
            config.SelectedPreviewPreset = p.CurrentPresetName;
        }

        return config;
    }

    /// <summary>
    /// Called when the target object has been updated.
    /// </summary>
    protected virtual void OnTargetUpdated()
    {
    }

    #endregion

    #region IServiceProvider

    /// <inheritdoc/>
    public virtual object GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        if ((_target as IServiceProvider)?.GetService(serviceType) is object obj)
        {
            return obj;
        }

        if (_target != null && serviceType.IsInstanceOfType(_target))
        {
            return _target;
        }

        return null;
    }

    #endregion

    #region IInspectorContext

    /// <inheritdoc/>
    public virtual void InspectorEnter()
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorExit()
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorBeginMacro(string name)
    {
    }

    /// <inheritdoc/>
    public virtual void InspectorEndMarco(string name)
    {
    }

    /// <inheritdoc/>
    public virtual bool InspectorDoAction(UndoRedoAction action)
    {
        action.Do();
        return true;
    }

    /// <inheritdoc/>
    public virtual void InspectorEditFinish()
    {
        QueueRefresh();
    }

    // Property editor update
    /// <inheritdoc/>
    public virtual void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        // Document set dirty
        OnDirty();

        HandleObjectEdited(objs.ToArray(), null);

        // Corresponding to currently selected node, refresh its PropertyTarget
        foreach (var node in _treeView.SelectedNodes)
        {
            (node.Tag as PropertyTarget)?.ClearFields();
        }

        // Tree view update
        _treeView.UpdateDisplayedObject();

        QueueRefresh();
    }

    /// <inheritdoc/>
    public virtual object InspectorUserData { get; set; } = 0.7f;

    #endregion

    #region IColumnPreview

    /// <inheritdoc/>
    bool IColumnPreview.AddPreviewPath(PreviewPath path)
    {
        if (_treeView.AddPreviewPath(path))
        {
            QueueRefresh();

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    bool IColumnPreview.RemovePreviewPath(PreviewPath path)
    {
        if (_treeView.RemovePreviewPath(path))
        {
            QueueRefresh();

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    void IColumnPreview.ClearPreviewPaths()
    {
        _treeView.ClearPreviewPath();

        QueueRefresh();
    }

    #endregion

    #region IViewSelectable

    /// <inheritdoc/>
    public virtual ViewSelection GetSelection()
    {
        return new ViewSelection(_treeView.LastSelection);
    }

    /// <inheritdoc/>
    public virtual bool SetSelection(ViewSelection selection)
    {
        return InternalSetSelection(selection.Selection);
    }

    /// <summary>
    /// Internally sets the selection based on various input types.
    /// </summary>
    /// <param name="selection">The selection object, which can be a string, path, array, or enumerable.</param>
    /// <returns>True if the selection was successfully set; otherwise, false.</returns>
    private bool InternalSetSelection(object selection)
    {
        if (selection is null)
        {
            return false;
        }

        if (selection is string str)
        {
            object obj = (_target as IViewElementOwner)?.GetElement(str);
            if (obj is not string)
            {
                return InternalSetSelection(obj);
            }
            else
            {
                return false;
            }
        }
        else if (selection is ISyncPathObject o)
        {
            SyncPath path = o.GetPath();

            return InternalSetSelection(path);
        }
        else if (selection is VirtualPath[] paths)
        {
            _treeView.SetSelection(paths);

            return true;
        }
        else if (selection is SyncPath path)
        {
            _treeView.SetSelection(path, out SyncPath rest);

            if (!SyncPath.IsNullOrEmpty(rest))
            {
                if (_treeView.SelectedObjects.CountOne() && _treeView.SelectedObjects.FirstOrDefault() is IViewGotoDefinitionAction v)
                {
                    v.GotoDefinition(rest, out rest);
                }

                if (!SyncPath.IsNullOrEmpty(rest))
                {
                    EditorUtility.Inspector.SetSelection(rest, out _);
                }
            }

            return true;
        }
        else if (selection is IEnumerable<object> objs)
        {
            return _treeView.SetSelection(objs);
        }
        else
        {
            return _treeView.SetSelection([selection]);
        }
    }

    #endregion

    #region IViewSelectionInfo

    /// <inheritdoc/>
    public IEnumerable<object> SelectedObjects => _treeView.SelectedObjects;

    /// <inheritdoc/>
    public IEnumerable<T> FindSelectionOrParent<T>(bool distinct = true) where T : class
        => _treeView.FindSelectionOrParent<T>(distinct);

    #endregion

    #region IViewClipboard

    /// <inheritdoc/>
    public void ClipboardCopy() => _treeView.ClipboardCopy();

    /// <inheritdoc/>
    public void ClipboardCut() => _treeView.ClipboardCut();

    /// <inheritdoc/>
    public void ClipboardPaste() => _treeView.ClipboardPaste();

    #endregion

    #region IViewComment

    /// <inheritdoc/>
    public bool CanComment => _treeView.CanComment;

    /// <inheritdoc/>
    public bool IsComment
    {
        get => _treeView.IsComment;
        set => _treeView.IsComment = value;
    }

    #endregion

    #region TreeView events

    /// <summary>
    /// Handles the tree view selection changed event.
    /// </summary>
    private void TreeView_SelectionChanged(object sender, EventArgs e)
    {
        // Update property editor
        UpdateInspector();
        OnSelectionChanged();
    }

    /// <summary>
    /// Handles the tree view request to execute an action.
    /// </summary>
    private void TreeView_RequestDoAction(object sender, ValueActionEventArgs e)
    {
        e.Handled = OnTreeDoAction(new ValueWrapperAction(this, e.Action));
    }

    /// <summary>
    /// Handles the tree model request to execute an undo/redo action.
    /// </summary>
    private void TreeModel_RequestDoAction(object sender, UndoRedoActionEventArgs e)
    {
        if (e.Action is SingleTreeSelectionAction)
        {
            e.Handled = OnTreeDoAction(new CompondSelectionAction(this, e.Action));
        }
        else
        {
            e.Handled = OnTreeDoAction(new WrapperAction(this, e.Action));
        }
    }

    /// <summary>
    /// Handles the tree model begin set value event.
    /// </summary>
    private void TreeModel_BeginSetValue(object sender, MacroEventArgs e)
    {
        OnTreeBeginMacro(e.Name);
    }

    /// <summary>
    /// Handles the tree model end set value event.
    /// </summary>
    private void TreeModel_EndSetValue(object sender, MacroEventArgs e)
    {
        OnTreeEndMacro(e.Name);
    }

    /// <summary>
    /// Handles the tree model value edited event.
    /// </summary>
    private void TreeModel_ValueEdited(object sender, TreeValueEditEventArgs e)
    {
        OnTreeEditFinish();

        // Document set dirty
        OnDirty();

        if (e.PropertyName != null)
        {
            HandleObjectEdited([e.Value], e.PropertyName);
        }

        // Update property editor
        UpdateInspector();
    }

    /// <summary>
    /// Handles the tree model list edited event.
    /// </summary>
    private void TreeModel_ListEdited(object sender, ListEditEventArgs e)
    {
        OnTreeEditFinish();

        // Document set dirty
        OnDirty();

        HandleObjectEdited([e.Value], null);

        // Update property editor
        UpdateInspector();
    }

    /// <summary>
    /// Called when a macro operation begins on the tree.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    protected virtual void OnTreeBeginMacro(string name)
    {
    }

    /// <summary>
    /// Called when a macro operation ends on the tree.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    protected virtual void OnTreeEndMacro(string name)
    {
    }

    /// <summary>
    /// Called to execute an action on the tree.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>True if the action was handled; otherwise, false.</returns>
    protected virtual bool OnTreeDoAction(UndoRedoAction action)
    {
        action.Do();
        return true;
    }

    /// <summary>
    /// Called when editing is finished on the tree.
    /// </summary>
    protected virtual void OnTreeEditFinish()
    {
    }

    #endregion

    #region Selection

    /// <summary>
    /// Gets the currently selected virtual nodes in the tree view.
    /// </summary>
    internal IEnumerable<VirtualNode> SelectedNodes => _treeView.SelectedNodes;

    #endregion

    #region Functions

    /// <summary>
    /// Gets the appropriate object to display in the inspector view.
    /// </summary>
    /// <param name="o">The object to resolve.</param>
    /// <returns>The resolved inspector view object, or null if not applicable.</returns>
    public object GetInspectorViewObject(object o)
    {
        if (o is null)
        {
            return null;
        }

        if (o is IViewRedirect showOption)
        {
            return EditorUtility.GetViewRedirectedObject(showOption, ViewIds.Inspector);
        }
        else if (o is SArray)
        {
            return null;
        }
        else
        {
            return o;
        }
    }

    /// <summary>
    /// Creates a context menu for the tree view with the specified name.
    /// </summary>
    /// <param name="menuName">The name of the menu to create.</param>
    public void CreateMenu(string menuName)
    {
        _treeView.CreateMenu(menuName);
    }

    /// <summary>
    /// Called when the selection in the tree view has changed.
    /// </summary>
    protected virtual void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called to mark the view as dirty.
    /// </summary>
    protected virtual void OnDirty()
    {
        Dirty?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when objects have been edited.
    /// </summary>
    /// <param name="objs">The edited objects.</param>
    protected virtual void OnEdited(object[] objs)
    {
        Edited?.Invoke(this, objs);
    }

    /// <summary>
    /// Called to request inspection of objects.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <returns>True if the inspection was handled; otherwise, false.</returns>
    protected virtual bool OnInspectObjects(IEnumerable<object> objs)
    {
        if (RequestInspect != null)
        {
            RequestInspect(this, objs.ToArray());
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Updates the inspector with the current selection.
    /// </summary>
    private void UpdateInspector()
    {
        if (_lastEnabled)
        {
            var selection = _treeView.SelectedObjects.Select(GetInspectorViewObject).Where(o => o != null);

            if (!OnInspectObjects(selection))
            {
                EditorUtility.Inspector.InspectObjects(selection, this);
            };
        }
        else
        {
            EditorUtility.Inspector.InspectObject(null);
        }
    }

    /// <summary>
    /// Handles object editing notifications and updates related views.
    /// </summary>
    /// <param name="objs">The edited objects.</param>
    /// <param name="name">The property name that was edited, or null.</param>
    private void HandleObjectEdited(object[] objs, string name)
    {
        // Update is executed during editor running, to avoid order issues, use async call
        QueuedAction.Do(() =>
        {
            var e = FindSelectionOrParent<IViewEditNotify>(false).ToArray();
            for (int i = 0; i < e.Length; i++)
            {
                // The members of the two arrays correspond 1-to-1
                e[i]?.NotifyViewEdited(objs.GetArrayItemSafe(i), name);
            }

            (_target as IViewElementEditNotify)?.NotifyViewElementEdited(objs);

            OnEdited(objs);
        });
    }

    #endregion

    #region Analysis

    private int _analyzeRequested = 0;

    /// <inheritdoc/>
    public void RequestAnalyze()
    {
        if (_analyzeRequested < 2)
        {
            _analyzeRequested = 2;
        }
    }

    /// <summary>
    /// Requests an update to the reference count analysis.
    /// </summary>
    public void RequestUpdateReferenceCount()
    {
        if (_analyzeRequested < 1)
        {
            _analyzeRequested = 1;
        }
    }

    /// <inheritdoc/>
    public void UpdateAnalysis()
    {
        if (_analyzeRequested == 0)
        {
            return;
        }

        int level = _analyzeRequested;

        _analyzeRequested = 0;

        if (_treeView.DisplayedObject is { } o)
        {
            AnalysisOption option = null;

            switch (level)
            {
                case 2:
                    option = new AnalysisOption
                    {
                    };
                    break;

                case 1:
                    option = new AnalysisOption
                    {
                        CollectProblem = false,
                        CollectMember = false,
                        CollectReference = true,
                        CollectConflict = false,
                        CollectExternalDependencies = false,
                        CollectProblemDependencies = false,
                        CollectRenderTargets = false,
                    };
                    break;

                default:
                    break;
            }

            if (option is null)
            {
                return;
            }

            EditorServices.AnalysisService.QueueAnalyze(o, option, () =>
            {
                _treeView.UpdateDisplayedObject();
                EditorUtility.Inspector.UpdateInspector();
                QueueRefresh();
            });
        }
    }

    #endregion

    #region CompondSelectionAction

    /// <summary>
    /// A compound undo/redo action that combines selection changes with a base action.
    /// </summary>
    private class CompondSelectionAction : UndoRedoAction
    {
        private readonly TreeImGui _view;
        private readonly UndoRedoAction _baseAction;
        private readonly ViewSelection _detailSelection;

        /// <summary>
        /// Initializes a new instance of the compound selection action.
        /// </summary>
        /// <param name="view">The tree view instance.</param>
        /// <param name="baseAction">The base undo/redo action to wrap.</param>
        public CompondSelectionAction(TreeImGui view, UndoRedoAction baseAction)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _baseAction = baseAction ?? throw new ArgumentNullException(nameof(baseAction));
        }

        /// <inheritdoc/>
        public override string Name => _baseAction.Name;

        /// <inheritdoc/>
        public override bool Modifying => false;

        /// <inheritdoc/>
        public override void Do()
        {
            _baseAction.Do();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            _baseAction.Undo();
        }
    }

    #endregion

    #region WrapperAction

    /// <summary>
    /// A wrapper undo/redo action that refreshes the view after execution.
    /// </summary>
    private class WrapperAction : UndoRedoAction
    {
        private readonly TreeImGui _view;
        private readonly UndoRedoAction _action;

        /// <summary>
        /// Initializes a new instance of the wrapper action.
        /// </summary>
        /// <param name="view">The tree view instance.</param>
        /// <param name="action">The action to wrap.</param>
        public WrapperAction(TreeImGui view, UndoRedoAction action)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _action = action ?? throw new ArgumentNullException(nameof(action));

            // Disable first-time no-refresh -> Always refresh in ImGui mode
            if (_action is VirtualNodeSetterAction vAction)
            {
                vAction.FirstDo = false;
            }
        }

        /// <inheritdoc/>
        public override string Name => _action.ToString();

        /// <inheritdoc/>
        public override bool IsVoid => _action.IsVoid;

        /// <inheritdoc/>
        public override void Do()
        {
            _action.Do();

            _view.QueueRefresh();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            _action.Undo();

            _view.QueueRefresh();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _action.ToString();
        }
    }

    /// <summary>
    /// A wrapper action for value-based operations that notifies the inspector on changes.
    /// </summary>
    private class ValueWrapperAction : UndoRedoAction
    {
        private readonly TreeImGui _view;
        private readonly IValueAction _action;

        /// <summary>
        /// Initializes a new instance of the value wrapper action.
        /// </summary>
        /// <param name="view">The tree view instance.</param>
        /// <param name="action">The value action to wrap.</param>
        public ValueWrapperAction(TreeImGui view, IValueAction action)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _action = action ?? throw new ArgumentNullException(nameof(action));

            // Disable first-time no-refresh -> Always refresh in ImGui mode
            if (_action is VirtualNodeSetterAction vAction)
            {
                vAction.FirstDo = false;
            }
        }

        /// <inheritdoc/>
        public override string Name => _action.ToString();

        /// <inheritdoc/>
        public override bool IsVoid => false;

        /// <inheritdoc/>
        public override void Do()
        {
            _action.DoAction();

            ((IInspectorContext)_view).InspectorObjectEdited(_action.ParentObjects, _action.Name ?? string.Empty);
            EditorUtility.Inspector.UpdateInspector();

            _view.QueueRefresh();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            _action.UndoAction();

            ((IInspectorContext)_view).InspectorObjectEdited(_action.ParentObjects, _action.Name ?? string.Empty);
            EditorUtility.Inspector.UpdateInspector();

            _view.QueueRefresh();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _action.ToString();
        }
    }

    #endregion

    #region DelayAction

    /// <summary>
    /// A delayed action that triggers a queue refresh on the tree view.
    /// </summary>
    private class DelayRefreshAction(TreeImGui value) : DelayedAction<TreeImGui>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.QueueRefresh();
        }
    }

    #endregion

}
