using Suity.Drawing;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Represents a field object within the editor hierarchy, serving as a base class for editor-managed fields.
/// Implements ISelectionItem for selection handling, IViewObject for view synchronization, and ITextDisplay for text rendering.
/// </summary>
public abstract class FieldObject : EditorObject,
    ISelectionItem,
    IViewObject,
    ITextDisplay
{
    internal EditorObject _parent;
    internal string _name;
    internal Guid _recordedId;

    /// <summary>
    /// Initializes a new instance of the FieldObject class.
    /// </summary>
    protected FieldObject()
    {
    }

    /// <summary>
    /// Gets the parent EditorObject of this field.
    /// </summary>
    public override EditorObject Parent => _parent;

    /// <summary>
    /// Retrieves the owner's storage object from the parent hierarchy.
    /// </summary>
    /// <param name="tryLoadStorage">If true, attempts to load the storage object if not already loaded.</param>
    /// <returns>The storage object from the parent hierarchy, or null if not found.</returns>
    protected object GetParentOwner(bool tryLoadStorage) => _parent?.GetStorageObject(tryLoadStorage);

    /// <summary>
    /// Gets the name of this field.
    /// </summary>
    /// <returns>The name of the field.</returns>
    protected override string GetName() => _name;

    /// <summary>
    /// Gets the full qualified name of this field, including the parent hierarchy.
    /// </summary>
    public override string FullName => _parent != null ? $"{_parent.FullName}.{_name}" : _name;

    /// <summary>
    /// Gets the recorded identifier for this field.
    /// </summary>
    /// <returns>The recorded GUID identifier.</returns>
    internal override Guid OnGetRecordedId() => _recordedId;

    /// <summary>
    /// Gets the icon associated with this field.
    /// </summary>
    /// <returns>The Image icon for this field.</returns>
    public virtual ImageDef GetIcon() => CoreIconCache.Field;

    #region ISelectionItem

    /// <summary>
    /// Gets the selection key used for identifying this field in selection operations.
    /// </summary>
    public virtual string SelectionKey => FullName;

    /// <summary>
    /// Gets the display text for this field.
    /// </summary>
    public override string DisplayText => _parent != null ? $"{_parent.Name}.{_name}" : _name;

    /// <summary>
    /// Gets the display icon for this field.
    /// </summary>
    public virtual object DisplayIcon => GetIcon()?.ToIconSmall();

    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    #region IViewObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        try
        {
            OnSync(sync, context);
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        sync.Sync(nameof(Id), Id, SyncFlag.GetOnly);
    }

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        try
        {
            OnSetupView(setup);
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        setup.InspectorField(Id, new ViewProperty(nameof(Id)) { ReadOnly = true });
    }

    /// <summary>
    /// Called when synchronizing this field with a property sync operation.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    /// <summary>
    /// Called when setting up the view for this field.
    /// </summary>
    /// <param name="setup">The view object setup object.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    #endregion
}

/// <summary>
/// Represents a generic collection of FieldObject instances.
/// Provides abstract methods for managing field objects by name or GUID.
/// </summary>
/// <typeparam name="T">The type of FieldObject contained in the collection.</typeparam>
public abstract class FieldObjectCollection<T>
    where T : FieldObject, new()
{
    /// <summary>
    /// Gets a field by its name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The field object if found, otherwise null.</returns>
    public abstract T GetField(string name);

    /// <summary>
    /// Gets a field by its GUID identifier.
    /// </summary>
    /// <param name="id">The GUID identifier of the field.</param>
    /// <returns>The field object if found, otherwise null.</returns>
    public abstract T GetField(Guid id);

    /// <summary>
    /// Gets an enumerable of all fields in the collection.
    /// </summary>
    public abstract IEnumerable<T> Fields { get; }

    /// <summary>
    /// Gets the number of fields in the collection.
    /// </summary>
    public abstract int FieldCount { get; }

    /// <summary>
    /// Gets an existing field by name, or adds a new field if it does not exist.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="resolveType">The ID resolution type (defaults to Auto).</param>
    /// <param name="recoredId">Optional recorded GUID to assign to the new field.</param>
    /// <returns>The existing or newly created field object.</returns>
    public abstract T GetOrAddField(string name, IdResolveType resolveType = IdResolveType.Auto, Guid? recoredId = null);

    /// <summary>
    /// Removes a field from the collection by name.
    /// </summary>
    /// <param name="name">The name of the field to remove.</param>
    /// <returns>True if the field was removed; otherwise, false.</returns>
    public abstract bool RemoveField(string name);

    /// <summary>
    /// Renames an existing field.
    /// </summary>
    /// <param name="oldName">The current name of the field.</param>
    /// <param name="newName">The new name for the field.</param>
    /// <returns>The renamed field object, or null if the field was not found.</returns>
    public abstract T RenameField(string oldName, string newName);

    /// <summary>
    /// Clears all fields from the collection.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Resolves the identifiers for all fields in the collection.
    /// </summary>
    /// <param name="resolveType">The ID resolution type to apply.</param>
    public abstract void ResolveFieldsId(IdResolveType resolveType);

    /// <summary>
    /// Sorts the fields in the collection using the specified comparison.
    /// </summary>
    /// <param name="comparison">The comparison delegate to use for sorting.</param>
    public abstract void Sort(Comparison<T> comparison);
}