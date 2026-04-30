using Suity.Editor;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Selecting;

#region SelectionProperty

/// <summary>
/// A property that represents a single selection from a selection list, with synchronization support.
/// </summary>
public class SelectionProperty : IValueProperty
{
    private ISelectionList _selectionList = EmptySelectionList.Empty;

    private Selection _selection = new();

    /// <summary>
    /// Gets the underlying view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the synchronization flag for this property.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.NotNull;

    /// <summary>
    /// Gets or sets the selection list for this property.
    /// </summary>
    public ISelectionList SelectionList
    {
        get => _selectionList;
        set 
        {
            _selection ??= new();
            _selection.List = _selectionList = value;
        }
    }

    /// <summary>
    /// Gets or sets the key of the currently selected item.
    /// </summary>
    public string SelectedKey
    {
        get => _selection.SelectedKey;
        set => _selection.SelectedKey = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionProperty"/>.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="description">Optional description of the property.</param>
    /// <param name="toolTips">Optional tooltips for the property.</param>
    public SelectionProperty(string name, string description = null, string toolTips = null)
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
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionProperty"/> with an existing view property.
    /// </summary>
    /// <param name="property">The view property to use.</param>
    public SelectionProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>
    /// Synchronizes the selection value with the specified property sync context.
    /// </summary>
    public virtual string Sync(IPropertySync sync)
    {
        if (Flag.HasFlag(SyncFlag.GetOnly) || sync.IsGetter())
        {
            sync.Sync(Property.Name, _selection, Flag);
        }
        else
        {
            _selection = sync.Sync(Property.Name, _selection, Flag) ?? new Selection();
        }

        _selection.List = _selectionList;

        return _selection.SelectedKey;
    }

    /// <summary>
    /// Renders this property as an inspector field in the UI.
    /// </summary>
    /// <param name="setup">The view object setup context.</param>
    /// <param name="config">Optional configuration action for the view property.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(_selection, prop);
    }
}


#endregion

#region SelectionListProperty

/// <summary>
/// A property that represents a list of selections. Marked as <see cref="NotAvailableAttribute"/>.
/// </summary>
[NotAvailable]
public class SelectionListProperty : IValueProperty
{
    private List<Selection> _list = [];

    /// <summary>
    /// Gets the underlying view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the synchronization flag for this property.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    /// <summary>
    /// Gets the list of selections.
    /// </summary>
    public List<Selection> List => _list;

    /// <summary>
    /// Gets or sets the asset filter for this property.
    /// </summary>
    public IAssetFilter Filter { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionListProperty"/>.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="description">Optional description of the property.</param>
    /// <param name="toolTips">Optional tooltips for the property.</param>
    public SelectionListProperty(string name, string description = null, string toolTips = null)
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
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionListProperty"/> with an existing view property.
    /// </summary>
    /// <param name="property">The view property to use.</param>
    public SelectionListProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>
    /// Synchronizes the selection list with the specified property sync context.
    /// </summary>
    public virtual void Sync(IPropertySync sync)
    {
        if (sync.IsNameOf(Property.Name))
        {
            sync.Sync(Property.Name, _list, Flag);
        }
    }

    /// <summary>
    /// Renders this property as an inspector field in the UI.
    /// </summary>
    /// <param name="setup">The view object setup context.</param>
    /// <param name="config">Optional configuration action for the view property.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(_list, prop);
    }
}

#endregion