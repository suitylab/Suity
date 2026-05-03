using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Image asset
/// </summary>
public class ImageAsset : Asset
{
    private ImageDef _image;
    private ImageDef _iconSmall;

    public ImageAsset()
    {
        UpdateAssetTypes(typeof(ImageDef));
    }

    public ImageAsset(ImageDef image, string assetKey)
        : this()
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));

        if (image is BitmapDef bitmap)
        {
            // Set to standard resolution
            // bitmap.SetResolution(100, 100);
        }

        LocalName = assetKey;
    }

    public ImageDef GetImage()
    {
        if (_image is null)
        {
            var path = FileName;

            if (path != null)
            {
                _image = LoadImage(path);
            }
        }

        return _image;
    }

    /// <summary>
    /// Get a picture with a size of 32
    /// </summary>
    /// <returns></returns>
    public ImageDef GetIconSmall()
    {
        lock (this)
        {
            if (_iconSmall != null)
            {
                return _iconSmall;
            }
            else
            {
                var path = FileName;

                if (_image is null && path != null)
                {
                    _image = LoadImage(path);
                }

                if (_image is BitmapDef bmp)
                {
                    if (bmp.Width <= ImageHelper.SmallSize)
                    {
                        _iconSmall = bmp;
                        return _iconSmall;
                    }

                    _iconSmall = bmp.Resize(ImageHelper.SmallSize, ImageHelper.SmallSize);
                    if (path != null)
                    {
                        _image.Dispose();
                        _image = null;
                    }

                    return _iconSmall;
                }
                else
                {
                    return null;
                }
            } 
        }
    }

    private ImageDef LoadImage(StorageLocation fileName)
    {
        if (fileName is null)
        {
            return null;
        }

        try
        {
            using var stroage = fileName.GetStorageItem();
            return ImageHelper.FromStream(stroage.GetInputStream());
        }
        catch (Exception err)
        {
            err.LogError($"Load image failed : {fileName}");
        }

        return null;
    }

    public override ImageDef GetIcon()
    {
        return GetIconSmall();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EditorUtility.AddDelayedAction(new DelayDestroy(this));
    }

    /// <summary>
    /// Resize image with GDI+ so that image is nice and clear with required size.
    /// </summary>
    public static ImageDef ImageResize(BitmapDef source, int width, int height)
    {
        /*        var bitmap = new Bitmap(width, height, source.PixelFormat);
                var graphicsImage = Graphics.FromImage(bitmap);
                graphicsImage.SmoothingMode = SmoothingMode.HighQuality;
                graphicsImage.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsImage.DrawImage(source, 0, 0, bitmap.Width, bitmap.Height);
                graphicsImage.Dispose();

                return bitmap;*/

        return source;
    }

    public override bool CanExportToLibrary => true;

    private class DelayDestroy : DelayedAction<ImageAsset>
    {
        public DelayDestroy(ImageAsset asset)
            : base(asset, 5)
        {
        }

        public override void DoAction()
        {
            Value._image?.Dispose();
            Value._image = null;

            Value._iconSmall?.Dispose();
            Value._iconSmall = null;
        }
    }
}

public class ImageAssetActivator : AssetActivator
{
    private static readonly string[] _extensions = ["png"];

    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new ImageAsset();
    }

    public override string[] GetExtensions() => _extensions;
}