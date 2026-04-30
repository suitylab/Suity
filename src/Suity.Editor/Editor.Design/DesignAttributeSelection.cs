using Suity.Editor.Selecting;
using Suity.Editor.Types;
using System;

namespace Suity.Editor.Design;

/// <summary>
/// Provides asset selection for design attributes.
/// </summary>
public class DesignAttributeSelection : AssetSelection<DStruct>
{
    private static readonly AbstractTypeFilter _filter = new("*Suity|Attribute");

    public override IAssetFilter Filter => _filter;

    public DesignAttributeSelection()
    {
    }

    public DesignAttributeSelection(DStruct dStruct)
    {
        Target = dStruct;
    }

    public DesignAttributeSelection(Guid id)
    {
        Id = id;
    }
}