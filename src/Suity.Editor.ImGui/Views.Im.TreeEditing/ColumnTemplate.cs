using Suity.Collections;
using Suity.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.TreeEditing;

#region ColumnConfig

/// <summary>
/// Represents the configuration for a single column in a tree view.
/// </summary>
/// <typeparam name="T">The type of data represented by each tree node.</typeparam>
public class ColumnConfig<T>
{
    /// <summary>
    /// Gets the zero-based index of this column within its group.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets or sets the display title of the column header.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether this column is visible and rendered.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a custom action to render the column header content.
    /// If null, the column <see cref="Title"/> will be displayed as text.
    /// </summary>
    public Action<ImGuiNode>? HeaderGui { get; set; }

    /// <summary>
    /// Gets or sets a custom action to render the column cell content for each row.
    /// </summary>
    /// <param name="node">The ImGui node for the cell.</param>
    /// <param name="item">The data item for the current row.</param>
    public Action<ImGuiNode, T>? RowGui { get; set; }

    /// <summary>
    /// Gets or sets an optional color associated with this column.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets an optional icon associated with this column.
    /// </summary>
    public ImageDef? Icon { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary object tag for storing custom data associated with this column.
    /// </summary>
    public object? Tag { get; set; }
}

#endregion

#region ColumnConfigGroup

/// <summary>
/// Manages a collection of <see cref="ColumnConfig{T}"/> objects for a tree view.
/// </summary>
/// <typeparam name="T">The type of data represented by each tree node.</typeparam>
public class ColumnConfigGroup<T>
{
    private readonly List<ColumnConfig<T>?> _columns = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnConfigGroup{T}"/> class.
    /// </summary>
    public ColumnConfigGroup()
    {
    }

    /// <summary>
    /// Gets or sets the number of columns in this group.
    /// Setting this value will add or remove columns to match the specified count.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 100.</exception>
    public int Count
    {
        get => _columns.Count;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            while (value > _columns.Count)
            {
                _columns.Add(new ColumnConfig<T>());
            }

            while (value < _columns.Count)
            {
                _columns.RemoveAt(_columns.Count - 1);
            }
        }
    }

    /// <summary>
    /// Gets or sets the column configuration at the specified index.
    /// If the index is beyond the current count, empty columns are added to fill the gap.
    /// </summary>
    /// <param name="index">The zero-based index of the column.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is less than 0 or greater than 100.</exception>
    public ColumnConfig<T>? this[int index]
    {
        get => _columns.GetListItemSafe(index);
        set
        {
            if (index < 0 || index > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            while (index >= _columns.Count)
            {
                _columns.Add(new ColumnConfig<T>());
            }

            _columns[index] = value;

            value?.Index = index;
        }
    }

    /// <summary>
    /// Ensures a column exists at the specified index, creating empty columns if necessary.
    /// </summary>
    /// <param name="index">The zero-based index of the column to ensure.</param>
    /// <returns>The <see cref="ColumnConfig{T}"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is less than 0 or greater than 100.</exception>
    public ColumnConfig<T> EnsureColumn(int index)
    {
        if (index < 0 || index > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        while (index >= _columns.Count)
        {
            _columns.Add(new ColumnConfig<T>());
        }

        return _columns[index]!;
    }

    /// <summary>
    /// Gets an enumerable of all column configurations in this group.
    /// </summary>
    public IEnumerable<ColumnConfig<T>?> Columns => _columns.Pass();

    /// <summary>
    /// Removes the column at the specified index and updates the indices of remaining columns.
    /// </summary>
    /// <param name="index">The zero-based index of the column to remove.</param>
    /// <returns>True if the column was removed; false if the index was out of range.</returns>
    public bool RemoveColumn(int index)
    {
        if (!_columns.RemoveAtSafe(index))
        {
            return false;
        }

        for (int i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            column?.Index = i;
        }

        return true;
    }

    /// <summary>
    /// Swaps the positions of two columns and updates their indices.
    /// </summary>
    /// <param name="index">The zero-based index of the first column.</param>
    /// <param name="indexTo">The zero-based index of the second column.</param>
    /// <returns>True if the swap was successful; false if either index was out of range.</returns>
    public bool SwapColumn(int index, int indexTo)
    {
        if (!_columns.SwapListItem(index, indexTo))
        {
            return false;
        }

        var path = _columns[index];
        var pathTo = _columns[indexTo];

        path?.Index = indexTo;
        pathTo?.Index = index;

        return true;
    }

    /// <summary>
    /// Removes a column from one position and inserts it at another position, then updates all indices.
    /// </summary>
    /// <param name="indexFrom">The zero-based index of the column to move.</param>
    /// <param name="indexInsert">The zero-based index where the column should be inserted.</param>
    /// <returns>True if the operation was successful; false if the source index was out of range.</returns>
    public bool RemoveInsertColumn(int indexFrom, int indexInsert)
    {
        if (!_columns.RemoveInserListItem(indexFrom, indexInsert))
        {
            return false;
        }

        for (int i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            column?.Index = i;
        }

        return true;
    }
}

#endregion

#region ColumnTemplate

/// <summary>
/// A tree view template that renders columns with configurable headers and row content.
/// Implements <see cref="ITreeViewTemplate{T}"/> to provide column-based rendering with resizable column widths.
/// </summary>
/// <typeparam name="T">The type of data represented by each tree node.</typeparam>
public class ColumnTemplate<T> : ITreeViewTemplate<T>
    where T : class
{
    private readonly GroupedResizerState _resizerState = new();

    private readonly ColumnConfigGroup<T> _columnConfigs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnTemplate{T}"/> class with default columns:
    /// Name (enabled), Description (disabled), and Preview (enabled).
    /// </summary>
    public ColumnTemplate()
    {
        _resizerState.SetLengths(200, 100, 200);

        _columnConfigs[0] = new ColumnConfig<T> { Enabled = true, Title = "Name" };
        _columnConfigs[1] = new ColumnConfig<T> { Enabled = false, Title = "Description" };
        _columnConfigs[2] = new ColumnConfig<T> { Enabled = true, Title = "Preview" };
    }

    /// <summary>
    /// Gets or sets whether column resizers span the full height of each column.
    /// When false, resizers will fit to content height instead.
    /// </summary>
    public bool FullColumnResizer { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum width in pixels for column resizing.
    /// </summary>
    public int? ResizerMin { get; set; } = 40;

    /// <summary>
    /// Gets or sets the maximum width in pixels for column resizing. Null means no maximum.
    /// </summary>
    public int? ResizerMax { get; set; } = null;

    /// <summary>
    /// Gets or sets the size in pixels for column icons.
    /// </summary>
    public int IconSize { get; set; } = 16;

    /// <summary>
    /// Gets or sets the default width in pixels for columns that have no explicit size set.
    /// </summary>
    public int DefaultLength { get; set; } = 150;

    /// <summary>
    /// Gets the grouped resizer state that manages column widths.
    /// </summary>
    public GroupedResizerState ResizerState => _resizerState;

    /// <summary>
    /// Gets the group of column configurations for this template.
    /// </summary>
    public ColumnConfigGroup<T> ColumnConfigs => _columnConfigs;

    /// <summary>
    /// Gets or sets an optional action invoked during row rendering at different pipeline stages.
    /// </summary>
    public Action<ImGuiNode, T, EditorImGuiPipeline>? RowPipeline { get; set; }

    /// <summary>
    /// Gets or sets an optional action invoked when a row begins editing mode.
    /// </summary>
    public Action<ImGuiNode>? BeginEditAction { get; set; }

    /// <summary>
    /// Initializes the tree view with the resizer state.
    /// </summary>
    /// <param name="treeViewNode">The root ImGui node for the tree view.</param>
    public void TreeViewGui(ImGuiNode treeViewNode)
    {
        treeViewNode.InitValue(_resizerState);
    }

    /// <summary>
    /// Renders the header section of the tree view with column titles and resizers.
    /// Each column displays either its custom <see cref="ColumnConfig{T}.HeaderGui"/> or its <see cref="ColumnConfig{T}.Title"/>.
    /// </summary>
    /// <param name="node">The ImGui node for the header.</param>
    /// <param name="headerHeight">Optional height override for the header.</param>
    public virtual void HeaderGui(ImGuiNode node, float? headerHeight = null)
    {
        float hh = headerHeight ?? ImGuiTreeView.DefaultHeaderHeight;

        node.InitRenderFunction(nameof(GuiCommonExtensions.Frame))
        .InitClass("header")
        .InitHeight(hh)
        .OnPartialContent(() =>
        {
            var gui = node.Gui;

            GroupedResizerState? state = null;
            state = node.FindValueInHierarchy<GroupedResizerState>();

            int count = _columnConfigs.Count;

            for (int i = 0; i < count; i++)
            {
                if (!(_columnConfigs[i] is { } c) || !c.Enabled)
                {
                    continue;
                }

                if (c.HeaderGui is { } headerGui)
                {
                    gui.HorizontalLayout($"##c{i}")
                    .SetWidth(state?.GetLength(i, DefaultLength) ?? DefaultLength)
                    .InitFullHeight()
                    .OnContent(n =>
                    {
                        try
                        {
                            headerGui(n);
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    });
                }
                else
                {
                    gui.Text($"##t{i}", L(c.Title ?? string.Empty))
                    .InitVerticalAlignment(GuiAlignment.Center, true)
                    .SetWidth(state?.GetLength(i, DefaultLength) ?? 150);
                }

                gui.HorizontalResizer($"##r{i}", ResizerMin, ResizerMax)
                .InitClass("resizer")
                .InitGroupedResizer(i);
            }
        });
    }

    /// <summary>
    /// Renders a single row in the tree view with column cells and resizers.
    /// Invokes <see cref="RowPipeline"/> at Begin, Normal, and End pipeline stages.
    /// </summary>
    /// <param name="node">The ImGui node for the row.</param>
    /// <param name="item">The visual tree node data to display.</param>
    public virtual void RowGui(ImGuiNode node, VisualTreeNode<T> item)
    {
        var v = item.Value;
        var gui = node.Gui;

        RowPipeline?.Invoke(node, v, EditorImGuiPipeline.Normal);

        node.OnPartialContent(() =>
        {
            RowPipeline?.Invoke(node, v, EditorImGuiPipeline.Begin);

            GroupedResizerState? state = null;
            state = node.FindValueInHierarchy<GroupedResizerState>();

            int count = _columnConfigs.Count;

            for (int i = 0; i < count; i++)
            {
                if (!(_columnConfigs[i] is { } c) || !c.Enabled)
                {
                    continue;
                }

                if (i == 0)
                {
                    gui.TreeNodeTitle(item, n =>
                    {
                        try
                        {
                            c.RowGui?.Invoke(n, v);
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    })
                    .SetWidth(state?.GetLength(i, DefaultLength) ?? 150)
                    .InitFullHeight();
                }
                else
                {
                    gui.HorizontalLayout($"##c{i}")
                    .SetWidth(state?.GetLength(i, DefaultLength) ?? 150)
                    .InitFullHeight()
                    .OnContent(n =>
                    {
                        try
                        {
                            c.RowGui?.Invoke(n, v);
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    });
                }

                if (FullColumnResizer)
                {
                    gui.HorizontalResizer($"##r{i}", ResizerMin, ResizerMax)
                    .InitClass("resizer")
                    .InitGroupedResizer(i);
                }
                else
                {
                    gui.HorizontalResizerFitter($"##r{i}", ResizerMin, ResizerMax)
                    .InitClass("resizer")
                    .InitGroupedResizer(i);
                }
            }

            RowPipeline?.Invoke(node, v, EditorImGuiPipeline.End);
        });
    }

    /// <summary>
    /// Called when a row begins editing mode. Invokes <see cref="BeginEditAction"/> if set.
    /// </summary>
    /// <param name="rowNode">The ImGui node for the row being edited.</param>
    public virtual void BeginRowEdit(ImGuiNode rowNode)
    {
        BeginEditAction?.Invoke(rowNode);
    }

    /// <summary>
    /// Removes the column at the specified index and its corresponding resizer state.
    /// </summary>
    /// <param name="index">The zero-based index of the column to remove.</param>
    public void RemoveColumn(int index)
    {
        _columnConfigs.RemoveColumn(index);
        _resizerState.RemoveAt(index);
    }

    /// <summary>
    /// Swaps the positions of two columns and their corresponding resizer states.
    /// </summary>
    /// <param name="index">The zero-based index of the first column.</param>
    /// <param name="indexTo">The zero-based index of the second column.</param>
    public void SwapColumn(int index, int indexTo)
    {
        _columnConfigs.SwapColumn(index, indexTo);
        _resizerState.Swap(index, indexTo);
    }

    /// <summary>
    /// Removes a column from one position and inserts it at another, updating both column configs and resizer state.
    /// </summary>
    /// <param name="indexFrom">The zero-based index of the column to move.</param>
    /// <param name="indexInsert">The zero-based index where the column should be inserted.</param>
    public void RemoveInsertColumn(int indexFrom, int indexInsert)
    {
        // Unknown reason: error occurs after insert operation
        _columnConfigs.RemoveInsertColumn(indexFrom, indexInsert);
        _resizerState.RemoveInsert(indexFrom, indexInsert);
    }

    /// <summary>
    /// Gets the current widths of all columns from the resizer state.
    /// </summary>
    /// <returns>An array of column widths in pixels.</returns>
    public float[] GetColumnWidths()
    {
        int len = _columnConfigs.Count;

        float[] widths = new float[len];

        for (int i = 0; i < len; i++)
        {
            widths[i] = _resizerState.GetLength(i, ResizerMin ?? 40);
        }

        return widths;
    }

    /// <summary>
    /// Sets the widths of columns from the provided array.
    /// </summary>
    /// <param name="widths">An array of column widths in pixels.</param>
    public void SetColumnWidths(float[] widths)
    {
        if (widths is null || widths.Length == 0)
        {
            return;
        }

        for (int i = 0; i < widths.Length; i++)
        {
            _resizerState.SetLength(i, widths[i]);
        }
    }
}

#endregion