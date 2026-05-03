using Suity.Drawing;
using System.Collections;
using System.Resources;

namespace Suity.Editor;

/// <summary>
/// Group asset that loads images from a <see cref="ResourceManager"/>, creating <see cref="ImageAsset"/> children for each image resource.
/// </summary>
public class ResourceImageGroupAsset : GroupAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceImageGroupAsset"/> class.
    /// Enumerates all resources in the specified <see cref="ResourceManager"/> and creates <see cref="ImageAsset"/> children
    /// for each resource that is an <see cref="ImageDef"/> or a byte array representing image data.
    /// </summary>
    /// <param name="resourceManager">The resource manager to load images from.</param>
    /// <param name="assetKey">The asset key used as the local name for this group asset.</param>
    public ResourceImageGroupAsset(ResourceManager resourceManager, string assetKey)
    {
        LocalName = assetKey;

        ResourceSet resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);

        if (resourceSet != null)
        {
            foreach (var item in resourceSet)
            {
                var entry = (DictionaryEntry)item;
                if (entry.Value is ImageDef image)
                {
                    string name = entry.Key.ToString();
                    var asset = new ImageAsset(image, name);

                    AddOrUpdateChildAsset(asset);
                }
                else if (entry.Value is byte[] bytes)
                {
                    string name = entry.Key.ToString();
                    var bmp = new BitmapDef(bytes);
                    var asset = new ImageAsset(bmp, name);

                    AddOrUpdateChildAsset(asset);
                }
            }
        }

        ResolveId();
    }
}