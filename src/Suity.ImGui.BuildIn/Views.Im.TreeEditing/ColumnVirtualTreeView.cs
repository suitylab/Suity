using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Services;
using Suity.Editor.VirtualTree;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents a column-based virtual tree view with multiple columns including name, description, and preview columns.
/// Supports preview presets for saving and restoring column configurations.
/// </summary>
public class ColumnVirtualTreeView : ImGuiVirtualTreeView, IHasPreviewPreset
{
    /// <summary>
    /// The name of the default preview preset.
    /// </summary>
    public const string DefaultPresetName = "_default";

    private readonly Column3Template<VirtualNode> _template;

    private readonly HeaderMenu _headerMenu;

    private readonly Dictionary<string, PreviewPreset> _previewPresets = [];
    private string _currentPresetName = DefaultPresetName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnVirtualTreeView"/> class with a view ID.
    /// </summary>
    /// <param name="viewId">The ID of the view.</param>
    /// <param name="menuName">Optional name for the context menu.</param>
    public ColumnVirtualTreeView(int viewId, string? menuName = null)
        : this(new ImGuiVirtualTreeModel { ViewId = viewId }, menuName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnVirtualTreeView"/> class with a tree model.
    /// </summary>
    /// <param name="model">The tree model to display.</param>
    /// <param name="menuName">Optional name for the context menu.</param>
    public ColumnVirtualTreeView(IImGuiTreeModel<VirtualNode> model, string? menuName = null)
        : base(model, menuName)
    {
        _template = new Column3Template<VirtualNode>()
        {
            RowPipeline = DrawRowPipeline,
            BeginEditAction = ConfigBeginEdit,
        };

        _template.NameColumn.RowGui = DrawNameColumn;
        _template.DescriptionColumn.RowGui = DrawDescriptionColumn;
        _template.PreviewColumn.RowGui = DrawPreviewColumn;

        this.ViewTemplate = _template;

        _previewPresets[DefaultPresetName] = new PreviewPreset { Name = DefaultPresetName };

        _headerMenu = new HeaderMenu(this);
    }

    /// <summary>
    /// Gets the column template used for rendering the tree view.
    /// </summary>
    public Column3Template<VirtualNode> Column => _template;


    #region IHasPreviewPreset

    /// <inheritdoc/>
    public string CurrentPresetName => _currentPresetName;

    /// <inheritdoc/>
    public PreviewPreset CreatePreset()
    {
        var preset = new PreviewPreset();
        preset.ColumnWidths.AddRange(Column.GetColumnWidths());
        preset.SetPreviewPaths(VirtualModel?.PreviewPaths);

        return preset;
    }

    /// <inheritdoc/>
    public void ApplyPreset(PreviewPreset preset)
    {
        if (preset is null)
        {
            return;
        }

        Column.SetColumnWidths([.. preset.ColumnWidths]);
        if (preset.HasPreviewPath())
        {
            var paths = preset.GetPreviewPaths();
            AddPreviewPaths(paths);
        }
    }

    /// <inheritdoc/>
    public void RestorePresets(IEnumerable<PreviewPreset> presets)
    {
        _previewPresets.RemoveAllByKey(s => s != DefaultPresetName);

        foreach (var preset in presets)
        {
            if (string.IsNullOrWhiteSpace(preset.Name))
            {
                continue;
            }

            _previewPresets[preset.Name] = preset;
        }

        _headerMenu.PresetMenu.MarkDirty();
    }

    /// <inheritdoc/>
    public IEnumerable<PreviewPreset> GetAllPresets() => _previewPresets.Values;

    /// <inheritdoc/>
    public void MarkPreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var preset = CreatePreset();
        preset.Name = name;
        _previewPresets[name] = preset;
    }

    /// <inheritdoc/>
    public void MarkCurrentPreset()
    {
        if (!string.IsNullOrWhiteSpace(_currentPresetName))
        {
            MarkPreset(_currentPresetName);
        }
    }

    /// <inheritdoc/>
    public void ChangePreset(string name, bool markCurrent)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (markCurrent)
        {
            MarkPreset(_currentPresetName);
        }

        var preset = _previewPresets.GetValueSafe(name);
        if (preset != null)
        {
            _currentPresetName = name;
            ApplyPreset(preset);
        }
    }

    /// <inheritdoc/>
    public void RemovePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }
        if (_currentPresetName == DefaultPresetName)
        {
            return;
        }

        _previewPresets.Remove(name);

        if (_currentPresetName == name)
        {
            ChangePreset(DefaultPresetName, false);
        }
    }

    #endregion

    #region Protected & Override

    /// <summary>
    /// Draws the row pipeline for a tree node at the specified pipeline stage.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual node data.</param>
    /// <param name="pipeline">The current pipeline stage.</param>
    protected virtual void DrawRowPipeline(ImGuiNode node, VirtualNode vNode, EditorImGuiPipeline pipeline)
    {
        switch (pipeline)
        {
            case EditorImGuiPipeline.Normal:
                node.SetReadonly(vNode.ReadOnly)
                .OnTreeNodeDragStart(n =>
                {
                    if (TreeData is { } treeData && treeData.SelectedNodesT.Any())
                    {
                        return new VirtualTreeDragData(treeData.SelectedNodesT);
                    }
                    else
                    {
                        return null;
                    }
                })
                .OnTreeNodeDragOver((dropEvent, mode) =>
                {
                    node.HandleDragOver(this, dropEvent, mode);
                })
                .OnTreeNodeDragDrop((dropEvent, mode) =>
                {
                    node.HandleDragDrop(this, dropEvent, mode);
                });
                break;

            case EditorImGuiPipeline.Begin:
                {
                    var rootTarget = vNode.Tag as PropertyTarget;
                    if (rootTarget is null || !ReferenceEquals(rootTarget.GetValues().FirstOrDefault(), vNode.DisplayedValue))
                    {
                        vNode.Tag = rootTarget = CreateRootTarget(vNode.DisplayedValue);
                    }

                    // Reset Target at start
                    if (rootTarget != null)
                    {
                        rootTarget.ClearFields();
                        rootTarget.PopulatePath(SyncPath.Empty, true);
                    }
                }
                break;
        }
    }




    /// <summary>
    /// Configures the node to begin editing, focusing on the title edit input.
    /// </summary>
    /// <param name="node">The ImGui node to configure for editing.</param>
    protected virtual void ConfigBeginEdit(ImGuiNode node)
    {
        var titleStringInputNode = node.GetNodeAt(0)?.GetChildNode("##title_edit");
        if (titleStringInputNode is { })
        {
            titleStringInputNode.BeginEdit();
        }
    }

    /// <inheritdoc/>
    protected override void OnColumnRemoved(int index)
    {
        base.OnColumnRemoved(index);

        _template.RemoveColumn(index + 3);
    }

    /// <inheritdoc/>
    protected override void OnColumnSwap(int index, int indexTo)
    {
        base.OnColumnSwap(index, indexTo);

        _template.SwapColumn(index + 3, indexTo + 3);
    }

    /// <inheritdoc/>
    protected override void OnColumnRemoveInsert(int indexFrom, int indexInsert)
    {
        base.OnColumnRemoveInsert(indexFrom, indexInsert);

        _template.RemoveInsertColumn(indexFrom + 3, indexInsert + 3);
    }

    /// <inheritdoc/>
    protected override void OnColumnUpdated()
    {
        base.OnColumnUpdated();

        if (VirtualModel is not { } model)
        {
            return;
        }

        bool showDesc = EditorUtility.ShowAsDescription.Value;

        var paths = model.PreviewPaths;
        _template.ColumnConfigs.Count = 3 + paths.Count;

        // Get existing custom columns (skip the 3 default columns: name, description, preview)
        var currentColumns = _template.ColumnConfigs.Columns
            .OfType<ColumnConfig<VirtualNode>>()
            .Where(o => o.Tag != null)
            .ToArray();

        // Update or create column configs for each preview path
        for (int i = 0; i < paths.Count; i++)
        {
            var path = paths[i];

            var column = currentColumns.FirstOrDefault(o => Equals(o?.Tag, path));
            if (column != null)
            {
                // Reuse existing column config, preserve user settings
                _template.ColumnConfigs[3 + i] = column;
                column.Index = 3 + i;
                column.Enabled = true;
            }
            else
            {
                // Create new column config for this preview path
                column = _template.ColumnConfigs[3 + i] = new ColumnConfig<VirtualNode>
                {
                    Index = 3 + i,
                    Title = showDesc ? path.DisplayName : path.Name,
                    Enabled = true,
                    Tag = path,
                };

                column.HeaderGui = CreateColumnHeaderGui(column);
                column.RowGui = CreateColumnRowGui(column);
            }
        }

        QueueRefresh();
    }

    #endregion

    private Action<ImGuiNode> CreateColumnHeaderGui(ColumnConfig<VirtualNode> column)
    {
        return node =>
        {
            int index = column.Index - 3;

            node
            .InitRenderFunction("Frame")
            .InitRenderFunctionChain(ColumnHeaderDragDropRender)
            .InitInputFunctionChain(ColumnHeaderDragDropInput)
            .SetValueFluent(column)
            .InitOverrideBorder(0);

            var path = VirtualModel?.PreviewPaths[index];
            if (path is null)
            {
                return;
            }

            bool hasColor = false;

            if (column.Color is { } color)
            {
                node
                .OverrideBorder(2, color)
                //.OverrideColor(color)
                .OverrideCorner(3)
                .OverridePadding(3);

                hasColor = true;
            }
            else
            {
                node.RemoveValue<GuiColorStyle>();
                node.RemoveValue<GuiFrameStyle>();
                node.RemoveValue<GuiPaddingStyle>();
            }

            string name = EditorUtility.ShowAsDescription.Value ? path.DisplayName : path.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = path.Name;
            }

            var gui = node.Gui;

            node.InitMenu(_headerMenu, index);

            if (column.Icon is Image icon)
            {
                gui.Image("#i", icon)
                .InitClass("icon");
            }

            gui.Text("#t", name ?? string.Empty)
            .InitVerticalAlignment(GuiAlignment.Center, true)
            .SetToolTipsL(EditorUtility.GetBriefString(path.Path))
            .SetFontColor(Color.White);
            //.SetFontColor(hasColor ? Color.Black : Color.White);
        };
    }

    private Action<ImGuiNode, VirtualNode> CreateColumnRowGui(ColumnConfig<VirtualNode> column)
    {
        return (node, vNode) =>
        {
            // There are 3 default non-preview fields
            int index = column.Index - 3;

            var path = VirtualModel?.PreviewPaths[index];
            if (path is null)
            {
                return;
            }

            var rootTarget = vNode.Tag as PropertyTarget;

            // Set forceRepopulate to false to prevent repeated generation of sub-Targets, especially when multiple Columns are set which severely impacts performance
            var target = rootTarget?.PopulatePath(path.Path, false);
            if (target is not null)
            {
                // Compensate for missing Column info, this will cause moderate lag
                column.Color = target.Color;
                column.Icon = target.Icon;

                node.InitOverridePadding(1);

                var gui = node.Gui;

                gui.PropertyEditor(target, act =>
                {
                    RaiseRequestDoAction(act);

                    // Highlight in Inspector
                    if (column.Tag is PreviewPath p)
                    {
                        EditorUtility.Inspector.SetSelection(p.Path, out _, true);
                    }
                });
            }
        };
    }

    private PropertyTarget CreateRootTarget(object obj, int viewId = 0)
    {
        object[] objs = [obj];
        var target = PropertyTargetUtility.CreatePropertyTarget(objs);
        target.ViewId = viewId;
        target.ServiceProvider = this;

        // Try CacheValues to improve rendering efficiency
        //target.CacheValues = false;

        return target;
    }


    #region Imgui System Chain

    private GuiInputState ColumnHeaderDragDropInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        if (state == GuiInputState.FullSync)
        {
            return state;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                // Start drag control when left mouse button is pressed on column header
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    if (node.Gui.ControllingNode is null)
                    {
                        node.SetIsControlling(true);
                    }

                    ImGui.MergeState(ref state, GuiInputState.Render);
                }
                break;

            case GuiEventTypes.MouseUp:
                // End drag and reorder columns when mouse is released
                if (input.MouseButton == GuiMouseButtons.Left)
                {
                    node.SetIsControlling(false);
                    ImGui.MergeState(ref state, GuiInputState.Render);

                    // Find the column header under the mouse cursor
                    var column = node.Gui.MouseInNodes
                        .Select(o => o.GetValue<ColumnConfig<VirtualNode>>())
                        .OfType<ColumnConfig<VirtualNode>>()
                        .FirstOrDefault();

                    if (column is null)
                    {
                        break;
                    }

                    var draggable = node.Parent?.GetValue<HeaderDragValue>();
                    if (draggable != null && draggable.DraggingColumn != null)
                    {
                        node.Parent?.RemoveValue<HeaderDragValue>();

                        if (draggable.DraggingColumn != column)
                        {
                            int indexFrom = draggable.DraggingColumn.Index - 3;
                            int indexTo = column.Index - 3;
                            if (draggable.After)
                            {
                                indexTo++;
                            }

                            // Execute column reorder operation
                            RemoveInsertPreviewPath(indexFrom, indexTo);
                        }
                    }
                    else
                    {
                        // No drag occurred, treat as click to navigate to the path
                        if (column.Tag is PreviewPath p)
                        {
                            QueuedAction.Do(() =>
                            {
                                EditorUtility.Inspector.SetSelection(p.Path, out _, true);
                            });
                        }
                    }
                }
                break;

            case GuiEventTypes.MouseMove:
                // Initiate column drag when mouse moves beyond threshold distance
                if (node.IsControlling && input.MouseLocation is { } pos)
                {
                    int offset = Math.Abs(pos.X - node.Gui.LastMouseDownLocation.X) +
                                Math.Abs(pos.Y - node.Gui.LastMouseDownLocation.Y);

                    if (offset >= 10)
                    {
                        var column = node.GetValue<ColumnConfig<VirtualNode>>();
                        var draggable = node.Parent?.GetOrCreateValue<HeaderDragValue>();
                        if (column is { } && draggable is { })
                        {
                            draggable.DraggingColumn = column;
                            ImGui.MergeState(ref state, GuiInputState.Layout);
                        }
                    }
                }
                break;
        }

        return state;
    }

    private static readonly Brush _dragDropBrush = new SolidBrush(ImGuiTheme.DefaultDragColor);

    private static void ColumnHeaderDragDropRender(GuiPipeline pipeLine, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeLine);

        var column = node.GetValue<ColumnConfig<VirtualNode>>();
        if (column is null)
        {
            return;
        }

        var draggable = node.Parent?.GetValue<HeaderDragValue>();
        if (draggable is null)
        {
            return;
        }

        // Don't render indicator on the column being dragged
        if (draggable.DraggingColumn == column)
        {
            return;
        }

        if (!(node.Gui.Input.MouseLocation is { } pos))
        {
            return;
        }

        var rect = node.GlobalRect;
        if (!rect.Contains(pos))
        {
            return;
        }

        // Calculate left and right drop zones (half-width regions at edges)
        float h2 = rect.Width * 0.5f;
        float left = rect.Left + h2;
        float right = rect.Right - h2;

        // +1 is reserved space for the expand button
        float indent = 1;
        float y = rect.Y + indent;
        float h = rect.Height - indent;

        // Draw drop indicator based on mouse position relative to column center
        if (pos.X < left)
        {
            // Drop before this column
            output.FillRectangle(_dragDropBrush, new RectangleF(rect.X, y, 3, h));
            draggable.After = false;
        }
        else if (pos.X > right)
        {
            // Drop after this column
            output.FillRectangle(_dragDropBrush, new RectangleF(rect.Right - 3, y, 3, h));
            draggable.After = true;
        }
    }

    private class HeaderDragValue
    {
        public ColumnConfig<VirtualNode>? DraggingColumn { get; set; }
        public bool After { get; set; }
    }

    #endregion

    #region Menus

    private class HeaderMenu : RootMenuCommand
    {
        private readonly ColumnVirtualTreeView _treeView;

        private readonly HeaderPresetMenu _presetMenu;

        public HeaderMenu(ColumnVirtualTreeView treeView)
        {
            _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));

            _presetMenu = new HeaderPresetMenu(treeView);

            AddCommand("Move Left", CoreIconCache.Left, c =>
            {
                if (Sender is int index)
                {
                    _treeView.SwapPreviewPath(index, index - 1);
                }
            });

            AddCommand("Move Right", CoreIconCache.Right, c =>
            {
                if (Sender is int index)
                {
                    _treeView.SwapPreviewPath(index, index + 1);
                }
            });

            AddCommand("Remove", CoreIconCache.Delete, c =>
            {
                if (Sender is int index)
                {
                    _treeView.RemovePreviewPathAt(index);
                }
            });

            AddSeparator();

            AddCommand(_presetMenu);

            AddCommand("Add Preset", CoreIconCache.Add, async c =>
            {
                string name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Preset Name", string.Empty, s => true);
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                _treeView.MarkPreset(name);
                _treeView.ChangePreset(name, false);
                _presetMenu.MarkDirty();
            });

            AddCommand("Delete Current Preset", CoreIconCache.Remove, c =>
            {
                if (string.IsNullOrWhiteSpace(_treeView.CurrentPresetName))
                {
                    return;
                }

                if (_treeView.CurrentPresetName == ColumnVirtualTreeView.DefaultPresetName)
                {
                    DialogUtility.ShowMessageBoxAsyncL("Cannot delete default preset");
                    return;
                }

                _treeView.RemovePreset(_treeView.CurrentPresetName);
                _presetMenu.MarkDirty();
            });

            //Random rnd = new Random();

            //RegisterCommand($"AA{rnd.Next(10000)}", CoreIconCache.Delete, c =>
            //{
            //});
        }

        public HeaderPresetMenu PresetMenu => _presetMenu;

        protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
        {
            base.OnPopUp(selectionCount, types, commonNodeType);
        }
    }

    private class HeaderPresetMenu : MenuCommand
    {
        private readonly ColumnVirtualTreeView _treeView;

        public HeaderPresetMenu(ColumnVirtualTreeView treeView)
            : base("Presets")
        {
            _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));

            MarkDirty();
        }

        public void MarkDirty()
        {
            Clear();

            foreach (var preset in _treeView.GetAllPresets())
            {
                AddCommand(new HeaderPresetItemMenu(_treeView, preset.Name));
            }
        }

        protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
        {
            base.OnPopUp(selectionCount, types, commonNodeType);
        }
    }

    private class HeaderPresetItemMenu : MenuCommand
    {
        private readonly ColumnVirtualTreeView _treeView;

        public HeaderPresetItemMenu(ColumnVirtualTreeView treeView, string name)
            : base(name)
        {
            _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
        }

        protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
        {
            base.OnPopUp(selectionCount, types, commonNodeType);

            if (this.Text == _treeView.CurrentPresetName)
            {
                this.Icon = CoreIconCache.Check;
            }
            else
            {
                this.Icon = null;
            }
        }

        protected override void OnDropDown()
        {
            base.OnDropDown();
        }

        public override void DoCommand()
        {
            _treeView.ChangePreset(this.Text, true);
        }
    }

    #endregion
}