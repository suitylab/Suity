using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Im.PropertyEditing.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// A sub-property grid used for displaying and editing properties within expanded node views.
/// </summary>
public class ImSubPropertyGrid : IPropertyGrid, IDrawExpandedImGui
{
    /// <summary>
    /// Default width for the expanded content.
    /// </summary>
    public const int DefaultContentWidth = 470;

    private static PropertyGridRootMenu _menu;

    private RootPropertyTarget? _rootTarget;

    private readonly ImGuiTheme _theme;
    private readonly PropertyGridData _gridData;
    private readonly GroupedResizerState _resizerState = new(150, 250);

    private bool _scrollable;
    private bool _showContextMenu = true;
    private bool _showToolBar = false;
    private bool _readOnly;
    private object[]? _objs;
    private readonly ImGuiNodeRef _guiRef = new();

    private Dictionary<Type, object>? _services = null;


    /// <inheritdoc/>
    public bool ShowContextMenu { get => _showContextMenu; set => _showContextMenu = value; }

    /// <inheritdoc/>
    public bool ShowToolBar { get => _showToolBar; set => _showToolBar = value; }

    /// <summary>
    /// Gets a value indicating whether this grid has a target object.
    /// </summary>
    public bool HasTarget => _rootTarget != null;

    /// <inheritdoc/>
    public bool ReadOnly => _readOnly;


    /// <inheritdoc/>
    public event EventHandler? RequestRefresh;

    /// <inheritdoc/>
    public event EventHandler<UndoRedoActionEventArgs>? RequestDoAction;

    /// <inheritdoc/>
    public event EventHandler<ObjectPropertyEventArgs>? Edited;

    /// <summary>
    /// Gets the inspector context associated with this grid.
    /// </summary>
    public IInspectorContext? Context { get; private set; }

    /// <summary>
    /// Gets the underlying property grid data.
    /// </summary>
    public PropertyGridData GridData => _gridData;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImSubPropertyGrid"/> class.
    /// </summary>
    /// <param name="name">The display name for the property grid.</param>
    /// <param name="scrollable">Indicates whether the grid content should be scrollable.</param>
    public ImSubPropertyGrid(string name, bool scrollable)
    {
        _theme = PropertyGridTheme.Default; //EditorUtility.GetEditorImGuiTheme();

        _gridData = new(name);
        //_gridData.CanSelectField = false;
        _gridData.SelectionChanged += _gridData_SelectionChanged;
        _gridData.ValueActionRequest += _gridData_ValueActionRequest;

        if (_menu is null)
        {
            _menu = new PropertyGridRootMenu(":SubInspector");
            EditorUtility.PrepareMenu(_menu);
        }

        _scrollable = scrollable;
    }


    #region IDrawExpandedImGui

    /// <inheritdoc/>
    public bool ResizableOnExpand => false;

    /// <inheritdoc/>
    float? IDrawExpandedImGui.ContentScale => 0.4f;

    /// <inheritdoc/>
    public void EnterExpandedView(object target, IInspectorContext? context = null)
    {
        if (target is FlowNode flowNode)
        {
            target = flowNode.ExpandedViewObject ?? flowNode;
        }

        InspectObjects([target], context: context);
    }

    /// <inheritdoc/>
    public void ExitExpandedView()
    {
    }

    /// <inheritdoc/>
    public void UpdateExpandedTarget()
    {
    }

    /// <inheritdoc/>
    public ImGuiNode OnExpandedGui(ImGui gui)
    {
        if (_guiRef.Node is null)
        {
            if (_showContextMenu)
            {
                // Pre-generate menu
                (gui.Context as IGraphicContextMenu)?.RegisterContextMenu(_menu);
            }
        }

        var node = _guiRef.Node = gui.PropertyFrame("##property_grid_group", _scrollable, _gridData)
        .OnInitialize(n =>
        {
            n.SetTheme(_theme);
            n.SetClass("bg");
            n.SetValue(_resizerState);
            n.SetValue(_gridData);

            if (ResizableOnExpand)
            {
                n.SetFullWidth();
                //n.OverridePadding(0, 10, 0, 0);
                //n.OverrideMargin(0, 10, 0, 0);
            }
            else
            {
                n.SetFitVertical();
                n.SetWidth(DefaultContentWidth);
                //n.OverridePadding(0, 10, 0, 0);
                //n.OverrideMargin(0, 10, 0, 0);
            }

            //n.InitKeyDownInput((n, input) =>
            //{
            //    if (_gridData.SelectedField is { } value)
            //    {
            //        switch (input.KeyCode)
            //        {
            //            case "Up":
            //                PropertyGridExternalBK.HandleMoveUp(n, value, _gridData);
            //                input.Handled = true;
            //                return GuiInputState.Render;

            //            case "Down":
            //                PropertyGridExternalBK.HandleMoveDown(n, value, _gridData);
            //                input.Handled = true;
            //                return GuiInputState.Render;

            //            default:
            //                //TODO: Cannot determine here whether the key is already consumed
            //                value.KeyDownRequest = input.KeyCode;
            //                return GuiInputState.Render;
            //        }
            //    }

            //    return null;
            //});
        })
        .InitInputMouseUp(GuiMouseButtons.Right, n =>
        {
            var sel = _gridData.SelectedField;
            if (sel is { } && _showContextMenu)
            {
                _menu.ApplySender(this);
                _menu.PopUp(1, [typeof(PropertyRowData)], typeof(PropertyRowData));
                (gui.Context as IGraphicContextMenu)?.ShowContextMenu(_menu);

                return GuiInputState.Render;
            }

            return GuiInputState.None;
        })
        .OnContent(() =>
        {
            if (_rootTarget != null)
            {
                gui.PropertyField(_rootTarget);
            }
        });

        return node;
    }

    /// <summary>
    /// Clears the current selection in the property grid.
    /// </summary>
    public void ClearSelection()
    {
        _gridData?.SetSelection(null);
    }

    #endregion

    /// <summary>
    /// Inspects the specified objects and displays their properties in the grid.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <param name="readOnly">Indicates whether the properties should be read-only.</param>
    /// <param name="context">The inspector context.</param>
    /// <param name="styles">Optional node reader for property styles.</param>
    public void InspectObjects(IEnumerable<object> objs, bool readOnly = false, IInspectorContext? context = null, INodeReader? styles = null)
    {
        if (Context != context)
        {
            Context?.InspectorExit();
            Context = context;
            context?.InspectorEnter();
        }

        _readOnly = readOnly;

        _objs = [..objs];


        for (int i = 0; i < _objs.Length; i++)
        {
            var obj = _objs[i];
            if (obj is IInspectorRoute r)
            {
                try
                {
                    obj = EditorUtility.GetViewRedirectedObject(r, ViewIds.Inspector);
                    if (r.GetRoutedReadonly())
                    {
                        readOnly = true;
                    }
                    if (r.GetRoutedStyles() is { } routedStyle)
                    {
                        styles = routedStyle;
                    }

                    _objs[i] = obj;
                }
                catch (Exception err)
                {
                    _objs = null;

                    err.LogError();

                    return;
                }
            }
        }

        if (styles is null)
        {
            // The sub-property editor is used for viewing properties after node expansion, so connectors are hidden by default
            var rawNode = new RawNode();
            rawNode.SetAttribute(ViewProperty.HideConnectorAttribute, "true");
            styles = rawNode;
        }

        _rootTarget = new RootPropertyTarget(_objs)
        {
            ServiceProvider = context ?? (IServiceProvider)this,
            Styles = styles
        };
    }

    /// <inheritdoc/>
    public IEnumerable<object> InspectedObjects => _objs ?? [];


    /// <inheritdoc/>
    public void DoAction(IValueAction action)
    {
        _gridData.DoAction(action);
        _guiRef.QueueRefresh();
    }


    /// <summary>
    /// Notifies that a property has changed on the inspected objects.
    /// </summary>
    /// <param name="objs">The objects whose properties changed.</param>
    /// <param name="name">The name of the changed property.</param>
    internal void NotifyObjectPropertyChanged(IEnumerable<object> objs, string name)
    {
        Context?.InspectorObjectEdited(objs, name);

        if (_objs is { } inspectObjs)
        {
            var objAry = objs.ToArray();
            for (int i = 0; i < inspectObjs.Length; i++)
            {
                // Members of the two arrays correspond 1-to-1
                (inspectObjs[i] as IViewEditNotify)?.NotifyViewEdited(objAry.GetArrayItemSafe(i), name);
            }
        }

        if (_rootTarget is { } target)
        {
            foreach (var value in target.Values.OfType<IInspectorEditNotify>())
            {
                value.NotifyInspectorEdited();
            }
        }

        _guiRef.QueueRefresh();
    }

    /// <summary>
    /// Handles selection changes in the property grid data.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The selected property row data.</param>
    private void _gridData_SelectionChanged(object sender, PropertyRowData? e)
    {
        var obj = e?.Target?.GetValues()?.FirstOrDefault();
        if (obj is IViewObject || obj is SObject || obj is SArray)
        {
            EditorUtility.Inspector.InspectObject(obj, Context);
        }
        else
        {
            EditorUtility.Inspector.InspectObject(null);
        }
    }


    /// <summary>
    /// Handles value action requests from the property grid.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The value action event arguments.</param>
    private void _gridData_ValueActionRequest(object sender, ValueActionEventArgs e)
    {
        SubGridActionWrapper action = null;

        if (RequestDoAction != null)
        {
            action ??= new SubGridActionWrapper(this, e.Action);
            var args = new UndoRedoActionEventArgs(action);

            RequestDoAction(this, args);
            // Set Handled on success to prevent re-execution
            e.Handled = args.Handled;
        }

        if (!e.Handled && Context != null)
        {
            action ??= new SubGridActionWrapper(this, e.Action);
            // Set Handled on success to prevent re-execution
            e.Handled = Context.InspectorDoAction(action);
        }

        if (!e.Handled)
        {
            e.Action.DoAction();
            // Set Handled on success to prevent re-execution
            e.Handled = true;
        }

        NotifyObjectPropertyChanged(e.Action.ParentObjects, e.Action.Name ?? string.Empty);
        RequestRefresh?.Invoke(this, EventArgs.Empty);
        _guiRef.QueueRefresh();
    }

    /// <inheritdoc/>
    public void AddService<T>(T service) where T : class
    {
        (_services ??= [])[typeof(T)] = service;
    }

    /// <inheritdoc/>
    public void SetSelection(SyncPath path, out SyncPath rest)
    {
        rest = path;
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        OnExpandedGui(gui);
    }

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        var service = Context?.GetService(serviceType) ?? _services?.GetValueSafe(serviceType);
        if (service != null)
        {
            return service;
        }

        if (serviceType == typeof(IViewRefresh))
        {
            return this;
        }

        return null;
    }
}

/// <summary>
/// Wraps a value action as an undo/redo action for the sub-property grid.
/// </summary>
internal class SubGridActionWrapper(ImSubPropertyGrid grid, IValueAction action) : UndoRedoAction
{
    private readonly ImSubPropertyGrid _grid = grid ?? throw new ArgumentNullException(nameof(grid));
    private readonly IValueAction _action = action ?? throw new ArgumentNullException(nameof(action));

    /// <inheritdoc/>
    public override string Name => _action.ToString();

    /// <inheritdoc/>
    public override void Do()
    {
        _action.DoAction();

        EditorUtility.Inspector.UpdateInspector();
        _grid.NotifyObjectPropertyChanged(_action.ParentObjects, _action.Name ?? string.Empty);
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _action.UndoAction();

        EditorUtility.Inspector.UpdateInspector();
        _grid.NotifyObjectPropertyChanged(_action.ParentObjects, _action.Name ?? string.Empty);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _action.ToString();
    }
}
