using FastFuzzyStringMatcher;
using FuzzySharp;
using FuzzySharp.Extractor;
using Suity.Collections;
using Suity.Helpers;
using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Editor.Views.Selecting;

public interface IFilteredSelectionList : ISelectionList
{
    IEnumerable<ISelectionItem> GetFilteredItems(string filter, SelectionOption option = null);
}

public class ScoreSharpSelectionList : IFilteredSelectionList
{
    private readonly ISelectionList _list;

    public ScoreSharpSelectionList(ISelectionList list)
    {
        _list = list ?? throw new ArgumentNullException(nameof(list));
    }

    public IEnumerable<ISelectionItem> GetItems() => _list.GetItems();

    public ISelectionItem GetItem(string key) => _list.GetItem(key);

    public IEnumerable<ISelectionItem> GetFilteredItems(string filter, SelectionOption option = null)
    {
        filter = filter.ToLowerInvariant();

        var origin = _list.GetItems().Where(item =>
        {
            string s = $"{item.ToDisplayText()} {item.SelectionKey}".ToLowerInvariant();
            double score = ScoreSharp.score(s, filter, 0.12);
            return score > 0.2;
        });

        ISelectionItem[] ary;

        if (option?.HideEmptySelection == true)
        {
            ary = [EmptyGuiSelectionItem.Empty, .. origin];
        }
        else
        {
            ary = origin.ToArray();
        }

        return ScoreSharp.sorter(ary, filter);
    }
}

//public class PhoneticNavigationList : IFilteredNavigationList
//{
//    readonly INavigationList _list;
//    StringFuzzyMatcher<INavigationItem> _matcher;

//    public PhoneticNavigationList(INavigationList list)
//    {
//        _list = list ?? throw new ArgumentNullException(nameof(list));
//    }

//    public IEnumerable<INavigationItem> GetFilteredItems(string filter, NavigationOption option = null)
//    {
//        if (_matcher == null)
//        {
//            _matcher = new StringFuzzyMatcher<INavigationItem>(_list.GetItems().ToList(), o => $"{o.DisplayText} {o.NaviKey}");
//        }

//        var result = _matcher.FindNearest(filter);

//        return _matcher.FindNearest(filter, 100).OrderBy(o => o.Distance).Select(o => o.Element);
//    }

//    public IEnumerable<INavigationItem> GetItems()
//    {
//        return _list.GetItems();
//    }

//    public INavigationItem GetItem(string key)
//    {
//        return _list.GetItem(key);
//    }
//}

public class FastFuzzySelectionList : IFilteredSelectionList
{
    private readonly ISelectionList _list;

    private StringMatcher<ISelectionItem> _matcher;

    public FastFuzzySelectionList(ISelectionList list)
    {
        _list = list ?? throw new ArgumentNullException(nameof(list));
    }

    public IEnumerable<ISelectionItem> GetFilteredItems(string filter, SelectionOption option = null)
    {
        filter = filter.ToLowerInvariant();

        if (_matcher == null)
        {
            _matcher = new StringMatcher<ISelectionItem>();
            foreach (var item in _list.GetItems())
            {
                string s = $"{item.ToDisplayText()} {item.SelectionKey}".ToLowerInvariant();
                _matcher.Add(s, item);
            }
        }

        var match = _matcher.Search(filter, 100);
        var result = match.Select(o => o.AssociatedData);

        if (option?.HideEmptySelection == true)
        {
            return new ISelectionItem[] { EmptyGuiSelectionItem.Empty }.Concat(result);
        }
        else
        {
            return result;
        }
    }

    public IEnumerable<ISelectionItem> GetItems()
    {
        return _list.GetItems();
    }

    public ISelectionItem GetItem(string key)
    {
        return _list.GetItem(key);
    }
}

public class FuzzySharpSelectionList : IFilteredSelectionList
{
    public const int MinChar = 2;

    private readonly HashSet<ISelectionList> _listCache = [];
    private readonly Dictionary<string, ISelectionItem> _items = [];

    public FuzzySharpSelectionList(ISelectionList list)
    {
        CollectionItem(list);
    }

    private void CollectionItem(ISelectionList list)
    {
        if (list is null)
        {
            return;
        }

        if (!_listCache.Add(list))
        {
            return;
        }

        foreach (var item in list.GetItems().SkipNull())
        {
            if (string.IsNullOrWhiteSpace(item.SelectionKey))
            {
                continue;
            }

            _items.TryAdd(item.SelectionKey, item);
        }

        foreach (var childList in list.GetItems().OfType<ISelectionList>())
        {
            CollectionItem(childList);
        }
    }

    public IEnumerable<ISelectionItem> GetItems() => _items.Values;

    public ISelectionItem GetItem(string key) => _items.GetValueSafe(key);

    public IEnumerable<ISelectionItem> GetFilteredItems(string filter, SelectionOption option = null)
    {
        // Get length that supports dual characters
        if (string.IsNullOrWhiteSpace(filter) || Encoding.Default.GetByteCount(filter) < MinChar)
        {
            return [];
        }

        string f = filter.ToLowerInvariant();

        var preAry = _items.Values
            .Where(o => o is not ISelectionNode)
            .Where(o => (o.ToName() is string n && n.ToLowerInvariant().Contains(f)) ||
                (o.SelectionKey != null && o.SelectionKey.ToLowerInvariant().Contains(f)) ||
                (o.ToDisplayText() is string ds && ds.ToLowerInvariant().Contains(f)))
            .ToArray();

        string[] s = _items.Values.Select(o => o.ToDisplayText().ToLowerInvariant()).ToArray();
        
        var query = new SearchQueryItem(filter.ToLowerInvariant());

        var result1 = Process.ExtractSorted(query, preAry, o => o.ToName()?.ToLowerInvariant() ?? string.Empty, cutoff: 0);
        var result2 = Process.ExtractSorted(query, preAry, o => o.SelectionKey?.ToLowerInvariant() ?? string.Empty, cutoff: 0);
        var result3 = Process.ExtractSorted(query, preAry, o => o.ToDisplayText()?.ToLowerInvariant() ?? string.Empty, cutoff: 0);

        List<ExtractedResult<ISelectionItem>> list = [.. result1, .. result2, .. result3];

        list.Sort((a, b) =>
        {
            string nameA = a.Value.ToName();
            string nameB = b.Value.ToName();

            if (nameA == nameB)
            {
                return 0;
            }

            if (string.Equals(filter, nameA, StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            if (string.Equals(filter, nameB, StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return -a.Score.CompareTo(b.Score);
        });

        return list.Select(o => o.Value).Take(100).Distinct().Take(50).ToArray();

        //var result = Process.ExtractSorted<ISelectionItem>(query, _list.GetItems(), o => $"{o.DisplayText} {o.ItemKey}".ToLowerInvariant(), cutoff: 30);

        //return result.Take(50).Select(o => o.Value).ToArray();
    }

    private class SearchQueryItem(string query) : ISelectionItem
    {
        public string SelectionKey { get; } = query;
    }
}