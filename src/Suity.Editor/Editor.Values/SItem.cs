using Suity.Editor.Types;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Values;

/// <summary>
/// Base class for all data items in the Suity editor framework.
/// </summary>
[NativeType(Name = "SItem", Description = "Data base object", CodeBase = "*Core", Icon = "*CoreIcon|Value")]
public abstract class SItem :
    IReference,
    IValidate,
    IValueEqual,
    ISyncPathObject
{
    internal const string Attribute_InputType = "base";

    private TypeDefinition _inputType;
    private readonly bool _inputTypeLocked;

    private SContainer _parent;

    // Field id is synced in SObject
    private Guid _fieldId;

    private int _index = -1;

    public SItem()
    {
        _inputType = TypeDefinition.Empty;
    }

    public SItem(TypeDefinition inputType)
    {
        _inputType = inputType ?? TypeDefinition.Empty;
        _inputTypeLocked = true;
    }

    /// <summary>
    /// Gets the parent node
    /// </summary>
    public SContainer Parent
    {
        get => _parent;
        internal set
        {
            if (_parent != value)
            {
                _parent = value;
                if (_parent is null)
                {
                    _index = -1;
                }

                OnParentChanged();
            }
        }
    }

    /// <summary>
    /// Gets the root node
    /// </summary>
    public SItem Root
    {
        get
        {
            SItem current = this;

            while (true)
            {
                if (current.Parent is null)
                {
                    return current;
                }

                current = current.Parent;

                // cyclic detect
                if (current == this)
                {
                    Logs.LogError($"{nameof(SItem)} Parent cyclic detected.");
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Gets whether this item is the root item.
    /// </summary>
    public bool IsRoot => _parent is null;

    /// <summary>
    /// Gets the root context.
    /// </summary>
    public object RootContext => (Root as SContainer)?.Context;

    /// <summary>
    /// Gets or sets the input type definition.
    /// </summary>
    public TypeDefinition InputType
    {
        get => _inputType ?? TypeDefinition.Empty;
        set
        {
            if (_inputTypeLocked)
            {
                return;
            }

            value ??= TypeDefinition.Empty;

            if (_inputType == value)
            {
                return;
            }

            _inputType = value;
            OnInputTypeChanged();
        }
    }

    /// <summary>
    /// Gets the target type definition.
    /// </summary>
    public virtual TypeDefinition TargetType => InputType;

    /// <summary>
    /// Gets whether the input type is locked.
    /// </summary>
    public bool InputTypeLocked => _inputTypeLocked;

    /// <summary>
    /// Gets or sets the property name, only valid when it becomes a child property of <see cref="SObject"/>.
    /// </summary>
    public Guid FieldId
    {
        get => _fieldId;
        internal set => _fieldId = value;
    }

    /// <summary>
    /// Gets the field for this item.
    /// </summary>
    public FieldObject GetField() => Parent?.GetField(this);

    /// <summary>
    /// Gets the index on the array parent node, only valid when it becomes a child item of <see cref="DArray"/>, otherwise returns -1.
    /// </summary>
    public int Index
    {
        get => _parent != null ? _index : -1;
        internal set => _index = value;
    }

    /// <summary>
    /// Gets the name of this item.
    /// </summary>
    public string Name => (Parent as SObject)?.ResolvePropertyName(_fieldId) ?? null;

    /// <summary>
    /// Removes this item from its parent.
    /// </summary>
    public void Unparent() => _parent?.RemoveItem(this);

    /// <summary>
    /// Creates a clone of this item.
    /// </summary>
    public SItem Clone() => Cloner.Clone(this);

    /// <summary>
    /// Gets or sets whether this item is read-only.
    /// </summary>
    public bool ReadOnly { get; set; }

    #region Path

    /// <summary>
    /// Gets the synchronization path.
    /// </summary>
    public SyncPath GetSyncPath()
    {
        if (_parent is null)
        {
            return SyncPath.Empty;
        }

        int index = Index;
        if (index >= 0)
        {
            return _parent.GetSyncPath().Append(index);
        }
        else
        {
            return _parent.GetSyncPath().Append(_fieldId);
        }
    }

    /// <summary>
    /// Gets the path string.
    /// </summary>
    public string GetPath() => SItemExternal._external.GetPath(this);

    #endregion

    #region Type

    public virtual void AutoConvertValue()
    { }

    protected virtual void OnParentChanged()
    { }

    protected virtual void OnInputTypeChanged()
    { }

    internal void RepairInputTypeForce(TypeDefinition inputType)
    {
        if (_inputType != inputType)
        {
            _inputType = inputType;
            OnInputTypeChanged();
        }
    }

    #endregion

    internal bool GetInstanceAccessMode() => (_parent as SObject)?.Controller?.InstanceAccess ?? false;

    #region Comparison

    public virtual bool GetIsDefault()
    {
        if (this.GetField() is DStructField field)
        {
            return field.EqualsDefaultValue(this);
        }
        else
        {
            return GetIsBlank();
        }
    }

    public virtual bool GetIsBlank() => false;

    public virtual bool ValueEquals(object other) => ReferenceEquals(this, other);

    public virtual bool TypeEquals(SItem other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }
        return _inputType == other._inputType;
    }

    public static bool ValueEquals(SItem valueA, SItem valueB)
    {
        if (ReferenceEquals(valueA, valueB))
        {
            return true;
        }
        if (valueA is null || valueB is null)
        {
            return false;
        }
        return valueA.ValueEquals(valueB);
    }

    public static bool AllTypesAreEqual(IEnumerable<SItem> values)
    {
        SItem first = values.FirstOrDefault();

        if (first != null)
        {
            return values.Skip(1).All(other => first.TypeEquals(other));
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region IReference

    /// <summary>
    /// Synchronizes references.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync object.</param>
    public virtual void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        _inputType = TypeDefinition.ReferenceSync(_inputType, path, sync);
    }

    #endregion

    #region IValidate Members

    /// <summary>
    /// Finds items matching the search criteria.
    /// </summary>
    /// <param name="contex">The validation context.</param>
    /// <param name="find">The search string.</param>
    /// <param name="findOption">The search option.</param>
    public virtual void Find(ValidationContext contex, string find, SearchOption findOption)
    { }

    /// <summary>
    /// Validates this item.
    /// </summary>
    /// <param name="context">The validation context.</param>
    public virtual void Validate(ValidationContext context)
    { }

    #endregion

    #region ISyncPathObject

    /// <summary>
    /// Gets the path for synchronization.
    /// </summary>
    SyncPath ISyncPathObject.GetPath() => GetSyncPath();

    #endregion

    #region IHasId

    /// <summary>
    /// Gets the type ID.
    /// </summary>
    public virtual Guid TypeId => _inputType?.TargetId ?? Guid.Empty;

    #endregion

    #region Resolve static

    /// <summary>
    /// Resolves the S type from a native type.
    /// </summary>
    /// <param name="type">The native type.</param>
    public static Type ResolveSType(Type type)
        => SItemExternal._external.ResolveSType(type);

    /// <summary>
    /// Resolves an SItem from an object value.
    /// </summary>
    /// <param name="value">The object value.</param>
    public static SItem ResolveSItem(object value)
        => SItemExternal._external.ResolveSItem(value);

    /// <summary>
    /// Resolves the value from an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    /// <param name="context">The condition context.</param>
    public static object ResolveValue(SItem item, ICondition context = null)
        => SItemExternal._external.ResolveValue(item, context);

    /// <summary>
    /// Resolves the value from an object.
    /// </summary>
    /// <param name="value">The object value.</param>
    /// <param name="context">The condition context.</param>
    public static object ResolveValue(object value, ICondition context = null)
        => SItemExternal._external.ResolveValue(value, context);

    /// <summary>
    /// Resolves the original value from an object.
    /// </summary>
    /// <param name="value">The object value.</param>
    /// <param name="context">The condition context.</param>
    public static object ResolveOriginValue(object value, ICondition context = null)
        => SItemExternal._external.ResolveOriginValue(value, context);

    /// <summary>
    /// Determines if the item is the same as or a parent of the specified parent.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <param name="parent">The parent to check against.</param>
    public static bool IsMeOrParent(SItem item, SItem parent)
        => SItemExternal._external.IsMeOrParent(item, parent);

    /// <summary>
    /// Determines if the item is null or default.
    /// </summary>
    /// <param name="item">The item to check.</param>
    public static bool IsNullOrDefault(SItem item)
    {
        if (item is null)
        {
            return true;
        }

        return item.GetIsDefault();
    }

    /// <summary>
    /// Determines if the item is null or blank.
    /// </summary>
    /// <param name="item">The item to check.</param>
    public static bool IsNullOrBlank(SItem item)
    {
        if (item is null)
        {
            return true;
        }

        return item.GetIsBlank();
    }


    /// <summary>
    /// Clones an SItem.
    /// </summary>
    /// <typeparam name="T">The type of SItem.</typeparam>
    /// <param name="item">The item to clone.</param>
    public static T Clone<T>(T item) where T : SItem => Cloner.Clone(item);

    #endregion
}