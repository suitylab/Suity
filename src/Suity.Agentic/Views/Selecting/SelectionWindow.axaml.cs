using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Selecting;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Views.Selecting;

public partial class SelectionWindow : Window, IDrawImGui
{
    private const int _delayTime = 200;
    
    private static string? _lastKey;

    private readonly ImGuiNodeRef _nodeRef = new();

    private readonly SelectionModel _model;
    private readonly SelectionTreeView _treeView;

    private readonly ISelectionList _list;

    private readonly SelectionOption _option;
    private readonly string _initKey;
    private readonly bool _allowSelectList;

    private readonly System.Threading.Timer _timer;

    string _inputText = string.Empty;

    public SelectionWindow()
    {
        InitializeComponent();
    }

    public SelectionWindow(ISelectionList selList, string title, SelectionOption? option = null)
        : this()
    {
        if (selList is null)
        {
            throw new ArgumentNullException(nameof(selList));
        }

        var colorConfig = EditorServices.ColorConfig;
        Color textColor = colorConfig.GetStatusColor(TextStatus.Normal);
        Color bg = colorConfig.GetColor(ColorStyle.Background);

        var bgColorBrush = new SolidBrushDef(bg);

        _list = selList ?? throw new ArgumentNullException();
        _option = option;
        _initKey = option?.SelectedKey ?? string.Empty;
        _allowSelectList = option?.AllowSelectList ?? false;

        _model = new SelectionModel(_list, option);
        _treeView = new SelectionTreeView(_model);
        _treeView.DoubleClicked += treeViewAdv_DoubleClick;

        if (option?.Multiple == true)
        {
            _model.TreeData.SelectionMode = ImTreeViewSelectionMode.MultipleSameParent;
        }
        else
        {
            _model.TreeData.SelectionMode = ImTreeViewSelectionMode.Single;
        }

        if (!string.IsNullOrEmpty(title))
        {
            this.Title = title;
        }
        else
        {
            this.Title = L("Browse Items");
        }

        _timer = new System.Threading.Timer(_timerCallBack);
    }

    public object? SelectedObject => _treeView.SelectedNode;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var theme = AvaImGuiService.Instance.GetEditorTheme(false);

        ImGuiControl.GuiTheme = theme;
        ImGuiControl.BackgroundColor = theme.Colors.GetColor(ColorStyle.Background);

        ImGuiControl.DrawImGui = this;
    }

    public void OnGui(ImGui gui)
    {
        _nodeRef.Node = gui.Frame("#Selection")
        .InitFullSize()
        .OnContent(() => 
        {
            string newSearch = gui.StringInput("search", _inputText, submitMode: TextBoxEditSubmitMode.TextChanged)
            .OnInitialize(n =>
            {
                QueuedAction.Do(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_initKey))
                    {
                        if (!SelectItem(_initKey))
                        {
                            n.BeginEdit();
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(_lastKey))
                    {
                        SelectItem(_lastKey);
                        n.BeginEdit();
                    }
                    else
                    {
                        n.BeginEdit();
                    }
                });
            })
            .InitFullWidth()
            .InitFitVertical().Text ?? string.Empty;

            if (newSearch != _inputText)
            {
                _inputText = newSearch;
                _timer.Change(_delayTime, System.Threading.Timeout.Infinite);
            }

            _treeView.OnGui(gui, "tree_view", n => n.InitSizeRest());
        });
    }

    #region Status

    public bool IsSuccess { get; private set; }

    public bool IsClosed { get; private set; }

    public ISelectionItem? SelectedItem => _treeView.SelectedNode;
    public IEnumerable<ISelectionItem> SelectedItems => _treeView.SelectedNodes;

    public string? SelectedKey => SelectedItem?.SelectionKey;

    public string InputText => _inputText;

    #endregion

    #region Event
    private void treeViewAdv_DoubleClick(ISelectionItem item)
    {
        CheckSelectionAndOkClose();
    }

    private void _timerCallBack(object? state)
    {
        ThreadedUpdateFilter();
    }
    #endregion

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Enter:
                CheckSelectionAndOkClose();
                break;

            case Key.Escape:
                Close();
                break;
        }
    }

    private bool SelectItem(string key)
    {
        if (FindItem(key) is { } item)
        {
            _treeView.ClearSelection();
            _treeView.SelectNode(item);

            _nodeRef.QueueRefresh();

            return true;
        }

        return false;
    }

    private ISelectionItem? FindItem(string key)
    {
        if (_list.GetItem(key) is { } item)
        {
            return item;
        }

        foreach (var node in _list.GetItems().OfType<ISelectionNode>())
        {
            if (node.GetItem(key) is { } subItem)
            {
                return subItem;
            }
        }

        return null;
    }

    private void ThreadedUpdateFilter()
    {
        string filter = _inputText;
        _model.Filter(filter);

        if (IsClosed)
        {
            return;
        }

        QueuedAction.Do(() =>
        {
            try
            {
                //treeViewAdv.ExpandAll();
                _treeView.QueueRefresh();

                var primary = _model.PrimaryItem;
                if (primary != null)
                {
                    _treeView.ClearSelection();
                    _treeView.SelectNode(primary);
                }
                else
                {
                    ISelectionItem? first;

                    first = _model.Items.FirstOrDefault();
                // No need to select the second item, because search filtering will filter out the first EmptySelection
                //if (_hideEmpty)
                //{
                //    first = _model.Items.FirstOrDefault();
                //}
                //else
                //{
                //    // The first is empty selection, default to select the second

                //    first = _model.Items.Skip(1).FirstOrDefault();
                    //    if (first is null)
                    //    {
                    //        first = _model.Items.FirstOrDefault();
                    //    }
                    //}

                    if (first != null)
                    {
                        _treeView.ClearSelection();
                        _treeView.SelectNode(first);
                    }
                }
            }
            catch (Exception)
            {
            }
        });
    }

    private void CheckSelectionAndOkClose()
    {
        var obj = SelectedObject;

        if (obj is ITextDisplay display && display.DisplayStatus != TextStatus.Normal)
        {
            return;
        }

        if (obj is ISelectionNode node)
        {
            if (node.Selectable)
            {
                // This node is selectable, select it directly and close
                OkClose();
            }

            return;
        }

        if (obj is ISelectionList && !_allowSelectList)
        {
            return;
        }

        if (obj is ISelectionItem || obj is null)
        {
            OkClose();
        }
    }

    private void OkClose()
    {
        _lastKey = SelectedKey;
        IsSuccess = true;

        Close();
    }
}