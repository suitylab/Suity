using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Values;

#region SKeyProperty

/// <summary>
/// A property that holds an SKey reference to an SObjectController.
/// </summary>
/// <typeparam name="T">The type of SObjectController.</typeparam>
public class SKeyProperty<T> : IValueProperty
    where T : SObjectController
{
    /// <summary>
    /// Gets the view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the sync flag.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    /// <summary>
    /// Gets the type definition.
    /// </summary>
    public TypeDefinition Type { get; }

    /// <summary>
    /// Gets the key.
    /// </summary>
    public SKey Key { get; private set; }

    /// <summary>
    /// Gets the target controller.
    /// </summary>
    public T Target => (Key.TargetAsset as IDataAsset)?.GetComponent<T>();

    /// <summary>
    /// Creates a SKeyProperty with the specified name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The description.</param>
    /// <param name="toolTips">The tooltips.</param>
    public SKeyProperty(string name, string description = null, string toolTips = null)
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

        Type = TypeDefinition.FromNative<T>();
        if (TypeDefinition.IsNullOrEmpty(Type))
        {
            Logs.LogError("Cannot resolve TypeDefinition from native type: " + typeof(T).FullName);
        }

        Key = new SKey(Type);
    }

    /// <summary>
    /// Creates a SKeyProperty with the specified view property.
    /// </summary>
    /// <param name="property">The view property.</param>
    public SKeyProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Property.WithWriteBack();

        Type = TypeDefinition.FromNative<T>();
        if (TypeDefinition.IsNullOrEmpty(Type))
        {
            Logs.LogError("Cannot resolve TypeDefinition from native type: " + typeof(T).FullName);
        }

        Key = new SKey(Type);
    }

    /// <summary>
    /// Synchronizes the property value.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    public virtual void Sync(IPropertySync sync)
    {
        Key = sync.Sync(Property.Name, Key) ?? new SKey(Type);

        if (sync.IsSetter())
        {
            if (Key.InputType != Type)
            {
                Key.InputType = Type;
            }
        }
    }

    /// <summary>
    /// Creates an inspector field for this property.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="config">The property configuration.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Key, prop);
    }
}

#endregion

#region SKeyArrayProperty

/// <summary>
/// An array property that holds multiple SKey references to SObjectControllers.
/// </summary>
/// <typeparam name="T">The type of SObjectController.</typeparam>
public class SKeyArrayProperty<T> : IValueProperty
    where T : SObjectController
{
    /// <summary>
    /// Gets the view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the sync flag.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    /// <summary>
    /// Gets the data type.
    /// </summary>
    public TypeDefinition DataType { get; }

    /// <summary>
    /// Gets the data array type.
    /// </summary>
    public TypeDefinition DataArrayType { get; }

    /// <summary>
    /// Gets the array.
    /// </summary>
    public SArray Array { get; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IEnumerable<T> Items
        => Array.Items
        .OfType<SKey>()
        .Select(o => o.GetComponent<T>())
        .OfType<T>();

    /// <summary>
    /// Creates a SKeyArrayProperty with the specified name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The description.</param>
    /// <param name="toolTips">The tooltips.</param>
    public SKeyArrayProperty(string name, string description = null, string toolTips = null)
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

        DataType = TypeDefinition.FromNative<T>().MakeDataLinkType();
        DataArrayType = DataType.MakeArrayType();
        Array = DataArrayType.CreateArray();
    }

    public SKeyArrayProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Property.WithWriteBack();

        DataType = TypeDefinition.FromNative<T>().MakeDataLinkType();
        DataArrayType = DataType.MakeArrayType();
        Array = DataArrayType.CreateArray();
    }

    /// <summary>
    /// Synchronizes the property value.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    public virtual void Sync(IPropertySync sync)
    {
        sync.Sync(Property.Name, Array, Flag | SyncFlag.GetOnly);
    }

    /// <summary>
    /// Creates an inspector field for this property.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="config">The property configuration.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Array, prop);
    }
}

#endregion

#region SAssetKeyArrayProperty

/// <summary>
/// An array property that holds multiple SAssetKey references.
/// </summary>
/// <typeparam name="T">The type of asset.</typeparam>
public class SAssetKeyArrayProperty<T> : IValueProperty
    where T : class
{
    /// <summary>
    /// Gets the view property.
    /// </summary>
    public ViewProperty Property { get; }

    /// <summary>
    /// Gets or sets the sync flag.
    /// </summary>
    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    /// <summary>
    /// Gets the type definition.
    /// </summary>
    public TypeDefinition Type { get; }

    /// <summary>
    /// Gets the array type.
    /// </summary>
    public TypeDefinition ArrayType { get; }

    /// <summary>
    /// Gets the array.
    /// </summary>
    public SArray Array { get; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IEnumerable<T> Items
        => Array.Items
        .OfType<SAssetKey>()
        .Select(o => o.TargetAsset as T)
        .OfType<T>();

    /// <summary>
    /// Creates a SAssetKeyArrayProperty with the specified name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The description.</param>
    /// <param name="toolTips">The tooltips.</param>
    public SAssetKeyArrayProperty(string name, string description = null, string toolTips = null)
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

        var assetLink = AssetManager.Instance.GetAssetLink<T>();
        Type = assetLink.Definition.MakeAssetLinkType();
        ArrayType = Type.MakeArrayType();
        Array = ArrayType.CreateArray();
    }

    public SAssetKeyArrayProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Property.WithWriteBack();

        var assetLink = AssetManager.Instance.GetAssetLink<T>();
        Type = assetLink.Definition.MakeAssetLinkType();
        ArrayType = Type.MakeArrayType();
        Array = ArrayType.CreateArray();
    }

    /// <summary>
    /// Synchronizes the property value.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    public virtual void Sync(IPropertySync sync)
    {
        sync.Sync(Property.Name, Array, Flag | SyncFlag.GetOnly);
    }

    /// <summary>
    /// Creates an inspector field for this property.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    /// <param name="config">The property configuration.</param>
    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Array, prop);
    }
}

#endregion
