using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Helpers;
using System;
using System.IO;

namespace Suity.Editor.Services;

internal class AvaIconService : IIconService
{
    public static readonly AvaIconService Instance = new();

    public ImageDef? GetIconById(Guid id)
    {
        ImageAsset imageRef = AssetManager.Instance.GetAsset<ImageAsset>(id);
        return imageRef?.GetIconSmall();
    }

    public ImageDef? GetIconForFile(string path)
    {
        if (Directory.Exists(path))
        {
            return Suity.Editor.CoreIconCache.Folder;
        }

        return null;
    }

    public ImageDef? GetIconForFileExact(string path)
    {
        var image = FileAssetManager.Current.GetIcon(path);
        if (image != null)
        {
            return image;
        }

        var factory = DocumentManager.Instance.GetDocumentFormatByPath(path);

        image = factory?.Icon;
        if (image != null)
        {
            return image;
        }

        if (path.FileExtensionEquals(Asset.MetaExtension))
        {
            return CoreIconCache.Meta;
        }

        if (Directory.Exists(path))
        {
            return CoreIconCache.Folder;
        }

        return null;
    }
}
