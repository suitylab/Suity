using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Virtual list data source for items with a fixed height.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class FixedVisualListData<T>(IList<T> list, float height) : VisualListData<T>
{
    /// <summary>
    /// Gets the underlying list of items.
    /// </summary>
    public IList<T> List { get; } = list;

    /// <summary>
    /// Gets the fixed height of each item.
    /// </summary>
    public float Height { get; } = height;

    /// <inheritdoc/>
    public override float TotalHeight => (Height + Spacing) * Count;

    /// <inheritdoc/>
    public override int Count => List.Count;

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    public T this[int index] => List[index];

    /// <inheritdoc/>
    public override object? GetItemAt(int index) => List[index];

    /// <inheritdoc/>
    public override void PropagateContents(ImGuiNode node, NodeFactory nodeFactory, float posX, float posY)
    {
        //Debug.Print($"propagate scroll={scroll}");

        float h = Height;
        float height = h + Spacing;
        if (height <= 1)
        {
            return;
        }

        int indexBegin = (int)(posY / height);
        if (indexBegin < 0)
        {
            return;
        }

        var rect = node.InnerRect;
        var y = -posY + indexBegin * height;
        if (HeaderHeight is { } headerH)
        {
            y += headerH;
        }

        node.SetInitialLayoutPosition(new PointF(-posX, y));

        var gui = node.Gui;

        var list = List;
        var template = RowTemplate;

        for (int i = indexBegin; i < list.Count; i++)
        {
            var value = list[i];

            var itemNode = nodeFactory(gui, i.ToString(), value);

            if (Width.HasValue)
            {
                // By absolute value
                itemNode.SetSize(Width.Value, GuiLengthMode.ScaledFixed, h, GuiLengthMode.ScaledFixed);
            }
            else
            {
                // By percentage
                itemNode.SetSize(100, GuiLengthMode.Percentage, h, GuiLengthMode.ScaledFixed);
            }

            template?.Invoke(itemNode, value);

            if (node.CurrentLayoutPosition.Y >= rect.Height)
            {
                break;
            }
        }
    }
}

/// <summary>
/// Virtual list data source for items with variable (ranged) heights.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class RangedVisualListData<T> : VisualListData<T>
{
    /// <summary>
    /// The default item height when not specified.
    /// </summary>
    public const float DefaultItemHeight = 16;

    private readonly RangeCollection<T> _collection = new();
    private readonly LengthGetter<T> _heightGetter;
    private readonly float _defaultLen;

    /// <summary>
    /// Initializes a new ranged visual list data with a height getter.
    /// </summary>
    /// <param name="heightGetter">Function to get the height of an item.</param>
    /// <param name="defaultLen">The default item height.</param>
    public RangedVisualListData(LengthGetter<T> heightGetter, float defaultLen = DefaultItemHeight)
    {
        _heightGetter = heightGetter ?? throw new ArgumentNullException(nameof(heightGetter));
        _defaultLen = Math.Max(0, defaultLen);
    }

    /// <summary>
    /// Initializes a new ranged visual list data with initial items.
    /// </summary>
    /// <param name="items">The initial items.</param>
    /// <param name="heightGetter">Function to get the height of an item.</param>
    /// <param name="defaultHeight">The default item height.</param>
    public RangedVisualListData(
        IEnumerable<T> items,
        LengthGetter<T> heightGetter,
        float defaultHeight = DefaultItemHeight)
        : this(heightGetter, defaultHeight)
    {
        foreach (var value in items)
        {
            AppendItem(value);
        }
    }

    /// <inheritdoc/>
    public override float TotalHeight => _collection.TotalLength;

    /// <inheritdoc/>
    public override int Count => _collection.Count;

    /// <summary>
    /// Gets the default item height.
    /// </summary>
    public float DefaultHeight => _defaultLen;

    /// <summary>
    /// Appends an item to the list.
    /// </summary>
    /// <param name="item">The item to append.</param>
    /// <returns>The new total length.</returns>
    public int AppendItem(T item)
    {
        float len = _heightGetter(item);
        len += Spacing;

        _collection.Append(item, (int)len);
        SetIndex(item, _collection.Count - 1);

        return _collection.TotalLength;
    }

    /// <summary>
    /// Appends multiple items to the list.
    /// </summary>
    /// <param name="items">The items to append.</param>
    public void AppendItems(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            AppendItem(item);
        }
    }

    /// <summary>
    /// Clears all items from the list.
    /// </summary>
    public void Clear()
    {
        _collection.Clear();
    }

    /// <summary>
    /// Finds the item index at the given position.
    /// </summary>
    /// <param name="position">The position to search.</param>
    /// <returns>The item index.</returns>
    public int FindIndex(int position) => _collection.FindIndex(position);

    /// <summary>
    /// Finds the item value at the given position.
    /// </summary>
    /// <param name="position">The position to search.</param>
    /// <returns>The item value.</returns>
    public T FindValue(int position) => _collection.FindValue(position);

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    public T this[int index] => _collection[index].Value;

    /// <inheritdoc/>
    public override object? GetItemAt(int index) => _collection[index].Value;

    /// <inheritdoc/>
    public override void PropagateContents(ImGuiNode node, NodeFactory nodeFactory, float posX, float posY)
    {
        int indexBegin = _collection.FindIndexMinMax((int)posY);
        if (indexBegin < 0)
        {
            return;
        }

        var y = -posY + _collection[indexBegin].Low;
        if (HeaderHeight is { } headerH)
        {
            y += headerH;
        }

        node.SetInitialLayoutPosition(new PointF(-posX, y));

        if (indexBegin >= _collection.Count)
        {
            return;
        }

        var gui = node.Gui;
        var template = RowTemplate;
        var rect = node.InnerRect;

        for (int i = indexBegin; i < _collection.Count; i++)
        {
            var group = _collection[i];
            float h = group.High - group.Low + 1 - Spacing;

            string id = GetId(group.Value, i);
            if (string.IsNullOrEmpty(id))
            {
                id = i.ToString();
            }

            var itemNode = nodeFactory(gui, id, group.Value);

            if (Width.HasValue)
            {
                // By absolute value
                itemNode.SetSize(Width.Value, GuiLengthMode.ScaledFixed, h, GuiLengthMode.ScaledFixed);
            }
            else
            {
                // By percentage
                itemNode.SetSize(100, GuiLengthMode.Percentage, h, GuiLengthMode.ScaledFixed);
            }

            template?.Invoke(itemNode, group.Value);

            if (node.CurrentLayoutPosition.Y >= rect.Height)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Sets the index for a value. Override to customize indexing behavior.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    protected virtual void SetIndex(T value, int index)
    { }

    /// <summary>
    /// Gets the ID for a value at a given index. Override to customize ID generation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    /// <returns>The ID string.</returns>
    protected virtual string GetId(T value, int index)
    {
        return index.ToString();
    }
}