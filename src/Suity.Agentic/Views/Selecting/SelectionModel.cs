using Suity.Selecting;
using Suity.Views.Im.TreeEditing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Views.Selecting;

internal class SelectionModel : ImGuiTreeModel<ISelectionItem>
{
    public static bool AdvSortMode = true;

    private readonly ISelectionList _list;
    private readonly SelectionOption _option;
    private readonly IFilteredSelectionList _filteredList;

    private ISelectionItem _firstItem;

    private IEnumerable<ISelectionItem> _items = [];

    public SelectionModel(ISelectionList list, SelectionOption option)
    {
        _list = list ?? EmptySelectionList.Empty;

        //_filteredList = new ScoreSharpSelectionList(_list);
        _filteredList = new FuzzySharpSelectionList(_list);

        _option = option;

        _items = GetChildrenUnsorted(list);
    }

    public void Filter(string filter)
    {
        if (_list is null)
        {
            _items = [];
        }

        _firstItem = null;

        if (!string.IsNullOrEmpty(filter))
        {
            _items = _filteredList.GetFilteredItems(filter, _option) ?? [];
        }
        else
        {
            _items = GetChildrenUnsorted(_list);
        }
    }

    public ISelectionItem PrimaryItem => _firstItem;
    public IEnumerable<ISelectionItem> Items => _items;

    private IEnumerable<ISelectionItem> GetChildrenUnsorted(ISelectionList list)
    {
        if (_option?.InitialHideItems == true)
        {
            return [];
        }
        else
        {
            if (_option?.HideEmptySelection == true)
            {
                return list.GetItems();
            }
            else
            {
                return new ISelectionItem[] { EmptyGuiSelectionItem.Empty }.Concat(list.GetItems());
            }
        }
    }

    #region ImGuiTreeModel

    public override string GetId(ISelectionItem value)
    {
        return value.SelectionKey ?? "?Empty?";
    }

    public override IEnumerable<ISelectionItem> GetChildNodes()
    {
        if (_option?.DisplayFilter is Predicate<ISelectionItem> filter)
        {
            return _items.Where(o => filter(o));
        }
        else
        {
            return _items;
        }
    }

    public override IEnumerable<ISelectionItem> GetChildNodes(ISelectionItem value)
    {
        if (value is ISelectionList list)
        {
            if (_option?.DisplayFilter is Predicate<ISelectionItem> filter)
            {
                return list.GetItems().Where(o => filter(o));
            }
            else
            {
                return list.GetItems();
            }
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetParent(ISelectionItem value)
    {
        string key = value.SelectionKey;

        if (_list.GetItem(key) != null)
        {
            // Top level
            return null;
        }

        // Support two-level retrieval
        foreach (var node in _list.GetItems().OfType<ISelectionNode>())
        {
            if (node.GetItem(key) is { } subItem)
            {
                return node;
            }
        }

        return null;
    }

    #endregion
}