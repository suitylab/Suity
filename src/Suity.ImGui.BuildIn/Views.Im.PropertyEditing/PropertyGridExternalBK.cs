using Suity.Collections;
using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides an extended implementation of the property grid with support for custom drawers,
/// keyboard navigation, and specialized UI rendering for property rows, groups, labels, and buttons.
/// </summary>
internal class PropertyGridExternalBK : PropertyGridExternal
{
    /// <summary>
    /// Gets or sets a value indicating whether small color rectangles are rendered in the prefix column.
    /// </summary>
    public static bool SmallRectColor = true;

    /// <summary>
    /// Gets or sets a value indicating whether large color rectangles are rendered in the name column.
    /// </summary>
    public static bool LargeRectColor = false;

    /// <summary>
    /// Gets the singleton instance of <see cref="PropertyGridExternalBK"/>.
    /// </summary>
    public static PropertyGridExternalBK Instance { get; } = new();

    private List<ImGuiPropertyDrawer> _customDrawerList;
    private Dictionary<string, ImGuiPropertyDrawer>? _customDrawers;


    internal bool _customDrawerEnabled = true;


    /// <summary>
    /// Determines whether any custom drawers are registered.
    /// </summary>
    /// <returns><c>true</c> if custom drawers are available; otherwise, <c>false</c>.</returns>
    internal bool HasCustomDrawer()
    {
        if (_customDrawers is null)
        {
            InitializeCustomDrawers();
        }

        return _customDrawers?.Count > 0;
    }

    /// <summary>
    /// Retrieves a custom drawer by its registered name.
    /// </summary>
    /// <param name="name">The name of the custom drawer to retrieve.</param>
    /// <returns>The custom drawer if found and enabled; otherwise, <c>null</c>.</returns>
    internal ImGuiPropertyDrawer? GetCustromDrawer(string? name)
    {
        if (!_customDrawerEnabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (_customDrawers is null)
        {
            InitializeCustomDrawers();
        }

        return _customDrawers?.GetValueSafe(name);
    }

    /// <summary>
    /// Initializes the custom drawer collection by scanning all available types derived from <see cref="ImGuiPropertyDrawer"/>
    /// and registering those decorated with <see cref="InsertIntoAttribute"/>.
    /// </summary>
    private void InitializeCustomDrawers()
    {
        _customDrawerList = [];
        _customDrawers = [];

        foreach (var type in typeof(ImGuiPropertyDrawer).GetAvailableClassTypes())
        {
            var inserts = type.GetAttributesCached<InsertIntoAttribute>().ToArray();
            if (inserts.Length == 0)
            {
                continue;
            }

            try
            {
                if (Activator.CreateInstance(type) is ImGuiPropertyDrawer drawer)
                {
                    _customDrawerList.Add(drawer);

                    foreach (var insert in inserts)
                    {
                        string name = insert.Position;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            if (!_customDrawers.ContainsKey(name))
                            {
                                _customDrawers.Add(name, drawer);
                            }
                            else
                            {
                                Logs.LogWarning($"CustomPropertyRowDrawer for '{name}' is already registered.");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }


    /// <inheritdoc/>
    public override IPropertyGrid CreatePropertyGrid(string name)
    {
        return new ImGuiPropertyGrid(name);
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyFrame(ImGui gui, bool scrollable)
    {
        string id = $"##property_grid{gui.CurrentNode.CurrentLayoutIndex}";
        return PropertyFrame(gui, id, scrollable);
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyFrame(ImGui gui, string id, bool scrollable)
    {
        ImGuiNode node;
        if (scrollable)
        {
            node = gui.ScrollableFrame(id, GuiOrientation.Vertical)
                .InitClass("propScroll", "propBox");
        }
        else
        {
            node = gui.Frame(id)
                .InitClass("propBox");
        }

        if (node.IsInitializing)
        {
            node.InitFullWidth();
            node.GetOrCreateValue(() => new GroupedResizerState(160, 400));

            var initGridData = node.GetOrCreateValue<PropertyGridData>();
            initGridData.GridNodePath = node.FullPath;
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyFrame(ImGui gui, string id, bool scrollable, PropertyGridData initGridData, GroupedResizerState? initResizerState = null)
    {
        ImGuiNode node;
        if (scrollable)
        {
            node = gui.ScrollableFrame(id, GuiOrientation.Vertical)
                .InitClass("propScroll", "propBox");
        }
        else
        {
            node = gui.Frame(id)
                .InitClass("propBox");
        }

        if (node.IsInitializing)
        {
            node.InitFullWidth();
            node.SetValue(initGridData);
            if (initResizerState is { })
            {
                node.SetValue(initResizerState);
            }

            initGridData.GridNodePath = node.FullPath;

            node.InitKeyDownInput((n, input) =>
             {
                 if (initGridData.SelectedField is { } value)
                 {
                     switch (input.KeyCode)
                     {
                         case "Up":
                             HandleMoveUp(n, value, initGridData);
                             input.Handled = true;
                             return GuiInputState.Render;

                         case "Down":
                             HandleMoveDown(n, value, initGridData);
                             input.Handled = true;
                             return GuiInputState.Render;

                         default:
                             //TODO: Cannot recognize whether the key is used here
                             value.KeyDownRequest = input.KeyCode;
                             return GuiInputState.Render;
                     }
                 }

                 return null;
             });
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyBox(ImGui gui, string id, string title, bool initExpand = true)
    {
        return gui.ExpandablePanel(id, title, initExpand)
            .InitFullWidth()
            .InitClass("propBox", "propHeader");
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyRow(ImGui gui, PropertyTarget target, PropertyEditorFunction? func, PropertyRowAction? rowAction)
    {
        return PropertyRow(gui, target, (n, t0, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                if (column == PropertyGridColumn.Main)
                {
                    func?.Invoke(gui, t0, act => n.DoValueAction(act));
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyRow(ImGui gui, PropertyTarget target, PropertyTargetAction? targetAction = null) 
        => PropertyRow(gui, target.Id, target, targetAction);

    /// <inheritdoc/>
    public override ImGuiNode PropertyRow(ImGui gui, string id, PropertyTarget target, PropertyTargetAction? targetAction = null)
    {
        ImGuiNode node = PropertyRowFrame(gui, id, (n, column, pipeline) =>
        {
            var rowData = n.GetOrCreatePropertyRowData(target);

            if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
            {
                try
                {
                    if (drawer.DrawPropertyRow(n, id, target, targetAction))
                    {
                        return;
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        {
                            if (target.Color is { } color)
                            {
                                if (SmallRectColor)
                                {
                                    var f = gui.Rect("#color")
                                    .SetWidth(5)
                                    //.SetFullHeight()
                                    .OverrideColor(color);
                                }
                            }

                            n.SetColor(Color.Red);

                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            targetAction?.Invoke(n, target, column, GuiPipeline.Main | GuiPipeline.PostAction);
                            break;
                        }

                    case PropertyGridColumn.Name:
                        {
                            n.InitRenderFunction("Frame")
                            .InitOverrideBorder(0);

                            if (target.ErrorInHierarchy)
                            {
                                gui.Image("#error", CoreIconCache.Error)
                                .InitClass("icon");
                            }

                            bool dark = false;

                            if (target.Color is { } color)
                            {
                                if (LargeRectColor)
                                {
                                    n.OverrideColor(color)
                                    .OverrideCorner(3);
                                    dark = true;
                                }
                            }
                            else
                            {
                                n.RemoveValue<GuiColorStyle>();
                                n.RemoveValue<GuiFrameStyle>();
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            if (!target.IsRoot && !target.HideTitle)
                            {
                                PropertyTitle(gui, target, dark);
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.Main);

                            if (target.ToolTips is { } toolTip && !string.IsNullOrWhiteSpace(toolTip))
                            {
                                gui.Button("#tooltips", CoreIconCache.Info)
                                .InitClass("configBtn")
                                .InitInputFunction(ToolTipButtonInput)
                                .OnClick(n =>
                                {
                                    (gui.Context as IGraphicToolTip)?.ShowToolTip(toolTip, (int)n.GlobalRect.X, (int)n.GlobalRect.Bottom);
                                }, true);
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.PostAction);
                            break;
                        }

                    case PropertyGridColumn.Main:
                        {
                            PropertyMainColumn(gui, target, targetAction, n, column);
                            break;
                        }

                    case PropertyGridColumn.Option:
                        {
                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            targetAction?.Invoke(n, target, column, GuiPipeline.Main | GuiPipeline.PostAction);

                            if (target.GetValues().FirstOrDefault() is IDrawEditorImGui draw)
                            {
                                draw.OnEditorGui(gui, EditorImGuiPipeline.Option, target);
                            }

                            if (target.Navigation && n.Parent?.GetIsPropertyFieldSelected() == true)
                            {
                                gui.Button("#navigation", CoreIconCache.GotoDefination)
                                .InitClass("configBtn")
                                .InitInputFunction(ToolTipButtonInput)
                                .OnClick(n =>
                                {
                                    if ((target.Parent?.GetValues()?.FirstOrDefault() as INavigateMember)?.GetNavigateMember(target.PropertyName) is { } navi)
                                    {
                                        EditorUtility.GotoDefinition(navi);
                                    }
                                }, true);
                            }
                            break;
                        }

                    default:
                        break;
                }
            }
        }, target.GetOrCreatePropertyRowData());

        node.IsDisabled = target.Disabled;
        node.IsReadOnly = target.ReadOnly;
        if (target.CachedTheme is ImGuiTheme theme)
        {
            node.InitTheme(theme);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyRow(ImGui gui, string id, string title, TextStatus? status = null, PropertyRowAction? rowAction = null)
    {
        return PropertyRowFrame(gui, id, (n, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        {
                            if (status?.ToStatusIcon() is { } statusIcon)
                            {
                                gui.Image("#statusIcon", statusIcon)
                                .InitClass("icon");
                            }
                            gui.Text(title).InitClass(PropertyGridThemes.ClassPropertyInput);
                        }
                        break;

                    case PropertyGridColumn.Main:
                        break;

                    case PropertyGridColumn.Option:
                        break;

                    default:
                        break;
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyRowFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null)
    {
        var node = gui.HorizontalFrame(id);
        node.InitFullWidth();
        node.InitInputFunctionChain(PropertyRowInput);

        var system = gui.GetOrAddSystem<PropertyGridSystem>();
        rowData ??= node.GetOrCreatePropertyRowData();
        node.InitializePropertyRowData(rowData);
        PropertyGridData? root = rowData.GridData;

        rowData.NodePath = node.FullPath;
        rowData.ParentRow = system.CurrentEditorNode?.GetValue<PropertyRowData>();
        if (rowData.ParentRow is { } parentValue)
        {
            rowData.Indent = parentValue.Indent + 1;
        }
        else
        {
            rowData.Indent = 0;
        }

        node.InitClass(PropertyGridThemes.ClassPropertyLine)
        .OnContent(row =>
        {
            if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
            {
                try
                {
                    if (drawer.DrawPropertyRowFrame(node, id, rowAction, rowData))
                    {
                        return;
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            GroupedResizerState? state = null;
            if (row.IsInitializing)
            {
                state = row.FindValueInHierarchy<GroupedResizerState>();
            }

            gui.HorizontalLayout("#propPre")
            .InitClass(PropertyGridThemes.ClassPropertyCell, "propCell1")
            .InitWidth(PropertyGridExtensions.PrefixColumnWidth)
            .OnContent(n =>
            {
                rowAction?.Invoke(n, PropertyGridColumn.Prefix, GuiPipeline.Main);
            });

            gui.HorizontalLayout("#propName")
            .InitClass(PropertyGridThemes.ClassPropertyCell)
            .InitWidth(state?.GetLength(0, 160) ?? 160)
            .OnContent(n =>
            {
                if (rowData.Indent > 0)
                {
                    gui.EmptyFrame("#indent").SetWidth(rowData.Indent * PropertyGridExtensions.IndentWidth);
                }
                rowAction?.Invoke(n, PropertyGridColumn.Name, GuiPipeline.Main);
            });

            gui.HorizontalResizer(root?.NameColumnWidthMin ?? 40, root?.NameColumnWidthMax)
            .InitGroupedResizer(0)
            .SetClass(PropertyGridThemes.ClassPropertyResizer);

            gui.HorizontalLayout("#propLast")
            .InitWidthRest()
            .OnContent(n =>
            {
                gui.VerticalLayout("#propMain")
                .InitClass(PropertyGridThemes.ClassPropertyCell)
                .InitWidthRest(32)
                .OnContent(n =>
                {
                    rowAction?.Invoke(n, PropertyGridColumn.Main, GuiPipeline.Main);
                });

                gui.VerticalLayout("#propOption")
                .InitClass(PropertyGridThemes.ClassPropertyCell)
                .InitWidth(32)
                .OnContent(n =>
                {
                    rowAction?.Invoke(n, PropertyGridColumn.Option, GuiPipeline.Main);
                });
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyLabel(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
    {
        return PropertyLabel(gui, target.Id, target.DisplayName,
            icon: target.Icon,
            status: target.Status,
            value: target.GetOrCreatePropertyRowData(),
            rowAction: (n, c, p) => 
            {
                var rowData = n.GetOrCreatePropertyRowData(target);

                if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
                {
                    try
                    {
                        if (drawer.DrawPropertyLabel(n, target, rowAction))
                        {
                            return;
                        }
                    }
                    catch (Exception err)
                    {
                        err.LogError();
                    }
                }


                rowAction?.Invoke(n, c, p);

                if (p.HasFlag(GuiPipeline.PostAction) && c == PropertyGridColumn.Name && target.ToolTips is { } toolTip && !string.IsNullOrWhiteSpace(toolTip))
                {
                    gui.Button("#tooltips", CoreIconCache.Info)
                    .InitClass("configBtn")
                    .InitInputFunction(ToolTipButtonInput)
                    .OnClick(n =>
                    {
                        (gui.Context as IGraphicToolTip)?.ShowToolTip(toolTip, (int)n.GlobalRect.X, (int)n.GlobalRect.Bottom);
                    }, true);
                }
            });
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyLabel(ImGui gui, string id, string title, ImageDef? icon = null, TextStatus? status = null, PropertyRowAction? rowAction = null, PropertyRowData? value = null)
    {
        return PropertyLabelFrame(gui, id, (n, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        {
                            if (status?.ToStatusIcon() is { } statusIcon)
                            {
                                gui.Image("#statusIcon", statusIcon)
                                .InitClass("icon")
                                .SetVerticalAlignment(GuiAlignment.Far, false);
                            }

                            if (icon is not null)
                            {
                                gui.Image("#icon", icon)
                                .InitClass("icon")
                                .SetVerticalAlignment(GuiAlignment.Far, false);
                            }

                             // Empty escape
                            if (title == "-")
                            {
                                title = " ";
                            }

                            var t = gui.Text(title)
                            .InitClass("propLabelText")
                            .SetVerticalAlignment(GuiAlignment.Far, false);

                            if (status is { } s && s != TextStatus.Normal)
                            {
                                t.OverrideFont(color: EditorServices.ColorConfig.GetStatusColor(s));
                            }
                            else
                            {
                                t.RemoveValue<GuiFontStyle>();
                            }
                        }

                        break;

                    case PropertyGridColumn.Main:
                        break;

                    case PropertyGridColumn.Option:
                        break;

                    default:
                        break;
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        }, value);
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyLabelFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, PropertyRowData? rowData = null)
    {
        var node = gui.HorizontalFrame(id)
        .InitFullWidth()
        .InitClass(PropertyGridThemes.ClassLabel);

        var system = gui.GetOrAddSystem<PropertyGridSystem>();
        rowData ??= node.GetOrCreatePropertyRowData();
        node.InitializePropertyRowData(rowData);
        PropertyGridData? root = rowData.GridData;

        rowData.SelectEnabled = false;
        rowData.NodePath = node.FullPath;
        rowData.ParentRow = system.CurrentEditorNode?.GetValue<PropertyRowData>();
        if (rowData.ParentRow is { } parentValue)
        {
            rowData.Indent = parentValue.Indent + 1;
        }
        else
        {
            rowData.Indent = 0;
        }

        node.OnContent(() =>
        {
            if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
            {
                try
                {
                    if (drawer.DrawPropertyLabelFrame(node, id, rowAction, rowData))
                    {
                        return;
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            gui.HorizontalLayout("#propPre")
            .InitClass(PropertyGridThemes.ClassLabelCell, "propCell1")
            .InitWidth(PropertyGridExtensions.PrefixColumnWidth)
            .OnContent(() =>
            {
                rowAction?.Invoke(node, PropertyGridColumn.Prefix, GuiPipeline.Main);
            });

            gui.HorizontalLayout("#propName")
            .InitClass(PropertyGridThemes.ClassLabelCell)
            .InitWidthRest()
            .OnContent(() =>
            {
                if (rowData.Indent > 0)
                {
                    gui.EmptyFrame("#indent").SetWidth(rowData.Indent * PropertyGridExtensions.IndentWidth);
                }
                rowAction?.Invoke(node, PropertyGridColumn.Name, GuiPipeline.Main);
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyTooltips(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
    {
        return PropertyTooltips(gui, target.Id, target.DisplayName,
            icon: target.Icon,
            status: target.Status,
            rowAction: rowAction,
            value: target.GetOrCreatePropertyRowData());
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyTooltips(ImGui gui, string id, string title, ImageDef? icon = null, TextStatus? status = null, PropertyRowAction? rowAction = null, PropertyRowData? value = null)
    {
        if (icon is null)
        {
            switch (status ?? TextStatus.Normal)
            {
                case TextStatus.Normal:
                    icon = CoreIconCache.Info;
                    break;

                case TextStatus.Info:
                    icon = CoreIconCache.Notice;
                    break;

                case TextStatus.Warning:
                    icon = CoreIconCache.Warning;
                    break;

                case TextStatus.Error:
                    icon = CoreIconCache.Error;
                    break;
            }
        }

        var node = gui.VerticalFrame(id)
        .InitClass(PropertyGridThemes.ClassBG)
        .InitFullWidth()
        .InitFitVertical()
        .OnContent(n =>
        {
            gui.VerticalLayout()
            .InitFullWidth()
            .InitPadding(5)
            .InitFitVertical()
            .OnContent(() => 
            {
                TextStatus s = status ?? TextStatus.Normal;
                var color = EditorServices.ColorConfig.GetStatusColor(s);

                var frame = gui.VerticalFrame("#frame")
                .InitClass("toolTipFrame")
                .InitFullWidth()
                .InitFitVertical()
                .OnContent(() =>
                {
                    ImGuiNode? text = null;

                    if (icon != null)
                    {
                        gui.HorizontalLayout("#hori")
                        .InitFullWidth()
                        .InitFitVertical()
                        .OnContent(() =>
                        {
                            gui.Image("#icon", icon)
                            .InitSize(32, 32);

                            text = gui.TextArea("#text", title)
                            .InitClass("toolTipText")
                            .InitWidthRest();

                            if (s != TextStatus.Normal)
                            {
                                text?.OverrideFont(null, color);
                            }
                        });
                    }
                    else
                    {
                        text = gui.TextArea("#text", title)
                        .InitClass("toolTipText")
                        .InitFullWidth();

                        if (s != TextStatus.Normal)
                        {
                            text?.OverrideFont(null, color);
                        }
                    }
                });

                if (s != TextStatus.Normal)
                {
                    frame.OverrideBorder(1, color);
                    frame.OverrideColor(color.MultiplyAlpha(0.3f));
                }
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
    {
        var node = PropertyButton(gui, target.Id, target.DisplayName,
            icon: target.Icon,
            rowAction: rowAction,
            onClick: async n =>
            {
                if (target is ISupportStyle style && style.Styles?.GetConfirm() is { } confirmMsg)
                {
                    bool ok = await DialogUtility.ShowYesNoDialogAsync(confirmMsg);
                    if (ok)
                    {
                        target.SetValues([ButtonValue.Clicked], new ButtonSetterContext(target, n));
                    }
                }
                else
                {
                    target.SetValues([ButtonValue.Clicked], new ButtonSetterContext(target, n));
                }
            });

        if (target.ToolTips is { } tooltips && !string.IsNullOrWhiteSpace(tooltips))
        {
            node.SetToolTipsL(tooltips);
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyButton(ImGui gui, string id, string title, ImageDef? icon = null, PropertyRowAction? rowAction = null, Action? onClick = null)
    {
        var node = gui.VerticalFrame(id)
        .InitClass(PropertyGridThemes.ClassBG)
        .InitFullWidth()
        .OnContent(n =>
        {
            gui.VerticalLayout()
            .InitFullWidth()
            .InitPadding(5)
            .OnContent(() => 
            {
                gui.Button("#btn", title, icon)
                .InitClass("simpleBtn")
                .InitCenter()
                .InitFitHorizontal()
                .OnClick(n =>
                {
                    onClick?.Invoke();
                    n.QueueRefresh();
                });
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyButton(ImGui gui, string id, string title, ImageDef? icon = null, PropertyRowAction? rowAction = null, Action<ImGuiNode>? onClick = null)
    {
        var node = gui.VerticalFrame(id)
        .InitClass(PropertyGridThemes.ClassBG)
        .InitFullWidth()
        .OnContent(n =>
        {
            gui.VerticalLayout()
            .InitFullWidth()
            .InitPadding(5)
            .OnContent(() =>
            {
                gui.Button("#btn", title, icon)
                .InitClass("simpleBtn")
                .InitCenter()
                .InitFitHorizontal()
                .OnClick(n =>
                {
                    onClick?.Invoke(n);
                    n.QueueRefresh();
                });
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyMultipleButton(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction = null)
    {
        var first = target.GetValues().FirstOrDefault() as MultipleButtonValue;

        var node = gui.VerticalFrame(target.Id)
        .InitClass(PropertyGridThemes.ClassBG)
        .InitFullWidth()
        .OnContent(n =>
        {
            gui.HorizontalLayout()
            .InitFit()
            .InitCenterHorizontal()
            .InitPadding(5)
            .OnContent(() =>
            {
                var buttons = first?.Buttons ?? [];

                foreach (var btn in buttons)
                {
                    ImageDef img = EditorUtility.GetIcon(btn.Image);

                    var btnNode = gui.Button(btn.Key, L(btn.Title), img)
                    .InitClass("simpleBtn")
                    .InitFitHorizontal()
                    .OnClick(n =>
                    {
                        var clicked = first?.CreateClicked(n.Id);
                        if (clicked != null)
                        {
                            target.SetValues([clicked]);
                        }

                        n.QueueRefresh();
                    });

                    if (!string.IsNullOrWhiteSpace(btn.ToolTips))
                    {
                        btnNode.SetToolTipsL(btn.ToolTips);
                    }
                }
            });
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyGroup(ImGui gui, PropertyTarget target, string? preview = null, PropertyTargetAction? targetAction = null)
    {
        var node = PropertyGroupFrame(gui, target.Id, (n, column, pipeline) =>
        {
            var rowData = n.GetOrCreatePropertyRowData(target);

            if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
            {
                try
                {
                    if (drawer.DrawPropertyGroup(n, target, preview, targetAction))
                    {
                        return;
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        {
                            if (target.Color is { } color)
                            {
                                if (SmallRectColor)
                                {
                                    var f = gui.Rect("#color")
                                    .SetWidth(5)
                                    //.SetFullHeight()
                                    .OverrideColor(color);
                                }
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            targetAction?.Invoke(n, target, column, GuiPipeline.Main | GuiPipeline.PostAction);
                            break;
                        }

                    case PropertyGridColumn.Name:
                        {
                            n.InitRenderFunction("Frame")
                            .InitOverrideBorder(0);

                            if (target.ErrorInHierarchy)
                            {
                                gui.Image("#error", CoreIconCache.Error)
                                .InitClass("icon");
                            }

                            bool dark = false;

                            if (target.Color is { } color)
                            {
                                if (LargeRectColor)
                                {
                                    n.OverrideColor(color)
                                    .OverrideCorner(3);
                                    dark = true;
                                }
                            }
                            else
                            {
                                n.RemoveValue<GuiColorStyle>();
                                n.RemoveValue<GuiFrameStyle>();
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            if (!target.HideTitle)
                            {
                                PropertyTitle(gui, target, dark);
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.Main);

                            if (target.ToolTips is { } toolTip && !string.IsNullOrWhiteSpace(toolTip))
                            {
                                gui.Button("#tooltips", CoreIconCache.Info)
                                .InitClass("configBtn")
                                .InitInputFunction(ToolTipButtonInput)
                                .OnClick(n =>
                                {
                                    (gui.Context as IGraphicToolTip)?.ShowToolTip(toolTip, (int)n.GlobalRect.X, (int)n.GlobalRect.Bottom);
                                }, true);
                            }

                            targetAction?.Invoke(n, target, column, GuiPipeline.PostAction);

                            break;
                        }

                    case PropertyGridColumn.Main:
                        {
                            if (!string.IsNullOrEmpty(preview))
                            {
                                targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                                gui.Text(preview ?? string.Empty).InitClass(PropertyGridThemes.ClassPropertyInput);
                                targetAction?.Invoke(n, target, column, GuiPipeline.PostAction);
                            }
                            else
                            {
                                PropertyMainColumn(gui, target, targetAction, n, column);
                            }
                        }
                        break;

                    case PropertyGridColumn.Option:
                        {
                            targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);
                            targetAction?.Invoke(n, target, column, GuiPipeline.Main | GuiPipeline.PostAction);

                            if (target.GetValues().FirstOrDefault() is IDrawEditorImGui draw)
                            {
                                draw.OnEditorGui(gui, EditorImGuiPipeline.Option, target);
                            }

                            if (target.Navigation && n.Parent?.GetIsPropertyFieldSelected() == true)
                            {
                                gui.Button("#navigation", CoreIconCache.GotoDefination)
                                .InitClass("configBtn")
                                .InitInputFunction(ToolTipButtonInput)
                                .OnClick(n =>
                                {
                                    if ((target.Parent?.GetValues()?.FirstOrDefault() as INavigateMember)?.GetNavigateMember(target.PropertyName) is { } navi)
                                    {
                                        EditorUtility.GotoDefinition(navi);
                                    }
                                }, true);
                            }

                            break;
                        }

                    default:
                        break;
                }
            }
        }, target.InitExpanded, target.GetOrCreatePropertyRowData());

        node.IsDisabled = target.Disabled;
        node.IsReadOnly = target.ReadOnly;
        if (target.CachedTheme is ImGuiTheme theme)
        {
            node.InitTheme(theme);
        }

        if (node.IsInitializing & target.Styles?.GetAttribute("HeaderStyle") == "Emboss")
        {
            node.InitUnionClass(PropertyGridThemes.ClassPropertyEmboss);
        }

        if (target.ExpandRequest is { } forceExpaned)
        {
            target.ExpandRequest = null;
            node.GetOrCreateValue<GuiExpandableValue>().Expanded = forceExpaned;
        }

        target.InitExpanded = node.GetIsExpanded();

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyGroup(ImGui gui, string id, string title, string? preview = null, PropertyRowAction? rowAction = null, bool initExpand = true)
    {
        return PropertyGroupFrame(gui, id, (n, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        gui.Text(title).InitClass(PropertyGridThemes.ClassPropertyInput);
                        break;

                    case PropertyGridColumn.Main:
                        if (!string.IsNullOrEmpty(preview))
                        {
                            gui.Text(preview ?? string.Empty).InitClass(PropertyGridThemes.ClassPropertyInput);
                        }
                        break;

                    case PropertyGridColumn.Option:
                        break;

                    default:
                        break;
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        }, initExpand);
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyGroupFrame(ImGui gui, string id, PropertyRowAction? rowAction = null, bool? initExpand = true, PropertyRowData? rowData = null)
    {
        var node = gui.HorizontalFrame(id);
        node.InitFullWidth();
        node.InitInputFunctionChain(PropertyRowInput);

        var system = gui.GetOrAddSystem<PropertyGridSystem>();
        rowData ??= node.GetOrCreatePropertyRowData();
        node.InitializePropertyRowData(rowData);
        PropertyGridData? root = rowData.GridData;

        rowData.NodePath = node.FullPath;
        rowData.ParentRow = system.CurrentEditorNode?.GetValue<PropertyRowData>();
        if (rowData.ParentRow is { } parentValue)
        {
            rowData.Indent = parentValue.Indent + 1;
        }
        else
        {
            rowData.Indent = 0;
        }

        return node.InitClass(PropertyGridThemes.ClassPropertyLine)
        .InitInputFunctionChain(ExpandDoubleClickInput)
        .OnContent(row =>
        {
            if (GetCustromDrawer(rowData.GridData?.GridName) is { } drawer)
            {
                try
                {
                    if (drawer.DrawPropertyGroupFrame(node, id, rowAction, initExpand, rowData))
                    {
                        return;
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            GroupedResizerState? state = null;
            if (row.IsInitializing)
            {
                state = row.FindValueInHierarchy<GroupedResizerState>();
            }

            GuiExpandableValue? expandValue = null;
            if (initExpand.HasValue)
            {
                expandValue = row.GetOrCreateValue<GuiExpandableValue>();
                if (row.IsInitializing)
                {
                    expandValue.Expanded = initExpand.Value;
                    gui.RestoreState(row);
                }
            }

            gui.HorizontalLayout("#propPre")
            .InitWidth(PropertyGridExtensions.PrefixColumnWidth)
            .InitClass(PropertyGridThemes.ClassPropertyCell, "propCell1")
            .OnContent(n =>
            {
                //if (value.Indent == 0 && expandValue != null)
                //{
                //    gui.Button("expandBtn", line.Theme.ExpandImage)
                //    .InitClass("configBtn")
                //    .InitInputFunction(ExpandButtonInput)
                //    .InitRenderFunctionChain(RenderButtonExpandImage)
                //    .InitValue(expandValue);
                //}

                rowAction?.Invoke(n, PropertyGridColumn.Prefix, GuiPipeline.Main);
            });

            gui.HorizontalLayout("#propName")
            .InitWidth(state?.GetLength(0, 160) ?? 160)
            .InitClass(PropertyGridThemes.ClassPropertyCell)
            .OnContent(n =>
            {
                if (rowData.Indent > 0)
                {
                    gui.EmptyFrame().SetWidth((rowData.Indent) * PropertyGridExtensions.IndentWidth);
                }

                if (expandValue != null)
                {
                    gui.ExpandButton("expandBtn", expandValue: expandValue)
                    .InitClass("configBtn");
                }

                rowAction?.Invoke(n, PropertyGridColumn.Name, GuiPipeline.Main);
            });

            gui.HorizontalResizer(root?.NameColumnWidthMin ?? 40, root?.NameColumnWidthMax)
            .InitGroupedResizer(0)
            .InitClass(PropertyGridThemes.ClassPropertyResizer);

            gui.HorizontalLayout("#propLast")
            .InitWidthRest()
            .OnContent(n =>
            {
                gui.VerticalLayout("#propMain")
                .InitClass(PropertyGridThemes.ClassPropertyCell)
                .InitWidthRest(32)
                .OnContent(n =>
                {
                    rowAction?.Invoke(n, PropertyGridColumn.Main, GuiPipeline.Main);
                });

                gui.VerticalLayout("#propOption")
                .InitClass(PropertyGridThemes.ClassPropertyCell)
                .InitWidth(32)
                .OnContent(n =>
                {
                    rowAction?.Invoke(n, PropertyGridColumn.Option, GuiPipeline.Main);
                });
            });
        });
    }

    /// <inheritdoc/>
    public override void PropertyTitle(ImGui gui, PropertyTarget target, bool dark = false)
    {
        if (target.Status.ToStatusIcon() is { } statusIcon)
        {
            gui.Image("#statusIcon", statusIcon)
            .InitClass("icon");
        }

        if (target.Parent?.ArrayTarget is { })
        {
            gui.Image("#array_item_icon", CoreIconCache.Row)
                .InitClass("icon");
        }
        else if (target.Icon is { } icon)
        {
            gui.Image("#property_icon", icon)
                .InitClass("icon");
        }

        var text = gui.Text(target.DisplayName)
            .InitClass(PropertyGridThemes.ClassPropertyInput)
            .SetPropertyTitleColor(target, dark);

        if (target.ToolTips is { } toolTips && !string.IsNullOrWhiteSpace(toolTips))
        {
            text.SetToolTips(toolTips);
        }
    }

    /// <inheritdoc/>
    public override ImGuiNode PropertyTitle(ImGui gui, IValueTarget target, bool dark = false)
    {
        return gui.Text(target.DisplayName)
            .InitClass(PropertyGridThemes.ClassPropertyInput)
            .SetPropertyTitleColor(target, dark);
    }

    /// <inheritdoc/>
    public override ImGuiNode OnPropertyGroupExpand(ImGuiNode node, Action action)
    {
        if (node.GetIsExpanded())
        {
            node.GetOrCreateValue<PropertyRowData>();

            var system = node.Gui.GetOrAddSystem<PropertyGridSystem>();

            system.PushEditorNode(node);

            try
            {
                action();
            }
            finally
            {
                system.PopEditorNode();
            }
        }

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode OnPropertyGroupExpand(ImGuiNode node, Action<bool> action)
    {
        if (node.GetIsExpanded())
        {
            node.GetOrCreateValue<PropertyRowData>();

            var system = node.Gui.GetOrAddSystem<PropertyGridSystem>();

            system.PushEditorNode(node);

            try
            {
                action(true);
            }
            finally
            {
                system.PopEditorNode();
            }
        }
        else
        {
            action(false);
        }

        return node;
    }

    /// <summary>
    /// Renders the main column of a property row, supporting optional multi-column layouts.
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering.</param>
    /// <param name="target">The property target containing the values to display.</param>
    /// <param name="targetAction">Optional action invoked during rendering for each column and pipeline stage.</param>
    /// <param name="n">The current ImGuiNode being rendered.</param>
    /// <param name="column">The column identifier being rendered.</param>
    private void PropertyMainColumn(ImGui gui, PropertyTarget target, PropertyTargetAction? targetAction, ImGuiNode n, PropertyGridColumn column)
    {
        bool gridSupportMultipleColumn = n.GetOrCreatePropertyRowData(target).GridData?.SupportMultipleColumn == true;
        int count = (gridSupportMultipleColumn && target.SupportMultipleColumn)
            ? target.GetValues().CountWithMaxValue(6) : 1;

        targetAction?.Invoke(n, target, column, GuiPipeline.PreAction);

        gui.HorizontalLayout("#horizontal")
            .InitFullWidth()
            .OnContent(() =>
        {
            float? span;

            switch (count)
            {
                case 0:
                    span = null;
                    break;

                case 2:
                    span = 50f;
                    break;

                case 3:
                    span = 33.3333f;
                    break;

                case 4:
                    span = 25f;
                    break;

                case 5:
                    span = 20f;
                    break;

                case 1:
                default:
                    span = null;
                    targetAction?.Invoke(n, target, column, GuiPipeline.Main);
                    break;
            }

            if (span.HasValue)
            {
                for (int i = 0; i < count; i++)
                {
                    gui.HorizontalLayout($"#column{i}")
                    .InitOverrideSiblingSpacing(0)
                    .SetWidthPercentage(span.Value)
                    .OnContent(() =>
                    {
                        var inner = target.GetColumnTarget(i);
                        if (!PropertyTarget.IsNullOrEmpty(inner))
                        {
                            targetAction?.Invoke(n, inner, column, GuiPipeline.Main);
                        }
                    });
                }
            }
        });

        targetAction?.Invoke(n, target, column, GuiPipeline.PostAction);
    }

    /// <summary>
    /// Handles input events for property rows, including mouse selection and focus management.
    /// </summary>
    /// <param name="pipeline">The current rendering pipeline stage.</param>
    /// <param name="node">The ImGuiNode receiving the input.</param>
    /// <param name="input">The input event data.</param>
    /// <param name="baseAction">The base input processing function for child nodes.</param>
    /// <returns>The resulting input state after processing.</returns>
    private GuiInputState PropertyRowInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown when input.MouseButton == GuiMouseButtons.Left || input.MouseButton == GuiMouseButtons.Right:
                {
                    var value = node.GetValue<PropertyRowData>();
                    if (value is null || value.GridData is null)
                    {
                        break;
                    }

                    if (!value.IsSelected && value.GridData is { })
                    {
                        if (value.GridData.SetSelection(value))
                        {
                            ImGui.MergeState(ref state, GuiInputState.FullSync);
                        }
                    }

                    if (node.Gui.ControllingNode is null)
                    {
                        node.SetIsControlling(true);
                    }
                }

                break;

            case GuiEventTypes.MouseUp when input.MouseButton == GuiMouseButtons.Left || input.MouseButton == GuiMouseButtons.Right:
                {
                    node.SetIsControlling(false);
                }
                
                break;

            default:
                break;
        }

        return state;
    }

    /// <summary>
    /// Handles double-click input events to toggle the expanded state of a property group.
    /// </summary>
    /// <param name="pipeline">The current rendering pipeline stage.</param>
    /// <param name="node">The ImGuiNode receiving the input.</param>
    /// <param name="input">The input event data.</param>
    /// <param name="baseAction">The base input processing function for child nodes.</param>
    /// <returns>The resulting input state after processing.</returns>
    private GuiInputState ExpandDoubleClickInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        var value = node.GetOrCreateValue<GuiExpandableValue>();

        if (input.EventType == GuiEventTypes.MouseUp && node.GetIsClicked(true) && node.Gui.IsDoubleClick)
        {
            value.Expanded = !value.Expanded;

            ImGui.MergeState(ref state, GuiInputState.FullSync);
        }

        return state;
    }

    /// <summary>
    /// Handles input events for tooltip buttons, managing visual pseudo-states on mouse interactions.
    /// </summary>
    /// <param name="pipeLine">The current rendering pipeline stage.</param>
    /// <param name="node">The ImGuiNode receiving the input.</param>
    /// <param name="input">The input event data.</param>
    /// <param name="childNodesAction">The input processing function for child nodes.</param>
    /// <returns>The resulting input state after processing.</returns>
    private GuiInputState ToolTipButtonInput(GuiPipeline pipeLine, ImGuiNode node, IGraphicInput input, ChildInputFunction childNodesAction)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseUp:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.FullSync;

            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                return GuiInputState.Render;

            default:
                return GuiInputState.None;
        }
    }

    /// <summary>
    /// Handles moving the selection focus to the previous selectable property row.
    /// </summary>
    /// <param name="gridNode">The grid node containing the property rows.</param>
    /// <param name="field">The currently selected property row data.</param>
    /// <param name="data">The property grid data managing selection state.</param>
    internal static void HandleMoveUp(ImGuiNode gridNode, PropertyRowData field, PropertyGridData data)
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
    /// Handles moving the selection focus to the next selectable property row.
    /// </summary>
    /// <param name="gridNode">The grid node containing the property rows.</param>
    /// <param name="field">The currently selected property row data.</param>
    /// <param name="data">The property grid data managing selection state.</param>
    internal static void HandleMoveDown(ImGuiNode gridNode, PropertyRowData field, PropertyGridData data)
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
}
