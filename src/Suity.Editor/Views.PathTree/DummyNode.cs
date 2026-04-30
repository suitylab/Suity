namespace Suity.Views.PathTree;

/// <summary>
/// A placeholder node used to indicate that a parent node has unlisted child nodes.
/// </summary>
public class DummyNode : PathNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DummyNode"/> class.
    /// </summary>
    public DummyNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DummyNode"/> class with the specified model.
    /// </summary>
    /// <param name="model">The path tree model.</param>
    public DummyNode(IPathTreeModel model)
        : base(model)
    {
    }

    /// <summary>
    /// Returns the display text for this dummy node.
    /// </summary>
    /// <returns>Ellipsis ("...") to indicate more content exists.</returns>
    protected override string OnGetText()
    {
        return "...";
    }

    //public override void SetupNodePath(string nodePath)
    //{
    //    // Do nothing
    //}
}
