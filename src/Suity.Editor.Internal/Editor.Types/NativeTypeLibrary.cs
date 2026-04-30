using Suity.Editor.Design;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Types;

/// <summary>
/// Editor type library reflected from native types
/// </summary>
public class NativeTypeLibrary : DTypeFamily
{
    private readonly Dictionary<Type, DType> _types = [];

    public NativeTypeLibrary()
    {
    }

    internal NativeTypeLibrary(string codeBase)
    {
        LocalName = codeBase;
        NameSpace = codeBase;
    }

    internal void AddType(Type type, DType dtype)
    {
        _types.Add(type, dtype);
        AddOrUpdateChildAsset(dtype);
    }
}

/// <summary>
/// Builder for creating and configuring <see cref="NativeTypeLibrary"/> assets.
/// </summary>
public class NativeTypeFamilyBuilder : GroupAssetBuilder<NativeTypeLibrary>, IDesignBuilder
{
    private IAttributeDesign _attribute = EmptyAttributeDesign.Empty;

    /// <summary>
    /// Creates a new instance of <see cref="NativeTypeFamilyBuilder"/>.
    /// </summary>
    public NativeTypeFamilyBuilder()
    {
        AddAutoUpdate(nameof(DTypeFamily.Attributes), o => o.UpdateAttributes(_attribute, false));
    }

    /// <summary>
    /// Updates the attributes for the type family.
    /// </summary>
    /// <param name="attributes">The attribute design to apply.</param>
    public void UpdateAttributes(IAttributeDesign attributes)
    {
        _attribute = attributes ?? EmptyAttributeDesign.Empty;
        TryUpdateNow(d => d.UpdateAttributes(_attribute, true));
    }

    /// <inheritdoc/>
    public void SetBindingInfo(object bindingInfo)
    {
    }

    /// <summary>
    /// Updates the code base (local name and namespace) for the type family.
    /// </summary>
    /// <param name="codeBase">The code base string to set.</param>
    public void UpdateCodeBase(string codeBase)
    {
        TryUpdateNow(d => 
        {
            d.LocalName = codeBase;
            d.NameSpace = codeBase;
        });
    }

    /// <inheritdoc/>
    public override void NewAsset()
    {
        var lib = NativeTypeReflector.Instance.EnsureTypeLibrary(LocalName);
        if (lib != null)
        {
            base.SetAsset(lib);
        }
        else
        {
            base.NewAsset();
        }
    }
}