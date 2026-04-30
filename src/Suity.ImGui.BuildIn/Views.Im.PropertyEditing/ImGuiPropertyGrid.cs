using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing.Targets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// An ImGui-based property grid that inspects and edits object properties.
/// Implements property display, context menus, toolbar, selection, and undo/redo integration.
/// </summary>
public class ImGuiPropertyGrid : IPropertyGrid,
    IServiceProvider,
    IViewRefresh
{
    private RootPropertyTarget? _rootTarget;

    private readonly PropertyGridData _gridData;
    private readonly GroupedResizerState _resizerState = new(150, 250);
    private readonly PropertyGridRootMenu _menu;

    private bool _showContextMenu = true;
    private bool _showToolBar = true;
    private bool _readOnly;
    private bool _init;
    private object[]? _objs;
    private readonly ImGuiNodeRef _guiRef = new();

    private Action<ImGuiNode>? _postGuiAction;

    private Dictionary<Type, object>? _services = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiPropertyGrid"/> class.
    /// </summary>
    /// <param name="name">The name used to identify this property grid instance.</param>
    public ImGuiPropertyGrid(string name)
    {
        _menu = new PropertyGridRootMenu(":" + name);
        EditorUtility.PrepareMenu(_menu);

        _gridData = new(name);
        _gridData.ValueActionRequest += _gridData_ValueActionRequest;
    }

    /// <inheritdoc/>
    public bool ShowContextMenu { get => _showContextMenu; set => _showContextMenu = value; }

    /// <inheritdoc/>
    public bool ShowToolBar { get => _showToolBar; set => _showToolBar = value; }

    /// <inheritdoc/>
    public bool HasTarget => _rootTarget != null;

    /// <inheritdoc/>
    public bool ReadOnly => _readOnly;

    /// <summary>
    /// Gets or sets the provider used to locate property editors for specific types.
    /// </summary>
    public IImGuiPropertyEditorProvider? Provider { get; set; }

    /// <inheritdoc/>
    public IInspectorContext? Context { get; private set; }

    /// <inheritdoc/>
    public event EventHandler? RequestRefresh;

    /// <inheritdoc/>
    public event EventHandler<UndoRedoActionEventArgs>? RequestDoAction;

    /// <inheritdoc/>
    public event EventHandler<ObjectPropertyEventArgs>? Edited;


    /// <inheritdoc/>
    public PropertyGridData GridData => _gridData;

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        if (!_init)
        {
            _init = true;

            if (_showContextMenu)
            {
                // Pre-generate menu
                (gui.Context as IGraphicContextMenu)?.RegisterContextMenu(_menu);
            }
        }

        ImGuiNode? gridNode = null;

        _guiRef.Node = gui.VerticalLayout("##property_grid_group")
        .InitFullWidth()
        .InitHeightRest()
        .InitValue(_resizerState)
        .InitValue(_gridData)
        .OnContent(() =>
        {
            ImGuiNode toolBoxNode;

            if (_showToolBar)
            {
                if (_rootTarget != null)
                {
                    toolBoxNode = gui.PropertyRow("#toolbox", _rootTarget, (n, t, c, p) =>
                    {
                        if (p.HasFlag(GuiPipeline.PreAction))
                        {
                            if (c == PropertyGridColumn.Prefix)
                            {
                                OnGuiToolBox_MultiColumn(gui);
                            }
                            else if (c == PropertyGridColumn.Name)
                            {
                                OnGuiToolBox_Condition(gui);
                            }
                        }

                        if (p.HasFlag(GuiPipeline.Main))
                        {
                            if (c == PropertyGridColumn.Main)
                            {
                                string brief = string.Empty;

                                var childValues = t.GetValues();
                                if (childValues.CountOne() && childValues.FirstOrDefault() is { } o)
                                {
                                    brief = EditorUtility.GetBriefStringL(o);
                                }
                                else
                                {
                                    brief = L($"{childValues.Count()} items");
                                }

                                gui.Text("#brief", brief)
                                .InitClass("brief")
                                .InitFullWidth()
                                .InitCenterVertical();
                            }
                            else if (c == PropertyGridColumn.Option)
                            {
                                if (_rootTarget?.GetValues()?.OnlyOneOfDefault() is { } o && o.ToToolTipsText() is string toolTips)
                                {
                                    gui.Button("#tooltips", CoreIconCache.Info)
                                    .InitClass("configBtn")
                                    .OnClick(n => 
                                    {
                                        (gui.Context as IGraphicToolTip)?.ShowToolTip(toolTips, (int)n.GlobalRect.X, (int)n.GlobalRect.Bottom);
                                    });
                                }
                            }
                        }
                    }).InitClass("toolBar");
                }
                else
                {
                    toolBoxNode = gui.PropertyRowFrame("#toolbox", (n, c, p) =>
                    {
                        if (c == PropertyGridColumn.Name && p.HasFlag(GuiPipeline.PreAction))
                        {
                            OnGuiToolBox_MultiColumn(gui);
                            OnGuiToolBox_Condition(gui);
                        }
                    }).InitClass("toolBar");
                }

                if (toolBoxNode.GetValue<PropertyRowData>() is { } editorValue)
                {
                    editorValue.SelectEnabled = false;
                }
            }

            gridNode = gui.PropertyFrame("##prop_frame", true, _gridData)
            .SetReadonly(_readOnly)
            .InitInputMouseUp(GuiMouseButtons.Right, n =>
            {
                var sel = _gridData.SelectedField;
                if (sel is { } && _showContextMenu)
                {
                    object?[] sels = [sel.Target];

                    _menu.ApplySender(this);
                    _menu.PopUp(1, [typeof(PropertyTarget)], typeof(PropertyTarget), sels);
                    (gui.Context as IGraphicContextMenu)?.ShowContextMenu(_menu, sels);
                }

                return GuiInputState.None;
            })
            .OnPartialContent(() =>
            {
                if (_rootTarget != null)
                {
                    gui.PropertyField(_rootTarget);
                }
            });
        });

        if (_postGuiAction is { } action && gridNode is { })
        {
            _postGuiAction = null;
            action(gridNode);
        }
    }

    #region Inspect

    /// <inheritdoc/>
    public void InspectObjects(IEnumerable<object> objs, bool readOnly = false, IInspectorContext? context = null, INodeReader? styles = null)
    {
        _objs = [.. objs];

        if (Context != context)
        {
            Context?.InspectorExit();
            Context = context;
            context?.InspectorEnter();
        }

        _readOnly = readOnly;

        var objAry = new object[_objs.Length];
        for (int i = 0; i < _objs.Length; i++)
        {
            var obj = _objs[i];

            if (obj is IInspectorRoute r)
            {
                try
                {
                    objAry[i] = EditorUtility.GetViewRedirectedObject(r, ViewIds.Inspector);
                    if (r.GetRoutedReadonly())
                    {
                        readOnly = true;
                    }
                    if (r.GetRoutedStyles() is { } routedStyles)
                    {
                        styles = routedStyles;
                    }
                }
                catch (Exception err)
                {
                    _objs = null;

                    err.LogError();

                    return;
                }
            }
            else
            {
                // Here we need to perform redirection handling first, because the redirected object may not implement the IInspectorRoute interface
                objAry[i] = EditorUtility.GetViewRedirectedObject(_objs[i], ViewIds.Inspector);

                //objAry[i] = obj;
            }
        }

        _rootTarget = new RootPropertyTarget(objAry)
        {
            ServiceProvider = this,
            Styles = styles,
            ReadOnly = _readOnly,
        };
    }

    /// <inheritdoc/>
    public IEnumerable<object> InspectedObjects => _objs ?? [];

    /// <summary>
    /// Determines whether the specified object is currently selected in the grid.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>Always returns false in the current implementation.</returns>
    public bool IsObjectSelected(object obj)
    {
        return false;
    }

    /// <inheritdoc/>
    public void SetSelection(SyncPath path, out SyncPath rest)
    {
        // Fixed empty path issue. Empty path caused forced selection of root node, then executing ScrollToPosition caused misalignment.
        if (SyncPath.IsNullOrEmpty(path))
        {
            rest = SyncPath.Empty;

            return;
        }

        var target = GetEditorBySyncPath(path, out rest);
        if (target is null)
        {
            return;
        }

        var data = target.GetOrCreatePropertyRowData();
        data.GridData = _gridData;

        _gridData.SetSelection(data);
        //ScrollToPosition(gui, data);

        _postGuiAction = gridNode =>
        {
            ScrollToPosition(gridNode, data);
        };
    }

    /// <summary>
    /// Updates the display of the currently selected objects.
    /// </summary>
    public void UpdateSelectedObjects()
    {
    }

    /// <summary>
    /// Retrieves a service from the inspector context.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <param name="nullable">If true, returns null when the service is not found; otherwise throws <see cref="NotImplementedException"/>.</param>
    /// <returns>The service instance, or null if not found and <paramref name="nullable"/> is true.</returns>
    public T? GetContextService<T>(bool nullable) where T : class
    {
        if (Context?.GetService(typeof(T)) is T obj)
        {
            return obj;
        }

        if (nullable)
        {
            return null;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Resolves a <see cref="PropertyTarget"/> by traversing the property hierarchy using a synchronization path.
    /// </summary>
    /// <param name="path">The synchronization path to resolve.</param>
    /// <param name="rest">The remaining portion of the path that could not be resolved.</param>
    /// <returns>The resolved property target, or null if resolution failed.</returns>
    private PropertyTarget? GetEditorBySyncPath(SyncPath path, out SyncPath rest)
    {
        rest = SyncPath.Empty;
        if (path is null)
        {
            return null;
        }

        if (path.Length == 0)
        {
            return _rootTarget;
        }

        PropertyTarget? target = _rootTarget;
        if (target is null)
        {
            return null;
        }

        PropertyTarget? childTarget = null;

        int i = 0;

        if (target.Path is SyncPath rootPath)
        {
            if (path.Match(0, rootPath))
            {
                i = rootPath.Length;
            }
            else
            {
                rest = path;

                return null;
            }
        }

        var provider = Provider ?? PropertyEditorProviderBK.Instance;

        while (i < path.Length)
        {
            if (target is { })
            {
            // Expand properties in a non-Gui manner
                if (target.PopulateProperties(provider))
                {
                    target.ExpandRequest = target.InitExpanded = true;
                    //ExpandPropertyTarget(target);
                }

                if (target.ArrayTarget is { } arrayTarget && path[i] is int index)
                {
                    childTarget = arrayTarget.GetOrCreateElementTarget(index);
                }
                else
                {
                    childTarget = target.Fields.FirstOrDefault(o =>
                    {
                        if (o.Path is SyncPath p && path.Match(i, p))
                        {
                            return true;
                        }
                        else if (path.Match(i, o.PropertyName))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
            }
            else
            {
                childTarget = null;
            }

            if (childTarget is null)
            {
                rest = path.SubPath(i, path.Length - i);
                return target;
            }

            target = childTarget;
            if (target.Path is SyncPath ePath)
            {
                i += ePath.Length;
            }
            else
            {
                i++;
            }
        }

        return target;
    }

    #endregion

    #region IServiceProvider

    /// <summary>
    /// Registers a service instance for the specified service type.
    /// </summary>
    /// <param name="serviceType">The type of service to register.</param>
    /// <param name="service">The service instance to register.</param>
    /// <exception cref="ArgumentException">Thrown when the service instance is not assignable to the specified service type.</exception>
    public void AddService(Type serviceType, object service)
    {
        if (!serviceType.IsAssignableFrom(service.GetType()))
        {
            throw new ArgumentException($"Service type {serviceType} is not assignable from {service.GetType()}");
        }

        (_services ??= [])[serviceType] = service;
    }

    /// <inheritdoc/>
    public void AddService<T>(T service) 
        where T : class
    {
        (_services ??= [])[typeof(T)] = service;
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType)
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



    #endregion

    #region IViewRefresh

    /// <inheritdoc/>
    public void QueueRefreshView()
    {
        _guiRef.QueueRefresh(true);
    }

    #endregion

    /// <inheritdoc/>
    public void DoAction(IValueAction action)
    {
        _gridData.DoAction(action);
        RequestRefresh?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Notifies listeners that a property on the inspected objects has changed.
    /// </summary>
    /// <param name="objs">The objects whose property was edited.</param>
    /// <param name="name">The name of the property that changed.</param>
    internal void NotifyObjectPropertyChanged(IEnumerable<object> objs, string name)
    {
        Context?.InspectorObjectEdited(objs, name);
        Edited?.Invoke(this, new ObjectPropertyEventArgs([.. objs], name));

        if (_rootTarget is { } target)
        {
            foreach (var value in target.Values.OfType<IInspectorEditNotify>())
            {
                value.NotifyInspectorEdited();
            }
        }

        RequestRefresh?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Triggers a refresh request for the property grid.
    /// </summary>
    internal void NotifyRequestRefresh()
    {
        RequestRefresh?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles value action requests from the grid data, routing them through undo/redo or the inspector context.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments containing the action to perform.</param>
    private void _gridData_ValueActionRequest(object sender, ValueActionEventArgs e)
    {
        PropertyGridActionWrapper action = null;

        if (e.Action.Preview)
        {
            e.Action.DoAction();
            e.Handled = true;

            NotifyObjectPropertyChanged(e.Action.ParentObjects, e.Action.Name ?? string.Empty);
            RequestRefresh?.Invoke(this, EventArgs.Empty);
            _guiRef.QueueRefresh();

            return;
        }

        if (RequestDoAction != null)
        {
            action ??= new PropertyGridActionWrapper(this, e.Action);
            var args = new UndoRedoActionEventArgs(action);

            RequestDoAction(this, args);
            // Set Handled on success to avoid repeated execution
            e.Handled = args.Handled;
        }

        if (!e.Handled && Context != null)
        {
            action ??= new PropertyGridActionWrapper(this, e.Action);
            // Set Handled on success to avoid repeated execution
            e.Handled = Context.InspectorDoAction(action);
        }

        if (!e.Handled)
        {
            e.Action.DoAction();
            // Set Handled on success to avoid repeated execution
            e.Handled = true;
        }

        NotifyObjectPropertyChanged(e.Action.ParentObjects, e.Action.Name ?? string.Empty);
        RequestRefresh?.Invoke(this, EventArgs.Empty);
        _guiRef.QueueRefresh();
    }

    /// <summary>
    /// Renders the multi-column toggle button in the toolbar.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    private void OnGuiToolBox_MultiColumn(ImGui gui)
    {
        gui.ToggleButton("#multiColumn", ImGuiIcons.Column, _gridData.SupportMultipleColumn)
        .InitClass("configBtn")
        .SetToolTipsL("Bind multiple objects of the same type")
        .OnChecked((n, v) =>
        {
            _gridData.SupportMultipleColumn = v;
        });
    }

    /// <summary>
    /// Renders the condition selection button and current condition label in the toolbar.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    private void OnGuiToolBox_Condition(ImGui gui)
    {
        //if (!ImGuiServices._license!.GetCapability(EditorCapabilities.ValueCondition))
        //{
        //    return;
        //}

        gui.Button("#condition", ImGuiIcons.Condition)
        .InitClass("configBtn")
        .SetToolTipsL("Data condition branching")
        .InitInputFunctionChain(ConditionInput);

        string? condition = Context?.GetService<IConditionSelection>()?.SelectedCondition;
        if (!string.IsNullOrWhiteSpace(condition))
        {
            gui.Text("#currentCondition", condition!)
            .InitClass("brief")
            .InitCenterVertical();
        }
    }

    /// <summary>
    /// Handles input events for the condition selection button, showing a dropdown to select a condition.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline stage.</param>
    /// <param name="node">The ImGui node receiving the input.</param>
    /// <param name="input">The input event data.</param>
    /// <param name="baseAction">The base input processing function.</param>
    /// <returns>The resulting input state after processing.</returns>
    private GuiInputState ConditionInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (node.IsDisabled)
        {
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && !node.IsReadOnly && node.Gui.Context is IGraphicDropDownEdit dropDownEdit)
                {
                    var rect = node.GlobalRect;
                    var dropDownRect = new RectangleF(rect.X, rect.Bottom, 150, 100);

                    var sel = Context?.GetService<IConditionSelection>();
                    if (sel is null)
                    {
                        break;
                    }

                    var conditions = sel.Conditions;
                    if (conditions is null)
                    {
                        break;
                    }

                    conditions = conditions.Where(s => !string.IsNullOrWhiteSpace(s));
                    if (!conditions.Any())
                    {
                        break;
                    }

                    string[] values = [string.Empty, .. conditions];

                    string currentValue = sel.SelectedCondition;
                    if (string.IsNullOrWhiteSpace(currentValue))
                    {
                        currentValue = string.Empty;
                    }

                    dropDownEdit.ShowComboBoxDropDown(dropDownRect.ToInt(), values, currentValue, obj =>
                    {
                        sel.SelectedCondition = obj?.ToString();
                        node.QueueRefresh();
                        Context?.InspectorEditFinish();
                    });
                }

                ImGui.MergeState(ref state, GuiInputState.None);
                break;

            default:
                ImGui.MergeState(ref state, GuiInputState.None);
                break;
        }

        return state;
    }

    /// <summary>
    /// Moves the selection to the previous selectable property row.
    /// </summary>
    /// <param name="gridNode">The grid ImGui node.</param>
    /// <param name="field">The currently selected property row data.</param>
    /// <param name="data">The property grid data model.</param>
    private static void HandleMoveUp(ImGuiNode gridNode, PropertyRowData field, PropertyGridData data)
    {
        var path = field.NodePath;
        if (path is null)
        {
            return;
        }

        var node = gridNode.Gui.FindNode(path);
        if (node is null)
        {
            return;
        }

        ImGuiNode? prevNode = node.Previous;
        PropertyRowData? prevField = null;

        while (prevNode != null)
        {
            prevField = prevNode.GetValue<PropertyRowData>();

            if (prevField is { SelectEnabled: true })
            {
                break;
            }

            prevNode = prevNode.Previous;
        }

        if (prevNode is { } && prevField is { SelectEnabled: true })
        {
            data.SetSelection(prevField);
            gridNode.ScrollToPositionY(prevNode.GlobalRect, true);
            //TODO : Change to no refresh
            gridNode.QueueRefresh();
        }
    }

    /// <summary>
    /// Moves the selection to the next selectable property row.
    /// </summary>
    /// <param name="gridNode">The grid ImGui node.</param>
    /// <param name="field">The currently selected property row data.</param>
    /// <param name="data">The property grid data model.</param>
    private static void HandleMoveDown(ImGuiNode gridNode, PropertyRowData field, PropertyGridData data)
    {
        var path = field.NodePath;
        if (path is null)
        {
            return;
        }

        var node = gridNode.Gui.FindNode(path);
        if (node is null)
        {
            return;
        }

        ImGuiNode? nextNode = node.Next;
        PropertyRowData? nextField = null;

        while (nextNode != null)
        {
            nextField = nextNode.GetValue<PropertyRowData>();

            if (nextField is { SelectEnabled: true })
            {
                break;
            }

            nextNode = nextNode.Next;
        }

        if (nextNode is { } && nextField is { SelectEnabled: true })
        {
            data.SetSelection(nextField);
            gridNode.ScrollToPositionY(nextNode.GlobalRect, true);
            //TODO : Change to no refresh
            gridNode.QueueRefresh();
        }
    }

    /// <summary>
    /// Scrolls the grid view to make the specified property field visible.
    /// </summary>
    /// <param name="gridNode">The grid ImGui node.</param>
    /// <param name="field">The property row data to scroll to.</param>
    private void ScrollToPosition(ImGuiNode gridNode, PropertyRowData field)
    {
        var path = field.NodePath;
        if (path is null)
        {
            return;
        }

        var gridPath = _gridData.GridNodePath;
        if (gridPath is null)
        {
            return;
        }

        var node = gridNode.Gui.FindNode(path);
        if (node is null)
        {
            return;
        }

        gridNode.ScrollToPositionY(node.GlobalRect, true);

        //TODO : Change to no refresh
        //gui.QueueRefresh();
        QueuedAction.Do(() =>
        {
            gridNode.QueueRefresh();
        });
    }

    /// <summary>
    /// Finds the ImGui node associated with the specified property target.
    /// </summary>
    /// <param name="gui">The ImGui instance to search within.</param>
    /// <param name="target">The property target to locate.</param>
    /// <returns>The associated ImGuiNode, or null if not found.</returns>
    private ImGuiNode? FindImGuiNode(ImGui gui, PropertyTarget target)
    {
        var field = target.FieldGuiData as PropertyRowData;
        if (field is null)
        {
            return null;
        }

        var path = field.NodePath;
        if (path is null)
        {
            return null;
        }

        return gui.FindNode(path);
    }
}
