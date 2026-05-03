using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.PathTree;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents a render target node in the project tree view.
/// </summary>
public class RenderTargetNode : FileNode, IRenderTargetNode
{
    /// <summary>
    /// Gets or sets the name of the workspace this render target belongs to.
    /// </summary>
    public string WorkSpaceName { get; set; }

    private ImageDef _icon;
    private ImageDef _iconEx;
    private ImageDef _iconStatus;
    private string _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetNode"/> class.
    /// </summary>
    public RenderTargetNode()
    {
    }

    /// <inheritdoc/>
    protected override void OnSetupNodePath(string nodePath)
    {
        base.OnSetupNodePath(nodePath);

        _icon = EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();
        if (!string.IsNullOrEmpty(WorkSpaceName))
        {
            _text = $"{base.OnGetText()} ({WorkSpaceName})";
        }
        else
        {
            _text = base.OnGetText();
        }

        _iconEx = Editor.ProjectGui.Properties.IconCache.Rendering;

        //if (!File.Exists(NodePath))
        //{
        //    _iconStatus = CoreIconCache.New;
        //}
        //else
        //{
        //    _iconStatus = null;
        //}
    }

    /// <inheritdoc/>
    public override bool CanUserDrag => false;

    /// <inheritdoc/>
    public override ImageDef Image => _icon;

    /// <inheritdoc/>
    protected override string OnGetText() => _text;

    /// <inheritdoc/>
    public override TextStatus TextColorStatus => TextStatus.Reference;

    /// <summary>
    /// Gets the extended image indicating rendering status.
    /// </summary>
    public ImageDef ImageEx => _iconEx;

    /// <summary>
    /// Gets the status image for this node (always null).
    /// </summary>
    public ImageDef ImageStatus => null;
}