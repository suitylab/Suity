using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Base class for asset update actions that implement <see cref="IValueUpdateAction"/>.
/// </summary>
public abstract class AssetUpdateAction : IValueUpdateAction
{
    /// <summary>
    /// Initializes a new instance of <see cref="AssetUpdateAction"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the update action. Cannot be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public AssetUpdateAction(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(L($"\"{nameof(name)}\" cannot be null or whitespace."), nameof(name));
        }

        Name = name;
    }

    /// <summary>
    /// Gets the name of this update action.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public abstract void Update();
}

/// <summary>
/// Generic base class for asset update actions that operate on a specific asset type.
/// Implements <see cref="IAssetUpdateAction{TAsset}"/>.
/// </summary>
/// <typeparam name="TAsset">The type of asset this action operates on.</typeparam>
public abstract class AssetUpdateAction<TAsset> : AssetUpdateAction
    , IAssetUpdateAction<TAsset>
    where TAsset : Asset, new()
{
    private readonly AssetBuilder<TAsset> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="AssetUpdateAction{TAsset}"/>.
    /// </summary>
    /// <param name="builder">The asset builder associated with this action.</param>
    /// <param name="name">The name of the update action.</param>
    internal AssetUpdateAction(AssetBuilder<TAsset> builder, string name)
        : base(name)
    {
        _builder = builder;
    }

    /// <summary>
    /// Gets the asset builder associated with this update action.
    /// </summary>
    public AssetBuilder<TAsset> Builder => _builder;

    /// <inheritdoc/>
    public override void Update()
    {
        _builder.UpdateAuto(Name);
    }

    /// <summary>
    /// Executes the update action on the specified asset.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    public abstract void DoAction(TAsset asset);
}

/// <summary>
/// A simple asset update action that executes a delegate action on the target asset.
/// </summary>
/// <typeparam name="TAsset">The type of asset this action operates on.</typeparam>
public sealed class AssetSimpleUpdateAction<TAsset> : AssetUpdateAction<TAsset>
    where TAsset : Asset, new()
{
    private readonly Action<TAsset> _updateAction;

    /// <summary>
    /// Initializes a new instance of <see cref="AssetSimpleUpdateAction{TAsset}"/>.
    /// </summary>
    /// <param name="builder">The asset builder associated with this action.</param>
    /// <param name="name">The name of the update action.</param>
    /// <param name="updateAction">The action to execute on the asset.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updateAction"/> is null.</exception>
    internal AssetSimpleUpdateAction(AssetBuilder<TAsset> builder, string name, Action<TAsset> updateAction)
        : base(builder, name)
    {
        _updateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
    }

    /// <inheritdoc/>
    public override void DoAction(TAsset asset)
    {
        _updateAction(asset);
    }
}

/// <summary>
/// An asset update action that caches and applies a value to the target asset.
/// Implements <see cref="IValueUpdateAction{TValue}"/>.
/// </summary>
/// <typeparam name="TAsset">The type of asset this action operates on.</typeparam>
/// <typeparam name="TValue">The type of value to update on the asset.</typeparam>
public sealed class AssetValueUpdateAction<TAsset, TValue> : AssetUpdateAction<TAsset>
    , IValueUpdateAction<TValue>
    where TAsset : Asset, new()
{
    private readonly Action<TAsset, TValue> _updateAction;

    private TValue _value;

    /// <summary>
    /// Initializes a new instance of <see cref="AssetValueUpdateAction{TAsset, TValue}"/>.
    /// </summary>
    /// <param name="builder">The asset builder associated with this action.</param>
    /// <param name="name">The name of the update action.</param>
    /// <param name="updateAction">The action to execute with the asset and value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updateAction"/> is null.</exception>
    internal AssetValueUpdateAction(AssetBuilder<TAsset> builder, string name, Action<TAsset, TValue> updateAction)
        : base(builder, name)
    {
        _updateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
    }

    /// <summary>
    /// Gets the currently cached value.
    /// </summary>
    public TValue CachedValue => _value;

    /// <summary>
    /// Updates the cached value and triggers the update action.
    /// </summary>
    /// <param name="value">The new value to cache and apply.</param>
    public void UpdateValue(TValue value)
    {
        _value = value;
        Update();
    }

    /// <inheritdoc/>
    public override void DoAction(TAsset asset)
    {
        _updateAction(asset, _value);
    }
}

/// <summary>
/// An asset update action that manages a reference to another asset by ID.
/// Implements <see cref="IRefUpdateAction{TValue}"/>.
/// </summary>
/// <typeparam name="TAsset">The type of asset this action operates on.</typeparam>
/// <typeparam name="TValue">The type of referenced value (must be a class).</typeparam>
public sealed class AssetRefUpdateAction<TAsset, TValue> : AssetUpdateAction<TAsset>
    , IRefUpdateAction<TValue>
    where TAsset : Asset, new()
    where TValue : class
{
    private readonly Action<TAsset, Guid> _updateAction;

    private readonly EditorAssetRef<TValue> _ref = new();

    /// <summary>
    /// Initializes a new instance of <see cref="AssetRefUpdateAction{TAsset, TValue}"/>.
    /// </summary>
    /// <param name="builder">The asset builder associated with this action.</param>
    /// <param name="name">The name of the update action.</param>
    /// <param name="updateAction">The action to execute with the asset and reference ID.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updateAction"/> is null.</exception>
    internal AssetRefUpdateAction(AssetBuilder<TAsset> builder, string name, Action<TAsset, Guid> updateAction)
        : base(builder, name)
    {
        _updateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
    }

    /// <summary>
    /// Gets the currently cached referenced value.
    /// </summary>
    public TValue CachedValue => _ref.Target;
    /// <summary>
    /// Gets the currently cached reference ID.
    /// </summary>
    public Guid CachedId => _ref.Id;

    /// <summary>
    /// Updates the reference by target value and triggers the update action.
    /// </summary>
    /// <param name="value">The new referenced value.</param>
    public void UpdateValue(TValue value)
    {
        _ref.Target = value;
        Update();
    }

    /// <summary>
    /// Updates the reference by ID and triggers the update action.
    /// </summary>
    /// <param name="id">The new reference ID.</param>
    public void UpdateId(Guid id)
    {
        _ref.Id = id;
        Update();
    }

    /// <inheritdoc/>
    public override void DoAction(TAsset asset)
    {
        _updateAction(asset, _ref.Id);
    }
}

/// <summary>
/// Collects and manages field objects for a specific asset type, supporting add, update, remove,
/// and rename operations with deferred application to the asset.
/// </summary>
/// <typeparam name="TAsset">The type of asset this collector operates on.</typeparam>
/// <typeparam name="TField">The type of field objects being collected.</typeparam>
internal class AssetFieldCollector<TAsset, TField> : IAssetFieldCollector<TField>
        where TAsset : Asset, new()
        where TField : FieldObject, new()
{
    private readonly string _name;
    private readonly AssetBuilder<TAsset> _builder;
    private readonly Func<TAsset, FieldObjectCollection<TField>> _getCollection;

    private readonly FieldAddOrUpdateAction<TAsset, TField> _addOrUpdateAction;
    private readonly FieldUpdateAction<TAsset, TField> _updateAction;

    // Temporary objects are saved here, they will be recreated when actually used
    private readonly Dictionary<string, TField> _tempFields = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetFieldCollector{TAsset, TField}"/> class.
    /// </summary>
    /// <param name="name">The name identifier for this collector.</param>
    /// <param name="builder">The asset builder used to update the underlying asset.</param>
    /// <param name="getCollection">A function to retrieve the field collection from an asset.</param>
    /// <param name="addOrUpdateAction">The action invoked when a field is added or updated.</param>
    /// <param name="updateDisplayAction">The action invoked to update field display properties.</param>
    public AssetFieldCollector(
        string name,
        AssetBuilder<TAsset> builder,
        Func<TAsset, FieldObjectCollection<TField>> getCollection,
        FieldAddOrUpdateAction<TAsset, TField> addOrUpdateAction,
        FieldUpdateAction<TAsset, TField> updateDisplayAction
        )
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _getCollection = getCollection ?? throw new ArgumentNullException(nameof(getCollection));
        _addOrUpdateAction = addOrUpdateAction;
        _updateAction = updateDisplayAction;
    }

    /// <inheritdoc/>
    public void AddOrUpdatedField(string name, Action<TField> addOrUpdateAction, IdResolveType resolveType = IdResolveType.Auto, Guid? recorededId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var field = _tempFields.GetOrAdd(name, _ =>
        {
            var newField = new TField
            {
                _name = name
            };
            return newField;
        });

        // Critical bug fix: ensure field's name is consistent
        field._name = name;
        if (recorededId.HasValue)
        {
            field._recordedId = recorededId.Value;
        }

        addOrUpdateAction(field);

        _builder.TryUpdateNow(asset =>
        {
            _getCollection(asset).GetOrAddField(name, resolveType, recorededId);
            _addOrUpdateAction?.Invoke(asset, field, resolveType);
        });
    }

    /// <inheritdoc/>
    public bool UpdateField(string name, Action<TField> updateAction, Guid? recorededId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var field = _tempFields.GetValueSafe(name);

        if (field != null)
        {
            // Critical bug fix: ensure field's name is consistent
            field._name = name;
            if (recorededId.HasValue)
            {
                field._recordedId = recorededId.Value;
            }

            updateAction(field);
            _builder.TryUpdateNow(asset => _updateAction?.Invoke(asset, field));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void RemoveField(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (_tempFields.Remove(name))
        {
            _builder.TryUpdateNow(asset => _getCollection(asset).RemoveField(name));
        }
    }

    /// <inheritdoc/>
    public void RenameField(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        var field = _tempFields.RemoveAndGet(oldName);
        if (field != null)
        {
            // Critical bug fix: ensure field's name is consistent
            field._name = oldName;

            //Logs.LogDebug($"AssetFieldCollector rename field : {oldName}->{newName}");

            _tempFields.Add(newName, field);
            _builder.TryUpdateNow(asset => _getCollection(asset).RenameField(oldName, newName));
        }
        else
        {
            //Logs.LogWarning($"AssetFieldCollector rename field FAILED (old name not found) : {oldName}->{newName}");
        }
    }

    /// <summary>
    /// Updates the specified asset by synchronizing its field collection with the temporary fields.
    /// Adds missing fields and removes fields that are no longer tracked.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    /// <exception cref="NullReferenceException">Thrown when the field object collection cannot be retrieved.</exception>
    internal void UpdateAsset(TAsset asset)
    {
        var collection = _getCollection(asset) 
            ?? throw new NullReferenceException("Get FieldObjectCollection failed.");

        var addOrUpdateAction = _addOrUpdateAction;
        var updateDisplayAction = _updateAction;

        foreach (var f in _tempFields.Values)
        {
            collection.GetOrAddField(f.Name, IdResolveType.Auto, f._recordedId);
            addOrUpdateAction?.Invoke(asset, f, IdResolveType.Auto);
            updateDisplayAction?.Invoke(asset, f);
        }

        List<string> removes = null;
        foreach (var field in collection.Fields)
        {
            if (!_tempFields.ContainsKey(field.Name))
            {
                (removes ??= []).Add(field.Name);
            }
        }

        if (removes != null)
        {
            foreach (var remove in removes)
            {
                collection.RemoveField(remove);
            }
        }
    }
}