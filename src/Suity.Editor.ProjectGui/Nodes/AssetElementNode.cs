using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents an asset element (child asset) node in the project tree view.
/// </summary>
public class AssetElementNode : PopulatePathNode, IAssetElementNode
{
    private Asset _cachedAsset;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetElementNode"/> class.
    /// </summary>
    /// <param name="elementKey">The key identifying this element within its parent asset.</param>
    public AssetElementNode(string elementKey)
    {
        ElementKey = elementKey;
    }

    /// <summary>
    /// Gets the key identifying this element within its parent asset.
    /// </summary>
    public string ElementKey { get; }

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override string TypeName => GetAsset()?.PreviewText ?? string.Empty;

    /// <inheritdoc/>
    public override ImageDef Image
    {
        get
        {
            Asset asset = GetAsset();

            if (asset != null)
            {
                return asset.Icon?.ToIconSmall() ?? CoreIconCache.Attribute.ToIconSmall();
            }
            else
            {
                return CoreIconCache.Attribute.ToIconSmall();
            }
        }
    }

    /// <inheritdoc/>
    protected override string OnGetText()
    {
        return GetAsset()?.NameInTreeView ?? base.OnGetText();
    }

    /// <summary>
    /// Gets the status image indicating errors or warnings.
    /// </summary>
    public ImageDef ImageStatus
    {
        get
        {
            Asset asset = GetAsset();
            if (asset is null)
            {
                return null;
            }

            if (asset.Problems?.Count > 0)
            {
                return CoreIconCache.Error;
            }
            else if (asset.IsLegacy)
            {
                return CoreIconCache.Warning;
            }
            else if (asset.IdConflict || asset.AssetKeyConflict)
            {
                return CoreIconCache.Warning;
            }
            //else if (asset.IsImported)
            //{
            //    return Properties.Resources.Import;
            //}

            return null;
        }
    }

    //public override TextStatus TextColorStatus
    //{
    //    get
    //    {
    //        Asset asset = GetAsset();
    //        if (asset is null)
    //        {
    //            return TextStatus.Normal;
    //        }

    //        if (asset.IsBroken)
    //        {
    //            return TextStatus.Error;
    //        }
    //        else if (asset.IsLegacy)
    //        {
    //            return TextStatus.Warning;
    //        }

    //        return TextStatus.Normal;
    //    }
    //}

    /// <inheritdoc/>
    public override TextStatus TextColorStatus
    {
        get
        {
            Asset asset = GetAsset();
            if (asset is null)
            {
                return base.TextColorStatus;
            }

            if (asset is ITextDisplay t)
            {
                return t.DisplayStatus;
            }
            else if (asset is ICodeLibrary)
            {
                return TextStatus.UserCode;
            }

            return base.TextColorStatus;
        }
    }

    /// <inheritdoc/>
    public override Color? Color
    {
        get
        {
            if (TextColorStatus == TextStatus.Normal && GetAsset() is IViewColor c)
            {
                return c.ViewColor;
            }

            return base.Color;
        }
    }

    /// <summary>
    /// Gets the asset associated with this element node.
    /// </summary>
    /// <returns>The associated asset, or null if not found.</returns>
    public Asset GetAsset()
    {
        if (_cachedAsset != null && _cachedAsset.Id != Guid.Empty)
        {
            return _cachedAsset;
        }

        if (this.Parent is AssetFileNode fileNode)
        {
            if (fileNode.GetAsset() is GroupAsset asset)
            {
                _cachedAsset = asset.GetChildAsset(Terminal, AssetFilters.All);

                return _cachedAsset;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the file path of the parent asset file.
    /// </summary>
    /// <returns>The file path, or null if no parent file node exists.</returns>
    public string GetFilePath()
    {
        if (this.Parent is AssetFileNode fileNode)
        {
            return fileNode.NodePath;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override bool CanUserDrag => false;

    /// <inheritdoc/>
    protected override bool CanPopulate()
    {
        return HasRender() || (GetAsset() is IFieldGroup group && group.FieldObjects.Any());
    }

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        Asset asset = GetAsset();
        if (asset is null)
        {
            yield break;
        }

        if (asset is IFieldGroup group)
        {
            foreach (var field in group.FieldObjects)
            {
                var fieldNode = new AssetFieldNode(field.Name);
                fieldNode.SetupNodePath(Path.Combine(NodePath, field.Name));

                yield return fieldNode;
            }
        }

        ICodeRenderInfoService buildInfo = Device.Current.GetService<ICodeRenderInfoService>();
        if (buildInfo != null)
        {
            var targets = buildInfo.GetAffectedRenderTargets(asset.Id);
            foreach (var target in targets.OrderBy(o => Path.GetFileName(o.FileName.PhysicFullPath)))
            {
                var renderTargetNode = new RenderTargetNode
                {
                    WorkSpaceName = (target.Tag as IWorkSpaceRefItem)?.WorkSpace?.Name
                };

                renderTargetNode.SetupNodePath(target.FileName.PhysicFullPath);

                yield return renderTargetNode;
            }
        }
    }

    /// <inheritdoc/>
    public override object DisplayedValue => GetAsset();

    /// <summary>
    /// Gets the extended image indicating rendering status.
    /// </summary>
    public ImageDef ImageEx
    {
        get
        {
            if (HasRender())
            {
                return Editor.ProjectGui.Properties.IconCache.Rendering;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    Guid IHasId.Id => GetAsset()?.Id ?? Guid.Empty;

    /// <summary>
    /// Determines whether this element has any associated render targets.
    /// </summary>
    /// <returns>True if there are render targets; otherwise, false.</returns>
    private bool HasRender()
    {
        Asset asset = GetAsset();
        if (asset != null)
        {
            ICodeRenderInfoService buildInfo = Device.Current.GetService<ICodeRenderInfoService>();
            if (buildInfo != null)
            {
                var targets = buildInfo.GetAffectedRenderTargets(asset.Id);
                return targets.Any();
            }
        }

        return false;
    }

}