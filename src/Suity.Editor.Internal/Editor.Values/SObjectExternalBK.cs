using Suity;
using Suity.Collections;
using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Values;

/// <summary>
/// Back-end implementation of <see cref="SObjectExternal"/> that manages object properties,
/// synchronization, attachments, and controller integration.
/// </summary>
internal class SObjectExternalBK : SObjectExternal
{
    private readonly SObject _obj;

    private TypeDefinition _objType = TypeDefinition.Empty;
    private readonly bool _typeLocked;

    private readonly Dictionary<Guid, SItem> _props = [];
    private SObjectController _controller;

    private Dictionary<string, object> _attachments;

    /// <summary>
    /// Initializes a new instance with the specified object.
    /// </summary>
    /// <param name="obj">The parent SObject.</param>
    public SObjectExternalBK(SObject obj)
    {
        _obj = obj;
    }

    /// <summary>
    /// Initializes a new instance with the specified object and controller.
    /// </summary>
    /// <param name="obj">The parent SObject.</param>
    /// <param name="controller">The controller that manages this object's values.</param>
    public SObjectExternalBK(SObject obj, SObjectController controller)
    {
        _obj = obj;
        _objType = TypeDefinition.FromNative(controller.GetType()) ?? TypeDefinition.Empty;
        _controller = controller;
        _typeLocked = true;
    }

    /// <summary>
    /// Initializes a new instance with the specified object and type definition.
    /// </summary>
    /// <param name="obj">The parent SObject.</param>
    /// <param name="objType">The type definition for this object.</param>
    public SObjectExternalBK(SObject obj, TypeDefinition objType)
    {
        _obj = obj;
        _objType = objType ?? TypeDefinition.Empty;
        _typeLocked = true;
        UpdateController(false, false);
    }

    /// <summary>
    /// Initializes a new instance with the specified object, type definition, and controller.
    /// </summary>
    /// <param name="obj">The parent SObject.</param>
    /// <param name="objType">The type definition for this object.</param>
    /// <param name="controller">The controller that manages this object's values.</param>
    public SObjectExternalBK(SObject obj, TypeDefinition objType, SObjectController controller)
    {
        _obj = obj;
        _objType = objType ?? TypeDefinition.Empty;
        _controller = controller;
        _typeLocked = true;
    }

    /// <inheritdoc/>
    public override TypeDefinition ObjectType
    {
        get => _objType;
        set
        {
            if (_typeLocked)
            {
                return;
            }

            value ??= TypeDefinition.Empty;

            if (_objType == value)
            {
                return;
            }

            _objType = value;

            UpdateController(true, true);
        }
    }

    /// <inheritdoc/>
    public override bool ObjectTypeLocked => _typeLocked;

    #region Properties

    /// <inheritdoc/>
    public override string ResolvePropertyName(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        if (_objType.Target?.GetPublicField(id)?.Name is { } name)
        {
            return name;
        }

        if (EditorObjectManager.Instance.GetObject(id) is { } obj)
        {
            return obj.Name;
        }

        if (_objType.Target?.UseNativeFields == true)
        {
            return NativeFieldResolver.Instance.ResolveFieldName(_objType.Target, id);
        }

        return null;
    }

    /// <inheritdoc/>
    public override Guid ResolvePropertyId(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Guid.Empty;
        }

        var type = _objType.Target;
        if (type is null)
        {
            return Guid.Empty;
        }

        if (type.UseNativeFields)
        {
            return NativeFieldResolver.Instance.ResolveFieldId(type, name);
        }
        else
        {
            return type.GetPublicField(name)?.Id ?? Guid.Empty;
        }
    }


    /// <inheritdoc/>
    public override DStructField GetField(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return _objType.Target?.GetPublicField(id) as DStructField;
    }

    /// <inheritdoc/>
    public override DStructField GetField(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _objType.Target?.GetPublicField(name) as DStructField;
    }

    /// <inheritdoc/>
    public override object this[string name]
    {
        get => GetPropertyFormatted(name);
        set => SetProperty(name, value);
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> GetPropertyIds()
    {
        if (_objType.Target is DCompond type)
        {
            return type.PublicStructFields.Select(o => o.Id);
        }
        else
        {
            return _props.Keys;
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> GetStoredPropertyIds()
    {
        return _props.Keys;
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> GetLegacyPropertyIds()
    {
        if (_objType.Target is DCompond type)
        {
            return _props.Keys.Where(id => type.GetPublicStructFieldFromBase(id) is null);
        }
        else
        {
            return _props.Keys;
        }
    }

    /// <inheritdoc/>
    public override string[] GetPropertyNames()
    {
        if (_objType.Target is DCompond type)
        {
            return type.PublicStructFields.Select(o => o.Name).ToArray();
        }
        else
        {
            lock (_props)
            {
                return _props.Keys.Select(ResolvePropertyName).SkipNull().ToArray();
            }
        }
    }

    /// <inheritdoc/>
    public override bool ContainsProperty(Guid id)
    {
        lock (_props)
        {
            return _props.ContainsKey(id);
        }
    }

    /// <inheritdoc/>
    public override bool ContainsProperty(string name)
    {
        if (ObjectType.Target is DCompond type)
        {
            return type.GetPublicStructFieldFromBase(name) != null;
        }

        Guid id = ResolvePropertyId(name);
        lock (_props)
        {
            return _props.ContainsKey(id);
        }
    }

    /// <inheritdoc/>
    public override object GetPropertyFormatted(Guid id, ICondition context = null)
    {
        if (_objType.Target is not DCompond type)
        {
            return null;
        }

        if (type.GetPublicField(id) is not DStructField field)
        {
            return null;
        }

        bool hasValue = _props.TryGetValue(field.Id, out var item);

        if (hasValue)
        {
            // Automatically converts mismatched types
            if (field.FieldType.SupportValue(item, field.Optional))
            {
                return SItem.ResolveValue(item, context);
            }
            else
            {
                object fixedValue = field.CreateOrRepairValue(item);
                // Note: Set operation is built into Get, which will break enumeration operations
                SetProperty(field.Name, fixedValue);

                return SItem.ResolveValue(fixedValue, context);
            }
        }
        else
        {
            object newValue = field.CreateDefaultValue();
            SetProperty(field.Name, newValue);

            return SItem.ResolveValue(newValue, context);
        }
    }

    /// <inheritdoc/>
    public override object GetPropertyFormatted(string name, ICondition context = null)
    {
        if (_objType.Target is not DCompond type)
        {
            return null;
        }

        if (type.UseNativeFields)
        {
            Guid id = GlobalIdResolver.Resolve($"{type.FullName}.{name}");
            return GetProperty(id, context);
        }
        else
        {
            if (type.GetPublicField(name) is not DStructField field)
            {
                return null;
            }

            bool hasValue = _props.TryGetValue(field.Id, out var item);

            if (hasValue)
            {
                // Automatically converts mismatched types
                if (field.FieldType.SupportValue(item, field.Optional))
                {
                    return SItem.ResolveValue(item, context);
                }
                else
                {
                    object fixedValue = field.CreateOrRepairValue(item);
                    // Note: Set operation is built into Get, which will break enumeration operations
                    SetProperty(field.Name, fixedValue);

                    return SItem.ResolveValue(fixedValue, context);
                }
            }
            else
            {
                object newValue = field.CreateDefaultValue();
                SetProperty(field.Name, newValue);

                return SItem.ResolveValue(newValue, context);
            }
        }
    }

    /// <inheritdoc/>
    public override object GetProperty(Guid id, ICondition context = null)
    {
        SItem item;
        lock (_props)
        {
            item = _props.GetValueSafe(id);
        }

        return SItem.ResolveValue(item, context);
    }

    /// <inheritdoc/>
    public override object GetProperty(string name, ICondition context = null)
    {
        if (_objType.Target is not DCompond type)
        {
            return null;
        }

        if (type.UseNativeFields)
        {
            Guid id = GlobalIdResolver.Resolve($"{type.FullName}.{name}");
            return GetProperty(id, context);
        }
        else
        {
            if (type.GetPublicField(name) is DStructField field)
            {
                object item = GetItem(field.Id);

                return SItem.ResolveValue(item, context);
            }
            else
            {
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public override T GetProperty<T>(Guid id, T defaultValue = default, ICondition context = null)
    {
        object value = GetProperty(id, context);

        if (value is T t)
        {
            return t;
        }
        else
        {
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public override T GetProperty<T>(string name, T defaultValue = default, ICondition context = null)
    {
        object value = GetPropertyFormatted(name, context);

        if (value is T t)
        {
            return t;
        }
        else
        {
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public override void SetProperty(Guid id, object value)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        if (_controller != null)
        {
            string name = ResolvePropertyName(id);
            _controller.InternalSetProperty(id, name, SItem.ResolveValue(value));
        }
        else
        {
            InternalSetProperty(id, value);
        }
    }

    /// <inheritdoc/>
    public override void SetProperty(string name, object value)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        Guid id = ResolvePropertyId(name);
        if (id == Guid.Empty)
        {
            return;
        }

        if (_controller != null)
        {
            _controller.InternalSetProperty(id, name, SItem.ResolveValue(value));
        }
        else
        {
            InternalSetProperty(id, value);
        }
    }

    /// <inheritdoc/>
    public override bool RemoveProperty(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        if (_controller != null)
        {
            return false;
        }

        SItem current;
        bool removed;

        lock (_props)
        {
            current = _props.GetValueSafe(id);
            if (current != null)
            {
                _props.Remove(id);
                //if (_controller != null)
                //{
                //    _controller.SetProperty(name, null);
                //}
                removed = true;
            }
            else
            {
                removed = false;
            }
        }

        if (removed)
        {
            current.Parent = null;
        }

        return removed;
    }

    /// <inheritdoc/>
    public override bool RemoveProperty(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (_objType.Target is INativeType)
        {
            return false;
        }

        Guid id = ResolvePropertyId(name);

        return RemoveProperty(id);
    }

    /// <inheritdoc/>
    public override void RemoveItem(SItem item)
    {
        if (item.Parent != _obj)
        {
            return;
        }

        RemoveProperty(item.FieldId);
    }

    /// <inheritdoc/>
    public override bool Clear()
    {
        // Cannot clear when controlled
        if (_controller != null)
        {
            return false;
        }

        SItem[] items;
        lock (_props)
        {
            if (_props.Count == 0)
            {
                return true;
            }

            items = [.. _props.Values];
            _props.Clear();
        }

        foreach (var item in items)
        {
            item.Parent = null;
        }

        return true;
    }

    /// <inheritdoc/>
    public override SItem GetItem(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        if (_props.TryGetValue(id, out var item))
        {
            return item;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override SItem GetItem(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Guid id = ResolvePropertyId(name);

        return GetItem(id);
    }

    /// <inheritdoc/>
    public override SItem GetItemFormatted(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        if (_objType.Target is not DCompond type)
        {
            return null;
        }

        bool hasValue = _props.TryGetValue(id, out var item);
        if (type.UseNativeFields)
        {
            return item;
        }

        if (type.GetPublicField(id) is not DStructField field)
        {
            return null;
        }

        if (hasValue)
        {
            // Automatically converts mismatched types
            if (field.FieldType.SupportValue(item, field.Optional))
            {
                return item;
            }
            else
            {
                object fixedValue = field.CreateOrRepairValue(item);
                // Note: Set operation is built into Get, which will break enumeration operations
                SetProperty(field.Name, fixedValue);

                return _props.GetValueSafe(id);
            }
        }
        else
        {
            object newValue = field.CreateDefaultValue();
            SetProperty(field.Name, newValue);

            return _props.GetValueSafe(id);
        }
    }

    /// <inheritdoc/>
    public override SItem GetItemFormatted(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Guid id = ResolvePropertyId(name);

        return GetItemFormatted(id);
    }

    /// <inheritdoc/>
    public override void InternalSetProperty(Guid id, object value)
    {
        SItem item = SItem.ResolveSItem(value);

        if (id == Guid.Empty)
        {
            return;
        }

        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item == _obj)
        {
            throw new ArgumentException(nameof(item));
        }

        if (item == _obj.Root)
        {
            throw new ArgumentException(nameof(item));
        }

        // Ignore duplicates
        if (item.Parent == _obj && item.FieldId == id)
        {
            return;
        }

        item.Unparent();

        SItem current;

        lock (_props)
        {
            current = _props.GetValueSafe(id);
        }

        current?.Parent = null;

        item.FieldId = id;
        item.Parent = _obj;

        lock (_props)
        {
            _props[id] = item;
        }

        // Auto fix field type
        //var field = _objType.Target?.GetField(id) as DObjectField;
        //if (field != null)
        //{
        //    item.InputType = field.FieldType;
        //}
    }

    /// <inheritdoc/>
    public override void InternalSetProperty(string name, object value)
    {
        Guid id = ResolvePropertyId(name);
        InternalSetProperty(id, value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetValues(ICondition context = null)
    {
        return _props.Values.Select(o => SItem.ResolveValue(o, context));
    }

    /// <inheritdoc/>
    public override IEnumerable<SItem> Items => _props.Values.Pass();

    /// <inheritdoc/>
    public override bool MergeTo(SObject target, bool skipDynamic)
    {
        var obj = this._obj;

        if (target is null)
        {
            return false;
        }

        if (ReferenceEquals(target, obj))
        {
            return false;
        }

        if (target._ex.ObjectType != _objType)
        {
            return false;
        }

        foreach (var pair in _props)
        {
            var value = pair.Value;
            var targetValue = target.GetItem(pair.Key);
            if (skipDynamic && targetValue is SDynamic)
            {
                continue;
            }

            bool merged = false;

            if (value is SObject childSObj && targetValue is SObject childTargetSObj)
            {
                if (childSObj.MergeTo(childTargetSObj, skipDynamic))
                {
                    merged = true;
                }
            }
            else if (value is SArray childAry && targetValue is SArray childTargetAry)
            {
                if (childAry.MergeTo(childTargetAry, skipDynamic))
                {
                    merged = true;
                }
            }
            
            if (!merged)
            {
                target.SetProperty(pair.Key, Cloner.Clone(value));
            }
        }

        return true;
    }

    #endregion

    #region Attachments

    /// <inheritdoc/>
    public override bool HasAttachments => _attachments?.Count > 0;

    /// <inheritdoc/>
    public override object GetAttachment(string name) => _attachments?.GetValueSafe(name);

    /// <inheritdoc/>
    public override void SetAttachment(string name, object value)
    {
        if (!NamingVerifier.VerifyIdentifier(name))
        {
            throw new ArgumentException("Invalid name: " + name, nameof(name));
        }

        if (value != null)
        {
            (_attachments ??= [])[name] = value;
        }
        else
        {
            _attachments?.Remove(name);
            if (_attachments?.Count == 0)
            {
                _attachments = null;
            }
        }
    }

    /// <inheritdoc/>
    public override void ClearAttachments()
    {
        _attachments?.Clear();
        _attachments = null;
    }

    /// <inheritdoc/>
    public override IEnumerable<KeyValuePair<string, object>> GetAttachments()
    {
        if (_attachments is { } attachment)
        {
            foreach (var pair in attachment)
            {
                yield return pair;
            }
        }
    }

    #endregion

    #region Controller

    /// <inheritdoc/>
    public override SObjectController Controller
    {
        get => _controller;
        set
        {
            // Can only set once
            if (value is null)
            {
                return;
            }

            if (_controller != null)
            {
                return;
            }

            Clear();

            _controller = value;
            _controller.Start(_obj);
        }
    }

    /// <summary>
    /// Updates the controller based on the current object type.
    /// </summary>
    /// <param name="autoStart">Whether to automatically start the controller after creation.</param>
    /// <param name="isResume">Whether to resume an existing controller or start fresh.</param>
    private void UpdateController(bool autoStart, bool isResume)
    {
        DCompond type = _objType.Target as DCompond;

        if (type?.NativeType != null)
        {
            if (_controller is null || _controller.GetType() != type.NativeType)
            {
                _controller?.Release();
                _controller = type.CreateController();

                if (_controller != null)
                {
                    if (autoStart)
                    {
                        if (isResume)
                        {
                            _controller.Resume(_obj);
                        }
                        else
                        {
                            _controller.Start(_obj);
                        }
                    }
                }
            }
        }
        else
        {
            _controller?.Release();
            _controller = null;
        }
    }

    #endregion

    #region Comparison

    /// <inheritdoc/>
    public override bool ValueEquals(object other)
    {
        if (other is not SObject)
        {
            return false;
        }

        SObject otherObj = (SObject)other;
        var otherEx = (SObjectExternalBK)otherObj._ex;

        if (_objType != otherEx._objType)
        {
            return false;
        }

        if (_controller != otherEx._controller)
        {
            return false;
        }

        if (_props.Count != otherEx._props.Count)
        {
            return false;
        }

        foreach (KeyValuePair<Guid, SItem> pair in _props)
        {
            if (!otherEx._props.ContainsKey(pair.Key)) return false;

            if (!SItem.ValueEquals(pair.Value, otherEx._props[pair.Key]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool TypeEquals(SItem other)
    {
        SObject otherObj = (SObject)other;
        var otherEx = (SObjectExternalBK)otherObj._ex;

        if (_objType != otherEx._objType)
        {
            return false;
        }

        if (_controller is null && otherEx._controller is null)
        {
            return true;
        }

        if (_controller is null || otherEx._controller is null)
        {
            return false;
        }

        return _controller.GetType() == otherEx._controller.GetType();
    }

    #endregion

    #region ISyncObject

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        switch (sync.Mode)
        {
            case SyncMode.Get:
                if (sync.Name == SObject.Attribute_ObjectType && _objType != null)
                {
                    sync.Sync(sync.Name, _objType.TypeCode, SyncFlag.AttributeMode);
                }
                else if (sync.Name == SObject.Attribute_InputType && _obj.InputType != null)
                {
                    sync.Sync(sync.Name, _obj.InputType.TypeCode, SyncFlag.AttributeMode);
                }
                else if (sync.Name == SObject.Attribute_Comment)
                {
                    sync.Sync(sync.Name, _obj.IsComment ? "true" : "false", SyncFlag.AttributeMode);
                }
                else if (sync.Name.StartsWith("ex-"))
                {
                    sync.Sync(sync.Name, GetAttachment(sync.Name[3..]));
                }
                else 
                {
                    sync.Sync(sync.Name, GetItem(sync.Name));
                }
                break;

            case SyncMode.Set:
                if (sync.Name == SObject.Attribute_ObjectType)
                {
                    ObjectType = TypeDefinition.Resolve(sync.Value as string);
                    UpdateController(true, false);
                }
                else if (sync.Name == SObject.Attribute_InputType)
                {
                    _obj.InputType = TypeDefinition.Resolve(sync.Value as string);
                }
                else if (sync.Name == SObject.Attribute_Comment)
                {
                    _obj.IsComment = sync.Value as string == "true";
                }
                else if (sync.Name.StartsWith("ex-"))
                {
                    SetAttachment(sync.Name[3..], sync.Value);
                }
                else
                {
                    if (Guid.TryParseExact(sync.Name, "D", out Guid id))
                    {
                        SetProperty(id, sync.Value);
                    }
                }
                break;

            case SyncMode.GetAll:
                SyncGetAll(sync, context);
                break;

            case SyncMode.SetAll:
                SyncSetAll(sync, context);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Synchronizes all properties in get mode.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context.</param>
    private void SyncGetAll(IPropertySync sync, ISyncContext context)
    {
        //TODO: SObject SyncMode.GetAll causes multi-threading error
        var objType = _objType.Target;
        if (objType != null)
        {
            if (_controller != null)
            {
                // Update status, reconnect lost references
                _controller.UpdateStatus();

                var getField = new GetFieldsPropertySync(sync.Intent);
                _controller.InternalSync(getField, context);
                foreach (var name in getField.Fields)
                {
                    Guid id = NativeFieldResolver.Instance.ResolveFieldId(objType, name);
                    SItem item = GetItem(id);

                    if (item != null)
                    {
                        sync.Sync(name, item);
                    }
                }
            }
            else
            {
                foreach (var field in objType.PublicFields)
                {
                    //if (sync.Intent == SyncIntent.Serialize && field is null)
                    //{
                    //    // Filter extra items but don't delete to support Undo
                    //    continue;
                    //}

                    var item = _props.GetValueSafe(field.Id);
                    if (item is null)
                    {
                        continue;
                    }

                    if (sync.Intent == SyncIntent.Serialize)
                    {
                        // Guid to id-Guid
                        sync.Sync(field.Id.GetDataAccessFieldName(), item);
                    }
                    else
                    {
                        //string name = ResolvePropertyName(field.Id);
                        //if (string.IsNullOrEmpty(name))
                        //{
                        //    name = EditorObjectManager.Instance.GetObject(field.Id)?.Name;
                        //}

                        //if (string.IsNullOrEmpty(name))
                        //{
                        //    name = GlobalIdResolver.RevertResolve(field.Id);
                        //    if (!string.IsNullOrEmpty(name))
                        //    {
                        //        name = "old-" + name.FindLastAndGetAfter('.');
                        //    }
                        //}

                        //if (string.IsNullOrEmpty(name))
                        //{
                        //    name = field.Id.GetDataAccessFieldName();
                        //}

                        // Guid to string
                        sync.Sync(field.Name, item);
                    }
                }
            }
        }
        else
        {
            foreach (var pair in _props)
            {
                sync.Sync(pair.Key.GetDataAccessFieldName(), pair.Value); // Guid to string
            }
        }

        if (_attachments is { } attachment)
        {
            foreach (var pair in attachment)
            {
                sync.Sync("ex-" + pair.Key, pair.Value);
            }
        }

        if (_objType != null)
        {
            sync.SyncGetTypeDefinition(SObject.Attribute_ObjectType, _objType);
        }

        if (_obj.InputType != null)
        {
            sync.SyncGetTypeDefinition(SObject.Attribute_InputType, _obj.InputType);
        }

        if (_obj.IsComment)
        {
            sync.Sync(SObject.Attribute_Comment, "true", SyncFlag.AttributeMode);
        }
    }

    /// <summary>
    /// Synchronizes all properties in set mode.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context.</param>
    private void SyncSetAll(IPropertySync sync, ISyncContext context)
    {
        // Clear all values
        Clear();

        if (sync.SyncSetTypeDefinition(SObject.Attribute_ObjectType, ObjectType, out TypeDefinition newObjType, out string newObjTypeId))
        {
            ObjectType = newObjType;
        }

        if (sync.SyncSetTypeDefinition(SObject.Attribute_InputType, _obj.InputType, out TypeDefinition newInputType, out string newInputTypeId))
        {
            _obj.InputType = newInputType;
        }

        UpdateController(true, false);

        // Copy values
        if (_controller != null)
        {
            foreach (string name in sync.Names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (name.StartsWith("ex-"))
                {
                    object attachment = sync.Sync<object>(name, null);
                    SetAttachment(name[3..], attachment);
                    continue;
                }

                object value = sync.Sync<object>(name, null);
                SetProperty(name, value);
            }
        }
        else
        {
            foreach (string name in sync.Names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (name.StartsWith("ex-"))
                {
                    object attachment = sync.Sync<object>(name, null);
                    SetAttachment(name[3..], attachment);
                    continue;
                }

                Guid fieldId;
                if (name.StartsWith("old-"))
                {
                    fieldId = ResolvePropertyId(name.RemoveFromFirst(4));
                }
                else
                {
                    fieldId = ResolvePropertyId(name);
                }

                if (fieldId == Guid.Empty)
                {
                    fieldId = SyncExportExtensions.ResolveFieldId(newObjTypeId, name, context);
                }

                if (fieldId != Guid.Empty)
                {
                    object value = sync.Sync<object>(name, null);
                    SetProperty(fieldId, value);
                }
            }
        }

        _obj.IsComment = sync.Sync<string>(SObject.Attribute_Comment, null, SyncFlag.AttributeMode) as string == "true";
    }

    #endregion

    #region ISupportAnalysis

    /// <inheritdoc/>
    public override void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        do
        {
            var inputType = _obj.InputType;

            if (TypeDefinition.IsNullOrEmpty(ObjectType))
            {
                // Can be empty struct
                //problems.Add(new AnalysisProblem(TextStatus.Error, $"Type is undefined"));
                break;
            }

            if (!TypeDefinition.IsNullOrEmpty(inputType) && !inputType.IsAssignableFrom(ObjectType))
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"{inputType.GetFullTypeNameText()} cannot be implemented by {ObjectType.GetFullTypeNameText()}"));
                break;
            }
        } while (false);

        try
        {
            _controller?.CollectProblem(problems, intent);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    #endregion

    #region Repair

    /// <inheritdoc/>
    public override void RepairIds()
    {
        lock (_props)
        {
            //TODO: This RepairIds method has issues, causing data to rollback to old Id values

            //foreach (var pair in _dic.ToArray())
            //{
            //    Guid id = GlobalIdResolver.FixId(pair.Key);
            //    if (id != Guid.Empty && id != pair.Key)
            //    {
            //        _dic[id] = pair.Value;
            //    }
            //}
        }
    }

    #endregion

    #region Misc

    /// <inheritdoc/>
    public override void AutoConvertValue()
    {
        if (_objType?.Target is not DCompond objType)
        {
            return;
        }

        List<Guid> removes = null;
        foreach (var pair in _props)
        {
            if (objType.GetPublicField(pair.Key) is not DStructField field)
            {
                //TODO: No operation on missing fields
                (removes ??= []).Add(pair.Key);
                continue;
            }

            pair.Value.InputType = field.FieldType;
            pair.Value.AutoConvertValue();
        }
    }

    /// <inheritdoc/>
    public override void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        _objType = TypeDefinition.ReferenceSync(_objType, path, sync, () => ToString());

        switch (sync.Mode)
        {
            case ReferenceSyncMode.Build:
                try
                {
                    //TODO will cause multi-threading issue
                    KeyValuePair<Guid, SItem>[] pairs;
                    lock (_props)
                    {
                        pairs = [.. _props];
                    }

                    foreach (var pair in pairs)
                    {
                        string name = pair.Value.Name;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            sync.SyncId(path.Append(name), pair.Key, pair.Value?.ToString());
                        }
                        else
                        {
                            sync.SyncId(path, pair.Key, pair.Value?.ToString());
                        }
                    }
                }
                catch (Exception err)
                {
                    err.LogWarning();
                }
                break;

            case ReferenceSyncMode.Redirect:
                {
                    SItem value;
                    lock (_props)
                    {
                        value = _props.RemoveAndGet(sync.OldId);
                    }
                    if (value != null)
                    {
                        RemoveProperty(sync.Id);
                        lock (_props)
                        {
                            _props[sync.Id] = value;
                        }
                    }
                }
                break;

            case ReferenceSyncMode.Find:
                {
                    if (_props.TryGetValue(sync.Id, out SItem item))
                    {
                        string name = item.Name;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            sync.SyncId(path.Append(name), sync.Id, null);
                        }
                        else
                        {
                            sync.SyncId(path, sync.Id, null);
                        }
                    }
                }
                break;

            default:
                break;
        }


        //List<Tuple<Guid, Guid>> renames = null;
        //var target = _objType.Target;

        //foreach (var id in _dic.Keys)
        //{
        //    // Mask extra items, but don't delete to support Undo
        //    if (target != null && target.GetField(id) == null)
        //    {
        //        continue;
        //    }

        //    var id2 = sync.SyncId(path, id);
        //    if (id2 != id)
        //    {
        //        if (renames == null)
        //        {
        //            renames = new List<Tuple<Guid, Guid>>();
        //        }
        //        renames.Add(new Tuple<Guid, Guid>(id, id2));
        //    }
        //}

        //if (renames != null)
        //{
        //    foreach (var rename in renames)
        //    {
        //        SItem item = _dic.RemoveAndGet(rename.Item1);
        //        _dic[rename.Item2] = item;
        //        item.FieldId = rename.Item2;
        //    }
        //}
    }

    /// <inheritdoc/>
    public override FieldObject GetField(SItem item)
    {
        var id = item?.FieldId ?? Guid.Empty; //ResolvePropertyId(item?.Name);
        if (EditorObjectManager.Instance.GetObject(id) is FieldObject field)
        {
            return field;
        }

        if (_obj.InputType.IsAbstractFunction)
        {
            return _obj.Parent?.GetField(_obj);
        }

        return null;
    }

    /// <inheritdoc/>
    public override string GetBrief(int depth = 10)
    {
        if (_controller != null)
        {
            if (_controller is IPreviewDisplay previewDisplay)
            {
                return previewDisplay.PreviewText;
            }
            else
            {
                return _controller.ToString();
            }
        }
        else if (_objType != null)
        {
            if (_objType.Target is DCompond s)
            {
                return s.GetBrief(_obj, depth);
            }
            else
            {
                return _objType.ToDisplayString();
            }
        }
        else
        {
            return string.Empty;
        }
    }

    #endregion
}
