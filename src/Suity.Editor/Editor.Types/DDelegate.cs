using Suity.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a delegate type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Delegate, "Delegate")]
public class DDelegate : DType
{
    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.Delegate;

    /// <inheritdoc />
    public override bool IsNative => true;

    /// <inheritdoc />
    public override ImageDef DefaultIcon => CoreIconCache.Delegate;
}

/// <summary>
/// Builder for creating DDelegate instances.
/// </summary>
public class DDelegateBuilder : DTypeBuilder<DDelegate>
{
    /// <summary>
    /// Initializes a new instance of the DDelegateBuilder class.
    /// </summary>
    public DDelegateBuilder()
    { }

    /// <summary>
    /// Initializes a new instance of the DDelegateBuilder class with a name and icon.
    /// </summary>
    public DDelegateBuilder(string name, string iconKey)
    {
        SetLocalName(name);
        SetIconKey(iconKey);
    }
}