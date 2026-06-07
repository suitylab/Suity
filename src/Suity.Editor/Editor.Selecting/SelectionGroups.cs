using Suity.Selecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Selecting;

/// <summary>
/// Represents a group node in the grouped asset selection list.
/// </summary>
class ParentAssetSelectionGroup : BaseSelectionNode
{
    private readonly IGrouping<Asset, Asset> _group;
    private readonly bool _selectable;

    /// <summary>
    /// Initializes a new instance of the GroupNode class.
    /// </summary>
    /// <param name="group">The asset grouping.</param>
    /// <param name="selectable">Whether the group is selectable.</param>
    public ParentAssetSelectionGroup(IGrouping<Asset, Asset> group, bool selectable = false)
    {
        _group = group ?? throw new ArgumentNullException(nameof(group));
        _selectable = selectable;
    }

    /// <inheritdoc />
    public override string SelectionKey => _group.Key?.AssetKey;

    /// <inheritdoc />
    public override string Name => _group.Key?.Name;

    /// <inheritdoc />
    public override string DisplayText => _group.Key?.ToDisplayText() ?? "(Other)";

    /// <inheritdoc />
    public override object DisplayIcon => _group.Key?.Icon;

    /// <inheritdoc />
    public override TextStatus DisplayStatus => _group.Key?.DisplayStatus ?? TextStatus.Normal;

    /// <inheritdoc />
    public override bool Selectable => _selectable;

    /// <inheritdoc />
    public override IEnumerable<ISelectionItem> GetItems() => _group.OrderBy(o => o.AssetKey);

    /// <inheritdoc />
    public override ISelectionItem GetItem(string key) => _group.FirstOrDefault(o => o.AssetKey == key);
}

class CategorySelectionGroup : BaseSelectionNode, IPreviewDisplay
{
    private readonly string _selectionKey;
    private readonly string _category;

    private readonly ISelectionItem[] _items;
    private readonly string _displayText;

    /// <summary>
    /// Initializes a new instance of the GroupNode class.
    /// </summary>
    /// <param name="group">The asset grouping.</param>
    public CategorySelectionGroup(string selectionKey, string category, IEnumerable<ISelectionItem> items, string displayText = null)
    {
        _selectionKey = selectionKey ?? throw new ArgumentNullException(nameof(selectionKey));
        _category = category ?? throw new ArgumentNullException(nameof(category));
        _items = items?.OrderBy(o => o.SelectionKey).ToArray() ?? throw new ArgumentNullException(nameof(items));
        _displayText = _category;
    }

    /// <inheritdoc />
    public override string SelectionKey => _selectionKey;

    /// <inheritdoc />
    public override string Name => _category;

    /// <inheritdoc />
    public override string DisplayText => _displayText;

    /// <inheritdoc />
    public override object DisplayIcon => null;

    /// <inheritdoc />
    public override TextStatus DisplayStatus => TextStatus.Normal;

    /// <inheritdoc />
    public override bool Selectable => false;

    #region IPreviewDisplay
    public string PreviewText => string.Empty;

    public object PreviewIcon => null; 
    #endregion

    /// <inheritdoc />
    public override IEnumerable<ISelectionItem> GetItems() => _items;

    /// <inheritdoc />
    public override ISelectionItem GetItem(string key) => _items.FirstOrDefault(o => o.SelectionKey == key);
}