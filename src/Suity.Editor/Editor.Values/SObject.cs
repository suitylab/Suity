using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Values;

/// <summary>
/// Interface for creating SObject instances.
/// </summary>
public interface ISObjectCreate
{
    /// <summary>
    /// Creates an SObject.
    /// </summary>
    SObject CreateSObject();
}

/// <summary>
/// Represents a data object containing structured data properties.
/// </summary>
[NativeAlias]
[NativeType(Name = "SObject", Description = "Data Object", CodeBase = "*Core", Icon = "*CoreIcon|Value")]
public sealed class SObject : SContainer,
    ISyncObject,
    ISupportAnalysis,
    IEditorObjectListener,
    IViewComment,
    ISyncPathIdObject
{
    internal const string Attribute_ObjectType = "struct";
    internal const string Attribute_Comment = "comment";

    internal readonly SObjectExternal _ex;

    public SObject()
    {
        _ex = SItemExternal._external.CreateSObjectEx(this);
        _ex.Controller?.Start(this);
    }

    public SObject(SObjectController controller)
        : base(TypeDefinition.FromNative(controller.GetType())?.BaseAbstractType ?? TypeDefinition.Empty)
    {
        _ex = SItemExternal._external.CreateSObjectEx(this, controller);
        _ex.Controller?.Start(this);
    }

    public SObject(TypeDefinition inputType, TypeDefinition objType)
        : base(inputType)
    {
        _ex = SItemExternal._external.CreateSObjectEx(this, objType);
        _ex.Controller?.Start(this);
    }

    public SObject(TypeDefinition inputType, TypeDefinition objType, SObjectController controller)
        : this(inputType, objType)
    {
        _ex = SItemExternal._external.CreateSObjectEx(this, objType, controller);
        _ex.Controller?.Start(this);
    }

    public SObject(TypeDefinition objType)
        : base(objType)
    {
        _ex = SItemExternal._external.CreateSObjectEx(this, objType);
        _ex.Controller?.Start(this);
    }

    /// <summary>
    /// Gets or sets the object type definition.
    /// </summary>
    public TypeDefinition ObjectType
    {
        get => _ex.ObjectType;
        set => _ex.ObjectType = value;
    }

    /// <summary>
    /// Gets a value indicating whether the object type is locked.
    /// </summary>
    public bool ObjectTypeLocked => _ex.ObjectTypeLocked;

    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        //After the parent level update, it is necessary to notify the controller to update the status
        _ex.Controller?.UpdateStatus();
    }

    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();

        _ex.Controller?.UpdateStatus();
    }

    public override TypeDefinition TargetType => _ex.ObjectType;

    #region Properties

    /// <summary>
    /// Resolves the property name for a given ID.
    /// </summary>
    /// <param name="id">The property ID.</param>
    /// <returns>The property name, or null if not found.</returns>
    public string ResolvePropertyName(Guid id) => _ex.ResolvePropertyName(id);

    /// <summary>
    /// Gets the field for a given ID.
    /// </summary>
    /// <param name="id">The field ID.</param>
    /// <returns>The struct field, or null if not found.</returns>
    public DStructField GetField(Guid id) => _ex.GetField(id);

    /// <summary>
    /// Gets the field for a given name.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The struct field, or null if not found.</returns>
    public DStructField GetField(string name) => _ex.GetField(name);

    /// <summary>
    /// Gets or sets the property value by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    public object this[string name]
    {
        get => GetPropertyFormatted(name);
        set => SetProperty(name, value);
    }

    /// <summary>
    /// Gets or sets the property value by field.
    /// </summary>
    /// <param name="field">The field object.</param>
    public object this[FieldObject field]
    {
        get => GetPropertyFormatted(field?.Id ?? Guid.Empty);
        set => SetProperty(field?.Id ?? Guid.Empty, value);
    }

    /// <summary>
    /// Gets all property IDs.
    /// </summary>
    public IEnumerable<Guid> GetPropertyIds() => _ex.GetPropertyIds();

    /// <summary>
    /// Gets all stored property IDs.
    /// </summary>
    public IEnumerable<Guid> GetStoredPropertyIds() => _ex.GetStoredPropertyIds();

    /// <summary>
    /// Gets all legacy property IDs.
    /// </summary>
public IEnumerable<Guid> GetLegacyPropertyIds() => _ex.GetLegacyPropertyIds();

    public string[] GetPropertyNames() => _ex.GetPropertyNames();

    public bool ContainsProperty(Guid id) => _ex.ContainsProperty(id);

    /// <summary>
    /// Gets whether the property exists by name.
    /// </summary>
    /// <param name="name">Property name</param>
    /// <returns>Returns whether the property exists</returns>
    public bool ContainsProperty(string name) => _ex.ContainsProperty(name);

    /// <summary>
    /// Gets the formatted property value by ID.
    /// </summary>
    /// <param name="id">The property ID.</param>
    /// <param name="context">The condition context.</param>
    public object GetPropertyFormatted(Guid id, ICondition context = null) => _ex.GetPropertyFormatted(id, context);

    /// <summary>
    /// Ensures getting property, will auto repair and write when property is missing or mismatched
    /// </summary>
    /// <param name="name">Property name</param>
    /// <returns>Returns the property.</returns>
    /// <summary>
    /// Gets the formatted property value by name.
    /// </summary>
    /// <param name="name">Property name</param>
    /// <param name="context">Condition context</param>
    public object GetPropertyFormatted(string name, ICondition context = null) => _ex.GetPropertyFormatted(name, context);

    /// <summary>
    /// Gets the formatted property value by field.
    /// </summary>
    /// <param name="field">Field</param>
    /// <param name="context">Condition context</param>
    public object GetPropertyFormatted(FieldObject field, ICondition context = null) => _ex.GetPropertyFormatted(field.Id, context);

    /// <summary>
    /// Gets the property value by ID.
    /// </summary>
    /// <param name="id">The property ID.</param>
    /// <param name="context">The condition context.</param>
    public object GetProperty(Guid id, ICondition context = null) => _ex.GetProperty(id, context);

    /// <summary>
    /// Gets the property value by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="context">The condition context.</param>
    public object GetProperty(string name, ICondition context = null) => _ex.GetProperty(name, context);

    /// <summary>
    /// Gets the property value by ID with type conversion.
    /// </summary>
    /// <param name="id">The property ID.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <param name="context">The condition context.</param>
    public T GetProperty<T>(Guid id, T defaultValue = default, ICondition context = null) => _ex.GetProperty<T>(id, defaultValue, context);

    /// <summary>
    /// Gets the property value by name with type conversion.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <param name="context">The condition context.</param>
    public T GetProperty<T>(string name, T defaultValue = default, ICondition context = null) => _ex.GetProperty<T>(name, defaultValue, context);

    /// <summary>
    /// Sets property by ID.
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="value">Property value</param>
    public void SetProperty(Guid id, object value) => _ex.SetProperty(id, value);

    /// <summary>
    /// Sets property by name.
    /// </summary>
    /// <param name="name">Property name</param>
    /// <param name="value">Property value</param>
public void SetProperty(string name, object value) =>
        _ex.SetProperty(name, value);

    public void SetProperty(FieldObject field, object value) => _ex.SetProperty(field?.Id ?? Guid.Empty, value);

    public bool RemoveProperty(Guid id) => _ex.RemoveProperty(id);

    public bool RemoveProperty(string name) => _ex.RemoveProperty(name);

    /// <summary>
    /// Removes child item.
    /// </summary>
    /// <param name="item">Item to remove.</param>
    public override void RemoveItem(SItem item) => _ex.RemoveItem(item);


    /// <summary>
    /// Gets the SItem by ID.
    /// </summary>
    /// <param name="id">The item ID.</param>
    public SItem GetItem(Guid id) => _ex.GetItem(id);

    /// <summary>
    /// Gets the SItem by name.
    /// </summary>
    /// <param name="name">The item name.</param>
    public SItem GetItem(string name) => _ex.GetItem(name);

    /// <summary>
    /// Gets the SItem by field.
    /// </summary>
    /// <param name="field">The field object.</param>
    public SItem GetItem(FieldObject field) => _ex.GetItem(field.Id);

    /// <summary>
    /// Gets the formatted SItem by ID.
    /// </summary>
    /// <param name="id">The item ID.</param>
    public SItem GetItemFormatted(Guid id) => _ex.GetItemFormatted(id);

    /// <summary>
    /// Gets the formatted SItem by name.
    /// </summary>
    /// <param name="name">The item name.</param>
    public SItem GetItemFormatted(string name) => _ex.GetItemFormatted(name);

    /// <summary>
    /// Gets the formatted SItem by field.
    /// </summary>
    /// <param name="field">The field object.</param>
    public SItem GetItemFormatted(FieldObject field) => _ex.GetItemFormatted(field.Id);

    /// <summary>
    /// Sets to empty object
    /// </summary>
    /// <summary>
    /// Clears all properties.
    /// </summary>
    public override bool Clear() => _ex.Clear();


    internal void InternalSetProperty(Guid id, object value) => _ex.InternalSetProperty(id, value);

    internal void InternalSetProperty(string name, object value) => _ex.InternalSetProperty(name, value);

    /// <summary>
    /// Gets all values.
    /// </summary>
    /// <param name="context">The condition context.</param>
    public override IEnumerable<object> GetValues(ICondition context = null) => _ex.GetValues(context);

    /// <summary>
    /// Gets all child items.
    /// </summary>
    public override IEnumerable<SItem> Items => _ex.Items;

    /// <summary>
    /// Merges properties to another SObject.
    /// </summary>
    /// <param name="target">The target SObject.</param>
    /// <param name="skipDynamic">Whether to skip dynamic properties.</param>
    public bool MergeTo(SObject target, bool skipDynamic) => _ex.MergeTo(target, skipDynamic);

    #endregion

    #region Attachments

    /// <summary>
    /// Gets whether there are attachments.
    /// </summary>
    public bool HasAttachments => _ex.HasAttachments;

    /// <summary>
    /// Gets an attachment by name.
    /// </summary>
    /// <param name="name">The attachment name.</param>
    public object GetAttachment(string name) => _ex.GetAttachment(name);

    /// <summary>
    /// Sets an attachment.
    /// </summary>
    /// <param name="name">The attachment name.</param>
    /// <param name="value">The attachment value.</param>
    public void SetAttachment(string name, object value) => _ex.SetAttachment(name, value);

    /// <summary>
    /// Clears all attachments.
    /// </summary>
    public void ClearAttachments() => _ex.ClearAttachments();

    /// <summary>
    /// Gets all attachments.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> GetAttachments() => _ex.GetAttachments();

    #endregion

    #region Controller

    /// <summary>
    /// Gets or sets value
    /// </summary>
    public SObjectController Controller
    {
        get => _ex.Controller;
        internal set => _ex.Controller = value;
    }

    #endregion

    #region Judgment

    public override bool ValueEquals(object other) => _ex.ValueEquals(other);

    /// <summary>
    /// Determines if the type is equal to another SItem.
    /// </summary>
    /// <param name="other">The other SItem.</param>
    public override bool TypeEquals(SItem other)
    {
        if (!base.TypeEquals(other))
        {
            return false;
        }

        return _ex.TypeEquals(other);
    }

    public override bool GetIsDefault() => GetIsBlank();

    public override bool GetIsBlank()
    {
        if (TypeDefinition.IsNullOrEmpty(ObjectType))
        {
            return true;
        }

        var s = ObjectType.Target as DCompond;
        if (s != null)
        {
            foreach (var field in s.GetPublicStructFields(true))
            {
                var item = this.GetItemFormatted(field.Id);
                if (item != null && !item.GetIsBlank())
                {
                    return false;
                }
            }
        }
        else
        {
            foreach (var item in this.Items)
            {
                if (item != null && !item.GetIsBlank())
                {
                    return false;
                }
            }
        }

        return true;
    }

    #endregion

    #region ISyncObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context) => _ex.Sync(sync, context);

    #endregion

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    AnalysisResult ISupportAnalysis.Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems.
    /// </summary>
    /// <param name="problems">The analysis problem collection.</param>
    /// <param name="intent">The analysis intent.</param>
    void ISupportAnalysis.CollectProblem(AnalysisProblem problems, AnalysisIntents intent) => _ex.CollectProblem(problems, intent);

    #endregion

    #region IEditorObjectListener

    /// <summary>
    /// Handles object update events.
    /// </summary>
    /// <param name="id">The object ID.</param>
    /// <param name="obj">The editor object.</param>
    /// <param name="args">The entry event arguments.</param>
    /// <param name="handled">Whether the event has been handled.</param>
    void IEditorObjectListener.HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    {
        _ex.Controller?.HandleObjectUpdate(id, obj, args, ref handled);
    }

    #endregion

    #region IViewComment

    /// <summary>
    /// Gets whether comments are supported.
    /// </summary>
    public bool CanComment => true;
    /// <summary>
    /// Gets or sets whether this is a comment.
    /// </summary>
    public bool IsComment { get; set; }

    #endregion

    #region ISyncPathIdObject, IHasId

    /// <summary>
    /// Gets the ID.
    /// </summary>
    Guid ISyncPathIdObject.Id => _ex.ObjectType?.TargetId ?? Guid.Empty;

    /// <summary>
    /// Gets the type ID.
    /// </summary>
    public override Guid TypeId => _ex.ObjectType?.TargetId ?? Guid.Empty;

    #endregion

    #region Repair

    /// <summary>
    /// Repairs IDs in the object.
    /// </summary>
    internal void RepairIds() => _ex.RepairIds();

    #endregion

    /// <summary>
    /// Auto converts the value based on the input type.
    /// </summary>
    public override void AutoConvertValue() => _ex.AutoConvertValue();

    /// <summary>
    /// Synchronizes references.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync object.</param>
    public override void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        base.ReferenceSync(path, sync);

        _ex.ReferenceSync(path, sync);
    }

    /// <summary>
    /// Gets the field for an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public override FieldObject GetField(SItem item) => _ex.GetField(item);

    public override string ToString() => GetBrief();

    /// <summary>
    /// Gets a brief string representation.
    /// </summary>
    /// <param name="depth">The depth for traversal.</param>
    public string GetBrief(int depth = 10) => _ex.GetBrief(depth);

    /// <summary>
    /// Determines if an SObject is null or empty.
    /// </summary>
    /// <param name="obj">The SObject to check.</param>
    public static bool IsNullOrEmpty(SObject obj)
    {
        //When obj.ObjectiType is empty, it also means that obj is empty
        return obj is null || TypeDefinition.IsNullOrEmpty(obj.ObjectType);
    }
}