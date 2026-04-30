using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Selecting;

#region AssetHolder

/// <summary>
/// A holder for managing asset selections with event support.
/// </summary>
/// <typeparam name="T">The type of asset.</typeparam>
public class AssetHolder<T> : IHasId, IHasAsset
where T : class
{
    IAssetFilter _filter;

    /// <summary>
    /// Gets or sets the asset filter.
    /// </summary>
    public IAssetFilter Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            _selection?.Filter = _filter;
        }
    }

    private AssetSelection<T> _selection = new();

    /// <summary>
    /// Event raised when the selection changes.
    /// </summary>
    public event EventHandler SelectionChanged;

    /// <summary>
    /// Event raised when the target asset is updated.
    /// </summary>
    public event EditorObjectEventHandler<EntryEventArgs> TargetUpdated;

    /// <summary>
    /// Initializes a new instance of the AssetHolder class.
    /// </summary>
    public AssetHolder()
    {
        _selection.TargetUpdated += _selection_ObjectUpdated;
    }

    /// <summary>
    /// Initializes a new instance of the AssetHolder class with the specified target.
    /// </summary>
    /// <param name="target">The target asset.</param>
    public AssetHolder(T target)
        : this()
    {
        _selection.Target = target;
    }

    /// <summary>
    /// Gets or sets the asset selection.
    /// </summary>
    public AssetSelection<T> Selection
    {
        get => _selection;
        set
        {
            if (ReferenceEquals(_selection, value))
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            bool listen = _selection.ListenEnabled;
            _selection.TargetUpdated -= _selection_ObjectUpdated;
            _selection.ListenEnabled = false;

            _selection = value;
            _selection.Filter = _filter;
            _selection.TargetUpdated += _selection_ObjectUpdated;
            _selection.ListenEnabled = listen;

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the target asset.
    /// </summary>
    public T Target
    {
        get => _selection.Target;
        set => _selection.Target = value;
    }

    /// <inheritdoc />
    public Asset TargetAsset
    {
        get => _selection.TargetAsset;
        set => _selection.TargetAsset = value;
    }

    /// <inheritdoc />
    public Guid Id
    {
        get => _selection.Id;
        set => _selection.Id = value;
    }

    /// <inheritdoc />
    public bool ListenEnabled
    {
        get => _selection.ListenEnabled;
        set => _selection.ListenEnabled = value;
    }

    private void _selection_ObjectUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        TargetUpdated?.Invoke(sender, e, ref handled);
    }
}

#endregion

#region AssetProperty

/// <summary>
/// A property wrapper for asset selections that provides sync and inspector support.
/// </summary>
/// <typeparam name="T">The type of asset.</typeparam>
public class AssetProperty<T> : AssetHolder<T>, IValueProperty
    where T : class
{
    /// <summary>
    /// Gets the view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the sync flags.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.NotNull;


    /// <summary>
    /// Initializes a new instance of the AssetProperty class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The property description.</param>
    /// <param name="toolTips">The tooltip text.</param>
    /// <param name="filter">The asset filter.</param>
    public AssetProperty(string name, string description = null, string toolTips = null, IAssetFilter filter = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);
        Property.WithWriteBack();

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }

        Filter = filter;
        Selection.Filter = filter;
    }

    /// <summary>
    /// Initializes a new instance of the AssetProperty class with the specified view property.
    /// </summary>
    /// <param name="property">The view property.</param>
    public AssetProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>
    /// Synchronizes the property value.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <returns>The target asset.</returns>
    public virtual T Sync(IPropertySync sync)
    {
        if (Flag.HasFlag(SyncFlag.GetOnly) || sync.IsGetter())
        {
            sync.Sync(Property.Name, Selection, Flag);
        }
        else
        {
            Selection = sync.Sync(Property.Name, Selection, Flag) ?? new();
        }

        return Target;
    }

    /// <summary>
    /// Configures the inspector field.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="config">The property configuration action.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Selection, prop);
    }
}

#endregion

#region AssetListProperty

/// <summary>
/// A property wrapper for a list of asset selections.
/// </summary>
/// <typeparam name="T">The type of asset.</typeparam>
public class AssetListProperty<T> : IValueProperty
    where T : class
{
    private List<AssetSelection<T>> _list = [];

    /// <summary>
    /// Gets the view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the sync flags.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    /// <summary>
    /// Gets the list of asset selections.
    /// </summary>
    public List<AssetSelection<T>> List => _list;

    /// <summary>
    /// Gets or sets the asset filter.
    /// </summary>
    public IAssetFilter Filter { get; set; }

    /// <summary>
    /// Gets all target assets.
    /// </summary>
    public IEnumerable<T> Targets => _list.Select(o => o.Target);

    /// <summary>
    /// Initializes a new instance of the AssetListProperty class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The property description.</param>
    /// <param name="toolTips">The tooltip text.</param>
    /// <param name="filter">The asset filter.</param>
    public AssetListProperty(string name, string description = null, string toolTips = null, IAssetFilter filter = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);
        Property.WithWriteBack();

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }

        Filter = filter;
    }

    /// <summary>
    /// Initializes a new instance of the AssetListProperty class with the specified view property.
    /// </summary>
    /// <param name="property">The view property.</param>
    public AssetListProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>
    /// Synchronizes the property value.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    public virtual void Sync(IPropertySync sync)
    {
        if (sync.IsNameOf(Property.Name))
        {
            sync.Sync(Property.Name, _list, Flag);

            // Update Filter
            if (sync.IsSetter() && Filter is { } filter)
            {
                foreach (var selection in _list)
                {
                    selection.Filter = filter;
                }
            }
        }
    }

    /// <summary>
    /// Configures the inspector field.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="config">The property configuration action.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(_list, prop);
    }
}

#endregion
