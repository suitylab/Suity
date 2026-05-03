using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.VirtualTree;
using Suity.NodeQuery;
using Suity.Properties;
using Suity.Rex;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Gui;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Im.TreeEditing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Gui.InspectorGui;

/// <summary>
/// ImGui-based property inspector that displays object properties in a grid with optional tree view for hierarchical data.
/// </summary>
public partial class InspectorImGui : IDrawImGui, IInspector, IToolWindow
{
    /// <summary>
    /// Default initial height for the tree view resizer.
    /// </summary>
    public const float InitResizerHeight = 400;

    /// <summary>
    /// User data stored per inspector context for persisting UI state.
    /// </summary>
    private class UserData
    {
        /// <summary>
        /// Gets or sets the splitter position between tree view and property grid.
        /// </summary>
        public float SplitterPosition { get; set; }
        /// <summary>
        /// Gets or sets the width of the name column in the tree view.
        /// </summary>
        public float NameColumnWidth { get; set; }
        /// <summary>
        /// Gets or sets the width of the preview column in the tree view.
        /// </summary>
        public float PreviewColumnWidth { get; set; }
    }

    private readonly ImGuiNodeRef _guiRef = new();

    private readonly ImGuiTheme _theme;
    private readonly ImGuiPropertyGrid _propGrid;
    private readonly ImGuiVirtualTreeModel _treeModel;
    private readonly ColumnVirtualTreeView _treeView;

    private float _resizerPos = InitResizerHeight;
    private float? _resizerPosSetter;

    private bool _treeVisible = false;
    private readonly List<object> _detailObjects = [];

    /// <inheritdoc/>
    public event EventHandler<UndoRedoActionEventArgs> RequestDoAction;

    /// <inheritdoc/>
    public event EventHandler<ObjectPropertyEventArgs> Edited;

    private DisposeCollector _listeners;

    private bool _readOnly;
    private bool _isHidden;

    private bool? _treeViewReadOnly;
    private INodeReader _treeViewStyles;

    /// <inheritdoc/>
    public bool IsReadOnly => _readOnly || (_treeViewReadOnly == true);

    //TODO: readonly refinement executed in TreeView and PropertyGrid
    //TODO: TreeView selection can also support IInspectorRoute

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectorImGui"/> class.
    /// </summary>
    public InspectorImGui()
    {
        _theme = EditorUtility.GetEditorImGuiTheme();

        _propGrid = new ImGuiPropertyGrid("Inspector");
        _propGrid.RequestDoAction += _grid_RequestDoAction;
        _propGrid.RequestRefresh += (s, e) => _guiRef.QueueRefresh();
        _propGrid.Edited += _grid_Edited;

        _treeModel = new ImGuiVirtualTreeModel { ViewId = ViewIds.DetailTreeView };

        _treeView = new ColumnVirtualTreeView(_treeModel, ":InspectorTree");
        _treeView.Column.FullColumnResizer = true;
        _treeView.Column.ResizerMax = 1000;
        _treeView.SelectionChanged += TreeData_SelectionChanged;

        var model = _treeView.VirtualModel;
        model.RequestDoAction += TreeModel_RequestDoAction;
        model.BeginSetValue += TreeModel_BeginSetValue;
        model.EndSetValue += TreeModel_EndSetValue;
        model.ValueEdited += TreeModel_ValueEdited;
        model.ListEdited += TreeModel_ListEdited;

        EditorRexes.Mapper.Provide<IInspector>(this);
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        //Debug.WriteLine("ImGuiInspector OnGui()");

        //_demo.OnGui(gui);
        _guiRef.Node = gui.Frame("propEditorBody")
        .OnInitialize(n =>
        {
            n.InitTheme(_theme)
            .InitClass("editorBg")
            .InitFullWidth();
            //.InitGroupedResizerState(_resizerState)
        })
        .OnContent(() =>
        {
            if (!_propGrid.HasTarget)
            {
                return;
            }

            if (_treeVisible)
            {
                _treeView.OnGui(gui, "vision_tree_view", n =>
                {
                    n.InitFullWidth();
                    n.InitHeight(_resizerPos);

                    if (_resizerPosSetter.HasValue)
                    {
                        n.SetHeight(_resizerPosSetter.Value);
                        _resizerPosSetter = null;
                    }

                    _resizerPos = n.Rect.Height;
                })
                .SetReadonly(IsReadOnly);

                gui.VerticalResizer(40, null)
                .InitFullWidth()
                .InitClass("resizer");

                //Debug.WriteLine(_resizerPos);
            }

            _propGrid.OnGui(gui);
        });
    }

    #region IToolWindow

    /// <inheritdoc/>
    string IToolWindow.WindowId => "Inspector";

    /// <inheritdoc/>
    string IToolWindow.Title => "Property Inspector";

    /// <inheritdoc/>
    ImageDef IToolWindow.Icon => IconCache.inspector;

    /// <inheritdoc/>
    DockHint IToolWindow.DockHint => DockHint.Right;

    /// <inheritdoc/>
    bool IToolWindow.CanDockDocument => false;

    /// <inheritdoc/>
    object IToolWindow.GetUIObject()
    {
        return null;
    }

    /// <inheritdoc/>
    void IToolWindow.NotifyShow()
    {
        _isHidden = false;

        _listeners += EditorUtility.ShowAsDescription.AsRexListener().Subscribe(_ =>
        {
            _guiRef.QueueRefresh(true);
        });

        _guiRef.QueueRefresh(true);
    }

    /// <inheritdoc/>
    void IToolWindow.NotifyHide()
    {
        _isHidden = true;

        _listeners?.Dispose();
    }

    #endregion

    #region IInspector

    /// <inheritdoc/>
    IEnumerable<object> IInspector.DetailTreeSelection
    {
        get => _treeView.SelectedObjects;
        set => _treeView.SetSelection(value, false);
    }

    /// <inheritdoc/>
    float IInspector.SplitterPosition { get; set; }

    /// <inheritdoc/>
    void IInspector.InspectObject(object obj, IInspectorContext context, bool readOnly, ISupportStyle supportStyle, InspectorTreeModes treeMode)
    {
        if (_isHidden)
        {
            return;
        }

        foreach (var listener in _propGrid.InspectedObjects.OfType<IViewListener>())
        {
            try
            {
                listener.NotifyViewExit(ViewIds.Inspector);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        readOnly = readOnly || supportStyle?.ReadOnly == true;
        INodeReader styles = supportStyle?.Styles;

        if (obj is IInspectorRoute r)
        {
            try
            {
                if (r.GetRoutedTreeMode() is { } routedTreeMode)
                {
                    treeMode = routedTreeMode;
                }
            }
            catch (Exception err)
            {
                err.LogError();

                return;
            }
        }

        try
        {
            (obj as IViewListener)?.NotifyViewEnter(ViewIds.Inspector);
        }
        catch (Exception err)
        {
            err.LogError();
        }

        if (obj is IInspectorSplittedView && treeMode == InspectorTreeModes.None)
        {
            treeMode = InspectorTreeModes.DetailTree;
        }

        if (treeMode != InspectorTreeModes.None)
        {
            ShowTreeView(obj, treeMode, supportStyle);
        }
        else
        {
            ClearDetailView();
        }

        bool contextChanged = context != _propGrid.Context;
        if (contextChanged)
        {
            SaveUserData();
        }

        _readOnly = readOnly;
        _treeViewReadOnly = null;

        var gui = _guiRef.Gui;

        gui?.BackupState(InspectorPlugin.s_expandState);
        if (obj != null)
        {
            _propGrid.InspectObjects([obj], readOnly, context, styles);
        }
        else
        {
            _propGrid.InspectObjects([], readOnly, context, styles);
        }
        gui?.RestoreState(InspectorPlugin.s_expandState);

        if (contextChanged)
        {
            LoadUserData();
        }

        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    void IInspector.InspectObjects(IEnumerable<object> objs, IInspectorContext context, bool readOnly, ISupportStyle supportStyle, InspectorTreeModes treeMode)
    {
        if (_isHidden)
        {
            return;
        }

        if (objs.CountOne())
        {
            ((IInspector)this).InspectObject(objs.First(), context, readOnly, supportStyle, treeMode);

            return;
        }

        foreach (var listener in _propGrid.InspectedObjects.OfType<IViewListener>())
        {
            try
            {
                listener.NotifyViewExit(ViewIds.Inspector);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        readOnly = readOnly || supportStyle?.ReadOnly == true;
        INodeReader styles = supportStyle?.Styles;

        foreach (var listener in objs.OfType<IViewListener>())
        {
            try
            {
                listener.NotifyViewEnter(ViewIds.Inspector);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        _readOnly = readOnly;
        _treeViewReadOnly = null;
        _treeVisible = false;
        _treeView.DisplayedObject = null;

        bool contextChanged = context != _propGrid.Context;
        if (contextChanged)
        {
            SaveUserData();
        }

        var gui = _guiRef.Gui;

        gui?.BackupState(InspectorPlugin.s_expandState);
        _propGrid.InspectObjects(objs, readOnly, context, styles);
        gui?.RestoreState(InspectorPlugin.s_expandState);

        if (contextChanged)
        {
            LoadUserData();
        }

        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    bool IInspector.IsObjectSelected(object obj)
    {
        if (_isHidden)
        {
            return false;
        }

        return _propGrid.InspectedObjects.Contains(obj);
    }

    /// <inheritdoc/>
    void IInspector.UpdateInspector()
    {
        if (_isHidden)
        {
            return;
        }

        if (_treeVisible)
        {
            _treeView.UpdateDisplayedObject();
        }

        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    void IInspector.SetSelection(SyncPath path, out SyncPath rest, bool skipDetailView)
    {
        if (_isHidden)
        {
            rest = SyncPath.Empty;
            return;
        }

        SyncPath path2 = path;

        if (_treeVisible && !skipDetailView)
        {
            var gui = _guiRef.Gui;

            gui?.BackupState(InspectorPlugin.s_expandState);
            _treeView.SetSelection(path, out path2, false);
            _propGrid.InspectObjects(_treeView.SelectedObjects, IsReadOnly, _propGrid.Context);
            gui?.RestoreState(InspectorPlugin.s_expandState);

            _guiRef.QueueRefresh();
        }

        _propGrid.SetSelection(path2, out rest);

        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    DField IInspector.SelectedField
    {
        get
        {
            var target = _propGrid.GridData.SelectedField?.Target;
            if (target?.GetSItemFieldInfomation() is Guid id)
            {
                return EditorObjectManager.Instance.GetObject(id) as DField;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    object IInspector.GetCurrentTarget() => _propGrid.GridData.SelectedField?.Target;

    /// <inheritdoc/>
    void IInspector.DoAction(object action)
    {
        if (action is UndoRedoAction undoRedoAction)
        {
            _propGrid.Context?.InspectorDoAction(undoRedoAction);
        }
        else if (action is IValueAction valueAction)
        {
            _propGrid.Context?.InspectorDoAction(new PropertyGridActionWrapper(_propGrid, valueAction));
        }
    }

    #endregion

    #region PropertyGrid events

    private void _grid_RequestDoAction(object sender, UndoRedoActionEventArgs e)
    {
        if (_propGrid.Context?.InspectorDoAction(e.Action) == true)
        {
            e.Handled = true;
        }
        else if (RequestDoAction != null)
        {
            RequestDoAction(this, e);
        }
        else
        {
            e.Action.Do();
            e.Handled = true;
        }

        QueueRefresh();
    }

    private void _grid_Edited(object sender, ObjectPropertyEventArgs e)
    {
        var inspectObjs = _propGrid.InspectedObjects.As<IViewEditNotify>().ToArray();
        for (int i = 0; i < inspectObjs.Length; i++)
        {
            // Members of the two arrays correspond 1-1
            inspectObjs[i]?.NotifyViewEdited(e.Objects.GetArrayItemSafe(i), e.PropertyName);
        }

        Edited?.Invoke(this, e);

        if (_treeView.DisplayedObject != null)
        {
            // Notification flow
            var items = _treeView.FindSelectionOrParent<IViewEditNotify>(false).ToArray();
            for (int i = 0; i < items.Length; i++)
            {
// Members of the two arrays correspond 1-1
                items[i]?.NotifyViewEdited(e.Objects.GetArrayItemSafe(i), e.PropertyName);
            }

            _treeView.UpdateDisplayedObject();

            _guiRef.QueueRefresh();
        }
    }

    #endregion

    #region TreeView events

    private void TreeData_SelectionChanged(object sender, EventArgs e)
    {
        //Debug.WriteLine($"selection changed.");

        var selAction = new SingleTreeSelectionAction(_treeView);

        bool handled = _propGrid.Context?.InspectorDoAction(new WrapperAction(this, selAction)) == true;
        if (!handled)
        {
            selAction.Do();
        }

        _guiRef.Gui?.BackupState(InspectorPlugin.s_expandState);

        object[] objs = _treeView.SelectedObjects.Select(GetDetailInspectorViewObject).ToArray();

        foreach (var listener in _propGrid.InspectedObjects.OfType<IViewListener>())
        {
            try
            {
                listener.NotifyViewExit(ViewIds.Inspector);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        if (objs.Length == 1 && objs[0] is IInspectorRoute r)
        {
            try
            {
                _treeViewReadOnly = r.GetRoutedReadonly();
                _treeViewStyles = r.GetRoutedStyles();
            }
            catch (Exception err)
            {
                err.LogError();

                return;
            }
        }
        else
        {
            _treeViewReadOnly = null;
            _treeViewStyles = null;
        }

        foreach (var listener in objs.OfType<IViewListener>())
        {
            try
            {
                listener.NotifyViewEnter(ViewIds.Inspector);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        _propGrid.InspectObjects(objs, IsReadOnly, _propGrid.Context, _treeViewStyles);

        _guiRef.Gui?.RestoreState(InspectorPlugin.s_expandState);

        _guiRef.QueueRefresh();
    }

    private void TreeModel_RequestDoAction(object sender, UndoRedoActionEventArgs e)
    {
        var context = _propGrid.Context;
        if (context != null)
        {
            var wrapperAction = new WrapperAction(this, e.Action);
            if (context.InspectorDoAction(wrapperAction))
            {
            }
            else
            {
                e.Action.Do();
            }
            e.Handled = true;
        }
        else
        {
            e.Action.Do();
            e.Handled = true;
        }
    }

    private void TreeModel_BeginSetValue(object sender, MacroEventArgs e)
    {
        _propGrid.Context?.InspectorBeginMacro(e.Name);
    }

    private void TreeModel_EndSetValue(object sender, MacroEventArgs e)
    {
        _propGrid.Context?.InspectorEndMarco(e.Name);
    }

    private void TreeModel_ValueEdited(object sender, TreeValueEditEventArgs e)
    {
        if (_treeView.VirtualModel is ImGuiVirtualTreeModel model)
        {
            (model.DisplayedObject as IViewEditNotify)?.NotifyViewEdited(e.Value, e.PropertyName);
        }

        // Refresh edited object
        _propGrid.InspectObjects(_treeView.SelectedObjects, IsReadOnly, _propGrid.Context, _treeViewStyles);

        _propGrid.Context?.InspectorObjectEdited([e.Value], e.PropertyName);
    }

    private void TreeModel_ListEdited(object sender, ListEditEventArgs e)
    {
        if (_treeView.VirtualModel is ImGuiVirtualTreeModel model)
        {
            (model.DisplayedObject as IViewEditNotify)?.NotifyViewEdited(e.Value, null);
        }

        // Refresh edited object
        _propGrid.InspectObjects(_treeView.SelectedObjects, IsReadOnly, _propGrid.Context, _treeViewStyles);

        _propGrid.Context?.InspectorObjectEdited([e.Value], null);
    }

    #endregion

    #region UserData

    private void SaveUserData()
    {
        var context = _propGrid.Context;
        if (context is null)
        {
            return;
        }

        var userData = context.InspectorUserData as UserData;
        if (userData is null)
        {
            userData = new UserData();
            context.InspectorUserData = userData;
        }

        userData.SplitterPosition = _resizerPos;
        userData.NameColumnWidth = _treeView.Column.NameColumnWidth;
        userData.PreviewColumnWidth = _treeView.Column.PreviewColumnWidth;
    }

    private void LoadUserData()
    {
        if (_propGrid.Context?.InspectorUserData is not UserData userData)
        {
            return;
        }

        _resizerPosSetter = _resizerPos = userData.SplitterPosition;

        float nameColumn = userData.NameColumnWidth;
        float previewColumn = userData.PreviewColumnWidth;
        if (nameColumn <= 0)
        {
            nameColumn = Column3Template<VirtualNode>.DefaultNameColumnWidth;
        }
        if (previewColumn <= 0)
        {
            previewColumn = Column3Template<VirtualNode>.DefaultPreviewColumnWidth;
        }

        _treeView.Column.NameColumnWidth = nameColumn;
        _treeView.Column.PreviewColumnWidth = previewColumn;
    }

    #endregion

    //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    //{
    //    switch (keyData)
    //    {
    //        case (Keys.Control | Keys.C):
    //            if (_detailTree.IsMouseIn)
    //            {
    //                _detailTree.HandleArraySetClipboard(true);
    //                return true;
    //            }
    //            break;
    //        case (Keys.Control | Keys.X):
    //            if (_detailTree.IsMouseIn)
    //            {
    //                _detailTree.HandleArraySetClipboard(false);
    //                return true;
    //            }
    //            break;
    //        case (Keys.Control | Keys.V):
    //            if (_detailTree.IsMouseIn)
    //            {
    //                _detailTree.HandleArrayPaste();
    //                return true;
    //            }
    //            break;
    //        default:
    //            break;
    //    }

    //    return base.ProcessCmdKey(ref msg, keyData);
    //}

    private void ShowTreeView(object obj, InspectorTreeModes treeMode, ISupportStyle style = null)
    {
        if (treeMode == InspectorTreeModes.MainTree) 
        {
            obj = EditorUtility.GetViewRedirectedObject(obj, ViewIds.MainTreeView);
        }
        else if (treeMode == InspectorTreeModes.DetailTree)
        {
            obj = EditorUtility.GetViewRedirectedObject(obj, ViewIds.DetailTreeView);
        }

        if (obj != null)
        {
            _treeVisible = true;
            _treeView.DisplayedObject = obj;
            _treeView.ClearSelection(false);
            _treeModel.ViewId = treeMode == InspectorTreeModes.MainTree ?
                ViewIds.MainTreeView :
                ViewIds.DetailTreeView;

            if (_treeView.DisplayedNode is VirtualNode v)
            {
                if (style != null)
                {
                    v.ReadOnly = style.ReadOnly || IsReadOnly;
                    v.Styles = style.Styles;
                }
                else
                {
                    v.ReadOnly = IsReadOnly;
                }
            }

            _treeView.ExpandAll();
        }
        else
        {
            _treeVisible = false;
            _treeView.DisplayedObject = null;
        }
    }

    private void ClearDetailView()
    {
        _treeVisible = false;
        _treeView.DisplayedObject = null;
    }


    private object GetDetailInspectorViewObject(object obj)
    {
        if (obj is IViewRedirect redirect)
        {
            return EditorUtility.GetViewRedirectedObject(redirect, ViewIds.Inspector);
        }
        else
        {
            return obj;
        }
    }

    /// <summary>
    /// Requests the UI to refresh.
    /// </summary>
    internal void QueueRefresh()
    {
        _guiRef.QueueRefresh();
    }

    #region WrapperAction

    /// <summary>
    /// Wraps an undo/redo action to automatically refresh the inspector after execution.
    /// </summary>
    public class WrapperAction : UndoRedoAction
    {
        private readonly InspectorImGui _inspactor;
        private readonly UndoRedoAction _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperAction"/> class.
        /// </summary>
        /// <param name="inspector">The inspector instance.</param>
        /// <param name="action">The action to wrap.</param>
        public WrapperAction(InspectorImGui inspector, UndoRedoAction action)
        {
            _inspactor = inspector ?? throw new ArgumentNullException(nameof(inspector));
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
        public override bool Modifying => _action.Modifying;

        /// <inheritdoc/>
        public override void Do()
        {
            _action.Do();

            _inspactor.QueueRefresh();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            _action.Undo();

            _inspactor.QueueRefresh();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _action.ToString();
        }
    }

    #endregion
}