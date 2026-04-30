namespace Suity.Views.PathTree;

/// <summary>
/// An abstract base class for file system nodes in the path tree, providing support for text editing and node reuse.
/// </summary>
public abstract class FsNode : PopulatePathNode
{
    /// <summary>
    /// Gets a value indicating whether this node's text can be edited by the user.
    /// </summary>
    public override bool CanEditText => true;

    /// <summary>
    /// Called when this node has been renamed.
    /// </summary>
    /// <param name="oldName">The previous name of the node before renaming.</param>
    protected virtual void OnRenamed(string oldName)
    { }

    /// <summary>
    /// Gets a value indicating whether this node can be reused during population updates.
    /// </summary>
    public override bool Reusable => true;
}
