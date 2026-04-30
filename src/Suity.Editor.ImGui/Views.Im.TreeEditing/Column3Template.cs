namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// A column template that provides three predefined columns: Name, Description, and Preview.
/// </summary>
/// <typeparam name="T">The type of data represented by each row in the tree view.</typeparam>
public class Column3Template<T> : ColumnTemplate<T>
    where T : class
{
    /// <summary>
    /// The default width in pixels for the Name column.
    /// </summary>
    public const int DefaultNameColumnWidth = 200;

    /// <summary>
    /// The default width in pixels for the Description column.
    /// </summary>
    public const int DefaultDescriptionColumnWidth = 100;

    /// <summary>
    /// The default width in pixels for the Preview column.
    /// </summary>
    public const int DefaultPreviewColumnWidth = 200;

    /// <summary>
    /// The minimum allowed width in pixels for any column.
    /// </summary>
    public const int ColumnMinLength = 40;

    /// <summary>
    /// Initializes a new instance of <see cref="Column3Template{T}"/> with default column widths and configurations.
    /// </summary>
    public Column3Template()
        : base()
    {
        ResizerState.SetLengths(
            DefaultNameColumnWidth, 
            DefaultDescriptionColumnWidth, 
            DefaultPreviewColumnWidth);

        ColumnConfigs[0] = NameColumn;
        ColumnConfigs[1] = DescriptionColumn;
        ColumnConfigs[2] = PreviewColumn;
    }

    /// <summary>
    /// Gets the configuration for the Name column, which is enabled by default.
    /// </summary>
    public ColumnConfig<T> NameColumn { get; } = new ColumnConfig<T> { Enabled = true, Title = "Name" };

    /// <summary>
    /// Gets the configuration for the Description column, which is disabled by default.
    /// </summary>
    public ColumnConfig<T> DescriptionColumn { get; } = new ColumnConfig<T> { Enabled = false, Title = "Description" };

    /// <summary>
    /// Gets the configuration for the Preview column, which is enabled by default.
    /// </summary>
    public ColumnConfig<T> PreviewColumn { get; } = new ColumnConfig<T> { Enabled = true, Title = "Preview" };

    /// <summary>
    /// Gets or sets the width of the Name column. The value is clamped to <see cref="ColumnMinLength"/> as a minimum.
    /// </summary>
    public float NameColumnWidth
    {
        get => ResizerState.GetLength(0, ResizerMin ?? ColumnMinLength);
        set => ResizerState.SetLength(0, System.Math.Max(value, ColumnMinLength));
    }

    /// <summary>
    /// Gets or sets the width of the Description column. The value is clamped to <see cref="ColumnMinLength"/> as a minimum.
    /// </summary>
    public float DescriptionColumnWidth
    {
        get => ResizerState.GetLength(1, ResizerMin ?? ColumnMinLength);
        set => ResizerState.SetLength(1, System.Math.Max(value, ColumnMinLength));
    }

    /// <summary>
    /// Gets or sets the width of the Preview column. The value is clamped to <see cref="ColumnMinLength"/> as a minimum.
    /// </summary>
    public float PreviewColumnWidth
    {
        get => ResizerState.GetLength(2, ResizerMin ?? ColumnMinLength);
        set => ResizerState.SetLength(2, System.Math.Max(value, ColumnMinLength));
    }
}