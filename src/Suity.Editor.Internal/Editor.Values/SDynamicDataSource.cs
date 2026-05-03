using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Values;

/// <summary>
/// Represents a dynamic value sourced from an external data source,
/// resolving values by row name and field name at runtime.
/// </summary>
[DisplayText("DataSource")]
public class SDynamicDataSource : SDynamic
{
    private AssetSelection<IDataSource> _dataRef = new();
    private IDataSource _dataSource;
    private object _value;

    private string _fieldName = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="SDynamicDataSource"/>.
    /// </summary>
    public SDynamicDataSource()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public SDynamicDataSource(object value)
        : base(value)
    {
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Data;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _dataRef = sync.Sync("DataSource", _dataRef, SyncFlag.NotNull);
        _fieldName = sync.Sync("FieldName", _fieldName, SyncFlag.NotNull);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_dataRef, new ViewProperty("DataSource", "Data Source"));
        setup.InspectorField(_fieldName, new ViewProperty("FieldName", "Field"));
    }

    /// <inheritdoc/>
    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();

        // Clear cache after type change
        _dataSource = null;
        _value = null;
    }

    /// <inheritdoc/>
    public override object GetValue(ICondition condition = null)
    {
        if (_dataSource?.DataAvailable == true)
        {
            return _value;
        }

        _dataSource = GetDataSource();
        string rowName = ((Root as SContainer)?.Context as IMember)?.Name;
        string columnName = GetFieldName();

        if (_dataSource == null || !_dataSource.DataAvailable || string.IsNullOrEmpty(rowName) || string.IsNullOrEmpty(columnName))
        {
            return base.GetValue(condition);
        }

        _value = _dataSource.GetData(rowName, columnName);
        if (InputType != null && !InputType.SupportValue(_value, false))
        {
            _value = InputType.CreateOrRepairValue(_value, false);
        }

        return _value;
    }

    /// <summary>
    /// Gets the data source from the reference or the root context.
    /// </summary>
    /// <returns>The resolved data source, or null if not available.</returns>
    private IDataSource GetDataSource()
    {
        var ds = _dataRef.GetTarget();

        if (ds != null)
        {
            return ds;
        }

        var dsc = (this.RootContext as IDataItem)?.DataContainer as IDataSourceContext;

        return dsc?.DataSource;
    }

    /// <summary>
    /// Gets the field name to use for data lookup.
    /// </summary>
    /// <returns>The field name, or the item's path if not explicitly set.</returns>
    private string GetFieldName()
    {
        if (!string.IsNullOrEmpty(_fieldName))
        {
            return _fieldName;
        }

        return GetPath();
    }
}
