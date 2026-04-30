using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Default material for rendering text assets.
/// </summary>
[AssetAutoCreate]
public class DefaultTextMaterial : Asset, IMaterial
{
    /// <summary>
    /// Gets the default instance of this material.
    /// </summary>
    public static DefaultTextMaterial Default { get; private set; }

    private static bool _init;

    internal DefaultTextMaterial()
        : base($"*{nameof(DefaultTextMaterial)}")
    {
        if (_init)
        {
            return;
        }

        _init = true;

        UpdateAssetTypes(typeof(IMaterial));
        ResolveId();

        Default = this;
        MaterialUtility.DefaultTextMaterial = this;
    }

    /// <inheritdoc/>
    public override string DisplayText => "Default Text Material";

    /// <inheritdoc/>
    public override Image DefaultIcon => CoreIconCache.Text;

    /// <summary>
    /// Gets the render targets for this material.
    /// </summary>
    /// <param name="item">The render item.</param>
    /// <param name="basePath">The base path for output files.</param>
    /// <returns>A collection of render targets.</returns>
    public IEnumerable<RenderTarget> GetRenderTargets(RenderItem item, RenderFileName basePath)
    {
        if (item.RenderType.LocalName == RenderType.TextName)
        {
            yield return new TextRenderTarget(basePath.Append(item.Name), this, item);
        }
    }

    /// <summary>
    /// Render target for text content.
    /// </summary>
    private class TextRenderTarget : RenderTarget
    {
        /// <summary>
        /// Creates a text render target.
        /// </summary>
        /// <param name="fileName">The output file name.</param>
        /// <param name="material">The material.</param>
        /// <param name="item">The render item.</param>
        public TextRenderTarget(RenderFileName fileName, IMaterial material, RenderItem item)
            : base(item, material, fileName)
        {
        }

        /// <inheritdoc/>
        public override RenderResult Render(object option)
        {
            try
            {
                Stream stream = null;

                switch (Item.Object)
                {
                    case Asset asset:
                        stream = asset.FileName.GetStorageItem().GetInputStream();
                        break;

                    case StorageLocation location:
                        stream = location.GetStorageItem().GetInputStream();
                        break;

                    case IStorageItem storageItem:
                        stream = storageItem.GetInputStream();
                        break;

                    case Stream s:
                        stream = s;
                        break;

                    default:
                        break;
                }

                if (stream is null)
                {
                    Logs.LogError($"Cannot extract Stream from {Item}.");
                    return null;
                }

                using (stream)
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();

                    return new TextRenderResult(RenderStatus.Success, text);
                }
            }
            catch (Exception err)
            {
                Logs.LogError(err);
                return null;
            }
        }
    }
}

/// <summary>
/// Default material for rendering binary assets.
/// </summary>
[AssetAutoCreate]
public class DefaultBinaryMaterial : Asset, IMaterial
{
    /// <summary>
    /// Gets the default instance of this material.
    /// </summary>
    public static DefaultBinaryMaterial Default { get; private set; }

    private static bool _init;

    internal DefaultBinaryMaterial()
        : base($"*{nameof(DefaultBinaryMaterial)}")
    {
        if (_init)
        {
            return;
        }

        _init = true;

        UpdateAssetTypes(typeof(IMaterial));
        ResolveId();

        Default = this;
        MaterialUtility.DefaultBinaryMaterial = this;
    }

    /// <inheritdoc/>
    public override string DisplayText => "Default Binary Material";

    /// <inheritdoc/>
    public override Image DefaultIcon => CoreIconCache.Binary;

    /// <summary>
    /// Gets the render targets for this material.
    /// </summary>
    /// <param name="item">The render item.</param>
    /// <param name="basePath">The base path for output files.</param>
    /// <returns>A collection of render targets.</returns>
    public IEnumerable<RenderTarget> GetRenderTargets(RenderItem item, RenderFileName basePath)
    {
        if (item.RenderType.LocalName == RenderType.BinaryName)
        {
            yield return new BinaryRenderTarget(basePath.Append(item.Name), this, item);
        }
    }

    /// <summary>
    /// Render target for binary content.
    /// </summary>
    private class BinaryRenderTarget : RenderTarget
    {
        /// <summary>
        /// Creates a binary render target.
        /// </summary>
        /// <param name="fileName">The output file name.</param>
        /// <param name="material">The material.</param>
        /// <param name="item">The render item.</param>
        public BinaryRenderTarget(RenderFileName fileName, IMaterial material, RenderItem item)
            : base(item, material, fileName)
        {
        }

        /// <inheritdoc/>
        public override RenderResult Render(object option)
        {
            try
            {
                Stream stream = null;

                switch (Item.Object)
                {
                    case Asset asset:
                        stream = asset.FileName.GetStorageItem().GetInputStream();
                        break;

                    case StorageLocation location:
                        stream = location.GetStorageItem().GetInputStream();
                        break;

                    case IStorageItem storageItem:
                        stream = storageItem.GetInputStream();
                        break;

                    case Stream s:
                        stream = s;
                        break;

                    default:
                        break;
                }

                if (stream is null)
                {
                    Logs.LogError($"Cannot extract Stream from {Item}.");
                    return null;
                }

                using (stream)
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    return new BinaryRenderResult(RenderStatus.Success, bytes);
                }
            }
            catch (Exception err)
            {
                Logs.LogError(err);

                return null;
            }
        }
    }
}