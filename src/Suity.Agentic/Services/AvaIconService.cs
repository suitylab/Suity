using Suity.Editor.Documents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Suity.Helpers;

namespace Suity.Editor.Services;

internal class AvaIconService : IIconService
{
    public static readonly AvaIconService Instance = new();

    public Image? GetIconById(Guid id)
    {
        ImageAsset imageRef = AssetManager.Instance.GetAsset<ImageAsset>(id);
        return imageRef?.GetIconSmall();
    }

    public Image? GetIconForFile(string path)
    {
        if (Directory.Exists(path))
        {
            return Suity.Editor.CoreIconCache.Folder;
        }

        return null;
    }

    public Image? GetIconForFileExact(string path)
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
