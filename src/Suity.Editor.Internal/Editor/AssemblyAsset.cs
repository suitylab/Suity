using Suity.Editor.WorkSpaces;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Asset class representing a .NET assembly reference (.dll file).
/// Implements <see cref="IAssemblyReference"/> to participate in assembly resolution.
/// </summary>
public class AssemblyAsset : Asset, IAssemblyReference
{
    /// <summary>
    /// Initializes a new instance of <see cref="AssemblyAsset"/> and registers it as an assembly reference type.
    /// </summary>
    public AssemblyAsset()
    {
        UpdateAssetTypes(typeof(IAssemblyReference));
    }

    /// <inheritdoc/>
    public override Image DefaultIcon => CoreIconCache.Assembly;
}

/// <summary>
/// Activator for <see cref="AssemblyAsset"/> instances that handles .dll file discovery and asset creation.
/// </summary>
public class AssemblyAssetActivator : AssetActivator
{
    private static readonly string[] _extensions = ["dll"];

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new AssemblyAsset();
    }

    /// <inheritdoc/>
    public override string[] GetExtensions()
    {
        return _extensions;
    }
}