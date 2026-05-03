using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
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
/// Default asset filter for the project view, showing only public and private assets.
/// </summary>
public class ProjectViewAssetFilter : IAssetFilter
{
    /// <summary>
    /// Gets the singleton instance of the filter.
    /// </summary>
    public static readonly ProjectViewAssetFilter Instance = new();

    /// <inheritdoc/>
    public bool FilterAsset(Asset content)
    {
        if (content is null)
        {
            return false;
        }

        if (content.AccessMode != AssetAccessMode.Public && content.AccessMode != AssetAccessMode.Private)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override string ToString() => "Default Filter";
}

/// <summary>
/// Represents an asset file node in the project tree view.
/// </summary>
public class AssetFileNode : FileNode,
    IAssetFileNode,
    IAssetFsNode,
    IHasAsset,
    IDropTarget,
    IHasId
{
    // This value defaults to empty, takes effect when set to child Asset
    private Asset _cachedAsset;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetFileNode"/> class.
    /// </summary>
    public AssetFileNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetFileNode"/> class with a specific asset.
    /// </summary>
    /// <param name="asset">The asset to associate with this node.</param>
    public AssetFileNode(Asset asset)
    {
        _cachedAsset = asset;

        if (_cachedAsset != null)
        {
            _cachedAsset.ObjectUpdated += _cachedAsset_ObjectUpdated;
        }
    }

    /// <inheritdoc/>
    protected internal override void OnRemoved(PathNode fromParent)
    {
        base.OnRemoved(fromParent);

        if (_cachedAsset != null)
        {
            _cachedAsset.ObjectUpdated -= _cachedAsset_ObjectUpdated;

            _cachedAsset = null;
        }
    }

    /// <summary>
    /// Handles the object updated event from the cached asset.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void _cachedAsset_ObjectUpdated(object sender, EntryEventArgs e)
    {
        Model?.OnNodeChanged(this);
    }

    /// <inheritdoc/>
    public override ImageDef Image => GetAsset()?.Icon ?? EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();

    /// <summary>
    /// Gets the list of attached file nodes associated with this asset.
    /// </summary>
    /// <returns>A list of attached path nodes, or null if none.</returns>
    private List<PathNode> GetAttachedNodes()
    {
        List<PathNode> nodes = null;

        foreach (var ext in AssetManager.Instance.AttachedAssetExtensions)
        {
            string filePath = NodePath + ext;
            var node = NodeList.Where(o => o.NodePath == filePath).FirstOrDefault();

            if (node != null)
            {
                (nodes ??= []).Add(node);
            }
        }

        return nodes;
    }

    /// <inheritdoc/>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        List<PathNode> attachedNodes = null;

        if (!IsAttachedFile)
        {
            EnsurePopulate();
            attachedNodes = GetAttachedNodes();
        }

        // PopulateReset();
        // If renamed and then PopulateDummy() is executed->CanPopulate() is called->Asset not found

        bool ok = base.MoveNode(newNodePath, results);

        // Populate();

        if (ok && attachedNodes != null)
        {
            foreach (var node in attachedNodes)
            {
                var ext = Path.GetExtension(node.NodePath);
                node.MoveNode(newNodePath + ext, results);
            }
        }

        (Parent as PopulatePathNode)?.PopulateUpdate();

        return ok;
    }

    /// <inheritdoc/>
    public override void Delete(bool sendToRecycleBin)
    {
        List<PathNode> attachedNodes = null;

        if (!IsAttachedFile)
        {
            EnsurePopulate();
            attachedNodes = GetAttachedNodes();
        }

        base.Delete(sendToRecycleBin);

        if (attachedNodes != null)
        {
            foreach (var node in attachedNodes)
            {
                node.Delete(sendToRecycleBin);
            }
        }
    }

    /// <inheritdoc/>
    public override void ChangeNodePath(string newNodePath, HashSet<RenameItem> results)
    {
        List<PathNode> attachedNodes = null;

        if (!IsAttachedFile)
        {
            EnsurePopulate();
            attachedNodes = GetAttachedNodes();
        }

        base.ChangeNodePath(newNodePath, results);

        if (attachedNodes != null)
        {
            foreach (var node in attachedNodes)
            {
                var ext = Path.GetExtension(node.NodePath);
                node.ChangeNodePath(newNodePath + ext, results);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this asset has any attached files.
    /// </summary>
    public bool ContainsAttachedFiles
    {
        get
        {
            foreach (var ext in AssetManager.Instance.AttachedAssetExtensions)
            {
                string path = NodePath + ext;
                if (File.Exists(path))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <inheritdoc/>
    protected override bool CanPopulate()
    {
        Asset asset = GetAsset();
        if (asset != null)
        {
            if (asset is IFileBunch)
            {
                return true;
            }

            if (asset is ICodeLibrary)
            {
                return true;
            }

            if (asset is LibraryAsset)
            {
                return true;
            }

            if ((asset as GroupAsset)?.GetChildAssetLocalNames(ProjectViewAssetFilter.Instance).Any() ?? false)
            {
                return true;
            }

            ICodeRenderInfoService buildInfo = Device.Current.GetService<ICodeRenderInfoService>();
            if (buildInfo != null)
            {
                var targets = buildInfo.GetAffectedRenderTargets(asset.Id);
                if (targets.Any())
                {
                    return true;
                }
            }
        }

        if (ContainsAttachedFiles)
        {
            return true;
        }

        return base.CanPopulate();
    }

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        if (!IsAttachedFile)
        {
            foreach (var ext in AssetManager.Instance.AttachedAssetExtensions)
            {
                string path = NodePath + ext;
                if (File.Exists(path))
                {
                    var node = new AssetFileNode();
                    node.SetupNodePath(path);

                    yield return node;
                }
            }
        }

        Asset asset = GetAsset();

        if (asset != null)
        {
            var keys = (asset as GroupAsset)?.GetChildAssetLocalNames(ProjectViewAssetFilter.Instance)?.ToList() ?? [];
            keys.Sort();

            foreach (string elementKey in keys)
            {
                var elementNode = new AssetElementNode(elementKey);
                elementNode.SetupNodePath(Path.Combine(NodePath, elementKey));

                yield return elementNode;
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
                    renderTargetNode.SetupNodePath($"#affected:{target.FileName.PhysicFullPath}");

                    yield return renderTargetNode;
                }
            }

            //if (asset is IFileBunch bunch)
            //{
            //    foreach (var item in bunch.GetRenderTargets(string.Empty, false))
            //    {
            //        BunchInnerFileNode volFileNode = new BunchInnerFileNode(item.FileName);
            //        volFileNode.SetupNodePath(NodePath.PathAppend(item.FileName));
            //        yield return volFileNode;
            //    }
            //}

            if (asset is ICodeLibrary userCode)
            {
                foreach (var fileId in userCode.IncludedFiles)
                {
                    var userCodeFileName = new UserCodeFileNode(fileId);
                    userCodeFileName.SetupNodePath(NodePath.PathAppend(fileId));

                    yield return userCodeFileName;
                }
            }

            if (asset is LibraryAsset libraryAsset)
            {
                foreach (var contentAsset in libraryAsset.ContentAssets)
                {
                    var contentFileNode = new AssetFileNode(contentAsset);
                    contentFileNode.SetupNodePath(NodePath.PathAppend(contentAsset.AssetKey));

                    yield return contentFileNode;
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override string OnGetText()
    {
        return Path.GetFileNameWithoutExtension(Terminal);
    }

    /// <summary>
    /// Gets the extended image indicating rendering status.
    /// </summary>
    public ImageDef ImageEx
    {
        get
        {
            Asset asset = GetAsset();
            if (asset != null)
            {
                ICodeRenderInfoService buildInfo = Device.Current.GetService<ICodeRenderInfoService>();
                if (buildInfo != null)
                {
                    var targets = buildInfo.GetAffectedRenderTargets(asset.Id);
                    if (targets.Any())
                    {
                        return Editor.ProjectGui.Properties.IconCache.Rendering;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the status image indicating errors or warnings.
    /// </summary>
    public ImageDef ImageStatus
    {
        get
        {
            DocumentEntry doc = GetDocument(false);
            if (doc != null)
            {
                if (doc.State == DocumentState.Failed)
                {
                    return CoreIconCache.Error;
                }
            }

            Asset asset = GetAsset();
            if (asset != null)
            {
                if (asset.Problems?.Count > 0)
                {
                    return CoreIconCache.Error;
                }
                else if (asset.Library is null && asset.IsLegacy)
                {
                    return CoreIconCache.Warning;
                }
                else if (asset.IdConflict || asset.AssetKeyConflict)
                {
                    return CoreIconCache.Warning;
                }
                //else if (asset.IsImport)
                //{
                //    return Properties.Resources.Import;
                //}
            }

            return null;
        }
    }

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

    /// <inheritdoc/>
    public override object DisplayedValue => GetAsset();

    /// <inheritdoc/>
    public override bool ExtensionHidden => true;

    /// <summary>
    /// Gets the asset associated with this node.
    /// </summary>
    /// <returns>The associated asset, or null if not found.</returns>
    public Asset GetAsset()
    {
        if (_cachedAsset != null && _cachedAsset.Id != Guid.Empty)
        {
            return _cachedAsset;
        }

        _cachedAsset = FileAssetManager.Current.GetAsset(NodePath);
        if (_cachedAsset != null)
        {
            _cachedAsset.ObjectUpdated += _cachedAsset_ObjectUpdated;
        }

        return _cachedAsset;
    }

    /// <summary>
    /// Gets the asset associated with this node, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the asset to.</typeparam>
    /// <returns>The asset cast to T, or null if not found or cast fails.</returns>
    public T GetAsset<T>() where T : class
    {
        return GetAsset() as T;
    }

    /// <summary>
    /// Gets the document entry associated with this asset file.
    /// </summary>
    /// <param name="open">Whether to open the document if not already open.</param>
    /// <returns>The document entry, or null if not found.</returns>
    public DocumentEntry GetDocument(bool open)
    {
        if (_cachedAsset != null)
        {
            return _cachedAsset.GetDocumentEntry(open);
        }
        else
        {
            return DocumentManager.Instance.GetDocument(NodePath);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this file is a file bunch.
    /// </summary>
    public bool IsFileBunch => Terminal.FileExtensionEquals(Asset.FileBunchExtension);

    /// <summary>
    /// Gets a value indicating whether this file is a code library.
    /// </summary>
    public bool IsCodeLibrary => Terminal.FileExtensionEquals(Asset.CodeLibraryExtension);

    /// <summary>
    /// Gets a value indicating whether this file is a meta file.
    /// </summary>
    public bool IsMeta => Terminal.FileExtensionEquals(Asset.MetaExtension);

    /// <summary>
    /// Gets a value indicating whether this file is an attached file.
    /// </summary>
    public bool IsAttachedFile => Terminal.GetIsAttachedFile();

    #region IHasAsset

    /// <inheritdoc/>
    public Asset TargetAsset => GetAsset();

    #endregion

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        var nodes = e.GetDraggingNodes<PathNode>();

        if (IsFileBunch)
        {
            if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
            {
                if (AssetRootNode.CanCreateFileBunch(nodes))
                {
                    e.SetCopyEffect();
                }
                else
                {
                    e.SetNoneEffect();
                }
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else if (IsCodeLibrary)
        {
            if (nodes.Any() && nodes.All(o => o is WorkSpaceReferenceNode) && nodes.IsParentSame())
            {
                e.SetCopyEffect();
            }
        }
        else
        {
            var view = Device.Current.GetService<IProjectGui>();
            view.DragOver(e);
        }
    }

    /// <inheritdoc/>
    async void IDropTarget.DragDrop(IDragEvent e)
    {
        var nodes = e.GetDraggingNodes<PathNode>();
        if (IsFileBunch)
        {
            if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
            {
                if (AssetRootNode.CanCreateFileBunch(nodes))
                {
                    await AssetRootNode.HandleCreateOrUpdateFileBunch(nodes, this.NodePath);
                }
                else
                {
                    e.SetNoneEffect();
                }
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else if (IsCodeLibrary)
        {
            if (nodes.Any() && nodes.All(o => o is WorkSpaceReferenceNode) && nodes.IsParentSame())
            {
                AssetRootNode.HandleMergeUserCodeLibrary(nodes.OfType<WorkSpaceReferenceNode>(), this);
            }
        }
        else
        {
            var view = Device.Current.GetService<IProjectGui>();
            view.DragDrop(e);
        }
    }

    #endregion

    /// <inheritdoc/>
    public override bool CanEditText
    {
        get
        {
            var asset = GetAsset();
            if (asset != null)
            {
                return asset.Library is null;
            }
            else
            {
                return base.CanEditText;
            }
        }
    }

    /// <inheritdoc/>
    Guid IHasId.Id => GetAsset()?.Id ?? Guid.Empty;

    /// <inheritdoc/>
    IAssetElementNode IAssetFileNode.FindElement(string elementKey)
    {
        base.EnsurePopulate();

        return base.NodeList.OfType<IAssetElementNode>().Where(o => o.ElementKey == elementKey).FirstOrDefault();
    }
}