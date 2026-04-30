using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Drawing;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// An assembly reference item backed by an asset that implements <see cref="IAssemblyReference"/>.
/// </summary>
public class AssetAssemblyReferenceItem : IAssemblyReferenceItem, ISyncObject, IReference
{
    private readonly EditorAssetRef<IAssemblyReference> _assemblyRef = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="AssetAssemblyReferenceItem"/>.
    /// </summary>
    public AssetAssemblyReferenceItem()
    {
    }

    /// <summary>
    /// Initializes a new instance referencing the asset with the specified ID.
    /// </summary>
    /// <param name="id">The asset ID.</param>
    public AssetAssemblyReferenceItem(Guid id)
    {
        _assemblyRef.Id = id;
    }

    /// <inheritdoc/>
    public Guid Id => _assemblyRef.Id;

    /// <inheritdoc/>
    public string Key => Id.ToString();

    /// <inheritdoc/>
    public string Name
    {
        get
        {
            string shortName = AssetManager.Instance.GetAsset(Id)?.ShortTypeName ?? string.Empty;
            return shortName ?? Id.ToString();
        }
    }

    /// <inheritdoc/>
    public string HintPath => AssetManager.Instance.GetAsset(Id)?.FileName?.PhysicFileName;

    /// <inheritdoc/>
    public Image Icon => _assemblyRef.TargetAsset?.Icon;

    /// <inheritdoc/>
    public bool IsValid => _assemblyRef.Target is not null;

    /// <inheritdoc/>
    public bool IsDisabled => false;

    /// <summary>
    /// Gets the underlying assembly reference asset, or null if not resolved.
    /// </summary>
    public IAssemblyReference Reference => _assemblyRef.Target;

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        sync.SyncAssetRef(_assemblyRef, context);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _assemblyRef.ToString();
    }

    /// <inheritdoc/>
    public int CompareTo(IAssemblyReferenceItem other)
    {
        return Name.CompareTo(other.Name);
    }

    /// <inheritdoc/>
    void IReference.ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        _assemblyRef.Id = sync.SyncId(path, _assemblyRef.Id, null);
    }
}

/// <summary>
/// An assembly reference item that represents a system assembly reference by name.
/// </summary>
public class SystemAssemblyReferenceItem : IAssemblyReferenceItem, ISyncObject
{
    /// <summary>
    /// Initializes a new empty instance of <see cref="SystemAssemblyReferenceItem"/>.
    /// </summary>
    public SystemAssemblyReferenceItem()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified assembly key (name).
    /// </summary>
    /// <param name="key">The assembly name or key.</param>
    public SystemAssemblyReferenceItem(string key)
    {
        Key = key ?? string.Empty;
    }

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public string Key { get; private set; }

    /// <inheritdoc/>
    public string Name => Key;

    /// <inheritdoc/>
    public string HintPath => Key;

    /// <inheritdoc/>
    public Image Icon => CoreIconCache.System;

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrEmpty(Key);

    /// <inheritdoc/>
    public bool IsDisabled => false;

    /// <inheritdoc/>
    public int CompareTo(IAssemblyReferenceItem other)
    {
        return Name.CompareTo(other.Name);
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Key = sync.Sync(nameof(Key), Key);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// An assembly reference item that represents a disabled assembly reference by file path.
/// </summary>
public class DisabledAssemblyReferenceItem : IAssemblyReferenceItem, ISyncObject
{
    /// <summary>
    /// Initializes a new empty instance of <see cref="DisabledAssemblyReferenceItem"/>.
    /// </summary>
    public DisabledAssemblyReferenceItem()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified file path.
    /// </summary>
    /// <param name="fileName">The file path of the disabled assembly.</param>
    public DisabledAssemblyReferenceItem(string fileName)
    {
        Key = fileName ?? string.Empty;
    }

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public string Key { get; private set; }

    /// <inheritdoc/>
    public string Name => Key;

    /// <inheritdoc/>
    public string HintPath => Key;

    /// <inheritdoc/>
    public Image Icon => CoreIconCache.Disable;

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrEmpty(Key);

    /// <inheritdoc/>
    public bool IsDisabled => true;

    /// <inheritdoc/>
    public int CompareTo(IAssemblyReferenceItem other)
    {
        return Name.CompareTo(other.Name);
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Key = sync.Sync(nameof(Key), Key);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}