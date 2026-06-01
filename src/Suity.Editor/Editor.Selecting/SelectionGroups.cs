using Suity.Selecting;
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

class CategoryAssetSelectionGroup : BaseSelectionNode
{
    private readonly string _categoryName;

    private readonly IGrouping<string, Asset> _group;
    private readonly string _displayText;

    /// <summary>
    /// Initializes a new instance of the GroupNode class.
    /// </summary>
    /// <param name="group">The asset grouping.</param>
    public CategoryAssetSelectionGroup(IGrouping<string, Asset> group, string categoryName = null, string displayText = null)
    {
        _categoryName = categoryName;
        _group = group ?? throw new ArgumentNullException(nameof(group));
        _displayText = displayText;
    }

    /// <inheritdoc />
    public override string SelectionKey => _group.Key;

    /// <inheritdoc />
    public override string Name => _categoryName;

    /// <inheritdoc />
    public override string DisplayText => _displayText ?? _categoryName;

    /// <inheritdoc />
    public override object DisplayIcon => null;

    /// <inheritdoc />
    public override TextStatus DisplayStatus => TextStatus.Normal;

    /// <inheritdoc />
    public override bool Selectable => false;

    /// <inheritdoc />
    public override IEnumerable<ISelectionItem> GetItems() => _group.OrderBy(o => o.AssetKey);

    /// <inheritdoc />
    public override ISelectionItem GetItem(string key) => _group.FirstOrDefault(o => o.AssetKey == key);
}