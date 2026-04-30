namespace Suity.Editor.Types;

/// <summary>
/// Represents an asset key type in the editor.
/// </summary>
public class DAssetKey : DType
{
    /// <summary>
    /// Initializes a new instance of the DAssetKey class.
    /// </summary>
    public DAssetKey()
    {
    }

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.AssetLink;
}