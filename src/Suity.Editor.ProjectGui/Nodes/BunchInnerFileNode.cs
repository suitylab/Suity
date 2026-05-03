using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Views.PathTree;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents a file inside a file bunch asset.
/// </summary>
public class BunchInnerFileNode : PathNode
{
    private readonly string _fileId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BunchInnerFileNode"/> class.
    /// </summary>
    /// <param name="fileId">The identifier of the file within the bunch.</param>
    public BunchInnerFileNode(string fileId)
    {
        _fileId = fileId;
    }

    /// <inheritdoc/>
    public override ImageDef Image => EditorUtility.GetIconForFileExact(Terminal)?.ToIconSmall();

    /// <inheritdoc/>
    protected override string OnGetText() => _fileId;

    /// <inheritdoc/>
    public override bool CanEditText => false;

    /// <inheritdoc/>
    public override void Delete(bool sendToRecycleBin)
    {
        if (Parent is not AssetFileNode parent)
        {
            return;
        }

        IFileBunch bunch = parent.GetAsset<IFileBunch>();
        bunch?.DeleteFile(_fileId);
    }
}