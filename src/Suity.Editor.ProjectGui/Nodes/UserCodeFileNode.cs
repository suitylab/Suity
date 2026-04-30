using Suity.Helpers;
using Suity.Views.PathTree;
using System.Drawing;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents a user code file node within a code library.
/// </summary>
public class UserCodeFileNode : PathNode
{
    private readonly string _fileId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCodeFileNode"/> class.
    /// </summary>
    /// <param name="fileId">The identifier of the user code file.</param>
    public UserCodeFileNode(string fileId)
    {
        _fileId = fileId;
    }

    /// <inheritdoc/>
    public override Image Image => EditorUtility.GetIconForFileExact(Terminal)?.ToIconSmall();

    /// <inheritdoc/>
    protected override string OnGetText()
    {
        return _fileId;
    }

    /// <inheritdoc/>
    public override bool CanEditText => false;
}