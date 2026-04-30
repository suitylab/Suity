using Suity.Collections;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Im;
using Suity.Views.PathTree;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.Packaging;

/// <summary>
/// Interface for nodes that support an enabled/disabled check state in package preview.
/// </summary>
public interface IPackagePreviewNode
{
    /// <summary>
    /// Gets or sets the check state indicating whether this node is enabled for the package operation.
    /// </summary>
    CheckState EnableState { get; set; }
}

/// <summary>
/// Represents a directory node in the package preview tree, managing child directories and item nodes with hierarchical enable state propagation.
/// </summary>
public class PackagePreviewDirectoryNode : PopulatePathNode, IPackagePreviewNode
{
    internal CheckState _enableState = CheckState.Unchecked;
    internal PackageTypes _packageType;

    // Cannot use auto Populate because it needs to handle both Export and Import
    // Node structure must be created manually
    private Dictionary<string, PackagePreviewDirectoryNode> _directories;

    private Dictionary<string, PackagePreviewItemNode> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackagePreviewDirectoryNode"/> class.
    /// </summary>
    /// <param name="dirName">The directory name for this node.</param>
    /// <param name="direction">The package operation direction (export or import).</param>
    /// <param name="location">The file location category (asset or workspace).</param>
    public PackagePreviewDirectoryNode(string dirName, PackageDirection direction, FileLocations location)
        : base(dirName)
    {
        Direction = direction;
        Location = location;
    }

    /// <summary>
    /// Gets the package operation direction for this node.
    /// </summary>
    public PackageDirection Direction { get; }

    /// <summary>
    /// Gets the file location category for this node.
    /// </summary>
    public FileLocations Location { get; }

    /// <summary>
    /// Gets or sets the package type, propagating the value to all child nodes.
    /// </summary>
    public PackageTypes PackageType
    {
        get => _packageType;
        set
        {
            _packageType = value;
            if (_items != null)
            {
                foreach (var item in _items.Values)
                {
                    item.PackageType = value;
                }
            }

            if (_directories != null)
            {
                foreach (var dir in _directories.Values)
                {
                    dir.PackageType = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the workspace associated with this directory node.
    /// </summary>
    public WorkSpace WorkSpace { get; set; }

    /// <summary>
    /// Adds or retrieves a child directory node with the specified name.
    /// </summary>
    /// <param name="name">The name of the directory to add.</param>
    /// <returns>The existing or newly created directory node.</returns>
    public PackagePreviewDirectoryNode AddDirectory(string name)
    {
        return this.EnsureDirectory(name);
    }

    /// <summary>
    /// Adds an item node at the specified relative path, creating intermediate directories as needed.
    /// </summary>
    /// <param name="path">The relative path of the item, using forward slashes as separators.</param>
    /// <param name="enabled">Whether the item should be initially enabled.</param>
    /// <returns>The created item node, or null if the path is empty.</returns>
    public PackagePreviewItemNode AddItem(string path, bool enabled)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        path = path.Replace('\\', '/');
        string[] split = path.Split('/');

        PackagePreviewDirectoryNode current = this;
        for (int i = 0; i < split.Length; i++)
        {
            if (i != split.Length - 1)
            {
                current = current.EnsureDirectory(split[i]);
            }
            else
            {
                var file = current.EnsureItem(split[i]);
                file.Enabled = enabled;

                return file;
            }
        }

        return null;
    }

    /// <summary>
    /// Recursively sets the enable state on this node and all descendant nodes.
    /// </summary>
    /// <param name="enabled">Whether to enable or disable all nodes.</param>
    public void SetEnableDeep(bool enabled)
    {
        CheckState state = enabled ? CheckState.Checked : CheckState.Unchecked;
        _enableState = state;

        if (_directories != null)
        {
            foreach (var dir in _directories.Values)
            {
                dir.SetEnableDeep(enabled);
            }
        }

        if (_items != null)
        {
            foreach (var item in _items.Values)
            {
                item.Enabled = enabled;
            }
        }
    }

    /// <summary>
    /// Updates the enable state of this node based on its child nodes, then propagates upward to the parent.
    /// </summary>
    public void UpdateEnableState()
    {
        bool same = PreviewNodes.Select(o => o.EnableState).AllEqual(true);

        if (same)
        {
            _enableState = PreviewNodes.FirstOrDefault()?.EnableState ?? CheckState.Unchecked;
        }
        else
        {
            _enableState = CheckState.Indeterminate;
        }

        (Parent as PackagePreviewDirectoryNode)?.UpdateEnableState();
    }

    /// <summary>
    /// Recursively updates the enable state of all descendant nodes, then updates this node's state.
    /// </summary>
    public void UpdateEnableStateDeep()
    {
        if (_directories != null)
        {
            foreach (var dir in _directories.Values)
            {
                dir.UpdateEnableStateDeep();
            }
        }

        bool same = PreviewNodes.Select(o => o.EnableState).AllEqual(true);

        if (same)
        {
            _enableState = PreviewNodes.FirstOrDefault()?.EnableState ?? CheckState.Unchecked;
        }
        else
        {
            _enableState = CheckState.Indeterminate;
        }
    }

    /// <summary>
    /// Ensures a child directory node exists, creating it if necessary.
    /// </summary>
    /// <param name="name">The directory name.</param>
    /// <returns>The existing or newly created directory node.</returns>
    private PackagePreviewDirectoryNode EnsureDirectory(string name)
    {
        _directories ??= [];

        return _directories.GetOrAdd(name,
            _ => new PackagePreviewDirectoryNode(NodePath.PathAppend(name), Direction, Location)
            {
                PackageType = _packageType
            });
    }

    /// <summary>
    /// Ensures a child item node exists, creating it if necessary.
    /// </summary>
    /// <param name="name">The item name.</param>
    /// <returns>The existing or newly created item node.</returns>
    private PackagePreviewItemNode EnsureItem(string name)
    {
        _items ??= [];

        return _items.GetOrAdd(name,
            _ => new PackagePreviewItemNode(NodePath.PathAppend(name), Direction, Location)
            {
                PackageType = _packageType
            });
    }

    /// <summary>
    /// Enumerates all direct child preview nodes (both directories and items).
    /// </summary>
    private IEnumerable<IPackagePreviewNode> PreviewNodes
    {
        get
        {
            if (_directories != null)
            {
                foreach (var dir in _directories.Values)
                {
                    yield return dir;
                }
            }

            if (_items != null)
            {
                foreach (var item in _items.Values)
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// Generates a suggested asset path based on the enabled items within this directory tree.
    /// </summary>
    /// <returns>A dot-separated path string representing the suggested asset location.</returns>
    public string GetSuggestedAssetPath()
    {
        if (_items?.Values.Where(o => o.Enabled).Any() == true)
        {
            return Terminal;
        }

        if (_directories is null)
        {
            return Terminal;
        }

        var dirs = _directories.Values.Where(o => o._enableState != CheckState.Unchecked);
        if (dirs.CountOne())
        {
            string childResult = dirs.First().GetSuggestedAssetPath();
            if (!string.IsNullOrEmpty(childResult))
            {
                return $"{Terminal}.{childResult}";
            }
            else
            {
                return Terminal;
            }
        }

        return Terminal;
    }

    /// <summary>
    /// Recursively collects all enabled item nodes into the specified collection.
    /// </summary>
    /// <param name="collection">The collection to add enabled items to.</param>
    public void CollectEnabledItemsDeep(ICollection<PackagePreviewItemNode> collection)
    {
        if (_enableState == CheckState.Unchecked)
        {
            return;
        }

        if (_directories != null)
        {
            foreach (var dir in _directories.Values)
            {
                dir.CollectEnabledItemsDeep(collection);
            }
        }

        if (_items != null)
        {
            foreach (var item in _items.Values)
            {
                if (item.Enabled)
                {
                    collection.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Checks whether any enabled item in this directory tree has an error status.
    /// </summary>
    /// <returns>True if any enabled item has an error text status; otherwise, false.</returns>
    public bool ContainsError()
    {
        if (_enableState == CheckState.Unchecked)
        {
            return false;
        }

        if (_directories != null)
        {
            foreach (var dir in _directories.Values)
            {
                if (dir.ContainsError())
                {
                    return true;
                }
            }
        }

        if (_items != null)
        {
            foreach (var item in _items.Values)
            {
                if (item.Enabled && item.TextColorStatus == TextStatus.Error)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Collects all enabled child directory nodes into the specified collection.
    /// </summary>
    /// <param name="collection">The collection to add enabled directories to.</param>
    public void CollectEnabledDirectories(ICollection<PackagePreviewDirectoryNode> collection)
    {
        if (_enableState == CheckState.Unchecked)
        {
            return;
        }

        if (_directories != null)
        {
            foreach (var dir in _directories.Values)
            {
                if (dir._enableState != CheckState.Unchecked)
                {
                    collection.Add(dir);
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override bool CanPopulate()
    {
        return _directories?.Count > 0 || _items?.Count > 0;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        if (_directories != null)
        {
            foreach (var dir in _directories.Values.OrderBy(o => o.Terminal))
            {
                yield return dir;
            }
        }

        if (_items != null)
        {
            foreach (var item in _items.Values.OrderBy(o => o.Terminal))
            {
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.Folder.ToIconSmall();

    /// <inheritdoc/>
    public CheckState EnableState
    {
        get => _enableState;
        set
        {
            if (_enableState == value)
            {
                return;
            }

            if (value == CheckState.Indeterminate)
            {
                value = CheckState.Unchecked;
            }

            _enableState = value;

            if (_enableState == CheckState.Checked)
            {
                SetEnableDeep(true);
            }
            else
            {
                SetEnableDeep(false);
            }

            (Parent as PackagePreviewDirectoryNode)?.UpdateEnableState();
        }
    }
}

/// <summary>
/// Represents an individual file item node in the package preview tree, with status indicators and enable state management.
/// </summary>
public class PackagePreviewItemNode : PathNode, IPackagePreviewNode
{
    private PackageTypes _packageType;
    private bool _enabled = false;
    private bool _isRenderTarget;
    private bool _exportDisabled;
    private bool _isImportantFile;

    private TextStatus _textStatus;
    private string _text;
    private Image _imageEx;

    private bool _fileExists;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackagePreviewItemNode"/> class.
    /// </summary>
    /// <param name="path">The file path for this node.</param>
    /// <param name="direction">The package operation direction (export or import).</param>
    /// <param name="location">The file location category (asset or workspace).</param>
    public PackagePreviewItemNode(string path, PackageDirection direction, FileLocations location)
        : base(path)
    {
        Direction = direction;
        Location = location;
    }

    /// <inheritdoc/>
    protected override void OnSetupNodePath(string nodePath)
    {
        base.OnSetupNodePath(nodePath);

        _fileExists = File.Exists(nodePath);
    }

    /// <summary>
    /// Gets the package operation direction for this node.
    /// </summary>
    public PackageDirection Direction { get; }

    /// <summary>
    /// Gets the file location category for this node.
    /// </summary>
    public FileLocations Location { get; }

    /// <summary>
    /// Gets or sets the package type, updating the display status when changed.
    /// </summary>
    public PackageTypes PackageType
    {
        get => _packageType;
        set
        {
            _packageType = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Gets or sets whether this item is enabled for the package operation.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Gets or sets whether this item is a render target file that may contain internal IDs.
    /// </summary>
    public bool IsRenderTarget
    {
        get => _isRenderTarget;
        set
        {
            _isRenderTarget = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Gets or sets whether this item is excluded from export by default.
    /// </summary>
    public bool ExportDisabled
    {
        get => _exportDisabled;
        set
        {
            _exportDisabled = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Gets or sets whether this is an important file that should be flagged when disabled.
    /// </summary>
    public bool IsImportantFile
    {
        get => _isImportantFile;
        set
        {
            _isImportantFile = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Gets or sets whether this file resides in the workspace master directory.
    /// </summary>
    public bool InMaster { get; set; }

    /// <summary>
    /// Gets or sets the local file name relative to the workspace root.
    /// </summary>
    public string LocalFileName { get; set; }

    /// <summary>
    /// Gets or sets whether the physical file exists on disk.
    /// </summary>
    public bool FileExists
    {
        get => _fileExists;
        set
        {
            if (_fileExists == value)
            {
                return;
            }

            _fileExists = value;

            UpdateStatus();
        }
    }

    /// <inheritdoc/>
    public override Image Image => EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();

    /// <inheritdoc/>
    public CheckState EnableState
    {
        get => _enabled ? CheckState.Checked : CheckState.Unchecked;
        set
        {
            bool enabled = value == CheckState.Checked;

            if (_enabled == enabled)
            {
                return;
            }

            _enabled = enabled;

            UpdateStatus();
            (Parent as PackagePreviewDirectoryNode)?.UpdateEnableState();
        }
    }

    /// <inheritdoc/>
    public override TextStatus TextColorStatus => _textStatus;

    /// <summary>
    /// Gets the status text describing the current state of this item.
    /// </summary>
    public string StatusText => _text;

    /// <summary>
    /// Gets the additional status image displayed alongside the file icon.
    /// </summary>
    public Image ImageEx => _imageEx;

    /// <inheritdoc/>
    public override Image CustomImage => _imageEx;

    /// <inheritdoc/>
    public override void UpdateStatus()
    {
        base.UpdateStatus();

        _textStatus = GetTextStatus();
        _text = GetText();
        _imageEx = GetImageEx();
    }

    /// <summary>
    /// Traverses up the tree to find the workspace associated with this item.
    /// </summary>
    /// <returns>The associated workspace, or null if none is found.</returns>
    public WorkSpace FindWorkSpace()
    {
        PathNode node = this;

        while (node != null)
        {
            if (node is PackagePreviewDirectoryNode dirNode && dirNode.WorkSpace != null)
            {
                return dirNode.WorkSpace;
            }

            node = node.Parent;
        }

        return null;
    }

    /// <summary>
    /// Determines the text color status based on the current node state and operation direction.
    /// </summary>
    /// <returns>The appropriate <see cref="TextStatus"/> value.</returns>
    private TextStatus GetTextStatus()
    {
        switch (Direction)
        {
            case PackageDirection.Export:
                if (_enabled)
                {
                    if (_isRenderTarget)
                    {
                        return TextStatus.UserCode;
                    }
                    else if (_exportDisabled)
                    {
                        return TextStatus.Normal;
                    }
                    else if (!GetCanExportToLibrary())
                    {
                        return TextStatus.Error;
                    }
                    else
                    {
                        return TextStatus.Normal;
                    }
                }
                else
                {
                    if (_isRenderTarget)
                    {
                        return TextStatus.UserCode;
                    }
                    else if (_isImportantFile)
                    {
                        return TextStatus.Warning;
                    }
                    else
                    {
                        return TextStatus.Disabled;
                    }
                }
            case PackageDirection.Import:
                if (_enabled)
                {
                    if (_fileExists)
                    {
                        return TextStatus.Warning;
                    }
                    else
                    {
                        return TextStatus.Normal;
                    }
                }
                else
                {
                    return TextStatus.Disabled;
                }
            default:
                return TextStatus.Disabled;
        }
    }

    /// <summary>
    /// Generates a descriptive status text based on the current node state and operation direction.
    /// </summary>
    /// <returns>A status message string, or empty if no special status applies.</returns>
    private string GetText()
    {
        switch (Direction)
        {
            case PackageDirection.Import:
                if (_enabled && _fileExists)
                {
                    return "Will overwrite existing file";
                }
                break;

            case PackageDirection.Export:
                if (_enabled)
                {
                    //if (_isRenderTarget)
                    //{
                    //    return "This file may contain internal Ids";
                    //}
                    if (_exportDisabled)
                    {
                        return "This file is not exported by default";
                    }
                    if (!GetCanExportToLibrary())
                    {
                        return "This file does not support export";
                    }
                }
                else
                {
                    if (_isImportantFile)
                    {
                        return "This is an important file";
                    }
                }
                break;
        }

        return string.Empty;
    }

    /// <summary>
    /// Determines the additional status image to display based on the current state.
    /// </summary>
    /// <returns>An image for the status indicator, or null if no additional image is needed.</returns>
    private Image GetImageEx()
    {
        if (Direction == PackageDirection.Import && _fileExists)
        {
            return CoreIconCache.Duplicated.ToIconSmall();
        }

        if (Direction == PackageDirection.Export)
        {
            if (_exportDisabled)
            {
                return CoreIconCache.System.ToIconSmall();
            }

            if (_isRenderTarget)
            {
                return CoreIconCache.Render.ToIconSmall();
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether this file can be exported to a library package based on its type and location.
    /// </summary>
    /// <returns>True if the file can be exported to a library; otherwise, false.</returns>
    private bool GetCanExportToLibrary()
    {
        if (_packageType == PackageTypes.SuityLibrary)
        {
            if (Location == FileLocations.WorkSpace)
            {
                return false;
            }

            var asset = EditorUtility.GetFileAsset(this.NodePath);
            if (asset != null && !asset.CanExportToLibrary)
            {
                return false;
            }
        }

        return true;
    }
}
