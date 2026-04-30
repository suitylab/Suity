using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Suity.Editor.Values;

[NativeAlias]
/// <summary>
/// Represents an array of SItem values.
/// </summary>
public class SArray : SContainer, ISyncList, IValidate, IList
{
    internal readonly SArrayExternal _ex;

    /// <summary>
    /// Creates an empty SArray.
    /// </summary>
    public SArray()
    {
        _ex = SItemExternal._external.CreateSArrayEx(this);
    }

    /// <summary>
    /// Creates an SArray with the specified values.
    /// </summary>
    /// <param name="values">The values.</param>
    public SArray(IEnumerable<object> values)
    {
        _ex = SItemExternal._external.CreateSArrayEx(this, values);
    }

    /// <summary>
    /// Creates an SArray with the specified input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public SArray(TypeDefinition inputType)
        : base(inputType.MakeArrayType())
    {
        if (!InputType.IsArray)
        {
            throw new InvalidOperationException(L("Input type is not an array type."));
        }

        _ex = SItemExternal._external.CreateSArrayEx(this);
    }

    /// <summary>
    /// Creates an SArray with the specified input type and values.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="values">The values.</param>
    public SArray(TypeDefinition inputType, IEnumerable<object> values)
        : base(inputType.MakeArrayType())
    {
        if (!InputType.IsArray)
        {
            throw new InvalidOperationException(L("Input type is not an array type."));
        }

        _ex = SItemExternal._external.CreateSArrayEx(this, values);
    }

    /// <summary>
    /// Called when the input type changes.
    /// </summary>
    protected override void OnInputTypeChanged() => _ex.OnInputTypeChanged();

    #region Members

    /// <summary>
    /// Gets or sets the child item
    /// </summary>
    /// <param name="index">The index of the child item</param>
    /// <returns>Returns the child item at the index</returns>
    public object this[int index]
    {
        get => _ex[index];
        set => _ex[index] = value;
    }

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="context">The condition context.</param>
    public object GetValue(int index, ICondition context = null) => _ex.GetValue(index, context);

    /// <summary>
    /// Ensures a value exists at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="context">The condition context.</param>
    public object EnsureValue(int index, ICondition context = null) => _ex.EnsureValue(index, context);

    /// <summary>
    /// Sets the value at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The value.</param>
    public void SetValue(int index, object value) => _ex.SetValue(index, value);

    /// <summary>
    /// Gets all values.
    /// </summary>
    /// <param name="context">The condition context.</param>
    public override IEnumerable<object> GetValues(ICondition context = null) => _ex.GetValues(context);

    /// <summary>
    /// Gets the SItem at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    public SItem GetItem(int index) => _ex.GetItem(index);

    /// <summary>
    /// Gets the formatted SItem at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    public SItem GetItemFormatted(int index) => _ex.GetItemFormatted(index);

    /// <summary>
    /// Sets the SItem at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The SItem.</param>
    public void SetItem(int index, SItem item) => _ex.SetItem(index, item);

    /// <summary>
    /// Gets all child items.
    /// </summary>
    public override IEnumerable<SItem> Items => _ex.Items;

    /// <summary>
    /// Adds a child item
    /// </summary>
    /// <param name="value">Child item</param>
    /// <summary>
    /// Adds a child item.
    /// </summary>
    /// <param name="value">Child item</param>
    public void Add(object value) => _ex.Add(value);

    /// <summary>
    /// Adds a range of items.
    /// </summary>
    /// <param name="values">The values to add.</param>
    public void AddRange(IEnumerable<object > values)
    {
        foreach (var value in values)
        {
            _ex.Add(value);
        }
    }

    /// <summary>
    /// Inserts a child item.
    /// </summary>
    /// <param name="index">Insert index</param>
    /// <param name="value">Child item</param>
    public void Insert(int index, object value) => _ex.Insert(index, value);

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public override void RemoveItem(SItem item) => _ex.RemoveItem(item);

    /// <summary>
    /// Removes a child item at the index.
    /// </summary>
    /// <param name="index">Index</param>
    public void RemoveAt(int index) => _ex.RemoveAt(index);

    /// <summary>
    /// Clears the array.
    /// </summary>
    public override bool Clear() => _ex.Clear();

    /// <summary>
    /// Gets the count of child items.
    /// </summary>
    public int Count => _ex.Count;

    /// <summary>
    /// Merges to another array.
    /// </summary>
    /// <param name="target">The target array.</param>
    /// <param name="skipDynamic">Whether to skip dynamic items.</param>
    public bool MergeTo(SArray target, bool skipDynamic) => _ex.MergeTo(target, skipDynamic);

    /// <summary>
    /// Converts to Array.
    /// </summary>
    public Array ToArray() => _ex.ToArray();

    /// <summary>
    /// Converts to Array of the specified type.
    /// </summary>
    /// <param name="type">The element type.</param>
    public Array ToArray(Type type) => _ex.ToArray(type);

    /// <summary>
    /// Converts to Array of type T.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="defaultValue">The default value.</param>
    public T[] ToArray<T>(T defaultValue = default) => _ex.ToArray<T>(defaultValue);

    #endregion

    #region Comparison

    /// <summary>
    /// Compares the value equality with another object.
    /// </summary>
    /// <param name="other">The other object.</param>
    public override bool ValueEquals(object other) => _ex.ValueEquals(other);

    /// <summary>
    /// Gets whether this is the default value.
    /// </summary>
    public override bool GetIsDefault() => GetIsBlank();

    public override bool GetIsBlank() => _ex.Count == 0;

    #endregion

    #region ISyncList

    /// <summary>
    /// Synchronizes the array contents.
    /// </summary>
    /// <param name="sync">The index sync.</param>
    /// <param name="context">The sync context.</param>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context) => _ex.Sync(sync, context);

    #endregion

    /// <summary>
    /// Auto converts the value based on the input type.
    /// </summary>
    public override void AutoConvertValue() => _ex.AutoConvertValue();

    /// <summary>
    /// Validates the array.
    /// </summary>
    /// <param name="context">The validation context.</param>
    public override void Validate(ValidationContext context) => _ex.Validate(context);

    /// <summary>
    /// Gets the field for an SItem.
    /// </summary>
    /// <param name="item">The SItem.</param>
    public override FieldObject GetField(SItem item) => _ex.GetField(item);

    /// <summary>
    /// Returns a string representation of this array.
    /// </summary>
    public override string ToString() => $"[{Count}]";

    internal void AssertIndex() => _ex.AssertIndex();

    #region ICollection
    int ICollection.Count => _ex.Count;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index] { get => this[index]; set => this[index] = value; }

    void ICollection.CopyTo(Array array, int index)
    {
        int i = index;
        foreach (var item in _ex.Items)
        {
            array.SetValue(item, i);
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => _ex.Items.GetEnumerator();

    #endregion

    #region IList

    int IList.Add(object value)
    {
        _ex.Add(value);
        return _ex.Count;
    }

    void IList.Clear()
    {
        _ex.Clear();
    }

    bool IList.Contains(object value)
    {
        return value is SItem item && item.Parent == this;
    }

    int IList.IndexOf(object value)
    {
        if (value is SItem item && item.Parent == this)
        {
            return item.Index;
        }
        else
        {
            return -1;
        }
    }

    void IList.Insert(int index, object value)
    {
        _ex.Insert(index, value);
    }

    void IList.Remove(object value)
    {
        if (value is SItem item && item.Parent == this)
        {
            _ex.RemoveItem(item);
        }
    }

    void IList.RemoveAt(int index)
    {
        _ex.RemoveAt(index);
    }

    #endregion
}