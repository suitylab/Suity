using Suity.Editor.Types;

namespace Suity.Editor;

/// <summary>
/// Value asset collection
/// </summary>
public interface IValueAssetCollection : IAssetCollection<ValueAsset>
{
    TypeDefinition Type { get; }
}

/// <summary>
/// Value manager
/// </summary>
public abstract class ValueManager
{
    public static ValueManager Instance { get; internal set; }

    internal ValueManager()
    { }

    public abstract IValueAssetCollection GetValueCollection(TypeDefinition type);

    internal abstract IRegistryHandle<ValueAsset> AddToValueType(ValueAsset value);
}