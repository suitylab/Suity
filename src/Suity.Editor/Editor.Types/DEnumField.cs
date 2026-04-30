using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Design;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a field in an enum type.
/// </summary>
public class DEnumField : DField, IComparable
{
    internal int _value;

    internal string _description;
    internal int _index;
    internal object _bindingInfo;
    internal bool _forceUpdate;
    private IAttributeGetter _attributes;
    private Color? _color;

    /// <summary>
    /// Initializes a new instance of the DEnumField class.
    /// </summary>
    public DEnumField()
    { }

    /// <inheritdoc />
    protected override string GetName()
    {
        return _name;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public int Value => _value;

    /// <inheritdoc />
    public override int Index => _index;

    /// <inheritdoc />
    public override string Description => _description;

    /// <inheritdoc />
    public override object BindingInfo => _bindingInfo;

    /// <inheritdoc />
    public override IAttributeGetter Attributes => _attributes ?? EmptyAttributeDesign.Empty;

    /// <summary>
    /// Updates the attributes.
    /// </summary>
    internal void UpdateAttributes(IAttributeGetter value, bool notify)
    {
        //if (ReferenceEquals(_attributes, value))
        //{
        //    return;
        //}

        _attributes = value ?? EmptyAttributeDesign.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        _color = _attributes.GetAttribute<IViewColor>()?.ViewColor;

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }

    /// <inheritdoc />
    public override Image GetIcon() => CoreIconCache.EnumField;

    /// <inheritdoc />
    public override string DisplayText => Attributes?.GetIsHiddenOrDisabled() == true ? string.Empty : base.DisplayText;

    /// <inheritdoc />
    public override Color? ViewColor => _color;

    /// <summary>
    /// Gets the numeric value based on ID automation.
    /// </summary>
    public int GetNumber()
    {
        if (this.Parent is not DEnum parent)
        {
            return 0;
        }

        switch (parent.IdAutomation)
        {
            case IdAutomations.Index:
                return _index;

            case IdAutomations.IndexOne:
                return _index + 1;

            case IdAutomations.Defined:
                return _value;

            case IdAutomations.IndexBinaryFlag:
                return 2 ^ _index;

            case IdAutomations.IndexOneBinaryFlag:
                return 2 ^ (_index + 1);

            case IdAutomations.DefinedBinaryFlag:
                return 2 ^ (_value + 1);

            default:
                return _index;
        }
    }

    int IComparable.CompareTo(object obj)
    {
        if (obj is DEnumField other)
        {
            return -string.CompareOrdinal(Name, other.Name);
        }
        else
        {
            return 1;
        }
    }

    public override string ToString() => L(DisplayText);
}