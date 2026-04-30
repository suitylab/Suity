using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.Reflecting;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// A field list for struct fields, supporting creation of parameters and labels.
/// </summary>
[NativeAlias]
public class StructFieldList : SNamedFieldList<StructFieldItem>, IHasObjectCreationGUI
{
    static readonly ObjectCreationOption[] _options = [new(typeof(StructField), "Parameter"), new(typeof(StructFieldLabel), "Label")];

    /// <summary>
    /// Initializes a new instance of the <see cref="StructFieldList"/> class.
    /// </summary>
    /// <param name="fieldName">The field name for this list.</param>
    /// <param name="parentElement">The parent named item.</param>
    public StructFieldList(string fieldName, SNamedItem parentElement)
        : base(fieldName, parentElement, null)
    {
    }

    /// <inheritdoc/>
    protected override NamedField OnCreateNewItem() => new StructField();

    /// <inheritdoc/>
    protected override async Task<NamedField> OnGuiCreateItemAsync(Type typeHint)
    {
        if (typeHint is null)
        {
            List<KeyValuePair<string, object>> list =
            [
                new KeyValuePair<string, object>(L("Parameter"), typeof(StructField)),
                new KeyValuePair<string, object>(L("Label"), typeof(StructFieldLabel)),
            ];

            typeHint = DialogUtility.ShowSimpleSelectDialog(L("Create"), list) as Type;
        }

        if (typeHint != null && (typeHint == typeof(StructField) || typeHint == typeof(StructFieldLabel)))
        {
            return (NamedField)typeHint.CreateInstanceOf();
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override object ResolveObject(string typeName, string parameter)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return new StructField();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the sibling item immediately before the specified field.
    /// </summary>
    /// <param name="field">The reference field.</param>
    /// <returns>The previous sibling field item, or null if none exists.</returns>
    public StructFieldItem GetSiblingPrevious(StructFieldItem field)
    {
        int index = IndexOf(field);
        if (index < 0)
        {
            return null;
        }

        index--;
        if (index >= 0)
        {
            return this[index] as StructField;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the sibling item immediately after the specified field.
    /// </summary>
    /// <param name="field">The reference field.</param>
    /// <returns>The next sibling field item, or null if none exists.</returns>
    public StructFieldItem GetSiblingNext(StructFieldItem field)
    {
        int index = IndexOf(field);
        if (index < 0)
        {
            return null;
        }

        index++;
        if (index < Count)
        {
            return this[index] as StructField;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the label sibling immediately before the specified field.
    /// </summary>
    /// <param name="field">The reference field.</param>
    /// <returns>The previous sibling label, or null if none exists.</returns>
    public StructFieldLabel GetSibling(StructField field)
    {
        int index = IndexOf(field);
        if (index < 0)
        {
            return null;
        }

        index--;
        if (index >= 0)
        {
            return this[index] as StructFieldLabel;
        }
        else
        {
            return null;
        }
    }

    #region IHasObjectCreationGUI

    /// <inheritdoc/>
    public IEnumerable<ObjectCreationOption> CreationOptions => _options;

    /// <inheritdoc/>
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        return await OnGuiCreateItemAsync(typeHint);
    } 

    #endregion
}
