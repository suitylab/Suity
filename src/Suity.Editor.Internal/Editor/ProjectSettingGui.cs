using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Im.TreeEditing;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor;

/// <summary>
/// ImGui-based project settings GUI with plugin tree navigation and property grid editing.
/// Displays a hierarchical tree of configurable plugins on the left panel and a property grid on the right.
/// </summary>
public class ProjectSettingGui : IDrawImGui, IViewRefresh, IViewSave
{
    /// <summary>
    /// Delayed action that saves the current project settings after a debounce period.
    /// </summary>
    private class SaveProjectSettingAction : DelayedAction
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="SaveProjectSettingAction"/>.
        /// </summary>
        public static SaveProjectSettingAction Instance { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveProjectSettingAction"/> class with a 10ms delay.
        /// </summary>
        private SaveProjectSettingAction()
            : base(10)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            Project.Current.SaveSetting();
        }
    }


    private readonly ImGuiTheme _theme;
    private readonly ImGuiPathTreeModel _treeModel;
    private readonly HeaderlessPathTreeView _treeView;
    private readonly IPropertyGrid _propGrid;

    private readonly ImGuiNodeRef _guiRef = new();

    private PluginInfo _selectedPlugin;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectSettingGui"/> class.
    /// Sets up the theme, tree model, tree view, property grid, and selects the first available node.
    /// </summary>
    public ProjectSettingGui()
    {
        _theme = CreateTheme();

        _treeModel = new();
        _treeView = new HeaderlessPathTreeView(_treeModel);
        _treeView.TreeData.SelectionMode = ImTreeViewSelectionMode.Single;
        _treeView.SelectionChanged += _treeView_SelectionChanged;

        _propGrid = PropertyGridExtensions.CreatePropertyGrid("ProjectSetting");

        _propGrid.ShowContextMenu = false;
        _propGrid.ShowToolBar = false;

        _propGrid.AddService<IViewRefresh>(this);
        _propGrid.AddService<IViewSave>(this);
        _propGrid.Edited += _grid_Edited;

        InitTreeView();

        PathNode firstSelect = _treeModel.Nodes.FirstOrDefault();
        while (firstSelect is PopulatePathNode p)
        {
            firstSelect = p.NodeList.FirstOrDefault();
        }

        _treeView.SelectNode(firstSelect);
    }

    /// <summary>
    /// Creates and configures the ImGui theme for the project settings GUI, including title text, description text, and button styles.
    /// </summary>
    /// <returns>The configured <see cref="ImGuiTheme"/> instance.</returns>
    private ImGuiTheme CreateTheme()
    {
        var theme = new ImGuiTheme();
        theme.ClassStyle("titleText")
        .SetFont(new Font(ImGuiTheme.DefaultFont, 36), Color.White);

        theme.ClassStyle("descText")
        .SetFont(new Font(ImGuiTheme.DefaultFont, 16), Color.White);

        theme.ClassStyle("mainBtn")
            .SetCornerRound(5)
            .SetPadding(7);

        return theme;
    }

    /// <summary>
    /// Initializes the tree view with plugin nodes organized by type (Editor, API, Others).
    /// Filters out backend plugins and plugins marked with <see cref="NotAvailableAttribute"/>.
    /// </summary>
    private void InitTreeView()
    {
        _treeModel.Clear();

        var plugins = EditorServices.PluginService.Plugins
            .Where(o => o.Plugin is not BackendPlugin && o.Plugin is IViewObject && !o.Plugin.GetType().HasAttributeCached<NotAvailableAttribute>())
            .ToArray();

        // Get all configurable plugins
        var pluginSet = new HashSet<PluginInfo>(plugins);

        var editorPlugins = pluginSet.Where(o => o.Plugin is EditorPlugin).OrderByDescending(o => o.Order).ToArray();
        if (editorPlugins.Any())
        {
            _treeModel.Add(new PluginGroupNode(editorPlugins, "Editor", "Editor Setting"));
            pluginSet.ExceptWith(editorPlugins);
        }

        var apiPlugin = pluginSet.Where(o => o.Plugin is ApiPlugin).OrderByDescending(o => o.Order).ToArray();
        if (apiPlugin.Any())
        {
            _treeModel.Add(new PluginGroupNode(apiPlugin, "api", "API"));
            pluginSet.ExceptWith(apiPlugin);
        }

        if (pluginSet.Count > 0)
        {
            // List remaining plugins
            var mistPlugins = pluginSet.OrderByDescending(o => o.Order).ToArray();
            _treeModel.Add(new PluginGroupNode(mistPlugins, "Others", "Others"));
        }

        _treeModel.ExpandAll();
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        if (gui is null)
        {
            return;
        }

        _guiRef.Node = gui.HorizontalFrame("#bg")
        .InitClass("editorBg")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.VerticalLayout("panel_left")
            .InitFullHeight()
            .InitWidthPercentage(25f)
            .OnContent(() =>
            {
                _treeView.OnGui(gui, "tree_view", n => n.InitSizeRest());
            });

            gui.HorizontalResizer(min: 40, affectSibling: true)
            .InitClass("propResizer")
            .InitWidth(10)
            .InitFullHeight();

            gui.VerticalLayout($"panel_right:{_selectedPlugin?.Name}")
            .InitTheme(_theme)
            .InitFullHeight()
            .InitWidthRest()
            .OverridePadding(2)
            .OnContent(() =>
            {
                _propGrid.OnGui(gui);
            });
        });
    }

    private void _treeView_SelectionChanged(object sender, EventArgs e)
    {
        // Update the property grid when tree selection changes
        if (_treeView.SelectedNode is PluginNode pluginNode && pluginNode.Plugin != null)
        {
            _selectedPlugin = pluginNode.Plugin;

            if (pluginNode.Plugin.Plugin is IViewObject viewObj)
            {
                _propGrid.InspectObjects([viewObj]);
            }
            else
            {
                _propGrid.InspectObjects([]);
            }
        }
        else
        {
            _selectedPlugin = null;

            _propGrid.InspectObjects([]);
        }

        _treeView.QueueRefresh();
        _guiRef.QueueRefresh();
    }

    private void _grid_Edited(object sender, ObjectPropertyEventArgs e)
    {
        // Queue a delayed save action when the property grid is edited
        EditorUtility.AddDelayedAction(SaveProjectSettingAction.Instance);
    }

    #region IViewRefresh
    /// <inheritdoc/>
    public void QueueRefreshView()
    {
        _guiRef.QueueRefresh(true);
    }
    #endregion

    #region IViewSave
    /// <inheritdoc/>
    public void SaveView()
    {
        EditorUtility.AddDelayedAction(SaveProjectSettingAction.Instance);
        _guiRef.QueueRefresh(true);
    }
    #endregion

    #region PluginGroupNode
    /// <summary>
    /// A tree node that represents a group of plugins, populating child <see cref="PluginNode"/> entries on demand.
    /// </summary>
    internal class PluginGroupNode : PopulatePathNode
    {
        private readonly PluginInfo[] _infos;

        private readonly string _name;
        private readonly string _displayText;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginGroupNode"/> class.
        /// </summary>
        /// <param name="infos">The collection of plugin info entries to display as children.</param>
        /// <param name="name">The internal name of the group node.</param>
        /// <param name="displayName">The display text shown in the tree view. If null, <paramref name="name"/> is used.</param>
        public PluginGroupNode(IEnumerable<PluginInfo> infos, string name, string displayName = null)
            : base(name)
        {
            _infos = [.. infos];

            _name = name;
            _displayText = displayName;
        }

        /// <inheritdoc/>
        protected override bool CanPopulate()
        {
            return _infos.Any();
        }

        /// <inheritdoc/>
        protected override IEnumerable<PathNode> OnPopulate()
        {
            foreach (var info in _infos)
            {
                yield return new PluginNode(info);
            }
        }

        /// <inheritdoc/>
        protected override string OnGetText()
        {
            return L(_displayText) ?? _name;
        }
    }
    #endregion

    #region PluginNode
    /// <summary>
    /// A tree node representing a single configurable plugin in the project settings.
    /// </summary>
    internal class PluginNode : PathNode
    {
        /// <summary>
        /// Gets the plugin info associated with this node.
        /// </summary>
        public PluginInfo Plugin { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginNode"/> class.
        /// </summary>
        /// <param name="plugin">The plugin info to represent. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugin"/> is null.</exception>
        public PluginNode(PluginInfo plugin)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

            SetupNodePath(plugin.Name);
        }

        /// <inheritdoc/>
        protected override string OnGetText()
        {
            return L(Plugin.DisplayText);
        }

        /// <inheritdoc/>
        public override Image TextStatusIcon => Plugin.Plugin.Icon;
    }
    #endregion
}