using Suity.Drawing;
using Suity.Synchonizing.Core;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Base class for named field lists.
/// </summary>
public abstract class SNamedFieldList : NamedFieldList
{
    private readonly string _fieldName;

    internal SNamedFieldList(string fieldName)
        : base()
    {
        _fieldName = fieldName;
    }

    internal SNamedFieldList(string fieldName, Type defaultItemType)
         : base(defaultItemType)
    {
        if (!typeof(SNamedField).IsAssignableFrom(defaultItemType))
        {
            throw new ArgumentException();
        }

        _fieldName = fieldName;
    }

    internal SNamedFieldList(string fieldName, NamedItem parentItem)
        : base(parentItem)
    {
        _fieldName = fieldName;
    }

    internal SNamedFieldList(string fieldName, NamedItem parentItem, Type defaultItemType)
        : base(parentItem, defaultItemType)
    {
        if (!typeof(SNamedField).IsAssignableFrom(defaultItemType))
        {
            throw new ArgumentException();
        }

        _fieldName = fieldName;
    }

    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string FieldName => _fieldName;

    /// <summary>
    /// Gets or sets the field description.
    /// </summary>
    public string FieldDescription { get; set; }

    /// <summary>
    /// Gets or sets the field icon.
    /// </summary>
    public ImageDef FieldIcon { get; set; }

    public override SyncPath GetPath() => ParentItem?.GetPath().Append(_fieldName) ?? SyncPath.Empty;

    protected override string OnGetText() => FieldDescription ?? base.OnGetText();

    protected override ImageDef OnGetIcon() => FieldIcon ?? base.OnGetIcon();
}

/// <summary>
/// Generic named field list with a specific field type.
/// </summary>
public class SNamedFieldList<TItem> : SNamedFieldList
    where TItem : SNamedField
{
    private readonly Func<TItem> _factory;

    public SNamedFieldList(string fieldName, NamedItem parentItem, Func<TItem> factory = null)
        : base(fieldName, parentItem, typeof(TItem))
    {
        _factory = factory;
    }

    public SNamedFieldList(string fieldName, Func<TItem> factory = null)
        : base(fieldName, typeof(TItem))
    {
        _factory = factory;
    }

    protected override NamedField OnCreateNewItem()
    {
        if (_factory != null)
        {
            return _factory();
        }
        else
        {
            return base.OnCreateNewItem();
        }
    }
}