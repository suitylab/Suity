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
/// A field list for enum items, supporting creation of enum items and labels.
/// </summary>
[NativeAlias]
public class EnumItemList : SNamedFieldList<EnumItemBase>, IHasObjectCreationGUI
{
    static readonly ObjectCreationOption[] _options = [new(typeof(EnumItem), "Enum item"), new(typeof(EnumItemLabel), "Label")];

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumItemList"/> class.
    /// </summary>
    /// <param name="fieldName">The field name for this list.</param>
    /// <param name="parentElement">The parent named item.</param>
    public EnumItemList(string fieldName, SNamedItem parentElement) : base(fieldName, parentElement, null)
    {
    }

    /// <inheritdoc/>
    protected override NamedField OnCreateNewItem() => new EnumItem();

    /// <inheritdoc/>
    protected override async Task<NamedField> OnGuiCreateItemAsync(Type typeHint)
    {
        if (typeHint is null)
        {
            List<KeyValuePair<string, object>> list =
            [
                new KeyValuePair<string, object>(L("Enum Item"), typeof(EnumItem)),
                new KeyValuePair<string, object>(L("Label"), typeof(EnumItemLabel)),
            ];

            typeHint = DialogUtility.ShowSimpleSelectDialog(L("Create"), list) as Type;
        }

        if (typeHint != null && (typeHint == typeof(EnumItem) || typeHint == typeof(EnumItemLabel)))
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
            return new EnumItem();
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
